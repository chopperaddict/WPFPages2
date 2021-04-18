using System;


using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WPFPages.Views;

using WpfUI;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow: INotifyPropertyChanged
	{


		public Frame theFrame;
		public static Page _Blank = new BlankPage ();
		public static Page _Page0 = new Page0 ();
		public static Page _Page1 = new Page1 ();
		public static Page _Page2 = new Page2 ();
		public static Page _Page3 = new Page3 ();
		public static Page _Page4 = new Page4 ();
		public static Page _Page5 = new Page5 ();
		//		public static Page ButtonStackPanel = new ButtonStackPanel();
		public static GridViewer gv = new GridViewer ();
		public static string _baseDataText;
		public static DbSelector DbSelectorOpen = null;
		private string _randomText1 = "button1";
		private string _randomText2 = "button2";
		public static DbSelector dbs = new DbSelector ();
		public bool Autoload = false;
		public bool EventHandlerDebug = true;
		SqlDbViewer tw = null;
		
		public MainWindow ()
		{
			// Set this as the main dataContext
//			DataContext = this;
			InitializeComponent ();
			Loaded += MainWindowLoaded;
			BaseDataText = "Starting text";
			RandomText1 = "button1";
			RandomText2 = "button2";
			gv = GridViewer.Viewer_instance;
		}

		
		private void Loaded_click (object sender, RoutedEventArgs e)
		{
			MainPageHolder.NavigationService.Navigate (MainWindow._Blank);
			this.MouseDown += delegate { DoDragMove (); };
			string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			SqlConnection con;
			try
			{
				con = new SqlConnection (ConString);
			}
			catch (Exception ex)
			{
				Console.WriteLine ($"SQL connection failed ....{ex.Message}");
				MessageBox.Show ($"SQL connection failed ....\r\n{ex.Message}\r\n{ex.Data}");
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
		//Declare a Property variable to prove Slider changes work both ways
		private int _boundNumber;
		public int BoundNumber
		{
			get { return _boundNumber; }
			set
			{
				if (_boundNumber != value)
				{
					_boundNumber = value;
					OnPropertyChanged ();
				}
			}
		}
		public string RandomText1
		{
			get { return _randomText1; }
			set
			{
				if (_randomText1 != value)
				{
					_randomText1 = value;
					OnPropertyChanged ();
				}
			}
		}
		public string RandomText2
		{
			get { return _randomText2; }
			set
			{
				_randomText2 = value;
				OnPropertyChanged ();
			}
		}

		// Now on with other code

		// Setup a generic Notifier so property changes are broadcast automatically
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged ([CallerMemberName] string PropertyName = null)
		{
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (PropertyName));
		}

		//This is how to declare properties for binding
		public string BaseDataText
		{
			get { return (string)GetValue (BaseDataTextProperty); }
			set { SetValue (BaseDataTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BaseDataText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BaseDataTextProperty =
			DependencyProperty.Register ("BaseDataText", typeof (string), typeof (MainWindow), new PropertyMetadata (default));

		public string Button1Text
		{
			get { return (string)GetValue (Button1TextProperty); }
			set { SetValue (Button1TextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BaseDataText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty Button1TextProperty =
			DependencyProperty.Register ("Button1Text", typeof (string), typeof (MainWindow), new PropertyMetadata (default));

		public string Button2Text
		{
			get { return (string)GetValue (Button2TextProperty); }
			set { SetValue (Button2TextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BaseDataText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty Button2TextProperty =
			DependencyProperty.Register ("Button2Text", typeof (string), typeof (MainWindow), new PropertyMetadata (default));

		public void MainWindowLoaded (object sender, RoutedEventArgs e)
		{
			//_frame.NavigationService.Navigate(_Page1);
			BaseDataText = "Hello World";
		}

		private void Button1_Click (object sender, RoutedEventArgs e)
		{

		}

		private void Page0_Click (object sender, RoutedEventArgs e)
		{
			//Button btn = (Button)sender;
			//btn.FontSize = 28;
			MainPageHolder.NavigationService.Navigate (MainWindow._Page0);
		}
		private void Page1_Click (object sender, RoutedEventArgs e)
		{
			//Button btn = (Button)sender;
			//btn.FontSize = 28;
			MainPageHolder.NavigationService.Navigate (MainWindow._Page1);
		}
		private void Page2_Click (object sender, RoutedEventArgs e)
		{
			MainPageHolder.NavigationService.Navigate (MainWindow._Page2);
		}
		private void Page3_Click (object sender, RoutedEventArgs e)
		{
			MainPageHolder.NavigationService.Navigate (MainWindow._Page3);
		}
		private void Page4_Click (object sender, RoutedEventArgs e)
		{
			MainPageHolder.NavigationService.Navigate (MainWindow._Page4);
		}
		private void Page5_Click (object sender, RoutedEventArgs e)
		{
			MainPageHolder.NavigationService.Navigate (MainWindow._Page5);
		}

		private void Page6_Click (object sender, RoutedEventArgs e)
		{
			//	this allows the loading of up to 5 different Db Viewer Windows
			//	and to select between them if needed
			//while (true)
			//{
				int selected = 0;
			if (MainWindow.gv.DbSelectorWindow != null)
			{
				if (MainWindow.gv.DbSelectorWindow.ViewersList.Items.Count > 0)
				{
					// Been opened before, so just show it again
					MainWindow.gv.DbSelectorWindow.Visibility = Visibility.Visible;
					if (MainWindow.DbSelectorOpen.ViewersList.Items.Count > 1)
					{
						MainWindow.DbSelectorOpen.ViewerDeleteAll.IsEnabled = true;
						MainWindow.DbSelectorOpen.ViewerDelete.IsEnabled = true;
					}

					MainWindow.gv.DbSelectorWindow.Focus ();
					MainWindow.gv.DbSelectorWindow.BringIntoView ();
					MainWindow.gv.DbSelectorWindow.Focus ();
				}
				return;
			}
			else
			{
				// create a new Db Viewer Control window
				MainWindow.DbSelectorOpen = new DbSelector ();
				// Load and display a new viewer for the selected Db Type
				// (returned in the selected var from dbSelector window)
				Mouse.OverrideCursor = Cursors.Wait;
				tw = new SqlDbViewer (selected);
				if (Autoload) {

					// find first blank entry of the 5 available slots we have
					// and save our details into it
					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
					{
						if (gv.window[x] == null)
						{
							//						gv.SelectedViewerType = selected; // store the Db type in our "current" viewer type variable
							gv.ViewerSelectiontype = -1;  // reset flag field for next time
							gv.ViewerCount++;

#pragma warning TODO			
							// THIS TAKES FOR EVER ON 1ST LOAD
							tw.Show ();
							tw.Focus ();
							//Save the Window handle in the Viewer Window itself - Now done in window loaded
							//tw.Tag = gv.window[x];
							//break;
							break;
						}
					}
				}
				MainWindow.DbSelectorOpen.Show ();
				Mouse.OverrideCursor = Cursors.Arrow;
				return;
			}
			//Create a new Db Selector Window system.
			//				DbSelector dbs = new DbSelector ();
			//Store window handle to DbSelector window in control structure (GridViewer)
			MainWindow.DbSelectorOpen.Show ();
			//Store the "Handle" to this Db Selector window
//				DbSelectorOpen = dbs;
			if (gv.ViewerSelectiontype == 1)
				{
				//					selected = gv.SelectedViewerType;
				//					// Load and display a new viewer for the selected Db Type
				//					// (returned in the selected var from dbSelector window)
				//					Mouse.OverrideCursor = Cursors.Wait;
				//					tw = new SqlDbViewer (selected);

				//					// find first blank entry of the 5 available slots we have
				//					// and save our details into it
				//					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
				//					{
				//						if (gv.window[x] == null)
				//						{
				//							gv.SelectedViewerType = selected; // store the Db type in our "current" viewer type variable
				//							gv.ViewerSelectiontype = -1;  // reset flag field for next time
				//							gv.ViewerCount++;

				//#pragma warning TODO			
				//							// THIS TAKES FOR EVER ON 1ST LOAD
				//							tw.Show ();
				//							tw.Focus ();
				//							//Save the Window handle in the Viewer Window itself - Now done in window loaded
				//							//tw.Tag = gv.window[x];
				//							//break;
				//							Mouse.OverrideCursor = Cursors.Arrow;
				//							return;
				//						}
				//					}
				//					Mouse.OverrideCursor = Cursors.Arrow;
				//					return;
			}
			//else if (gv.ViewerSelectiontype == 2)
			//{
			//	// find selected entry of the "used"  slots we have
			//	// 
			//	MainWindow.gv.ChosenViewer.Show ();
			//	MainWindow.gv.ChosenViewer.Topmost = true;
			//	MainWindow.gv.ChosenViewer.Focus ();
			//	MainWindow.gv.ChosenViewer.Topmost = false;
			//	return;
			//}
			//				else if (gv.ViewerSelectiontype == 3)
			//				{
			//					// Close/Delete viewer
			//					bool moveremaining = false;
			//					if (gv.ChosenViewer == null)
			//						return;
			//					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			//					{
			//						if (!moveremaining && gv.window[x] == gv.ChosenViewer)
			//						{
			//							if (gv.window[x] != null)
			//							{
			//								//Close the viewer
			//								gv.window[x].Close ();
			//								//remove all record of it's very existence
			//								gv.window[x] = null;
			//								gv.CurrentDb[x] = "";
			//								//							gv.CurrentGrid[x] = null;
			//								gv.SelectedViewerType = -1; // store the Db type in our "current" viewer type variable
			//								gv.ViewerSelectiontype = -1;  // reset flag field for next time
			//								gv.ChosenViewer = null;
			//								gv.DBSelectorWindow = null;
			////								gv.ViewerListIndex[x] = -1;
			//								gv.ViewerCount--;
			//								moveremaining = true;
			//							}
			//							continue;
			//						}
			//						//if (moveremaining)
			//						//{   // decrement remaining items position in list !  Smart eh ?
			//						//	if (MainWindow.gv.[x] != -1)
			//						//		MainWindow.gv.ViewerListIndex[x] -= 1;
			//						//}
			//					}
			//				}
			else if (gv.ViewerSelectiontype == 4)
				{
					// Close/Delete ALL open viewers
					for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
					{
						//Close the viewer
						if(gv.window[x] != null)
							gv.window[x].Close ();
						//remove all record of it's very existence
						gv.window[x] = null;
						gv.CurrentDb[x] = "";
						//							gv.CurrentGrid[x] = null;
//						gv.SelectedViewerType = -1; // store the Db type in our "current" viewer type variable
						gv.ViewerSelectiontype = -1;  // reset flag field for next time
//						gv.ChosenViewer = null;
//						gv.DbSelectorWindow = null;
//						gv.ViewerListIndex[x] = -1;
//						gv.ViewerCount--;
					}               //return;
				gv.ViewerCount = 0;
			}
//			} // end Do while
		}
		private void ExitButton_Click (object sender, RoutedEventArgs e) { App.Current.Shutdown (); }

		private void Blank_Click (object sender, RoutedEventArgs e)
		{
			MainPageHolder.NavigationService.Navigate (MainWindow._Blank);
		}
	}
}
