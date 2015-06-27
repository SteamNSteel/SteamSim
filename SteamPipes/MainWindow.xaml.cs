using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SteamPipes
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ClickBehaviour _clickBehaviour;

		public MainWindow()
		{
			InitializeComponent();

			PlacePipeButton_Click(PlacePipeButton, null);
			SteamManager.StartSimulationThread();
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
			ToggleButton((Button)sender);
			_clickBehaviour = ClickBehaviour.PlaceBlock;
		}

		private void CleareButton_OnClick(object sender, RoutedEventArgs e)
		{
			ToggleButton((Button)sender);
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
					case ClickBehaviour.AddSteam:
						objectIdentified.InjectSteam(100);
						break;
					case ClickBehaviour.RemoveSteam:
						objectIdentified.RemoveSteam(100);
						break;
				}
			}
		}

		private void AddSteamButton_Click(object sender, RoutedEventArgs e)
		{

			ToggleButton((Button)sender);
			_clickBehaviour = ClickBehaviour.AddSteam;
		}

		private void RemoveSteamButton_Click(object sender, RoutedEventArgs e)
		{

			ToggleButton((Button)sender);
			_clickBehaviour = ClickBehaviour.RemoveSteam;
		}

		private void StepSimulationButton_Click(object sender, RoutedEventArgs e)
		{
			SteamManager.StepSimulation();
		}
	}
}
