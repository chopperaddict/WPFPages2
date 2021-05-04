﻿using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Threading . Tasks;
using System . Windows;

namespace WPFPages . Views
{
	/// <summary>
	/// Class to hold the Customers data for the system as an Observabke collection
	/// </summary>
	public class CustCollection : ObservableCollection<CustomerViewModel>
	{
		//Declare a global pointer to Observable Customers Collection
		public static CustCollection Custcollection;

		public static DataTable dtCust = new DataTable();

		// THIS IS  HOW  TO HANDLE EVENTS RIGHT NOW //
		//Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes
		public  static  event EventHandler<LoadedEventArgs> CustDataLoaded;
		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		protected virtual void OnCustDataLoaded ( )
		{
			if ( CustDataLoaded != null )
			{
				Console . WriteLine ( $"Broadcasting from OnCustDataLoaded in " );
				CustDataLoaded?.Invoke ( this, new LoadedEventArgs ( ) { DataSource = Custcollection, CallerDb = "CUSTOMER" } );
			}
		}


		#region startup/load data / load collection (CustCollection)

		//**************************************************************************************************************************************************************//
		public CustCollection ( ) : base ( )
		{
			Custcollection = this;
			//System.Diagnostics . Stopwatch  st = System.Diagnostics . Stopwatch . StartNew ( );
			//Console . WriteLine ( $"Sql : loading Custcollection ...." );
			//LoadCustomerTaskInSortOrderAsync ( true );
			//st . Stop ( );
			//Console . WriteLine ( $"CustCollection has completed - {Custcollection . Count} records loaded in {( double ) st . ElapsedMilliseconds / ( double ) 1000} Seconds" );

			//if ( CustDataLoaded != null )
			//	CustDataLoaded . Invoke ( Custcollection, new LoadedEventArgs { CallerDb = "CUSTOMER", DataSource = Custcollection } );
		}

		// Entry point for all data load/Reload
		//**************************************************************************************************************************************************************//
		public static async Task<bool> LoadCustomerTaskInSortOrderAsync ( bool isOriginator, int mode = -1 )
		{
			if ( dtCust . Rows . Count > 0 )
				dtCust . Clear ( );
			if ( Custcollection == null )
				Custcollection = new CustCollection ( );

			if ( Custcollection . Items . Count > 0 )
				Custcollection . ClearItems ( );

			await Task . Run (async  ( ) =>
			{
				await LoadCustDataSql ( );

				Application . Current . Dispatcher . Invoke (
					async ( ) =>
					{
						await LoadCustomerCollection ( );
					} );
			} );
			return true;
		}

		//**************************************************************************************************************************************************************//
		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public async static Task<int> LoadCustDataSql ( DataTable dt = null, int mode = -1, bool isMultiMode = false )
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
					{ commandline = Flags . FilterCommand; }
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from Customer  order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dtCust );
					Console . WriteLine ( $"Sql data loaded into Customers DataTable [{dtCust . Rows . Count}] ...." );
					return dtCust . Rows . Count;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Customer Details - {ex . Message}" );
				return 0;
			}
		}

		//**************************************************************************************************************************************************************//
		private static async Task<bool> LoadCustomerCollection ( )
		{
			int count = 0;
			for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
			{
				Custcollection . Add ( new CustomerViewModel
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
					Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ),
					ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ),
					CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
				} );
				count = i;
			}
			Console . WriteLine ( $"Sql data loaded into Customers Observable Collection \"CustCollection\"[{count}] ...." );

			if (CustDataLoaded != null )
				CustDataLoaded . Invoke ( Custcollection, new LoadedEventArgs { CallerDb = "CUSTOMER", DataSource = Custcollection } );
			return true;
		}

		#endregion startup/load data / load collection (CustCollection)

		public static void SubscribeToLoadedEvent ( object o )
		{
			if ( o == Custcollection && CustDataLoaded == null )
				CustDataLoaded += SqlDbViewer. SqlDbViewer_DataLoaded;
		}
		public static void UnSubscribeToLoadedEvent ( object o )
		{
			if ( CustDataLoaded != null )
				CustDataLoaded -= SqlDbViewer.SqlDbViewer_DataLoaded;
		}
		public static Delegate [ ] GetEventCount7 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( CustDataLoaded != null )
				dglist2 =  CustDataLoaded?.GetInvocationList ( );
			return dglist2;
		}
		//**************************************************************************************************************************************************************//
	}
}