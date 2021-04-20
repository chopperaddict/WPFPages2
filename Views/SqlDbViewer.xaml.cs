#define SHOWSQLERRORMESSAGEBOX
#define SHOWWINDOWDATA
#define ALLOWREFRESH
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using WPFPages.Libraries;
using WPFPages.ViewModels;
using WPFPages.Views;

using static WPFPages.Views.DbSelector;

namespace WPFPages
{

	public class DataChangeArgs: EventArgs
	{
		public string SenderName { get; set; }
		public string DbName { get; set; }
	}

	public delegate void DbUpdated (object sender, DataChangeArgs args);

	public partial class SqlDbViewer: Window, INotifyPropertyChanged
	{

		public DataChangeArgs dca = new DataChangeArgs ();

		#region Global ViewModel declarations
		// SQL Data Setup
		public BankAccountViewModel bvm = MainWindow.bvm;
		public CustomerViewModel cvm = MainWindow.cvm;
		public DetailsViewModel dvm = MainWindow.dvm;
		#endregion Global ViewModel declarations

		public event DbUpdated NotifyOfDataChange;


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
		public bool isMultiMode = false;
		private BankAccountViewModel BankCurrentRowAccount;
		private CustomerViewModel CustomerCurrentRowAccount;
		private DetailsViewModel DetailsCurrentRowAccount;
		private DataGridRow BankCurrentRow;
		private DataGridRow CustomerCurrentRow;
		private DataGridRow DetailsCurrentRow;
		public Window ThisWindow = new Window ();
		private bool IsViewerLoaded = false;
		private int LoadIndex = -1;
		public static bool SqlUpdating = false;
		private bool IsCellChanged = false;
		public DataGrid EditDataGrid = null;

		//Get "Local" copies of our global DataTables
		public DataTable dtDetails = DetailsViewModel.dtDetails;
		public DataTable dtBank = BankAccountViewModel.dtBank;
		public DataTable dtCust = CustomerViewModel.dtCust;

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
		event SQLViewerGridSelectionChanged SQLVSelChange;

		public EventHandlers EventHandler = null;
		private static bool SelectionhasChanged = false;

		//Variables used when a cell is edited to se if we need to update via SQL
		private object OriginalCellData = null;
		private string OriginalDataType = "";
		private int OrignalCellRow = 0;
		private int OriginalCellColumn = 0;

		public static DataGridController dgControl;

		//		public DbUpdated UpdateRequired;

		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		//Delegate & Event handler for Db Updates
		// I am declaring  the Event in THIS FILE
		// the ViewModels will be the subscribers
		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		///  A(Globally visible) Delegateto hold all the global flags and other stuff that is needed to handle 
		///  Static -> non static  movements with EditDb &b SqlDbViewer in particular
		/// </summary>


		// Handler to send notifications ot the Various ViewModels
		public void DbHasChanged (object sender, DataChangeArgs args)
		{
			Console.WriteLine ($"\n^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\nSqlServerDb has received DbHasChanged notification\nFrom {args.SenderName} due toUpdate of {args.DbName} Db");
		}
		/// <summary>
		///  Function that is called to broadcast a notification to whoever to 
		///  notify that one of the Obs collections has been changed by something
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		private void SendDataChanged (object o, string dbName)
		{
			dca.SenderName = o.ToString ();
			dca.DbName = dbName;
			if (NotifyOfDataChange != null)
			{
				NotifyOfDataChange (o, dca);
			}
		}
		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		/// Used to keep track of currently selected row in GridViwer
		/// </summary>
		private int _selectedRow;
		public int SelectedRow
		{
			get { return _selectedRow; }
			set { _selectedRow = value; OnPropertyChanged (SelectedRow.ToString ()); }
		}
		public static int SequentialId
		{
			get { return _sequentialId; }
			set { _sequentialId = value; }
		}

		#endregion setup

		#region GetSqlInstance - Fn to allow me to call standard merthods from inside a Static method
		//*****************************************************//
		//this is really clever stuff
		// It lets me call standard methods (private, public, protected etc)
		//from INSIDE a Static method
		// using syntax : GetSqlInstance().MethodToCall();
		//and it works really great
		private static SqlDbViewer _Instance;
		public static SqlDbViewer GetSqlInstance ()
		{
			if (_Instance == null)
				_Instance = new SqlDbViewer ();
			return _Instance;
		}
		//*****************************************************//
		#endregion GetSqlInstance

		#region SqlDbViewer Class Constructors
		//Dummy Constructor for Event handlers
		public SqlDbViewer (char x)
		{
			// dummy constructor to let others get a pointer
			Console.WriteLine ($"Received arg of {x} in SqlDbViewer dummy constructor");
		}
		public SqlDbViewer ()
		{
			InitializeComponent ();
			//if (bvm == null) bvm = new BankAccountViewModel ();
			//if (cvm == null) cvm = new CustomerViewModel ();
			//if (dvm == null) dvm = new DetailsViewModel ();

			sqldbForm = this;
			dgControl = new DataGridController ();
			//Setup our delegate receive function to get messages form DbSelector
			Console.WriteLine ($"\r\n%%%%%%%% In SqlDbViewe() Constructor %%%%%%%%%%%\r\n");

			if (Flags.CurrentSqlViewer != this)
				Flags.CurrentSqlViewer = this;

			NotifyViewer SendCommand = new NotifyViewer (MyNotification);
			SendDbSelectorCommand = MyNotification;
			//This DOES call handler in DbSelector !!
			SendDbSelectorCommand (1, "Basic Constructor completed\n=======================", this);
			Utils.GetWindowHandles ();
			////subscribing viewmodels to data changed event !!!
			NotifyOfDataChange += bvm.DbHasChanged;
			NotifyOfDataChange += cvm.DbHasChanged;
			NotifyOfDataChange += dvm.DbHasChanged;

		}

