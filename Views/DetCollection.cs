using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Threading . Tasks;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class DetCollection : ObservableCollection<DetailsViewModel>
	{
		//Declare a global pointer to Observable Details Collection
		public static DetCollection Detcollection;

		public  static DataTable dtDetails = new DataTable();

		#region startup/load data / load collection (DetCollection)

		//**************************************************************************************************************************************************************//
		public DetCollection ( )
		{
			Detcollection = this;
			Task<bool>  result = LoadDetailsTaskInSortOrder (true  );
			Console . WriteLine ( $"Sql data loaded {Detcollection . Count} records into Details Datatable  ...." );
		}

		// Entry point for all data load/Reload
		//**************************************************************************************************************************************************************//
		public async static Task<bool> LoadDetailsTaskInSortOrder ( bool b = false )
		{
			if ( dtDetails . Rows . Count > 0 )
				dtDetails . Clear ( );
			await LoadDetailsDataSql ( );
			if ( Detcollection . Items . Count > 0 )
				Detcollection . ClearItems ( );
			await LoadDetailsDataSql ( b );
			await LoadDetCollection ( );
			return true;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		//**************************************************************************************************************************************************************//
		public static async Task<bool> LoadDetailsDataSql ( bool isMultiMode = false )
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
					Console . WriteLine ( $"Sql data loaded into Details DataTable [{dtDetails . Rows . Count}] row(s) ...." );
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
		public static async Task<bool> LoadDetCollection ( )
		{
			int count = 0;
			for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
			{
				Detcollection . Add ( new DetailsViewModel
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
				count = i;
			}
			Console . WriteLine ( $"Sql data loaded into Details ObservableCollection \"DetCollection\" [{count}] ...." );
			return true;
		}

		//**************************************************************************************************************************************************************//

		#endregion startup/load data / load collection (DetCollection)
	}
}