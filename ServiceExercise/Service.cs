using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace ServiceExercise
{
    public class Service : IService, IDisposable
    {
        private ConcurrentQueue<Request> _requestsQueue;
        private int _connectionCount;
        private static long _sum = 0;
        private static object _locker = new object();
        private AutoResetEvent _eventFinish = new AutoResetEvent(false);
       
        public Service(int connectionCount)
        {
            _requestsQueue = new ConcurrentQueue<Request>();
            _connectionCount = connectionCount;
        }

        private void ProcessLocked()
        {
            if (Monitor.TryEnter(_locker))
            {
                try
                {
                    Process();
                }
                finally
                {
                    Monitor.Exit(_locker);
                }
            }
        }

        private void Process()
        {
            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
            using SemaphoreSlim sm = new SemaphoreSlim(_connectionCount);
            while (_requestsQueue.Any())
            {
                sm.Wait();
                if (_requestsQueue.TryDequeue(out Request request))
                {
                    Task task = Task.Run(() =>
                    {
                        int result = ProcessRequest(request);
                        Interlocked.Add(ref _sum, result);
                        Console.WriteLine($"Please wait... current sum={Interlocked.Read(ref _sum)}");
                        sm.Release(1);
                    });
                    tasks.Add(task);
                }

            }
            Task.WaitAll(tasks.ToArray());
            _eventFinish.Set();
        }

        private int ProcessRequest(Request request)
        {
            using Connection connection = new Connection();
            return connection.runCommand(request.Command);
        }

        public void sendRequest(Request request)
        {
            _requestsQueue.Enqueue(request);
            Task.Run(ProcessLocked);
        }

        public long getSummary()
        {
            _eventFinish.WaitOne();
            return Interlocked.Read(ref _sum);
        }

        public void Dispose()
        {
            _eventFinish.Dispose();           
        }
    }
}
