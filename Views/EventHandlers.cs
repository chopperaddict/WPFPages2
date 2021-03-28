using DocumentFormat.OpenXml.Bibliography;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using WPFPages.Properties;

namespace WPFPages.Views
{
	//First we have to define a delegate that acts as a signature for the
	//function that is ultimately called when the event is triggered.
	//You will notice that the second parameter is of MyEventArgs type.
	//This object will contain information about the triggered event.
	//	public delegate void SelectionChanged (object source, SelectedEventArgs e);
	public delegate void SQLViewerGridSelectionChanged (int value, DataGrid caller);
	public delegate void EditDbGridSelectionChanged (int value, DataGrid caller);

	public sealed class newtester
	{
		static event SQLViewerGridSelectionChanged SQLVSelChange;
		static  event EditDbGridSelectionChanged EditDbSelChange;

		public MainWindow mw = null;

		public static EditDb edDb { get; set; }
		public static SqlDbViewer sqlDb { get; set; }

		public newtester GetEventWindowHandle ()
		{
			return this;
		}
		public static void SetWindowHandles (EditDb edb, SqlDbViewer sdb)
		{
			if (edb != null)
			{
				edDb = edb;
				EditDbSelChange += new EditDbGridSelectionChanged (edDb.resetEditDbindex);
			}
			if (sdb != null)
			{
				sqlDb = sdb;
				SQLVSelChange += new SQLViewerGridSelectionChanged (sqlDb.resetSQLDBindex);
			}
		}	
		/// Constructor (Specialised)
		/// <param name="edb"> This is the Caller class</param>
		/// <param name="dg"> This is the (Active) Datagrid in the above class</param>
		public  newtester (DataGrid dg, string CallerName, out  newtester thisWin)
		{
			int count = 0;
			int count2 = 0;
			Console.WriteLine ($"EventHandler.NewTester(51) : In Constructor - CallerName = {CallerName}.");
			//			MainWindow mw = new MainWindow ();
			if (CallerName == "SQLDBVIEWER")
			{
//				sqlDb = edb as SqlDbViewer;
				Console.WriteLine ($"EventHandler.NewTester(59) : Subscribing to Event SQLVSelChange - CallerName = {CallerName}, hWnd={sqlDb}.");
//				SQLVSelChange += new SQLViewerGridSelectionChanged (sqlDb.resetSQLDBindex);
				ShowSubscribersCount ();
			}
			else
			{
//				edDb = edb as EditDb;
				Console.WriteLine ($"EventHandler.NewTester(63) : Subscribing to Event EditDbSelChange - CallerName = {CallerName}, hWnd={edDb}.");
//				EditDbSelChange += new EditDbGridSelectionChanged (edDb.resetEditDbindex);
				ShowSubscribersCount ();
			}
			thisWin = this;
		}


		private void ShowSubscribersCount ()
		{
			int count = -1;
			int count2 = -1;
			if (SQLVSelChange != null)
			{	try
				{count = SQLVSelChange.GetInvocationList ().Length;}
				catch { }
			}
			if (EditDbSelChange != null)
			{	try
				{count2 = EditDbSelChange.GetInvocationList ().Length;}
				catch { }
			}
			Console.WriteLine ($"EventHandler(95) Subscribed Events:\r\n		[SQLVSelChange = {count}, EditDbSelChange = {count2}]");
			Delegate[ ] dglist = SQLVSelChange?.GetInvocationList ();
			if (dglist != null)
			{
				foreach (var item in dglist)
				{ Console.WriteLine ($"SQLVSelChange - {item.Target.ToString ()}"); }
			}
			Delegate[] dglist2 = EditDbSelChange?.GetInvocationList ();
			if (dglist2 != null)
			{
				foreach (var item in dglist2)
				{ Console.WriteLine ($"EditDbSelChange - {item.Target.ToString ()}"); }
			}
			Console.WriteLine ($"SQLVSelChange = {SQLVSelChange?.ToString()},\r\nEditDbSelChange = {EditDbSelChange?.ToString()}");
		}

		/// <summary>
		/// Event Handlers
		/// </summary>
		/// <param name="x"></param>
		/// <param name="caller"></param>
		public void SqlDbTriggerEvent (int x, DataGrid caller)
		{
			/// <summary>
			/// This is the Trigger that is called when the selectedIndex changes
			/// in an SqlDbViewer DataGrid to let  EditDb  Grid update in parallel
			/// </summary>
			Console.WriteLine ($"EventHandler (110) SqlDbTriggerEvent called with {x} and {caller}");
			Console.WriteLine ($"EventHandler(111) SqlDbTriggerEvent Invoking EditDbSelchange()");
			ShowSubscribersCount ();
			if (EditDbSelChange != null)
			{
				MainWindow mw = new MainWindow (); ;
				if (mw.EventHandlerDebug)
					Console.WriteLine ($"EventHandler(136)  EditDb is triggering EventHandler.SqlDbTriggerEvent:  EditDbSelChange.?Invoke ({x}) called (for SqlDbViewer to change itself ?) "); ;
				EditDbSelChange?.Invoke (x, caller);
			}
		}
		public void EditDbTriggerEvent (int x, DataGrid caller)
		{
			/// <summary>
			/// This is the actual Trigger called when the selectedIndex
			/// changes in a SqlDbviewer DataGrid to let EditDb's grid update in parallel
			/// </summary>

			Console.WriteLine ($"EventHandler(128) EditDbTriggerEvent  called with {x} and {caller}");
			Console.WriteLine ($"EventHandler(129) EditDbTriggerEvent - Invoking SQLVSelchange()");
			ShowSubscribersCount ();
			if (SQLVSelChange != null)
			{
				MainWindow mw = new MainWindow (); ;
				if (mw.EventHandlerDebug)
					Console.WriteLine ($"EventHandler(117) SqlDbViewer triggering EventHandler.EditDbTriggerEvent \r\n Trigger  :  SQLVSelChange.?Invoke ({x}) called so EditDb can change itself ?) "); 
				SQLVSelChange?.Invoke (x, caller);
			}

		}


