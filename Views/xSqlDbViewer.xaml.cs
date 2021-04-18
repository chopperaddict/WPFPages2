#define SHOWSQLERRORMESSAGEBOX

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using WPFPages.Libraries;
using WPFPages.ViewModels;
using WPFPages.Views;

namespace WPFPages
{

	// We MUST declare our delegates here - in NameSpace, but not usually in a class, although you can do so
	//public delegate void AddDelegate (int a, int b);
	//public delegate string SayDelegate (string s);


	//public class DelegateClass
	//{
	//	public DelegateClass () { }
	//	// These will be called via a delegate - AddDelegate();
	//	public void AddNums (int a, int b)
	//	{ Console.WriteLine (a + b); }
	//	public static string SayHello (string s)
	//	{ return "Hello " + s; }
	//}

	public partial class SqlDbViewer: Window, INotifyPropertyChanged
	{
	
	#region Class setup
		private int CurrentGridViewerIndex = -1;
		public string CurrentDb = "BANKACCOUNT";
		public SqlDataAdapter sda;
		//		public DataTable dtBank = new System.Data.DataTable ("BankAccountDataTable");
		//public DataTable dtCust = new DataTable ("CustomerDataTable");
		//public DataTable dtDetails = new DataTable ("DetailsDataTable");
		//		DataTable dtBank = BankAccountViewModel.dtBank;
		//		DataTable dtCust = CustomerViewModel.dtCust;
		//		DataTable dtDetails = DetailsViewModel.dtDetails;
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
		public static bool IsviewerLoaded = false;
		private int LoadIndex = -1;
		public static bool SqlUpdating = false;
		private bool IsCellChanged = false;
		//		public EditDb EditdbWndBank = null;
		public DataGrid EditDataGrid = null;

		//Get "Local" copies of our global DataTables
		public DataTable dtDetails = DetailsViewModel.dtDetails;
		public DataTable dtBank = BankAccountViewModel.dtBank;
		public DataTable dtCust = CustomerViewModel.dtCust;

		//Variables for Edithasoccurred delegate
		private SQLEditOcurred SqlEdit = HandleEdit;
		private EditEventArgs EditArgs = null;


		// SQL Data Setup
		public BankAccountViewModel Bank = new BankAccountViewModel ();
		public CustomerViewModel Customers = new CustomerViewModel ();
		public DetailsViewModel Details = new DetailsViewModel ();

		public static SqlDbViewer sqldbForm = null;
		public EditDb edb = null;

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

		public SqlSelChange SelectionChanged;
		public static int SequentialId = 12345;
		event SQLViewerGridSelectionChanged SQLVSelChange;

		public EventHandlers EventHandler = new EventHandlers ();
		private static bool SelectionhasChanged = false;

		//Variables used when a cell is edited to se if we need to update via SQL
		private object OriginalCellData = null;
		private string OriginalDataType = "";
		private int OrignalCellRow = 0;
		private int OriginalCellColumn = 0;


		// Event handler  to let DbSelector know once window is full y loaded
		// and ready to load DataGrid SQL data 
		public delegate void IsViewerLoaded (SqlDbViewer  sqlv);
		public event IsViewerLoaded ViewerIsLoaded;

		/// <summary>
		/// Used to keep track of currently selected row in GridViwer
		/// </summary>
		private int _selectedRow;
		public int SelectedRow
		{
			get { return _selectedRow; }
			set { _selectedRow = value; OnPropertyChanged (SelectedRow.ToString ()); }
		}

		//Dummy Constructor for Event handlers
		public SqlDbViewer ()
		{
			sqldbForm = this;
			//subscribe to Loaded event 

		}

		#endregion setup

	#region delegate/Event handlers
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

		static void HandleEdit (object sender, EditEventArgs e)
		{
			//Handler for Datagrid Edit occurred delegate
			if (Flags.EventHandlerDebug)
			{
				Console.WriteLine ($"\r\nRecieved by SQLDBVIEWER (150) Caller={e.Caller}, Index = {e.CurrentIndex},  Grid = {e.ToString ()}\r\n ");
			}
		}
	#endregion delegate/Event handlers

	#region load/startup
		//*****************************************************************************************//
		public SqlDbViewer (int selection)
		{
			int selectedDb = -1;
			//			IsviewerLoaded = false;
			DbSelector dbs = new DbSelector ();
			ViewerIsLoaded += dbs.OnViewerLoaded;
			InitializeComponent ();
			ThisWindow = this;
			EditArgs = new EditEventArgs ();

			Show ();
			//Trigger the event to ntify DbASelector we are ready for data loading
			if(ViewerIsLoaded != null)
				ViewerIsLoaded?.Invoke (this);
			return;
		}
//		public void   OnViewerLoaded()
//		//Event handlernotification we receive to go ahead with loadin Viewer
//		{
//			HandleWindowLoaded ("xxxxx");
////			return true;
//		}
		
		public void HandleWindowLoaded (string currentDb)
		{
			int selected = 0;
			//Called ONLY after thread has loaded the window fully (by DbSelector)
			CurrentDb = currentDb;

			if (!IsviewerLoaded)
			{
				int currentindex = -1;
				this.MouseDown += delegate { DoDragMove (); };
				//setup our delegate for data loading notification ( does the += stuff for us)
				EventHandlers.SetWindowHandles (null, null, DetailsGrid);
				EventHandlers.DetailsDataCanBeLoaded += new DetailsLoadedDelegate (LoadDetailsData);
				EventHandlers.SetWindowHandles (null, null, CustomerGrid);
				EventHandlers.CustDataCanBeLoaded += new CustLoadedDelegate (LoadCustData);
				EventHandlers.SetWindowHandles (null, null, BankGrid);
				EventHandlers.BankDataCanBeLoaded += new BankLoadedDelegate (BankAccountViewModel.LoadBankTask);// LoadBankData);

				if (currentDb == "BANKACCOUNT")
					selected = 0;
				else if (currentDb == "CUSTOMER")
					selected = 1;
				else if (currentDb == "DETAILS")
					selected = 2;
				//Create Instance of EditDb window
				edb = new EditDb ();

				//Setup our delegate so we can update EdutDb grid on selection  changed
				//					Dispatcher.Invoke (() =>
				//					{
				//						DgControl.
				//					});


				for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
				{
					if (MainWindow.gv.window[x] == null)
					{
						// save or details in our Singleton Gridviewer Class
						MainWindow.gv.CurrentDb[x] = CurrentDb;
						CurrentGridViewerIndex = x;
						//Save Window handle of current selected Viewer window (in our list)
						MainWindow.gv.window[x] = this;
						//Set this windows Tag property to a unique value so we 
						// track it in the list of open viewers
						this.Tag = SequentialId++;
						MainWindow.gv.ListBoxId[x] = (int)this.Tag;
						MainWindow.gv.ViewerCount++;
						LoadIndex = x;
						break;
					}
				}
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
					//	Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
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
					//	Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
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
					//	Flags.SetGlobalFlags (this, DetailsGrid, CurrentDb);
					//	//Setup the Event handler to notify EditDb viewer of index changes
					//	//						nt = new EventHandlers (this, DetailsGrid, "SQLDBVIEWER");
					//}
				}
				if (CurrentDb == "BANKACCOUNT")
				{
					ShowBank_Click (null, null);
					Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
					BankGrid.SelectedIndex = 0;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					ShowCust_Click (null, null);
					Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
					CustomerGrid.SelectedIndex = 0;
					CustomerGrid.SelectedItem = 0;
				}
				else if (CurrentDb == "DETAILS")
				{
					ShowDetails_Click (null, null);
					Flags.SetGlobalFlags (this, DetailsGrid, CurrentDb);
					DetailsGrid.SelectedIndex = 0;
					DetailsGrid.SelectedItem = 0;
				}
				//Store pointers to our DataGrids in all ModelViews for access by Data row updating code
				EventHandlers.SetWindowHandles (null, this, null);
				Mouse.OverrideCursor = Cursors.Arrow;
			}
			IsviewerLoaded = true;
		}

		#endregion load/startup

	#region load all data base data
		///<summary>
		/// BankAccount database
		/// </summary>
		private bool FillBankAccountDataGrid ()
		{
			string CmdString = "";
			//if (FilterCommand != "")
			//{
			//	CmdString = FilterCommand;
			//	FilterCommand = "";
			//}
			//else
			//{
			//	if (isMultiMode)
			//	{
			//		//THIS IS THE SQL COMMAND TO GET FULL LINES OF DUPLICATED CUSTOMER ACCOUNT #'S DATA
			//		CmdString = "SELECT * FROM BankAccount WHERE CUSTNO IN "
			//			+ "(SELECT CUSTNO FROM BANKACCOUNT "
			//			+ " GROUP BY CUSTNO"
			//			+ " HAVING COUNT(*) > 1)";
			//		//clear the datatable first as we are only showing a subset
			//		dtBank.Clear ();
			//	}
			//	else
			//	{
			CmdString = "SELECT * FROM BankAccount order by CustNo";
			//clear the datatable first as we are only showing a subset
			BankAccountViewModel.dtBank.Clear ();
			//	}
			//}
			CurrentDb = "BANKACCOUNT";
			bool result = LoadSqlData (CmdString, BankAccountViewModel.dtBank, "BANKACCOUNT", isMultiMode, Filters, StatusBar, Multiaccounts);
			// dtBank is fully loaded here
			isMultiMode = false;
			return result;
		}

