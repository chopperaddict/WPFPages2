
using System;
using System.Windows.Controls;

using WPFPages.ViewModels;

namespace WPFPages.Views
{
	//First we have to define a delegate that acts as a signature for the
	//function that is ultimately called when the event is triggered.
	//You will notice that the second parameter is of MyEventArgs type.
	//This object will contain information about the triggered event.
	//	public delegate void SelectionChanged (object source, SelectedEventArgs e);
	public delegate void SQLViewerGridSelectionChanged (bool self, int value, DataGrid caller);
	public delegate void EditDbGridSelectionChanged (bool self, int value, DataGrid caller);

	// Details Db data fully loaded
	public delegate void BankLoadedDelegate ();
	public delegate void CustLoadedDelegate ();
	public delegate void DetailsLoadedDelegate ();

	public class EventHandlers
	{
		public static event SQLViewerGridSelectionChanged SQLVSelChange;
		public static event EditDbGridSelectionChanged EditDbSelChange;

		//SqlDbViewer Data Loading Events
		// Bank Db data fully loaded
		public static event BankLoadedDelegate BankDataCanBeLoaded;
		// Customers Db data fully loaded
		public static event CustLoadedDelegate CustDataCanBeLoaded;
		// Details Db data fully loaded
		public static event DetailsLoadedDelegate DetailsDataCanBeLoaded;

		public static event DbUpdated NotifyOfDataChange;


		public MainWindow mw = null;

		public static EditDb edDb { get; set; }
		public static SqlDbViewer sqlDb { get; set; }

		public  EventHandlers()
		{

		}

		/// <summary>
		///  Triggered by a Task() completing the load of Details data from SQL in SqlDbViewer (and elsewhere ? )
		/// </summary>
		public static void TriggerDetailsDataLoaded ()
		{ EventHandlers.DetailsDataCanBeLoaded.Invoke (); }
		public static void TriggerBankDataLoaded ()
		{ EventHandlers.BankDataCanBeLoaded.Invoke (); }
		public static void TriggerCustDataLoaded ()
		{ EventHandlers.CustDataCanBeLoaded.Invoke (); }

		public EventHandlers GetEventWindowHandle ()
		{
			return this;
		}
		public static void SetWindowHandles (EditDb edb, SqlDbViewer sdb, DataGrid detailsGrid)
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
			ShowSubscribersCount ();
		}
		public static void ClearWindowHandles (EditDb edb, SqlDbViewer sdb)
		{
			if (edb != null)
			{
				edDb = edb;
				EditDbSelChange -= new EditDbGridSelectionChanged (edDb.resetEditDbindex);
			}
			if (sdb != null)
			{
				sqlDb = sdb;
				SQLVSelChange -= new SQLViewerGridSelectionChanged (sqlDb.resetSQLDBindex);
			}
			ShowSubscribersCount ();
		}
		/// Constructor (Specialised)
		/// <param name="edb"> This is the Caller class</param>
		/// <param name="dg"> This is the (Active) Datagrid in the above class</param>
		public EventHandlers (DataGrid dg, string CallerName, out EventHandlers thisWin)
		{
			thisWin = this;
			if (!BankAccountViewModel.ShowSubscribeData)
				return;
			int count = 0;
			int count2 = 0;
			if (Flags.EventHandlerDebug)
			{
				Console.WriteLine ($"EventHandler.EventHandlers(51) : In Constructor - CallerName = {CallerName}.");
				if (CallerName == "SQLDBVIEWER")
					Console.WriteLine ($"EventHandler.EventHandlers(59) : Subscribing to Event SQLVSelChange - CallerName = {CallerName}, hWnd={sqlDb}.");
				if (CallerName == "CUSTOMER")
					Console.WriteLine ($"EventHandler.EventHandlers(59) : Subscribing to Event SQLVSelChange - CallerName = {CallerName}, hWnd={sqlDb}.");
				else
					Console.WriteLine ($"EventHandler.EventHandlers(63) : Subscribing to Event EditDbSelChange - CallerName = {CallerName}, hWnd={edDb}.");
				ShowSubscribersCount ();
			}
		}


