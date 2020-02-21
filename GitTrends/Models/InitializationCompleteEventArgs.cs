using System;
namespace GitTrends
{
    public class InitializationCompleteEventArgs : EventArgs
    {
        public InitializationCompleteEventArgs(bool isInitializationSuccessful) =>
            IsInitializationSuccessful = isInitializationSuccessful;

        public bool IsInitializationSuccessful { get; }
    }
}
