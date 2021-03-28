using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPages.ViewModels
{
	public  class BankAccountViewModel: INotifyPropertyChanged
	{
        private int id;
        private string bankno;
        private string custno;
        private int actype;
        private decimal balance;
        private decimal intrate;
        private DateTime odate;
        private DateTime cdate;
		private int _selectedItem;

        private readonly ObservableCollection<BankAccount> _items = new ObservableCollection<BankAccount> ();
        public ObservableCollection<BankAccount> Items { get { return new ObservableCollection<BankAccount> (this._items); } }

		public BankAccountViewModel ()
		{
			// Load data here ??
			int x = 0;
			Items i = new Items ();
		}
		public int SelectedItem
		{
			get { return this._selectedItem; }
			set
			{
				this._selectedItem = value;
				this.OnPropertyChanged ("SelectedItem");
				//				this.ExtraStuff = new ExtraInformation (value);
			}
		}
		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged (Id.ToString ()); }
		}
		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged (BankNo.ToString ()); }
		}
		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged (CustNo.ToString ()); }
		}
		public int AcType
		{
			get { return actype; }
			set { actype = value; OnPropertyChanged (AcType.ToString ()); }
		}
		public decimal Balance
		{
			get { return balance; }
			set
			{
				balance = value;
				OnPropertyChanged (Id.ToString (Balance.ToString ()));
			}
		}
		//public string BalanceAsString {
		//	get { return balanceAsString; }
		//	set {
		//		balanceAsString = value;
		//		Balance = decimal.Parse(balanceAsString, CultureInfo.InvariantCulture);
		//	}
		//}
		public decimal IntRate
		{
			get { return intrate; }
			set { intrate = value; OnPropertyChanged (IntRate.ToString ()); }
		}
		public DateTime ODate
		{
			get { return odate; }
			set { odate = value; OnPropertyChanged (ODate.ToString ()); }
		}
		public DateTime CDate
		{
			get { return cdate; }
			set { cdate = value; OnPropertyChanged (CDate.ToString ()); }
		}




        #region Inotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged (string name)
        {
            if (null != this.PropertyChanged)
            {
                this.PropertyChanged (this, new PropertyChangedEventArgs (name));
            }
        }
     #endregion Inotify

    }
}