		//*****************************************************************************************//
		public bool LoadSqlData (string CmdString, DataTable dt, string CallerType, bool isMultiMode, Button Btn, TextBlock StatusBar, Button Multiaccounts)
		{
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
					SqlCommand cmd = new SqlCommand (CmdString, con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					//load the SQL data into our OC named bankaccounts
					if (CallerType == "BANKACCOUNT")
					{
						//This loads the BankAccount data into DataTable dtBank
						if (BankAccountViewModel.LoadBankAccountObsCollection ())
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
						LoadCustData ();
						if (CustomerViewModel.LoadCustomerObsCollection ())
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
					return true;
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
			if (DetailsViewModel.DetailsObs.Count > 0)
			{
				DetailsViewModel.DetailsObs.Clear ();
				DetailsViewModel.DetailsList.Clear ();
			}
			try
			{
				for (int i = 0; i < DetailsViewModel.dtDetails.Rows.Count; ++i)
					DetailsViewModel.DetailsObs.Add (new DetailsViewModel
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

		/// <summary>
		/// Fetches SQL data for BankAccount Db and fills BankAccount DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowBank_Click (object sender, RoutedEventArgs e)
		{
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
			//Clear  up the current button wording ?
			ClearCurrentGridData ();
			ParseButtonText (false);
			StatusBar.Text = "Please wait, loading data from SQL Database...";

			//This is where we load the Data via SQL
			BankAccountViewModel.LoadBankTask ();

			//Window_MouseDown (sender, null);
			CurrentDb = "BANKACCOUNT";
			this.DetailsGrid.Visibility = Visibility.Hidden;
			this.CustomerGrid.Visibility = Visibility.Hidden;
			this.BankGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			Mouse.OverrideCursor = Cursors.Wait;
#pragma NOT NEEDED ?????
			//			ClearCollectionsData (CurrentDb);

#pragma TODO Load window later
			//Window is ONSCREEN by here
			UpdateAuxilliaries ("BankAccount Data Loaded...");
			StatusBar.Text = "Data loaded from SQL Database Successfully...";

			ParseButtonText (true);
			Mouse.OverrideCursor = Cursors.Arrow;
			//this.BringIntoView ();
			//this.Focus ();
		}

		//*****************************************************************************************//
		/// <summary>
		/// handle clearing down the data to allow a switch to a different Db view
		/// </summary>
		private void ClearCurrentGridData ()
		{
			if (CurrentDb == "BANKACCOUNT")
			{
				BankGrid.ItemsSource = null;
				BankAccountViewModel.BankList?.Clear ();
				BankAccountViewModel.BankAccountObs?.Clear ();
				BankGrid.Items.Clear ();
				BankGrid.ItemsSource = CollectionViewSource.GetDefaultView (BankAccountViewModel.BankAccountObs);
			}
			else if (CurrentDb == "CUSTOMER")
			{
				CustomerGrid.ItemsSource = null;
				CustomerViewModel.CustomersList?.Clear ();
				CustomerViewModel.CustomersObs?.Clear ();
				CustomerGrid.Items.Clear ();
				CustomerGrid.ItemsSource = CollectionViewSource.GetDefaultView (CustomerViewModel.CustomersObs);
			}
			else if (CurrentDb == "DETAILS")
			{
				DetailsGrid.ItemsSource = null;
				DetailsViewModel.DetailsList?.Clear ();
				DetailsViewModel.DetailsObs?.Clear ();
				DetailsGrid.Items.Clear ();
				DetailsGrid.ItemsSource = CollectionViewSource.GetDefaultView (DetailsViewModel.DetailsObs);
			}
			MainWindow.DgControl.SelectedGrid = null;
			MainWindow.DgControl.SelectedIndex = 0;
			MainWindow.DgControl.SelectedItem = null;
		}

		//
		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public  void ShowCust_Click (object sender, RoutedEventArgs e)
		{
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
			StatusBar.Text = "Please wait, loading data from SQL Database...";

#pragma ASYNC  loading grid here !!!
			CustomerGrid.ItemsSource = CustomerViewModel.CustomersObs;
			//if (CustomerViewModel.CustomersObs != null)
			//	ClearCurrentGridData ();
			ParseButtonText (false);
//			CustomerViewModel.LoadCustomersTask ();

			CurrentDb = "CUSTOMER";
#pragma NOT NEEDED ????
			this.DetailsGrid.Visibility = Visibility.Hidden;
			this.BankGrid.Visibility = Visibility.Hidden;
			this.CustomerGrid.Visibility = Visibility.Visible;
			this.CustomerGrid.BringIntoView ();
			Filters.IsEnabled = true;

			Mouse.OverrideCursor = Cursors.Wait;

			//			if (CustomerViewModel.dtCust.Rows.Count > 0)
			//			{
			//				//Need to update the Db via SQL - does nothing right now
			//				CustomerViewModel.UpdateBankDb (CustomerViewModel.dtCust, "CUSTOMER");
			//			}
			//			// Set up our data binding after loading the Db via SQL
			//			if (CustomerViewModel.CustomersObs == null || CustomerViewModel.CustomersObs.Count == 0)
			//				CustomerViewModel.LoadCustomersCollection ();

			//			if (CustomerViewModel.dtCust.Rows.Count > 0)
			//			{
			//#pragma TODO
			//				//Need to update the Db via SQL  -does nothing at present
			//				CustomerViewModel.UpdateCustomerDb ("CUSTOMER");
			//			}
			//			// Clear DataTable ????  Why here ?
			//			if (CustomerViewModel.dtCust != null && CustomerViewModel.dtCust.Rows.Count > 0)
			//				CustomerViewModel.dtCust.Clear ();

			//			// load  the data from SQL and load it into List<Customer>
			//			if (CustomerViewModel.CustomersObs.Count == 0)
			//				MessageBox.Show ("No data was recovered from the Customer Database ?");

			//Still no data loaded
//			UpdateAuxilliaries ("Customer Data Loaded...");
			StatusBar.Text = "Data loaded from SQL Database Successfully...";

			Mouse.OverrideCursor = Cursors.Arrow;
		}
		//UNUSED right now ???????
		public void FinaliseStartup ()
		{
			//			Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
			CustomerGrid.ItemsSource = CollectionViewSource.GetDefaultView (CustomerViewModel.CustomersObs);
			MainWindow.gv.Datagrid[LoadIndex] = CustomerGrid;
			ParseButtonText (false);
			IsFiltered = "";
			ResetOptionButtons ();
			UpdateDbSelector (this);
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if (IsviewerLoaded == false)
				UpdateGridviewController ("CUSTOMER");
			// Reset auxilliary Buttons
			ResetauxilliaryButtons ();
			CustomerGrid.SelectedIndex = 0;

			if (CustomerGrid.CurrentItem == null)
			{
				CustomerGrid.CurrentItem = 0;
				CustomerGrid.SelectedItem = 0;
				MainWindow.DgControl.SelectedIndex = 0;
				MainWindow.DgControl.SelectedItem = 0;
				MainWindow.DgControl.SelectedGrid = CustomerGrid;
			}
			UpdateViewersList ();
			//Set global flags
			//			Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
			if (Flags.ActiveSqlGrid?.ItemsSource != null)
				CollectionViewSource.GetDefaultView (Flags.ActiveSqlGrid.ItemsSource).Refresh ();
			Debug.WriteLine ($" *** Current Active... =  {Flags.ActiveSqlGridStr}\r\n");
			Mouse.OverrideCursor = Cursors.Arrow;
		}
		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for DetailsViewModel Db and fills relevant DataGrid
		/// <param name="sender"></param>
		/// <param name="e"></param></summary>
		private void ShowDetails_Click (object sender, RoutedEventArgs e)
		{
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
			StatusBar.Text = "Please wait, loading data from SQL Database...";

			if (DetailsViewModel.DetailsObs != null)
				ClearCurrentGridData ();
			ParseButtonText (false);
			DetailsViewModel.LoadDetailsTask ();

			CurrentDb = "DETAILS";
			this.CustomerGrid.Visibility = Visibility.Hidden;
			this.BankGrid.Visibility = Visibility.Hidden;
			this.DetailsGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			Mouse.OverrideCursor = Cursors.Wait;
			if (DetailsViewModel.dtDetails == null)
				DetailsViewModel.dtDetails = new DataTable ();
			if (DetailsViewModel.dtDetails.Rows.Count > 0)
			{
				//Need to update the Db via SQL - does nothing right now
				DetailsViewModel.UpdateBankDb ("DETAILS");
			}
			UpdateAuxilliaries ("Details Data Loaded...");
			StatusBar.Text = "Data loaded from SQL Database Successfully...";

			//Set global flags
			Flags.SetGlobalFlags (this, DetailsGrid, CurrentDb);
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		private void ClearCollectionsData (string Caller)
		{
			//Warning - pointers are ALL WRONG here !!!!!   3/4/21
			BankAccountViewModel.dtBank.Clear ();
			CustomerViewModel.dtCust.Clear ();
			DetailsViewModel.dtDetails.Clear ();
			if (Caller == "BANKACCOUNT")
			{
				if (CustomerViewModel.CustomersObs != null) CustomerViewModel.CustomersObs.Clear ();
				if (DetailsViewModel.DetailsObs != null) DetailsViewModel.DetailsObs.Clear ();
				CustomerViewModel.CustomersList.Clear ();
				DetailsViewModel.DetailsList.Clear ();

			}
			if (Caller == "CUSTOMER")
			{
				if (BankAccountViewModel.BankAccountObs != null) BankAccountViewModel.BankAccountObs.Clear ();
				if (CustomerViewModel.CustomersObs != null) CustomerViewModel.CustomersObs.Clear ();
				BankAccountViewModel.BankList.Clear ();
				DetailsViewModel.DetailsList.Clear ();
			}
			if (Caller == "DETAILS")
			{
				if (BankAccountViewModel.BankAccountObs != null) BankAccountViewModel.BankAccountObs.Clear ();
				if (DetailsViewModel.DetailsObs != null) DetailsViewModel.DetailsObs.Clear ();
				CustomerViewModel.CustomersList.Clear ();
				BankAccountViewModel.BankList.Clear ();
			}
		}
		#endregion Load/show selected data base data

	#region Filtering code
		//*****************************************************************************************//
		private void SetFilter_Click (object sender, RoutedEventArgs e)
		{
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

	#region ObservableCollection setup


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
		//		OnPropertyChanged (DetailsViewModel.DetailsObs.ToString ());
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

		//*****************************************************************************************//
		/// <summary>
		///  Handle the exit from Cell Editing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DataGrid_RowEditEnded (object sender, DataGridRowEditEndingEventArgs e)
		{
			///
			/// This ONLY gets called when a cell is edited
			/// After a fight, this is now working and updates the relevant RECORD correctly
			/// 

			BankAccountViewModel ss = null;
			CustomerViewModel cs = null;
			DetailsViewModel sa = null;

			//if data has NOT changed, do NOT bother updating the Db
			// Clever stuff Eh - saves lots of processing time?
			if (!SelectionhasChanged)
				return;
			else
				SelectionhasChanged = false;    // clear the edit status flag again

			//Only called whn an edit has been completed
			if (e == null)
			{
				if (CurrentDb == "BANKACCOUNT")
				{
					// set global flag to show we are in edit/Save mode
					BankAccountViewModel.SqlUpdating = true;

					SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row);
					//					return;
					ss = new BankAccountViewModel ();
					ss = BankCurrentRow.Item as BankAccountViewModel;
				}
				else if (CurrentDb == "DETAILS")
				{
					SQLHandlers.UpdateDbRow ("DETAILS", e.Row);
					sa = new DetailsViewModel ();
					sa = DetailsCurrentRow.Item as DetailsViewModel;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					SQLHandlers.UpdateDbRow ("CUSTOMER", e.Row);
					cs = new CustomerViewModel ();
					cs = CustomerCurrentRow.Item as CustomerViewModel;
				}
			}
			else
			{
				if (CurrentDb == "BANKACCOUNT")
				{
					ss = new BankAccountViewModel ();
					ss = BankGrid.Items.CurrentItem as BankAccountViewModel;
					//					//Now call delegate  to inform DbEdit we have a change  to the data in SQLDBVIEWER
					//					TriggerEditDbUpdate ("BANKACCOUNT", BankGrid.SelectedIndex, ss);

					// set global flag to show we are in edit/Save mode
					BankAccountViewModel.SqlUpdating = true;
					//Notify EditDb of the change
					SQLHandlers.UpdateDbRow ("BANKACCOUNT", e.Row);
					return;
				}
				else if (CurrentDb == "DETAILS")
				{
					sa = new DetailsViewModel ();
					sa = e.Row.Item as DetailsViewModel;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					cs = new CustomerViewModel ();
					cs = e.Row.Item as CustomerViewModel;
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
							MessageBox.Show ($"Invalid A/C Type ({ss.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
							return;
						}
						Y = Convert.ToDecimal (ss.Balance);
						Y = Convert.ToDecimal (ss.IntRate);
						//Check for invalid Interest rate
						if (Y > 100)
						{
							Console.WriteLine ($"SQL Invalid Interest Rate of {ss.IntRate} > 100% in grid Data");
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
							MessageBox.Show ($"Invalid A/C Type ({sa.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
							return;
						}
						Y = Convert.ToDecimal (sa.Balance);
						Y = Convert.ToDecimal (sa.IntRate);
						//Check for invalid Interest rate
						if (Y > 100)
						{
							Console.WriteLine ($"SQL Invalid Interest Rate of {sa.IntRate} > 100% in grid Data");
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
							cmd.ExecuteNonQuery ();

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery ();

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery ();
							Console.WriteLine ("SQL Update successful for Bank Account Data...");
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
							cmd.ExecuteNonQuery ();
							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery ();

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery ();
							Console.WriteLine ("SQL Update successful for DETAILS Data...");
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
							cmd.ExecuteNonQuery ();

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery ();

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery ();
							Console.WriteLine ("SQL Update successful for SECACCOUNTS Data...");
						}
						StatusBar.Text = "Database updated successfully....";
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error - {ex.Message} Data = {ex.Data}");

#if SHOWSQLERRORMESSAGEBOX
					MessageBox.Show ("SQL error occurred - See Output for details");
#endif
				}
				finally
				{
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
						cmd.ExecuteNonQuery ();

						cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype,  ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						cmd.ExecuteNonQuery ();

						cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						cmd.ExecuteNonQuery ();
						Console.WriteLine ("SQL Update successful for CUSTOMER Data...");

					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error - {ex.Message} Data = {ex.Data}");
#if SHOWSQLERRORMESSAGEBOX
					MessageBox.Show ("SQL error occurred - See Output for details");
#endif

				}
				finally
				{
					con.Close ();
				}
			}
		}

		//*****************************************************************************************//
		/// <summary>
		///  Handles the closing of the Window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CloseViewer_Click (object sender, RoutedEventArgs e)
		{
			//Close current (THIS) Viewer Window
			//clear flags in ViewModel
			if (CurrentDb == "BANKACCOUNT")
			{
				BankAccountViewModel.CurrentSQLViewerBankGrid = null;
				BankAccountViewModel.ActiveSqlDbViewer = null;
				BankAccountViewModel.ClearFromSqlList (BankGrid, CurrentDb);
			}
			else if (CurrentDb == "CUSTOMER")
			{
				BankAccountViewModel.CurrentSQLViewerCustomerGrid = null;
				BankAccountViewModel.ActiveSqlDbViewer = null;
				BankAccountViewModel.ClearFromSqlList (CustomerGrid, CurrentDb);
			}
			else if (CurrentDb == "DETAILS")
			{
				BankAccountViewModel.CurrentSQLViewerDetailsGrid = null;
				BankAccountViewModel.ActiveSqlDbViewer = null;
				BankAccountViewModel.ClearFromSqlList (DetailsGrid, CurrentDb);
			}
			EventHandlers.ClearWindowHandles (null, this);
			BankAccountViewModel.EditdbWndBank = null;
			//Set global flags
			Flags.SetGlobalFlags (this, null, CurrentDb);
			Close ();
		}


		/// <summary>
		/// does very little really
		///</summary>
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
		private void UpdateViewersList ()
		{
			if (MainWindow.gv.DbSelectorWindow.ViewersList?.Items?.Count == 1)
				return;
			for (int i = 0; i < MainWindow.gv.DbSelectorWindow?.ViewersList?.Items?.Count; i++)
			{
				if (i + 1 == MainWindow.gv.DbSelectorWindow?.ViewersList?.Items.Count)
					return;
				if (MainWindow.gv.ListBoxId[i] == (int)this.Tag)
				{
					ListBoxItem lbi = new ListBoxItem ();
					lbi = MainWindow.DbSelectorOpen.ViewersList.Items[i + 1] as ListBoxItem;
					lbi.Content = MainWindow.gv.CurrentDb[i];
					break;
				}
			}
		}
		/// <summary>
		/// Update "Open Viewers" Listbox text of the DbSelector window 
		/// to match the fact we have changed Db source in this viewer
		/// </summary>
		/// <param name="viewer"> = SqlDbSelector</param>
		private void UpdateDbSelector (SqlDbViewer viewer)
		{
			// works with multiple entries 22 March 2021

			if (MainWindow.DbSelectorOpen == null)
				return;

			if (MainWindow.DbSelectorOpen.ViewersList.Items.Count == 1)
			{
				MainWindow.DbSelectorOpen.ViewerDeleteAll.IsEnabled = false;
				MainWindow.DbSelectorOpen.ViewerDelete.IsEnabled = false;
				return;
			}

			int indx = (int)viewer.Tag;
		}
		//*****************************************************************************************//
		/// <summary>
		/// Fills out the most detailed info we have on what the DbViewer window 
		/// is setup to display in the DbSelector viewers list
		/// </summary>
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
				ShowBank.IsEnabled = true; ;
				ShowCust.IsEnabled = true; ;
				ShowDetails.IsEnabled = true; ;
				if (CurrentDb == "BANKACCOUNT")
				{
					if (!obj)
					{
						if (isMultiMode)
							ShowBank.Content = $"<M> Bank A/c's  ({BankGrid.Items.Count})";
						else
							ShowBank.Content = $"Bank A/c's  ({BankGrid.Items.Count})";
					}
					else
						ShowBank.Content = $"Bank A/c's  ({BankGrid.Items.Count})";
					ShowBank.IsEnabled = false;

				}
				else if (CurrentDb == "CUSTOMER")
				{
					if (!obj)
					{
						if (isMultiMode)
							ShowCust.Content = $"<M> Customer A/c's  ({CustomerGrid.Items.Count})";
						else
							ShowCust.Content = $"Customer A/c's  ({CustomerGrid.Items.Count})";
					}
					else
						ShowCust.Content = $"Customer A/c's  ({CustomerGrid.Items.Count})";
					ShowCust.IsEnabled = false;
				}
				else if (CurrentDb == "DETAILS")
				{
					if (!obj)
					{
						if (isMultiMode)
							ShowDetails.Content = $"<M> Details A/c's  ({DetailsGrid.Items.Count})";
						else
							ShowDetails.Content = $"Details A/c's  ({DetailsGrid.Items.Count})";
					}
					else
						ShowDetails.Content = $"Details A/c's  ({DetailsGrid.Items.Count})";
					ShowDetails.IsEnabled = false;
				}
			}
		}
		//*****************************************************************************************//
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
		private void ScrollList (DataGrid grid, int count)
		{
			//			int currentindex = grid.SelectedIndex;
			//			object selitem = grid.SelectedItem;
			//			int maxrows = grid.Items.Count;
			if (grid.SelectedItem == null) return;
			int indx = grid.SelectedIndex;
			grid.SelectedItem = indx;
			grid.ScrollIntoView (grid.SelectedItem);
			//		MainWindow.DgControl.SelectedItem = grid.SelectedItem;
			var border = VisualTreeHelper.GetChild (grid, 0) as Decorator;

			if (border == null) return;
			var scroll = border.Child as ScrollViewer;

			if (scroll == null) return;

			scroll.UpdateLayout ();
			return;
			//#pragma TODO
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

	#region data change functions
		//*****************************************************************************************//
		private void BankGrid_DataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			int x = 1;
		}

		//*****************************************************************************************//
		private void UpdateRowDetails (object datarecord, string caller)
		// This updates the data in the DbSelector window's Viewers listbox
		{

			for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			{
				if ((int)MainWindow.gv.ListBoxId[x] == (int)this.Tag)
				{
					if (caller == "BankGrid")
					{
						//	BankAccountViewModel record = (BankAccountViewModel)datarecord;
						var record = datarecord as BankAccountViewModel;//CurrentBankSelectedRecord;
						MainWindow.gv.CurrentDb[x] = $"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, Balance £ {record?.Balance}, Interest {record?.IntRate}%";
						PrettyDetails = MainWindow.gv.CurrentDb[x];
						MainWindow.gv.PrettyDetails = PrettyDetails;
						//Update list in DbSelector
						UpdateDbSelector (this);
					}
					else if (caller == "CustomerGrid")

					{
						var record = datarecord as CustomerViewModel;
						MainWindow.gv.CurrentDb[x] = $"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, {record?.LName} {record?.Town}, {record?.County}";
						PrettyDetails = MainWindow.gv.CurrentDb[x];
						MainWindow.gv.PrettyDetails = PrettyDetails;
						//Update list in DbSelector
						UpdateDbSelector (this);
					}
					else if (caller == "DetailsGrid")
					{
						var record = datarecord as DetailsViewModel;
						MainWindow.gv.CurrentDb[x] = $"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, Balance {record?.Balance}, Interest % {record?.IntRate}";
						PrettyDetails = MainWindow.gv.CurrentDb[x];
						MainWindow.gv.PrettyDetails = PrettyDetails;
						//Update list in DbSelector
						UpdateDbSelector (this);
					}
					break;
				}
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

			if (CustomerGrid.SelectedItem != null)
			{
				if (CustomerGrid.SelectedItem == null)
					return;
				TextBlock tb = new TextBlock ();

				//This gives me an entrie Db Record in "c"
				CustomerViewModel c = CustomerGrid.SelectedItem as CustomerViewModel;

			}
		}

		private void CustomerGrid_TargetUpdated (object sender, System.Windows.Data.DataTransferEventArgs e)
		//*****************************************************************************************//
		{
			// row data Loading ???
			MainWindow.gv.Datagrid[LoadIndex] = this.CustomerGrid;
			CustomerGrid.SelectedIndex = 0;
			SelectedRow = 0;

		}
		#endregion data change functions

	#region general stuff
		//*****************************************************************************************//
		private void ExitFilter_Click (object sender, RoutedEventArgs e)
		{
			//Just "Close" the Filter panel
			//			FilterFrame.Visibility = Visibility.Hidden;
		}

		//*****************************************************************************************//
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
		//*****************************************************************************************//
		private void ContextMenu2_Click (object sender, RoutedEventArgs e)
		{
			//Delete current Row
			BankAccountViewModel dg = sender as BankAccountViewModel;
			DataRowView row = (DataRowView)BankGrid.SelectedItem;

		}

		//*****************************************************************************************//
		private void ContextMenu3_Click (object sender, RoutedEventArgs e)
		{
			//Close Window
		}

		//*****************************************************************************************//
		private void Multiaccs_Click (object sender, RoutedEventArgs e)
		{
			//Show only Customers with multiple Bank Accounts
			Window_MouseDown (sender, null);
			string s = Multiaccounts.Content as string;
			if (s.Contains ("<<-"))
				isMultiMode = false;
			else
				isMultiMode = true;
			if (CurrentDb == "BANKACCOUNT")
				FillBankAccountDataGrid ();
			else if (CurrentDb == "CUSTOMER")
#pragma TODO we need this
				CurrentDb = "CUSTOMER";
			//				FillCustomerDataGrid ();
			else if (CurrentDb == "DETAILS")
				FillDetailsDataGrid ();

			ControlTemplate tmp = Utils.GetDictionaryControlTemplate ("HorizontalGradientTemplateGray");
			Filters.Template = tmp;
			Brush br = Utils.GetDictionaryBrush ("HeaderBrushGray");
			Filters.Background = br;
			Filters.Content = "Filtering";

		}

		//*****************************************************************************************//
		private void ContextMenuFind_Click (object sender, RoutedEventArgs e)
		{
			// find something - this returns  the top rows data in full
			BankAccountViewModel b = BankGrid.Items.CurrentItem as BankAccountViewModel;

		}
		//*****************************************************************************************//
		private void CloseAccount_Click (object sender, RoutedEventArgs e)
		{
			//Now get the actual data item behind the selected row
			if (CurrentDb == "BANKACCOUNT")
			{
				int id = BankCurrentRowAccount.Id;
				BankCurrentRowAccount.CDate = DateTime.Now;
				DataGrid_RowEditEnded (null, null);
				BankGrid.Items.Refresh ();
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
		//*****************************************************************************************//

		// not used

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
				Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
			else if (CurrentDb == "CUSTOMER")
				Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
			else if (CurrentDb == "DETAILS")
				Flags.SetGlobalFlags (this, DetailsGrid, CurrentDb);

#pragma  TEMP RETURN ONLY 2/4/21
			return;
			int tag = -1;
			tag = (int)this.Tag; ;
			for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			{
				// set the pointer in our structure to current Viewer Window handle
				if (MainWindow.gv.ListBoxId[x] == tag)
				{
					MainWindow.DbSelectorOpen.ViewersList.SelectedIndex = x + 1;
					//					Console.WriteLine ("Focus code handled...");
					break;
				}
			}
		}

		//*****************************************************************************************//
		private void Window_Closed (object sender, EventArgs e)
		{
			if (this.Tag == null) return;
			int tag = (int)this.Tag;

			if (MainWindow.DbSelectorOpen == null)
				return;
			ListBoxItem lbi = new ListBoxItem ();
			for (int y = 0; y < MainWindow.DbSelectorOpen.ViewersList.Items.Count; y++)
			{
				lbi = MainWindow.DbSelectorOpen.ViewersList.Items[y + 1] as ListBoxItem;
				int lbtag = (int)lbi.Tag;
				if (lbtag == tag)
				{
					//Remove this entry from ViewersList
					MainWindow.gv.ViewerCount--;
					MainWindow.gv.CurrentDb[y] = "";
					MainWindow.gv.ListBoxId[y] = -1;
					MainWindow.gv.Datagrid[y] = null;
					MainWindow.gv.window[y] = null;
#pragma   TODO Chosenviewer
					MainWindow.DbSelectorOpen.ViewersList.Items.RemoveAt (y + 1);
					break;
				}
			}
			if (MainWindow.DbSelectorOpen.ViewersList.Items.Count == 1)
			{
				MainWindow.DbSelectorOpen.ViewerDeleteAll.IsEnabled = false;
				MainWindow.DbSelectorOpen.ViewerDelete.IsEnabled = false;
			}
		}

		//*****************************************************************************************//
		private void Minimize_click (object sender, RoutedEventArgs e)
		{
			Window_MouseDown (sender, null);
			this.WindowState = WindowState.Normal;
		}
		//*****************************************************************************************//
		private void Window_MouseDown (object sender, MouseButtonEventArgs e)
		{
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

		private void CustomerGrid_Initialized_1 (object sender, EventArgs e)
		{
			if (CustomerGrid.SelectedItem != null)
			{
				if (CustomerGrid.SelectedItem == null)
					return;
				TextBlock tb = new TextBlock ();

				//This gives me an entrie Db Record in "c"
				CustomerViewModel c = CustomerGrid.SelectedItem as CustomerViewModel;
				//				Console.WriteLine ($"CustomerGrid_Initialized_1 - Identified row data of [{c.CustNo} {c.FName} {c.LName}]");

			}

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
		#endregion general stuff

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
			if (BankGrid.SelectedItem != null)
			{
				//This gives me the entire Db Record in "c"
				BankAccountViewModel c = BankGrid?.SelectedItem as BankAccountViewModel;
				if (c == null) return;
				//			MainWindow.DgControl.SelectedItem = BankGrid?.SelectedItem;

				string date = Convert.ToDateTime (c.ODate).ToShortDateString ();
				if (c != null)
				{
					string s = $"Bank - # {c.CustNo}, Bank #{c.BankNo}, Customer # {c.CustNo}, £{c.Balance}, {c.IntRate}%,  {date}";
					UpdateDbSelector (s);
				}
				//				if(BankAccountViewModel.EditdbWndBank  != null)
				//					BankAccountViewModel.EditdbWndBank .DataGrid1.SelectedItem = BankGrid.SelectedItem;

			}
		}
		private void CustomerGrid_SelectedCellsChanged (object sender, SelectedCellsChangedEventArgs e)
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSleector ViewersList text
			if (CustomerGrid.SelectedItem != null)
			{
				if (CustomerGrid.SelectedItem == null)
					return;
				//TextBlock tb = new TextBlock ();

				//This gives me an entire Db Record in "c"
				CustomerViewModel c = CustomerGrid?.SelectedItem as CustomerViewModel;
				if (c != null)
				{
					string s = $"Customer - # {c.CustNo}, Bank #@{c.BankNo}, {c.LName}, {c.Town}, {c.County} {c.PCode}";
					UpdateDbSelector (s);
				}

			}
		}
		private void DetailsGrid_SelectedCellsChanged (object sender, SelectedCellsChangedEventArgs e)
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSleector ViewersList text
			if (DetailsGrid.SelectedItem != null)
			{
				if (DetailsGrid.SelectedItem == null)
					return;
//				TextBlock tb = new TextBlock ();

				//This gives me an entrie Db Record in "c"

				DetailsViewModel c = DetailsGrid?.SelectedItem as DetailsViewModel;
				if (c == null) return;
				//			MainWindow.DgControl.SelectedItem = DetailsGrid.SelectedItem;
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"DetailsGrid_SelectedCellsChanged - Identified row data");
				}
				if (c != null)
				{
					string date = Convert.ToDateTime (c.ODate).ToShortDateString ();
					string s = $"Details - # {c.CustNo}, Bank #@{c.BankNo}, Customer # {c.CustNo}, £{c.Balance}, {c.IntRate}%,  {date}";
					UpdateDbSelector (s);
				}
			}
		}
		#endregion grid row selection code

		private void UpdateDbSelector (string data)
		{
			ListBoxItem lbi = new ListBoxItem ();
			for (int i = 0; i < MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count - 1; i++)
			{
				lbi = MainWindow.DbSelectorOpen.ViewersList.Items[i + 1] as ListBoxItem;
				int lbtag = MainWindow.gv.ListBoxId[i];
				if ((int)this.Tag == lbtag)
				{
					//got the matching entry, update its "Content" field
					MainWindow.DbSelectorOpen.ListBoxItemText = data;
					lbi.Content = data;
					break;
				}
			}
		}

		private void Edit_Click (object sender, RoutedEventArgs e)
		{
			// Open Edit Window for the current record in SqlDbViewer DataGrid
			if (CurrentDb == "BANKACCOUNT")
			{
				EditDb edb = new EditDb ("BANKACCOUNT", BankGrid.SelectedIndex, BankGrid.SelectedItem, this);
				BankAccountViewModel.EditdbWndBank = edb;
				edb.Owner = this;
				if (BankGrid.SelectedIndex >= 0)
				{
					edb.DataContext = BankGrid.SelectedItem;
					edb.Show ();
					EditDataGrid = edb.DataGrid1;
				}
			}
			else if (CurrentDb == "CUSTOMER")
			{
				EditDb edb = new EditDb ("CUSTOMER", CustomerGrid.SelectedIndex, CustomerGrid.SelectedItem, this);
				BankAccountViewModel.EditdbWndBank = edb;
				edb.Owner = this;
				if (CustomerGrid.SelectedIndex >= 0)
				{
					edb.DataContext = CustomerGrid.SelectedItem;
					edb.Show ();
					EditDataGrid = edb.DataGrid2;
				}
			}
			else if (CurrentDb == "DETAILS")
			{
				EditDb edb = new EditDb ("DETAILS", DetailsGrid.SelectedIndex, DetailsGrid.SelectedItem, this);
				BankAccountViewModel.EditdbWndBank = edb;
				edb.Owner = this;
				if (DetailsGrid.SelectedIndex >= 0)
				{
					edb.DataContext = DetailsGrid.SelectedItem;
					edb.Show ();
					EditDataGrid = edb.DataGrid1;
				}
			}
		}

		public void EditDbClosing ()
		{
			BankAccountViewModel.EditdbWndBank = null;
		}

	#region EVENTHANDLERS
		/// <summary>
		///  This is triggered by an Event started by EditDb changing it's SelectedIndex
		///  to let us update our selectuon in line with it
		/// </summary>
		/// <param name="selectedRow"></param>
		/// <param name="caller"></param>
		public void resetSQLDBindex (bool self, int RowSelected, DataGrid caller)
		{
			//This is received when a change is made in EditDb DataGrid
			// and handles selecting the new index row and scrolling it into view
			int id1 = 0;
			int id2 = 0;

			// Don't botther if we triggered the selection change
			//			if (Flags.isEditDbCaller) 
			//				return;
			// use our Global Grid pointer for access
			if (caller.Name == "BankGrid")
			{
				//				Console.WriteLine ($"SqlDbViewer - (RESETSQLDBINDEX HANDLER 2065) - Called by {caller.Name} \r\nCurrent index = {BankGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER 2330) - Called by {caller.Name} \r\n" +
					"Current index = {BankGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");
				}
				if (BankGrid.SelectedIndex != RowSelected)
				{
					if (BankAccountViewModel.CurrentEditDbViewerBankGrid.SelectedIndex != RowSelected)
						BankGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (BankGrid, RowSelected, -1);
					ScrollList (BankGrid, -1);
					if (BankAccountViewModel.SqlUpdating)
					{
						//Need to update the grid as EDITDB has made a change
						//						Console.WriteLine ($"\r\nSQLDBVIEWER (2092) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						CollectionViewSource.GetDefaultView (BankGrid.ItemsSource).Refresh ();
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
				if (BankGrid.SelectedIndex != -1 && BankGrid.SelectedIndex != RowSelected)
				{
					if (BankAccountViewModel.CurrentEditDbViewerBankGrid.SelectedIndex != RowSelected)
						BankGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (BankGrid, RowSelected, -1);
					ScrollList (BankGrid, -1);
					if (BankGrid.ItemsSource != null)
					{
						//Need to update the grid as EDITDB has made a change
						//						Console.WriteLine ($"\r\nSQLDBVIEWER (2092) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						CollectionViewSource.GetDefaultView (BankGrid.ItemsSource).Refresh ();
						BankAccountViewModel.SqlUpdating = false;
					}


				}
			}
			else if (caller.Name == "DataGrid2")
			{
				//this is a customer account viewer grid !!!!
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"\r\n*** EVENTHANDLER *** - SqlDbViewer - (RESETSQLDBINDEX HANDLER (2371) - Called by {caller.Name} - \r\nCurrent index = {CustomerGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");
				}
				if (BankAccountViewModel.CurrentEditDbViewerCustomerGrid == null)
					return;
				if (CustomerGrid.SelectedIndex != RowSelected && CustomerGrid.SelectedIndex != -1)
				{
					if (BankAccountViewModel.CurrentEditDbViewerCustomerGrid.SelectedIndex != RowSelected)
						//						if (BankAccountViewModel.CurrentEditDbCustomerGrid.SelectedIndex != RowSelected)
						CustomerGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (CustomerGrid, RowSelected, -1);
					ScrollList (CustomerGrid, -1);

					if (BankAccountViewModel.SqlUpdating)
					{
						//Need to update the grid as EDITDB has made a change
						//						Console.WriteLine ($"\r\nSQLDBVIEWER (2092) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						CollectionViewSource.GetDefaultView (CustomerGrid.ItemsSource).Refresh ();
						BankAccountViewModel.SqlUpdating = false;

					}
				}
			}
			else if (caller.Name == "DetailsGrid")
			{
				Console.WriteLine ($"SqlDbViewer - (RESETSQLDBINDEX HANDLER 2076) - Called by {caller.Name} - \r\nCurrent index = {DetailsGrid.SelectedIndex} received row change of {RowSelected} from EditDb()for {caller.CurrentItem}");

				if (DetailsGrid.SelectedIndex != RowSelected)
				{
					if (BankAccountViewModel.CurrentEditDbViewerDetailsGrid.SelectedIndex == null)
						BankAccountViewModel.CurrentEditDbViewerDetailsGrid.SelectedIndex = RowSelected;
					if (BankAccountViewModel.CurrentEditDbViewerDetailsGrid.SelectedIndex != RowSelected)
						DetailsGrid.SelectedIndex = RowSelected;
					DataGridNavigation.SelectRowByIndex (DetailsGrid, RowSelected, -1);
					ScrollList (DetailsGrid, -1);

					if (DetailsViewModel.SqlUpdating)
					{
						//Need to update the grid as EDITDB has made a change
						if (Flags.EventHandlerDebug)
						{
							Console.WriteLine ($"\r\nSQLDBVIEWER (2088) RESETETSQLDBINDEX HANDLER() - Calling CollectionViewSource Function\r\n");
						}
						CollectionViewSource.GetDefaultView (DetailsGrid.ItemsSource).Refresh ();
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
		#endregion EVENTHANDLERS

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				return;
			else
			{
				e.Handled = false;
				//				base.OnKeyDown (e);
			}
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
					BankAccountViewModel.CurrentSQLViewerBankGrid = null;
					BankAccountViewModel.ActiveSqlDbViewer = null;
					BankAccountViewModel.ClearFromSqlList (BankGrid, CurrentDb);
					//					Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
				}
				else if (CurrentDb == "CUSTOMER")
				{
					BankAccountViewModel.CurrentSQLViewerCustomerGrid = null;
					BankAccountViewModel.ActiveSqlDbViewer = null;
					BankAccountViewModel.ClearFromSqlList (CustomerGrid, CurrentDb);
					//					Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
				}
				else if (CurrentDb == "DETAILS")
				{
					BankAccountViewModel.CurrentSQLViewerDetailsGrid = null;
					BankAccountViewModel.ActiveSqlDbViewer = null;
					BankAccountViewModel.ClearFromSqlList (DetailsGrid, CurrentDb);
				}
				EventHandlers.ClearWindowHandles (null, this);
				BankAccountViewModel.EditdbWndBank = null;
				//Set global flags
				//				Flags.SetGlobalFlags (this, DetailsGrid, CurrentDb);
				Close ();
			}
			else if (e.Key == Key.RightAlt)
			{
				Flags.ListFlags ();
			}
			else
				e.Handled = false;
			Console.WriteLine ($"{e.Key}");
		}
		private int _sequentialId
		{
			get { return _sequentialId; }
			set { _sequentialId = value; }
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
			BankAccountViewModel bvm = dgr.Item as BankAccountViewModel;
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
			DetailsViewModel bvm = dgr.Item as DetailsViewModel;
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

		private void CustomerGrid_BeginningEdit (object sender, DataGridBeginningEditEventArgs e)
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e.Row.GetIndex ();
			OriginalCellColumn = e.Column.DisplayIndex;
			DataGridColumn dgc = e.Column as DataGridColumn;
			string name = dgc.SortMemberPath;
			DataGridRow dgr = e.Row;
			CustomerViewModel bvm = dgr.Item as CustomerViewModel;
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
				case "FNAME":
					OriginalCellData = bvm.FName;
					break;
				case "LNAME":
					OriginalCellData = bvm.LName;
					break;
				case "ADDR1":
					OriginalCellData = bvm.Addr1;
					break;
				case "ADDR2":
					OriginalCellData = bvm.Addr2;
					break;
				case "TOWN":
					OriginalCellData = bvm.Town;
					break;
				case "COUNTY":
					OriginalCellData = bvm.County;
					break;
				case "PCODE":
					OriginalCellData = bvm.PCode;
					break;
				case "PHONE":
					OriginalCellData = bvm.Phone;
					break;
				case "MOBILE":
					OriginalCellData = bvm.Mobile;
					break;
				case "DOB":
					OriginalCellData = bvm.Dob;
					break;
				case "ODATE":
					OriginalCellData = bvm.ODate;
					break;
				case "CDATE":
					OriginalCellData = bvm.CDate;
					break;
			}
		}
		#endregion CellEdit Checker functions

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
				BankAccountViewModel bvm = new BankAccountViewModel ();
				cellValue = DataGridSupport.GetCellContent (sender, e, CurrentDb, out row, out col, out colName, out rowdata);
				if (row == -1)
					row = BankGrid.SelectedIndex;
			}
			else if (CurrentDb == "CUSTOMER")
			{
				CustomerViewModel bvm = new CustomerViewModel ();
				cellValue = DataGridSupport.GetCellContent (sender, e, CurrentDb, out row, out col, out colName, out rowdata);
				if (row == -1)
					row = BankGrid.SelectedIndex;
			}
			else if (CurrentDb == "DETAILS")
			{
				CustomerViewModel bvm = new CustomerViewModel ();
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

		#region Data Startup/Load function
		//*****************************************************************************************//
		/// <summary>
		/// We are starting initial loading of a Db into a Grid....
		/// Updates the MainWindow.GridViewer structure data, called
		/// by the 3  different "Show xxxxx" Funstion's
		/// </summary>
		/// <param name="type"></param>
		//*****************************************************************************************//
		public void LoadBankData ()
		{
			Debug.WriteLine ($"LoadDetailsData callback has been called successfully\r\nNow loading data into system....");
			//			BankAccountViewModel.LoadBankIntoList (BankAccountViewModel.BankList);
			// Now our List<DetailsViewModel is populated
			//			BankAccountViewModel.LoadBankObsCollection ();
			//Make the List<> data Observable
			BankAccountViewModel.BankAccountObs = new ObservableCollection<BankAccountViewModel> (BankAccountViewModel.BankList);
			// Refresh grid ???
			Debug.WriteLine ($"Callback has Finished loading data into system....");
			FinishBankWindowLoad ();
			StatusBar.Text = "";
			//			FinaliseBankLoad ("Bank Data Loaded....");
			Mouse.OverrideCursor = Cursors.Arrow;
		}
		//public   void LoadCustData ()
		//{
		//	Debug.WriteLine ($"LoadCustData callback has been called successfully\r\nNow loading data into system....");
		//	// load data into List<Customer>
		//	CustomerViewModel.LoadCustomersIntoList ();
		//	CustomerViewModel.LoadCustomerObsCollection ();
		//	//Make the List<> data Observable
		//	CustomerViewModel.CustomersObs = new ObservableCollection<CustomerViewModel> (CustomerViewModel.CustomersList);
		//	// Refresh grid ???
		//	Debug.WriteLine ($"Callback has Finished loading data into system....");
		//	FinishCustWindowLoad ();
		//	StatusBar.Text = "";
		//	Mouse.OverrideCursor = Cursors.Arrow;
		//}

		public void LoadCustData ()
		{
			Debug.WriteLine ($"LoadCustData callback has been called successfully\r\nNow loading data into system....");
			// load data into List<Customer>
			CustomerViewModel.LoadCustomersIntoList ();
			// load our data into Observable Collection
			CustomerViewModel.LoadCustomerObsCollection ();

			Debug.WriteLine ($"Callback has Finished loading data into system....");
			//			FinishCustWindowLoad ();
			//			StatusBar.Text = "";
			Debug.WriteLine ($" *** Current Active...2 =  {Flags.ActiveSqlGridStr}\r\n");
			Debug.WriteLine ($" *** Alll Customer data is loaded into Obs here v\r\n");
			FinishCustWindowLoad ();
			Mouse.OverrideCursor = Cursors.Arrow;
			//*****************************************************************************//
			// We now have ALL data into the Observable Collection
			//*****************************************************************************//
		}

		public void LoadDetailsData ()
		{
			Debug.WriteLine ($"LoadDetailsData callback has been called successfully\r\nNow loading data into system....");
			if (DetailsViewModel.DetailsList.Count == 0)
				DetailsViewModel.LoadDetailsIntoList (DetailsViewModel.DetailsList);
			// Now our List<DetailsViewModel is populated
			DetailsViewModel.LoadDetailsObsCollection ();
			//Make the List<> data Observable
			DetailsViewModel.DetailsObs = new ObservableCollection<DetailsViewModel> (DetailsViewModel.DetailsList);
			// Refresh grid ???
			Debug.WriteLine ($"Callback has Finished loading data into system....");
			UpdateAuxilliaries ("Details  Data Loaded...");

			StatusBar.Text = "";
			FinishDetWindowLoad ();
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		public void FinishBankWindowLoad ()
		{
			BankAccountViewModel.CurrentSQLViewerBankGrid = BankGrid;
			BankAccountViewModel.ActiveSqlDbViewer = BankGrid;
			//Setup global flags
#pragma FLAGS
			Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
		}
		public void FinishCustWindowLoad ()
		{
			BankAccountViewModel.CurrentSQLViewerCustomerGrid = CustomerGrid;
			BankAccountViewModel.ActiveSqlDbViewer = CustomerGrid;
			//Setup global flags
#pragma FLAGS
			Flags.SetGlobalFlags (this, CustomerGrid, CurrentDb);
		}

		public void FinishDetWindowLoad ()
		{
			BankAccountViewModel.CurrentSQLViewerDetailsGrid = DetailsGrid;
			BankAccountViewModel.ActiveSqlDbViewer = BankGrid;
			//Setup global flags
#pragma FLAGS
			Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
		}
	#endregion Data Startup/Load function

		private void TextBlock_RequestBringIntoView (object sender, RequestBringIntoViewEventArgs e)
		{
			this.Show ();
			this.BringIntoView ();
		}

	// NOT being called at present 2/4/21
		//**************************************************************************************************************************************************************//
		public void FinaliseBankLoad (string comment)
		{
			//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
			BankAccountViewModel.CurrentSQLViewerBankGrid = BankGrid;
			BankAccountViewModel.ActiveSqlDbViewer = BankGrid;

			BankGrid.ItemsSource = CollectionViewSource.GetDefaultView (BankAccountViewModel.BankAccountObs);
			BankGrid.Visibility = Visibility.Visible;
			MainWindow.gv.Datagrid[LoadIndex] = this.BankGrid;
			//just update the Buttons text
			ParseButtonText (true);
			IsFiltered = "";
			//set filter/Multi buttons ++
			ResetOptionButtons ();
			UpdateDbSelector (this);
			//Update the DbSelector listboxitems structure and our GridViewer control Structure
			if (IsviewerLoaded == false)
				UpdateGridviewController ("BANKACCOUNT");
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			BankGrid.SelectedIndex = 0;
			if (BankGrid.CurrentItem == null)
			{
				BankGrid.CurrentItem = 0;
				MainWindow.DgControl.SelectedIndex = 0;
				MainWindow.DgControl.SelectedItem = 0;
				MainWindow.DgControl.SelectedGrid = null;
			}
			BankGrid.SelectedItem = 0;
			UpdateViewersList ();
			CollectionViewSource.GetDefaultView (BankGrid.ItemsSource).Refresh ();
			//Set global flags
			Flags.SetGlobalFlags (this, BankGrid, CurrentDb);
			StatusBar.Text = comment;
		}
		private void ItemsView_OnSelectionChanged (object sender, SelectionChangedEventArgs e)
		//User has clicked a row in our DataGrid OR in EditDb grid
		{
			//declare vars to hold item data from relevant Classes
			// Get pointer to the Datagrid
			var datagrid = sender as DataGrid;
			if (datagrid == null) return;
			//Get the NEW selected index
			var index = datagrid.SelectedIndex;
			if (index == -1)
			{ Console.WriteLine ("OnSelectionChanged ERROR - SelectedIndex == -1\r\n"); return; }

			Flags.isEditDbCaller = true;


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
						MainWindow.DgControl.SelectedIndex = index;
						MainWindow.DgControl.SelectedItem = BankGrid?.SelectedItem;
						MainWindow.DgControl.SelectedGrid = BankGrid;
						MainWindow.DgControl.CurrentSqlGrid = BankGrid;
						MainWindow.DgControl.SqlSelChange = true;
						MainWindow.DgControl.EditSelChange = false;
						//Dispatcher.Invoke (() =>
						//{
						//	edb.UpdateSelection (index, BankGrid.SelectedItem);
						//});
						//SelectionChanged (index, BankGrid.SelectedItem);
						EventHandler.SqlDbTriggerEvent (Flags.isEditDbCaller, index, BankGrid, BankAccountViewModel.SqlUpdating);
					}
				}
				else if (CurrentDb == "CUSTOMER")
				{
					if (EventHandler != null)
					{
						//Set global  of the ID we want to find
						//						BankAccountViewModel.CurrentSelectedIndex = index;
						MainWindow.DgControl.SelectedIndex = index;
						MainWindow.DgControl.SelectedItem = CustomerGrid?.SelectedItem;
						MainWindow.DgControl.SelectedGrid = CustomerGrid;
						MainWindow.DgControl.CurrentSqlGrid = CustomerGrid;
						MainWindow.DgControl.SqlSelChange = true;
						MainWindow.DgControl.EditSelChange = false;
						//Dispatcher.Invoke (() =>
						//{
						//	edb.UpdateSelection (index, CustomerGrid.SelectedItem);
						//});

						EventHandler.SqlDbTriggerEvent (Flags.isEditDbCaller, index, CustomerGrid, CustomerViewModel.SqlUpdating);
					}
				}
				else if (CurrentDb == "DETAILS")
				{
					if (EventHandler != null)
					{
						//Set global  of the ID we want to find
						BankAccountViewModel.CurrentSelectedIndex = index;
						MainWindow.DgControl.SelectedIndex = index;
						MainWindow.DgControl.SelectedItem = DetailsGrid?.SelectedItem;
						MainWindow.DgControl.SelectedGrid = DetailsGrid;
						MainWindow.DgControl.CurrentSqlGrid = DetailsGrid;
						MainWindow.DgControl.SqlSelChange = true;
						MainWindow.DgControl.EditSelChange = false;
						//						SelectionChanged (index, DetailsGrid.SelectedItem);
						//Dispatcher.Invoke (() =>
						//{
						//	edb.UpdateSelection (index, DetailsGrid.SelectedItem);
						//});
						EventHandler.SqlDbTriggerEvent (Flags.isEditDbCaller, index, DetailsGrid, BankAccountViewModel.SqlUpdating);
					}
				}
				SqlUpdating = false;
			}
			else
			{
				//ONLY called if EditDb is NOT OPEN

				CustomerViewModel custacct = null;
				BankAccountViewModel bankacct = null;
				DetailsViewModel detsacct = null;
				int CurrentId = 0;
				if (datagrid.Name == "CustomerGrid")
				{
					if (CustomerGrid.SelectedItem != null)
					{
						custacct = CustomerGrid?.SelectedItem as CustomerViewModel;
						if (custacct != null)
						{
							CurrentId = custacct.Id;
							Flags.bvmCustRecord = custacct;
						}
						MainWindow.DgControl.SelectedIndex = BankGrid.SelectedIndex;
						MainWindow.DgControl.SelectedItem = BankGrid?.SelectedItem;
						//					MainWindow.DgControl.SelectedRow= CustomerGrid?.S;

					}
				}
				else if (datagrid.Name == "BankAccountGrid" || datagrid.Name == "BankGrid")
				{
					if (BankGrid.SelectedItem != null)
					{
						//Get copy of entire BankAccvount record
						bankacct = BankGrid?.SelectedItem as BankAccountViewModel;
						if (bankacct != null)
						{
							CurrentId = bankacct.Id;
							Flags.bvmBankRecord = bankacct;
						}
						MainWindow.DgControl.SelectedIndex = CustomerGrid.SelectedIndex;
						MainWindow.DgControl.SelectedItem = CustomerGrid?.SelectedItem;
						MainWindow.DgControl.SelectedGrid = CustomerGrid;
					}
				}
				else if (datagrid.Name == "DetailsGrid")
				{
					{
						if (DetailsGrid?.SelectedItem != null)
						{
							detsacct = DetailsGrid?.SelectedItem as DetailsViewModel;
							if (detsacct != null)
							{
								CurrentId = detsacct.Id;
								Flags.bvmDetRecord = detsacct;
							}
							MainWindow.DgControl.SelectedIndex = DetailsGrid.SelectedIndex;
							MainWindow.DgControl.SelectedItem = DetailsGrid?.SelectedItem;
							MainWindow.DgControl.SelectedGrid = DetailsGrid;
						}
					}
				}

				//Update the Loading window content for "Viewers Open"
				if (Flags.EventHandlerDebug)
				{
					Console.WriteLine ($"\r\n\r\nSqlDbViewer(1810) - ONSELECTIONCHANGED() - index changed\r\n");
					Console.WriteLine ($"Selected Index has changed, NOT using Event handler");
				}
				if (CurrentDb == "BANKACCOUNT")
				{
					//					BankAccountViewModel b = (BankAccountViewModel)BankGrid.SelectedItem;
					if (BankAccountViewModel.EditdbWndBank != null)
					{
						//Update the string in the Viewer Selection list of DbSelector
						BankAccountViewModel.EditdbWndBank.DataGrid1.SelectedItem = BankGrid.SelectedItem;
						BankAccountViewModel.EditdbWndBank.DataGrid1.SelectedIndex = BankGrid.SelectedIndex;
						CurrentBankSelectedRecord = BankGrid.SelectedItem as BankAccountViewModel;
					}
					else
						CurrentBankSelectedRecord = BankGrid.SelectedItem as BankAccountViewModel;
					if (BankGrid.SelectedItem != null)
						UpdateRowDetails (BankGrid.SelectedItem, "BankGrid");
					//we now have FULL PrettyDetails
				}
				else if (CurrentDb == "CUSTOMER")
				{
					//CustomerViewModel b = (CustomerViewModel)CustomerGrid.SelectedItem;
					if (BankAccountViewModel.EditdbWndBank != null)
					{
						//Update the string in the Viewer Selection list of DbSelector
						EditDataGrid.SelectedItem = CustomerGrid.SelectedItem;
						EditDataGrid.SelectedIndex = CustomerGrid.SelectedIndex;
						CurrentCustomerSelectedRecord = CustomerGrid.SelectedItem as CustomerViewModel;
					}
					else
						CurrentCustomerSelectedRecord = CustomerGrid.SelectedItem as CustomerViewModel;
					if (CustomerGrid.SelectedItem != null)
						UpdateRowDetails (CustomerGrid.SelectedItem, "CustomerGrid");
				}
				else if (CurrentDb == "DETAILS")
				{
					//					DetailsViewModel b = (DetailsViewModel)DetailsGrid?.SelectedItem;
					if (BankAccountViewModel.EditdbWndBank != null)
					{
						if (DetailsGrid.SelectedItem != null)
							BankAccountViewModel.EditdbWndBank.DataGrid1.SelectedItem = DetailsGrid.SelectedItem;
						BankAccountViewModel.EditdbWndBank.DataGrid1.SelectedIndex = DetailsGrid.SelectedIndex;
					}
					else
						CurrentDetailsSelectedRecord = DetailsGrid.SelectedItem as DetailsViewModel;

					if (DetailsGrid.SelectedItem != null)
						UpdateRowDetails (DetailsGrid.SelectedItem, "DetailsGrid");
				}
			}
			UpdateAuxilliaries ($"Selection changed to row {index}");
			Mouse.OverrideCursor = Cursors.Arrow;

		}

