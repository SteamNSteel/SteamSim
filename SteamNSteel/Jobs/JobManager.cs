﻿using System;
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

        public JobManager()
        {
            
        }

	    public void AddBackgroundJob(IJob job)
	    {
		    _backgroundJobs.Add(job);
	    }

        public void Start()
        {
            Stop();
            _backgroundJobs = new BlockingCollection<IJob>();
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
            _backgroundJobs.CompleteAdding();
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
                foreach (var job in _backgroundJobs.GetConsumingEnumerable())
                {
                    job.Execute();
                }
            }
        }
    }
}
