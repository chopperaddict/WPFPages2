using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
//		public static int GridWindows = 0;
		//public DataGrid CurrentGrid;
		//public DateTime LoadTime;

			

		// These are used to try to force Textbox's to always select
		// all the content in the field, rahter than the default
		// // of putting the cursor at end of the current content
		protected override void OnStartup(StartupEventArgs e) {
			EventManager.RegisterClassHandler(typeof(TextBox),
			    TextBox.GotFocusEvent,
			    new RoutedEventHandler(TextBox_GotFocus));

			FrameworkElement.LanguageProperty.OverrideMetadata(
			 typeof(FrameworkElement),
			 new FrameworkPropertyMetadata(
			 System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));


			base.OnStartup(e);
		}
		private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
			TextBox tb = sender as TextBox;
				e.Handled = true;
			tb.SelectAll();
		}
	}
}
