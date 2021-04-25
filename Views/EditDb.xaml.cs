using System . Windows;
using System;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Data . SqlClient;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . Views;
using System . Threading;
using System . Threading . Tasks;
using WPFPages . ViewModels;

namespace WPFPages . Views
{

	//	public delegate void SqlSelChange ( int RowIndex, object selRowItem );
	//	public delegate void SelectedRowChanged ( int row, string CurentDb );

	/// <summary>
	/// Interaction logic for EditDb.xaml
	/// </summary>


	public partial class EditDb : INotifyPropertyChanged
	{
		public static BankAccountViewModel bvm = MainWindow . bvm;
		public static CustomerViewModel cvm = MainWindow . cvm;
		public static DetailsViewModel dvm = MainWindow . dvm;

		// Event we CONSUME on SqlViewer selectedIndex change
		public static event SQLViewerGridSelectionChanged EditDbSelChange;
		// Event we TRIGGER to notify SqlViewer of  a selectedindex change
		public static event EditDbRowChange EditDbViewerSelectedIndexChanged;

		//		public static event SqlSelectedRowChanged SqlViewerIndexChanged;


		DataTable dt = new DataTable ( );
		SQLDbSupport sqlsupport = new SQLDbSupport ( );
		public bool RemoteIndexChange = false;
		int CurrentIndex = -1;
		object CurrentItem = null;
		SqlDbViewer sqldbv = null;
		public static DataGrid CurrentGrid = null;
		public string CurrentDb = "";
		//		bool bDirty = false;
		//		bool testing = false;
		public EventHandlers EventHandler = null;
		public BankAccountViewModel Bank;

		//private BankAccountViewModel BankCurrentRowAccount;
		//private CustomerViewModel CustomerCurrentRowAccount;
		//private DetailsViewModel DetailsCurrentRowAccount;
		//private DataGridRow BankCurrentRow;
		//private DataGridRow CustomerCurrentRow;
		//private DataGridRow DetailsCurrentRow;

		private SQLEditOcurred SqlEditOccurred = HandleSQLEdit;
		private EditEventArgs EditArgs = null;
		public Task mainTask = null;
		public static object thisWindow = null;
		public static bool SqlUpdating = false;

		/// <summary>
		/// CONSTRUCTOR for Data Editing Window
		/// </summary>		
		public EditDb ( string Caller, int index, object Item, SqlDbViewer sqldb )
		{
			//Get handle to SQLDbViewer Window

			sqldbv = sqldb;
			CurrentIndex = index;
			CurrentItem = Item;
			CurrentDb = Caller;
			InitializeComponent ( );
			SetupBackgroundGradient ( );
			thisWindow = this;
		}

		//		Trigger event to notify SqlDbViewer of index changes
		private void NotifyViewerofEditIndexChange ( int row, string CurentDb )
		{
			if ( !Flags . EditDbChangeHandled )
				return;

			EventHandlers ev = new EventHandlers ( );
			//			ev . ShowSubscribersCount ( );
			if ( EventHandlers . EditDbViewerSelectedIndexChanged != null )
			{
				//				Flags . EditDbIndexTriggered = true;
				EventHandlers . EditDbViewerSelectedIndexChanged . Invoke ( row, CurrentDb );
				Flags . EditDbChangeHandled = false;

			}
		}


