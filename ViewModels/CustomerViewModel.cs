using DocumentFormat.OpenXml.Spreadsheet;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using WPFPages.DataSources;

using WPFPages.Views;

namespace WPFPages.ViewModels
{
	public class CustomerViewModel: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static List<CustomerViewModel> CustomersList = new List<CustomerViewModel> ();
		private ObservableCollection<CustomerViewModel> _customersObs = new ObservableCollection<CustomerViewModel> (CustomersList);

		public static ObservableCollection<CustomerViewModel> CustomersObs { get; set; }


		#region Setup
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
		private int selectedRow;


		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";
		public bool FilterResult = false;
		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";
		public bool isMultiMode = false;


		public  static DataTable dtCust = new DataTable	();
		#region Setters
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
		public int SelectedRow
		{
			get { return selectedRow; }
			set
			{
				selectedRow = value;
				OnPropertyChanged (selectedRow.ToString ());
			}
		}
		#endregion Setters
		#endregion setup

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

		/// <summary>
		///  Set the Customer Db up
		/// </summary>
		/// <param name="dtcust"></param>
		public static  bool LoadCustomersCollection (DataTable dtcust)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			dtCust.Clear ();
			if (CustomersObs != null)
				CustomersObs.Clear ();
			else
				CustomersObs = new ObservableCollection<CustomerViewModel> (CustomersList);
			//Load data via SQL from Db

			if (!FillCustomersDataGrid (dtCust, true))
				return false;
			// load data into List<Customer>
			if (!LoadCustomerCollection (dtCust))
				return false;
			if (CustomersObs?.Count > 0)
				CustomersObs.Clear ();
			// assign data above to OC
			Mouse.OverrideCursor = Cursors.Arrow;
			CustomersObs = new ObservableCollection<CustomerViewModel> (CustomersList);
			if (CustomersObs.Count > 0)
				return true;
			else return false;
		}
		#region SQL data handling


		public static bool FillCustomersDataGrid (DataTable dtCust, bool Multiaccounts)
		{
			string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			string CmdString = string.Empty;
			SqlConnection con;
			//	if (isMultiMode)
			//	{
			//		//THIS IS THE SQL COMMAND TO GET FULL LINES OF DUPLICATED CUSTOMER ACCOUNT #'S DATA
			//		CmdString = "SELECT * FROM Customer WHERE CUSTNO IN "
			//			+ "(SELECT CUSTNO FROM CUSTOMER "
			//			+ " GROUP BY CUSTNO"
			//			+ " HAVING COUNT(*) > 1)";
			//		//clear the datatable first as we are only showing a subset
			//		dtCust.Clear ();
			//	}
			CmdString = "SELECT * FROM Customer";
			//clear the datatable first as we are only showing a subset
			//				}
			if (dtCust.Rows.Count > 0)
				dtCust.Clear ();
			bool result = LoadSqlData (CmdString, dtCust, "CUSTOMER");
			//			isMultiMode = false;
			return result;

		}

		public static bool LoadSqlData (string CmdString, DataTable dt, string CallerType)
		{
			try
			{
				SqlConnection con;
				string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				using (con = new SqlConnection (ConString))
				{
					SqlCommand cmd = new SqlCommand (CmdString, con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					// vTHIS LOADS OUR LIST<CUSTOMER>
					//if (!LoadCustomerCollection (dt))
					//	return false;
					////StatusBar.Text = $"Customer Account loaded successfully ({dt.Rows.Count}) records";
					//else
					//{
						//reset filter flag
						//						isMultiMode = false;
					if(dt.Rows.Count > 0) return true;
					else  return false;
					//}
				}
				//reset filter flag

#pragma TODO MVVM
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
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load Bank Details - {ex.Message}");
				return false;
			}
			return true;
		}

		//*****************************************************************************************//
		public static bool LoadCustomerCollection (DataTable dtCust)
		{
			//Load the data into our List<Customer>
			if (CustomersList.Count > 0)
				CustomersList.Clear ();
			try
			{
				for (int i = 0; i < dtCust.Rows.Count; ++i)
					CustomersList.Add (new CustomerViewModel
					{
						Id = Convert.ToInt32 (dtCust.Rows[i][0]),
						CustNo = dtCust.Rows[i][1].ToString (),
						BankNo = dtCust.Rows[i][2].ToString (),
						AcType = Convert.ToInt32 (dtCust.Rows[i][3]),
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
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Error loading Details Data {ex.Message}");
				return false;
			}

		}

		#endregion SQL data handling
	}
}