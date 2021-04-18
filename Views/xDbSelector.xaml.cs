using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using WPFPages.ViewModels;

using static WPFPages.SqlDbViewer;

namespace WPFPages.Views
{
	/// <summary>
	/// Interaction logic for DbSelector.xaml
	/// </summary>
	public partial class DbSelector: Window, INotifyPropertyChanged
	{
		//public event EventHandler ViewerIsLoaded;

		public int selection = 0;
		private int CurrentList = -1;
		private Window thiswin;
		public SqlDbViewer NewSqlViewer = null;
		public bool alldone = false;
		public bool HoldLoad = true;


		// Event handler  to let DbSelector know once window is full y loaded
		// and ready to load DataGrid SQL data 
		public delegate void IsViewerLoaded (SqlDbViewer   sqlv);
		public event IsViewerLoaded ViewerIsLoaded;

		//Constructor
		//********************************************************************************************//
		public DbSelector ()
		{
			InitializeComponent ();
			MainWindow.dbs = this;
			if (ViewersList.Items.Count > 2)
			{// ignore the dummy blank entry line
				ViewersList.SelectedIndex = 0;
				ViewersList.SelectedItem = 0;
			}
			sqlSelector.SelectedIndex = 0;
			sqlSelector.Focus ();
			thiswin = this;
			// set a pointer to this window in GridViewer control struct
			MainWindow.gv.DbSelectorWindow = this;
			
			//Subscribe to Event returned by SqlDbviewer when screen should be loaded
			// It actually works, I get the notification correctly. 8/4/21
			ViewerIsLoaded+= OnViewerLoaded;
		}

		public  void OnViewerLoaded (SqlDbViewer   sqlv)
		{
			//Window is fully loaded, so we can go ahead and load the datagrids etc.
			OnWindowLoaded (null, null);
			this.Show ();
			sqlv.BringIntoView ();

			// We should start by loading The data - ending up  with it in  the OC ready for use.
			CustomerViewModel.LoadCustomersTask();
			CustomerViewModel.LoadCustomersIntoList ();
			CustomerViewModel.LoadCustomerObsCollection ();
//				CustomerViewModel.
			//finally  we should assign the data to the grid (x.ItemsSource=OC)
			sqlv.ShowCust_Click (null, null);
		}
		//*****************************************************************************************//

		// Variable to hold string content for ListBox items in ViewerList of DbSelector.
		private string _listBoxItemText;
		public string ListBoxItemText
		{
			get { return _listBoxItemText; }
			set
			{
				_listBoxItemText = value;
				OnPropertyChanged (ListBoxItemText.ToString ());
			}

		}

		private void OnWindowLoaded (object sender, RoutedEventArgs e)
		{
			int counter = 0;
			this.MouseDown += delegate { DoDragMove (); };
			//Try to populate our list of existing Viewers
			for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			{
				if (MainWindow.gv.window[x] != null)
				{
					ListBoxItem lbi = new ListBoxItem ();

					Binding binding = new Binding ("ListBoxItemText");
					binding.Source = ListBoxItemText;
					lbi.SetBinding (ContentProperty, binding);
					lbi.Content = MainWindow.gv.PrettyDetails;

					ViewersList.SelectedIndex = ViewersList.Items.Add (lbi);

					//Inital values going into our listbox item (entry)!!
					lbi.Tag = MainWindow.gv.ListBoxId[x];
					counter++;
					ViewerDelete.IsEnabled = true;
					SelectViewerBtn.IsEnabled = true;
				}
			}
			//Set default active item to 1st valid entry
			if (counter == 0)
			{
				ViewerDelete.IsEnabled = false;
				SelectViewerBtn.IsEnabled = false;
			}
			else
			{
				ViewersList.SelectedIndex = 1;
				ViewerDeleteAll.IsEnabled = true;
				ViewerDelete.IsEnabled = true;
				ViewerDeleteAll.IsEnabled = true;
			}

			// select the 1st entry in the lower (New Viewer) list
			sqlSelector.SelectedIndex = 1;
			this.BringIntoView ();
			this.Topmost = true;
			//			if (thiswin != null)
			//			MainWindow.gv.DbSelectorWindow = (DbSelector)thiswin;
		}

