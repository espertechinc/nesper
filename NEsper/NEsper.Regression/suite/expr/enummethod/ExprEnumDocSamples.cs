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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.lrreport;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumDocSamples
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
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

        private static void Validate(
            RegressionEnvironment env,
            string select,
            object expected)
        {
            var epl = "@Name('s0') select " + select + " as result from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 0));
            var result = env.Listener("s0").AssertOneGetNewAndReset().Get("result");

            if (expected is object[]) {
                var returned = ((ICollection<object>) result).ToArray();
                EPAssertionUtil.AssertEqualsExactOrder((object[]) expected, returned);
            }
            else {
                Assert.AreEqual(expected, result);
            }

            env.UndeployAll();
        }

        private static void AssertStmt(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            env.CompileDeploy("@Name('s0')" + epl, path).UndeployModuleContaining("s0");
            env.EplToModelCompileDeploy("@Name('s0') " + epl, path).UndeployModuleContaining("s0");
        }

        private static Zone[] ToArrayZones(ICollection<Zone> it)
        {
            return it.ToArray();
        }

        private static Item[] ToArrayItems(ICollection<Item> it)
        {
            return it.ToArray();
        }

        internal class ExprEnumHowToUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment =
                    "@Name('s0') select Items.where(i -> i.Location.X = 0 and i.Location.Y = 0) as zeroloc from LocationReport";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());

                var items = ToArrayItems(
                    (ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("zeroloc"));
                Assert.AreEqual(1, items.Length);
                Assert.AreEqual("P00020", items[0].AssetId);

                env.UndeployAll();
                eplFragment =
                    "@Name('s0') select Items.where(i -> i.Location.X = 0).where(i -> i.Location.Y = 0) as zeroloc from LocationReport";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());

                items = ToArrayItems((ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("zeroloc"));
                Assert.AreEqual(1, items.Length);
                Assert.AreEqual("P00020", items[0].AssetId);

                env.UndeployAll();
            }
        }

        internal class ExprEnumSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplFragment = "@Name('s0') select AssetId," +
                                  "  (select * from Zone#keepall).where(z -> inrect(z.Rectangle, Location)) as zones " +
                                  "from Item";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                var zones = ToArrayZones((ICollection<Zone>) env.Listener("s0").AssertOneGetNewAndReset().Get("zones"));
                Assert.AreEqual(1, zones.Length);
                Assert.AreEqual("Z1", zones[0].Name);

                // subquery with event as input
                var epl = "create schema SettlementEvent (Symbol string, Price double);" +
                          "create schema PriceEvent (Symbol string, Price double);\n" +
                          "create schema OrderEvent (OrderId string, Pricedata PriceEvent);\n" +
                          "select (select Pricedata from OrderEvent#unique(OrderId))\n" +
                          ".anyOf(v -> v.Symbol = 'GE') as has_ge from SettlementEvent(Symbol = 'GE')";
                env.CompileDeploy(epl);

                // subquery with aggregation
                env.CompileDeploy(
                    "select (select Name, count(*) as cnt from Zone#keepall group by Name).where(v -> cnt > 1) from LocationReport");

                env.UndeployAll();
            }
        }

        internal class ExprEnumNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;
                Zone[] zones;

                var path = new RegressionPath();
                env.CompileDeploy("create window ZoneWindow#keepall as Zone", path);
                env.CompileDeploy("insert into ZoneWindow select * from Zone", path);

                epl = "@Name('s0') select ZoneWindow.where(z -> inrect(z.rectangle, location)) as zones from Item";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new Zone("Z1", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Zone("Z2", new Rectangle(21, 21, 40, 40)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                zones = ToArrayZones((ICollection<Zone>) env.Listener("s0").AssertOneGetNewAndReset().Get("zones"));
                Assert.AreEqual(1, zones.Length);
                Assert.AreEqual("Z1", zones[0].Name);

                env.UndeployModuleContaining("s0");

                epl =
                    "@Name('s0') select ZoneWindow(name in ('Z4', 'Z5', 'Z3')).where(z -> inrect(z.rectangle, location)) as zones from Item";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new Zone("Z3", new Rectangle(0, 0, 20, 20)));
                env.SendEventBean(new Item("A1", new Location(10, 10)));

                zones = ToArrayZones((ICollection<Zone>) env.Listener("s0").AssertOneGetNewAndReset().Get("zones"));
                Assert.AreEqual(1, zones.Length);
                Assert.AreEqual("Z3", zones[0].Name);

                env.UndeployAll();
            }
        }

        internal class ExprEnumAccessAggWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select window(*).where(p -> distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) group by AssetId";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("P0001", new Location(10, 10), "P", null));
                var items = ToArrayItems(
                    (ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("centeritems"));
                Assert.AreEqual(1, items.Length);
                Assert.AreEqual("P0001", items[0].AssetId);

                env.SendEventBean(new Item("P0002", new Location(10, 1000), "P", null));
                items = ToArrayItems(
                    (ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("centeritems"));
                Assert.AreEqual(0, items.Length);

                env.UndeployAll();
            }
        }

        internal class ExprEnumPrevWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select prevwindow(items).where(p -> distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from Item(type='P')#time(10) as items";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("P0001", new Location(10, 10), "P", null));
                var items = ToArrayItems(
                    (ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("centeritems"));
                Assert.AreEqual(1, items.Length);
                Assert.AreEqual("P0001", items[0].AssetId);

                env.SendEventBean(new Item("P0002", new Location(10, 1000), "P", null));
                items = ToArrayItems(
                    (ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("centeritems"));
                Assert.AreEqual(1, items.Length);
                Assert.AreEqual("P0001", items[0].AssetId);

                env.UndeployAll();
            }
        }

        internal class ExprEnumProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select Items.where(p -> distance(0, 0, p.Location.X, p.Location.Y) < 20) as centeritems " +
                    "from LocationReport";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());
                var items = ToArrayItems(
                    (ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("centeritems"));
                Assert.AreEqual(1, items.Length);
                Assert.AreEqual("P00020", items[0].AssetId);

                env.UndeployAll();
            }
        }

        internal class ExprEnumUDFSingleRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select ZoneFactory.getZones().where(z -> inrect(z.rectangle, item.Location)) as zones\n" +
                    "from Item as item";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new Item("A1", new Location(5, 5)));
                var zones = ToArrayZones((ICollection<Zone>) env.Listener("s0").AssertOneGetNewAndReset().Get("zones"));
                Assert.AreEqual(1, zones.Length);
                Assert.AreEqual("Z1", zones[0].Name);

                env.UndeployAll();
            }
        }

        internal class ExprEnumDeclared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') expression passengers {\n" +
                          "  lr -> lr.Items.where(l -> l.Type='P')\n" +
                          "}\n" +
                          "select passengers(lr) as p," +
                          "passengers(lr).where(x -> AssetId = 'P01') as p2 from LocationReport lr";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(LocationReportFactory.MakeSmall());
                var items = ToArrayItems((ICollection<Item>) env.Listener("s0").AssertOneGetNewAndReset().Get("p"));
                Assert.AreEqual(2, items.Length);
                Assert.AreEqual("P00002", items[0].AssetId);
                Assert.AreEqual("P00020", items[1].AssetId);

                env.UndeployAll();
            }
        }

        internal class ExprEnumExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                AssertStmt(env, path, "select Items.firstof().AssetId as firstcenter from LocationReport");
                AssertStmt(env, path, "select Items.where(p -> p.Type=\"P\") from LocationReport");
                AssertStmt(env, path, "select Items.where((p,ind) -> p.Type=\"P\" and ind>2) from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.aggregate(\"\",(result,item) -> result||(case when result=\"\" then \"\" else \",\" end)||item.AssetId) as assets from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.allof(i -> distance(i.Location.X,i.Location.Y,0,0)<1000) as assets from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.average(i -> distance(i.Location.X,i.Location.Y,0,0)) as avgdistance from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.countof(i -> distance(i.Location.X,i.Location.Y,0,0)<20) as cntcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.firstof(i -> distance(i.Location.X,i.Location.Y,0,0)<20) as firstcenter from LocationReport");
                AssertStmt(env, path, "select Items.lastof().AssetId as firstcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.lastof(i -> distance(i.Location.X,i.Location.Y,0,0)<20) as lastcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.where(i -> i.Type=\"L\").groupby(i -> AssetIdPassenger) as luggagePerPerson from LocationReport");
                AssertStmt(env, path, "select Items.where((p,ind) -> p.Type=\"P\" and ind>2) from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.groupby(k -> AssetId,v -> distance(v.Location.X,v.Location.Y,0,0)) as distancePerItem from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.min(i -> distance(i.Location.X,i.Location.Y,0,0)) as mincenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.max(i -> distance(i.Location.X,i.Location.Y,0,0)) as maxcenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.minBy(i -> distance(i.Location.X,i.Location.Y,0,0)) as minItemCenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.minBy(i -> distance(i.Location.X,i.Location.Y,0,0)).AssetId as minItemCenter from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.orderBy(i -> distance(i.Location.X,i.Location.Y,0,0)) as itemsOrderedByDist from LocationReport");
                AssertStmt(env, path, "select Items.selectFrom(i -> AssetId) as itemAssetIds from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.take(5) as first5Items, Items.takeLast(5) as last5Items from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.toMap(k -> k.AssetId,v -> distance(v.Location.X,v.Location.Y,0,0)) as assetDistance from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select Items.where(i -> i.AssetId=\"L001\").union(Items.where(i -> i.Type=\"P\")) as itemsUnion from LocationReport");
                AssertStmt(
                    env,
                    path,
                    "select (select name from Zone#unique(name)).orderBy() as orderedZones from pattern [every timer:interval(30)]");

                env.CompileDeployWBusPublicType("create schema MyEvent as (seqone String[], seqtwo String[])", path);

                AssertStmt(env, path, "select seqone.sequenceEqual(seqtwo) from MyEvent");
                AssertStmt(
                    env,
                    path,
                    "select window(AssetId).orderBy() as orderedAssetIds from Item#time(10) group by AssetId");
                AssertStmt(
                    env,
                    path,
                    "select prevwindow(AssetId).orderBy() as orderedAssetIds from Item#time(10) as items");
                AssertStmt(
                    env,
                    path,
                    "select getZoneNames().where(z -> z!=\"Z1\") from pattern [every timer:interval(30)]");
                AssertStmt(
                    env,
                    path,
                    "select Items.selectFrom(i -> new{AssetId,distanceCenter=distance(i.Location.X,i.Location.Y,0,0)}) as itemInfo from LocationReport");
                AssertStmt(env, path, "select Items.leastFrequent(i -> type) as leastFreqType from LocationReport");

                var epl = "expression myquery {itm -> " +
                          "(select * from Zone#keepall).where(z -> inrect(z.rectangle,itm.Location))" +
                          "} " +
                          "select AssetId, myquery(item) as subq, myquery(item).where(z -> z.name=\"Z01\") as assetItem " +
                          "from Item as item";
                AssertStmt(env, path, epl);

                AssertStmt(
                    env,
                    path,
                    "select za.Items.except(zb.items) as itemsCompared from LocationReport as za unidirectional, LocationReport#length(10) as zb");

                env.UndeployAll();
            }
        }

        internal class ExprEnumScalarArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validate(env, "{1, 2, 3}.aggregate(0, (result, value) -> result + value)", 6);
                Validate(env, "{1, 2, 3}.allOf(v -> v > 0)", true);
                Validate(env, "{1, 2, 3}.allOf(v -> v > 1)", false);
                Validate(env, "{1, 2, 3}.anyOf(v -> v > 1)", true);
                Validate(env, "{1, 2, 3}.anyOf(v -> v > 3)", false);
                Validate(env, "{1, 2, 3}.average()", 2.0);
                Validate(env, "{1, 2, 3}.countOf()", 3);
                Validate(env, "{1, 2, 3}.countOf(v -> v < 2)", 1);
                Validate(
                    env,
                    "{1, 2, 3}.except({1})",
                    new object[] {2, 3});
                Validate(
                    env,
                    "{1, 2, 3}.intersect({2,3})",
                    new object[] {2, 3});
                Validate(env, "{1, 2, 3}.firstOf()", 1);
                Validate(env, "{1, 2, 3}.firstOf(v -> v / 2 = 1)", 2);
                Validate(
                    env,
                    "{1, 2, 3}.intersect({2, 3})",
                    new object[] {2, 3});
                Validate(env, "{1, 2, 3}.lastOf()", 3);
                Validate(env, "{1, 2, 3}.lastOf(v -> v < 3)", 2);
                Validate(env, "{1, 2, 3, 2, 1}.leastFrequent()", 3);
                Validate(env, "{1, 2, 3, 2, 1}.max()", 3);
                Validate(env, "{1, 2, 3, 2, 1}.min()", 1);
                Validate(env, "{1, 2, 3, 2, 1, 2}.mostFrequent()", 2);
                Validate(
                    env,
                    "{2, 3, 2, 1}.orderBy()",
                    new object[] {1, 2, 2, 3});
                Validate(
                    env,
                    "{2, 3, 2, 1}.distinctOf()",
                    new object[] {2, 3, 1});
                Validate(
                    env,
                    "{2, 3, 2, 1}.reverse()",
                    new object[] {1, 2, 3, 2});
                Validate(env, "{1, 2, 3}.sequenceEqual({1})", false);
                Validate(env, "{1, 2, 3}.sequenceEqual({1, 2, 3})", true);
                Validate(env, "{1, 2, 3}.sumOf()", 6);
                Validate(
                    env,
                    "{1, 2, 3}.take(2)",
                    new object[] {1, 2});
                Validate(
                    env,
                    "{1, 2, 3}.takeLast(2)",
                    new object[] {2, 3});
                Validate(
                    env,
                    "{1, 2, 3}.takeWhile(v -> v < 3)",
                    new object[] {1, 2});
                Validate(
                    env,
                    "{1, 2, 3}.takeWhile((v,ind) -> ind < 2)",
                    new object[] {1, 2});
                Validate(
                    env,
                    "{1, 2, -1, 4, 5, 6}.takeWhile((v,ind) -> ind < 5 and v > 0)",
                    new object[] {1, 2});
                Validate(
                    env,
                    "{1, 2, 3}.takeWhileLast(v -> v > 1)",
                    new object[] {2, 3});
                Validate(
                    env,
                    "{1, 2, 3}.takeWhileLast((v,ind) -> ind < 2)",
                    new object[] {2, 3});
                Validate(
                    env,
                    "{1, 2, -1, 4, 5, 6}.takeWhileLast((v,ind) -> ind < 5 and v > 0)",
                    new object[] {4, 5, 6});
                Validate(
                    env,
                    "{1, 2, 3}.union({4, 5})",
                    new object[] {1, 2, 3, 4, 5});
                Validate(
                    env,
                    "{1, 2, 3}.where(v -> v != 2)",
                    new object[] {1, 3});
            }
        }
    }
} // end of namespace