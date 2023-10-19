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
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportSpatialUtil;

// assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.spatial
{
    public class EPLSpatialMXCIFQuadTreeFilterIndex
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPatternSimple(execs);
            WithPerfPattern(execs);
            WithTypeAssertion(execs);
            WithWContext(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialMXCIFFilterIndexWContext());
            return execs;
        }

        public static IList<RegressionExecution> WithTypeAssertion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialMXCIFFilterIndexTypeAssertion());
            return execs;
        }

        public static IList<RegressionExecution> WithPerfPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialMXCIFFilterIndexPerfPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialMXCIFFilterIndexPatternSimple());
            return execs;
        }

        private class EPLSpatialMXCIFFilterIndexTypeAssertion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNoIndex =
                    "@name('s0') select * from SupportSpatialEventRectangle(rectangle(0, 0, 1, 1).intersects(rectangle(x, y, width, height)))";
                env.CompileDeploy(eplNoIndex);
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s0",
                    "SupportSpatialEventRectangle",
                    new FilterItem[][] { new[] { FilterItem.BoolExprFilterItem } });
                env.UndeployAll();

                var eplIndexed = "@name('s0') expression myindex {mxcifquadtree(0, 0, 100, 100)}" +
                                 "select * from SupportSpatialEventRectangle(rectangle(10, 20, 5, 6, filterindex:myindex).intersects(rectangle(x, y, width, height)))";
                env.CompileDeploy(eplIndexed).AddListener("s0");
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s0",
                    "SupportSpatialEventRectangle",
                    new FilterItem[][] {
                        new[] {
                            new FilterItem(
                                "x,y,width,height/myindex/mxcifquadtree/0.0,0.0,100.0,100.0,4.0,20.0",
                                FilterOperator.ADVANCED_INDEX)
                        }
                    });

                SendAssertEventRectangle(env, 10, 20, 0, 0, true);
                SendAssertEventRectangle(env, 9, 19, 0.9999, 0.9999, false);

                env.Milestone(0);

                SendAssertEventRectangle(env, 9, 19, 1, 1, true);
                SendAssertEventRectangle(env, 15, 26, 0, 0, true);
                SendAssertEventRectangle(env, 15.001, 26.001, 0, 0, false);

                env.UndeployAll();
            }
        }

        public class EPLSpatialMXCIFFilterIndexPatternSimple : RegressionExecution
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
                var epl = "@name('s0') expression myindex {mxcifquadtree(0, 0, 100, 100)}" +
                          "select p.id as c0 from pattern [every p=SupportSpatialEventRectangle -> every SupportSpatialAABB(rectangle(p.x, p.y, p.width, p.height, filterindex:myindex).intersects(rectangle(x, y, width, height)))]";
                env.CompileDeploy(epl).AddListener("s0");
                env.Milestone(0);

                SendEventRectangle(env, "R0", 10, 10, 1, 1);
                SendEventRectangle(env, "R1", 60, 60, 1, 1);
                SendEventRectangle(env, "R2", 60, 10, 1, 1);
                SendEventRectangle(env, "R3", 10, 60, 1, 1);
                SendEventRectangle(env, "R4", 10, 10, 1, 1);
                env.AssertThat(() => Assert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
                AssertRectanglesManyRow(env, BOXES, "R0,R4", "R2", "R3", "R1", "R1");

                env.Milestone(1);

                env.AssertThat(() => Assert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
                AssertRectanglesManyRow(env, BOXES, "R0,R4", "R2", "R3", "R1", "R1");

                env.UndeployAll();
            }
        }

        private class EPLSpatialMXCIFFilterIndexPerfPattern : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') expression myindex {mxcifquadtree(0, 0, 100, 100)}" +
                    "select * from pattern [every p=SupportSpatialEventRectangle -> SupportSpatialAABB(rectangle(p.x, p.y, p.width, p.height, filterindex:myindex).intersects(rectangle(x, y, width, height)))]");
                env.AddListener("s0");

                SendSpatialEventRectanges(env, 100, 50);
                SendAssertSpatialAABB(env, 100, 50, 1000);

                env.UndeployAll();
            }
        }

        public class EPLSpatialMXCIFFilterIndexWContext : RegressionExecution
        {
            private const int WIDTH = 10;
            private const int HEIGHT = 10;
            private const int NUM_POINTS = 100;
            private const int NUM_QUERIES = 100;
            private const int NUM_ITERATIONS = 3;

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create context RectangleContext initiated by SupportSpatialEventRectangle ssr terminated by SupportBean(theString=ssr.id);\n" +
                    "@name('s0') expression myindex {mxcifquadtree(0, 0, 10, 10)}" +
                    "context RectangleContext select context.ssr.id as c0 from SupportSpatialAABB(rectangle(context.ssr.x, context.ssr.y, context.ssr.width, context.ssr.height, filterindex:myindex).intersects(rectangle(x, y, width, height)))";
                env.CompileDeploy(epl).AddListener("s0");

                IList<SupportSpatialEventRectangle> rectangles = new List<SupportSpatialEventRectangle>();
                var milestone = new AtomicLong();
                for (var iteration = 0; iteration < NUM_ITERATIONS; iteration++) {
                    Query(env, "s0", rectangles);
                    AddRectangles(env, rectangles, milestone);
                    Query(env, "s0", rectangles);

                    env.MilestoneInc(milestone);

                    Query(env, "s0", rectangles);
                    RemoveRectangles(env, rectangles);
                    Query(env, "s0", rectangles);

                    env.MilestoneInc(milestone);
                }

                env.UndeployAll();
            }

            private void RemoveRectangles(
                RegressionEnvironment env,
                IList<SupportSpatialEventRectangle> points)
            {
                foreach (var point in points) {
                    env.SendEventBean(new SupportBean(point.Id, 0));
                }

                points.Clear();
            }

            private void Query(
                RegressionEnvironment env,
                string stmtName,
                IList<SupportSpatialEventRectangle> points)
            {
                var random = new Random();
                for (var i = 0; i < NUM_QUERIES; i++) {
                    var x = (int)random.NextDouble() * WIDTH;
                    var y = (int)random.NextDouble() * HEIGHT;
                    var bb = new BoundingBox(x - 3, y - 2, x + 5, y + 5);
                    SupportSpatialUtil.AssertBBRectangles(env, stmtName, bb, points);
                }
            }

            private void AddRectangles(
                RegressionEnvironment env,
                IList<SupportSpatialEventRectangle> rectangles,
                AtomicLong rectangleCount)
            {
                var random = new Random();
                for (var i = 0; i < NUM_POINTS; i++) {
                    var x = (int)random.NextDouble() * WIDTH;
                    var y = (int)random.NextDouble() * HEIGHT;
                    var width = random.NextDouble() * WIDTH / 0.25;
                    var height = random.NextDouble() * HEIGHT / 0.25;
                    SendAddRectangle(env, rectangles, "R" + rectangleCount.IncrementAndGet(), x, y, width, height);
                }
            }
        }

        internal static void SendAssertEventRectangle(
            RegressionEnvironment env,
            double x,
            double y,
            double width,
            double height,
            bool expected)
        {
            env.SendEventBean(new SupportSpatialEventRectangle(null, x, y, width, height));
            env.AssertListenerInvokedFlag("s0", expected);
        }

        internal static void SendSpatialEventRectanges(
            RegressionEnvironment env,
            int numX,
            int numY)
        {
            for (var x = 0; x < numX; x++) {
                for (var y = 0; y < numY; y++) {
                    env.SendEventBean(
                        new SupportSpatialEventRectangle(
                            Convert.ToString(x) + "_" + Convert.ToString(y),
                            x,
                            y,
                            0.1,
                            0.2));
                }
            }
        }
    }
} // end of namespace