// if set, Datatable is cleared and reloaded, otherwise it is not reloaded
//#define PERSISTENTDATA
#define USETASK
#undef USETASK
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using WPFPages.Commands;
using System.Timers;
using System.Windows.Data;
using System.Collections;
using WPFPages.Views;
using System.Windows;
using System.Collections.Specialized;

/// <summary>
///  this is a mirror image of the original BankAccount.cs file
/// </summary>
namespace WPFPages.ViewModels
{
	public partial class BankAccountViewModel: INotifyPropertyChanged
	{
		//==================================
		//Delegate & Event handler for Db Updates
		//==================================
		/// <summary>
		///  A(Globally visible) Delegateto hold all the global flags and other stuff that is needed to handle 
		///  Static -> non static  movements with EditDb &b SqlDbViewer in particular
		/// </summary>
		//public delegate void DbUpdated (object sender);
		//public event DbUpdated NotifyOfDataChange;
		// CONSTRUCTOR
		public BankAccountViewModel ()
		{
			BankAccountObs.CollectionChanged += BankAccountObs_CollectionChanged1;
		}

		private void BankAccountObs_CollectionChanged1 (object sender, NotifyCollectionChangedEventArgs e)
		{
			Type t = sender.GetType ();
			if (!t.FullName.Contains ("ViewModels.BankAccountViewModel"))
				Console.WriteLine ($"BankAccountViewModel has received a notofication that collection \"{t.FullName}\"has changed..... YEAH");
		}

