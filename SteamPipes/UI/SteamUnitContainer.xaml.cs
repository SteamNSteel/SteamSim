using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Steam.Machines;
using SteamNSteel.Impl;

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
			    var steamTransport = SteamUnit.getSteamTransport();

				//if (SteamUnit.Debug)
				//TODO: Vary colour by pipe temperature
				//{
					var colorBrush = steamTransport.getShouldDebug() ? Brushes.Red : Brushes.Blue;
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

				var waterUsage = (steamTransport.getWaterStored()/(double)steamTransport.getMaximumWater());
				var waterHeightPixels = RenderSize.Height*waterUsage;
				var actualMaximumSteam = SteamMaths.calculateMaximumSteam(
						steamTransport.getWaterStored(),
						steamTransport.getMaximumWater(),
						steamTransport.getMaximumSteam()
					);

				var steamDensity = SteamMaths.calculateSteamDensity(
					steamTransport.getSteamStored(),
					actualMaximumSteam);
				var steamRenderHeight = Math.Max(0, (RenderSize.Height - waterHeightPixels)* steamDensity / 100);
				
				drawingContext.DrawRectangle(Brushes.LightGray, null,
					new Rect(new Size(RenderSize.Width, steamRenderHeight)));
				drawingContext.DrawRectangle(Brushes.CornflowerBlue, null,
					new Rect(new Point(0, RenderSize.Height - waterHeightPixels), new Size(RenderSize.Width, waterHeightPixels)));

				if (steamTransport.canTransportAbove())
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

				if (steamTransport.canTransportBelow())
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

				if (steamTransport.canTransportWest())
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
				if (steamTransport.canTransportEast())
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
					new FormattedText($"steam: {steamTransport.getSteamStored():0}/{actualMaximumSteam:0}",
						CultureInfo.CurrentUICulture,
						FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText($"density: {Math.Floor(steamDensity)}%", CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText($"water: {steamTransport.getWaterStored():0}/{steamTransport.getMaximumWater():0}",
					CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight, new Typeface("Ariel"), 14, Brushes.Black);
				drawingContext.DrawText(text, new Point(0, y));

				y += text.Height + 2;
				text = new FormattedText($"temp: {Math.Floor(steamTransport.getTemperature())}%", CultureInfo.CurrentUICulture,
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
                SteamUnit.destroy();
				SteamUnit = null;
			}
			InvalidateVisual();
		}

		public void InjectSteam(int amount)
		{
		    SteamUnit?.getSteamTransport().addSteam(amount);
			InvalidateVisual();
		}

		public void InjectCondensation(int amount)
		{
			SteamUnit?.getSteamTransport().addCondensate(amount);
			InvalidateVisual();
		}

		public void RemoveSteam(int amount)
		{
		    SteamUnit?.getSteamTransport().takeSteam(amount);
			InvalidateVisual();
		}

		public void RemoveCondensation(int amount)
		{
			SteamUnit?.getSteamTransport().takeCondensate(amount);
			InvalidateVisual();
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
		    SteamUnit?.getSteamTransport().toggleDebug();
			InvalidateVisual();
		}

	    private void PlaceSteamUnit<T>() where T : ModTileEntity, new()
		{
			if (SteamUnit != null && (!(SteamUnit is T)))
			{
			    SteamUnit.destroy();
				SteamUnit.DataChanged -= SteamUnitDataChanged;
				SteamUnit = null;
			}
			if (SteamUnit == null)
			{
                SteamUnit = new T();
                SteamUnit.setLocation(Grid.GetColumn(this), Grid.GetRow(this));
				SteamUnit.DataChanged += SteamUnitDataChanged;
			}
			InvalidateVisual();
		}
	}
}