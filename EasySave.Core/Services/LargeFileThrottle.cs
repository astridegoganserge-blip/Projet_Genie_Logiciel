using System.Threading;



namespace EasySave.Core.Services
{
    public static class LargeFileThrottle
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);



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



            Semaphore.Wait();
            return true;
        }



        public static void Release(bool wasAcquired)
        {
            if (!wasAcquired)
            {
                return;
            }



            Semaphore.Release();
        }
    }
}