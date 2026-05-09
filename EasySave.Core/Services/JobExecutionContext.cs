using System;
using System.Threading;



namespace EasySave.Core.Services
{
    public sealed class JobExecutionContext : IDisposable
    {
        private readonly CancellationTokenSource _stopSource = new();
        private volatile bool _pauseRequested;
        private bool _disposed;



        public CancellationToken StopToken => _stopSource.Token;



        public bool IsPauseRequested => _pauseRequested;



        public void RequestPause()
        {
            _pauseRequested = true;
        }



        public void RequestResume()
        {
            _pauseRequested = false;
        }



        public void RequestStop()
        {
            _stopSource.Cancel();
        }



        public bool WaitIfPaused()
        {
            while (_pauseRequested)
            {
                if (StopToken.IsCancellationRequested)
                {
                    return false;
                }



                Thread.Sleep(200);
            }



            return !StopToken.IsCancellationRequested;
        }



        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }



            _stopSource.Dispose();
            _disposed = true;
        }
    }
}