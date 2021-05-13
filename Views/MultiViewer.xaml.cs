using System;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for MultiViewer.xaml
	/// </summary>
	public partial class MultiViewer : Window
	{

		int bIndex = 0;
		int cIndex = 0;
		int dIndex = 0;

		#region DELEGATES / EVENTS Declarations

		// Repeat DEclaratoin to srtop Erro messages
		public static  event EventHandler<LoadedEventArgs> BankDataLoaded;
		public static  event EventHandler<LoadedEventArgs> CustDataLoaded;
		public static  event EventHandler<LoadedEventArgs> DetDataLoaded;

		#endregion DELEGATES / EVENTS Declarations

		#region DECLARATIONS

		public string CurrentDb = "";
		bool inprogress = false;

		#endregion DECLARATIONS

		#region EVENT HANDLERS

		public void MultiViewer_DataLoaded ( object sender , LoadedEventArgs e )
		{
			inprogress = true;
			if ( e . CallerDb == "BANKACCOUNT" )
			{
				inprogress = true;
				BankGrid . Visibility = Visibility . Visible;
				BankGrid . ItemsSource = sender as BankCollection;
				//				this . BankGrid . DataContext = sender as BankCollection; 
				this . BankGrid . SelectedIndex = bIndex;
				this . BankGrid . SelectedItem = cIndex;
				this . BankGrid . Refresh ( );
				inprogress = false;
				Console . WriteLine ($"Bank Grid Updated");
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
				inprogress = true;
				CustomerGrid . Visibility = Visibility . Visible;
				CustomerGrid . ItemsSource = sender as CustCollection;
				//			this . CustomerGrid . DataContext = sender as CustCollection;
				this . CustomerGrid . SelectedIndex = cIndex;
				this . CustomerGrid . SelectedItem = cIndex;
				this . CustomerGrid . Refresh ( );
				inprogress = false;
				Console . WriteLine ( $"Customer Grid Updated" );
			}
			else if ( e . CallerDb == "DETAILS" )
			{
				inprogress = true;
				DetailsGrid . Visibility = Visibility . Visible;
				DetailsGrid . ItemsSource = sender as DetCollection;
				//		this . DetailsGrid . DataContext = sender as DetCollection;
				this . DetailsGrid . SelectedIndex = dIndex;
				this . DetailsGrid . SelectedItem = dIndex;
				this . DetailsGrid . Refresh ( );
				inprogress = false;
				Console . WriteLine ( $"Details Grid Updated" );
			}
			ExtensionMethods . Refresh ( BankGrid );
			ExtensionMethods . Refresh ( CustomerGrid );
			ExtensionMethods . Refresh ( DetailsGrid );
			BankGrid . Focus ( );
			inprogress = false;
		}

		#endregion EVENT HANDLERS

		#region DATA UPDATING

		/// <summary>
		/// Handles the SQL updateof any changes made and updates all grids
		/// </summary>
		/// <param name="CurrentDb"></param>
		/// <param name="e"></param>
		public void UpdateOnDataChange ( string CurrentDb , DataGridRowEditEndingEventArgs e )
		{
			SQLHandlers sqlh = new SQLHandlers  ();
			sqlh . UpdateAllDb ( CurrentDb , e , 2 );
		}
		private void ViewerGrid_RowEditEnding ( object sender , System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			UpdateOnDataChange ( CurrentDb , e );
		}
		#endregion DATA UPDATING

		#region STARTUP/CLOSE

		public MultiViewer ( )
		{
			InitializeComponent ( );
			BankCollection bc = new BankCollection ( );
			BankGrid . ItemsSource = BankCollection .Bankcollection;
			CustomerGrid . ItemsSource = CustCollection . Custcollection;
			DetailsGrid . ItemsSource = DetCollection . Detcollection;
			this . MouseDown += delegate { DoDragMove ( ); };
			BankGrid . MouseDown += delegate { DoDragMove ( ); };
			CustomerGrid . MouseDown += delegate { DoDragMove ( ); };
			DetailsGrid . MouseDown += delegate { DoDragMove ( ); };
			Flags . SqlBankGrid = BankGrid;
			Flags . SqlCustGrid = CustomerGrid;
			Flags . SqlDetGrid = DetailsGrid;
			Flags . MultiViewer = this;
		}
		private async void Window_Loaded ( object sender , RoutedEventArgs e )
		{
			Flags . MultiViewer = this;
			BankCollection bc = new BankCollection ( );

			BankCollection . SubscribeToLoadedEvent ( BankCollection.Bankcollection );
			BankCollection b  =new BankCollection();
			b . LoadBankTaskInSortOrderasync ( true , 0 );

			CustCollection . SubscribeToLoadedEvent ( CustCollection . Custcollection );
			CustCollection c  =new CustCollection();
			c . LoadCustomerTaskInSortOrderAsync ( true , 0 );

			DetCollection . SubscribeToLoadedEvent ( DetCollection . Detcollection );
			DetCollection d  =new DetCollection();
			d . LoadDetailsTaskInSortOrderAsync ( true , 0 );

			//DataGridView dgv = new DataGridView ();
			//			DataGridViewRow  dgv = sender  as DataGridView;
			//			dgv. DefaultCellStyle . SelectionBackColor = Color . Blue;
			//			BankGrid . DefaultCellStyle . SelectionForeColor = Color . Red;
		}
		private void Window_Closing ( object sender , System . ComponentModel . CancelEventArgs e )
		{
			BankCollection bc = new BankCollection ( );
			BankCollection . UnSubscribeToLoadedEvent ( BankCollection . Bankcollection );
			CustCollection . UnSubscribeToLoadedEvent ( CustCollection . Custcollection );
			DetCollection . UnSubscribeToLoadedEvent ( DetCollection . Detcollection );
			Flags . SqlBankGrid = null;
			Flags . SqlCustGrid = null;
			Flags . SqlDetGrid = null;
			Flags . MultiViewer = null;
		}

		private async void ReLoadAllDataBases ( )
		{
			BankCollection b  =new BankCollection();
			await b . LoadBankTaskInSortOrderasync ( true , 0 );
			CustCollection c  =new CustCollection();
			await c . LoadCustomerTaskInSortOrderAsync ( true , 0 );
			DetCollection d  =new DetCollection();
			await d . LoadDetailsTaskInSortOrderAsync ( true , 0 );
		}
		private void Close_Click ( object sender , RoutedEventArgs e )
		{
			Close ( );
		}

		#endregion STARTUP/CLOSE

		private void Window_PreviewKeyDown ( object sender , System . Windows . Input . KeyEventArgs e )
		{
			if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
			else if ( e . Key == Key . Escape )
			{
				Close ( );
			}
			else if ( e . Key == Key . Home )
			{
				BankGrid . SelectedIndex = 0;
				CustomerGrid . SelectedIndex = 0;
				DetailsGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( BankGrid );
				ExtensionMethods . Refresh ( CustomerGrid );
				ExtensionMethods . Refresh ( DetailsGrid );
			}
			else if ( e . Key == Key . End )
			{
				BankGrid . SelectedIndex = BankGrid . Items . Count - 1;
				CustomerGrid . SelectedIndex = CustomerGrid . Items . Count - 1;
				DetailsGrid . SelectedIndex = DetailsGrid . Items . Count - 1;
				BankGrid . SelectedItem = BankGrid . Items . Count - 1;
				CustomerGrid . SelectedItem = CustomerGrid . Items . Count - 1;
				DetailsGrid . SelectedItem = DetailsGrid . Items . Count - 1;
				BankGrid . ScrollIntoView ( BankGrid . Items . Count - 1 );
				CustomerGrid . ScrollIntoView ( CustomerGrid . Items . Count - 1 );
				DetailsGrid . ScrollIntoView ( DetailsGrid . Items . Count - 1 );
				ExtensionMethods . Refresh ( BankGrid );
				ExtensionMethods . Refresh ( CustomerGrid );
				ExtensionMethods . Refresh ( DetailsGrid );
			}
		}

		#region DATAGRID HANDLERS
		private void DataGrids_SelectionChanged ( object sender , System . Windows . Controls . SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;
			SetScrollVariables ( sender );
			DataGrid dg = sender as   DataGrid;
			BankData . DataContext = CustomerGrid . SelectedItem;
			if ( inprogress ) return;
			//			if ( inprogress ) return;
			if ( sender == BankGrid )
			{
				int  currsel = BankGrid.SelectedIndex;
				BankAccountViewModel dgr = BankGrid.SelectedItem as BankAccountViewModel ;
				if ( dgr == null ) return;
				rec = FindMatchingRecord ( dgr . CustNo , CustomerGrid , "CUSTOMER" );
				CustomerGrid . SelectedIndex = rec;
//				Utils . ScrollRecordIntoView ( CustomerGrid , 1 );

				rec = FindMatchingRecord ( dgr . CustNo , DetailsGrid , "DETAILS" );
				DetailsGrid . SelectedIndex = rec;
				bIndex = currsel;
				Utils . ScrollRecordIntoView ( DetailsGrid , 1 );
				return;
				{
					scroll = SqlDbViewer . GetScrollViewer ( CustomerGrid ) as ScrollViewer;
					if ( scroll != null )
					{
						scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex - 5 );
						Application . Current . Dispatcher . Invoke ( ( ) =>
						{
							scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						} );
					}
					DetailsGrid . SelectedIndex = currsel;
					DetailsGrid . ScrollIntoView ( currsel );
					scroll = SqlDbViewer . GetScrollViewer ( DetailsGrid ) as ScrollViewer;
					if ( scroll != null )
					{
						scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex - 5 );
						Application . Current . Dispatcher . Invoke ( ( ) =>
						{
							scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						} );
					}
				}
			}
			if ( sender == CustomerGrid )
			{
				int  currsel = CustomerGrid .SelectedIndex;
				CustomerViewModel dgr = CustomerGrid.SelectedItem as CustomerViewModel ;
				if ( dgr == null ) return;
				rec = FindMatchingRecord ( dgr . CustNo , BankGrid , "BANKACCOUNT" );
				BankGrid . SelectedIndex = rec;
//				Utils . ScrollRecordIntoView ( BankGrid , 1 );

				rec = FindMatchingRecord ( dgr . CustNo , DetailsGrid , "DETAILS" );
				this . DetailsGrid . SelectedIndex = rec;
				cIndex = currsel;
//				Utils . ScrollRecordIntoView ( DetailsGrid , 1 );
				return;
				{
					Utils . ScrollRecordIntoView ( BankGrid , 1 );
					Utils . ScrollRecordIntoView ( DetailsGrid , 1 );
					return;
					currsel = CustomerGrid . SelectedIndex;
					BankGrid . SelectedIndex = currsel;
					scroll = SqlDbViewer . GetScrollViewer ( BankGrid ) as ScrollViewer;
					if ( scroll != null )
					{
						scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						Application . Current . Dispatcher . Invoke ( ( ) =>
						{
							scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						} );
					}
					DetailsGrid . SelectedIndex = CustomerGrid . SelectedIndex;
					scroll = SqlDbViewer . GetScrollViewer ( DetailsGrid ) as ScrollViewer;
					if ( scroll != null )
					{
						scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						Application . Current . Dispatcher . Invoke ( ( ) =>
						{
							scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						} );
					}
				}
			}
			if ( sender == DetailsGrid )
			{
				int  currsel = DetailsGrid .SelectedIndex;
				DetailsViewModel dgr = DetailsGrid.SelectedItem as DetailsViewModel ;
				if ( dgr == null ) return;
				rec = FindMatchingRecord ( dgr . CustNo , CustomerGrid , "CUSTOMER" );
				CustomerGrid . SelectedIndex = rec;
				Utils . ScrollRecordIntoView ( CustomerGrid , 1 );

				rec = FindMatchingRecord ( dgr . CustNo , BankGrid , "BANKACCOUNT" );
				BankGrid . SelectedIndex = rec;
				dIndex = currsel;
				Utils . ScrollRecordIntoView ( BankGrid , 1 );
				return;
				{
					Utils . ScrollRecordIntoView ( BankGrid , 1 );
					Utils . ScrollRecordIntoView ( CustomerGrid , 1 );
					return;
					currsel = DetailsGrid . SelectedIndex;
					BankGrid . SelectedIndex = currsel;
					CustomerGrid . SelectedIndex = BankGrid . SelectedIndex;
					scroll = SqlDbViewer . GetScrollViewer ( BankGrid ) as ScrollViewer;
					if ( scroll != null )
					{
						scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						Application . Current . Dispatcher . Invoke ( ( ) =>
						{
							scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						} );
					}
					scroll = SqlDbViewer . GetScrollViewer ( CustomerGrid ) as ScrollViewer;
					if ( scroll != null )
					{
						scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						Application . Current . Dispatcher . Invoke ( ( ) =>
						{
							scroll . ScrollToVerticalOffset ( BankGrid . SelectedIndex );
						} );
					}
					CustomerGrid . Refresh ( );
				}
			}
		}

		#endregion DATAGRID HANDLERS

		#region focus events
		private void CustomerGrid_GotFocus ( object sender , RoutedEventArgs e )
		{ CurrentDb = "CUSTOMER"; }
		private void BankGrid_GotFocus ( object sender , RoutedEventArgs e )
		{ CurrentDb = "BANKACCOUNT"; }
		private void DetailsGrid_GotFocus ( object sender , RoutedEventArgs e )
		{ CurrentDb = "DETAILS"; }
		#endregion focus events

		#region SCROLLBARS

		// scroll bar movement is automatically   stored by these three methods
		// So we can use them to reset position CORRECTLY after refreshes
		private void BankGrid_ScrollChanged ( object sender , ScrollChangedEventArgs e )
		{
			int rec = 0;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg);
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			CustomerGrid . ScrollIntoView ( DetailsGrid . SelectedIndex );
			DetailsGrid . ScrollIntoView ( CustomerGrid . SelectedIndex );

		}
		private void CustomerGrid_ScrollChanged ( object sender , ScrollChangedEventArgs e )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			BankGrid . ScrollIntoView ( CustomerGrid . SelectedIndex );
			DetailsGrid . ScrollIntoView ( CustomerGrid . SelectedIndex );
		}

		private void DetailsGrid_ScrollChanged ( object sender , ScrollChangedEventArgs e )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg);
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			CustomerGrid . ScrollIntoView ( DetailsGrid . SelectedIndex );
			BankGrid . ScrollIntoView ( DetailsGrid . SelectedIndex );
		}
		#endregion SCROLLBARS

		#region Scroll bar utilities
		public void SetScrollVariables ( object sender )
		{
			SetTopViewRow ( sender );
			SetBottomViewRow ( sender );
			SetViewPort ( sender );
		}

		public void SetTopViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)sender );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert.ToInt32(d);
			if ( dg == BankGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == CustomerGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == DetailsGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleDetGridRow = ( double ) rounded;
			}
			//			Flags . ViewPortHeight = scroll . ViewportHeight;
		}

		public void SetBottomViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert.ToInt32(d);
			if ( dg == BankGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == CustomerGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == DetailsGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleDetGridRow = ( double ) rounded;
			}
		}
		public void SetViewPort ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg);
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			Flags . ViewPortHeight = scroll . ViewportHeight;
		}

		private int FindMatchingRecord ( string Custno , DataGrid Grid , string CurrentDb )
		{
			int index = 0;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				foreach ( var item in BankGrid . Items )
				{
					BankAccountViewModel cvm = item as  BankAccountViewModel ;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno )
					{
						break;
					}
					index++;
				}
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				foreach ( var item in CustomerGrid . Items )
				{
					CustomerViewModel cvm = item as     CustomerViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno )
					{
						break;
					}
					index++;
				}
			}
			else if ( CurrentDb == "DETAILS" )
			{
				foreach ( var item in DetailsGrid . Items )
				{
					DetailsViewModel cvm = item as     DetailsViewModel ;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno )
					{
						break;
					}
					index++;
				}
			}
			return index;
		}

		#endregion Scroll bar utilities

		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....     cos it has to be the primary button !!!
			try
			{ DragMove ( ); }
			catch ( Exception ex )
			{ Console . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" ); return; }
		}



		/// <summary>
		/// Limit datagrid content to multiple accounts data only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Details_Click ( object sender , RoutedEventArgs e )
		{

		}

		private void Filter_Click ( object sender , RoutedEventArgs e )
		{
			Filtering f = new Filtering();
			if ( CurrentDb == "" )
			{
				MessageBox . Show ( "Please select an entry in one of the data grids before trying to filter the data listed." );
				return;
			}
			Flags . FilterCommand = f . DoFilters ( sender , CurrentDb , 1 );
			ReLoadAllDataBases ( );
		}

		private void BankGrid_Selected ( object sender , RoutedEventArgs e )
		{
			// hit when grid selection is changed by anything
			int x = 0;
			//Console . WriteLine("...");
		}

		private async void Refresh_Click ( object sender , RoutedEventArgs e )
		{
			bIndex = BankGrid.SelectedIndex;
			cIndex = CustomerGrid.SelectedIndex;
			dIndex = DetailsGrid.SelectedIndex;
			// Refresh all grids
			Mouse . OverrideCursor = Cursors . Wait;
			BankGrid . ItemsSource = null;
			CustomerGrid . ItemsSource = null;
			DetailsGrid . ItemsSource = null;
			BankCollection bc = new BankCollection();
			await bc . LoadBankTaskInSortOrderasync ( true , 0 );
			CustCollection cc = new CustCollection ();
			await cc . LoadCustomerTaskInSortOrderAsync ( true , 0 );
			DetCollection dc = new DetCollection ();
			await dc . LoadDetailsTaskInSortOrderAsync ( true , 0 );
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		private void Db_Click ( object sender , RoutedEventArgs e )
		{
			CustomerDbView cdbv = new CustomerDbView();
			cdbv . Show ( );
		}
	}
}