		//******************************************//
		public SqlDbViewer (int selection)
		{
			//if (bvm == null) bvm = new BankAccountViewModel ();
			//if (cvm == null) cvm = new CustomerViewModel ();
			//if (dvm == null) dvm = new DetailsViewModel ();
			int selectedDb = -1;
			IsViewerLoaded = false;
			//			InitializeComponent ();
			sqldbForm = this;
			dgControl = new DataGridController ();

			if (Flags.CurrentSqlViewer != this)
				Flags.CurrentSqlViewer = this;
			Console.WriteLine ($"\r\n%%%%%%%% In SqlDbViewer(int) Constructor %%%%%%%%%%%\r\n");
			//Setup our delegate receive function to get messages form DbSelector
			NotifyViewer SendCommand = new NotifyViewer (MyNotification);
			SendDbSelectorCommand = MyNotification;
			//This DOES call handler in DbSelector !!
			SendDbSelectorCommand (102, ">>> SqlDbviewer (Full) Constructor loading ....", Flags.CurrentSqlViewer);
			selectedDb = selection;
			{
				switch (selectedDb)
				{
					case 0:
						CurrentDb = "BANKACCOUNT";
						new EventHandlers (BankGrid, "BANKACOUNT", out EventHandler);
						//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
						//						Flags.CurrentSqlViewerBankGrid = BankGrid;
						//						Flags.ActiveSqlDbViewer = BankGrid;
						//						LoadBankData ();
						break;
					case 1:
						CurrentDb = "CUSTOMER";
						new EventHandlers (CustomerGrid, "CUSTOMER", out EventHandler);
						//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
						//						Flags.CurrentSqlViewerCustomerGrid = CustomerGrid;
						//						Flags.ActiveSqlDbViewer = CustomerGrid;
						//LoadCustData ();
						break;
					case 2:
						CurrentDb = "DETAILS";
						new EventHandlers (DetailsGrid, "DETAILS", out EventHandler);
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
			EditArgs = new EditEventArgs ();
			this.MouseDown += delegate { DoDragMove (); };
			SendDbSelectorCommand (103, "<<< SqlDbViewer Constructor  completed....", Flags.CurrentSqlViewer);

			////subscribing viewmodels to data changed event !!!
			NotifyOfDataChange += bvm.DbHasChanged;
			NotifyOfDataChange += cvm.DbHasChanged;
			NotifyOfDataChange += dvm.DbHasChanged;
		}
		//******************************************//
		/// <summary>
		/// MAIN STARTUP CALL from DbSelector
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		public SqlDbViewer (string caller)
		{
			int selectedDb = -1;
			IsViewerLoaded = false;
			InitializeComponent ();
			sqldbForm = this;
			dgControl = new DataGridController ();

#if USEDETAILEDEXCEPTIONHANDLER
			Console.WriteLine ($"\r\n%%%%%%%% In SqlDbViewer(string) Constructor %%%%%%%%%%%\r\n");
#endif
			//			if (Flags.CurrentSqlViewer != this)
			//			{
			Flags.CurrentSqlViewer = this;
			//			}

			//Setup our delegate receive function to get messages form DbSelector
			NotifyViewer SendCommand = new NotifyViewer (MyNotification);
			SendDbSelectorCommand = MyNotification;
			//This DOES call handler in DbSelector !!
			SendDbSelectorCommand (102, ">>> SqlDbviewer (Full) Constructor loading ....", Flags.CurrentSqlViewer);

			switch (caller)
			{
				case "BANKACCOUNT":
					CurrentDb = "BANKACCOUNT";
					new EventHandlers (BankGrid, "BANKACOUNT", out EventHandler);
					Flags.ActiveSqlDbViewer = BankGrid;
					break;
				case "CUSTOMER":
					CurrentDb = "CUSTOMER";
					new EventHandlers (CustomerGrid, "CUSTOMER", out EventHandler);
					Flags.ActiveSqlDbViewer = CustomerGrid;
					break;
				case "DETAILS":
					CurrentDb = "DETAILS";
					new EventHandlers (DetailsGrid, "DETAILS", out EventHandler);
					Flags.ActiveSqlDbViewer = DetailsGrid;
					break;
				default:
					break;
			}
			ThisWindow = this;
			EditArgs = new EditEventArgs ();
			this.MouseDown += delegate { DoDragMove (); };
			SendDbSelectorCommand (103, "<<< SqlDbViewer Constructor  completed....", Flags.CurrentSqlViewer);
			//			SendDbSelectorCommand (111, "Secondary Constructor Completed", Flags.CurrentSqlViewer);

			////subscribing viewmodels to data changed event !!!
			NotifyOfDataChange += bvm.DbHasChanged;
			NotifyOfDataChange += cvm.DbHasChanged;
			NotifyOfDataChange += dvm.DbHasChanged;
			this.Show ();
		}
		#endregion Constructors

		#region CallBack/Delegate stuff - SqlViewerNotify (int status, string info, SqlDbViewer NewSqlViewer)

		//*************************************************************************//
		// delegate object for others to access (listen for notifications sent by THIS CLASS)
		public delegate void SqlViewerNotify (int status, string info, SqlDbViewer NewSqlViewer);
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
		public async static void DbSelectorMessage (int status, string info, SqlDbViewer NewSqlViewer)
		{
			switch (status)
			{
				case 100:
					break;
			}
			if (status == 100)
			{
				//instruction received from DbSelector to load relevant data
				Debug.WriteLine ($"\r\nSQLDBV - Command :  [{status}] Received");
				Debug.WriteLine ($"Data received : [{info}]\r\n");

				//Window is now on screen & fully painted
				if (info == "BANKACCOUNT")
				{
					Flags.CurrentSqlViewer.SendDbSelectorCommand (102, ">>> Starting load of Bank Data", Flags.CurrentSqlViewer);

					Flags.CurrentSqlViewer.GetData (info);
					Flags.CurrentSqlViewer.SendDbSelectorCommand (103, $"<<< Bank Data Loaded {Flags.SqlBankGrid.Items.Count} records..", Flags.CurrentSqlViewer);
					Flags.CurrentSqlViewer.SetGridVisibility ("BANKACCOUNT");
					var BnkTuple = CreateTuple (info);
					Flags.CurrentSqlViewer.UpdateViewersList ();
					Flags.SqlBankGrid.SelectedIndex = 0;
				}
				else if (info == "CUSTOMER")
				{
					Flags.CurrentSqlViewer.SendDbSelectorCommand (102, ">>> Starting load of Customer Data", Flags.CurrentSqlViewer);
					Flags.CurrentSqlViewer.GetData (info);
					Flags.CurrentSqlViewer.SendDbSelectorCommand (103, $"<<< Customer Data Loaded {Flags.SqlCustGrid.Items.Count} records...", Flags.CurrentSqlViewer);
					Flags.CurrentSqlViewer.SetGridVisibility ("CUSTOMERS");
					var CustTuple = CreateTuple (info);
					Flags.CurrentSqlViewer.UpdateViewersList ();
					Flags.SqlCustGrid.SelectedIndex = 0;
				}
				else if (info == "DETAILS")
				{
					Flags.CurrentSqlViewer.SendDbSelectorCommand (102, ">>> Starting load of Details Data", Flags.CurrentSqlViewer);
					Flags.CurrentSqlViewer.GetData (info);
					Flags.CurrentSqlViewer.SendDbSelectorCommand (103, $"<<< Details Data Loaded {Flags.SqlDetGrid.Items.Count} records...", Flags.CurrentSqlViewer);
					Flags.CurrentSqlViewer.SetGridVisibility ("DETAILS");
					var DetTuple = CreateTuple (info);
					Flags.CurrentSqlViewer.UpdateViewersList ();
					Flags.SqlDetGrid.SelectedIndex = 0;
				}
				ExtensionMethods.Refresh (Flags.CurrentSqlViewer);
			}
		}

		#endregion CallBack/Delegate stuff

		#region Callback response functions
		/// <summary>
		/// Calls  the relevant SQL data load calls to load data, fill Lists and populate Obs collections
		/// </summary>
		/// <param name="CurrentDb"></param>
		public async void GetData (string CurrentDb)
		{
			if (CurrentDb == "BANKACCOUNT")
			{
				ExtensionMethods.Refresh (this);
				ExtensionMethods.Refresh (BankGrid);
				await bvm.LoadBankTaskInSortOrder (-1);
				this.BankGrid.ItemsSource = bvm.BankAccountObs;
				//				Flags.SqlViewerIsLoading = true;
				this.BankGrid.SelectedIndex = 0;
				//				Flags.SqlViewerIsLoading = false;
				ExtensionMethods.Refresh (BankGrid);
			}
			else if (CurrentDb == "CUSTOMER")
			{
				ExtensionMethods.Refresh (this);
				ExtensionMethods.Refresh (CustomerGrid);
				await cvm.LoadCustomerTaskInSortOrder(3);
				//await cvm.LoadCustomersTask ();
				this.CustomerGrid.ItemsSource = cvm.CustomersObs;
				//				Flags.SqlViewerIsLoading = true;
				this.CustomerGrid.SelectedIndex = 0;
				//				Flags.SqlViewerIsLoading = false;
				ExtensionMethods.Refresh (CustomerGrid);
			}
			else if (CurrentDb == "DETAILS")
			{
				//Loads the data Asynchronously
				ExtensionMethods.Refresh (this);
				ExtensionMethods.Refresh (DetailsGrid);
				await dvm.LoadDetailsTaskInSortOrder (-1);
				this.DetailsGrid.ItemsSource = dvm.DetailsObs;
				//				Flags.SqlViewerIsLoading = true;
				this.DetailsGrid.SelectedIndex = 0;
				//				Flags.SqlViewerIsLoading = false;
				ExtensionMethods.Refresh (DetailsGrid);
			}
		}
		public void SetGridVisibility (string CurrentDb)
		{
			if (CurrentDb == "BANKACCOUNT")
			{
				Utils.GetWindowHandles ();
				this.BankGrid.ItemsSource = bvm.BankAccountObs;
				this.BankGrid.SelectedIndex = 0;
				ExtensionMethods.Refresh (BankGrid);
			}
			if (CurrentDb == "CUSTOMER")
			{

				Utils.GetWindowHandles ();
				this.CustomerGrid.ItemsSource = cvm.CustomersObs;
				this.CustomerGrid.SelectedIndex = 0;
				ExtensionMethods.Refresh (CustomerGrid);
			}
			if (CurrentDb == "DETAILS")
			{
				Utils.GetWindowHandles ();
				this.DetailsGrid.ItemsSource = dvm.DetailsObs;
				this.DetailsGrid.SelectedIndex = 0;
				ExtensionMethods.Refresh (DetailsGrid);
			}

		}
		#endregion Callback response functions


		#region load/startup
		//*****************************************************************************************//
		private void aRecieveSelectorCommand (int status, string info)
		// not used
		{
			//Handle delegate messages from DbSelector
			Debug.WriteLine ($"\r\n ########## SqlDbViewer - Message {status} received from DbSelector");
			Debug.WriteLine ($"{info}\r\n");
		}
		private void OnWindowLoaded (object sender, RoutedEventArgs e)
		{
			// THIS IS WHERE WE NEED TO SET THIS FLAG
			Flags.SqlViewerIsLoading = true;
			this.Show ();
			int currentindex = -1;
			this.MouseDown += delegate { DoDragMove (); };

			this.Show ();
			ExtensionMethods.Refresh (this);
			BringIntoView ();
			//This is the EventHandler declared  in THIS FILE
			LoadedEventArgs ex = new LoadedEventArgs ();

			ex.CallerDb = CurrentDb;
			//Delegate call only
			OnDataLoaded (CurrentDb);

			//Load BankAccount grid automatically on startup
			if (CurrentGridViewerIndex != -1)
			{
				//if (CurrentDb == "BANKACCOUNT")
				//{
				//	if (IsViewerLoaded)
				//	{
				//		ShowBank_Click (null, null);
				//		if (BankGrid.Items.Count > 0)
				//		{
				//			Focus ();
				//			BringIntoView ();
				//		}
				//	}
				//	//Set global flags
				//	Flags.SetGridviewControlFlags (this, BankGrid, CurrentDb);
				//	Console.WriteLine ($"SqlDbViewer(221) Window just loaded : getting instance of EventHandlers class with this,BankGrid, \"SQLDBVIEWERDB\"");
				//	//Setup the Event handler to notify EditDb viewer of index changes
				//}
				//else if (CurrentDb == "CUSTOMER")
				//{
				//	ShowCust_Click (null, null);
				//	if (CustomerGrid.Items.Count > 0)
				//	{
				//		Focus ();
				//		BringIntoView ();
				//	}
				//	//Set global flags
				//	Flags.SetGridviewControlFlags (this, CustomerGrid, CurrentDb);
				//	//??? Sets it to null !!!
				//	CustomerGrid.ItemsSource = CustomerViewModel.CustomersObs;
				//	Console.WriteLine ($"SqlDbViewer(233) Window just loaded : getting instance of EventHandlers class with this,BankGrid, \"SQLDBVIEWERDB\"");
				//	// When we reach here - we have NO DATA LOADED - 1st time arouind at least

				//	//Setup the Event handler to notify EditDb viewer of index changes
				//	//						nt = new EventHandlers (this, CustomerGrid, "SQLDBVIEWER");
				//}
				//else if (CurrentDb == "DETAILS")
				//{
				//	ShowDetails_Click (null, null);
				//	if (DetailsGrid.Items.Count > 0)
				//	{
				//		Focus ();
				//		BringIntoView ();

				//	}
				//	Console.WriteLine ($"SqlDbViewer(208) Window just loaded : getting instance of EventHandlers class with this, DetailsGrid, \"SQLDBVIEWERDB\"");
				//	//Set global flags
				//	Flags.SetGridviewControlFlags (this, DetailsGrid, CurrentDb);
				//	//Setup the Event handler to notify EditDb viewer of index changes
				//	//						nt = new EventHandlers (this, DetailsGrid, "SQLDBVIEWER");
				//}
			}
			//Use Delegate to notify DbSelector
			this.WaitMessage.Visibility = Visibility.Visible;
			this.BankGrid.Visibility = Visibility.Collapsed;
			this.CustomerGrid.Visibility = Visibility.Collapsed;
			this.DetailsGrid.Visibility = Visibility.Collapsed;

			SendDbSelectorCommand (103, $"<<< SqlDbViewer has Finished OnWindowLoading", Flags.CurrentSqlViewer);
			if (CurrentDb == "BANKACCOUNT")
			{
				Flags.ActiveDbGrid = this.BankGrid;
				Flags.SqlBankGrid = this.BankGrid;
				Flags.SetGridviewControlFlags (this, this.BankGrid);
				//				Flags.SqlViewerIsLoading = true;
				this.BankGrid.ItemsSource = bvm.BankAccountObs;
				//				Flags.SqlViewerIsLoading = false;
				this.BankGrid.SelectedIndex = 0;
				this.BankGrid.SelectedItem = 0;
				this.BankGrid.Visibility = Visibility.Visible;
				//if (BankGrid.SelectedIndex == -1)
				//	Debug.WriteLine ($"\n******** ONWINDOWLOADED : Current Grid selection clicked ono is\n[{BankGrid.Name.ToString ()}]\nSelectedIndex IS NOT YET SET.... ");
				//else
				//	Debug.WriteLine ($"\n******** ONWINDOWLOADED : Current Grid selection clicked on is\n[{BankGrid.Name.ToString ()}]\nSelectedIndex = {BankGrid.SelectedIndex}\n{BankGrid.SelectedItem.ToString ()}");
			}
			else if (CurrentDb == "CUSTOMER")
			{
				Flags.ActiveDbGrid = this.CustomerGrid;
				Flags.SqlCustGrid = this.CustomerGrid;
				Flags.ActiveSqlDbViewer = this.CustomerGrid;
				Flags.SetGridviewControlFlags (this, this.CustomerGrid);
				//				Flags.SqlViewerIsLoading = true;
				this.CustomerGrid.ItemsSource = cvm.CustomersObs;
				//				Flags.SqlViewerIsLoading = false;
				this.CustomerGrid.SelectedIndex = 0;
				this.CustomerGrid.Visibility = Visibility.Visible;
				//if (CustomerGrid.SelectedIndex == -1)
				//	Debug.WriteLine ($"\n******** ONWINDOWLOADED : Current Grid selection clicked ono is\n[{CustomerGrid.Name.ToString ()}]\nSelectedIndex IS NOT YET SET.... ");
				//else
				//	Debug.WriteLine ($"\n******** ONWINDOWLOADED : Current Grid selection clicked on is\n[{CustomerGrid.Name.ToString ()}]\nSelectedIndex = {CustomerGrid.SelectedIndex}\n{CustomerGrid.SelectedItem.ToString ()}");
			}
			else if (CurrentDb == "DETAILS")
			{
				Flags.ActiveDbGrid = this.DetailsGrid;
				Flags.SqlDetGrid = this.DetailsGrid;
				Flags.SetGridviewControlFlags (this, this.DetailsGrid);

				// This triggers the selectionchnaged event
				//				Flags.SqlViewerIsLoading = true;
				this.DetailsGrid.ItemsSource = dvm.DetailsObs;
				//				Flags.SqlViewerIsLoading = false;
				this.DetailsGrid.SelectedIndex = 0;
				this.DetailsGrid.Visibility = Visibility.Visible;
				//if (DetailsGrid.SelectedIndex == -1)
				//	Debug.WriteLine ($"\n******** ONWINDOWLOADED : Current Grid selection clicked ono is\n[{DetailsGrid.Name.ToString ()}]\nSelectedIndex IS NOT YET SET.... ");
				//else
				//	Debug.WriteLine ($"\n******** ONWINDOWLOADED : Current Grid selection clicked on is\n[{DetailsGrid.Name.ToString ()}]\nSelectedIndex = {DetailsGrid.SelectedIndex}\n{DetailsGrid.SelectedItem.ToString ()}");
			}
			// Grab a Guid for this viewer window early on
			if (Flags.CurrentSqlViewer == null)
			{
				Flags.CurrentSqlViewer = this;
			}

			//This is the ONE & ONLY place we should set the Guid
			Flags.CurrentSqlViewer.Tag = Guid.NewGuid ();
			MainWindow.gv.SqlViewerGuid = (Guid)Flags.CurrentSqlViewer.Tag;
			IsViewerLoaded = true;
			//Store pointers to our DataGrids in all ModelViews for access by Data row updating code
			EventHandlers.SetWindowHandles (null, this, null);

			// We must now GET THE DATA LOADED
			//Tell DbSelector to command US TO load THE REQUIRED DATA
			// THIS IS THE FIRST CALL - I THINK ??
			SendDbSelectorCommand (25, CurrentDb, this);
			ParseButtonText (true);

			// clear global "loading new window" flag
			Flags.SqlViewerIsLoading = false;
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		#endregion load/startup

		#region load all data base data
		///<summary>
		/// BankAccount database
		/// </summary>
		private bool FillBankAccountDataGrid ()
		{
			SendDbSelectorCommand (102, ">>> SqlDbViewer Entering FillBankAccountDataGrid()", Flags.CurrentSqlViewer);

			string CmdString = "";
			CmdString = "SELECT * FROM BankAccount order by CustNo";
			//clear the datatable first as we are only showing a subset
			BankAccountViewModel.dtBank.Clear ();
			CurrentDb = "BANKACCOUNT";
			bool result = LoadSqlData (CmdString, BankAccountViewModel.dtBank, "BANKACCOUNT", isMultiMode, Filters, StatusBar, Multiaccounts);
			// dtBank is fully loaded here
			isMultiMode = false;
			SendDbSelectorCommand (103, "<<< SqlDbViewer Exiting FillBankAccountDataGrid()", Flags.CurrentSqlViewer);
			return result;
		}

		//*****************************************************************************************//
		public bool LoadSqlData (string CmdString, DataTable dt, string CallerType, bool isMultiMode, Button Btn, TextBlock StatusBar, Button Multiaccounts)
		{
			SqlConnection con;
			string ConString = "";
			this.Show ();

			ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			con = new SqlConnection (ConString);
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (102, $">>> Starting LoadSqlData", Flags.CurrentSqlViewer);

			try
			{
				//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
				using (con)
				{
					SqlCommand cmd = new SqlCommand (CmdString, con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					//load the SQL data into our OC named bankaccounts
					if (CallerType == "BANKACCOUNT")
					{
						//This loads the BankAccount data into DataTable dtBank
						bvm.LoadBankAccountObsCollection ();
						if (bvm.BankAccountObs.Count > 0)
							StatusBar.Text = $"Bank Account loaded successfully ({dt.Rows.Count}) records";
						else
						{
							//reset filter flag
							isMultiMode = false;
							return false;
						}
					}
					else if (CallerType == "CUSTOMER")
					{
						Dispatcher.Invoke (() =>
						{
							LoadCustData ();
						});
						//						LoadCustData ();
						Task<bool> result = cvm.LoadCustomerObsCollection ();
						if (result.IsCompleted)
							StatusBar.Text = $"Customer Account loaded successfully ({dt.Rows.Count}) records";
						else
						{
							//reset filter flag
							isMultiMode = false;
							return false;
						}
					}
					else if (CallerType == "DETAILS")
					{
						// Load data into OBS collection
						if (LoadDetailsObsCollection ())
							StatusBar.Text = $"Details Account loaded successfully ({dt.Rows.Count}) records";
						else
						{
							//reset filter flag
							isMultiMode = false;
							return false;
						}
					}

					//reset filter flag
					if (isMultiMode)
					{
						Multiaccounts.Content = " <<- Show All";
						Btn.IsEnabled = false;
					}
					else
					{
						Multiaccounts.Content = " Multiple A/C's";
						Btn.IsEnabled = true;
					}
					isMultiMode = false;
					//					SendDbSelectorCommand (103, $"<<< LoadSqlData Completed for {CallerType}", Flags.CurrentSqlViewer);
					//					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load Bank Details - {ex.Message}");
#if SHOWSQLERRORMESSAGEBOX
				MessageBox.Show ("SQL error occurred - See Output for details");
#endif

				return false;
			}
			SendDbSelectorCommand (102, $">>> Completed LoadSqlData successfully", Flags.CurrentSqlViewer);
			return true;
		}

		//*****************************************************************************************//

		/// <summary>
		/// DetailsViewModel database
		/// </summary>
		private bool FillDetailsDataGrid ()
		{
			string CmdString = string.Empty;
			string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];

			SqlConnection con;
			if (FilterCommand != "")
			{
				CmdString = FilterCommand;
				FilterCommand = "";
			}
			else
			{
				if (isMultiMode)
				{
					//THIS IS THE SQL COMMAND TO GET FULL LINES OF DUPLICATED CUSTOMER ACCOUNT #'S DATA
					CmdString = "SELECT * FROM Secaccounts WHERE CUSTNO IN "
						+ "(SELECT CUSTNO FROM SECACCOUNTS "
						+ " GROUP BY CUSTNO"
						+ " HAVING COUNT(*) > 1)  order by CustNo";
					//clear the datatable first as we are only showing a subset
					DetailsViewModel.dtDetails.Clear ();
				}
				else
				{
					CmdString = "SELECT * FROM SecAccounts order by CustNo";
					//clear the datatable first as we are only showing a subset
					DetailsViewModel.dtDetails.Clear ();
				}
			}
			bool result = LoadSqlData (CmdString, DetailsViewModel.dtDetails, "DETAILS", isMultiMode, Filters, StatusBar, Multiaccounts);
			isMultiMode = false;
			return true;

		}
		//*****************************************************************************************//
		public bool LoadDetailsObsCollection ()
		{
			//Load the data into our ObservableCollection BankAccounts
			if (dvm.DetailsObs.Count > 0)
			{
				dvm.DetailsObs.Clear ();
			}
			try
			{
				for (int i = 0; i < DetailsViewModel.dtDetails.Rows.Count; ++i)
					dvm.DetailsObs.Add (new DetailsViewModel
					{
						Id = Convert.ToInt32 (dtDetails.Rows[i][0]),
						BankNo = dtDetails.Rows[i][1].ToString (),
						CustNo = dtDetails.Rows[i][2].ToString (),
						AcType = Convert.ToInt32 (dtDetails.Rows[i][3]),
						Balance = Convert.ToDecimal (dtDetails.Rows[i][4]),
						IntRate = Convert.ToDecimal (dtDetails.Rows[i][5]),
						ODate = Convert.ToDateTime (dtDetails.Rows[i][6]),
						CDate = Convert.ToDateTime (dtDetails.Rows[i][7])
					});
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Error loading Details Data {ex.Message}");
#if SHOWSQLERRORMESSAGEBOX
				MessageBox.Show ("SQL error occurred - See Output for details");
#endif

				return false;
			}

		}
		#endregion load all data base data

		#region Load/show selected data base data

		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for BankAccount Db and fills BankAccount DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <summary>
		/// Fetches SQL data for BankAccount Db and fills BankAccount DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*****************************************************************************************//
		private async void ShowBank_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;

			string currentViewerListEntry = Flags.DbSelectorOpen.GetCurrentViewerListEntry (sender as SqlDbViewer);
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (102, ">>> Entering ShowBank_Click()", Flags.CurrentSqlViewer);
			SendDbSelectorCommand (111, $"SqlDbViewer  - calling AWAIT Task LoadBankTask", Flags.CurrentSqlViewer);
#if ALLOWREFRESH
			if (BankGrid.Items.Count > 0)
			{
				Mouse.OverrideCursor = Cursors.Arrow;
				BankGrid.ItemsSource = null;
				await bvm.LoadBankTaskInSortOrder (-1);
				BankGrid.ItemsSource = bvm.BankAccountObs;
				ParseButtonText (true);
				BankGrid.Refresh ();
				StatusBar.Text = "Bank Account Data Reloaded ....";
				return;
			}
#else
			{
				if (MainWindow.gv.Bankviewer != Guid.Empty)
				{
					Flags.DbSelectorOpen.SetFocusToExistingViewer (MainWindow.gv.Bankviewer);
					Mouse.OverrideCursor = Cursors.Arrow;
					MessageBox.Show ($"A seperate Viewer is already open for the Bank Accounts Database....");
					return;
				}
			}
#endif
			if (BankAccountViewModel.EditdbWndBank != null)
			{
				if (MessageBox.Show ("You have an Edit Window open for the current Database.\r\nThis will be closed if you proceed !", "Edit Window Closing",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.OK) == MessageBoxResult.OK)
				{
					// gotta close the edit window
					BankAccountViewModel.EditdbWndBank.Close ();
					BankAccountViewModel.EditdbWndBank = null;
				}
				else
					return;
			}

			{
				//await Task.Factory.StartNew (() =>
				//{
				//	Dispatcher.Invoke (async () =>
				//	{
				//		await bvm.LoadBankTask ();
				//	});
				//	//All data is loaded  by the above task
				//}
				//);
			}
			await bvm.LoadBankTaskInSortOrder (-1);
			CurrentDb = "BANKACCOUNT";
			this.DetailsGrid.Visibility = Visibility.Hidden;
			this.CustomerGrid.Visibility = Visibility.Hidden;
			this.BankGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			Mouse.OverrideCursor = Cursors.Wait;
			//Window is ONSCREEN by here

			var str = this.BankGrid.SelectedItem as BankAccountViewModel;
			DbSelector.ChangeViewerListEntry (currentViewerListEntry, $"Bank - A/c # {str?.BankNo}, Cust # {str?.CustNo}, Balance £ {str?.Balance}, Interest {str?.IntRate}%", this);

			SendDbSelectorCommand (111, $"ALL BANK DATA LOADED [{this.BankGrid.Items.Count}] records& Grid is setup to index 0", Flags.CurrentSqlViewer);
			ParseButtonText (true);
			UpdateAuxilliaries ("BankAccount Data Loaded...");
			//Set global flags
			Flags.SetGridviewControlFlags (this, this.BankGrid);
			Mouse.OverrideCursor = Cursors.Arrow;
			SendDbSelectorCommand (103, "<<< SqlDbViewer Exiting ShowBank_Click()", Flags.CurrentSqlViewer);
			this.BringIntoView ();
			this.Focus ();
		}

		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// NB it is ONLY called by use of the buttons on the Viewer window, so we 
		/// actually changing the grid contents, which means we need to update 
		/// the DbSelector list correctly as well
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*****************************************************************************************//
		private async void ShowCust_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;

			Mouse.OverrideCursor = Cursors.Wait;
			//Get and store current entry in ViewersList se we can update it later
			string currentViewerListEntry = Flags.DbSelectorOpen.GetCurrentViewerListEntry (sender as SqlDbViewer);

			SendDbSelectorCommand (102, $">>> Entering ShowCust_Click()", Flags.CurrentSqlViewer);
#if ALLOWREFRESH
			if (CustomerGrid.Items.Count > 0)
			{
				Mouse.OverrideCursor = Cursors.Arrow;
				CustomerGrid.ItemsSource = null;
				await cvm.LoadCustomerTaskInSortOrder (-1);
				CustomerGrid.ItemsSource = cvm.CustomersObs;
				CustomerGrid.Refresh ();
				ParseButtonText (true);
				StatusBar.Text = "Customer Data Reloaded ....";
				return;
			}
#else
			{
				if (MainWindow.gv.Custviewer != Guid.Empty)
				{
					Flags.DbSelectorOpen.SetFocusToExistingViewer (MainWindow.gv.Custviewer);
					Mouse.OverrideCursor = Cursors.Arrow;
					MessageBox.Show ($"A seperate Viewer is already open for the Customer Database....");
					return;
				}
			}
#endif
			if (BankAccountViewModel.EditdbWndBank != null)
			{
				if (MessageBox.Show ("You have an Edit Window open for the current Database.\r\nThis will be closed if you proceed !", "Edit Window Closing",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.OK) == MessageBoxResult.OK)
				{
					// gotta close the edit window
					BankAccountViewModel.EditdbWndBank.Close ();
					BankAccountViewModel.EditdbWndBank = null;
				}
				else
					return;
			}

			//We need to go and load the Customer data etc
			this.Show ();
			ExtensionMethods.Refresh (this);
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (111, $"SqlDbViewer  - calling AWAIT Task LoadCustomersTask", Flags.CurrentSqlViewer);

			await cvm.LoadCustomerTaskInSortOrder (-1);
			this.CustomerGrid.ItemsSource = cvm.CustomersObs;
			this.CustomerGrid.SelectedIndex = 0;
			this.CustomerGrid.SelectedItem = 0;
			this.CustomerGrid.CurrentItem = 0;

			CurrentDb = "CUSTOMER";
			this.DetailsGrid.Visibility = Visibility.Hidden;
			this.BankGrid.Visibility = Visibility.Hidden;
			this.CustomerGrid.Visibility = Visibility.Visible;
			this.CustomerGrid.BringIntoView ();
			Filters.IsEnabled = true;

			var str = this.CustomerGrid.SelectedItem as CustomerViewModel;
			DbSelector.ChangeViewerListEntry (currentViewerListEntry, $"Customer - Customer # {str?.CustNo}, Bank # {str?.BankNo}, {str?.LName} {str.Town}, {str?.County}", this);
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (111, $"ALL CUSTOMER DATA LOADED [{this.CustomerGrid.Items.Count}] records& Grid is setup to index 0", Flags.CurrentSqlViewer);
			ParseButtonText (true);
			UpdateAuxilliaries ("Customer Data Loaded...");
			//Set global flags
			Flags.SetGridviewControlFlags (this, this.CustomerGrid);
			Mouse.OverrideCursor = Cursors.Arrow;
			SendDbSelectorCommand (103, $"<<< Exiting ShowCust_Click()", Flags.CurrentSqlViewer);
			this.BringIntoView ();
			this.Focus ();
		}
		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for DetailsViewModel Db and fills relevant DataGrid
		/// <param name="sender"></param>
		/// <param name="e"></param></summary>
		//*****************************************************************************************//
		private async void ShowDetails_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;

			string currentViewerListEntry = Flags.DbSelectorOpen.GetCurrentViewerListEntry (sender as SqlDbViewer);
			SendDbSelectorCommand (102, ">>> Entering ShowDetails_Click()", Flags.CurrentSqlViewer);
#if ALLOWREFRESH
			if (DetailsGrid.Items.Count > 0)
			{
				Mouse.OverrideCursor = Cursors.Arrow;
				DetailsGrid.ItemsSource = null;
				await dvm.LoadDetailsTaskInSortOrder (-1);
				DetailsGrid.ItemsSource = dvm.DetailsObs;
				DetailsGrid.Refresh ();
				ParseButtonText (true);
				StatusBar.Text = "Details Data Reloaded ....";
				return;
			}
#else
			{
				if (MainWindow.gv.Detviewer != Guid.Empty)
				{
					Mouse.OverrideCursor = Cursors.Wait;
					Flags.DbSelectorOpen.SetFocusToExistingViewer (MainWindow.gv.Detviewer);
					Mouse.OverrideCursor = Cursors.Arrow;
					MessageBox.Show ($"A seperate Viewer is already open for the Details Database....");
					return;
				}
			}
#endif
			if (BankAccountViewModel.EditdbWndBank != null)
			{
				if (MessageBox.Show ("You have an Edit Window open for the current Database.\r\nThis will be closed if you proceed !", "Edit Window Closing",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.OK) == MessageBoxResult.OK)
				{
					// gotta close the edit window
					BankAccountViewModel.EditdbWndBank.Close ();
					BankAccountViewModel.EditdbWndBank = null;
				}
				else
				{
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
				}
			}
			SendDbSelectorCommand (111, $"SqlDbViewer  - calling AWAIT Task LoadDetailsTask", Flags.CurrentSqlViewer);
			dvm.LoadDetailsTaskInSortOrder (-1);
			CurrentDb = "DETAILS";
			this.CustomerGrid.Visibility = Visibility.Hidden;
			this.BankGrid.Visibility = Visibility.Hidden;
			this.DetailsGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			Mouse.OverrideCursor = Cursors.Wait;
			var str = this.DetailsGrid.SelectedItem as DetailsViewModel;
			DbSelector.ChangeViewerListEntry (currentViewerListEntry, $"Details - Bank A/C # {str?.BankNo}, Cust # {str?.CustNo}, Balance {str?.Balance}, Interest % {str?.IntRate}", this);
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (111, $"ALL DETAILS DATA LOADED [{this.DetailsGrid.Items.Count}] records& Grid is setup to index 0", Flags.CurrentSqlViewer);

			ParseButtonText (true);
			UpdateAuxilliaries ("Details Data Loaded...");
			//Set global flags
			Flags.SetGridviewControlFlags (this, this.DetailsGrid);
			Mouse.OverrideCursor = Cursors.Arrow;
			SendDbSelectorCommand (103, $"<<< Exiting ShowDetals_Click()", Flags.CurrentSqlViewer);
			this.BringIntoView ();
			this.Focus ();
		}
		//*****************************************************************************************//

		#endregion Load/show selected data base data

		#region Standard Click Events
		private void ExitFilter_Click (object sender, RoutedEventArgs e)
		{
			//Just "Close" the Filter panel
			//			FilterFrame.Visibility = Visibility.Hidden;
		}
		private void ContextMenu1_Click (object sender, RoutedEventArgs e)
		{
			//Add a new row
			if (CurrentDb == "BANKACCOUNT")
			{
				DataRow dr = BankAccountViewModel.dtBank.NewRow ();
				BankAccountViewModel.dtBank.Rows.Add (dr);
				//				BankGrid.DataContext = dtBank;
			}
		}
		private void ContextMenu2_Click (object sender, RoutedEventArgs e)
		{
			//Delete current Row
			BankAccountViewModel dg = sender as BankAccountViewModel;
			DataRowView row = (DataRowView)this.BankGrid.SelectedItem;

		}
		private void ContextMenu3_Click (object sender, RoutedEventArgs e)
		{
			//Close Window
		}
		private void Multiaccs_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;
			//			//Show only Customers with multiple Bank Accounts
			//			Window_MouseDown (sender, null);
			//			string s = Multiaccounts.Content as string;
			//			if (s.Contains ("<<-"))
			//				isMultiMode = false;
			//			else
			//				isMultiMode = true;
			//			if (CurrentDb == "BANKACCOUNT")
			//				FillBankAccountDataGrid ();
			//			else if (CurrentDb == "CUSTOMER")
			//#pragma TODO we need this
			////				CurrentDb = "CUSTOMER";
			//				FillCustomerDataGrid ();
			//			else if (CurrentDb == "DETAILS")
			//				FillDetailsDataGrid ();

			//			ControlTemplate tmp = Utils.GetDictionaryControlTemplate ("HorizontalGradientTemplateGray");
			//			Filters.Template = tmp;
			//			Brush br = Utils.GetDictionaryBrush ("HeaderBrushGray");
			//			Filters.Background = br;
			//			Filters.Content = "Filtering";

		}
		private void ContextMenuFind_Click (object sender, RoutedEventArgs e)
		{
			// find something - this returns  the top rows data in full
			BankAccountViewModel b = this.BankGrid.Items.CurrentItem as BankAccountViewModel;

		}
		private void CloseAccount_Click (object sender, RoutedEventArgs e)
		{
			//Now get the actual data item behind the selected row
			if (CurrentDb == "BANKACCOUNT")
			{
				int id = BankCurrentRowAccount.Id;
				BankCurrentRowAccount.CDate = DateTime.Now;
#pragma checksum   why is thi smissing ?????
				//ViewerGrid_RowEditEnded (null, null);
				this.BankGrid.Items.Refresh ();
			}
			else if (CurrentDb == "CUSTOMER")
			{
				int id = CustomerCurrentRowAccount.Id;
				CustomerCurrentRowAccount.CDate = DateTime.Now;
			}
			else if (CurrentDb == "DETAILS")
			{
				int id = DetailsCurrentRowAccount.Id;
				DetailsCurrentRowAccount.CDate = DateTime.Now;
			}
		}

