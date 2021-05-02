using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Threading . Tasks;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class BankCollection : ObservableCollection<BankAccountViewModel>
	{
		//Declare a global pointer to Observable BankAccount Collection
		public static BankCollection Bankcollection;

		public static DataTable dtBank = new DataTable();

		#region startup/load data / load collection (BankCollection)

		//**************************************************************************************************************************************************************//
		public BankCollection ( ) : base ( )
		{
			Bankcollection = this;
			LoadBankTaskInSortOrder ( );
			Console . WriteLine ( $"Sql data loaded {Bankcollection . Count} records into Bank Datatable  ...." );
		}

		// Entry point for all data load/Reload
		//**************************************************************************************************************************************************************//
		public async static Task<bool> LoadBankTaskInSortOrder ( bool b = false, int i = -1 )
		{
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );
			await LoadBankData ( );
			if ( Bankcollection . Items . Count > 0 )
				Bankcollection . ClearItems ( );
			await LoadBankCollection ( );
			return true;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		//**************************************************************************************************************************************************************//
		public async static Task<bool> LoadBankData ( int mode = -1, bool isMultiMode = false )
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
					sda . Fill ( dtBank );
					return true;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
			}
		}

		//**************************************************************************************************************************************************************//
		public async static Task<bool> LoadBankCollection ( )
		{
			int count = 0;
			for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
			{
				Bankcollection . Add ( new BankAccountViewModel
				{
					Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ),
					BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
					CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ),
					AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
					Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ),
					IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
					ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ),
					CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
				} );
				count = i;
			}
			Console . WriteLine ( $"Sql data loaded into Bank ObservableCollection \"Bankcollection\" [{count}] ...." );
			return true;
		}

		//**************************************************************************************************************************************************************//

		#endregion startup/load data / load collection (BankCollection)
	}
}