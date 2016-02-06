using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Steam.Machines;
using SteamNSteel.API;

namespace SteamPipes.UI
{
	/// <summary>
	///     Interaction logic for SteamUnitContainer.xaml
	/// </summary>
	public partial class SteamUnitContainer
	{
		public SteamUnitContainer()
		{
			InitializeComponent();
		}

		internal ModTileEntity SteamUnit { get; set; }
        
		private void SteamUnitDataChanged(object sender, EventArgs e)
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(InvalidateVisual));
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
			    var steamTransport = SteamUnit.GetSteamTransport();

				//if (SteamUnit.Debug)
				//TODO: Vary colour by pipe temperature
				//{
					var colorBrush = steamTransport.GetShouldDebug() ? Brushes.Red : Brushes.Blue;
					drawingContext.DrawRoundedRectangle(Brushes.Transparent, new Pen(colorBrush, 2), new Rect(RenderSize), 5, 5);
				//}

				var solidColorBrush = Brushes.SandyBrown;
				if (SteamUnit is BoilerTileEntity)
				{
					solidColorBrush = Brushes.Green;
				}
				else if (SteamUnit is FurnaceTileEntity || SteamUnit is BallMillTileEntity)
				{
					solidColorBrush = Brushes.Tomato;
				}

				var waterUsage = (steamTransport.GetWaterStored()/(double)steamTransport.GetMaximumWater());
				var waterHeightPixels = RenderSize.Height*waterUsage;
				var steamRenderHeight = Math.Max(0, (RenderSize.Height - waterHeightPixels)*steamTransport.GetCalculatedSteamDensity()/100);
				
				drawingContext.DrawRectangle(Brushes.LightGray, null,
					new Rect(new Size(RenderSize.Width, steamRenderHeight)));
				drawingContext.DrawRectangle(Brushes.CornflowerBlue, null,
					new Rect(new Point(0, RenderSize.Height - waterHeightPixels), new Size(RenderSize.Width, waterHeightPixels)));

				if (steamTransport.CanTransportAbove())
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
						new Point(RenderSize.Width*1/4, RenderSize.Height*1/4),
						new Point(RenderSize.Width*3/4, RenderSize.Height*1/4));
				}

				if (steamTransport.CanTransportBelow())
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
						new Point(RenderSize.Width*1/4, RenderSize.Height*3/4),
						new Point(RenderSize.Width*3/4, RenderSize.Height*3/4));
				}

				if (steamTransport.CanTransportWest())
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
						new Point(RenderSize.Width*1/4, RenderSize.Height*1/4),
						new Point(RenderSize.Width*1/4, RenderSize.Height*3/4));
				}
				if (steamTransport.CanTransportEast())
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
						new Point(RenderSize.Width*3/4, RenderSize.Height*1/4),
						new Point(RenderSize.Width*3/4, RenderSize.Height*3/4));
				}

				double y = 0;
				var text =
					new FormattedText($"steam: {steamTransport.GetSteamStored():0}/{steamTransport.GetCalculatedMaximumSteam():0}",
						CultureInfo.CurrentUICulture,
						FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText($"density: {Math.Floor(steamTransport.GetCalculatedSteamDensity())}%", CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText($"water: {steamTransport.GetWaterStored():0}/{steamTransport.GetMaximumWater():0}",
					CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText($"temp: {Math.Floor(steamTransport.GetTemperature())}%", CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));
			}
		}

		public void PlacePipe()
		{
			PlaceSteamUnit<PipeTileEntity>();
		}

		public void RemovePipe()
		{
			if (SteamUnit != null)
			{
				SteamUnit.DataChanged -= SteamUnitDataChanged;
                SteamUnit.Destroy();
				SteamUnit = null;
			}
			InvalidateVisual();
		}

		public void InjectSteam(int amount)
		{
		    SteamUnit?.GetSteamTransport().AddSteam(amount);
		}

	    public void RemoveSteam(int amount)
		{
		    SteamUnit?.GetSteamTransport().TakeSteam(amount);
		}

	    public void PlaceBoiler()
		{
			PlaceSteamUnit<BoilerTileEntity>();
		}

		public void PlaceBallMill()
		{
			PlaceSteamUnit<BallMillTileEntity>();
		}

		public void PlaceFurnace()
		{
			PlaceSteamUnit<FurnaceTileEntity>();
		}

		public void ToggleDebug()
		{
		    SteamUnit?.GetSteamTransport().ToggleDebug();
		}

	    private void PlaceSteamUnit<T>() where T : ModTileEntity, new()
		{
			if (SteamUnit != null && (!(SteamUnit is T)))
			{
			    SteamUnit.Destroy();
				SteamUnit.DataChanged -= SteamUnitDataChanged;
				SteamUnit = null;
			}
			if (SteamUnit == null)
			{
                SteamUnit = new T();
                SteamUnit.SetLocation(Grid.GetColumn(this), Grid.GetRow(this));
				SteamUnit.DataChanged += SteamUnitDataChanged;
			}
			InvalidateVisual();
		}
	}
}