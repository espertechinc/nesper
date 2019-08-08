///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSpatialPRFilterIndexPerfStatement());
            execs.Add(new EPLSpatialPRFilterIndexPerfContextPartition());
            execs.Add(new EPLSpatialPRFilterIndexPerfPattern());
            execs.Add(new EPLSpatialPRFilterIndexUnoptimized());
            execs.Add(new EPLSpatialPRFilterIndexTypeAssertion());
            execs.Add(new EPLSpatialPRFilterIndexPatternSimple());
            execs.Add(new EPLSpatialPRFilterIndexContext());
            return execs;
        }

        internal class EPLSpatialPRFilterIndexTypeAssertion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNoIndex =
                    "@Name('s0') select * from SupportSpatialAABB(point(0, 0).inside(rectangle(x, y, width, height)))";
                env.CompileDeploy(eplNoIndex);
                SupportFilterHelper.AssertFilterMulti(
                    env.Statement("s0"),
                    "SupportSpatialAABB",
                    new[] {
                        new[] {FilterItem.BoolExprFilterItem}
                    });
                env.UndeployAll();

                var eplIndexed = "@Name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                                 "select * from SupportSpatialAABB(point(0, 0, filterindex:myindex).inside(rectangle(x, y, width, height)))";
                env.CompileDeploy(eplIndexed);
                SupportFilterHelper.AssertFilterMulti(
                    env.Statement("s0"),
                    "SupportSpatialAABB",
                    new[] {
                        new[] {
                            new FilterItem(
                                "x,y,width,height/myindex/pointregionquadtree/0.0,0.0,100.0,100.0,4.0,20.0",
                                FilterOperator.ADVANCED_INDEX)
                        }
                    });

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPRFilterIndexUnoptimized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select * from SupportSpatialAABB(point(5, 10).inside(rectangle(x, y, width, height)))");
                env.AddListener("s0");

                SendRectangle(env, "R1", 0, 0, 5, 10);
                SendRectangle(env, "R2", 4, 3, 2, 20);
                Assert.AreEqual("R2", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPRFilterIndexPerfStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile(
                    "expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from SupportSpatialAABB(point(?::int, ?::int, filterindex:myindex).inside(rectangle(x, y, width, height)))");
                var listener = new SupportUpdateListener();

                var count = 0;
                for (var x = 0; x < 10; x++) {
                    for (var y = 0; y < 10; y++) {
                        var finalX = x;
                        var finalY = y;
                        var name = x + "_" + y;
                        var options = new DeploymentOptions().WithStatementSubstitutionParameter(
                                prepared => {
                                    prepared.SetObject(1, finalX);
                                    prepared.SetObject(2, finalY);
                                })
                            .WithStatementNameRuntime(ctx => name);
                        env.Deploy(compiled, options).Statement(name).AddListener(listener);
                        // System.out.println("Deployed #" + count);
                        count++;
                    }
                }

                SendAssertSpatialAABB(env, listener, 10, 10, 1000);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPRFilterIndexPerfPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                    "select * from pattern [every p=SupportSpatialPoint -> SupportSpatialAABB(point(p.px, p.py, filterindex:myindex).inside(rectangle(x, y, width, height)))]");
                env.AddListener("s0");

                SendSpatialPoints(env, 100, 100);
                SendAssertSpatialAABB(env, env.Listener("s0"), 100, 100, 1000);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPRFilterIndexPerfContextPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context PerPointCtx initiated by SupportSpatialPoint ssp", path);
                env.CompileDeploy(
                    "@Name('s0') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                    "context PerPointCtx select count(*) from SupportSpatialAABB(point(context.ssp.px, context.ssp.py, filterindex:myindex).inside(rectangle(x, y, width, height)))",
                    path);
                env.AddListener("s0");

                SendSpatialPoints(env, 100, 100);
                SendAssertSpatialAABB(env, env.Listener("s0"), 100, 100, 1000);

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
                var epl = "@Name('out') expression myindex {pointregionquadtree(0, 0, 100, 100)}" +
                          "select p.Id as c0 from pattern [every p=SupportSpatialPoint -> every SupportSpatialAABB(point(p.px, p.py, filterindex:myindex).inside(rectangle(x, y, width, height)))]";
                env.CompileDeploy(epl).AddListener("out");

                env.Milestone(0);

                SendPoint(env, "P0", 10, 10);
                SendPoint(env, "P1", 60, 60);
                SendPoint(env, "P2", 60, 10);
                SendPoint(env, "P3", 10, 60);
                SendPoint(env, "P4", 10, 10);
                Assert.AreEqual(6, SupportFilterHelper.GetFilterCountApprox(env));
                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, "P0,P4", "P2", "P3", "P1", "P1");

                env.Milestone(1);

                Assert.AreEqual(6, SupportFilterHelper.GetFilterCountApprox(env));
                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, "P0,P4", "P2", "P3", "P1", "P1");

                env.UndeployAll();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        public class EPLSpatialPRFilterIndexContext : RegressionExecution
        {
            private static readonly int WIDTH = 10;
            private static readonly int HEIGHT = 10;
            private static readonly int NUM_POINTS = 100;
            private static readonly int NUM_QUERIES = 100;
            private static readonly int NUM_ITERATIONS = 3;

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context PointContext initiated by SupportSpatialPoint ssp terminated by SupportBean(TheString=ssp.Id);\n" +
                    "@Name('out') expression myindex {pointregionquadtree(0, 0, 10, 10)}" +
                    "context PointContext select context.ssp.Id as c0 from SupportSpatialAABB(point(context.ssp.px, context.ssp.py, filterindex:myindex).inside(rectangle(x, y, width, height)))";
                env.CompileDeploy(epl).AddListener("out");

                IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
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
                IList<SupportSpatialPoint> points)
            {
                foreach (var point in points) {
                    env.SendEventBean(new SupportBean(point.Id, 0));
                }

                points.Clear();
            }

            private void Query(
                RegressionEnvironment env,
                IList<SupportSpatialPoint> points)
            {
                var random = new Random();
                for (var i = 0; i < NUM_QUERIES; i++) {
                    var x = (int) random.NextDouble() * WIDTH;
                    var y = (int) random.NextDouble() * HEIGHT;
                    var bb = new BoundingBox(x - 3, y - 2, x + 5, y + 5);
                    AssertBBPoints(env, bb, points);
                }
            }

            private void AddPoints(
                RegressionEnvironment env,
                IList<SupportSpatialPoint> points,
                AtomicLong pointCount)
            {
                var random = new Random();
                for (var i = 0; i < NUM_POINTS; i++) {
                    var x = (int) random.NextDouble() * WIDTH;
                    var y = (int) random.NextDouble() * HEIGHT;
                    SendAddPoint(env, points, "P" + pointCount.IncrementAndGet(), x, y);
                }
            }
        }
    }
}