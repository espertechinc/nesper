///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumDocSamples
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithExpressions(execs);
            WithHowToUse(execs);
            WithSubquery(execs);
            WithNamedWindow(execs);
            WithAccessAggWindow(execs);
            WithPrevWindow(execs);
            WithProperties(execs);
            WithUDFSingleRow(execs);
            WithScalarArray(execs);
            WithDeclared(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDeclared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumDeclared());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumScalarArray());
            return execs;
        }

        public static IList<RegressionExecution> WithUDFSingleRow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumUDFSingleRow());
            return execs;
        }

        public static IList<RegressionExecution> WithProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithPrevWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumPrevWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAccessAggWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithHowToUse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumHowToUse());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumExpressions());
            return execs;
        }

        private class ExprEnumHowToUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@name('s0') select Items.where(i => i.Location.X = 0 and i.Location.Y = 0) as zeroloc from LocationReport";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("zeroloc"));
                        ClassicAssert.AreEqual(1, items.Length);
                        ClassicAssert.AreEqual("P00020", items[0].AssetId);
                    });

                env.UndeployAll();
                eplFragment =
                    "@name('s0') select Items.where(i => i.Location.X = 0).where(i => i.Location.Y = 0) as zeroloc from LocationReport";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("zeroloc"));
                        ClassicAssert.AreEqual(1, items.Length);
                        ClassicAssert.AreEqual("P00020", items[0].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment = "@name('s0') select AssetId," +
                                  "  (select * from Zone#keepall).where(z => inrect(z.Rectangle, Location)) as zones " +
                                  "from Item";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        ClassicAssert.AreEqual(1, zones.Length);
                        ClassicAssert.AreEqual("Z1", zones[0].Name);
                    });

                // subquery with event as input
                var epl = "create schema SettlementEvent (Symbol string, Price double);" +
                          "create schema PriceEvent (Symbol string, Price double);\n" +
                          "create schema OrderEvent (OrderId string, pricedata PriceEvent);\n" +
                          "select (select pricedata from OrderEvent#unique(OrderId))\n" +
                          ".anyOf(v => v.Symbol = 'GE') as has_ge from SettlementEvent(Symbol = 'GE')";
                env.CompileDeploy(epl);

                // subquery with aggregation
                env.CompileDeploy(
                    "select (select Name, count(*) as cnt from Zone#keepall group by Name).where(v => cnt > 1) from LocationReport");

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

                epl = "@name('s0') select ZoneWindow.where(z => inrect(z.Rectangle, Location)) as zones from Item";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        ClassicAssert.AreEqual(1, zones.Length);
                        ClassicAssert.AreEqual("Z1", zones[0].Name);
                    });

                env.UndeployModuleContaining("s0");

                epl =
                    "@name('s0') select ZoneWindow(Name in ('Z4', 'Z5', 'Z3')).where(z => inrect(z.Rectangle, Location)) as zones from Item";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new Zone("Z3", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        ClassicAssert.AreEqual(1, zones.Length);
                        ClassicAssert.AreEqual("Z3", zones[0].Name);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumAccessAggWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select window(*).where(p => distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from Item(Type='P')#time(10) group by AssetId";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("P0001", new Location(10, 10), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        ClassicAssert.AreEqual(1, items.Length);
                        ClassicAssert.AreEqual("P0001", items[0].AssetId);
                    });

                env.SendEventBean(new Item("P0002", new Location(10, 1000), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        ClassicAssert.AreEqual(0, items.Length);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumPrevWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select prevwindow(Items).where(p => distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from Item(Type='P')#time(10) as Items";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("P0001", new Location(10, 10), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        ClassicAssert.AreEqual(1, items.Length);
                        ClassicAssert.AreEqual("P0001", items[0].AssetId);
                    });

                env.SendEventBean(new Item("P0002", new Location(10, 1000), "P", null));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        ClassicAssert.AreEqual(1, items.Length);
                        ClassicAssert.AreEqual("P0001", items[0].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Items.where(p => distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from LocationReport";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("centeritems"));
                        ClassicAssert.AreEqual(1, items.Length);
                        ClassicAssert.AreEqual("P00020", items[0].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumUDFSingleRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select ZoneFactory.GetZones().where(z => inrect(z.Rectangle, Item.Location)) as zones\n" +
                    "from Item as Item";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("A1", new Location(5, 5)));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var zones = ToArrayZones((ICollection<Zone>)@event.Get("zones"));
                        ClassicAssert.AreEqual(1, zones.Length);
                        ClassicAssert.AreEqual("Z1", zones[0].Name);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') expression passengers {\n" +
                          "  lr => lr.Items.where(l => l.Type='P')\n" +
                          "}\n" +
                          "select passengers(lr) as p," +
                          "passengers(lr).where(x => AssetId = 'P01') as p2 from LocationReport lr";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var items = ToArrayItems((ICollection<Item>)@event.Get("p"));
                        ClassicAssert.AreEqual(2, items.Length);
                        ClassicAssert.AreEqual("P00002", items[0].AssetId);
                        ClassicAssert.AreEqual("P00020", items[1].AssetId);
                    });

                env.UndeployAll();
            }
        }

        private class ExprEnumExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                AssertStmt(env, path, "select Items.firstof().AssetId as firstcenter from LocationReport");
                AssertStmt(env, path, "select Items.where(p => p.Type=\"P\") from LocationReport");
                AssertStmt(env, path, "select Items.where((p,ind) => p.Type=\"P\" and ind>2) from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.aggregate(\"\",(result,Item) => result||(case when result=\"\" then \"\" else \",\" end)||Item.AssetId) as assets from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.allof(i => distance(i.Location.X,i.Location.Y,0,0)<1000) as assets from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.Average(i => distance(i.Location.X,i.Location.Y,0,0)) as avgdistance from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.countof(i => distance(i.Location.X,i.Location.Y,0,0)<20) as cntcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.firstof(i => distance(i.Location.X,i.Location.Y,0,0)<20) as firstcenter from LocationReport");
                AssertStmt(env, path, "select Items.lastof().AssetId as firstcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.lastof(i => distance(i.Location.X,i.Location.Y,0,0)<20) as lastcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.where(i => i.Type=\"L\").groupby(i => AssetIdPassenger) as luggagePerPerson from LocationReport");
                AssertStmt(env, path, "select Items.where((p,ind) => p.Type=\"P\" and ind>2) from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.groupby(k => AssetId,v => distance(v.Location.X,v.Location.Y,0,0)) as distancePerItem from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.min(i => distance(i.Location.X,i.Location.Y,0,0)) as mincenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.max(i => distance(i.Location.X,i.Location.Y,0,0)) as maxcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.minBy(i => distance(i.Location.X,i.Location.Y,0,0)) as minItemCenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.minBy(i => distance(i.Location.X,i.Location.Y,0,0)).AssetId as minItemCenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.orderBy(i => distance(i.Location.X,i.Location.Y,0,0)) as itemsOrderedByDist from LocationReport");
                AssertStmt(env, path, "select Items.selectFrom(i => AssetId) as itemAssetIds from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.take(5) as first5Items, Items.takeLast(5) as last5Items from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.toMap(k => k.AssetId,v => distance(v.Location.X,v.Location.Y,0,0)) as assetDistance from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.where(i => i.AssetId=\"L001\").union(Items.where(i => i.Type=\"P\")) as itemsUnion from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select (select Name from Zone#unique(Name)).orderBy() as orderedZones from pattern [every timer:interval(30)]");

                env.CompileDeploy(
                    "@buseventtype @public create schema MyEvent as (seqone String[], seqtwo String[])",
                    path);

                AssertStmt(env, path, "select seqone.sequenceEqual(seqtwo) from MyEvent");
                AssertStmt(
                    env,
                    path,
                    "select window(AssetId).orderBy() as orderedAssetIds from Item#time(10) group by AssetId");
                AssertStmt(
                    env,
                    path,
                    "select prevwindow(AssetId).orderBy() as orderedAssetIds from Item#time(10) as Items");
                AssertStmt(
                    env,
                    path,
                    "select getZoneNames().where(z => z!=\"Z1\") from pattern [every timer:interval(30)]");
                AssertStmt(
                    env,
                    path,
                    "select Items.selectFrom(i => new{AssetId,distanceCenter=distance(i.Location.X,i.Location.Y,0,0)}) as itemInfo from LocationReport");
                AssertStmt(env, path, "select Items.leastFrequent(i => Type) as leastFreqType from LocationReport");

                var epl = "expression myquery {itm => " +
                          "(select * from Zone#keepall).where(z => inrect(z.Rectangle,itm.Location))" +
                          "} " +
                          "select AssetId, myquery(Item) as subq, myquery(Item).where(z => z.Name=\"Z01\") as assetItem " +
                          "from Item as Item";
                AssertStmt(env, path, epl);

                AssertStmt(
                    env,
                    path,
                    "select za.Items.except(zb.Items) as itemsCompared from LocationReport as za unidirectional, LocationReport#length(10) as zb");

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
                Validate(env, "{1, 2, 3}.Average()", 2.0);
                Validate(env, "{1, 2, 3}.Average(v => v+1)", 3d);
                Validate(env, "{1, 2, 3}.Average((v, index) => v+10*index)", 12d);
                Validate(env, "{1, 2, 3}.Average((v, index, size) => v+10*index + 100*size)", 312d);
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
                    "{1, 2, 3}.groupby(k => 'K' || Convert.ToString(k))",
                    CollectionUtil.BuildMap(
                        "K1",
                        Collections.SingletonList(1),
                        "K2",
                        Collections.SingletonList(2),
                        "K3",
                        Collections.SingletonList(3)));
                Validate(
                    env,
                    "{1, 2, 3}.groupby(k => 'K' || Convert.ToString(k), v => 'V' || Convert.ToString(v))",
                    CollectionUtil.BuildMap(
                        "K1",
                        Collections.SingletonList("V1"),
                        "K2",
                        Collections.SingletonList("V2"),
                        "K3",
                        Collections.SingletonList("V3")));
                Validate(
                    env,
                    "{1, 2, 3}.groupby((k, i) => 'K' || Convert.ToString(k) || \"_\" || Convert.ToString(i), (v, i) => 'V' || Convert.ToString(v) || \"_\" || Convert.ToString(i))",
                    CollectionUtil.BuildMap(
                        "K1_0",
                        Collections.SingletonList("V1_0"),
                        "K2_1",
                        Collections.SingletonList("V2_1"),
                        "K3_2",
                        Collections.SingletonList("V3_2")));
                Validate(
                    env,
                    "{1, 2, 3}.groupby((k, i, s) => 'K' || Convert.ToString(k) || \"_\" || Convert.ToString(s), (v, i, s) => 'V' || Convert.ToString(v) || \"_\" || Convert.ToString(s))",
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
                    "{'A','B','C'}.selectFrom((v, index) => v || '_' || Convert.ToString(index))",
                    Arrays.AsList("A_0", "B_1", "C_2"));
                Validate(
                    env,
                    "{'A','B','C'}.selectFrom((v, index, size) => v || '_' || Convert.ToString(size))",
                    Arrays.AsList("A_3", "B_3", "C_3"));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf()",
                    result => EPAssertionUtil.AssertEqualsExactOrder(result.Unwrap<object>(), new object[] { 1, 2, 3 }));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf(v => v+1)",
                    result => EPAssertionUtil.AssertEqualsExactOrder(result.Unwrap<object>(), new object[] { 2, 3, 4 }));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf((v, index) => v+index)",
                    result => EPAssertionUtil.AssertEqualsExactOrder(result.Unwrap<object>(), new object[] { 1, 3, 5 }));
                ValidateWithVerifier(
                    env,
                    "{1, 2, 3}.arrayOf((v, index, size) => v+index+size)",
                    result => EPAssertionUtil.AssertEqualsExactOrder(result.Unwrap<object>(), new object[] { 4, 6, 8 }));
                Validate(
                    env,
                    "{1, 2, 3}.toMap(k => 'K' || Convert.ToString(k), v => 'V' || Convert.ToString(v))",
                    CollectionUtil.BuildMap("K1", "V1", "K2", "V2", "K3", "V3"));
                Validate(
                    env,
                    "{1, 2, 3}.toMap((k, i) => 'K' || Convert.ToString(k) || \"_\" || Convert.ToString(i), (v, i) => 'V' || Convert.ToString(v) || \"_\" || Convert.ToString(i))",
                    CollectionUtil.BuildMap("K1_0", "V1_0", "K2_1", "V2_1", "K3_2", "V3_2"));
                Validate(
                    env,
                    "{1, 2, 3}.toMap((k, i, s) => 'K' || Convert.ToString(k) || \"_\" || Convert.ToString(s), (v, i, s) => 'V' || Convert.ToString(v) || \"_\" || Convert.ToString(s))",
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
                ValidateWithVerifier(env, select, result => { ClassicAssert.AreEqual(expected, result); });
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