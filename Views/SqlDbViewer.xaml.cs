#define SHOWSQLERRORMESSAGEBOX
#define SHOWWINDOWDATA
#define ALLOWREFRESH
using System;
using System . ComponentModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Linq;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . Views;
using WPFPages . ViewModels;


namespace WPFPages
{

	#region EventArg Declarations
	public class LoadedEventArgs : EventArgs
	{
		public string CallerDb { get; set; }
	}

	//Inherited Event Args  for Callbacks
	public class DataChangeArgs : EventArgs
	{
		public string SenderName { get; set; }
		public string DbName { get; set; }
	}
	public class DataLoadedArgs : EventArgs
	{
		public DataGrid Grid { get; set; }
		public string DbName { get; set; }
		public int CurrentIndex { get; set; }
	}
	#endregion EventArg Declarations

	public partial class SqlDbViewer : Window, INotifyPropertyChanged
	{
		public DbUpdated NotifyOfDataChange;

		//		public static event SqlSelectedRowChanged SqlViewerIndexChanged;
		//		public static event EditDbRowChange EditDbViewerSelectedIndexChanged;


		public static bool RemoteIndexChange = false;

		#region Global ViewModel declarations
		// SQL Data Setup
		public BankAccountViewModel bvm = MainWindow . bvm;
		public CustomerViewModel cvm = MainWindow . cvm;
		public DetailsViewModel dvm = MainWindow . dvm;
		#endregion Global ViewModel declarations
		//*********************************************************************************************************//

		#region SqlDbViewer Class Constructors
		//Dummy Constructor for Event handlers
		//*********************************************************************************************************//
		public SqlDbViewer ( char x )
		{
			// dummy constructor to let others get a pointer
			InitializeComponent ( );
			SendDbSelectorCommand ( 102, ">>> SqlDbviewer(char) (Full) Constructor loading ....", Flags . CurrentSqlViewer );
			SendDbSelectorCommand ( 103, "<<< SqlDbViewer(char) Constructor  completed....", Flags . CurrentSqlViewer );
		}
		//*********************************************************************************************************//
		public SqlDbViewer ( )
		{
			InitializeComponent ( );
			sqldbForm = this;
			dgControl = new DataGridController ( );
			//Setup our delegate receive function to get messages form DbSelector
			SendDbSelectorCommand ( 102, ">>> SqlDbviewer() (Full) Constructor loading ....", Flags . CurrentSqlViewer );

			if ( Flags . CurrentSqlViewer != this )
				Flags . CurrentSqlViewer = this;

			NotifyViewer SendCommand = new NotifyViewer ( DbSelector . MyNotification );
			SendDbSelectorCommand = DbSelector . MyNotification;
			//This DOES call handler in DbSelector !!
			SendDbSelectorCommand ( 103, "<<< SqlDbViewer() Constructor  completed....", Flags . CurrentSqlViewer );
			Utils . GetWindowHandles ( );
			SubscribeToEvents ( );
		}
		private void SubscribeToEvents ( )
		{
			////subscribing viewmodels to data changed event !!!
			// point to our callback functions in REMOTE FILES DbHasChangedHandler(Viewer, Grid, , DatachangedArgs)
			NotifyOfDataChange += bvm . DbHasChangedHandler;                // Callback in REMOTE FILE
			NotifyOfDataChange += cvm . DbHasChangedHandler;                // Callback in REMOTE FILE
			NotifyOfDataChange += dvm . DbHasChangedHandler;                // Callback in REMOTE FILE

			// point to callback function in THIS FILE - DbDataLoadedHandler(object, DataLoadedArgs)
			NotifyOfDataLoaded += DbDataLoadedHandler;              // Callback in THIS FILE
										// point to callback function in THIS FILE - OnEditDbViewerIndexChanged(( int row, string CurrentDb )
										// EVENT  DECLARATION = EditDbRowChange EditDbViewerSelectedIndexChanged;
										//			if ( EventHandlers . EditDbViewerSelectedIndexChanged == null )
			EventHandlers ev = new EventHandlers ( );
			EventHandlers . EditDbViewerSelectedIndexChanged += OnEditDbViewerIndexChanged;                 // Callback in THIS FILE
			ev . ShowSubscribersCount ( );

			// used by Exception handler
			DataGridBase dgb = new DataGridBase ( );
		}


		//*********************************************************************************************************//
		public SqlDbViewer ( int selection )
		{
			int selectedDb = -1;
			IsViewerLoaded = false;
			//			InitializeComponent ();
			sqldbForm = this;
			dgControl = new DataGridController ( );
			SubscribeToEvents ( );

			if ( Flags . CurrentSqlViewer != this )
				Flags . CurrentSqlViewer = this;
			//Setup our delegate receive function to get messages form DbSelector
			NotifyViewer SendCommand = new NotifyViewer ( DbSelector . MyNotification );
			SendDbSelectorCommand = DbSelector . MyNotification;
			//This DOES call handler in DbSelector !!
			SendDbSelectorCommand ( 102, ">>> SqlDbviewer(int) (Full) Constructor loading ....", Flags . CurrentSqlViewer );
			selectedDb = selection;
			{
				switch ( selectedDb )
				{
					case 0:
						CurrentDb = "BANKACCOUNT";
						new EventHandlers ( BankGrid, "BANKACOUNT", out EventHandler );
						break;
					case 1:
						CurrentDb = "CUSTOMER";
						new EventHandlers ( CustomerGrid, "CUSTOMER", out EventHandler );
						//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
						//						Flags.CurrentSqlViewerCustomerGrid = CustomerGrid;
						//						Flags.ActiveSqlDbViewer = CustomerGrid;
						//LoadCustData ();
						break;
					case 2:
						CurrentDb = "DETAILS";
						new EventHandlers ( DetailsGrid, "DETAILS", out EventHandler );
						//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
						//						Flags.CurrentSqlViewerDetailsGrid = DetailsGrid;
						//						Flags.ActiveSqlDbViewer = DetailsGrid;
						//						LoadDetailsData ();

						break;
					default:
						break;
				}
			}
			ThisWindow = this;
			EditArgs = new EditEventArgs ( );
			this . MouseDown += delegate { DoDragMove ( ); };
			SendDbSelectorCommand ( 103, "<<< SqlDbViewer(int) Constructor  completed....", Flags . CurrentSqlViewer );

		}
		//*********************************************************************************************************//
		/// <summary>
		/// MAIN STARTUP CALL from DbSelector
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		public SqlDbViewer ( string caller )
		{
			int selectedDb = -1;
			IsViewerLoaded = false;
			InitializeComponent ( );
			//Setup our delegate receive function to get messages form DbSelector
			NotifyViewer SendCommand = new NotifyViewer ( DbSelector . MyNotification );
			SendDbSelectorCommand = DbSelector . MyNotification;
			////subscribing viewmodels to data changed event !!!
			SubscribeToEvents ( );
			//This DOES call handler in DbSelector !!
			sqldbForm = this;
			dgControl = new DataGridController ( );
			SendDbSelectorCommand ( 102, ">>> SqlDbviewer(string) (Full) Constructor loading ....", Flags . CurrentSqlViewer );

#if USEDETAILEDEXCEPTIONHANDLER
			Console.WriteLine ($"\r\n%%%%%%%% In SqlDbViewer(string) Constructor %%%%%%%%%%%\r\n");
#endif
			//			if (Flags.CurrentSqlViewer != this)
			//			{
			Flags . CurrentSqlViewer = this;
			//			}

			SendDbSelectorCommand ( 102, ">>> SqlDbviewer (Full) Constructor loading ....", Flags . CurrentSqlViewer );

			switch ( caller )
			{
				case "BANKACCOUNT":
					CurrentDb = "BANKACCOUNT";
					new EventHandlers ( BankGrid, "BANKACOUNT", out EventHandler );
					Flags . ActiveSqlDbViewer = BankGrid;
					break;
				case "CUSTOMER":
					CurrentDb = "CUSTOMER";
					new EventHandlers ( CustomerGrid, "CUSTOMER", out EventHandler );
					Flags . ActiveSqlDbViewer = CustomerGrid;
					break;
				case "DETAILS":
					CurrentDb = "DETAILS";
					new EventHandlers ( DetailsGrid, "DETAILS", out EventHandler );
					Flags . ActiveSqlDbViewer = DetailsGrid;
					break;
				default:
					break;
			}
			ThisWindow = this;
			EditArgs = new EditEventArgs ( );
			this . MouseDown += delegate { DoDragMove ( ); };
			SendDbSelectorCommand ( 103, "<<< SqlDbViewer(string) Constructor  completed....", Flags . CurrentSqlViewer );
		}
		#endregion Constructors


		#region  Declare our Delegates & Events

		// Use with EVENT NotifyOfDataChange (TRIGGER EVENT)
		//Declare our Event
		//declared in NameSpace
		//public delegate void DbUpdated ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args );
		public DataChangeArgs dca = new DataChangeArgs ( );

		// Use with EVENT NotifyOfDataLoaded (TRIGGER EVENT)
		public delegate bool DbReloaded ( object sender, DataLoadedArgs args );
		//Declare our Event that hopefully will call us back ponce the data has been loaded Asynchronously
		public event DbReloaded NotifyOfDataLoaded;

		#endregion  Declare our Delegates 
		//*********************************************************************************************************//

		#region CallBack/Delegate stuff - SqlViewerNotify (int status, string info, SqlDbViewer NewSqlViewer)

		//EVENT Callback for when SQL data has been full loaded Aynchronously
		// Even on initial load of a viewer window
		//	EVENT DECLARATION
		//		public event DbReloaded NotifyOfDataLoaded;
		//	DELEGATE :-
		//		public delegate bool DbReloaded ( object sender, DataLoadedArgs args );
		//	TRIGGER :
		//		SendDBLoadedMsg ( null, args );
		// Recieves the message - Yeahhhhh
		public bool DbDataLoadedHandler ( object sender, DataLoadedArgs args )
		{
			DataGrid Grid;
			bool result = false;

			Console . WriteLine ( $"DbDataLoadedHandler Callback has been activated for {args . DbName} Db" );
			//Wrap all the data updating functionality into this function - 
			//Set up the relevant DataGrid from assigning the ItemsSource to the current index and Refresh
			// so it does not need to be handled elsewheree
			if ( args . DbName == "BANKACCOUNT" )
			{
				// Handle assigning  the Collections to the grids etc
				if ( bvm . BankAccountObs != null )
				{
					BankGrid . ItemsSource = bvm . BankAccountObs;
					BankGrid . SelectedIndex = args . CurrentIndex;
					BankGrid . SelectedItem = args . CurrentIndex;
					BankGrid . Visibility = Visibility . Visible;
					DetailsGrid . Visibility = Visibility . Hidden;
					CustomerGrid . Visibility = Visibility . Hidden;
					ExtensionMethods . Refresh ( BankGrid );
					ParseButtonText ( true );
					UpdateAuxilliaries ( "BankAccount Data Loaded..." );
					Console . WriteLine ( $"{BankGrid . Items . Count} Records reloaded for the {args . DbName} Db, CurrentIndex = {args . CurrentIndex}" );
					result = true;
				}
			}
			else if ( args . DbName == "CUSTOMER" )
			{
				CustomerGrid . ItemsSource = cvm . CustomersObs;
				CustomerGrid . SelectedIndex = args . CurrentIndex;
				CustomerGrid . SelectedItem = args . CurrentIndex;
				BankGrid . Visibility = Visibility . Hidden;
				DetailsGrid . Visibility = Visibility . Hidden;
				CustomerGrid . Visibility = Visibility . Visible;
				ExtensionMethods . Refresh ( CustomerGrid );
				ParseButtonText ( true );
				UpdateAuxilliaries ( "Customer Data Loaded..." );
				Console . WriteLine ( $"{CustomerGrid . Items . Count} Records reloaded for the {args . DbName} Db, CurrentIndex = {args . CurrentIndex}" );
				result = true;
			}
			else if ( args . DbName == "DETAILS" )
			{
				DetailsGrid . ItemsSource = dvm . DetailsObs;
				DetailsGrid . SelectedIndex = args . CurrentIndex;
				DetailsGrid . SelectedItem = args . CurrentIndex;
				BankGrid . Visibility = Visibility . Hidden;
				DetailsGrid . Visibility = Visibility . Visible;
				CustomerGrid . Visibility = Visibility . Hidden;
				ExtensionMethods . Refresh ( DetailsGrid );
				ParseButtonText ( true );
				UpdateAuxilliaries ( "Details Data Loaded..." );
				Console . WriteLine ( $"{DetailsGrid . Items . Count} Records reloaded for the {args . DbName} Db, CurrentIndex = {args . CurrentIndex}" );

				result = true;
			}
			return result;
		}


		//*************************************************************************//
		// delegate object for others to access (listen for notifications sent by THIS CLASS)
		public delegate void SqlViewerNotify ( int status, string info, SqlDbViewer NewSqlViewer );
		public SqlViewerNotify Notifier = null;

		public NotifyViewer SendDbSelectorCommand;
		//*************************************************************************//

		/// <summary>
		///  Messages SENT by this Window
		///  25 - go ahead and command me to Load data
		///  102 -  Starting process xxxxx
		///  103 -  Ended process xxxxx
		///  111 - General status information reports
		///  
		/// Messages Received by this window
		/// 100 - Telling me to Load relevant data
		/// </summary>
		/// <param name="status"></param>
		/// <param name="info"></param>
		/// <param name="NewSqlViewer"></param>
		///Event Callback handler for SqlDbViewer
		public async static void DbSelectorMessage ( int status, string info, SqlDbViewer NewSqlViewer )
		{
			switch ( status )
			{
				case 100:
					break;
			}
			if ( status == 100 )
			{
				//instruction received from DbSelector to load relevant data
				Debug . WriteLine ( $"\r\nSQLDBV - Command :  [{status}] Received" );
				Debug . WriteLine ( $"Data received : [{info}]\r\n" );

				//Window is now on screen & fully painted
				if ( info == "BANKACCOUNT" )
				{
					Flags . CurrentSqlViewer . SendDbSelectorCommand ( 102, ">>> Starting load of Bank Data", Flags . CurrentSqlViewer );

					Flags . CurrentSqlViewer . GetData ( info );
					Flags . CurrentSqlViewer . SendDbSelectorCommand ( 103, $"<<< Bank Data Loaded {Flags . SqlBankGrid . Items . Count} records..", Flags . CurrentSqlViewer );
					Flags . CurrentSqlViewer . SetGridVisibility ( "BANKACCOUNT" );
					var BnkTuple = CreateTuple ( info );
					Flags . CurrentSqlViewer . UpdateViewersList ( );
					Flags . SqlBankGrid . SelectedIndex = 0;
				}
				else if ( info == "CUSTOMER" )
				{
					Flags . CurrentSqlViewer . SendDbSelectorCommand ( 102, ">>> Starting load of Customer Data", Flags . CurrentSqlViewer );
					Flags . CurrentSqlViewer . GetData ( info );
					Flags . CurrentSqlViewer . SendDbSelectorCommand ( 103, $"<<< Customer Data Loaded {Flags . SqlCustGrid . Items . Count} records...", Flags . CurrentSqlViewer );
					Flags . CurrentSqlViewer . SetGridVisibility ( "CUSTOMERS" );
					var CustTuple = CreateTuple ( info );
					Flags . CurrentSqlViewer . UpdateViewersList ( );
					Flags . SqlCustGrid . SelectedIndex = 0;
				}
				else if ( info == "DETAILS" )
				{
					Flags . CurrentSqlViewer . SendDbSelectorCommand ( 102, ">>> Starting load of Details Data", Flags . CurrentSqlViewer );
					Flags . CurrentSqlViewer . GetData ( info );
					Flags . CurrentSqlViewer . SendDbSelectorCommand ( 103, $"<<< Details Data Loaded {Flags . SqlDetGrid . Items . Count} records...", Flags . CurrentSqlViewer );
					Flags . CurrentSqlViewer . SetGridVisibility ( "DETAILS" );
					var DetTuple = CreateTuple ( info );
					Flags . CurrentSqlViewer . UpdateViewersList ( );
					Flags . SqlDetGrid . SelectedIndex = 0;
				}
				ExtensionMethods . Refresh ( Flags . CurrentSqlViewer );
			}
		}

		#endregion CallBack/Delegate stuff
		//*********************************************************************************************************//

		#region Class setup - General Declarations

		private int CurrentGridViewerIndex = -1;
		public string CurrentDb = "CUSTOMER";
		public SqlDataAdapter sda;
		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";
		public bool FilterResult = false;
		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";
		//		public static bool Flags.IsMultiMode = false;
		//private BankAccountViewModel BankCurrentRowAccount;
		private CustomerViewModel CustomerCurrentRowAccount;
		private DetailsViewModel DetailsCurrentRowAccount;
		private DataGridRow BankCurrentRow;
		private DataGridRow CustomerCurrentRow;
		private DataGridRow DetailsCurrentRow;
		public Window ThisWindow = new Window ( );
		private bool IsViewerLoaded = false;
		private int LoadIndex = -1;
		public static bool SqlUpdating = false;
		private bool IsCellChanged = false;
		public DataGrid EditDataGrid = null;

		//Get "Local" copies of our global DataTables
		public DataTable dtDetails = DetailsViewModel . dtDetails;
		public DataTable dtBank = BankAccountViewModel . dtBank;
		public DataTable dtCust = CustomerViewModel . dtCust;

		//Variables for Edithasoccurred delegate
		private SQLEditOcurred SqlEdit = HandleEdit;
		private EditEventArgs EditArgs = null;

