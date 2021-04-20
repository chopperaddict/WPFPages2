//if NOT set, dtDetails is persistent
//#define PERSISTENTDATA
#define TASK1
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using WPFPages.Views;

namespace WPFPages.ViewModels
{

	public class DetailsViewModel: INotifyPropertyChanged
	{
		public static BankAccountViewModel bvm = MainWindow.bvm;
		public static CustomerViewModel cvm = MainWindow.cvm;
		public static DetailsViewModel dvm = MainWindow.dvm;

		public event PropertyChangedEventHandler PropertyChanged;

		// Setup our data structures here for SecAccounts Database
		//		public static List<DetailsViewModel> DetailsList = new List<DetailsViewModel> ();
		//		public ObservableCollection<DetailsViewModel> _DetailsObs = new ObservableCollection<DetailsViewModel> (DetailsList);
		public ObservableCollection<DetailsViewModel> _DetailsObs = new ObservableCollection<DetailsViewModel> ();

		public ObservableCollection<DetailsViewModel> DetailsObs
		{
			get { return _DetailsObs; }
			set
			{
				_DetailsObs = value; OnPropertyChanged (DetailsObs.ToString ());
				OnPropertyChanged (cvm.CustomersObs.ToString ());
				OnPropertyChanged (bvm.BankAccountObs.ToString ());
			}
			//			set { _DetailsObs = value; OnPropertyChanged (DetailsObs.ToString ()); if (NotifyOfDataChange != null) NotifyOfDataChange (this); }
		}

