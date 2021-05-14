using System;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for BankDbView.xaml
	/// </summary>
	public partial class BankDbView : Window
	{
		public BankDbView ( )
		{
			InitializeComponent ( );

		}
		#region Mouse support
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}
		#endregion Mouse support

		#region Startup/ Closedown
		private void Window_Loaded ( object sender , RoutedEventArgs e )
		{
			// Data source is handled in XAML !!!!
			if ( this . BankGrid . Items . Count > 0 )
				this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankCollection . Bankcollection;
			this . MouseDown += delegate { DoDragMove ( ); };
			DataFields . DataContext = this . BankGrid . SelectedItem;

			EventControl . ViewerDataHasBeenChanged += ExternalDataUpdate;      // Callback in THIS FILE
														  //Subscribe to Bank Data Changed event declared in EventControl
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;

		}

		public void ExternalDataUpdate ( int DbEditChangeType , int row , string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Console . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankCollection . Bankcollection;
			this . BankGrid . Refresh ( );
		}
		#endregion Startup/ Closedown

		private void button_Click ( object sender , RoutedEventArgs e )
		{
			int x =0;
		}

		private void ViewerGrid_RowEditEnding ( object sender , System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . BankGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
			Flags . SqlBankCurrentIndex = currow;
			BankAccountViewModel ss = new BankAccountViewModel();
			ss = this . BankGrid . SelectedItem as BankAccountViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers();
			sqlh . UpdateDbRowAsync ( "BANKACCOUNT" , ss , this . BankGrid . SelectedIndex );

			this . BankGrid . SelectedIndex = Flags . SqlBankCurrentIndex;
			this . BankGrid . ScrollIntoView ( Flags . SqlBankCurrentIndex );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notidfication to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerViewerDataChanged ( 2 , this . BankGrid . SelectedIndex , "BANKACCOUNT" );

		}

		private void EventControl_BankDataLoaded ( object sender , LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			if ( e . CurrSelection == this . BankGrid . SelectedIndex )
				return;
			this . BankGrid . ItemsSource = null;
			this . BankGrid . ItemsSource = BankCollection . Bankcollection;
			this . BankGrid . Refresh ( );
		}

		private void ShowBank_KeyDown ( object sender , System . Windows . Input . KeyEventArgs e )
		{

		}

		private void Close_Click ( object sender , RoutedEventArgs e )
		{
			Close ( );
		}

		private void Window_Closing ( object sender , System . ComponentModel . CancelEventArgs e )
		{
			EventControl . ViewerDataHasBeenChanged -= ExternalDataUpdate;      // Callback in THIS FILE
														  //UnSubscribe from Bank Data Changed event declared in EventControl
			EventControl . BankDataLoaded -= EventControl_BankDataLoaded;

		}

		private void BankGrid_SelectionChanged ( object sender , System . Windows . Controls . SelectionChangedEventArgs e )
		{
			DataFields . DataContext = this . BankGrid . SelectedItem;

		}

		private void SaveBtn ( object sender , RoutedEventArgs e )
		{

		}

		private void BankGrid_CellEditEnding ( object sender , System . Windows . Controls . DataGridCellEditEndingEventArgs e )
		{
		}

		private void BankGrid_CurrentCellChanged ( object sender , System . EventArgs e )
		{
			DataFields . DataContext = e;

		}

		private void Window_Closing_1 ( object sender , System . ComponentModel . CancelEventArgs e )
		{

		}
	}
}
