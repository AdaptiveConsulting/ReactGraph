using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ApiTest
    {
        [Fact]
        public void BasicApiTest()
        {
            /* Each vertex is a node in this graph
             * 
             *       5<-----4
             *        \    /
             *         \  /
             *          ▼▼
             *      6   1------->3
             *      |  ^  \     ^
             *      | /    \   /
             *      ▼/      ▼ /
             *      0------->2----->7
             *               \
             *                \
             *                 ▼
             *                  8
             */
            var vertex0 = new SinglePropertyType();
            var vertex1 = new SinglePropertyType();
            var vertex2 = new SinglePropertyType();
            var vertex3 = new SinglePropertyType();
            var vertex4 = new SinglePropertyType();
            var vertex5 = new SinglePropertyType();
            var vertex6 = new SinglePropertyType();
            var vertex7 = new SinglePropertyType();
            var vertex8 = new SinglePropertyType();
            var lookup = new Dictionary<object, string>
            {
                {vertex0, "vertex0"},
                {vertex1, "vertex1"},
                {vertex2, "vertex2"},
                {vertex3, "vertex3"},
                {vertex4, "vertex4"},
                {vertex5, "vertex5"},
                {vertex6, "vertex6"},
                {vertex7, "vertex7"},
                {vertex8, "vertex8"}
            };
            var engine = new DependencyEngine();

            engine.Bind(() => vertex0.Value, () => Addition(vertex6.Value));
            engine.Bind(() => vertex1.Value, () => Addition(vertex0.Value, vertex5.Value, vertex4.Value));
            engine.Bind(() => vertex2.Value, () => Addition(vertex0.Value, vertex1.Value));
            engine.Bind(() => vertex3.Value, () => Addition(vertex1.Value, vertex2.Value));
            engine.Bind(() => vertex5.Value, () => Addition(vertex4.Value));
            engine.Bind(() => vertex7.Value, () => Addition(vertex2.Value));
            engine.Bind(() => vertex8.Value, () => Addition(vertex2.Value));

            var changes = new StringBuilder();
            engine.SettingValue += (o, s) =>
            {
                var s1 = lookup[o];
                if (s1.EndsWith("1") || s1.EndsWith("2") || s1.EndsWith("3"))
                    changes.AppendLine(s1 + "." + s);
            };

            // We set the value to 2, then tell the engine the value has changed
            vertex0.Value = 2;
            engine.PropertyChanged(vertex0, "Value");
            vertex1.Value.ShouldBe(2);
            vertex2.Value.ShouldBe(4);
            vertex3.Value.ShouldBe(6);
            changes.ToString().Trim().ShouldBe(
@"vertex1.Value
vertex2.Value
vertex3.Value");
        }

        private int Addition(int i1, int i2, int i3)
        {
            return i1 + i2 + i3;
        }

        private int Addition(int i1, int i2)
        {
            return i1 + i2;
        }

        private int Addition(int i1)
        {
            return i1;
        }

        private class SinglePropertyType
        {
            public int Value { get; set; }
        }
    }
}
