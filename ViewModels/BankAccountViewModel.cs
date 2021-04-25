// if set, Datatable is cleared and reloaded, otherwise it is not reloaded
//#define PERSISTENTDATA
#define USETASK
#undef USETASK
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Data . SqlClient;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Input;
using System . Threading . Tasks;
using System . Timers;
using System . Windows . Data;
using System . Collections;
using WPFPages . Views;
using System . Windows;
using System . Collections . Specialized;

/// <summary>
///  this is a mirror image of the original BankAccount.cs file
/// </summary>
namespace WPFPages . ViewModels
{
	public partial class BankAccountViewModel : INotifyPropertyChanged
	{
		//==================================
		//Delegate & Event handler for Db Updates
		//==================================
		/// </summary>
		/// Also declared in SqlDbViewer
		// CONSTRUCTOR
		public BankAccountViewModel ( )
		{
			BankAccountObs . CollectionChanged += BankAccountObs_CollectionChanged1;
//			EventHandlers . ShowSubscribersCount ( );

		}

		private void BankAccountObs_CollectionChanged1 ( object sender, NotifyCollectionChangedEventArgs e )
		{
			//Something has changed my ItemsSource.  No need to do anything really ?
			//This gets triggered for each record reloaded into our Obs collection !!!!
			Type t = sender . GetType ( );
			if ( !t . FullName . Contains ( "ViewModels.BankAccountViewModel" ) )
				Console . WriteLine ( $"BankAccountViewModel has received a notofication that collection \"{t . FullName}\"has changed..... YEAH" );
			//			else
			//				Console . WriteLine ( $"BankAccountViewModel has received a notification that  another collection has been changed..... WOW" );
		}

		public void BankAccountObsChanged ( object o, NotifyCollectionChangedEventArgs e )
		{
			int y = 0;
		}
		public void SubscribeToChangeEvents ( )
		{
			// subscribe to Data chngned event fired by SqlDbviewer
			SqlDbViewer sqlv = new SqlDbViewer ( 'A' );
			// assign event handler function
			NotifyOfDataChange += DbHasChangedHandler;
			//			CollectionChanged += test;
			BankAccountObs . CollectionChanged += BankAccountObsChanged;
			EventHandlers ev = new EventHandlers ( );
			ev . ShowSubscribersCount ( );
		}
		public void test ( object o, EventArgs e )
		{
			int y = 0;
		}

		/// <summary>
		/// Callback handler for db change notifications sent by another SqlDbViewer
		/// We have to try to work out whether we have one or more other viewers open
		/// and update their datagris as relevant
		/// </summary>
		/// <param name="sender"></param>
		public  void DbHasChangedHandler ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args )
		{
			if ( Grid . Name == "BankGrid" )
				return;         // Nothing to do, it was us that sent the broadcast

			// Send it to the correct open viewer window
			if ( Flags . SqlBankViewer != null )
				Flags . SqlBankViewer . RefreshBankOnUpdateNotification ( sender, Grid, args );
			return;
		}


		// MVVM TO DO STUFF/INFO		
		#region MVVMstuff

		// How to configure a RelayCommand with lambda expressions:

		RelayCommand _saveCommand; public ICommand SaveCommand
		{
			get
			{
				if ( _saveCommand == null )
				{
#pragma MVVM TODO
					//_saveCommand = new RelayCommand (param => this.Save (),
					//    param => this.CanSave);
				}
				return _saveCommand;
			}
		}

		//RelayCommand RequestClose
		//{

		//}
		#endregion MVVMstuff

		#region setup
		public event PropertyChangedEventHandler PropertyChanged;

		//public flags
		public static bool EditDbEventInProcess = false;
		//if true - shows subsribers to events in Output
		public static bool ShowSubscribeData = true;

		public static EditDb EditdbWndBank = null;
		public static EditDb EditdbWndBankCust = null;
		public static EditDb EditdbWndBankDet = null;

		//**********************
		// dbEdit db viewer GLOBALS
		//**********************
		public static List<DataGrid> CurrentEditDbViewerBankGridList = new List<DataGrid> ( );
		public static List<DataGrid> CurrentEditDbViewerCustomerGridList = new List<DataGrid> ( );
		public static List<DataGrid> CurrentEditDbViewerDetailsGridList = new List<DataGrid> ( );
		public static DataGrid ActiveEditDbViewer = null;

		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;
		#endregion setup


		//		public IList SeletedItems { get; set; }

