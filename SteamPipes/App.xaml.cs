using System.Windows;
using Steam.API;
using SteamPipes.Impl;

namespace SteamPipes
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    public static readonly SteamManager2 SteamManager = new SteamManager2();

	    public App()
	    {
            Steam.Machines.TheMod.OnSteamNSteelInitialized(new SteamNSteelInitializedEvent(SteamManager));
        }
	}
}