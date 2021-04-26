#undef SHOWWINDOWDATA
using System;
using System . Collections . Generic;
using System . Linq;
using System . Runtime . CompilerServices;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Media;

using DocumentFormat . OpenXml . ExtendedProperties;


using WPFPages . Views;

namespace WPFPages
{
	/// <summary>
	/// Class to handle various utility functions such as fetching 
	/// Style/Templates/Brushes etc to Set/Reset control styles 
	/// from various Dictionary sources for use in "code behind"
	/// </summary>
	public class Utils
	{
//		public NewFlags Flags = new NewFlags();
		//************************************************************************************//
		/// <summary>
		///  checks an Enum in Flags.cs andf appends the correct sort 
		///  order to the SQL command string it receives
		/// </summary>
		/// <param name="commandline"></param>
		/// <returns></returns>
		public static string GetDataSortOrder ( string commandline )
		{
			if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . DEFAULT)
				commandline += "Custno, BankNo";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ID )
				commandline += "ID";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . BANKNO )
				commandline += "BankNo, CustNo";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . CUSTNO )
				commandline += "CustNo";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ACTYPE )
				commandline += "AcType";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . DOB )
				commandline += "Dob";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ODATE )
				commandline += "Odate";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . CDATE )
				commandline += "Cdate";
			return commandline;
		}

		//************************************************************************************//
		public static bool CheckForExistingGuid ( Guid guid )
		{
			bool retval = false;
			for ( int x = 0 ; x < Flags . DbSelectorOpen . ViewersList . Items . Count ; x++ )
			{
				ListBoxItem lbi = new ListBoxItem ( );
				//lbi.Tag = viewer.Tag;
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				if ( lbi . Tag == null ) return retval;
				Guid g = ( Guid ) lbi . Tag;
				if ( g == guid )
				{
					retval = true;
					break;
				}
			}
			return retval;
		}
		//************************************************************************************//
		public static void GetWindowHandles ( )
		{
#if SHOWWINDOWDATA
			Console.WriteLine ($"Current Windows\r\n"+"===============");
			foreach (Window window in System.Windows.Application.Current.Windows)
			{
				if (window.Title != "" && window.Content != "")
				{
					Console.WriteLine ($"Title:  {window.Title },\r\nContent - {window.Content}");
					Console.WriteLine ($"Name = [{window.Name}]\r\n");
				}
			}
#endif
		}
		//************************************************************************************//
		public static Style GetDictionaryStyle ( string tempname )
		{
			Style ctmp = System . Windows . Application . Current . FindResource ( tempname ) as Style;
			return ctmp;
		}
		//************************************************************************************//
		public static Template GetDictionaryTemplate ( string tempname )
		{
			Template ctmp = System . Windows . Application . Current . FindResource ( tempname ) as Template;
			return ctmp;
		}
		//************************************************************************************//
		public static ControlTemplate GetDictionaryControlTemplate ( string tempname )
		{
			ControlTemplate ctmp = System . Windows . Application . Current . FindResource ( tempname ) as ControlTemplate;
			return ctmp;
		}
		//************************************************************************************//
		public static Brush GetDictionaryBrush ( string brushname )
		{
			Brush brs = System . Windows . Application . Current . FindResource ( brushname ) as Brush;
			return brs;
		}

	}
}