		/// <summary>
		/// / Handlers for SqlDbViewer and EditDb windows
		/// </summary>
		/// <param name="selectedRow"></param>
		/// <param name="caller"> is DataGrid</param>
		//public static void resetSQLDBindex (int selectedRow, DataGrid caller)
		//{
		//	int x = 0;
		//	x = selectedRow;

		//	Console.WriteLine ($"SqlDbViewer() got row change of {x} in EditDb()");
		//	caller.SelectedIndex = x;
		//}
		//public static  void resetEditDbindex (int selectedRow, DataGrid caller)
		//{
		//	int x = 0;
		//	x = selectedRow;
		//	Console.WriteLine ($"EditDb() got row change of {x} in EditDb()");
		//	// use our Global Grid pointer for access
		//	caller.SelectedIndex = x;
		//}

	}



	public class tester
	{
		public tester () { }


		public void IndexChange (int index, string caller)
		{
			int newindex = 0;
			/// <summary>
			/// This value will normally trigger our event
			/// </summary>
			newindex = index;
		}
	}   /// <summary>
	    /// Define EventArgs for our delegate event
	    /// </summary>
	    /// //This is a class which describes the event to the class that recieves it.
	//An EventArgs class must always derive from System.EventArgs.
	public class SelectedEventArgs: EventArgs
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
		/// <summary>
		///  Constructor
		/// </summary>
		/// <param name="index" - Selected row></param>
		/// <param name="caller"> Caller's Grid Name</param>
		public SelectedEventArgs (int index, string caller)
		{ NewSelectedindex = index; Caller = caller; }

		// public access functions
		public int GetIndex ()
		{ return NewSelectedindex; }
		public string GetCaller ()
		{ return Caller; }
	}

	//This next class is the one which contains an event and triggers it
	//once an action is performed. For example, lets trigger this event
	//once a variable is incremented over a particular value. Notice the
	//event uses the MyEventHandler delegate to create a signature
	//for the called function.
	class MonitorDataGridSelectionChanges
	{
		int _newindex = 0;

		// We have to Declare our event handler in the class that is going to trigger it
		// but it is based on the Delegate declared in the Namespace
		// NB we do NOT need to use the args in this declaration
		//		public event SelectionChanged OnSelectionChanged;
		/// <summary>
		/// Main Constructor
		/// This receives the newly selected index, and if different to
		/// previous value, triggers the Event Notification
		/// </summary>
		public MonitorDataGridSelectionChanges ()
		{
			tester tst = new tester ();
			//			SQLViewerGridSelectionChanged  gsc = tst.IndexChange;
		}


		/// <summary>
		///  Property for selectedindex that will trigger Event
		/// </summary>
		//public int newindex
		//{
		//	get { return _newindex; }
		//	set
		//	{
		//		if (_newindex != value)
		//		{
		//			_newindex = value;
		//			//To make sure we only trigger the event if a handler is present
		//			//we check the event to make sure it's not null.
		//			if (OnSelectionChanged != null)
		//			{
		//				OnSelectionChanged (this, new SelectedEventArgs (newindex));
		//			}
		//		}
		//	}

		//}  // End class MonitorDataGridSelectionChanges

		public class HandleSelectionChange
		{
			// We have to Declare our event handler in the class that is going to trigger it
			// but it is based on the Delegate declared in the Namespace
			// NB we do NOT need to use the args in this declaration
			//			public event SelectionChanged OnSelectionChanged;
			//public HandleSelectionChange ()
			//{
			//	SayDelegate sd = new SayDelegate (showstring);
			//	sd ("dafsasds");
			//}
			public string showstring (string s)
			{
				return "Hello there - " + s;
			}
			/// <summary>
			/// Now lets test the event contained in the above class.
			/// we pass  the handler function name as the argument
			/// </summary>

			MonitorDataGridSelectionChanges MyObject = new MonitorDataGridSelectionChanges ();
			//			MyObject += new SelectionChanged (IndexChanged);


			/// <summary>
			///This is the actual method that will be assigned to the event handler
			///within the above class. This is where we perform an action once the
			///event has been triggered.
			/// </summary>
			static void OnSelectionChanged (object source, SelectedEventArgs e)
			{
				int newindex = e.GetIndex ();
				// TODO 
				//Changed the current SeletedIndex in Datagrid of interest to us
				//...
			}
		}


	}

} // End namespace