		public void UpdateAuxilliaries (string comment)
		{
			//Application.Current.Dispatcher.Invoke (() =>
			ParseButtonText (true);
			IsFiltered = "";
			ResetOptionButtons ();
			UpdateDbSelector (this);
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if (IsviewerLoaded == false)
				UpdateGridviewController (CurrentDb);
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			if (CurrentDb == "BANKACCOUNT")
			{
				if (BankAccountViewModel.BankAccountObs == null) return;
				BankGrid.ItemsSource = CollectionViewSource.GetDefaultView (BankAccountViewModel.BankAccountObs);
				//  paint the grid onscreen
				CollectionViewSource.GetDefaultView (BankGrid.ItemsSource).Refresh ();
				MainWindow.gv.Datagrid[LoadIndex] = this.BankGrid;
				BankGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
				MainWindow.DgControl.SelectedIndex = BankGrid.SelectedIndex;
				MainWindow.DgControl.SelectedItem = BankGrid.SelectedItem;
				MainWindow.DgControl.SelectedGrid = BankGrid;
				CollectionViewSource.GetDefaultView (BankGrid.ItemsSource).Refresh ();
			}
			if (CurrentDb == "CUSTOMER")
			{
				if (CustomerViewModel.CustomersObs == null) return;
				CustomerGrid.ItemsSource = CollectionViewSource.GetDefaultView (CustomerViewModel.CustomersObs);
				//  paint the grid onscreen
				CollectionViewSource.GetDefaultView (CustomerGrid.ItemsSource).Refresh ();
				MainWindow.gv.Datagrid[LoadIndex] = this.CustomerGrid;
				CustomerGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
				MainWindow.DgControl.SelectedIndex = CustomerGrid.SelectedIndex;
				MainWindow.DgControl.SelectedItem = CustomerGrid.SelectedItem;
				MainWindow.DgControl.SelectedGrid = CustomerGrid;
				CollectionViewSource.GetDefaultView (CustomerGrid.ItemsSource).Refresh ();
			}
			if (CurrentDb == "DETAILS")
			{
				if (DetailsViewModel.DetailsObs == null) return;
				DetailsGrid.ItemsSource = CollectionViewSource.GetDefaultView (DetailsViewModel.DetailsObs);
				//  paint the grid onscreen
				CollectionViewSource.GetDefaultView (DetailsGrid.ItemsSource).Refresh ();
				MainWindow.gv.Datagrid[LoadIndex] = this.DetailsGrid;
				DetailsGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
				MainWindow.DgControl.SelectedIndex = DetailsGrid.SelectedIndex;
				MainWindow.DgControl.SelectedItem = DetailsGrid.SelectedItem;
				MainWindow.DgControl.SelectedGrid = DetailsGrid;
				CollectionViewSource.GetDefaultView (DetailsGrid.ItemsSource).Refresh ();
			}
			UpdateViewersList ();
			StatusBar.Text = comment;
			WaitMessage.Visibility = Visibility.Collapsed;
		}
		private void ShowBank_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.RightAlt)
			{
				Flags.ListFlags ();
			}
			else if (e.Key == Key.Home)
				Application.Current.Shutdown ();
			else
				e.Handled = false;


		}
		public void startup1 (int selectedDb)
		//Never called
		{
			switch (selectedDb)
			{
				case 0:
					CurrentDb = "BANKACCOUNT";
					new EventHandlers (BankGrid, "BANKACOUNT", out EventHandler);
					//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
					BankAccountViewModel.CurrentSQLViewerBankGrid = BankGrid;
					BankAccountViewModel.ActiveSqlDbViewer = BankGrid;
					LoadBankData ();
					break;
				case 1:
					CurrentDb = "CUSTOMER";
					new EventHandlers (CustomerGrid, "CUSTOMER", out EventHandler);
					//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
					BankAccountViewModel.CurrentSQLViewerCustomerGrid = CustomerGrid;
					BankAccountViewModel.ActiveSqlDbViewer = CustomerGrid;
					break;
				case 2:
					CurrentDb = "DETAILS";
					new EventHandlers (DetailsGrid, "DETAILS", out EventHandler);
					//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
					BankAccountViewModel.CurrentSQLViewerDetailsGrid = DetailsGrid;
					BankAccountViewModel.ActiveSqlDbViewer = DetailsGrid;
					break;
				default:
					break;
			}
		}
	}
}


