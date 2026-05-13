using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Core.Models;
using EasySave.Core.Services;



namespace EasySave.Core.Strategies
{
	/// <summary>
	/// Template Method base class for all backup strategies.
	/// Encapsulates the full backup pipeline:
	///   - source path validation
	///   - priority-ordered file selection
	///   - per-file pause / stop / priority barrier handling
	///   - large-file throttling
	///   - encryption via CryptoSoft
	///   - daily log writing (EasyLog) + optional centralized logging
	///   - state tracking
	///
	/// Subclasses only override two extension points:
	///   - SelectSourceFiles  : which files to copy
	///   - RunPostCopyStep    : optional step after the main loop
	/// </summary>
	public abstract class BackupStrategyBase : IBackupStrategy
	{
		public bool Execute(
			BackupJob job,
			EasyLog.EasyLog logger,
			AppSettings settings,
			JobExecutionContext context)
		{
			if (!job.ValidatePaths())
			{
				StateTracker.MarkAsError(
					job.Name,
					$"Chemin source introuvable : {job.SourcePath}");
				return false;
			}



			// === Variation point #1: which files does this strategy want to copy? ===
			string[] files = SelectSourceFiles(job, settings)
				.OrderByDescending(file =>
					PriorityFileFilter.IsPriorityFile(file, settings.PriorityExtensions))
				.ThenBy(file => file, StringComparer.OrdinalIgnoreCase)
				.ToArray();



			long totalSize = files.Sum(GetFileSize);
			int priorityFilesCount = files.Count(file =>
				PriorityFileFilter.IsPriorityFile(file, settings.PriorityExtensions));



			PriorityFileFilter.RegisterPriorityFiles(job.Name, priorityFilesCount);
			StateTracker.Initialize(job, files.Length, totalSize);



			// === Shared transfer loop — used to be duplicated in both subclasses ===
			foreach (string sourceFile in files)
			{
				if (!TransferOneFile(job, sourceFile, logger, settings, context))
				{
					return false;
				}
			}



			// === Variation point #2: optional post-copy step (orphan cleanup, etc.) ===
			if (!RunPostCopyStep(job, settings, logger, context))
			{
				return false;
			}



			StateTracker.MarkAsCompleted(job.Name);
			return true;
		}



		/// <summary>
		/// Returns the set of source files this strategy wants to copy.
		/// CompleteBackupStrategy returns every file.
		/// DifferentialBackupStrategy filters out files already up-to-date in target.
		/// </summary>
		protected abstract IEnumerable<string> SelectSourceFiles(
			BackupJob job,
			AppSettings settings);



		/// <summary>
		/// Optional post-copy hook. Default = do nothing and succeed.
		/// DifferentialBackupStrategy overrides this to delete orphan files.
		/// </summary>
		protected virtual bool RunPostCopyStep(
			BackupJob job,
			AppSettings settings,
			EasyLog.EasyLog logger,
			JobExecutionContext context)
		{
			return true;
		}



		// ============================================================
		//   Shared helpers — used to be duplicated in both subclasses
		// ============================================================

		private static bool TransferOneFile(
			BackupJob job,
			string sourceFile,
			EasyLog.EasyLog logger,
			AppSettings settings,
			JobExecutionContext context)
		{
			if (!context.WaitIfPaused())
			{
				PriorityFileFilter.ResetJob(job.Name);
				StateTracker.MarkAsInterrupted(job.Name);
				return false;
			}



			if (context.StopToken.IsCancellationRequested)
			{
				PriorityFileFilter.ResetJob(job.Name);
				StateTracker.MarkAsInterrupted(job.Name);
				return false;
			}



			if (!PriorityFileFilter.CanProcess(sourceFile, settings.PriorityExtensions, context))
			{
				PriorityFileFilter.ResetJob(job.Name);
				StateTracker.MarkAsInterrupted(job.Name);
				return false;
			}



			bool isPriorityFile =
				PriorityFileFilter.IsPriorityFile(sourceFile, settings.PriorityExtensions);
			bool wasLargeFileSlotAcquired = false;



			string targetFile = BuildTargetPath(job, sourceFile);
			long fileSize = GetFileSize(sourceFile);



			try
			{
				wasLargeFileSlotAcquired = LargeFileThrottle.Acquire(
					fileSize,
					settings.MaxFileSizeKb);



				EnsureTargetDirectoryExists(targetFile);



				DateTime transferStartTime = DateTime.Now;
				File.Copy(sourceFile, targetFile, true);
				long transferTimeMs = Math.Max(
					0,
					(long)(DateTime.Now - transferStartTime).TotalMilliseconds);



				long encryptionTimeMs = CryptoService.Encrypt(
					targetFile,
					settings.ExtensionsToEncrypt);



				logger.LogFileTransfer(
					job.Name,
					sourceFile,
					targetFile,
					fileSize,
					transferTimeMs,
					encryptionTimeMs);



				SendCentralizedLogIfRequired(
					settings,
					job.Name,
					sourceFile,
					targetFile,
					fileSize,
					transferTimeMs,
					encryptionTimeMs);



				StateTracker.UpdateProgress(
					job.Name,
					sourceFile,
					targetFile,
					fileSize);

				return true;
			}
			catch
			{
				logger.LogFileTransfer(
					job.Name,
					sourceFile,
					targetFile,
					fileSize,
					-1,
					0);



				SendCentralizedLogIfRequired(
					settings,
					job.Name,
					sourceFile,
					targetFile,
					fileSize,
					-1,
					0);



				StateTracker.MarkAsError(
					job.Name,
					$"Erreur lors du transfert : {sourceFile}");
				return false;
			}
			finally
			{
				LargeFileThrottle.Release(wasLargeFileSlotAcquired);

				if (isPriorityFile)
				{
					PriorityFileFilter.NotifyPriorityFileCompleted(job.Name);
				}
			}
		}



		protected static string BuildTargetPath(BackupJob job, string sourceFile)
		{
			string relativePath = Path.GetRelativePath(job.SourcePath, sourceFile);
			return Path.Combine(job.TargetPath, relativePath);
		}



		protected static void EnsureTargetDirectoryExists(string targetFile)
		{
			string? targetDirectory = Path.GetDirectoryName(targetFile);

			if (!string.IsNullOrWhiteSpace(targetDirectory) &&
				!Directory.Exists(targetDirectory))
			{
				Directory.CreateDirectory(targetDirectory);
			}
		}



		protected static long GetFileSize(string filePath)
		{
			return File.Exists(filePath)
				? new FileInfo(filePath).Length
				: 0;
		}



		protected static void SendCentralizedLogIfRequired(
			AppSettings settings,
			string backupName,
			string sourceFile,
			string targetFile,
			long fileSize,
			long transferTimeMs,
			long encryptionTimeMs)
		{
			if (settings.LogMode == LogMode.Local)
			{
				return;
			}



			var entry = new LogEntry
			{
				Timestamp = DateTime.Now,
				BackupName = backupName,
				SourceFile = sourceFile,
				TargetFile = targetFile,
				FileSize = fileSize,
				TransferTimeMs = transferTimeMs,
				EncryptionTimeMs = encryptionTimeMs
			};



			_ = LogCentralizer.SendAsync(entry, Environment.MachineName);
		}
	}
}