using System;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for MultiViewer.xaml
	/// </summary>
	public partial class MultiViewer : Window
	{

		// Repeat DEclaratoin to srtop Erro messages
		public static  event EventHandler<LoadedEventArgs> BankDataLoaded;
		public static  event EventHandler<LoadedEventArgs> CustDataLoaded;
		public static  event EventHandler<LoadedEventArgs> DetDataLoaded;
		public string CurrentDb = "";
		//public static BankAccountViewModel bvm = MainWindow.bvm;
		//public static CustomerViewModel cvm = MainWindow.cvm;
		//public static DetailsViewModel dvm = MainWindow.dvm;

		//public BankCollection Bankcollection = BankCollection .Bankcollection;
		//public CustCollection Custcollection = CustCollection . Custcollection;
		//public DetCollection Detcollection = DetCollection.Detcollection;

		public MultiViewer ( )
		{
			InitializeComponent ( );
			BankGrid . ItemsSource = BankCollection. Bankcollection;
			CustomerGrid . ItemsSource = CustCollection. Custcollection;
			DetailsGrid . ItemsSource = DetCollection. Detcollection;

		}
		public void MultiViewer_DataLoaded ( object sender , LoadedEventArgs e )
		{
			if ( e . CallerDb == "BANKACCOUNT" )
			{
				this.BankGrid . Visibility = Visibility . Visible;
				this . BankGrid . ItemsSource = sender as BankCollection;
//				this . BankGrid . DataContext = sender as BankCollection; 
				 this . BankGrid . SelectedIndex = 0;
				this . BankGrid . SelectedItem = 0;
				this . BankGrid . Refresh ( );
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
	//			this . CustomerGrid . Visibility = Visibility . Visible;
	//			this . CustomerGrid . ItemsSource = sender as CustCollection;
	////			this . CustomerGrid . DataContext = sender as CustCollection;
	//			this . CustomerGrid . SelectedIndex = 0;
	//			this . CustomerGrid . SelectedItem = 0;
	//			this . CustomerGrid . Refresh ( );
			}
			else if ( e . CallerDb == "DETAILS" )
			{
		//		this . DetailsGrid . Visibility = Visibility . Visible;
		//		this . DetailsGrid . ItemsSource = sender as DetCollection;
		////		this . DetailsGrid . DataContext = sender as DetCollection;
		//		this . DetailsGrid . SelectedIndex = 0;
		//		this . DetailsGrid . SelectedItem = 0;
		//		this . DetailsGrid . Refresh ( );
			}


		}
		public void UpdateOnDataChange ( string CurrentDb, DataGridRowEditEndingEventArgs e )
		{
			SQLHandlers sqlh = new SQLHandlers  ();
			sqlh . UpdateAllDb ( CurrentDb , e ,2);
		}

		private async void Window_Loaded ( object sender , RoutedEventArgs e )
		{
			//			if (dvm.DetailsObs == null)
			//				DetailsViewModel.LoadDetailsTask (DetailsGrid);
			//			GetDetInstance ().LoadDetailsTask ();
			//			DetailsViewModel.LoadDetailsTask ();
			//BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Bankcollection );
			//			BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );
			//CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Custcollection );
			//			CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( cvm . CustomersObs );
			BankCollection . SubscribeToLoadedEvent ( BankCollection . Bankcollection );
			BankCollection b  =new BankCollection();
			b.LoadBankTaskInSortOrderasync( true, 0 );

			CustCollection . SubscribeToLoadedEvent ( CustCollection . Custcollection );
			CustCollection c  =new CustCollection();
			c . LoadCustomerTaskInSortOrderAsync ( true , 0 );

			DetCollection . SubscribeToLoadedEvent ( DetCollection.Detcollection );
			DetCollection d  =new DetCollection();
			d . LoadDetailsTaskInSortOrderAsync ( 0 , true );

			//DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Detcollection );
			//			BankGrid.DataContext = (bvm.BankAccountObs );
			//			CustomerGrid.DataContext = (CustomerViewModel.CustomersObs);
			//			DetailsGrid.DataContext = (dvm.DetailsObs );
		}

		private void Window_PreviewKeyDown ( object sender , System . Windows . Input . KeyEventArgs e )
		{
			if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
		}

		private void BankGrid_SelectionChanged ( object sender , System . Windows . Controls . SelectionChangedEventArgs e )
		{

		}

		private void Button_Click ( object sender , RoutedEventArgs e )
		{
			Console . WriteLine ($"BankGrid : {BankGrid.Items.Count}, Visibility : {BankGrid.Visibility.ToString()}");
		}

		private void ViewerGrid_RowEditEnding ( object sender , System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			UpdateOnDataChange ( CurrentDb, e );
		}
		#region focus events
		private void CustomerGrid_GotFocus ( object sender , RoutedEventArgs e )
		{ CurrentDb = "BANKACCOUNT"; }
		private void BankGrid_GotFocus ( object sender , RoutedEventArgs e )
		{CurrentDb = "CUSTOMER";}
		private void DetailsGrid_GotFocus ( object sender , RoutedEventArgs e )
		{CurrentDb = "DETAILS";}
		#endregion focus events
	}
}
