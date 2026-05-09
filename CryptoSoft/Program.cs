using System;
using System.Threading;



namespace CryptoSoft;



public static class Program
{
    private const string CryptoSoftMutexName = @"Global\CryptoSoft_ProSoft_Mutex";
    private static readonly TimeSpan MutexTimeout = TimeSpan.FromSeconds(30);



    public static void Main(string[] args)
    {
        Mutex? mutex = null;
        bool ownsMutex = false;



        try
        {
            if (args.Length < 2)
            {
                Environment.Exit(-99);
                return;
            }



            mutex = new Mutex(true, CryptoSoftMutexName, out bool createdNew);



            if (createdNew)
            {
                ownsMutex = true;
            }
            else
            {
                try
                {
                    ownsMutex = mutex.WaitOne(MutexTimeout);
                }
                catch (AbandonedMutexException)
                {
                    ownsMutex = true;
                }



                if (!ownsMutex)
                {
                    Environment.Exit(-2);
                    return;
                }
            }



            var fileManager = new FileManager(args[0], args[1]);
            int elapsedTime = fileManager.TransformFile();



            Environment.Exit(elapsedTime);
        }
        catch
        {
            Environment.Exit(-99);
        }
        finally
        {
            if (ownsMutex && mutex != null)
            {
                mutex.ReleaseMutex();
            }



            mutex?.Dispose();
        }
    }
}