		#endregion Standard Click Events

		#region Filtering code
		//*****************************************************************************************//
		private void SetFilter_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;
			// Call up the Filtering Window to select 
			// the filtering conditions required
			Window_MouseDown (sender, null);
			bool isRecovering = false;
			if (CurrentDb == "")
			{
				MessageBox.Show ("You need to have loaded one of the data tables\r\nbefore you can access the filtering system");
				return;
			}
			if (Filters.Content == "Reset")
			{
				Filters.Content = "Filter";
				if (CurrentDb == "BANKACCOUNT")
					ShowBank_Click (null, null);
				else if (CurrentDb == "CUSTOMER")
					ShowCust_Click (null, null);
				else if (CurrentDb == "DETAILS")
					ShowDetails_Click (null, null);
				ControlTemplate tmp = Utils.GetDictionaryControlTemplate ("HorizontalGradientTemplateGray");
				Filters.Template = tmp;
				Brush br = Utils.GetDictionaryBrush ("HeaderBrushGray");
				Filters.Background = br;
				Filters.Content = "Filtering";
				Mouse.OverrideCursor = Cursors.Arrow;

				return;

			}
			SQLFilter sf = new SQLFilter (this);
			//// filter any table
			if (CurrentDb == "BANKACCOUNT")
			{
				sf.FilterList.Items.Clear ();
				sf.FilterList.Items.Add ("ID");
				sf.FilterList.Items.Add ("BANKNO");
				sf.FilterList.Items.Add ("CUSTNO");
				sf.FilterList.Items.Add ("ACTYPE");
				sf.FilterList.Items.Add ("BALANCE");
				sf.FilterList.Items.Add ("INTRATE");
				sf.FilterList.Items.Add ("ODATE");
				sf.FilterList.Items.Add ("CDATE");
			}
			else if (CurrentDb == "CUSTOMER")
			{
				sf.FilterList.Items.Clear ();
				sf.FilterList.Items.Add ("Id");
				sf.FilterList.Items.Add ("ID");
				sf.FilterList.Items.Add ("BANKNO");
				sf.FilterList.Items.Add ("CUSTNO");
				sf.FilterList.Items.Add ("ACTYPE");
				sf.FilterList.Items.Add ("FNAME");
				sf.FilterList.Items.Add ("LNAME");
				sf.FilterList.Items.Add ("ADDR1");
				sf.FilterList.Items.Add ("ADDR2");
				sf.FilterList.Items.Add ("TOWN");
				sf.FilterList.Items.Add ("COUNTY");
				sf.FilterList.Items.Add ("PCODE");
				sf.FilterList.Items.Add ("PHONE");
				sf.FilterList.Items.Add ("MOBILE");
				sf.FilterList.Items.Add ("DOB");
				sf.FilterList.Items.Add ("ODATE");
				sf.FilterList.Items.Add ("CDATE");
			}
			else if (CurrentDb == "DETAILS")
			{
				sf.FilterList.Items.Clear ();
				sf.FilterList.Items.Add ("ID");
				sf.FilterList.Items.Add ("BANKNO");
				sf.FilterList.Items.Add ("CUSTNO");
				sf.FilterList.Items.Add ("ACTYPE");
				sf.FilterList.Items.Add ("BALANCE");
				sf.FilterList.Items.Add ("INTRATE");
				sf.FilterList.Items.Add ("ODATE");
				sf.FilterList.Items.Add ("CDATE");
			}
			sf.Operand.Items.Add ("Equal to");
			sf.Operand.Items.Add ("Not Equal to");
			sf.Operand.Items.Add ("Greater than or Equal to");
			sf.Operand.Items.Add ("Less than or Equal to");
			sf.Operand.Items.Add (">= value1 AND <= value2");
			sf.Operand.Items.Add ("> value1 AND < value2");
			sf.Operand.Items.Add ("< value1 OR > value2");
			sf.Operand.SelectedIndex = 0;
			//			}
			sf.currentDb = CurrentDb;
			sf.FilterResult = false;
			sf.ShowDialog ();
			if (sf.FilterResult)
			{
				columnToFilterOn = sf.ColumnToFilterOn;
				filtervalue1 = sf.FilterValue.Text;
				filtervalue2 = sf.FilterValue2.Text;
				operand = sf.operand;
				DoFilter (null, null);
				StatusBar.Text = $"Filtered Results are shown above. Column = {columnToFilterOn}, Condition = {operand}, Value(s) = {filtervalue1}, {filtervalue2} ";
				Filters.IsEnabled = false;
				Filters.Content = "Reset";
				Filters.IsEnabled = true;
				ControlTemplate ctmp = Utils.GetDictionaryControlTemplate ("HorizontalGradientTemplateGreen");
				Filters.Template = ctmp;
				Brush brs = Utils.GetDictionaryBrush ("HeaderBrushGreen");
				Filters.Background = brs;
			}
			else
			{
				ControlTemplate tmp = Utils.GetDictionaryControlTemplate ("HorizontalGradientTemplateGray");
				Filters.Template = tmp;
				Brush br = Utils.GetDictionaryBrush ("HeaderBrushGray");
				Filters.Background = br;
				Filters.Content = "Filtering";
			}
		}
		//*****************************************************************************************//
		private void DoFilter (object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			// carry out the filtering operation
			string Commandline1 = "";
			string Commandline = "";
			if (CurrentDb == "BANKACCOUNT")
			{
				Commandline1 = $"Select * from BankAccount where ";
				BankAccountViewModel.dtBank.Clear ();
			}
			else if (CurrentDb == "CUSTOMER")
			{
				Commandline1 = $"Select * from Customer where ";
				CustomerViewModel.dtCust.Clear ();
			}
			else if (CurrentDb == "DETAILS")
			{
				Commandline1 = $"Select * from SecAccounts where ";
				DetailsViewModel.dtDetails.Clear ();
			}

			if (operand.Contains ("Not Equal"))
				Commandline = Commandline1 + $" {columnToFilterOn} <> '{filtervalue1}'";
			else if (operand.Contains ("Equal to"))
				Commandline = Commandline1 + $" {columnToFilterOn} = '{filtervalue1}'";
			else if (operand.Contains ("Greater than"))
				Commandline = Commandline1 + $" {columnToFilterOn} >= '{filtervalue1}'";
			else if (operand.Contains ("less than"))
				Commandline = Commandline1 + $" {columnToFilterOn} <= '{filtervalue1}'";
			else
			{
				if (filtervalue1 == "" || filtervalue2 == "")
				{
					System.Windows.MessageBox.Show ("The Filter you have selected needs TWO seperate Values.\r\nUnable to continue with Filtering process...");
					return;
				}
				else if (operand.Contains (">= value1 AND <= value2"))
					Commandline = Commandline1 + $" {columnToFilterOn} >= {filtervalue1} AND {columnToFilterOn} <= '{filtervalue2}'";
				else if (operand.Contains ("> value1 AND < value2"))
					Commandline = Commandline1 + $" {columnToFilterOn} > {filtervalue1} AND {columnToFilterOn} < '{filtervalue2}'";
				else if (operand.Contains ("< value1 OR > value2"))
					Commandline = Commandline1 + $" {columnToFilterOn} < {filtervalue1} OR {columnToFilterOn} > '{filtervalue2}'";
			}
			Commandline += " order by CustNo";
			//	set file wide filter command line
			FilterCommand = Commandline;
			if (CurrentDb == "BANKACCOUNT")
			{
				IsFiltered = "BANKACCOUNT";
				ShowBank_Click (null, null);
			}
			else if (CurrentDb == "CUSTOMER")
			{
				IsFiltered = "CUSTOMER";
				ShowCust_Click (null, null);
			}
			else if (CurrentDb == "DETAILS")
			{
				IsFiltered = "DETAILS";
				ShowDetails_Click (null, null);
			}
			Mouse.OverrideCursor = Cursors.Arrow;

		}
		//*****************************************************************************************//
		/// <summary>
		/// Just sets up the Filter/Duplicate buttons status
		/// </summary>
		private void ResetOptionButtons ()
		{
			Filters.IsEnabled = true;
			Multiaccounts.Content = " Multiple A/C's";
		}
		private void ResetauxilliaryButtons ()
		{
			ControlTemplate tmp = Utils.GetDictionaryControlTemplate ("HorizontalGradientTemplateGray");
			Filters.Template = tmp;
			Brush br = Utils.GetDictionaryBrush ("HeaderBrushGray");
			Filters.Background = br;
			Filters.Content = "Filtering";
		}

