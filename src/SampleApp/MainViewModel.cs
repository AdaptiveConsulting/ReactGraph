using System;
using System.IO;
using ReactGraph;
using ReactGraph.Visualisation;

namespace SampleApp
{
    public class MainViewModel : NotifyPropertyChanged
    {
        readonly DependencyEngine mortgageEngine;
        readonly DependencyEngine graphOptionsEngine;
        readonly DependencyEngine engine;

        public MainViewModel()
        {
            Mortgage = new MortgageViewModel();
            engine = new DependencyEngine();

            engine.Assign(() => C).From(() => Add(A, B), e => { });
            engine.Assign(() => E).From(() => Multiply(C, D), e => { });

            A = 2;
            B = 3;
            D = 5;
            engine.LogTransitionsInDotFormat(Path.Combine(Environment.CurrentDirectory, "Transitions.log"));

            mortgageEngine = new DependencyEngine();
            mortgageEngine
                .Assign(() => Mortgage.Payments)
                .From(() => MortgageCalculations.CalculatePayments(
                        Mortgage.Amount,
                        Mortgage.InterestRate,
                        Mortgage.LoanLength,
                        Mortgage.PaymentFrequency),
                        ex => { });

            mortgageEngine
                .Assign(() => Mortgage.LoanLength)
                .From(() => MortgageCalculations.CalculateLength(
                        Mortgage.Amount,
                        Mortgage.Payments,
                        Mortgage.InterestRate,
                        Mortgage.PaymentFrequency),
                        ex => { });

            graphOptionsEngine = new DependencyEngine();
            graphOptionsEngine
                .Assign(() => MortgageGraph)
                .From(() => CreateMortgageGraph(ShowRoots, ShowFormulas), ex => { });
            MortgageGraph = CreateMortgageGraph(ShowRoots, ShowFormulas);
        }

        string CreateMortgageGraph(bool showRoots, bool showFormulas)
        {
            return mortgageEngine.ToDotFormat(options: new VisualisationOptions
            {
                ShowFormulas = showFormulas,
                ShowRoot = showRoots
            });
        }

        public bool ShowFormulas { get; set; }

        public bool ShowRoots { get; set; }

        static int Multiply(int i, int j)
        {
            return i*j;
        }

        private static int Add(int i, int j)
        {
            throw new Exception("Boom");
        }

        public int A { get; set; }

        public int B { get; set; }

        public int C { get; set; }

        public int D { get; set; }

        public int E { get; set; }

        public MortgageViewModel Mortgage { get; private set; }

        public string MortgageGraph { get; private set; }
    }
}
