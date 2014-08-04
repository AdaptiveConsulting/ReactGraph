namespace ReactGraph.Tests.TestObjects
{
    class MortgateCalculatorViewModel : NotifyPropertyChanged
    {
        private ScheduleViewModel paymentSchedule;
        bool canApply;

        public void RegeneratePaymentSchedule(bool hasValidationError)
        {
            PaymentSchedule = new ScheduleViewModel
            {
                HasValidationError = hasValidationError
            };
        }

        public ScheduleViewModel PaymentSchedule
        {
            get { return paymentSchedule; }
            private set
            {
                if (Equals(value, paymentSchedule)) return;
                paymentSchedule = value;
                OnPropertyChanged("PaymentSchedule");
            }
        }

        public bool CanApply
        {
            get { return canApply; }
            private set { canApply = value; }
        }
    }
}
