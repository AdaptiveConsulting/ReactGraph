using System;
using System.Linq;
using ReactGraph.Graph;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    using ReactGraph.Visualisation;

    public class GraphTests
    {
        private readonly DirectedGraph<int> sut;

        public GraphTests()
        {
            sut = new DirectedGraph<int>();
        }

        [Fact]
        public void PopulateGraph()
        {
            AddEdge(1, 2);
            AddEdge(1, 3);
            AddEdge(2, 3);

            sut.VerticiesCount.ShouldBe(3);
            sut.EdgesCount.ShouldBe(3);
        }

        void AddEdge(int source, int target)
        {
            sut.AddEdge(source, target, source.ToString(), target.ToString());
        }

        [Fact]
        public void DeapthFirstSeach()
        {
            AddEdge(1, 2);
            AddEdge(1, 3);
            AddEdge(2, 3);
            AddEdge(3, 4);

            var result = sut.DepthFirstSearch(2).Select(v => v.Data);

            result.ShouldBe(new[] { 2, 3, 4 });
        }

        [Fact]
        public void Subgraph()
        {
            AddEdge(1, 2);
            AddEdge(1, 3);
            AddEdge(2, 3);
            AddEdge(3, 4);

            var result = sut.SubGraph(2);

            result.VerticiesCount.ShouldBe(3);
            result.EdgesCount.ShouldBe(2);
        }

        [Fact]
        public void Sources()
        {
            AddEdge(1, 2);
            AddEdge(1, 3);
            AddEdge(2, 3);
            AddEdge(3, 4);
            AddEdge(5, 4);

            var result = sut.FindSources();

            result.Select(v => v.Data).ShouldBe(new[] { 1, 5 });
        }

        [Fact]
        public void TopologicalSort()
        {
            AddEdge(1, 2);
            AddEdge(1, 3);
            AddEdge(2, 3);
            AddEdge(3, 4);
            AddEdge(5, 4);
            AddEdge(5, 4);

            var result = sut.TopologicalSort(1);

            result.Select(v => v.Data).ShouldBe(new[] { 1, 2, 3, 4 });
        }

        [Fact]
        public void TopologicalSort2()
        {
            AddEdge(0, 1);
            AddEdge(0, 3);
            AddEdge(0, 4);
            AddEdge(1, 2);
            AddEdge(1, 3);
            AddEdge(3, 2);
            AddEdge(3, 4);
            AddEdge(2, 4);
            AddEdge(2, 5);
            AddEdge(4, 5);

            var result = sut.TopologicalSort(1);

            result.Select(v => v.Data).ShouldBe(new[] { 1, 3, 2, 4, 5 });
        }

        [Fact]
        public void CyclicGraph()
        {
            var mortgage = new Mortgate
                               {
                                   InterestRate = 5.5,
                                   LoanAmount = 150000,
                                   LoanLength = 15,
                                   PaymentFrequency = PaymentFrequency.Monthly
                               };
            var engine = new DependencyEngine();

            engine.Assign(() => mortgage.PaymentAmount)
                .From(() => CalculatePayments(
                        mortgage.LoanAmount,
                        mortgage.InterestRate,
                        mortgage.LoanLength,
                        mortgage.PaymentFrequency),
                        ex => { });

            engine.Assign(() => mortgage.LoanLength)
                .From(() => CalculateLength(
                        mortgage.LoanAmount,
                        mortgage.PaymentAmount,
                        mortgage.InterestRate,
                        mortgage.PaymentFrequency),
                        ex => { });

            var dotFormat = engine.ToDotFormat();
            Console.WriteLine(dotFormat);

            engine.ValueHasChanged(mortgage, "PaymentFrequency");

            mortgage.PaymentAmount.ShouldBe(1225, 1);
            engine.ToDotFormat().ShouldBe(dotFormat);
        }

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

        public class Mortgate
        {
            public double LoanAmount { get; set; }

            public double LoanLength { get; set; }

            public double InterestRate { get; set; }

            public double PaymentAmount { get; set; }

            public PaymentFrequency PaymentFrequency { get; set; }
        }

        public enum PaymentFrequency
        {
            Weekly = 52,
            Fortnightly = 26,
            Monthly = 12,
            Quaterly = 4
        }

        [Fact]
        public void CycleDetection()
        {
            // cycle 1
            AddEdge(0, 1);
            AddEdge(1, 2);
            AddEdge(2, 0);

            AddEdge(2, 3);

            // cycle 2
            AddEdge(3, 4);
            AddEdge(4, 3);

            var detectedCyles = sut.DetectCyles().ToList();

            foreach (var cyle in detectedCyles)
            {
                Console.WriteLine("Cycle: " + string.Join(", ", cyle.Select(v => v.Data.ToString())));
            }

            detectedCyles.Count().ShouldBe(2);
        }
    }
}
