
using System;
using System . Windows . Controls;
using WPFPages . ViewModels;
using static WPFPages . SqlDbViewer;

namespace WPFPages . Views
{
	//First we have to define a delegate that acts as a signature for the
	//function that is ultimately called when the event is triggered.
	//You will notice that the second parameter is of MyEventArgs type.
	//This object will contain information about the triggered event.
	//	public delegate void SelectionChanged (object source, SelectedEventArgs e);
	public delegate void SQLViewerGridSelectionChanged ( bool self, int value, DataGrid caller );
	public delegate void SqlSelectedRowChanged ( int row, string CurentDb );
	public delegate void EditDbGridSelectionChanged ( bool self, int value, DataGrid caller );

	//SqlDbViewer.Selectedindex changed - notify EditDb window	
	public delegate void SqlViewerRowChange ( int row, string CurentDb );
	//EditDb.Selectedindex changed - notify SqlDbViewer window	
	public delegate void EditDbRowChange ( int row, string CurentDb );

	public delegate void DbUpdated ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args );
	public delegate void NotifyViewer ( int status, string info, SqlDbViewer NewSqlViewer );

	public class EventHandlers
	{
		// Event sent by ALL ViewModel classes  to notify of Db changes
		public  DbUpdated NotifyOfDataChange;
		// Event sent by SqlDbViewer to notify EditDb of index change
		public static SqlSelectedRowChanged SqlViewerIndexChanged;
		// Event sent by EditDb to notify SqlDbViewer of index change
		public static EditDbGridSelectionChanged EditDbSelChange;
		//public static event SqlViewerRowChange SqlViewerSelectedIndexChanged;
		public static EditDbRowChange EditDbViewerSelectedIndexChanged;
		public static SQLViewerGridSelectionChanged SqlViewerGridChanged;

		public static SqlViewerRowChange SqlViewerRowChanged;
		public static EditDbRowChange EditDbRowchanged ;


		public MainWindow mw = null;

		//		public DbUpdated NotifyOfDataChange;

		//public static EditDb edDb { get; set; }
		//public static SqlDbViewer sqlDb { get; set; }

		public EventHandlers()
		{

		}
		/// Constructor (Specialised)
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
				//	Console . WriteLine ( $"EventHandler.EventHandlers(63) : Subscribing to Event EditDbSelChange - CallerName = {CallerName}, hWnd={edDb}." );
				//				ShowSubscribersCount ( );
			//}
		}


		/// <summary>
		/// Define EventArgs for our delegate event
		/// </summary>
		/// //This is a class which describes the event to the class that recieves it.
		//An EventArgs class must always derive from System.EventArgs.
		public class SelectedEventArgs : EventArgs
		{
			private int _newSelectedindex;
			private string _caller;

			public int NewSelectedindex
			{
				get { return _newSelectedindex; }
				set { _newSelectedindex = value; }
			}
			public string Caller
			{
				get { return _caller; }
				set { _caller = value; }
			}
		}

		public  void ShowSubscribersCount ( )
		{
			int count = -1;
			int count2 = -1;
			int count3 = -1;
			int count4 = -1;
			int count5 = -1;
			int count6 = -1;
			if ( !BankAccountViewModel . ShowSubscribeData )
				return;
			if ( NotifyOfDataChange != null )
			{
				try
				{ count = NotifyOfDataChange . GetInvocationList ( ) . Length; }
				catch { }
			}
			if ( SqlViewerIndexChanged != null )
			{
				try
				{ count2 = SqlViewerIndexChanged . GetInvocationList ( ) . Length; }
				catch { }
			}
			if ( EditDbViewerSelectedIndexChanged != null )
			{
				try
				{ count3 = EditDbViewerSelectedIndexChanged . GetInvocationList ( ) . Length; }
				catch { }
			}
			if ( EditDbSelChange != null )
			{
				try
				{ count4 = EditDbSelChange . GetInvocationList ( ) . Length; }
				catch { }
			}
			//if ( DetailsDataCanBeLoaded != null )
			//{
			//	try
			//	{ count5 = DetailsDataCanBeLoaded . GetInvocationList ( ) . Length; }
			//	catch { }
			//}
			//if ( NotifyOfDataChange != null )
			//{
			//	try
			//	{ count6 = NotifyOfDataChange . GetInvocationList ( ) . Length; }
			//	catch { }
			//}

//			if ( Flags . EventHandlerDebug )
				Console . WriteLine ( $"\nEventHandler(98) Subscribed Events:\nNotifyOfDataChange =				{count},\nSqlViewerIndexChanged =				{count2}, \nEditDbViewerSelectedIndexChanged =	{count3} \nEditDbSelChange =					{count4} " );

			Delegate [ ] dglist = NotifyOfDataChange?.GetInvocationList ( );
			if ( Flags . EventHandlerDebug )
			{
				//if ( dglist != null )
				//{
				//	foreach ( var item in dglist )
				//	{ Console . WriteLine ( $"SQLVSelChange - {item . Target . ToString ( )}\r\nMethod = {item . Method . ToString ( )}" ); }
				//}
				Delegate [ ] dglist2 = EditDbSelChange?.GetInvocationList ( );
				if ( dglist2 != null )
				{
					foreach ( var item in dglist2 )
					{ Console . WriteLine ( $"EditDbSelChange - {item . Target . ToString ( )}\r\nMethod = {item . Method . ToString ( )}" ); }
				}
//				Console . WriteLine ( $"SQLVSelChange = {SQLVSelChange?.ToString ( )},\r\nEditDbSelChange = {EditDbSelChange?.ToString ( )}" );
			}
		}
	}

} // End namespace




