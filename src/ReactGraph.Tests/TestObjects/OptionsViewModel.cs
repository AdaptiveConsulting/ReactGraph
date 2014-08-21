using System.Collections.ObjectModel;

namespace ReactGraph.Tests.TestObjects
{
    class OptionsViewModel : NotifyPropertyChanged
    {
        ObservableCollection<string> options;
        string selectedOption;

        public ObservableCollection<string> Options
        {
            get { return options; }
            set
            {
                if (Equals(value, options)) return;
                options = value;
                OnPropertyChanged("Options");
            }
        }

        public string SelectedOption
        {
            get { return selectedOption; }
            set
            {
                if (value == selectedOption) return;
                selectedOption = value;
                OnPropertyChanged("SelectedOption");
            }
        }
    }
}