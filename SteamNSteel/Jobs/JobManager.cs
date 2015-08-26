using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SteamPipes.Jobs
{
    public class JobManager : IJobManager
    {
        private readonly List<Thread> JobThreads = new List<Thread>();
        private bool running = true;
        private readonly IProducerConsumerCollection<IJob> BackgroundJobs = new ConcurrentQueue<IJob>(); 

        public JobManager()
        {
            
        }

        public void Start()
        {
            Stop();
            running = true;
            for (int i = 0; i < Environment.ProcessorCount; ++i)
            {
                Thread t = new Thread(StartJobThread);
                t.Name = "Job Thread #" + i;
                JobThreads.Add(t);
                t.Start();
            }
        }

        public void Stop()
        {
            running = false;
            foreach (var thread in JobThreads)
            {
                thread.Join();
            }
            JobThreads.Clear();
        }

        private void StartJobThread()
        {
            while (running)
            {
                IJob job;
                if (BackgroundJobs.TryTake(out job))
                {
                    job.Execute();
                }
            }
        }
    }
}
