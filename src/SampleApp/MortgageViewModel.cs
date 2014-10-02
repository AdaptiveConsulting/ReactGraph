namespace SampleApp
{
    using PropertyChanged;

    using ReactGraph;

    [ImplementPropertyChanged]
    public class MortgageViewModel
    {
        DependencyEngine engine;

        public MortgageViewModel()
        {
            engine = new DependencyEngine();

        }

        public int Amount { get; set; }

        public decimal InterestRate { get; set; }
    }
}