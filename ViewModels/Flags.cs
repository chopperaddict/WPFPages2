//#define SHOWFLAGS
#define SHOWALLFLAGS
#define USEDETAILEDEXCEPTIONHANDLER
#undef USEDETAILEDEXCEPTIONHANDLER

//using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

using WPFPages.Views;

namespace WPFPages.ViewModels
{
	
	public static class Flags
	{

		// declare a delegate to be used by the various ObservableCollections to notify
		// each other when one or other makes a change to the data because all the 
		// Db's are inter-related

		public static BankAccountViewModel bvmBankRecord = null;
		public static CustomerViewModel bvmCustRecord = null;
		public static DetailsViewModel bvmDetRecord = null;

		//set to the active Grid, no matter which window has focus ?
		public static DataGrid CurrentActiveGrid = null;

		// Grid pointers
		public static DataGrid SqlBankGrid = null;
		public static DataGrid SqlCustGrid = null;
		public static DataGrid SqlDetGrid = null;

		// Grid pointers Names
		public static string SqlBankGridStr = "";
		public static string SqlCustGridStr = "";
		public static string SqlDetGridStr = "";

		// Current active SQL Viewer pointer and Name
		public static SqlDbViewer ActiveSqlViewer = null;
		public static string ActiveSqlViewerStr = "";

		// Current active Grid pointer and Name
		public static DataGrid ActiveSqlGrid = null;
		public static string ActiveSqlGridStr = "";

		//EditDb Grid info
		public static EditDb EditBankGrid = null;
		public static EditDb ActiveEditGrid = null;

		public static DataGrid ActiveDbGrid = null;
		public static string ActiveDbGridStr = "";

#pragma TODO add other Edit Db windows 2/4/21

		public static string currentDb = "";
		public static bool isEditDbCaller = false;

		//Flags to hold pointers to current DbSelector & SqlViewer Windows
		// Needed to avoi dInstance issues when calling methods from inside Static methods
		// that are needed to handle the interprocess messaging system I have designed for this thing
		public static DbSelector DbSelectorOpen = null;
		public static EditDb CurrentEditDbViewer = null;
		public static SqlDbViewer CurrentSqlViewer = null;

		//pointers for our data classes
//		public static BankAccountViewModel BankModel = null;
//		public static CustomerViewModel CustomerModel = null;
//		public static DetailsViewModel DetailsModel = null;

		//Control CW output of event handlers
		public static bool EventHandlerDebug = false;


		/// <summary>
		///  Holds the DataGrid pointer fort each open SqlDbViewer Window as they
		///  can each have diffrent datasets in use at any one time
		/// </summary>
		public static DataGrid ActiveSqlDbViewer = null;


		/// <summary>
		///  Used to  control the initial load of Viewer windows to avoid 
		///  mutliple additions to DbSelector's viewer  listbox
		/// </summary>
		public static bool SqlViewerIsLoading = false;
		public static bool SqlViewerIsUpdating = false;



