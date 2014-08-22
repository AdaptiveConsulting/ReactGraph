using System;
using System.IO;
using System.Windows;
using PropertyChanged;
using ReactGraph;
using ReactGraph.Visualisation;

namespace SampleApp
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        readonly DependencyEngine engine;
        int a;
        int b;
        int c;
        int d;

        public MainViewModel()
        {
            engine = new DependencyEngine();

            engine.Expr(() => Add(A, B)).Bind(() => C, e => {});
            engine.Expr(() => Multiply(C, D)).Bind(() => E, e => { });

            A = 2;
            B = 3;
            D = 5;
            engine.LogTransitionsInDotFormat(Path.Combine(Environment.CurrentDirectory, "Transitions.log"));
        }

        int Multiply(int i, int j)
        {
            return i*j;
        }

        private int Add(int i, int j)
        {
            throw new Exception("Boom");
            return i + j;
        }

        public int A
        {
            get { return a; }
            set
            {
                a = value;
                engine.ValueHasChanged(this, "A");
            }
        }

        public int B
        {
            get { return b; }
            set
            {
                b = value;
                engine.ValueHasChanged(this, "B");
            }
        }

        public int C
        {
            get { return c; }
            set
            {
                c = value;
                engine.ValueHasChanged(this, "C");
            }
        }

        public int D
        {
            get { return d; }
            set
            {
                d = value;
                engine.ValueHasChanged(this, "D");
            }
        }

        public int E { get; set; }
    }
}
