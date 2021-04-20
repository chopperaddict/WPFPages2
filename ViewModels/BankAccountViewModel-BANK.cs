using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPages.ViewModels
{
	/// <summary>
	///  This partial BankAccountViewModel class handles all BANKACCOUNT related items
	/// </summary>
	public partial class BankAccountViewModel: INotifyPropertyChanged
	{
		public static BankAccountViewModel bvm = MainWindow.bvm;
		public static CustomerViewModel cvm = MainWindow.cvm;
		public static DetailsViewModel dvm = MainWindow.dvm;

		public static List<BankAccountViewModel> BankList = new List<BankAccountViewModel> ();
		public DbUpdated NotifyOfDataChange;

		// Set the Observable collection up so it is Notifiable (	INotifyPropertyChanged())
		//		public ObservableCollection<BankAccountViewModel> BankAccountObs { get; set; }
		public  ObservableCollection<BankAccountViewModel> _bankAccountObs = new ObservableCollection<BankAccountViewModel> (BankList);
		public ObservableCollection<BankAccountViewModel> BankAccountObs
		{
			get { return _bankAccountObs; }
			set
			{
				_bankAccountObs = value;
				SqlDbViewer sqlv = new SqlDbViewer ('B');
				OnPropertyChanged (BankAccountObs.ToString ());
				OnPropertyChanged (cvm.CustomersObs.ToString ());
				OnPropertyChanged (dvm.DetailsObs.ToString ());

				//				OnPropertyChanged (BankAccountObs.ToString (); if (NotifyOfDataChange != null) NotifyOfDataChange (this);
			}
		}

		public void BankAccountObs_CollectionChanged ()
		{
			if (NotifyOfDataChange != null) NotifyOfDataChange (this);
		}


//		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		///  Function that is called to broadcast a notification to whoever to 
		///  notify that one of the Obs collections has been changed by something
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		private void SendDataChanged (object o, string dbName)
		{
			DataChangeArgs dca = new DataChangeArgs ();
			dca.SenderName = o.ToString ();
			dca.DbName = dbName;
			if (NotifyOfDataChange != null)
			{
				NotifyOfDataChange (this);
			}
		}

		#region SQL data loading - including special Task handler code
		static async Task HandleTask (Task task)
		{
			task.Await (OnCompleted);
			int x = 0;
			if (x != 0)
			{
				//These two work
				//			var t =  Task.Run (() => FillBankAccountDataGrid (dtBank));
				//			t.Wait ();

				//So do these two, (or first one alone id load is done externally)
				// This uses a special Class Taskextensions.cs that handles the AWAIT of whatever task is passed  into it
				//using the syntax 
				//			FillBankAccountDataGrid (dtBank).Await ();
				//			LoadBankAccountIntoList (BankList, dtBank);
			}
		}



		/// <summary>
		/// Load SQL Db from SQL Server
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DataTable LoadSqlData (DataTable dt, int mode = -1)
		{
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection (ConString);
				using (con)
				{
//					SqlCommand cmd = new SqlCommand ("Select * from BankAccount order by CustNo", con);
					string commandline = "";
					commandline = "Select * from BankAccount order by ";
					if (mode == -1)         // default
						commandline += "CustNo";
					else if (mode == 1)
						commandline += "BankNo";
					else if (mode == 2)
						commandline += "Id";
					else if (mode == 3)
						commandline += "AcType";
					else if (mode == 4)
						commandline += "Dob";
					else if (mode == 5)
						commandline += "Odate";

					SqlCommand cmd = new SqlCommand (commandline, con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dt);
					return dt;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load Bank Details - {ex.Message}, {ex.Data}"); return (DataTable)null;
			}
			return (DataTable)null;
		}
		#endregion SQL data loading

		#region General Functionality
		public void BankAccountObs_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged ("BankAccountObs");

		}

		private static void OnCompleted ()
		{
			Console.WriteLine ("SQL data loading completed....");
		}


		public static void UpdateBankDb (DataTable dtBank, string CallerDb)
		{
			if (CallerDb == "BANKACCOUNT")
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
			set { id = value; OnPropertyChanged (Id.ToString ()); }
		}
		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged (BankNo.ToString ()); }
		}
		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged (CustNo.ToString ()); }
		}
		public int AcType
		{
			get { return actype; }
			set
			{actype = value; OnPropertyChanged (AcType.ToString ()); }
		}
		public decimal Balance
		{
			get { return balance; }
			set
			{balance = value; OnPropertyChanged (Balance.ToString ()); }
		}
		public decimal IntRate
		{
			get { return intrate; }
			set { intrate = value; OnPropertyChanged (IntRate.ToString ()); }
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
			set { selectedItem = value; OnPropertyChanged (SelectedItem.ToString ()); }
		}
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set { selectedIndex = value; OnPropertyChanged (SelectedIndex.ToString ()); }
		}
		#endregion Class properties

	}
}
