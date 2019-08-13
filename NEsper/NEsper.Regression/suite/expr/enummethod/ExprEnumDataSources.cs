///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumDataSources
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumProperty());
            execs.Add(new ExprEnumSubstitutionParameter());
            execs.Add(new ExprEnumEnumObject());
            execs.Add(new ExprEnumSortedMaxMinBy());
            execs.Add(new ExprEnumJoin());
            execs.Add(new ExprEnumPrevWindowSorted());
            execs.Add(new ExprEnumNamedWindow());
            execs.Add(new ExprEnumSubselect());
            execs.Add(new ExprEnumAccessAggregation());
            execs.Add(new ExprEnumPrevFuncs());
            execs.Add(new ExprEnumUDFStaticMethod());
            execs.Add(new ExprEnumPropertySchema());
            execs.Add(new ExprEnumPropertyInsertIntoAtEventBean());
            execs.Add(new ExprEnumPatternFilter());
            execs.Add(new ExprEnumVariable());
            execs.Add(new ExprEnumTableRow());
            execs.Add(new ExprEnumMatchRecognizeDefine());
            execs.Add(new ExprEnumMatchRecognizeMeasures(false));
            execs.Add(new ExprEnumMatchRecognizeMeasures(true));
            return execs;
        }

        public static void RunAssertionMethodInvoke(
            RegressionEnvironment env,
            string epl)
        {
            var fields = "flag".SplitCsv();
            env.CompileDeploy("@Name('s0') " + epl).AddListener("s0");

            env.SendEventBean(new SupportSelectorWithListEvent("1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {true});

            env.SendEventBean(new SupportSelectorWithListEvent("4"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {false});

            env.UndeployAll();
        }

        private static void TrySubstitutionParameter(
            RegressionEnvironment env,
            string substitution,
            object parameter)
        {
            var compiled = env.Compile(
                "@Name('s0') select * from SupportBean(" + substitution + ".sequenceEqual({1, IntPrimitive, 100}))");
            env.Deploy(
                compiled,
                new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => prepared.SetObject(1, parameter)));
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(new SupportBean("E2", 20));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }

        private static SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> items)
        {
            if (!items.IsEmpty() && items.First() is EventBean) {
                Assert.Fail("Iterator provides EventBean instances");
            }

            return items.ToArray();
        }

        private static IDictionary<string, object> MakeBEvent(string symbol)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("a", Collections.SingletonMap("Symbol", symbol));
            return map;
        }

        private static void AssertPropsMapRows(
            ICollection<object> rows,
            string[] fields,
            object[][] objects)
        {
            var mapsColl = (ICollection<IDictionary<string, object>>) rows;
            var maps = mapsColl.ToArray();
            EPAssertionUtil.AssertPropsPerRow(maps, fields, objects);
        }

        private static void AssertColl(
            string expected,
            object value)
        {
            EPAssertionUtil.AssertEqualsExactOrder(
                expected.SplitCsv(),
                value.UnwrapIntoArray<object>());
        }

        internal class ExprEnumPropertySchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema OrderDetail(itemId string);\n" +
                          "create schema OrderEvent(details OrderDetail[]);\n" +
                          "@Name('s0') select details.where(i -> i.ItemId = '001') as c0 from OrderEvent;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                var detailOne = CollectionUtil.PopulateNameValueMap("itemId", "002");
                var detailTwo = CollectionUtil.PopulateNameValueMap("itemId", "001");
                env.SendEventMap(
                    CollectionUtil.PopulateNameValueMap("details", new[] {detailOne, detailTwo}),
                    "OrderEvent");

                var c = env.Listener("s0").AssertOneGetNewAndReset().Get("c0").UnwrapIntoArray<object>();
                EPAssertionUtil.AssertEqualsExactOrder(c, new[] {detailTwo});

                env.UndeployAll();
            }
        }

        internal class ExprEnumPropertyInsertIntoAtEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create objectarray schema StockTick(Id string, Price int);\n" +
                          "insert into TicksLarge select window(*).where(e -> e.Price > 100) @eventbean as ticksLargePrice\n" +
                          "from StockTick#time(10) having count(*) > 2;\n" +
                          "@Name('s0') select ticksLargePrice.where(e -> e.Price < 200) as ticksLargeLess200 from TicksLarge;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                env.SendEventObjectArray(new object[] {"E1", 90}, "StockTick");
                env.SendEventObjectArray(new object[] {"E2", 120}, "StockTick");
                env.SendEventObjectArray(new object[] {"E3", 95}, "StockTick");

                Assert.AreEqual(
                    1,
                    env.Listener("s0").AssertOneGetNewAndReset().Get("ticksLargeLess200").Unwrap<object>().Count);

                env.UndeployAll();
            }
        }

        internal class ExprEnumMatchRecognizeMeasures : RegressionExecution
        {
            private readonly bool select;

            public ExprEnumMatchRecognizeMeasures(bool select)
            {
                this.select = select;
            }

            public void Run(RegressionEnvironment env)
            {
                string epl;
                if (!select) {
                    epl = "select Ids from SupportBean match_recognize ( " +
                          "  measures A.selectFrom(o -> o.TheString) as Ids ";
                }
                else {
                    epl =
                        "select a.selectFrom(o -> o.TheString) as Ids from SupportBean match_recognize (measures A as a ";
                }

                epl = "@Name('s0') " + epl + " pattern (A{3}) define A as A.IntPrimitive = 1)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E3", 1));
                AssertColl("E1,E2,E3", env.Listener("s0").AssertOneGetNewAndReset().Get("ids"));

                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportBean("E5", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E6", 1));
                AssertColl("E4,E5,E6", env.Listener("s0").AssertOneGetNewAndReset().Get("ids"));

                env.UndeployAll();
            }
        }

        internal class ExprEnumSubstitutionParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TrySubstitutionParameter(env, "?::int[primitive]", new[] {1, 10, 100});
                TrySubstitutionParameter(
                    env,
                    "?::System.Object[]",
                    new object[] {1, 10, 100});
                TrySubstitutionParameter(env, "?::int[]", new int?[] {1, 10, 100});
            }
        }

        internal class ExprEnumTableRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test table access expression
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTableUnkeyed(theWindow window(*) @type(SupportBean))", path);
                env.CompileDeploy(
                    "into table MyTableUnkeyed select window(*) as theWindow from SupportBean#time(30)",
                    path);
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                env.CompileDeploy(
                    "@Name('s0')select MyTableUnkeyed.theWindow.anyOf(v->IntPrimitive=10) as c0 from SupportBean_A",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_A("A0"));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class ExprEnumPatternFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from pattern [ ([2]a=SupportBean_ST0) -> b=SupportBean(IntPrimitive > a.max(i -> P00))]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("E1", 10));
                env.SendEventBean(new SupportBean_ST0("E2", 15));
                env.SendEventBean(new SupportBean("E3", 15));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E4", 16));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a[0].Id,a[1].Id,b.TheString".SplitCsv(),
                    new object[] {"E1", "E2", "E4"});
                env.UndeployAll();

                env.CompileDeploy(
                    "@Name('s0') select * from pattern [ a=SupportBean_ST0 until b=SupportBean => c=SupportBean(IntPrimitive > a.sumOf(i -> P00))]");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("E10", 10));
                env.SendEventBean(new SupportBean_ST0("E11", 15));
                env.SendEventBean(new SupportBean("E12", -1));
                env.SendEventBean(new SupportBean("E13", 25));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E14", 26));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a[0].Id,a[1].Id,b.TheString,c.TheString".SplitCsv(),
                    new object[] {"E10", "E11", "E12", "E14"});

                env.UndeployAll();
            }
        }

        internal class ExprEnumMatchRecognizeDefine : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try define-clause
                var fieldsOne = "a_array[0].TheString,a_array[1].TheString,b.TheString".SplitCsv();
                var textOne = "@Name('s0') select * from SupportBean " +
                              "match_recognize (" +
                              " measures A as a_array, B as b " +
                              " pattern (A* B)" +
                              " define" +
                              " B as A.anyOf(v-> v.IntPrimitive = B.IntPrimitive)" +
                              ")";
                env.CompileDeploy(textOne).AddListener("s0");
                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("A2", 20));
                env.SendEventBean(new SupportBean("A3", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {"A1", "A2", "A3"});

                env.SendEventBean(new SupportBean("A4", 1));
                env.SendEventBean(new SupportBean("A5", 2));
                env.SendEventBean(new SupportBean("A6", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("A7", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {"A4", "A5", "A7"});
                env.UndeployAll();

                // try measures-clause
                var fieldsTwo = new [] { "c0" };
                var textTwo = "@Name('s0') select * from SupportBean " +
                              "match_recognize (" +
                              " measures A.anyOf(v-> v.IntPrimitive = B.IntPrimitive) as c0 " +
                              " pattern (A* B)" +
                              " define" +
                              " A as A.TheString like 'A%'," +
                              " B as B.TheString like 'B%'" +
                              ")";
                env.CompileDeploy(textTwo).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("A2", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("B1", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {true});

                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("A2", 20));
                env.SendEventBean(new SupportBean("B1", 15));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {false});

                env.UndeployAll();
            }
        }

        internal class ExprEnumEnumObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl = "@Name('s0') select " +
                          "SupportEnumTwo.ENUM_VALUE_1.getMystrings().anyOf(v -> v = Id) as c0, " +
                          "value.getMystrings().anyOf(v -> v = '2') as c1 " +
                          "from SupportEnumTwoEvent";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEnumTwoEvent("0", SupportEnumTwo.ENUM_VALUE_1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                env.SendEventBean(new SupportEnumTwoEvent("2", SupportEnumTwo.ENUM_VALUE_2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true});

                env.UndeployAll();
            }
        }

        internal class ExprEnumSortedMaxMinBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3", "c4" };

                var eplWindowAgg = "@Name('s0') select " +
                                   "sorted(TheString).allOf(x -> x.IntPrimitive < 5) as c0," +
                                   "maxby(TheString).allOf(x -> x.IntPrimitive < 5) as c1," +
                                   "minby(TheString).allOf(x -> x.IntPrimitive < 5) as c2," +
                                   "maxbyever(TheString).allOf(x -> x.IntPrimitive < 5) as c3," +
                                   "minbyever(TheString).allOf(x -> x.IntPrimitive < 5) as c4" +
                                   " from SupportBean#length(5)";
                env.CompileDeploy(eplWindowAgg).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, true, true});

                env.UndeployAll();
            }
        }

        internal class ExprEnumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from SupportSelectorEvent#keepall as sel, SupportContainerEvent#keepall as cont " +
                    "where cont.items.anyOf(i -> sel.selector = i.selected)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportSelectorEvent("S1", "sel1"));
                env.SendEventBean(new SupportContainerEvent("C1", new SupportContainedItem("I1", "sel1")));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprEnumPrevWindowSorted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select prevwindow(st0) as val0, prevwindow(st0).esperInternalNoop() as val1 " +
                          "from SupportBean_ST0#sort(3, P00 asc) as st0";
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0", "val1" },
                    new[] {typeof(SupportBean_ST0[]), typeof(ICollection<object>)});

                env.SendEventBean(new SupportBean_ST0("E1", 5));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E2", 6));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1,E2");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E3", 4));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E3,E1,E2");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E5", 3));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E5,E3,E1");
                env.Listener("s0").Reset();
                env.UndeployAll();

                // Scalar version
                string[] fields = {"val0"};
                var stmtScalar = "@Name('s0') select prevwindow(Id).where(x -> x not like '%ignore%') as val0 " +
                                 "from SupportBean_ST0#keepall as st0";
                env.CompileDeploy(stmtScalar).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(new SupportBean_ST0("E1", 5));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E2ignore", 6));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E3", 4));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E3", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("ignoreE5", 3));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E3", "E1");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindow#keepall as SupportBean_ST0;\n" +
                          "on SupportBean_A delete from MyWindow;\n" +
                          "insert into MyWindow select * from SupportBean_ST0;\n";
                env.CompileDeploy(epl, path);

                env.CompileDeploy(
                    "@Name('s0') select MyWindow.allOf(x -> x.P00 < 5) as allOfX from SupportBean#keepall",
                    path);
                env.AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    "allOfX".SplitCsv(),
                    new[] {typeof(bool?)});

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean_ST0("ST0", "1", 10));
                env.SendEventBean(new SupportBean("E2", 10));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.UndeployModuleContaining("s0");
                env.SendEventBean(new SupportBean_A("A1"));

                // test named window correlated
                var eplNamedWindowCorrelated =
                    "@Name('s0') select MyWindow(Key0 = sb.TheString).allOf(x -> x.P00 < 5) as allOfX from SupportBean#keepall sb";
                env.CompileDeploy(eplNamedWindowCorrelated, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean_ST0("E2", "KEY1", 1));
                env.SendEventBean(new SupportBean("E2", 0));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean("KEY1", 0));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.UndeployAll();
            }
        }

        internal class ExprEnumSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test subselect-wildcard
                var eplSubselect =
                    "@Name('s0') select (select * from SupportBean_ST0#keepall).allOf(x -> x.P00 < 5) as allOfX from SupportBean#keepall";
                env.CompileDeploy(eplSubselect).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("ST0", "1", 0));
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean_ST0("ST0", "1", 10));
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));
                env.UndeployAll();

                // test subselect scalar return
                var eplSubselectScalar =
                    "@Name('s0') select (select Id from SupportBean_ST0#keepall).allOf(x -> x  like '%B%') as allOfX from SupportBean#keepall";
                env.CompileDeploy(eplSubselectScalar).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("B1", 0));
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean_ST0("A1", 0));
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));
                env.UndeployAll();

                // test subselect-correlated scalar return
                var eplSubselectScalarCorrelated =
                    "@Name('s0') select (select Key0 from SupportBean_ST0#keepall st0 where st0.Id = sb.TheString).allOf(x -> x  like '%hello%') as allOfX from SupportBean#keepall sb";
                env.CompileDeploy(eplSubselectScalarCorrelated).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("A1", "hello", 0));
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean_ST0("A2", "hello", 0));
                env.SendEventBean(new SupportBean("A2", 1));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(new SupportBean_ST0("A3", "test", 0));
                env.SendEventBean(new SupportBean("A3", 1));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));
                env.UndeployAll();

                // test subselect multivalue return
                string[] fields = {"Id", "P00"};
                var eplSubselectMultivalue =
                    "@Name('s0') select (select Id, P00 from SupportBean_ST0#keepall).take(10) as c0 from SupportBean";
                env.CompileDeploy(eplSubselectMultivalue).AddListener("s0");

                env.SendEventBean(new SupportBean_ST0("B1", 10));
                env.SendEventBean(new SupportBean("E1", 0));
                AssertPropsMapRows(
                    env.Listener("s0").AssertOneGetNewAndReset().Get("c0").Unwrap<object>(),
                    fields,
                    new[] {new object[] {"B1", 10}});

                env.SendEventBean(new SupportBean_ST0("B2", 20));
                env.SendEventBean(new SupportBean("E2", 0));
                AssertPropsMapRows(
                    env.Listener("s0").AssertOneGetNewAndReset().Get("c0").Unwrap<object>(),
                    fields,
                    new[] {
                        new object[] {"B1", 10}, new object[] {"B2", 20}
                    });
                env.UndeployAll();

                // test subselect that delivers events
                var epl = "create schema AEvent (Symbol string);\n" +
                          "create schema BEvent (a AEvent);\n" +
                          "@Name('s0') select (select a from BEvent#keepall).anyOf(v -> Symbol = 'GE') as flag from SupportBean;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                env.SendEventMap(MakeBEvent("XX"), "BEvent");
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("flag"));

                env.SendEventMap(MakeBEvent("GE"), "BEvent");
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("flag"));

                env.UndeployAll();
            }
        }

        internal class ExprEnumVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable string[] myvar = { 'E1', 'E3' };\n" +
                          "@Name('s0') select * from SupportBean(myvar.anyOf(v -> v = TheString));\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ExprEnumAccessAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0", "val1", "val2", "val3", "val4"};

                // test window(*) and first(*)
                var eplWindowAgg = "@Name('s0') select " +
                                   "window(*).allOf(x -> x.IntPrimitive < 5) as val0," +
                                   "first(*).allOf(x -> x.IntPrimitive < 5) as val1," +
                                   "first(*, 1).allOf(x -> x.IntPrimitive < 5) as val2," +
                                   "last(*).allOf(x -> x.IntPrimitive < 5) as val3," +
                                   "last(*, 1).allOf(x -> x.IntPrimitive < 5) as val4" +
                                   " from SupportBean#length(2)";
                env.CompileDeploy(eplWindowAgg).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, null, true, null});

                env.SendEventBean(new SupportBean("E2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, false, true});

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true, false});

                env.UndeployAll();

                // test scalar: window(*) and first(*)
                var eplWindowAggScalar = "@Name('s0') select " +
                                         "window(IntPrimitive).allOf(x -> x < 5) as val0," +
                                         "first(IntPrimitive).allOf(x -> x < 5) as val1," +
                                         "first(IntPrimitive, 1).allOf(x -> x < 5) as val2," +
                                         "last(IntPrimitive).allOf(x -> x < 5) as val3," +
                                         "last(IntPrimitive, 1).allOf(x -> x < 5) as val4" +
                                         " from SupportBean#length(2)";
                env.CompileDeploy(eplWindowAggScalar).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, null, true, null});

                env.SendEventBean(new SupportBean("E2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, false, true});

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true, false});

                env.UndeployAll();
            }
        }

        internal class ExprEnumProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test fragment type - collection inside
                var eplFragment =
                    "@Name('s0') select Contained.allOf(x -> x.P00 < 5) as allOfX from SupportBean_ST0_Container#keepall";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("ID1,KEY1,1"));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("ID1,KEY1,10"));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("allOfX"));
                env.UndeployAll();

                // test array and iterable
                var fields = new [] { "val0", "val1" };
                eplFragment = "@Name('s0') select intarray.sumof() as val0, " +
                              "intiterable.sumOf() as val1 " +
                              " from SupportCollection#keepall";
                env.CompileDeploy(eplFragment).AddListener("s0");

                env.SendEventBean(SupportCollection.MakeNumeric("5,6,7"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5 + 6 + 7, 5 + 6 + 7});

                env.UndeployAll();

                // test map event type with object-array prop
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType("create schema MySchema (books BookDesc[])", path);

                env.CompileDeploy("@Name('s0') select books.max(i -> i.Price) as mymax from MySchema", path);
                env.AddListener("s0");

                var @event = Collections.SingletonDataMap(
                    "books",
                    new[] {
                        new BookDesc("1", "book1", "dave", 1.00, null)
                    });
                env.SendEventMap(@event, "MySchema");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "mymax".SplitCsv(),
                    new object[] {1.0});

                env.UndeployAll();

                // test method invocation variations returning list/array of string and test UDF +property as well
                RunAssertionMethodInvoke(
                    env,
                    "select e.getTheList().anyOf(v -> v = selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select convertToArray(theList).anyOf(v -> v = selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select theArray.anyOf(v -> v = selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select e.getTheArray().anyOf(v -> v = selector) as flag from SupportSelectorWithListEvent e");
                RunAssertionMethodInvoke(
                    env,
                    "select e.theList.anyOf(v -> v = e.selector) as flag from pattern[every e=SupportSelectorWithListEvent]");
                RunAssertionMethodInvoke(
                    env,
                    "select e.NestedMyEvent.myNestedList.anyOf(v -> v = e.selector) as flag from pattern[every e=SupportSelectorWithListEvent]");
                RunAssertionMethodInvoke(
                    env,
                    "select " +
                    typeof(SupportSelectorWithListEvent).Name +
                    ".convertToArray(theList).anyOf(v -> v = selector) as flag from SupportSelectorWithListEvent e");

                env.UndeployAll();
            }
        }

        internal class ExprEnumPrevFuncs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0", "val1", "val2"};
                // test prevwindow(*) etc
                var epl = "@Name('s0') select " +
                          "prevwindow(sb).allOf(x -> x.IntPrimitive < 5) as val0," +
                          "prev(sb,1).allOf(x -> x.IntPrimitive < 5) as val1," +
                          "prevtail(sb,1).allOf(x -> x.IntPrimitive < 5) as val2" +
                          " from SupportBean#length(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, null, null});

                env.SendEventBean(new SupportBean("E2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false});

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true});

                env.SendEventBean(new SupportBean("E4", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true});
                env.UndeployAll();

                // test scalar prevwindow(property) etc
                var eplScalar = "@Name('s0') select " +
                                "prevwindow(IntPrimitive).allOf(x -> x < 5) as val0," +
                                "prev(IntPrimitive,1).allOf(x -> x < 5) as val1," +
                                "prevtail(IntPrimitive,1).allOf(x -> x < 5) as val2" +
                                " from SupportBean#length(2) as sb";
                env.CompileDeploy(eplScalar).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, null, null});

                env.SendEventBean(new SupportBean("E2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false});

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true});

                env.SendEventBean(new SupportBean("E4", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true});

                env.UndeployAll();
            }
        }

        internal class ExprEnumUDFStaticMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val1,val2,val3,val4".SplitCsv();
                var epl = "@Name('s0') select " +
                          "SupportBean_ST0_Container.makeSampleList().where(x -> x.P00 < 5) as val1, " +
                          "SupportBean_ST0_Container.makeSampleArray().where(x -> x.P00 < 5) as val2, " +
                          "makeSampleList().where(x -> x.P00 < 5) as val3, " +
                          "makeSampleArray().where(x -> x.P00 < 5) as val4 " +
                          "from SupportBean#length(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                SupportBean_ST0_Container.Samples = new[] {"E1,1", "E2,20", "E3,3"};
                env.SendEventBean(new SupportBean());
                foreach (var field in fields) {
                    var result = env.Listener("s0")
                        .AssertOneGetNew()
                        .Get(field)
                        .UnwrapIntoArray<SupportBean_ST0>();

                    Assert.AreEqual(2, result.Length, "Failed for field " + field);
                }

                env.Listener("s0").Reset();

                SupportBean_ST0_Container.Samples = null;
                env.SendEventBean(new SupportBean());
                foreach (var field in fields) {
                    Assert.IsNull(env.Listener("s0").AssertOneGetNew().Get(field));
                }

                env.Listener("s0").Reset();

                SupportBean_ST0_Container.Samples = new string[0];
                env.SendEventBean(new SupportBean());
                foreach (var field in fields) {
                    var result = env.Listener("s0")
                        .AssertOneGetNew()
                        .Get(field)
                        .UnwrapIntoArray<SupportBean_ST0>();
                    Assert.AreEqual(0, result.Length);
                }

                env.Listener("s0").Reset();
                env.UndeployAll();

                // test UDF returning scalar values collection
                fields = "val0,val1,val2,val3".SplitCsv();
                var eplScalar = "@Name('s0') select " +
                                "SupportCollection.makeSampleListString().where(x -> x != 'E1') as val0, " +
                                "SupportCollection.makeSampleArrayString().where(x -> x != 'E1') as val1, " +
                                "makeSampleListString().where(x -> x != 'E1') as val2, " +
                                "makeSampleArrayString().where(x -> x != 'E1') as val3 " +
                                "from SupportBean#length(2) as sb";
                env.CompileDeploy(eplScalar).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>),
                        typeof(ICollection<object>)
                    });

                SupportCollection.SampleCSV = "E1,E2,E3";
                env.SendEventBean(new SupportBean());
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), field, "E2", "E3");
                }

                env.Listener("s0").Reset();

                SupportCollection.SampleCSV = null;
                env.SendEventBean(new SupportBean());
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), field, null);
                }

                env.Listener("s0").Reset();

                SupportCollection.SampleCSV = "";
                env.SendEventBean(new SupportBean());
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), field);
                }

                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace