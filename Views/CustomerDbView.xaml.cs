using System . Windows;
using System . Windows . Data;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for CustomerDbView.xaml
	/// </summary>
	public partial class CustomerDbView : Window
	{
		public CustomerDbView ( )
		{
			InitializeComponent ( );
		}

		private void Window_Loaded ( object sender , RoutedEventArgs e )
		{
			// Data source is handled in XAML !!!!
			//dataGrid . ItemsSource = BankCollection.Bankcollection;
		}

		private void button_Click ( object sender , RoutedEventArgs e )
		{
			int x =0;
		}
	}
}
