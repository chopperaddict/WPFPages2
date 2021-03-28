using DocumentFormat.OpenXml.ExtendedProperties;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFPages
{
	/// <summary>
	/// Class to handle various utility functions such as fetching 
	/// Style/Templates/Brushes etc to Set/Reset control styles 
	/// from various Dictionary sources for use in "code behind"
	/// </summary>
	public class Utils
	{
		public static Style GetDictionaryStyle (string tempname)
		{
			Style ctmp = System.Windows.Application.Current.FindResource (tempname) as Style;
			return ctmp;
		}
		public static Template GetDictionaryTemplate (string tempname)
		{
			Template ctmp = System.Windows.Application.Current.FindResource (tempname) as Template;
			return ctmp;
		}
		public static ControlTemplate GetDictionaryControlTemplate (string tempname)
		{
			ControlTemplate ctmp = System.Windows.Application.Current.FindResource (tempname) as ControlTemplate;
			return ctmp;
		}
		public static Brush GetDictionaryBrush (string brushname)
		{
			Brush brs = System.Windows.Application.Current.FindResource (brushname) as Brush;
			return brs;
		}

	}
}
