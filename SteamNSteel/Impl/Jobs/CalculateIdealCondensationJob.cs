using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class CalculateIdealCondensationJob : IJob {
		private INotifyIdealCondenstationComplete _notifyCalculateIdealCondensation;

		public CalculateIdealCondensationJob()
		{
			
		}

		public CalculateIdealCondensationJob(INotifyIdealCondenstationComplete notifyCalculateIdealCondensation)
		{
			this._notifyCalculateIdealCondensation = notifyCalculateIdealCondensation;
		}

		public void Execute()
		{
			_notifyCalculateIdealCondensation.IdealCondensationCalculated(new Result(null));
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