using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SteamPipes.API;

namespace SteamPipes
{

	public static class ExtensionMethods
	{
		private static Action EmptyDelegate = delegate() { };

		public static void Refresh(this UIElement uiElement)
		{
			uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
		}
	}


	/// <summary>
	///     Interaction logic for SteamUnitContainer.xaml
	/// </summary>
	public partial class SteamUnitContainer
	{
		public SteamUnitContainer()
		{
			InitializeComponent();
		}

		internal SteamUnit SteamUnit { get; set; }

		private void SteamUnitOnConnectionsChanged(object sender, EventArgs eventArgs)
		{
			//Dispatcher.BeginInvoke(new Action(InvalidateVisual));
			//this.Refresh();
			Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(
				() =>
				{
					InvalidateVisual();
				}
				));
		}

		private void SteamUnitDataChanged(object sender, EventArgs e)
		{
			//Dispatcher.BeginInvoke(new Action(InvalidateVisual));
			//this.Refresh();
			Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(
				() =>
				{
					InvalidateVisual();
				}
				));
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if (SteamUnit == null)
			{
				drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 1), new Rect(RenderSize));
			}
			else
			{
				//TODO: Vary colour by pipe temperature
				drawingContext.DrawRoundedRectangle(Brushes.Transparent, new Pen(Brushes.Red, 2), new Rect(RenderSize), 5, 5);



				var solidColorBrush = Brushes.SandyBrown;
				if (SteamUnit is ISteamProvider)
				{
					solidColorBrush = Brushes.Green;
				}
				else if (SteamUnit is ISteamConsumer)
				{
					solidColorBrush = Brushes.Tomato;
				}

				drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(new Size(RenderSize.Width, RenderSize.Height * SteamUnit.SteamDensity / 100)));
				

				if (SteamUnit.UnitAbove != null)
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*1/4, RenderSize.Height*0/4),
						new Point(RenderSize.Width*1/4, RenderSize.Height*1/4));
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*3/4, RenderSize.Height*0/4),
						new Point(RenderSize.Width*3/4, RenderSize.Height*1/4));
				}
				else
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width * 1 / 4, RenderSize.Height * 1 / 4),
						new Point(RenderSize.Width * 3 / 4, RenderSize.Height * 1 / 4));
				}

				if (SteamUnit.UnitBelow != null)
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*1/4, RenderSize.Height*3/4),
						new Point(RenderSize.Width*1/4, RenderSize.Height*4/4));
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*3/4, RenderSize.Height*3/4),
						new Point(RenderSize.Width*3/4, RenderSize.Height*4/4));
				}
				else
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width * 1 / 4, RenderSize.Height * 3 / 4),
						new Point(RenderSize.Width * 3 / 4, RenderSize.Height * 3 / 4));
				}

				if (SteamUnit.UnitLeft != null)
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*0/4, RenderSize.Height*1/4),
						new Point(RenderSize.Width*1/4, RenderSize.Height*1/4));
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*0/4, RenderSize.Height*3/4),
						new Point(RenderSize.Width*1/4, RenderSize.Height*3/4));
				}
				else
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width * 1 / 4, RenderSize.Height * 1 / 4),
						new Point(RenderSize.Width * 1 / 4, RenderSize.Height * 3 / 4));
				}
				if (SteamUnit.UnitRight != null)
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*3/4, RenderSize.Height*1/4),
						new Point(RenderSize.Width*4/4, RenderSize.Height*1/4));
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width*3/4, RenderSize.Height*3/4),
						new Point(RenderSize.Width*4/4, RenderSize.Height*3/4));
				}
				else
				{
					drawingContext.DrawLine(new Pen(solidColorBrush, 4.0),
						new Point(RenderSize.Width * 3 / 4, RenderSize.Height * 1 / 4),
						new Point(RenderSize.Width * 3 / 4, RenderSize.Height * 3 / 4));
				}

				double y = 0;
				var text = new FormattedText("steam: " + Math.Floor(SteamUnit.SteamStored), CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));
				y += text.Height + 2;
				text = new FormattedText("density: " + Math.Floor(SteamUnit.SteamDensity) + '%', CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText("temp: " + Math.Floor(SteamUnit.Temperature) + '%', CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));
			}
		}

		public void PlacePipe()
		{
			PlaceSteamUnit<SteamUnit>();
		}

		public void RemovePipe()
		{
			if (SteamUnit != null)
			{
				SteamUnit.DataChanged -= SteamUnitDataChanged;
				SteamUnit.ConnectionsChanged -= SteamUnitOnConnectionsChanged;
				SteamManager.RemoveSteamUnit(SteamUnit);
				SteamUnit = null;
			}
			InvalidateVisual();
		}

		public void InjectSteam(int amount)
		{
			if (SteamUnit != null)
			{
				SteamManager.InjectSteam(amount, SteamUnit);
			}
		}

		public void RemoveSteam(int amount)
		{
			if (SteamUnit != null)
			{
				SteamManager.RemoveSteam(amount, SteamUnit);
			}
		}

		public void PlaceBoiler()
		{
			PlaceSteamUnit<Boiler>();
		}

		public void PlaceBallMill()
		{
			PlaceSteamUnit<BallMill>();
		}

		public void PlaceFurnace()
		{
			PlaceSteamUnit<Furnace>();
		}
		
		private void PlaceSteamUnit<T>() where T : SteamUnit, new()
		{
			if (SteamUnit != null && (!(SteamUnit is T)))
			{
				SteamManager.RemoveSteamUnit(SteamUnit);
				SteamUnit.ConnectionsChanged -= SteamUnitOnConnectionsChanged;
				SteamUnit.DataChanged -= SteamUnitDataChanged;
				SteamUnit = null;
			}
			if (SteamUnit == null)
			{
				SteamUnit = SteamManager.CreateSteamUnit<T>(Grid.GetColumn(this), Grid.GetRow(this));
				SteamUnit.ConnectionsChanged += SteamUnitOnConnectionsChanged;
				SteamUnit.DataChanged += SteamUnitDataChanged;
			}
			InvalidateVisual();
		}
	}
}