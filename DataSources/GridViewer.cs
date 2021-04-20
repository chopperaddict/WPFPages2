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
		public Guid[ ] ListBoxId = { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, };
		//store this windows position in the Listbox
		public DataGrid[ ] Datagrid = { null, null, null, null, null, null, null, null, null, null };
		//The handle to the ONLY Selection Window currently open
		public DbSelector DbSelectorWindow = (DbSelector)null;
		public SqlDbViewer SqlViewerWindow = (SqlDbViewer)null;
		public Guid  Bankviewer = Guid.Empty;
		public Guid Custviewer = Guid.Empty;
		public Guid Detviewer = Guid.Empty;
		public Guid  SqlViewerGuid = Guid.Empty;
		public int ViewerSelectiontype = -1;    // flag  for return value from selection window
		//total viewers open right now
		public int ViewerCount = 0;
		public int SelectedViewerType = 0;
	}

}
/*
 MainWindow.gv structure
Maximum is 10

Window[ ] window
string[ ] CurrentDb 
string PrettyDetails
int[ ] ListBoxId 
DataGrid[ ] Datagrid
DbSelector DbSelectorWindow
int ViewerSelectiontype 
int ViewerCount 
int SelectedViewerType
*/