		public static SqlDbViewer sqldbForm = null;

		public event PropertyChangedEventHandler PropertyChanged;

		//***************** store the record data for whatever account type's record is the currently selected item
		//so DbSelector can bind to it as well
		private static BankAccountViewModel currentBankSelectedRecord;
		public static BankAccountViewModel CurrentBankSelectedRecord
		{ get { return currentBankSelectedRecord; } set { currentBankSelectedRecord = value; } }

		private static CustomerViewModel currentCustomerSelectedRecord;
		public static CustomerViewModel CurrentCustomerSelectedRecord
		{ get { return currentCustomerSelectedRecord; } set { currentCustomerSelectedRecord = value; } }

		private static DetailsViewModel currentDetailsSelectedRecord;
		public static DetailsViewModel CurrentDetailsSelectedRecord
		{ get { return currentDetailsSelectedRecord; } set { currentDetailsSelectedRecord = value; } }

		private static int _sequentialId = 12345;

		//Delegate is Declared in EventHandlers - they want to be notified of this event
		event SQLViewerGridSelectionChanged SQLVSelChange;

		public EventHandlers EventHandler = null;
		private static bool SelectionhasChanged = false;

		//Variables used when a cell is edited to se if we need to update via SQL
		private object OriginalCellData = null;
		private string OriginalDataType = "";
		private int OrignalCellRow = 0;
		private int OriginalCellColumn = 0;

		public static DataGridController dgControl;

		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		//Delegate & Event handler for Db Updates
		// I am declaring  the Event in THIS FILE
		// the ViewModels will be the subscribers
		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		///  A(Globally visible) Delegate to hold all the global flags and other stuff that is needed to handle 
		///  Static -> non static  movements with EditDb &b SqlDbViewer in particular
		/// </summary>

		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		/// Used to keep track of currently selected row in GridViwer
		/// </summary>
		private int _selectedRow;
		public int SelectedRow
		{
			get { return _selectedRow; }
			set { _selectedRow = value; OnPropertyChanged ( SelectedRow . ToString ( ) ); }
		}

		#endregion setup
		//*********************************************************************************************************//

		/// <summary>
		/// supposed to intercept keystrokes in the window ????
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*********************************************************************************************************//

		#region Callback response functions
		/// <summary>
		/// Calls  the relevant SQL data load calls to load data, fill Lists and populate Obs collections
		/// </summary>
		/// <param name="CurrentDb"></param>
		//*********************************************************************************************************//
		public async void GetData ( string CurrentDb )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( Flags . SqlBankGrid != null )
				{
					Console . WriteLine ( "\nA viewer showing BankAccount data is already open\n" );
					//					return;
				}
				ExtensionMethods . Refresh ( this );
				ExtensionMethods . Refresh ( BankGrid );
				//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				Console . WriteLine ( $"BANKACCOUNT: {BankGrid . Items?.Count}" );
				// The Fn handles the Task.Run()
				await bvm . LoadBankTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( bvm . LoadBankTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				Console . WriteLine ( $"BANKACCOUNT: {BankGrid . Items?.Count}" );
				//ExtensionMethods . Refresh ( BankGrid );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( Flags . SqlBankGrid != null )
				{
					Console . WriteLine ( "\nA viewer showing Customer data is already open\n" );
					//					return;
				}
				ExtensionMethods . Refresh ( this );
				ExtensionMethods . Refresh ( CustomerGrid );
				//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				Console . WriteLine ( $"CUSTOMER: {CustomerGrid . Items?.Count}" );
				await cvm . LoadCustomerTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( cvm . LoadCustomerTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				Console . WriteLine ( $"CUSTOMER: {CustomerGrid . Items?.Count}" );
				//ExtensionMethods . Refresh ( CustomerGrid );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( Flags . SqlDetGrid != null )
				{
					Console . WriteLine ( "" );
				}
				//Loads the data Asynchronously
				ExtensionMethods . Refresh ( this );
				ExtensionMethods . Refresh ( DetailsGrid );
				//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				Console . WriteLine ( $"DETAILS : {DetailsGrid . Items?.Count}" );
				dvm . LoadDetailsTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( dvm . LoadDetailsTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				Console . WriteLine ( $"DETAILS : {DetailsGrid . Items?.Count}" );
				//await dvm . LoadDetailsTaskInSortOrder (true, 0 );
				//ExtensionMethods . Refresh ( DetailsGrid );
			}
		}
		//*********************************************************************************************************//
		public void SetGridVisibility ( string CurrentDb )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Utils . GetWindowHandles ( );
				this . BankGrid . ItemsSource = bvm . BankAccountObs;
				this . BankGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( BankGrid );
			}
			if ( CurrentDb == "CUSTOMER" )
			{

				Utils . GetWindowHandles ( );
				this . CustomerGrid . ItemsSource = cvm . CustomersObs;
				this . CustomerGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( CustomerGrid );
			}
			if ( CurrentDb == "DETAILS" )
			{
				Utils . GetWindowHandles ( );
				this . DetailsGrid . ItemsSource = dvm . DetailsObs;
				this . DetailsGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( DetailsGrid );
			}

		}
		#endregion Callback response functions
		//*********************************************************************************************************//

		#region load/startup
		//*********************************************************************************************************//
		private void OnWindowLoaded ( object sender, RoutedEventArgs e )
		{
			// THIS IS WHERE WE NEED TO SET THIS FLAG
			Flags . SqlViewerIsLoading = true;

			this . Show ( );
			int currentindex = -1;
			this . MouseDown += delegate { DoDragMove ( ); };

			this . Show ( );
			ExtensionMethods . Refresh ( this );
			BringIntoView ( );
			//This is the EventHandler declared  in THIS FILE
			LoadedEventArgs ex = new LoadedEventArgs ( );

			ex . CallerDb = CurrentDb;
			//Broadcast that we have Arrived !!
			OnDataLoaded ( CurrentDb );
			//Use Delegate to notify DbSelector
			this . WaitMessage . Visibility = Visibility . Visible;
			this . BankGrid . Visibility = Visibility . Collapsed;
			this . CustomerGrid . Visibility = Visibility . Collapsed;
			this . DetailsGrid . Visibility = Visibility . Collapsed;
			ExtensionMethods . Refresh ( this );
			SendDbSelectorCommand ( 103, $"<<< SqlDbViewer has Finished OnWindowLoading", Flags . CurrentSqlViewer );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Set up the various globalflags we use to control activity
				//				Flags . ActiveDbGrid = this . BankGrid;
				Flags . SqlBankGrid = this . BankGrid;
				Flags . SqlBankViewer = this;

				Flags . ActiveSqlDbViewer = this . BankGrid;
				MainWindow . gv . SqlBankViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . BankGrid );

				this . BankGrid . ItemsSource = bvm . BankAccountObs;
				this . BankGrid . SelectedIndex = 0;
				this . BankGrid . SelectedItem = 0;
				Flags . SqlBankCurrentIndex = 0;
				this . BankGrid . Visibility = Visibility . Visible;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Set up the various globalflags we use to control activity
				//				Flags . ActiveDbGrid = this . CustomerGrid;
				Flags . SqlCustGrid = this . CustomerGrid;
				Flags . SqlCustViewer = this;
				Flags . ActiveSqlDbViewer = this . CustomerGrid;
				MainWindow . gv . SqlCustViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . CustomerGrid );

				this . CustomerGrid . ItemsSource = cvm . CustomersObs;
				this . CustomerGrid . SelectedIndex = 0;
				this . CustomerGrid . SelectedItem = 0;
				Flags . SqlCustCurrentIndex = 0;
				this . CustomerGrid . Visibility = Visibility . Visible;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Set up the various globalflags we use to control activity
				//				Flags . ActiveDbGrid = this . DetailsGrid;
				Flags . SqlDetGrid = this . DetailsGrid;
				Flags . SqlDetViewer = this;
				MainWindow . gv . SqlDetViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . DetailsGrid );

				// This triggers the selectionchnaged event
				this . DetailsGrid . ItemsSource = dvm . DetailsObs;
				this . DetailsGrid . SelectedIndex = 0;
				this . DetailsGrid . SelectedItem = 0;
				Flags . SqlDetCurrentIndex = 0;
				this . DetailsGrid . Visibility = Visibility . Visible;
			}
			// Grab a Guid for this viewer window early on
			if ( Flags . CurrentSqlViewer == null )
			{
				Flags . CurrentSqlViewer = this;
			}

			//This is the ONE & ONLY place we should set the Guid
			Flags . CurrentSqlViewer . Tag = Guid . NewGuid ( );
			MainWindow . gv . SqlViewerGuid = ( Guid ) Flags . CurrentSqlViewer . Tag;
			IsViewerLoaded = true;
			//Store pointers to our DataGrids in all ModelViews for access by Data row updating code
			//			EventHandlers . SetWindowHandles ( null, this, null );

			// We must now GET THE DATA LOADED
			//Tell DbSelector to command US TO load THE REQUIRED DATA
			// THIS IS THE FIRST CALL - I THINK ??
			SendDbSelectorCommand ( 25, CurrentDb, this );
			ParseButtonText ( true );


			//			if( EventHandlers . EditDbViewerSelectedIndexChanged == null)
			EventHandlers ev = new EventHandlers ( );
			EventHandlers . EditDbViewerSelectedIndexChanged += OnEditDbViewerIndexChanged;
			ev . ShowSubscribersCount ( );

			// clear global "loading new window" flag
			Flags . SqlViewerIsLoading = false;
			Mouse . OverrideCursor = Cursors . Arrow;
#pragma TEMP DEBUG
			Console . WriteLine ( $"\n***Window has just Opened***" );
			Flags . ListGridviewControlFlags ( );
		}

		#endregion load/startup
		//*********************************************************************************************************//

		#region EVENTHANDLERS

		//*******************************************************************************************************//
		// EVENT HANDLER
		//*******************************************************************************************************//
		private void TriggerEditDbUpdate ( string Caller, int index, object datatype )
		// NOT USED NOW
		{
			////This triggers the delegate which wkill resuolt in the Handler Function inside DbEdit
			//// to be called, and it can handle the changes made in there as needed
			//EditArgs . Caller = Caller;
			//EditArgs . CurrentIndex = index;
			//EditArgs . DataType = datatype;
			//SQLEditOcurred dbe = new SQLEditOcurred ( EditDb . HandleSQLEdit );
			////Trigger the delegate
			//dbe ( this, EditArgs );
		}
		//*********************************************************************************************************//
		public void resetSQLDBindex ( bool self, int RowSelected, DataGrid caller )
		// NOT USED NOW
		{
			////This is received when a change is made in EditDb DataGrid
			//// and handles selecting the new index row and scrolling it into view
			//int id1 = 0;
			//int id2 = 0;

			//// use our Global Grid pointer for access
			//if ( caller . Name == "BankGrid" )
			//{
			//	if ( Flags . EventHandlerDebug )
			//	{
			//		//					Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 2330) - Called by {caller . Name}\n" +
			//		//					$"Current index = {BankGrid.SelectedIndex} \nreceived row change of {RowSelected} from EditDb()" );
			//		Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 719) - " +
			//			$"\nCalled by {caller . Name} \r\nCurrent index = {BankGrid . SelectedIndex} \nreceived row change of {RowSelected} from EditDb()" );
			//	}
			//	if ( this . BankGrid . SelectedIndex != RowSelected )
			//	{
			//		if ( this . BankGrid . SelectedIndex != RowSelected )
			//			this . BankGrid . SelectedIndex = RowSelected;
			//		DataGridNavigation . SelectRowByIndex ( BankGrid, RowSelected, -1 );
			//		ScrollList ( this . BankGrid, -1 );
			//		if ( BankAccountViewModel . SqlUpdating )
			//		{
			//			//Need to update the grid as EDITDB has made a change
			//			CollectionViewSource . GetDefaultView ( this . BankGrid . ItemsSource ) . Refresh ( );
			//			BankAccountViewModel . SqlUpdating = false;
			//		}
			//	}
			//}
			//else if ( caller . Name == "DataGrid1" )
			//{
			//	//Data selection changed in EditDb viewer
			//	if ( Flags . EventHandlerDebug )
			//	{
			//		Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 739) - " +
			//			$"\nCalled by {caller . Name} \r\nCurrent index = {BankGrid . SelectedIndex} \nreceived row change of {RowSelected} from EditDb()" );
			//	}
			//	if ( this . BankGrid . SelectedIndex != -1 && this . BankGrid . SelectedIndex != RowSelected )
			//	{
			//		if ( this . BankGrid . SelectedIndex != RowSelected )
			//			this . BankGrid . SelectedIndex = RowSelected;
			//		DataGridNavigation . SelectRowByIndex ( this . BankGrid, RowSelected, -1 );
			//		ScrollList ( this . BankGrid, -1 );
			//		if ( this . BankGrid . ItemsSource != null )
			//		{
			//			//Need to update the grid as EDITDB has made a change
			//			CollectionViewSource . GetDefaultView ( this . BankGrid . ItemsSource ) . Refresh ( );
			//			BankAccountViewModel . SqlUpdating = false;
			//		}
			//	}
			//}
			//else if ( caller . Name == "DataGrid2" )
			//{
			//	//this is a customer account viewer grid !!!!
			//	if ( Flags . EventHandlerDebug )
			//	{
			//		//					Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER (2371) - Called by {caller . Name} - \r\nCurrent index = {this . CustomerGrid . SelectedIndex} received row change of {RowSelected} from EditDb()for {caller . CurrentItem}" );
			//		Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 762) - " +
			//			$"\nCalled by {caller . Name} \r\nCurrent index = {BankGrid . SelectedIndex} \nreceived row change of {RowSelected} from EditDb()" );
			//	}
			//	if ( this . CustomerGrid . SelectedIndex != RowSelected )
			//	{
			//		if ( this . CustomerGrid . SelectedIndex != RowSelected )
			//			//						if (BankAccountViewModel.CurrentEditDbCustomerGrid.SelectedIndex != RowSelected)
			//			this . CustomerGrid . SelectedIndex = RowSelected;
			//		DataGridNavigation . SelectRowByIndex ( this . CustomerGrid, RowSelected, -1 );
			//		ScrollList ( this . CustomerGrid, -1 );

			//		if ( BankAccountViewModel . SqlUpdating )
			//		{
			//			//Need to update the grid as EDITDB has made a change
			//			//						Console.WriteLine ($"\r\nSQLDBVIEWER (2092) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
			//			CollectionViewSource . GetDefaultView ( this . CustomerGrid . ItemsSource ) . Refresh ( );
			//			BankAccountViewModel . SqlUpdating = false;
			//		}
			//	}
			//}
			//else if ( caller . Name == "DetailsGrid" )
			//{
			//	//				Console . WriteLine ( $"SqlDbViewer - (RESETSQLDBINDEX EVENT HANDLER 2076) - Called by {caller . Name} - \r\nCurrent index = {this . DetailsGrid . SelectedIndex}\nReceived row change of {RowSelected} from EditDb()" );
			//	Console . WriteLine ( $"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 784) - " +
			//		$"\nCalled by {caller . Name} \r\nCurrent index = {BankGrid . SelectedIndex} \nreceived row change of {RowSelected} from EditDb()" );

			//	if ( this . DetailsGrid . SelectedIndex != RowSelected )
			//	{
			//		if ( this . DetailsGrid . SelectedIndex == null )
			//			this . DetailsGrid . SelectedIndex = RowSelected;
			//		if ( this . DetailsGrid . SelectedIndex != RowSelected )
			//			this . DetailsGrid . SelectedIndex = RowSelected;
			//		DataGridNavigation . SelectRowByIndex ( this . DetailsGrid, RowSelected, -1 );
			//		ScrollList ( this . DetailsGrid, -1 );

			//		if ( DetailsViewModel . SqlUpdating )
			//		{
			//			//Need to update the grid as EDITDB has made a change
			//			if ( Flags . EventHandlerDebug )
			//			{
			//				Console . WriteLine ( $"\r\nSQLDBVIEWER (2088) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n" );
			//			}
			//			CollectionViewSource . GetDefaultView ( this . DetailsGrid . ItemsSource ) . Refresh ( );
			//			BankAccountViewModel . SqlUpdating = false;

			//		}
			//	}
			//}
		}
		//*********************************************************************************************************//

		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		//Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes
		public event EventHandler<LoadedEventArgs> DataLoaded;
		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		protected virtual void OnDataLoaded ( string info )
		{
			if ( DataLoaded != null )
			{
				Console . WriteLine ( $"Broadcasting from OnDataLoaded sent from {info}" );
				DataLoaded?.Invoke ( this, new LoadedEventArgs ( ) { CallerDb = info } );
			}
		}
		/// <summary>
		/// Notify EditDb when SelectedIndex changes
		/// </summary>
		/// <param name="row"></param>
		/// <param name="CurentDb"></param>
		private void SendViewerIndexChange ( int row, string CurentDb )
		{
			if ( Flags . EditDbChangeHandled )
				return;

			EventHandlers ev = new EventHandlers ( );
			ev . ShowSubscribersCount ( );
			if ( EventHandlers . SqlViewerIndexChanged != null )
			{
				EventHandlers . SqlViewerIndexChanged ( row, CurrentDb );
				//				Flags . EditDbIndexTriggered = false;

			}
		}

		/// <summary>
		///  Function that is broadcasts a notification to whoever to 
		///  notify that one of the Obs collections has been changed by something
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		private void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		{
			dca . SenderName = o . ToString ( );
			dca . DbName = dbName;
			if ( NotifyOfDataChange != null )
			{
				EventHandlers ev = new EventHandlers ( );
				ev . NotifyOfDataChange ( o, Grid, dca );
			}
		}

		/// <summary>
		/// Triigered by the three ViewModels 
		/// so WE broadcast a notification to whoever to 
		///  notify that the reloading of data via Asnyc SQL has been fuly completed
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		public static void SendDBLoadedMsg ( object o, DataLoadedArgs args )
		{
			if ( Flags . CurrentSqlViewer . NotifyOfDataLoaded != null )
			{
				Flags . CurrentSqlViewer . NotifyOfDataLoaded ( o, args );
			}
		}
		#endregion EVENTHANDLERS
		//*********************************************************************************************************//

		#region load all data base data

		//*********************************************************************************************************//
		public bool LoadDetailsObsCollection ( )
		{
			//Load the data into our ObservableCollection BankAccounts
			if ( dvm . DetailsObs . Count > 0 )
			{
				dvm . DetailsObs . Clear ( );
			}
			try
			{
				for ( int i = 0 ; i < DetailsViewModel . dtDetails . Rows . Count ; ++i )
					dvm . DetailsObs . Add ( new DetailsViewModel
					{
						Id = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 0 ] ),
						BankNo = dtDetails . Rows [ i ] [ 1 ] . ToString ( ),
						CustNo = dtDetails . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 3 ] ),
						Balance = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 4 ] ),
						IntRate = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 5 ] ),
						ODate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 6 ] ),
						CDate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 7 ] )
					} );
				return true;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Error loading Details Data {ex . Message}" );
