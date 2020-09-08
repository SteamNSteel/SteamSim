using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SteamNSteel.Jobs
{
    public class JobManager : IJobManager
    {
        private readonly List<Thread> JobThreads = new List<Thread>();
        private bool running = true;
        private BlockingCollection<IJob> _backgroundJobs = new BlockingCollection<IJob>(); 
		private ConcurrentQueue<IJob> _pretickJobs = new ConcurrentQueue<IJob>();
		
	    public void addBackgroundJob(IJob job)
	    {
		    _backgroundJobs.Add(job);
	    }

	    public void addPreTickJob(IJob job)
	    {
		    _pretickJobs.Enqueue(job);
	    }

	    public void doPretickJobs()
	    {
		    while (!_pretickJobs.IsEmpty)
		    {
			    IJob job;
			    if (_pretickJobs.TryDequeue(out job))
			    {
				    job.execute();
			    }
		    }
	    }

        public void start()
        {
            stop();
            _backgroundJobs = new BlockingCollection<IJob>();
            running = true;
	        int processorCount = Environment.ProcessorCount;
	        processorCount = 1;
	        for (int i = 0; i < processorCount; ++i)
            {
                Thread t = new Thread(startJobThread);
                t.Name = "Job Thread #" + i;
                JobThreads.Add(t);
                t.Start();
            }
        }

        public void stop()
        {
            running = false;
            _backgroundJobs.CompleteAdding();
            foreach (Thread thread in JobThreads)
            {
                thread.Join();
            }
            JobThreads.Clear();
        }

        private void startJobThread()
        {
            while (running)
            {
                foreach (IJob job in _backgroundJobs.GetConsumingEnumerable())
                {
                    job.execute();
                }
            }
        }
    }
}
