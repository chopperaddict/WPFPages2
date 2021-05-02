using System;
using System . ComponentModel;
using System . Data;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;

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
		public static event SqlSelectedRowChanged SqlViewerIndexChanged;
		public static event DbUpdated NotifyOfDataChange;
		public static event EditDbGridSelectionChanged SqlHasChangedSelection;
		// Event we TRIGGER to notify SqlViewer of  a selectedindex change
		public event EditDbRowChange EditDbViewerSelectedIndexChanged;


		public BankAccountViewModel bvm = MainWindow . bvm;
		public CustomerViewModel cvm = MainWindow . cvm;
		public DetailsViewModel dvm = MainWindow . dvm;

		public BankCollection Bankcollection = BankCollection .Bankcollection;
		public CustCollection Custcollection = CustCollection . Custcollection;
		public DetCollection Detcollection = DetCollection.Detcollection;

		internal static SqlDbViewer ThisParent = null;

		DataTable dt = new DataTable ( );
		SQLDbSupport sqlsupport = new SQLDbSupport ( );
		int CurrentIndex = -1;
		object CurrentItem = null;
		SqlDbViewer sqldbv = null;
		public DataGrid CurrentGrid = null;
		public string CurrentDb = "";
		public EventHandlers EventHandler = null;
		public BankAccountViewModel Bank;
		private RowInfoPopup rip = null;
		private bool PopupActive = false;
		//		private static bool EditHasChangedData = true;
		private SQLEditOcurred SqlEditOccurred = HandleSQLEdit;
		private EditEventArgs EditArgs = null;
		public Task mainTask = null;
		public static bool SqlUpdating = false;

		// Flags to let me handle jupdates to/From SqlViewer
		private int ViewerChangeType = 0;
		private int EditChangeType = 0;

		public static EditDb ThisWindow;/// <summary>
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
			ThisWindow = this;
			ThisParent = sqldb;
			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				this . Height = 400;
				this . MinHeight = 400;
			}
			else
			{
				this . Height = 640;
				this . MinHeight = 640;
			}
		}


		#region EventHandler

		/// <summary>
		/// Callback handler for db change notifications sent by an SqlDbViewer
		/// We have to update our datagrid as relevant
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="Grid"></param>Detcollection
		/// <param name="args"></param>
		public void DbChangedHandler ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args )
		{
			int currentrow = Grid . SelectedIndex;
			if ( sender == ThisParent )
			{
				if ( DataGrid1 . Items . Count > 0 )
				{
					//					int currentrow = DataGrid1 . SelectedIndex;
					// refresh our grid
					DataGrid1 . ItemsSource = null;
					try
					{
						DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( Bankcollection );
					}
					catch
					{
						Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Bankcollection " );
					}
					DataGrid1 . SelectedIndex = currentrow;
				}
				if ( DataGrid2 . Items . Count > 0 )
				{
					//					int currentrow = DataGrid2 . SelectedIndex;
					// refresh our grid
					DataGrid2 . ItemsSource = null;
					try
					{
						DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( CustCollection . Custcollection );
					}
					catch
					{
						Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( CustCollection . Custcollection " );
					}
					DataGrid2 . SelectedIndex = currentrow;
				}
				if ( DetailsGrid . Items . Count > 0 )
				{
					//					int currentrow = DetailsGrid . SelectedIndex;
					// refresh our grid
					ViewerChangeType = 2;
					DetailsGrid . ItemsSource = null;
					DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Detcollection );
					DetailsGrid . SelectedIndex = currentrow;
				}
				return;
			}
			else if ( Flags . CustEditDb == this )
			{
				if ( DataGrid2 . Items . Count > 0 )
				{
					//					int currentrow = DataGrid2 . SelectedIndex;
					// refresh our grid
					DataGrid2 . ItemsSource = null;
					DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( CustCollection . Custcollection );
					DataGrid2 . SelectedIndex = currentrow;
				}
			}
			else if ( Flags . CustEditDb == this )
			{
				if ( DetailsGrid . Items . Count > 0 )
				{
					//					int currentrow = DetailsGrid . SelectedIndex;
					// refresh our grid
					ViewerChangeType = 2;
					DetailsGrid . ItemsSource = null;
					DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Detcollection );
					DetailsGrid . SelectedIndex = currentrow;
				}
			}
			return;
		}

		//	We have changed the selected item, so Trigger event to notify SqlDbViewer of index changes
		private void NotifyViewerofEditIndexChange ( int EditHasChangedData, int row, string CurentDb )
		{

			if ( EditDbViewerSelectedIndexChanged != null )
			{
				// Notify SqlDbViewer() of a change OR Edit
				EditDbViewerSelectedIndexChanged . Invoke ( EditHasChangedData, row, CurrentDb );
				//	This actually calls   EditDb.EditDbHasChangedIndex(int a, int b, string s) Line 576	C# in THIS FILE !!!!   Not needed of course, but a good test

			}
		}


		/// <summary>
		/// SqlViewerGridChanged
		///  A CallBack that RECEIVES notifications from SqlDbViewer on SelectIndex changes or data updates
		///  so that we can update our row position to match. It sends 1  for index or 2  for data change in (int DbChangeTpe)
		/// DELEGATE USED is : SQLViewerSelectionChanged(bool DbEditIndexChangeOnly, int row, string CurrentDb);
		///  EVENT = public static event SqlViewerGridChanged
		/// SendViewerIndexChange ( index, CurrentDb );
		/// </summary>
		/// <param name="row"></param>
		/// <param name="CurrentDb"></param>
		public void OnSqlViewerGridChanged ( int DbChangeType, int row, string CurrentDb )
		{
			// Received a notification from Viewer of a change
			// Set Form wide flags to see what  actions we need to take ?  (Edited value or just index change)
			ViewerChangeType = DbChangeType;        // Change of some type  made in Viewer
			EditChangeType = 0;     // We are being notified by viewer, so clear ourOWN  control flag but set the flags for ViewerChangeType
			ViewerChangeType = DbChangeType;             // but set the Viewer flag to whatever has been passed on to us by caller
										   //			Flags . EditDbChangeHandled = true;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//				if ( DbChangeType == 2 )
				if ( ViewerChangeType == 2 )
				{
					DataGrid1 . ItemsSource = null;
					DataGrid1 . ItemsSource = Bankcollection;
					//					DataGrid1 . ItemsSource = bvm . BankAccountObs;
				}
				DataGrid1 . SelectedItem = null;  //Clear current selection to avoid multiple selections
				DataGrid1 . SelectedIndex = row;
				DataGrid1 . SelectedItem = row;
				if ( DataGrid1 . SelectedItem != null )
					DataGrid1 . ScrollIntoView ( DataGrid1 . SelectedItem );
				DataGrid1 . Refresh ( );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				//				if ( DbChangeType == 2 )
				if ( ViewerChangeType == 2 )
				{
					DataGrid2 . ItemsSource = null;
					DataGrid2 . ItemsSource = CustCollection . Custcollection;
				}
				DataGrid2 . SelectedItem = null;    //Clear current selection to avoid multiple selections
				DataGrid2 . SelectedIndex = row;
				DataGrid2 . SelectedItem = row;
				if ( DataGrid2 . SelectedItem != null )
					DataGrid2 . ScrollIntoView ( DataGrid2 . SelectedItem );
				DataGrid2 . Refresh ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( ViewerChangeType == 2 )
				{
					// Need to update our data cos SqlDbViewer has changed it
					DetailsGrid . ItemsSource = null;
					DetailsGrid . ItemsSource = Detcollection;
				}
				//				DetailsGrid . SelectedItem = null;  //Clear current selection to avoid multiple selections

				// This triggers a call  to DetailsGrid_SelectionChanged in this file
				DetailsGrid . SelectedIndex = row;
				DetailsGrid . SelectedItem = row;
				if ( DetailsGrid . SelectedItem != null )
					DetailsGrid . ScrollIntoView ( DetailsGrid . SelectedItem );
				//				DetailsGrid . Refresh ( );
			}
			// Reset flags
			EditChangeType = 0;
			ViewerChangeType = 0;
		}
		private void TriggerEditOccurred ( string Caller, int index, object datatype )
		{
			EditArgs . Caller = Caller;
			EditArgs . CurrentIndex = index;
			EditArgs . DataType = datatype;
			SqlEditOccurred ( this, EditArgs );
		}

		#endregion EventHandler

		public static void HandleSQLEdit ( object sender, EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			Console . WriteLine ( $"\r\nDelegate Recieved in EDITDB (83) Caller={e . Caller}, Index = {e . CurrentIndex},   {e . DataType . ToString ( )} \r\n" );
			//We no have at our disposal (in e) the entire Updated record, including its Index in the sender grid
			// plus we know its Model type from e.Caller (eg"BANKACCOUNT") 
			//and we even have a pointer to the datagrid in the sender parameter
			int RowToFind = -1;
			if ( ThisWindow != null )
			{
				//only try this if we actually have an EditDb window open
				if ( e . Caller == "BANKACCOUNT" )
				{

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
			// Subscribe to notifications of data changes to SQL data
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . Height = 400;
				this . MinHeight = 400;
				// Hide all relevant controls
				CustomerLabelsGrid . Visibility = Visibility . Collapsed;
				CustomerEditFields . Visibility = Visibility . Collapsed;
				DetailsEditFields . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;
				DetailsGrid . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;

				BankLabels . Visibility = Visibility . Visible;
				BankEditFields . Visibility = Visibility . Visible;
				DataGrid1 . Visibility = Visibility . Visible;

				this . Title += " Bank Accounts Db";
				try
				{
					// setup the Data Contexts for THIS type of grid
					DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( BankCollection . Bankcollection );
					BankEditFields . DataContext = CollectionViewSource . GetDefaultView ( Bankcollection );
					//				DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );
				}
				catch
				{
					Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( BankCollection . Bankcollection " );
				}
				//			BankEditFields . DataContext = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );

				DataGrid1 . SelectedIndex = 0;
				DataGrid1 . SelectedItem = 0;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( DataGrid1, CurrentIndex, -1 );

				CurrentGrid = DataGrid1;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(242) Window just loaded : getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( DataGrid1, "EDITDB", out EventHandler );

				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerBankGrid = DataGrid1;
				BankAccountViewModel . ActiveEditDbViewer = DataGrid1;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "BankAccount";

				DataGrid1 . Focus ( );
				DataGrid1 . BringIntoView ( );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . Height = 640;
				this . MinHeight = 640;
				// Hide all relevant controls
				BankLabels . Visibility = Visibility . Collapsed;
				BankEditFields . Visibility = Visibility . Collapsed;
				DetailsEditFields . Visibility = Visibility . Collapsed;
				DataGrid1 . Visibility = Visibility . Collapsed;
				DetailsGrid . Visibility = Visibility . Collapsed;

				CustomerLabelsGrid . Visibility = Visibility . Visible;
				CustomerEditFields . Visibility = Visibility . Visible;
				DataGrid2 . Visibility = Visibility . Visible;

				this . Title += " Customer Accounts Db";

				try
				{
					DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( CustCollection . Custcollection );
					DataGrid2 . DataContext = CollectionViewSource . GetDefaultView ( CustCollection . Custcollection );
					CustomerEditFields . DataContext = CollectionViewSource . GetDefaultView ( CustCollection . Custcollection );
				}
				catch
				{
					Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( custCollection . custcollection " );
				}

				DataGrid2 . SelectedIndex = CurrentIndex;
				DataGrid2 . SelectedItem = CurrentItem;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( DataGrid2, CurrentIndex, -1 );
				CurrentGrid = DataGrid2;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(287) Window just loaded :  getting instance of EventHandlers class with this,DataGrid2,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( DataGrid2, "EDITDB", out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerCustomerGrid = DataGrid2;
				BankAccountViewModel . ActiveEditDbViewer = DataGrid2;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Customer";

				DataGrid2 . Focus ( );
				DataGrid2 . BringIntoView ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Hide all relevant controls
				this . Height = 400;
				this . MinHeight = 400;
				BankEditFields . Visibility = Visibility . Collapsed;
				DataGrid1 . Visibility = Visibility . Collapsed;
				CustomerLabelsGrid . Visibility = Visibility . Collapsed;
				CustomerEditFields . Visibility = Visibility . Collapsed;
				DataGrid2 . Visibility = Visibility . Collapsed;
				DataGrid1 . Visibility = Visibility . Collapsed;

				BankLabels . Visibility = Visibility . Visible;
				DetailsEditFields . Visibility = Visibility . Visible;
				DetailsGrid . Visibility = Visibility . Visible;

				this . Title += " Secondary Accounts Db";

				// use this or the next one
				//				DetailsGrid . ItemsSource = Detcollection;
				//				DetailsGrid . DataContext = Detcollection;
				//				DetailsEditFields . DataContext = Detcollection;

				try
				{
					DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Detcollection );
					DetailsGrid . DataContext = CollectionViewSource . GetDefaultView ( Detcollection );
					DetailsEditFields . DataContext = CollectionViewSource . GetDefaultView ( Detcollection );
				}
				catch
				{
					Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( DetCollection . Detcollection " );
				}
				// Set the 2 control flags so that we know we have changed data when we notify other windows
				ViewerChangeType = 2;
				EditChangeType = 0;
				//EditHasChangedData = false;
				DetailsGrid . SelectedIndex = 0;
				DetailsGrid . SelectedItem = 0;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( DetailsGrid, CurrentIndex, -1 );
				CurrentGrid = DetailsGrid;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(312) Window just loaded :  getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( DetailsGrid, "DETAILS", out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerDetailsGrid = DetailsGrid;
				BankAccountViewModel . ActiveEditDbViewer = DetailsGrid;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Details";
				DetailsGrid . Focus ( );
				DetailsGrid . BringIntoView ( );

			}

			MainWindow . gv . SqlCurrentEditViewer = this;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
			EditChangeType = 0;     // We have not done anything  

			//+++++++++++++++++++++++++//
			// Subscribe to relevant Events
			//+++++++++++++++++++++++++//
			NotifyOfDataChange += bvm . DbHasChangedHandler; // Callback in REMOTE FILE

			// set up our windows dragging
			this . MouseDown += delegate { DoDragMove ( ); };

		}

		public void EditDbHasChangedIndex ( int a, int b, string s )
		{
			//This gets called whenever a grid is clicked in ?
			int x = 0;
			x++;
			Console . WriteLine ( $"EditDbHasChangedIndex in EditDb has been called ...." );
		}
		public void OnSqlViewerIndexChanged ( int ChangeType, int row, string CurrentDb )
		{

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
		private async void DataGrid1_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;
			var sqlh = new SQLHandlers ( );
			//EditHasChangedData = true;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
			EditChangeType = 2;     // We have changed data in grid

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					await sqlh . UpdateDbRow ( "BANKACCOUNT", e . Row );
					SqlUpdating = true;
					Flags . EditDbDataChanged = true;
					NotifyViewerofEditIndexChange ( 2, DataGrid1 . SelectedIndex, "BANKACCOUNT" );
					Flags . EditDbDataChanged = false;
					return;
				}
			}
			else
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankAccountViewModel . SqlUpdating = true;
					await sqlh . UpdateDbRow ( "BANKACCOUNT", e . Row );
					Flags . EditDbDataChanged = true;
					NotifyViewerofEditIndexChange ( 2, DataGrid1 . SelectedIndex, "BANKACCOUNT" );
					Flags . EditDbDataChanged = false;
					//EditHasChangedData = false;
					return;
				}
			}
		}
		//Customer grid
		private async void DataGrid2_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;
			var sqlh = new SQLHandlers ( );

			//EditHasChangedData = true;
			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
			EditChangeType = 2;     // We have changed data in grid

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "CUSTOMER" )
				{
					await sqlh . UpdateDbRow ( "CUSTOMER", e . Row );
					SqlUpdating = true;
					Flags . EditDbDataChanged = true;
					NotifyViewerofEditIndexChange ( 2, DataGrid2 . SelectedIndex, "CUSTOMER" );
					Flags . EditDbDataChanged = false;
					//		EditHasChangedData = false;
					return;
				}
			}
			else
			{
				if ( CurrentDb == "CUSTOMER" )
				{
					BankAccountViewModel . SqlUpdating = true;
					await sqlh . UpdateDbRow ( "CUSTOMER", e . Row );
					SqlUpdating = true;
					Flags . EditDbDataChanged = true;
					NotifyViewerofEditIndexChange ( 2, DataGrid2 . SelectedIndex, "CUSTOMER" );
					Flags . EditDbDataChanged = false;
					return;
				}
			}
		}
		//Details Grid
		private async void DetailsGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;
			//EditHasChangedData = true;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
			EditChangeType = 2;     // We have changed data in grid

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "DETAILS" )
				{
					await sqlh . UpdateDbRow ( "DETAILS", e . Row );
					SqlUpdating = true;
					Flags . EditDbDataChanged = true;
					NotifyViewerofEditIndexChange ( 2, DetailsGrid . SelectedIndex, "DETAILS" );
					Flags . EditDbDataChanged = false;
				}
			}
			else
			{
				if ( CurrentDb == "DETAILS" )
				{
					DetailsViewModel . SqlUpdating = true;
					await sqlh . UpdateDbRow ( "DETAILS", e . Row );
					Flags . EditDbDataChanged = true;
					NotifyViewerofEditIndexChange ( 2, DetailsGrid . SelectedIndex, "DETAILS" );
					Flags . EditDbDataChanged = false;
				}
			}
			Flags . EditDbDataChanged = false;
			//EditHasChangedData = false;
		}

		#endregion RowEdithandlers

		#region RowSelection handlers

		//public void ViewerHasChangedIndex ( int newRow, string CurrentDb )
		//{
		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		DataGrid1 . SelectedIndex = newRow;

		//	}
		//	else if ( CurrentDb == "CUSTOMER" )
		//	{

		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{

		//	}
		//}
		// BankAccount
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid1_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			//			if ( this.DataGrid1 . SelectedIndex == -1 ) return;
			if ( sqldbv == null ) return;

			//if ( Flags . isMultiMode )
			//	this . Status . Content = $"Total Records = {DataGrid1 . Items . Count}, Current Record = {DataGrid1 . SelectedIndex}, Duplicate A/C's only shown...";
			//else if ( Flags . IsFiltered )
			//	this . Status . Content = $"Total Records = {DataGrid1 . Items . Count}, Current Record = {DataGrid1 . SelectedIndex}, Resullts ARE Filtered";
			//else
			//	this.Status . Content = $"Total Records = {DataGrid1 . Items . Count}, Current Record = {DataGrid1 . SelectedIndex}";
			BankEditFields . DataContext = DataGrid1 . SelectedItem;

			// Notify Viewer of selection change if they have not initiated this change of index
			if ( ViewerChangeType == 0 || EditChangeType == 1 )
			{
				EditChangeType = 1;
				NotifyViewerofEditIndexChange ( EditChangeType, DataGrid1 . SelectedIndex, "BANKACCOUNT" );
			}
			//this.Status . Content = $"Total Records = {DataGrid1 . Items . Count}, Current Record = {DataGrid1 . SelectedIndex}";
			DataGrid1 . Focus ( );
		}

		//Customer Db
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		private void DataGrid2_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( DataGrid2 . SelectedIndex == -1 ) return;
			if ( sqldbv == null ) return;

			//if ( Flags . isMultiMode )
			//	this . Status . Content = $"Total Records = {DataGrid2 . Items . Count}, Current Record = {DataGrid2 . SelectedIndex}, Duplicate A/C's only shown...";
			//else if ( Flags . IsFiltered )
			//	this . Status . Content = $"Total Records = {DataGrid2 . Items . Count}, Current Record = {DataGrid2 . SelectedIndex}, Resullts ARE Filtered";
			//else
			//	this . Status . Content = $"Total Records = {DataGrid2 . Items . Count}, Current Record = {DataGrid2 . SelectedIndex}";
			CustomerEditFields . DataContext = DataGrid2 . SelectedItem;
			// Notify Viewer of selection change if they have not initiated this change of index
			if ( ViewerChangeType == 0 || EditChangeType == 1 )
			{
				EditChangeType = 1;
				NotifyViewerofEditIndexChange ( EditChangeType, DataGrid2 . SelectedIndex, "CUSTOMER" );
			}
			//this . Status . Content = $"Total Records = {DataGrid2 . Items . Count}, Current Record = {DataGrid2 . SelectedIndex}";
			DataGrid2 . Focus ( );
		}

		// Details
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simultaneously.
		/// </summary>
		public void DetailsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( DetailsGrid . SelectedIndex == -1 ) return;
			if ( sqldbv == null ) return;

			//if ( Flags . isMultiMode )
			//	this . Status . Content = $"Total Records = {DetailsGrid . Items . Count}, Current Record = {DetailsGrid . SelectedIndex}, Duplicate A/C's only shown...";
			//else if ( Flags . IsFiltered )
			//	this . Status . Content = $"Total Records = {DetailsGrid . Items . Count}, Current Record = {DetailsGrid . SelectedIndex}, Resullts ARE Filtered";
			//else
			//	this . Status . Content = $"Total Records = {DetailsGrid . Items . Count}, Current Record = {DetailsGrid . SelectedIndex}";
			DetailsEditFields . DataContext = DetailsGrid . SelectedItem;
			//NOTIFY VIEWER OF SELECTION CHANGE, so pass 1 as current mode for Index change ONLY
			if ( ViewerChangeType == 0 || EditChangeType == 1 )
			{
				EditChangeType = 1;
				NotifyViewerofEditIndexChange ( EditChangeType, DetailsGrid . SelectedIndex, "DETAILS" );
			}
			//this . Status . Content = $"Total Records = {DetailsGrid . Items . Count}, Current Record = {DetailsGrid . SelectedIndex}";
			// LAST POINT AFTER CLICKING IN THIS GRID !!
			// But NOT SO if the click was in Viewer grid
			DetailsGrid . Focus ( );
		}
		#endregion RowSelection handlers
		//Bank Edit fields
		#region Bank Editing fields
		private void ActypeEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }
		private void BanknoEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }
		private void CustNoEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }
		private void BalanceEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }
		private void IntRateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }
		private void OpenDateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }
		private void CloseDateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid1 ); }

		#endregion Bank Editing fields
		private void RefreshItemsSource ( DataGrid Grid )
		{
			Flags . EditDbDataChanged = true;
			if ( Flags . SqlBankGrid != null )
				CollectionViewSource . GetDefaultView ( Flags . SqlBankGrid . ItemsSource ) . Refresh ( );
			NotifyViewerofEditIndexChange ( 2, Grid . SelectedIndex, CurrentDb );
			if ( Flags . SqlCustGrid != null )
			{
				CollectionViewSource . GetDefaultView ( Flags . SqlCustGrid . ItemsSource ) . Refresh ( );
				NotifyViewerofEditIndexChange ( 2, Grid . SelectedIndex, CurrentDb );
			}
			if ( Flags . SqlDetGrid != null )
			{
				CollectionViewSource . GetDefaultView ( Flags . SqlDetGrid . ItemsSource ) . Refresh ( );
				NotifyViewerofEditIndexChange ( 2, Grid . SelectedIndex, CurrentDb );
			}
			Flags . EditDbDataChanged = false;
		}
		//Customer edit fields
		#region Customer Editing fields
		private void BanknoEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void CustnoEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void openDateEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void CloseDateEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void AcTypeEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void Addr1Edit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void Addr2Edit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void MobileEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void PhoneEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void CountyEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void PcodeEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void DobEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void FirstnameEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void LastnameEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void TownEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void ODate2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void CDate2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		private void Dob2_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DataGrid2 ); }
		#endregion Customer Editing fields

		#region Details Editing fields
		private void ActypeEdit3LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }
		private void CustnoEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }
		private void BanknoEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }
		private void BalanceEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }
		private void IntRateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }
		private void OpenDateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }
		private void CloseDateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ RefreshItemsSource ( DetailsGrid ); }

		#endregion Details Editing fields

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			Console . WriteLine ( $"Key : {e . Key}" );
			DataGrid dg = null;
			if ( e . Key == Key . Escape )
			{
				if ( CurrentDb == "BANKACCOUNT" )
					BankAccountViewModel . ClearFromEditDbList ( DataGrid1, CurrentDb );
				else if ( CurrentDb == "CUSTOMER" )
					BankAccountViewModel . ClearFromEditDbList ( DataGrid2, CurrentDb );
				else if ( CurrentDb == "DETAILS" )
					BankAccountViewModel . ClearFromEditDbList ( DetailsGrid, CurrentDb );
				BankAccountViewModel . EditdbWndBank = null;
				Close ( );
			}
			else if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
			else if ( e . Key == Key . Up )
			{
				int CurrentRow = 0;

				//	Application . Current . Shutdown ( );
				if ( CurrentDb == "BANKACCOUNT" )
					dg = DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = DataGrid2;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex > 0 )
					dg . SelectedIndex--;

				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . Down )
			{
				int CurrentRow = 0;

				//	Application . Current . Shutdown ( );
				if ( CurrentDb == "BANKACCOUNT" )
					dg = DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = DataGrid2;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 1 )
					dg . SelectedIndex++;

				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . Home )
			{
				int CurrentRow = 0;

				//	Application . Current . Shutdown ( );
				if ( CurrentDb == "BANKACCOUNT" )
					dg = DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = DataGrid2;
				else
					dg = DetailsGrid;
				dg . SelectedIndex = 0;
				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . End )
			{
				// DataGrid keyboard navigation = END
				if ( CurrentDb == "BANKACCOUNT" )
					dg = DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = DataGrid2;
				else
					dg = DetailsGrid;
				dg . SelectedIndex = dg . Items . Count - 1;
				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . PageDown )
			{
				// DataGrid keyboard navigation = PAGE DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = DataGrid2;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 10 )
				{
					dg . SelectedIndex += 10;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
				else
				{
					dg . SelectedIndex = dg . Items . Count - 1;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
			}
			else if ( e . Key == Key . PageUp )
			{
				// DataGrid keyboard navigation = PAGE UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = DataGrid2;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex >= 10 )
				{
					dg . SelectedIndex -= 10;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
				else
				{
					dg . SelectedIndex = 0;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
			}
			else if ( e . Key == Key . OemQuotes )
			{
				EventHandlers . ShowSubscribersCount ( );
			}
			else if ( e . Key == Key . Delete )
			{
				Flags . CurrentSqlViewer . Window_PreviewKeyDown ( sender, e );
			}
			if ( dg != null )
			{
				// Now process it
				if ( dg == DataGrid1 )
					DataGrid1_SelectionChanged ( dg, null );
				else if ( dg == DataGrid2 )
					DataGrid2_SelectionChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectionChanged ( dg, null );
				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
				e . Handled = true;
			}
		}

		private void Button_Click ( object sender, RoutedEventArgs e )
		{
			// Window is being closed
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
			// all handled in _Closing method
		}

		private async void DataGrid1_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
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
					if ( Flags . CurrentSqlViewer . BankGrid . SelectedItem != null )
						Flags . CurrentSqlViewer . BankGrid . ScrollIntoView ( curritem );
					return;
				}
			}
			//Reset our selected row
			DataGrid1 . SelectedIndex = currindx;
			DataGrid1 . SelectedItem = curritem;
		}

		private async void DataGrid2_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			int currindx = DataGrid2 . SelectedIndex;
			var curritem = DataGrid2 . SelectedItem;

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
			//Reset our selected row
			DataGrid2 . SelectedIndex = currindx;
			DataGrid2 . SelectedItem = curritem;
		}

		private async void DetailsGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			int currindx = DetailsGrid . SelectedIndex;
			var curritem = DetailsGrid . SelectedItem;

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
					//return;
				}
			}

			//Reset our selected row
			DataGrid1 . SelectedIndex = currindx;
			DataGrid1 . SelectedItem = curritem;
			//e . Cancel = true;
		}
		private void DataGrid1_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				RowInfoPopup rip = new RowInfoPopup ( "BANKACCOUNT", DataGrid1, RowData );
				rip . DataContext = RowData;
				e . Handled = true;
				rip . ShowDialog ( );
				DataGrid1 . SelectedItem = RowData . Item;
				DataGrid1 . ItemsSource = null;
				DataGrid1 . ItemsSource = Bankcollection;
				//				DataGrid1 . ItemsSource = bvm . BankAccountObs;
				DataGrid1 . Refresh ( );
				Flags . CurrentSqlViewer . BankGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . BankGrid . ItemsSource = Bankcollection;
				//				Flags . CurrentSqlViewer . BankGrid . ItemsSource = bvm . BankAccountObs;
				Flags . CurrentSqlViewer . BankGrid . Refresh ( );
			}
			else
				e . Handled = false;
		}

		private void DataGrid2_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				rip = new RowInfoPopup ( "CUSTOMER", DataGrid2, RowData );
				//PopupActive = true;
				rip . DataContext = RowData;
				e . Handled = true;
				rip . ShowDialog ( );

				//If data has been changed, update everywhere
				DataGrid2 . SelectedItem = RowData . Item;
				DataGrid2 . ItemsSource = null;
				DataGrid2 . ItemsSource = CustCollection . Custcollection;
				DataGrid2 . Refresh ( );
				Flags . CurrentSqlViewer . CustomerGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . CustomerGrid . ItemsSource = CustCollection . Custcollection;
				Flags . CurrentSqlViewer . CustomerGrid . Refresh ( );
			}
			else
				e . Handled = false;
		}

		private void DetailsGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				RowInfoPopup rip = new RowInfoPopup ( "DETAILS", DetailsGrid, RowData );
				rip . DataContext = RowData;
				e . Handled = true;
				rip . ShowDialog ( );

				//If data has been changed, update everywhere
				DetailsGrid . SelectedItem = RowData . Item;
				DetailsGrid . ItemsSource = null;
				DetailsGrid . ItemsSource = Detcollection;
				DetailsGrid . Refresh ( );
				Flags . CurrentSqlViewer . DetailsGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . DetailsGrid . ItemsSource = Detcollection;
				Flags . CurrentSqlViewer . DetailsGrid . Refresh ( );
			}
			//			else
			//				e . Handled = false;
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

		private void Viewer_Click ( object sender, RoutedEventArgs e )
		{
			RowInfoPopup rip = null;
			DataGridRow RowData;
			object row;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				row = DataGrid1 . SelectedItem;
				RowData = DataGrid1 . ItemContainerGenerator . ContainerFromItem ( row ) as DataGridRow;
				rip = new RowInfoPopup ( "BANKACCOUNT", DataGrid1, RowData );
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				row = DataGrid2 . SelectedItem;
				RowData = DataGrid2 . ItemContainerGenerator . ContainerFromItem ( row ) as DataGridRow;
				rip = new RowInfoPopup ( "CUSTOMER", DataGrid2, RowData );
			}
			else
			{
				row = DetailsGrid . SelectedItem;
				RowData = DetailsGrid . ItemContainerGenerator . ContainerFromItem ( row ) as DataGridRow;
				rip = new RowInfoPopup ( "DETAILS", DetailsGrid, RowData );
			}
			rip . DataContext = RowData;
			rip . Topmost = true;
			e . Handled = true;
			rip . ShowDialog ( );
			//If data has been changed, update everywhere
			if ( RowData != null )
			{
				DataGrid2 . SelectedItem = RowData . Item;
				DataGrid2 . ItemsSource = null;
				DataGrid2 . ItemsSource = CustCollection . Custcollection;
				DataGrid2 . Refresh ( );
				Flags . CurrentSqlViewer . CustomerGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . CustomerGrid . ItemsSource = CustCollection . Custcollection;
				Flags . CurrentSqlViewer . CustomerGrid . Refresh ( );
			}
		}

		private void Window_Closing ( object sender, CancelEventArgs e )
		{
			if ( NotifyOfDataChange != null )
				NotifyOfDataChange -= DbChangedHandler;

			if ( EditDbViewerSelectedIndexChanged != null )
				EditDbViewerSelectedIndexChanged -= EditDbHasChangedIndex;      // Callback in THIS FILE
														    //			EventHandlers . ShowSubscribersCount ( );
			
			// Clear up pointers to this instance of an EditDb window
			MainWindow . gv . SqlCurrentEditViewer = null;
			Flags . CurrentEditDbViewer = null;
			//Clear flags
			if ( CurrentDb == "BANKACCOUNT" )
				Flags . BankEditDb = null;
			else if ( CurrentDb == "CUSTOMER" )
				Flags . CustEditDb = null;
			else if ( CurrentDb == "DETAILS" )
				Flags . DetEditDb = null;

			//Flags.DbSelectorOpen. DeleteCurrentViewer ( );
		}
		public static Delegate [ ] GetEventCount5 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( SqlViewerIndexChanged != null )
				dglist2 = SqlViewerIndexChanged . GetInvocationList ( );
			return dglist2;
		}
		public static Delegate [ ] GetEventCount4 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( SqlHasChangedSelection != null )
				dglist2 = SqlHasChangedSelection?.GetInvocationList ( );
			return dglist2;
		}
	}
}

