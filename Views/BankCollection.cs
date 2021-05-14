using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Threading;

using WPFPages . ViewModels;

using static WPFPages . SqlDbViewer;

namespace WPFPages . Views
{
	// declre  delegate to be used to notify SqlDbViewers when data is FULLY loaded
	//	delegate void DataLoaded ( object o );

	public class BankCollection : ObservableCollection<BankAccountViewModel>
	{

		//		//Declare a global pointer to Observable BankAccount Collection
		public  static BankCollection Bankcollection=new BankCollection();
										  //public  static BankCollection Bankcollection ;

		public static DataTable dtBank = new DataTable("BankDataTable");
		//public static DataTable dtBank;
		// THIS IS  HOW  TO HANDLE EVENTS RIGHT NOW //
		////Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes
		//public static  event EventHandler<LoadedEventArgs> BankDataLoaded;

		#region CONSTRUCTOR

		public BankCollection ( )
		{
		}


		public static BankCollection LoadBank ( BankCollection bc )
		{
			// Called to Load/reload the One & Only Bankcollection data source
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );

			if ( bc != null )
				Bankcollection = bc;
			else
				Bankcollection = new BankCollection ( );

			LoadBankData ( );
			if ( Bankcollection == null )
				Bankcollection = new BankCollection ( );
			if ( Bankcollection . Count > 0 )
				Bankcollection . ClearItems ( );
			if ( dtBank.Rows.Count > 0 )
			{
				BankCollection b = new BankCollection();
				Bankcollection = b. LoadBankTest ( );
			}
			// We now have the ONE AND ONLY pointer the the Bank data in variable Bankcollection
			return Bankcollection;
		}
		#endregion CONSTRUCTOR

		//		private Timer timer = new Timer ();
		//		public static DataTable dtBank = null;


		#region DATA LOADED EVENT


		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		//private static void OnBankDataLoaded ( BankCollection bnkdata )
		//{
		//	if ( BankDataLoaded != null )
		//	{
		//		Console . WriteLine ( $"BANKACCOUNT : Broadcasting DATA LOADED NOTIFICATION from OnBankDataLoaded" );
		//		BankDataLoaded?.Invoke ( bnkdata , new LoadedEventArgs ( ) { DataSource = bnkdata , CallerDb = "BANKACCOUNT" } );
		//	}
		//}

		#endregion DATA LOADED EVENT

		#region LOAD THE DATA

		public async Task<BankCollection> LoadBankTaskInSortOrderasync ( bool Notify = false , int i = -1 )
		{
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );

			if ( Bankcollection . Items . Count > 0 )
				Bankcollection . ClearItems ( );

			// This all woks just fine, and DOES switch back to UI thread that is MANDATORY before doing the Collection load processing
			// thanks to the use of TaskScheduler.FromCurrentSynchronizationContext() that oerforms the magic switch back to the UI thread
			//			Console . WriteLine ( $"BANK : Entering Method to call Task.Run in BankCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );

			#region process code to load data

			Task t1 = Task . Run(
					async ( ) =>
					{
						await LoadBankData ( );
//						Console . WriteLine ( $"After initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
						//Test Exception handler
						// throw new AccessViolationException();
					}
				);
			#region Continuations
			t1 . ContinueWith
			(
				async ( Bankcollection ) =>
				{
					//					Console . WriteLine ( $"Before starting second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
					await LoadBankCollection ( Notify );
				} , TaskScheduler . FromCurrentSynchronizationContext ( )
			 );
			#endregion process code to load data

