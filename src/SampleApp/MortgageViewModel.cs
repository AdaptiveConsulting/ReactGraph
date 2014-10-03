namespace SampleApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using PropertyChanged;

    using ReactGraph;
    using ReactGraph.Properties;

    [ImplementPropertyChanged]
    public class MortgageViewModel : INotifyPropertyChanged
    {
        public MortgageViewModel()
        {
            PaymentFrequencies = Enum.GetValues(typeof(PaymentFrequency)).Cast<PaymentFrequency>().ToArray();
        }

        public double Amount { get; set; }

        public double InterestRate { get; set; }

        public double Payments { get; set; }

        public double LoanLength { get; set; }

        public PaymentFrequency PaymentFrequency { get; set; }

        public IEnumerable<PaymentFrequency> PaymentFrequencies { get; private set; }

        public double CalculatePayments(
            double loanAmount,
            double interestRate,
            double loanLength,
            PaymentFrequency paymentFrequency)
        {
            var numberPayments = loanLength * 12;

            interestRate /= 1200.00;
            var monthlyPayment = -((-loanAmount * Math.Pow(1 + interestRate, numberPayments)) / (((Math.Pow((1 + interestRate), numberPayments) - 1) / interestRate))) * 12 / (int)paymentFrequency;
            return monthlyPayment;
        }

        public double CalculateLength(
           double loanAmount,
           double paymentAmount,
           double interestRate,
           PaymentFrequency paymentFrequency)
        {
            interestRate /= 1200.00;
            loanAmount *= (-1);
            var monthlyPayment = paymentAmount / 12 * (int)paymentFrequency;

            return Math.Log(monthlyPayment / (loanAmount * interestRate + monthlyPayment)) / Math.Log(1 + interestRate) / 12;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}