		/// <summary>
		///  A CallBack that RECEIVES notifications from SqlDbViewer on SelectIndex changes
		///  so that we can update our row position to match
		///  EVENT = public static event SqlSelectedRowChanged SqlViewerIndexChanged;
		/// SendViewerIndexChange ( index, CurrentDb );
		/// </summary>
		/// <param name="row"></param>
		/// <param name="CurrentDb"></param>
		public void OnSqlViewerIndexChanged ( int row, string CurrentDb )
		{
			Flags . EditDbChangeHandled = true;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				DataGrid1 . SelectedItem = null;  //Clear current selection to avoid multiple selections
				RemoteIndexChange = true;
				DataGrid1 . SelectedIndex = row;
				DataGrid1 . SelectedItem = row;
				DataGrid1 . ScrollIntoView ( DataGrid1 . SelectedItem );
				DataGrid1 . Refresh ( );
				RemoteIndexChange = false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				DataGrid2 . SelectedItem = null;    //Clear current selection to avoid multiple selections
				RemoteIndexChange = true;
				DataGrid2 . SelectedIndex = row;
				DataGrid2 . SelectedItem = row;
				DataGrid2 . ScrollIntoView ( DataGrid2 . SelectedItem );
				DataGrid2 . Refresh ( );
				RemoteIndexChange = false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsGrid . SelectedItem = null;  //Clear current selection to avoid multiple selections
				DetailsGrid . SelectedIndex = row;
				DetailsGrid . SelectedItem = row;
				DetailsGrid . ScrollIntoView ( DetailsGrid . SelectedItem );
				DetailsGrid . Refresh ( );
				RemoteIndexChange = false;
			}
		}
		private void TriggerEditOccurred ( string Caller, int index, object datatype )
		{
			EditArgs . Caller = Caller;
			EditArgs . CurrentIndex = index;
			EditArgs . DataType = datatype;
			SqlEditOccurred ( this, EditArgs );
		}
		public static void HandleSQLEdit ( object sender, EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			Console . WriteLine ( $"\r\nDelegate Recieved in EDITDB (83) Caller={e . Caller}, Index = {e . CurrentIndex},   {e . DataType . ToString ( )} \r\n" );
			//We no have at our disposal (in e) the entire Updated record, including its Index in the sender grid
			// plus we know its Model type from e.Caller (eg"BANKACCOUNT") 
			//and we even have a pointer to the datagrid in the sender parameter
			int RowToFind = -1;
			if ( thisWindow != null )
			{
				//only try this if we actually have an EditDb window open
				if ( e . Caller == "BANKACCOUNT" )
				{

					//We CANNOT ACCESS the Grids because we are in  a Static functoin - GRRRRRRRRRRRRRRRRRRRRR

					//if (DataGrid1.SelectedIndex != e.CurrentIndex)
					//{
					//	RowToFind = e.CurrentIndex;
					//	Console.WriteLine ($"EDITDB (88) HandleSQLEdit HANDLER() - Current index = {DataGrid1.SelectedIndex} received row change of {RowToFind} from SqlDbViewer()");
					//	DataGridNavigation.SelectRowByIndex (DataGrid1, RowToFind, -1);

					//	//Do we need to update the grid after an edit by SqlDbViewer ??
					//	if (BankAccountViewModel.SqlUpdating)
					//	{
					//		Console.WriteLine ($"\r\nEDITDB (787) HANDLESQLEDIT HANDLER() - Calling CollectionViewSource Function\r\n");
					//		//Clear Editing flag
					//		BankAccountViewModel.SqlUpdating = false;
					//		// update ourselves to show changes made in SqlDbviewer
					//		CollectionViewSource.GetDefaultView (DataGrid1.ItemsSource).Refresh ();
					//	}
					//}
				}
			}
		}
		private void SetupBackgroundGradient ( )
		{
			//Get a new LinearGradientBrush
			LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush ( );
			//Set the start and end points of the drawing
			myLinearGradientBrush . StartPoint = new Point ( 1.3, 0 );
			myLinearGradientBrush . EndPoint = new Point ( 0.0, 1 );
			if ( CurrentDb == "BANKACCOUNT" )
			// Gradient Stops below are light to dark
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . PowderBlue, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . LightSteelBlue, 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DodgerBlue, 0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF, 0x6B, 0x8E, 0x95 );
				gs2 . Color = Color . FromArgb ( 0xFF, 0x14, 0xA7, 0xC1 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x1E, 0x42, 0x4E );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x1D, 0x48, 0x55 );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x1D, 0x48, 0x55 );
				gs6 . Color = Color . FromArgb ( 0xFF, 0x19, 0x3A, 0x44 );
				gs1 . Offset = 1;
				gs2 . Offset = 0.509;
				gs3 . Offset = 0.542;
				gs4 . Offset = 0.542;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . White, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Gold, 0.3 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkKhaki, 0.0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				//Yellow buttons
				gs1 . Color = Color . FromArgb ( 0xFF, 0x7a, 0x6f, 0x2d );
				gs2 . Color = Color . FromArgb ( 0xFF, 0xf5, 0xd8, 0x16 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x7d, 0x70, 0x15 );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x5e, 0x56, 0x2a );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x59, 0x50, 0x13 );
				gs6 . Color = Color . FromArgb ( 0xFF, 0x38, 0x32, 0x0c );
				gs1 . Offset = 1;
				gs2 . Offset = 0.209;
				gs3 . Offset = 0.342;
				gs4 . Offset = 0.442;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
			}
			if ( CurrentDb == "DETAILS" )
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . White, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Green, 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkGreen, 0.25 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF, 0x75, 0xDD, 0x75 );
				gs2 . Color = Color . FromArgb ( 0xFF, 0x00, 0xFF, 0x00 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x33, 0x66, 0x33 );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x44, 0x55, 0x44 );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x33, 0x55, 0x55 );
				gs6 . Color = Color . FromArgb ( 0xff, 0x22, 0x40, 0x22 );
				gs1 . Offset = 1;
				gs2 . Offset = 0.509;
				gs3 . Offset = 0.542;
				gs4 . Offset = 0.542;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );

			}
			// Use the brush to paint the rectangle.
			Background = myLinearGradientBrush;

		}
		#region OLbservable declarations
		//public ObservableCollection<BankAccountViewModel> _bankaccountCollection;
		//public ObservableCollection<Customer> _customeraccountCollection;
		//public ObservableCollection<DetailsViewModel> _detailsaccountCollection;

		//public ObservableCollection<BankAccountViewModel> BankAccountCollection
		//{
		//	get { return _bankaccountCollection; }
		//	set { _bankaccountCollection = value; OnPropertyChanged ("_bankaccountCollection"); }
		//}
		//public ObservableCollection<Customer> CustomerAccountCollection
		//{
		//	get { return _customeraccountCollection; }
		//	set { _customeraccountCollection = value; OnPropertyChanged ("_customeraccountCollection"); }
		//}
		//public ObservableCollection<DetailsViewModel> DetailsAccountCollection
		//{
		//	get { return _detailsaccountCollection; }
		//	set { _detailsaccountCollection = value; OnPropertyChanged ("_detailsaccountCollection"); }
		//}

		#endregion OLbservable declarations

		private void WindowLoaded ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankLabels . Visibility = Visibility . Visible;
				BankEditFields . Visibility = Visibility . Visible;
				DataGrid1 . Visibility = Visibility . Visible;

				CustomerLabelsGrid . Visibility = Visibility . Collapsed;
				CustomerEditFields . Visibility = Visibility . Collapsed;
				DetailsEditFields . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;

				DetailsGrid . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;

				this . Title += " Bank Accounts Db";

				// use this or the next one
				DataGrid1 . ItemsSource = bvm . BankAccountObs;
				DataGrid1 . DataContext = bvm . BankAccountObs;
				BankEditFields . DataContext = bvm . BankAccountObs;

				//DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );
				//BankEditFields.DataContext = CollectionViewSource.GetDefaultView (bvm.BankAccountObs );
				//DataGrid1.DataContext = CollectionViewSource.GetDefaultView (bvm.BankAccountObs );

				DataGrid1 . SelectedIndex = 0;
				DataGrid1 . SelectedItem = 0;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( DataGrid1, CurrentIndex, -1 );

				// set up our windows dragging
				this . MouseDown += delegate { DoDragMove ( ); };
				CurrentGrid = DataGrid1;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(242) Window just loaded : getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( DataGrid1, "EDITDB", out EventHandler );

				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				//				Flags.CurrentEditDbViewerBankGrid = DataGrid1;
				BankAccountViewModel . ActiveEditDbViewer = DataGrid1;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "BankAccount";


				DataGrid1 . Focus ( );
				DataGrid1 . BringIntoView ( );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				BankLabels . Visibility = Visibility . Collapsed;
				BankEditFields . Visibility = Visibility . Collapsed;
				DetailsEditFields . Visibility = Visibility . Collapsed;
				DataGrid1 . Visibility = Visibility . Collapsed;
				DetailsGrid . Visibility = Visibility . Collapsed;

				CustomerLabelsGrid . Visibility = Visibility . Visible;
				CustomerEditFields . Visibility = Visibility . Visible;
				DataGrid2 . Visibility = Visibility . Visible;

				this . Title += " Customer Accounts Db";

				// use this or the next one
				DataGrid2 . ItemsSource = cvm . CustomersObs;
				DataGrid2 . DataContext = cvm . CustomersObs;
				CustomerEditFields . DataContext = cvm . CustomersObs;

				//DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( cvm . CustomersObs );
				//DataGrid2 . DataContext = CollectionViewSource . GetDefaultView ( cvm . CustomersObs );
				//CustomerEditFields . DataContext = CollectionViewSource . GetDefaultView ( cvm . CustomersObs );

				DataGrid2 . SelectedIndex = CurrentIndex;
				DataGrid2 . SelectedItem = CurrentItem;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( DataGrid2, CurrentIndex, -1 );
				this . MouseDown += delegate { DoDragMove ( ); };
				CurrentGrid = DataGrid2;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(287) Window just loaded :  getting instance of EventHandlers class with this,DataGrid2,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( DataGrid2, "EDITDB", out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				//				Flags.CurrentEditDbViewerCustomerGrid = DataGrid2;
				BankAccountViewModel . ActiveEditDbViewer = DataGrid2;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Customer";

				DataGrid2 . Focus ( );
				DataGrid2 . BringIntoView ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				BankEditFields . Visibility = Visibility . Collapsed;
				DataGrid1 . Visibility = Visibility . Collapsed;
				CustomerLabelsGrid . Visibility = Visibility . Collapsed;
				CustomerEditFields . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;
				DataGrid1 . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;

				BankLabels . Visibility = Visibility . Visible;
				DetailsEditFields . Visibility = Visibility . Visible;
				DetailsGrid . Visibility = Visibility . Visible;

				this . Title += " Secondary Accounts Db";

				// use this or the next one
				DetailsGrid . ItemsSource = dvm . DetailsObs;
				DetailsGrid . DataContext = dvm . DetailsObs;
				DetailsEditFields . DataContext = dvm . DetailsObs;

				//this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( dvm . DetailsObs );
				//DetailsGrid.DataContext = CollectionViewSource.GetDefaultView (dvm.DetailsObs );
				//DetailsEditFields.DataContext = CollectionViewSource.GetDefaultView (dvm.DetailsObs );

				DetailsGrid . SelectedIndex = 0;
				DetailsGrid . SelectedItem = 0;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( DetailsGrid, CurrentIndex, -1 );
				this . MouseDown += delegate { DoDragMove ( ); };
				CurrentGrid = DetailsGrid;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(312) Window just loaded :  getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( DetailsGrid, "DETAILS", out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				//				Flags.CurrentEditDbViewerDetailsGrid = DetailsGrid;
				BankAccountViewModel . ActiveEditDbViewer = DetailsGrid;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Details";
				DetailsGrid . Focus ( );
				DetailsGrid . BringIntoView ( );
			}

			MainWindow.gv.SqlCurrentEditViewer = this;
			// subscribe to SqlViewerIndexChanged EVENT. Assign Handling to THIS FILE ->  OnSqlViewerIndexChanged;
			// SqlViewerIndexChanged  is the Trigger function (In Sqlviewer) we call on index change
			EventHandlers . SqlViewerIndexChanged += MainWindow . gv . SqlCurrentEditViewer . OnSqlViewerIndexChanged;
			EventHandlers ev = new EventHandlers ( );
			ev . ShowSubscribersCount ( );

		}

		private void Cancel_Click ( object sender, RoutedEventArgs e )
		{
			sqldbv = null;
			if ( CurrentDb == "BANKACCOUNT" )
				BankAccountViewModel . ClearFromEditDbList ( DataGrid1, CurrentDb );
			else if ( CurrentDb == "CUSTOMER" )
				BankAccountViewModel . ClearFromEditDbList ( DataGrid2, CurrentDb );
			else if ( CurrentDb == "DETAILS" )
				BankAccountViewModel . ClearFromEditDbList ( DetailsGrid, CurrentDb );
			//			EventHandlers . ClearWindowHandles ( this, null );
			BankAccountViewModel . EditdbWndBank = null;
			//			tokenSource.Cancel ();
			Close ( );
		}


		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}


		#endregion

		#region RowEdithandlers

		//Bank Grid
		private void DataGrid1_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			//BankAccountViewModel ss = null;
			//CustomerViewModel cs = null;
			//DetailsViewModel sa = null;

			////Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			//if (e == null)
			//{
			//	if (CurrentDb == "BANKACCOUNT")
			//	{
			//		SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row);
			//		SqlUpdating = true;
			//		return;
			//	}
			//}
			//else
			//{
			//	if (CurrentDb == "BANKACCOUNT")
			//	{
			//		BankAccountViewModel.SqlUpdating = true;
			//		SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row);
			//		return;
			//	}
			//}
		}
		//Customer grid
		private void DataGrid2_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			//BankAccountViewModel ss = null;
			//CustomerViewModel cs = null;
			//DetailsViewModel sa = null;

			////Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			//if (e == null)
			//{
			//	if (CurrentDb == "BANKACCOUNT")
			//	{
			//		SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row);
			//		SqlUpdating = true;
			//		return;
			//	}
			//}
			//else
			//{
			//	if (CurrentDb == "BANKACCOUNT")
			//	{
			//		BankAccountViewModel.SqlUpdating = true;
			//		SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row);
			//		return;
			//	}
			//}
		}
		//Details Grid
		private void DetailsGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			//BankAccountViewModel ss = null;
			//CustomerViewModel cs = null;
			//DetailsViewModel sa = null;

			////Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			//if (e == null)
			//{
			//	if (CurrentDb == "DETAILS")
			//	{
			//		SQLHandlers.UpdateDbRow ("DETAILS", e.Row);
			//		SqlUpdating = true;
			//	}
			//}
			//else
			//{
			//	if (CurrentDb == "DETAILS")
			//	{
			//		DetailsViewModel.SqlUpdating = true;
			//		SQLHandlers.UpdateDbRow ("DETAILS", e.Row);
			//		return;
			//	}
			//}
		}

		#endregion RowEdithandlers

		#region RowSelection handlers

		public void ViewerHasChangedIndex ( int newRow, string CurrentDb )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				DataGrid1 . SelectedIndex = newRow;

			}
			else if ( CurrentDb == "CUSTOMER" )
			{

			}
			else if ( CurrentDb == "DETAILS" )
			{

			}
		}
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid1_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( DataGrid1 . SelectedIndex == -1 ) return;
			if ( !Flags . EditDbChangeHandled )
				Flags . EditDbChangeHandled = true;
			if ( sqldbv == null ) return;
			if ( RemoteIndexChange ) return;
			//we need to notify SQLDBVIEWER so it can update its selected index
			Flags . EditDbChangeHandled = true;
			NotifyViewerofEditIndexChange ( DataGrid1 . SelectedIndex, "BANKACCOUNT" );
		}

		//Customer Db
		private void DataGrid2_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( DataGrid2 . SelectedIndex == -1 ) return;
			if ( !Flags . EditDbChangeHandled )
				Flags . EditDbChangeHandled = true;

			// Handle Customer Row selection
			//Sanity check
			if ( sqldbv == null ) return;
			if ( RemoteIndexChange ) return;
			if (DataGrid2.SelectedIndex == -1) return;
			{
				//if ( DataGrid2 . SelectedIndex == -1 ) DataGrid2 . SelectedIndex = 0;
				//CustomerViewModel objItemToEdit = DataGrid2 . SelectedItem as CustomerViewModel;
				//DataGrid dg = ( DataGrid ) DataGrid2;
				//int rowIndex = dg . SelectedIndex;
				//object item = dg . Items [ rowIndex ]; // = Product X
				//dg . SelectedItem = item;

				//DataGridRow row = dg . ItemContainerGenerator . ContainerFromIndex ( rowIndex ) as DataGridRow;
			}

			Flags . EditDbChangeHandled = true;
			NotifyViewerofEditIndexChange ( DataGrid2 . SelectedIndex, "CUSTOMER" );
			Flags . EditDbChangeHandled = false;
		}

		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DetailsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			// THIS WORKS CORRECTLY IN THAT THE MAIN WINDOW SCROLLS CORRECTLY WHEN SELECTION IS CHANGED
			if ( DetailsGrid. SelectedIndex == -1 ) return;
			if ( !Flags . EditDbChangeHandled )
				Flags . EditDbChangeHandled = true;
			// Handle the SecAccounts (DETAILS) Edit Window
			if ( sqldbv == null ) return;
			if ( RemoteIndexChange ) return;
			{
				////Sanity check
				//if ( DetailsGrid . SelectedIndex == -1 ) DetailsGrid . SelectedIndex = 0;
				//DetailsViewModel objItemToEdit = DetailsGrid . SelectedItem as DetailsViewModel;
				//DataGrid dg = ( DataGrid ) sender;
				//DataRowView SelRow = dg . SelectedItem as DataRowView;
				//MainWindow . DgControl . SelectedIndex = DetailsGrid . SelectedIndex;
				//MainWindow . DgControl . EditSelChange = true;
			}
			Flags . EditDbChangeHandled = true;
			NotifyViewerofEditIndexChange ( DetailsGrid . SelectedIndex, "DETAILS" );
			Flags . EditDbChangeHandled = false;
		}
		#endregion RowSelection handlers


		//*******************************************************************************************************//
		// EVENT HANDLER
		//*******************************************************************************************************//


		public void UpdateSelection ( int row, object Grid )
		{
			//Dispatcher.Invoke (() =>
			//{
			//	DataGrid1.SelectedIndex = row;
			//});


		}
		#region EventHandler
		/// <summary>
		///  this is called by an ~Event triggered by SqlDBViewer changing it's SelectedIndex
		///  so EditDb  can update to match
		/// </summary>
		/// <param name="selectedRow"></param>
		/// <param name="caller"></param>
		public void resetEditDbindex ( bool self, int RowToFind, DataGrid caller )
		{
			int SelectedIdRow = -1;

			if ( DataGrid1 . ItemsSource == null ) return;
			//			if (!Flags.isEditDbCaller)
			//				return;
			if ( caller . Name == "BankGrid" )
			{
				Console . WriteLine ( $"resetDbIndex : - current {DataGrid1 . SelectedIndex} new - {RowToFind}" );
				if ( DataGrid1 . SelectedIndex != RowToFind )
				{
					DataGrid1 . SelectedIndex = RowToFind;
					//1st we get the specified row selected in the grid
					if ( Flags . EventHandlerDebug )
					{
						Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - EDITDB (769) RESETETDBINDEX HANDLER() - Current index = {DataGrid1 . SelectedIndex} received row change of {RowToFind} from SqlDbViewer()" );
					}
					if ( BankAccountViewModel . CurrentSelectedIndex != -1 )
					{
						int temp = BankAccountViewModel . CurrentSelectedIndex;
						if ( temp == -1 )
							temp = RowToFind;
						DataGridNavigation . SelectRowByIndex ( DataGrid1, RowToFind, -1 );
					}
					else
						DataGridNavigation . SelectRowByIndex ( DataGrid1, RowToFind, -1 );

					//Clear our ID flag
					//BankAccountViewModel.CurrentSelectedIndex = -1;

					//Do we need to update the grid after an edit by SqlDbViewer ??
					if ( BankAccountViewModel . SqlUpdating )
					{
						//						Console.WriteLine ($"\r\nEDITDB (787) RESETETDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						//Clear Editing flag
						BankAccountViewModel . SqlUpdating = false;
						// update ourselves to show changes made in SqlDbviewer
						CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( );
					}
				}
				//Now scroll it into view if necessary
				if ( DataGrid1 . SelectedIndex == -1 )
				{
					DataGrid1 . SelectedIndex = 0;
					DataGrid1 . SelectedItem = 0;
				}
				DataGrid1 . ScrollIntoView ( DataGrid1 . SelectedItem );
				var view = CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource );
				Console . WriteLine ( $"CollectionViewSource is type : {view . GetType ( )}" );
				Console . WriteLine ( $"DataGrid is type : {DataGrid1 . ItemsSource . GetType ( )}" );

			}
			else if ( caller . Name == "CustomerGrid" )
			{
				Console . WriteLine ( $"resetDbIndex : - Caller : {caller . Name}, current Destination row {DataGrid2 . SelectedIndex}, rowToFind- {RowToFind}" );
				if ( DataGrid2 . SelectedIndex != RowToFind )
				{
					if ( Flags . EventHandlerDebug )
					{
						Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - EDITDB (804) RESETETDBINDEX HANDLER() - Current index = {DataGrid2 . SelectedIndex} received row change of {RowToFind} from SqlDbViewer()" );
					}
					DataGrid2 . SelectedIndex = RowToFind;
					DataGrid2 . SelectedItem = RowToFind;
					if ( CustomerViewModel . CurrentSelectedIndex != -1 )
					{
						int temp = CustomerViewModel . CurrentSelectedIndex;
						if ( temp == -1 )
							temp = RowToFind;
						//SelectedIdRow = FindRowById (caller, temp);
						//if (SelectedIdRow != -1)
						//{
						//	DataGridNavigation.SelectRowByIndex (DataGrid2, SelectedIdRow, -1);
						//}
						//else
						DataGridNavigation . SelectRowByIndex ( DataGrid2, RowToFind, -1 );
					}
					else
					{
						//						DataGridNavigation.SelectRowByIndex (DataGrid2, RowToFind, -1);
					}

					//Clear our ID flag
					CustomerViewModel . CurrentSelectedIndex = -1;

					//Do we need to update the grid after an edit by SqlDbViewer ??
					if ( CustomerViewModel . SqlUpdating )
					{
						//						Console.WriteLine ($"\r\nEDITDB (787) RESETETDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						//Clear Editing flag
						CustomerViewModel . SqlUpdating = false;
						// update ourselves to show changes made in SqlDbviewer
						CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( );
					}
				}

				var view = CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource );
				Console . WriteLine ( $"CollectionViewSource is type : {view . GetType ( )}" );
				Console . WriteLine ( $"DataGrid2 is type : {DataGrid2 . ItemsSource . GetType ( )}" );
				DataGrid2 . ScrollIntoView ( DataGrid2 . SelectedItem, null );
				CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( );
			}
			else if ( caller . Name == "DetailsGrid" )
			{
				Console . WriteLine ( $"resetDbIndex : - current {DetailsGrid . SelectedIndex} new - {RowToFind}" );
				if ( DetailsGrid . SelectedIndex != RowToFind )
				{
					if ( Flags . EventHandlerDebug )
					{
						Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - EDITDB (839) RESETETDBINDEX HANDLER() - Current index = {DetailsGrid . SelectedIndex} received row change of {RowToFind} from SqlDbViewer()" );
					}
					if ( DetailsViewModel . CurrentSelectedIndex != -1 )
					{
						int temp = DetailsViewModel . CurrentSelectedIndex;
						if ( temp == -1 )
							temp = RowToFind;
						//						SelectedIdRow = FindRowById (caller, temp);
						//if (SelectedIdRow != -1)
						//{
						//	DataGridNavigation.SelectRowByIndex (DetailsGrid, SelectedIdRow, -1);
						//}
						//else
						DataGridNavigation . SelectRowByIndex ( DetailsGrid, RowToFind, -1 );
					}
					else
						DataGridNavigation . SelectRowByIndex ( DetailsGrid, RowToFind, -1 );

					//Clear our ID flag
					DetailsViewModel . CurrentSelectedIndex = -1;

					//Do we need to update the grid after an edit by SqlDbViewer ??
					if ( DetailsViewModel . SqlUpdating )
					{
						//						Console.WriteLine ($"\r\nEDITDB (787) RESETETDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						//Clear Editing flag
						DetailsViewModel . SqlUpdating = false;
						// update ourselves to show changes made in SqlDbviewer
						var view = CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource );
						CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( );
						Console . WriteLine ( $"CollectionViewSource is type : {view . GetType ( )}" );
						Console . WriteLine ( $"DataGrid is type : {DetailsGrid . ItemsSource . GetType ( )}" );
					}
				}
				DetailsGrid . ScrollIntoView ( DetailsGrid . SelectedItem );

			}
			else
			{
				if ( Flags . EventHandlerDebug )
				{
					Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - EDITDBVIEWER (797) RESETETDBINDEX HANDLER() - Current index = {DataGrid2 . SelectedIndex} received row change of {RowToFind} from SqlDbViewer() for {caller . CurrentItem}" );
				}
				if ( DataGrid2 . SelectedIndex != RowToFind )
				{
					DataGrid2 . SelectedIndex = RowToFind;
					DataGridNavigation . SelectRowByIndex ( DataGrid2, DataGrid2 . SelectedIndex, -1 );
				}
				var view = CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource );
				CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( );
				Console . WriteLine ( $"CollectionViewSource is type : {view . GetType ( )}" );
				Console . WriteLine ( $"DataGrid is type : {DataGrid2 . ItemsSource . GetType ( )}" );
			}
		}

		private int FindRowById ( DataGrid caller, int SelectedId )
		{
			return -1;
			//Not needed currently because the columns are always in the
			////same sort sequence across BOTH SQL and DbEdit windows right now
			int rowindex = -1;
			return -1;

			BankAccountViewModel bvm = caller . ItemsSource as BankAccountViewModel;
			/// Iterate thru Grid to find the Row for matching ID (SelectedId)
			for ( int i = 0 ; i < caller . Items . Count ; i++ )
			{
				bvm = caller . CurrentItem as BankAccountViewModel;
				if ( bvm . Id == SelectedId )
				{
					rowindex = caller . SelectedIndex;
					break;
				}
			}
			return rowindex;
		}
		#endregion EventHandler

		//Bank Edit fields
		#region Bank Editing fields
		private void ActypeEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		private void BanknoEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		private void CustNoEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		private void BalanceEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		private void IntRateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		private void OpenDateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		private void CloseDateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid1 . ItemsSource ) . Refresh ( ); }
		#endregion Bank Editing fields

		//Customer edit fields
		#region Customer Editing fields
		private void BanknoEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void CustnoEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void openDateEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void CloseDateEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void AcTypeEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void Addr1Edit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void Addr2Edit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void MobileEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void PhoneEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void CountyEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void PcodeEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void DobEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void FirstnameEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void LastnameEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void TownEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void ODate2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void CDate2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		private void Dob2_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DataGrid2 . ItemsSource ) . Refresh ( ); }
		#endregion Customer Editing fields

		#region Details Editing fields

		private void ActypeEdit3LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }
		private void CustnoEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }
		private void BanknoEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }
		private void BalanceEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }
		private void IntRateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }
		private void OpenDateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }
		private void CloseDateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ CollectionViewSource . GetDefaultView ( DetailsGrid . ItemsSource ) . Refresh ( ); }

		#endregion Details Editing fields

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . Escape )
			{
				if ( CurrentDb == "BANKACCOUNT" )
					BankAccountViewModel . ClearFromEditDbList ( DataGrid1, CurrentDb );
				else if ( CurrentDb == "CUSTOMER" )
					BankAccountViewModel . ClearFromEditDbList ( DataGrid2, CurrentDb );
				else if ( CurrentDb == "DETAILS" )
					BankAccountViewModel . ClearFromEditDbList ( DetailsGrid, CurrentDb );
				//				EventHandlers . ClearWindowHandles ( this, null );
				BankAccountViewModel . EditdbWndBank = null;
				//				tokenSource.Cancel ();
				Close ( );
			}
			else if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
			else if ( e . Key == Key . Home )
				Application . Current . Shutdown ( );
		}

		private void Button_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
				BankAccountViewModel . ClearFromEditDbList ( DataGrid1, CurrentDb );
			else if ( CurrentDb == "CUSTOMER" )
				BankAccountViewModel . ClearFromEditDbList ( DataGrid2, CurrentDb );
			else if ( CurrentDb == "DETAILS" )
				BankAccountViewModel . ClearFromEditDbList ( DetailsGrid, CurrentDb );
			//			EventHandlers . ClearWindowHandles ( this, null );
			BankAccountViewModel . EditdbWndBank = null;
			//			mainTask.CreationOptions(CancellationToken);
			//			tokenSource.Cancel ();
			Close ( );
		}

		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....
			try
			{
				this . DragMove ( );
			}
			catch
			{
				return;
			}
		}

		private void Window_Closed ( object sender, EventArgs e )
		{
			//			EventHandlers . ClearWindowHandles ( this, null );
			// Clear EditDb pointers
			EventHandlers . SqlViewerIndexChanged -= MainWindow . gv . SqlCurrentEditViewer . OnSqlViewerIndexChanged;
			MainWindow . gv . SqlCurrentEditViewer = null;
			Flags . CurrentEditDbViewer = null;
		}

		private async void DataGrid1_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			// This ONLY called when a cell is edited
			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;
			int currindx = DataGrid1 . SelectedIndex;
			var curritem = DataGrid1 . SelectedItem;
			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "BANKACCOUNT", e . Row );
					SqlUpdating = true;
					return;
				}
			}
			else
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankAccountViewModel . SqlUpdating = true;
					SQLHandlers sqlh = new SQLHandlers ( );
					// This call updates the SQL Db and the main viewer is also updated correctly
					await sqlh . UpdateDbRow ( "BANKACCOUNT", e . Row );
					Flags . CurrentSqlViewer . BankGrid . ScrollIntoView ( curritem );
					return;
				}
			}
		}

		private async void DataGrid2_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			// This ONLY called when a cell is edited
			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "CUSTOMER" )
				{
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "CUSTOMER", e . Row );
					SqlUpdating = true;
					return;
				}
			}
			else
			{
				if ( CurrentDb == "CUSTOMER" )
				{
					BankAccountViewModel . SqlUpdating = true;
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "CUSTOMER", e . Row );
					return;
				}
			}
		}

		private async void DetailsGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			// This ONLY called when a cell is edited
			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "DETAILS" )
				{
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "DETAILS", e . Row );
					SqlUpdating = true;
				}
			}
			else
			{
				if ( CurrentDb == "DETAILS" )
				{
					DetailsViewModel . SqlUpdating = true;
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "DETAILS", e . Row );
					return;
				}
			}
		}
		private void DataGrid2_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			RemoteIndexChange = false;
		}

		private void DataGrid1_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			RemoteIndexChange = false;
		}

		private void DetailsGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			RemoteIndexChange = false;
		}
		#region Task experimentation - NO LONGER USED
		//***********************************************************************************//
		//************************Task to handle selection changes**********************************//
		//***********************************************************************************//

		//		public async void HandleSelChange ()
		//		{

		////			if(tokenSource  != null)
		//			try
		//			{
		//				mainTask = Task.Run (() => looper (token), token);
		//			}
		//                        catch (ObjectDisposedException ex)
		//			{
		//				Console.WriteLine ($"\r\nObject Disposed Task Exception has occurred.{ex.Message}\r\n\r\n");
		//			}
		//			catch (OperationCanceledException ex)
		//			{
		//				Console.WriteLine ($"\r\nTask was cancelled by me.{ex.Message}\r\n\r\n");
		//			}
		//			catch (Exception ex)
		//			{
		//				Console.WriteLine ($"\r\nUnknown Task Exception has occurred.{ex.Message}\r\n\r\n");
		//			}
		//			finally
		//			{
		//		//		tokenSource.Dispose ();
		//			}

		//		}
		//		private async void looper (CancellationToken ct)
		//		{
		//			while (true)
		//			{
		////				if (ct.IsCancellationRequested)
		////					ct.ThrowIfCancellationRequested ();
		//				Thread.Sleep (350);
		//				if (MainWindow.DgControl.SqlSelChange == true)
		//				//only  do this  if called by SqlDbViewer, else it hangs big time
		//				{
		//					MainWindow.DgControl.SqlSelChange = false;
		//					Dispatcher.Invoke (() =>
		//					{
		//						try
		//						{
		//							Task task = Task.Factory.StartNew (UpdateGrid);
		//							Console.WriteLine ($"\r\nTask thread cancelled intentionally.\r\n\r\n");
		//						}
		//						catch (ObjectDisposedException ex)
		//						{
		//							Console.WriteLine ($"\r\nObject Disposed Task Exception has occurred.{ex.Message}\r\n\r\n");
		//						}
		//						catch (OperationCanceledException ex)
		//						{
		//							Console.WriteLine ($"\r\nTask was cancelled by me.{ex.Message}\r\n\r\n");
		//						}
		//						catch (Exception ex )
		//						{
		//							Console.WriteLine ($"\r\nUnknown Task Exception has occurred.{ex.Message}\r\n\r\n");
		//						}
		//						finally
		//						{
		//							//tokenSource.Dispose ();
		//						}
		//					});
		//				}
		//			}
		//		}

		//		private async void UpdateGrid ()
		//		{
		//			// Handle the updating of the current selection
		//			Dispatcher.Invoke (() =>
		//			{
		//				DataGrid1.SelectedIndex = MainWindow.DgControl.SelectedIndex;
		//			});
		//		}
		#endregion Task experimentation
	}
}