		private void DetailsObs_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Type t = sender.GetType ();
			if (!t.FullName.Contains ("ViewModels.DetailsViewModel"))
				Console.WriteLine ($"DetailsViewModel has received a notofication that  the Customer Obs collection has changhed..... YEAH");
		}
		/// <summary>
		/// Callback for db change notifications
		/// </summary>
		/// <param name="sender"></param>
		public void DbHasChanged (object sender, DataChangeArgs args)
		{
			DataGrid cvm = sender as DataGrid;
			if (cvm.Name != "DetailsGrid")
				Console.WriteLine ($"\n^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\nDetailsViewModel has received DbHasChanged notification\ndue toUpdate of \"{args.DbName}\" Db");
			if (args.DbName != "DETAILS")
			{
				// need to update our Collection
				DataGrid d = sender as DataGrid;
				{
					d.Refresh ();
					try
					{
						if (Flags.SqlDetGrid == null) return;

						if (args.DbName != "DETAILS")
						{
							int curr = Flags.SqlDetGrid.SelectedIndex;
							if (curr >= 0)
							{
								LoadDetailsTask ();
								Flags.SqlDetGrid.SelectedIndex = curr;
								Flags.SqlDetGrid.Refresh ();
							}
							Console.WriteLine ($"\nDB REFRESH performed in DetailsViewModel");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine ($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\nDB REFRESH UPDATE ERROR in DetailsViewModel\n{ex.Message} : {ex.Data}");
					}

				}
			}
		}













		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;
		public static DataTable dtDetails = null;

		//==================================
		//Delegate & Event handler for Db Updates
		//==================================
		public delegate void DbUpdated (object sender);
		public DbUpdated NotifyOfDataChange;

		/// <summary>
		///  A Delegate declared in SqlDbViewer to notify all ViewModels when a data change occurs
		/// </summary>

		private static void RefreshObs (object sender)
		{
			//			NotifyOfDataChange (sender);
			Console.WriteLine ($"\n()()()()()()()()()()()()()()()()()()()()   Message received in CustomerView Model due to a Db Update");
		}

		// CONSTRUCTOR
		public DetailsViewModel ()
		{
			//			Console.WriteLine ($"Details ViewModel Constructor - Subscribed to DBUPDATED(NotifyOfDataChange) Event");
			DetailsObs.CollectionChanged += DetailsObs_CollectionChanged;

		}

		public void SubscribeToChangeEvents ()
		{
			// subscribe to Data chnaged event fired by SqlDbviewer
			SqlDbViewer sqlv = new SqlDbViewer ('C');
			// assign event handler function
			sqlv.NotifyOfDataChange += DbHasChanged;

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
		public decimal Balance
		{
			get { return balance; }
			set { balance = value; OnPropertyChanged (Balance.ToString ()); }
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

		#endregion properties

		#region INotifyProp		
		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}
		#endregion INotifyProp		


		public static void UpdateBankDb (string CallerDb)
		{
			if (CallerDb == "DETAILS")
			{
				//				string Cmd = "Update Bankaccount, "
			}
		}



		//**************************************************************************************************************************************************************//
		/// <summary>
		///  Initialise Data for Details Grid using Task.Factory thread
		///  and it then triggers the [TriggerCustDataLoad Event]
		///  and finally ensures the ItemsSource is set to the ObsCollection
		///  once ithe Event has triggered all theother load code ?
		///  Called from DBSELECTOR
		/// </summary>
		/// <param> </param>
		public async Task LoadDetailsTask ()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			// load SQL data in DataTable
			//Create the one and only dtDetails instance if not already there

			if (dtDetails == null)
				DetailsViewModel.dtDetails = new DataTable ();
			else
				dtDetails.Clear ();
			try
			{
				if (DetailsObs != null && DetailsObs.Count > 0)
					DetailsObs.Clear ();
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"DetailsObs Exception [{ex.Data}\r\n");
			}
			DateTime start = DateTime.Now;
			Console.WriteLine ($"Starting AWAITED task to load Details  Data via Sql");
#if USETASK
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
#else
			try
			{
				{
					await LoadSqlData ();
					await LoadDetailsObsCollection ();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Task error {ex.Data},\n{ex.Message}");
			}
			Mouse.OverrideCursor = Cursors.Arrow;
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs >?		
		}
#endif


		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public async static Task<DataTable> LoadSqlData ()
		{
			try
			{
				if (dtDetails.Rows.Count > 0)
					dtDetails.Clear ();
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection (ConString);
				using (con)
				{
					SqlCommand cmd = new SqlCommand ("Select * from SecAccounts order by BankNo", con);
					SqlDataAdapter sda = new SqlDataAdapter (cmd);
					sda.Fill (dtDetails);
					return dtDetails;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load Details Details - {ex.Message}, {ex.Data}");
				return dtDetails;
			}
			return dtDetails;
		}
		//**************************************************************************************************************************************************************//
		// Loads data from DataTable into Observable Collection
		public async Task<bool> LoadDetailsObsCollection ()
		{
			//if (dtDetails.Rows.Count > 0)
			//	return true;
			try
			{
				//Load the data into our ObservableCollection BankAccounts
				if (DetailsObs.Count > 0)
				{ DetailsObs.Clear (); }
				for (int i = 0; i < DetailsViewModel.dtDetails.Rows.Count; ++i)
					DetailsObs.Add (new DetailsViewModel
					{
						Id = Convert.ToInt32 (dtDetails.Rows[i][0]),
						BankNo = dtDetails.Rows[i][1].ToString (),
						CustNo = dtDetails.Rows[i][2].ToString (),
						AcType = Convert.ToInt32 (dtDetails.Rows[i][3]),
						Balance = Convert.ToDecimal (dtDetails.Rows[i][4]),
						IntRate = Convert.ToDecimal (dtDetails.Rows[i][5]),
						ODate = Convert.ToDateTime (dtDetails.Rows[i][6]),
						CDate = Convert.ToDateTime (dtDetails.Rows[i][7])
					});
				// WE NOW HAVE OUR DATA HERE - fully loaded into Obs 
				Console.WriteLine ($"Loaded Sql data into DetailsObs directly....");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Error loading Details Data {ex.Message}");
				return false;
			}

		}

		//**************************************************************************************************************************************************************//
	}
}
