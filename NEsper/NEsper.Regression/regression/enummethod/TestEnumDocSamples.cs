///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumDocSamples
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Item", typeof(Item));
            config.AddEventType("LocationReport", typeof(LocationReport));
            config.AddEventType("Zone", typeof(Zone));
            config.AddPlugInSingleRowFunction("inrect", typeof(LRUtil).FullName, "Inrect");
            config.AddPlugInSingleRowFunction("distance", typeof(LRUtil).FullName, "Distance");
            config.AddPlugInSingleRowFunction("getZoneNames", typeof(Zone).FullName, "GetZoneNames");

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestHowToUse()
        {
            String eplFragment = "select items.where(i => i.Location.X = 0 and i.Location.Y = 0) as zeroloc from LocationReport";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());

            Item[] items = _listener.AssertOneGetNewAndReset().Get("zeroloc").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P00020", items[0].AssetId);

            stmtFragment.Dispose();
            eplFragment = "select items.where(i => i.Location.X = 0).where(i => i.Location.Y = 0) as zeroloc from LocationReport";
            stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());

            items = _listener.AssertOneGetNewAndReset().Get("zeroloc").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P00020", items[0].AssetId);
        }

        [Test]
        public void TestSubquery()
        {
            String eplFragment = "select assetId," +
                    "  (select * from Zone#keepall).where(z => inrect(z.Rectangle, location)) as zones " +
                    "from Item";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
            _epService.EPRuntime.SendEvent(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
            _epService.EPRuntime.SendEvent(new Item("A1", new Location(10, 10)));

            Zone[] zones = _listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z1", zones[0].Name);

            // subquery with event as input
            String epl = "create schema SettlementEvent (symbol string, price double);" +
                         "create schema PriceEvent (symbol string, price double);\n" +
                         "create schema OrderEvent (orderId string, pricedata PriceEvent);\n" +
                         "select (select pricedata from OrderEvent#unique(orderId))\n" +
                                ".anyOf(v => v.symbol = 'GE') as has_ge from SettlementEvent(symbol = 'GE')";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            // subquery with aggregation
            _epService.EPAdministrator.CreateEPL("select (select name, count(*) as cnt from Zone#keepall group by name).where(v => cnt > 1) from LocationReport");
        }

        [Test]
        public void TestNamedWindow()
        {
            _epService.EPAdministrator.CreateEPL("create window ZoneWindow#keepall as Zone");
            _epService.EPAdministrator.CreateEPL("insert into ZoneWindow select * from Zone");

            String epl = "select ZoneWindow.where(z => inrect(z.Rectangle, location)) as zones from Item";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
            _epService.EPRuntime.SendEvent(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
            _epService.EPRuntime.SendEvent(new Item("A1", new Location(10, 10)));

            Zone[] zones = _listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z1", zones[0].Name);
            stmt.Dispose();

            epl = "select ZoneWindow(name in ('Z4', 'Z5', 'Z3')).where(z => inrect(z.Rectangle, location)) as zones from Item";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Zone("Z3", new Rectangle(0, 0, 20, 20)));
            _epService.EPRuntime.SendEvent(new Item("A1", new Location(10, 10)));

            zones = _listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z3", zones[0].Name);
        }

        [Test]
        public void TestAccessAggWindow()
        {
            String epl = "select Window(*).where(p => distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) group by assetId";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Item("P0001", new Location(10, 10), "P", null));
            Item[] items = _listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P0001", items[0].AssetId);

            _epService.EPRuntime.SendEvent(new Item("P0002", new Location(10, 1000), "P", null));
            items = _listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(0, items.Length);
        }

        [Test]
        public void TestPrevWindow()
        {
            String epl = "select prevwindow(items).where(p => distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) as items";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Item("P0001", new Location(10, 10), "P", null));
            Item[] items = _listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P0001", items[0].AssetId);

            _epService.EPRuntime.SendEvent(new Item("P0002", new Location(10, 1000), "P", null));
            items = _listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P0001", items[0].AssetId);
        }

        [Test]
        public void TestProperties()
        {
            String epl = "select items.where(p => distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from LocationReport";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());
            Item[] items = _listener.AssertOneGetNewAndReset().Get("centeritems").UnwrapIntoArray<Item>();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("P00020", items[0].AssetId);
        }

        [Test]
        public void TestUDFSingleRow()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof(ZoneFactory));

            String epl = "select ZoneFactory.GetZones().where(z => inrect(z.Rectangle, item.Location)) as zones\n" +
                    "from Item as item";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Item("A1", new Location(5, 5)));
            Zone[] zones = _listener.AssertOneGetNewAndReset().Get("zones").UnwrapIntoArray<Zone>();
            Assert.AreEqual(1, zones.Length);
            Assert.AreEqual("Z1", zones[0].Name);
        }

        [Test]
        public void TestDeclared()
        {
            String epl = "expression passengers {\n" +
                    "  lr => lr.items.where(l => l.type='P')\n" +
                    "}\n" +
                    "select passengers(lr) as p," +
                    "passengers(lr).where(x => assetId = 'P01') as p2 from LocationReport lr";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(LocationReportFactory.MakeSmall());
            Item[] items = _listener.AssertOneGetNewAndReset().Get("p").UnwrapIntoArray<Item>();
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("P00002", items[0].AssetId);
            Assert.AreEqual("P00020", items[1].AssetId);
        }

        [Test]
        public void TestExpressions()
        {
            AssertStmt("select items.firstof().assetId as firstcenter from LocationReport");
            AssertStmt("select items.where(p => p.type=\"P\") from LocationReport");
            AssertStmt("select items.where((p,ind) => p.type=\"P\" and ind>2) from LocationReport");
            AssertStmt("select items.aggregate(\"\",(result,item) => result||(case when result=\"\" then \"\" else \",\" end)||item.assetId) as assets from LocationReport");
            AssertStmt("select items.allof(i => distance(i.Location.X,i.Location.Y,0,0)<1000) as assets from LocationReport");
            AssertStmt("select items.average(i => distance(i.Location.X,i.Location.Y,0,0)) as avgdistance from LocationReport");
            AssertStmt("select items.countof(i => distance(i.Location.X,i.Location.Y,0,0)<20) as cntcenter from LocationReport");
            AssertStmt("select items.firstof(i => distance(i.Location.X,i.Location.Y,0,0)<20) as firstcenter from LocationReport");
            AssertStmt("select items.lastof().assetId as firstcenter from LocationReport");
            AssertStmt("select items.lastof(i => distance(i.Location.X,i.Location.Y,0,0)<20) as lastcenter from LocationReport");
            AssertStmt("select items.where(i => i.type=\"L\").groupby(i => assetIdPassenger) as luggagePerPerson from LocationReport");
            AssertStmt("select items.where((p,ind) => p.type=\"P\" and ind>2) from LocationReport");
            AssertStmt("select items.groupby(k => assetId,v => distance(v.Location.X,v.Location.Y,0,0)) as distancePerItem from LocationReport");
            AssertStmt("select items.min(i => distance(i.Location.X,i.Location.Y,0,0)) as mincenter from LocationReport");
            AssertStmt("select items.max(i => distance(i.Location.X,i.Location.Y,0,0)) as maxcenter from LocationReport");
            AssertStmt("select items.minBy(i => distance(i.Location.X,i.Location.Y,0,0)) as minItemCenter from LocationReport");
            AssertStmt("select items.minBy(i => distance(i.Location.X,i.Location.Y,0,0)).assetId as minItemCenter from LocationReport");
            AssertStmt("select items.orderBy(i => distance(i.Location.X,i.Location.Y,0,0)) as itemsOrderedByDist from LocationReport");
            AssertStmt("select items.selectFrom(i => assetId) as itemAssetIds from LocationReport");
            AssertStmt("select items.take(5) as first5Items, items.takeLast(5) as last5Items from LocationReport");
            AssertStmt("select items.toMap(k => k.assetId,v => distance(v.Location.X,v.Location.Y,0,0)) as assetDistance from LocationReport");
            AssertStmt("select items.where(i => i.assetId=\"L001\").union(items.where(i => i.type=\"P\")) as itemsUnion from LocationReport");
            AssertStmt("select (select name from Zone#unique(name)).orderBy() as orderedZones from pattern [every timer:interval(30)]");
            AssertStmt("create schema MyEvent as (seqone String[], seqtwo String[])");
            AssertStmt("select seqone.sequenceEqual(seqtwo) from MyEvent");
            AssertStmt("select window(assetId).orderBy() as orderedAssetIds from Item#time(10) group by assetId");
            AssertStmt("select prevwindow(assetId).orderBy() as orderedAssetIds from Item#time(10) as items");
            AssertStmt("select GetZoneNames().where(z => z!=\"Z1\") from pattern [every timer:interval(30)]");
            AssertStmt("select items.selectFrom(i => new{assetId,distanceCenter=distance(i.Location.X,i.Location.Y,0,0)}) as itemInfo from LocationReport");
            AssertStmt("select items.leastFrequent(i => type) as leastFreqType from LocationReport");

            String epl="expression myquery {itm => " +
                    "(select * from Zone#keepall).where(z => inrect(z.Rectangle,itm.Location))" +
                    "} " +
                    "select assetId, myquery(item) as subq, myquery(item).where(z => z.name=\"Z01\") as assetItem " +
                    "from Item as item";
            AssertStmt(epl);

            AssertStmt("select za.items.except(zb.items) as itemsCompared from LocationReport as za unidirectional, LocationReport#length(10) as zb");
        }

        [Test]
        public void TestScalarArray()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            Validate("{1, 2, 3}.aggregate(0, (result, value) => result + value)", 6);
            Validate("{1, 2, 3}.allOf(v => v > 0)", true);
            Validate("{1, 2, 3}.allOf(v => v > 1)", false);
            Validate("{1, 2, 3}.anyOf(v => v > 1)", true);
            Validate("{1, 2, 3}.anyOf(v => v > 3)", false);
            Validate("{1, 2, 3}.average()", 2.0);
            Validate("{1, 2, 3}.countOf()", 3);
            Validate("{1, 2, 3}.countOf(v => v < 2)", 1);
            Validate("{1, 2, 3}.except({1})", new Object[] { 2, 3 });
            Validate("{1, 2, 3}.intersect({2,3})", new Object[] { 2, 3 });
            Validate("{1, 2, 3}.firstOf()", 1);
            Validate("{1, 2, 3}.firstOf(v => v / 2 = 1)", 2);
            Validate("{1, 2, 3}.intersect({2, 3})", new Object[] { 2, 3 });
            Validate("{1, 2, 3}.lastOf()", 3);
            Validate("{1, 2, 3}.lastOf(v => v < 3)", 2);
            Validate("{1, 2, 3, 2, 1}.leastFrequent()", 3);
            Validate("{1, 2, 3, 2, 1}.max()", 3);
            Validate("{1, 2, 3, 2, 1}.min()", 1);
            Validate("{1, 2, 3, 2, 1, 2}.mostFrequent()", 2);
            Validate("{2, 3, 2, 1}.orderBy()", new Object[] { 1, 2, 2, 3 });
            Validate("{2, 3, 2, 1}.distinctOf()", new Object[] { 2, 3, 1 });
            Validate("{2, 3, 2, 1}.reverse()", new Object[] { 1, 2, 3, 2 });
            Validate("{1, 2, 3}.sequenceEqual({1})", false);
            Validate("{1, 2, 3}.sequenceEqual({1, 2, 3})", true);
            Validate("{1, 2, 3}.sumOf()", 6);
            Validate("{1, 2, 3}.take(2)", new Object[] { 1, 2 });
            Validate("{1, 2, 3}.takeLast(2)", new Object[] { 2, 3 });
            Validate("{1, 2, 3}.takeWhile(v => v < 3)", new Object[] { 1, 2 });
            Validate("{1, 2, 3}.takeWhile((v,ind) => ind < 2)", new Object[] { 1, 2 });
            Validate("{1, 2, -1, 4, 5, 6}.takeWhile((v,ind) => ind < 5 and v > 0)", new Object[] { 1, 2 });
            Validate("{1, 2, 3}.takeWhileLast(v => v > 1)", new Object[] { 2, 3 });
            Validate("{1, 2, 3}.takeWhileLast((v,ind) => ind < 2)", new Object[] { 2, 3 });
            Validate("{1, 2, -1, 4, 5, 6}.takeWhileLast((v,ind) => ind < 5 and v > 0)", new Object[] { 4, 5, 6 });
            Validate("{1, 2, 3}.union({4, 5})", new Object[] { 1, 2, 3, 4, 5 });
            Validate("{1, 2, 3}.where(v => v != 2)", new Object[] { 1, 3 });
        }

        private void Validate(String select, Object expected)
        {
            String epl = "select " + select + " as result from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Object result = _listener.AssertOneGetNewAndReset().Get("result");

            if (expected is Object[])
            {
                Object[] returned = ((IEnumerable)result).Cast<object>().ToArray();
                EPAssertionUtil.AssertEqualsExactOrder((Object[])expected, returned);
            }
            else
            {
                Assert.AreEqual(expected, result);
            }

            stmt.Dispose();
        }


        private void AssertStmt(String epl)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Dispose();

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());

            stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmt.Text);
        }
    }
}
