//#define SHOWFLAGS
#define SHOWALLFLAGS
#undef SHOWALLFLAGS
#define USEDETAILEDEXCEPTIONHANDLER
#undef USEDETAILEDEXCEPTIONHANDLER

//using System;
using System;
using System . Diagnostics;
using System . Windows . Controls;
using WPFPages . Views;

namespace WPFPages
{

	public static class Flags
	{

		// Viewer Handle and Grid pointers for each Type of grid that is open somewhere
		/// Used as  Pairs so we can access window and grid
		public static DataGrid SqlBankGrid = null;
		public static SqlDbViewer SqlBankViewer = null;

		public static DataGrid SqlCustGrid = null;
		public static SqlDbViewer SqlCustViewer = null;

		public static DataGrid SqlDetGrid = null;
		public static SqlDbViewer SqlDetViewer = null;

		// Current active Grid pointer and Name - Used as a pointer to the currently active DataGrid
		public static DataGrid ActiveSqlGrid = null;

		// current SelectedIndex for each grid type in SqlDbViewers
		//Updated whenever the selection changes in any of the grids
		public static int SqlBankCurrentIndex = 0;
		public static int SqlCustCurrentIndex = 0;
		public static int SqlDetCurrentIndex = 0;

		//EditDb Grid info
		public static EditDb EditBankGrid = null;
		public static EditDb ActiveEditGrid = null;

		// Flag ot control Multi account data loading
		public static bool isMultiMode = false;
		public static string MultiAccountCommandString = "";

#pragma TODO add other Edit Db windows 2/4/21

		public static bool isEditDbCaller = false;
		public static bool SqlDataChanged = false;
		public static bool EditDbDataChanged = false;
		public static EditDb BankEditDb = null;
		public static EditDb CustEditDb = null;
		public static EditDb DetEditDb = null;

		//Flags to hold pointers to current DbSelector & SqlViewer Windows
		// Needed to avoi dInstance issues when calling methods from inside Static methods
		// that are needed to handle the interprocess messaging system I have designed for this thing
		public static DbSelector DbSelectorOpen = null;
		public static EditDb CurrentEditDbViewer = null;
		public static SqlDbViewer CurrentSqlViewer = null;
		public static SqlDbViewer SqlUpdateOriginatorViewer = null;
		public static bool EditDbChangeHandled = false;

		public static bool IsFiltered = false;
		public static string FilterCommand = "";

		//Control CW output of event handlers
		public static bool EventHandlerDebug = false;
		public static bool IsMultiMode = false;

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

		/*
		 *	Sorting Checkbox enumeration
		 *	Default_option . IsChecked = true;		// 0
			Id_option . IsChecked = false;			// 1
			Bankno_option . IsChecked = false;		// 2
			Custno_option . IsChecked = false;		// 3
			actype_option . IsChecked   = false;		// 4
			Dob_option . IsChecked = false;		// 5
			Odate_option . IsChecked = false;		// 6
			Flags . SortOrderRequested = 6;

		*/
		// Set default sort to Custno + Bankno
		public static int SortOrderRequested = 0;
		public enum SortOrderEnum
		{
			DEFAULT = 0,
			ID,
			BANKNO,
			CUSTNO,
			ACTYPE,
			DOB,
			ODATE,
			CDATE
		}

