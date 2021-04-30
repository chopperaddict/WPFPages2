using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFPages . ViewModels;


namespace WPFPages .Views
{
	public class SQLHandlers
	{

		public static BankAccountViewModel bvm = MainWindow.bvm;
		public static CustomerViewModel cvm = MainWindow.cvm;
		public static DetailsViewModel dvm = MainWindow.dvm;
		//private static DataGridRow BankCurrentRow;
		//private static DataGridRow CustomerCurrentRow;
		//private static DataGridRow DetailsCurrentRow;

	//*****************************************************************************************//
		public async  Task<bool> UpdateDbRow (string CurrentDb, object  Row)
		{
			///TRIGGERED when a Cell is EDITED
			/// After a fight, this is now working and updates the relevant RECORD correctly
			///
			BankAccountViewModel ss = new BankAccountViewModel ( );
			CustomerViewModel cs = new CustomerViewModel();
			DetailsViewModel sa = new DetailsViewModel();
			if ( CurrentDb == "BANKACCOUNT" ) {
				ss = Row as BankAccountViewModel;
				if(ss == null) return false;
			}
			else if ( CurrentDb == "CUSTOMER" ){
				cs = Row as CustomerViewModel;
				if ( cs == null ) return false;
			}
			else if ( CurrentDb == "DETAILS" ){
				sa = Row as DetailsViewModel;
				if ( sa == null ) return false;
			}

			//Sanity checks - are values actualy valid ???
			//They should be as Grid vlaidate entries itself !!
			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS")
			{
		#region BANK/DETAILS UPDATE PROCESSING
				try
				{
					int x;
					decimal Y;
					if (CurrentDb == "BANKACCOUNT")
					{
						x = Convert.ToInt32 (ss.Id);
						x = Convert.ToInt32 (ss.AcType);
						//Check for invalid A/C Type
						if (x < 1 || x > 4)
						{
							Console.WriteLine ($"SQL UpdateDbRow(92) Invalid A/c type of {ss.AcType} in grid Data");
							MessageBox.Show ($"Invalid A/C Type ({ss.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
							return false;
						}
						Y = Convert.ToDecimal (ss.Balance);
						Y = Convert.ToDecimal (ss.IntRate);
						//Check for invalid Interest rate
						if (Y > 100)
						{
							Console.WriteLine ($"SQL UpdateDbRow(101) Invalid Interest Rate of {ss.IntRate} > 100% in grid Data");
							MessageBox.Show ($"Invalid Interest rate ({ss.IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!");
							return false;
						}
						DateTime dtm = Convert.ToDateTime (ss.ODate);
						dtm = Convert.ToDateTime (ss.CDate);
					}
					else if (CurrentDb == "DETAILS")
					{
						x = Convert.ToInt32 (sa.Id);
						x = Convert.ToInt32 (sa.AcType);
						//Check for invalid A/C Type
						if (x < 1 || x > 4)
						{
							Console.WriteLine ($"SQL UpdateDbRow(117) Invalid A/c type of {sa.AcType} in grid Data");
							MessageBox.Show ($"Invalid A/C Type ({sa.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
							return false;
						}
						Y = Convert.ToDecimal (sa.Balance);
						Y = Convert.ToDecimal (sa.IntRate);
						//Check for invalid Interest rate
						if (Y > 100)
						{
							Console.WriteLine ($"SQL UpdateDbRow(126) Invalid Interest Rate of {sa.IntRate} > 100% in grid Data");
							MessageBox.Show ($"Invalid Interest rate ({sa.IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!");
							return false;
						}
						DateTime dtm = Convert.ToDateTime (sa.ODate);
						dtm = Convert.ToDateTime (sa.CDate);
					}
					//					string sndr = sender.ToString();
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL UpdateDbRow(137) Invalid grid Data - {ex.Message} Data = {ex.Data}");
					MessageBox.Show ("Invalid data entered in the Grid !!!! - See Output for details.\r\nNEITHER Db has been updated !!");
					return false;
				}
				SqlConnection con =  null;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using (con = new SqlConnection (ConString))
					{
						con.Open ();
						if (CurrentDb == "BANKACCOUNT" )//|| CurrentDb == "DETAILS" )
						{
							SqlCommand cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (ss.Id));
							cmd.Parameters.AddWithValue ("@bankno", ss.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", ss.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (ss.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (ss.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (ss.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (ss.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (ss.CDate));
							cmd.ExecuteNonQuery();
							Console.WriteLine ("SQL Update successful for Bank Account Data...");

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (ss.Id));
							cmd.Parameters.AddWithValue ("@bankno", ss.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", ss.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (ss.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (ss.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (ss.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (ss.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (ss.CDate));
							cmd.ExecuteNonQuery();
							Console.WriteLine ("SQL Update successful for Secondary Accounts Data...");

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (ss.Id));
							cmd.Parameters.AddWithValue ("@bankno", ss.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", ss.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (ss.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (ss.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (ss.CDate));
							cmd.ExecuteNonQuery();
							Console.WriteLine ("SQL Update successful for Customers Data...");
						}
						else if (CurrentDb ==  "DETAILS" )
						{
							SqlCommand cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery();
							Console.WriteLine ("SQL Update successful for Bank Account Data...");

							cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@balance", Convert.ToDecimal (sa.Balance));
							cmd.Parameters.AddWithValue ("@intrate", Convert.ToDecimal (sa.IntRate));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery();
							Console.WriteLine ("SQL Update successful for Secondary Accounts Data...");

							cmd = new SqlCommand ("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (sa.Id));
							cmd.Parameters.AddWithValue ("@bankno", sa.BankNo.ToString ());
							cmd.Parameters.AddWithValue ("@custno", sa.CustNo.ToString ());
							cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (sa.AcType));
							cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (sa.ODate));
							cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (sa.CDate));
							cmd.ExecuteNonQuery();
							Console.WriteLine ("SQL Update successful for Customers Data...");
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Error UpdateDbRow(180) - BankAccount/Sec" +
						$"accounts not updated {ex.Message} Data = {ex.Data}");
				}
				finally
				{
					con.Close ();
				}
	#endregion BANK UPDATE PROCESSING
			}
			else if (CurrentDb == "CUSTOMER")
			{
		#region CUSTOMER UPDATE PROCESSING

//				if ( Row == null && CurrentDb == "CUSTOMER")
//					cs = Row.Item as CustomerViewModel;
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
						Console.WriteLine ($"SQL UpdateDbRow(204) Invalid A/c type of {cs.AcType} in grid Data");
						MessageBox.Show ($"Invalid A/C Type ({cs.AcType}) in the Grid !!!!\r\nPlease correct this entry!");
						return false;
					}
					DateTime dtm = Convert.ToDateTime (cs.ODate);
					dtm = Convert.ToDateTime (cs.CDate);
					dtm = Convert.ToDateTime (cs.Dob);
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"SQL Invalid grid Data UpdateDbRow(214)- {ex.Message} Data = {ex.Data}");
					MessageBox.Show ("Invalid data entered in the Grid !!!! - See Output for details");
					return false;
				}
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				con = new SqlConnection (ConString);
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using (con)
					{
						con.Open ();

						SqlCommand cmd = new SqlCommand ("UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
							"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
							"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con);


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
						cmd.ExecuteNonQuery();
						Console.WriteLine ("SQL Update successful for Customers Data...");

						cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						cmd.ExecuteNonQuery();
						Console.WriteLine ("SQL Update successful for Bank Account Data...");

						cmd = new SqlCommand ("UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
						cmd.Parameters.AddWithValue ("@id", Convert.ToInt32 (cs.Id));
						cmd.Parameters.AddWithValue ("@bankno", cs.BankNo.ToString ());
						cmd.Parameters.AddWithValue ("@custno", cs.CustNo.ToString ());
						cmd.Parameters.AddWithValue ("@actype", Convert.ToInt32 (cs.AcType));
						cmd.Parameters.AddWithValue ("@odate", Convert.ToDateTime (cs.ODate));
						cmd.Parameters.AddWithValue ("@cdate", Convert.ToDateTime (cs.CDate));
						cmd.ExecuteNonQuery();
						Console.WriteLine ("SQL Update successful for Secondary Accounts Data...");
					}
				}
				catch (Exception ex)
				{
					con.Close ();
					Console.WriteLine ($"SQL Error UpdateDbRow(255)- {ex.Message} Data = {ex.Data}");
				}
				finally
				{
					//Lets force the grids to update when we return from here ??
					Console.WriteLine ($"SQL - Updated Row for {CurrentDb}");
					con.Close();
				}
				return true;
	#endregion CUSTOMER UPDATE PROCESSING
			}
			return true;

		}

	}
}
