///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportSpatialUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.spatial
{
    public class ExecSpatialPointRegionQuadTreeEventIndex : RegressionExecution {
        private static readonly IList<BoundingBox> BOXES = Collections.List(
                new BoundingBox(0, 0, 50, 50),
                new BoundingBox(50, 0, 100, 50),
                new BoundingBox(0, 50, 50, 100),
                new BoundingBox(50, 50, 100, 100),
                new BoundingBox(25, 25, 75, 75)
        );
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in Collections.List(typeof(SupportSpatialPoint), typeof(SupportSpatialAABB), typeof(MyEventRectangleWithOffset), typeof(SupportSpatialDualPoint))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionEventIndexUnindexed(epService);
    
            RunAssertionEventIndexUnusedOnTrigger(epService);
            RunAssertionEventIndexUnusedNamedWindowFireAndForget(epService);
    
            RunAssertionEventIndexOnTriggerNWInsertRemove(epService, false);
            RunAssertionEventIndexOnTriggerNWInsertRemove(epService, true);
            RunAssertionEventIndexOnTriggerContextParameterized(epService);
            RunAssertionEventIndexSubqNamedWindowIndexShare(epService);
            RunAssertionEventIndexOnTriggerTable(epService);
            RunAssertionEventIndexChoiceOfTwo(epService);
            RunAssertionEventIndexExpression(epService);
            RunAssertionEventIndexUnique(epService);
            RunAssertionEventIndexPerformance(epService);
            RunAssertionEventIndexChoiceBetweenIndexTypes(epService);
            RunAssertionEventIndexTableFireAndForget(epService);
            RunAssertionEventIndexNWFireAndForgetPerformance(epService);
        }
    
        private void RunAssertionEventIndexNWFireAndForgetPerformance(EPServiceProvider epService) {
            var epl = "create window MyPointWindow#keepall as (id string, px double, py double);\n" +
                    "insert into MyPointWindow select id, px, py from SupportSpatialPoint;\n" +
                    "create index Idx on MyPointWindow( (px, py) pointregionquadtree(0, 0, 100, 100));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            var random = new Random();
            var points = new List<SupportSpatialPoint>();
            for (var i = 0; i < 10000; i++) {
                var px = random.NextDouble() * 100;
                var py = random.NextDouble() * 100;
                var point = new SupportSpatialPoint("P" + Convert.ToString(i), px, py);
                epService.EPRuntime.SendEvent(point);
                points.Add(point);
                // Comment-me-in: Log.Info("Point P" + i + " " + px + " " + py);
            }
    
            var prepared = epService.EPRuntime.PrepareQueryWithParameters("select * from MyPointWindow where point(px,py).inside(rectangle(?,?,?,?))");
            var start = PerformanceObserver.MilliTime;
            var fields = "id".Split(',');
            for (var i = 0; i < 500; i++) {
                var x = random.NextDouble() * 100;
                var y = random.NextDouble() * 100;
                // Comment-me-in: Log.Info("Query " + x + " " + y + " " + width + " " + height);
    
                prepared.SetObject(1, x);
                prepared.SetObject(2, y);
                prepared.SetObject(3, 5);
                prepared.SetObject(4, 5);
                var events = epService.EPRuntime.ExecuteQuery(prepared).Array;
                var expected = SupportSpatialUtil.GetExpected(points, x, y, 5, 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(events, fields, expected);
            }
            var delta = PerformanceObserver.MilliTime - start;
            Assert.IsTrue(delta < 1000, "delta=" + delta);
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexChoiceBetweenIndexTypes(EPServiceProvider epService) {
            var epl = "@Name('win') create window MyPointWindow#keepall as (id string, category string, px double, py double);\n" +
                    "@Name('insert') insert into MyPointWindow select id, category, px, py from SupportSpatialPoint;\n" +
                    "@Name('idx1') create index IdxHash on MyPointWindow(category);\n" +
                    "@Name('idx2') create index IdxQuadtree on MyPointWindow((px, py) pointregionquadtree(0, 0, 100, 100));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            SendPoint(epService, "P1", 10, 15, "X");
            SendPoint(epService, "P2", 10, 15, "Y");
            SendPoint(epService, "P3", 10, 15, "Z");
    
            AssertIndexChoice(epService, "", "IdxQuadtree");
            AssertIndexChoice(epService, "@Hint('Index(IdxHash, bust)')", "IdxQuadtree");
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexUnique(EPServiceProvider epService) {
            var epl = "@Name('win') create window MyPointWindow#keepall as (id string, px double, py double);\n" +
                    "@Name('insert') insert into MyPointWindow select id, px, py from SupportSpatialPoint;\n" +
                    "@Name('idx') create unique index Idx on MyPointWindow( (px, py) pointregionquadtree(0, 0, 100, 100));\n" +
                    "@Name('out') on SupportSpatialAABB select mpw.id as c0 from MyPointWindow as mpw where point(px, py).inside(rectangle(x, y, width, height));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            SendPoint(epService, "P1", 10, 15);
            try {
                SendPoint(epService, "P2", 10, 15);
                Assert.Fail();
            } catch (Exception ex) { // we have a handler
                SupportMessageAssertUtil.AssertMessage(ex,
                        "Unexpected exception in statement 'win': Unique index violation, index 'Idx' is a unique index and key '(10,15)' already exists");
            }
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexPerformance(EPServiceProvider epService) {
            var epl = "create window MyPointWindow#keepall as (id string, px double, py double);\n" +
                    "insert into MyPointWindow select id, px, py from SupportSpatialPoint;\n" +
                    "create index Idx on MyPointWindow( (px, py) pointregionquadtree(0, 0, 100, 100));\n" +
                    "@Name('out') on SupportSpatialAABB select mpw.id as c0 from MyPointWindow as mpw where point(px, py).inside(rectangle(x, y, width, height));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            for (var x = 0; x < 100; x++) {
                for (var y = 0; y < 100; y++) {
                    epService.EPRuntime.SendEvent(new SupportSpatialPoint(Convert.ToString(x) + "_" + Convert.ToString(y), (double) x, (double) y));
                }
            }
    
            var start = PerformanceObserver.MilliTime;
            for (var x = 0; x < 100; x++) {
                for (var y = 0; y < 100; y++) {
                    epService.EPRuntime.SendEvent(new SupportSpatialAABB("R", x, y, 0.5, 0.5));
                    Assert.AreEqual(Convert.ToString(x) + "_" + Convert.ToString(y), listener.AssertOneGetNewAndReset().Get("c0"));
                }
            }
            var delta = PerformanceObserver.MilliTime - start;
            Assert.IsTrue(delta < 1000, "delta=" + delta);
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexUnusedNamedWindowFireAndForget(EPServiceProvider epService) {
            var epl = "@Resilient create window PointWindow#keepall as (id string, px double, py double);\n" +
                    "@Resilient create index MyIndex on PointWindow((px,py) pointregionquadtree(0,0,100,100,2,12));\n" +
                    "@Resilient insert into PointWindow select id, px, py from SupportSpatialPoint;\n" +
                    "@Resilient @Name('out') on SupportSpatialAABB as aabb select pt.id as c0 from PointWindow as pt where point(px,py).inside(rectangle(x,y,width,height));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            epService.EPRuntime.ExecuteQuery("delete from PointWindow where id='P1'");
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deploymentId);
        }
    
        private void RunAssertionEventIndexTableFireAndForget(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTable(id string primary key, tx double, ty double)");
            epService.EPAdministrator.CreateEPL("insert into MyTable select id, px as tx, py as ty from SupportSpatialPoint");
            epService.EPRuntime.SendEvent(new SupportSpatialPoint("P1", 50d, 50d));
            epService.EPRuntime.SendEvent(new SupportSpatialPoint("P2", 49d, 49d));
            epService.EPAdministrator.CreateEPL("create index MyIdxWithExpr on MyTable( (tx, ty) pointregionquadtree(0, 0, 100, 100))");
    
            var result = epService.EPRuntime.ExecuteQuery(IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "select id as c0 from MyTable where point(tx, ty).inside(rectangle(45, 45, 10, 10))");
            SupportQueryPlanIndexHook.AssertFAFAndReset("MyIdxWithExpr", "EventTableQuadTreePointRegionImpl");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, "c0".Split(','), new object[][]{new object[] {"P1"}, new object[] {"P2"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventIndexExpression(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTable(id string primary key, tx double, ty double)");
            epService.EPRuntime.ExecuteQuery("insert into MyTable values ('P1', 50, 30)");
            epService.EPRuntime.ExecuteQuery("insert into MyTable values ('P2', 50, 28)");
            epService.EPRuntime.ExecuteQuery("insert into MyTable values ('P3', 50, 30)");
            epService.EPRuntime.ExecuteQuery("insert into MyTable values ('P4', 49, 29)");
            epService.EPAdministrator.CreateEPL("create index MyIdxWithExpr on MyTable( (tx*10, ty*10) pointregionquadtree(0, 0, 1000, 1000))");
    
            var eplOne = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "on SupportSpatialAABB select tbl.id as c0 from MyTable as tbl where point(tx, ty).inside(rectangle(x, y, width, height))";
            var statementOne = epService.EPAdministrator.CreateEPL(eplOne);
            var listener = new SupportUpdateListener();
            statementOne.Events += listener.Update;
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);
            AssertRectanglesManyRow(epService, listener, BOXES, "P4", "P1,P2,P3", null, null, "P1,P2,P3,P4");
            statementOne.Dispose();
    
            var eplTwo = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "on SupportSpatialAABB select tbl.id as c0 from MyTable as tbl where point(tx*10, tbl.ty*10).inside(rectangle(x, y, width, height))";
            var statementTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            statementTwo.Events += listener.Update;
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("MyIdxWithExpr", "non-unique hash={} btree={} advanced={pointregionquadtree(tx*10,ty*10)}");
            AssertRectanglesManyRow(epService, listener, BOXES, null, null, null, null, null);
            AssertRectanglesManyRow(epService, listener, Collections.SingletonList(new BoundingBox(500, 300, 501, 301)), "P1,P3");
            statementTwo.Dispose();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventIndexChoiceOfTwo(EPServiceProvider epService) {
            var epl =
                    "create table MyPointTable(" +
                            " id string primary key," +
                            " x1 double, y1 double, \n" +
                            " x2 double, y2 double);\n" +
                            "create index Idx1 on MyPointTable( (x1, y1) pointregionquadtree(0, 0, 100, 100));\n" +
                            "create index Idx2 on MyPointTable( (x2, y2) pointregionquadtree(0, 0, 100, 100));\n" +
                            "on SupportSpatialDualPoint dp merge MyPointTable t where dp.id = t.id when not matched then insert select dp.id as id,x1,y1,x2,y2;\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            var textOne = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "on SupportSpatialAABB select tbl.id as c0 from MyPointTable as tbl where point(x1, y1).inside(rectangle(x, y, width, height))";
            var statementOne = epService.EPAdministrator.CreateEPL(textOne);
            var listener = new SupportUpdateListener();
            statementOne.Events += listener.Update;
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("Idx1", "non-unique hash={} btree={} advanced={pointregionquadtree(x1,y1)}");
    
            var textTwo = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "on SupportSpatialAABB select tbl.id as c0 from MyPointTable as tbl where point(tbl.x2, y2).inside(rectangle(x, y, width, height))";
            var statementTwo = epService.EPAdministrator.CreateEPL(textTwo);
            statementTwo.Events += listener.Update;
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("Idx2", "non-unique hash={} btree={} advanced={pointregionquadtree(x2,y2)}");
    
            epService.EPRuntime.SendEvent(new SupportSpatialDualPoint("P1", 10, 10, 60, 60));
            epService.EPRuntime.SendEvent(new SupportSpatialDualPoint("P2", 55, 20, 4, 88));
    
            AssertRectanglesSingleValue(epService, listener, BOXES, "P1", "P2", "P2", "P1", "P1");
    
            statementOne.Dispose();
            statementTwo.Dispose();
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexSubqNamedWindowIndexShare(EPServiceProvider epService) {
            var epl = "@Hint('enable_window_subquery_indexshare') create window MyWindow#length(5) as select * from SupportSpatialPoint;\n" +
                    "create index MyIndex on MyWindow((px,py) pointregionquadtree(0,0,100,100));\n" +
                    "insert into MyWindow select * from SupportSpatialPoint;\n" +
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                    "@Name('out') select (select id from MyWindow as mw where point(mw.px,mw.py).inside(rectangle(aabb.x,aabb.y,aabb.width,aabb.height))).aggregate('', \n" +
                    "  (result, item) => result || (case when result='' then '' else ',' end) || item) as c0 from SupportSpatialAABB aabb";
            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual("non-unique hash={} btree={} advanced={pointregionquadtree(px,py)}", subquery.Tables[0].IndexDesc);
            Assert.AreEqual("MyIndex", subquery.Tables[0].IndexName);
    
            SendPoint(epService, "P1", 10, 40);
            AssertRectanglesSingleValue(epService, listener, BOXES, "P1", "", "", "", "");
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResult.DeploymentId);
        }
    
        private void RunAssertionEventIndexUnusedOnTrigger(EPServiceProvider epService) {
            var epl = "create window MyWindow#length(5) as select * from SupportSpatialPoint;\n" +
                    "create index MyIndex on MyWindow((px,py) pointregionquadtree(0,0,100,100));\n" +
                    "insert into MyWindow select * from SupportSpatialPoint;\n";
            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            SendPoint(epService, "P1", 5, 5);
            SendPoint(epService, "P2", 55, 60);
    
            RunIndexUnusedConstantsOnly(epService);
            RunIndexUnusedPointValueDepends(epService);
            RunIndexUnusedRectValueDepends(epService);
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResult.DeploymentId);
        }
    
        private void RunIndexUnusedRectValueDepends(EPServiceProvider epService) {
            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "@Name('out') on SupportSpatialAABB as aabb select points.id as c0 " +
                    "from MyWindow as points where point(px, py).inside(rectangle(px,py,1,1))";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);
    
            AssertRectanglesManyRow(epService, listener, BOXES, "P1,P2", "P1,P2", "P1,P2", "P1,P2", "P1,P2");
    
            stmt.Dispose();
        }
    
        private void RunIndexUnusedPointValueDepends(EPServiceProvider epService) {
            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "@Name('out') on SupportSpatialAABB as aabb select points.id as c0 " +
                    "from MyWindow as points where point(px + x, py + y).inside(rectangle(x,y,width,height))";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);
    
            AssertRectanglesManyRow(epService, listener, BOXES, "P1", "P1", "P1", "P1", "P1");
    
            stmt.Dispose();
        }
    
        private void RunIndexUnusedConstantsOnly(EPServiceProvider epService) {
            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "@Name('out') on SupportSpatialAABB as aabb select points.id as c0 " +
                    "from MyWindow as points where point(0, 0).inside(rectangle(x,y,width,height))";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset(null, null);
    
            AssertRectanglesManyRow(epService, listener, BOXES, "P1,P2", null, null, null, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionEventIndexUnindexed(EPServiceProvider epService) {
            var stmt = epService.EPAdministrator.CreateEPL("select point(xOffset, yOffset).inside(rectangle(x, y, width, height)) as c0 from MyEventRectangleWithOffset");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendAssert(epService, listener, 1, 1, 0, 0, 2, 2, true);
            SendAssert(epService, listener, 3, 1, 0, 0, 2, 2, false);
            SendAssert(epService, listener, 2, 1, 0, 0, 2, 2, false);
            SendAssert(epService, listener, 1, 3, 0, 0, 2, 2, false);
            SendAssert(epService, listener, 1, 2, 0, 0, 2, 2, false);
            SendAssert(epService, listener, 0, 0, 1, 1, 2, 2, false);
            SendAssert(epService, listener, 1, 0, 1, 1, 2, 2, false);
            SendAssert(epService, listener, 0, 1, 1, 1, 2, 2, false);
            SendAssert(epService, listener, 1, 1, 1, 1, 2, 2, true);
            SendAssert(epService, listener, 2.9999, 2.9999, 1, 1, 2, 2, true);
            SendAssert(epService, listener, 3, 2.9999, 1, 1, 2, 2, false);
            SendAssert(epService, listener, 2.9999, 3, 1, 1, 2, 2, false);
            SendAssertWNull(epService, listener, null, 0d, 0d, 0d, 0d, 0d, null);
            SendAssertWNull(epService, listener, 0d, 0d, 0d, null, 0d, 0d, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventIndexOnTriggerContextParameterized(EPServiceProvider epService) {
            var epl = "create context CtxBox initiated by MyEventRectangleWithOffset box;\n" +
                    "context CtxBox create window MyWindow#keepall as SupportSpatialPoint;\n" +
                    "context CtxBox create index MyIndex on MyWindow((px+context.box.xOffset, py+context.box.yOffset) pointregionquadtree(context.box.x, context.box.y, context.box.width, context.box.height));\n" +
                    "context CtxBox on SupportSpatialPoint(category = context.box.id) merge MyWindow when not matched then insert select *;\n" +
                    "@Name('out') context CtxBox on SupportSpatialAABB(category = context.box.id) aabb " +
                    "  select points.id as c0 from MyWindow points where point(px, py).inside(rectangle(x, y, width, height))";
            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new MyEventRectangleWithOffset("NW", 0d, 0d, 0d, 0d, 50d, 50d));
            epService.EPRuntime.SendEvent(new MyEventRectangleWithOffset("SE", 0d, 0d, 50d, 50d, 50d, 50d));
            SendPoint(epService, "P1", 60, 90, "SE");
            SendPoint(epService, "P2", 5, 20, "NW");
    
            epService.EPRuntime.SendEvent(new SupportSpatialAABB("R1", 60, 60, 40, 40, "SE"));
            Assert.AreEqual("P1", listener.AssertOneGetNewAndReset().Get("c0"));
    
            epService.EPRuntime.SendEvent(new SupportSpatialAABB("R2", 0, 0, 5.0001, 20.0001, "NW"));
            Assert.AreEqual("P2", listener.AssertOneGetNewAndReset().Get("c0"));
    
            epService.EPRuntime.SendEvent(new SupportSpatialAABB("R3", 0, 0, 5, 30, "NW"));
            epService.EPRuntime.SendEvent(new SupportSpatialAABB("R3", 0, 0, 30, 20, "NW"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResult.DeploymentId);
        }
    
        private void RunAssertionEventIndexOnTriggerNWInsertRemove(EPServiceProvider epService, bool soda) {
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create window MyWindow#length(5) as select * from SupportSpatialPoint");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create index MyIndex on MyWindow((px,py) pointregionquadtree(0,0,100,100))");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "insert into MyWindow select * from SupportSpatialPoint");
    
            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + " on SupportSpatialAABB as aabb " +
                    "select points.id as c0 from MyWindow as points where point(px,py).inside(rectangle(x,y,width,height))";
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("MyIndex", "non-unique hash={} btree={} advanced={pointregionquadtree(px,py)}");
    
            SendPoint(epService, "P1", 10, 40);
            AssertRectanglesManyRow(epService, listener, BOXES, "P1", null, null, null, null);
    
            SendPoint(epService, "P2", 80, 80);
            AssertRectanglesManyRow(epService, listener, BOXES, "P1", null, null, "P2", null);
    
            SendPoint(epService, "P3", 10, 40);
            AssertRectanglesManyRow(epService, listener, BOXES, "P1,P3", null, null, "P2", null);
    
            SendPoint(epService, "P4", 60, 40);
            AssertRectanglesManyRow(epService, listener, BOXES, "P1,P3", "P4", null, "P2", "P4");
    
            SendPoint(epService, "P5", 20, 75);
            AssertRectanglesManyRow(epService, listener, BOXES, "P1,P3", "P4", "P5", "P2", "P4");
    
            SendPoint(epService, "P6", 50, 50);
            AssertRectanglesManyRow(epService, listener, BOXES, "P3", "P4", "P5", "P2,P6", "P4,P6");
    
            SendPoint(epService, "P7", 0, 0);
            AssertRectanglesManyRow(epService, listener, BOXES, "P3,P7", "P4", "P5", "P6", "P4,P6");
    
            SendPoint(epService, "P8", 99.999, 0);
            AssertRectanglesManyRow(epService, listener, BOXES, "P7", "P4,P8", "P5", "P6", "P4,P6");
    
            SendPoint(epService, "P9", 0, 99.999);
            AssertRectanglesManyRow(epService, listener, BOXES, "P7", "P8", "P5,P9", "P6", "P6");
    
            SendPoint(epService, "P10", 99.999, 99.999);
            AssertRectanglesManyRow(epService, listener, BOXES, "P7", "P8", "P9", "P6,P10", "P6");
    
            SendPoint(epService, "P11", 0, 0);
            AssertRectanglesManyRow(epService, listener, BOXES, "P7,P11", "P8", "P9", "P10", null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventIndexOnTriggerTable(EPServiceProvider epService) {
            var epl =
                    "create table MyPointTable(my_x double primary key, my_y double primary key, my_id string);\n" +
                            "@Audit create index MyIndex on MyPointTable( (my_x, my_y) pointregionquadtree(0, 0, 100, 100));\n" +
                            "on SupportSpatialPoint ssp merge MyPointTable where ssp.px = my_x and ssp.py = my_y when not matched then insert select px as my_x, py as my_y, id as my_id;\n" +
                            IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                            "@Audit @Name('s0') on SupportSpatialAABB select my_id as c0 from MyPointTable as c0 where point(my_x, my_y).inside(rectangle(x, y, width, height))";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
    
            var stmt = epService.EPAdministrator.GetStatement("s0");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("MyIndex", "non-unique hash={} btree={} advanced={pointregionquadtree(my_x,my_y)}");
    
            SendPoint(epService, "P1", 55, 45);
            AssertRectanglesManyRow(epService, listener, BOXES, null, "P1", null, null, "P1");
    
            SendPoint(epService, "P2", 45, 45);
            AssertRectanglesManyRow(epService, listener, BOXES, "P2", "P1", null, null, "P1,P2");
    
            SendPoint(epService, "P3", 55, 55);
            AssertRectanglesManyRow(epService, listener, BOXES, "P2", "P1", null, "P3", "P1,P2,P3");
    
            epService.EPRuntime.ExecuteQuery("delete from MyPointTable where my_x = 55 and my_y = 45");
            SendPoint(epService, "P4", 45, 55);
            AssertRectanglesManyRow(epService, listener, BOXES, "P2", null, "P4", "P3", "P2,P3,P4");
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void AssertIndexChoice(EPServiceProvider epService, string hint, string expectedIndexName) {
            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + hint +
                    "on SupportSpatialAABB as aabb select mpw.id as c0 from MyPointWindow as mpw " +
                    "where aabb.category = mpw.category and point(px, py).inside(rectangle(x, y, width, height))\n";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var plan = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(expectedIndexName, plan.Tables[0].IndexName);
    
            epService.EPRuntime.SendEvent(new SupportSpatialAABB("R1", 9, 14, 1.0001, 1.0001, "Y"));
            Assert.AreEqual("P2", listener.AssertOneGetNewAndReset().Get("c0"));
    
            stmt.Dispose();
        }
    
        public class MyEventRectangleWithOffset {
            private readonly string id;
            private readonly double? xOffset;
            private readonly double? yOffset;
            private readonly double? x;
            private readonly double? y;
            private readonly double? width;
            private readonly double? height;
    
            public MyEventRectangleWithOffset(string id, double? xOffset, double? yOffset, double? x, double? y, double? width, double? height) {
                this.id = id;
                this.xOffset = xOffset;
                this.yOffset = yOffset;
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
    
            public string GetId() {
                return id;
            }
    
            public double? GetxOffset() {
                return xOffset;
            }
    
            public double? GetyOffset() {
                return yOffset;
            }
    
            public double? GetX() {
                return x;
            }
    
            public double? GetY() {
                return y;
            }
    
            public double? GetWidth() {
                return width;
            }
    
            public double? GetHeight() {
                return height;
            }
        }
    }
} // end of namespace
