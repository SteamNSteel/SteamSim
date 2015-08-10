using System.Windows;
using Steam.API;
using Steam.Machines;
using SteamPipes.Impl;

namespace SteamPipes
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    public static readonly SteamTransportRegistry SteamTransportRegistry = new SteamTransportRegistry();

	    public App()
	    {
            TheMod.OnSteamNSteelInitialized(new SteamNSteelInitializedEvent(SteamTransportRegistry));
        }
	}
}