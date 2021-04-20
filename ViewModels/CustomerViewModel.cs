//#define PERSISTENTDATA
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;

using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;

using WPFPages.Views;

using static System.Windows.Forms.LinkLabel;

namespace WPFPages.ViewModels
{

	//==================================
	//Delegate & Event handler for Db Updates
	//==================================
	/// <summary>
	///  A(Globally visible) Delegateto hold all the global flags and other stuff that is needed to handle 
	///  Static -> non static  movements with EditDb &b SqlDbViewer in particular
	/// </summary>
	public delegate void DbUpdated (object sender);

	//===========================
	//CUSTOMER VIEW MODEL CLASS
	//===========================
	public class CustomerViewModel: INotifyPropertyChanged
	{

		#region INotifyProp
		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}
		#endregion

		#region CONSTRUCTORS
		//==================
		// BASIC CONSTRUCTOR
		//==================
		public CustomerViewModel ()
		{
			CustomersObs.CollectionChanged += CustomersObs_CollectionChanged1;
		}
		#endregion CONSTRUCTORS

		#region  Events & Delegates declarations

		public DbUpdated NotifyOfDataChange;
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion  Events & Delegates

		#region Customer Collection (CustomersObs)
		//		public static List<CustomerViewModel> CustomersList = new List<CustomerViewModel> ();
		//		public  ObservableCollection<CustomerViewModel> _customersObs = new ObservableCollection<CustomerViewModel> (CustomersList);
		// Setup our data structures here for Customers Database
		public ObservableCollection<CustomerViewModel> _customersObs = new ObservableCollection<CustomerViewModel> ();
		public ObservableCollection<CustomerViewModel> CustomersObs
		{
			get { return _customersObs; }
			set
			{
				_customersObs = value;
				OnPropertyChanged (CustomersObs.ToString ());
				OnPropertyChanged (bvm.BankAccountObs.ToString ());
				OnPropertyChanged (dvm.DetailsObs.ToString ());
				//if (NotifyOfDataChange != null) NotifyOfDataChange (this);
			}
		}
		private void CustomersObs_CollectionChanged1 (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Type t = sender.GetType ();
			if (!t.FullName.Contains ("ViewModels.CustomerViewModel"))
				Console.WriteLine ($"CustomerViewModel has received a notofication that  the Customer Obs collection has changed..... YEAH");
		}
		public void SubscribeToChangeEvents ()
		{
			if (loaded) return;
			// subscribe to Data chnaged event fired by SqlDbviewer
			//			SqlDbViewer sqlv = new SqlDbViewer ();
			// assign event handler function
			loaded = true;
		}
		/// <summary>
		/// Callback for db change notifications
		/// </summary>
		/// <param name="sender"></param>
		public void DbHasChanged (object sender, DataChangeArgs args)
		{
			DataGrid cvm = sender as DataGrid;
			if (cvm.Name != "CustomerGrid")
				Console.WriteLine ($"\nCustomerViewModel received Data Change in \"{args.DbName}\" Db");
			if (args.DbName != "CUSTOMER")
			{
				// need to update our Collection
				DataGrid d = sender as DataGrid;
				{
					d.Refresh ();
					try
					{
						if (Flags.SqlCustGrid == null) return;

						CustomerViewModel cv = CustomersObs.ElementAt (d.SelectedIndex) as CustomerViewModel;
						int hash = cv.GetHashCode ();
						if (args.DbName != "CUSTOMER")

						{
							int curr = Flags.SqlCustGrid.SelectedIndex;
							if (curr >= 0)
							{
								LoadCustomerTaskInSortOrder ();
								Flags.SqlCustGrid.SelectedIndex = curr;
								Flags.SqlCustGrid.Refresh ();
							}
							Console.WriteLine ($"\nDB REFRESH performed in CustomerViewModel");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine ($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\nDB REFRESH UPDATE ERROR in CustomerViewModel\n{ex.Message} : {ex.Data}");
					}

				}
			}
		}
		public void CustomersObs_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged ("CustomerViewModel.CustomerObs");

		}

		#endregion Customer Collection (CustomersObs)


		/// <summary>
		/// Delegate Callback that is triggered whenever  one of the Db's are updated
		/// so that we can ALL update at the same time
		/// </summary>
		/// <param name="sender"></param>
		private static void RefreshObs (object sender)
		{
			Console.WriteLine ($"\n()()()()()()()()()()()()()()()()()()()()   Message received in CustomerView Model due to a Db Update");
		}

		#region PRIVATE Variables declarations 
		private int id;
		private string custno;
		private string bankno;
		private int actype;
		private string fname;
		private string lname;
		private string addr1;
		private string addr2;
		private string town;
		private string county;
		private string pcode;
		private string phone;
		private string mobile;
		private DateTime dob;
		private DateTime odate;
		private DateTime cdate;
		private int selectedItem;
		private int selectedIndex;
		private int selectedRow;

		private static bool loaded = false;
		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";
		public bool FilterResult = false;
		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";
		public bool isMultiMode = false;

		// one and only dtCust instance
		public static DataTable dtCust = new DataTable ();

		#endregion PRIVATE Variables declarations 

		#region PROPERTY SETTERS
		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged (Id.ToString ()); }
		}
		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged (CustNo.ToString ()); }
		}
		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged (BankNo.ToString ()); }
		}
		public int AcType
		{
			get { return actype; }
			set { actype = value; OnPropertyChanged (AcType.ToString ()); }
		}
		public string FName
		{
			get { return fname; }
			set { fname = value; OnPropertyChanged (FName.ToString ()); }
		}

		public string LName
		{
			get { return lname; }
			set { lname = value; OnPropertyChanged (LName.ToString ()); }
		}
		public string Addr1
		{
			get { return addr1; }
			set { addr1 = value; OnPropertyChanged (Addr1.ToString ()); }
		}
		public string Addr2
		{
			get { return addr2; }
			set { addr2 = value; OnPropertyChanged (Addr2.ToString ()); }
		}
		public string Town
		{
			get { return town; }
			set { town = value; OnPropertyChanged (Town.ToString ()); }
		}
		public string County
		{
			get { return county; }
			set { county = value; OnPropertyChanged (County.ToString ()); }
		}
		public string PCode
		{
			get { return pcode; }
			set { pcode = value; OnPropertyChanged (PCode.ToString ()); }
		}
		public string Phone
		{
			get { return phone; }
			set { phone = value; OnPropertyChanged (Phone.ToString ()); }
		}
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; OnPropertyChanged (Mobile.ToString ()); }
		}
		public DateTime Dob
		{
			get { return dob; }
			set { dob = value; OnPropertyChanged (Dob.ToString ()); }
		}

		public DateTime ODate
		{
			get { return odate; }
			set { odate = value; OnPropertyChanged (ODate.ToString ()); }
		}
		public DateTime CDate
		{
			get { return cdate; }
			set { cdate = value; OnPropertyChanged (CDate.ToString ()); }
		}
		public int SelectedItem
		{
			get { return selectedItem; }
			set
			{
				selectedItem = value;
				OnPropertyChanged (SelectedItem.ToString ());
			}
		}
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				selectedIndex = value;
				OnPropertyChanged (SelectedIndex.ToString ());
			}
		}
		public int SelectedRow
		{
			get { return selectedRow; }
			set
			{
				selectedRow = value;
				OnPropertyChanged (selectedRow.ToString ());
			}
		}
		#endregion PROPERTY SETTERS


		#region PUBLIC & STATIC DECLARATIONS

		public static BankAccountViewModel bvm = MainWindow.bvm;
		public static CustomerViewModel cvm = MainWindow.cvm;
		public static DetailsViewModel dvm = MainWindow.dvm;

		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;


		#endregion PUBLIC & STATIC DECLARATIONS


		static async Task HandleTask (Task task)
		{
			await (task);
			int x = 0;
			if (x != 0)
			{
				//These two work
				//			var t =  Task.Run (() => FillBankAccountDataGrid (dtBank));
				//			t.Wait ();

				//So do these two, (or first one alone id load is done externally)
				// This uses a special Class Taskextensions.cs that handles the AWAIT of whatever task is passed  into it
				//using the syntax 
				//			FillBankAccountDataGrid (dtBank).Await ();
				//			LoadBankAccountIntoList (BankList, dtBank);
			}
		}
		private static void OnCompleted ()
		{
			Console.WriteLine ("SQL data loading completed....");
		}

		#region SQL data handling

		public static async Task<bool> LoadSqlData (string CmdString, DataTable dt, string CallerType)
		{
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection (ConString);
				using (con)
				{
					SqlCommand cmd = new SqlCommand (CmdString, con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					if (dt.Rows.Count > 0) return true;
					else return false;
					//}
				}
#pragma TODO MVVM
				{
					//if (isMultiMode)
					//{
					//	Multiaccounts.Content = " <<- Show All";
					//	Btn.IsEnabled = false;
					//}
					//else
					//{
					//	Multiaccounts.Content = " Multiple A/C's";
					//	Btn.IsEnabled = true;
					//}
					//				isMultiMode = false;
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load Customer Details - {ex.Message}, {ex.Data}");
#if SHOWSQLERRORMESSAGEBOX
					MessageBox.Show ("SQL error occurred - See Output for details");
#endif
				return false;
			}
			return true;
		}

		//*****************************************************************************************//

		//**************************************************************************************************************************************************************//
		/// <summary>
		///  Initialise Data for Customer Grid using Task.Factory thread
		///  and it then triggers the [TriggerCustDataLoad Event]
		///  and finally ensures the ItemsSource is set to the ObsCollection
		///  once ithe Event has triggered all theother load code ?
		///  Called from DBSELECTOR
		/// </summary>
		/// <param> </param>
		public async Task<bool> LoadCustomersTask (int mode = -1)
		{
			Mouse.OverrideCursor = Cursors.Wait;

			//Create the one and only dtCust instance if not already there

			if (CustomerViewModel.dtCust == null)
				CustomerViewModel.dtCust = new DataTable ();
			else
				dtCust.Clear ();
			try
			{
				if (this.CustomersObs != null && this.CustomersObs.Count > 0)
					this.CustomersObs.Clear ();
				//				this.CustomersObs = new ObservableCollection<CustomerViewModel> (CustomerViewModel.CustomersList);
				//				if (this.CustomersObs == null)
				//					this.CustomersObs = new ObservableCollection<CustomerViewModel> (CustomerViewModel.CustomersList);
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"CustomersObs Exception [{ex.Data}\r\n");
			}
			Console.WriteLine ($"Starting AWAITED task to load Customers Data via Sql");
			DateTime date = DateTime.Now;

#if USETASK
			{
			// THIS ALL WORKS PERFECTLY - THANKS TO VIDEO BY JEREMY CLARKE OF JEREMYBYTES YOUTUBE CHANNEL
			int? taskid = Task.CurrentId;
			DateTime start = DateTime.Now;
			Task<DataTable> DataLoader =  LoadSqlData (dtCust);
			DataLoader.ContinueWith (task =>
			{
				LoadCustomerObsCollection ();
			}, TaskScheduler.FromCurrentSynchronizationContext ());
			Console.WriteLine ($"Completed AWAITED task to load Customers Data via Sql\n" +
				$"task =Id is [ {taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start).Ticks} Ticks\n");
			
			}
#else
			{
				await LoadSqlData (dtCust, mode);
				await LoadCustomerObsCollection ();
			}
#endif
			//We now have all the data in our ObservableCollection
			//so it just needs loaded into the datagrid 

			Mouse.OverrideCursor = Cursors.Arrow;
			return true;
		}

		//private static bool FillCustomerDataTable ()
		////Not in use
		//{
		//	//if (dtCust.Rows.Count > 0)
		//	//	dtCust.Clear ();
		//	//dtCust = LoadSqlData (dtCust);
		//	//return true;
		//	// dtcust should be fully loaded here
		//	#region unwanted
		//	//string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
		//	////			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
		//	//string CmdString = string.Empty;
		//	//SqlConnection con;
		//	//if (FilterCommand != "")
		//	//{
		//	//	CmdString = FilterCommand;
		//	//	FilterCommand = "";
		//	//}
		//	//else
		//	//{
		//	//	if (isMultiMode)
		//	//	{
		//	//		//THIS IS THE SQL COMMAND TO GET FULL LINES OF DUPLICATED CUSTOMER ACCOUNT #'S DATA
		//	//		CmdString = "SELECT * FROM Customer WHERE CUSTNO IN "
		//	//			+ "(SELECT CUSTNO FROM CUSTOMER "
		//	//			+ " GROUP BY CUSTNO"
		//	//			+ " HAVING COUNT(*) > 1)";
		//	//		//clear the datatable first as we are only showing a subset
		//	//		dtCust.Clear ();
		//	//	}
		//	//	else
		//	//\			{
		//	//				//CmdString = "SELECT * FROM Customer";
		//	//				//clear the datatable first as we are only showing a subset
		//	//				dtCust.Clear ();
		//	//			}
		//	//#pragma TODO  
		//	//			dtCust = = LoadSqlData (dtCust);
		//	//			//			isMultiMode = false;
		//	//			return result;
		//	#endregion unwanted

		//}

		public async static Task<DataTable> LoadSqlData (DataTable dt, int mode = -1)
		//Load data from Sql Server
		{
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection (ConString);

				using (con)
				{
					string commandline = "";
					commandline = "Select * from Customer  order by ";
					if (mode == -1)		// default
						commandline += "CustNo";
					else if (mode == 1)
						commandline += "BankNo";
					else if (mode == 2)
						commandline += "Id";
					else if (mode == 3)
						commandline += "AcType";
					else if (mode == 4)
						commandline += "Dob";
					else if (mode == 5)
						commandline += "Odate";

					SqlCommand cmd = new SqlCommand( commandline, con);
					
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					return dt;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load Customer Details - {ex.Message}");
#if SHOWSQLERRORMESSAGEBOX
					MessageBox.Show ("SQL error occurred - See Output for details");
#endif
				return null;
			}
			return null;
		}

		//public void DispatchIfNecessary (Action action)
		//{
		//	if (!Dispatcher.CheckAccess ())
		//		Dispatcher.Invoke (action);
		//	else
		//		action.Invoke ();
		//}
		////static   void LoadDataAsync ()
		//{
		//	FillCustomersDataGrid ();

		//}
		//**************************************************************************************************************************************************************//


		private void UpdateControls ()
		{
			//			if (!Dispatcher.CheckAccess ())
			//			{
			//				// We're not in the UI thread, ask the dispatcher to call this same method in the UI thread, then exit
			//				Dispatcher.BeginInvoke (new Action (UpdateControls));
			//				return;
			//			}

			//// We're in the UI thread, update the controls
			//			TextTime.Text = DateTime.Now.ToLongTimeString ();
		}

		/// <summary>
		///  Loads the data from dtCust into CustomerObs Observable collection
		/// </summary>
		/// <returns></returns>
		public async Task<bool> LoadCustomerObsCollection ()
		{
			//			return true;
			if (CustomersObs.Count == 0)
			{
				if (CustomersObs.Count > 0)
					CustomersObs.Clear ();
				try
				{
					for (int i = 0; i < dtCust.Rows.Count; ++i)
						CustomersObs.Add (new CustomerViewModel

						{
							Id = Convert.ToInt32 (dtCust.Rows[i][0]),
							CustNo = dtCust.Rows[i][1].ToString (),
							BankNo = dtCust.Rows[i][2].ToString (),
							AcType = Convert.ToInt32 (dtCust.Rows[3][3]),
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
					Console.WriteLine ($"Sql data loaded into CustomersObs directly [{CustomersObs.Count}] ....");
					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"Error loading Customers Data {ex.Message}");
					MessageBox.Show ($"SQL error occurred \n\n{ex.Message},\n{ex.Data}");

					return false;
				}
			}
			else
				return true;
		}
		//**************************************************************************************************************************************************************//

		#endregion SQL data handling

		#region CallBacks

		public async Task<bool>  LoadCustomerTaskInSortOrder(int mode = -1)
		{
			/*
			if (mode == -1)		// default
				commandline += "CustNo";
			else if (mode == 1)
				commandline += "BankNo";
			else if (mode == 2)
				commandline += "Id";
			else if (mode == 3)
				commandline += "AcType";
			else if (mode == 4)
				commandline += "Dob";
			else if (mode == 5)
				commandline += "Odate";
			 * */
			await LoadCustomersTask(mode);
			return true;
		}
		#endregion CallBacks
	}
}