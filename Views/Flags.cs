//#define SHOWFLAGS
#define SHOWALLFLAGS
#undef SHOWALLFLAGS
#define USEDETAILEDEXCEPTIONHANDLER
#undef USEDETAILEDEXCEPTIONHANDLER

//using System;
using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Data;

using DocumentFormat . OpenXml . ExtendedProperties;

using WPFPages . Views;

namespace WPFPages
{

	public static class Flags
	{

		// declare a delegate to be used by the various ObservableCollections to notify
		// each other when one or other makes a change to the data because all the 
		// Db's are inter-related

		//		public static DataGrid CurrentActiveGrid = null;

		// Viewer Handle and Grid pointers for each Type of grid that is open somewhere
		/// Used as  Pairs so we can access window and grid
		public static DataGrid SqlBankGrid = null;
		public static SqlDbViewer SqlBankViewer = null;

		public static DataGrid SqlCustGrid = null;
		public static SqlDbViewer SqlCustViewer = null;

		public static DataGrid SqlDetGrid = null;
		public static SqlDbViewer SqlDetViewer = null;

		// Current active SQL Viewer pointer and Name
		public static SqlDbViewer ActiveSqlViewer = null;

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

		//Flags to hold pointers to current DbSelector & SqlViewer Windows
		// Needed to avoi dInstance issues when calling methods from inside Static methods
		// that are needed to handle the interprocess messaging system I have designed for this thing
		public static DbSelector DbSelectorOpen = null;
		public static EditDb CurrentEditDbViewer = null;
		public static SqlDbViewer CurrentSqlViewer = null;
		public static SqlDbViewer SqlUpdateOriginatorViewer = null;

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
			//Setup global flags - first clear them all as relevant
			Flags . ActiveSqlViewer = instance;
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
				//Remove a SINGLE Viewer Windows data
				SqlDbViewer . DeleteViewerAndFlags ( );
				Flags . CurrentSqlViewer . UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			}
		}

		public static void ListGridviewControlFlags ( int mode = 0 )
		{
			if ( mode == 1 )
			{
				Debug . WriteLine (
				"#################################################################\n" +
				$"FULL INFO\n" +
				"#################################################################\n" +
				//$"ActiveSqlViewer =          {ActiveSqlViewer.Name}\n" +
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
			$"ActiveSqlGrid  =		[{ActiveSqlGrid? . Name}]\n" +
			$"CurrentSqlViewer :-		[{CurrentSqlViewer?.Tag}]\n" +
			//				$"\nCurrentSqlViewer :- [{ActiveDbGridStr}]\n" +
			$"\nACTIVE VIEWERS:\n===============\n" +
			$"CurrentSqlViewer :-		 [ {CurrentSqlViewer?.Tag} ]\n" +
			$"ActiveSqlViewer :-		 [ {ActiveSqlViewer?.Tag} ]\n" +
			$"ALL VIEWERS:\n===========\n" +
			$"BANK     SqlBankViewer:- [ {SqlBankViewer?.Tag} ]\n" +
			$"CUST     SqlCustViewer:- [ {SqlCustViewer?.Tag} ]\n" +
			$"DETS     SqlDetViewer:-  [ {SqlDetViewer?.Tag} ]\n" +
			"=================================================================\n"
			);
		}
	}
}

