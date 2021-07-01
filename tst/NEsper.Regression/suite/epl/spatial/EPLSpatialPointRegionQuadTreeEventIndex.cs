///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a coPy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportSpatialUtil;

namespace com.espertech.esper.regressionlib.suite.epl.spatial
{
    public class EPLSpatialPointRegionQuadTreeEventIndex
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
            WithUnindexed(execs);
            WithUnusedOnTrigger(execs);
            WithUnusedNamedWindowFireAndForget(execs);
            WithOnTriggerNWInsertRemove(execs);
            WithOnTriggerTable(execs);
            WithChoiceOfTwo(execs);
            WithUnique(execs);
            WithPerformance(execs);
            WithChoiceBetweenIndexTypes(execs);
            WithNWFireAndForgetPerformance(execs);
            WithTableFireAndForget(execs);
            WithOnTriggerContextParameterized(execs);
            WithExpression(execs);
            WithEdgeSubdivide(execs);
            WithRandomDoublePointsWRandomQuery(execs);
            WithRandomIntPointsInSquareUnique(execs);
            WithRandomMovingPoints(execs);
            WithTableSimple(execs);
            WithTableSubdivideDeepAddDestroy(execs);
            WithTableSubdivideDestroy(execs);
            WithTableSubdivideMergeDestroy(execs);
            WithSubqNamedWindowIndexShare(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSubqNamedWindowIndexShare(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexSubqNamedWindowIndexShare());
            return execs;
        }

