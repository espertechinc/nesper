///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportSpatialUtil;

namespace com.espertech.esper.regressionlib.suite.epl.spatial
{
    public class EPLSpatialMXCIFQuadTreeEventIndex
    {
        private static readonly IList<BoundingBox> BOXES = Arrays.AsList(
            new BoundingBox(0, 0, 50, 50),
            new BoundingBox(50, 0, 100, 50),
            new BoundingBox(0, 50, 50, 100),
            new BoundingBox(50, 50, 100, 100),
            new BoundingBox(25, 25, 75, 75)
        );

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSpatialMXCIFEventIndexNamedWindowSimple());
            execs.Add(new EPLSpatialMXCIFEventIndexUnindexed());
            execs.Add(new EPLSpatialMXCIFEventIndexOnTriggerNWInsertRemove(false));
            execs.Add(new EPLSpatialMXCIFEventIndexOnTriggerNWInsertRemove(true));
            execs.Add(new EPLSpatialMXCIFEventIndexUnique());
            execs.Add(new EPLSpatialMXCIFEventIndexPerformance());
            execs.Add(new EPLSpatialMXCIFEventIndexTableFireAndForget());
            execs.Add(new EPLSpatialMXCIFEventIndexZeroWidthAndHeight());
            execs.Add(new EPLSpatialMXCIFEventIndexTableSubdivideMergeDestroy());
            execs.Add(new EPLSpatialMXCIFEventIndexTableSubdivideDeepAddDestroy());
            execs.Add(new EPLSpatialMXCIFEventIndexTableSubdivideDestroy());
            execs.Add(new EPLSpatialMXCIFEventIndexEdgeSubdivide(true));
            execs.Add(new EPLSpatialMXCIFEventIndexEdgeSubdivide(false));
            execs.Add(new EPLSpatialMXCIFEventIndexRandomMovingPoints());
            execs.Add(new EPLSpatialMXCIFEventIndexRandomIntPointsInSquareUnique());
            execs.Add(new EPLSpatialMXCIFEventIndexRandomRectsWRandomQuery());
            return execs;
        }

        private static void RunAssertionFAF(
            RegressionEnvironment env,
            RegressionPath path,
            double x,
            double y,
            double width,
            double height,
            bool expected)
        {
            var result = env.CompileExecuteFAF(
                IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                "select Id as c0 from MyTable where rectangle(tx, ty, tw, th).intersects(rectangle(" +
                x +
                ", " +
                y +
                ", " +
                width +
                ", " +
                height +
                "))",
                path);
            SupportQueryPlanIndexHook.AssertFAFAndReset("MyIdxCIFQuadTree", "EventTableQuadTreeMXCIF");
            if (expected) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    result.Array,
                    new [] { "c0" },
                    new[] {
                        new object[] {"R1"}
                    });
            }
            else {
                Assert.AreEqual(0, result.Array.Length);
            }
        }

        private static void SendEventRectangle(
            RegressionEnvironment env,
            string id,
            double x,
            double y,
            double width,
            double height)
        {
            env.SendEventBean(new SupportSpatialEventRectangle(id, x, y, width, height));
        }

        private static SupportSpatialAABB Rect(
            double x,
            double y,
            double width,
            double height)
        {
            return new SupportSpatialAABB(null, x, y, width, height);
        }

        private static void SendAssert(
            RegressionEnvironment env,
            SupportSpatialAABB one,
            SupportSpatialAABB two,
            bool expected)
        {
            var bbOne = BoundingBox.From(one.X, one.Y, one.Width, one.Height);
            Assert.AreEqual(expected, bbOne.IntersectsBoxIncludingEnd(two.X, two.Y, two.Width, two.Height));

            var bbTwo = BoundingBox.From(two.X, two.Y, two.Width, two.Height);
            Assert.AreEqual(expected, bbTwo.IntersectsBoxIncludingEnd(one.X, one.Y, one.Width, one.Height));

            env.SendEventBean(new SupportSpatialDualAABB(one, two));
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

            env.SendEventBean(new SupportSpatialDualAABB(two, one));
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
        }

        private static void SendSpatialEventRectanges(
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

        internal class EPLSpatialMXCIFEventIndexNamedWindowSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create window RectangleWindow#keepall as (Id string, rx double, ry double, rw double, rh double);\n" +
                    "create index MyIndex on RectangleWindow((rx,ry,rw,rh) mxcifquadtree(0,0,100,100));\n" +
                    "insert into RectangleWindow select Id, X as rx, Y as ry, Width as rw, Height as rh from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleWindow as pt where rectangle(rx,ry,rw,rh).intersects(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SendEventRectangle(env, "R0", 73.32704983331149, 23.46990952575032, 1, 1);
                SendEventRectangle(env, "R1", 53.09747562396894, 17.100976152185034, 1, 1);
                SendEventRectangle(env, "R2", 56.75757294858788, 25.508506696809608, 1, 1);
                SendEventRectangle(env, "R3", 83.66639067675291, 76.53772974832937, 1, 1);
                SendEventRectangle(env, "R4", 51.01654641861326, 43.49009281983866, 1, 1);

                env.Milestone(0);

                var beginX = 50.45945198254618;
                var endX = 88.31594559038719;
                var beginY = 4.577595744501329;
                var endY = 22.93393078279351;

                env.SendEventBean(new SupportSpatialAABB("", beginX, beginY, endX - beginX, endY - beginY));
                var received = SortJoinProperty(env.Listener("out").GetAndResetLastNewData(), "c0");
                Assert.AreEqual("R1", received);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialMXCIFEventIndexZeroWidthAndHeight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType("create schema Geofence(x double, y double, vin string)", path);

                env.CompileDeploy(
                    "create table Regions(regionId string primary key, rx double, ry double, rwidth double, rheight double)",
                    path);
                env.CompileDeploy(
                    "create index RectangleIndex on Regions((rx, ry, rwidth, rheight) mxcifquadtree(0, 0, 10, 12))",
                    path);
                env.CompileDeploy(
                    "@Name('s0') " +
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                    "on Geofence as vin insert into VINWithRegion select regionId, vin from Regions where rectangle(rx, ry, rwidth, rheight).intersects(rectangle(vin.x, vin.y, 0, 0))",
                    path);
                env.AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "RectangleIndex",
                    "non-unique hash={} btree={} advanced={mxcifquadtree(rx,ry,rwidth,rheight)}");

                env.CompileExecuteFAF("insert into Regions values ('R1', 2, 2, 5, 5)", path);
                env.SendEventMap(CollectionUtil.PopulateNameValueMap("x", 3d, "y", 3d, "vin", "V1"), "Geofence");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "vin","regionId" },
                    new object[] {"V1", "R1"});

                env.UndeployAll();
            }
        }

        internal class EPLSpatialMXCIFEventIndexTableFireAndForget : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table MyTable(Id string primary key, tx double, ty double, tw double, th double)",
                    path);
                env.CompileExecuteFAF("insert into MyTable values ('R1', 10, 20, 5, 6)", path);
                env.CompileDeploy(
                    "create index MyIdxCIFQuadTree on MyTable( (tx, ty, tw, th) mxcifquadtree(0, 0, 100, 100))",
                    path);

                RunAssertionFAF(env, path, 10, 20, 0, 0, true);
                RunAssertionFAF(env, path, 9, 19, 1, 1, true);
                RunAssertionFAF(env, path, 9, 19, 0.9999, 0.9999, false);
                RunAssertionFAF(env, path, 15, 26, 0, 0, true);
                RunAssertionFAF(env, path, 15.0001, 26.0001, 0, 0, false);
                RunAssertionFAF(env, path, 0, 0, 100, 100, true);
                RunAssertionFAF(env, path, 11, 21, 1, 1, true);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialMXCIFEventIndexPerformance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create window MyRectangleWindow#keepall as (Id string, rx double, ry double, rw double, rh double);\n" +
                    "insert into MyRectangleWindow select Id, x as rx, y as ry, width as rw, height as rh from SupportSpatialEventRectangle;\n" +
                    "create index Idx on MyRectangleWindow( (rx, ry, rw, rh) mxcifquadtree(0, 0, 100, 100));\n" +
                    "@Name('out') on SupportSpatialAABB select mpw.Id as c0 from MyRectangleWindow as mpw where rectangle(rx, ry, rw, rh).intersects(rectangle(x, y, width, height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SendSpatialEventRectanges(env, 100, 50);

                var start = PerformanceObserver.MilliTime;
                var listener = env.Listener("out");
                for (var x = 0; x < 100; x++) {
                    for (var y = 0; y < 50; y++) {
                        env.SendEventBean(new SupportSpatialAABB("R", x, y, 0.5, 0.5));
                        Assert.AreEqual(
                            Convert.ToString(x) + "_" + Convert.ToString(y),
                            listener.AssertOneGetNewAndReset().Get("c0"));
                    }
                }

                var delta = PerformanceObserver.MilliTime - start;
                Assert.IsTrue(delta < 2000, "delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialMXCIFEventIndexUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('win') create window MyRectWindow#keepall as (Id string, rx double, ry double, rw double, rh double);\n" +
                    "@Name('insert') insert into MyRectWindow select Id, x as rx, y as ry, width as rw, height as rh from SupportSpatialEventRectangle;\n" +
                    "@Name('Idx') create unique index Idx on MyRectWindow( (rx, ry, rw, rh) mxcifquadtree(0, 0, 100, 100));\n" +
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                    "@Name('out') on SupportSpatialAABB select mpw.Id as c0 from MyRectWindow as mpw where rectangle(rx, ry, rw, rh).intersects(rectangle(x, y, width, height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "Idx",
                    "unique hash={} btree={} advanced={mxcifquadtree(rx,ry,rw,rh)}");

                SendEventRectangle(env, "P1", 10, 15, 1, 2);
                try {
                    SendEventRectangle(env, "P1", 10, 15, 1, 2);
                    Assert.Fail();
                }
                catch (Exception ex) { // we have a handler
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unexpected exception in statement 'win': Unique index violation, index 'Idx' is a unique index and key '(10.0,15.0,1.0,2.0)' already exists");
                }

                env.UndeployAll();
            }
        }

        internal class EPLSpatialMXCIFEventIndexUnindexed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select rectangle(One.X, One.Y, One.Width, One.Height).intersects(rectangle(Two.X, Two.Y, Two.Width, Two.Height)) as c0 from SupportSpatialDualAABB");
                env.AddListener("s0");

                // For example, in MySQL:
                // SET @g1 = ST_GeomFromText('Polygon((1 1,1 2,2 2,2 1,1 1))');
                // SET @g2 = ST_GeomFromText('Polygon((2 2,2 4,4 4,4 2,2 2))');
                // SELECT MBRIntersects(@g1,@g2), MBRIntersects(@g2,@g1);
                // includes exterior

                SendAssert(env, Rect(1, 1, 5, 5), Rect(2, 2, 2, 2), true);
                SendAssert(env, Rect(1, 1, 1, 1), Rect(2, 2, 2, 2), true);

                env.Milestone(0);

                SendAssert(env, Rect(1, 0.9999, 1, 0.99999), Rect(2, 2, 2, 2), false);
                SendAssert(env, Rect(1, 1, 1, 0.99999), Rect(2, 2, 2, 2), false);
                SendAssert(env, Rect(1, 0.9999, 1, 1), Rect(2, 2, 2, 2), false);

                SendAssert(env, Rect(4, 4, 1, 1), Rect(2, 2, 2, 2), true);
                SendAssert(env, Rect(4.0001, 4, 1, 1), Rect(2, 2, 2, 2), false);
                SendAssert(env, Rect(4, 4.0001, 1, 1), Rect(2, 2, 2, 2), false);

                env.Milestone(1);

                SendAssert(env, Rect(10, 20, 5, 5), Rect(0, 0, 50, 50), true);
                SendAssert(env, Rect(10, 20, 5, 5), Rect(20, 20, 50, 50), false);
                SendAssert(env, Rect(10, 20, 5, 5), Rect(9, 19, 1, 1), true);
                SendAssert(env, Rect(10, 20, 5, 5), Rect(15, 25, 1, 1), true);
                env.UndeployAll();
            }
        }

        internal class EPLSpatialMXCIFEventIndexOnTriggerNWInsertRemove : RegressionExecution
        {
            private readonly bool soda;

            public EPLSpatialMXCIFEventIndexOnTriggerNWInsertRemove(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    soda,
                    "create window MyWindow#length(5) as select * from SupportSpatialEventRectangle",
                    path);
                env.CompileDeploy(
                    soda,
                    "create index MyIndex on MyWindow((X,Y,Width,Height) mxcifquadtree(0,0,100,100))",
                    path);
                env.CompileDeploy(soda, "insert into MyWindow select * from SupportSpatialEventRectangle", path);

                var epl = "@Name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          " on SupportSpatialAABB as aabb " +
                          "select rects.Id as c0 from MyWindow as rects where rectangle(rects.X,rects.Y,rects.Width,rects.Height).intersects(rectangle(aabb.X,aabb.Y,aabb.Width,aabb.Height))";
                env.CompileDeploy(soda, epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIndex",
                    "non-unique hash={} btree={} advanced={mxcifquadtree(X,Y,Width,Height)}");

                SendEventRectangle(env, "R1", 10, 40, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R1", null, null, null, null);

                SendEventRectangle(env, "R2", 80, 80, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R1", null, null, "R2", null);

                env.Milestone(0);

                SendEventRectangle(env, "R3", 10, 40, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R1,R3", null, null, "R2", null);

                env.Milestone(1);

                SendEventRectangle(env, "R4", 60, 40, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R1,R3", "R4", null, "R2", "R4");

                SendEventRectangle(env, "R5", 20, 75, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R1,R3", "R4", "R5", "R2", "R4");

                env.Milestone(2);

                SendEventRectangle(env, "R6", 50, 50, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R3,R6", "R4,R6", "R5,R6", "R2,R6", "R4,R6");

                SendEventRectangle(env, "R7", 0, 0, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R3,R6,R7", "R4,R6", "R5,R6", "R6", "R4,R6");

                env.Milestone(3);

                SendEventRectangle(env, "R8", 99.999, 0, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R6,R7", "R4,R6,R8", "R5,R6", "R6", "R4,R6");

                SendEventRectangle(env, "R9", 0, 99.999, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R6,R7", "R6,R8", "R5,R6,R9", "R6", "R6");

                env.Milestone(4);

                SendEventRectangle(env, "R10", 99.999, 99.999, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R6,R7", "R6,R8", "R6,R9", "R6,R10", "R6");

                SendEventRectangle(env, "R11", 0, 0, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "R7,R11", "R8", "R9", "R10", null);

                env.UndeployAll();
            }
        }

        public class EPLSpatialMXCIFEventIndexTableSubdivideMergeDestroy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create table RectangleTable(Id string primary key, rx double, ry double, rw double, rh double);\n" +
                    "create index MyIndex on RectangleTable((rx,ry,rw,rh) mxcifquadtree(0,0,100,100,4,20));\n" +
                    "insert into RectangleTable select Id, X as rx, Y as ry, Width as rw, Height as rh from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleTable as pt where rectangle(rx,ry,rw,rh).intersects(rectangle(x,y,width,height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                SendEventRectangle(env, "P1", 80, 80, 1, 1);
                SendEventRectangle(env, "P2", 81, 80, 1, 1);
                SendEventRectangle(env, "P3", 80, 81, 1, 1);
                SendEventRectangle(env, "P4", 80, 80, 1, 1);
                SendEventRectangle(env, "P5", 45, 55, 1, 1);
                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3,P4", "P5");

                env.Milestone(0);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3,P4", "P5");
                env.CompileExecuteFAF("delete from RectangleTable where Id = 'P4'", path);
                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3", "P5");

                env.Milestone(1);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3", "P5");

                env.UndeployAll();
            }
        }

        public class EPLSpatialMXCIFEventIndexTableSubdivideDeepAddDestroy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table RectangleTable(Id string primary key, X double, Y double, Width double, Height double);\n" +
                    "create index MyIndex on RectangleTable((X,Y,Width,Height) mxcifquadtree(0,0,100,100,2,12));\n" +
                    "insert into RectangleTable select Id, X, Y, Width, Height from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleTable as pt where rectangle(pt.X,pt.Y,pt.Width,pt.Height).intersects(rectangle(aabb.X,aabb.Y,aabb.Width,aabb.Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                IList<SupportSpatialEventRectangle> rectangles = new List<SupportSpatialEventRectangle>();
                var bbtree =
                    new BoundingBox(0, 0, 100, 100).TreeForPath(new [] { "nw","se","sw","ne","nw","nw","nw","nw","nw","nw","nw","nw" });
                var somewhere = bbtree.nw.se.sw.ne.nw.nw.nw.nw.nw.nw.nw.nw.bb;

                AddSendRectangle(env, rectangles, "P1", somewhere.MinX, somewhere.MinY, 0.0001, 0.0001);
                AddSendRectangle(env, rectangles, "P2", somewhere.MinX, somewhere.MinY, 0.0001, 0.0001);
                AddSendRectangle(env, rectangles, "P3", somewhere.MinX, somewhere.MinY, 0.0001, 0.0001);
                AssertBBTreeRectangles(env, bbtree, rectangles);

                env.Milestone(0);

                AssertBBTreeRectangles(env, bbtree, rectangles);

                env.UndeployAll();
            }
        }

        public class EPLSpatialMXCIFEventIndexTableSubdivideDestroy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table RectangleTable(Id string primary key, X double, Y double, Width double, Height double);\n" +
                    "create index MyIndex on RectangleTable((X,Y,Width,Height) mxcifquadtree(0,0,100,100,4,40));\n" +
                    "insert into RectangleTable select Id, X, Y, Width, Height from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleTable as pt where rectangle(pt.X,pt.Y,pt.Width,pt.Height).intersects(rectangle(aabb.X,aabb.Y,aabb.Width,aabb.Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SendEventRectangle(env, "P1", 80, 40, 1, 1);
                SendEventRectangle(env, "P2", 81, 41, 1, 1);
                SendEventRectangle(env, "P3", 80, 40, 1, 1);

                env.Milestone(0);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, "P1,P2,P3", null, null, null);
                SendEventRectangle(env, "P4", 80, 40, 1, 1);
                SendEventRectangle(env, "P5", 81, 41, 1, 1);

                env.Milestone(1);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, "P1,P2,P3,P4,P5", null, null, null);

                env.UndeployAll();
            }
        }

        public class EPLSpatialMXCIFEventIndexEdgeSubdivide : RegressionExecution
        {
            private readonly bool straddle;

            public EPLSpatialMXCIFEventIndexEdgeSubdivide(bool straddle)
            {
                this.straddle = straddle;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create window RectangleWindow#keepall as (Id string, X double, Y double, Width double, Height double);\n" +
                    "create index MyIndex on RectangleWindow((X,Y,Width,Height) mxcifquadtree(0,0,100,100,2,5));\n" +
                    "insert into RectangleWindow select Id, X, Y, Width, Height from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleWindow as pt where rectangle(pt.X,pt.Y,pt.Width,pt.Height).intersects(rectangle(aabb.X,aabb.Y,aabb.Width,aabb.Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var boxesLevel4 = GetLevel5Boxes();
                var count = 0;
                IList<SupportSpatialEventRectangle> rectangles = new List<SupportSpatialEventRectangle>();
                var offset = straddle ? 0 : 0.001;
                foreach (var bb in boxesLevel4) {
                    SendAddRectangle(env, rectangles, "A" + count, bb.MinX + offset, bb.MinY + offset, 0.001, 0.001);
                    SendAddRectangle(env, rectangles, "B" + count, bb.MinX + offset, bb.MinY + offset, 0.001, 0.001);
                    SendAddRectangle(env, rectangles, "C" + count, bb.MinX + offset, bb.MinY + offset, 0.001, 0.001);
                    count++;
                }

                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);

                env.Milestone(0);

                RemoveAllABRectangles(env, path, rectangles);
                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);

                env.Milestone(1);

                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);
                RemoveEverySecondRectangle(env, path, rectangles);
                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);

                env.Milestone(2);

                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);
                RemoveEverySecondRectangle(env, path, rectangles);
                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);

                env.Milestone(3);

                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);
                RemoveAllRectangles(env, path, rectangles);
                AssertAllRectangles(env, rectangles, 0, 0, 100, 100);

                env.Milestone(4);

                RemoveAllRectangles(env, path, rectangles);

                env.UndeployAll();
            }

            private void RemoveEverySecondRectangle(
                RegressionEnvironment env,
                RegressionPath path,
                IList<SupportSpatialEventRectangle> rectangles)
            {
                var count = 0;
                var idList = new List<string>();
                for (var ii = 0; ii < rectangles.Count; ii++) {
                    var p = rectangles[ii];
                    if (count % 2 == 1) {
                        rectangles.RemoveAt(ii--);
                        idList.Add(p.Id);
                    }

                    count++;
                }

                var deleteQuery = BuildDeleteQueryWithInClause("RectangleWindow", "Id", idList);
                env.CompileExecuteFAF(deleteQuery, path);
            }

            private void RemoveAllRectangles(
                RegressionEnvironment env,
                RegressionPath path,
                IList<SupportSpatialEventRectangle> points)
            {
                IList<string> idList = new List<string>();
                for (var ii = 0; ii < points.Count; ii++) {
                    var p = points[ii];
                    points.RemoveAt(ii--);
                    idList.Add(p.Id);
                }

                if (idList.IsEmpty()) {
                    return;
                }

                var deleteQuery = BuildDeleteQueryWithInClause("RectangleWindow", "Id", idList);
                env.CompileExecuteFAF(deleteQuery, path);
            }

            private void RemoveAllABRectangles(
                RegressionEnvironment env,
                RegressionPath path,
                IList<SupportSpatialEventRectangle> rectangles)
            {
                IList<string> idList = new List<string>();
                for (var ii = 0; ii < rectangles.Count; ii++) {
                    var p = rectangles[ii];
                    if (p.Id[0] == 'A' || p.Id[0] == 'B') {
                        rectangles.RemoveAt(ii--);
                        idList.Add(p.Id);
                    }
                }

                var deleteQuery = BuildDeleteQueryWithInClause("RectangleWindow", "Id", idList);
                env.CompileExecuteFAF(deleteQuery, path);
            }

            private ISet<BoundingBox> GetLevel5Boxes()
            {
                var bb = new BoundingBox(0, 0, 100, 100);
                var bbtree = bb.TreeForDepth(4);
                var bbs = new LinkedHashSet<BoundingBox>();
                foreach (var lvl1 in EnumHelper.GetValues<QuadrantEnum>()) {
                    var q1 = bbtree.GetQuadrant(lvl1);
                    foreach (var lvl2 in EnumHelper.GetValues<QuadrantEnum>()) {
                        var q2 = q1.GetQuadrant(lvl2);
                        foreach (var lvl3 in EnumHelper.GetValues<QuadrantEnum>()) {
                            var q3 = q2.GetQuadrant(lvl3);
                            foreach (var lvl4 in EnumHelper.GetValues<QuadrantEnum>()) {
                                var q4 = q3.GetQuadrant(lvl4);
                                bbs.Add(q4.bb);
                            }
                        }
                    }
                }

                Assert.AreEqual(256, bbs.Count);
                return bbs;
            }
        }

        public class EPLSpatialMXCIFEventIndexRandomRectsWRandomQuery : RegressionExecution
        {
            private const int NUM_POINTS = 1000;
            private const int X = 0;
            private const int Y = 0;
            private const int WIDTH = 100;
            private const int HEIGHT = 100;
            private const int NUM_QUERIES_AFTER_LOAD = 100;
            private const int NUM_QUERIES_AFTER_EACH_REMOVE = 5;
            private static readonly int[] CHECKPOINT_REMAINING = {100, 300, 700}; // must be sorted
            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create window RectangleWindow#keepall as (Id string, Px double, Py double, Pw double, Ph double);\n" +
                    "create index MyIndex on RectangleWindow((Px,Py,pw,ph) mxcifquadtree(0,0,100,100));\n" +
                    "insert into RectangleWindow select Id, X as Px, Y as Py, Width as Pw, Height as Ph from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleWindow as pt where rectangle(Px,Py,pw,ph).intersects(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var random = new Random();
                var rectangles = RandomRectangles(random, NUM_POINTS, X, Y, WIDTH, HEIGHT);
                foreach (var rectangle in rectangles) {
                    SendEventRectangle(
                        env,
                        rectangle.Id,
                        rectangle.X.Value,
                        rectangle.Y.Value,
                        rectangle.Width.Value,
                        rectangle.Height.Value);
                    // Comment-me-in: log.info("Point: " + rectangle);
                }

                env.Milestone(0);

                for (var i = 0; i < NUM_QUERIES_AFTER_LOAD; i++) {
                    RandomQuery(env, path, random, rectangles);
                }

                var milestone = new AtomicLong();
                var deleteQuery = env.CompileFAF("delete from RectangleWindow where Id=?::string", path);
                var preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);

                while (!rectangles.IsEmpty()) {
                    var removed = RandomRemove(random, rectangles);
                    preparedDelete.SetObject(1, removed.Id);
                    env.Runtime.FireAndForgetService.ExecuteQuery(preparedDelete);

                    for (var i = 0; i < NUM_QUERIES_AFTER_EACH_REMOVE; i++) {
                        RandomQuery(env, path, random, rectangles);
                    }

                    if (Array.BinarySearch(CHECKPOINT_REMAINING, rectangles.Count) >= 0) {
                        log.Info("Checkpoint at " + rectangles.Count);
                        env.MilestoneInc(milestone);
                        preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);
                    }
                }

                env.UndeployAll();
            }

            private SupportSpatialEventRectangle RandomRemove(
                Random random,
                IList<SupportSpatialEventRectangle> rectangles)
            {
                var index = random.Next(rectangles.Count);
                return rectangles.DeleteAt(index);
            }

            private void RandomQuery(
                RegressionEnvironment env,
                RegressionPath path,
                Random random,
                IList<SupportSpatialEventRectangle> rectangles)
            {
                var bbWidth = random.NextDouble() * WIDTH * 1.5;
                var bbHeight = random.NextDouble() * HEIGHT * 1.5;
                var bbMinX = random.NextDouble() * WIDTH + X * 0.8;
                var bbMinY = random.NextDouble() * HEIGHT + Y * 0.8;
                var bbMaxX = bbMinX + bbWidth;
                var bbMaxY = bbMinY + bbHeight;
                var boundingBox = new BoundingBox(bbMinX, bbMinY, bbMaxX, bbMaxY);
                // Comment-me-in: log.info("Query: " + boundingBox);
                AssertBBRectangles(env, boundingBox, rectangles);
            }
        }

        public class EPLSpatialMXCIFEventIndexRandomMovingPoints : RegressionExecution
        {
            private const int NUM_RECTANGLES = 1000;
            private const int NUM_MOVES = 5000;
            private const int WIDTH = 100;
            private const int HEIGHT = 100;
            private static readonly int[] CHECKPOINT_AT = {500, 3000, 4000};
            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create table RectangleTable as (Id string primary key, Px double, Py double, Pw double, Ph double);\n" +
                    "create index MyIndex on RectangleTable((Px,Py,pw,ph) mxcifquadtree(0,0,100,100));\n" +
                    "insert into RectangleTable select Id, X as Px, Y as Py, Width as Pw, Height as Ph from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleTable as pt where rectangle(Px,Py,pw,ph).intersects(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var random = new Random();
                var rectangles = GenerateCoordinates(random, NUM_RECTANGLES, WIDTH, HEIGHT);
                foreach (var r in rectangles) {
                    SendEventRectangle(
                        env,
                        r.Id,
                        r.X.Value,
                        r.Y.Value,
                        r.Width.Value,
                        r.Height.Value);
                }

                env.Milestone(0);

                var milestone = new AtomicLong();
                var deleteQuery = env.CompileFAF("delete from RectangleTable where Id=?:Id:string", path);
                var preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);

                for (var i = 0; i < NUM_MOVES; i++) {
                    var pointMoved = rectangles[random.Next(rectangles.Count)];
                    MovePoint(env, pointMoved, random, preparedDelete);

                    var startX = pointMoved.X - 5;
                    var startY = pointMoved.Y - 5;
                    var bb = new BoundingBox(
                        startX.Value,
                        startY.Value,
                        startX.Value + 10,
                        startY.Value + 10);
                    AssertBBRectangles(env, bb, rectangles);

                    if (Array.BinarySearch(CHECKPOINT_AT, i) >= 0) {
                        log.Info("Checkpoint at " + rectangles.Count);
                        env.MilestoneInc(milestone);
                        preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);
                    }
                }

                env.UndeployAll();
            }

            private void MovePoint(
                RegressionEnvironment env,
                SupportSpatialEventRectangle rectangle,
                Random random,
                EPFireAndForgetPreparedQueryParameterized preparedDelete)
            {
                double newX;
                double newY;

                while (true) {
                    newX = rectangle.X.Value;
                    newY = rectangle.Y.Value;
                    var direction = random.Next(4);
                    if (direction == 0 && newX > 0) {
                        newX--;
                    }

                    if (direction == 1 && newY > 0) {
                        newY--;
                    }

                    if (direction == 2 && newX < WIDTH - 1) {
                        newX++;
                    }

                    if (direction == 3 && newY < HEIGHT - 1) {
                        newY++;
                    }

                    if (BoundingBox.IntersectsBoxIncludingEnd(
                        0,
                        0,
                        WIDTH,
                        HEIGHT,
                        newX,
                        newY,
                        rectangle.Width.Value,
                        rectangle.Height.Value)) {
                        break;
                    }
                }

                // Comment-me-in:
                // log.info("Moving " + rectangle.getId() + " from " + printPoint(rectangle.getX(), rectangle.getY()) + " to " + printPoint(newX, newY));
                preparedDelete.SetObject("Id", rectangle.Id);
                env.Runtime.FireAndForgetService.ExecuteQuery(preparedDelete);

                rectangle.X = newX;
                rectangle.Y = newY;
                SendEventRectangle(
                    env,
                    rectangle.Id,
                    rectangle.X.Value,
                    rectangle.Y.Value,
                    rectangle.Width.Value,
                    rectangle.Height.Value);
            }

            private IList<SupportSpatialEventRectangle> GenerateCoordinates(
                Random random,
                int numPoints,
                int width,
                int height)
            {
                IList<SupportSpatialEventRectangle> result = new List<SupportSpatialEventRectangle>(numPoints);
                for (var i = 0; i < numPoints; i++) {
                    var x = random.Next(width);
                    var y = random.Next(height);
                    var w = random.NextDouble() * width;
                    var h = random.NextDouble() * height;
                    result.Add(new SupportSpatialEventRectangle("P" + i, x, y, w, h));
                }

                return result;
            }
        }

        public class EPLSpatialMXCIFEventIndexRandomIntPointsInSquareUnique : RegressionExecution
        {
            private const int SIZE = 1000;

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create window RectangleWindow#keepall as (Id string, Px double, Py double, Pw double, Ph double);\n" +
                    "create unique index MyIndex on RectangleWindow((Px,Py,pw,ph) mxcifquadtree(0,0,1000,1000));\n" +
                    "insert into RectangleWindow select Id, X as Px, Y as Py, Width as Pw, Height as Ph from SupportSpatialEventRectangle;\n" +
                    "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from RectangleWindow as pt where rectangle(Px,Py,pw,ph).intersects(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var random = new Random();
                var rectangles = GenerateCoordinates(random);
                foreach (var rectangle in rectangles) {
                    SendEventRectangle(
                        env,
                        rectangle.Id,
                        rectangle.X.Value,
                        rectangle.Y.Value,
                        rectangle.Width.Value,
                        rectangle.Height.Value);
                }

                env.Milestone(0);

                // find all individually
                var listener = env.Listener("out");
                foreach (var r in rectangles) {
                    env.SendEventBean(new SupportSpatialAABB("", r.X.Value, r.Y.Value, 0.1, 0.1));
                    Assert.AreEqual(r.Id, listener.AssertOneGetNewAndReset().Get("c0"));
                }

                // get all content
                AssertAllRectangles(env, rectangles, 0, 0, SIZE, SIZE);

                env.Milestone(1);

                // add duplicate: note these events are still is named window
                foreach (var r in rectangles) {
                    try {
                        SendEventRectangle(env, r.Id, r.X.Value, r.Y.Value, r.Width.Value, r.Height.Value);
                        Assert.Fail();
                    }
                    catch (Exception t) {
                        // expected
                    }
                }

                // remove all
                IList<string> idList = new List<string>();
                foreach (var p in rectangles) {
                    idList.Add(p.Id);
                }

                while (!idList.IsEmpty()) {
                    var first = idList.Count > 100 ? idList.SubList(0, 100) : idList;
                    env.CompileExecuteFAF(BuildDeleteQueryWithInClause("RectangleWindow", "Id", first), path);
                    idList.RemoveAll(first);
                }

                env.SendEventBean(new SupportSpatialAABB("", 0, 0, SIZE, SIZE));
                Assert.IsFalse(env.Listener("out").GetAndClearIsInvoked());

                env.Milestone(2);

                env.SendEventBean(new SupportSpatialAABB("", 0, 0, SIZE, SIZE));
                Assert.IsFalse(env.Listener("out").GetAndClearIsInvoked());

                env.UndeployAll();
            }

            private static ICollection<SupportSpatialEventRectangle> GenerateCoordinates(Random random)
            {
                IDictionary<UniformPair<int>, SupportSpatialEventRectangle> points =
                    new Dictionary<UniformPair<int>, SupportSpatialEventRectangle>();
                while (points.Count < SIZE) {
                    var x = random.Next(SIZE);
                    var y = random.Next(SIZE);
                    points.Put(
                        new UniformPair<int>(x, y),
                        new SupportSpatialEventRectangle(
                            Convert.ToString(x) + "_" + Convert.ToString(y),
                            x,
                            y,
                            0.001d,
                            0.001d));
                }

                return points.Values;
            }
        }
    }
} // end of namespace