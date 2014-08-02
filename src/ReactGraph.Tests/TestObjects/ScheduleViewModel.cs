namespace ReactGraph.Tests.TestObjects
{
    internal class ScheduleViewModel : NotifyPropertyChanged
    {
        private bool hasValidationError;

        public bool HasValidationError
        {
            get { return hasValidationError; }
            set
            {
                if (value.Equals(hasValidationError)) return;
                hasValidationError = value;
                OnPropertyChanged("HasValidationError");
            }
        }
    }
}