		public void BankAccountObsChanged (object o, NotifyCollectionChangedEventArgs e)
		{
			int y = 0;
		}
		public void SubscribeToChangeEvents ()
		{
			// subscribe to Data chngned event fired by SqlDbviewer
			SqlDbViewer sqlv = new SqlDbViewer ('A');
			// assign event handler function
			sqlv.NotifyOfDataChange += DbHasChanged;
			CollectionChanged += test;
			BankAccountObs.CollectionChanged += BankAccountObsChanged;
			EventHandlers.ShowSubscribersCount();
		}
		public void test (object o, EventArgs e)
		{
			int y = 0;
		}
		/// <summary>
		/// Callback handler for db change notifications snet by SqlDbViewer
		/// </summary>
		/// <param name="sender"></param>
		public void DbHasChanged (object sender, DataChangeArgs args)
		{
			DataGrid cvm = sender as DataGrid;
			if (cvm.Name != "BankGrid")
				Console.WriteLine ($"\n^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\nBankAccount View Model has received DbHasChanged notification\ndue toUpdate of \"{args.DbName}\" Db");
			if (args.DbName != "BANKACCOUNT")
			{
				// need to update our Collection
				DataGrid d = sender as DataGrid;
				{
					d.Refresh ();
					try
					{
						if (Flags.SqlBankGrid == null) return;

						if (args.DbName != "BANKACCOUNT")
						{
							int curr = Flags.SqlBankGrid.SelectedIndex;
							if (curr >= 0)
							{
								LoadBankTask ();
								Flags.SqlBankGrid.SelectedIndex = curr;
								Flags.SqlBankGrid.Refresh ();
							}
							Console.WriteLine ($"\nDB REFRESH performed in BankAccountViewModel");
						}
					}
					catch(Exception ex)
					{
						Console.WriteLine ($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\nDB REFRESH UPDATE ERROR in BankAccountViewModel\n{ex.Message} : {ex.Data}");
					}
				}
			}
		}

		// MVVM TO DO STUFF/INFO		
		#region MVVMstuff

		// How to configure a RelayCommand with lambda expressions:

		RelayCommand _saveCommand; public ICommand SaveCommand
		{
			get
			{
				if (_saveCommand == null)
				{
#pragma MVVM TODO
					//_saveCommand = new RelayCommand (param => this.Save (),
					//    param => this.CanSave);
				}
				return _saveCommand;
			}
		}

		//RelayCommand RequestClose
		//{

		//}
		#endregion MVVMstuff

		#region setup
		public event PropertyChangedEventHandler PropertyChanged;

		//public flags
		public static bool EditDbEventInProcess = false;
		//if true - shows subsribers to events in Output
		public static bool ShowSubscribeData = true;

		public static EditDb EditdbWndBank = null;
		public static EditDb EditdbWndBankCust = null;
		public static EditDb EditdbWndBankDet = null;

		//**********************
		// dbEdit db viewer GLOBALS
		//**********************
		public static List<DataGrid> CurrentEditDbViewerBankGridList = new List<DataGrid> ();
		public static List<DataGrid> CurrentEditDbViewerCustomerGridList = new List<DataGrid> ();
		public static List<DataGrid> CurrentEditDbViewerDetailsGridList = new List<DataGrid> ();
		public static DataGrid ActiveEditDbViewer = null;

		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;
		#endregion setup


		//		public IList SeletedItems { get; set; }

		#region INotifyPropertyChanged Members

		#region Data update/Events/Delegates Support  functions
		public static void ClearFromEditDbList (DataGrid grid, string caller)
		{
			//if (caller == "BANKACCOUNT")
			//{
			//	for (var item = 0; item < CurrentEditDbViewerBankGridList.Count; item++)
			//	{ if (CurrentEditDbViewerBankGridList[item] == grid) { CurrentEditDbViewerBankGridList.RemoveAt (item); Flags.CurrentEditDbViewerBankGrid = null; break; } }
			//}
			//else if (caller == "CUSTOMER")
			//{
			//	for (var item = 0; item < CurrentEditDbViewerCustomerGridList.Count; item++)
			//	{ if (CurrentEditDbViewerCustomerGridList[item] == grid) { CurrentEditDbViewerCustomerGridList.RemoveAt (item); Flags.CurrentEditDbViewerCustomerGrid = null; break; } }
			//}
			//else if (caller == "DETAILS")
			//{
			//	for (var item = 0; item < CurrentEditDbViewerDetailsGridList.Count; item++)
			//	{ if (CurrentEditDbViewerDetailsGridList[item] == grid) { CurrentEditDbViewerDetailsGridList.RemoveAt (item); Flags.CurrentEditDbViewerDetailsGrid = null; break; } }
			//}
		}
		public static void ClearFromSqlList (DataGrid grid, string caller)
		//Remove the datagrid from our List<Datagrid>
		{
			//if (caller == "BANKACCOUNT")
			//{
			//	for (var item = 0; item < Flags.CurrentEditDbViewerBankGridList.Count; item++)
			//	{ if (Flags.CurrentEditDbViewerBankGridList[item] == grid) { Flags.CurrentEditDbViewerBankGridList.RemoveAt (item); break; } }
			//}
			//else if (caller == "CUSTOMER")
			//{
			//	for (var item = 0; item < Flags.CurrentEditDbViewerCustomerGridList.Count; item++)
			//	{ if (Flags.CurrentEditDbViewerCustomerGridList[item] == grid) { Flags.CurrentEditDbViewerCustomerGridList.RemoveAt (item); break; } }
			//}
			//else if (caller == "DETAILS")
			//{
			//	for (var item = 0; item < Flags.CurrentEditDbViewerDetailsGridList.Count; item++)
			//	{ if (Flags.CurrentEditDbViewerCustomerGridList[item] == grid) { Flags.CurrentEditDbViewerCustomerGridList.RemoveAt (item); break; } }
			//}
		}
		#endregion Data update Support  functions

		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}
		#endregion

		#region data loading stuff

		//**************************************************************************************************************************************************************//
		public async Task LoadBankTask ()
		{
			//Create the one and only dtBank instance if not already there
			if (dtBank == null)
				dtBank = new DataTable ();
			else
				dtBank.Clear ();
			try
			{
				if (BankAccountObs != null && BankAccountObs.Count > 0)
					BankAccountObs.Clear ();
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"BankAccountObs Exception [{ex.Data}\r\n");
			}
			Console.WriteLine ($"Starting AWAITED task to load Bank Data via Sql");
			DateTime date = DateTime.Now;

