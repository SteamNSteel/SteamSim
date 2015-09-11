using System;
using System.Collections.Generic;
using System.Threading;
using Steam.API;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class RecalculateTopologyJob : IJob
	{
		private readonly SteamTransportLocation _location;
		private readonly long _topologyGeneration;
		private readonly INotifyTopologyCalculated _notifyTopologyCalculated;

		public RecalculateTopologyJob(SteamTransportLocation location, long topologyGeneration, INotifyTopologyCalculated notifyTopologyCalculated)
		{
			_location = location;
			_topologyGeneration = topologyGeneration;
			_notifyTopologyCalculated = notifyTopologyCalculated;
		}

		public void Execute()
		{
			SteamTransportRegistry steamTransportRegistry = TheMod.SteamTransportRegistry;
			HashSet<SteamTransportTopology> topologiesToMerge = new HashSet<SteamTransportTopology>();
			SteamTransportTopology newTopology = new SteamTransportTopology(_topologyGeneration);
			var threadName = Thread.CurrentThread.Name;

			lock (newTopology.GetLockObject())
			{
				Stack<SteamTransportLocation> todoList = new Stack<SteamTransportLocation>();
				todoList.Push(_location);
				while (todoList.Count > 0)
				{
					SteamTransportLocation location = todoList.Pop();
					Console.WriteLine($"{threadName} - Working on {location}");
					SteamTransport transportAtLocation = (SteamTransport)steamTransportRegistry.GetSteamTransportAtLocation(location);
					if (transportAtLocation == null) continue;
					
					lock (transportAtLocation._syncObj)
					{
						var currentTransportTopology = transportAtLocation.GetTopology();
						if (currentTransportTopology != null && currentTransportTopology.IsSameGenerationAs(newTopology))
						{
							Console.WriteLine($"{threadName} - {location} has already been processed this generation.");
							continue;
						}

						transportAtLocation.SetTopology(newTopology);

						foreach (var forgeDirection in ForgeDirection.VALID_DIRECTIONS)
						{
							SteamTransportLocation steamTransportLocation = location.Offset(forgeDirection);
							SteamTransport steamTransportAtLocation =
								(SteamTransport) steamTransportRegistry.GetSteamTransportAtLocation(steamTransportLocation);
							if (steamTransportAtLocation == null)
							{
								Console.WriteLine($"{threadName} - No block found at {steamTransportLocation} ({forgeDirection})");
								continue;
							}
							Console.WriteLine($"{threadName} - Block found at {steamTransportLocation} ({forgeDirection})");

							SteamTransportTopology currentTopology = steamTransportAtLocation.GetTopology();
							if (currentTopology == null || currentTopology.IsSupercededBy(newTopology))
							{
								Console.WriteLine($"{threadName} - Found a block to replace at {steamTransportLocation}");
								todoList.Push(steamTransportLocation);
							}
							else if (!currentTopology.Equals(newTopology) && currentTopology.IsSameGenerationAs(newTopology))
							{
								//This check might not be nessessary in Java
								if (currentTopology.HasPriorityOver(newTopology) && !topologiesToMerge.Contains(currentTopology))
								{
									topologiesToMerge.Add(currentTopology);
									Console.WriteLine($"{threadName} - Plan to merge {currentTopology} to {newTopology}");
								}
							}
						}
					}
				}
			}

			foreach (SteamTransportTopology topology in topologiesToMerge)
			{
				Console.WriteLine($"{threadName} - Merging Topology {topology} into {newTopology}");
				lock (topology.GetLockObject())
				{
					foreach (SteamTransport transport in topology.GetTransports())
					{
						transport.SetTopology(newTopology);
					}
					topology.MakeObsolete();
				}
			}

			_notifyTopologyCalculated.TopologyCalculated(new Result(newTopology));
		}

		public class Result
		{
			internal readonly SteamTransportTopology NewTopology;

			internal Result(SteamTransportTopology newTopology)
			{
				NewTopology = newTopology;
			}
		}
	}
}