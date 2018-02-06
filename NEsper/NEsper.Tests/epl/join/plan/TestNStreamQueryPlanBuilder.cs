///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.@base;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestNStreamQueryPlanBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private EventType[] _typesPerStream;
        private QueryGraph _queryGraph;
        private DependencyGraph _dependencyGraph;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _typesPerStream = new EventType[]
            {
                _container.Resolve<EventAdapterService>().AddBeanType(typeof (SupportBean_S0).FullName, typeof (SupportBean_S0),
                                                               true, true, true),
                _container.Resolve<EventAdapterService>().AddBeanType(typeof (SupportBean_S1).FullName, typeof (SupportBean_S1),
                                                               true, true, true),
                _container.Resolve<EventAdapterService>().AddBeanType(typeof (SupportBean_S2).FullName, typeof (SupportBean_S2),
                                                               true, true, true),
                _container.Resolve<EventAdapterService>().AddBeanType(typeof (SupportBean_S3).FullName, typeof (SupportBean_S3),
                                                               true, true, true),
                _container.Resolve<EventAdapterService>().AddBeanType(typeof (SupportBean_S4).FullName, typeof (SupportBean_S4),
                                                               true, true, true)
            };

            _queryGraph = new QueryGraph(5, null, false);
            _queryGraph.AddStrictEquals(0, "P00", Make(0, "P00"), 1, "P10", Make(1, "P10"));
            _queryGraph.AddStrictEquals(0, "P01", Make(0, "P01"), 2, "P20", Make(2, "P20"));
            _queryGraph.AddStrictEquals(4, "P40", Make(4, "P40"), 3, "P30", Make(3, "P30"));
            _queryGraph.AddStrictEquals(4, "P41", Make(4, "P41"), 3, "P31", Make(3, "P31"));
            _queryGraph.AddStrictEquals(4, "P42", Make(4, "P42"), 2, "P21", Make(2, "P21"));

            _dependencyGraph = new DependencyGraph(5, false);
        }

        [Test]
        public void TestBuild()
        {
            QueryPlan plan = NStreamQueryPlanBuilder.Build(_queryGraph, _typesPerStream, new HistoricalViewableDesc(6), _dependencyGraph, null, false, new string[_queryGraph.NumStreams][][], new TableMetadata[_queryGraph.NumStreams]);

            Log.Debug(".testBuild plan=" + plan);
        }

        [Test]
        public void TestBuildDefaultNestingOrder()
        {
            int[] result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 0);
            Assert.IsTrue(Collections.AreEqual(result, new int[] {1, 2, 3}));

            result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 1);
            Assert.IsTrue(Collections.AreEqual(result, new int[] {0, 2, 3}));

            result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 2);
            Assert.IsTrue(Collections.AreEqual(result, new int[] {0, 1, 3}));

            result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 3);
            Assert.IsTrue(Collections.AreEqual(result, new int[] {0, 1, 2}));
        }

        [Test]
        public void TestComputeBestPath()
        {
            NStreamQueryPlanBuilder.BestChainResult bestChain = NStreamQueryPlanBuilder.ComputeBestPath(
                0, _queryGraph, _dependencyGraph);
            Assert.AreEqual(3, bestChain.Depth);
            Assert.IsTrue(Collections.AreEqual(bestChain.Chain, new int[] {2, 4, 3, 1}));

            bestChain = NStreamQueryPlanBuilder.ComputeBestPath(3, _queryGraph, _dependencyGraph);
            Assert.AreEqual(4, bestChain.Depth);
            Assert.IsTrue(Collections.AreEqual(bestChain.Chain, new int[] {4, 2, 0, 1}));

            // try a stream that is not connected in any way
            _queryGraph = new QueryGraph(6, null, false);
            bestChain = NStreamQueryPlanBuilder.ComputeBestPath(5, _queryGraph, _dependencyGraph);
            Log.Debug(".testComputeBestPath bestChain=" + bestChain);
            Assert.AreEqual(0, bestChain.Depth);
            Assert.IsTrue(Collections.AreEqual(bestChain.Chain, new int[] {0, 1, 2, 3, 4}));
        }

        [Test]
        public void TestComputeNavigableDepth()
        {
            _queryGraph.AddStrictEquals(3, "P30", null, 2, "P20", null);
            _queryGraph.AddStrictEquals(2, "P30", null, 1, "P20", null);

            int depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(0, new int[] {1, 2, 3, 4}, _queryGraph);
            Assert.AreEqual(4, depth);

            depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(0, new int[] {4, 2, 3, 1}, _queryGraph);
            Assert.AreEqual(0, depth);

            depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(4, new int[] {3, 2, 1, 0}, _queryGraph);
            Assert.AreEqual(4, depth);

            depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(1, new int[] {0, 3, 4, 2}, _queryGraph);
            Assert.AreEqual(1, depth);
        }

        [Test]
        public void TestCreateStreamPlan()
        {
            QueryPlanIndex[] indexes = QueryPlanIndexBuilder.BuildIndexSpec(_queryGraph, _typesPerStream, new string[_queryGraph.NumStreams][][]);
            for (int i = 0; i < indexes.Length; i++)
            {
                Log.Debug(".testCreateStreamPlan index " + i + " = " + indexes[i]);
            }

            QueryPlanNode plan = NStreamQueryPlanBuilder.CreateStreamPlan(
                0, new int[] {2, 4, 3, 1}, _queryGraph,
                indexes, _typesPerStream, new bool[5], null, 
                new TableMetadata[_queryGraph.NumStreams]);

            Log.Debug(".testCreateStreamPlan plan=" + plan);

            Assert.IsTrue(plan is NestedIterationNode);
            var nested = (NestedIterationNode) plan;
            var tableLookupSpec = (TableLookupNode) nested.ChildNodes[0];

            // Check lookup strategy for first lookup
            var lookupStrategySpec = (IndexedTableLookupPlanSingle) tableLookupSpec.LookupStrategySpec;
            Assert.AreEqual("P01", ((QueryGraphValueEntryHashKeyedProp) lookupStrategySpec.HashKey).KeyProperty);
            Assert.AreEqual(0, lookupStrategySpec.LookupStream);
            Assert.AreEqual(2, lookupStrategySpec.IndexedStream);
            Assert.NotNull(lookupStrategySpec.IndexNum);

            // Check lookup strategy for last lookup
            tableLookupSpec = (TableLookupNode) nested.ChildNodes[3];
            var unkeyedSpecScan = (FullTableScanLookupPlan) tableLookupSpec.LookupStrategySpec;
            Assert.AreEqual(1, unkeyedSpecScan.IndexedStream);
            Assert.NotNull(unkeyedSpecScan.IndexNum);
        }

        [Test]
        public void TestIsDependencySatisfied()
        {
            var graph = new DependencyGraph(3, false);
            graph.AddDependency(1, 0);
            graph.AddDependency(2, 0);

            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(0, new int[] {1, 2}, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] {0, 2}, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(2, new int[] {0, 1}, graph));

            graph = new DependencyGraph(5, false);
            graph.AddDependency(4, 1);
            graph.AddDependency(4, 2);
            graph.AddDependency(2, 0);

            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(0, new int[] {1, 2, 3, 4}, graph));
            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] {0, 2, 3, 4}, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] {2, 0, 3, 4}, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] {4, 0, 3, 2}, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(3, new int[] {4, 0, 1, 2}, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(2, new int[] {3, 1, 4, 0}, graph));
            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(3, new int[] {1, 0, 2, 4}, graph));
        }

        private ExprIdentNode Make(int stream, string p)
        {
            return new ExprIdentNodeImpl(_typesPerStream[stream], p, stream);
        }
    }
}