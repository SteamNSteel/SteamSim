using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Steam.Machines;
using SteamPipes.API;
using SteamPipes.UI;

namespace SteamPipes
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ClickBehaviour _clickBehaviour;
	    private bool _tickThreadRunning;
	    private DateTime _previousTick;

	    public MainWindow()
		{
			InitializeComponent();

			PlacePipeButton_Click(PlacePipeButton, null);
			SteamManager.StartSimulationThread();
		    //App.SteamManager.StartSimulationThread();
		    StartTickThread();
		}

	    private void StartTickThread()
	    {
            var thread = new Thread(TickThreadStart) { Name = "Tick Thread" };
	        _tickThreadRunning = true;
            thread.Start();
        }

	    private void TickThreadStart()
	    {
	        var tps = TimeSpan.FromMilliseconds( 1 / 20.0 * 1000);
            
            while (_tickThreadRunning)
            {
                lock (TheMod.TileEntities)
                {
                    foreach (var tileEntity in TheMod.TileEntities)
                    {
                        tileEntity.OnTick();
                    }
                }

                var thisTick = DateTime.Now;
                _previousTick = thisTick;

	            var timeSpan = tps - (DateTime.Now - _previousTick);
                _previousTick = DateTime.Now;
                Thread.Sleep(timeSpan);
                
            }
	    }

	    private void ToggleButton(Button theButton)
		{
			theButton.IsEnabled = false;
			foreach (var button in PlaceBlockGroup.Children.OfType<Button>().Except(new[] {theButton}))
			{
				button.IsEnabled = true;
			}
		}

		private void PlacePipeButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.PlaceBlock;
		}

		private void CleareButton_OnClick(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.Clear;
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			var objectIdentified = VisualTreeHelper.HitTest(this, e.GetPosition(this)).VisualHit as SteamUnitContainer;
			if (objectIdentified != null)
			{
				switch (_clickBehaviour)
				{
					case ClickBehaviour.PlaceBlock:
						objectIdentified.PlacePipe();
						break;
					case ClickBehaviour.Clear:
						objectIdentified.RemovePipe();
						break;
					case ClickBehaviour.ToggleDebug:
						objectIdentified.ToggleDebug();
						break;
					case ClickBehaviour.AddSteam:
						objectIdentified.InjectSteam(100);
						break;
					case ClickBehaviour.RemoveSteam:
						objectIdentified.RemoveSteam(100);
						break;
					case ClickBehaviour.PlaceBoiler:
						objectIdentified.PlaceBoiler();
						break;
					case ClickBehaviour.PlaceBallMill:
						objectIdentified.PlaceBallMill();
						break;
					case ClickBehaviour.PlaceFurnace:
						objectIdentified.PlaceFurnace();
						break;
				}
			}
		}

		private void AddSteamButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.AddSteam;
		}

		private void RemoveSteamButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.RemoveSteam;
		}

		private void StepSimulationButton_Click(object sender, RoutedEventArgs e)
		{
			SteamManager.StepSimulation(1);
		}

		private void PlaceBoilerButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.PlaceBoiler;
		}

		private void PlaceFurnaceButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.PlaceFurnace;
		}

		private void PlaceBallMillButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button) sender);
			_clickBehaviour = ClickBehaviour.PlaceBallMill;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			SteamManager.Stop();
		}

		private void DebugButton_OnClick(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button)sender);
			_clickBehaviour = ClickBehaviour.ToggleDebug;
		}
	}
}