        public static IList<RegressionExecution> WithTableSubdivideMergeDestroy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexTableSubdivideMergeDestroy());
            return execs;
        }

        public static IList<RegressionExecution> WithTableSubdivideDestroy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexTableSubdivideDestroy());
            return execs;
        }

        public static IList<RegressionExecution> WithTableSubdivideDeepAddDestroy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexTableSubdivideDeepAddDestroy());
            return execs;
        }

        public static IList<RegressionExecution> WithTableSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexTableSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithRandomMovingPoints(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexRandomMovingPoints());
            return execs;
        }

        public static IList<RegressionExecution> WithRandomIntPointsInSquareUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexRandomIntPointsInSquareUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithRandomDoublePointsWRandomQuery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexRandomDoublePointsWRandomQuery());
            return execs;
        }

        public static IList<RegressionExecution> WithEdgeSubdivide(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexEdgeSubdivide());
            return execs;
        }

        public static IList<RegressionExecution> WithExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithOnTriggerContextParameterized(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexOnTriggerContextParameterized());
            return execs;
        }

        public static IList<RegressionExecution> WithTableFireAndForget(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexTableFireAndForget());
            return execs;
        }

        public static IList<RegressionExecution> WithNWFireAndForgetPerformance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexNWFireAndForgetPerformance());
            return execs;
        }

        public static IList<RegressionExecution> WithChoiceBetweenIndexTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexChoiceBetweenIndexTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithPerformance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexPerformance());
            return execs;
        }

        public static IList<RegressionExecution> WithUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithChoiceOfTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexChoiceOfTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithOnTriggerTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexOnTriggerTable());
            return execs;
        }

        public static IList<RegressionExecution> WithOnTriggerNWInsertRemove(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexOnTriggerNWInsertRemove(false));
            execs.Add(new EPLSpatialPREventIndexOnTriggerNWInsertRemove(true));
            return execs;
        }

        public static IList<RegressionExecution> WithUnusedNamedWindowFireAndForget(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexUnusedNamedWindowFireAndForget());
            return execs;
        }

        public static IList<RegressionExecution> WithUnusedOnTrigger(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexUnusedOnTrigger());
            return execs;
        }

        public static IList<RegressionExecution> WithUnindexed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSpatialPREventIndexUnindexed());
            return execs;
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            RegressionPath path,
            string hint,
            string expectedIndexName)
        {
            var epl = "@Name('s0') " +
                      IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                      hint +
                      "on SupportSpatialAABB as aabb select mpw.Id as c0 from MyPointWindow as mpw " +
                      "where aabb.Category = mpw.Category and point(Px, Py).inside(rectangle(X, Y, Width, Height))\n";
            env.CompileDeploy(epl, path).AddListener("s0");

            var plan = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(expectedIndexName, plan.Tables[0].IndexName);

            env.SendEventBean(new SupportSpatialAABB("R1", 9, 14, 1.0001, 1.0001, "Y"));
            Assert.AreEqual("P2", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

            env.UndeployModuleContaining("s0");
        }

        internal class EPLSpatialPREventIndexNWFireAndForgetPerformance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyPointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "insert into MyPointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "create index Idx on MyPointWindow( (Px, Py) pointregionquadtree(0, 0, 100, 100));\n";
                env.CompileDeploy(epl, path);

                var random = new Random();
                IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
                for (var i = 0; i < 10000; i++) {
                    var px = random.NextDouble() * 100;
                    var py = random.NextDouble() * 100;
                    var point = new SupportSpatialPoint("P" + Convert.ToString(i), px, py);
                    env.SendEventBean(point);
                    points.Add(point);
                    // Comment-me-in: log.info("Point P" + i + " " + Px + " " + Py);
                }

                var compiled = env.CompileFAF(
                    "select * from MyPointWindow where point(Px,Py).inside(rectangle(?::double,?::double,?::double,?::double))",
                    path);
                var prepared = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
                var start = PerformanceObserver.MilliTime;
                var fields = new[] {"Id"};
                for (var i = 0; i < 500; i++) {
                    var x = random.NextDouble() * 100;
                    var y = random.NextDouble() * 100;
                    // Comment-me-in: log.info("Query " + x + " " + y + " " + width + " " + height);

                    prepared.SetObject(1, x);
                    prepared.SetObject(2, y);
                    prepared.SetObject(3, 5d);
                    prepared.SetObject(4, 5d);
                    var events = env.Runtime.FireAndForgetService.ExecuteQuery(prepared).Array;
                    var expected = GetExpected(points, x, y, 5, 5);
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(events, fields, expected);
                }

                var delta = PerformanceObserver.MilliTime - start;
                Assert.That(delta, Is.LessThan(1000), "delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexChoiceBetweenIndexTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "@Name('win') create window MyPointWindow#keepall as (Id string, Category string, Px double, Py double);\n" +
                    "@Name('insert') insert into MyPointWindow select Id, Category, Px, Py from SupportSpatialPoint;\n" +
                    "@Name('Idx1') create index IdxHash on MyPointWindow(Category);\n" +
                    "@Name('Idx2') create index IdxQuadtree on MyPointWindow((Px, Py) pointregionquadtree(0, 0, 100, 100));\n";
                env.CompileDeploy(epl, path);

                SendPoint(env, "P1", 10, 15, "X");
                SendPoint(env, "P2", 10, 15, "Y");
                SendPoint(env, "P3", 10, 15, "Z");

                AssertIndexChoice(env, path, "", "IdxQuadtree");
                AssertIndexChoice(env, path, "@Hint('index(IdxHash, bust)')", "IdxQuadtree");

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('win') create window MyPointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "@Name('insert') insert into MyPointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('Idx') create unique index Idx on MyPointWindow( (Px, Py) pointregionquadtree(0, 0, 100, 100));\n" +
                          "@Name('out') on SupportSpatialAABB select mpw.Id as c0 from MyPointWindow as mpw where point(Px, Py).inside(rectangle(X, Y, Width, Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SendPoint(env, "P1", 10, 15);
                try {
                    SendPoint(env, "P2", 10, 15);
                    Assert.Fail();
                }
                catch (Exception ex) { // we have a handler
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Unexpected exception in statement 'win': Unique index violation, index 'Idx' is a unique index and key '(10.0d,15.0d)' already exists");
                }

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexPerformance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyPointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "insert into MyPointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "create index Idx on MyPointWindow( (Px, Py) pointregionquadtree(0, 0, 100, 100));\n" +
                          "@Name('s0') on SupportSpatialAABB select mpw.Id as c0 from MyPointWindow as mpw where point(Px, Py).inside(rectangle(X, Y, Width, Height));\n";
                env.CompileDeploy(epl).AddListener("s0");

                for (var x = 0; x < 100; x++) {
                    for (var y = 0; y < 100; y++) {
                        env.SendEventBean(
                            new SupportSpatialPoint(Convert.ToString(x) + "_" + Convert.ToString(y), x, y));
                    }
                }

                var start = PerformanceObserver.MilliTime;
                for (var x = 0; x < 100; x++) {
                    for (var y = 0; y < 100; y++) {
                        env.SendEventBean(new SupportSpatialAABB("R", x, y, 0.5, 0.5));
                        Assert.AreEqual(
                            Convert.ToString(x) + "_" + Convert.ToString(y),
                            env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                    }
                }

                var delta = PerformanceObserver.MilliTime - start;
#if DEBUG
                Assert.That(delta, Is.LessThan(2500), "delta=" + delta);
#else
                Assert.That(delta, Is.LessThan(1000), "delta=" + delta);
#endif

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexUnusedNamedWindowFireAndForget : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window PointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "create index MyIndex on PointWindow((Px,Py) pointregionquadtree(0,0,100,100,2,12));\n" +
                          "insert into PointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointWindow as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path);

                env.CompileExecuteFAF("delete from PointWindow where Id='P1'", path);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexTableFireAndForget : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTable(Id string primary key, tx double, ty double)", path);
                env.CompileDeploy("insert into MyTable select Id, Px as tx, Py as ty from SupportSpatialPoint", path);
                env.SendEventBean(new SupportSpatialPoint("P1", 50d, 50d));
                env.SendEventBean(new SupportSpatialPoint("P2", 49d, 49d));
                env.CompileDeploy(
                    "create index MyIdxWithExpr on MyTable( (tx, ty) pointregionquadtree(0, 0, 100, 100))",
                    path);

                var result = env.CompileExecuteFAF(
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                    "select Id as c0 from MyTable where point(tx, ty).inside(rectangle(45, 45, 10, 10))",
                    path);
                SupportQueryPlanIndexHook.AssertFAFAndReset("MyIdxWithExpr", "EventTableQuadTreePointRegion");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    result.Array,
                    new[] {"c0"},
                    new[] {new object[] {"P1"}, new object[] {"P2"}});

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTable(Id string primary key, tx double, ty double)", path);
                env.CompileExecuteFAF("insert into MyTable values ('P1', 50, 30)", path);
                env.CompileExecuteFAF("insert into MyTable values ('P2', 50, 28)", path);
                env.CompileExecuteFAF("insert into MyTable values ('P3', 50, 30)", path);
                env.CompileExecuteFAF("insert into MyTable values ('P4', 49, 29)", path);
                env.CompileDeploy(
                    "create index MyIdxWithExpr on MyTable((tx*10, ty*10) pointregionquadtree(0, 0, 1000, 1000))",
                    path);

                var eplOne = "@Name('s0') " +
                             IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                             "on SupportSpatialAABB select tbl.Id as c0 from MyTable as tbl where point(tx, ty).inside(rectangle(X, Y, Width, Height))";
                env.CompileDeploy(eplOne, path).AddListener("s0");
                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIdxWithExpr",
                    "non-unique hash={} btree={} advanced={pointregionquadtree(tx*10,ty*10)}");
                // Invalid use of index, the properties match and the bounding boxes do not match due to "x*10" missing.
                env.UndeployModuleContaining("s0");

                var eplTwo = "@Name('s0') " +
                             IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                             "on SupportSpatialAABB select tbl.Id as c0 from MyTable as tbl where point(tx*10, tbl.ty*10).inside(rectangle(X, Y, Width, Height))";
                env.CompileDeploy(eplTwo, path).AddListener("s0");
                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIdxWithExpr",
                    "non-unique hash={} btree={} advanced={pointregionquadtree(tx*10,ty*10)}");
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, null, null, null, null, null);
                AssertRectanglesManyRow(
                    env,
                    env.Listener("s0"),
                    Collections.SingletonList(new BoundingBox(500, 300, 501, 301)),
                    "P1,P3");

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexChoiceOfTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create table MyPointTable(" +
                    " Id string primary key," +
                    " X1 double, Y1 double, \n" +
                    " X2 double, Y2 double);\n" +
                    "create index Idx1 on MyPointTable( (X1, Y1) pointregionquadtree(0, 0, 100, 100));\n" +
                    "create index Idx2 on MyPointTable( (X2, Y2) pointregionquadtree(0, 0, 100, 100));\n" +
                    "on SupportSpatialDualPoint dp merge MyPointTable t where dp.Id = t.Id when not matched then insert select dp.Id as Id,X1,Y1,X2,Y2;\n";
                env.CompileDeploy(epl, path);

                var listener = new SupportUpdateListener();
                var textOne = "@Name('s0') " +
                              IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                              "on SupportSpatialAABB select tbl.Id as c0 from MyPointTable as tbl where point(X1, Y1).inside(rectangle(X, Y, Width, Height))";
                env.CompileDeploy(textOne, path).Statement("s0").AddListener(listener);

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "Idx1",
                    "non-unique hash={} btree={} advanced={pointregionquadtree(X1,Y1)}");

                var textTwo = "@Name('s1') " +
                              IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                              "on SupportSpatialAABB select tbl.Id as c0 from MyPointTable as tbl where point(tbl.X2, Y2).inside(rectangle(X, Y, Width, Height))";
                env.CompileDeploy(textTwo, path).Statement("s1").AddListener(listener);
                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "Idx2",
                    "non-unique hash={} btree={} advanced={pointregionquadtree(X2,Y2)}");

                env.SendEventBean(new SupportSpatialDualPoint("P1", 10, 10, 60, 60));
                env.SendEventBean(new SupportSpatialDualPoint("P2", 55, 20, 4, 88));

                AssertRectanglesSingleValue(env, listener, BOXES, "P1", "P2", "P2", "P1", "P1");

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexSubqNamedWindowIndexShare : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Hint('enable_window_subquery_indexshare') create window MyWindow#length(5) as select * from SupportSpatialPoint;\n" +
                    "create index MyIndex on MyWindow((Px,Py) pointregionquadtree(0,0,100,100));\n" +
                    "insert into MyWindow select * from SupportSpatialPoint;\n" +
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                    "@Name('out') select (select Id from MyWindow as mw where point(mw.Px,mw.Py).inside(rectangle(aabb.X,aabb.Y,aabb.Width,aabb.Height))).aggregate('', \n" +
                    "  (result, item) -> result || (case when result='' then '' else ',' end) || item) as c0 from SupportSpatialAABB aabb";
                env.CompileDeploy(epl).AddListener("out");

                var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
                Assert.AreEqual(
                    "non-unique hash={} btree={} advanced={pointregionquadtree(Px,Py)}",
                    subquery.Tables[0].IndexDesc);
                Assert.AreEqual("MyIndex", subquery.Tables[0].IndexName);

                SendPoint(env, "P1", 10, 40);
                AssertRectanglesSingleValue(env, env.Listener("out"), BOXES, "P1", "", "", "", "");

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexUnusedOnTrigger : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindow#length(5) as select * from SupportSpatialPoint;\n" +
                          "create index MyIndex on MyWindow((Px,Py) pointregionquadtree(0,0,100,100));\n" +
                          "insert into MyWindow select * from SupportSpatialPoint;\n";
                env.CompileDeploy(epl, path);

                SendPoint(env, "P1", 5, 5);
                SendPoint(env, "P2", 55, 60);

                RunIndexUnusedConstantsOnly(env, path);
                RunIndexUnusedPointValueDepends(env, path);
                RunIndexUnusedRectValueDepends(env, path);

                env.UndeployAll();
            }

            private void RunIndexUnusedRectValueDepends(
                RegressionEnvironment env,
                RegressionPath path)
            {
                var epl = "@Name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "on SupportSpatialAABB as aabb select points.Id as c0 " +
                          "from MyWindow as points where point(Px, Py).inside(rectangle(Px,Py,1,1))";
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);

                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1,P2", "P1,P2", "P1,P2", "P1,P2", "P1,P2");

                env.UndeployModuleContaining("s0");
            }

            private void RunIndexUnusedPointValueDepends(
                RegressionEnvironment env,
                RegressionPath path)
            {
                var epl = "@Name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "on SupportSpatialAABB as aabb select points.Id as c0 " +
                          "from MyWindow as points where point(Px + X, Py + Y).inside(rectangle(X,Y,Width,Height))";
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);

                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1", "P1", "P1", "P1", "P1");

                env.UndeployModuleContaining("s0");
            }

            private void RunIndexUnusedConstantsOnly(
                RegressionEnvironment env,
                RegressionPath path)
            {
                var epl = "@Name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "@Name('out') on SupportSpatialAABB as aabb select points.Id as c0 " +
                          "from MyWindow as points where point(0, 0).inside(rectangle(X,Y,Width,Height))";
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);

                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1,P2", null, null, null, null);

                env.UndeployModuleContaining("s0");
            }
        }

        internal class EPLSpatialPREventIndexUnindexed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select point(XOffset, YOffset).inside(rectangle(X, Y, Width, Height)) as c0 from SupportEventRectangleWithOffset");
                env.AddListener("s0");

                SendAssert(env, env.Listener("s0"), 1, 1, 0, 0, 2, 2, true);
                SendAssert(env, env.Listener("s0"), 3, 1, 0, 0, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 2, 1, 0, 0, 2, 2, false);

                env.Milestone(0);

                SendAssert(env, env.Listener("s0"), 1, 3, 0, 0, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 1, 2, 0, 0, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 0, 0, 1, 1, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 1, 0, 1, 1, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 0, 1, 1, 1, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 1, 1, 1, 1, 2, 2, true);
                SendAssert(env, env.Listener("s0"), 2.9999, 2.9999, 1, 1, 2, 2, true);

                env.Milestone(1);

                SendAssert(env, env.Listener("s0"), 3, 2.9999, 1, 1, 2, 2, false);
                SendAssert(env, env.Listener("s0"), 2.9999, 3, 1, 1, 2, 2, false);
                SendAssertWNull(env, env.Listener("s0"), null, 0d, 0d, 0d, 0d, 0d, null);
                SendAssertWNull(env, env.Listener("s0"), 0d, 0d, 0d, null, 0d, 0d, null);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexOnTriggerContextParameterized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context CtxBox initiated by SupportEventRectangleWithOffset box;\n" +
                          "context CtxBox create window MyWindow#keepall as SupportSpatialPoint;\n" +
                          "context CtxBox create index MyIndex on MyWindow((Px+context.box.XOffset, Py+context.box.YOffset) pointregionquadtree(context.box.X, context.box.Y, context.box.Width, context.box.Height));\n" +
                          "context CtxBox on SupportSpatialPoint(Category = context.box.Id) merge MyWindow when not matched then insert select *;\n" +
                          "@Name('s0') context CtxBox on SupportSpatialAABB(Category = context.box.Id) aabb " +
                          "  select points.Id as c0 from MyWindow points where point(Px, Py).inside(rectangle(X, Y, Width, Height))";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventRectangleWithOffset("NW", 0d, 0d, 0d, 0d, 50d, 50d));
                env.SendEventBean(new SupportEventRectangleWithOffset("SE", 0d, 0d, 50d, 50d, 50d, 50d));
                SendPoint(env, "P1", 60, 90, "SE");
                SendPoint(env, "P2", 5, 20, "NW");

                env.SendEventBean(new SupportSpatialAABB("R1", 60, 60, 40, 40, "SE"));
                Assert.AreEqual("P1", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.SendEventBean(new SupportSpatialAABB("R2", 0, 0, 5.0001, 20.0001, "NW"));
                Assert.AreEqual("P2", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.Milestone(0);

                env.SendEventBean(new SupportSpatialAABB("R3", 0, 0, 5, 30, "NW"));
                env.SendEventBean(new SupportSpatialAABB("R3", 0, 0, 30, 20, "NW"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexOnTriggerNWInsertRemove : RegressionExecution
        {
            private readonly bool soda;

            public EPLSpatialPREventIndexOnTriggerNWInsertRemove(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(soda, "create window MyWindow#length(5) as select * from SupportSpatialPoint", path);
                env.CompileDeploy(
                    soda,
                    "create index MyIndex on MyWindow((Px,Py) pointregionquadtree(0,0,100,100))",
                    path);
                env.CompileDeploy(soda, "insert into MyWindow select * from SupportSpatialPoint", path);

                var epl = "@Name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          " on SupportSpatialAABB as aabb " +
                          "select points.Id as c0 from MyWindow as points where point(Px,Py).inside(rectangle(X,Y,Width,Height))";
                env.CompileDeploy(soda, epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIndex",
                    "non-unique hash={} btree={} advanced={pointregionquadtree(Px,Py)}");

                SendPoint(env, "P1", 10, 40);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1", null, null, null, null);

                env.Milestone(0);

                SendPoint(env, "P2", 80, 80);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1", null, null, "P2", null);

                SendPoint(env, "P3", 10, 40);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1,P3", null, null, "P2", null);

                env.Milestone(1);

                SendPoint(env, "P4", 60, 40);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1,P3", "P4", null, "P2", "P4");

                SendPoint(env, "P5", 20, 75);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P1,P3", "P4", "P5", "P2", "P4");

                SendPoint(env, "P6", 50, 50);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P3", "P4", "P5", "P2,P6", "P4,P6");

                env.Milestone(2);

                SendPoint(env, "P7", 0, 0);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P3,P7", "P4", "P5", "P6", "P4,P6");

                SendPoint(env, "P8", 99.999, 0);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P7", "P4,P8", "P5", "P6", "P4,P6");

                env.Milestone(3);

                SendPoint(env, "P9", 0, 99.999);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P7", "P8", "P5,P9", "P6", "P6");

                SendPoint(env, "P10", 99.999, 99.999);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P7", "P8", "P9", "P6,P10", "P6");

                SendPoint(env, "P11", 0, 0);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P7,P11", "P8", "P9", "P10", null);

                env.UndeployAll();
            }
        }

        internal class EPLSpatialPREventIndexOnTriggerTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create table MyPointTable(my_x double primary key, my_y double primary key, my_Id string);\n" +
                    "@Audit create index MyIndex on MyPointTable( (my_x, my_y) pointregionquadtree(0, 0, 100, 100));\n" +
                    "on SupportSpatialPoint ssp merge MyPointTable where ssp.Px = my_x and ssp.Py = my_y when not matched then insert select Px as my_x, Py as my_y, Id as my_Id;\n" +
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                    "@Audit @Name('s0') on SupportSpatialAABB select my_Id as c0 from MyPointTable as c0 where point(my_x, my_y).inside(rectangle(X, Y, Width, Height))";
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIndex",
                    "non-unique hash={} btree={} advanced={pointregionquadtree(my_x,my_y)}");

                SendPoint(env, "P1", 55, 45);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, null, "P1", null, null, "P1");

                SendPoint(env, "P2", 45, 45);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P2", "P1", null, null, "P1,P2");

                env.Milestone(0);

                SendPoint(env, "P3", 55, 55);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P2", "P1", null, "P3", "P1,P2,P3");

                env.CompileExecuteFAF("delete from MyPointTable where my_x = 55 and my_y = 45", path);
                SendPoint(env, "P4", 45, 55);
                AssertRectanglesManyRow(env, env.Listener("s0"), BOXES, "P2", null, "P4", "P3", "P2,P3,P4");

                env.UndeployAll();
            }
        }

        public class EPLSpatialPREventIndexEdgeSubdivide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window PointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "create index MyIndex on PointWindow((Px,Py) pointregionquadtree(0,0,100,100,2,5));\n" +
                          "insert into PointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointWindow as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var boxesLevel4 = GetLevel5Boxes();
                var count = 0;
                IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
                foreach (var bb in boxesLevel4) {
                    SendAddPoint(env, points, "A" + count, bb.MinX, bb.MinY);
                    SendAddPoint(env, points, "B" + count, bb.MinX, bb.MinY);
                    SendAddPoint(env, points, "C" + count, bb.MinX, bb.MinY);
                    count++;
                }

                AssertAllPoints(env, points, 0, 0, 100, 100);

                env.Milestone(0);

                RemoveAllABPoints(env, path, points);
                AssertAllPoints(env, points, 0, 0, 100, 100);

                env.Milestone(1);

                AssertAllPoints(env, points, 0, 0, 100, 100);
                RemoveEverySecondPoints(env, path, points);
                AssertAllPoints(env, points, 0, 0, 100, 100);

                env.Milestone(2);

                AssertAllPoints(env, points, 0, 0, 100, 100);
                RemoveEverySecondPoints(env, path, points);
                AssertAllPoints(env, points, 0, 0, 100, 100);

                env.Milestone(3);

                AssertAllPoints(env, points, 0, 0, 100, 100);
                RemoveAllPoints(env, path, points);
                AssertAllPoints(env, points, 0, 0, 100, 100);

                env.Milestone(4);

                RemoveAllPoints(env, path, points);

                env.UndeployAll();
            }

            private void RemoveEverySecondPoints(
                RegressionEnvironment env,
                RegressionPath path,
                IList<SupportSpatialPoint> points)
            {
                using (var enumerator = points.GetEnumerator()) {
                    var count = 0;
                    IList<string> ids = new List<string>();
                    for (var ii = 0; ii < points.Count; ii++) {
                        var p = points[ii];
                        if (count % 2 == 1) {
                            points.RemoveAt(ii--);
                            ids.Add(p.Id);
                        }

                        count++;
                    }

                    var query = BuildDeleteQueryWithInClause("PointWindow", "Id", ids);
                    env.CompileExecuteFAF(query, path);
                }
            }

            private void RemoveAllPoints(
                RegressionEnvironment env,
                RegressionPath path,
                IList<SupportSpatialPoint> points)
            {
                IList<string> ids = new List<string>();
                for (var ii = 0; ii < points.Count; ii++) {
                    var p = points[ii];
                    points.RemoveAt(ii--);
                    ids.Add(p.Id);
                }

                if (ids.IsEmpty()) {
                    return;
                }

                var query = BuildDeleteQueryWithInClause("PointWindow", "Id", ids);
                env.CompileExecuteFAF(query, path);
            }

            private void RemoveAllABPoints(
                RegressionEnvironment env,
                RegressionPath path,
                IList<SupportSpatialPoint> points)
            {
                IList<string> ids = new List<string>();
                for (var ii = 0; ii < points.Count; ii++) {
                    var p = points[ii];
                    if (p.Id[0] == 'A' || p.Id[0] == 'B') {
                        points.RemoveAt(ii--);
                        ids.Add(p.Id);
                    }
                }

                var query = BuildDeleteQueryWithInClause("PointWindow", "Id", ids);
                env.CompileExecuteFAF(query, path);
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

        public class EPLSpatialPREventIndexRandomDoublePointsWRandomQuery : RegressionExecution
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
                var epl = "create window PointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "create index MyIndex on PointWindow((Px,Py) pointregionquadtree(0,0,100,100));\n" +
                          "insert into PointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointWindow as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var random = new Random();
                var points = RandomPoints(random, NUM_POINTS, X, Y, WIDTH, HEIGHT);
                foreach (var point in points) {
                    SendPoint(env, point.Id, point.Px, point.Py);
                    // Comment-me-in: log.info("Point: " + point);
                }

                env.Milestone(0);

                for (var i = 0; i < NUM_QUERIES_AFTER_LOAD; i++) {
                    RandomQuery(env, random, points);
                }

                var milestone = new AtomicLong();
                var deleteQuery = env.CompileFAF("delete from PointWindow where Id=?::string", path);
                var preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);

                while (!points.IsEmpty()) {
                    var removed = RandomRemove(random, points);
                    preparedDelete.SetObject(1, removed.Id);
                    env.Runtime.FireAndForgetService.ExecuteQuery(preparedDelete);

                    for (var i = 0; i < NUM_QUERIES_AFTER_EACH_REMOVE; i++) {
                        RandomQuery(env, random, points);
                    }

                    if (Array.BinarySearch(CHECKPOINT_REMAINING, points.Count) >= 0) {
                        log.Info("Checkpoint at " + points.Count);
                        env.MilestoneInc(milestone);
                        preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);
                    }
                }

                env.UndeployAll();
            }

            private SupportSpatialPoint RandomRemove(
                Random random,
                IList<SupportSpatialPoint> points)
            {
                var index = random.Next(points.Count);
                return points.DeleteAt(index);
            }

            private void RandomQuery(
                RegressionEnvironment env,
                Random random,
                IList<SupportSpatialPoint> points)
            {
                var bbWidth = random.NextDouble() * WIDTH * 1.5;
                var bbHeight = random.NextDouble() * HEIGHT * 1.5;
                var bbMinX = random.NextDouble() * WIDTH + X * 0.8;
                var bbMinY = random.NextDouble() * HEIGHT + Y * 0.8;
                var bbMaxX = bbMinX + bbWidth;
                var bbMaxY = bbMinY + bbHeight;
                var boundingBox = new BoundingBox(bbMinX, bbMinY, bbMaxX, bbMaxY);
                // Comment-me-in: log.info("Query: " + boundingBox);
                AssertBBPoints(env, boundingBox, points);
            }
        }

        public class EPLSpatialPREventIndexRandomIntPointsInSquareUnique : RegressionExecution
        {
            private const int SIZE = 1000;

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window PointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "create unique index MyIndex on PointWindow((Px,Py) pointregionquadtree(0,0,1000,1000));\n" +
                          "insert into PointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointWindow as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var random = new Random();
                var points = GenerateCoordinates(random);
                foreach (var point in points) {
                    SendPoint(env, point.Id, point.Px, point.Py);
                }

                env.Milestone(0);

                // find all individually
                var listener = env.Listener("out");
                foreach (var p in points) {
                    env.SendEventBean(new SupportSpatialAABB("", p.Px.Value, p.Py.Value, 1, 1));
                    Assert.AreEqual(p.Id, listener.AssertOneGetNewAndReset().Get("c0"));
                }

                // get all content
                AssertAllPoints(env, points, 0, 0, SIZE, SIZE);

                env.Milestone(1);

                // add duplicate: note these events are still is named window
                foreach (var p in points) {
                    try {
                        SendPoint(env, p.Id, p.Px, p.Py);
                        Assert.Fail();
                    }
                    catch (Exception) {
                        // expected
                    }
                }

                // remove all
                IList<string> ids = new List<string>();
                foreach (var p in points) {
                    ids.Add(p.Id);
                }

                while (!ids.IsEmpty()) {
                    var first = ids.Count < 100 ? ids : ids.SubList(0, 99);
                    var deleteQuery = BuildDeleteQueryWithInClause("PointWindow", "Id", first);
                    env.CompileExecuteFAF(deleteQuery, path);
                    ids.RemoveAll(first.ToArray());
                }

                env.SendEventBean(new SupportSpatialAABB("", 0, 0, SIZE, SIZE));
                Assert.IsFalse(env.Listener("out").GetAndClearIsInvoked());

                env.Milestone(2);

                env.SendEventBean(new SupportSpatialAABB("", 0, 0, SIZE, SIZE));
                Assert.IsFalse(env.Listener("out").GetAndClearIsInvoked());

                env.UndeployAll();
            }

            private static ICollection<SupportSpatialPoint> GenerateCoordinates(Random random)
            {
                IDictionary<UniformPair<int>, SupportSpatialPoint> points =
                    new Dictionary<UniformPair<int>, SupportSpatialPoint>();
                while (points.Count < SIZE) {
                    var x = random.Next(SIZE);
                    var y = random.Next(SIZE);
                    points.Put(
                        new UniformPair<int>(x, y),
                        new SupportSpatialPoint(Convert.ToString(x) + "_" + Convert.ToString(y), x, y));
                }

                return points.Values;
            }
        }

        public class EPLSpatialPREventIndexRandomMovingPoints : RegressionExecution
        {
            private const int NUM_POINTS = 1000;
            private const int NUM_MOVES = 5000;
            private const int WIDTH = 100;
            private const int HEIGHT = 100;
            private static readonly int[] CHECKPOINT_AT = {500, 3000, 4000};
            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create table PointTable as (Id string primary key, Px double, Py double);\n" +
                          "create index MyIndex on PointTable((Px,Py) pointregionquadtree(0,0,100,100));\n" +
                          "insert into PointTable select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointTable as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                var random = new Random();
                var points = GenerateCoordinates(random, NUM_POINTS, WIDTH, HEIGHT);
                foreach (var point in points) {
                    SendPoint(env, point.Id, point.Px, point.Py);
                }

                env.Milestone(0);
                var deleteQuery = env.CompileFAF("delete from PointTable where Id=?::string", path);
                var preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);

                var milestone = new AtomicLong();
                for (var i = 0; i < NUM_MOVES; i++) {
                    var pointMoved = points[random.Next(points.Count)];
                    MovePoint(env, path, pointMoved, random, preparedDelete);

                    var startX = pointMoved.Px.Value - 5;
                    var startY = pointMoved.Py.Value - 5;
                    var bb = new BoundingBox(startX, startY, startX + 10, startY + 10);
                    AssertBBPoints(env, bb, points);

                    if (Array.BinarySearch(CHECKPOINT_AT, i) >= 0) {
                        log.Info("Checkpoint at " + points.Count);
                        env.MilestoneInc(milestone);
                        preparedDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(deleteQuery);
                    }
                }

                env.UndeployAll();
            }

            private void MovePoint(
                RegressionEnvironment env,
                RegressionPath path,
                SupportSpatialPoint point,
                Random random,
                EPFireAndForgetPreparedQueryParameterized preparedDelete)
            {
                var direction = random.Next(4);
                var newX = point.Px;
                var newY = point.Py;
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

                // Comment-me-in:
                // log.info("Moving " + point.getId() + " from " + printPoint(point.getX(), point.getY()) + " to " + printPoint(newX, newY));
                preparedDelete.SetObject(1, point.Id);
                env.Runtime.FireAndForgetService.ExecuteQuery(preparedDelete);

                point.Px = newX;
                point.Py = newY;
                SendPoint(env, point.Id, point.Px, point.Py);
            }

            private IList<SupportSpatialPoint> GenerateCoordinates(
                Random random,
                int numPoints,
                int width,
                int height)
            {
                IList<SupportSpatialPoint> result = new List<SupportSpatialPoint>(numPoints);
                for (var i = 0; i < numPoints; i++) {
                    var x = random.Next(width);
                    var y = random.Next(height);
                    result.Add(new SupportSpatialPoint("P" + i, x, y));
                }

                return result;
            }
        }

        public class EPLSpatialPREventIndexTableSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window PointWindow#keepall as (Id string, Px double, Py double);\n" +
                          "create index MyIndex on PointWindow((Px,Py) pointregionquadtree(0,0,100,100,2,12));\n" +
                          "insert into PointWindow select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointWindow as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SendPoint(env, "P0", 1.9290410254557688, 79.2596477701767);
                SendPoint(env, "P1", 22.481380138332298, 38.21826613078289);
                SendPoint(env, "P2", 96.68069967422869, 60.135596079815734);
                SendPoint(env, "P3", 2.013086221651661, 79.96973017670842);
                SendPoint(env, "P4", 72.7141155566794, 34.769073156981754);
                SendPoint(env, "P5", 99.3778672522394, 97.26599233260606);
                SendPoint(env, "P6", 92.5721971936789, 45.52450892745069);
                SendPoint(env, "P7", 64.81513547235994, 74.40317040273223);
                SendPoint(env, "P8", 34.431526832055994, 77.1868630618566);
                SendPoint(env, "P9", 63.94019334876596, 60.49807218353348);
                SendPoint(env, "P10", 72.6304354938367, 33.08578043563804);
                SendPoint(env, "P11", 67.34486150692311, 23.93727603716781);
                SendPoint(env, "P12", 3.2289468086465156, 21.0564103499303);
                SendPoint(env, "P13", 54.93362964889536, 76.95495628291773);
                SendPoint(env, "P14", 62.626040886628786, 37.228228790772334);
                SendPoint(env, "P15", 31.89777659905859, 15.41080966535403);
                SendPoint(env, "P16", 54.54495428051385, 50.57461489577466);
                SendPoint(env, "P17", 72.07758279891948, 47.84348117893323);
                SendPoint(env, "P18", 96.10730711977887, 59.22231623726726);
                SendPoint(env, "P19", 1.4354270415599113, 20.003636602020634);
                SendPoint(env, "P20", 17.252052662019757, 10.711353613675922);
                SendPoint(env, "P21", 9.460168333656016, 76.32486040394515);

                env.Milestone(0);

                var bb = new BoundingBox(32.53403315866078, 2.7359221041404314, 69.34282527128134, 80.49662463068397);
                env.SendEventBean(new SupportSpatialAABB("", bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY));
                var received = SortJoinProperty(env.Listener("out").GetAndResetLastNewData(), "c0");
                Assert.AreEqual("P7,P8,P9,P11,P13,P14,P16", received);

                env.UndeployAll();
            }
        }

        public class EPLSpatialPREventIndexTableSubdivideDeepAddDestroy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table PointTable(Id string primary key, Px double, Py double);\n" +
                          "create index MyIndex on PointTable((Px,Py) pointregionquadtree(0,0,100,100,2,12));\n" +
                          "insert into PointTable select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointTable as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
                var bbtree =
                    new BoundingBox(0, 0, 100, 100).TreeForPath(new[] {"nw", "se", "sw", "ne", "nw", "nw", "nw", "nw", "nw", "nw", "nw", "nw"});
                var somewhere = bbtree.nw.se.sw.ne.nw.nw.nw.nw.nw.nw.nw.nw.bb;

                AddSendPoint(env, points, "P1", somewhere.MinX, somewhere.MinY);
                AddSendPoint(env, points, "P2", somewhere.MinX, somewhere.MinY);
                AddSendPoint(env, points, "P3", somewhere.MinX, somewhere.MinY);
                AssertBBTreePoints(env, bbtree, points);

                env.Milestone(0);

                AssertBBTreePoints(env, bbtree, points);

                env.UndeployAll();
            }
        }

        public class EPLSpatialPREventIndexTableSubdivideDestroy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table PointTable(Id string primary key, Px double, Py double);\n" +
                          "create index MyIndex on PointTable((Px,Py) pointregionquadtree(0,0,100,100,4,40));\n" +
                          "insert into PointTable select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointTable as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl).AddListener("out");

                SendPoint(env, "P1", 80, 40);
                SendPoint(env, "P2", 81, 41);
                SendPoint(env, "P3", 80, 40);

                env.Milestone(0);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, "P1,P2,P3", null, null, null);
                SendPoint(env, "P4", 80, 40);
                SendPoint(env, "P5", 81, 41);

                env.Milestone(1);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, "P1,P2,P3,P4,P5", null, null, null);

                env.UndeployAll();
            }
        }

        public class EPLSpatialPREventIndexTableSubdivideMergeDestroy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create table PointTable(Id string primary key, Px double, Py double);\n" +
                          "create index MyIndex on PointTable((Px,Py) pointregionquadtree(0,0,100,100,4,40));\n" +
                          "insert into PointTable select Id, Px, Py from SupportSpatialPoint;\n" +
                          "@Name('out') on SupportSpatialAABB as aabb select pt.Id as c0 from PointTable as pt where point(Px,Py).inside(rectangle(X,Y,Width,Height));\n";
                env.CompileDeploy(epl, path).AddListener("out");

                SendPoint(env, "P1", 80, 80);
                SendPoint(env, "P2", 81, 80);
                SendPoint(env, "P3", 80, 81);
                SendPoint(env, "P4", 80, 80);
                SendPoint(env, "P5", 45, 55);
                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3,P4", "P5");

                env.Milestone(0);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3,P4", "P5");
                env.CompileExecuteFAF("delete from PointTable where Id = 'P4'", path);
                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3", "P5");

                env.Milestone(1);

                AssertRectanglesManyRow(env, env.Listener("out"), BOXES, null, null, "P5", "P1,P2,P3", "P5");

                env.UndeployAll();
            }
        }
    }
} // end of namespace