using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WPFPages;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace WPFPages.Views
{
	/// <summary>
	/// Interaction logic for DbSelector.xaml
	/// </summary>
	public partial class DbSelector: Window, INotifyPropertyChanged
	{
		public int selection = 0;
		private int CurrentList = -1;
		private Window thiswin;

		//Constructor
		//********************************************************************************************//
		public DbSelector ()
		{
			InitializeComponent ();
			MainWindow.dbs = this;
			if (ViewersList.Items.Count > 2)
			{// ignore the dummy blank entry line
				ViewersList.SelectedIndex = 1;
				ViewersList.SelectedItem = 1;
			}
			sqlSelector.SelectedIndex = 0;
			sqlSelector.Focus ();
			thiswin = this;
			// set a pointer to this window in GridViewer control struct
			MainWindow.gv.DbSelectorWindow = this;
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
				OnPropertyChanged (ListBoxItemText.ToString());
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
			sqlSelector.SelectedIndex = 0;
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
		//private void OnClose (object sender, KeyEventArgs e)
		//{
		//	for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
		//	{
		//		if (MainWindow.gv.window[x] == this)
		//		{
		//			// clear entire data structure for this instance of a viewer window
		//			MainWindow.gv.ViewerSelectiontype = -1;
		//			MainWindow.gv.ViewerCount--;
		//			MainWindow.gv.window = null;
		//			MainWindow.gv.CurrentDb[x] = "";
		//			MainWindow.gv.ListBoxId[x] = -1;
		//			MainWindow.gv.Datagrid[x] = null;
		//			MainWindow.gv.SelectedViewerType = -1;
		//			MainWindow.gv.ChosenViewer = null;
		//			MainWindow.gv.DBSelectorWindow = null;
		//			break;
		//		}
		//	}
		//	this.Close ();
		//}

		//*****************************************************************************************//
		private void CheckEnter_Click (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
//				MainWindow.gv.SelectedViewerType = sqlSelector.SelectedIndex;
	//			selection = MainWindow.gv.SelectedViewerType;
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
//				MainWindow.gv.SelectedViewerType = -1;
	//			selection = MainWindow.gv.SelectedViewerType;
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
			//MainWindow.gv.SelectedViewerType = sqlSelector.SelectedIndex;
			////Set selection made type to 1 for a new window
			//MainWindow.gv.ViewerSelectiontype = 1;
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
					//if (sqlSelector.SelectedIndex == -1)
					//	return;
					//MainWindow.gv.SelectedViewerType = sqlSelector.SelectedIndex;
					////Set selection made type to 1 for a new window
					//MainWindow.gv.ViewerSelectiontype = 1;
					//Close ();

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
		private void HandleSelection (ListBox listbox, string Command)
		{
			// Called when Closing/deleting a Db Viewer window
			//and most other functionality
			int selected = -1;
			string selectedItem = "";
			if (listbox == sqlSelector)
			{
				//upper listbox - NEW command
				if (Command == "NEW")
				{
					selectedItem = listbox.SelectedItem.ToString ();
					//				selected = MainWindow.gv.SelectedViewerType;
					if (selectedItem.ToUpper ().Contains ("MULTI BANK"))
						selected = 2;
					else if (selectedItem.ToUpper ().Contains ("BANK"))
						selected = 0;
					else if (selectedItem.ToUpper ().Contains ("CUSTOMER"))
						selected = 1;
					// Load and display a new viewer for the selected Db Type
					// (returned in the selected var from dbSelector window)
					Mouse.OverrideCursor = Cursors.Wait;
					SqlDbViewer tw = new SqlDbViewer (selected);

					if (ViewersList.Items.Count > 10)
					{
						Mouse.OverrideCursor = Cursors.Arrow;
						MessageBox.Show ("Sorry, but there is a limit of TEN Viewers open at any one time.\r\nPlease close one or more Viewers if you want to open a new one.","Maximum Viewers Open !");
						return;
					}
						// find first blank entry of the 5 available slots we have
						// and save our details into it
						for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
					{
						if (MainWindow.gv.window[x] == null)
						{
							string currentRowText = "";
							MainWindow.gv.ViewerSelectiontype = -1;  // reset flag field for next time
#pragma warning TODO			
							// THIS TAKES FOR EVER ON 1ST LOAD
							// probably while connecting to SLQ Server ??

							//We already have the full row PrettyDetails when we reach here on initial load
							tw.Show ();

							//Get the data from the current row
							currentRowText = MainWindow.gv.CurrentDb[x];

							// This DOES update our Property ListBoxItemText
							//which we "should" be able to use to update the Selector list entries text
							// but it doesn't work - so far at least
							//DbSelector dbs = new DbSelector ();

							//Create/Add new viewer entry (ListBoxItem) to Selection viewer Listbox
							ListBoxItem lbi = new ListBoxItem ();

							Binding binding = new Binding ("Content");
							binding.Source = MainWindow.DbSelectorOpen.ListBoxItemText;

							// Set Tag of this LB Item to the DbViewer Window
							lbi.Tag = tw.Tag;
							//Bind the new lbItem to our Data source
							lbi.SetBinding (ContentProperty, binding);

							//update our DependencyProperty ListBoxItemText - in DbSelector.cs
							MainWindow.DbSelectorOpen.ListBoxItemText = currentRowText;
							//This is the normal way to update the lists data
							//							lbi.Content = currentRowText;
							lbi.Content = MainWindow.DbSelectorOpen.ListBoxItemText;
							lbi.Content = MainWindow.gv.PrettyDetails;

							int indx = ViewersList.Items.Add (lbi);
							ViewersList.SelectedIndex = indx;
							tw.Focus ();
							ViewersList.Items.Refresh ();
							if (ViewersList.Items.Count > 1)
							{
								ViewerDeleteAll.IsEnabled = true;
								ViewerDelete.IsEnabled = true;
							}
							Mouse.OverrideCursor = Cursors.Arrow;
							return;
						}
					}
					Mouse.OverrideCursor = Cursors.Arrow;
					return;
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

					int SelectedId = MainWindow.gv.ListBoxId[selindex-1];
					Window w = MainWindow.gv.window[selindex-1];
					w.Show ();
					w.Topmost = true;
					w.Focus ();
					w.Topmost = false;
					return;
				}
				else if (Command == "DELETE")
				{	//	
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
								if (MainWindow.gv.ListBoxId[x] == (int)lbi.Tag )
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
						if (ViewersList.Items.Count == x+1)
						{
							ViewersList.Items.RemoveAt (x);
//							break;
						}
					}
				}				
			}
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
	}
}