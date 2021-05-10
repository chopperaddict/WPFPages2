using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Runtime . CompilerServices;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Threading;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class DetCollection : ObservableCollection<DetailsViewModel>//, INotifyCompletion
	{
		//Declare a global pointer to Observable Details Collection
		//		public  DetCollection DetcollectionStatic;
		public static DetCollection Detcollection = new DetCollection();

		public  static DataTable dtDetails = new DataTable();
		public static Stopwatch  st;


		#region DATA LOADED EVENT trigger method

		// THIS IS  HOW  TO HANDLE EVENTS RIGHT NOW  - WORKING WELL 4/5/21//
		// We no longer need ot delcare a Delegate  to do this
		//Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes
		public   static event EventHandler<LoadedEventArgs> DetDataLoaded;

		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		private static void OnDetDataLoaded ( DetCollection dtdata , int row)
		{
			if ( DetDataLoaded != null )
			{
				Console . WriteLine ( $"DETAILS : Broadcasting DATA LOADED NOTIFICATION from OnDetDataLoaded" );
				DetDataLoaded?.Invoke ( dtdata , new LoadedEventArgs ( ) { DataSource = dtdata , CallerDb = "DETAILS", CurrSelection = row } );
			}
		}

		#endregion DATA LOADED EVENT trigger method

		#region CONSTRUCTOR

		public DetCollection ( )
		{
			//set the static pointer to this class
			//			Detcollection = this;
			Flags . DetailsCollection = this;
		}

		#endregion CONSTRUCTOR


		#region startup/load data / load collection (DetCollection)

		// Entry point for all data load/Reload
		CancellationTokenSource  cts = new CancellationTokenSource();

		//**************************************************************************************************************************************************************//
		public async Task<DetCollection> LoadDetailsTaskInSortOrderAsync (  int row, bool b = false )
		{
			if ( dtDetails . Rows . Count > 0 )
				dtDetails . Clear ( );

			if ( Detcollection . Items . Count > 0 )
				Detcollection . ClearItems ( );

			// This all woks just fine, and DOES switch back to UI thread that is MANDATORY before doing the Collection load processing
			// thanks to the use of TaskScheduler.FromCurrentSynchronizationContext() that oerforms the magic switch back to the UI thread
			Console . WriteLine ( $"DETAILS : Entering Method to call Task.Run in DetCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );

			#region process code to load data

			Task t1 = Task . Run(
					async ( ) =>
						{
							Console . WriteLine ( $"Before starting initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
							await LoadDetailsDataSql();
							Console . WriteLine ( $"After initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
						}
				);
			t1 . ContinueWith
			(
				async ( Detcollection ) =>
				{
					Console . WriteLine ( $"Before starting second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
					await LoadDetCollection (row, b );
				} , TaskScheduler . FromCurrentSynchronizationContext ( )
			 );
			Console . WriteLine ( $"After second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
			
			#endregion process code to load data

			#region Success//Error reporting/handling

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1 . ContinueWith (
				( Detcollection ) =>
				{
					Console . WriteLine ( $"DetCollection : Task.Run() Completed : Status was [ {Detcollection . Status} ]." );
				} , CancellationToken . None , TaskContinuationOptions . OnlyOnRanToCompletion , TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1 . ContinueWith (
				( Detcollection ) =>
				{
					AggregateException ae =  t1 . Exception . Flatten ( );
					Console . WriteLine ( $"Exception in DetCollection data processing \n" );
					foreach ( var item in ae . InnerExceptions )
					{
						Console . WriteLine ( $"DetCollection : Exception : {item . Message}, : {item . Data}" );
					}
				} , CancellationToken . None , TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
			);

			Console . WriteLine ($"DETAILS : END OF PROCESSING & Error checking functionality\nDETAILS : *** Detcollection total = {Detcollection.Count} ***\n\n");
			#endregion Success//Error reporting/handling

			return Detcollection;
	}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		//**************************************************************************************************************************************************************//
		public async Task<bool> LoadDetailsDataSql ( bool isMultiMode = false )
		{
			Stopwatch st = new Stopwatch();
			try
			{
				st . Start ( );
				SqlConnection con;
				string ConString = "";
				string commandline = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );
				using ( con )
				{
					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					else if ( Flags . FilterCommand != "" )
					{
						commandline = Flags . FilterCommand;
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
					st . Stop ( );
					Console . WriteLine ( $"DETAILS : Sql data loaded  [{dtDetails . Rows . Count}] row(s) into Details DataTable in {( double ) st . ElapsedMilliseconds / ( double ) 1000}...." );
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
				return false;
			}
			return true;
		}

		//**************************************************************************************************************************************************************//
		public static async Task<DetCollection> LoadDetCollection (int row, bool Notify = true )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
				{
					Detcollection . Add ( new DetailsViewModel
					{
						Id = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 0 ] ) ,
						BankNo = dtDetails . Rows [ i ] [ 1 ] . ToString ( ) ,
						CustNo = dtDetails . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 3 ] ) ,
						Balance = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 4 ] ) ,
						IntRate = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 5 ] ) ,
						ODate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 6 ] ) ,
						CDate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 7 ] )
					} );
					count = i;
				}
				Console . WriteLine ( $"DETAILS : Sql data loaded into Details ObservableCollection \"DetCollection\" [{count}] ...." );
				if(Notify)
					OnDetDataLoaded ( Detcollection , row);
				return Detcollection;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
		}

		//**************************************************************************************************************************************************************//

		#endregion startup/load data / load collection (DetCollection)

		#region Event Subscribing Hsndlers

		public static void SubscribeToLoadedEvent ( object o )
		{
			if ( o == Detcollection && DetDataLoaded == null )
			{
				if( Flags . CurrentSqlViewer != null)
				DetDataLoaded += Flags . CurrentSqlViewer . SqlDbViewer_DataLoaded;
				MultiViewer mv = new MultiViewer();
				DetDataLoaded += mv. MultiViewer_DataLoaded;
			}
		}

		public static void UnSubscribeToLoadedEvent ( object o )
		{
			if ( DetDataLoaded != null ) { 
				DetDataLoaded -= Flags . CurrentSqlViewer . SqlDbViewer_DataLoaded;
				MultiViewer mv = new MultiViewer();
				DetDataLoaded -= mv . MultiViewer_DataLoaded;
			}
		}

		#endregion Event Subscribing Hsndlers

		public static Delegate [ ] GetEventCount8 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( DetDataLoaded != null )
				dglist2 = DetDataLoaded?.GetInvocationList ( );
			return dglist2;
		}

		#region INotifyCompletion Interface code
		// INotifyCompletion Interface code

		//public static TaskAwaiter GetAwaiter ( object o)
		//{
		//	return GetResult ( );
		//}
		public static bool IsCompleted ( )
		{
			return false;
		}

		public static bool GetResult ( )
		{
			return true;
		}
		#endregion INotifyCompletion Interface code
		#region TEST CODE
		//**************************************************************************************************************************************************************//
		//Test codeonly
		//**************************************************************************************************************************************************************//
		public DataTable DoTask ( )
		{
			DataTable dt = new DataTable();
			return dt;
		}
		public static async Task go ( )
		{
			DataTable dt = null ;
			Task task  =  Task . Run ( async () =>
			{
				dt = Flags.DetailsCollection.DoTask();
			} );


			Console . WriteLine ( $"New dt = {dt . Rows}, {dt . Columns}" );

		}

		public void OnCompleted ( Action continuation )
		{
			throw new NotImplementedException ( );
		}
		#endregion TEST CODE
	}
}
/*
 * 
 				Console . WriteLine ( $"\nCalling dat aload system in DetCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				Task t1 = Task . Run( 
					async ( ) =>
						{
//							Console . WriteLine ( $"Before starting initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
							await LoadDetailsDataSql();
//							Console . WriteLine ( $"After initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
						}
				) . ContinueWith (
					{
//					Console . WriteLine ( $"Before starting second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
					async ( Detcollection ) => await LoadDetCollection ( ),
						TaskScheduler . FromCurrentSynchronizationContext ( )
//					Console . WriteLine ( $"After second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				}
				);
* */