		#region INotifyPropertyChanged Members

		#region Data update/Events/Delegates Support  functions
		public static void ClearFromEditDbList ( DataGrid grid, string caller )
		{
			//if (caller == "BANKACCOUNT")
			//{
			//	for (var item = 0; item < CurrentEditDbViewerBankGridList.Count; item++)
			//	{ if (CurrentEditDbViewerBankGridList[item] == grid) { CurrentEditDbViewerBankGridList.RemoveAt (item); Flags.CurrentEditDbViewerBankGrid = null; break; } }
			//}
			//else if (caller == "CUSTOMER")
			//{
			//	for (var item = 0; item < CurrentEditDbViewerCustomerGridList.Count; item++)
			//	{ if (CurrentEditDbViewerCustomerGridList[item] == grid) { CurrentEditDbViewerCustomerGridList.RemoveAt (item); Flags.CurrentEditDbViewerCustomerGrid = null; break; } }
			//}
			//else if (caller == "DETAILS")
			//{
			//	for (var item = 0; item < CurrentEditDbViewerDetailsGridList.Count; item++)
			//	{ if (CurrentEditDbViewerDetailsGridList[item] == grid) { CurrentEditDbViewerDetailsGridList.RemoveAt (item); Flags.CurrentEditDbViewerDetailsGrid = null; break; } }
			//}
		}
		public static void ClearFromSqlList ( DataGrid grid, string caller )
		//Remove the datagrid from our List<Datagrid>
		{
			//if (caller == "BANKACCOUNT")
			//{
			//	for (var item = 0; item < Flags.CurrentEditDbViewerBankGridList.Count; item++)
			//	{ if (Flags.CurrentEditDbViewerBankGridList[item] == grid) { Flags.CurrentEditDbViewerBankGridList.RemoveAt (item); break; } }
			//}
			//else if (caller == "CUSTOMER")
			//{
			//	for (var item = 0; item < Flags.CurrentEditDbViewerCustomerGridList.Count; item++)
			//	{ if (Flags.CurrentEditDbViewerCustomerGridList[item] == grid) { Flags.CurrentEditDbViewerCustomerGridList.RemoveAt (item); break; } }
			//}
			//else if (caller == "DETAILS")
			//{
			//	for (var item = 0; item < Flags.CurrentEditDbViewerDetailsGridList.Count; item++)
			//	{ if (Flags.CurrentEditDbViewerCustomerGridList[item] == grid) { Flags.CurrentEditDbViewerCustomerGridList.RemoveAt (item); break; } }
			//}
		}
		#endregion Data update Support  functions

		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}
		#endregion

		#region data loading stuff

		//**************************************************************************************************************************************************************//
		public async Task<bool> LoadBankTaskInSortOrder ( bool IsOriginator, int mode = -1, bool isMultiMode = false )
		{
			//THIS Fn HANDLES SPAWNING THE TASK/AWAIT
			//and handles the Broadcast Notification
			List<Task<bool>> tasks = new List<Task<bool>> ( );
			tasks . Add ( LoadBankTask ( 0 ) );
			var Results = await Task . WhenAll ( tasks );

			DataLoadedArgs args = new DataLoadedArgs ( );
			args . DbName = "BANKACCOUNT";
			args . CurrentIndex = 0;
			// Notify all interested parties that the full BankAccount data is loaded and available for them in -  dtBank & BankAccountObs at least
			if ( !IsOriginator )
				SqlDbViewer . SendDBLoadedMsg ( null, args );
			return true;
		}

		//**************************************************************************************************************************************************************//
		public async Task<bool> LoadBankTask ( int mode = -1, bool isMultiMode = false )
		{
			//Create the one and only dtBank instance if not already there
			if ( dtBank == null )
				dtBank = new DataTable ( );
			else
				dtBank . Clear ( );
			try
			{
				if ( BankAccountObs != null && BankAccountObs . Count > 0 )
					BankAccountObs . Clear ( );
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"BankAccountObs Exception [{ex . Data}\r\n" );
				return false;
			}
			Console . WriteLine ( $"Starting AWAITED task to load Bank Data via Sql" );
			DateTime start = DateTime . Now;
			// THIS ALL WORKS PERFECTLY - THANKS TO VIDEO BY JEREMY CLARKE OF JEREMYBYTES YOUTUBE CHANNEL
			await FillBankAccountDataGrid ( isMultiMode );
			await LoadBankAccountObsCollection ( );
			Task . WaitAll ( );
			//				await FillBankAccountDataGrid ( isMultiMode );
			//				await LoadBankAccountObsCollection ( );
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs, So Notify any interested Party that the data is here?		
			Console . WriteLine ( $"BankAccount fully loaded in {( DateTime . Now - start ) . Milliseconds} milli seconds" );
			Mouse . OverrideCursor = Cursors . Arrow;
			return true;
		}

		//**************************************************************************************************************************************************************//
		///<summary>
		/// fill DataTable with data from SQL BankAccount database
		/// </summary>
		public async Task<bool> FillBankAccountDataGrid ( bool isMultiMode = false )
		{
			//clear the datatable first as we are only showing a subset
			if ( dtBank . Rows . Count > 0 )
				return false;
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );
			dtBank = LoadSqlData ( dtBank, 0, isMultiMode );
			return true;
			// dtBank should be fully loaded here
		}
		//**************************************************************************************************************************************************************//
		public async Task<bool> LoadBankAccountObsCollection ( )
		{
			if ( this . BankAccountObs != null && this . BankAccountObs . Count > 0 )
			{
				try // Clear the collection out)
				{
					this . BankAccountObs . Clear ( );
					BankAccountViewModel . BankList . Clear ( );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"Failed to load clear Bank Details Obslist - {ex . Message}" );
				}
			}
			//This DOES access the Bank/Account Class properties !!!!!
			for ( int i = 0 ; i < dtBank . Rows . Count ; ++i )
			{
				if ( this . BankAccountObs != null )
				{
					try
					{
						this . BankAccountObs . Add ( new BankAccountViewModel
						{
							Id = Convert . ToInt32 ( BankAccountViewModel . dtBank . Rows [ i ] [ 0 ] ),
							BankNo = BankAccountViewModel . dtBank . Rows [ i ] [ 1 ] . ToString ( ),
							CustNo = BankAccountViewModel . dtBank . Rows [ i ] [ 2 ] . ToString ( ),
							AcType = Convert . ToInt32 ( BankAccountViewModel . dtBank . Rows [ i ] [ 3 ] ),
							Balance = Convert . ToDecimal ( BankAccountViewModel . dtBank . Rows [ i ] [ 4 ] ),
							IntRate = Convert . ToDecimal ( BankAccountViewModel . dtBank . Rows [ i ] [ 5 ] ),
							ODate = Convert . ToDateTime ( BankAccountViewModel . dtBank . Rows [ i ] [ 6 ] ),
							CDate = Convert . ToDateTime ( BankAccountViewModel . dtBank . Rows [ i ] [ 7 ] ),
						} );
					}
					catch ( Exception e )
					{
						Console . WriteLine ( $"Bank Obs load failed - {e . Data}, - {e . Message}" );
					}
					//					return true;
				}
				//				else return false;
			}
			Console . WriteLine ( $"Sql data LOADED into Bank Collection...." );
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs 

			//			if (Flags.ActiveSqlGrid != null)
			//				Flags.ActiveSqlGrid.ItemsSource = CollectionViewSource.GetDefaultView (bvm.BankAccountObs );
			return true;
		}

		#endregion data loading stuff
		#region CallBacks

		#endregion CallBacks
	}
	/// <summary>
	///  This partial BankAccountViewModel class handles all BANKACCOUNT related items
	/// </summary>
	public partial class BankAccountViewModel : INotifyPropertyChanged
	{
		public static BankAccountViewModel bvm = MainWindow . bvm;
		public static CustomerViewModel cvm = MainWindow . cvm;
		public static DetailsViewModel dvm = MainWindow . dvm;

		public static List<BankAccountViewModel> BankList = new List<BankAccountViewModel> ( );
		public DbUpdated NotifyOfDataChange;

		// Set the Observable collection up so it is Notifiable (	INotifyPropertyChanged())
		//		public ObservableCollection<BankAccountViewModel> BankAccountObs { get; set; }
		public ObservableCollection<BankAccountViewModel> _bankAccountObs = new ObservableCollection<BankAccountViewModel> ( BankList );
		public ObservableCollection<BankAccountViewModel> BankAccountObs
		{
			get { return _bankAccountObs; }
			set
			{
				_bankAccountObs = value;
				SqlDbViewer sqlv = new SqlDbViewer ( 'B' );
				OnPropertyChanged ( BankAccountObs . ToString ( ) );
				OnPropertyChanged ( cvm . CustomersObs . ToString ( ) );
				OnPropertyChanged ( dvm . DetailsObs . ToString ( ) );

				//				OnPropertyChanged (BankAccountObs.ToString (); if (NotifyOfDataChange != null) NotifyOfDataChange (this);
			}
		}





		#region SQL data loading - including special Task handler code
		//static async Task HandleTask (Task task)
		//{
		//	Task.Await (OnCompleted);
		//	int x = 0;
		//	if (x != 0)
		//	{
		//		//These two work
		//		//			var t =  Task.Run (() => FillBankAccountDataGrid (dtBank));
		//		//			t.Wait ();

		//		//So do these two, (or first one alone id load is done externally)
		//		// This uses a special Class Taskextensions.cs that handles the AWAIT of whatever task is passed  into it
		//		//using the syntax 
		//		//			FillBankAccountDataGrid (dtBank).Await ();
		//		//			LoadBankAccountIntoList (BankList, dtBank);
		//	}
		//}



		/// <summary>
		/// Load SQL Db from SQL Server
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DataTable LoadSqlData ( DataTable dt, int mode = -1, bool isMultiMode = false )
		{
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection ( ConString );
				using ( con )
				{
					string commandline = "";

					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM BANKACCOUNT "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";

						commandline = Utils . GetDataSortOrder ( commandline );

					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from BankAccount order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}

					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
					return dt;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return ( DataTable ) null;
			}
			return ( DataTable ) null;
		}
		#endregion SQL data loading

		#region General Functionality
		public void BankAccountObs_CollectionChanged ( object sender, System . Collections . Specialized . NotifyCollectionChangedEventArgs e )
		{
			OnPropertyChanged ( "BankAccountObs" );

		}

		private static void OnCompleted ( )
		{
			Console . WriteLine ( "SQL data loading completed...." );
		}


		public static void UpdateBankDb ( DataTable dtBank, string CallerDb )
		{
			if ( CallerDb == "BANKACCOUNT" )
			{
				//				string Cmd = "Update Bankaccount, "
			}
		}
		#endregion General Functionality


		//public int Id { get; set; }		
		//public string BankNo { get; set; }
		//public string CustNo { get; set; }
		//public int AcType { get; set; }
		//public decimal Balance { get; set; }

		//public decimal IntRate { get; set; }
		//public DateTime ODate { get; set; }
		//public DateTime CDate { get; set; }

		#region Standard Class Properties setup
		private int id;
		private string bankno;
		private string custno;
		private int actype;
		private decimal balance;
		private decimal intrate;
		private DateTime odate;
		private DateTime cdate;
		private int selectedItem;
		private int selectedIndex;
		//		private Timer timer = new Timer ();
		public static DataTable dtBank = null;


		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged ( Id . ToString ( ) ); }
		}
		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged ( BankNo . ToString ( ) ); }
		}
		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged ( CustNo . ToString ( ) ); }
		}
		public int AcType
		{
			get { return actype; }
			set
			{ actype = value; OnPropertyChanged ( AcType . ToString ( ) ); }
		}
		public decimal Balance
		{
			get { return balance; }
			set
			{ balance = value; OnPropertyChanged ( Balance . ToString ( ) ); }
		}
		public decimal IntRate
		{
			get { return intrate; }
			set { intrate = value; OnPropertyChanged ( IntRate . ToString ( ) ); }
		}
		public DateTime ODate
		{
			get { return odate; }
			set { odate = value; OnPropertyChanged ( ODate . ToString ( ) ); }
		}
		public DateTime CDate
		{
			get { return cdate; }
			set { cdate = value; OnPropertyChanged ( CDate . ToString ( ) ); }
		}
		public int SelectedItem
		{
			get { return selectedItem; }
			set { selectedItem = value; OnPropertyChanged ( SelectedItem . ToString ( ) ); }
		}
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set { selectedIndex = value; OnPropertyChanged ( SelectedIndex . ToString ( ) ); }
		}
		#endregion Class properties

	}

}

/*
 * 
 #if USETASK
			{
				int? taskid = Task.CurrentId;
				DateTime start = DateTime.Now;
				Task<bool> DataLoader = FillBankAccountDataGrid ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadBankAccountIntoList (dtBank);
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load BankAccount  Data via Sql\n" +
					$"task =Id is [ {taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start)} Ticks\n");
			}
#else
			{
* */