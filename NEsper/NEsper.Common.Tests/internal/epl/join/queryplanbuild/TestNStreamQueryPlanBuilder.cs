///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using NUnit.Framework;

using static com.espertech.esper.common.@internal.supportunit.util.SupportExprNodeFactory;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    [TestFixture]
    public class TestNStreamQueryPlanBuilder : AbstractCommonTest
    {
        private EventType[] typesPerStream;
        private QueryGraphForge queryGraph;
        private DependencyGraph dependencyGraph;

        [SetUp]
        public void SetUp()
        {
            typesPerStream = new EventType[]{
                    supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S0)),
                    supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S1)),
                    supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S2)),
                    supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S3)),
                    supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S4))
            };

            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "P00", Make(0, "P00"), 1, "P10", Make(1, "P10"));
            queryGraph.AddStrictEquals(0, "P01", Make(0, "P01"), 2, "P20", Make(2, "P20"));
            queryGraph.AddStrictEquals(4, "P40", Make(4, "P40"), 3, "P30", Make(3, "P30"));
            queryGraph.AddStrictEquals(4, "P41", Make(4, "P41"), 3, "P31", Make(3, "P31"));
            queryGraph.AddStrictEquals(4, "P42", Make(4, "P42"), 2, "P21", Make(2, "P21"));

            dependencyGraph = new DependencyGraph(5, false);
        }

        [Test]
        public void TestBuild()
        {
            var plan = NStreamQueryPlanBuilder.Build(
                queryGraph,
                typesPerStream,
                new HistoricalViewableDesc(6),
                dependencyGraph,
                null,
                false,
                new string[queryGraph.NumStreams][][],
                new TableMetaData[queryGraph.NumStreams],
                new StreamJoinAnalysisResultCompileTime(5),
                null,
                SerdeCompileTimeResolverNonHA.INSTANCE);

            log.Debug(".testBuild plan=" + plan);
        }

        [Test]
        public void TestCreateStreamPlan()
        {
            QueryPlanIndexForge[] indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, typesPerStream, new string[queryGraph.NumStreams][][]);
            for (int i = 0; i < indexes.Length; i++)
            {
                log.Debug(".testCreateStreamPlan index " + i + " = " + indexes[i]);
            }

            var plan = NStreamQueryPlanBuilder.CreateStreamPlan(
                0,
                new int[] {2, 4, 3, 1},
                queryGraph,
                indexes,
                typesPerStream,
                new bool[5],
                null,
                new TableMetaData[queryGraph.NumStreams],
                new StreamJoinAnalysisResultCompileTime(5),
                null,
                SerdeCompileTimeResolverNonHA.INSTANCE);

            log.Debug(".testCreateStreamPlan plan=" + plan);

            Assert.IsTrue(plan.Forge is NestedIterationNodeForge);
            NestedIterationNodeForge nested = (NestedIterationNodeForge) plan.Forge;
            TableLookupNodeForge tableLookupSpec = (TableLookupNodeForge) nested.ChildNodes[0];

            // Check lookup strategy for first lookup
            IndexedTableLookupPlanHashedOnlyForge lookupStrategySpec = (IndexedTableLookupPlanHashedOnlyForge) tableLookupSpec.LookupStrategySpec;
            Assert.AreEqual("P01", ((ExprIdentNode) (lookupStrategySpec.HashKeys[0]).KeyExpr).ResolvedPropertyName);
            Assert.AreEqual(0, lookupStrategySpec.LookupStream);
            Assert.AreEqual(2, lookupStrategySpec.IndexedStream);
            Assert.IsNotNull(lookupStrategySpec.IndexNum);

            // Check lookup strategy for last lookup
            tableLookupSpec = (TableLookupNodeForge) nested.ChildNodes[3];
            FullTableScanLookupPlanForge unkeyedSpecScan = (FullTableScanLookupPlanForge) tableLookupSpec.LookupStrategySpec;
            Assert.AreEqual(1, unkeyedSpecScan.IndexedStream);
            Assert.IsNotNull(unkeyedSpecScan.IndexNum);
        }

        [Test]
        public void TestComputeBestPath()
        {
            NStreamQueryPlanBuilder.BestChainResult bestChain = NStreamQueryPlanBuilder.ComputeBestPath(0, queryGraph, dependencyGraph);
            Assert.AreEqual(3, bestChain.Depth);
            Assert.IsTrue(Arrays.AreEqual(bestChain.Chain, new int[] { 2, 4, 3, 1 }));

            bestChain = NStreamQueryPlanBuilder.ComputeBestPath(3, queryGraph, dependencyGraph);
            Assert.AreEqual(4, bestChain.Depth);
            Assert.IsTrue(Arrays.AreEqual(bestChain.Chain, new int[] { 4, 2, 0, 1 }));

            // try a stream that is not connected in any way
            queryGraph = new QueryGraphForge(6, null, false);
            bestChain = NStreamQueryPlanBuilder.ComputeBestPath(5, queryGraph, dependencyGraph);
            log.Debug(".testComputeBestPath bestChain=" + bestChain);
            Assert.AreEqual(0, bestChain.Depth);
            Assert.IsTrue(Arrays.AreEqual(bestChain.Chain, new int[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void TestComputeNavigableDepth()
        {
            ExprIdentNode fake = supportExprNodeFactory.MakeIdentNode("TheString", "s0");
            queryGraph.AddStrictEquals(3, "P30", fake, 2, "P20", fake);
            queryGraph.AddStrictEquals(2, "P30", fake, 1, "P20", fake);

            int depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(0, new int[] { 1, 2, 3, 4 }, queryGraph);
            Assert.AreEqual(4, depth);

            depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(0, new int[] { 4, 2, 3, 1 }, queryGraph);
            Assert.AreEqual(0, depth);

            depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(4, new int[] { 3, 2, 1, 0 }, queryGraph);
            Assert.AreEqual(4, depth);

            depth = NStreamQueryPlanBuilder.ComputeNavigableDepth(1, new int[] { 0, 3, 4, 2 }, queryGraph);
            Assert.AreEqual(1, depth);
        }

        [Test]
        public void TestBuildDefaultNestingOrder()
        {
            int[] result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 0);
            Assert.IsTrue(Arrays.AreEqual(result, new int[] { 1, 2, 3 }));

            result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 1);
            Assert.IsTrue(Arrays.AreEqual(result, new int[] { 0, 2, 3 }));

            result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 2);
            Assert.IsTrue(Arrays.AreEqual(result, new int[] { 0, 1, 3 }));

            result = NStreamQueryPlanBuilder.BuildDefaultNestingOrder(4, 3);
            Assert.IsTrue(Arrays.AreEqual(result, new int[] { 0, 1, 2 }));
        }

        [Test]
        public void TestIsDependencySatisfied()
        {
            DependencyGraph graph = new DependencyGraph(3, false);
            graph.AddDependency(1, 0);
            graph.AddDependency(2, 0);

            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(0, new int[] { 1, 2 }, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] { 0, 2 }, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(2, new int[] { 0, 1 }, graph));

            graph = new DependencyGraph(5, false);
            graph.AddDependency(4, 1);
            graph.AddDependency(4, 2);
            graph.AddDependency(2, 0);

            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(0, new int[] { 1, 2, 3, 4 }, graph));
            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] { 0, 2, 3, 4 }, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] { 2, 0, 3, 4 }, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(1, new int[] { 4, 0, 3, 2 }, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(3, new int[] { 4, 0, 1, 2 }, graph));
            Assert.IsFalse(NStreamQueryPlanBuilder.IsDependencySatisfied(2, new int[] { 3, 1, 4, 0 }, graph));
            Assert.IsTrue(NStreamQueryPlanBuilder.IsDependencySatisfied(3, new int[] { 1, 0, 2, 4 }, graph));
        }

        private ExprIdentNode Make(int stream, string p)
        {
            return new ExprIdentNodeImpl(typesPerStream[stream], p, stream);
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
