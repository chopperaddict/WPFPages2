﻿//if NOT set, dtDetails is persistent
//#define PERSISTENTDATA
#define TASK1
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;

using DocumentFormat . OpenXml . Office2010 . Excel;

using WPFPages . Views;

namespace WPFPages . ViewModels
{

	public class DetailsViewModel : INotifyPropertyChanged
	{
		public static BankAccountViewModel bvm = MainWindow . bvm;
		public static CustomerViewModel cvm = MainWindow . cvm;
		public static DetailsViewModel dvm = MainWindow . dvm;

		public event PropertyChangedEventHandler PropertyChanged;

		// Setup our data structures here for SecAccounts Database
		//		public static List<DetailsViewModel> DetailsList = new List<DetailsViewModel> ();
		//		public ObservableCollection<DetailsViewModel> _DetailsObs = new ObservableCollection<DetailsViewModel> (DetailsList);
		public ObservableCollection<DetailsViewModel> _DetailsObs = new ObservableCollection<DetailsViewModel> ( );

		public ObservableCollection<DetailsViewModel> DetailsObs
		{
			get { return _DetailsObs; }
			set
			{
				_DetailsObs = value; OnPropertyChanged ( DetailsObs . ToString ( ) );
				OnPropertyChanged ( cvm . CustomersObs . ToString ( ) );
				OnPropertyChanged ( bvm . BankAccountObs . ToString ( ) );
			}
		}

		private void DetailsObs_CollectionChanged ( object sender, System . Collections . Specialized . NotifyCollectionChangedEventArgs e )
		{
			//Something has changed my ItemsSource.  No need to do anything really ?
			Type t = sender . GetType ( );
			if ( !t . FullName . Contains ( "ViewModels.DetailsViewModel" ) )
				Console . WriteLine ( $"DetailsViewModel has received a notification that  the Customer Obs collection has changed..... YEAH" );
			//			else
			//				Console . WriteLine ( $"DetailsViewModel has received a notification that  another collection has been changed..... WOW" );
		}
		/// <summary>
		/// Callback for db change notifications
		/// </summary>
		/// <param name="sender"></param>
		public async void DbHasChangedHandler ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args )
		{
			if ( Grid. Name == "DetailsGrid" )
				return;         // Nothing to do, it was us that sent the broadcast

			if ( Flags . SqlDetViewer != null )
				Flags . SqlDetViewer . RefreshDetailsOnUpdateNotification ( sender, Grid, args);
			return;
		}


		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;
		public static DataTable dtDetails = null;

		//==================================
		//Delegate & Event handler for Db Updates
		//==================================
//		public delegate void DbUpdated ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args );
		public DbUpdated NotifyOfDataChange;

		/// <summary>
		///  A Delegate declared in SqlDbViewer to notify all ViewModels when a data change occurs
		/// </summary>

		private static void RefreshObs ( object sender )
		{
			Console . WriteLine ( $"\n()()()()()()()()()()()()()()()()()()()()   Message received in CustomerView Model due to a Db Update" );
		}

		// CONSTRUCTOR
		public DetailsViewModel ( )
		{
			DetailsObs . CollectionChanged += DetailsObs_CollectionChanged;

		}
		#region properties

