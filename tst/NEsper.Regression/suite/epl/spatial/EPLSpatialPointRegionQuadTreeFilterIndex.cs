///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportSpatialUtil;

namespace com.espertech.esper.regressionlib.suite.epl.spatial
{
    public class EPLSpatialPointRegionQuadTreeFilterIndex
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFilterIndexPerfStatement(execs);
            WithFilterIndexPerfContextPartition(execs);
            WithFilterIndexPerfPattern(execs);
            WithFilterIndexUnoptimized(execs);
            WithFilterIndexTypeAssertion(execs);
            WithFilterIndexPatternSimple(execs);
            WithFilterIndexContext(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexContext());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexPatternSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexPatternSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexTypeAssertion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexTypeAssertion());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexUnoptimized(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPrFilterIndexUnoptimized());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexPerfPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexPerfPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexPerfContextPartition(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexPerfContextPartition());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterIndexPerfStatement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexPerfStatement());
            return execs;
        }

        private class EPLSpatialPRFilterIndexTypeAssertion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNoIndex =
                    "@name('s0') select * from SupportSpatialAABB(point(0, 0).inside(rectangle(x, y, width, height)))";
                env.CompileDeploy(eplNoIndex);
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s0",
                    "SupportSpatialAABB",
                    new FilterItem[][] { new FilterItem[] { FilterItem.BoolExprFilterItem } });
                env.UndeployAll();

                var eplIndexed = "@name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                                 "select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))";
                env.CompileDeploy(eplIndexed);
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s0",
                    "SupportSpatialAABB",
                    new FilterItem[][] {
                        new FilterItem[] {
                            new FilterItem(
                                "x,y,width,height/myindex/pointregionquadtree/0.0,0.0,100.0,100.0,4.0,20.0",
                                FilterOperator.ADVANCED_INDEX)
                        }
                    });

                env.UndeployAll();
            }
        }

        private class EPLSpatialPrFilterIndexUnoptimized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') select * from SupportSpatialAABB(point(5, 10).inside(rectangle(x, y, width, height)))");
                env.AddListener("s0");

                SendRectangle(env, "R1", 0, 0, 5, 10);
                SendRectangle(env, "R2", 4, 3, 2, 20);
                env.AssertEqualsNew("s0", "id", "R2");

                env.UndeployAll();
            }
        }

        private class EPLSpatialPRFilterIndexPerfStatement : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile(
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from SupportSpatialAABB(point(?::int, ?::int, filterindex:myindex).inside(rectangle(x, y, width, height)))");
                var listener = new SupportUpdateListener();

                var count = 0;
                for (var x = 0; x < 10; x++) {
                    for (var y = 0; y < 10; y++) {
                        var readonlyX = x;
                        var readonlyY = y;
                        var name = x + "_" + y;
                        var options =
                            new DeploymentOptions()
                                .WithStatementSubstitutionParameter(
                                    prepared => {
                                        prepared.SetObject(1, readonlyX);
                                        prepared.SetObject(2, readonlyY);
                                    })
                                .WithStatementNameRuntime(ctx => name);
                        env.Deploy(compiled, options).Statement(name).AddListener(listener);
                        // Console.WriteLine("Deployed #" + count);
                        count++;
                    }
                }

                SendAssertSpatialAABB(env, listener, 10, 10, 1000);

                env.UndeployAll();
            }
        }

        private class EPLSpatialPRFilterIndexPerfPattern : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from pattern [every p=SupportSpatialPoint -> SupportSpatialAABB(point(p.px, p.py, filterindex:myindex).inside(rectangle(x, y, width, height)))]");
                env.AddListener("s0");

                SendSpatialPoints(env, 100, 100);
                SendAssertSpatialAABB(env, 100, 100, 1000);

                env.UndeployAll();
            }
        }

        private class EPLSpatialPRFilterIndexPerfContextPartition : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create context PerPointCtx initiated by SupportSpatialPoint ssp", path);
                env.CompileDeploy(
                    "@name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                    "context PerPointCtx select count(*) from SupportSpatialAABB(point(context.ssp.px, context.ssp.py, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    path);
                env.AddListener("s0");

                SendSpatialPoints(env, 100, 100);
                SendAssertSpatialAABB(env, 100, 100, 1000);

                env.UndeployAll();
            }
        }

        public class EPLSpatialPRFilterIndexPatternSimple : RegressionExecution
        {
            private static readonly IList<BoundingBox> BOXES = Arrays.AsList(
                new BoundingBox(0, 0, 50, 50),
                new BoundingBox(50, 0, 100, 50),
                new BoundingBox(0, 50, 50, 100),
                new BoundingBox(50, 50, 100, 100),
                new BoundingBox(25, 25, 75, 75)
            );

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                          "select p.id as c0 from pattern [every p=SupportSpatialPoint -> every SupportSpatialAABB(point(p.px, p.py, filterindex:myindex).inside(rectangle(x, y, width, height)))]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendPoint(env, "P0", 10, 10);
                SendPoint(env, "P1", 60, 60);
                SendPoint(env, "P2", 60, 10);
                SendPoint(env, "P3", 10, 60);
                SendPoint(env, "P4", 10, 10);
                env.AssertThat(() => Assert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
                AssertRectanglesManyRow(env, BOXES, "P0,P4", "P2", "P3", "P1", "P1");

                env.Milestone(1);

                env.AssertThat(() => Assert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
                AssertRectanglesManyRow(env, BOXES, "P0,P4", "P2", "P3", "P1", "P1");

                env.UndeployAll();
                env.AssertThat(() => Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
            }
        }

        public class EPLSpatialPRFilterIndexContext : RegressionExecution
        {
            private static readonly int WIDTH = 10;
            private static readonly int HEIGHT = 10;
            private static readonly int NUM_POINTS = 100;
            private static readonly int NUM_QUERIES = 100;
            private static readonly int NUM_ITERATIONS = 3;

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context PointContext initiated by SupportSpatialPoint ssp terminated by SupportBean(theString=ssp.id);\n" +
                    "@name('s0') expression myindex {pointregionquadtree(0, 0, 10, 10)}" +
                    "context PointContext select context.ssp.id as c0 from SupportSpatialAABB(point(context.ssp.px, context.ssp.py, filterindex:myindex).inside(rectangle(x, y, width, height)))";
                env.CompileDeploy(epl).AddListener("s0");

                var points = new List<SupportSpatialPoint>();
                var count = 0;
                var milestone = new AtomicLong();
                for (var iteration = 0; iteration < NUM_ITERATIONS; iteration++) {
                    Query(env, points);
                    AddPoints(env, points, milestone);
                    Query(env, points);

                    env.MilestoneInc(milestone);

                    Query(env, points);
                    RemovePoints(env, points);
                    Query(env, points);

                    env.MilestoneInc(milestone);
                }

                env.UndeployAll();
            }

            private void RemovePoints(
                RegressionEnvironment env,
                List<SupportSpatialPoint> points)
            {
                foreach (var point in points) {
                    env.SendEventBean(new SupportBean(point.Id, 0));
                }

                points.Clear();
            }

            private void Query(
                RegressionEnvironment env,
                List<SupportSpatialPoint> points)
            {
                var random = new Random();
                for (var i = 0; i < NUM_QUERIES; i++) {
                    var x = (int)random.NextDouble() * WIDTH;
                    var y = (int)random.NextDouble() * HEIGHT;
                    var bb = new BoundingBox(x - 3, y - 2, x + 5, y + 5);
                    AssertBBPoints(env, bb, points);
                }
            }

            private void AddPoints(
                RegressionEnvironment env,
                List<SupportSpatialPoint> points,
                AtomicLong pointCount)
            {
                var random = new Random();
                for (var i = 0; i < NUM_POINTS; i++) {
                    var x = (int)random.NextDouble() * WIDTH;
                    var y = (int)random.NextDouble() * HEIGHT;
                    SendAddPoint(env, points, "P" + pointCount.IncrementAndGet(), x, y);
                }
            }
        }
    }
}