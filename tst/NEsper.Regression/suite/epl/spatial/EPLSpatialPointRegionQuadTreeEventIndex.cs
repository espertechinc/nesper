///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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
			execs.Add(new EPLSpatialPREventIndexUnindexed());
			execs.Add(new EPLSpatialPREventIndexUnusedOnTrigger());
			execs.Add(new EPLSpatialPREventIndexUnusedNamedWindowFireAndForget());
			execs.Add(new EPLSpatialPREventIndexOnTriggerNWInsertRemove(false));
			execs.Add(new EPLSpatialPREventIndexOnTriggerNWInsertRemove(true));
			execs.Add(new EPLSpatialPREventIndexOnTriggerTable());
			execs.Add(new EPLSpatialPREventIndexChoiceOfTwo());
			execs.Add(new EPLSpatialPREventIndexUnique());
			execs.Add(new EPLSpatialPREventIndexPerformance());
			execs.Add(new EPLSpatialPREventIndexChoiceBetweenIndexTypes());
			execs.Add(new EPLSpatialPREventIndexNWFireAndForgetPerformance());
			execs.Add(new EPLSpatialPREventIndexTableFireAndForget());
			execs.Add(new EPLSpatialPREventIndexOnTriggerContextParameterized());
			execs.Add(new EPLSpatialPREventIndexExpression());
			execs.Add(new EPLSpatialPREventIndexEdgeSubdivide());
			execs.Add(new EPLSpatialPREventIndexRandomDoublePointsWRandomQuery());
			execs.Add(new EPLSpatialPREventIndexRandomIntPointsInSquareUnique());
			execs.Add(new EPLSpatialPREventIndexRandomMovingPoints());
			execs.Add(new EPLSpatialPREventIndexTableSimple());
			execs.Add(new EPLSpatialPREventIndexTableSubdivideDeepAddDestroy());
			execs.Add(new EPLSpatialPREventIndexTableSubdivideDestroy());
			execs.Add(new EPLSpatialPREventIndexTableSubdivideMergeDestroy());
			execs.Add(new EPLSpatialPREventIndexSubqNamedWindowIndexShare());
			return execs;
		}

		private class EPLSpatialPREventIndexNWFireAndForgetPerformance : RegressionExecution
		{
			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@Public create window MyPointWindow#keepall as (id string, px double, py double);\n" +
				          "insert into MyPointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "create index Idx on MyPointWindow( (px, py) pointregionquadtree(0, 0, 100, 100));\n";
				env.CompileDeploy(epl, path);

				var random = new Random();
				IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
				for (var i = 0; i < 10000; i++) {
					var px = random.NextDouble() * 100;
					var py = random.NextDouble() * 100;
					var point = new SupportSpatialPoint("P" + i, px, py);
					env.SendEventBean(point);
					points.Add(point);
					// Comment-me-in: log.info("Point P" + i + " " + px + " " + py);
				}

				var compiled = env.CompileFAF(
					"select * from MyPointWindow where point(px,py).inside(rectangle(?::double,?::double,?::double,?::double))",
					path);
				var prepared = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
				var start = PerformanceObserver.MilliTime;
				var fields = "id".SplitCsv();
				for (var i = 0; i < 500; i++) {
					var x = random.NextDouble() * 100;
					var y = random.NextDouble() * 100;
					// Comment-me-in: log.info("Query " + x + " " + y + " " + width + " " + height);

					prepared.SetObject(1, x);
					prepared.SetObject(2, y);
					prepared.SetObject(3, 5d);
					prepared.SetObject(4, 5d);
					var events = env.Runtime.FireAndForgetService.ExecuteQuery(prepared).Array;
					var expected = SupportSpatialUtil.GetExpected(points, x, y, 5, 5);
					EPAssertionUtil.AssertPropsPerRowAnyOrder(events, fields, expected);
				}

				var delta = PerformanceObserver.MilliTime - start;
				Assert.IsTrue(delta < 1000);

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexChoiceBetweenIndexTypes : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl =
					"@name('win') @public create window MyPointWindow#keepall as (id string, category string, px double, py double);\n" +
					"@name('insert') insert into MyPointWindow select id, category, px, py from SupportSpatialPoint;\n" +
					"@name('idx1') create index IdxHash on MyPointWindow(category);\n" +
					"@name('idx2') create index IdxQuadtree on MyPointWindow((px, py) pointregionquadtree(0, 0, 100, 100));\n";
				env.CompileDeploy(epl, path);

				SendPoint(env, "P1", 10, 15, "X");
				SendPoint(env, "P2", 10, 15, "Y");
				SendPoint(env, "P3", 10, 15, "Z");

				AssertIndexChoice(env, path, "", "IdxQuadtree");
				AssertIndexChoice(env, path, "@Hint('index(IdxHash, bust)')", "IdxQuadtree");

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexUnique : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('win') create window MyPointWindow#keepall as (id string, px double, py double);\n" +
				          "@name('insert') insert into MyPointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "@name('idx') create unique index Idx on MyPointWindow( (px, py) pointregionquadtree(0, 0, 100, 100));\n" +
				          "@name('s0') on SupportSpatialAABB select mpw.id as c0 from MyPointWindow as mpw where point(px, py).inside(rectangle(x, y, width, height));\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendPoint(env, "P1", 10, 15);
				try {
					SendPoint(env, "P2", 10, 15);
					Assert.Fail();
				}
				catch (Exception ex) { // we have a handler
					SupportMessageAssertUtil.AssertMessage(
						ex,
						"Unexpected exception in statement 'win': Unique index violation, index 'Idx' is a unique index and key '(10.0,15.0)' already exists");
				}

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		private class EPLSpatialPREventIndexPerformance : RegressionExecution
		{
			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
			}

			public void Run(RegressionEnvironment env)
			{
				var epl = "create window MyPointWindow#keepall as (id string, px double, py double);\n" +
				          "insert into MyPointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "create index Idx on MyPointWindow( (px, py) pointregionquadtree(0, 0, 100, 100));\n" +
				          "@name('s0') on SupportSpatialAABB select mpw.id as c0 from MyPointWindow as mpw where point(px, py).inside(rectangle(x, y, width, height));\n";
				env.CompileDeploy(epl).AddListener("s0");

				for (var x = 0; x < 100; x++) {
					for (var y = 0; y < 100; y++) {
						env.SendEventBean(new SupportSpatialPoint(x + "_" + y, (double)x, (double)y));
					}
				}

				var start = PerformanceObserver.MilliTime;
				for (var x = 0; x < 100; x++) {
					for (var y = 0; y < 100; y++) {
						env.SendEventBean(new SupportSpatialAABB("R", x, y, 0.5, 0.5));
						env.AssertEqualsNew("s0", "c0", x + "_" + y);
					}
				}

				var delta = PerformanceObserver.MilliTime - start;
				Assert.IsTrue(delta < 1000);

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexUnusedNamedWindowFireAndForget : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@Public create window PointWindow#keepall as (id string, px double, py double);\n" +
				          "create index MyIndex on PointWindow((px,py) pointregionquadtree(0,0,100,100,2,12));\n" +
				          "insert into PointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointWindow as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl, path);

				env.CompileExecuteFAF("delete from PointWindow where id='P1'", path);

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.FIREANDFORGET);
			}
		}

		private class EPLSpatialPREventIndexTableFireAndForget : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@Public create table MyTable(id string primary key, tx double, ty double)", path);
				env.CompileDeploy("insert into MyTable select id, px as tx, py as ty from SupportSpatialPoint", path);
				env.SendEventBean(new SupportSpatialPoint("P1", 50d, 50d));
				env.SendEventBean(new SupportSpatialPoint("P2", 49d, 49d));
				env.CompileDeploy(
					"create index MyIdxWithExpr on MyTable( (tx, ty) pointregionquadtree(0, 0, 100, 100))",
					path);

				var result = env.CompileExecuteFAF(
					IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
					"select id as c0 from MyTable where point(tx, ty).inside(rectangle(45, 45, 10, 10))",
					path);
				SupportQueryPlanIndexHook.AssertFAFAndReset("MyIdxWithExpr", "EventTableQuadTreePointRegion");
				EPAssertionUtil.AssertPropsPerRowAnyOrder(
					result.Array,
					"c0".SplitCsv(),
					new object[][] { new object[] { "P1" }, new object[] { "P2" } });

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.FIREANDFORGET);
			}
		}

		private class EPLSpatialPREventIndexExpression : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SupportQueryPlanIndexHook.Reset();
				var path = new RegressionPath();
				env.CompileDeploy("@Public create table MyTable(id string primary key, tx double, ty double)", path);
				env.CompileExecuteFAFNoResult("insert into MyTable values ('P1', 50, 30)", path);
				env.CompileExecuteFAFNoResult("insert into MyTable values ('P2', 50, 28)", path);
				env.CompileExecuteFAFNoResult("insert into MyTable values ('P3', 50, 30)", path);
				env.CompileExecuteFAFNoResult("insert into MyTable values ('P4', 49, 29)", path);
				env.CompileDeploy(
					"create index MyIdxWithExpr on MyTable((tx*10, ty*10) pointregionquadtree(0, 0, 1000, 1000))",
					path);

				var eplOne = "@name('s0') " +
				             IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				             "on SupportSpatialAABB select tbl.id as c0 from MyTable as tbl where point(tx, ty).inside(rectangle(x, y, width, height))";
				env.CompileDeploy(eplOne, path).AddListener("s0");
				env.AssertThat(
					() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
						"MyIdxWithExpr",
						"non-unique hash={} btree={} advanced={pointregionquadtree(tx*10,ty*10)}"));
				// Invalid use of index, the properties match and the bounding boxes do not match due to "x*10" missing.
				env.UndeployModuleContaining("s0");

				var eplTwo = "@name('s0') " +
				             IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				             "on SupportSpatialAABB select tbl.id as c0 from MyTable as tbl where point(tx*10, tbl.ty*10).inside(rectangle(x, y, width, height))";
				env.CompileDeploy(eplTwo, path).AddListener("s0");
				env.AssertThat(
					() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
						"MyIdxWithExpr",
						"non-unique hash={} btree={} advanced={pointregionquadtree(tx*10,ty*10)}"));
				AssertRectanglesManyRow(env, BOXES, null, null, null, null, null);
				AssertRectanglesManyRow(env, Collections.SingletonList(new BoundingBox(500, 300, 501, 301)), "P1,P3");

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexChoiceOfTwo : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SupportQueryPlanIndexHook.Reset();
				var path = new RegressionPath();
				var epl =
					"@Public create table MyPointTable(" +
					" id string primary key," +
					" x1 double, y1 double, \n" +
					" x2 double, y2 double);\n" +
					"create index Idx1 on MyPointTable( (x1, y1) pointregionquadtree(0, 0, 100, 100));\n" +
					"create index Idx2 on MyPointTable( (x2, y2) pointregionquadtree(0, 0, 100, 100));\n" +
					"on SupportSpatialDualPoint dp merge MyPointTable t where dp.id = t.id when not matched then insert select dp.id as id,x1,y1,x2,y2;\n";
				env.CompileDeploy(epl, path);

				var listener = new SupportUpdateListener();
				var textOne = "@name('s0') " +
				              IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				              "on SupportSpatialAABB select tbl.id as c0 from MyPointTable as tbl where point(x1, y1).inside(rectangle(x, y, width, height))";
				env.CompileDeploy(textOne, path).Statement("s0").AddListener(listener);

				env.AssertThat(
					() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
						"Idx1",
						"non-unique hash={} btree={} advanced={pointregionquadtree(x1,y1)}"));

				var textTwo = "@name('s1') " +
				              IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				              "on SupportSpatialAABB select tbl.id as c0 from MyPointTable as tbl where point(tbl.x2, y2).inside(rectangle(x, y, width, height))";
				env.CompileDeploy(textTwo, path).Statement("s1").AddListener(listener);
				env.AssertThat(
					() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
						"Idx2",
						"non-unique hash={} btree={} advanced={pointregionquadtree(x2,y2)}"));

				env.SendEventBean(new SupportSpatialDualPoint("P1", 10, 10, 60, 60));
				env.SendEventBean(new SupportSpatialDualPoint("P2", 55, 20, 4, 88));

				AssertRectanglesSingleValueAssertS0S1(env, listener, BOXES, "P1", "P2", "P2", "P1", "P1");

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.OBSERVEROPS);
			}
		}

		private class EPLSpatialPREventIndexSubqNamedWindowIndexShare : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@Hint('enable_window_subquery_indexshare') create window MyWindow#length(5) as select * from SupportSpatialPoint;\n" +
					"create index MyIndex on MyWindow((px,py) pointregionquadtree(0,0,100,100));\n" +
					"insert into MyWindow select * from SupportSpatialPoint;\n" +
					IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
					"@name('s0') select (select id from MyWindow as mw where point(mw.px,mw.py).inside(rectangle(aabb.x,aabb.y,aabb.width,aabb.height))).aggregate('', \n" +
					"  (result, item) => result || (case when result='' then '' else ',' end) || item) as c0 from SupportSpatialAABB aabb";
				env.CompileDeploy(epl).AddListener("s0");

				env.AssertThat(
					() => {
						var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
						Assert.AreEqual(
							"non-unique hash={} btree={} advanced={pointregionquadtree(px,py)}",
							subquery.Tables[0].IndexDesc);
						Assert.AreEqual("MyIndex", subquery.Tables[0].IndexName);
					});

				SendPoint(env, "P1", 10, 40);
				AssertRectanglesSingleValueAssertS0(env, BOXES, "P1", "", "", "", "");

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexUnusedOnTrigger : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@Public create window MyWindow#length(5) as select * from SupportSpatialPoint;\n" +
				          "create index MyIndex on MyWindow((px,py) pointregionquadtree(0,0,100,100));\n" +
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
				var epl = "@name('s0') " +
				          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				          "on SupportSpatialAABB as aabb select points.id as c0 " +
				          "from MyWindow as points where point(px, py).inside(rectangle(px,py,1,1))";
				env.CompileDeploy(epl, path).AddListener("s0");

				env.AssertThat(() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null));

				AssertRectanglesManyRow(env, BOXES, "P1,P2", "P1,P2", "P1,P2", "P1,P2", "P1,P2");

				env.UndeployModuleContaining("s0");
			}

			private void RunIndexUnusedPointValueDepends(
				RegressionEnvironment env,
				RegressionPath path)
			{
				var epl = "@name('s0') " +
				          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				          "on SupportSpatialAABB as aabb select points.id as c0 " +
				          "from MyWindow as points where point(px + x, py + y).inside(rectangle(x,y,width,height))";
				env.CompileDeploy(epl, path).AddListener("s0");

				env.AssertThat(() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null));

				AssertRectanglesManyRow(env, BOXES, "P1", "P1", "P1", "P1", "P1");

				env.UndeployModuleContaining("s0");
			}

			private void RunIndexUnusedConstantsOnly(
				RegressionEnvironment env,
				RegressionPath path)
			{
				SupportQueryPlanIndexHook.Reset();
				var epl = "@name('s0') " +
				          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				          "@name('s0') on SupportSpatialAABB as aabb select points.id as c0 " +
				          "from MyWindow as points where point(0, 0).inside(rectangle(x,y,width,height))";
				env.CompileDeploy(epl, path).AddListener("s0");

				env.AssertThat(() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null));

				AssertRectanglesManyRow(env, BOXES, "P1,P2", null, null, null, null);

				env.UndeployModuleContaining("s0");
			}
		}

		private class EPLSpatialPREventIndexUnindexed : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
					"@name('s0') select point(xOffset, yOffset).inside(rectangle(x, y, width, height)) as c0 from SupportEventRectangleWithOffset");
				env.AddListener("s0");

				SendAssert(env, 1, 1, 0, 0, 2, 2, true);
				SendAssert(env, 3, 1, 0, 0, 2, 2, false);
				SendAssert(env, 2, 1, 0, 0, 2, 2, false);

				env.Milestone(0);

				SendAssert(env, 1, 3, 0, 0, 2, 2, false);
				SendAssert(env, 1, 2, 0, 0, 2, 2, false);
				SendAssert(env, 0, 0, 1, 1, 2, 2, false);
				SendAssert(env, 1, 0, 1, 1, 2, 2, false);
				SendAssert(env, 0, 1, 1, 1, 2, 2, false);
				SendAssert(env, 1, 1, 1, 1, 2, 2, true);
				SendAssert(env, 2.9999, 2.9999, 1, 1, 2, 2, true);

				env.Milestone(1);

				SendAssert(env, 3, 2.9999, 1, 1, 2, 2, false);
				SendAssert(env, 2.9999, 3, 1, 1, 2, 2, false);
				SendAssertWNull(env, null, 0d, 0d, 0d, 0d, 0d, null);
				SendAssertWNull(env, 0d, 0d, 0d, null, 0d, 0d, null);

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexOnTriggerContextParameterized : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create context CtxBox initiated by SupportEventRectangleWithOffset box;\n" +
				          "context CtxBox create window MyWindow#keepall as SupportSpatialPoint;\n" +
				          "context CtxBox create index MyIndex on MyWindow((px+context.box.xOffset, py+context.box.yOffset) pointregionquadtree(context.box.x, context.box.y, context.box.width, context.box.height));\n" +
				          "context CtxBox on SupportSpatialPoint(category = context.box.id) merge MyWindow when not matched then insert select *;\n" +
				          "@name('s0') context CtxBox on SupportSpatialAABB(category = context.box.id) aabb " +
				          "  select points.id as c0 from MyWindow points where point(px, py).inside(rectangle(x, y, width, height))";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportEventRectangleWithOffset("NW", 0d, 0d, 0d, 0d, 50d, 50d));
				env.SendEventBean(new SupportEventRectangleWithOffset("SE", 0d, 0d, 50d, 50d, 50d, 50d));
				SendPoint(env, "P1", 60, 90, "SE");
				SendPoint(env, "P2", 5, 20, "NW");

				env.SendEventBean(new SupportSpatialAABB("R1", 60, 60, 40, 40, "SE"));
				env.AssertEqualsNew("s0", "c0", "P1");

				env.SendEventBean(new SupportSpatialAABB("R2", 0, 0, 5.0001, 20.0001, "NW"));
				env.AssertEqualsNew("s0", "c0", "P2");

				env.Milestone(0);

				env.SendEventBean(new SupportSpatialAABB("R3", 0, 0, 5, 30, "NW"));
				env.SendEventBean(new SupportSpatialAABB("R3", 0, 0, 30, 20, "NW"));
				env.AssertListenerNotInvoked("s0");

				env.UndeployAll();
			}
		}

		private class EPLSpatialPREventIndexOnTriggerNWInsertRemove : RegressionExecution
		{
			private readonly bool soda;

			public EPLSpatialPREventIndexOnTriggerNWInsertRemove(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				SupportQueryPlanIndexHook.Reset();
				env.CompileDeploy(
					soda,
					"@Public create window MyWindow#length(5) as select * from SupportSpatialPoint",
					path);
				env.CompileDeploy(
					soda,
					"create index MyIndex on MyWindow((px,py) pointregionquadtree(0,0,100,100))",
					path);
				env.CompileDeploy(soda, "insert into MyWindow select * from SupportSpatialPoint", path);

				var epl = "@name('s0') " +
				          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
				          " on SupportSpatialAABB as aabb " +
				          "select points.id as c0 from MyWindow as points where point(px,py).inside(rectangle(x,y,width,height))";
				env.CompileDeploy(soda, epl, path).AddListener("s0");

				env.AssertThat(
					() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
						"MyIndex",
						"non-unique hash={} btree={} advanced={pointregionquadtree(px,py)}"));

				SendPoint(env, "P1", 10, 40);
				AssertRectanglesManyRow(env, BOXES, "P1", null, null, null, null);

				env.Milestone(0);

				SendPoint(env, "P2", 80, 80);
				AssertRectanglesManyRow(env, BOXES, "P1", null, null, "P2", null);

				SendPoint(env, "P3", 10, 40);
				AssertRectanglesManyRow(env, BOXES, "P1,P3", null, null, "P2", null);

				env.Milestone(1);

				SendPoint(env, "P4", 60, 40);
				AssertRectanglesManyRow(env, BOXES, "P1,P3", "P4", null, "P2", "P4");

				SendPoint(env, "P5", 20, 75);
				AssertRectanglesManyRow(env, BOXES, "P1,P3", "P4", "P5", "P2", "P4");

				SendPoint(env, "P6", 50, 50);
				AssertRectanglesManyRow(env, BOXES, "P3", "P4", "P5", "P2,P6", "P4,P6");

				env.Milestone(2);

				SendPoint(env, "P7", 0, 0);
				AssertRectanglesManyRow(env, BOXES, "P3,P7", "P4", "P5", "P6", "P4,P6");

				SendPoint(env, "P8", 99.999, 0);
				AssertRectanglesManyRow(env, BOXES, "P7", "P4,P8", "P5", "P6", "P4,P6");

				env.Milestone(3);

				SendPoint(env, "P9", 0, 99.999);
				AssertRectanglesManyRow(env, BOXES, "P7", "P8", "P5,P9", "P6", "P6");

				SendPoint(env, "P10", 99.999, 99.999);
				AssertRectanglesManyRow(env, BOXES, "P7", "P8", "P9", "P6,P10", "P6");

				SendPoint(env, "P11", 0, 0);
				AssertRectanglesManyRow(env, BOXES, "P7,P11", "P8", "P9", "P10", null);

				env.UndeployAll();
			}

			public string Name()
			{
				return this.GetType().Name +
				       "{" +
				       "soda=" +
				       soda +
				       '}';
			}
		}

		private class EPLSpatialPREventIndexOnTriggerTable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				SupportQueryPlanIndexHook.Reset();
				var epl =
					"@public create table MyPointTable(my_x double primary key, my_y double primary key, my_id string);\n" +
					"@Audit create index MyIndex on MyPointTable( (my_x, my_y) pointregionquadtree(0, 0, 100, 100));\n" +
					"on SupportSpatialPoint ssp merge MyPointTable where ssp.px = my_x and ssp.py = my_y when not matched then insert select px as my_x, py as my_y, id as my_id;\n" +
					IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
					"@Audit @name('s0') on SupportSpatialAABB select my_id as c0 from MyPointTable as c0 where point(my_x, my_y).inside(rectangle(x, y, width, height))";
				env.CompileDeploy(epl, path).AddListener("s0");

				env.AssertThat(
					() => SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
						"MyIndex",
						"non-unique hash={} btree={} advanced={pointregionquadtree(my_x,my_y)}"));

				SendPoint(env, "P1", 55, 45);
				AssertRectanglesManyRow(env, BOXES, null, "P1", null, null, "P1");

				SendPoint(env, "P2", 45, 45);
				AssertRectanglesManyRow(env, BOXES, "P2", "P1", null, null, "P1,P2");

				env.Milestone(0);

				SendPoint(env, "P3", 55, 55);
				AssertRectanglesManyRow(env, BOXES, "P2", "P1", null, "P3", "P1,P2,P3");

				env.CompileExecuteFAFNoResult("delete from MyPointTable where my_x = 55 and my_y = 45", path);
				SendPoint(env, "P4", 45, 55);
				AssertRectanglesManyRow(env, BOXES, "P2", null, "P4", "P3", "P2,P3,P4");

				env.UndeployAll();
			}
		}

		public class EPLSpatialPREventIndexEdgeSubdivide : RegressionExecution
		{

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@public create window PointWindow#keepall as (id string, px double, py double);\n" +
				          "create index MyIndex on PointWindow((px,py) pointregionquadtree(0,0,100,100,2,5));\n" +
				          "insert into PointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointWindow as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl, path).AddListener("s0");

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
				var count = 0;
				IList<string> ids = new List<string>();
				for (int ii = 0 ; ii < points.Count ; ii++) {
					var p = points[ii];
					if (count % 2 == 1) {
						points.RemoveAt(ii--);
						ids.Add(p.Id);
					}

					count++;
				}

				var query = SupportSpatialUtil.BuildDeleteQueryWithInClause("PointWindow", "id", ids);
				env.CompileExecuteFAF(query, path);
			}

			private void RemoveAllPoints(
				RegressionEnvironment env,
				RegressionPath path,
				IList<SupportSpatialPoint> points)
			{
				IList<string> ids = new List<string>();
				for (int ii = 0 ; ii < points.Count ; ii++) {
					var p = points[ii];
					points.RemoveAt(ii--);
					ids.Add(p.Id);
				}

				if (ids.IsEmpty()) {
					return;
				}

				var query = SupportSpatialUtil.BuildDeleteQueryWithInClause("PointWindow", "id", ids);
				env.CompileExecuteFAF(query, path);
			}

			private void RemoveAllABPoints(
				RegressionEnvironment env,
				RegressionPath path,
				IList<SupportSpatialPoint> points)
			{
				IList<string> ids = new List<string>();
				for (int ii = 0 ; ii < points.Count ; ii++) {
					var p = points[ii];
					if (p.Id[0] == 'A' || p.Id[0] == 'B') {
						points.RemoveAt(ii--);
						ids.Add(p.Id);
					}
				}

				var query = SupportSpatialUtil.BuildDeleteQueryWithInClause("PointWindow", "id", ids);
				env.CompileExecuteFAF(query, path);
			}

			private ISet<BoundingBox> GetLevel5Boxes()
			{
				var bb = new BoundingBox(0, 0, 100, 100);
				var bbtree = bb.TreeForDepth(4);
				var bbs = new LinkedHashSet<BoundingBox>();

				var quadrantValues = EnumHelper.GetValues<QuadrantEnum>().ToArray();
				foreach (var lvl1 in quadrantValues) {
					var q1 = bbtree.GetQuadrant(lvl1);
					foreach (var lvl2 in quadrantValues) {
						var q2 = q1.GetQuadrant(lvl2);
						foreach (var lvl3 in quadrantValues) {
							var q3 = q2.GetQuadrant(lvl3);
							foreach (var lvl4 in quadrantValues) {
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
			private static readonly ILog log =
				LogManager.GetLogger(typeof(EPLSpatialPREventIndexRandomDoublePointsWRandomQuery));

			private const int NUM_POINTS = 1000;
			private const int X = 0;
			private const int Y = 0;
			private const int WIDTH = 100;
			private const int HEIGHT = 100;
			private const int NUM_QUERIES_AFTER_LOAD = 100;
			private const int NUM_QUERIES_AFTER_EACH_REMOVE = 5;
			private static readonly int[] CHECKPOINT_REMAINING = new int[] { 100, 300, 700 }; // must be sorted

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
			}

			public void Run(RegressionEnvironment env)
			{

				var path = new RegressionPath();
				var epl = "@public create window PointWindow#keepall as (id string, px double, py double);\n" +
				          "create index MyIndex on PointWindow((px,py) pointregionquadtree(0,0,100,100));\n" +
				          "insert into PointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointWindow as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				var random = new Random();
				var points = RandomPoints(random, NUM_POINTS, X, Y, WIDTH, HEIGHT);
				foreach (var point in points) {
					SendPoint(env, point.Id, point.Px.Value, point.Py.Value);
					// Comment-me-in: log.info("Point: " + point);
				}

				env.Milestone(0);

				for (var i = 0; i < NUM_QUERIES_AFTER_LOAD; i++) {
					RandomQuery(env, random, points);
				}

				var milestone = new AtomicLong();
				var deleteQuery = env.CompileFAF("delete from PointWindow where id=?::string", path);
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

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@public create window PointWindow#keepall as (id string, px double, py double);\n" +
				          "create unique index MyIndex on PointWindow((px,py) pointregionquadtree(0,0,1000,1000));\n" +
				          "insert into PointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointWindow as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				var random = new Random();
				var points = GenerateCoordinates(random);
				foreach (var point in points) {
					SendPoint(env, point.Id, point.Px.Value, point.Py.Value);
				}

				env.Milestone(0);

				// find all individually
				env.AssertListener(
					"s0",
					listener => {
						foreach (var p in points) {
							env.SendEventBean(new SupportSpatialAABB("", p.Px.Value, p.Py.Value, 1, 1));
							Assert.AreEqual(p.Id, listener.AssertOneGetNewAndReset().Get("c0"));
						}
					});

				// get all content
				AssertAllPoints(env, points, 0, 0, SIZE, SIZE);

				env.Milestone(1);

				// add duplicate: note these events are still is named window
				foreach (var p in points) {
					try {
						SendPoint(env, p.Id, p.Px.Value, p.Py.Value);
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
					var deleteQuery = SupportSpatialUtil.BuildDeleteQueryWithInClause("PointWindow", "id", first);
					env.CompileExecuteFAF(deleteQuery, path);
					ids.RemoveAll(first);
				}

				env.SendEventBean(new SupportSpatialAABB("", 0, 0, SIZE, SIZE));
				env.AssertListenerNotInvoked("s0");

				env.Milestone(2);

				env.SendEventBean(new SupportSpatialAABB("", 0, 0, SIZE, SIZE));
				env.AssertListenerNotInvoked("s0");

				env.UndeployAll();
			}

			private static ICollection<SupportSpatialPoint> GenerateCoordinates(Random random)
			{
				IDictionary<UniformPair<int>, SupportSpatialPoint> points =
					new Dictionary<UniformPair<int>, SupportSpatialPoint>();
				while (points.Count < SIZE) {
					var x = random.Next(SIZE);
					var y = random.Next(SIZE);
					points.Put(new UniformPair<int>(x, y), new SupportSpatialPoint(x + "_" + y, (double)x, (double)y));
				}

				return points.Values;
			}
		}

		public class EPLSpatialPREventIndexRandomMovingPoints : RegressionExecution
		{
			private static readonly ILog log = LogManager.GetLogger(typeof(EPLSpatialPREventIndexRandomMovingPoints));

			private const int NUM_POINTS = 1000;
			private const int NUM_MOVES = 5000;
			private const int WIDTH = 100;
			private const int HEIGHT = 100;
			private static readonly int[] CHECKPOINT_AT = new int[] { 500, 3000, 4000 };

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
			}

			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@public create table PointTable as (id string primary key, px double, py double);\n" +
				          "create index MyIndex on PointTable((px,py) pointregionquadtree(0,0,100,100));\n" +
				          "insert into PointTable select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointTable as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				var random = new Random();
				var points = GenerateCoordinates(random, NUM_POINTS, WIDTH, HEIGHT);
				foreach (var point in points) {
					SendPoint(env, point.Id, point.Px.Value, point.Py.Value);
				}

				env.Milestone(0);
				var deleteQuery = env.CompileFAF("delete from PointTable where id=?::string", path);
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
				var newX = point.Px.Value;
				var newY = point.Py.Value;
				if (direction == 0 && newX > 0) {
					newX--;
				}

				if (direction == 1 && newY > 0) {
					newY--;
				}

				if (direction == 2 && newX < (WIDTH - 1)) {
					newX++;
				}

				if (direction == 3 && newY < (HEIGHT - 1)) {
					newY++;
				}

				// Comment-me-in:
				// log.info("Moving " + point.getId() + " from " + printPoint(point.getX(), point.getY()) + " to " + printPoint(newX, newY));
				preparedDelete.SetObject(1, point.Id);
				env.Runtime.FireAndForgetService.ExecuteQuery(preparedDelete);

				point.Px = newX;
				point.Py = newY;
				SendPoint(env, point.Id, point.Px.Value, point.Py.Value);
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
					result.Add(new SupportSpatialPoint("P" + i, (double)x, (double)y));
				}

				return result;
			}
		}

		public class EPLSpatialPREventIndexTableSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public create window PointWindow#keepall as (id string, px double, py double);\n" +
				          "create index MyIndex on PointWindow((px,py) pointregionquadtree(0,0,100,100,2,12));\n" +
				          "insert into PointWindow select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointWindow as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl).AddListener("s0");

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
				env.AssertListener(
					"s0",
					listener => {
						var received = SortJoinProperty(listener.GetAndResetLastNewData(), "c0");
						Assert.AreEqual("P7,P8,P9,P11,P13,P14,P16", received);
					});

				env.UndeployAll();
			}
		}

		public class EPLSpatialPREventIndexTableSubdivideDeepAddDestroy : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "create table PointTable(id string primary key, px double, py double);\n" +
				          "create index MyIndex on PointTable((px,py) pointregionquadtree(0,0,100,100,2,12));\n" +
				          "insert into PointTable select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointTable as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl).AddListener("s0");

				IList<SupportSpatialPoint> points = new List<SupportSpatialPoint>();
				var bbtree =
					new BoundingBox(0, 0, 100, 100).TreeForPath("nw,se,sw,ne,nw,nw,nw,nw,nw,nw,nw,nw".SplitCsv());
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

				var epl = "create table PointTable(id string primary key, px double, py double);\n" +
				          "create index MyIndex on PointTable((px,py) pointregionquadtree(0,0,100,100,4,40));\n" +
				          "insert into PointTable select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointTable as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendPoint(env, "P1", 80, 40);
				SendPoint(env, "P2", 81, 41);
				SendPoint(env, "P3", 80, 40);

				env.Milestone(0);

				AssertRectanglesManyRow(env, BOXES, null, "P1,P2,P3", null, null, null);
				SendPoint(env, "P4", 80, 40);
				SendPoint(env, "P5", 81, 41);

				env.Milestone(1);

				AssertRectanglesManyRow(env, BOXES, null, "P1,P2,P3,P4,P5", null, null, null);

				env.UndeployAll();
			}
		}

		public class EPLSpatialPREventIndexTableSubdivideMergeDestroy : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@public create table PointTable(id string primary key, px double, py double);\n" +
				          "create index MyIndex on PointTable((px,py) pointregionquadtree(0,0,100,100,4,40));\n" +
				          "insert into PointTable select id, px, py from SupportSpatialPoint;\n" +
				          "@name('s0') on SupportSpatialAABB as aabb select pt.id as c0 from PointTable as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				SendPoint(env, "P1", 80, 80);
				SendPoint(env, "P2", 81, 80);
				SendPoint(env, "P3", 80, 81);
				SendPoint(env, "P4", 80, 80);
				SendPoint(env, "P5", 45, 55);
				AssertRectanglesManyRow(env, BOXES, null, null, "P5", "P1,P2,P3,P4", "P5");

				env.Milestone(0);

				AssertRectanglesManyRow(env, BOXES, null, null, "P5", "P1,P2,P3,P4", "P5");
				env.CompileExecuteFAFNoResult("delete from PointTable where id = 'P4'", path);
				AssertRectanglesManyRow(env, BOXES, null, null, "P5", "P1,P2,P3", "P5");

				env.Milestone(1);

				AssertRectanglesManyRow(env, BOXES, null, null, "P5", "P1,P2,P3", "P5");

				env.UndeployAll();
			}
		}

		private static void AssertIndexChoice(
			RegressionEnvironment env,
			RegressionPath path,
			string hint,
			string expectedIndexName)
		{
			SupportQueryPlanIndexHook.Reset();
			var epl = "@name('s0') " +
			          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
			          hint +
			          "on SupportSpatialAABB as aabb select mpw.id as c0 from MyPointWindow as mpw " +
			          "where aabb.category = mpw.category and point(px, py).inside(rectangle(x, y, width, height))\n";
			env.CompileDeploy(epl, path).AddListener("s0");

			env.AssertThat(
				() => {
					var plan = SupportQueryPlanIndexHook.AssertOnExprAndReset();
					Assert.AreEqual(expectedIndexName, plan.Tables[0].IndexName);
				});

			env.SendEventBean(new SupportSpatialAABB("R1", 9, 14, 1.0001, 1.0001, "Y"));
			env.AssertEqualsNew("s0", "c0", "P2");

			env.UndeployModuleContaining("s0");
		}
	}
} // end of namespace
