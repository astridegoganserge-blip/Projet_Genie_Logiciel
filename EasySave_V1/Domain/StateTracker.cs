using EasySave.Application;
using EasySave.Shared;

namespace EasySave.Domain;

public sealed class StateTracker
{
  private readonly StateService _stateService;

  public BackupJob? CurrentJob { get; private set; }

  public int TotalFiles { get; private set; }
  public long TotalSize { get; private set; }

  public RemainingFiles { get; private set; }
  public long RemainingSize { get; private set; }

  public string currentSourceFile { get; private set; } = string.Empty;
  public string currentTargetFile { get; private set; } = string.Empty;

  public JobStatus Status { get; private set; } = JobStatus.Active;

  public StateTracker(StateService stateService)
    {
        _stateService = stateService;
    }

    public void Initialize(BackupJob job)
    {
        CurrentJob = job;
        TotalFiles = 0;
        TotalSize = 0;
        RemainingFiles = 0;
        RemainingSize = 0;
        Status = JobStatus.Active;
        SaveState();
    }

    public void SetInitialCounts(int totalFiles, long totalSize)
    {
        TotalFiles = totalFiles;
        TotalSize = totalSize;
        RemainingFiles = totalFiles;
        RemainingSize = totalSize;
        SaveState();
    }

    public void UpdateProgress(long fileSize, string sourceFile, string targetFile)
    {
        currentSourceFile = sourceFile;
        currentTargetFile = targetFile;
        RemainingSize = Math.Max(0, RemainingSize - fileSize);
        RemainingFiles = Math.Max(0, RemainingFiles - 1);
        SaveState();
    }
}