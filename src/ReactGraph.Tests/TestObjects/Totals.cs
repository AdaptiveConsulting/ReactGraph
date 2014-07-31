namespace ReactGraph.Tests.TestObjects
{
    class Totals : NotifyPropertyChanged
    {
        private int total;
        private int taxPercentage;
        private int subTotal;

        public int SubTotal
        {
            get { return subTotal; }
            set
            {
                if (value == subTotal) return;
                subTotal = value;
                OnPropertyChanged("SubTotal");
            }
        }

        public int TaxPercentage
        {
            get { return taxPercentage; }
            set
            {
                if (value == taxPercentage) return;
                taxPercentage = value;
                OnPropertyChanged("TaxPercentage");
            }
        }

        public int Total
        {
            get { return total; }
            set
            {
                if (value == total) return;
                total = value;
                OnPropertyChanged("Total");
            }
        }
    }
}