		private int id;
		private string bankno;
		private string custno;
		private int actype;
		private decimal balance;
		private decimal intrate;
		private DateTime odate;
		private DateTime cdate;
		private int selectedItem;
		private int selectedRow;

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
		public decimal Balance
		{
			get { return balance; }
			set { balance = value; OnPropertyChanged ( Balance . ToString ( ) ); }
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
			set
			{
				selectedItem = value;
				OnPropertyChanged ( SelectedItem . ToString ( ) );
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

		#endregion properties

		#region INotifyProp		
		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}
		#endregion INotifyProp		


		public static void UpdateBankDb ( string CallerDb )
		{
			if ( CallerDb == "DETAILS" )
			{
				//				string Cmd = "Update Bankaccount, "
			}
		}


		public async Task<bool> LoadDetailsTaskInSortOrder ( bool IsOriginator, int mode = -1 )
		{
			//THIS Fn HANDLES SPAWNING THE TASK/AWAIT
			//and handles the Broadcast Notification
			List<Task<bool>> tasks = new List<Task<bool>> ( );
			tasks . Add ( LoadDetailsTask ( true, 0 ) );
			var Results = await Task . WhenAll ( tasks );

			DataLoadedArgs args = new DataLoadedArgs ( );
			args . DbName = "DETAILS";
			args . CurrentIndex = 0;
			// Notify all interested parties that the full Details data is loaded and available for them in -  dtDetails & DetailsObs at least
			if ( !IsOriginator )
				SqlDbViewer . SendDBLoadedMsg ( null, args );
			return true;
		}

		//**************************************************************************************************************************************************************//
		/// <summary>
		/// // Only called by LoadDetailsTaskInSortOrder()
		/// </summary>
		/// <param> </param>
		public async Task<bool> LoadDetailsTask ( bool isOriginator, int mode = -1 )
		{
			Mouse . OverrideCursor = Cursors . Wait;
			// load SQL data in DataTable
			if ( dtDetails == null ) DetailsViewModel . dtDetails = new DataTable ( );
			else dtDetails . Clear ( );
			try
			{ if ( DetailsObs != null && DetailsObs . Count > 0 ) DetailsObs . Clear ( ); }
			catch ( Exception ex )
			{ Console . WriteLine ( $"DetailsObs Exception [{ex . Data}\r\n" ); }
			DateTime start = DateTime . Now;
			Console . WriteLine ( $"Starting AWAITED task to load Details  Data via Sql" );

			try
			{ 
				await LoadSqlData ( 0, Flags . IsMultiMode ); 
				await LoadDetailsObsCollection ( ); }
			catch ( Exception ex )
			{ Console . WriteLine ( $"Task error {ex . Data},\n{ex . Message}" ); }

			Mouse . OverrideCursor = Cursors . Arrow;
			// WE NOW HAVE OUR DATA HERE - fully loaded into O
			return true;
		}


		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public async static Task<bool> LoadSqlData ( int mode = -1, bool isMultiMode = false )
		{
			try
			{
				if ( dtDetails . Rows . Count > 0 )
					dtDetails . Clear ( );
				SqlConnection con;
				string ConString = "";
				string commandline = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );
				using ( con )
				{
					if ( Flags.IsMultiMode)
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from SecAccounts  order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dtDetails );
					Console . WriteLine ( $"Sql data loaded into Details DataTable [{dtDetails . Rows . Count}] ...." );
					return true;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Details Details - {ex . Message}, {ex . Data}" );
				return false;
			}
			return true;
		}
		//**************************************************************************************************************************************************************//
		// Loads data from DataTable into Observable Collection
		public async Task<bool> LoadDetailsObsCollection ( )
		{
			try
			{
				//Load the data into our ObservableCollection BankAccounts
				if ( DetailsObs . Count > 0 )
				{ DetailsObs . Clear ( ); }

				for ( int i = 0 ; i < DetailsViewModel . dtDetails . Rows . Count ; ++i )
					DetailsObs . Add ( new DetailsViewModel
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
				// WE NOW HAVE OUR DATA HERE - fully loaded into Obs 
				Console . WriteLine ( $"Sql data loaded into Details Collection [{DetailsObs . Count}] ...." );
				return true;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Error loading Details Data {ex . Message}" );
				return false;
			}
		}


		#region CallBacks

		#endregion CallBacks
		//**************************************************************************************************************************************************************//
	}
}
/*
 * 
#if USETASK
{
			try
			{
			// THIS ALL WORKS PERFECTLY - THANKS TO VIDEO BY JEREMY CLARKE OF JEREMYBYTES YOUTUBE CHANNEL
				int? taskid = Task.CurrentId;
				Task<DataTable> DataLoader = LoadSqlData ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadDetailsObsCollection();
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load Details Data via Sql\n" +
					$"task =Id is [{taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start).Ticks} ticks]\n");
			}
			catch (Exception ex)
			{ Console.WriteLine ($"Task error {ex.Data},\n{ex.Message}"); }
			Mouse.OverrideCursor = Cursors.Arrow;
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs >?
}
#else * */