//#define PERSISTENTDATA
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using WPFPages . ViewModels;
using System.Reflection;
using System.Windows.Threading;

namespace WPFPages . Views


{

	//==================================
	//Delegate & Event handler for Db Updates
	//==================================
	//===========================
	//CUSTOMER VIEW MODEL CLASS
	//===========================
	public class CustomerViewModel : INotifyPropertyChanged
	{

		#region INotifyProp
		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}
		#endregion

		#region CONSTRUCTORS
		//==================
		// BASIC CONSTRUCTOR
		//==================
		public CustomerViewModel ( )
		{
			if (!IsSubscribedToObsNotifications)
			{
				CustomersObs.CollectionChanged += CustomersObs_CollectionChanged1;
				IsSubscribedToObsNotifications = true;
			}
		}
		#endregion CONSTRUCTORS

		#region  Events & Delegates declarations

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion  Events & Delegates

		#region Customer Collection (CustomersObs)
		// Setup our data structures here for Customers Database
		public ObservableCollection<CustomerViewModel> _customersObs = new ObservableCollection<CustomerViewModel> ( );
		public ObservableCollection<CustomerViewModel> CustomersObs
		{
			get { return _customersObs; }
			set
			{
				_customersObs = value;
				OnPropertyChanged ( CustomersObs . ToString ( ) );
				OnPropertyChanged ( bvm . BankAccountObs . ToString ( ) );
				OnPropertyChanged ( dvm . DetailsObs . ToString ( ) );
			}
		}
		private void CustomersObs_CollectionChanged1 ( object sender, System . Collections . Specialized . NotifyCollectionChangedEventArgs e )
		{
			Type t = sender . GetType ( );
			//Something has changed my ItemsSource.  No need to do anything really ?
			//if ( !t . FullName . Contains ( "ViewModels.CustomerViewModel" ) )
			//	Console . WriteLine ( $"Customer Obs Collection has changed" );
		}
		public void SubscribeToChangeEvents ( )
		{
			if ( loaded ) return;
			// subscribe to Data chnaged event fired by SqlDbviewer
			//			SqlDbViewer sqlv = new SqlDbViewer ();
			// assign event handler function
			loaded = true;		}
		/// <summary>
		/// We have received a Callback for db change notification from one or other of the GridViewers
		/// so we need to update OURSELVES
		/// </summary>
		/// <param name="sender"></param>
		public  void DbHasChangedHandler ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args )
		{
			if ( Grid . Name == "CustomerGrid" )
				return;         // Nothing to do, it was us that sent the broadcast
						// Sent by a different DataGrid - so we need to update our Collection
						// FIRST check to see what sort of Grid we are displaying so we can refresh it correctly

			if ( Flags . SqlCustViewer != null )
				Flags . SqlCustViewer . RefreshCustomerOnUpdateNotification ( sender, Grid, args );
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . DbChangedHandler ( sender, Grid, args );
			return;
		}

		public void CustomersObs_CollectionChanged ( object sender, System . Collections . Specialized . NotifyCollectionChangedEventArgs e )
		{
			OnPropertyChanged ( "CustomerViewModel.CustomerObs" );

		}

		#endregion Customer Collection (CustomersObs)


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

		private static bool IsSubscribedToObsNotifications = false;

		// one and only dtCust instance
		public static DataTable dtCust = new DataTable ( );

		#endregion PRIVATE Variables declarations 

		#region PROPERTY SETTERS
		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged ( Id . ToString ( ) ); }
		}
		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged ( CustNo . ToString ( ) ); }
		}
		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged ( BankNo . ToString ( ) ); }
		}
		public int AcType
		{
			get { return actype; }
			set { actype = value; OnPropertyChanged ( AcType . ToString ( ) ); }
		}
		public string FName
		{
			get { return fname; }
			set { fname = value; OnPropertyChanged ( FName . ToString ( ) ); }
		}

		public string LName
		{
			get { return lname; }
			set { lname = value; OnPropertyChanged ( LName . ToString ( ) ); }
		}
		public string Addr1
		{
			get { return addr1; }
			set { addr1 = value; OnPropertyChanged ( Addr1 . ToString ( ) ); }
		}
		public string Addr2
		{
			get { return addr2; }
			set { addr2 = value; OnPropertyChanged ( Addr2 . ToString ( ) ); }
		}
		public string Town
		{
			get { return town; }
			set { town = value; OnPropertyChanged ( Town . ToString ( ) ); }
		}
		public string County
		{
			get { return county; }
			set { county = value; OnPropertyChanged ( County . ToString ( ) ); }
		}
		public string PCode
		{
			get { return pcode; }
			set { pcode = value; OnPropertyChanged ( PCode . ToString ( ) ); }
		}
		public string Phone
		{
			get { return phone; }
			set { phone = value; OnPropertyChanged ( Phone . ToString ( ) ); }
		}
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; OnPropertyChanged ( Mobile . ToString ( ) ); }
		}
		public DateTime Dob
		{
			get { return dob; }
			set { dob = value; OnPropertyChanged ( Dob . ToString ( ) ); }
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
			set
			{
				selectedItem = value;
				OnPropertyChanged ( SelectedItem . ToString ( ) );
			}
		}
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				selectedIndex = value;
				OnPropertyChanged ( SelectedIndex . ToString ( ) );
			}
		}
		public int SelectedRow
		{
			get { return selectedRow; }
			set
			{
				selectedRow = value;
				OnPropertyChanged ( selectedRow . ToString ( ) );
			}
		}
		#endregion PROPERTY SETTERS


		#region PUBLIC & STATIC DECLARATIONS

		public static BankAccountViewModel bvm = MainWindow . bvm;
		public static CustomerViewModel cvm = MainWindow . cvm;
		public static DetailsViewModel dvm = MainWindow . dvm;

		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;


		#endregion PUBLIC & STATIC DECLARATIONS


		#region SQL data handling

		//**************************************************************************************************************************************************************//

		public async Task<bool> LoadCustomerTaskInSortOrder ( bool isOriginator, int mode = -1 )
		{
			//THIS Fn HANDLES SPAWNING THE TASK/AWAIT
			//and handles the Broadcast Notification
			Console . WriteLine ( $"Starting AWAITED Task LoadCustomerTaskInSortOrder using Sql" );
			Stopwatch sw = new Stopwatch ( );
			sw . Start ( );
//			List<Task<bool>> tasks = new List<Task<bool>> ( );

			Task<bool> task = Task . Run ( ( ) => LoadCustomersTask ( isOriginator, 0 ) );
			await task . ContinueWith ( ( antecedent ) => LoadSqlData ( dtCust ) );
			await LoadCustomerObsCollection ( );
			Console . WriteLine ( $"Customer fully loaded in { sw . ElapsedMilliseconds} milli seconds" );
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs, So Notify any interested Party that the data is here?		

			DataLoadedArgs args = new DataLoadedArgs ( );
			args . DbName = "CUSTOMER";
			args . CurrentIndex = 0;
			// Notify all interested parties that the full Customer data is loaded and available for them in -  dtcust & CustomersObs at least
			if ( !isOriginator )
				SqlDbViewer . SendDBLoadedMsg ( null, args );
			Mouse . OverrideCursor = Cursors . Arrow;
			return true;
		}

		//**************************************************************************************************************************************************************//
		/// <summary>
		///  Initialise Data for Customer Grid using Task.Factory thread
		///  and it then triggers the [TriggerCustDataLoad Event]
		///  and finally ensures the ItemsSource is set to the ObsCollection
		///  once ithe Event has triggered all theother load code ?
		///  Called from DBSELECTOR
		/// </summary>
		/// <param> </param>
		public async Task<bool> LoadCustomersTask ( bool isOriginator, int mode = -1 )
		{
			if ( CustomerViewModel . dtCust == null ) CustomerViewModel . dtCust = new DataTable ( );
			else dtCust . Clear ( );
			
			try
			{if ( this . CustomersObs != null && this . CustomersObs . Count > 0 )
					this . CustomersObs . Clear ( );}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"CustomersObs Exception [{ex . Data}\r\n" );
			}
			//// WE NOW HAVE OUR DATA HERE - fully loaded into Obs, So Notify any interested Party that the data is here?		
			//Console . WriteLine ( $"Customer fully loaded in {( DateTime . Now - start ) . Milliseconds} milli seconds" );
			//Mouse . OverrideCursor = Cursors . Arrow;
			return true;
		}

		//**************************************************************************************************************************************************************//

		public async static Task<bool> LoadSqlData ( DataTable dt, int mode = -1, bool isMultiMode = false )
		//Load data from Sql Server
		{
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );

				using ( con )
				{
					string commandline = "";

					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM CUSTOMER WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM CUSTOMER  "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					else if ( Flags . FilterCommand != "" )
					{commandline = Flags . FilterCommand;}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from Customer  order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
					Console . WriteLine ( $"Sql data loaded into Customers DataTable [{dtCust . Rows . Count}] ...." );
					return true;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Customer Details - {ex . Message}" );
				return false;
			}
			return true;
		}

		//**************************************************************************************************************************************************************//
		/// <summary>
		///  Loads the data from dtCust into CustomerObs Observable collection
		/// </summary>
		/// <returns></returns>
		public async Task<bool> LoadCustomerObsCollection ( )
		{
			try
			{
				if ( CustomersObs . Count > 0 )
					CustomersObs . Clear ( );
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{

					CustomersObs . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ),
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ),
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ),
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ),
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ),
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ),
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ),
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ),
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ),
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ),
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ),
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ),
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ),      // != null ? Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ) : Convert . ToDateTime ( "01/01/2020" ),
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ),
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
				}
				Console . WriteLine ( $"Sql data loaded into Customers Db [{CustomersObs . Count}] ...." );
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Error loading Customers Data {ex . Message}" );
				MessageBox . Show ( $"SQL error occurred \n\n{ex . Message},\n{ex . Data}" );
				return false;
			}
			return true;
		}
		//**************************************************************************************************************************************************************//

		#endregion SQL data handling

	}
}

