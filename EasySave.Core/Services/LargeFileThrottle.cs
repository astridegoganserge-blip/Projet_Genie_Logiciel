using System;
using System.Threading;



namespace EasySave.Core.Services
{
    public static class LargeFileThrottle
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private static readonly TimeSpan AcquireTimeout = TimeSpan.FromMinutes(10);



        public static bool Acquire(long fileSizeBytes, long maxFileSizeKb)
        {
            if (maxFileSizeKb <= 0)
            {
                return false;
            }



            long maxFileSizeBytes = maxFileSizeKb * 1024;



            if (fileSizeBytes < maxFileSizeBytes)
            {
                return false;
            }



            return Semaphore.Wait(AcquireTimeout);
        }



        public static void Release(bool wasAcquired)
        {
            if (!wasAcquired)
            {
                return;
            }



            try
            {
                Semaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                // Already released — defensive guard against double-release after recovery
            }
        }
    }
}