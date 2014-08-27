using System;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ActionTarget
    {
        readonly DependencyEngine engine;

        public ActionTarget()
        {
            engine = new DependencyEngine();
        }

        [Fact]
        public void ExpressionCanTriggerAction2()
        {
            var simple = new SimpleWithNotification();
            var actionInvoked = 0;
            Action action = () => actionInvoked++;

            engine.When(() => simple.Value).Do(v => action(), ex => { });
            actionInvoked.ShouldBe(0);

            simple.Value = 2;

            actionInvoked.ShouldBe(1);
            Console.WriteLine(engine.ToString());
        }
    }
}