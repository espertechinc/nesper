///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumDocSamples : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Item", typeof(Item));
            configuration.AddEventType("LocationReport", typeof(LocationReport));
            configuration.AddEventType("Zone", typeof(Zone));
            configuration.AddPlugInSingleRowFunction("inrect", typeof(LRUtil).Name, "inrect");
            configuration.AddPlugInSingleRowFunction("distance", typeof(LRUtil).Name, "distance");
            configuration.AddPlugInSingleRowFunction("getZoneNames", typeof(Zone).Name, "getZoneNames");
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionHowToUse(epService);
            RunAssertionSubquery(epService);
            RunAssertionNamedWindow(epService);
            RunAssertionAccessAggWindow(epService);
            RunAssertionPrevWindow(epService);
            RunAssertionProperties(epService);
            RunAssertionUDFSingleRow(epService);
            RunAssertionDeclared(epService);
            RunAssertionExpressions(epService);
            RunAssertionScalarArray(epService);
        }
    
        private void RunAssertionHowToUse(EPServiceProvider epService) {
            string eplFragment = "select Items.Where(i => i.location.x = 0 and i.location.y = 0) as zeroloc from LocationReport";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());
    
            Item[] items = listener.AssertOneGetNewAndReset().Get("zeroloc").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P00020", items[0].AssetId);
    
            stmtFragment.Dispose();
            eplFragment = "select Items.Where(i => i.location.x = 0).Where(i => i.location.y = 0) as zeroloc from LocationReport";
            stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());
    
            items = listener.AssertOneGetNewAndReset().Get("zeroloc").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P00020", items[0].AssetId);
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionSubquery(EPServiceProvider epService) {
    
            string eplFragment = "select assetId," +
                    "  (select * from Zone#keepall).Where(z => Inrect(z.rectangle, location)) as zones " +
                    "from Item";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
            epService.EPRuntime.SendEvent(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
            epService.EPRuntime.SendEvent(new Item("A1", new Location(10, 10)));
    
            Zone[] zones = listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z1", zones[0].Name);
    
            // subquery with event as input
            string epl = "create schema SettlementEvent (symbol string, price double);" +
                    "create schema PriceEvent (symbol string, price double);\n" +
                    "create schema OrderEvent (orderId string, pricedata PriceEvent);\n" +
                    "select (select pricedata from OrderEvent#unique(orderId))\n" +
                    ".AnyOf(v => v.symbol = 'GE') as has_ge from SettlementEvent(symbol = 'GE')";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            // subquery with aggregation
            epService.EPAdministrator.CreateEPL("select (select name, count(*) as cnt from Zone#keepall group by name).Where(v => cnt > 1) from LocationReport");
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionNamedWindow(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window ZoneWindow#keepall as Zone");
            epService.EPAdministrator.CreateEPL("insert into ZoneWindow select * from Zone");
    
            string epl = "select ZoneWindow.Where(z => Inrect(z.rectangle, location)) as zones from Item";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
            epService.EPRuntime.SendEvent(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
            epService.EPRuntime.SendEvent(new Item("A1", new Location(10, 10)));
    
            Zone[] zones = listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z1", zones[0].Name);
            stmt.Dispose();
    
            epl = "select ZoneWindow(name in ('Z4', 'Z5', 'Z3')).Where(z => Inrect(z.rectangle, location)) as zones from Item";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Zone("Z3", new Rectangle(0, 0, 20, 20)));
            epService.EPRuntime.SendEvent(new Item("A1", new Location(10, 10)));
    
            zones = listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z3", zones[0].Name);
    
            stmt.Dispose();
        }
    
        private void RunAssertionAccessAggWindow(EPServiceProvider epService) {
            string epl = "select window(*).Where(p => Distance(0, 0, p.location.x, p.location.y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) group by assetId";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Item("P0001", new Location(10, 10), "P", null));
            Item[] items = listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P0001", items[0].AssetId);
    
            epService.EPRuntime.SendEvent(new Item("P0002", new Location(10, 1000), "P", null));
            items = listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(0, items.Length);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPrevWindow(EPServiceProvider epService) {
            string epl = "select prevwindow(items).Where(p => Distance(0, 0, p.location.x, p.location.y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) as items";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Item("P0001", new Location(10, 10), "P", null));
            Item[] items = listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P0001", items[0].AssetId);
    
            epService.EPRuntime.SendEvent(new Item("P0002", new Location(10, 1000), "P", null));
            items = listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P0001", items[0].AssetId);
    
            stmt.Dispose();
        }
    
        private void RunAssertionProperties(EPServiceProvider epService) {
            string epl = "select Items.Where(p => Distance(0, 0, p.location.x, p.location.y) < 20) as centeritems " +
                    "from LocationReport";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());
            Item[] items = listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P00020", items[0].AssetId);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUDFSingleRow(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(ZoneFactory));
    
            string epl = "select ZoneFactory.Zones.Where(z => Inrect(z.rectangle, item.location)) as zones\n" +
                    "from Item as item";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Item("A1", new Location(5, 5)));
            Zone[] zones = listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z1", zones[0].Name);
    
            stmt.Dispose();
        }
    
        private void RunAssertionDeclared(EPServiceProvider epService) {
            string epl = "expression passengers {\n" +
                    "  lr => lr.items.Where(l => l.type='P')\n" +
                    "}\n" +
                    "select Passengers(lr) as p," +
                    "Passengers(lr).Where(x => assetId = 'P01') as p2 from LocationReport lr";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());
            Item[] items = listener.AssertOneGetNewAndReset().Get("p").UnwrapIntoArray<Item>();
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("P00002", items[0].AssetId);
            Assert.AreEqual("P00020", items[1].AssetId);
    
            stmt.Dispose();
        }
    
        private void RunAssertionExpressions(EPServiceProvider epService) {
            AssertStmt(epService, "select Items.Firstof().assetId as firstcenter from LocationReport");
            AssertStmt(epService, "select Items.Where(p => p.type=\"P\") from LocationReport");
            AssertStmt(epService, "select Items.Where((p,ind) => p.type=\"P\" and ind>2) from LocationReport");
            AssertStmt(epService, "select Items.Aggregate(\"\",(result,item) => result||(case when result=\"\" then \"\" else \",\" end)||item.assetId) as assets from LocationReport");
            AssertStmt(epService, "select Items.Allof(i => Distance(i.location.x,i.location.y,0,0)<1000) as assets from LocationReport");
            AssertStmt(epService, "select Items.Average(i => Distance(i.location.x,i.location.y,0,0)) as avgdistance from LocationReport");
            AssertStmt(epService, "select Items.Countof(i => Distance(i.location.x,i.location.y,0,0)<20) as cntcenter from LocationReport");
            AssertStmt(epService, "select Items.Firstof(i => Distance(i.location.x,i.location.y,0,0)<20) as firstcenter from LocationReport");
            AssertStmt(epService, "select Items.Lastof().assetId as firstcenter from LocationReport");
            AssertStmt(epService, "select Items.Lastof(i => Distance(i.location.x,i.location.y,0,0)<20) as lastcenter from LocationReport");
            AssertStmt(epService, "select Items.Where(i => i.type=\"L\").Groupby(i => assetIdPassenger) as luggagePerPerson from LocationReport");
            AssertStmt(epService, "select Items.Where((p,ind) => p.type=\"P\" and ind>2) from LocationReport");
            AssertStmt(epService, "select Items.Groupby(k => assetId,v => Distance(v.location.x,v.location.y,0,0)) as distancePerItem from LocationReport");
            AssertStmt(epService, "select Items.min(i => Distance(i.location.x,i.location.y,0,0)) as mincenter from LocationReport");
            AssertStmt(epService, "select Items.max(i => Distance(i.location.x,i.location.y,0,0)) as maxcenter from LocationReport");
            AssertStmt(epService, "select Items.MinBy(i => Distance(i.location.x,i.location.y,0,0)) as minItemCenter from LocationReport");
            AssertStmt(epService, "select Items.MinBy(i => Distance(i.location.x,i.location.y,0,0)).assetId as minItemCenter from LocationReport");
            AssertStmt(epService, "select Items.OrderBy(i => Distance(i.location.x,i.location.y,0,0)) as itemsOrderedByDist from LocationReport");
            AssertStmt(epService, "select Items.SelectFrom(i => assetId) as itemAssetIds from LocationReport");
            AssertStmt(epService, "select Items.Take(5) as first5Items, items.TakeLast(5) as last5Items from LocationReport");
            AssertStmt(epService, "select Items.ToMap(k => k.assetId,v => Distance(v.location.x,v.location.y,0,0)) as assetDistance from LocationReport");
            AssertStmt(epService, "select Items.Where(i => i.assetId=\"L001\").Union(items.Where(i => i.type=\"P\")) as itemsUnion from LocationReport");
            AssertStmt(epService, "select (select name from Zone#unique(name)).OrderBy() as orderedZones from pattern [every timer:interval(30)]");
            epService.EPAdministrator.CreateEPL("create schema MyEvent as (seqone string[], seqtwo string[])");
            AssertStmt(epService, "select Seqone.SequenceEqual(seqtwo) from MyEvent");
            AssertStmt(epService, "select window(assetId).OrderBy() as orderedAssetIds from Item#time(10) group by assetId");
            AssertStmt(epService, "select prevwindow(assetId).OrderBy() as orderedAssetIds from Item#time(10) as items");
            AssertStmt(epService, "select GetZoneNames().Where(z => z!=\"Z1\") from pattern [every timer:interval(30)]");
            AssertStmt(epService, "select Items.SelectFrom(i => new{assetId,distanceCenter=Distance(i.location.x,i.location.y,0,0)}) as itemInfo from LocationReport");
            AssertStmt(epService, "select Items.LeastFrequent(i => type) as leastFreqType from LocationReport");
    
            string epl = "expression myquery {itm => " +
                    "(select * from Zone#keepall).Where(z => Inrect(z.rectangle,itm.location))" +
                    "} " +
                    "select assetId, Myquery(item) as subq, Myquery(item).Where(z => z.name=\"Z01\") as assetItem " +
                    "from Item as item";
            AssertStmt(epService, epl);
    
            AssertStmt(epService, "select Za.items.Except(zb.items) as itemsCompared from LocationReport as za unidirectional, LocationReport#length(10) as zb");
        }
    
        private void RunAssertionScalarArray(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            Validate(epService, "{1, 2, 3}.Aggregate(0, (result, value) => result + value)", 6);
            Validate(epService, "{1, 2, 3}.AllOf(v => v > 0)", true);
            Validate(epService, "{1, 2, 3}.AllOf(v => v > 1)", false);
            Validate(epService, "{1, 2, 3}.AnyOf(v => v > 1)", true);
            Validate(epService, "{1, 2, 3}.AnyOf(v => v > 3)", false);
            Validate(epService, "{1, 2, 3}.Average()", 2.0);
            Validate(epService, "{1, 2, 3}.CountOf()", 3);
            Validate(epService, "{1, 2, 3}.CountOf(v => v < 2)", 1);
            Validate(epService, "{1, 2, 3}.Except({1})", new object[]{2, 3});
            Validate(epService, "{1, 2, 3}.Intersect({2,3})", new object[]{2, 3});
            Validate(epService, "{1, 2, 3}.FirstOf()", 1);
            Validate(epService, "{1, 2, 3}.FirstOf(v => v / 2 = 1)", 2);
            Validate(epService, "{1, 2, 3}.Intersect({2, 3})", new object[]{2, 3});
            Validate(epService, "{1, 2, 3}.LastOf()", 3);
            Validate(epService, "{1, 2, 3}.LastOf(v => v < 3)", 2);
            Validate(epService, "{1, 2, 3, 2, 1}.LeastFrequent()", 3);
            Validate(epService, "{1, 2, 3, 2, 1}.max()", 3);
            Validate(epService, "{1, 2, 3, 2, 1}.min()", 1);
            Validate(epService, "{1, 2, 3, 2, 1, 2}.MostFrequent()", 2);
            Validate(epService, "{2, 3, 2, 1}.OrderBy()", new object[]{1, 2, 2, 3});
            Validate(epService, "{2, 3, 2, 1}.DistinctOf()", new object[]{2, 3, 1});
            Validate(epService, "{2, 3, 2, 1}.Reverse()", new object[]{1, 2, 3, 2});
            Validate(epService, "{1, 2, 3}.SequenceEqual({1})", false);
            Validate(epService, "{1, 2, 3}.SequenceEqual({1, 2, 3})", true);
            Validate(epService, "{1, 2, 3}.SumOf()", 6);
            Validate(epService, "{1, 2, 3}.Take(2)", new object[]{1, 2});
            Validate(epService, "{1, 2, 3}.TakeLast(2)", new object[]{2, 3});
            Validate(epService, "{1, 2, 3}.TakeWhile(v => v < 3)", new object[]{1, 2});
            Validate(epService, "{1, 2, 3}.TakeWhile((v,ind) => ind < 2)", new object[]{1, 2});
            Validate(epService, "{1, 2, -1, 4, 5, 6}.TakeWhile((v,ind) => ind < 5 and v > 0)", new object[]{1, 2});
            Validate(epService, "{1, 2, 3}.TakeWhileLast(v => v > 1)", new object[]{2, 3});
            Validate(epService, "{1, 2, 3}.TakeWhileLast((v,ind) => ind < 2)", new object[]{2, 3});
            Validate(epService, "{1, 2, -1, 4, 5, 6}.TakeWhileLast((v,ind) => ind < 5 and v > 0)", new object[]{4, 5, 6});
            Validate(epService, "{1, 2, 3}.Union({4, 5})", new object[]{1, 2, 3, 4, 5});
            Validate(epService, "{1, 2, 3}.Where(v => v != 2)", new object[]{1, 3});
        }
    
        private void Validate(EPServiceProvider epService, string select, Object expected) {
            string epl = "select " + select + " as result from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Object result = listener.AssertOneGetNewAndReset().Get("result");
    
            if (expected is object[]) {
                object[] returned = result.UnwrapIntoArray<object>();
                EPAssertionUtil.AssertEqualsExactOrder((object[]) expected, returned);
            } else {
                Assert.AreEqual(expected, result);
            }
    
            stmt.Dispose();
        }
    
    
        private void AssertStmt(EPServiceProvider epService, string epl) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Dispose();
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
    
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmt.Text);
    
            stmt.Dispose();
        }
    }
} // end of namespace
