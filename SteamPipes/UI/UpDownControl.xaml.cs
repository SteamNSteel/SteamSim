using System.Windows;
using System.Windows.Controls;

namespace SteamPipes.UI
{
	/// <summary>
	///     Interaction logic for UpDownControl.xaml
	/// </summary>
	public partial class UpDownControl : UserControl
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value", typeof (int), typeof (UpDownControl), new PropertyMetadata(default(int)));

		public UpDownControl()
		{
			InitializeComponent();
		}

		public int Value
		{
			get { return (int) GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		private void UpButton_Click(object sender, RoutedEventArgs e)
		{
			Value++;
		}

		private void DownButton_Click(object sender, RoutedEventArgs e)
		{
			Value--;
		}
	}
}