		/// <summary>
		///  handle maintenance of global flags used to control mutliple 
		///  viewers and EditDb windows, called from Focus()
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="CurrentDb"></param>
		public static void SetGridviewControlFlags ( SqlDbViewer instance, DataGrid Grid )
		{
			//Setup global flags -
			Flags . CurrentSqlViewer = instance;
			//only do this if we are not closing a windows (Sends Grid=null)
			if ( Grid != null )
			{
				if ( Grid == Flags . SqlBankGrid )
				{
					Flags . ActiveSqlGrid = Grid;
					Flags . SqlBankGrid = Grid;
					Flags . SqlBankViewer = Flags . CurrentSqlViewer;
					Flags . CurrentSqlViewer = instance;
				}
				else if ( Grid == Flags . SqlCustGrid )
				{
					Flags . ActiveSqlGrid = Grid;
					Flags . SqlCustGrid = Grid;
					Flags . SqlCustViewer = Flags . CurrentSqlViewer;
					Flags . CurrentSqlViewer = instance;
				}
				else if ( Grid == Flags . SqlDetGrid )
				{
					Flags . ActiveSqlGrid = Grid;
					Flags . SqlDetGrid = Grid;
					Flags . SqlDetViewer = Flags . CurrentSqlViewer;
					Flags . CurrentSqlViewer = instance;
				}
			}
			else
			{
				// we need to clear the  details in Gridviewer flag system
				ClearGridviewControlStructure ( instance, Grid );
			}
#if SHOWFLAGS
			ListGridviewControlFlags();
#endif
		}
		public static void ClearGridviewControlStructure ( SqlDbViewer instance, DataGrid Grid )
		{
			//No more viewers open, so clear entire gv[] control structure
			if ( instance == null || Flags . DbSelectorOpen . ViewersList . Items . Count == 1 )
			{
				// Remove ALL Viewers Data - There are no Viewers open apparently !
				for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
				{
					MainWindow . gv . window [ x ] = null;
					MainWindow . gv . CurrentDb [ x ] = "";
					MainWindow . gv . Datagrid [ x ] = null;
					MainWindow . gv . ListBoxId [ x ] = Guid . Empty;
					MainWindow . gv . SelectedViewerType = -1;
					MainWindow . gv . ViewerSelectiontype = -1;
				}
				MainWindow . gv . ViewerCount = 0;
				MainWindow . gv . PrettyDetails = "";
			}
			else
			{
				//Remove a SINGLE Viewer Windows data from Flags & gv[]
				DeleteViewerAndFlags ( );
				Flags . CurrentSqlViewer . UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			}
		}
		//Remove a SINGLE Viewer Windows data from Flags & gv[]
		public static bool DeleteViewerAndFlags ( int index = -1, string currentDb = "" )
		{
			int x = index;
			SqlDbViewer sqlv;                        // x = GridView[] index if received
			if ( Flags . CurrentSqlViewer == null )
				return false;
			Guid tag = ( Guid ) Flags . CurrentSqlViewer?.Tag;
			ListBoxItem lbi = new ListBoxItem ( );
			if ( x == -1 )
			{ // Delete all
				for ( int z = 0 ; z < MainWindow . gv . MaxViewers ; z++ )
				{
					DbSelector . UpdateControlFlags ( null, null, MainWindow . gv . PrettyDetails );
					MainWindow . gv . CurrentDb [ z ] = "";
					MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
					MainWindow . gv . Datagrid [ z ] = null;
					MainWindow . gv . window [ z ] = null;
				}
				MainWindow . gv . ViewerCount = 0;
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlBankViewer = null;
				MainWindow . gv . SqlCustViewer = null;
				MainWindow . gv . SqlDetViewer = null;
				MainWindow . gv . SqlViewerGuid = Guid . Empty;
				MainWindow . gv . SqlViewerWindow = null;

				MainWindow . gv . Bankviewer = Guid . Empty;
				MainWindow . gv . Custviewer = Guid . Empty;
				MainWindow . gv . Detviewer = Guid . Empty;

				Flags . ActiveSqlGrid = null;
				Flags . SqlBankViewer = null;
				Flags . SqlCustViewer = null;
				Flags . SqlDetViewer = null;
				Flags . CurrentSqlViewer = null;

				// ALL entries in our GridView structure are now cleared  ** totally **
				return true;
			}
			else
			{
				// we have NOT received the index of the viewer in the list
				// so  get the index for the correct Entry
				if ( x == 99 )
				{
					// got to find it ourselves
					for ( int i = 0 ; i < 3 ; i++ )
					{
						if ( MainWindow . gv . CurrentDb [ i ] == currentDb )
						{ x = i; break; }
					}
				}
				// we have got the index of the viewer in the list
				// so  get the Tag of that selected Entry
				lbi = Flags . DbSelectorOpen . ViewersList . Items [x ] as ListBoxItem;
				Guid lbtag = ( Guid ) lbi . Tag;
				// Get a pointer to the window so we can close it
				sqlv = Flags . CurrentSqlViewer as SqlDbViewer;
				//See if it matches the one we are closing down
				if ( ( Guid ) lbtag == ( Guid ) tag )
				{

					// We know which gv[] entry  we need to clear, so do it and return
					MainWindow . gv . ViewerCount--;
					MainWindow . gv . CurrentDb [ x ] = "";
					MainWindow . gv . ListBoxId [ x ] = Guid . Empty;
					MainWindow . gv . Datagrid [ x ] = null;
					// Actually close thre Viewer window here, before we delete the relevant pointers
					// done late ron anyway
					//					sqlv . Close ( );
					MainWindow . gv . window [ x ] = null;
				}
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlViewerGuid = Guid . Empty;
				//Finally we can remove this entry from ViewersList
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				lbi . Content = "";
				Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( x );
				Flags . DbSelectorOpen . ViewersList . Refresh ( );
				// Set selectedIndex pointer to current position in list
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count <= 1 )
				{       // List is basically empty (No viewers in  the list)
					sqlv . Close ( );
					return true;
				}
				int currentIndex = x;
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count > currentIndex )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex;
					Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex;
				}
				else if ( Flags . DbSelectorOpen . ViewersList . Items . Count == currentIndex )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex - 1;
					Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex - 1;
				}
				sqlv . Close ( );
				return true;
			}

			// Unreachable code ...

			// Now sort out the  global gv[] flags
			for ( int y = 1 ; y < Flags . DbSelectorOpen . ViewersList . Items . Count ; y++ )
			{
				// Get the Tag of eaxch Viewer in the list
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ y ] as ListBoxItem;
				Guid lbtag = ( Guid ) lbi . Tag;
				//See if it matches the one we are closing down
				if ( ( Guid ) lbtag == ( Guid ) tag )
				{
					//Yes, we have got a match, so go ahead and remove its gv[] entries first
					for ( int z = 0 ; z < MainWindow . gv . MaxViewers ; z++ )
					{
						if ( MainWindow . gv . ListBoxId [ z ] == lbtag )
						{
							MainWindow . gv . ViewerCount--;
							MainWindow . gv . CurrentDb [ z ] = "";
							MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
							MainWindow . gv . Datagrid [ z ] = null;
							MainWindow . gv . window [ z ] = null;
							break;
						}

					}
					MainWindow . gv . PrettyDetails = "";
					//Finally we can remove this entry from ViewersList
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ y ] as ListBoxItem;
					lbi . Content = "";
					Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( y );
					// Set selectedIndex pointer to current position in list
					int currentIndex = y - 1;
					if ( y <= 1 )             // List is basically empty (No viewers in  the list)
						return true;
					if ( Flags . DbSelectorOpen . ViewersList . Items . Count > currentIndex )
					{
						Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex;
						Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex;
					}
					else if ( Flags . DbSelectorOpen . ViewersList . Items . Count == currentIndex )
					{
						Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex - 1;
						Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex - 1;
					}
					return true;
				}
			}
			MainWindow . gv . SqlViewerGuid = Guid . Empty;

			return false;
		}

		public static void ListGridviewControlFlags ( int mode = 0 )
		{
			if ( mode == 1 )
			{
				Debug . WriteLine (
				"#################################################################\n" +
				$"FULL INFO\n" +
				"#################################################################\n" +
				$"\nMAJOR FLAGS :\n===========\n" +
				$"CurrentSqlViewer :-		 [{CurrentSqlViewer?.Tag}]\n" +
				$"\nBANKACCOUNT SqlBankCurrentIndex :- [{SqlBankCurrentIndex}]\n" +
				$"CUSTOMER    SqlCustCurrentIndex :- [{SqlCustCurrentIndex }]\n" +
				$"DETAILS     SqlDetCurrentIndex:-   [{SqlDetCurrentIndex }]" );
			}
			Debug . WriteLine ( $"\n" +
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
				$"\nMAJOR FLAGS :\n===========\n" + 
				$"CurrentSqlViewer :- [{CurrentSqlViewer?.Tag}]\n" +
				$"\nBANKACCOUNT SqlBankGrid :- [{SqlBankGrid?.GetHashCode()}]" +
				$"\nCUSTOMER    SqlCustGrid :- [{SqlCustGrid? .GetHashCode ( )}]" +
				$"\nDETAILS     SqlDetGrid  :- [{SqlDetGrid? .GetHashCode ( )}]\n\n" + 
				$"\nBANKACCOUNT SqlBankCurrentIndex :- [{SqlBankCurrentIndex}]" +
				$"\nCUSTOMER    SqlCustCurrentIndex :- [{SqlCustCurrentIndex }]" +
				$"\nDETAILS     SqlDetCurrentIndex:-   [{SqlDetCurrentIndex }]\n" +
#endif
			"-----------------------------------------------------------------\n" +
			$"ActiveSqlGrid  =		[{ActiveSqlGrid?.Name}]\n" +
			$"CurrentSqlViewer :-		[{CurrentSqlViewer?.Tag}]\n" +
			$"CurrentEditDbViewer		[{CurrentEditDbViewer?.Name}]\n" +
			$"\nACTIVE VIEWERS:\n===============\n" +
			$"CurrentSqlViewer :-		 [ {CurrentSqlViewer?.Tag} ]\n" +
			$"ALL VIEWERS:\n===========\n" +
			$"BANK     SqlBankViewer:- [ {SqlBankViewer?.Tag} ]\n" +
			$"CUST     SqlCustViewer:- [ {SqlCustViewer?.Tag} ]\n" +
			$"DETS     SqlDetViewer:-  [ {SqlDetViewer?.Tag} ]\n" +
			"=================================================================\n"
			);
		}
		public static void PrintSundryVariables ( )
		{
			Console . WriteLine ( "" );
			if ( Flags . SqlBankGrid != null )
				Console . WriteLine ( $"Viewer : BankGrid : ItemsSource : { Flags . SqlBankGrid . ItemsSource}" );
			if ( Flags . SqlCustGrid != null )
				Console . WriteLine ( $"Viewer : CustomerGrid : ItemsSource : { Flags . SqlCustGrid . ItemsSource}" );
			if ( Flags.SqlDetGrid != null )
				Console . WriteLine ( $"Viewer : Details Grid : ItemsSource : { Flags . SqlDetGrid . ItemsSource}" );
		}
	}
}