		//*****************************************************************************************//
		private void DoDragMove ()
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this.DragMove (); }
			catch { return; }
		}

		//*****************************************************************************************//
		private void Cancel_Click (object sender, RoutedEventArgs e)
		{
			// close this Db Selector window
			this.Visibility = Visibility.Collapsed;
		}

		//*****************************************************************************************//
		private void Selected_Click (object sender, MouseButtonEventArgs e)
		{
			//			MainWindow.gv.SelectedViewerType = sqlSelector.SelectedIndex;
			//			selection = MainWindow.gv.SelectedViewerType;
			//			Close ();
			this.Visibility = Visibility.Collapsed;

		}


		//*****************************************************************************************//
		private void CheckEnter_Click (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Close ();
			}
			else
			{
				e.Handled = false;
				return;
			}
		}


		//*****************************************************************************************//
		private void CheckKeyDown_Click (object sender, KeyEventArgs e)
		{//keydown in lower list
			if (e.Key == Key.Escape)
			{
				Close ();
			}
			else
				e.Handled = false;
		}

		private void sqlselector_Select (object sender, MouseButtonEventArgs e)
		{//Select Btn or dbl click on top list, so get a new window of selected type
			if (sqlSelector.SelectedIndex == -1)
				return;
			HandleSelection (sqlSelector, "NEW");
			//Close ();
		}
		//**************************** LOWER LIST - EXISTING VIEWER *************************************//
		private void SelectViewer_Click (object sender, RoutedEventArgs e)
		{//Select Btn button for lower viewers list
		 //open / bring the window to the front
			HandleSelection (ViewersList, "SELECT");
			//ViewersList_Select (sender, null);
		}
		//********************************************************************************************//
		private void ViewersList_Select (object sender, MouseButtonEventArgs e)
		{// double click on list2 - existing viewer list - pass the selected item data back
		 // and open/bring the window to the front
			if (ViewersList.SelectedIndex == -1)
				return;
			HandleSelection (ViewersList, "SELECT");
		}
		//********************************************************************************************//
		private void DeleteViewer_Click (object sender, RoutedEventArgs e)
		{
			// delete just the selected viewer
			if (ViewersList.SelectedIndex < 1)
				return;

			HandleSelection (ViewersList, "DELETE");
		}

		//********************************************************************************************//
		private void SQLlist_Focused (object sender, RoutedEventArgs e)
		{
			//Set the flag so we know which list is active for key press checking
			CurrentList = 1;
		}

		//********************************************************************************************//
		private void Viewerslist_Focused (object sender, RoutedEventArgs e)
		{
			//Set the flag so we know which list is active for key press checking
			CurrentList = 2;
		}

		//********************************************************************************************//
		private void sqlselectorbtn_Select (object sender, RoutedEventArgs e)
		{   // top list Select button pressed - open a new viewer of selected type
			if (sqlSelector.SelectedIndex == -1)
				return;
			if (MainWindow.gv.ViewerCount == MainWindow.gv.MaxViewers)
			{
				MessageBox.Show ($"Sorry, but the maximum of {MainWindow.gv.MaxViewers} Viewer Windows are already open.\r\nPlease close one or more, or select an existing Viewer...", "Maximum viewer count reached");
				return;
			}

			HandleSelection (sqlSelector, "NEW");
		}

		//********************************************************************************************//
		private void DeleteAllViewers_Click (object sender, RoutedEventArgs e)
		{
			if (ViewersList.Items.Count == 1)
				return;
			HandleSelection (ViewersList, "DELETEALL");
		}



		//*******************************MAIN KEY HANDLER FOR LIST BOXES*************************************//
		private void IsEnterKey (object sender, KeyEventArgs e)
		{
			//PreviewKeyDown - in either list 
			if (e.Key == Key.Enter)
			{
				if (CurrentList == 1)
				{ // Top list - new Viewer type
				  //					sqlselectorbtn_Select (sender, null);
					HandleSelection (sqlSelector, "NEW");
					e.Handled = true;
				}
				else if (CurrentList == 2)
				{ // Lower list (open Viewers)
					if (ViewersList.SelectedIndex == -1)
						return;

					HandleSelection (ViewersList, "SELECT");
				}
			}
			else if (e.Key == Key.NumPad2 || e.Key == Key.Down)
			{
				ListBox lb = sender as ListBox;
				if (lb.SelectedIndex < lb.Items.Count - 1)
					lb.SelectedIndex++;
				return;
			}
			else if (e.Key == Key.NumPad8 || e.Key == Key.Up)
			{
				ListBox lb = sender as ListBox;
				if (lb.SelectedIndex > 0)
					lb.SelectedIndex--;
				return;
			}
			else if (e.Key == Key.Escape)
			{
				Close ();
				return;
			}
		}

		//********************************************************************************************//
		private async void HandleSelection (ListBox listbox, string Command)
		{
			// This is the FIRST Fn Called when Opening/ Closing/deleting a Db Viewer window
			//and most other functionality
			int selected = -1;
			string selectedItem = "";
			if (listbox == sqlSelector)
			{
				//upper listbox - NEW command
				if (Command == "NEW")
				{
					selectedItem = listbox.SelectedItem.ToString ();
					if (selectedItem.ToUpper ().Contains ("MULTI BANK"))
						selected = 2;
					else if (selectedItem.ToUpper ().Contains ("BANK"))
						selected = 0;
					else if (selectedItem.ToUpper ().Contains ("CUSTOMER"))
						selected = 1;
					Mouse.OverrideCursor = Cursors.Wait;

					////*****************************************************//
					//// Load and display a new viewer for the selected Db Type
					//// (returned in the selected var from dbSelector window)
					////*****************************************************//
					//SqlDbViewer NewSqlViewer = new SqlDbViewer (selected);

					if (ViewersList.Items.Count > 10)
					{
						Mouse.OverrideCursor = Cursors.Arrow;
						MessageBox.Show ("Sorry, but there is a limit of TEN Viewers open at any one time.\r\nPlease close one or more Viewers if you want to open a new one.", "Maximum Viewers Open !");
						return;
					}
					// find first blank entry of the 10 available slots we have
					// and save our details into it
					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
					{
						if (MainWindow.gv.window[x] == null)
						{
							string currentRowText = "";
							MainWindow.gv.ViewerSelectiontype = -1;  // reset flag field for next time
							Stopwatch sw = new Stopwatch ();
							SqlDbViewer NewSqlViewer = new SqlDbViewer (selected);

							//*****************************************************//
							// THIS WORKS PRETTY WELL, THE SQLdBVIEWER WINDOW IS DISPLAYED VIRTUALLY IMMEDIATELY
							// Load and display a new viewer for the selected Db Type
							// (returned in the selected var from dbSelector window)
							//*****************************************************//
							//							SqlDbViewer NewSqlViewer = new SqlDbViewer (selected);
							// call  our Async AWAIT function to spawn the SqlDbViewer
							Console.WriteLine ("1 - Calling StartViewer Task");
							sw.Start ();
							{
								// // Atempt 4
								// // works, but get white window until it has loaded.....
								bool b = false;
								string str = "";
								Task[ ] tasks = new Task[1];
								tasks[0] = Task.Factory.StartNew (() => StartViewerAsync (NewSqlViewer, selected));
								// Wait for the background task to finish
								tasks[0].Wait ();
//								StartViewerAsync (NewSqlViewer, selected);
								{
									//								taskAsync.Start ();
									//								Task<string> task =  StartViewer (NewSqlViewer, selected);
									//task.ContinueWith
									//	(delegate
									//	{
									//		sc.Post (delegate {str = task.Result; }, null);
									//	});
									//										s => runloader = task.Result, TaskScheduler.FromCurrentSynchronizationContext ()
									Console.WriteLine ($"string str = {str}");
								}
								sw.Stop ();

								//By returning here, the window paints fully !!!!
								
								return;

								//***************************************//

								{
									//Thread thread = new Thread (() =>
									//while (true)
									//{
									//	Thread.Sleep (100);
									//	if (alldone)
									//		break;
									//}
								}
							}
							Console.WriteLine ($"2 - ***FINAL*** Task returned -  took {sw.ElapsedMilliseconds} Milliseconds");
							sw.Start ();

							{
								//	// Atempt 3							
								//	// works, but get white window until it has loaded.....
								//	bool result = await Task.Run (() => StartViewer (selected).ContinueWith (task => CompleteLoad ()));
								//	Console.WriteLine ($"Task result = {result}");
								//	// Task above returns immediately
								//	//								loadViewer.Wait ();
								//	//await Task.WhenAll (loadViewer);
							}
							sw.Stop ();
							Console.WriteLine ($"3 - Overall Load took {sw.ElapsedMilliseconds} Milliseconds");

							{
								//// Atempt 2							
								//// works, but get white window until it has loaded.....
								//Task<bool> loadViewer = StartViewer (selected);
								//// Task above returns immediately
								//loadViewer.Wait ();
								////await Task.WhenAll (loadViewer);
							}
							{       // Attempt 1
								// Atempt 1
								// works, but still get white window until it has loaded.....
								//	var loadViewer = StartViewer (selected);
								//	await Task.WhenAll (loadViewer);
							}
							//*****************************************************************************//

							Console.WriteLine ("4 - StartViewer Task - ALL COMPLETED");
							//Thread t = new Thread (2000);
							{
								//*****************************************************************************//
								//This actually loads the SQLDbViewer windows and populates it
								// and it is fully displayed on screen, but with no data in the grid
								//							NewSqlViewer.Show ();
								//*****************************************************************************//
								Mouse.OverrideCursor = Cursors.Wait;
								//NewSqlViewer.DetailsGrid.Visibility = Visibility.Hidden;
								//NewSqlViewer.WaitMessage.Visibility = Visibility.Visible;
								//NewSqlViewer.WaitMessage.BringIntoView ();
							}

							//while (true) {
							//	if (SqlDbViewer.IsViewerLoaded)
							//		break;
							//	else
							//	{
							//		Thread.Sleep (200);
							//	}
							//}




							if (!HoldLoad)
							{
								// NOW WE CAN TRIGGER THE LOADING OF ACTUAL DATA IN VIEWER ?
								Console.WriteLine ("Calling NewSqlViewer.HandleWindowLoaded ");
								if (selected == 0)
									NewSqlViewer.HandleWindowLoaded ("BANKACCOUNT");
								else if (selected == 1)
									NewSqlViewer.HandleWindowLoaded ("CUSTOMER");
								else if (selected == 2)
									NewSqlViewer.HandleWindowLoaded ("DETAILS");
								Console.WriteLine ("returned from NewSqlViewer.HandleWindowLoaded ");
#pragma LOADING SPEED
								//if (selected == 0) 
								//	BankAccountViewModel.LoadBankTask ();
								//else if (selected == 1) { //This  fully loads the Customer Data into dtCust ONLY
								//	CustomerViewModel.LoadCustomersTask ();
								//}
								//// Calls a function to load data into List<>
								//else if (selected == 2) 
								//	DetailsViewModel.LoadDetailsTask ();

								//Now we can Get the data from the current row
								currentRowText = MainWindow.gv.CurrentDb[x];
								{

									// This DOES update our Property ListBoxItemText
									//which we "should" be able to use to update the Selector list entries text
									// but it doesn't work - so far at least
									//DbSelector dbs = new DbSelector ();

									//Create/Add new viewer entry (ListBoxItem) to Selection viewer Listbox
									ListBoxItem lbi = new ListBoxItem ();

									Binding binding = new Binding ("Content");
									binding.Source = MainWindow.DbSelectorOpen.ListBoxItemText;

									// Set Tag of this LB Item to the DbViewer Window
									lbi.Tag = NewSqlViewer.Tag;
									//Bind the new lbItem to our Data source
									lbi.SetBinding (ContentProperty, binding);

									//update our DependencyProperty ListBoxItemText - in DbSelector.cs
									MainWindow.DbSelectorOpen.ListBoxItemText = currentRowText;
									//This is the normal way to update the lists data
									lbi.Content = MainWindow.DbSelectorOpen.ListBoxItemText;
									lbi.Content = MainWindow.gv.PrettyDetails;

									// update BankAccountViewModel data as required

									int indx = ViewersList.Items.Add (lbi);
									ViewersList.SelectedIndex = indx;

									NewSqlViewer.Focus ();
									ViewersList.Items.Refresh ();
									if (ViewersList.Items.Count > 1)
									{
										ViewerDeleteAll.IsEnabled = true;
										ViewerDelete.IsEnabled = true;
									}

									//*************************************
									// DetailsGrid is now fully populated 2/4/21
									//*************************************
									NewSqlViewer.WaitMessage.Visibility = Visibility.Collapsed;
									if (selected == 0)
										NewSqlViewer.BankGrid.Visibility = Visibility.Visible;
									else if (selected == 1)
									{
										NewSqlViewer.CustomerGrid.Visibility = Visibility.Visible;
									}
									else if (selected == 2)
										NewSqlViewer.DetailsGrid.Visibility = Visibility.Visible;

									if (BankAccountViewModel.BankAccountObs != null)
										Flags.ActiveSqlGrid.ItemsSource = CollectionViewSource.GetDefaultView (BankAccountViewModel.BankAccountObs);


									// This WORKS for details 2/4/21
									Debug.WriteLine ($" *** Current Active...3 =  {Flags.ActiveSqlGridStr}\r\n");
									if (Flags.ActiveSqlGrid?.ItemsSource != null)
										CollectionViewSource.GetDefaultView (Flags.ActiveSqlGrid.ItemsSource).Refresh ();

									Mouse.OverrideCursor = Cursors.Arrow;
									return;
								}
							}
						}
					}
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
					NewSqlViewer.HandleWindowLoaded ("CUSTOMER");
				}
			}
			else if (listbox == ViewersList)
			{
				if (Command == "SELECT")
				{
					//	
					// This works = 22 March 2021
					// 
					int selindex = -1;
					selindex = ViewersList.SelectedIndex;
					ListBoxItem lbi = new ListBoxItem ();
					lbi = ViewersList.Items[selindex] as ListBoxItem;

					int SelectedId = MainWindow.gv.ListBoxId[selindex - 1];
					Window w = MainWindow.gv.window[selindex - 1];
					w.Show ();
					w.Topmost = true;
					w.Focus ();
					w.Topmost = false;
					return;
				}
				else if (Command == "DELETE")
				{       //	
					// This also works = 22 March 2021
					// 
					int selindex = -1;
					selindex = ViewersList.SelectedIndex;
					ListBoxItem lbi = new ListBoxItem ();
					lbi = ViewersList.Items[selindex] as ListBoxItem;

					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
					{
						try
						{
							if (ViewersList.SelectedItem != null)
							{
								if (MainWindow.gv.ListBoxId[x] == (int)lbi.Tag)
								{
									// found the match - so delete viewer and remove from Viewers list
									CloseviewerWindow (x);
									//									ViewersList.Items.RemoveAt (selindex);
									break;
								}
							}
						}
						catch
						{

						}
					}
					if (ViewersList.Items.Count >= 1)
						ViewersList.SelectedIndex = selindex;
					ViewersList.Items.Refresh ();
					return;
				}
				else if (Command == "DELETEALL")
				{
					//	
					// This also works = 22 March 2021
					// 
					// Close/Delete ALL open viewers
					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
					{
						//Close the viewer
						if (MainWindow.gv.window[x] != null)
							MainWindow.gv.window[x].Close ();
						//remove all record of it's very existence
						MainWindow.gv.window[x] = null;
						MainWindow.gv.CurrentDb[x] = "";
						MainWindow.gv.ListBoxId[x] = -1;

						MainWindow.gv.ViewerSelectiontype = -1;  // reset flag field for next time
					}
					MainWindow.gv.ViewerCount = 0;
					for (int x = MainWindow.gv.MaxViewers; x > 0; x--)
					{
						if (ViewersList.Items.Count == x + 1)
						{
							ViewersList.Items.RemoveAt (x);
							//							break;
						}
					}
				}
			}
		}
		//********************************************************************************************//
		private async Task StartViewerAsync (SqlDbViewer sql, int selected)
		{
			{
				Stopwatch sw = new Stopwatch ();
				sw.Start ();
				//Also works, but still got white window
				Console.WriteLine ("5 - Calling runloader() ");
				//				sql.Show ();
				//				await Task.Delay (1000);
				runloader (sql);

				while (SqlDbViewer.IsviewerLoaded == false)
				{
					await Task.Delay (1000);
					int x = 0;
				}
				Console.WriteLine ("6 - runloader() call completed");
				sw.Stop ();
				Console.WriteLine ($"7 - StartViewer took {sw.ElapsedMilliseconds} Milliseconds\r\n");
				alldone = true;
				//				return true;
			}
			//{
		}
		private async Task<string> xStartViewer (SqlDbViewer sql, int selected)
		{
			{
				Stopwatch sw = new Stopwatch ();
				sw.Start ();
				//Also works, but still got white window
				Console.WriteLine ("5 - Calling runloader() ");
				//				sql.Show ();
				runloader (sql);
				//Task.Run (() => runloader (sql));
				Console.WriteLine ("6 - runloader() call completed");
				sw.Stop ();
				Console.WriteLine ($"7 - StartViewer took {sw.ElapsedMilliseconds} Milliseconds\r\n");
				return "All done in StartViewer";
			}
			//{
			//	Console.WriteLine ("Calling runloader() ");
			//	await runloader (NewSqlViewer);
			//	Console.WriteLine ("runloader() call completed");
			//}
			//return NewSqlViewer;
		}
		private void runloader (SqlDbViewer NewSqlViewer)
		{
			//Thuis actually shows the window fully ?
			// and is why we are waiting for it to complete
			//before starting sql loading
			Stopwatch sw = new Stopwatch ();
			sw.Start ();
			Console.WriteLine ("8 - runLoader - Showing Sql Window");
			NewSqlViewer.Show ();
			//Task.WaitAll ();
			Console.WriteLine ("9 - runLoader - Completed");
			sw.Stop ();
			Console.WriteLine ($"10 - runloader took {sw.ElapsedMilliseconds} Milliseconds ");
			//return true;
		}
		//public void RunTaskSyncrhonously (SqlDbViewer sql, int selected)
		//{
		//	var tasks = new List<Task>
		//    {
		//		new Task(StartViewerAsync ,
		//		    System.Threading.Tasks.TaskCreationOptions.LongRunning),
		//		new Task(runloader (),
		//		    System.Threading.Tasks.TaskCreationOptions.LongRunning)
		//    };

		//	foreach (Task task in tasks)
		//	{
		//		task.RunSynchronously (TaskScheduler.Default);
		//	}
		//}
		private bool CompleteLoad ()
		{
			Console.WriteLine ($"11 - Finished in 'loader' dummy Fn ....");
			return true;
		}
		//********************************************************************************************//

		private void CloseviewerWindow (int index)
		{
			//Close the specified viewer
			if (MainWindow.gv.window != null)
			{
				//Fn removes all record of it's very existence
				MainWindow.gv.window[index].Close ();
			}
		}

		//********************************************************************************************//
		private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
		{
			MainWindow.DbSelectorOpen = null;

		}
		#region PropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}
		#endregion PropertyChanged

		private void MultiViewer_Click (object sender, RoutedEventArgs e)
		{
			MultiViewer mv = new MultiViewer ();

			mv.Show ();
		}


		private void Window_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.RightAlt)
			{
				Flags.ListFlags ();
			}
			else if (e.Key == Key.Home)
				Application.Current.Shutdown ();

		}
	}
}