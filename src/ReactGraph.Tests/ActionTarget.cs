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
        public void ExpressionCanTriggerAction()
        {
            var simple = new SimpleWithNotification();
            var actionInvoked = 0;

            engine.Expr(() => simple.Value).Action(v => actionInvoked++, "actionInvoked = true", ex => { });
            actionInvoked.ShouldBe(0);

            simple.Value = 2;

            actionInvoked.ShouldBe(1);
        }

        [Fact]
        public void ExpressionCanTriggerAction2()
        {
            var simple = new SimpleWithNotification();
            var actionInvoked = 0;
            Action action = () => actionInvoked++;

            engine.Expr(() => simple.Value).Action(v => action(), ex => { });
            actionInvoked.ShouldBe(0);

            simple.Value = 2;

            actionInvoked.ShouldBe(1);
            Console.WriteLine(engine.ToString());
        }
    }
}