		/// <summary>
		///  handle maintenance of global flags used to control mutliple 
		///  viewers and EditDb windows, called from Focus()
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="CurrentDb"></param>
		public static void SetGridviewControlFlags (SqlDbViewer instance, DataGrid Grid)
		{
			//Setup global flags - first clear them all as relevant
			Flags.ActiveSqlViewer = instance;
			Flags.ActiveSqlViewerStr = instance?.Name;
			//only do this if we are not closing a windows (Sends Grid=null)
			if (Grid != null)
			{
				if (Grid == Flags.SqlBankGrid)
				{
					Flags.CurrentActiveGrid = Grid;
					Flags.ActiveSqlGrid = Grid;
					Flags.ActiveSqlGridStr = Grid?.Name;
					Flags.SqlBankGrid = Grid;
					Flags.SqlBankGridStr = Grid?.Name;
					Flags.CurrentSqlViewer = instance;
				}
				else if (Grid == Flags.SqlCustGrid)
				{
					Flags.CurrentActiveGrid = Grid;
					Flags.ActiveSqlGrid = Grid;
					Flags.ActiveSqlGridStr = Grid?.Name;
					Flags.SqlCustGrid = Grid;
					Flags.SqlCustGridStr = Grid?.Name;
					Flags.CurrentSqlViewer = instance;
				}
				else if (Grid == Flags.SqlDetGrid)
				{
					Flags.CurrentActiveGrid = Grid;
					Flags.ActiveSqlGrid = Grid;
					Flags.ActiveSqlGridStr = Grid?.Name;
					Flags.SqlDetGrid = Grid;
					Flags.SqlDetGridStr = Grid?.Name;
					Flags.CurrentSqlViewer = instance;
				}
			}
			else
			{
				// we need to clear the  details in Gridviewer flag system
				ClearGridviewControlStructure (instance, Grid);
			}
#if SHOWFLAGS
			ListGridviewControlFlags();
#endif
		}
		public static void ClearGridviewControlStructure (SqlDbViewer instance, DataGrid Grid)
		{
			//No more viewers open, so clear control structure
			if (instance == null || Flags.DbSelectorOpen.ViewersList.Items.Count == 1)
			{
				// Remove ALL Viewers Data - There are no Viewers open apparently !
				for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
				{
					MainWindow.gv.window[x] = null;
					MainWindow.gv.CurrentDb[x] = "";
					MainWindow.gv.Datagrid[x] = null;
					MainWindow.gv.ListBoxId[x] = Guid.Empty;
					MainWindow.gv.SelectedViewerType = -1;
					MainWindow.gv.ViewerSelectiontype = -1;
				}
				MainWindow.gv.ViewerCount = 0;
				MainWindow.gv.PrettyDetails = "";
			}
			else
			{
				//Remove a SINGLE Viewer Windows data
				SqlDbViewer.DeleteViewerAndFlags ();
				Flags.CurrentSqlViewer.UpdateDbSelectorBtns (Flags.CurrentSqlViewer);

			//	for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
			//	{
			//		if (MainWindow.gv.ListBoxId[x] == (Guid)instance.Tag)
			//		{
			//			//remove all record of it's very existence
			//			MainWindow.gv.window[x].Close ();
			//			MainWindow.gv.window[x] = null; 
			//			MainWindow.gv.Datagrid[x] = null;
			//			MainWindow.gv.CurrentDb[x] = "";
			//			MainWindow.gv.ListBoxId[x] = Guid.Empty;
			//			MainWindow.gv.SelectedViewerType = -1;
			//			MainWindow.gv.ViewerSelectiontype = -1;
			//			MainWindow.gv.ViewerCount--;
			//			break;
			//		}
			//	}
			}
			//Flags.CurrentSqlViewer.UpdateDbSelectorBtns (Flags.CurrentSqlViewer);
		}

		public static void ListGridviewControlFlags ()
		{
			Debug.WriteLine ($"\r\n" +
#if SHOWALLFLAGS
				$"ACTIVE DB                            \"{currentDb}\"\r\n" +
				$"EditDb Flags :" +
				$"EditGrid =                                               {EditBankGrid}\r\n" +
				$"ActiveDbGridStr =                                 {ActiveDbGridStr}\r\n" +
				$" *** Current ActiveDbGrid *** =       {ActiveDbGrid}\r\n" +
				$" *** Current ActiveDbGridStr *** =  {ActiveDbGridStr}\r\n" +
				"----\r\n" +
				$"Sql Db Flags :\r\n" +
				$"ActiveSqlViewer =                     {ActiveSqlViewer}\r\n" +
				$"ActiveSqlViewerStr  =               {ActiveSqlViewerStr}\r\n" +
				"----\r\n" +
				$"SqlBankGridStr =                    {SqlBankGridStr} \r\n" +
				$"SqlCustGridStr =                     {SqlCustGridStr}\r\n" +
				$"SqlDetGridStr =                      {SqlDetGridStr}\r\n" +
				"----\r\n" +
				$" *** Current ActiveSqlGrid *** =     {ActiveSqlGrid}\r\n" +
				$" *** Current ActiveSqlGridStr *** =  {ActiveSqlGridStr}\r\n" +
				$"Bank SelectedItem :-\r\n{bvmBankRecord}" +
				$"Cust SelectedItem :-\r\n{bvmCustRecord}" +
				$"Details  SelectedItem :-\r\n{bvmDetRecord}" +
#endif
				$"\n\nMAJOR FLAGS :\n===========\n" + 
				$"CurrentSqlViewer :- [{CurrentSqlViewer}]" +
				$"\nBANKACCOUNT SqlBankGrid :- [{SqlBankGrid}]" +
				$"\nCUSTOMER    SqlCustGrid :- [{SqlCustGrid}]" +
				$"\nDETAILS     SqlDetGrid  :- [{SqlDetGrid}]\n" 
			);
		}
	}
}

