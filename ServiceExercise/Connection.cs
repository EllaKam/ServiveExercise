using System;
using System.Threading;

namespace ServiceExercise
{
    public class Connection : IDisposable
    {
        private object _lock = new object();
        public int runCommand(int value)
        {
            lock (_lock)
            {
                Thread.Sleep(50);
                return (value % 5) + 1;
            }
        }

        public void Dispose()
        {

        }
    }
}