#if SHOWSQLERRORMESSAGEBOX
				MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif

				return false;
			}

		}
		//*********************************************************************************************************//
		public async void LoadCustData ( int mode = -1 )
		{
			SendDbSelectorCommand ( 102, ">>> SqlDbViewer in LoadCustData()", Flags . CurrentSqlViewer );

			//Handles the complete sql data Loading, adding to List, Obs and assigning it to the grid
			Debug . WriteLine ( $"LoadCustData callback has been called successfully\r\nNow loading data into system...." );
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand ( 102, $">>> SqlDbViewer calling TASK LoadCustomersTask", Flags . CurrentSqlViewer );
			// load data into DataTable



			//await Task . Factory . StartNew ( ( ) =>
			//{
			//	Dispatcher . Invoke ( ( ) =>
			//	{
			//		//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
			//		// It also broadcasts to any other viewer to update  if needed
			//		cvm . LoadCustomerTaskInSortOrder ( -1 );
			//	} );
			//} );
			await cvm . LoadCustomerTaskInSortOrder ( true, 0 );
			//List<Task<bool>> tasks = new List<Task<bool>> ( );
			//tasks . Add ( cvm . LoadCustomerTaskInSortOrder ( true, 0 ) );
			//var Results = await Task . WhenAll ( tasks );
			Debug . WriteLine ( $"returned from calling Task to load the data...." );

			// Bind our Grid to the data = not there yet in new version 
			//			CustomerGrid.ItemsSource = CustomerViewModel.CustomersObs;
			Mouse . OverrideCursor = Cursors . Arrow;
			//*****************************************************************************//
			// We now have ALL data into the Observable Collection
			//*****************************************************************************//
			this . CustomerGrid . Visibility = Visibility . Visible;
			this . BringIntoView ( );
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand ( 103, $"<<< SqlDbViewer Exiting LoadCustData", Flags . CurrentSqlViewer );
		}
		//*********************************************************************************************************//
		#endregion load all data base data
		//*********************************************************************************************************//

		#region Load/show selected data base data


		#region Utility functions for Grid loading preparation
		private void CleanViewerGridData ( )
		{
			if ( BankGrid . Visibility == Visibility . Visible )
			{
				//Clear the GridView data structure first, (false means kill ALL)
				GridViewer . CheckResetAllGridViewData ( "BANKACCOUNT", this, false );
				// now clear Flags structure
				Flags . SqlBankViewer = null;
				Flags . SqlBankGrid = null;
				BankGrid . ItemsSource = null;
				BankGrid . Items . Clear ( );
				BankAccountViewModel . dtBank . Clear ( );
				bvm . BankAccountObs . Clear ( );
				BankGrid . Visibility = Visibility . Hidden;
			}
			else if ( CustomerGrid . Visibility == Visibility . Visible )
			{
				//Clear the GridView data structure first, (false means kill ALL)
				GridViewer . CheckResetAllGridViewData ( "CUSTOMER", this, false );
				// now clear Flags structure
				Flags . SqlCustViewer = null;
				Flags . SqlCustGrid = null;
				CustomerGrid . ItemsSource = null;
				CustomerGrid . Items . Clear ( );
				CustomerViewModel . dtCust . Clear ( );
				cvm . CustomersObs . Clear ( );
				CustomerGrid . Visibility = Visibility . Hidden;
			}
			else if ( DetailsGrid . Visibility == Visibility . Visible )
			{
				//Clear the GridView data structure first, (false means kill ALL)
				GridViewer . CheckResetAllGridViewData ( "DETAILS", this, false );
				Flags . SqlDetViewer = null;
				Flags . SqlDetGrid = null;
				DetailsGrid . ItemsSource = null;
				DetailsGrid . Items . Clear ( );
				DetailsViewModel . dtDetails . Clear ( );
				dvm . DetailsObs . Clear ( );
				DetailsGrid . Visibility = Visibility . Hidden;
			}
		}

		#endregion Utility functions for Grid loadin preparation
		//*********************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for BankAccount Db and fills BankAccount DataGrid
		/// </summary>
		//*********************************************************************************************************//
		private async void ShowBank_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( BankGrid . Visibility != Visibility . Visible )
			{
				if ( Flags . SqlBankGrid != null )
				{
					// viewer already open with Bankgrid visible, so switch to it
					Flags . SqlBankViewer . Focus ( );
					return;
				}
				// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
				CleanViewerGridData ( );

				//Ok, we are going to load the new Db, but we need to 
				// Tidy up the flags before we load Bank Data
				//first, clear the Filtering buttons in case they are still in use
				Flags . IsMultiMode = false;
				SetButtonGradientBackground ( Multiaccounts );
				Multiaccounts . IsEnabled = true;
				ResetOptionButtons ( 1 );
				// Set the gradient background
				Filters . IsEnabled = true;
				SetButtonGradientBackground ( Multiaccounts );
			}
			if ( Flags . CurrentSqlViewer != null )
				SendDbSelectorCommand ( 102, $">>> Entering ShowBank_Click()", Flags . CurrentSqlViewer );
			// Make sure this window has it's pointer "Registered" cos we can 
			// Click the button before the window has had focus set
			Mouse . OverrideCursor = Cursors . Wait;
			Flags . CurrentSqlViewer = this;

			if ( Flags . SqlBankGrid != null && BankGrid . Items . Count > 0 && !Flags . IsMultiMode )
			{
				// Already got a Bank Grid open !
				if ( MainWindow . gv . SqlBankViewer == null ) return;

				MainWindow . gv . SqlBankViewer?.Focus ( );
				MainWindow . gv . SqlBankViewer?.BringIntoView ( );
				ExtensionMethods . Refresh ( MainWindow . gv . SqlBankViewer );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}
			CurrentDb = "BANKACCOUNT";
			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;

			// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			// Important call - it sets up global flags for all/any of the allowed viiewer windows
			if ( !SetFlagsForViewerGridChange ( this, BankGrid ) )
				return;

			ParseButtonText ( true );
			UpdateAuxilliaries ( "Details Data Loaded..." );
			SendDbSelectorCommand ( 111, $"SqlDbViewer  - calling AWAIT Task LoadBankTask", Flags . CurrentSqlViewer );
			// LOAD THE DATA - The Fn handles the Task.Run()
			await bvm . LoadBankTaskInSortOrder ( false, 0 );
			BankGrid . ItemsSource = bvm . BankAccountObs;
			BankGrid . Visibility = Visibility . Visible;
			ParseButtonText ( true );

			// UPDATE DbSelector ViewersList entry
			BankGrid . SelectedIndex = 0;
			BankAccountViewModel c = BankGrid . SelectedItem as BankAccountViewModel;
			string str = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {c . ODate}";
			UpdateDbSelectorItem ( str );
			this . Focus ( );
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		//*********************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// </summary>
		//*********************************************************************************************************//
		private async void ShowCust_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );


			if ( CustomerGrid . Visibility != Visibility . Visible )
			{
				// Customer Grid NOT open in this viewer, check which grid IS OPEN here ?
				if ( Flags . SqlCustGrid != null )
				{
					// viewer already open with Bankgrid visible, so switch to it
					Flags . SqlCustViewer . Focus ( );
					return;
				}
				if ( Flags . CurrentSqlViewer != null )
					SendDbSelectorCommand ( 102, $">>> Entering ShowCust_Click()", Flags . CurrentSqlViewer );

				// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
				CleanViewerGridData ( );

				Mouse . OverrideCursor = Cursors . Wait;
				// Make sure this window has it's pointer "Registreded" cos we can 
				// Click the button before the window has had focus set
				Flags . CurrentSqlViewer = this;
				if ( Flags . SqlCustGrid != null && CustomerGrid . Items . Count > 0 && !Flags . IsMultiMode )
				{
					MainWindow . gv . SqlCustViewer . Focus ( );
					MainWindow . gv . SqlCustViewer . BringIntoView ( );
					ExtensionMethods . Refresh ( MainWindow . gv . SqlCustViewer );
					Mouse . OverrideCursor = Cursors . Arrow;
					return;
				}

				CurrentDb = "CUSTOMER";
				//Reset the MultiMode flag as user has requested a FULL reload
				Flags . IsMultiMode = false;

				// create GV[] variables for this new viewer grid
				DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

				// Important call - it sets up global flags for all/any of the allowed viiewer windows
				if ( !SetFlagsForViewerGridChange ( this, CustomerGrid ) )
					return;
				// Clear the buttons text initially
				ParseButtonText ( false );
				UpdateAuxilliaries ( "Details Data Loaded..." );
				//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				await cvm . LoadCustomerTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( cvm . LoadCustomerTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				CustomerGrid . ItemsSource = cvm . CustomersObs;
				CustomerGrid . Visibility = Visibility . Visible;

				// UPDATE DbSelector ViewersList entry
				CustomerGrid . SelectedIndex = 0;
				CustomerViewModel c = CustomerGrid . SelectedItem as CustomerViewModel;
				string str = $"Customer - # {c . CustNo}, Bank #@{c . BankNo}, {c . LName}, {c . Town}, {c . County} {c . PCode}";
				//				string s = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				UpdateDbSelectorItem ( str );

				ParseButtonText ( true );
				this . Focus ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}
		//*********************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for DetailsViewModel Db and fills relevant DataGrid
		/// <param name="sender"></param>
		/// <param name="e"></param></summary>
		//*****************************************************************************************//
		private async void ShowDetails_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( DetailsGrid . Visibility != Visibility . Visible )
			{
				if ( Flags . SqlDetGrid != null )
				{
					// viewer already open with Bankgrid visible, so switch to it
					Flags . SqlDetViewer . Focus ( );
					return;
				}
				// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
				CleanViewerGridData ( );

				//// Tidy up the flags before we load Bank Data
				//if ( BankGrid . Visibility == Visibility . Visible )
				//{
				//	Flags . SqlBankViewer = null;
				//	Flags . SqlBankGrid = null;
				//	BankGrid . ItemsSource = null;
				//	BankGrid . Items . Clear ( );
				//	BankAccountViewModel . dtBank . Clear ( );
				//	bvm . BankAccountObs . Clear ( );
				//	BankGrid . Visibility = Visibility . Hidden;
				//}
				//if ( CustomerGrid . Visibility == Visibility . Visible )
				//{
				//	Flags . SqlCustViewer = null;
				//	Flags . SqlCustGrid = null;
				//	CustomerGrid . ItemsSource = null;
				//	CustomerGrid . Items . Clear ( );
				//	CustomerViewModel . dtCust . Clear ( );
				//	cvm . CustomersObs . Clear ( );
				//	CustomerGrid . Visibility = Visibility . Hidden;
				//}
			}
			if ( Flags . CurrentSqlViewer != null )
				SendDbSelectorCommand ( 102, $">>> Entering ShowDetails_Click()", Flags . CurrentSqlViewer );

			Mouse . OverrideCursor = Cursors . Wait;
			// Make sure this window has it's pointer "Registered" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			if ( Flags . SqlDetGrid != DetailsGrid && DetailsGrid . Items . Count > 0 && !Flags . IsMultiMode )
			{
				MainWindow . gv . SqlDetViewer . Focus ( );
				MainWindow . gv . SqlDetViewer . BringIntoView ( );
				ExtensionMethods . Refresh ( MainWindow . gv . SqlDetViewer );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			CurrentDb = "DETAILS";
			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;

			// Important call - it sets up global flags for all/any of the allowed viiewer windows
			// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			if ( !SetFlagsForViewerGridChange ( this, DetailsGrid ) )
				return;
			// Clear the buttons text initially
			ParseButtonText ( true );
			UpdateAuxilliaries ( "Details Data Loaded..." );

			//			if(Flags.)
			await dvm . LoadDetailsTaskInSortOrder ( true, 0 );
			//List<Task<bool>> tasks = new List<Task<bool>> ( );
			//tasks . Add ( dvm . LoadDetailsTaskInSortOrder ( true, 0 ) );
			//var Results = await Task . WhenAll ( tasks );
			DetailsGrid . ItemsSource = dvm . DetailsObs;
			DetailsGrid . Visibility = Visibility . Visible;
			ParseButtonText ( true );

			// UPDATE DbSelector ViewersList entry
			DetailsGrid . SelectedIndex = 0;
			DetailsViewModel c = DetailsGrid . SelectedItem as DetailsViewModel;
			string str = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {c . ODate}";
			UpdateDbSelectorItem ( str );

			this . Focus ( );
			IsFiltered = "";
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		private bool SetFlagsForViewerGridChange ( SqlDbViewer Viewer, DataGrid Grid )
		{
			bool result = false;
			// ONLY CALLED BY SHOWxxxxx_CLICK HANDLERS
			//First off - Clear other GRID flags so we dont have any confusion
			//but be aware they may be open in a different viewer so only null
			// them out if they do not have an owner Viewer Handle
			if ( Flags . SqlBankViewer == this )
			{
				Flags . SqlBankGrid = null;
				BankGrid . ItemsSource = null;
			}
			if ( Flags . SqlCustViewer == this )
			{
				Flags . SqlCustGrid = null;
				CustomerGrid . ItemsSource = null;
			}
			if ( Flags . SqlDetViewer == this )
			{
				Flags . SqlDetGrid = null;
				DetailsGrid . ItemsSource = null;
			}
			// Setup our pointer to this Viewer and Grid
			if ( Grid == BankGrid )
			{
				Flags . SqlBankGrid = BankGrid;
				Flags . SqlBankViewer = Viewer;
			}
			else if ( Grid == CustomerGrid )
			{
				Flags . SqlCustGrid = CustomerGrid;
				Flags . SqlCustViewer = Viewer;
			}
			else if ( Grid == DetailsGrid )
			{
				Flags . SqlDetGrid = DetailsGrid;
				Flags . SqlDetViewer = Viewer;
			}
			// Sort out ItemsSources for all possible open windows
			CustomerGrid . ItemsSource = null;
			if ( Flags . SqlBankViewer == null )
				BankGrid . ItemsSource = null;
			if ( Flags . SqlDetViewer == null )
				DetailsGrid . ItemsSource = null;
			return true;
		}

		//*********************************************************************************************************//

		#endregion Load/show selected data base data
		//*********************************************************************************************************//

		#region Standard Click Events
		private void ExitFilter_Click ( object sender, RoutedEventArgs e )
		{
			//Just "Close" the Filter panel
			//			FilterFrame.Visibility = Visibility.Hidden;
		}
		//*********************************************************************************************************//
		private void ContextMenu1_Click ( object sender, RoutedEventArgs e )
		{
			//Add a new row
			if ( CurrentDb == "BANKACCOUNT" )
			{
				DataRow dr = BankAccountViewModel . dtBank . NewRow ( );
				BankAccountViewModel . dtBank . Rows . Add ( dr );
				//				BankGrid.DataContext = dtBank;
			}
		}
		//*********************************************************************************************************//
		private void ContextMenu2_Click ( object sender, RoutedEventArgs e )
		{
			//Delete current Row
			BankAccountViewModel dg = sender as BankAccountViewModel;
			DataRowView row = ( DataRowView ) this . BankGrid . SelectedItem;

		}
		//*********************************************************************************************************//
		private void ContextMenu3_Click ( object sender, RoutedEventArgs e )
		{
			//Close Window
		}
		//*********************************************************************************************************//
		private async void Multiaccs_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			//Show only Customers with multiple Bank Accounts
			Window_MouseDown ( sender, null );
			string s = Flags . MultiAccountCommandString = Multiaccounts . Content as string;
			if ( s . Contains ( "<<-" ) || s . Contains ( "Clear Filter" ) )
			{
				Flags . IsMultiMode = false;
				ResetOptionButtons ( 1 );
				// Set the gradient background
				SetButtonGradientBackground ( Multiaccounts );
				SetButtonGradientBackground ( Filters );
			}
			else
			{
				Flags . IsMultiMode = true;
				ResetOptionButtons ( 1 );
				SetButtonGradientBackground ( Multiaccounts );
			}
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankGrid . ItemsSource = null;
				bvm . BankAccountObs . Clear ( );
				BankGrid . Items . Clear ( );
				BankAccountViewModel . dtBank . Clear ( );
				await bvm . LoadBankTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( bvm . LoadBankTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				//				await bvm . LoadBankTaskInSortOrder ( true, 0 );
				BankGrid . ItemsSource = bvm . BankAccountObs;
				ExtensionMethods . Refresh ( BankGrid );
				ParseButtonText ( true );
				//Flags.SortOrderRequested
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerGrid . ItemsSource = null;
				cvm . CustomersObs . Clear ( );
				CustomerGrid . Items . Clear ( );
				CustomerViewModel . dtCust . Clear ( );
				await cvm . LoadCustomerTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( cvm . LoadCustomerTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				CustomerGrid . ItemsSource = cvm . CustomersObs;
				ExtensionMethods . Refresh ( CustomerGrid );
				ParseButtonText ( true );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsGrid . ItemsSource = null;
				dvm . DetailsObs . Clear ( );
				DetailsGrid . Items . Clear ( );
				DetailsViewModel . dtDetails . Clear ( );
				dvm . LoadDetailsTaskInSortOrder ( true, 0 );
				//List<Task<bool>> tasks = new List<Task<bool>> ( );
				//tasks . Add ( dvm . LoadDetailsTaskInSortOrder ( true, 0 ) );
				//var Results = await Task . WhenAll ( tasks );
				DetailsGrid . ItemsSource = dvm . DetailsObs;
				ExtensionMethods . Refresh ( DetailsGrid );
				ParseButtonText ( true );
			}
			// Set the gradient background
			SetButtonGradientBackground ( Filters );
		}

		//*********************************************************************************************************//
		private void ContextMenuFind_Click ( object sender, RoutedEventArgs e )
		{
			// find something - this returns  the top rows data in full
			BankAccountViewModel b = this . BankGrid . Items . CurrentItem as BankAccountViewModel;

		}
		//*********************************************************************************************************//
		private void CloseAccount_Click ( object sender, RoutedEventArgs e )
		{
			//			//Now get the actual data item behind the selected row
			//			if ( CurrentDb == "BANKACCOUNT" )
			//			{
			//				int id = BankCurrentRowAccount . Id;
			//				BankCurrentRowAccount . CDate = DateTime . Now;
			//#pragma checksum   why is thi smissing ?????
			//				//ViewerGrid_RowEditEnded (null, null);
			//				this . BankGrid . Items . Refresh ( );
			//			}
			//			else if ( CurrentDb == "CUSTOMER" )
			//			{
			//				int id = CustomerCurrentRowAccount . Id;
			//				CustomerCurrentRowAccount . CDate = DateTime . Now;
			//			}
			//			else if ( CurrentDb == "DETAILS" )
			//			{
			//				int id = DetailsCurrentRowAccount . Id;
			//				DetailsCurrentRowAccount . CDate = DateTime . Now;
			//			}
		}

		#endregion Standard Click Events
		//*********************************************************************************************************//

		#region grid row selection code

		private bool CheckForDataChange ( BankAccountViewModel bvm )
		{
			//object newvalue = null;
			//if (OriginalCellData == null)
			//	return false;
			//switch (OriginalDataType.ToUpper ())
			//{
			//	case "BANKNO":
			//		newvalue = bvm.BankNo;
			//		break;
			//	case "CUSTO":
			//		newvalue = bvm.CustNo;
			//		break;
			//	case "ACTYPE":
			//		newvalue = bvm.AcType;
			//		break;
			//	case "BALANCE":
			//		newvalue = bvm.Balance;
			//		break;
			//	case "INTRATE":
			//		newvalue = bvm.IntRate;
			//		break;
			//	case "ODATE":
			//		newvalue = bvm.ODate;
			//		break;
			//	case "CDATE":
			//		newvalue = bvm.CDate;
			//		break;
			//}

			//if (OriginalCellData == newvalue)
			return true;
			//else
			//	return false;
			////			if(c.   OriginalCellData
			////		       OriginalDataType
		}

		private void BankGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			//This fires whenever we click inside the grid !!!
			// Even just selecting a different row
			//This is THE ONE to use to update our DbSleector ViewersList text
			if ( this . BankGrid . SelectedItem != null )
			{
				// All We are doing here is just updating the text in the DbSelectorViewersList
				//This gives me the entire Db Record in "c"
				BankAccountViewModel c = this . BankGrid?.SelectedItem as BankAccountViewModel;
				if ( c == null ) return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
				ExtensionMethods . Refresh ( this );

				// broadcast data change to all subscribers
				//								SendDataChanged (BankGrid, "BANKACCOUNT");
			}
		}
		private void CustomerGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSleector ViewersList text
			if ( this . CustomerGrid . SelectedItem != null )
			{
				//This gives me an entrie Db Record in "c"
				CustomerViewModel c = this . CustomerGrid?.SelectedItem as CustomerViewModel;
				if ( c == null ) return;
				string s = $"Customer - # {c . CustNo}, Bank #@{c . BankNo}, {c . LName}, {c . Town}, {c . County} {c . PCode}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
				CustomerGrid . ScrollIntoView ( CustomerGrid . SelectedItem );
				ExtensionMethods . Refresh ( this );
				// broadcast data change to all subscribers
				//								SendDataChanged (CustomerGrid, "CUSTOMER");
			}
		}
		private void DetailsGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSelector ViewersList text
			if ( this . DetailsGrid . SelectedItem != null )
			{
				//This gives me an entire Db Record in "c"
				DetailsViewModel c = this . DetailsGrid?.SelectedItem as DetailsViewModel;
				if ( c == null ) return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Details - # {c . CustNo}, Bank #@{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
#pragma NOT NEEDED ????
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
				ExtensionMethods . Refresh ( this );
				// broadcast data change to all subscribers
#pragma TEMP - blowing up on Db load
				//								SendDataChanged (DetailsGrid, "DETAILS");
			}
		}
		#endregion grid row selection code
		//*********************************************************************************************************//

		#region CellEdit Checker functions
		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			//			BankAccountViewModel bvm = dgr.Item as BankAccountViewModel;
			OriginalDataType = name;
			switch ( name . ToUpper ( ) )
			{
				case "BANKNO":
					OriginalCellData = bvm . BankNo;
					break;
				case "CUSTO":
					OriginalCellData = bvm . CustNo;
					break;
				case "ACTYPE":
					OriginalCellData = bvm . AcType;
					break;
				case "BALANCE":
					OriginalCellData = bvm . Balance;
					break;
				case "INTRATE":
					OriginalCellData = bvm . IntRate;
					break;
				case "ODATE":
					OriginalCellData = bvm . ODate;
					break;
				case "CDATE":
					OriginalCellData = bvm . CDate;
					break;
			}
		}

		//These all set a global bool to flag whether a cell has actually been changed
		//so we do not call SQL Update uneccessarily
		private void BankGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			BankAccountViewModel c = BankGrid?.SelectedItem as BankAccountViewModel;
			TextBox textBox = e . EditingElement as TextBox;
			if ( textBox == null )
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox . Text;
			SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
		}

		private void CustomerGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			CustomerViewModel c = CustomerGrid?.SelectedItem as CustomerViewModel;
			TextBox textBox = e . EditingElement as TextBox;
			if ( textBox == null )
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox . Text;
			SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
		}

		private void DetailsGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			DetailsViewModel c = DetailsGrid . SelectedItem as DetailsViewModel;
			TextBox textBox = e . EditingElement as TextBox;
			if ( textBox == null )
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox . Text;
			SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );

		}

		private void DetailsGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			//			DetailsViewModel dvm = dgr.Item as DetailsViewModel;
			OriginalDataType = name;
			switch ( name . ToUpper ( ) )
			{
				case "BANKNO":
					OriginalCellData = dvm . BankNo;
					break;
				case "CUSTO":
					OriginalCellData = dvm . CustNo;
					break;
				case "ACTYPE":
					OriginalCellData = dvm . AcType;
					break;
				case "BALANCE":
					OriginalCellData = dvm . Balance;
					break;
				case "INTRATE":
					OriginalCellData = dvm . IntRate;
					break;
				case "ODATE":
					OriginalCellData = dvm . ODate;
					break;
				case "CDATE":
					OriginalCellData = dvm . CDate;
					break;
			}

		}

		private void CustomerGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			//			CustomerViewModel bvm = dgr.Item as CustomerViewModel;
			OriginalDataType = name;
			switch ( name . ToUpper ( ) )
			{
				case "BANKNO":
					OriginalCellData = cvm . BankNo;
					break;
				case "CUSTO":
					OriginalCellData = cvm . CustNo;
					break;
				case "ACTYPE":
					OriginalCellData = cvm . AcType;
					break;
				case "FNAME":
					OriginalCellData = cvm . FName;
					break;
				case "LNAME":
					OriginalCellData = cvm . LName;
					break;
				case "ADDR1":
					OriginalCellData = cvm . Addr1;
					break;
				case "ADDR2":
					OriginalCellData = cvm . Addr2;
					break;
				case "TOWN":
					OriginalCellData = cvm . Town;
					break;
				case "COUNTY":
					OriginalCellData = cvm . County;
					break;
				case "PCODE":
					OriginalCellData = cvm . PCode;
					break;
				case "PHONE":
					OriginalCellData = cvm . Phone;
					break;
				case "MOBILE":
					OriginalCellData = cvm . Mobile;
					break;
				case "DOB":
					OriginalCellData = cvm . Dob;
					break;
				case "ODATE":
					OriginalCellData = cvm . ODate;
					break;
				case "CDATE":
					OriginalCellData = cvm . CDate;
					break;
			}
		}
		#endregion CellEdit Checker functions
		//*********************************************************************************************************//

		#region Keyboard /Mousebutton handlers
		private void Window_MouseDown ( object sender, MouseButtonEventArgs e )
		{
			Window_GotFocus ( sender, e );
		}

		//*********************************************************************************************************//
		public void UpdateDbSelectorBtns ( SqlDbViewer viewer )
		{
			// works with multiple entries 22 March 2021

			if ( Flags . DbSelectorOpen == null )
				return;

			if ( Flags . DbSelectorOpen . ViewersList . Items . Count == 1 )
			{
				Flags . DbSelectorOpen . ViewerDeleteAll . IsEnabled = false;
				Flags . DbSelectorOpen . ViewerDelete . IsEnabled = false;
				Flags . DbSelectorOpen . SelectViewerBtn . IsEnabled = false;
				return;
			}
			else
			{
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count > 2 )
					Flags . DbSelectorOpen . ViewerDeleteAll . IsEnabled = true;
				else
					Flags . DbSelectorOpen . ViewerDeleteAll . IsEnabled = false;
				Flags . DbSelectorOpen . ViewerDelete . IsEnabled = true;
				Flags . DbSelectorOpen . SelectViewerBtn . IsEnabled = true;
			}
		}
		/// <summary>
		/// No longer used - ignore....
		/// </summary>
		/// <param name="selection"></param>
		//*********************************************************************************************************//
		void SetButtonStatus ( string selection )
		{

			//			return;
			//This sets the currently selected Db button to be defaulted
			// making it change background color
			if ( selection == "BANKACCOUNT" )
			{
				ShowDetails . Tag = false;
				ShowBank . Tag = true;    //Allows auto coloration
			}
			else if ( selection == "CUSTOMER" )
			{
				ShowDetails . Tag = false;
				ShowCust . Tag = true;
			}
			else if ( selection == "DETAILS" )
			{
				Tag = false;
				ShowDetails . Tag = true;
			}
		}

		//*********************************************************************************************************//
		private void BankGrid_MouseRightButtonUp ( object sender, MouseButtonEventArgs e )
		{
			Type type;
			string cellData;
			int row = -1;
			int col = -1;
			string colName = "";
			object rowdata = null;
			object cellValue = null;


			if ( CurrentDb == "BANKACCOUNT" )
			{
				//				BankAccountViewModel bvm = bvm ();
				cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
				if ( row == -1 )
					row = BankGrid . SelectedIndex;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel bvm = cvm;
				cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
				if ( row == -1 )
					row = BankGrid . SelectedIndex;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				CustomerViewModel bvm = cvm;
				cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
				if ( row == -1 )
					row = BankGrid . SelectedIndex;
			}
			if ( cellValue == null )
			{
				MessageBox . Show ( $"Cannot access Data in the current cell, Row returned = {row}, Column = {col}, Column Name = {colName}" );
				return;
			}
			else if ( row == -1 && col == -1 )
			{
				//Header was clicked in
				type = cellValue . GetType ( );
				cellData = cellValue . ToString ( );
				if ( cellData != "" )
				{
					if ( cellData . Contains ( ":" ) )
					{
						int offset = cellData . IndexOf ( ':' );
						string result = cellData . Substring ( offset + 1 ) . Trim ( );
						MessageBox . Show ( $"Column clicked was a Header  =\"{result}\"" );
					}
				}
				return;
			}
			type = cellValue . GetType ( );
			cellData = cellValue . ToString ( );
			MessageBox . Show ( $"Data in the current cell \r\nColumn is \"{colName},\", Data Type=\"{type . Name}\"\r\nData = [{cellData}]\",\r\nRow={row}, Column={col}", "Requested Cell Contents" );
		}
		//*********************************************************************************************************//
		private void BankGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			return;
		}
		//*********************************************************************************************************//
		private void ShowBank_KeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
		}
		//*********************************************************************************************************//
		protected override void OnKeyDown ( KeyEventArgs e )
		{
			if ( e . Key == Key . Enter )

				return;
			else
				base . OnKeyDown ( e );
		}
		//private void OnKeyDown ( object sender, KeyEventArgs e )
		//{
		//	if ( e . Key == Key . Enter )

		//		return;
		//	else
		//		base . OnKeyDown ( e );

		//}
		//*********************************************************************************************************//
		private async void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			DataGrid dg;
			int CurrentRow = 0;
			if ( e . Key == Key . Escape )
			{
				//clear flags in ViewModel
				if ( CurrentDb == "BANKACCOUNT" )
				{
					Flags . ActiveSqlDbViewer = null;
					BankAccountViewModel . ClearFromSqlList ( this . BankGrid, CurrentDb );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					Flags . ActiveSqlDbViewer = null;
					BankAccountViewModel . ClearFromSqlList ( this . CustomerGrid, CurrentDb );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					Flags . ActiveSqlDbViewer = null;
					BankAccountViewModel . ClearFromSqlList ( this . DetailsGrid, CurrentDb );
				}
				SendDbSelectorCommand ( 99, "Window is closing", Flags . CurrentSqlViewer );
				RemoveFromViewerList ( );

				//				EventHandlers . ClearWindowHandles ( null, this );
				BankAccountViewModel . EditdbWndBank = null;
				Flags . CurrentSqlViewer = null;
				UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
				Close ( );
			}
			else if ( e . Key == Key . RightAlt )
			{       // list Flags in Console
				Flags . ListGridviewControlFlags ( );
			}
			else if ( e . Key == Key . Down )
			{       // DataGrid keyboard navigation = DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 1 )
				{
					dg . SelectedIndex++;
					dg . SelectedItem = dg . SelectedIndex;
					dg . ScrollIntoView ( dg . SelectedItem );
				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . Up )
			{       // DataGrid keyboard navigation = UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex > 0 )
				{
					dg . SelectedIndex--;
					dg . SelectedItem = dg . SelectedIndex;
					dg . ScrollIntoView ( dg . SelectedItem );
				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . PageUp )
			{       // DataGrid keyboard navigation = PAGE UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex >= 10 )
				{
					dg . SelectedIndex -= 10;
					dg . ScrollIntoView ( dg . SelectedItem );
				}
				else
				{
					dg . SelectedIndex = 0;
					dg . ScrollIntoView ( dg . SelectedItem );

				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . PageDown )
			{       // DataGrid keyboard navigation = PAGE DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 10 )
				{
					dg . SelectedIndex += 10;
					dg . ScrollIntoView ( dg . SelectedItem );
				}
				else
				{
					dg . SelectedIndex = dg . Items . Count - 1;
					dg . ScrollIntoView ( dg . SelectedItem );

				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . Home )
			{       // DataGrid keyboard navigation = HOME
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				dg . SelectedIndex = 0;
				dg . ScrollIntoView ( dg . SelectedItem );
				//				ItemsView_OnSelectionChanged ( dg, null );
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . End )
			{       // DataGrid keyboard navigation = END
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				dg . SelectedIndex = dg . Items . Count - 1;
				dg . ScrollIntoView ( dg . SelectedItem );
				//				ItemsView_OnSelectionChanged ( dg, null );
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				dg . ScrollIntoView ( dg . SelectedItem );
			}
			else if ( e . Key == Key . Delete )
			{       // DataGrid keyboard navigation = DELETE
				var v = e . OriginalSource . GetType ( );
				// Check to see if Del was pressed while Editing a field (in ANY of our grids)
				// if we have pressed it with just a Row selected, it will return "DataGridCell"  in v.Name
				// else it will have cell info in it
				if ( v . Name != "DataGridCell" )
				{
					e . Handled = false;
					return;         //NOT a Row that is selected, so let OS handle it normally
				}
				if ( CurrentDb == "BANKACCOUNT" )
				{
					dg = BankGrid;
					CurrentRow = dg . SelectedIndex;
					// Get and save the data in the row so we have access to it once it has gone from interface
					BankAccountViewModel BankRecord = BankGrid . SelectedItem as BankAccountViewModel;
					dg . ItemsSource = null;
					bvm . BankAccountObs . Clear ( );
					dtBank?.Clear ( );
					//					dg . Items?.Clear ( );
					//Remove it from SQL Db as well
					DeleteRecord ( "BANKACCOUNT", BankRecord . BankNo, BankRecord . CustNo );
					dg . ItemsSource = bvm . BankAccountObs;
					//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
					// It also broadcasts to any other viewer to update  if needed
					Console . WriteLine ( $"BANKACCOUNT : {BankGrid . Items?.Count}" );
					await bvm . LoadBankTaskInSortOrder ( true, 0 );
					//List<Task<bool>> tasks = new List<Task<bool>> ( );
					//tasks . Add ( bvm . LoadBankTaskInSortOrder ( true, 0 ) );
					//var Results = await Task . WhenAll ( tasks );
					Console . WriteLine ( $"BANKACCOUNT : {BankGrid . Items?.Count}" );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					dg = CustomerGrid;
					CurrentRow = dg . SelectedIndex;
					// Get and save the data in the row so we have access to it once it has gone from interface
					CustomerViewModel CustRecord = CustomerGrid . SelectedItem as CustomerViewModel;
					dg . ItemsSource = null;
					cvm . CustomersObs . Clear ( );
					dtCust?.Clear ( );
					//Remove it from SQL Db as well
					DeleteRecord ( "CUSTOMER", CustRecord . BankNo, CustRecord . CustNo );
					dg . ItemsSource = cvm . CustomersObs;
					//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
					// It also broadcasts to any other viewer to update  if needed
					Console . WriteLine ( $"CUSTOMER: {CustomerGrid . Items?.Count}" );
					cvm . LoadCustomerTaskInSortOrder ( true, 0 );
					//List<Task<bool>> tasks = new List<Task<bool>> ( );
					//tasks . Add ( cvm . LoadCustomerTaskInSortOrder ( true, 0 ) );
					//var Results = await Task . WhenAll ( tasks );
					Console . WriteLine ( $"CUSTOMER: {CustomerGrid . Items?.Count}" );
				}
				else
				{
					dg = DetailsGrid;
					// Get and save the data in the row so we have access to it once it has gone from interface
					DetailsViewModel DetailsRecord = DetailsGrid . SelectedItem as DetailsViewModel;
					CurrentRow = dg . SelectedIndex;
					// Remove it form THIS DataGrid here
					dg . ItemsSource = null;
					dvm . DetailsObs . Clear ( );
					dtDetails?.Clear ( );
					//Remove it from SQL Db as well
					DeleteRecord ( "DETAILS", DetailsRecord . BankNo, DetailsRecord . CustNo );
					Console . WriteLine ( $"Record DELETED - Grid Rows = {DetailsGrid . Items?.Count}, dtDetails = {dtDetails?.Rows . Count}, Collection = {dvm . DetailsObs}" );
					//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
					// It also broadcasts to any other viewer to update  if needed
					Console . WriteLine ( $"DETAILS : {DetailsGrid . Items?.Count}" );
					await dvm . LoadDetailsTaskInSortOrder ( true, 0 );
					//List<Task<bool>> tasks = new List<Task<bool>> ( );
					//tasks . Add ( dvm . LoadDetailsTaskInSortOrder ( true, 0 ) );
					//var Results = await Task . WhenAll ( tasks );
					Console . WriteLine ( $"DETAILS : {DetailsGrid . Items?.Count}" );
				}
#pragma TESTING DATA LOAD CALLBACK
				return;



				// Tidy up our own grid after ourselves				
				if ( dg . Items . Count > 0 && CurrentRow >= 0 )
					dg . SelectedIndex = CurrentRow;
				else if ( dg . Items . Count == 1 )
					dg . SelectedIndex = 0;

				//dg.SelectedIndex = Flags.
				dg . SelectedItem = dg . SelectedIndex;
				dg . ScrollIntoView ( dg . SelectedItem );

				// now tell any other open viewers they need to update
				ItemsView_OnSelectionChanged ( dg, null );
				if ( CurrentDb == "BANKACCOUNT" )
				{
					SendDataChanged ( this, BankGrid, "BANKACCOUNT" );
					DetailsGrid_SelectedCellsChanged ( dg, null );
				}
				else if ( CurrentDb == "CUSTOMER" )

				{
					SendDataChanged ( this, CustomerGrid, "CUSTOMER" );
					BankGrid_SelectedCellsChanged ( dg, null );
				}
				else if ( CurrentDb == "DETAILS" )

				{
					SendDataChanged ( this, DetailsGrid, "DETAILS" );
					BankGrid_SelectedCellsChanged ( dg, null );
				}
			}
			e . Handled = true;
		}
		/// <summary>
		/// CALLED BY ANY GRID TO PHYSICALLY DELETE A ROW of DATA FROM THE dB
		/// </summary>
		/// <param name="Caller"></param>
		/// <param name="Bankno"></param>
		/// <param name="Custno"></param>
		/// <returns></returns>
		public bool DeleteRecord ( string Caller, string Bankno, string Custno )
		{
			string Command = "";
			bool Result = false;
			Command = $"Delete from BANKACCOUNT WHERE BANKNO= {Bankno} AND Custno= {Custno} ";

			SqlConnection con = null;
			string ConString = "";
			ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
			try
			{
				//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
				using ( con = new SqlConnection ( ConString ) )
				{
					con . Open ( );
					SqlCommand cmd = new SqlCommand ( Command, con );
					cmd . ExecuteNonQuery ( );
					Console . WriteLine ( $"SQL Deletion successful from BANKACCOUNT DataBase for CustNo= {Custno} & BankNo= {Bankno}..." );

					Command = $"Delete from CUSTOMER WHERE BANKNO={Bankno} AND Custno={Custno}";
					cmd = new SqlCommand ( Command, con );
					cmd . ExecuteNonQuery ( );
					Console . WriteLine ( $"SQL Deletion successful from CUSTOMER DataBase for CustNo= {Custno} & BankNo= {Bankno}..." );

					Command = $"Delete from SECACCOUNTS WHERE BANKNO= {Bankno} AND Custno= {Custno}";
					cmd = new SqlCommand ( Command, con );
					cmd . ExecuteNonQuery ( );
					Console . WriteLine ( $"SQL Deletion successful from DETAILS  DataBase for CustNo= {Custno} & BankNo= {Bankno}..." );
					Result = true;
				}
			}
			catch ( Exception ex )
			{
				con . Close ( );
				Console . WriteLine ( $"SQL Error DeleteRecord(2381)- {ex . Message} Data = {ex . Data}" );
#if SHOWSQLERRORMESSAGEBOX
				MessageBox . Show ( "SQL error occurred in DeleteRecord(2383) - See Output for details" );
#endif
			}
			finally
			{
				//Lets force the grids to update when we return from here ??
				Console . WriteLine ( $"SQL - Row for Bank A/c {Bankno} & Customer # {Custno} deleted in {Caller} Db successfully" );
				con . Close ( );
			}
			return Result;
		}
		//public void KeyboardFocusChangedEventHandler ( object sender, KeyboardFocusChangedEventArgs e )
		//{
		//	object o = sender;
		//	SqlDbViewer sql = o as SqlDbViewer;

		//}

		#endregion Keyboard handlers
		//*********************************************************************************************************//

		#region Tuple Handlers
		/// <returns>A fully populated Tuple</returns>
		//*********************************************************************************************************//
		public static object CreateTuple ( string currentDb )
		{
			object tpl = null;
			//content of Tuple is : (This, string "currentDb", int selectedIndex, , int Tag, object selectedItem)
			/*
			Item1 = current SqlDbViewer
			Item2 = CurrentDb string`
			Item3 = Grid.SelectedIndex
				  */
			if ( currentDb == "BANKACCOUNT" )
				tpl = Tuple . Create ( Flags . CurrentSqlViewer, currentDb, Flags . SqlBankGrid . SelectedIndex );
			else if ( currentDb == "CUSTOMER" )
				tpl = Tuple . Create ( Flags . CurrentSqlViewer, currentDb, Flags . SqlCustGrid . SelectedIndex );
			else if ( currentDb == "DETAILS" )

				tpl = Tuple . Create ( Flags . CurrentSqlViewer, currentDb, Flags . SqlDetGrid . SelectedIndex );
			return tpl;
		}


		/// <summary>
		/// Fjunction that create a fully populated Tuple from the current DataGrid and Viewer Window
		/// Content of Tuple is : ( this, string "currentDb", int selectedIndex, , int Tag, object (a datarecord basically) selectedItem)
		/// </summary>
		/// <param name="currentDb" is the current viewer type identifier  eg"CUSTOMER"></param>

		//*********************************************************************************************************//
		//Receiver for messages FROM DbSelector
		/// <summary>
		/// Good example of how to pass Tuples around
		/// </summary>
		/// <param name="tuple"></param>
		//*********************************************************************************************************//
		public void GetTupleData ( Tuple<SqlDbViewer, string, int> tuple )
		{
			//content of Tuple is : (This, string "currentDb", int selectedIndex, , int Tag, object selectedItem)
			if ( tuple . Item2 == "BANKACCOUNT" )
			{
			}
			else if ( tuple . Item2 == "CUSTOMER" )
			{
			}
			else if ( tuple . Item2 == "DETAILS" )
			{
			}
		}

		#endregion Tuple Handlers
		//*********************************************************************************************************//

		#region Focus handling
		private void BankGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			Flags . SqlBankGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . BankGrid );
		}

		private void CustomerGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			Flags . SqlCustGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . CustomerGrid );
		}

		private void DetailsGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			Flags . SqlDetGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . DetailsGrid );
		}
		#endregion Focus handling
		//*********************************************************************************************************//
		static void HandleEdit ( object sender, EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			if ( Flags . EventHandlerDebug )
				Console . WriteLine ( $"\r\nRecieved by SQLDBVIEWER (150) Caller={e . Caller}, Index = {e . CurrentIndex},  Grid = {e . ToString ( )}\r\n " );
		}

		/// <summary>
		/// handle clearing down the data to allow a switch to a different Db view
		/// </summary>
		//*********************************************************************************************************//

		/// <summary>
		///  This is aMASSIVE Function that handles updating the Dbs via SQl plu sorting the current grid
		///  out & notifying all other viewers that a change has occurred so they can (& in fact do) update 
		///  their own data grids rather nicely right now - 22/4/21
		/// </summary>
		/// <param name="sender">Unused</param>
		/// <param name="e">Unused</param>
		public async void ViewerGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			///
			/// This ONLY gets called when a cell is edited
			/// After a fight, this is now working and updates the relevant RECORD correctly
			/// 

			BankAccountViewModel ss = bvm;
			CustomerViewModel cs = cvm;
			DetailsViewModel sa = dvm;

			//CustomerViewModel cv = cvm as CustomerViewModel;
			//	BankAccountViewModel bv = bvm as BankAccountViewModel;
			//	DetailsViewModel dv = e.Row.Item as DetailsViewModel;
			//if data has NOT changed, do NOT bother updating the Db
			// Clever stuff Eh - saves lots of processing time?
			if ( !SelectionhasChanged )
				return;
			else
				SelectionhasChanged = false;    // clear the edit status flag again

			SendDbSelectorCommand ( 103, $"CHANGES made to data in {CurrentDb} Db)", Flags . CurrentSqlViewer );


			Mouse . OverrideCursor = Cursors . Wait;
			//Only called whn an edit has been completed
			if ( e == null )
			{
				SQLHandlers sqlh = new SQLHandlers ( );
				Task t1;
				if ( CurrentDb == "BANKACCOUNT" )
				{
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel . SqlUpdating = true;
					t1 = await Task . Factory . StartNew ( ( ) => sqlh . UpdateDbRow ( "BANKACCOUNT", ( object ) ss ) );
					ss = BankCurrentRow . Item as BankAccountViewModel;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					t1 = await Task . Factory . StartNew ( ( ) => sqlh . UpdateDbRow ( "DETAILS", ( object ) sa ) );
					sa = DetailsCurrentRow . Item as DetailsViewModel;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					t1 = await Task . Factory . StartNew ( ( ) => sqlh . UpdateDbRow ( "CUSTOMER", ( object ) cs ) );
					cs = CustomerCurrentRow . Item as CustomerViewModel;
				}
			}
			else
			{
				// Save pointer to the viewer that is triggering the change event so we can get back to it
				Flags . SqlUpdateOriginatorViewer = this;

				SQLHandlers sqlh = new SQLHandlers ( );
				if ( CurrentDb == "BANKACCOUNT" )
				{
					ss = new BankAccountViewModel ( );
					ss = e . Row . Item as BankAccountViewModel;
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel . SqlUpdating = true;
					await sqlh . UpdateDbRow ( "BANKACCOUNT", ( object ) ss );
					SendDataChanged ( this, BankGrid, CurrentDb );
					Mouse . OverrideCursor = Cursors . Arrow;
					return;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					cs = new CustomerViewModel ( );
					cs = e . Row . Item as CustomerViewModel;
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel . SqlUpdating = true;
					await sqlh . UpdateDbRow ( "CUSTOMER", ( object ) cs );
					SendDataChanged ( this, CustomerGrid, CurrentDb );
					Mouse . OverrideCursor = Cursors . Arrow;
					return;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					sa = new DetailsViewModel ( );
					sa = e . Row . Item as DetailsViewModel;
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel . SqlUpdating = true;
					await sqlh . UpdateDbRow ( "DETAILS", ( object ) sa );
					SendDataChanged ( this, DetailsGrid, CurrentDb );
					Mouse . OverrideCursor = Cursors . Arrow;
					return;
				}
			}

			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				SqlCommand cmd = null;
				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					decimal Y;
					if ( CurrentDb == "BANKACCOUNT" )
					{
						//						ss = e.Row.Item as BankAccount;
						x = Convert . ToInt32 ( ss . Id );
						x = Convert . ToInt32 ( ss . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL Invalid A/c type of {ss . AcType} in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid A/C Type ({ss . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						Y = Convert . ToDecimal ( ss . Balance );
						Y = Convert . ToDecimal ( ss . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL Invalid Interest Rate of {ss . IntRate} > 100% in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid Interest rate ({ss . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						DateTime dtm = Convert . ToDateTime ( ss . ODate );
						dtm = Convert . ToDateTime ( ss . CDate );
					}
					else if ( CurrentDb == "DETAILS" )
					{
						//						sa = sacc;
						//						sa = e.Row.Item as DetailsViewModel;
						x = Convert . ToInt32 ( sa . Id );
						x = Convert . ToInt32 ( sa . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL Invalid A/c type of {sa . AcType} in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid A/C Type ({sa . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						Y = Convert . ToDecimal ( sa . Balance );
						Y = Convert . ToDecimal ( sa . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL Invalid Interest Rate of {sa . IntRate} > 100% in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid Interest rate ({sa . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						DateTime dtm = Convert . ToDateTime ( sa . ODate );
						dtm = Convert . ToDateTime ( sa . CDate );
					}
					//					string sndr = sender.ToString();
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
					Mouse . OverrideCursor = Cursors . Arrow;
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
					return;
				}
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection ( ConString );
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using ( con )
					{
						con . Open ( );

						if ( CurrentDb == "BANKACCOUNT" )
						{
							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( ss . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of SecAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of Customers successful..." );
						}
						else if ( CurrentDb == "DETAILS" )
						{
							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of SecAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of customers successful..." );
						}
						if ( CurrentDb == "SECACCOUNTS" )
						{
							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( ss . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of SecAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
							await cmd . ExecuteNonQueryAsync ( );
							Console . WriteLine ( "SQL Update of Customers successful..." );
						}
						StatusBar . Text = "Database updated successfully....";
					}
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );

#if SHOWSQLERRORMESSAGEBOX
					Mouse . OverrideCursor = Cursors . Arrow;
					MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif
				}
				finally
				{
					Mouse . OverrideCursor = Cursors . Arrow;
					con . Close ( );
				}
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( e == null && CurrentDb == "CUSTOMER" )
					cs = CustomerCurrentRow . Item as CustomerViewModel;
				else if ( e == null && CurrentDb == "CUSTOMER" )
					cs = e . Row . Item as CustomerViewModel;


				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					x = Convert . ToInt32 ( cs . Id );
					//					string sndr = sender.ToString();
					x = Convert . ToInt32 ( cs . AcType );
					//Check for invalid A/C Type
					if ( x < 1 || x > 4 )
					{
						Console . WriteLine ( $"SQL Invalid A/c type of {cs . AcType} in grid Data" );
						Mouse . OverrideCursor = Cursors . Arrow;
						MessageBox . Show ( $"Invalid A/C Type ({cs . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
						return;
					}
					DateTime dtm = Convert . ToDateTime ( cs . ODate );
					dtm = Convert . ToDateTime ( cs . CDate );
					dtm = Convert . ToDateTime ( cs . Dob );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
					Mouse . OverrideCursor = Cursors . Arrow;
					return;
				}
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection ( ConString );
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using ( con )
					{
						con . Open ( );
						SqlCommand cmd = new SqlCommand ( "UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
							"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
							"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con );

						cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@fname", cs . FName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@lname", cs . LName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr1", cs . Addr1 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr2", cs . Addr2 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@town", cs . Town . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@county", cs . County . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@pcode", cs . PCode . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@phone", cs . Phone . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@mobile", cs . Mobile . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@dob", Convert . ToDateTime ( cs . Dob ) );
						cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
						await cmd . ExecuteNonQueryAsync ( );
						Console . WriteLine ( "SQL Update of Customers successful..." );

						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype,  ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
						cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
						await cmd . ExecuteNonQueryAsync ( );
						Console . WriteLine ( "SQL Update of BankAccounts successful..." );

						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
						cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
						await cmd . ExecuteNonQueryAsync ( );
						Console . WriteLine ( "SQL Update of SecAccounts successful..." );

					}
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );
#if SHOWSQLERRORMESSAGEBOX
					Mouse . OverrideCursor = Cursors . Arrow;
					MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif
				}
				finally
				{
					Mouse . OverrideCursor = Cursors . Arrow;
					con . Close ( );
				}
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}
		//*****************************************************************************************//

		//*********************************************************************************************************//
		public void CloseViewer_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can 
			// Click the button before the window has been fully closed
			Flags . CurrentSqlViewer = this;

			//Close current (THIS) Viewer Window
			//clear flags in GV[] & Flags Structures
			if ( CurrentDb == "BANKACCOUNT" )
			{
				RemoveFromViewerList ( );
				Flags . SqlBankGrid = null;
				Flags . SqlBankViewer = null;
				Flags . SqlBankCurrentIndex = 0;
				MainWindow . gv . Bankviewer = Guid . Empty;
				Flags . ActiveSqlDbViewer = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				RemoveFromViewerList ( );
				Flags . SqlCustGrid = null;
				Flags . SqlCustViewer = null;
				Flags . SqlCustCurrentIndex = 0;
				MainWindow . gv . Custviewer = Guid . Empty;
				Flags . ActiveSqlDbViewer = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				RemoveFromViewerList ( );
				Flags . SqlDetGrid = null;
				Flags . SqlDetViewer = null;
				Flags . SqlDetCurrentIndex = 0;
				MainWindow . gv . Detviewer = Guid . Empty;
				Flags . ActiveSqlDbViewer = null;
			}
			SendDbSelectorCommand ( 99, "Window is closing", Flags . CurrentSqlViewer );
			//Set global flags
			//			EventHandlers . ClearWindowHandles ( null, this );
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			BankAccountViewModel . EditdbWndBank = null;
			//			Flags . CurrentSqlViewer . Tag = Guid . Empty;
			Flags . CurrentSqlViewer = null;
			//			MainWindow . gv . SqlViewerGuid = Guid . Empty;
			Mouse . OverrideCursor = Cursors . Arrow;
			Close ( );
		}
		//*********************************************************************************************************//
		/// <summary>
		/// We are loading a Db into a Grid....
		/// Updates the MainWindow.GridViewer structure data, called
		/// by the 3  different "Show xxxxx" Funstion's
		/// </summary>
		/// <param name="type"></param>
		private void UpdateGridviewController ( string type )
		{
			//Retrieve Window handle of current Viewer window
			int newindex = -1;

			newindex = MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count - 1;
			if ( newindex < 0 )
				return;
			if ( PrettyDetails != "" )
				MainWindow . gv . CurrentDb [ newindex ] = PrettyDetails;
		}
		//*********************************************************************************************************//
		public void UpdateViewersList ( )
		{
			if ( this . Tag == null ) return;
			if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count == 1 )
				return;
			for ( int i = 0 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
			{
				if ( i + 1 == MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count )
					return;
				if ( MainWindow . gv . ListBoxId [ i ] == ( Guid ) Flags . CurrentSqlViewer . Tag )
				{
					ListBoxItem lbi = new ListBoxItem ( );
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ i + 1 ] as ListBoxItem;
					lbi . Content = MainWindow . gv . CurrentDb [ i ];
					break;
				}
			}
		}
		//*********************************************************************************************************//
		public void RemoveFromViewerList ( int x = -1 )
		{
			//Close current (THIS) Viewer Window
			//clear flags in GV[] & Flags Structures
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//				RemoveFromViewerList ( );
				Flags . SqlBankGrid = null;
				Flags . SqlBankViewer = null;
				Flags . SqlBankCurrentIndex = 0;
				MainWindow . gv . Bankviewer = Guid . Empty;
				Flags . ActiveSqlDbViewer = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				//				RemoveFromViewerList ( );
				Flags . SqlCustGrid = null;
				Flags . SqlCustViewer = null;
				Flags . SqlCustCurrentIndex = 0;
				MainWindow . gv . Custviewer = Guid . Empty;
				Flags . ActiveSqlDbViewer = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				//				RemoveFromViewerList ( );
				Flags . SqlDetGrid = null;
				Flags . SqlDetViewer = null;
				Flags . SqlDetCurrentIndex = 0;
				MainWindow . gv . Detviewer = Guid . Empty;
				Flags . ActiveSqlDbViewer = null;
			}
			SendDbSelectorCommand ( 99, "Window is closing", Flags . CurrentSqlViewer );
			// THIS WORKED   19/4/21
			DeleteViewerAndFlags ( x );
			return;

			int viewerEntryCount = 0;
			if ( this . Tag == null ) return;
			if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count == 1 )
				return;
			for ( int i = 0 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
			{
				if ( i >= MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count )
					return;
				if ( MainWindow . gv . ListBoxId [ i ] == ( Guid ) this . Tag )
				{
					//					int currentindex = MainWindow.gv.DbSelectorWindow.ViewersList.SelectedIndex;
					Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( viewerEntryCount );
					break;
				}
				viewerEntryCount++;
			}
			Flags . DbSelectorOpen . ViewersList . Refresh ( );
			// If all viewers are closed, tidy up control structure dv[]
			if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count == 1 )
			{
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlViewerWindow = null;
			}
		}
		//*********************************************************************************************************//
		private void ParseButtonText ( bool obj )
		{
			ShowBank . Content = $" Bank A/c's  (0)";
			ShowCust . Content = $"Customer A/c's  (0)";
			ShowDetails . Content = $"Details A/c's  (0)";
			//			Flags.Flags.IsMultiMode
			if ( IsFiltered != "" )
			{
				if ( IsFiltered == "BANKACCOUNT" )
				{
					ShowBank . Content = $"(F) Bank A/c's  ({BankGrid . Items . Count})";
					MainWindow . gv . CurrentDb [ MainWindow . gv . ViewerCount - 1 ] = ( string ) ShowBank . Content;
				}
				else if ( IsFiltered == "CUSTOMER" )
				{
					ShowCust . Content = $"(F) Customer A/c's  ({CustomerGrid . Items . Count})";
					MainWindow . gv . CurrentDb [ MainWindow . gv . ViewerCount - 1 ] = ( string ) ShowCust . Content;
				}
				else if ( IsFiltered == "DETAILS" )
				{
					ShowDetails . Content = $"(F) Details A/c's  ({DetailsGrid . Items . Count})";
					MainWindow . gv . CurrentDb [ MainWindow . gv . ViewerCount - 1 ] = ( string ) ShowDetails . Content;
				}
			}
			else
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					if ( !obj )
					{
						if ( Flags . IsMultiMode )
							ShowBank . Content = $"<M> Bank A/c's  ({this . BankGrid . Items . Count})";
						else
							ShowBank . Content = $"Bank A/c's  ({this . BankGrid . Items . Count})";
					}
					else
						ShowBank . Content = $"Bank A/c's  ({this . BankGrid . Items . Count})";

				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( !obj )
					{
						if ( Flags . IsMultiMode )
							ShowCust . Content = $"<M> Customer A/c's  ({this . CustomerGrid . Items . Count})";
						else
							ShowCust . Content = $"Customer A/c's  ({this . CustomerGrid . Items . Count})";
					}
					else
						ShowCust . Content = $"Customer A/c's  ({this . CustomerGrid . Items . Count})";
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( !obj )
					{
						if ( Flags . IsMultiMode )
							ShowDetails . Content = $"<M> Details A/c's  ({this . DetailsGrid . Items . Count})";
						else
							ShowDetails . Content = $"Details A/c's  ({this . DetailsGrid . Items . Count})";
					}
					else
						ShowDetails . Content = $"Details A/c's  ({this . DetailsGrid . Items . Count})";
				}
			}
		}
		//*********************************************************************************************************//
		private void ScrollList ( DataGrid grid, int count )
		{
			if ( grid . SelectedItem == null ) return;
			grid . ScrollIntoView ( grid . SelectedItem );
			var border = System . Windows . Media . VisualTreeHelper . GetChild ( grid, 0 ) as Decorator;

			if ( border == null ) return;
			var scroll = border . Child as ScrollViewer;

			if ( scroll == null ) return;

			scroll . UpdateLayout ( );
			return;
#pragma TODO
			{
				//			//Temp
				//			return;

				//			if (grid.Items.Count > 0)
				//			{
				//				//				grid.Refresh ();
				//				var border = VisualTreeHelper.GetChild (grid, 0) as Decorator;
				//				if (border != null)
				//				{
				//					var scroll = border.Child as ScrollViewer;
				//					if (scroll != null)
				//					{
				//						//						grid.ScrollIntoView (grid.SelectedItem);
				//						if (currentindex + 5 < maxrows)
				//						{

				//							for (int i = 0; i < 5; i++)
				//							{
				//								scroll.LineDown (); scroll.UpdateLayout ();
				//							}
				//						}
				//						else if (currentindex > 5)
				//						{
				//							grid.ScrollIntoView (selitem);
				//						}
				//					}
				//					//scroll.ScrollToEnd ();
				//				}
				//			}
			}
		}

		//*********************************************************************************************************//
		private void UpdateRowDetails ( object datarecord, string caller )
		// This updates the data in the DbSelector window's Viewers listbox, or add a new entry ????
		{
			bool Updated = false;
			if ( this . Tag == null ) return;
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( x >= Flags . DbSelectorOpen . ViewersList . Items . Count ) break;
				if ( MainWindow . gv . ListBoxId [ x ] == ( Guid ) this . Tag )
				{
					if ( caller == "BankGrid" )
					{
						//	BankAccountViewModel record = (BankAccountViewModel)datarecord;
						var record = datarecord as BankAccountViewModel;//CurrentBankSelectedRecord;
						MainWindow . gv . CurrentDb [ x ] = $"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, Balance £ {record?.Balance}, Interest {record?.IntRate}%";
						PrettyDetails = MainWindow . gv . CurrentDb [ x ];
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						UpdateDbSelectorItem ( PrettyDetails );
					}
					else if ( caller == "CustomerGrid" )

					{
						var record = datarecord as CustomerViewModel;
						MainWindow . gv . CurrentDb [ x ] = $"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, {record?.LName} {record?.Town}, {record?.County}";
						PrettyDetails = MainWindow . gv . CurrentDb [ x ];
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
#pragma NOT NEEDED????
						UpdateDbSelectorItem ( PrettyDetails );
					}
					else if ( caller == "DetailsGrid" )
					{
						var record = datarecord as DetailsViewModel;
						MainWindow . gv . CurrentDb [ x ] = $"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, Balance {record?.Balance}, Interest % {record?.IntRate}";
						PrettyDetails = MainWindow . gv . CurrentDb [ x ];
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						UpdateDbSelectorItem ( PrettyDetails );
					}
					ExtensionMethods . Refresh ( Flags . DbSelectorOpen . ViewersList );
					break;
				}
			}
			if ( !Updated )
			{
				if ( Notifier != null )
					Notifier ( 111, "UpdateRowDetails has NOT updated anything ???", null );
				// UNUSED
				{
					//				Console.WriteLine ("\nUpdateRowDetails has NOT up[dated anything ???\n");
					//				if (Flags.SqlViewerIsLoading) 
					//					return;
					//				// Seems there are no entries right now, so we are starting up
					//				int x = MainWindow.gv.ViewerCount;
					//				{
					//					MainWindow.gv.CurrentDb[x] = caller;
					//					MainWindow.gv.SqlViewerWindow = this;
					//					if (caller == "BankGrid")
					//					{
					//						MainWindow.gv.window[x] = this;
					//						MainWindow.gv.Bankviewer = MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
					//						MainWindow.gv.ViewerCount++;
					////						MainWindow.gv.Bankviewer = (Guid)this.Tag;
					//						MainWindow.gv.CurrentDb[x] = MainWindow.gv.PrettyDetails;
					//						MainWindow.gv.Datagrid[x] = BankGrid;

					//					}

					//					else if (caller == "CustomerGrid")
					//					{
					//						MainWindow.gv.window[x] = this;
					//						MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
					//						MainWindow.gv.ViewerCount++;
					//						MainWindow.gv.Custviewer = (Guid)this.Tag;
					//						MainWindow.gv.CurrentDb[x] = MainWindow.gv.PrettyDetails;
					//						MainWindow.gv.Datagrid[x] = CustomerGrid;
					//					}
					//					else if (caller == "DetailsGrid")
					//					{
					//						MainWindow.gv.window[x] = this;
					//						MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
					//						MainWindow.gv.ViewerCount++;
					//						MainWindow.gv.Detviewer = (Guid)this.Tag;
					//						MainWindow.gv.CurrentDb[x] = MainWindow.gv.PrettyDetails;
					//						MainWindow.gv.Datagrid[x] = DetailsGrid;
					//					}
					//				}
					//				Guid g = (Guid)Flags.CurrentSqlViewer.Tag;
				}
			}
		}
		//*********************************************************************************************************//
		private void DetailsGrid_LoadingRowDetails ( object sender, DataGridRowDetailsEventArgs e )
		{
			// row data Loading ???
			MainWindow . gv . Datagrid [ LoadIndex ] = this . DetailsGrid;
		}

		//*********************************************************************************************************//
		private void BankGrid_LoadingRowDetails ( object sender, DataGridRowDetailsEventArgs e )
		{
			// row data Loading ???
			MainWindow . gv . Datagrid [ LoadIndex ] = this . BankGrid;

		}

		//*********************************************************************************************************//
		private void CustomerGrid_TargetUpdated ( object sender, System . Windows . Data . DataTransferEventArgs e )
		//*****************************************************************************************//
		{
			// row data Loading ???
			MainWindow . gv . Datagrid [ LoadIndex ] = this . CustomerGrid;
			this . CustomerGrid . SelectedIndex = 0;
			SelectedRow = 0;

		}
		private void Window_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Actually, this is Called mostly by MouseDown Handler
			//when Focus has been set to this window
			//Set global flags
			if ( Flags . CurrentSqlViewer == this ) return;

			Flags . CurrentSqlViewer = this;
			if ( CurrentDb == "BANKACCOUNT" )
				Flags . SetGridviewControlFlags ( this, this . BankGrid );
			else if ( CurrentDb == "CUSTOMER" )
				Flags . SetGridviewControlFlags ( this, this . CustomerGrid );
			else if ( CurrentDb == "DETAILS" )
				Flags . SetGridviewControlFlags ( this, this . DetailsGrid );

			Guid tag = Guid . Empty;
			tag = ( Guid ) this . Tag;
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				// find Tag that matches our Tag in ViewersList  
				if ( MainWindow . gv . ListBoxId [ x ] == tag )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = x + 1;
					Flags . DbSelectorOpen . ViewersList . Refresh ( );
					break;
				}
			}
		}

		/// <summary>
		///  Function handles TWO seperate functions
		///  1 - Clear the entry MainWindow.gv[]
		///  2 - Remove correct line form DbSelector.ViewersList
		/// </summary>
		//*********************************************************************************************************//
		public static bool DeleteViewerAndFlags ( int x = -1 )
		{
			SqlDbViewer sqlv;                        // x = GridView[] index if received
			Guid tag = ( Guid ) Flags . CurrentSqlViewer?.Tag;
			ListBoxItem lbi = new ListBoxItem ( );
			if ( x == -1 )
			{ // Delete all
				for ( int z = 0 ; z < MainWindow . gv . MaxViewers ; z++ )
				{
					MainWindow . gv . CurrentDb [ z ] = "";
					MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
					MainWindow . gv . Datagrid [ z ] = null;
					MainWindow . gv . window [ z ] = null;
				}
				MainWindow . gv . ViewerCount = 0;
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlBankViewer = null;
				MainWindow . gv . SqlCustViewer = null;
				MainWindow . gv . SqlDetViewer = null;
				MainWindow . gv . SqlViewerGuid = Guid . Empty;
				MainWindow . gv . SqlViewerWindow = null;

				MainWindow . gv . Bankviewer = Guid . Empty;
				MainWindow . gv . Custviewer = Guid . Empty;
				MainWindow . gv . Detviewer = Guid . Empty;

				Flags . ActiveSqlViewer = null;
				Flags . ActiveSqlGrid = null;
				Flags . SqlBankViewer = null;
				Flags . SqlCustViewer = null;
				Flags . SqlDetViewer = null;
				Flags . CurrentSqlViewer = null;
				Flags . ActiveSqlViewer = null;

				// ALL entries in our GridView structure are now cleared  ** totally **
				return true;
			}
			else
			{
				// we have received the index of the viewer in the list
				// so  get the Tag of that selected Entry
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				Guid lbtag = ( Guid ) lbi . Tag;
				// Get a pointer to the window so we can close it
				sqlv = Flags . CurrentSqlViewer as SqlDbViewer;
				//See if it matches the one we are closing down
				if ( ( Guid ) lbtag == ( Guid ) tag )
				{

					// We know which gv[] entry  we need to clear, so do it and return
					MainWindow . gv . ViewerCount--;
					MainWindow . gv . CurrentDb [ x ] = "";
					MainWindow . gv . ListBoxId [ x ] = Guid . Empty;
					MainWindow . gv . Datagrid [ x ] = null;
					// Actually close thre Viewer window here, before we delete the relevant pointers
					sqlv . Close ( );
					MainWindow . gv . window [ x ] = null;
				}
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlViewerGuid = Guid . Empty;
				//Finally we can remove this entry from ViewersList
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				lbi . Content = "";
				Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( x );
				// Set selectedIndex pointer to current position in list
				int currentIndex = x - 1;
				if ( x <= 1 )             // List is basically empty (No viewers in  the list)
					return true;
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count > currentIndex )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex;
					Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex;
				}
				else if ( Flags . DbSelectorOpen . ViewersList . Items . Count == currentIndex )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex - 1;
					Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex - 1;
				}
				sqlv . Close ( );
				return true;
			}
			// Now sort out the  global gv[] flags
			for ( int y = 1 ; y < Flags . DbSelectorOpen . ViewersList . Items . Count ; y++ )
			{
				// Get the Tag of eaxch Viewer in the list
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ y ] as ListBoxItem;
				Guid lbtag = ( Guid ) lbi . Tag;
				//See if it matches the one we are closing down
				if ( ( Guid ) lbtag == ( Guid ) tag )
				{
					//Yes, we have got a match, so go ahead and remove its gv[] entries first
					for ( int z = 0 ; z < MainWindow . gv . MaxViewers ; z++ )
					{
						if ( MainWindow . gv . ListBoxId [ z ] == lbtag )
						{
							MainWindow . gv . ViewerCount--;
							MainWindow . gv . CurrentDb [ z ] = "";
							MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
							MainWindow . gv . Datagrid [ z ] = null;
							MainWindow . gv . window [ z ] = null;
							break;
						}

					}
					MainWindow . gv . PrettyDetails = "";
					//Finally we can remove this entry from ViewersList
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ y ] as ListBoxItem;
					lbi . Content = "";
					Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( y );
					// Set selectedIndex pointer to current position in list
					int currentIndex = y - 1;
					if ( y <= 1 )             // List is basically empty (No viewers in  the list)
						return true;
					if ( Flags . DbSelectorOpen . ViewersList . Items . Count > currentIndex )
					{
						Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex;
						Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex;
					}
					else if ( Flags . DbSelectorOpen . ViewersList . Items . Count == currentIndex )
					{
						Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex - 1;
						Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex - 1;
					}
					return true;
				}
			}
			MainWindow . gv . SqlViewerGuid = Guid . Empty;

			return false;
		}
		//*********************************************************************************************************//
		private void Window_Closed ( object sender, EventArgs e )
		{
			//			SqlDbViewer sqlv = this;
			//			DeleteViewerAndFlags ();
			// clear final Viewer pointer when ALL viewers are closed down

			if ( this . BankGrid != null )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankGrid . ItemsSource = null;
					BankGrid?.Items . Clear ( );
					dtBank?.Rows . Clear ( );
					bvm . BankAccountObs . Clear ( );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					CustomerGrid . ItemsSource = null;
					CustomerGrid?.Items . Clear ( );
					dtCust?.Rows . Clear ( );
					cvm . CustomersObs . Clear ( );
				}
				else if ( this . BankGrid != null )
				{
					DetailsGrid . ItemsSource = null;
					DetailsGrid?.Items . Clear ( );
					dtDetails?.Rows . Clear ( );
					dvm . DetailsObs . Clear ( );
				}
			}
			if ( MainWindow . gv . ViewerCount == 0 )
				MainWindow . gv . SqlViewerWindow = null;
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );

			// Unsubscribe form relevant events here
			NotifyOfDataChange -= bvm . DbHasChangedHandler;
			NotifyOfDataChange -= cvm . DbHasChangedHandler;
			NotifyOfDataChange -= dvm . DbHasChangedHandler;
			// point to our callback function - DbDataLoadedHandler(object, DataLoadedArgs)
			NotifyOfDataLoaded -= DbDataLoadedHandler;
			EventHandlers . EditDbViewerSelectedIndexChanged -= OnEditDbViewerIndexChanged;
			Console . WriteLine ( $"Unsubscribed from All events successfully" );
			Console . WriteLine ( $"\n***Window has just closed***" );
			Flags . ListGridviewControlFlags ( );
		}

		//*********************************************************************************************************//
		private void Minimize_click ( object sender, RoutedEventArgs e )
		{
			Window_GotFocus ( sender, null );
			//Window_MouseDown ( sender, null );
			this . WindowState = WindowState . Normal;
		}
		//*********************************************************************************************************//
		public void CreateListboxItemBinding ( )
		{
			Binding binding = new Binding ( "ListBoxItemText" );
			binding . Source = PrettyDetails;
		}

		private void CustomerGrid_CurrentCellChanged ( object sender, EventArgs e )
		{
			//We get this on entering any cell to edit
#pragma possible cure
			//if (CustomerGrid.SelectedItem != null)
			//{
			//	if (CustomerGrid.SelectedItem == null)
			//		return;
			//	TextBlock tb = new TextBlock ();

			//	//This gives me an entrie Db Record in "c"
			//	Customer c = CustomerGrid.SelectedItem as  Customer;
			//	Console.WriteLine ($"CustomerGrid_CurrentCellChanged - Identified row data of [{c.CustNo} - {c.FName} {c.LName}]");

			//}
		}

		/// <summary>
		/// Updates the text of the relevant ViewersList entry when selection is changed
		/// </summary>
		/// <param name="data"></param>
		//*********************************************************************************************************//
		public static void UpdateDbSelectorItem ( string data )
		{
			//			bool IsAdded = false;
			if ( Flags . CurrentSqlViewer . Tag is null ) return;
			// handle global flag to control viewer addition/updating
			if ( Flags . SqlViewerIsLoading ) return;
			//			else
			//				Flags.SqlViewerIsLoading = true;

			Guid tag = ( Guid ) Flags . CurrentSqlViewer . Tag;

			ListBoxItem lbi;// = new ListBoxItem ();
			if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count > 1 )
			{
				for ( int i = 1 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
				{
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ i ] as ListBoxItem;
					//We start loop at ONE, so need ot use index MINUS 1 to access gv[] data correctly
					Guid lbtag = MainWindow . gv . ListBoxId [ i - 1 ];
					if ( lbtag == tag )
					{
						//got the matching entry, update its "Content" field
						Flags . DbSelectorOpen . ListBoxItemText = data;
						lbi . Content = data;
						Flags . DbSelectorOpen . ViewersList . Refresh ( );
						//						IsAdded = true;
						break;
					}
				}
				//if (!IsAdded)
				//{
				//	Flags.SqlViewerIsLoading = false;
				//	// Send Notification to DbSelector to Add/Update the ViewersList
				//	// clear flag cos we are calling iteratively via DbSelector 101 notification call below;
				//	SendDbSelectorCommand (101, data, Flags.CurrentSqlViewer);
				//}
			}
			else
			{
#pragma NOT NEEDED ???
				// Send Notification to DbSelector to Add/Update the ViewersList
				// clear flag cos we are calling iteratively via DbSelector 101 notification call below;
				// add a new entry here !!!  101 does nothing but report it is received
				//				SendDbSelectorCommand (101, data, this);
			}
		}

		//*********************************************************************************************************//
		private void Edit_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			// Open Edit Window for the current record in SqlDbViewer DataGrid
			if ( CurrentDb == "BANKACCOUNT" )
			{
				EditDb edb = new EditDb ( "BANKACCOUNT", this . BankGrid . SelectedIndex, this . BankGrid . SelectedItem, this );
				BankAccountViewModel . EditdbWndBank = edb;
				edb . Owner = this;
				//				if (BankGrid.SelectedIndex >= 0)
				//				{
				//					edb.DataContext = BankGrid.SelectedItem;
				edb . Show ( );
				ExtensionMethods . Refresh ( edb );
				EditDataGrid = edb . DataGrid1;
				//				}
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				EditDb edb = new EditDb ( "CUSTOMER", this . CustomerGrid . SelectedIndex, this . CustomerGrid . SelectedItem, this );
				BankAccountViewModel . EditdbWndBank = edb;
				edb . Owner = this;
				//				if (CustomerGrid.SelectedIndex >= 0)
				//				{
				//edb.DataContext = CustomerGrid.SelectedItem;
				edb . Show ( );
				ExtensionMethods . Refresh ( edb );
				EditDataGrid = edb . DataGrid2;
				//				}
			}
			else if ( CurrentDb == "DETAILS" )
			{
				EditDb edb = new EditDb ( "DETAILS", this . DetailsGrid . SelectedIndex, this . DetailsGrid . SelectedItem, this );
				BankAccountViewModel . EditdbWndBank = edb;
				edb . Owner = this;
				//				if (DetailsGrid.SelectedIndex >= 0)
				//				{
				//					edb.DataContext = DetailsGrid.SelectedItem;
				edb . Show ( );
				ExtensionMethods . Refresh ( edb );
				EditDataGrid = edb . DataGrid1;
				//				}
			}
		}

		//*********************************************************************************************************//
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

		private void TextBlock_RequestBringIntoView ( object sender, RequestBringIntoViewEventArgs e )
		{
			//			this.Show ();
			//			this.BringIntoView ();
		}
		//*******************************************************************************************************//
		private void ItemsView_OnSelectionChanged ( object sender, SelectionChangedEventArgs e )
		//User has clicked a row in our DataGrid OR in EditDb grid
		{
			//declare vars to hold item data from relevant Classes
			var datagrid = sender as DataGrid;
			//			DataGridBase dgb = new DataGridBase( );
			//			dgb . OnSelectionChanged ( e );
			if ( datagrid == null ) return;

			//Get the NEW selected index
			int index = ( int ) datagrid . SelectedIndex;
			if ( index == -1 ) return;
			Flags . isEditDbCaller = false;

			if ( BankAccountViewModel . EditdbWndBank != null )
			{
				//There is an EditDb window open, so this will trigger 
				//an event that lets the DataGrid in the EditDb class
				// change it's own index internally
				if ( CurrentDb == "BANKACCOUNT" )
				{
					Flags . SqlBankCurrentIndex = BankGrid . SelectedIndex;
					//					Console . WriteLine ( $"BankAccount Index now {BankGrid . SelectedIndex}" );
					if ( EventHandler != null )
					{
						//Set global  of the ID we want to find
						BankAccountViewModel . CurrentSelectedIndex = index;
						// Setting this to 1 will make EditDb update its position?
						//						MainWindow . DgControl . SelectionChangeInitiator = 1;
						//						MainWindow . gv . SqlCurrentEditViewer  . DataGrid1 . SelectedIndex = index;
						//EventHandler . SqlDbTriggerEvent ( Flags . isEditDbCaller, index, BankGrid, BankAccountViewModel . SqlUpdating );
						BankGrid . SelectedIndex = index;
						BankGrid . SelectedItem = index;
					}
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					Flags . SqlCustCurrentIndex = CustomerGrid . SelectedIndex;
					//					Console . WriteLine ( $"Customer Index now {CustomerGrid . SelectedIndex}" );
					if ( EventHandler != null )
					{
						//Set global  of the ID we want to find
						BankAccountViewModel . CurrentSelectedIndex = index;
						//						EventHandler . SqlDbTriggerEvent ( Flags . isEditDbCaller, index, CustomerGrid, CustomerViewModel . SqlUpdating );
						CustomerGrid . SelectedIndex = index;
						CustomerGrid . SelectedItem = index;
					}
				}
				else if ( CurrentDb == "DETAILS" )
				{
					Flags . SqlDetCurrentIndex = DetailsGrid . SelectedIndex;
					//					Console . WriteLine ( $"Details Index now {DetailsGrid . SelectedIndex}" );
					if ( EventHandler != null )
					{
						//Set global  of the ID we want to find
						BankAccountViewModel . CurrentSelectedIndex = index;
						//						EventHandler . SqlDbTriggerEvent ( Flags . isEditDbCaller, index, DetailsGrid, BankAccountViewModel . SqlUpdating );
						DetailsGrid . SelectedIndex = index;
						DetailsGrid . SelectedItem = index;
					}
				}
				// Notify EditDb viewer of selection changed
				SendViewerIndexChange ( index, CurrentDb );
				SqlUpdating = false;
			}
			else
			{
				//called when EditDb is NOT OPEN
				CustomerViewModel custacct = null;
				BankAccountViewModel bankacct = null;
				DetailsViewModel detsacct = null;
				int CurrentId = 0;
				{
					if ( datagrid . Name == "CustomerGrid" )
					{
						Flags . SqlCustCurrentIndex = CustomerGrid . SelectedIndex;
						//						Console . WriteLine ( $"Customer Index now {CustomerGrid . SelectedIndex}" );
						if ( CustomerGrid . SelectedItem != null )
						{
							custacct = CustomerGrid?.SelectedItem as CustomerViewModel;
							if ( custacct != null )
							{
								CurrentId = custacct . Id;
								//								Flags . bvmCustRecord = custacct;
							}
						}
					}
					else if ( datagrid . Name == "BankAccountGrid" || datagrid . Name == "BankGrid" )
					{
						Flags . SqlBankCurrentIndex = BankGrid . SelectedIndex;
						//						Console . WriteLine ( $"BankAccount Index now {BankGrid . SelectedIndex}" );
						if ( BankGrid . SelectedItem != null )
						{
							//Get copy of entire BankAccvount record
							bankacct = BankGrid?.SelectedItem as BankAccountViewModel;
							if ( bankacct != null )
							{
								CurrentId = bankacct . Id;
								//								Flags . bvmBankRecord = bankacct;
							}
						}
					}
					else if ( datagrid . Name == "DetailsGrid" )
					{
						Flags . SqlDetCurrentIndex = DetailsGrid . SelectedIndex;
						//						Console . WriteLine ( $"Details Index now {DetailsGrid . SelectedIndex}" );
						{
							if ( DetailsGrid?.SelectedItem != null )
							{
								detsacct = DetailsGrid?.SelectedItem as DetailsViewModel;
								if ( detsacct != null )
								{
									CurrentId = detsacct . Id;
									//									Flags . bvmDetRecord = detsacct;
								}
							}
						}
					}
				}
				//Update the Loading window content for "Viewers Open"
				if ( Flags . EventHandlerDebug )
				{
					Notifier ( 111, "Selected Index has changed", null );
				}
				if ( CurrentDb == "BANKACCOUNT" )
				{
					if ( this . BankGrid . SelectedItem != null )
					{
						CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
						// Fills/Updates the MainWindow.gv[] array
						Flags . SqlViewerIsUpdating = true;
						// Updates  the MainWindow.gv[] structure
						UpdateRowDetails ( this . BankGrid . SelectedItem, "BankGrid" );
						BankGrid . SelectedIndex = index;
						BankGrid . SelectedItem = index;
						this . Activate ( );
					}
					//we now have FULL PrettyDetails
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( this . CustomerGrid . SelectedItem != null )
					{
						CurrentCustomerSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
						Flags . SqlViewerIsUpdating = true;
						// Updates  the MainWindow.gv[] structure
						UpdateRowDetails ( this . CustomerGrid . SelectedItem, "CustomerGrid" );
						CustomerGrid . SelectedIndex = index;
						CustomerGrid . SelectedItem = index;
						this . Activate ( );
					}
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( this . DetailsGrid . SelectedItem != null )
					{
						CurrentDetailsSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
						// This creates a new entry in gv[] if this is a new window being loaded
						Flags . SqlViewerIsUpdating = true;
						// Updates  the MainWindow.gv[] structure
						UpdateRowDetails ( this . DetailsGrid . SelectedItem, "DetailsGrid" );
						//						SelectCurrentRowByIndex ( DetailsGrid, DetailsGrid . SelectedIndex );
						DetailsGrid . SelectedIndex = index;
						DetailsGrid . SelectedItem = index;
						this . Activate ( );
					}
				}
			}
			UpdateAuxilliaries ( $"Selection changed to row {index}" );
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		public static void SelectCurrentRowByIndex ( DataGrid dataGrid, int rowIndex )
		{
			DataGridRow row = dataGrid . ItemContainerGenerator . ContainerFromIndex ( rowIndex ) as DataGridRow;
			if ( row != null )
			{

				Console . WriteLine ( $"row.Focus failed" );
				int y = 0;
				//DataGridCell cell = GetCell ( dataGrid, row, 0 );
				//if ( cell != null )
				//	cell . Focus ( );
			}
		}

		//*******************************************************************************************************//
		public void UpdateAuxilliaries ( string comment )
		{
			//Application.Current.Dispatcher.Invoke (() =>
			ParseButtonText ( true );
			//			IsFiltered = "";
			ResetOptionButtons ( 0 );
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if ( IsViewerLoaded == false )
				UpdateGridviewController ( CurrentDb );
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ( );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( bvm . BankAccountObs == null ) return;
				if ( this . BankGrid . ItemsSource == null )
					this . BankGrid . ItemsSource = bvm . BankAccountObs;
				//  paint the grid onscreen
				this . BankGrid . SelectionUnit = DataGridSelectionUnit . FullRow;
				//dgControl.SelectedIndex = BankGrid.SelectedIndex;
				//dgControl.SelectedItem = BankGrid.SelectedItem;
				//dgControl.SelectedGrid = BankGrid;
				//				CollectionViewSource . GetDefaultView ( this . BankGrid . ItemsSource ) . Refresh ( );
			}
			if ( CurrentDb == "CUSTOMER" )
			{


				if ( cvm . CustomersObs == null ) return;
				this . CustomerGrid . ItemsSource = cvm . CustomersObs;
				//  paint the grid onscreen
				this . CustomerGrid . SelectionUnit = DataGridSelectionUnit . FullRow;
				//dgControl.SelectedIndex = CustomerGrid.SelectedIndex;
				//dgControl.SelectedItem = CustomerGrid.SelectedItem;
				//dgControl.SelectedGrid = CustomerGrid;
				//				CollectionViewSource . GetDefaultView ( this . CustomerGrid . ItemsSource ) . Refresh ( );
				//				CustomerGrid.ScrollIntoView (dgControl.SelectedIndex);
			}
			if ( CurrentDb == "DETAILS" )
			{
				if ( dvm . DetailsObs == null ) return;
				this . DetailsGrid . ItemsSource = dvm . DetailsObs;
				//  paint the grid onscreen
				this . DetailsGrid . SelectionUnit = DataGridSelectionUnit . FullRow;
				//dgControl.SelectedIndex = DetailsGrid.SelectedIndex;
				//dgControl.SelectedItem = DetailsGrid.SelectedItem;
				//dgControl.SelectedGrid = DetailsGrid;
				//				CollectionViewSource . GetDefaultView ( this . DetailsGrid . ItemsSource ) . Refresh ( );
			}
#pragma NOT NEEDED ????
			//			UpdateViewersList ();
			StatusBar . Text = comment;
			WaitMessage . Visibility = Visibility . Collapsed;
		}

		/// <summary>
		///  A CallBack that recieves notifications from EditDb viewer that a selection has changed
		///  so that we can update our row position to match
		/// </summary>
		/// <param name="row"></param>
		/// <param name="CurrentDb"></param>
		public void OnEditDbViewerIndexChanged ( int row, string CurrentDb )
		{
			if ( Flags . CurrentEditDbViewer == null ) return;
			if ( row == CustomerGrid . Items . Count )
				row -= 1;

			if ( CurrentDb == "BANKACCOUNT" )
			{
				RemoteIndexChange = true;
				this . BankGrid . SelectedIndex = row;
				//Needs -1 cos if the last record is selected it is out of bounds due to  zero start count
				if ( row == BankGrid . Items . Count )
					row -= 1;
				try
				{
					this . BankGrid . SelectedItem = bvm . BankAccountObs . ElementAt ( row );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedItem );
				ExtensionMethods . Refresh ( BankGrid );
				RemoteIndexChange = false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				RemoteIndexChange = true;
				this . CustomerGrid . SelectedIndex = row;
				if ( row == CustomerGrid . Items . Count )
					row -= 1;
				try
				{
					this . CustomerGrid . SelectedItem = cvm . CustomersObs . ElementAt ( row );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . SelectedItem );
				this . CustomerGrid . Refresh ( );
				RemoteIndexChange = false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				RemoteIndexChange = true;
				this . DetailsGrid . SelectedIndex = row;
				try
				{
					DetailsGrid . SelectedItem = dvm . DetailsObs . ElementAt ( row );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedItem );
				this . DetailsGrid . Refresh ( );
				RemoteIndexChange = false;
			}

		}

		//*******************************************************************************************************//

		#region CheckBoxhandlers
		private void Id_option_Click ( object sender, RoutedEventArgs e )
		{
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . ID;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Id sort order....";
		}
		private void Bankno_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . BANKNO;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Bank A/c # sort order....";
		}
		private void Custno_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . CUSTNO;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Customer # sort order....";
		}
		private void actype_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . ACTYPE;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in A/c Type sort order....";
		}
		private void Dob_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . DOB;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Birthday sort order....";
		}
		private void Odate_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . ODATE;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Date Opened sort order....";
		}
		private void Cdate_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . CDATE;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Date Closed sort order....";
		}

		private void Default_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . DEFAULT;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Bank A/c #  within Customer # sort order....";
		}
		#endregion CheckBoxhandlers
		//*********************************************************************************************************//

		#region GetSqlInstance - Fn to allow me to call standard merthods from inside a Static method

		//*****************************************************//
		//this is really clever stuff
		// It lets me call standard methods (private, public, protected etc)
		//from INSIDE a Static method
		// using syntax : GetSqlInstance().MethodToCall();
		//and it works really great
		private static SqlDbViewer _Instance;
		public static SqlDbViewer GetSqlInstance ( )
		{
			if ( _Instance == null )
				_Instance = new SqlDbViewer ( );
			return _Instance;
		}
		//*****************************************************//
		#endregion GetSqlInstance
		//*********************************************************************************************************//
		#region Filtering code
		//*****************************************************************************************//
		private void SetFilter_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can 
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			// Call up the Filtering Window to select 
			// the filtering conditions required
			Window_GotFocus ( sender, null );
			//			Window_MouseDown ( sender, null );
			bool isRecovering = false;
			if ( CurrentDb == "" )
			{
				MessageBox . Show ( "You need to have loaded one of the data tables\r\nbefore you can access the filtering system" );
				return;
			}
			if ( Filters . Content == "Reset" )
			{
				Filters . Content = "Filter";
				if ( CurrentDb == "BANKACCOUNT" )
					ShowBank_Click ( null, null );
				else if ( CurrentDb == "CUSTOMER" )
					ShowCust_Click ( null, null );
				else if ( CurrentDb == "DETAILS" )
					ShowDetails_Click ( null, null );
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				Filters . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				Filters . Background = br;
				Filters . Content = "Filtering";
				Mouse . OverrideCursor = Cursors . Arrow;

				return;

			}
			SQLFilter sf = new SQLFilter ( this );
			//// filter any table
			if ( CurrentDb == "BANKACCOUNT" )
			{
				sf . FilterList . Items . Clear ( );
				sf . FilterList . Items . Add ( "ID" );
				sf . FilterList . Items . Add ( "BANKNO" );
				sf . FilterList . Items . Add ( "CUSTNO" );
				sf . FilterList . Items . Add ( "ACTYPE" );
				sf . FilterList . Items . Add ( "BALANCE" );
				sf . FilterList . Items . Add ( "INTRATE" );
				sf . FilterList . Items . Add ( "ODATE" );
				sf . FilterList . Items . Add ( "CDATE" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				sf . FilterList . Items . Clear ( );
				sf . FilterList . Items . Add ( "Id" );
				sf . FilterList . Items . Add ( "ID" );
				sf . FilterList . Items . Add ( "BANKNO" );
				sf . FilterList . Items . Add ( "CUSTNO" );
				sf . FilterList . Items . Add ( "ACTYPE" );
				sf . FilterList . Items . Add ( "FNAME" );
				sf . FilterList . Items . Add ( "LNAME" );
				sf . FilterList . Items . Add ( "ADDR1" );
				sf . FilterList . Items . Add ( "ADDR2" );
				sf . FilterList . Items . Add ( "TOWN" );
				sf . FilterList . Items . Add ( "COUNTY" );
				sf . FilterList . Items . Add ( "PCODE" );
				sf . FilterList . Items . Add ( "PHONE" );
				sf . FilterList . Items . Add ( "MOBILE" );
				sf . FilterList . Items . Add ( "DOB" );
				sf . FilterList . Items . Add ( "ODATE" );
				sf . FilterList . Items . Add ( "CDATE" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				sf . FilterList . Items . Clear ( );
				sf . FilterList . Items . Add ( "ID" );
				sf . FilterList . Items . Add ( "BANKNO" );
				sf . FilterList . Items . Add ( "CUSTNO" );
				sf . FilterList . Items . Add ( "ACTYPE" );
				sf . FilterList . Items . Add ( "BALANCE" );
				sf . FilterList . Items . Add ( "INTRATE" );
				sf . FilterList . Items . Add ( "ODATE" );
				sf . FilterList . Items . Add ( "CDATE" );
			}
			sf . Operand . Items . Add ( "Equal to" );
			sf . Operand . Items . Add ( "Not Equal to" );
			sf . Operand . Items . Add ( "Greater than or Equal to" );
			sf . Operand . Items . Add ( "Less than or Equal to" );
			sf . Operand . Items . Add ( ">= value1 AND <= value2" );
			sf . Operand . Items . Add ( "> value1 AND < value2" );
			sf . Operand . Items . Add ( "< value1 OR > value2" );
			sf . Operand . SelectedIndex = 0;
			//			}
			sf . currentDb = CurrentDb;
			sf . FilterResult = false;
			sf . ShowDialog ( );
			if ( sf . FilterResult )
			{
				columnToFilterOn = sf . ColumnToFilterOn;
				filtervalue1 = sf . FilterValue . Text;
				filtervalue2 = sf . FilterValue2 . Text;
				operand = sf . operand;
				DoFilter ( sender, null );
				StatusBar . Text = $"Filtered Results are shown above. Column = {columnToFilterOn}, Condition = {operand}, Value(s) = {filtervalue1}, {filtervalue2} ";
				Filters . IsEnabled = false;
				Filters . Content = "Reset";
				Filters . IsEnabled = true;
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				Filters . Template = ctmp;
				Brush brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				Filters . Background = brs;
			}
			else
			{
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				Filters . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				Filters . Background = br;
				Filters . Content = "Filtering";
			}
		}
		//*****************************************************************************************//
		private void DoFilter ( object sender, System . Windows . Input . MouseButtonEventArgs e )
		{
			// carry out the filtering operation
			string Commandline1 = "";
			string Commandline = "";
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Commandline1 = $"Select * from BankAccount where ";
				BankAccountViewModel . dtBank . Clear ( );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Commandline1 = $"Select * from Customer where ";
				CustomerViewModel . dtCust . Clear ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Commandline1 = $"Select * from SecAccounts where ";
				DetailsViewModel . dtDetails . Clear ( );
			}

			if ( operand . Contains ( "Not Equal" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} <> '{filtervalue1}'";
			else if ( operand . Contains ( "Equal to" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} = '{filtervalue1}'";
			else if ( operand . Contains ( "Greater than" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} >= '{filtervalue1}'";
			else if ( operand . Contains ( "less than" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} <= '{filtervalue1}'";
			else
			{
				if ( filtervalue1 == "" || filtervalue2 == "" )
				{
					System . Windows . MessageBox . Show ( "The Filter you have selected needs TWO seperate Values.\r\nUnable to continue with Filtering process..." );
					return;
				}
				else if ( operand . Contains ( ">= value1 AND <= value2" ) )
					Commandline = Commandline1 + $" {columnToFilterOn} >= {filtervalue1} AND {columnToFilterOn} <= '{filtervalue2}'";
				else if ( operand . Contains ( "> value1 AND < value2" ) )
					Commandline = Commandline1 + $" {columnToFilterOn} > {filtervalue1} AND {columnToFilterOn} < '{filtervalue2}'";
				else if ( operand . Contains ( "< value1 OR > value2" ) )
					Commandline = Commandline1 + $" {columnToFilterOn} < {filtervalue1} OR {columnToFilterOn} > '{filtervalue2}'";
			}

			Commandline += " Order  by ";
			Commandline = Utils . GetDataSortOrder ( Commandline );
			Flags . FilterCommand = Commandline;
			//	set file wide filter command line
			FilterCommand = Commandline;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				IsFiltered = "BANKACCOUNT";
				ShowBank_Click ( null, null );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				IsFiltered = "CUSTOMER";
				ShowCust_Click ( null, null );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				IsFiltered = "DETAILS";
				ShowDetails_Click ( null, null );
			}

			UpdateAuxilliaries ( "" );
			Mouse . OverrideCursor = Cursors . Arrow;

		}
		//*****************************************************************************************//
		/// <summary>
		/// Just sets up the Filter/Duplicate buttons status
		/// </summary>
		private void ResetOptionButtons ( int mode )
		{
			if ( mode == 0 )
			{
				if ( !Filters . IsEnabled )
				{
					Filters . Content = " Clear Filter";
					//					Filters . IsEnabled = true;
				}
				else
				{
					//					Filters . IsEnabled = false;
					Filters . Content = "Filtering";
				}
			}
			else
			{
				if ( Flags . IsMultiMode )
				{
					Multiaccounts . Content = "Show All";
				}
				else
				{
					Multiaccounts . Content = " Multiple A'C's Only";
				}
			}
		}
		private void ResetauxilliaryButtons ( )
		{
			ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
			Filters . Template = tmp;
			Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			Filters . Background = br;
			Filters . Content = "Filtering";
		}

		public void SetButtonGradientBackground ( Button btn )
		{
			// how to change button background to a style in Code
			if ( btn == Filters )
			{
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				btn . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				btn . Background = br;
				btn . Content = "Filtering";
			}
			if ( btn == Multiaccounts )
			{
				if ( Flags . IsMultiMode )
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateRed" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushRed" );
					btn . Background = br;
					btn . Content = "Clear Filter";
				}
				else
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
					btn . Background = br;
					btn . Content = "Filtering";

				}
			}

		}
		#endregion Filtering code

		#region UPDATE NOTIFICATION HANDLERS

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		public async void RefreshBankOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{

			// On arrival, we appear to be pointing at ourselves + the BANK grid
			// This proves that this.Tag is the BANK viewers tag - (Thats the one we want to update ?
			//but Flags.CurrentSqlViewer is DETAILS viewers tag
			Console . WriteLine ( $"In Bank update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );

			// Get a pointer to THIS viewer we can work with hereafter
			SqlDbViewer ViewerPtr = this;
			Console . WriteLine ( $"In Bank update:\nNew Viewer Tag = {ViewerPtr . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			Console . WriteLine ( $"We are now pointing at BANK viewer with index {ViewerPtr?.BankGrid?.SelectedIndex}" );
			Console . WriteLine ( $"while SqlUpdateOrinator is pointing to orginator - {Flags . SqlUpdateOriginatorViewer?.Tag}, \nindex = {Flags . SqlUpdateOriginatorViewer . DetailsGrid?.SelectedIndex}\n" );
			DataGrid BnkGrid = ViewerPtr . BankGrid;
			//Save the current index in this viewer so wecan reset afterwards
			int currindx = BnkGrid . SelectedIndex;
			BnkGrid . ItemsSource = null;
			BnkGrid . Items . Clear ( );

			Debug . WriteLine ( $"Calling Task to reload Bank data...." );
			await bvm . LoadBankTaskInSortOrder ( true, 0 );
			//List<Task<bool>> tasks = new List<Task<bool>> ( );
			//tasks . Add ( bvm . LoadBankTaskInSortOrder ( true, 0 ) );
			//var Results = await Task . WhenAll ( tasks );
			Debug . WriteLine ( $"returned from Task loading Bank data ...." );
			BnkGrid . ItemsSource = bvm . BankAccountObs;
			BnkGrid . SelectedIndex = currindx;
			ExtensionMethods . Refresh ( BnkGrid );
			//Return focus to calling viewer
			Flags . CurrentSqlViewer . Focus ( );
		}
		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		public async void RefreshCustomerOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			Console . WriteLine ( $"In Customer update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			// Get a pointer to THIS viewer we can work with hereafter
			SqlDbViewer ViewerPtr = this;
			Console . WriteLine ( $"In Customer update:\nNew Viewer Tag = {ViewerPtr . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			Console . WriteLine ( $"We are now pointing at Customer viewer with index {ViewerPtr?.BankGrid?.SelectedIndex}" );
			Console . WriteLine ( $"while SqlUpdateOrinator is pointing to orginator - {Flags . SqlUpdateOriginatorViewer?.Tag}, \nindex = {Flags . SqlUpdateOriginatorViewer . DetailsGrid?.SelectedIndex}\n" );
			DataGrid CustGrid = ViewerPtr . CustomerGrid;
			int currindx = CustGrid . SelectedIndex;
			CustGrid . ItemsSource = null;
			CustGrid . Items . Clear ( );

			Debug . WriteLine ( $"Calling Task to reload Customer data...." );
			await cvm . LoadCustomerTaskInSortOrder ( true, 0 );
			//List<Task<bool>> tasks = new List<Task<bool>> ( );
			//tasks . Add ( cvm . LoadCustomerTaskInSortOrder ( true, 0 ) );
			//var Results = await Task . WhenAll ( tasks );
			Debug . WriteLine ( $"returned from Task loading customer data ...." );
			CustGrid . ItemsSource = cvm . CustomersObs;
			CustGrid . SelectedIndex = currindx;
			ExtensionMethods . Refresh ( CustGrid );
			Flags . CurrentSqlViewer . Focus ( );
		}

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		public async void RefreshDetailsOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			Console . WriteLine ( $"In Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			// Get a pointer to THIS viewer we can work with hereafter
			SqlDbViewer ViewerPtr = this;
			Console . WriteLine ( $"In Details  update:\nNew Viewer Tag = {ViewerPtr . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			Console . WriteLine ( $"We are now pointing at Details viewer with index {ViewerPtr?.BankGrid?.SelectedIndex}" );
			Console . WriteLine ( $"while SqlUpdateOrinator is pointing to orginator - {Flags . SqlUpdateOriginatorViewer?.Tag}, \nindex = {Flags . SqlUpdateOriginatorViewer . DetailsGrid?.SelectedIndex}\n" );
			DataGrid DetGrid = ViewerPtr . DetailsGrid;
			int currindx = DetGrid . SelectedIndex;
			DetGrid . ItemsSource = null;
			DetGrid . Items . Clear ( );

			Debug . WriteLine ( $"Calling Task to reload Details data...." );
			await dvm . LoadDetailsTaskInSortOrder ( true, 0 );
			//List<Task<bool>> tasks = new List<Task<bool>> ( );
			//tasks . Add ( dvm . LoadDetailsTaskInSortOrder ( true, 0 ) );
			//var Results = await Task . WhenAll ( tasks );
			Debug . WriteLine ( $"returned from Task loading Details data ...." );
			DetGrid . ItemsSource = dvm . DetailsObs;
			DetGrid . SelectedIndex = currindx;
			ExtensionMethods . Refresh ( DetGrid );
			Flags . SqlUpdateOriginatorViewer . Focus ( );
		}

		#endregion UPDATE NOTIFICATION HANDLERS
		//*********************************************************************************************************//

		#region NotifyPropertyChanged
		//*****************************************************************************************//
		public class User : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public void NotifyPropertyChanged ( string propName )
			{
				if ( this . PropertyChanged != null )
					PropertyChanged ( this, new PropertyChangedEventArgs ( propName ) );
			}
		}
		//*****************************************************************************************//
		private void OnPropertyChanged ( string PropertyName = null )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( PropertyName ) );
		}


		#endregion NotifyPropertyChanged
		//*********************************************************************************************************//
	}

}