		#endregion Filtering code

		#region NotifyPropertyChanged
		//*****************************************************************************************//
		public class User: INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public void NotifyPropertyChanged (string propName)
			{
				if (this.PropertyChanged != null)
					PropertyChanged (this, new PropertyChangedEventArgs (propName));
			}
		}
		//*****************************************************************************************//
		private void OnPropertyChanged (string PropertyName = null)
		{
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (PropertyName));
		}


		#endregion NotifyPropertyChanged

		#region ObservableCollection setup - NOTHING HERE


		//*****************************************************************************************//
		//We just need to declare an ObservableCollection of the relevant class type here
		// and then we can fill it with SQL data and access it
		//private ObservableCollection<BankAccountViewModel> _bankAccountObs;
		//public ObservableCollection<BankAccountViewModel> BankAccountObs
		//{
		//	get { return _bankAccountObs; }
		//	set
		//	{
		//		_bankAccountObs = value;
		//		OnPropertyChanged (BankAccountObs.ToString ());
		//	}
		//}
		//private ObservableCollection<CustomerViewModel> _customerObs;
		//public ObservableCollection<CustomerViewModel> CustomerObs
		//{
		//	get { return _customerObs; }
		//	set
		//	{
		//		_customerObs = value;
		//		OnPropertyChanged (CustomerViewModel.CustomersObs.ToString ());
		//	}
		//}
		//private ObservableCollection<DetailsViewModel> _detailsObs;
		//public ObservableCollection<DetailsViewModel> DetailsObs
		//{
		//	get { return _detailsObs; }
		//	set
		//	{
		//		_detailsObs = value;
		//		OnPropertyChanged (dvm.DetailsObs .ToString ());
		//	}
		//}
		//private BankAccountViewModel _selectedBankAccount;
		//public BankAccountViewModel SelectedBankAccount
		//{
		//	get { return _selectedBankAccount; }
		//	set
		//	{
		//		_selectedBankAccount = value;
		//		OnPropertyChanged (.SelectedBankAccount.ToString ());
		//	}
		//}

		//private CustomerViewModel._selectedCustomerAccount;
		//public CustomerViewModel.SelectedCustomerAccount
		//{
		//	get { return _selectedCustomerAccount; }
		//	set
		//	{
		//		_selectedCustomerAccount = value;
		//		OnPropertyChanged (SelectedBankAccount.ToString ());
		//	}
		//}
		//private DetailsViewModel _selectedDetailAccount;
		//public DetailsViewModel SelectedDetailAccount
		//{
		//	get { return _selectedDetailAccount; }
		//	set
		//	{
		//		_selectedDetailAccount = value;
		//		OnPropertyChanged (SelectedDetailAccount.ToString ());
		//	}
		//}
		#endregion ObservableCollection setup

		#region grid row selection code

		private bool CheckForDataChange (BankAccountViewModel bvm)
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

		private void BankGrid_SelectedCellsChanged (object sender, SelectedCellsChangedEventArgs e)
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSleector ViewersList text
			if (this.BankGrid.SelectedItem != null)
			{
				//This gives me the entire Db Record in "c"
				BankAccountViewModel c = this.BankGrid?.SelectedItem as BankAccountViewModel;
				if (c == null) return;
				string date = Convert.ToDateTime (c.ODate).ToShortDateString ();
				string s = $"Bank - # {c.CustNo}, Bank #{c.BankNo}, Customer # {c.CustNo}, £{c.Balance}, {c.IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags.SqlViewerIsLoading = false;
				UpdateDbSelectorItem (s);
				ExtensionMethods.Refresh (this);

				// broadcast data change to all subscribers
				//				SendDataChanged (BankGrid, "BANKACCOUNT");
			}
		}
		private void CustomerGrid_SelectedCellsChanged (object sender, SelectedCellsChangedEventArgs e)
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSleector ViewersList text
			if (this.CustomerGrid.SelectedItem != null)
			{
				//This gives me an entrie Db Record in "c"
				CustomerViewModel c = this.CustomerGrid?.SelectedItem as CustomerViewModel;
				if (c == null) return;
				string s = $"Customer - # {c.CustNo}, Bank #@{c.BankNo}, {c.LName}, {c.Town}, {c.County} {c.PCode}";
				// ensure global flag is cleared after loading a viewer
				Flags.SqlViewerIsLoading = false;
				UpdateDbSelectorItem (s);
				ExtensionMethods.Refresh (this);
				// broadcast data change to all subscribers
				// broadcast data change to all subscribers
				//				SendDataChanged (CustomerGrid, "CUSTOMER");
			}
		}
		private void DetailsGrid_SelectedCellsChanged (object sender, SelectedCellsChangedEventArgs e)
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSelector ViewersList text
			if (this.DetailsGrid.SelectedItem != null)
			{
				//This gives me an entire Db Record in "c"
				DetailsViewModel c = this.DetailsGrid?.SelectedItem as DetailsViewModel;
				if (c == null) return;
				string date = Convert.ToDateTime (c.ODate).ToShortDateString ();
				string s = $"Details - # {c.CustNo}, Bank #@{c.BankNo}, Customer # {c.CustNo}, £{c.Balance}, {c.IntRate}%,  {date}";
#pragma NOT NEEDED ????
				// ensure global flag is cleared after loading a viewer
				Flags.SqlViewerIsLoading = false;
				UpdateDbSelectorItem (s);
				ExtensionMethods.Refresh (this);
				// broadcast data change to all subscribers
				// broadcast data change to all subscribers
#pragma TEMP - blowing up on Db load
				//				SendDataChanged (DetailsGrid, "DETAILS");
			}
		}
		#endregion grid row selection code

		#region EVENTHANDLERS

		//*******************************************************************************************************//
		// EVENT HANDLER
		//*******************************************************************************************************//
		/// <summary>
		///  This is triggered by an Event started by EditDb changing it's SelectedIndex
		///  to let us update our selectuon in line with it
		/// </summary>
		/// <param name="selectedRow"></param>
		/// <param name="caller"></param>
		private void TriggerEditDbUpdate (string Caller, int index, object datatype)
		{
			//This triggers the delegate which wkill resuolt in the Handler Function inside DbEdit
			// to be called, and it can handle the changes made in there as needed
			EditArgs.Caller = Caller;
			EditArgs.CurrentIndex = index;
			EditArgs.DataType = datatype;
			SQLEditOcurred dbe = new SQLEditOcurred (EditDb.HandleSQLEdit);
			//Trigger the delegate
			dbe (this, EditArgs);
		}
		public void resetSQLDBindex (bool self, int RowSelected, DataGrid caller)
		{
			//This is received when a change is made in EditDb DataGrid
			// and handles selecting the new index row and scrolling it into view
			int id1 = 0;
			int id2 = 0;

			// use our Global Grid pointer for access
			if (caller.Name == "BankGrid")
			{
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 2330) - Called by {caller.Name} \r\n" +
					"Current index = {BankGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");
				}
				if (this.BankGrid.SelectedIndex != RowSelected)
				{
					if (this.BankGrid.SelectedIndex != RowSelected)
						this.BankGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (BankGrid, RowSelected, -1);
					ScrollList (this.BankGrid, -1);
					if (BankAccountViewModel.SqlUpdating)
					{
						//Need to update the grid as EDITDB has made a change
						CollectionViewSource.GetDefaultView (this.BankGrid.ItemsSource).Refresh ();
						BankAccountViewModel.SqlUpdating = false;
					}
				}
			}
			else if (caller.Name == "DataGrid1")
			{
				//Data selection changed in EditDb viewer
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 2350) - Called by {caller.Name} \r\nCurrent index = {BankGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");
				}
				if (this.BankGrid.SelectedIndex != -1 && this.BankGrid.SelectedIndex != RowSelected)
				{
					if (this.BankGrid.SelectedIndex != RowSelected)
						this.BankGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (this.BankGrid, RowSelected, -1);
					ScrollList (this.BankGrid, -1);
					if (this.BankGrid.ItemsSource != null)
					{
						//Need to update the grid as EDITDB has made a change
						CollectionViewSource.GetDefaultView (this.BankGrid.ItemsSource).Refresh ();
						BankAccountViewModel.SqlUpdating = false;
					}
				}
			}
			else if (caller.Name == "DataGrid2")
			{
				//this is a customer account viewer grid !!!!
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER (2371) - Called by {caller.Name} - \r\nCurrent index = {this.CustomerGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");
				}
				if (this.CustomerGrid.SelectedIndex != RowSelected)
				{
					if (this.CustomerGrid.SelectedIndex != RowSelected)
						//						if (BankAccountViewModel.CurrentEditDbCustomerGrid.SelectedIndex != RowSelected)
						this.CustomerGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (this.CustomerGrid, RowSelected, -1);
					ScrollList (this.CustomerGrid, -1);

					if (BankAccountViewModel.SqlUpdating)
					{
						//Need to update the grid as EDITDB has made a change
						//						Console.WriteLine ($"\r\nSQLDBVIEWER (2092) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						CollectionViewSource.GetDefaultView (this.CustomerGrid.ItemsSource).Refresh ();
						BankAccountViewModel.SqlUpdating = false;
					}
				}
			}
			else if (caller.Name == "DetailsGrid")
			{
				Console.WriteLine ($"SqlDbViewer - (RESETSQLDBINDEX HANDLER 2076) - Called by {caller.Name} - \r\nCurrent index = {this.DetailsGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");

				if (this.DetailsGrid.SelectedIndex != RowSelected)
				{
					if (this.DetailsGrid.SelectedIndex == null)
						this.DetailsGrid.SelectedIndex = RowSelected;
					if (this.DetailsGrid.SelectedIndex != RowSelected)
						this.DetailsGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (this.DetailsGrid, RowSelected, -1);
					ScrollList (this.DetailsGrid, -1);

					if (DetailsViewModel.SqlUpdating)
					{
						//Need to update the grid as EDITDB has made a change
						if (Flags.EventHandlerDebug)
						{
							Console.WriteLine ($"\r\nSQLDBVIEWER (2088) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						}
						CollectionViewSource.GetDefaultView (this.DetailsGrid.ItemsSource).Refresh ();
						BankAccountViewModel.SqlUpdating = false;

					}
				}
			}
		}
		public IEnumerable<DataGridRow> GetDataGridRows (DataGrid grid)
		{
			var itemsSource = grid.ItemsSource as IEnumerable;
			if (null == itemsSource) yield return null;
			foreach (var item in itemsSource)
			{
				var row = grid.ItemContainerGenerator.ContainerFromItem (item) as DataGridRow;
				if (null != row) yield return row;
			}
		}

		public event EventHandler<LoadedEventArgs> DataLoaded;

		protected virtual void OnDataLoaded (string info)
		{
			DataLoaded?.Invoke (this, new LoadedEventArgs () { CallerDb = info });
		}

		#endregion EVENTHANDLERS

		#region CellEdit Checker functions
		private void BankGrid_BeginningEdit (object sender, DataGridBeginningEditEventArgs e)
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e.Row.GetIndex ();
			OriginalCellColumn = e.Column.DisplayIndex;
			DataGridColumn dgc = e.Column as DataGridColumn;
			string name = dgc.SortMemberPath;
			DataGridRow dgr = e.Row;
			//			BankAccountViewModel bvm = dgr.Item as BankAccountViewModel;
			OriginalDataType = name;
			switch (name.ToUpper ())
			{
				case "BANKNO":
					OriginalCellData = bvm.BankNo;
					break;
				case "CUSTO":
					OriginalCellData = bvm.CustNo;
					break;
				case "ACTYPE":
					OriginalCellData = bvm.AcType;
					break;
				case "BALANCE":
					OriginalCellData = bvm.Balance;
					break;
				case "INTRATE":
					OriginalCellData = bvm.IntRate;
					break;
				case "ODATE":
					OriginalCellData = bvm.ODate;
					break;
				case "CDATE":
					OriginalCellData = bvm.CDate;
					break;
			}
		}

		//These all set a global bool to flag whether a cell has actually been changed
		//so we do not call SQL Update uneccessarily
		private void BankGrid_CellEditEnding (object sender, DataGridCellEditEndingEventArgs e)
		{
			BankAccountViewModel c = BankGrid.SelectedItem as BankAccountViewModel;
			TextBox textBox = e.EditingElement as TextBox;
			if (textBox == null)
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox.Text;
			SelectionhasChanged = (OriginalCellData.ToString () != str);
		}

		private void CustomerGrid_CellEditEnding (object sender, DataGridCellEditEndingEventArgs e)
		{
			CustomerViewModel c = CustomerGrid.SelectedItem as CustomerViewModel;
			TextBox textBox = e.EditingElement as TextBox;
			if (textBox == null)
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox.Text;
			SelectionhasChanged = (OriginalCellData.ToString () != str);
		}

		private void DetailsGrid_CellEditEnding (object sender, DataGridCellEditEndingEventArgs e)
		{
			DetailsViewModel c = DetailsGrid.SelectedItem as DetailsViewModel;
			TextBox textBox = e.EditingElement as TextBox;
			if (textBox == null)
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox.Text;
			SelectionhasChanged = (OriginalCellData.ToString () != str);

		}

		private void DetailsGrid_BeginningEdit (object sender, DataGridBeginningEditEventArgs e)
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e.Row.GetIndex ();
			OriginalCellColumn = e.Column.DisplayIndex;
			DataGridColumn dgc = e.Column as DataGridColumn;
			string name = dgc.SortMemberPath;
			DataGridRow dgr = e.Row;
			//			DetailsViewModel dvm = dgr.Item as DetailsViewModel;
			OriginalDataType = name;
			switch (name.ToUpper ())
			{
				case "BANKNO":
					OriginalCellData = dvm.BankNo;
					break;
				case "CUSTO":
					OriginalCellData = dvm.CustNo;
					break;
				case "ACTYPE":
					OriginalCellData = dvm.AcType;
					break;
				case "BALANCE":
					OriginalCellData = dvm.Balance;
					break;
				case "INTRATE":
					OriginalCellData = dvm.IntRate;
					break;
				case "ODATE":
					OriginalCellData = dvm.ODate;
					break;
				case "CDATE":
					OriginalCellData = dvm.CDate;
					break;
			}

		}

		private void CustomerGrid_BeginningEdit (object sender, DataGridBeginningEditEventArgs e)
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e.Row.GetIndex ();
			OriginalCellColumn = e.Column.DisplayIndex;
			DataGridColumn dgc = e.Column as DataGridColumn;
			string name = dgc.SortMemberPath;
			DataGridRow dgr = e.Row;
			//			CustomerViewModel bvm = dgr.Item as CustomerViewModel;
			OriginalDataType = name;
			switch (name.ToUpper ())
			{
				case "BANKNO":
					OriginalCellData = cvm.BankNo;
					break;
				case "CUSTO":
					OriginalCellData = cvm.CustNo;
					break;
				case "ACTYPE":
					OriginalCellData = cvm.AcType;
					break;
				case "FNAME":
					OriginalCellData = cvm.FName;
					break;
				case "LNAME":
					OriginalCellData = cvm.LName;
					break;
				case "ADDR1":
					OriginalCellData = cvm.Addr1;
					break;
				case "ADDR2":
					OriginalCellData = cvm.Addr2;
					break;
				case "TOWN":
					OriginalCellData = cvm.Town;
					break;
				case "COUNTY":
					OriginalCellData = cvm.County;
					break;
				case "PCODE":
					OriginalCellData = cvm.PCode;
					break;
				case "PHONE":
					OriginalCellData = cvm.Phone;
					break;
				case "MOBILE":
					OriginalCellData = cvm.Mobile;
					break;
				case "DOB":
					OriginalCellData = cvm.Dob;
					break;
				case "ODATE":
					OriginalCellData = cvm.ODate;
					break;
				case "CDATE":
					OriginalCellData = cvm.CDate;
					break;
			}
		}
		#endregion CellEdit Checker functions

		// NOT being called at present 2/4/21
		//**************************************************************************************************************************************************************//

		#region Keyboard /Mousebutton handlers
		private void Window_MouseDown (object sender, MouseButtonEventArgs e)
		{
			Window_GotFocus (sender, e);
		}

		//*****************************************************************************************//

		public void UpdateDbSelectorBtns (SqlDbViewer viewer)
		{
			// works with multiple entries 22 March 2021

			if (Flags.DbSelectorOpen == null)
				return;

			if (Flags.DbSelectorOpen.ViewersList.Items.Count == 1)
			{
				Flags.DbSelectorOpen.ViewerDeleteAll.IsEnabled = false;
				Flags.DbSelectorOpen.ViewerDelete.IsEnabled = false;
				Flags.DbSelectorOpen.SelectViewerBtn.IsEnabled = false;
				return;
			}
			else
			{
				if (Flags.DbSelectorOpen.ViewersList.Items.Count > 2)
					Flags.DbSelectorOpen.ViewerDeleteAll.IsEnabled = true;
				else
					Flags.DbSelectorOpen.ViewerDeleteAll.IsEnabled = false;
				Flags.DbSelectorOpen.ViewerDelete.IsEnabled = true;
				Flags.DbSelectorOpen.SelectViewerBtn.IsEnabled = true;
			}
		}
		/// <summary>
		/// No longer used - ignore....
		/// </summary>
		/// <param name="selection"></param>
		void SetButtonStatus (string selection)
		{

			//			return;
			//This sets the currently selected Db button to be defaulted
			// making it change background color
			if (selection == "BANKACCOUNT")
			{
				ShowDetails.Tag = false;
				ShowBank.Tag = true;    //Allows auto coloration
			}
			else if (selection == "CUSTOMER")
			{
				ShowDetails.Tag = false;
				ShowCust.Tag = true;
			}
			else if (selection == "DETAILS")
			{
				Tag = false;
				ShowDetails.Tag = true;
			}
		}

		private void BankGrid_MouseRightButtonUp (object sender, MouseButtonEventArgs e)
		{
			Type type;
			string cellData;
			int row = -1;
			int col = -1;
			string colName = "";
			object rowdata = null;
			object cellValue = null;


			if (CurrentDb == "BANKACCOUNT")
			{
				//				BankAccountViewModel bvm = bvm ();
				cellValue = DataGridSupport.GetCellContent (sender, e, CurrentDb, out row, out col, out colName, out rowdata);
				if (row == -1)
					row = BankGrid.SelectedIndex;
			}
			else if (CurrentDb == "CUSTOMER")
			{
				CustomerViewModel bvm = cvm;
				cellValue = DataGridSupport.GetCellContent (sender, e, CurrentDb, out row, out col, out colName, out rowdata);
				if (row == -1)
					row = BankGrid.SelectedIndex;
			}
			else if (CurrentDb == "DETAILS")
			{
				CustomerViewModel bvm = cvm;
				cellValue = DataGridSupport.GetCellContent (sender, e, CurrentDb, out row, out col, out colName, out rowdata);
				if (row == -1)
					row = BankGrid.SelectedIndex;
			}
			if (cellValue == null)
			{
				MessageBox.Show ($"Cannot access Data in the current cell, Row returned = {row}, Column = {col}, Column Name = {colName}");
				return;
			}
			else if (row == -1 && col == -1)
			{
				//Header was clicked in
				type = cellValue.GetType ();
				cellData = cellValue.ToString ();
				if (cellData != "")
				{
					if (cellData.Contains (":"))
					{
						int offset = cellData.IndexOf (':');
						string result = cellData.Substring (offset + 1).Trim ();
						MessageBox.Show ($"Column clicked was a Header  =\"{result}\"");
					}
				}
				return;
			}
			type = cellValue.GetType ();
			cellData = cellValue.ToString ();
			MessageBox.Show ($"Data in the current cell \r\nColumn is \"{colName},\", Data Type=\"{type.Name}\"\r\nData = [{cellData}]\",\r\nRow={row}, Column={col}", "Requested Cell Contents");
		}
		private void BankGrid_MouseRightButtonDown (object sender, MouseButtonEventArgs e)
		{
			return;
		}
		private void ShowBank_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.RightAlt)
			{
				Flags.ListGridviewControlFlags ();
			}
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Enter)

				return;
			else
				base.OnKeyDown (e);
		}

		private void OnKeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)

				return;
			else
				base.OnKeyDown (e);

		}

		private void Window_PreviewKeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				//clear flags in ViewModel
				if (CurrentDb == "BANKACCOUNT")
				{
					Flags.ActiveSqlDbViewer = null;
					BankAccountViewModel.ClearFromSqlList (this.BankGrid, CurrentDb);
				}
				else if (CurrentDb == "CUSTOMER")
				{
					Flags.ActiveSqlDbViewer = null;
					BankAccountViewModel.ClearFromSqlList (this.CustomerGrid, CurrentDb);
				}
				else if (CurrentDb == "DETAILS")
				{
					Flags.ActiveSqlDbViewer = null;
					BankAccountViewModel.ClearFromSqlList (this.DetailsGrid, CurrentDb);
				}
				SendDbSelectorCommand (99, "Window is closing", Flags.CurrentSqlViewer);
				RemoveFromViewerList ();

				EventHandlers.ClearWindowHandles (null, this);
				BankAccountViewModel.EditdbWndBank = null;
				Flags.CurrentSqlViewer = null;
				UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
				Close ();
			}
			else if (e.Key == Key.RightAlt)
			{
				Flags.ListGridviewControlFlags ();
			}
		}
		#endregion Keyboard handlers

		#region Tuple Handlers
		/// <returns>A fully populated Tuple</returns>
		public static object CreateTuple (string currentDb)
		{
			object tpl = null;
			//content of Tuple is : (This, string "currentDb", int selectedIndex, , int Tag, object selectedItem)
			/*
			Item1 = current SqlDbViewer
			Item2 = CurrentDb string`
			Item3 = Grid.SelectedIndex
				  */
			if (currentDb == "BANKACCOUNT")
				tpl = Tuple.Create (Flags.CurrentSqlViewer, currentDb, Flags.SqlBankGrid.SelectedIndex);
			else if (currentDb == "CUSTOMER")
				tpl = Tuple.Create (Flags.CurrentSqlViewer, currentDb, Flags.SqlCustGrid.SelectedIndex);
			else if (currentDb == "DETAILS")

				tpl = Tuple.Create (Flags.CurrentSqlViewer, currentDb, Flags.SqlDetGrid.SelectedIndex);
			return tpl;
		}

		private void dummy ()
		{
			//		 public void oldUpdateSqlControl (Tuple<SqlDbViewer, string, int, Guid, object> tuple)
			//		public  void UpdateSqlControl (object tpl)
			//		{
			//	// Method  to add a new entry to the GridConbtrokl structure used throughout DbSelector and SqlDbViewer

			//	/*
			//		 Tuple<SqlDbViewer, string, int, Guid, object> tuple
			//	 	 Content of Tuple is : (This, string "currentDb", int selectedIndex)

			//	 MainWindow.gv structure
			//	Maximum is 10

			//	string[ ]		CurrentDb 
			//	DataGrid[ ]	Datagrid
			//	DbSelector	DbSelectorWindow
			//	int[ ]		ListBoxId 
			//	string		PrettyDetails
			//	int			ViewerCount 
			//	Window[ ]	window
			//	*/
			//	var tuple = tpl as Tuple;
			//	int x = MainWindow.gv.ViewerCount + 1;
			//	{
			//		//				MainWindow.gv.DbSelectorWindow = ;
			//		MainWindow.gv.CurrentDb[x] = tuple.Item2;
			//		if (tuple.Item2 == "BANKACCOUNT")
			//		{
			//			MainWindow.gv.Datagrid[x] = tuple.Item1.BankGrid;
			//			MainWindow.gv.ListBoxId[x] = (int)tuple.Item1.Tag;
			//			var rec = tuple.Item5 as BankAccountViewModel;
			//			MainWindow.gv.PrettyDetails = "Not Know as yet...";
			//			if (rec != null)
			//				MainWindow.gv.PrettyDetails = $"Bank - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Balance £ {rec?.Balance}, Interest {rec?.IntRate}%";
			//		}
			//		MainWindow.gv.ViewerCount++;
			//		MainWindow.gv.window[x] = tuple.Item1;

			//			}
			//return null;
		}

		public void UpdateSqlControl (Tuple<SqlDbViewer, string, int> tuple)
		//	NOT IN USE public static void UpdateSqlControl ( object  tuple)
		{
			//{
			//	// Method  to add a new entry to the GridConbtrokl structure used throughout DbSelector and SqlDbViewer
			//	/*
			//		 Tuple<SqlDbViewer, string, int> tuple
			//		 Content of Tuple is : (This, string "currentDb", int selectedIndex)

			//	 MainWindow.gv structure
			//	Maximum is 10

			//	string[ ]		CurrentDb 
			//	DataGrid[ ]	Datagrid
			//	DbSelector	DbSelectorWindow
			//	int[ ]		ListBoxId 
			//	string		PrettyDetails
			//	int			ViewerCount 
			//	Window[ ]	window
			//	*/
			//}
			//// Get the current list count  1 - x & decrement it to match our Flags 0 - x by deducting 2
			//int x = MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count;
			//{
			//	SqlDbViewer sqlv = tuple.Item1 as SqlDbViewer;
			//	MainWindow.gv.CurrentDb[x] = tuple.Item2;
			//	if (tuple.Item2 == "BANKACCOUNT")
			//	{
			//		MainWindow.gv.Datagrid[x] = tuple.Item1.BankGrid;
			//		MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
			//		MainWindow.gv.Datagrid[x] = BankGrid;
			//		if (BankGrid.SelectedIndex == -1)
			//		{
			//			BankGrid.SelectedIndex = 0;
			//			BankGrid.SelectedItem = 0;
			//		}
			//		var rec = tuple.Item1.BankGrid.SelectedItem as BankAccountViewModel;
			//		MainWindow.gv.PrettyDetails = "Not Know as yet...";
			//		if (rec != null)
			//			MainWindow.gv.PrettyDetails = $"Bank - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Balance £ {rec?.Balance}, Interest {rec?.IntRate}%";
			//	}
			//	if (tuple.Item2 == "CUSTOMER")
			//	{
			//		MainWindow.gv.Datagrid[x] = tuple.Item1.BankGrid;
			//		MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
			//		MainWindow.gv.Datagrid[x] = CustomerGrid;
			//		if (CustomerGrid.SelectedIndex == -1)
			//		{
			//			CustomerGrid.SelectedIndex = 0;
			//			CustomerGrid.SelectedItem = 0;
			//		}
			//		var rec = tuple.Item1.CustomerGrid.SelectedItem as CustomerViewModel;
			//		MainWindow.gv.PrettyDetails = "Not Know as yet...";
			//		if (rec != null)
			//			MainWindow.gv.PrettyDetails = $"Bank - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Forename: {rec?.FName}, Surname : {rec?.LName}, Town : {rec?.Town}";
			//	}
			//	if (tuple.Item2 == "DETAILS")
			//	{
			//		MainWindow.gv.Datagrid[x] = tuple.Item1.BankGrid;
			//		MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
			//		MainWindow.gv.Datagrid[x] = DetailsGrid;
			//		if (DetailsGrid.SelectedIndex <= 0)
			//		{
			//			DetailsGrid.SelectedIndex = 0;
			//			DetailsGrid.SelectedItem = 0;
			//		}
			//		var rec = tuple.Item1.DetailsGrid.SelectedItem as DetailsViewModel;
			//		MainWindow.gv.PrettyDetails = "Not Know as yet...";
			//		if (rec != null)
			//		{       // This gets the full record data to \add to the Viewers List
			//			MainWindow.gv.PrettyDetails = $"Bank - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Balance £ {rec?.Balance}, Interest {rec?.IntRate}%";
			//		}
			//	}
			//	// Save  the current viewers "Handle"
			//	MainWindow.gv.window[x] = this;
			//	MainWindow.gv.ViewerCount++;
			//}
		}

		/// <summary>
		/// Fjunction that create a fully populated Tuple from the current DataGrid and Viewer Window
		/// Content of Tuple is : ( this, string "currentDb", int selectedIndex, , int Tag, object (a datarecord basically) selectedItem)
		/// </summary>
		/// <param name="currentDb" is the current viewer type identifier  eg"CUSTOMER"></param>

		//********************************************//
		//Receiver for messages FROM DbSelector
		/// <summary>
		/// Good example of how to pass Tuples around
		/// </summary>
		/// <param name="tuple"></param>
		public void GetTupleData (Tuple<SqlDbViewer, string, int> tuple)
		{
			//content of Tuple is : (This, string "currentDb", int selectedIndex, , int Tag, object selectedItem)
			if (tuple.Item2 == "BANKACCOUNT")
			{
			}
			else if (tuple.Item2 == "CUSTOMER")
			{
			}
			else if (tuple.Item2 == "DETAILS")
			{
			}

		}

		#endregion Tuple Handlers

		#region Focus handling
		private void BankGrid_GotFocus (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;
			Flags.SetGridviewControlFlags (this, this.BankGrid);
		}

		private void CustomerGrid_GotFocus (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;
			Flags.SetGridviewControlFlags (this, this.CustomerGrid);
		}

		private void DetailsGrid_GotFocus (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;
			Flags.SetGridviewControlFlags (this, this.DetailsGrid);
		}
		#endregion Focus handling

		static void HandleEdit (object sender, EditEventArgs e)
		{
			//Handler for Datagrid Edit occurred delegate
			if (Flags.EventHandlerDebug)
			{
				Console.WriteLine ($"\r\nRecieved by SQLDBVIEWER (150) Caller={e.Caller}, Index = {e.CurrentIndex},  Grid = {e.ToString ()}\r\n ");
			}
		}

		//*****************************************************************************************//
		/// <summary>
		/// handle clearing down the data to allow a switch to a different Db view
		/// </summary>
		private void ClearCurrentGridData ()
		{
			if (CurrentDb == "BANKACCOUNT")
			{
				this.BankGrid.ItemsSource = null;
				BankAccountViewModel.BankList?.Clear ();
				bvm.BankAccountObs?.Clear ();
				this.BankGrid.Items.Clear ();
				dtBank.Clear ();
				this.BankGrid.ItemsSource = bvm.BankAccountObs;
			}
			else if (CurrentDb == "CUSTOMER")
			{

				CustomerGrid.ItemsSource = null;
				//				CustomerViewModel.CustomersList?.Clear ();
				cvm.CustomersObs?.Clear ();
				CustomerGrid.Items.Clear ();
				dtCust.Clear ();
				CustomerGrid.ItemsSource = cvm.CustomersObs;
			}
			else if (CurrentDb == "DETAILS")
			{
				DetailsGrid.ItemsSource = null;
				dvm.DetailsObs?.Clear ();
				DetailsGrid.Items.Clear ();
				if (dtDetails != null)
					dtDetails.Clear ();
				DetailsGrid.ItemsSource = dvm.DetailsObs;
			}
			//dgControl.SelectedGrid = null;
			//dgControl.SelectedIndex = 0;
			//dgControl.SelectedItem = null;
		}

		private void ClearCollectionsData (string Caller)
		//NOT USED
		{
			//Warning - pointers are ALL WRONG here !!!!!   3/4/21

			BankAccountViewModel.dtBank.Clear ();
			CustomerViewModel.dtCust.Clear ();
			DetailsViewModel.dtDetails.Clear ();
			if (Caller == "BANKACCOUNT")
			{
				if (cvm.CustomersObs != null) cvm.CustomersObs.Clear ();
				if (dvm.DetailsObs != null) dvm.DetailsObs.Clear ();
				//				CustomerViewModel.CustomersList.Clear ();
				//				DetailsViewModel.DetailsList.Clear ();
				dvm.DetailsObs.Clear ();
			}
			if (Caller == "CUSTOMER")
			{
				if (bvm.BankAccountObs != null) bvm.BankAccountObs.Clear ();
				if (cvm.CustomersObs != null) cvm.CustomersObs.Clear ();
				BankAccountViewModel.BankList.Clear ();
				dvm.DetailsObs.Clear ();
			}
			if (Caller == "DETAILS")
			{
				if (bvm.BankAccountObs != null) bvm.BankAccountObs.Clear ();
				if (dvm.DetailsObs != null) dvm.DetailsObs.Clear ();
				//				CustomerViewModel.CustomersList.Clear ();
				BankAccountViewModel.BankList.Clear ();
			}
		}

		//*****************************************************************************************//
		public async void ViewerGrid_RowEditEnding (object sender, DataGridRowEditEndingEventArgs e)
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
			if (!SelectionhasChanged)
				return;
			else
				SelectionhasChanged = false;    // clear the edit status flag again

			SendDbSelectorCommand (103, $"CHANGES made to data in {CurrentDb} Db)", Flags.CurrentSqlViewer);


			Mouse.OverrideCursor = Cursors.Wait;
			//Only called whn an edit has been completed
			if (e == null)
			{
				SQLHandlers sqlh = new SQLHandlers ();
				Task t1;
				if (CurrentDb == "BANKACCOUNT")
				{
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel.SqlUpdating = true;
					t1 = await Task.Factory.StartNew (() => sqlh.UpdateDbRow ("BANKACCOUNT", (object)ss));
					ss = BankCurrentRow.Item as BankAccountViewModel;
				}
				else if (CurrentDb == "DETAILS")
				{
					t1 = await Task.Factory.StartNew (() => sqlh.UpdateDbRow ("DETAILS", (object)sa));
					sa = DetailsCurrentRow.Item as DetailsViewModel;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					t1 = await Task.Factory.StartNew (() => sqlh.UpdateDbRow ("CUSTOMER", (object)cs));
					cs = CustomerCurrentRow.Item as CustomerViewModel;
				}
			}
			else
			{
				SQLHandlers sqlh = new SQLHandlers ();
				if (CurrentDb == "BANKACCOUNT")
				{
					ss = new BankAccountViewModel ();
					ss = e.Row.Item as BankAccountViewModel;
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel.SqlUpdating = true;
//					Task t1 = await Task.Factory.StartNew (() => sqlh.UpdateDbRow ("BANKACCOUNT", (object)ss)
//					);
					{
						//Task t1 = Task.Factory.StartNew (
						//	async () => await SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row));
						//t1.Wait ();

						//Task[ ] tasks = new Task[1];
						//tasks[0] = Task.Factory.StartNew (() => SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row));
						//// Wait for the background task to finish
						//tasks[0].Wait ();
					}
					
					await sqlh.UpdateDbRow("BANKACCOUNT", (object) ss);
					//Console.WriteLine ($"t1 result = {t1.IsCompleted}");
					//					Thread.Sleep (500);
					SendDataChanged (BankGrid, CurrentDb);
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
				}
				else if (CurrentDb == "DETAILS")
				{
					sa = new DetailsViewModel ();
					sa = e.Row.Item as DetailsViewModel;
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel.SqlUpdating = true;
//					Task t1 = await Task.Factory.StartNew (() => sqlh.UpdateDbRow ("DETAILS", (object)sa));
					await sqlh.UpdateDbRow ("DETAILS", (object)sa);
					//Console.WriteLine ($"t1 result = {t1.IsCompleted}");
					SendDataChanged (DetailsGrid, CurrentDb);
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					cs = new CustomerViewModel ();
					cs = e.Row.Item as CustomerViewModel;
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel.SqlUpdating = true;
//					Task t1 = await Task.Factory.StartNew (() => sqlh.UpdateDbRow ("CUSTOMER", (object)cs));
					await sqlh.UpdateDbRow ("CUSTOMER", (object)cs);
					//Console.WriteLine ($"t1 result = {t1.IsCompleted}");
					SendDataChanged (CustomerGrid, CurrentDb);
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
					{
						//await SQLHandlers.UpdateDbRow ("CUSTOMER", e.Row);
						//Task t1 = Task.Factory.StartNew (
						//	async () => await SQLHandlers.UpdateDbRow ("CUSTOMER", e.Row));
						//t1.Wait ();


						//Task[ ] tasks = new Task[1];
						//tasks[0] = Task.Factory.StartNew (() => SQLHandlers.UpdateDbRow ("CUSTOMER", e.Row));
						//// Wait for the background task to finish
						//tasks[0].Wait ();
					}
				}
			}

			if (CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS")
			{
				SqlCommand cmd = null;
				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					decimal Y;
					if (CurrentDb == "BANKACCOUNT")
					{
						//						ss = e.Row.Item as BankAccount;
						x = Convert.ToInt32 (ss.Id);
						x = Convert.ToInt32 (ss.AcType);
						//Check for invalid A/C Type
						if (x < 1 || x > 4)
						{
							Console.WriteLine ($"SQL Invalid A/c type of {ss.AcType} in grid Data");
							Mouse.OverrideCursor = Cursors.Arrow;
							MessageBox.Show ($"Invalid A/C Type ({ss.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
							return;
						}
						Y = Convert.ToDecimal (ss.Balance);
						Y = Convert.ToDecimal (ss.IntRate);
						//Check for invalid Interest rate
						if (Y > 100)
						{
							Console.WriteLine ($"SQL Invalid Interest Rate of {ss.IntRate} > 100% in grid Data");
							Mouse.OverrideCursor = Cursors.Arrow;
							MessageBox.Show ($"Invalid Interest rate ({ss.IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!");
							return;
						}
						DateTime dtm = Convert.ToDateTime (ss.ODate);
						dtm = Convert.ToDateTime (ss.CDate);
					}
					else if (CurrentDb == "DETAILS")
					{
						//						sa = sacc;
						//						sa = e.Row.Item as DetailsViewModel;
						x = Convert.ToInt32 (sa.Id);
						x = Convert.ToInt32 (sa.AcType);
						//Check for invalid A/C Type
						if (x < 1 || x > 4)
						{
							Console.WriteLine ($"SQL Invalid A/c type of {sa.AcType} in grid Data");
							Mouse.OverrideCursor = Cursors.Arrow;
							MessageBox.Show ($"Invalid A/C Type ({sa.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
							return;
						}
						Y = Convert.ToDecimal (sa.Balance);
						Y = Convert.ToDecimal (sa.IntRate);
						//Check for invalid Interest rate
						if (Y > 100)
						{
							Console.WriteLine ($"SQL Invalid Interest Rate of {sa.IntRate} > 100% in grid Data");
							Mouse.OverrideCursor = Cursors.Arrow;
							MessageBox.Show ($"Invalid Interest rate ({sa.IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!");
							return;
						}
						DateTime dtm = Convert.ToDateTime (sa.ODate);
						dtm = Convert.ToDateTime (sa.CDate);
					}
					//					string sndr = sender.ToString();
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Invalid grid Data - {ex.Message} Data = {ex.Data}");
					Mouse.OverrideCursor = Cursors.Arrow;
					MessageBox.Show ("Invalid data entered in the Grid !!!! - See Output for details");
					return;
				}
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection (ConString);
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using (con)
					{
						con.Open ();

						if (CurrentDb == "BANKACCOUNT")
						{
							cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (ss.Id));
							cmd.Parameters.AddWithValue ("@bankno", ss.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", ss.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (ss.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (ss.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (ss.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (ss.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (ss.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of BankAccounts successful...");

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of SecAccounts successful...");

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of Customers successful...");
						}
						else if (CurrentDb == "DETAILS")
						{
							cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of BankAccounts successful...");

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of SecAccounts successful...");

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of customers successful...");
						}
						if (CurrentDb == "SECACCOUNTS")
						{
							cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (ss.Id));
							cmd.Parameters.AddWithValue ("@bankno", ss.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", ss.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (ss.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (ss.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (ss.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (ss.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (ss.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of BankAccounts successful...");

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of SecAccounts successful...");

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							await cmd.ExecuteNonQueryAsync ();
							Console.WriteLine ("SQL Update of Customers successful...");
						}
						StatusBar.Text = "Database updated successfully....";
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error - {ex.Message} Data = {ex.Data}");

#if SHOWSQLERRORMESSAGEBOX
					Mouse.OverrideCursor = Cursors.Arrow;
					MessageBox.Show ("SQL error occurred - See Output for details");
#endif
				}
				finally
				{
					Mouse.OverrideCursor = Cursors.Arrow;
					con.Close ();
				}
			}
			else if (CurrentDb == "CUSTOMER")
			{
				if (e == null && CurrentDb == "CUSTOMER")
					cs = CustomerCurrentRow.Item as CustomerViewModel;
				else if (e == null && CurrentDb == "CUSTOMER")
					cs = e.Row.Item as CustomerViewModel;


				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					x = Convert.ToInt32 (cs.Id);
					//					string sndr = sender.ToString();
					x = Convert.ToInt32 (cs.AcType);
					//Check for invalid A/C Type
					if (x < 1 || x > 4)
					{
						Console.WriteLine ($"SQL Invalid A/c type of {cs.AcType} in grid Data");
						Mouse.OverrideCursor = Cursors.Arrow;
						MessageBox.Show ($"Invalid A/C Type ({cs.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
						return;
					}
					DateTime dtm = Convert.ToDateTime (cs.ODate);
					dtm = Convert.ToDateTime (cs.CDate);
					dtm = Convert.ToDateTime (cs.Dob);
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Invalid grid Data - {ex.Message} Data = {ex.Data}");
					MessageBox.Show ("Invalid data entered in the Grid !!!! - See Output for details");
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
				}
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection (ConString);
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using (con)
					{
						con.Open ();
						SqlCommand cmd = new SqlCommand ("UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
							"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
							"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con);

						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@fname", cs.FName.ToString ());
						cmd.Parameters.AddWithValue ("@lname", cs.LName.ToString ());
						cmd.Parameters.AddWithValue ("@addr1", cs.Addr1.ToString ());
						cmd.Parameters.AddWithValue ("@addr2", cs.Addr2.ToString ());
						cmd.Parameters.AddWithValue ("@town", cs.Town.ToString ());
						cmd.Parameters.AddWithValue ("@county", cs.County.ToString ());
						cmd.Parameters.AddWithValue ("@pcode", cs.PCode.ToString ());
						cmd.Parameters.AddWithValue ("@phone", cs.Phone.ToString ());
						cmd.Parameters.AddWithValue ("@mobile", cs.Mobile.ToString ());
						cmd.Parameters.AddWithValue ("@dob", Convert.ToDateTime (cs.Dob));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						await cmd.ExecuteNonQueryAsync ();
						Console.WriteLine ("SQL Update of Customers successful...");

						cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype,  ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						await cmd.ExecuteNonQueryAsync ();
						Console.WriteLine ("SQL Update of BankAccounts successful...");

						cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						await cmd.ExecuteNonQueryAsync ();
						Console.WriteLine ("SQL Update of SecAccounts successful...");

					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error - {ex.Message} Data = {ex.Data}");
#if SHOWSQLERRORMESSAGEBOX
					Mouse.OverrideCursor = Cursors.Arrow;
					MessageBox.Show ("SQL error occurred - See Output for details");
#endif
				}
				finally
				{
					Mouse.OverrideCursor = Cursors.Arrow;
					con.Close ();
				}
				Mouse.OverrideCursor = Cursors.Arrow;
				return;
			}
			Mouse.OverrideCursor = Cursors.Arrow;
			return;
		}
		//*****************************************************************************************//

		public static void UpdateAllOpenViewers ()
		{
			return;
			{
				//			for (int x = 0; x < MainWindow.gv.ViewerCount; x++)
				//			{
				//				if (MainWindow.gv.window[x] != null)
				//				{
				//					string y = MainWindow.gv.CurrentDb[x];
				//					if (y.Contains ("Customer -") || y.Contains("CUSTOMER"))
				//					{
				//						Window w = MainWindow.gv.window[x];
				//						SqlDbViewer sqlv = w as SqlDbViewer;
				//						sqlv.CustomerGrid.ItemsSource = null;
				//						sqlv.CustomerGrid.ItemsSource = cvm.CustomersObs;
				//						CollectionViewSource.GetDefaultView (sqlv.CustomerGrid.ItemsSource).Refresh ();
				////						CustomerViewModel.CustomersObs.

				//						Console.WriteLine ("$$$$$$$$$$$$ Customer Grid Update completed...$$$$$$$$$$$$ ");
				//					}
				//					else if (y.Contains ("Details -") || y.Contains("DETAILS"))
				//					{
				//						Window w = MainWindow.gv.window[x];
				//						SqlDbViewer sqlv = w as SqlDbViewer;
				//						sqlv.DetailsGrid.ItemsSource = null;
				//						sqlv.DetailsGrid.Refresh ();
				//						sqlv.DetailsGrid.ItemsSource = dvm.DetailsObs ;
				//						sqlv.DetailsGrid.Refresh ();
				//						CollectionViewSource.GetDefaultView (sqlv.DetailsGrid.ItemsSource).Refresh ();
				//						Console.WriteLine ("$$$$$$$$$$$$ Secondary Accounts Grid Update completed...$$$$$$$$$$$$ ");
				//					}
				//					else if (y.Contains ("Bank -") || y.Contains("BANKACCOUNT"))
				//					{
				//						Window w = MainWindow.gv.window[x];
				//						SqlDbViewer sqlv = w as SqlDbViewer;
				//						sqlv.BankGrid.ItemsSource = null;
				//						sqlv.BankGrid.ItemsSource = bvm.BankAccountObs ;
				//						CollectionViewSource.GetDefaultView (sqlv.BankGrid.ItemsSource).Refresh ();
				//						Console.WriteLine ("$$$$$$$$$$$$ BankAccount Grid Update completed...$$$$$$$$$$$$ ");
				//					}
				//					MainWindow.gv.Datagrid[x].Refresh ();
				//				}
			}
		}
		//*****************************************************************************************//
		private void CloseViewer_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;

			//Close current (THIS) Viewer Window
			//clear flags in ViewModel
			if (CurrentDb == "BANKACCOUNT")
			{
				RemoveFromViewerList ();
				MainWindow.gv.Bankviewer = Guid.Empty;
				Flags.ActiveSqlDbViewer = null;
			}
			else if (CurrentDb == "CUSTOMER")
			{
				RemoveFromViewerList ();
				MainWindow.gv.Custviewer = Guid.Empty;
				Flags.ActiveSqlDbViewer = null;
			}
			else if (CurrentDb == "DETAILS")
			{
				RemoveFromViewerList ();
				MainWindow.gv.Detviewer = Guid.Empty;
				Flags.ActiveSqlDbViewer = null;
			}
			SendDbSelectorCommand (99, "Window is closing", Flags.CurrentSqlViewer);
			//Set global flags
			EventHandlers.ClearWindowHandles (null, this);
			UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
			BankAccountViewModel.EditdbWndBank = null;
			Flags.CurrentSqlViewer.Tag = Guid.Empty;
			Flags.CurrentSqlViewer = null;
			MainWindow.gv.SqlViewerGuid = Guid.Empty;
			Mouse.OverrideCursor = Cursors.Arrow;
			Close ();
		}
		//*****************************************************************************************//
		/// <summary>
		/// We are loading a Db into a Grid....
		/// Updates the MainWindow.GridViewer structure data, called
		/// by the 3  different "Show xxxxx" Funstion's
		/// </summary>
		/// <param name="type"></param>
		private void UpdateGridviewController (string type)
		{
			//Retrieve Window handle of current Viewer window
			int newindex = -1;

			newindex = MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count - 1;
			if (newindex < 0)
				return;
			if (PrettyDetails != "")
				MainWindow.gv.CurrentDb[newindex] = PrettyDetails;
		}
		//*****************************************************************************************//
		public void UpdateViewersList ()
		{
			if (this.Tag == null) return;
			if (MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count == 1)
				return;
			for (int i = 0; i < MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count; i++)
			{
				if (i + 1 == MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count)
					return;
				if (MainWindow.gv.ListBoxId[i] == (Guid)Flags.CurrentSqlViewer.Tag)
				{
					ListBoxItem lbi = new ListBoxItem ();
					lbi = Flags.DbSelectorOpen.ViewersList.Items[i + 1] as ListBoxItem;
					lbi.Content = MainWindow.gv.CurrentDb[i];
					break;
				}
			}
		}
		//*****************************************************************************************//
		public void RemoveFromViewerList (int x = -1)
		{
			// THIS WORKED   19/4/21
			DeleteViewerAndFlags (x);
			return;

			int viewerEntryCount = 0;
			if (this.Tag == null) return;
			if (MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count == 1)
				return;
			for (int i = 0; i < MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count; i++)
			{
				if (i >= MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count)
					return;
				if (MainWindow.gv.ListBoxId[i] == (Guid)this.Tag)
				{
					//					int currentindex = MainWindow.gv.DbSelectorWindow.ViewersList.SelectedIndex;
					Flags.DbSelectorOpen.ViewersList.Items.RemoveAt (viewerEntryCount);
					break;
				}
				viewerEntryCount++;
			}
			Flags.DbSelectorOpen.ViewersList.Refresh ();
			// If all viewers are closed, tidy up control structure dv[]
			if (MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count == 1)
			{
				MainWindow.gv.PrettyDetails = "";
				MainWindow.gv.SqlViewerWindow = null;
			}
		}
		//*****************************************************************************************//
		private void ParseButtonText (bool obj)
		{
			if (IsFiltered != "")
			{
				if (IsFiltered == "BANKACCOUNT")
				{
					ShowBank.Content = $"(F) Bank A/c's  ({BankGrid.Items.Count})";
					MainWindow.gv.CurrentDb[MainWindow.gv.ViewerCount - 1] = (string)ShowBank.Content;
				}
				else if (IsFiltered == "CUSTOMER")
				{
					ShowCust.Content = $"(F) Customer A/c's  ({CustomerGrid.Items.Count})";
					MainWindow.gv.CurrentDb[MainWindow.gv.ViewerCount - 1] = (string)ShowCust.Content;
				}
				else if (IsFiltered == "DETAILS")
				{
					ShowDetails.Content = $"(F) Details A/c's  ({DetailsGrid.Items.Count})";
					MainWindow.gv.CurrentDb[MainWindow.gv.ViewerCount - 1] = (string)ShowDetails.Content;
				}
			}
			else
			{
				if (CurrentDb == "BANKACCOUNT")
				{
					if (!obj)
					{
						if (isMultiMode)
							ShowBank.Content = $"<M> Bank A/c's  ({this.BankGrid.Items.Count})";
						else
							ShowBank.Content = $"Bank A/c's  ({this.BankGrid.Items.Count})";
					}
					else
						ShowBank.Content = $"Bank A/c's  ({this.BankGrid.Items.Count})";

				}
				else if (CurrentDb == "CUSTOMER")
				{
					if (!obj)
					{
						if (isMultiMode)
							ShowCust.Content = $"<M> Customer A/c's  ({this.CustomerGrid.Items.Count})";
						else
							ShowCust.Content = $"Customer A/c's  ({this.CustomerGrid.Items.Count})";
					}
					else
						ShowCust.Content = $"Customer A/c's  ({this.CustomerGrid.Items.Count})";
				}
				else if (CurrentDb == "DETAILS")
				{
					if (!obj)
					{
						if (isMultiMode)
							ShowDetails.Content = $"<M> Details A/c's  ({this.DetailsGrid.Items.Count})";
						else
							ShowDetails.Content = $"Details A/c's  ({this.DetailsGrid.Items.Count})";
					}
					else
						ShowDetails.Content = $"Details A/c's  ({this.DetailsGrid.Items.Count})";
				}
			}
		}
		//*****************************************************************************************//
		private void ScrollList (DataGrid grid, int count)
		{
			grid.ScrollIntoView (grid.SelectedItem);
			var border = VisualTreeHelper.GetChild (grid, 0) as Decorator;

			if (border == null) return;
			var scroll = border.Child as ScrollViewer;

			if (scroll == null) return;

			scroll.UpdateLayout ();
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


		//*****************************************************************************************//
		private void BankGrid_DataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			int x = 1;
		}

		//*****************************************************************************************//
		private void UpdateRowDetails (object datarecord, string caller)
		// This updates the data in the DbSelector window's Viewers listbox, or add a new entry ????
		{
			bool Updated = false;
			if (this.Tag == null) return;
			for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			{
				if (x >= Flags.DbSelectorOpen.ViewersList.Items.Count) break;
				if (MainWindow.gv.ListBoxId[x] == (Guid)this.Tag)
				{
					if (caller == "BankGrid")
					{
						//	BankAccountViewModel record = (BankAccountViewModel)datarecord;
						var record = datarecord as BankAccountViewModel;//CurrentBankSelectedRecord;
						MainWindow.gv.CurrentDb[x] = $"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, Balance £ {record?.Balance}, Interest {record?.IntRate}%";
						PrettyDetails = MainWindow.gv.CurrentDb[x];
						MainWindow.gv.PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						//						UpdateDbSelectorItem (PrettyDetails);
					}
					else if (caller == "CustomerGrid")

					{
						var record = datarecord as CustomerViewModel;
						MainWindow.gv.CurrentDb[x] = $"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, {record?.LName} {record?.Town}, {record?.County}";
						PrettyDetails = MainWindow.gv.CurrentDb[x];
						MainWindow.gv.PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
#pragma NOT NEEDED????
						//						UpdateDbSelectorItem (PrettyDetails);
					}
					else if (caller == "DetailsGrid")
					{
						var record = datarecord as DetailsViewModel;
						MainWindow.gv.CurrentDb[x] = $"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, Balance {record?.Balance}, Interest % {record?.IntRate}";
						PrettyDetails = MainWindow.gv.CurrentDb[x];
						MainWindow.gv.PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						//						UpdateDbSelectorItem (PrettyDetails);
					}
					break;
				}
			}
			if (!Updated)
			{
				if (Notifier != null)
					Notifier (111, "UpdateRowDetails has NOT updated anything ???", null);
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
		//*****************************************************************************************//
		private void OutputGridviewDataToConsole (int x)
		{
			//Console.WriteLine ($"GridView Data =  {MainWindow.gv.CurrentDb[x]}, \r\n" +
			//	$"SelectedViewerType={MainWindow.gv.SelectedViewerType}, ViewerSelectionType={MainWindow.gv.ViewerSelectiontype}" +
			//	$", ChosenViewer = {MainWindow.gv.ChosenViewer}, DataContext = {MainWindow.gv.DbSelectorWindow}.");
		}
		//*****************************************************************************************//
		private void DetailsGrid_LoadingRowDetails (object sender, DataGridRowDetailsEventArgs e)
		{
			// row data Loading ???
			MainWindow.gv.Datagrid[LoadIndex] = this.DetailsGrid;
		}

		//*****************************************************************************************//
		private void BankGrid_LoadingRowDetails (object sender, DataGridRowDetailsEventArgs e)
		{
			// row data Loading ???
			MainWindow.gv.Datagrid[LoadIndex] = this.BankGrid;

		}

		//*****************************************************************************************//
		private void CustomerGrid_SourceUpdated (object sender, System.Windows.Data.DataTransferEventArgs e)
		{
			// row data Loading ???
			MainWindow.gv.Datagrid[LoadIndex] = this.BankGrid;

			if (this.CustomerGrid.SelectedItem != null)
			{
				if (this.CustomerGrid.SelectedItem == null)
					return;
				TextBlock tb = new TextBlock ();

				//This gives me an entrie Db Record in "c"
				CustomerViewModel c = this.CustomerGrid.SelectedItem as CustomerViewModel;

			}
		}

		private void CustomerGrid_TargetUpdated (object sender, System.Windows.Data.DataTransferEventArgs e)
		//*****************************************************************************************//
		{
			// row data Loading ???
			MainWindow.gv.Datagrid[LoadIndex] = this.CustomerGrid;
			this.CustomerGrid.SelectedIndex = 0;
			SelectedRow = 0;

		}

		//*****************************************************************************************//
		private void CustomerGrid_Initialized (object sender, EventArgs e)
		{
			int x = 0;
		}

		//*****************************************************************************************//
		private void Window_GotFocus (object sender, RoutedEventArgs e)
		{
			// Actually, this is Called mostly by MouseDown Handler
			//when Focus has been set to this window
			//Set global flags

			if (CurrentDb == "BANKACCOUNT")
				Flags.SetGridviewControlFlags (this, this.BankGrid);
			else if (CurrentDb == "CUSTOMER")
				Flags.SetGridviewControlFlags (this, this.CustomerGrid);
			else if (CurrentDb == "DETAILS")
				Flags.SetGridviewControlFlags (this, this.DetailsGrid);

			Guid tag = Guid.Empty;
			tag = (Guid)this.Tag;
			for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			{
				// find Tag that matches our Tag in ViewersList  
				if (MainWindow.gv.ListBoxId[x] == tag)
				{
					Flags.DbSelectorOpen.ViewersList.SelectedIndex = x + 1;
					Flags.DbSelectorOpen.ViewersList.Refresh ();
					break;
				}
			}
		}

		/// <summary>
		///  Function handles TWO seperate functions
		///  1 - Clear the entry MainWindow.gv[]
		///  2 - Remove correct line form DbSelector.ViewersList
		/// </summary>
		public static bool DeleteViewerAndFlags (int x = -1)
		{
			SqlDbViewer sqlv;                        // x = GridView[] index if received
			Guid tag = (Guid)Flags.CurrentSqlViewer.Tag;
			ListBoxItem lbi = new ListBoxItem ();
			if (x != -1)
			{
				// we have received the index of the viewer in the list
				// so  get the Tag of that selected Entry
				lbi = Flags.DbSelectorOpen.ViewersList.Items[x] as ListBoxItem;
				Guid lbtag = (Guid)lbi.Tag;
				// Get a pointer to the window so we can close it
				sqlv = Flags.CurrentSqlViewer as SqlDbViewer;
				//See if it matches the one we are closing down
				if ((Guid)lbtag == (Guid)tag)
				{

					// We know which gv[] entry  we need to clear, so do it and return
					MainWindow.gv.ViewerCount--;
					MainWindow.gv.CurrentDb[x] = "";
					MainWindow.gv.ListBoxId[x] = Guid.Empty;
					MainWindow.gv.Datagrid[x] = null;
					// Actually close thre Viewer window here, before we delete the relevant pointers
					sqlv.Close ();
					MainWindow.gv.window[x] = null;
				}
				MainWindow.gv.PrettyDetails = "";
				MainWindow.gv.SqlViewerGuid = Guid.Empty;
				//Finally we can remove this entry from ViewersList
				lbi = Flags.DbSelectorOpen.ViewersList.Items[x] as ListBoxItem;
				lbi.Content = "";
				Flags.DbSelectorOpen.ViewersList.Items.RemoveAt (x);
				// Set selectedIndex pointer to current position in list
				int currentIndex = x - 1;
				if (x <= 1)             // List is basically empty (No viewers in  the list)
					return true;
				if (Flags.DbSelectorOpen.ViewersList.Items.Count > currentIndex)
				{
					Flags.DbSelectorOpen.ViewersList.SelectedIndex = currentIndex;
					Flags.DbSelectorOpen.ViewersList.SelectedItem = currentIndex;
				}
				else if (Flags.DbSelectorOpen.ViewersList.Items.Count == currentIndex)
				{
					Flags.DbSelectorOpen.ViewersList.SelectedIndex = currentIndex - 1;
					Flags.DbSelectorOpen.ViewersList.SelectedItem = currentIndex - 1;
				}
				sqlv.Close ();
				return true;
			}
			// Now sort out the  global gv[] flags
			for (int y = 1; y < Flags.DbSelectorOpen.ViewersList.Items.Count; y++)
			{
				// Get the Tag of eaxch Viewer in the list
				lbi = Flags.DbSelectorOpen.ViewersList.Items[y] as ListBoxItem;
				Guid lbtag = (Guid)lbi.Tag;
				//See if it matches the one we are closing down
				if ((Guid)lbtag == (Guid)tag)
				{
					//Yes, we have got a match, so go ahead and remove its gv[] entries first
					for (int z = 0; z < MainWindow.gv.MaxViewers; z++)
					{
						if (MainWindow.gv.ListBoxId[z] == lbtag)
						{
							MainWindow.gv.ViewerCount--;
							MainWindow.gv.CurrentDb[z] = "";
							MainWindow.gv.ListBoxId[z] = Guid.Empty;
							MainWindow.gv.Datagrid[z] = null;
							MainWindow.gv.window[z] = null;
							break;
						}

					}
					MainWindow.gv.PrettyDetails = "";
					//Finally we can remove this entry from ViewersList
					lbi = Flags.DbSelectorOpen.ViewersList.Items[y] as ListBoxItem;
					lbi.Content = "";
					Flags.DbSelectorOpen.ViewersList.Items.RemoveAt (y);
					// Set selectedIndex pointer to current position in list
					int currentIndex = y - 1;
					if (y <= 1)             // List is basically empty (No viewers in  the list)
						return true;
					if (Flags.DbSelectorOpen.ViewersList.Items.Count > currentIndex)
					{
						Flags.DbSelectorOpen.ViewersList.SelectedIndex = currentIndex;
						Flags.DbSelectorOpen.ViewersList.SelectedItem = currentIndex;
					}
					else if (Flags.DbSelectorOpen.ViewersList.Items.Count == currentIndex)
					{
						Flags.DbSelectorOpen.ViewersList.SelectedIndex = currentIndex - 1;
						Flags.DbSelectorOpen.ViewersList.SelectedItem = currentIndex - 1;
					}
					return true;
				}
			}
			MainWindow.gv.SqlViewerGuid = Guid.Empty;

			return false;
		}
		//*****************************************************************************************//
		private void Window_Closed (object sender, EventArgs e)
		{
			//			SqlDbViewer sqlv = this;
			//			DeleteViewerAndFlags ();
			// clear final Viewer pointer when ALL viewers are closed down
			if (MainWindow.gv.ViewerCount == 0)
				MainWindow.gv.SqlViewerWindow = null;
			UpdateDbSelectorBtns (Flags.CurrentSqlViewer);

			//			Guid tag = (Guid)this.Tag;

			//			if (Flags.DbSelectorOpen == null)
			//				return;
			//			ListBoxItem lbi = new ListBoxItem ();
			//			for (int y = 0; y < Flags.DbSelectorOpen.ViewersList.Items.Count; y++)
			//			{
			//				lbi = Flags.DbSelectorOpen.ViewersList.Items[y + 1] as ListBoxItem;
			//				Guid lbtag = (Guid)lbi.Tag;
			//				if (lbtag == tag)
			//				{
			//					for(int z = 0; z <  MainWindow.gv.MaxViewers; z++)
			//					{
			//						if(MainWindow.gv.ListBoxId[z] == lbtag)
			//						{
			//							MainWindow.gv.ViewerCount--;
			//							MainWindow.gv.CurrentDb[z] = "";
			//							MainWindow.gv.ListBoxId[z] = Guid.Empty;
			//							MainWindow.gv.Datagrid[z] = null;
			//							MainWindow.gv.window[z] = null;
			//							break;
			//						}
			//					}
			//					//Remove this entry from ViewersList
			//#pragma   TODO Chosenviewer
			//					Flags.DbSelectorOpen.ViewersList.Items.RemoveAt (y + 1);
			//					break;
			//				}
			//			}
			//			// clear final Viewer poinmter when ALL viewers are closed down
			//			if (MainWindow.gv.ViewerCount == 0)
			//				MainWindow.gv.SqlViewerWindow = null;
			//			UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
			// THIS IS  THE LAST LINE WHEN CLOSING A VIEWER
		}

		//*****************************************************************************************//
		private void Minimize_click (object sender, RoutedEventArgs e)
		{
			Window_MouseDown (sender, null);
			this.WindowState = WindowState.Normal;
		}
		//*****************************************************************************************//
		private void Window_LayoutUpdated (object sender, EventArgs e)
		{
			//Prevent window from being maximized
			Window w = new Window ();
			w = sender as Window;
			if (w.WindowState == WindowState.Maximized)
				w.WindowState = WindowState.Normal;
		}

		//*****************************************************************************************//
		public void CreateListboxItemBinding ()
		{
			Binding binding = new Binding ("ListBoxItemText");
			binding.Source = PrettyDetails;
		}

		private void CustomerGrid_CurrentCellChanged (object sender, EventArgs e)
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

		private void CustomerGrid_PreparingCellForEdit (object sender, DataGridPreparingCellForEditEventArgs e)
		{
			if (CustomerGrid.SelectedItem != null)
			{
				//if (CustomerGrid.SelectedItem == null)
				//	return;
				//TextBlock tb = new TextBlock ();

				////This gives me an entrie Db Record in "c"
				//Customer c = CustomerGrid.SelectedItem as Customer;
				//Console.WriteLine ($"CustomerGrid_PreparingCellForEdit - Identified row data of [{c.CustNo} {c.FName} {c.LName}]");

			}

		}
		/// <summary>
		/// Updates the text of the relevant ViewersList entry when selection is changed
		/// </summary>
		/// <param name="data"></param>
		public static void UpdateDbSelectorItem (string data)
		{
			//			bool IsAdded = false;
			if (Flags.CurrentSqlViewer.Tag is null) return;
			// handle global flag to control viewer addition/updating
			if (Flags.SqlViewerIsLoading) return;
			//			else
			//				Flags.SqlViewerIsLoading = true;

			Guid tag = (Guid)Flags.CurrentSqlViewer.Tag;

			ListBoxItem lbi;// = new ListBoxItem ();
			if (MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count > 1)
			{
				for (int i = 1; i < MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count; i++)
				{
					lbi = Flags.DbSelectorOpen.ViewersList.Items[i] as ListBoxItem;
					//We start loop at ONE, so need ot use index MINUS 1 to access gv[] data correctly
					Guid lbtag = MainWindow.gv.ListBoxId[i - 1];
					if (lbtag == tag)
					{
						//got the matching entry, update its "Content" field
						Flags.DbSelectorOpen.ListBoxItemText = data;
						lbi.Content = data;
						Flags.DbSelectorOpen.ViewersList.Refresh ();
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

		private void Edit_Click (object sender, RoutedEventArgs e)
		{
			// Make sure this window has it's pointer "Registreded" cos we can 
			// Click the button before the window has had focus set
			Flags.CurrentSqlViewer = this;
			// Open Edit Window for the current record in SqlDbViewer DataGrid
			if (CurrentDb == "BANKACCOUNT")
			{
				EditDb edb = new EditDb ("BANKACCOUNT", this.BankGrid.SelectedIndex, this.BankGrid.SelectedItem, this);
				BankAccountViewModel.EditdbWndBank = edb;
				edb.Owner = this;
				//				if (BankGrid.SelectedIndex >= 0)
				//				{
				//					edb.DataContext = BankGrid.SelectedItem;
				edb.Show ();
				ExtensionMethods.Refresh (edb);

				EditDataGrid = edb.DataGrid1;
				//				}
			}
			else if (CurrentDb == "CUSTOMER")
			{
				EditDb edb = new EditDb ("CUSTOMER", this.CustomerGrid.SelectedIndex, this.CustomerGrid.SelectedItem, this);
				BankAccountViewModel.EditdbWndBank = edb;
				edb.Owner = this;
				//				if (CustomerGrid.SelectedIndex >= 0)
				//				{
				//edb.DataContext = CustomerGrid.SelectedItem;
				edb.Show ();
				ExtensionMethods.Refresh (edb);
				EditDataGrid = edb.DataGrid2;
				//				}
			}
			else if (CurrentDb == "DETAILS")
			{
				EditDb edb = new EditDb ("DETAILS", this.DetailsGrid.SelectedIndex, this.DetailsGrid.SelectedItem, this);
				BankAccountViewModel.EditdbWndBank = edb;
				edb.Owner = this;
				//				if (DetailsGrid.SelectedIndex >= 0)
				//				{
				//					edb.DataContext = DetailsGrid.SelectedItem;
				edb.Show ();
				ExtensionMethods.Refresh (edb);
				EditDataGrid = edb.DataGrid1;
				//				}
			}
		}

		public void EditDbClosing ()
		{
			BankAccountViewModel.EditdbWndBank = null;
		}
		private void DoDragMove ()
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....
			try
			{
				this.DragMove ();
			}
			catch
			{
				return;
			}
		}

		#region DATA LOADING FUNCTIONS
		//*******************************************************************************************************//
		public async void LoadBankData ()
		{
			Debug.WriteLine ($"LoadDetailsData callback has been called successfully\r\nNow loading data into system....");
			//Make the List<> data Observable
			bvm.BankAccountObs = new ObservableCollection<BankAccountViewModel> (BankAccountViewModel.BankList);
			// Refresh grid ???
			Debug.WriteLine ($"Callback has Finished loading data into system....");
			FinishBankWindowLoad ();
			this.BankGrid.ItemsSource = bvm.BankAccountObs;
			Mouse.OverrideCursor = Cursors.Arrow;
		}
		//*******************************************************************************************************//
		public async void LoadCustData (int mode = -1)
		{
			SendDbSelectorCommand (102, ">>> SqlDbViewer in LoadCustData()", Flags.CurrentSqlViewer);

			//Handles the complete sql data Loading, adding to List, Obs and assigning it to the grid
			Debug.WriteLine ($"LoadCustData callback has been called successfully\r\nNow loading data into system....");
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (102, $">>> SqlDbViewer calling TASK LoadCustomersTask", Flags.CurrentSqlViewer);
			// load data into DataTable
			await Task.Factory.StartNew (() =>
			{
				Dispatcher.Invoke (() =>
				{
					cvm.LoadCustomerTaskInSortOrder (-1);
				});
			});
			Debug.WriteLine ($"returned from calling Task to load the data....");

			// Bind our Grid to the data = not there yet in new version 
			//			CustomerGrid.ItemsSource = CustomerViewModel.CustomersObs;
			Mouse.OverrideCursor = Cursors.Arrow;
			//*****************************************************************************//
			// We now have ALL data into the Observable Collection
			//*****************************************************************************//
			this.CustomerGrid.Visibility = Visibility.Visible;
			this.BringIntoView ();
			//Use Delegate to notify DbSelector
			SendDbSelectorCommand (103, $"<<< SqlDbViewer Exiting LoadCustData", Flags.CurrentSqlViewer);
		}
		//*******************************************************************************************************//
		public void LoadDetailsData ()
		{
			Debug.WriteLine ($"LoadDetailsData callback has been called successfully\r\nNow loading data into system....");
			dvm.LoadDetailsTaskInSortOrder (-1);
			if (dvm.DetailsObs.Count == 0)
				dvm.LoadDetailsObsCollection ();
			// Now our List<DetailsViewModel is populated
			//Make the List<> data Observable
			//			dvm.DetailsObs = new ObservableCollection<DetailsViewModel> (DetailsViewModel.DetailsList);
			//			dvm.LoadDetailsObsCollection ();
			// Refresh grid ???
			Debug.WriteLine ($"Callback has Finished loading data into system....");
			//			UpdateAuxilliaries ("Details  Data Loaded...");
			this.DetailsGrid.ItemsSource = dvm.DetailsObs;

			//			StatusBar.Text = "";
			FinishDetWindowLoad ();
			Mouse.OverrideCursor = Cursors.Arrow;
		}
		//*******************************************************************************************************//
		#endregion DATA LOADING FUNCTIONS

		public void FinishBankWindowLoad ()
		{
			Flags.ActiveSqlDbViewer = BankGrid;
			//Setup global flags
#pragma FLAGS
			Flags.SetGridviewControlFlags (this, BankGrid);
		}
		public void FinishCustWindowLoad ()
		{
			Flags.ActiveSqlDbViewer = CustomerGrid;
			//Setup global flags
#pragma FLAGS
			Flags.SetGridviewControlFlags (this, CustomerGrid);
		}

		public void FinishDetWindowLoad ()
		{
			Flags.ActiveSqlDbViewer = DetailsGrid;
			//Setup global flags
#pragma FLAGS
			Flags.SetGridviewControlFlags (this, DetailsGrid);
		}
		//*******************************************************************************************************//

		private void TextBlock_RequestBringIntoView (object sender, RequestBringIntoViewEventArgs e)
		{
			//			this.Show ();
			//			this.BringIntoView ();
		}
		//*******************************************************************************************************//
		public void FinaliseBankLoad (string comment)
		{
			//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
			//			Flags.CurrentEditDbViewerBankGrid = BankGrid;
			Flags.ActiveSqlDbViewer = BankGrid;

			this.BankGrid.ItemsSource = bvm.BankAccountObs;
			this.BankGrid.Visibility = Visibility.Visible;
			MainWindow.gv.Datagrid[LoadIndex] = this.BankGrid;
			//just update the Buttons text
			ParseButtonText (true);
			IsFiltered = "";
			//set filter/Multi buttons ++
			ResetOptionButtons ();
			UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
			//Update the DbSelector listboxitems structure and our GridViewer control Structure
			if (IsViewerLoaded == false)
				UpdateGridviewController ("BANKACCOUNT");
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			this.BankGrid.SelectedIndex = 0;
			if (this.BankGrid.CurrentItem == null)
			{
				this.BankGrid.CurrentItem = 0;
				//dgControl.SelectedIndex = 0;
				//dgControl.SelectedItem = 0;
				//dgControl.SelectedGrid = null;
			}
			this.BankGrid.SelectedItem = 0;
			UpdateViewersList ();
			CollectionViewSource.GetDefaultView (this.BankGrid.ItemsSource).Refresh ();
			//Set global flags
			Flags.SetGridviewControlFlags (this, this.BankGrid);
			StatusBar.Text = comment;
		}
		//*******************************************************************************************************//
		private void ItemsView_OnSelectionChanged (object sender, SelectionChangedEventArgs e)
		//User has clicked a row in our DataGrid OR in EditDb grid
		{
			//declare vars to hold item data from relevant Classes
			// Get pointer to the Datagrid
			//			var datagrid = Flags.CurrentSqlViewer.;
			var datagrid = sender as DataGrid;
			if (datagrid == null) return;

			//if (datagrid.SelectedIndex == -1)
			//	Debug.WriteLine ($"\n******** Current Grid selection clicked on is\n[{datagrid.Name.ToString ()}]\nSelectedIndex IS NOT YET SET.... ");
			//else
			//{
			//	Debug.WriteLine ($"\n******** Current Grid selection clicked on is\n[{datagrid.Name.ToString ()}]\nSelectedIndex = {datagrid.SelectedIndex}\n{datagrid.SelectedItem.ToString ()}\n");
			//}

			//Get the NEW selected index
			int index = (int)datagrid.SelectedIndex;
			//			if (index == -1)
			//				{ Console.WriteLine ($"\nOnSelectionChanged ERROR - SelectedIndex == -1 in {datagrid.Name}\n"); return; }
			//			else
			//			Console.WriteLine ($"\nOnSelectionChanged - New SelectedIndex == {datagrid.SelectedIndex} in {datagrid.Name}\r\n");
			Flags.isEditDbCaller = false;

			if (BankAccountViewModel.EditdbWndBank != null)
			{
				//There is an EditDb window open, so this will trigger 
				//an event that lets the DataGrid in the EditDb class
				// change it's own index internally
				if (CurrentDb == "BANKACCOUNT")
				{
					if (EventHandler != null)
					{
						//Set global  of the ID we want to find
						BankAccountViewModel.CurrentSelectedIndex = index;
						//dgControl.SelectedIndex = index;
						//dgControl.SelectedItem = BankGrid?.SelectedItem;
						//dgControl.SelectedGrid = BankGrid;
						EventHandler.SqlDbTriggerEvent (Flags.isEditDbCaller, index, BankGrid, BankAccountViewModel.SqlUpdating);
					}
				}
				else if (CurrentDb == "CUSTOMER")
				{
					if (EventHandler != null)
					{
						//Set global  of the ID we want to find
						BankAccountViewModel.CurrentSelectedIndex = index;
						EventHandler.SqlDbTriggerEvent (Flags.isEditDbCaller, index, CustomerGrid, CustomerViewModel.SqlUpdating);
						//MainWindow.DgControl.SelectionChangeInitiator = -1; // reset flag
						//dgControl.SelectedIndex = index;
						//dgControl.SelectedItem = CustomerGrid?.SelectedItem;
						//dgControl.SelectedGrid = CustomerGrid;
					}
				}
				else if (CurrentDb == "DETAILS")
				{
					if (EventHandler != null)
					{
						//Set global  of the ID we want to find
						BankAccountViewModel.CurrentSelectedIndex = index;
						EventHandler.SqlDbTriggerEvent (Flags.isEditDbCaller, index, DetailsGrid, BankAccountViewModel.SqlUpdating);
						//MainWindow.DgControl.SelectionChangeInitiator = -1; // reset flag
						//dgControl.SelectedIndex = index;
						//dgControl.SelectedItem = DetailsGrid?.SelectedItem;
						//dgControl.SelectedGrid = DetailsGrid;
					}
				}
				SqlUpdating = false;
			}
			else
			{
				//called when EditDb is NOT OPEN
				//				CustomerViewModel custacct = null;
				//				BankAccountViewModel bankacct = null;
				//				DetailsViewModel detsacct = null;
				//				int CurrentId = 0;
				{
					//if (datagrid.Name == "CustomerGrid")
					//{
					//	if (CustomerGrid.SelectedItem != null)
					//	{
					//		custacct = CustomerGrid?.SelectedItem as CustomerViewModel;
					//		if (custacct != null)
					//		{
					//			CurrentId = custacct.Id;
					//			Flags.bvmCustRecord = custacct;
					//		}
					//	}
					//}
					//else if (datagrid.Name == "BankAccountGrid" || datagrid.Name == "BankGrid")
					//{
					//	if (BankGrid.SelectedItem != null)
					//	{
					//		//Get copy of entire BankAccvount record
					//		bankacct = BankGrid?.SelectedItem as BankAccountViewModel;
					//		if (bankacct != null)
					//		{
					//			CurrentId = bankacct.Id;
					//			Flags.bvmBankRecord = bankacct;
					//		}
					//	}
					//}
					//else if (datagrid.Name == "DetailsGrid")
					//{
					//	{
					//		if (DetailsGrid?.SelectedItem != null)
					//		{
					//			detsacct = DetailsGrid?.SelectedItem as DetailsViewModel;
					//			if (detsacct != null)
					//			{
					//				CurrentId = detsacct.Id;
					//				Flags.bvmDetRecord = detsacct;
					//			}
					//		}
					//	}
					//}
				}
				//Update the Loading window content for "Viewers Open"
				if (Flags.EventHandlerDebug)
				{
					//					Console.WriteLine ($"\r\n\r\nSqlDbViewer(1810) - ONSELECTIONCHANGED() - index changed\r\n");
					//					Console.WriteLine ($"Selected Index has changed, NOT using Event handler");
					Notifier (111, "Selected Index has changed", null);
				}
				if (CurrentDb == "BANKACCOUNT")
				{
					if (this.BankGrid.SelectedItem != null)
					{
						CurrentBankSelectedRecord = this.BankGrid.SelectedItem as BankAccountViewModel;
						// Fills/Updates the MainWindow.gv[] array
						Flags.SqlViewerIsUpdating = true;
						UpdateRowDetails (this.BankGrid.SelectedItem, "BankGrid");
					}
					//we now have FULL PrettyDetails
				}
				else if (CurrentDb == "CUSTOMER")
				{
					if (this.CustomerGrid.SelectedItem != null)
					{
						CurrentCustomerSelectedRecord = this.CustomerGrid.SelectedItem as CustomerViewModel;
						Flags.SqlViewerIsUpdating = true;
						UpdateRowDetails (this.CustomerGrid.SelectedItem, "CustomerGrid");
					}
				}
				else if (CurrentDb == "DETAILS")
				{
					if (this.DetailsGrid.SelectedItem != null)
					{
						CurrentDetailsSelectedRecord = this.DetailsGrid.SelectedItem as DetailsViewModel;
						// This creates a new entry in gv[] if this is a new window being loaded
						Flags.SqlViewerIsUpdating = true;
						UpdateRowDetails (this.DetailsGrid.SelectedItem, "DetailsGrid");
					}
				}
			}
			UpdateAuxilliaries ($"Selection changed to row {index}");
			Mouse.OverrideCursor = Cursors.Arrow;
		}
		//*******************************************************************************************************//
		public void UpdateAuxilliaries (string comment)
		{
			//Application.Current.Dispatcher.Invoke (() =>
			ParseButtonText (true);
			IsFiltered = "";
			ResetOptionButtons ();
			UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if (IsViewerLoaded == false)
				UpdateGridviewController (CurrentDb);
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			if (CurrentDb == "BANKACCOUNT")
			{
				if (bvm.BankAccountObs == null) return;
				if (this.BankGrid.ItemsSource == null)
					this.BankGrid.ItemsSource = bvm.BankAccountObs;
				//  paint the grid onscreen
				this.BankGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
				//dgControl.SelectedIndex = BankGrid.SelectedIndex;
				//dgControl.SelectedItem = BankGrid.SelectedItem;
				//dgControl.SelectedGrid = BankGrid;
				CollectionViewSource.GetDefaultView (this.BankGrid.ItemsSource).Refresh ();
			}
			if (CurrentDb == "CUSTOMER")
			{


				if (cvm.CustomersObs == null) return;
				this.CustomerGrid.ItemsSource = cvm.CustomersObs;
				//  paint the grid onscreen
				this.CustomerGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
				//dgControl.SelectedIndex = CustomerGrid.SelectedIndex;
				//dgControl.SelectedItem = CustomerGrid.SelectedItem;
				//dgControl.SelectedGrid = CustomerGrid;
				CollectionViewSource.GetDefaultView (this.CustomerGrid.ItemsSource).Refresh ();
				//				CustomerGrid.ScrollIntoView (dgControl.SelectedIndex);
			}
			if (CurrentDb == "DETAILS")
			{
				if (dvm.DetailsObs == null) return;
				this.DetailsGrid.ItemsSource = dvm.DetailsObs;
				//  paint the grid onscreen
				this.DetailsGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
				//dgControl.SelectedIndex = DetailsGrid.SelectedIndex;
				//dgControl.SelectedItem = DetailsGrid.SelectedItem;
				//dgControl.SelectedGrid = DetailsGrid;
				CollectionViewSource.GetDefaultView (this.DetailsGrid.ItemsSource).Refresh ();
			}
#pragma NOT NEEDED ????
			//			UpdateViewersList ();
			StatusBar.Text = comment;
			WaitMessage.Visibility = Visibility.Collapsed;
		}
		//*******************************************************************************************************//

		private void updatevlist ()
		{
			//for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			//{
			//	if (MainWindow.gv.window[x] == null)
			//	{
			//		// save or details in our Singleton Gridviewer Class
			//		//Set this windows Tag property to a unique value so we 
			//		// track it in the list of open viewers
			//		this.Tag = SequentialId++;
			//		CurrentGridViewerIndex = x;
			//		MainWindow.gv.CurrentDb[x] = CurrentDb;
			//		MainWindow.gv.window[x] = this;
			//		MainWindow.gv.ListBoxId[x] = (int)this.Tag;
			//		MainWindow.gv.ViewerCount++;
			//		var v = Flags.ActiveDbGrid.SelectedIndex;
			//		LoadIndex = x;
			//		break;
			//	}
			//}

		}
		//*******************************************************************************************************//
		//UNUSED right now ???????
		public void FinaliseStartup ()
		{


			this.CustomerGrid.ItemsSource = cvm.CustomersObs;
			MainWindow.gv.Datagrid[LoadIndex] = this.CustomerGrid;
			ParseButtonText (false);
			IsFiltered = "";
			ResetOptionButtons ();
			UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if (IsViewerLoaded == false)
				UpdateGridviewController ("CUSTOMER");
			// Reset auxilliary Buttons
			ResetauxilliaryButtons ();
			this.CustomerGrid.SelectedIndex = 0;

			if (this.CustomerGrid.CurrentItem == null)
			{
				this.CustomerGrid.CurrentItem = 0;
				this.CustomerGrid.SelectedItem = 0;
				//dgControl.SelectedIndex = 0;
				//dgControl.SelectedItem = 0;
				//dgControl.SelectedGrid = CustomerGrid;
			}
			UpdateViewersList ();
			//Set global flagsViewerGrid_RowEditEndin
			if (Flags.ActiveSqlGrid?.ItemsSource != null)
				CollectionViewSource.GetDefaultView (Flags.ActiveSqlGrid.ItemsSource).Refresh ();
			Debug.WriteLine ($" *** Current Active... =  {Flags.ActiveSqlGridStr}\r\n");
			Mouse.OverrideCursor = Cursors.Arrow;
		}

	}
	public class LoadedEventArgs: EventArgs
	{
		public string CallerDb { get; set; }
	}
}


