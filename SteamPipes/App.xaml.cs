using System.Windows;
using Steam.API;
using Steam.Machines;
using SteamNSteel.Impl;
using SteamNSteel.Jobs;

namespace SteamPipes
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    public static readonly SteamTransportRegistry SteamTransportRegistry = new SteamTransportRegistry();
        private static readonly JobManager JobManagerImpl = new JobManager();
        public static IJobManager JobManager => JobManagerImpl;

	    public App()
	    {
            JobManagerImpl.Start();
            
            TheMod.OnSteamNSteelInitialized(new SteamNSteelInitializedEvent(SteamTransportRegistry));
        }

	    protected override void OnExit(ExitEventArgs e)
	    {
	        JobManagerImpl.Stop();
	    }
	}
}