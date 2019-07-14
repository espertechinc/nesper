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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableOnMerge
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraTableOnMergeSimple());
            execs.Add(new InfraOnMergePlainPropsAnyKeyed());
            execs.Add(new InfraMergeWhereWithMethodRead());
            execs.Add(new InfraMergeSelectWithAggReadAndEnum());
            return execs;
        }

        private static void RunOnMergeInsertUpdDeleteUngrouped(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var eplDeclare = "create table varaggIUD (p0 string, sumint sum(int))";
            env.CompileDeploy(soda, eplDeclare, path);

            var fields = "c0,c1".SplitCsv();
            var eplRead =
                "@Name('s0') select varaggIUD.p0 as c0, varaggIUD.sumint as c1, varaggIUD as c2 from SupportBean_S0";
            env.CompileDeploy(soda, eplRead, path).AddListener("s0");

            // assert selected column types
            object[][] expectedAggType = {new object[] {"c0", typeof(string)}, new object[] {"c1", typeof(int?)}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            // assert no row
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null});

            // create merge
            var eplMerge = "on SupportBean merge varaggIUD" +
                           " when not matched then" +
                           " insert select TheString as p0" +
                           " when matched and theString like \"U%\" then" +
                           " update set p0=\"updated\"" +
                           " when matched and theString like \"D%\" then" +
                           " delete";
            env.CompileDeploy(soda, eplMerge, path);

            // merge for varagg
            env.SendEventBean(new SupportBean("E1", 0));

            // assert
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", null});

            // also aggregate-into the same key
            env.CompileDeploy(soda, "into table varaggIUD select sum(50) as sumint from SupportBean_S1", path);
            env.SendEventBean(new SupportBean_S1(0));

            env.Milestone(0);

            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 50});

            // update for varagg
            env.SendEventBean(new SupportBean("U2", 10));

            env.Milestone(1);

            env.SendEventBean(new SupportBean_S0(0));
            var received = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                received,
                fields,
                new object[] {"updated", 50});
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) received.Get("c2"),
                "p0,sumint".SplitCsv(),
                "updated",
                50);

            // delete for varagg
            env.SendEventBean(new SupportBean("D3", 0));

            env.Milestone(2);

            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null});

            env.UndeployAll();
        }

        private static void RunOnMergeInsertUpdDeleteSingleKey(
            RegressionEnvironment env,
            bool soda)
        {
            var fieldsTable = "key,p0,p1,p2,sumint".SplitCsv();
            var path = new RegressionPath();
            var eplDeclare =
                "create table varaggMIU (key int primary key, p0 string, p1 int, p2 int[], sumint sum(int))";
            env.CompileDeploy(soda, eplDeclare, path);

            var fields = "c0,c1,c2,c3".SplitCsv();
            var eplRead =
                "@Name('s0') select varaggMIU[id].p0 as c0, varaggMIU[id].p1 as c1, varaggMIU[id].p2 as c2, varaggMIU[id].sumint as c3 from SupportBean_S0";
            env.CompileDeploy(soda, eplRead, path).AddListener("s0");

            // assert selected column types
            object[][] expectedAggType = {
                new object[] {"c0", typeof(string)}, new object[] {"c1", typeof(int?)},
                new object[] {"c2", typeof(int?[])}, new object[] {"c3", typeof(int?)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            // assert no row
            env.SendEventBean(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null, null});

            // create merge
            var eplMerge = "@Name('merge') on SupportBean merge varaggMIU" +
                           " where IntPrimitive=key" +
                           " when not matched then" +
                           " insert select IntPrimitive as key, \"v1\" as p0, 1000 as p1, {1,2} as p2" +
                           " when matched and theString like \"U%\" then" +
                           " update set p0=\"v2\", p1=2000, p2={3,4}" +
                           " when matched and theString like \"D%\" then" +
                           " delete";
            env.CompileDeploy(soda, eplMerge, path).AddListener("merge");

            // merge for varagg[10]
            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("merge").AssertOneGetNewAndReset(),
                fieldsTable,
                new object[] {10, "v1", 1000, new[] {1, 2}, null});

            // assert key "10"
            env.SendEventBean(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"v1", 1000, new int?[] {1, 2}, null});

            // also aggregate-into the same key
            env.CompileDeploy(
                soda,
                "into table varaggMIU select sum(50) as sumint from SupportBean_S1 group by id",
                path);
            env.SendEventBean(new SupportBean_S1(10));

            env.Milestone(0);

            env.SendEventBean(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"v1", 1000, new int?[] {1, 2}, 50});

            // update for varagg[10]
            env.SendEventBean(new SupportBean("U2", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("merge").LastNewData[0],
                fieldsTable,
                new object[] {10, "v2", 2000, new[] {3, 4}, 50});
            EPAssertionUtil.AssertProps(
                env.Listener("merge").GetAndResetLastOldData()[0],
                fieldsTable,
                new object[] {10, "v1", 1000, new[] {1, 2}, 50});

            env.Milestone(1);

            env.SendEventBean(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"v2", 2000, new int?[] {3, 4}, 50});

            // delete for varagg[10]
            env.SendEventBean(new SupportBean("D3", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("merge").AssertOneGetOldAndReset(),
                fieldsTable,
                new object[] {10, "v2", 2000, new[] {3, 4}, 50});

            env.Milestone(2);

            env.SendEventBean(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null, null});

            env.UndeployAll();
        }

        private static void RunOnMergeInsertUpdDeleteTwoKey(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var eplDeclare = "create table varaggMIUD (keyOne int primary key, keyTwo string primary key, prop string)";
            env.CompileDeploy(soda, eplDeclare, path);

            var fields = "c0,c1,c2".SplitCsv();
            var eplRead =
                "@Name('s0') select varaggMIUD[id,p00].keyOne as c0, varaggMIUD[id,p00].keyTwo as c1, varaggMIUD[id,p00].prop as c2 from SupportBean_S0";
            env.CompileDeploy(soda, eplRead, path).AddListener("s0");

            // assert selected column types
            object[][] expectedAggType = {
                new object[] {"c0", typeof(int?)}, new object[] {"c1", typeof(string)},
                new object[] {"c2", typeof(string)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            // assert no row
            env.SendEventBean(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null});

            // create merge
            var eplMerge = "@Name('merge') on SupportBean merge varaggMIUD" +
                           " where IntPrimitive=keyOne and theString=keyTwo" +
                           " when not matched then" +
                           " insert select IntPrimitive as keyOne, TheString as keyTwo, \"inserted\" as prop" +
                           " when matched and longPrimitive>0 then" +
                           " update set prop=\"updated\"" +
                           " when matched and longPrimitive<0 then" +
                           " delete";
            env.CompileDeploy(soda, eplMerge, path);
            object[][] expectedType = {
                new object[] {"keyOne", typeof(int?)}, new object[] {"keyTwo", typeof(string)},
                new object[] {"prop", typeof(string)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                env.Statement("merge").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            // merge for varagg[10, "A"]
            env.SendEventBean(new SupportBean("A", 10));

            env.Milestone(0);

            // assert key {"10", "A"}
            env.SendEventBean(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, "A", "inserted"});

            // update for varagg[10, "A"]
            env.SendEventBean(MakeSupportBean("A", 10, 1));

            env.Milestone(1);

            env.SendEventBean(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, "A", "updated"});

            // test typable output
            env.CompileDeploy(
                    "@Name('convert') insert into LocalBean select varaggMIUD[10, 'A'] as val0 from SupportBean_S1",
                    path)
                .AddListener("convert");
            env.SendEventBean(new SupportBean_S1(2));
            EPAssertionUtil.AssertProps(
                env.Listener("convert").AssertOneGetNewAndReset(),
                "val0.keyOne".SplitCsv(),
                new object[] {10});

            // delete for varagg[10, "A"]
            env.SendEventBean(MakeSupportBean("A", 10, -1));

            env.Milestone(2);

            env.SendEventBean(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null});

            env.UndeployAll();
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        private static void AssertResultAggRead(
            RegressionEnvironment env,
            object[] objects,
            int total)
        {
            var fields = "eventset,total".SplitCsv();
            env.SendEventBean(new SupportBean_S0(0));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {objects, total});
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {objects[objects.Length - 1]},
                ((ICollection<object>) @event.Get("c0")).ToArray());
        }

        private static void AssertKeyFound(
            RegressionEnvironment env,
            string keyCsv,
            bool[] expected)
        {
            var split = keyCsv.SplitCsv();
            for (var i = 0; i < split.Length; i++) {
                var key = split[i];
                env.SendEventBean(new SupportBean_S0(0, key));
                var expectedString = expected[i] ? key : null;
                Assert.AreEqual(
                    expectedString,
                    env.Listener("s0").AssertOneGetNewAndReset().Get("c0"),
                    "failed for key '" + key + "'"
                );
            }
        }

        internal class InfraTableOnMergeSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "k1,v1".SplitCsv();

                env.CompileDeploy("@Name('tbl') create table varaggKV (k1 string primary key, v1 int)", path);
                env.CompileDeploy(
                    "on SupportBean as sb merge varaggKV as va where sb.TheString = va.k1 " +
                    "when not matched then insert select TheString as k1, IntPrimitive as v1 " +
                    "when matched then update set v1 = IntPrimitive",
                    path);

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("tbl"),
                    fields,
                    new[] {new object[] {"E1", 10}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("tbl"),
                    fields,
                    new[] {new object[] {"E1", 11}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 100));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("tbl"),
                    fields,
                    new[] {new object[] {"E1", 11}, new object[] {"E2", 100}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 101));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("tbl"),
                    fields,
                    new[] {new object[] {"E1", 11}, new object[] {"E2", 101}});

                env.UndeployAll();
            }
        }

        internal class InfraMergeWhereWithMethodRead : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table varaggMMR (keyOne string primary key, cnt count(*))", path);
                env.CompileDeploy(
                    "into table varaggMMR select count(*) as cnt " +
                    "from SupportBean#lastevent group by TheString",
                    path);

                env.CompileDeploy("@Name('s0') select varaggMMR[p00].keyOne as c0 from SupportBean_S0", path)
                    .AddListener("s0");
                env.CompileDeploy("on SupportBean_S1 merge varaggMMR where cnt = 0 when matched then delete", path);

                env.SendEventBean(new SupportBean("G1", 0));
                env.SendEventBean(new SupportBean("G2", 0));
                AssertKeyFound(env, "G1,G2,G3", new[] {true, true, false});

                env.SendEventBean(new SupportBean_S1(0)); // delete
                AssertKeyFound(env, "G1,G2,G3", new[] {false, true, false});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G3", 0));
                AssertKeyFound(env, "G1,G2,G3", new[] {false, true, true});

                env.SendEventBean(new SupportBean_S1(0)); // delete
                AssertKeyFound(env, "G1,G2,G3", new[] {false, false, true});

                env.UndeployAll();
            }
        }

        internal class InfraMergeSelectWithAggReadAndEnum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table varaggMS (eventset window(*) @type(SupportBean), total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table varaggMS select window(*) as eventset, " +
                    "sum(IntPrimitive) as total from SupportBean#length(2)",
                    path);
                env.CompileDeploy(
                    "on SupportBean_S0 merge varaggMS " +
                    "when matched then insert into ResultStream select eventset, total, eventset.takeLast(1) as c0",
                    path);
                env.CompileDeploy("@Name('s0') select * from ResultStream", path).AddListener("s0");

                var e1 = new SupportBean("E1", 15);
                env.SendEventBean(e1);

                AssertResultAggRead(
                    env,
                    new object[] {e1},
                    15);

                env.Milestone(0);

                var e2 = new SupportBean("E2", 20);
                env.SendEventBean(e2);

                AssertResultAggRead(
                    env,
                    new object[] {e1, e2},
                    35);

                env.Milestone(1);

                var e3 = new SupportBean("E3", 30);
                env.SendEventBean(e3);

                AssertResultAggRead(
                    env,
                    new object[] {e2, e3},
                    50);

                env.UndeployAll();
            }
        }

        internal class InfraOnMergePlainPropsAnyKeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunOnMergeInsertUpdDeleteSingleKey(env, false);
                RunOnMergeInsertUpdDeleteSingleKey(env, true);

                RunOnMergeInsertUpdDeleteTwoKey(env, false);
                RunOnMergeInsertUpdDeleteTwoKey(env, true);

                RunOnMergeInsertUpdDeleteUngrouped(env, false);
                RunOnMergeInsertUpdDeleteUngrouped(env, true);
            }
        }

        public class LocalSubBean
        {
            public int KeyOne { get; private set; }

            public string KeyTwo { get; private set; }

            public string Prop { get; private set; }

            public void SetKeyOne(int keyOne)
            {
                KeyOne = keyOne;
            }

            public void SetKeyTwo(string keyTwo)
            {
                KeyTwo = keyTwo;
            }

            public void SetProp(string prop)
            {
                Prop = prop;
            }
        }

        public class LocalBean
        {
            public LocalSubBean Val0 { get; private set; }

            public void SetVal0(LocalSubBean val0)
            {
                Val0 = val0;
            }
        }
    }
} // end of namespace