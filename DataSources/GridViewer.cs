using System;
using System.Windows;
using System.Windows.Controls;

using WPFPages.Views;

namespace WPFPages
{
	//this is a classic Singleton class - only one can ever exist
	//& it is used to store the details of ALL Db Gridviewer windows
	//currently open, so we can allow selection of any currently open
	//window even if they are hidden.
	public sealed class GridViewer {
		public static readonly GridViewer Viewer_instance = new GridViewer();
		public int MaxViewers = 10;
		//Save windows Handle
		public Window[ ] window = { null, null, null, null, null, null, null, null, null, null };
		//Save content of this list box as text string
		public string[ ] CurrentDb = { "", "", "", "", "", "", "", "", "", "" };
		public string PrettyDetails = "";
		public int[ ] ListBoxId = { -1,-1,-1,-1,-1, -1, -1, -1, -1, -1 };
		//store this windows position in the Listbox
		public DataGrid[ ] Datagrid = { null, null, null, null, null, null, null, null, null, null };
		//Flag fields
		//The handle to the ONLY Selection Window currently open
		public DbSelector DbSelectorWindow = (DbSelector)null;
		public int ViewerSelectiontype = -1;    // flag  for return value from selection window
		//total viewers open right now
		public int ViewerCount = 0;
		//These next 2 both hold the handle of the currently selected Viewer window ??
//		public Window ChosenViewer = (Window)null;
//		public DbSelector DbSelectorWindow = (DbSelector)null;

	}

}
