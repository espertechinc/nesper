///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.lrreport;

// singletonList
using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumDocSamples
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumExpressions());
            execs.Add(new ExprEnumHowToUse());
            execs.Add(new ExprEnumSubquery());
            execs.Add(new ExprEnumNamedWindow());
            execs.Add(new ExprEnumAccessAggWindow());
            execs.Add(new ExprEnumPrevWindow());
            execs.Add(new ExprEnumProperties());
            execs.Add(new ExprEnumUDFSingleRow());
            execs.Add(new ExprEnumScalarArray());
            execs.Add(new ExprEnumDeclared());
            return execs;
        }

        private class ExprEnumHowToUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@name('s0') select items.where(i => i.location.x = 0 and i.location.y = 0) as zeroloc from LocationReport";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("zeroloc"));
                        Assert.AreEqual(1, items.Length);
                        Assert.AreEqual("P00020", items[0].AssetId);
                    });

                env.UndeployAll();
                eplFragment =
                    "@name('s0') select items.where(i => i.location.x = 0).where(i => i.location.y = 0) as zeroloc from LocationReport";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("zeroloc"));
                        Assert.AreEqual(1, items.Length);
                        Assert.AreEqual("P00020", items[0].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment = "@name('s0') select assetId," +
                                  "  (select * from Zone#keepall).where(z => inrect(z.rectangle, location)) as zones " +
                                  "from Item";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        Assert.AreEqual(1, zones.Length);
                        Assert.AreEqual("Z1", zones[0].Name);
                    });

                // subquery with event as input
                var epl = "create schema SettlementEvent (symbol string, price double);" +
                          "create schema PriceEvent (symbol string, price double);\n" +
                          "create schema OrderEvent (orderId string, pricedata PriceEvent);\n" +
                          "select (select pricedata from OrderEvent#unique(orderId))\n" +
                          ".anyOf(v => v.symbol = 'GE') as has_ge from SettlementEvent(symbol = 'GE')";
                env.CompileDeploy(epl);

                // subquery with aggregation
                env.CompileDeploy(
                    "select (select name, count(*) as cnt from Zone#keepall group by name).where(v => cnt > 1) from LocationReport");

                env.UndeployAll();
            }
        }

        private class ExprEnumNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                var path = new RegressionPath();
                env.CompileDeploy("@public create window ZoneWindow#keepall as Zone", path);
                env.CompileDeploy("insert into ZoneWindow select * from Zone", path);

                epl = "@name('s0') select ZoneWindow.where(z => inrect(z.rectangle, location)) as zones from Item";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        Assert.AreEqual(1, zones.Length);
                        Assert.AreEqual("Z1", zones[0].Name);
                    });

                env.UndeployModuleContaining("s0");

                epl =
                    "@name('s0') select ZoneWindow(name in ('Z4', 'Z5', 'Z3')).where(z => inrect(z.rectangle, location)) as zones from Item";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new Zone("Z3", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        Assert.AreEqual(1, zones.Length);
                        Assert.AreEqual("Z3", zones[0].Name);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumAccessAggWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select window(*).where(p => distance(0, 0, p.location.x, p.location.y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) group by assetId";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("P0001", new Location(10, 10), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        Assert.AreEqual(1, items.Length);
                        Assert.AreEqual("P0001", items[0].AssetId);
                    });

                env.SendEventBean(new Item("P0002", new Location(10, 1000), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        Assert.AreEqual(0, items.Length);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumPrevWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select prevwindow(items).where(p => distance(0, 0, p.location.x, p.location.y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) as items";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("P0001", new Location(10, 10), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        Assert.AreEqual(1, items.Length);
                        Assert.AreEqual("P0001", items[0].AssetId);
                    });

                env.SendEventBean(new Item("P0002", new Location(10, 1000), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        Assert.AreEqual(1, items.Length);
                        Assert.AreEqual("P0001", items[0].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select items.where(p => distance(0, 0, p.location.x, p.location.y) < 20) as centeritems " +
                    "from LocationReport";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        Assert.AreEqual(1, items.Length);
                        Assert.AreEqual("P00020", items[0].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumUDFSingleRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select ZoneFactory.getZones().where(z => inrect(z.rectangle, item.location)) as zones\n" +
                    "from Item as item";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("A1", new Location(5, 5)));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        Assert.AreEqual(1, zones.Length);
                        Assert.AreEqual("Z1", zones[0].Name);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') expression passengers {\n" +
                          "  lr => lr.items.where(l => l.type='P')\n" +
                          "}\n" +
                          "select passengers(lr) as p," +
                          "passengers(lr).where(x => assetId = 'P01') as p2 from LocationReport lr";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("p"));
                        Assert.AreEqual(2, items.Length);
                        Assert.AreEqual("P00002", items[0].AssetId);
                        Assert.AreEqual("P00020", items[1].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                AssertStmt(env, path, "select items.firstof().assetId as firstcenter from LocationReport");
                AssertStmt(env, path, "select items.where(p => p.type=\"P\") from LocationReport");
                AssertStmt(env, path, "select items.where((p,ind) => p.type=\"P\" and ind>2) from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.aggregate(\"\",(result,item) => result||(case when result=\"\" then \"\" else \",\" end)||item.assetId) as assets from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.allof(i => distance(i.location.x,i.location.y,0,0)<1000) as assets from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.average(i => distance(i.location.x,i.location.y,0,0)) as avgdistance from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.countof(i => distance(i.location.x,i.location.y,0,0)<20) as cntcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.firstof(i => distance(i.location.x,i.location.y,0,0)<20) as firstcenter from LocationReport");
                AssertStmt(env, path, "select items.lastof().assetId as firstcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.lastof(i => distance(i.location.x,i.location.y,0,0)<20) as lastcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.where(i => i.type=\"L\").groupby(i => assetIdPassenger) as luggagePerPerson from LocationReport");
                AssertStmt(env, path, "select items.where((p,ind) => p.type=\"P\" and ind>2) from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.groupby(k => assetId,v => distance(v.location.x,v.location.y,0,0)) as distancePerItem from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.min(i => distance(i.location.x,i.location.y,0,0)) as mincenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.max(i => distance(i.location.x,i.location.y,0,0)) as maxcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.minBy(i => distance(i.location.x,i.location.y,0,0)) as minItemCenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.minBy(i => distance(i.location.x,i.location.y,0,0)).assetId as minItemCenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.orderBy(i => distance(i.location.x,i.location.y,0,0)) as itemsOrderedByDist from LocationReport");
                AssertStmt(env, path, "select items.selectFrom(i => assetId) as itemAssetIds from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.take(5) as first5Items, items.takeLast(5) as last5Items from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.toMap(k => k.assetId,v => distance(v.location.x,v.location.y,0,0)) as assetDistance from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select items.where(i => i.assetId=\"L001\").union(items.where(i => i.type=\"P\")) as itemsUnion from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select (select name from Zone#unique(name)).orderBy() as orderedZones from pattern [every timer:interval(30)]");

                env.CompileDeploy(
                    "@buseventtype @public create schema MyEvent as (seqone String[], seqtwo String[])",
                    path);

                AssertStmt(env, path, "select seqone.sequenceEqual(seqtwo) from MyEvent");
                AssertStmt(
                    env,
                    path,
                    "select window(assetId).orderBy() as orderedAssetIds from Item#time(10) group by assetId");
                AssertStmt(
                    env,
                    path,
                    "select prevwindow(assetId).orderBy() as orderedAssetIds from Item#time(10) as items");
                AssertStmt(
                    env,
                    path,
                    "select getZoneNames().where(z => z!=\"Z1\") from pattern [every timer:interval(30)]");
                AssertStmt(
                    env,
                    path,
                    "select items.selectFrom(i => new{assetId,distanceCenter=distance(i.location.x,i.location.y,0,0)}) as itemInfo from LocationReport");
                AssertStmt(env, path, "select items.leastFrequent(i => type) as leastFreqType from LocationReport");

                var epl = "expression myquery {itm => " +
                          "(select * from Zone#keepall).where(z => inrect(z.rectangle,itm.location))" +
                          "} " +
                          "select assetId, myquery(item) as subq, myquery(item).where(z => z.name=\"Z01\") as assetItem " +
                          "from Item as item";
                AssertStmt(env, path, epl);

                AssertStmt(
                    env,
                    path,
                    "select za.items.except(zb.items) as itemsCompared from LocationReport as za unidirectional, LocationReport#length(10) as zb");

                env.UndeployAll();
            }
        }

        private class ExprEnumScalarArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validate(env, "{1, 2, 3}.aggregate(0, (result, value) => result + value)", 6);
                Validate(env, "{1, 2, 3}.aggregate(0, (result, value, index) => result + value + index*10)", 36);
                Validate(
                    env,
                    "{1, 2, 3}.aggregate(0, (result, value, index, size) => result + value + index*10 + size*100)",
                    936);
                Validate(env, "{1, 2, 3}.allOf(v => v > 0)", true);
                Validate(env, "{1, 2, 3}.allOf(v => v > 1)", false);
                Validate(env, "{1, 2, 3}.allOf((v, index) => case when index < 2 then true else v > 1 end)", true);
                Validate(env, "{1, 2, 3}.allOf((v, index, size) => v > 1 or size >= 3)", true);
                Validate(env, "{1, 2, 3}.anyOf(v => v > 1)", true);
                Validate(env, "{1, 2, 3}.anyOf(v => v > 3)", false);
                Validate(env, "{1, 2, 3}.anyOf( (v, index) => case when index < 2 then false else v = 3 end)", true);
                Validate(env, "{1, 2, 3}.anyOf( (v, index, size) => v > 100 or size >= 3)", true);
                Validate(env, "{1, 2, 3}.average()", 2.0);
                Validate(env, "{1, 2, 3}.average(v => v+1)", 3d);
                Validate(env, "{1, 2, 3}.average((v, index) => v+10*index)", 12d);
                Validate(env, "{1, 2, 3}.average((v, index, size) => v+10*index + 100*size)", 312d);
                Validate(env, "{1, 2, 3}.countOf()", 3);
                Validate(env, "{1, 2, 3}.countOf(v => v < 2)", 1);
                Validate(env, "{1, 2, 3}.countOf( (v, index) => v > index)", 3);
                Validate(env, "{1, 2, 3}.countOf( (v, index, size) => v >= size)", 1);
                Validate(env, "{1, 2, 3}.except({1})", new object[] { 2, 3 });
                Validate(env, "{1, 2, 3}.intersect({2,3})", new object[] { 2, 3 });
                Validate(env, "{1, 2, 3}.firstOf()", 1);
                Validate(env, "{1, 2, 3}.firstOf(v => v / 2 = 1)", 2);
                Validate(env, "{1, 2, 3}.firstOf((v, index) => index = 1)", 2);
                Validate(env, "{1, 2, 3}.firstOf((v, index, size) => v = size-1)", 2);
                Validate(env, "{1, 2, 3}.intersect({2, 3})", new object[] { 2, 3 });
                Validate(env, "{1, 2, 3}.lastOf()", 3);
                Validate(env, "{1, 2, 3}.lastOf(v => v < 3)", 2);
                Validate(env, "{1, 2, 3}.lastOf((v, index) => index < 2 )", 2);
                Validate(env, "{1, 2, 3}.lastOf((v, index, size) => index < size - 2 )", 1);
                Validate(env, "{1, 2, 3, 2, 1}.leastFrequent()", 3);
                Validate(env, "{1, 2, 3, 2, 1}.leastFrequent(v => case when v = 3 then 4 else v end)", 4);
                Validate(env, "{1, 2, 3, 2, 1}.leastFrequent((v, index) => case when index = 2 then 4 else v end)", 4);
                Validate(
                    env,
                    "{1, 2, 3, 2, 1}.leastFrequent((v, index, size) => case when index = size - 2 then 4 else v end)",
                    2);
                Validate(env, "{1, 2, 3, 2, 1}.max()", 3);
                Validate(env, "{1, 2, 3, 2, 1}.max(v => case when v >= 3 then 0 else v end)", 2);
                Validate(env, "{1, 2, 3, 2, 1}.max((v, index) => case when index = 2 then 0 else v end)", 2);
                Validate(
                    env,
                    "{1, 2, 3, 2, 1}.max((v, index, size) => case when index > size - 4 then 0 else v end)",
                    2);
                Validate(env, "{1, 2, 3, 2, 1}.min()", 1);
                Validate(env, "{1, 2, 3, 2, 1}.min(v => v + 1)", 2);
                Validate(env, "{1, 2, 3, 2, 1}.min((v, index) => v - index)", -3);
                Validate(env, "{1, 2, 3, 2, 1}.min((v, index, size) => v - size)", -4);
                Validate(env, "{1, 2, 3, 2, 1, 2}.mostFrequent()", 2);
                Validate(env, "{1, 2, 3, 2, 1, 2}.mostFrequent(v => case when v = 2 then 10 else v end)", 10);
                Validate(
                    env,
                    "{1, 2, 3, 2, 1, 2}.mostFrequent((v, index) => case when index > 2 then 4 else v end)",
                    4);
                Validate(
                    env,
                    "{1, 2, 3, 2, 1, 2}.mostFrequent((v, index, size) => case when size > 3 then 0 else v end)",
                    0);
                Validate(env, "{2, 3, 2, 1}.orderBy()", new object[] { 1, 2, 2, 3 });
                Validate(env, "{2, 3, 2, 1}.orderBy(v => -v)", new object[] { 3, 2, 2, 1 });
                Validate(env, "{2, 3, 2, 1}.orderBy((v, index) => index)", new object[] { 2, 3, 2, 1 });
                Validate(
                    env,
                    "{2, 3, 2, 1}.orderBy((v, index, size) => case when index < size - 2 then v else -v end)",
                    new object[] { 2, 1, 2, 3 });
                Validate(env, "{2, 3, 2, 1}.distinctOf()", new object[] { 2, 3, 1 });
                Validate(
                    env,
                    "{2, 3, 2, 1}.distinctOf(v => case when v > 1 then 0 else -1 end)",
                    new object[] { 2, 1 });
                Validate(
                    env,
                    "{2, 3, 2, 1}.distinctOf((v, index) => case when index = 0 then 1 else 2 end)",
                    new object[] { 2, 3 });
                Validate(
                    env,
                    "{2, 3, 2, 1}.distinctOf((v, index, size) => case when index+1=size then 1 else 2 end)",
                    new object[] { 2, 1 });
                Validate(env, "{2, 3, 2, 1}.reverse()", new object[] { 1, 2, 3, 2 });
                Validate(env, "{1, 2, 3}.sequenceEqual({1})", false);
                Validate(env, "{1, 2, 3}.sequenceEqual({1, 2, 3})", true);
                Validate(env, "{1, 2, 3}.sumOf()", 6);
                Validate(env, "{1, 2, 3}.sumOf(v => v+1)", 9);
                Validate(env, "{1, 2, 3}.sumOf((v, index) => v + index)", 1 + 3 + 5);
                Validate(env, "{1, 2, 3}.sumOf((v, index, size) => v+index+size)", 18);
                Validate(env, "{1, 2, 3}.take(2)", new object[] { 1, 2 });
                Validate(env, "{1, 2, 3}.takeLast(2)", new object[] { 2, 3 });
                Validate(env, "{1, 2, 3}.takeWhile(v => v < 3)", new object[] { 1, 2 });
                Validate(env, "{1, 2, 3}.takeWhile((v,ind) => ind < 2)", new object[] { 1, 2 });
                Validate(env, "{1, 2, -1, 4, 5, 6}.takeWhile((v,ind) => ind < 5 and v > 0)", new object[] { 1, 2 });
                Validate(
                    env,
                    "{1, 2, -1, 4, 5, 6}.takeWhile((v,ind,sz) => ind < sz - 5 and v > 0)",
                    new object[] { 1 });
                Validate(env, "{1, 2, 3}.takeWhileLast(v => v > 1)", new object[] { 2, 3 });
                Validate(env, "{1, 2, 3}.takeWhileLast((v,ind) => ind < 2)", new object[] { 2, 3 });
                Validate(
                    env,
                    "{1, 2, -1, 4, 5, 6}.takeWhileLast((v,ind) => ind < 5 and v > 0)",
                    new object[] { 4, 5, 6 });
                Validate(
                    env,
                    "{1, 2, -1, 4, 5, 6}.takeWhileLast((v,ind,sz) => ind < sz-4 and v > 0)",
                    new object[] { 5, 6 });
                Validate(env, "{1, 2, 3}.union({4, 5})", new object[] { 1, 2, 3, 4, 5 });
                Validate(env, "{1, 2, 3}.where(v => v != 2)", new object[] { 1, 3 });
                Validate(env, "{1, 2, 3}.where((v, index) => v != 2 and index < 2)", new object[] { 1 });
                Validate(env, "{1, 2, 3}.where((v, index, size) => v != 2 and index < size - 2)", new object[] { 1 });
                Validate(
                    env,
                    "{1, 2, 3}.groupby(k => 'K' || Integer.toString(k))",
                    CollectionUtil.BuildMap(
                        "K1",
                        Collections.SingletonList(1),
                        "K2",
                        Collections.SingletonList(2),
                        "K3",
                        Collections.SingletonList(3)));
                Validate(
                    env,
                    "{1, 2, 3}.groupby(k => 'K' || Integer.toString(k), v => 'V' || Integer.toString(v))",
                    CollectionUtil.BuildMap(
                        "K1",
                        Collections.SingletonList("V1"),
                        "K2",
                        Collections.SingletonList("V2"),
                        "K3",
                        Collections.SingletonList("V3")));
                Validate(
                    env,
                    "{1, 2, 3}.groupby((k, i) => 'K' || Integer.toString(k) || \"_\" || Integer.toString(i), (v, i) => 'V' || Integer.toString(v) || \"_\" || Integer.toString(i))",
                    CollectionUtil.BuildMap(
                        "K1_0",
                        Collections.SingletonList("V1_0"),
                        "K2_1",
                        Collections.SingletonList("V2_1"),
                        "K3_2",
                        Collections.SingletonList("V3_2")));
                Validate(
                    env,
                    "{1, 2, 3}.groupby((k, i, s) => 'K' || Integer.toString(k) || \"_\" || Integer.toString(s), (v, i, s) => 'V' || Integer.toString(v) || \"_\" || Integer.toString(s))",
                    CollectionUtil.BuildMap(
                        "K1_3",
                        Collections.SingletonList("V1_3"),
                        "K2_3",
                        Collections.SingletonList("V2_3"),
                        "K3_3",
                        Collections.SingletonList("V3_3")));
                Validate(env, "{1, 2, 3, 2, 1}.maxby(v => v)", 3);
                Validate(env, "{1, 2, 3, 2, 1}.maxby((v, index) => case when index < 3 then -1 else 0 end)", 2);
                Validate(
                    env,
                    "{1, 2, 3, 2, 1}.maxby((v, index, size) => case when index < size - 2 then -1 else 0 end)",
                    2);
                Validate(env, "{1, 2, 3, 2, 1}.minby(v => v)", 1);
                Validate(env, "{1, 2, 3, 2, 1}.minby((v, index) => case when index < 3 then -1 else 0 end)", 1);
                Validate(
                    env,
                    "{1, 2, 3, 2, 1}.minby((v, index, size) => case when index < size - 2 then -1 else 0 end)",
                    1);
                Validate(env, "{'A','B','C'}.selectFrom(v => '<' || v || '>')", Arrays.AsList("<A>", "<B>", "<C>"));
                Validate(
                    env,
                    "{'A','B','C'}.selectFrom((v, index) => v || '_' || Integer.toString(index))",
                    Arrays.AsList("A_0", "B_1", "C_2"));
                Validate(
                    env,
                    "{'A','B','C'}.selectFrom((v, index, size) => v || '_' || Integer.toString(size))",
                    Arrays.AsList("A_3", "B_3", "C_3"));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf()",
                    result => EPAssertionUtil.AssertEqualsExactOrder((object[])result, new object[] { 1, 2, 3 }));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf(v => v+1)",
                    result => EPAssertionUtil.AssertEqualsExactOrder((object[])result, new object[] { 2, 3, 4 }));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf((v, index) => v+index)",
                    result => EPAssertionUtil.AssertEqualsExactOrder((object[])result, new object[] { 1, 3, 5 }));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf((v, index, size) => v+index+size)",
                    result => EPAssertionUtil.AssertEqualsExactOrder((object[])result, new object[] { 4, 6, 8 }));
                Validate(
                    env,
                    "{1, 2, 3}.toMap(k => 'K' || Integer.toString(k), v => 'V' || Integer.toString(v))",
                    CollectionUtil.BuildMap("K1", "V1", "K2", "V2", "K3", "V3"));
                Validate(
                    env,
                    "{1, 2, 3}.toMap((k, i) => 'K' || Integer.toString(k) || \"_\" || Integer.toString(i), (v, i) => 'V' || Integer.toString(v) || \"_\" || Integer.toString(i))",
                    CollectionUtil.BuildMap("K1_0", "V1_0", "K2_1", "V2_1", "K3_2", "V3_2"));
                Validate(
                    env,
                    "{1, 2, 3}.toMap((k, i, s) => 'K' || Integer.toString(k) || \"_\" || Integer.toString(s), (v, i, s) => 'V' || Integer.toString(v) || \"_\" || Integer.toString(s))",
                    CollectionUtil.BuildMap("K1_3", "V1_3", "K2_3", "V2_3", "K3_3", "V3_3"));
            }
        }

        private static void Validate(
            RegressionEnvironment env,
            string select,
            object expected)
        {
            if (expected is object[]) {
                ValidateWithVerifier(
                    env,
                    select,
                    result => {
                        var returned = result.UnwrapIntoArray<object>();
                        EPAssertionUtil.AssertEqualsExactOrder((object[])expected, returned);
                    });
            }
            else if (expected is ICollection<object>) {
                ValidateWithVerifier(
                    env,
                    select,
                    result => {
                        var returned = result.UnwrapIntoArray<object>();
                        EPAssertionUtil.AssertEqualsExactOrder(((ICollection<object>)expected).ToArray(), returned);
                    });
            }
            else {
                ValidateWithVerifier(env, select, result => { Assert.AreEqual(expected, result); });
            }
        }

        private static void ValidateWithVerifier(
            RegressionEnvironment env,
            string select,
            Consumer<object> verifier)
        {
            var epl = "@name('s0') select " + select + " as result from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 0));
            env.AssertEventNew(
                "s0",
                @event => {
                    var result = @event.Get("result");
                    verifier.Invoke(result);
                });

            env.UndeployAll();
        }

        private static void AssertStmt(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            env.CompileDeploy("@name('s0')" + epl, path).UndeployModuleContaining("s0");
            env.EplToModelCompileDeploy("@name('s0') " + epl, path).UndeployModuleContaining("s0");
        }

        private static Zone[] ToArrayZones(ICollection<Zone> it)
        {
            return it.ToArray();
        }

        private static Item[] ToArrayItems(ICollection<Item> it)
        {
            return it.ToArray();
        }
    }
} // end of namespace