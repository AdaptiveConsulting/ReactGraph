namespace SampleApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using PropertyChanged;
    using ReactGraph.Properties;

    [ImplementPropertyChanged]
    public class MortgageViewModel : NotifyPropertyChanged
    {
        public MortgageViewModel()
        {
            PaymentFrequencies = Enum.GetValues(typeof(PaymentFrequency)).Cast<PaymentFrequency>().ToArray();
        }

        public double? Amount { get; set; }

        public double? InterestRate { get; set; }

        public double? Payments { get; set; }

        public double? LoanLength { get; set; }

        public PaymentFrequency? PaymentFrequency { get; set; }

        public IEnumerable<PaymentFrequency> PaymentFrequencies { get; private set; }
    }
}