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

using static com.espertech.esper.regression.spatial.ExecSpatialMXCIFQuadTreeFilterIndex;
using static com.espertech.esper.supportregression.util.SupportSpatialUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.spatial
{
    public class ExecSpatialMXCIFQuadTreeEventIndex : RegressionExecution {
    
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
            foreach (var clazz in Collections.List(typeof(SupportSpatialAABB), typeof(SupportSpatialEventRectangle), typeof(SupportSpatialDualAABB))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionEventIndexUnindexed(epService);
    
            RunAssertionEventIndexOnTriggerNWInsertRemove(epService, false);
            RunAssertionEventIndexOnTriggerNWInsertRemove(epService, true);
            RunAssertionEventIndexUnique(epService);
            RunAssertionEventIndexPerformance(epService);
            RunAssertionEventIndexTableFireAndForget(epService);
        }
    
        private void RunAssertionEventIndexTableFireAndForget(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTable(id string primary key, tx double, ty double, tw double, th double)");
            epService.EPRuntime.ExecuteQuery("insert into MyTable values ('R1', 10, 20, 5, 6)");
            epService.EPAdministrator.CreateEPL("create index MyIdxCIFQuadTree on MyTable( (tx, ty, tw, th) mxcifquadtree(0, 0, 100, 100))");
    
            RunAssertionFAF(epService, 10, 20, 0, 0, true);
            RunAssertionFAF(epService, 9, 19, 1, 1, true);
            RunAssertionFAF(epService, 9, 19, 0.9999, 0.9999, false);
            RunAssertionFAF(epService, 15, 26, 0, 0, true);
            RunAssertionFAF(epService, 15.0001, 26.0001, 0, 0, false);
            RunAssertionFAF(epService, 0, 0, 100, 100, true);
            RunAssertionFAF(epService, 11, 21, 1, 1, true);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFAF(EPServiceProvider epService, double x, double y, double width, double height, bool expected) {
            var result = epService.EPRuntime.ExecuteQuery(IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "select id as c0 from MyTable where rectangle(tx, ty, tw, th).intersects(rectangle(" + x + ", " + y + ", " + width + ", " + height + "))");
            SupportQueryPlanIndexHook.AssertFAFAndReset("MyIdxCIFQuadTree", "EventTableQuadTreeMXCIFImpl");
            if (expected) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, "c0".Split(','), new object[][]{new object[] {"R1"}});
            } else {
                Assert.AreEqual(0, result.Array.Length);
            }
        }
    
        private void RunAssertionEventIndexPerformance(EPServiceProvider epService) {
            var epl = "create window MyRectangleWindow#keepall as (id string, rx double, ry double, rw double, rh double);\n" +
                    "insert into MyRectangleWindow select id, x as rx, y as ry, width as rw, height as rh from SupportSpatialEventRectangle;\n" +
                    "create index Idx on MyRectangleWindow( (rx, ry, rw, rh) mxcifquadtree(0, 0, 100, 100));\n" +
                    "@Name('out') on SupportSpatialAABB select mpw.id as c0 from MyRectangleWindow as mpw where rectangle(rx, ry, rw, rh).intersects(rectangle(x, y, width, height));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            SendSpatialEventRectangles(epService, 100, 50);

            var delta = PerformanceObserver.TimeMillis(
                () => {
                    for (var x = 0; x < 100; x++) {
                        for (var y = 0; y < 50; y++) {
                            epService.EPRuntime.SendEvent(new SupportSpatialAABB("R", x, y, 0.5, 0.5));
                            Assert.AreEqual(
                                Convert.ToString(x) + "_" + Convert.ToString(y),
                                listener.AssertOneGetNewAndReset().Get("c0"));
                        }
                    }
                });

            Assert.That(delta, Is.LessThan(3000));
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexUnique(EPServiceProvider epService) {
            var epl = "@Name('win') create window MyRectWindow#keepall as (id string, rx double, ry double, rw double, rh double);\n" +
                    "@Name('insert') insert into MyRectWindow select id, x as rx, y as ry, width as rw, height as rh from SupportSpatialEventRectangle;\n" +
                    "@Name('idx') create unique index Idx on MyRectWindow( (rx, ry, rw, rh) mxcifquadtree(0, 0, 100, 100));\n" +
                    IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "@Name('out') on SupportSpatialAABB select mpw.id as c0 from MyRectWindow as mpw where rectangle(rx, ry, rw, rh).intersects(rectangle(x, y, width, height));\n";
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl).DeploymentId;
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("Idx", "unique hash={} btree={} advanced={mxcifquadtree(rx,ry,rw,rh)}");
    
            SendEventRectangle(epService, "P1", 10, 15, 1, 2);
            try {
                SendEventRectangle(epService, "P1", 10, 15, 1, 2);
                Assert.Fail();
            } catch (Exception ex) { // we have a handler
                SupportMessageAssertUtil.AssertMessage(ex,
                        "Unexpected exception in statement 'win': Unique index violation, index 'Idx' is a unique index and key '(10,15,1,2)' already exists");
            }
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
        }
    
        private void RunAssertionEventIndexUnindexed(EPServiceProvider epService) {
            var stmt = epService.EPAdministrator.CreateEPL("select rectangle(one.x, one.y, one.width, one.height).intersects(rectangle(two.x, two.y, two.width, two.height)) as c0 from SupportSpatialDualAABB");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // For example, in MySQL:
            // SET @g1 = ST_GeomFromText('Polygon((1 1,1 2,2 2,2 1,1 1))');
            // SET @g2 = ST_GeomFromText('Polygon((2 2,2 4,4 4,4 2,2 2))');
            // SELECT MBRIntersects(@g1,@g2), MBRIntersects(@g2,@g1);
            // includes exterior
    
            SendAssert(epService, listener, Rect(1, 1, 5, 5), Rect(2, 2, 2, 2), true);
            SendAssert(epService, listener, Rect(1, 1, 1, 1), Rect(2, 2, 2, 2), true);
            SendAssert(epService, listener, Rect(1, 0.9999, 1, 0.99999), Rect(2, 2, 2, 2), false);
            SendAssert(epService, listener, Rect(1, 1, 1, 0.99999), Rect(2, 2, 2, 2), false);
            SendAssert(epService, listener, Rect(1, 0.9999, 1, 1), Rect(2, 2, 2, 2), false);
    
            SendAssert(epService, listener, Rect(4, 4, 1, 1), Rect(2, 2, 2, 2), true);
            SendAssert(epService, listener, Rect(4.0001, 4, 1, 1), Rect(2, 2, 2, 2), false);
            SendAssert(epService, listener, Rect(4, 4.0001, 1, 1), Rect(2, 2, 2, 2), false);
    
            SendAssert(epService, listener, Rect(10, 20, 5, 5), Rect(0, 0, 50, 50), true);
            SendAssert(epService, listener, Rect(10, 20, 5, 5), Rect(20, 20, 50, 50), false);
            SendAssert(epService, listener, Rect(10, 20, 5, 5), Rect(9, 19, 1, 1), true);
            SendAssert(epService, listener, Rect(10, 20, 5, 5), Rect(15, 25, 1, 1), true);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventIndexOnTriggerNWInsertRemove(EPServiceProvider epService, bool soda) {
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create window MyWindow#length(5) as select * from SupportSpatialEventRectangle");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create index MyIndex on MyWindow((x,y,width,height) mxcifquadtree(0,0,100,100))");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "insert into MyWindow select * from SupportSpatialEventRectangle");
    
            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + " on SupportSpatialAABB as aabb " +
                    "select rects.id as c0 from MyWindow as rects where rectangle(rects.x,rects.y,rects.width,rects.height).intersects(rectangle(aabb.x,aabb.y,aabb.width,aabb.height))";
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertOnExprTableAndReset("MyIndex", "non-unique hash={} btree={} advanced={mxcifquadtree(x,y,width,height)}");
    
            SendEventRectangle(epService, "R1", 10, 40, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R1", null, null, null, null);
    
            SendEventRectangle(epService, "R2", 80, 80, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R1", null, null, "R2", null);
    
            SendEventRectangle(epService, "R3", 10, 40, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R1,R3", null, null, "R2", null);
    
            SendEventRectangle(epService, "R4", 60, 40, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R1,R3", "R4", null, "R2", "R4");
    
            SendEventRectangle(epService, "R5", 20, 75, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R1,R3", "R4", "R5", "R2", "R4");
    
            SendEventRectangle(epService, "R6", 50, 50, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R3,R6", "R4,R6", "R5,R6", "R2,R6", "R4,R6");
    
            SendEventRectangle(epService, "R7", 0, 0, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R3,R6,R7", "R4,R6", "R5,R6", "R6", "R4,R6");
    
            SendEventRectangle(epService, "R8", 99.999, 0, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R6,R7", "R4,R6,R8", "R5,R6", "R6", "R4,R6");
    
            SendEventRectangle(epService, "R9", 0, 99.999, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R6,R7", "R6,R8", "R5,R6,R9", "R6", "R6");
    
            SendEventRectangle(epService, "R10", 99.999, 99.999, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R6,R7", "R6,R8", "R6,R9", "R6,R10", "R6");
    
            SendEventRectangle(epService, "R11", 0, 0, 1, 1);
            AssertRectanglesManyRow(epService, listener, BOXES, "R7,R11", "R8", "R9", "R10", null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendEventRectangle(EPServiceProvider epService, string id, double x, double y, double width, double height) {
            epService.EPRuntime.SendEvent(new SupportSpatialEventRectangle(id, x, y, width, height));
        }
    
        private SupportSpatialAABB Rect(double x, double y, double width, double height) {
            return new SupportSpatialAABB(null, x, y, width, height);
        }
    
        private void SendAssert(EPServiceProvider epService, SupportUpdateListener listener, SupportSpatialAABB one, SupportSpatialAABB two, bool expected) {
            var bbOne = BoundingBox.From(one.X, one.Y, one.Width, one.Height);
            Assert.AreEqual(expected, bbOne.IntersectsBoxIncludingEnd(two.X, two.Y, two.Width, two.Height));
    
            var bbTwo = BoundingBox.From(two.X, two.Y, two.Width, two.Height);
            Assert.AreEqual(expected, bbTwo.IntersectsBoxIncludingEnd(one.X, one.Y, one.Width, one.Height));
    
            epService.EPRuntime.SendEvent(new SupportSpatialDualAABB(one, two));
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
    
            epService.EPRuntime.SendEvent(new SupportSpatialDualAABB(two, one));
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
        }
    }
} // end of namespace