		public static void ShowSubscribersCount ()
		{
			int count = -1;
			int count2 = -1;
			int count3 = -1;
			int count4 = -1;
			int count5 = -1;
			int count6 = -1;
			if (!BankAccountViewModel.ShowSubscribeData)
				return;
			if (SQLVSelChange != null)
			{
				try
				{ count = SQLVSelChange.GetInvocationList ().Length; }
				catch { }
			}
			if (EditDbSelChange != null)
			{
				try
				{ count2 = EditDbSelChange.GetInvocationList ().Length; }
				catch { }
			}
			if (BankDataCanBeLoaded != null)
			{
				try
				{ count3 = BankDataCanBeLoaded.GetInvocationList ().Length; }
				catch { }
			}
			if (CustDataCanBeLoaded != null)
			{
				try
				{ count4 = CustDataCanBeLoaded.GetInvocationList ().Length; }
				catch { }
			}
			if (DetailsDataCanBeLoaded != null)
			{
				try
				{ count5 = DetailsDataCanBeLoaded.GetInvocationList ().Length; }
				catch { }
			}
			if (NotifyOfDataChange != null)
			{
				try
				{ count6 = NotifyOfDataChange.GetInvocationList ().Length; }
				catch { }
			}
			
			if (Flags.EventHandlerDebug)
				Console.WriteLine ($"EventHandler(98) Subscribed Events:\r\n		[SQLVSelChange = {count}, EditDbSelChange = {count2}], \r\nBankDataCanBeLoaded = {count3} CustDataCanBeLoaded = {count4} DetailsDataCanBeLoaded = {count5}");
			Delegate[ ] dglist = SQLVSelChange?.GetInvocationList ();
			if (Flags.EventHandlerDebug)
			{
				if (dglist != null)
				{
					foreach (var item in dglist)
					{ Console.WriteLine ($"SQLVSelChange - {item.Target.ToString ()}\r\nMethod = {item.Method.ToString ()}"); }
				}
				Delegate[ ] dglist2 = EditDbSelChange?.GetInvocationList ();
				if (dglist2 != null)
				{
					foreach (var item in dglist2)
					{ Console.WriteLine ($"EditDbSelChange - {item.Target.ToString ()}\r\nMethod = {item.Method.ToString ()}"); }
				}
				Console.WriteLine ($"SQLVSelChange = {SQLVSelChange?.ToString ()},\r\nEditDbSelChange = {EditDbSelChange?.ToString ()}");
			}
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
		}

		/// <summary>
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

			//public class HandleSelectionChange
			//{
			//	// We have to Declare our event handler in the class that is going to trigger it
			//	// but it is based on the Delegate declared in the Namespace
			//	// NB we do NOT need to use the args in this declaration
			//	//			public event SelectionChanged OnSelectionChanged;
			//	//public HandleSelectionChange ()
			//	//{
			//	//	SayDelegate sd = new SayDelegate (showstring);
			//	//	sd ("dafsasds");
			//	//}
			//	public string showstring (string s)
			//	{
			//		return "Hello there - " + s;
			//	}
			//	/// <summary>
			//	/// Now lets test the event contained in the above class.
			//	/// we pass  the handler function name as the argument
			//	/// </summary>

			//	MonitorDataGridSelectionChanges MyObject = new MonitorDataGridSelectionChanges ();
			//	//			MyObject += new SelectionChanged (IndexChanged);


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

		/// <summary>
		/// Event Handler triggered by SQLDBVIEWER
		/// OR if EditDb has changed SelectedIndex
		/// </summary>
		/// <param name="x"></param>
		/// <param name="caller"></param>
		public void SqlDbTriggerEvent (bool self, int RecordNo, DataGrid caller, bool IsUpdated)
		{
			/// <summary>
			/// This is the Trigger that is ONLY called when the selectedIndex changes
			/// in an SqlDbViewer DataGrid  AND  we have an open EditDb  Grid to update 
			/// </summary>
			if (EditDbSelChange != null)
			{
				if (Flags.EventHandlerDebug)
					Console.WriteLine ($"\r\n*** EVENTHANDLER ***- EventHandlers.SqlDbTriggerEvent() (340)  Triggering EditDbSelChange.?Invoke ({RecordNo}, {caller})\r\n");

				ShowSubscribersCount ();
				// Edit has been triggered by SqlDbViewer ( MainWindow.DgControl.SelectionChangeInitiator == 1)
				// if set to 1, we need to triggers  resetEditDbindex (int RowToFind, DataGrid caller) in EditDb.cs
				// To make EditDb update its own data
				if (MainWindow.DgControl.SelectionChangeInitiator == 1) // tells us SqlDbViewer initiated the record change
					EditDbSelChange?.Invoke (Flags.isEditDbCaller, RecordNo, caller);
				else
					SQLVSelChange?.Invoke (Flags.isEditDbCaller, RecordNo, caller);
				//MainWindow.DgControl.SelectionChangeInitiator = -1; // tells us it is a EditDb initiated the record change

			}
		}
		/// <summary>
		///  this is triggered by changes made in DBEDIT
		/// </summary>
		/// <param name="x"></param>
		/// <param name="caller"></param>
		/// <param name="IsUpdated"></param>
		public void EditDbTriggerEvent (bool self, int x, DataGrid caller, bool IsUpdated)
		/* CALLED BY DBEDIT  */
		{
//			ShowSubscribersCount ();
			if (SQLVSelChange != null)
			{
				if (Flags.EventHandlerDebug)
					Console.WriteLine ($"\r\n*** EVENTHANDLER *** - EventHandlers.EditDbTrigger() (308)  Triggering SQLVSelChange.?Invoke ({x})\r\n");
				// This triggers -  resetSQLDBindex (int RowSelected, DataGrid caller) in SQLDbVIEWER  
				//				Flags.isEditDbCaller = false;
				if (MainWindow.DgControl.SelectionChangeInitiator == 2) // tells us EditDb initiated the record change, so tell SqlDbViewer to update
					SQLVSelChange?.Invoke (Flags.isEditDbCaller, x, caller);
			}
		}

	}

} // End namespace




