﻿using System;
using System.Collections.Generic;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ApiTest
    {
        DependencyEngine engine;

        public ApiTest()
        {
            engine = new DependencyEngine();
        }

        [Fact]
        public void FluentApiTest()
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
            var engine = new DependencyEngine();

            engine.Expr(() => Addition(vertex6.Value)).Bind(() => vertex0.Value, e => { });
            engine.Expr(() => Addition(vertex0.Value, vertex5.Value, vertex4.Value)).Bind(() => vertex1.Value, e => { });
            engine.Expr(() => Addition(vertex0.Value, vertex1.Value)).Bind(() => vertex2.Value, e => { });
            engine.Expr(() => Addition(vertex1.Value, vertex2.Value)).Bind(() => vertex3.Value, e => { });
            engine.Expr(() => Addition(vertex4.Value)).Bind(() => vertex5.Value, e => { });
            engine.Expr(() => Addition(vertex2.Value)).Bind(() => vertex7.Value, e => { });
            engine.Expr(() => Addition(vertex2.Value)).Bind(() => vertex8.Value, e => { });
                
            Console.WriteLine(engine.ToString());

            // We set the value to 2, then tell the engine the value has changed
            vertex0.Value = 2;
            engine.ValueHasChanged(vertex0, "Value");
            vertex1.Value.ShouldBe(2);
            vertex2.Value.ShouldBe(4);
            vertex3.Value.ShouldBe(6);
        }

        [Fact]
        public void ThrowsForTypeMismatch()
        {
            var a = new SinglePropertyType();
            var b = new SinglePropertyType();

            var ex = Should.Throw<ArgumentException>(() => 
                engine.Expr(() => (object) a.Value)
                      .Bind(() => b.Value, e => { }));

            ex.Message.ShouldBe("Cannot bind target of type System.Int32 to source of type System.Object");
        }

        [Fact]
        public void ExceptionsArePassedToHandlers()
        {
            var a = new SinglePropertyType();
            var b = new SinglePropertyType();

            Exception ex;
            engine.Expr(() => ThrowsInvalidOperationException(a.Value))
                  .Bind(() => b.Value, e => ex = e);

            ex = null;
            a.Value = 2;
            engine.ValueHasChanged(a, "Value");

            ex.ShouldBeOfType<InvalidOperationException>();
        }

        [Fact]
        public void NodeWhichThrowsExceptionHasSubgraphRemovedFromCurrentExecution()
        {
            /*
             *      A
             *     / \
             *    B   Throws
             *    |   |
             *    C   Skipped
             *   / \ /
             *  E   D
             */

            var a = new SinglePropertyType();
            var b = new SinglePropertyType();
            var c = new SinglePropertyType();
            var d = new SinglePropertyType();
            var e = new SinglePropertyType();
            var throws = new SinglePropertyType();
            var skipped = new SinglePropertyType();

            engine.Expr(() => a.Value).Bind(() => b.Value, ex => { });
            engine.Expr(() => b.Value).Bind(() => c.Value, ex => { });
            engine.Expr(() => ThrowsInvalidOperationException(a.Value)).Bind(() => throws.Value, ex => { });
            engine.Expr(() => throws.Value).Bind(() => skipped.Value, ex => { });
            engine.Expr(() => c.Value + skipped.Value).Bind(() => d.Value, ex => { });
            engine.Expr(() => c.Value).Bind(() => e.Value, ex => { });

            a.Value = 2;
            engine.ValueHasChanged(a, "Value");

            skipped.ValueSet.ShouldBe(0);
            d.ValueSet.ShouldBe(0);
            c.Value.ShouldBe(2);
            e.Value.ShouldBe(2);
        }

        [Fact]
        public void Instrumentation()
        {
            /*
             *      A
             *     / \
             *    B   Throws
             *    |   |
             *    C   Skipped
             *   / \ /
             *  E   D
             */
            var instrumentation = new CustomInstrumentation();
            engine = new DependencyEngine(instrumentation);

            var a = new SinglePropertyType();
            var b = new SinglePropertyType();
            var c = new SinglePropertyType();
            var d = new SinglePropertyType();
            var e = new SinglePropertyType();
            var throws = new SinglePropertyType();
            var skipped = new SinglePropertyType();

            engine.Expr(() => a.Value).Bind(() => b.Value, ex => { });
            engine.Expr(() => b.Value).Bind(() => c.Value, ex => { });
            engine.Expr(() => ThrowsInvalidOperationException(a.Value)).Bind(() => throws.Value, ex => { });
            engine.Expr(() => throws.Value).Bind(() => skipped.Value, ex => { });
            engine.Expr(() => c.Value + skipped.Value).Bind(() => d.Value, ex => { });
            engine.Expr(() => c.Value).Bind(() => e.Value, ex => { });

            a.Value = 2;
            engine.ValueHasChanged(a, "Value");

            Console.WriteLine(engine.ToString());

            instrumentation.WalkIndexStart.ShouldBe(1);
            instrumentation.WalkIndexEnd.ShouldBe(1);
            instrumentation.NodeEvaluations.Count.ShouldBe(8);
        }

        class CustomInstrumentation : IEngineInstrumentation
        {
            public CustomInstrumentation()
            {
                NodeEvaluations = new List<NodeEval>();
            }

            public long WalkIndexStart { get; private set; }
            public long WalkIndexEnd { get; private set; }
            public List<NodeEval> NodeEvaluations { get; private set; }

            public void OnDependencyWalkStart(long walkIndex, string sourceProperty)
            {
                WalkIndexStart = walkIndex;
            }

            public void OnNodeEvaluated(long walkIndex, string updatedNode, ReevaluationResult result)
            {
                NodeEvaluations.Add(new NodeEval(walkIndex, updatedNode, result));
            }

            public void OnDepdendencyWalkEnd(long walkIndex)
            {
                WalkIndexEnd = walkIndex;
            }
        }
        
        class NodeEval
        {
            public long WalkIndex { get; set; }
            public string UpdatedNode { get; set; }
            public ReevaluationResult Result { get; set; }

            public NodeEval(long walkIndex, string updatedNode, ReevaluationResult result)
            {
                WalkIndex = walkIndex;
                UpdatedNode = updatedNode;
                Result = result;
            }
        }

        int ThrowsInvalidOperationException(int value)
        {
            throw new InvalidOperationException();
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
            int value;

            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    ValueSet++;
                }
            }

            public int ValueSet { get; private set; }
        }
    }
}
