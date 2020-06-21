///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableAccessDotMethod
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPlainPropDatetimeAndEnumerationAndMethod(execs);
            WithAggDatetimeAndEnumerationAndMethod(execs);
            WithNestedDotMethod(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNestedDotMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNestedDotMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithAggDatetimeAndEnumerationAndMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraAggDatetimeAndEnumerationAndMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithPlainPropDatetimeAndEnumerationAndMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPlainPropDatetimeAndEnumerationAndMethod());
            return execs;
        }

        private static void TryAssertionNestedDotMethod(
            RegressionEnvironment env,
            bool grouped,
            bool soda,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare = "create table varaggNDM (" +
                             (grouped ? "key string primary key, " : "") +
                             "windowSupportBean window(*) @type('SupportBean'))";
            env.CompileDeploy(soda, eplDeclare, path);

            var eplInto = "into table varaggNDM " +
                          "select window(*) as windowSupportBean from SupportBean#length(2)" +
                          (grouped ? " group by TheString" : "");
            env.CompileDeploy(soda, eplInto, path);

            var key = grouped ? "[\"E1\"]" : "";
            var eplSelect = string.Format(
                "@Name('s0') select " +
                "varaggNDM{0}.windowSupportBean.last(*).IntPrimitive as c0, " +
                "varaggNDM{0}.windowSupportBean.window(*).countOf() as c1, " +
                "varaggNDM{0}.windowSupportBean.window(IntPrimitive).take(1) as c2" +
                " from SupportBean_S0",
                key);
            env.CompileDeploy(soda, eplSelect, path).AddListener("s0");
            object[][] expectedAggType = {
                new object[] {"c0", typeof(int?)},
                new object[] {"c1", typeof(int?)},
                new object[] {"c2", typeof(ICollection<object>)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            var fields = new[] {"c0", "c1", "c2"};
            MakeSendBean(env, "E1", 10, 0);
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, 1, Collections.SingletonList(10)});

            MakeSendBean(env, "E1", 20, 0);
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {20, 2, Collections.SingletonList(10)});

            env.MilestoneInc(milestone);

            MakeSendBean(env, "E1", 30, 0);
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {30, 2, Collections.SingletonList(20)});

            env.UndeployAll();
        }

        private static object[] MakePopulateEvent()
        {
            return new object[] {
                "E1",
                DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:55:00.000"), // ts
                new MyBean(), // mb
                new[] {new MyBean(), new MyBean()}, // mbarr
                new object[] {"p0value"}, // me
                new[] {
                    new object[] {"0_p0"},
                    new object[] {"1_p0"}
                } // mearr
            };
        }

        private static void RunAggregationWDatetimeEtc(
            RegressionEnvironment env,
            bool grouped,
            bool soda,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare = "create table varaggWDE (" +
                             (grouped ? "key string primary key, " : "") +
                             "a1 lastever(long), a2 window(*) @type('SupportBean'))";
            env.CompileDeploy(soda, eplDeclare, path);

            var eplInto = "@Name('into') into table varaggWDE " +
                          "select lastever(LongPrimitive) as a1, window(*) as a2 from SupportBean#time(10 seconds)" +
                          (grouped ? " group by TheString" : "");
            env.CompileDeploy(soda, eplInto, path);
            object[][] expectedAggType = {
                new object[] {"a1", typeof(long?)},
                new object[] {"a2", typeof(SupportBean[])}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType,
                env.Statement("into").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            var key = grouped ? "[\"E1\"]" : "";
            var eplGet = "@Name('s0') select varaggWDE" +
                         key +
                         ".a1.after(150L) as c0, " +
                         "varaggWDE" +
                         key +
                         ".a2.countOf() as c1 from SupportBean_S0";
            env.CompileDeploy(soda, eplGet, path).AddListener("s0");
            object[][] expectedGetType = {
                new object[] {"c0", typeof(bool?)},
                new object[] {"c1", typeof(int?)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedGetType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            var fields = new[] {"c0", "c1"};
            MakeSendBean(env, "E1", 10, 100);
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {false, 1});

            env.MilestoneInc(milestone);

            MakeSendBean(env, "E1", 20, 200);
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {true, 2});

            env.UndeployAll();
        }

        private static void MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        internal class InfraAggDatetimeAndEnumerationAndMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunAggregationWDatetimeEtc(env, false, false, milestone);
                RunAggregationWDatetimeEtc(env, true, false, milestone);
                RunAggregationWDatetimeEtc(env, false, true, milestone);
                RunAggregationWDatetimeEtc(env, true, true, milestone);
            }
        }

        internal class InfraPlainPropDatetimeAndEnumerationAndMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunPlainPropertyWDatetimeEtc(env, false, false, milestone);
                RunPlainPropertyWDatetimeEtc(env, true, false, milestone);
                RunPlainPropertyWDatetimeEtc(env, false, true, milestone);
                RunPlainPropertyWDatetimeEtc(env, true, true, milestone);
            }

            private static void RunPlainPropertyWDatetimeEtc(
                RegressionEnvironment env,
                bool grouped,
                bool soda,
                AtomicLong milestone)
            {
                var myBean = TypeHelper.MaskTypeName<MyBean>();
                var path = new RegressionPath();
                var eplType =
                    $"create objectarray schema MyEvent as (p0 string);" +
                    $"create objectarray schema PopulateEvent as (" +
                    $" key string," +
                    $" ts long," +
                    $" mb {myBean}," +
                    $" mbarr {myBean}[]," +
                    $" me MyEvent," +
                    $" mearr MyEvent[])";
                env.CompileDeployWBusPublicType(eplType, path);

                var primaryKey = (grouped ? "primary key" : "");
                var eplDeclare =
                    "create table varaggPWD " +
                    $"(key string {primaryKey}" +
                    $", ts long" +
                    $", mb {myBean}" +
                    $", mbarr {myBean}[]" +
                    $", me MyEvent" +
                    $", mearr MyEvent[])";
                env.CompileDeploy(soda, eplDeclare, path);

                var key = grouped ? "[\"E1\"]" : "";
                var eplSelect =
                    $"@Name('s0') select " +
                    $"varaggPWD{key}.ts.getMinuteOfHour() as c0, " +
                    $"varaggPWD{key}.mb.GetMyProperty() as c1, " +
                    $"varaggPWD{key}.mbarr.takeLast(1) as c2, " +
                    $"varaggPWD{key}.me.p0 as c3, " +
                    $"varaggPWD{key}.mearr.selectFrom(i -> i.p0) as c4 " +
                    $" from SupportBean_S0";
                env.CompileDeploy(eplSelect, path);
                env.AddListener("s0");

                var eplMerge = "on PopulateEvent merge varaggPWD " +
                               "when not matched then insert " +
                               "select key, ts, mb, mbarr, me, mearr";
                env.CompileDeploy(soda, eplMerge, path);

                env.MilestoneInc(milestone);

                var @event = MakePopulateEvent();
                env.SendEventObjectArray(@event, "PopulateEvent");
                env.SendEventBean(new SupportBean_S0(0, "E1"));
                var output = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    output,
                    new[] {
                        "c0", "c1", "c3"
                    },
                    new object[] {
                        55, "x", "p0value"
                    });
                Assert.AreEqual(1, ((ICollection<object>) output.Get("c2")).Count);
                Assert.AreEqual("[\"0_p0\", \"1_p0\"]", output.Get("c4").RenderAny());

                env.UndeployAll();
            }
        }

        internal class InfraNestedDotMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionNestedDotMethod(env, true, false, milestone);
                TryAssertionNestedDotMethod(env, false, false, milestone);
                TryAssertionNestedDotMethod(env, true, true, milestone);
                TryAssertionNestedDotMethod(env, false, true, milestone);
            }
        }

        public class MyBean
        {
            public string MyProperty => "x";

            public string GetMyProperty()
            {
                return "x";
            }
        }
    }
} // end of namespace