
using System;
using System . Diagnostics;
using System . Windows . Controls;
using DocumentFormat . OpenXml . Spreadsheet;
using WPFPages . ViewModels;
using static WPFPages . SqlDbViewer;

namespace WPFPages . Views
{
	//First we have to define a delegate that acts as a signature for the
	//function that is ultimately called when the event is triggered.
	//You will notice that the second parameter is of MyEventArgs type.
	//This object will contain information about the triggered event.
	//	public delegate void SelectionChanged (object source, SelectedEventArgs e);


	#region KNOWN DELEGATES IN USE
	public delegate bool DbReloaded ( object sender, DataLoadedArgs args );

	public delegate void DbUpdated ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args );

	public delegate void EditDbGridSelectionChanged ( int ChangeType, int value, string caller );

	public delegate void EditDbDataChanged ( int EditDbChangeType, int row, string CurentDb );

	public delegate void NotifyViewer ( int status, string info, SqlDbViewer NewSqlViewer );

	public delegate void SQLViewerSelectionChanged ( int ChangeType, int row, string CurrentDb );

	public delegate void SqlSelectedRowChanged ( int ChangeType, int row, string CurentDb );

	public delegate void SqlViewerNotify ( int status, string info, SqlDbViewer NewSqlViewer );


	//************************************************

	public delegate void ChimeEventHandler ( );

	public class ClockTower
	{
		// declare event based on delegate above
		public static event ChimeEventHandler Chime;
		public static void sChimeFivePm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}
		public static void sChimeSixAm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}
		public void ChimeFivePm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}
		public void ChimeSixAm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}
	}

	// UNUSED			public delegate void SqlViewerRowChange ( int row, string CurentDb );

	#endregion KNOWN DELEGATES IN USE

	public class EventHandlers
	{
		#region DELEGATES IN USE
		//Delegates I AM USING
		//Declare our Event that hopefully will call us back once the data has been loaded Asynchronously

		// Event sent by ALL ViewModel classes  to notify of Db changes
		//		public static DbUpdated NotifyOfDataChange;


		// Event sent by EditDb to notify SqlDbViewer of index change

		//		public static EditDbDataChanged EditDbViewerSelectedIndexChanged;

		public static NotifyViewer SendViewerCommand;

		public static SqlSelectedRowChanged SqlViewerIndexChanged;
		#endregion DELEGATES IN USE

		//NOT USED - 28/4/21
		//		public static event SqlViewerRowChange SqlViewerSelectedIndexChanged;


		//Delegates I AM NOT USING RIGHT NOW ??
		//		public static SqlViewerRowChange SqlViewerRowChanged;
		//		public static EditDbDataChanged EditDbDataChangedd;

		public MainWindow mw = null;

		/// ONLY CONSTRUCTOR (Specialised)
		/// <param name="edb"> This is the Caller class</param>
		/// <param name="dg"> This is the (Active) Datagrid in the above class</param>
		public EventHandlers ( DataGrid dg, string CallerName, out EventHandlers thisWin )
		{
			thisWin = this;
			if ( !BankAccountViewModel . ShowSubscribeData )
				return;
			int count = 0;
			int count2 = 0;
			//if ( Flags . EventHandlerDebug )
			//{
			Console . WriteLine ( $"EventHandler.EventHandlers(51) : In Constructor - CallerName = {CallerName}." );
			//if ( CallerName == "SQLDBVIEWER" )
			//	Console . WriteLine ( $"EventHandler.EventHandlers(59) : Subscribing to Event SQLVSelChange - CallerName = {CallerName}, hWnd={sqlDb}." );
			//if ( CallerName == "CUSTOMER" )
			//	Console . WriteLine ( $"EventHandler.EventHandlers(59) : Subscribing to Event SQLVSelChange - CallerName = {CallerName}, hWnd={sqlDb}." );
			//else
			//	Console . WriteLine ( $"EventHandler.EventHandlers(63) : Subscribing to Event SqlHasChangedSelection - CallerName = {CallerName}, hWnd={edDb}." );
			//				ShowSubscribersCount ( );
			//}
		}

		// Not used if DEBUG is UnDefined
		[Conditional ( "DEBUG" )]
		public static void ShowSubscribersCount ( )
		{
			int count = -1;
			int count2 = -1;
			int count3 = -1;
			int count4 = -1;
			int count5 = -1;
			int count6 = -1;
			int count7 = -1;
			int count8 = -1;
			if ( !BankAccountViewModel . ShowSubscribeData )
				return;
			//if ( NotifyOfDataChange != null )
			//{
			//	try
			//EditDbViewerSelectedIndexChanged
			Delegate [ ] dg = SqlDbViewer . GetEventCount ( );
			if ( dg != null )	count = dg . Length;
			
			// NotifyOfDataChange
			dg = SqlDbViewer . GetEventCount2 ( );
			if ( dg != null )	count2 = dg . Length;

			// NotifyOfDataLoaded
			dg = SqlDbViewer . GetEventCount3 ( );
			if ( dg != null )	count3 = dg . Length;

			// SqlHasChangedSelection
			dg = EditDb . GetEventCount4 ( );
			if ( dg != null )	count4 = dg . Length;

			// SqlViewerIndexChanged
			dg = EditDb. GetEventCount5 ( );			
			if ( dg != null )	count5 = dg . Length;

			// DetCollection. DetDataLoaded 
			dg = BankCollection. GetEventCount6 ( );
			if ( dg != null ) count6 = dg . Length;

			Console . WriteLine (
			$"\n *** Currently Subscribed Events  ***" +
			$"\nEditDbViewerSelectedIndexChanged =	{count} " +
			$"\nNotifyOfDataChange =		 		{count2}" +
			$"\nNotifyOfDataLoaded =		 		{count3}" +
			$"\nSqlHasChangedSelection = 			{count4}" +
			$"\nBankCollection. BankDataLoaded =	{count6}" +
			$"\nCustCollection. CustDataLoaded =	{count7}" +
			$"\nDetCollection. DetDataLoaded =		{count8}");
			//$"\nSqlViewerIndexChanged =				{count5} " );
			//			Console . WriteLine ( $"....................................................................................." );

			bool first = true;
			Delegate [ ] dglist2 = SqlDbViewer . GetEventCount ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : EditDbViewerSelectedIndexChanged :\n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
					//					Console . WriteLine ( );
				}
			}
			dglist2 = SqlDbViewer . GetEventCount2 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				first = true;
				Console . WriteLine ( $"=====================================================================================" );
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : NotifyOfDataChange : \n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
					//					Console . WriteLine ( );
				}
			}
			dglist2 = SqlDbViewer . GetEventCount3 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				first = true;
				Console . WriteLine ( $"=====================================================================================" );
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : NotifyOfDataLoaded: \n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
					//					Console . WriteLine ( );
				}
			}
			dglist2 = EditDb. GetEventCount4 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : SqlHasChangedSelection :\n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
					//					Console . WriteLine ( );
				}
			}
			dglist2 = EditDb . GetEventCount5 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : SqlViewerIndexChanged :\n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
				//				Console . WriteLine ( "\n" );	// not on last line
			}
			dglist2 = BankCollection . GetEventCount6 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if( item . Target != null)
						Console . WriteLine ( $"Delegate : BankCollection. BankDataLoaded:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : BankCollection. BankDataLoaded:\n >>> \nMethod = {item . Method . ToString ( )}" );
						cnt++;
				}
				//				Console . WriteLine ( "\n" );	// not on last line
			}
			dglist2 = CustCollection . GetEventCount7 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : CustCollection. CustDataLoaded:\n >>> {item . Target? . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : CustCollection. CustDataLoaded:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
				//				Console . WriteLine ( "\n" );	// not on last line
			}
			dglist2 = DetCollection . GetEventCount8 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : DetCollection. DetDataLoaded:\n >>> {item . Target? . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : DetCollection. DetDataLoaded:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
				//				Console . WriteLine ( "\n" );	// not on last line
			}


			Console . WriteLine ( $"+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
		}
	}
} // End namespace




