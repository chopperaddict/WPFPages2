using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFPages.DataSources;
using WPFPages.ViewModels;

namespace WPFPages.Views
{
	/// <summary>
	/// Interaction logic for EditDb.xaml
	/// </summary>
	public partial class EditDb: INotifyPropertyChanged
	{

		event SQLViewerGridSelectionChanged EditDbSelChange;

		DataTable dt = new DataTable ();
		SQLDbSupport sqlsupport = new SQLDbSupport ();
		int CurrentIndex = -1;
		object CurrentItem = null;
		SqlDbViewer sqldbv = null;
		public DataGrid CurrentGrid = null;
		public string CurrentDb = "";
		bool bDirty = false;
		//		bool testing = false;
		public newtester nt = null;
		public BankAccountViewModel Bank;

		public EditDb ()
		{
			int x = 0;
		}

		/// <summary>
		/// CONSTRUCTOR for Data Editing Window
		/// </summary>		
		public EditDb (string Caller, int index, object Item, SqlDbViewer sqldb)
		{
			//This loads the Observable Collection(s)
			sqldbv = sqldb;
			CurrentIndex = index;
			CurrentItem = Item;
			CurrentDb = Caller;
			InitializeComponent ();
			SetupBackgroundGradient ();
			//Store our Grids so the EVent code can access them

			//subscribe to our SelectedIndex Event ?? Duplicate call - YES !!
			//			EditDbSelChange += new SQLViewerGridSelectionChanged  (resetEditDbindex);
			//			Console.WriteLine ($"EditDb(76) Subscribed to EditDbSelChange");
		}
		private void SetupBackgroundGradient ()
		{
			//Get a new LinearGradientBrush
			LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush ();
			//Set the start and end points of the drawing
			myLinearGradientBrush.StartPoint = new Point (1.3, 0);
			myLinearGradientBrush.EndPoint = new Point (0.0, 1);
			if (CurrentDb == "BANKACCOUNT")
			// Gradient Stops below are light to dark
			{
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.PowderBlue, 1.0));
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.LightSteelBlue, 0.5));
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.DodgerBlue, 0.25));
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ();
				myLinearGradientBrush2.StartPoint = new Point (0.5, 0);
				myLinearGradientBrush2.EndPoint = new Point (0.5, 1);

				GradientStop gs1 = new GradientStop ();
				GradientStop gs2 = new GradientStop ();
				GradientStop gs3 = new GradientStop ();
				GradientStop gs4 = new GradientStop ();
				GradientStop gs5 = new GradientStop ();
				GradientStop gs6 = new GradientStop ();
				gs1.Color = Color.FromArgb (0xFF, 0x6B, 0x8E, 0x95);
				gs2.Color = Color.FromArgb (0xFF, 0x14, 0xA7, 0xC1);
				gs3.Color = Color.FromArgb (0xFF, 0x1E, 0x42, 0x4E);
				gs4.Color = Color.FromArgb (0xFF, 0x1D, 0x48, 0x55);
				gs5.Color = Color.FromArgb (0xFF, 0x1D, 0x48, 0x55);
				gs6.Color = Color.FromArgb (0xFF, 0x19, 0x3A, 0x44);
				gs1.Offset = 1;
				gs2.Offset = 0.509;
				gs3.Offset = 0.542;
				gs4.Offset = 0.542;
				gs5.Offset = 0.526;
				gs6.Offset = 0;
				myLinearGradientBrush2.GradientStops.Add (gs1);
				myLinearGradientBrush2.GradientStops.Add (gs2);
				myLinearGradientBrush2.GradientStops.Add (gs3);
				myLinearGradientBrush2.GradientStops.Add (gs4);
				myLinearGradientBrush2.GradientStops.Add (gs5);
				myLinearGradientBrush2.GradientStops.Add (gs6);
				Cancelbtn.Background = myLinearGradientBrush2;
				Savebtn.Background = myLinearGradientBrush2;
			}
			if (CurrentDb == "CUSTOMER")
			{
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.White, 1.0));
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.Gold, 0.3));
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.DarkKhaki, 0.0));
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ();
				myLinearGradientBrush2.StartPoint = new Point (0.5, 0);
				myLinearGradientBrush2.EndPoint = new Point (0.5, 1);

				GradientStop gs1 = new GradientStop ();
				GradientStop gs2 = new GradientStop ();
				GradientStop gs3 = new GradientStop ();
				GradientStop gs4 = new GradientStop ();
				GradientStop gs5 = new GradientStop ();
				GradientStop gs6 = new GradientStop ();
				//Yellow buttons
				gs1.Color = Color.FromArgb (0xFF, 0x7a, 0x6f, 0x2d);
				gs2.Color = Color.FromArgb (0xFF, 0xf5, 0xd8, 0x16);
				gs3.Color = Color.FromArgb (0xFF, 0x7d, 0x70, 0x15);
				gs4.Color = Color.FromArgb (0xFF, 0x5e, 0x56, 0x2a);
				gs5.Color = Color.FromArgb (0xFF, 0x59, 0x50, 0x13);
				gs6.Color = Color.FromArgb (0xFF, 0x38, 0x32, 0x0c);
				gs1.Offset = 1;
				gs2.Offset = 0.509;
				gs3.Offset = 0.542;
				gs4.Offset = 0.542;
				gs5.Offset = 0.526;
				gs6.Offset = 0;
				myLinearGradientBrush2.GradientStops.Add (gs1);
				myLinearGradientBrush2.GradientStops.Add (gs2);
				myLinearGradientBrush2.GradientStops.Add (gs3);
				myLinearGradientBrush2.GradientStops.Add (gs4);
				myLinearGradientBrush2.GradientStops.Add (gs5);
				myLinearGradientBrush2.GradientStops.Add (gs6);
				Cancelbtn.Background = myLinearGradientBrush2;
				Savebtn.Background = myLinearGradientBrush2;
			}
			if (CurrentDb == "DETAILS")
			{
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.White, 1.0));
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.Green, 0.5));
				myLinearGradientBrush.GradientStops.Add (
					new GradientStop (Colors.DarkGreen, 0.25));
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ();
				myLinearGradientBrush2.StartPoint = new Point (0.5, 0);
				myLinearGradientBrush2.EndPoint = new Point (0.5, 1);

				GradientStop gs1 = new GradientStop ();
				GradientStop gs2 = new GradientStop ();
				GradientStop gs3 = new GradientStop ();
				GradientStop gs4 = new GradientStop ();
				GradientStop gs5 = new GradientStop ();
				GradientStop gs6 = new GradientStop ();
				gs1.Color = Color.FromArgb (0xFF, 0x75, 0xDD, 0x75);
				gs2.Color = Color.FromArgb (0xFF, 0x00, 0xFF, 0x00);
				gs3.Color = Color.FromArgb (0xFF, 0x33, 0x66, 0x33);
				gs4.Color = Color.FromArgb (0xFF, 0x44, 0x55, 0x44);
				gs5.Color = Color.FromArgb (0xFF, 0x33, 0x55, 0x55);
				gs6.Color = Color.FromArgb (0xff, 0x22, 0x40, 0x22);
				gs1.Offset = 1;
				gs2.Offset = 0.509;
				gs3.Offset = 0.542;
				gs4.Offset = 0.542;
				gs5.Offset = 0.526;
				gs6.Offset = 0;
				myLinearGradientBrush2.GradientStops.Add (gs1);
				myLinearGradientBrush2.GradientStops.Add (gs2);
				myLinearGradientBrush2.GradientStops.Add (gs3);
				myLinearGradientBrush2.GradientStops.Add (gs4);
				myLinearGradientBrush2.GradientStops.Add (gs5);
				myLinearGradientBrush2.GradientStops.Add (gs6);
				Cancelbtn.Background = myLinearGradientBrush2;
				//				Cancelbtn.Foreground = Color.Add(Black, Black);
				Savebtn.Background = myLinearGradientBrush2;
				//				Savebtn.Foreground = Color.Black;

			}
			// Use the brush to paint the rectangle.
			Background = myLinearGradientBrush;

		}
		#region OLbservable declarations
		public ObservableCollection<BankAccountViewModel> _bankaccountCollection;
		public ObservableCollection<Customer> _customeraccountCollection;
		public ObservableCollection<SecAccounts> _detailsaccountCollection;

		public ObservableCollection<BankAccountViewModel> BankAccountCollection
		{
			get { return _bankaccountCollection; }
			set { _bankaccountCollection = value; OnPropertyChanged ("_bankaccountCollection"); }
		}
		public ObservableCollection<Customer> CustomerAccountCollection
		{
			get { return _customeraccountCollection; }
			set { _customeraccountCollection = value; OnPropertyChanged ("_customeraccountCollection"); }
		}
		public ObservableCollection<SecAccounts> DetailsAccountCollection
		{
			get { return _detailsaccountCollection; }
			set { _detailsaccountCollection = value; OnPropertyChanged ("_detailsaccountCollection"); }
		}

		#endregion OLbservable declarations

		private void WindowLoaded (object sender, RoutedEventArgs e)
		{
			if (CurrentDb == "BANKACCOUNT")
			{
				BankEditGrid.Visibility = Visibility.Visible;
				CustomerEditGrid.Visibility = Visibility.Collapsed;
				DataGrid1.Visibility = Visibility.Visible;
				DataGrid2.Visibility = Visibility.Collapsed;

				// got our SQL data - working 29/3/21
				DataGrid1.ItemsSource = BankAccountViewModel.BankAccountObs;
				BankEditGrid.DataContext = BankAccountViewModel.BankAccountObs;
				//DataGrid1.ItemsSource = sqldbv.BankAccountObs;
				this.Title += " Bank Accounts Db";
				DataGrid1.SelectedIndex = CurrentIndex;
				DataGrid1.SelectedItem = CurrentItem;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer

				DataGridNavigation.SelectRowByIndex (DataGrid1, CurrentIndex, -1);
				// set up our windows dragging
				this.MouseDown += delegate { DoDragMove (); };
				CurrentGrid = DataGrid1;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console.WriteLine ($"EditDb(262) Window just loaded : getting instance of newtester class with this,DataGrid1,\"EDITDB\"");
				newtester.SetWindowHandles (this, null);
				new newtester (DataGrid1, "EDITDB", out nt);
				DataGrid1.Focus ();
			}
			else if (CurrentDb == "CUSTOMER")
			{
				BankEditGrid.Visibility = Visibility.Collapsed;
				CustomerEditGrid.Visibility = Visibility.Visible;
				DataGrid1.Visibility = Visibility.Collapsed;
				DataGrid2.Visibility = Visibility.Visible;
				CustomerAccountCollection = new ObservableCollection<Customer> ();
				LoadSqlData ("Select * from Customer ", dt, "CUSTOMER");
				// use this or the next one
				DataGrid2.ItemsSource = CustomerAccountCollection;
				//DataGrid1.ItemsSource = sqldbv.CustomerObs;
				this.Title += " Customer Accounts Db";
				DataGrid2.SelectedIndex = CurrentIndex;
				DataGrid2.SelectedItem = CurrentItem;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation.SelectRowByIndex (DataGrid2, CurrentIndex, -1);
				this.MouseDown += delegate { DoDragMove (); };
				CurrentGrid = DataGrid2;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console.WriteLine ($"EditDb(287) Window just loaded :  getting instance of newtester class with this,DataGrid2,\"EDITDB\"");
				new newtester (DataGrid2, "EDITDB", out nt);
				DataGrid2.Focus ();
			}
			else if (CurrentDb == "DETAILS")
			{
				BankEditGrid.Visibility = Visibility.Visible;
				CustomerEditGrid.Visibility = Visibility.Collapsed;
				DataGrid1.Visibility = Visibility.Visible;
				DataGrid2.Visibility = Visibility.Collapsed;
				DetailsAccountCollection = new ObservableCollection<SecAccounts> ();
				LoadSqlData ("Select * from SecAccounts ", dt, "DETAILS");
				// use this or the next one
				DataGrid1.ItemsSource = DetailsAccountCollection;
				//DataGrid1.ItemsSource = sqldbv.DetailsObs;
				this.Title += " Secondary Accounts Db";
				DataGrid1.SelectedIndex = CurrentIndex;
				DataGrid1.SelectedItem = CurrentItem;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation.SelectRowByIndex (DataGrid1, CurrentIndex, -1);
				this.MouseDown += delegate { DoDragMove (); };
				CurrentGrid = DataGrid1;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console.WriteLine ($"EditDb(312) Window just loaded :  getting instance of newtester class with this,DataGrid1,\"EDITDB\"");
				new newtester (DataGrid1, "EDITDB", out nt);
				DataGrid1.Focus ();
			}
			// ensure our dirty flag is Set correctly
			bDirty = false;
		}


		private void SaveData_Click (object sender, RoutedEventArgs e)
		{

			bDirty = false;
			sqldbv.EditDbClosing ();
			Close ();
			sqldbv = null;
		}

		private void Cancel_Click (object sender, RoutedEventArgs e)
		{
			if (bDirty)
			{
				if (MessageBox.Show ("You have unsaved changes to the current record.\r\nDo you want to save them now ?", "Possible Data Loss",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question,
					MessageBoxResult.Yes) == MessageBoxResult.Yes)
				{
					// save the data and then close down
				}
			}
			bDirty = false;
			sqldbv.EditDbClosing ();
			sqldbv = null;
			Close ();
		}

		public bool LoadSqlData (string CmdString, DataTable dt, string CallerType)
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
					//load the SQL data into our OC named bankaccounts
					if (CallerType == "BANKACCOUNT")
					{
						if (! BankAccountViewModel.LoadBankAccountIntoList (BankAccountViewModel.BankList, dt))
							return false;
					}
					else if (CallerType == "CUSTOMER")
					{
						SqlDbViewer sqldbv = new SqlDbViewer (1);
						if (!LoadCustomerCollection (dt))
							return false;
					}
					else if (CallerType == "DETAILS")
					{
						SqlDbViewer sqldbv = new SqlDbViewer (2);
						if (!LoadDetailsCollection (dt))
							return false;
					}

					//reset filter flag
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Failed to load SQL Database - {ex.Message}");
				return false;
			}
			return true;
		}

		public bool LoadBankCollection (DataTable dtBank)
		{
			//Load the data into our ObservableCollection BankAccounts
			//			try
			//			{
			if (BankAccountCollection != null)
			{
				//				SelectedBankAccount = null;
				try
				{
					BankAccountCollection.Clear ();
				}
				catch (Exception ex) { Console.WriteLine ($"Failed to load clear Bank Details Obslist - {ex.Message}"); }
			}
			//This DOES access BankAccount.cs
			for (int i = 0; i < dtBank.Rows.Count; ++i)
			{
				BankAccountCollection.Add (new BankAccountViewModel
				{
					Id = Convert.ToInt32 (dtBank.Rows[i][0]),
					BankNo = dtBank.Rows[i][1].ToString (),
					CustNo = dtBank.Rows[i][2].ToString (),
					AcType = Convert.ToInt32 (dtBank.Rows[i][3]),
					Balance = Convert.ToDecimal (dtBank.Rows[i][4]),
					IntRate = Convert.ToDecimal (dtBank.Rows[i][5]),
					ODate = Convert.ToDateTime (dtBank.Rows[i][6]),
					CDate = Convert.ToDateTime (dtBank.Rows[i][7]),
				});
			}
			return true;
		}
		public bool LoadCustomerCollection (DataTable dtCust)
		{
			//Load the data into our ObservableCollection Customer
			try
			{
				if (CustomerAccountCollection.Count > 0)
					CustomerAccountCollection.Clear ();
			}
			catch
			{ }
			try
			{
				for (int i = 0; i < dtCust.Rows.Count; ++i)
					CustomerAccountCollection.Add (new Customer
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
		public bool LoadDetailsCollection (DataTable dtDetails)
		{
			//Load the data into our ObservableCollection BankAccounts
			if (DetailsAccountCollection.Count > 0)
				DetailsAccountCollection.Clear ();
			try
			{
				for (int i = 0; i < dtDetails.Rows.Count; ++i)
					DetailsAccountCollection.Add (new SecAccounts
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
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"Error loading Details Data {ex.Message}");
				return false;
			}

		}


		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}


		#endregion


		private void DataGrid2_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			//			DataGridNavigation.SelectRowByIndex (DataGrid2, DataGrid2.SelectedIndex, -1);
			if (sqldbv == null) return;
			sqldbv.CustomerGrid.SelectedIndex = DataGrid2.SelectedIndex;
#pragma TODO
			//This is where we SHOULD be updating the textboxes ???
		}


		private void CheckClosing (object sender, CancelEventArgs e)
		{
			// window is closing, check for data loss
			if (bDirty)
			{
				if (MessageBox.Show ("Window is being closed by it's parent.\r\nDo you want to save any changes ?", "Possible Data Loss",
					MessageBoxButton.OKCancel,
					MessageBoxImage.Question,
					MessageBoxResult.Cancel) == MessageBoxResult.OK)
				{
					// save the data
					SaveData_Click (sender, null);
				}
			}
		}
		private void DoDragMove ()
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....
			try
			{
				this.DragMove ();
			}
			catch
			{
				return;
			}
		}

		/// <summary>
		///  this is called by an ~Event triggered by SqlDBViewer changing it's SelectedIndex
		///  so we can update to match
		/// </summary>
		/// <param name="selectedRow"></param>
		/// <param name="caller"></param>
		public void resetEditDbindex (int selectedRow, DataGrid caller)
		{
			int x = 0;
			x = selectedRow;
			//			CurrentDb = caller;

			//			if (CurrentDb == "") return;
			// use our Global Grid pointer for access
			if (caller.Name == "BankGrid")
			{
				Console.WriteLine ($"resetEditDbindex(52) in EditDb - Called - Current index = {DataGrid1.SelectedIndex} received row change of {x} from SqlDbViewer() for {caller.CurrentItem}");
				if (DataGrid1.SelectedIndex != x)
				{
					if (DataGrid1.SelectedIndex + 10 < DataGrid1.Items.Count)
						DataGridNavigation.SelectRowByIndex (DataGrid1, x, -1);
					else
						DataGridNavigation.SelectRowByIndex (DataGrid1, x, -1);
				}
			}
			else
			{
				Console.WriteLine ($"resetEditDbindex(56) in EditDb Called - Current index = {DataGrid2.SelectedIndex} received row change of {x} from SqlDbViewer() for {caller.CurrentItem}");
				if (DataGrid2.SelectedIndex != x)
				{
					DataGrid2.SelectedIndex = x;
					DataGridNavigation.SelectRowByIndex (DataGrid2, DataGrid2.SelectedIndex, -1);
				}
			}
		}

		#region check for changes to text boxes
		private void BanknoEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void CustnoEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void BalanceEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void IntRateEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void OpenDateEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void CloseDateEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void AcTypeEdit_TextChanged (object sender, TextChangedEventArgs e)
		{
			//			BindingExpression be = ActypeEdit.GetBindingExpression (TextBox.TextProperty);
			//			be.UpdateSource ();
			//			e.Handled = false;
			DataGrid1.UpdateLayout ();
		}
		private void FirstNameEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void LastNameEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void Addr1Edit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void Addr2Edit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void TownEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void CountyEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void PcodeEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void PhoneEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void MobileEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		private void DobEdit_TextChanged (object sender, TextChangedEventArgs e)
		{ bDirty = true; }
		#endregion check for changes to text boxes
		private void BankEditGrid_KeyDown (object sender, KeyEventArgs e)
		{
			//Stop Enter key making it move selection
			//			if(e.Key == Key.Enter)
			//				e.Handled = true;
		}

		private void BankEditGrid_SourceUpdated (object sender, DataTransferEventArgs e)
		{
			int x = 0;
		}

		private void BankEditGrid_TargetUpdated (object sender, DataTransferEventArgs e)
		{
			int x = 0;
		}

		private void BankEditGrid_TextInput (object sender, TextCompositionEventArgs e)
		{
			int x = 0;
		}

		private void DataGrid1_CellEditEnding (object sender, DataGridCellEditEndingEventArgs e)
		{

		}

		private void DataGrid1_RowEditEnding (object sender, DataGridRowEditEndingEventArgs e)
		{

		}

		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid1_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			BankAccountViewModel objItemToEdit = DataGrid1.SelectedItem as BankAccountViewModel;

			DataGrid dg = (DataGrid)sender;
			DataRowView SelRow = dg.SelectedItem as DataRowView;
			if (SelRow != null)
			{

			}
			if (sqldbv == null) return;
			//We have to check which type of bank account we are working with
			if (CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS")
			{
				if (nt != null)
				{
					Console.WriteLine ($"EditDb (609) Index changed to {DataGrid1.SelectedIndex}, CurrentDb = {CurrentDb}");
					nt.EditDbTriggerEvent (DataGrid1.SelectedIndex, DataGrid1);
				}
			}
			else if (CurrentDb == "CUSTOMER")
			{
				if (nt != null)
				{
					Console.WriteLine ($"EditDb (617) Index changed to {DataGrid2.SelectedIndex}, CurrentDb = {CurrentDb}");
					nt.EditDbTriggerEvent (DataGrid2.SelectedIndex, DataGrid2);
				}
			}

#pragma TODO
			//This is where we SHOULD be updating the textboxes ???
		}

		

		private void Intrate_Changed (object sender, TextChangedEventArgs e)
		{
			int x = 0;
		}
	}
}

