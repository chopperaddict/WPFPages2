using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;

namespace WPFPages
{
	public class BankAccount : INotifyPropertyChanged
	{
		//This is the Class that holds and handles our SQL data
		//It receives updates etc on the fly via OnProertyChanged events
		//but is just a skeleton basically

		private int id;
		private string bankno;
		private string custno;
		private int actype;
		private decimal balance;
		private decimal intrate;
		private DateTime odate;
		private DateTime cdate;
		private int selectedItem;
		private int selectedIndex;
		private int currentItem;
		private int selectedRow;
		public int Id {
			get { return id; }
			set { id = value; OnPropertyChanged(Id.ToString()); }
		}
		public string BankNo {
			get { return bankno; }
			set { bankno = value; OnPropertyChanged(BankNo.ToString()); }
		}
		public string CustNo {
			get { return custno; }
			set { custno = value; OnPropertyChanged(CustNo.ToString()); }
		}
		public int AcType {
			get { return actype; }
			set { actype = value; OnPropertyChanged(AcType.ToString()); }
		}
		public decimal Balance {
			get { return balance; }
			set {
				balance = value;
				OnPropertyChanged(Id.ToString(Balance.ToString()));
			}
		}
		//public string BalanceAsString {
		//	get { return balanceAsString; }
		//	set {
		//		balanceAsString = value;
		//		Balance = decimal.Parse(balanceAsString, CultureInfo.InvariantCulture);
		//	}
		//}
		public decimal IntRate {
			get { return intrate; }
			set { intrate = value; OnPropertyChanged(IntRate.ToString()); }
		}
		public DateTime ODate {
			get { return odate; }
			set { odate = value; OnPropertyChanged(ODate.ToString()); }
		}
		public DateTime CDate {
			get { return cdate; }
			set { cdate = value; OnPropertyChanged(CDate.ToString()); }
		}

		public int SelectedItem
		{
			get { return selectedItem; }
			set
			{
				selectedItem = value;
				OnPropertyChanged (SelectedItem.ToString ());
			}
		}
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				selectedIndex = value;
				OnPropertyChanged (SelectedIndex.ToString ());
			}
		}
		public int SelectedRow
		{
			get { return selectedRow; }
			set
			{
				selectedRow = value;
				OnPropertyChanged (selectedRow.ToString ());
			}
		}
		public int CurrentItem
		{
			get { return currentItem; }
			set
			{
				currentItem = value;
				OnPropertyChanged (currentItem.ToString ());
			}
		}
		
		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;


		protected void OnPropertyChanged(string PropertyName) {
			if (null != PropertyChanged) {
				PropertyChanged(this,
					new PropertyChangedEventArgs(PropertyName));
			}
		}
		#endregion
	}
}