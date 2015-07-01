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
	/// Interaction logic for UpDownControl.xaml
	/// </summary>
	public partial class UpDownControl : UserControl
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value", typeof (int), typeof (UpDownControl), new PropertyMetadata(default(int)));

		public int Value
		{
			get { return (int) GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public UpDownControl()
		{
			InitializeComponent();
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
