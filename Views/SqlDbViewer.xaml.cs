using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using WPFPages.DataSources;
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
		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";
		public bool FilterResult = false;
		private int CurrentGridViewerIndex = -1;
		public string CurrentDb = "BANKACCOUNT";
		public SqlDataAdapter sda;
		public DataTable dtBank = new System.Data.DataTable ("BankAccountDataTable");
		public DataTable dtCust = new DataTable ("CustomerDataTable");
		public DataTable dtDetails = new DataTable ("DetailsDataTable");
		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";
		private BankAccount BankCurrentRowAccount;
		private Customer CustomerCurrentRowAccount;
		private SecAccounts DetailsCurrentRowAccount;
		private DataGridRow BankCurrentRow;
		private DataGridRow CustomerCurrentRow;
		private DataGridRow DetailsCurrentRow;
		public Window ThisWindow = new Window ();
		public bool isMultiMode = false;
		private bool IsLoaded = false;
		private int LoadIndex = -1;

		public EditDb EditdbWnd = null;
		public DataGrid EditDataGrid = null;

		private static SqlDbViewer sqldbForm = null;

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion setup


		/// <summary>
		/// Used to keep track of entries in our DbSelector Open Viewers list descriptions etc
		/// </summary>
		public static int SequentialId = 12345;
		event SQLViewerGridSelectionChanged SQLVSelChange;

		public newtester nt = null;
		private static bool SelectionIsChanging = false;
		//Dummy Constructor for Event handlers
		public SqlDbViewer () { sqldbForm = this; }
		private void handleselection (int x)
		{
			Console.WriteLine ("Delegate received " + x);
		}
		#region load/startup
		//*****************************************************************************************//
		public SqlDbViewer (int selection)
		{
			int selectedDb = -1;
			IsLoaded = false;
			InitializeComponent ();
			EditDb edb = new EditDb ();

			#region delegate testing
			//			SQLVSelChange += new SQLViewerGridSelectionChanged  (edb.resetindex);
			//Subscribe to the SelectedIndex changed event

			//			nt.SqlDbTriggerEvent ();


			// instantiate our Event handler class
			//			MonitorDataGridSelectionChanges mdgs = new MonitorDataGridSelectionChanges ();

			////This is a delegate ONLY declared in Class Tester in EventHandlers.CS
			////I only need an instance of Tester so I can access the handler IndexChange()
			//tester tst = new tester ();
			//SQLViewerGridSelectionChanged  gsc = tst.IndexChange;
			//gsc (23);
			//gsc = handleselection;
			//gsc (1243);
			//		mdgs.IndexChange (456);

			//**************************************************************************//
			////Delegate testing - declare an instance of the class
			//DelegateClass dc = new DelegateClass ();
			//// Called normally ....
			//dc.AddNums (3, 6);
			//string s = DelegateClass.SayHello ("this is me");
			//Console.WriteLine (s);

			////now call by delegate
			//// we can call the Static directly via class name
			//SayDelegate sd = new SayDelegate (DelegateClass.SayHello);
			//// but we need to create an instance of the class containking
			//// the delegate methods so we can access the NON static function AddNums()
			//AddDelegate ad = new AddDelegate (dc.AddNums);


			//// now call them both via the delegates
			//ad (455, 650);
			//Console.WriteLine (sd("Hi there") );
			//// we can also call them using .Invoke
			//ad.Invoke (66, 88);
			//Console.WriteLine (sd.Invoke ("gfhjdjdjfgfgg"));


			#endregion delegate testing

			//**************************************************************************//
			if (BankAccountObs == null)
				BankAccountObs = new ObservableCollection<BankAccount> ();
			if (CustomerObs == null)
				CustomerObs = new ObservableCollection<Customer> ();
			if (DetailsObs == null)
				DetailsObs = new ObservableCollection<SecAccounts> ();
			selectedDb = selection;
			if (selectedDb != -1)
			{
				switch (selectedDb)
				{
					case 0:
						CurrentDb = "BANKACCOUNT";
						new newtester (BankGrid, "BANKACOUNT", out nt);
						break;
					case 1:
						CurrentDb = "CUSTOMER";
						new newtester (BankGrid, "CUSTOMER", out nt);
						break;
					case 2:
						CurrentDb = "DETAILS";
						new newtester (BankGrid, "DETAILS", out nt);
						break;
					default:
						break;
				}
			}
			ThisWindow = this;

		}

		private void OnWindowLoaded (object sender, RoutedEventArgs e)
		{
			if (!IsLoaded)
			{
				int currentindex = -1;
				this.MouseDown += delegate { DoDragMove (); };
				// Load details of new SqlDbViewer (DataGridViewer Window)
				// Into our GridViewer structure (Mainwindow.dv) 
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
					if (CurrentDb == "BANKACCOUNT")
					{
						ShowBank_Click (null, null);
						//						UpdateDbSelector (this);
						if (BankGrid.Items.Count > 0)
						{
							Focus ();
							BringIntoView ();
						}
						Console.WriteLine ($"SqlDbViewer(221) Window just loaded : getting instance of newtester class with this,BankGrid, \"SQLDBVIEWERDB\"");
						//Setup the Event handler to notify EditDb viewer of index changes
					}
					else if (CurrentDb == "CUSTOMER")
					{
						ShowCust_Click (null, null);
						//						UpdateDbSelector (this);
						if (CustomerGrid.Items.Count > 0)
						{
							Focus ();
							BringIntoView ();
						}
						Console.WriteLine ($"SqlDbViewer(233) Window just loaded : getting instance of newtester class with this,BankGrid, \"SQLDBVIEWERDB\"");
						//Setup the Event handler to notify EditDb viewer of index changes
						//						nt = new newtester (this, CustomerGrid, "SQLDBVIEWER");
					}
					else if (CurrentDb == "DETAILS")
					{
						ShowDetails_Click (null, null);
						//						UpdateDbSelector (this);

						if (DetailsGrid.Items.Count > 0)
						{
							Focus ();
							BringIntoView ();
						}
						Console.WriteLine ($"SqlDbViewer(208) Window just loaded : getting instance of newtester class with this, DetailsGrid, \"SQLDBVIEWERDB\"");
						//Setup the Event handler to notify EditDb viewer of index changes
						//						nt = new newtester (this, DetailsGrid, "SQLDBVIEWER");
					}
				}
				IsLoaded = true;
				newtester.SetWindowHandles (null, this);
			}
		}
		private int _sequentialId
		{
			get { return _sequentialId; }
			set { _sequentialId = value; }
		}
		/// <summary>
		/// Used to keep track of currently selected row in GridViwer
		/// </summary>
		private int _selectedRow;
		public int SelectedRow
		{
			get { return _selectedRow; }
			set { _selectedRow = value; OnPropertyChanged (SelectedRow.ToString ()); }
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
		#endregion load/startup

		#region load all data base data
		///<summary>
		/// BankAccount database
		/// </summary>
		private bool FillBankAccountDataGrid ()
		{
			string CmdString = "";
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
					CmdString = "SELECT * FROM BankAccount WHERE CUSTNO IN "
						+ "(SELECT CUSTNO FROM BANKACCOUNT "
						+ " GROUP BY CUSTNO"
						+ " HAVING COUNT(*) > 1)";
					//clear the datatable first as we are only showing a subset
					dtBank.Clear ();
				}
				else
				{
					CmdString = "SELECT * FROM BankAccount";
					//clear the datatable first as we are only showing a subset
					dtBank.Clear ();
				}
			}
			CurrentDb = "BANKACCOUNT";
			bool result = LoadSqlData (CmdString, dtBank, "BANKACCOUNT", isMultiMode, Filters, StatusBar, Multiaccounts);
			isMultiMode = false;
			return result;
		}

		//*****************************************************************************************//
		public bool LoadSqlData (string CmdString, DataTable dt, string CallerType, bool isMultiMode, Button Btn, TextBlock StatusBar, Button Multiaccounts)
		{
			try
			{
				SqlConnection con;
				string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				using (con = new SqlConnection (ConString))
				{
					SqlCommand cmd = new SqlCommand (CmdString, con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					//load the SQL data into our OC named bankaccounts
					if (CallerType == "BANKACCOUNT")
					{
						if (LoadBankAccountCollection (dt))
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
						if (LoadCustomerCollection (dt))
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
						if (LoadDetailsCollection (dt))
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
				return false;
			}
			return true;
		}

		//*****************************************************************************************//
		public bool LoadBankAccountCollection (DataTable dtBank)
		{
			if (BankAccountObs.Count > 0)
			{
				try
				{
					BankAccountObs.Clear ();
				}
				catch (Exception ex) { Console.WriteLine ($"Failed to load clear Bank Details Obslist - {ex.Message}"); }
			}
			//This DOES access the Bank/Account Class properties !!!!!
			for (int i = 0; i < dtBank.Rows.Count; ++i)
			{
				BankAccountObs.Add (new BankAccount
				{
					Id = Convert.ToInt32 (dtBank.Rows[i][0]),
					BankNo = dtBank.Rows[i][1].ToString (),
					CustNo = dtBank.Rows[i][2].ToString (),
					AcType = Convert.ToInt32 (dtBank.Rows[i][3]),
					Balance = Convert.ToDecimal (dtBank.Rows[i][4]),
					IntRate = Convert.ToDecimal (dtBank.Rows[i][5]),
					ODate = Convert.ToDateTime (dtBank.Rows[i][6]),
					CDate = Convert.ToDateTime (dtBank.Rows[i][7]),
				});
			}
			return true;
		}
		/// <summary>
		/// Customer database
		/// </summary>
		private bool FillCustomerDataGrid ()
		{
			string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			string CmdString = string.Empty;
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
					CmdString = "SELECT * FROM Customer WHERE CUSTNO IN "
						+ "(SELECT CUSTNO FROM CUSTOMER "
						+ " GROUP BY CUSTNO"
						+ " HAVING COUNT(*) > 1)";
					//clear the datatable first as we are only showing a subset
					dtCust.Clear ();
				}
				else
				{
					CmdString = "SELECT * FROM Customer";
					//clear the datatable first as we are only showing a subset
					dtDetails.Clear ();
				}
			}
			bool result = LoadSqlData (CmdString, dtCust, "CUSTOMER", isMultiMode, Filters, StatusBar, Multiaccounts);
			isMultiMode = false;
			return result;

		}
		//*****************************************************************************************//
		public bool LoadCustomerCollection (DataTable dtCust)
		{
			//Load the data into our ObservableCollection Customer
			try
			{
				if (CustomerObs.Count > 0)
					CustomerObs.Clear ();
			}
			catch
			{ }
			try
			{
				for (int i = 0; i < dtCust.Rows.Count; ++i)
					CustomerObs.Add (new Customer
					{
						Id = Convert.ToInt32 (dtCust.Rows[i][0]),
						CustNo = dtCust.Rows[i][1].ToString (),
						BankNo = dtCust.Rows[i][2].ToString (),
						AcType = Convert.ToInt32 (dtCust.Rows[i][3]),
						FName = dtCust.Rows[i][4].ToString (),
						LName = dtCust.Rows[i][5].ToString (),
						Addr1 = dtCust.Rows[i][6].ToString (),
						Addr2 = dtCust.Rows[i][7].ToString (),
						Town = dtCust.Rows[i][8].ToString (),
						County = dtCust.Rows[i][9].ToString (),
						PCode = dtCust.Rows[i][10].ToString (),
						Phone = dtCust.Rows[i][11].ToString (),
						Mobile = dtCust.Rows[i][12].ToString (),
						Dob = Convert.ToDateTime (dtCust.Rows[i][13]),
						ODate = Convert.ToDateTime (dtCust.Rows[i][14]),
						CDate = Convert.ToDateTime (dtCust.Rows[i][15])
					});
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Error loading Details Data {ex.Message}");
				return false;
			}

		}
		/// <summary>
		/// Secaccounts database
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
						+ " HAVING COUNT(*) > 1)";
					//clear the datatable first as we are only showing a subset
					dtDetails.Clear ();
				}
				else
				{
					CmdString = "SELECT * FROM SecAccounts";
					//clear the datatable first as we are only showing a subset
					dtDetails.Clear ();
				}
			}
			bool result = LoadSqlData (CmdString, dtDetails, "DETAILS", isMultiMode, Filters, StatusBar, Multiaccounts);
			isMultiMode = false;
			return true;

		}
		//*****************************************************************************************//
		public bool LoadDetailsCollection (DataTable dtDetails)
		{
			//Load the data into our ObservableCollection BankAccounts
			if (DetailsObs.Count > 0)
				DetailsObs.Clear ();
			try
			{
				for (int i = 0; i < dtDetails.Rows.Count; ++i)
					DetailsObs.Add (new SecAccounts
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
			if (EditdbWnd != null)
			{
				if (MessageBox.Show ("You have an Edit Window open for the current Database.\r\nThis will be closed if you proceed !", "Possible Data Loss",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.Cancel) == MessageBoxResult.OK)
				{
					// gotta close the edit window
					EditdbWnd.Close ();
					EditdbWnd = null;
				}
				else
					return;
			}
			Window_MouseDown (sender, null);
			CurrentDb = "BANKACCOUNT";
			DetailsGrid.Visibility = Visibility.Hidden;
			CustomerGrid.Visibility = Visibility.Hidden;
			BankGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			if (dtBank != null)
				dtBank.Clear ();
			if (!FillBankAccountDataGrid ())
				MessageBox.Show ("Failed to load BankAccount data from SQL Server");

			// Set up our data binding
			BankGrid.ItemsSource = BankAccountObs;

			MainWindow.gv.Datagrid[LoadIndex] = this.BankGrid;
			//just update the Buttons text
			ParseButtonText ();
			IsFiltered = "";
			//set filter/Multi buttons accessibility
			ResetOptionButtons ();
			UpdateDbSelector (this);
			//Update the DbSelector listboxitems structure and our GridViewer control Structure
			if (IsLoaded == false)
				UpdateGridviewController ("BANKACCOUNT");
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			BankGrid.SelectedIndex = 0;
			if (BankGrid.CurrentItem == null)
				BankGrid.CurrentItem = 0;
			BankGrid.SelectedItem = 0;
			UpdateViewersList ();
			//this.BringIntoView ();
			//this.Focus ();
		}
		//
		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowCust_Click (object sender, RoutedEventArgs e)
		{
			if (EditdbWnd != null)
			{
				if (MessageBox.Show ("You have an Edit Window open for the current Database.\r\nThis will be closed if you proceed !", "Possible Data Loss",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.Cancel) == MessageBoxResult.OK)
				{
					// gotta close the edit window
					EditdbWnd.Close ();
					EditdbWnd = null;
				}
				else
					return;
			}
			Window_MouseDown (sender, null);
			CurrentDb = "CUSTOMER";

			DetailsGrid.Visibility = Visibility.Hidden;
			BankGrid.Visibility = Visibility.Hidden;
			CustomerGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			if (dtCust != null)
				dtCust.Clear ();
			if (!FillCustomerDataGrid ())
			{
				MessageBox.Show ("Failed to load Customer data from SQL Server");
			}
			CustomerGrid.ItemsSource = CustomerObs;
			//			CurrentDb = "CUSTOMER";
			//			ShowCust.Tag = true;
			//			if (CustomerGrid.CurrentItem == null)
			//				CustomerGrid.CurrentItem = 0;
			MainWindow.gv.Datagrid[LoadIndex] = this.CustomerGrid;
			ParseButtonText ();
			IsFiltered = "";
			ResetOptionButtons ();
			UpdateDbSelector (this);
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if (IsLoaded == false)
				UpdateGridviewController ("CUSTOMER");
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			CustomerGrid.SelectedIndex = 0;
			if (CustomerGrid.CurrentItem == null)
				CustomerGrid.CurrentItem = 0;
			CustomerGrid.SelectedItem = 0;
			UpdateViewersList ();
			//this.BringIntoView ();
			//this.Focus ();
		}

		//*****************************************************************************************//
		/// <summary>
		/// Fetches SQL data for SecAccounts Db and fills relevant DataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDetails_Click (object sender, RoutedEventArgs e)
		{
			if (EditdbWnd != null)
			{
				if (MessageBox.Show ("You have an Edit Window open for the current Database.\r\nThis will be closed if you proceed !", "Possible Data Loss",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.Cancel) == MessageBoxResult.OK)
				{
					// gotta close the edit window
					EditdbWnd.Close ();
					EditdbWnd = null;
				}
				else
					return;
			}
			Window_MouseDown (sender, null);
			CurrentDb = "DETAILS";
			CustomerGrid.Visibility = Visibility.Hidden;
			BankGrid.Visibility = Visibility.Hidden;
			DetailsGrid.Visibility = Visibility.Visible;
			Filters.IsEnabled = true;
			if (dtDetails != null)
				dtDetails.Clear ();
			if (!FillDetailsDataGrid ())
			{
				MessageBox.Show ("Failed to load Details data from SQL Server");
			}
			DetailsGrid.ItemsSource = DetailsObs;
			//if (DetailsGrid.CurrentItem == null)
			//	DetailsGrid.CurrentItem = 0;
			MainWindow.gv.Datagrid[LoadIndex] = this.DetailsGrid;
			//			CurrentDb = "DETAILS";
			ParseButtonText ();
			IsFiltered = "";
			ResetOptionButtons ();
			UpdateDbSelector (this);
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if (IsLoaded == false)
				UpdateGridviewController ("DETAILS");
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ();
			DetailsGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
			DetailsGrid.SelectedIndex = 0;
			if (DetailsGrid.CurrentItem == null)
				DetailsGrid.CurrentItem = 0;
			DetailsGrid.SelectedItem = 0;
			UpdateViewersList ();
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
				dtBank.Clear ();
			}
			else if (CurrentDb == "CUSTOMER")
			{
				Commandline1 = $"Select * from Customer where ";
				dtCust.Clear ();
			}
			else if (CurrentDb == "DETAILS")
			{
				Commandline1 = $"Select * from SecAccounts where ";
				dtDetails.Clear ();
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
		private ObservableCollection<BankAccount> _bankAccountObs;
		public ObservableCollection<BankAccount> BankAccountObs
		{
			get { return _bankAccountObs; }
			set
			{
				_bankAccountObs = value;
				OnPropertyChanged (BankAccountObs.ToString ());
			}
		}
		private ObservableCollection<Customer> _customerObs;
		public ObservableCollection<Customer> CustomerObs
		{
			get { return _customerObs; }
			set
			{
				_customerObs = value;
				OnPropertyChanged (CustomerObs.ToString ());
			}
		}
		private ObservableCollection<SecAccounts> _detailsObs;
		public ObservableCollection<SecAccounts> DetailsObs
		{
			get { return _detailsObs; }
			set
			{
				_detailsObs = value;
				OnPropertyChanged (DetailsObs.ToString ());
			}
		}
		private BankAccount _selectedBankAccount;
		public BankAccount SelectedBankAccount
		{
			get { return _selectedBankAccount; }
			set
			{
				_selectedBankAccount = value;
				OnPropertyChanged (SelectedBankAccount.ToString ());
			}
		}

		private Customer _selectedCustomerAccount;
		public Customer SelectedCustomerAccount
		{
			get { return _selectedCustomerAccount; }
			set
			{
				_selectedCustomerAccount = value;
				OnPropertyChanged (SelectedBankAccount.ToString ());
			}
		}
		private SecAccounts _selectedDetailAccount;
		public SecAccounts SelectedDetailAccount
		{
			get { return _selectedDetailAccount; }
			set
			{
				_selectedDetailAccount = value;
				OnPropertyChanged (SelectedDetailAccount.ToString ());
			}
		}
		#endregion ObservableCollection setup

		#region DataGrid positioning code
		public static void SelectRowByIndex (DataGrid dataGrid, int rowIndex)
		{
			if (!dataGrid.SelectionUnit.Equals (DataGridSelectionUnit.FullRow))
				throw new ArgumentException ("The SelectionUnit of the DataGrid must be set to FullRow.");

			if (dataGrid.Items.Count == 0)
				return;
			if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
				throw new ArgumentException (string.Format ("{0} is an invalid row index.", rowIndex));
			//Crashes if the grid is set to sibgle selecton only
			//			dataGrid.SelectedItems.Clear ();
			/* set the SelectedItem property */
			object item = dataGrid.Items[rowIndex]; // = Product X
			dataGrid.SelectedItem = item;

			DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;
			if (row == null)
			{
				/* bring the data item (Product object) into view
				 * in case it has been virtualized away */
				dataGrid.ScrollIntoView (item);
				row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;
			}
			//DataGridCell cell = GetCell (dataGrid, row, 2);
			//if (cell != null)
			//	cell.Focus ();
			//TODO: Retrieve and focus a DataGridCell object
		}
		public static DataGridCell GetCell (DataGrid dataGrid, DataGridRow rowContainer, int column)
		{
			if (rowContainer != null)
			{
				DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter> (rowContainer);
				if (presenter == null)
				{
					/* if the row has been virtualized away, call its ApplyTemplate() method 
					 * to build its visual tree in order for the DataGridCellsPresenter
					 * and the DataGridCells to be created */
					rowContainer.ApplyTemplate ();
					presenter = FindVisualChild<DataGridCellsPresenter> (rowContainer);
				}
				if (presenter != null)
				{
					DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex (column) as DataGridCell;
					if (cell == null)
					{
						/* bring the column into view
						 * in case it has been virtualized away */
						dataGrid.ScrollIntoView (rowContainer, dataGrid.Columns[column]);
						cell = presenter.ItemContainerGenerator.ContainerFromIndex (column) as DataGridCell;
					}
					return cell;
				}
			}
			return null;
		}

		public static T FindVisualChild<T> (DependencyObject obj) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount (obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild (obj, i);
				if (child != null && child is T)
					return (T)child;
				else
				{
					T childOfChild = FindVisualChild<T> (child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

		//******************************************** end of row selection code ****************************//
		#endregion DataGrid positioning code

		//*****************************************************************************************//
		private void DataGrid_RowEditEnded (object sender, DataGridRowEditEndingEventArgs e)
		{
			///
			/// After a fight, this is now working and updates the relevant RECORD correctly
			/// 

			BankAccount ss = null;
			Customer cs = null;
			SecAccounts sa = null;

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if (e == null)
			{
				if (CurrentDb == "BANKACCOUNT")
				{
					ss = new BankAccount ();
					ss = BankCurrentRow.Item as BankAccount;
				}
				else if (CurrentDb == "DETAILS")
				{
					sa = new SecAccounts ();
					sa = DetailsCurrentRow.Item as SecAccounts;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					cs = new Customer ();
					cs = CustomerCurrentRow.Item as Customer;
				}
			}
			else
			{
				if (CurrentDb == "BANKACCOUNT")
				{
					ss = new BankAccount ();
					ss = e.Row.Item as BankAccount;
				}
				else if (CurrentDb == "DETAILS")
				{
					sa = new SecAccounts ();
					sa = e.Row.Item as SecAccounts;
				}
				else if (CurrentDb == "CUSTOMER")
				{
					cs = new Customer ();
					cs = e.Row.Item as Customer;
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
						//						sa = e.Row.Item as SecAccounts;
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
				SqlConnection conn = null;
				string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				string CmdString = string.Empty;
				try
				{
					using (conn = new SqlConnection (ConString))
					{
						conn.Open ();
						if (CurrentDb == "BANKACCOUNT")
						{
							cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE Id=@id", conn);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (ss.Id));
							cmd.Parameters.AddWithValue ("@bankno", ss.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", ss.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (ss.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (ss.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (ss.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (ss.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (ss.CDate));
						}
						else
						{
							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE Id=@id", conn);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
						}
						cmd.ExecuteNonQuery ();
						conn.Close ();
						StatusBar.Text = "Database updated successfully....";
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error - {ex.Message} Data = {ex.Data}");
					conn.Close ();
				}
				finally
				{
					//conn.Close();
				}
			}
			else if (CurrentDb == "CUSTOMER")
			{
				if (e == null && CurrentDb == "CUSTOMER")
					cs = CustomerCurrentRow.Item as Customer;
				else if (e == null && CurrentDb == "CUSTOMER")
					cs = e.Row.Item as Customer;


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
				SqlConnection conn = null;
				string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//	@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				string CmdString = string.Empty;
				try
				{
					using (conn = new SqlConnection (ConString))
					{
						conn.Open ();
						SqlCommand cmd = new SqlCommand ("UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
							"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
							"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", conn);


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
						conn.Close ();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error - {ex.Message} Data = {ex.Data}");
					conn.Close ();
				}
				finally
				{
					//conn.Close();
				}
			}

		}

		//*****************************************************************************************//
		private void CloseViewer_Click (object sender, RoutedEventArgs e)
		{
			//Close current (THIS) Viewer Window
			if (EditdbWnd != null)
			{
				if (MessageBox.Show ("There is a Database Edit Window currently open.\r\nDo you want to continue, which may loose any changes\r\nthat may have been made ?",
				"Possible Data loss",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question,
				MessageBoxResult.Cancel) == MessageBoxResult.No)
				{
					return;
				}
			}
			this.Close ();
		}

		//*****************************************************************************************//
		/// <summary>
		/// We are loading a Db into a Grid....
		/// Updates the MainWindow.GridViewer structure data, called
		/// by the 3  different "Show xxxxx" Funstion's
		/// </summary>
		/// <param name="type"></param>
		//*****************************************************************************************//
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
			if (MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count == 1)
				return;
			for (int i = 0; i < MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count; i++)
			{
				if (i + 1 == MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count)
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
		private void ParseButtonText ()
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
					if (isMultiMode)
						ShowBank.Content = $"<M> Bank A/c's  ({BankGrid.Items.Count})";
					else
						ShowBank.Content = $"Bank A/c's  ({BankGrid.Items.Count})";
				}
				else if (CurrentDb == "CUSTOMER")
				{
					if (isMultiMode)
						ShowCust.Content = $"<M> Customer A/c's  ({CustomerGrid.Items.Count})";
					else
						ShowCust.Content = $"Customer A/c's  ({CustomerGrid.Items.Count})";
				}
				else if (CurrentDb == "DETAILS")
				{
					if (isMultiMode)
						ShowDetails.Content = $"<M> Details A/c's  ({DetailsGrid.Items.Count})";
					else
						ShowDetails.Content = $"Details A/c's  ({DetailsGrid.Items.Count})";
				}
			}
		}
		//*****************************************************************************************//
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
				ShowCust.Tag = false;
				ShowDetails.Tag = true;
			}
		}

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
				DataRow dr = dtBank.NewRow ();
				dtBank.Rows.Add (dr);
				//				BankGrid.DataContext = dtBank;
			}
		}
		//*****************************************************************************************//
		private void ContextMenu2_Click (object sender, RoutedEventArgs e)
		{
			//Delete current Row
			BankAccount dg = sender as BankAccount;
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
				FillCustomerDataGrid ();
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
			BankAccount b = BankGrid.Items.CurrentItem as BankAccount;

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
		private string CellData (DataGridCellInfo cell)
		{
			return $"{ cell.Column.Header}, {cell.Item}";
		}

		/// <summary>
		///  This is triggered by an Event started by EditDb changing it's SelectedIndex
		///  to let us u0pdate our selectuon in line with it
		/// </summary>
		/// <param name="selectedRow"></param>
		/// <param name="caller"></param>
		public void resetSQLDBindex (int selectedRow, DataGrid caller)
		{
			//This is received when a change is made in EditDb DataGrid
			// and handles selecting the new index row and scrolling it into view
			int x = 0;
			x = selectedRow;
			// use our Global Grid pointer for access
			if (caller.Name == "BankGrid" || caller.Name == "DataGrid1" || caller.Name == "DetailsGrid")
			{
				Console.WriteLine ($"SqlDbViewer 2082) resetSQLDBindex - Called - Current index = {BankGrid.SelectedIndex} received row change of {x} from EditDb()for {caller.CurrentItem}");
				if (BankGrid.SelectedIndex != x)
				{
					BankGrid.SelectedIndex = x;
					SelectRowByIndex (BankGrid, x);
					ScrollList (BankGrid, -1);
				}
			}
			else if (caller.Name == "CustomerGrid2")
			{
				Console.WriteLine ($"SqlDbViewer 2087) resetSQLDBindex - Called by {caller} - - Current index = {CustomerGrid.SelectedIndex} received row change of {x} from EditDb()for {caller.CurrentItem}");
				if (CustomerGrid.SelectedIndex != x)
				{
					CustomerGrid.SelectedIndex = x;
					SelectRowByIndex ( CustomerGrid, x);
					ScrollList (CustomerGrid, -1);
				}
			}
		}

		private void ScrollList (DataGrid grid, int count)
		{
			int currentindex = grid.SelectedIndex;
			object selitem =  grid.SelectedItem;
			int maxrows = grid.Items.Count;
#pragma TODO
			//Temp
			return;

			if (grid.Items.Count > 0)
			{
//				grid.Refresh ();
				var border = VisualTreeHelper.GetChild( grid, 0) as Decorator;
				if (border != null)
				{
					var scroll = border.Child as ScrollViewer;
					if (scroll != null)
					{
//						grid.ScrollIntoView (grid.SelectedItem);
						if (currentindex + 5 < maxrows)
						{

							for (int i = 0; i < 5; i++)
							{ 
								scroll.LineDown(); scroll.UpdateLayout ();
							}
						}
						else if (currentindex  > 5 )
						{
							grid.ScrollIntoView (selitem); }
						}
						//scroll.ScrollToEnd ();
					}
				}
		}

			private void ItemsView_OnSelectionChanged (object sender, SelectionChangedEventArgs e)
			{

				//User has clicked a row in our DataGrid
				var dg = sender as DataGrid;
				if (dg == null) return;
				var index = dg.SelectedIndex;

				// THESE WORK JUST FINE
				// SO ON TO EVENTS NOW.....
				//This is a delegate ONLY declared in Class Tester in EventHandlers.CS
				//I only need an instance of Tester so I can access the handler IndexChange()
				//tester tst = new tester ();
				//SQLViewerGridSelectionChanged  gsc = tst.IndexChange;
				//gsc (index);
				//gsc = handleselection;
				//gsc (index);

				if (EditdbWnd != null)
				{
					//There is an EditDb window open, so this will trigger 
					//an event that lets the DataGrid in the EditDb class
					// change it's own index internally

					Console.WriteLine ($"SqlDbViewer(1662) index changed, calling nt.EditDbTriggerEvent for {CurrentDb}");
					if (CurrentDb == "BANKACCOUNT")
					{
						if (nt != null)
							nt.SqlDbTriggerEvent (index, BankGrid);
					}
					else if (CurrentDb == "CUSTOMER")
					{
						if (nt != null)
							nt.SqlDbTriggerEvent (index, CustomerGrid);
					}
					else if (CurrentDb == "DETAILS")
					{
						if (nt != null)
							nt.SqlDbTriggerEvent (index, DetailsGrid);
					}
				}
				else
				{
					//Now get the actual data item behind the selected row & Save it to private variable
					Console.WriteLine ($"SqlDbViewer(1682) index changed, NOT using Event handler");
					if (CurrentDb == "BANKACCOUNT")
					{
						BankAccount b = (BankAccount)BankGrid.SelectedItem;
						//Update the string in the Viewer Selection list of DbSelector
						UpdateRowDetails (b, "BankGrid");
						if (EditdbWnd != null)
						{
							EditdbWnd.DataGrid1.SelectedItem = BankGrid.SelectedItem;
							EditdbWnd.DataGrid1.SelectedIndex = BankGrid.SelectedIndex;
						}
						//we now have FULL PrettyDetails
					}
					else if (CurrentDb == "CUSTOMER")
					{
						Customer b = (Customer)CustomerGrid.SelectedItem;
						//Update the string in the Viewer Selection list of DbSelector
						UpdateRowDetails (b, "CustomerGrid");
						if (EditdbWnd != null)
						{
							EditDataGrid.SelectedItem = CustomerGrid.SelectedItem;
							EditDataGrid.SelectedIndex = CustomerGrid.SelectedIndex;
						}
					}
					else if (CurrentDb == "DETAILS")
					{
						SecAccounts b = (SecAccounts)DetailsGrid.SelectedItem;
						//Update the string in the Viewer Selection list of DbSelector
						UpdateRowDetails (b, "DetailsGrid");
						if (EditdbWnd != null)
						{
							EditdbWnd.DataGrid1.SelectedItem = DetailsGrid.SelectedItem;
							EditdbWnd.DataGrid1.SelectedIndex = DetailsGrid.SelectedIndex;
						}
					}
				}
			}

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
							BankAccount record = (BankAccount)datarecord;
							MainWindow.gv.CurrentDb[x] =
								$"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, " +
								$"Balance £ {record?.Balance}, Interest {record?.IntRate}%";
							PrettyDetails = MainWindow.gv.CurrentDb[x];
							MainWindow.gv.PrettyDetails = PrettyDetails;
							UpdateDbSelector (this);
							break;
						}
						else if (caller == "CustomerGrid")

						{
							Customer record = (Customer)datarecord;
							MainWindow.gv.CurrentDb[x] =
								$"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, " +
								$"{record?.LName} {record?.Town}, {record?.County}";
							PrettyDetails = MainWindow.gv.CurrentDb[x];
							MainWindow.gv.PrettyDetails = PrettyDetails;
							UpdateDbSelector (this);
							break;
						}
						else if (caller == "DetailsGrid")

						{

							SecAccounts record = (SecAccounts)datarecord;
							MainWindow.gv.CurrentDb[x] =
								$"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, " +
								$"Balance {record?.Balance}, Interest % {record?.IntRate}";
							PrettyDetails = MainWindow.gv.CurrentDb[x];
							MainWindow.gv.PrettyDetails = PrettyDetails;
							UpdateDbSelector (this);
							break;
						}
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
					Customer c = CustomerGrid.SelectedItem as Customer;

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
				Window_GotFocus (sender, null);
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
					Customer c = CustomerGrid.SelectedItem as Customer;
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
			#region grid row selection code

			private void BankGrid_SelectedCellsChanged (object sender, SelectedCellsChangedEventArgs e)
			{
				//This fires when we click inside the grid !!!
				//This is THE ONE to use to update our DbSleector ViewersList text
				if (BankGrid.SelectedItem != null)
				{
					if (BankGrid.SelectedItem == null)
						return;
					TextBlock tb = new TextBlock ();

					//This gives me an entrie Db Record in "c"
					BankAccount c = BankGrid.SelectedItem as BankAccount;
					//				Console.WriteLine ($"BankGrid_SelectedCellsChanged - Identified row data ");
					string date = Convert.ToDateTime (c.ODate).ToShortDateString ();
					string s = $"Bank - # {c.CustNo}, Bank #{c.BankNo}, Customer # {c.CustNo}, £{c.Balance}, {c.IntRate}%,  {date}";
					UpdateDbSelector (s);
					//				if(EditdbWnd != null)
					//					EditdbWnd.DataGrid1.SelectedItem = BankGrid.SelectedItem;

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
					TextBlock tb = new TextBlock ();

					//This gives me an entrie Db Record in "c"
					Customer c = CustomerGrid.SelectedItem as Customer;
					//				Console.WriteLine ($"CustomerGrid_SelectedCellsChanged - Identified row data ");
					string s = $"Customer - # {c.CustNo}, Bank #@{c.BankNo}, {c.LName}, {c.Town}, {c.County} {c.PCode}";
					UpdateDbSelector (s);

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
					TextBlock tb = new TextBlock ();

					//This gives me an entrie Db Record in "c"

					SecAccounts c = DetailsGrid.SelectedItem as SecAccounts;
					Console.WriteLine ($"DetailsGrid_SelectedCellsChanged - Identified row data");
					string date = Convert.ToDateTime (c.ODate).ToShortDateString ();
					string s = $"Details - # {c.CustNo}, Bank #@{c.BankNo}, Customer # {c.CustNo}, £{c.Balance}, {c.IntRate}%,  {date}";
					UpdateDbSelector (s);

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
					EditdbWnd = edb;
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
					EditdbWnd = edb;
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
					EditdbWnd = edb;
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
				EditdbWnd = null;
			}
		}
	}


