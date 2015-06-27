using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SteamPipes
{
	public class SteamVis : Grid
	{
		public int FieldWidth
		{
			get { return (int) GetValue(FieldWidthProperty); }
			set { SetValue(FieldWidthProperty, value); }
		}

		public int FieldHeight
		{
			get { return (int) GetValue(FieldHeightProperty); }
			set { SetValue(FieldHeightProperty, value); }
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			for (int i = 0; i < FieldHeight; ++i)
			{
				RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
			}
			CreateNewColumns(0, FieldWidth);
		}

		private void WidthChanged(DependencyPropertyChangedEventArgs args)
		{
			var oldValue = (int) args.OldValue;
			var newValue = (int) args.NewValue;

			if (newValue > oldValue)
			{
				CreateNewColumns(oldValue, newValue);
			}
			else
			{
				var itemsToRemove = from item in Children.OfType<SteamUnitContainer>()
									where GetColumn(item) >= newValue select item;
				foreach (var item in itemsToRemove.ToList())
				{
					item.RemovePipe();
					Children.Remove(item);
				}
				for (var i = oldValue; i > newValue; --i)
				{
					ColumnDefinitions.RemoveAt(ColumnDefinitions.Count - 1);
				}
			}
		}

		private void CreateNewColumns(int oldValue, int newValue)
		{
			for (var x = oldValue; x < newValue; ++x)
			{
				for (var y = 0; y < FieldHeight; ++y)
				{
					var newContainer = new SteamUnitContainer();
					SetColumn(newContainer, x);
					SetRow(newContainer, y);
					Children.Add(newContainer);
				}
				ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			}
		}

		private void HeightChanged(DependencyPropertyChangedEventArgs args)
		{
			var oldValue = (int)args.OldValue;
			var newValue = (int)args.NewValue;

			if (newValue > oldValue)
			{
				CreateNewRows(oldValue, newValue);
			}
			else
			{
				var itemsToRemove = from item in Children.OfType<SteamUnitContainer>()
									where GetRow(item) >= newValue
									select item;
				foreach (var item in itemsToRemove.ToList())
				{
					item.RemovePipe();
					Children.Remove(item);
				}
				for (var i = oldValue; i > newValue; --i)
				{
					RowDefinitions.RemoveAt(RowDefinitions.Count - 1);
				}
			}
		}

		private void CreateNewRows(int oldValue, int newValue)
		{
			for (var y = oldValue; y < newValue; ++y)
			{
				for (var x = 0; x < FieldWidth; ++x)
				{
					var newContainer = new SteamUnitContainer();
					SetColumn(newContainer, x);
					SetRow(newContainer, y);
					Children.Add(newContainer);
				}
				RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
		}

		public static readonly DependencyProperty FieldWidthProperty = DependencyProperty.Register(
			"FieldWidth", typeof (int), typeof (SteamVis),
			new PropertyMetadata(7, (o, args) => ((SteamVis) o).WidthChanged(args)));

		public static readonly DependencyProperty FieldHeightProperty = DependencyProperty.Register(
			"FieldHeight", typeof (int), typeof (SteamVis), 
			new PropertyMetadata(6, (o, args) => ((SteamVis)o).HeightChanged(args)));
	}
}