			// THIS ALL WORKS PERFECTLY - THANKS TO VIDEO BY JEREMY CLARKE OF JEREMYBYTES YOUTUBE CHANNEL
#if USETASK
			{
				int? taskid = Task.CurrentId;
				DateTime start = DateTime.Now;
				Task<bool> DataLoader = FillBankAccountDataGrid ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadBankAccountIntoList (dtBank);
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load BankAccount  Data via Sql\n" +
					$"task =Id is [ {taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start)} Ticks\n");
			}
#else
			{
				await FillBankAccountDataGrid ();
				await LoadBankAccountObsCollection ();
				Task.WaitAll ();
			}
#endif
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		//**************************************************************************************************************************************************************//
		///<summary>
		/// fill DataTable with data from SQL BankAccount database
		/// </summary>
		public  async Task<bool>  FillBankAccountDataGrid ()
		{
			//clear the datatable first as we are only showing a subset
			if (dtBank.Rows.Count > 0)
				return  false;
			if (dtBank.Rows.Count > 0)
				dtBank.Clear ();
			dtBank = LoadSqlData (dtBank);
			return true;
			// dtBank should be fully loaded here
		}
		//**************************************************************************************************************************************************************//
		public async Task<bool> LoadBankAccountIntoList (DataTable dtBank)
		{
			if (BankAccountObs.Count > 0)
				return false;
			//This DOES access the Bank/Account Class properties !!!!!
			for (int i = 0; i < dtBank.Rows.Count; ++i)
			{
				BankAccountObs.Add (new BankAccountViewModel
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
			Console.WriteLine ($"Loaded Sql data into BankAccountObs directly....");

			return true;
		
		}
		public async Task OldLoadBankAccountIntoList (List<BankAccountViewModel> Banklist, DataTable dtBank)
		{
		//	if (Banklist.Count > 0)
				return ;
		//	//This DOES access the Bank/Account Class properties !!!!!
		//	for (int i = 0; i < dtBank.Rows.Count; ++i)
		//	{
		//		Banklist.Add (new BankAccountViewModel
		//		{
		//			Id = Convert.ToInt32 (dtBank.Rows[i][0]),
		//			BankNo = dtBank.Rows[i][1].ToString (),
		//			CustNo = dtBank.Rows[i][2].ToString (),
		//			AcType = Convert.ToInt32 (dtBank.Rows[i][3]),
		//			Balance = Convert.ToDecimal (dtBank.Rows[i][4]),
		//			IntRate = Convert.ToDecimal (dtBank.Rows[i][5]),
		//			ODate = Convert.ToDateTime (dtBank.Rows[i][6]),
		//			CDate = Convert.ToDateTime (dtBank.Rows[i][7]),
		//		});
		//	}
		//	//			return true;
		}

		//**************************************************************************************************************************************************************//
		public async Task<bool>  LoadBankAccountObsCollection ()
		{
			if (this.BankAccountObs != null && this.BankAccountObs.Count > 0)
			{
				try // Clear the collection out)
				{
					this.BankAccountObs.Clear ();
					BankAccountViewModel.BankList.Clear ();
				}
				catch (Exception ex)
				{
					Console.WriteLine ($"Failed to load clear Bank Details Obslist - {ex.Message}");
				}
			}
			//This DOES access the Bank/Account Class properties !!!!!
			for (int i = 0; i < dtBank.Rows.Count; ++i)
			{
				if (this.BankAccountObs != null)
				{
					try
					{
						this.BankAccountObs.Add (new BankAccountViewModel
						{
							Id = Convert.ToInt32 (BankAccountViewModel.dtBank.Rows[i][0]),
							BankNo = BankAccountViewModel.dtBank.Rows[i][1].ToString (),
							CustNo = BankAccountViewModel.dtBank.Rows[i][2].ToString (),
							AcType = Convert.ToInt32 (BankAccountViewModel.dtBank.Rows[i][3]),
							Balance = Convert.ToDecimal (BankAccountViewModel.dtBank.Rows[i][4]),
							IntRate = Convert.ToDecimal (BankAccountViewModel.dtBank.Rows[i][5]),
							ODate = Convert.ToDateTime (BankAccountViewModel.dtBank.Rows[i][6]),
							CDate = Convert.ToDateTime (BankAccountViewModel.dtBank.Rows[i][7]),
						});
					}
					catch (Exception e)
					{
						Console.WriteLine ($"Bank Obs load failed - {e.Data}, - {e.Message}");
					}
					//					return true;
				}
				//				else return false;
			}
			Console.WriteLine ($"Loaded Sql data into BankAccountObs directly....");
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs 

			//			if (Flags.ActiveSqlGrid != null)
			//				Flags.ActiveSqlGrid.ItemsSource = CollectionViewSource.GetDefaultView (bvm.BankAccountObs );
			return true;
		}

		#endregion data loading stuff
	}
}

