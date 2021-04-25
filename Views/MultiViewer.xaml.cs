using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WPFPages . ViewModels;

namespace WPFPages .Views
{
	/// <summary>
	/// Interaction logic for MultiViewer.xaml
	/// </summary>
	public partial class MultiViewer: Window
	{
		public static BankAccountViewModel bvm = MainWindow.bvm;
		public static CustomerViewModel cvm = MainWindow.cvm;
		public static DetailsViewModel dvm = MainWindow.dvm;

		public MultiViewer ()
		{
			InitializeComponent ();
			}

		private void Window_Loaded (object sender, RoutedEventArgs e)
		{
			

			//			if (dvm.DetailsObs == null)
			//				DetailsViewModel.LoadDetailsTask (DetailsGrid);
			//			GetDetInstance ().LoadDetailsTask ();
			//			DetailsViewModel.LoadDetailsTask ();
			BankGrid.ItemsSource = CollectionViewSource.GetDefaultView (bvm.BankAccountObs );
			CustomerGrid.ItemsSource = CollectionViewSource.GetDefaultView (cvm.CustomersObs);
			DetailsGrid.ItemsSource = CollectionViewSource.GetDefaultView (dvm.DetailsObs );
//			BankGrid.DataContext = (bvm.BankAccountObs );
//			CustomerGrid.DataContext = (CustomerViewModel.CustomersObs);
//			DetailsGrid.DataContext = (dvm.DetailsObs );

		}

		private void Window_PreviewKeyDown (object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.RightAlt)
			{
				Flags.ListGridviewControlFlags ();
			}

		}
	}
}
