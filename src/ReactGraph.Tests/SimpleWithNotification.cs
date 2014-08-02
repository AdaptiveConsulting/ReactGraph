using ReactGraph.Tests.TestObjects;

namespace ReactGraph.Tests
{
    class SimpleWithNotification : NotifyPropertyChanged
    {
        private int value;

        public int Value
        {
            get { return value; }
            set
            {
                if (value == this.value) return;
                this.value = value;
                OnPropertyChanged("Value");
            }
        }
    }
}