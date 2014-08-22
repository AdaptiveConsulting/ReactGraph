using System;
using System.Diagnostics;

namespace ReactGraph.PerfTests
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Warm up...");
            var data = new Data();
            var algoGraph = new AlgoGraph(data);
            RunWithGraph(algoGraph);
            data = new Data();
            var algoManual = new AlgoManual(data);
            RunManually(algoManual);

            Console.WriteLine("Running tests...");
            Stopwatch sw;
            int gc0;
            const int runs = 3;
            double sumGraphOpsPerSeconds = 0;
            for (int i = 0; i < runs; i++)
            {
                data = new Data();
                algoGraph = new AlgoGraph(data);
                gc0 = GC.CollectionCount(0);
                sw = Stopwatch.StartNew();

                var ops = RunWithGraph(algoGraph);

                var opsPerSecond = ops / sw.Elapsed.TotalSeconds;
                sumGraphOpsPerSeconds += opsPerSecond;
                Console.WriteLine("Run with graph: {0:F1}ops/sec, GC0: {1}", opsPerSecond, GC.CollectionCount(0) - gc0);
            }

            double sumManualOpsPerSeconds = 0;
            for (int i = 0; i < runs; i++)
            {
                data = new Data();
                algoManual = new AlgoManual(data);
                gc0 = GC.CollectionCount(0);
                sw = Stopwatch.StartNew();

                var ops = RunManually(algoManual);

                var opsPerSecond = ops / sw.Elapsed.TotalSeconds;
                sumManualOpsPerSeconds += opsPerSecond;
                Console.WriteLine("Run manually: {0:F1}ops/sec, GC0: {1}", opsPerSecond, GC.CollectionCount(0) - gc0);
            }

            Console.WriteLine();
            Console.WriteLine("Graph is {0:F}x slower than manual code.", sumManualOpsPerSeconds / sumGraphOpsPerSeconds);

            Console.ReadKey();
        }

        static int RunManually(AlgoManual algo)
        {
            const int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                algo.Run();
            }
//            Console.WriteLine(algo.Data._499_1);
            return iterations;
        }

        static int RunWithGraph(AlgoGraph algo)
        {
            const int iterations = 100;
            for (int i = 0; i < iterations; i++)
            {
                algo.Run();
            }
//            Console.WriteLine(algo.Data._499_1);
            return iterations;
        }
    }
}
