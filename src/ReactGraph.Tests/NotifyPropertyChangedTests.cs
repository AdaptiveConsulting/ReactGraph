using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class NotifyPropertyChangedTests
    {
        [Fact]
        public void TriggersOnPropertyChanged()
        {
            var engine = new DependencyEngine();
            var notifies = new Notifies
            {
                TaxPercentage = 20
            };
            engine.Bind(() => notifies.Total, () => (int) (notifies.SubTotal * (1m + (notifies.TaxPercentage / 100m))));

            notifies.SubTotal = 100;
            notifies.Total.ShouldBe(120);
        }

        class Notifies : NotifyPropertyChanged
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
}