			#region Success//Error reporting/handling

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1 . ContinueWith (
			( Bankcollection ) =>
				{
					Console . WriteLine ( $"BANKACCOUNT : Task.Run() Completed : Status was [ {Bankcollection . Status}" );
				} , CancellationToken . None , TaskContinuationOptions . OnlyOnRanToCompletion , TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1 . ContinueWith (
				( Bankcollection ) =>
				{
					AggregateException ae =  t1 . Exception . Flatten ( );
					Console . WriteLine ( $"Exception in BankCollection data processing \n" );
					MessageBox . Show ( $"Exception in BankCollection data processing \n" );
					foreach ( var item in ae . InnerExceptions )
					{
						Console . WriteLine ( $"BankCollection : Exception : {item . Message}, : {item . Data}" );
					}
				} , CancellationToken . None , TaskContinuationOptions . OnlyOnFaulted , TaskScheduler . FromCurrentSynchronizationContext ( )
			);

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1 . ContinueWith (
				( Bankcollection ) =>
				{
					Console . WriteLine ( $"BankCollection : Task.Run() processes all succeeded. \nBankcollection Status was [ {Bankcollection . Status} ]." );
				} , CancellationToken . None , TaskContinuationOptions . OnlyOnRanToCompletion , TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1 . ContinueWith (
				( Bankcollection ) =>
				{
					AggregateException ae =  t1 . Exception . Flatten ( );
					Console . WriteLine ( $"Exception in BankCollection data processing \n" );
					MessageBox . Show ( $"Exception in BankCollection data processing \n" );
					foreach ( var item in ae . InnerExceptions )
					{
						Console . WriteLine ( $"BankCollection : Exception : {item . Message}, : {item . Data}" );
					}
				} , CancellationToken . None , TaskContinuationOptions . OnlyOnFaulted , TaskScheduler . FromCurrentSynchronizationContext ( )
			);

			#endregion Continuations

			Console . WriteLine ( $"BANKACCOUNT : END OF PROCESSING & Error checking functionality\nBANKACCOUNT : *** Bankcollection total = {Bankcollection . Count} ***\n\n" );

			#endregion Success//Error reporting/handling

			return Bankcollection;
		}
		public async Task<BankCollection> ReLoadBankData ( bool b = false , int mode = -1 )
		{
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );

			//await LoadBankTaskInSortOrderasync ( false );
			if ( Bankcollection . Count > 0 )
				Bankcollection . ClearItems ( );
			LoadBankData ( );
			Bankcollection = LoadBankTest ( );
			return Bankcollection;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public async static Task<bool> LoadBankData ( int mode = -1 , bool isMultiMode = false )
		{
			//			Console . WriteLine ( $"BANK : Entered LoadBankData in Bankcollection ...." );

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
					if ( dtBank == null )
						dtBank = new DataTable ( );
					sda . Fill ( dtBank );
					//					Console . WriteLine ( $"Exiting LoadBankData {dtBank . Rows . Count} records in DataTable" );
					//					Console . WriteLine ( $"Exiting LoadBankData in Bankcollection ...." );
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
				MessageBox . Show ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
				return false;
			}
			return true;
		}

		public async static Task<bool> LoadBankCollection ( bool Notify = true )
		{
			int count = 0;
			//			Console . WriteLine ( $"BANK : Entered LoadBankCollection in Bankcollection ...." );
			try
			{
				BankCollection bc = new BankCollection ( );
				for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
				{
					Bankcollection . Add ( new BankAccountViewModel
					{
						Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ) ,
						BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ) ,
						CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ) ,
						Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ) ,
						IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ) ,
						ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ) ,
						CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ) ,
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				//				Console . WriteLine ( $"BANK : Completed load into Bankcollection :  {Bankcollection . Count} records in Bankcollection ...." );
				BankCollection bc = new BankCollection ( );

				if ( Notify )
					EventControl.TriggerBankDataLoaded ( null ,
						new LoadedEventArgs { 
						CallerDb = "BankAccount" , 
						DataSource = bc , 
						RowCount = Bankcollection . Count } );
//					EventControl . OnBankDataLoaded ( Bankcollection );
			}
			return true;
		}

		public BankCollection LoadBankTest ( )
		{
			int count = 0;
			//			Console . WriteLine ( $"BANK : Entered LoadBankCollection in Bankcollection ...." );
			try
			{
				for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
				{
					Bankcollection . Add ( new BankAccountViewModel
					{
						Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ) ,
						BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ) ,
						CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ) ,
						Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ) ,
						IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ) ,
						ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ) ,
						CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ) ,
					} );
					count = i;
				}
				return Bankcollection;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				Console . WriteLine ( $"BANK : Completed load into Bankcollection :  {Bankcollection . Count} records loaded successfully ...." );
			}
			return Bankcollection;
		}
		#endregion LOAD THE DATA

		#region EVENT Subscription methods

		//public static void SubscribeToLoadedEvent ( object o )
		//{
		//	if ( EventControl.BankDataLoaded == null && Flags . CurrentSqlViewer != null )
		//		BankDataLoaded += Flags . CurrentSqlViewer . SqlDbViewer_DataLoaded;
		//	if ( Flags . MultiViewer != null )
		//	{
		//		MultiViewer mv = new MultiViewer();
		//		BankDataLoaded += mv . MultiViewer_DataLoaded;
		//	}
		//}

		//public static void UnSubscribeToLoadedEvent ( object o )
		//{
		//	if ( EventControl . BankDataLoaded == null && Flags . CurrentSqlViewer != null )
		//		BankDataLoaded -= Flags . CurrentSqlViewer . SqlDbViewer_DataLoaded;
		//	if ( Flags . MultiViewer != null )
		//	{
		//		MultiViewer mv = new MultiViewer();
		//		BankDataLoaded -= mv . MultiViewer_DataLoaded;
		//	}
		//}

		#endregion EVENT Subscription methods

		//public static Delegate [ ] GetEventCount6 ( )
		//{
		//	Delegate [ ] dglist2 = null;
		//	if ( BankDataLoaded != null )
		//		dglist2 = BankDataLoaded?.GetInvocationList ( );
		//	return dglist2;
		//}
		public void ListBankInfo ( KeyboardDelegate KeyBoardDelegate )
		{
			// Run a specified delegate sent by SqlDbViewer
			KeyBoardDelegate ( 1 );
		}
	}
}
