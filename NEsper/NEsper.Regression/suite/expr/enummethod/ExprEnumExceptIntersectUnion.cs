///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumExceptIntersectUnion
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if false
            execs.Add(new ExprEnumStringArrayIntersection());
            execs.Add(new ExprEnumSetLogicWithContained());
            execs.Add(new ExprEnumSetLogicWithScalar());
            execs.Add(new ExprEnumInheritance());
            execs.Add(new ExprEnumInvalid());
            execs.Add(new ExprEnumSetLogicWithEvents());
#endif
            execs.Add(new ExprEnumUnionWhere());
            return execs;
        }

        private static void TryAssertionInheritance(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var epl = eventRepresentationEnum.GetAnnotationText() + " create schema BaseEvent as (b1 string);\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " create schema SubEvent as (s1 string) inherits BaseEvent;\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " create schema OuterEvent as (bases BaseEvent[], subs SubEvent[]);\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " @Name('s0') select bases.union(subs) as val from OuterEvent;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(
                    new object[] {
                        new[] {new object[] {"b10"}},
                        new[] {new object[] {"b10", "s10"}}
                    },
                    "OuterEvent");
            }
            else {
                var baseEvent = MakeMap("b1", "b10");
                var subEvent = MakeMap("s1", "s10");
                var outerEvent = MakeMap("bases", new[] {baseEvent}, "subs", new[] {subEvent});
                env.SendEventMap(outerEvent, "OuterEvent");
            }

            var result = env.Listener("s0").AssertOneGetNewAndReset().Get("val").Unwrap<object>();
            Assert.AreEqual(2, result.Count);

            env.UndeployAll();
        }

        private static IDictionary<string, object> MakeMap(
            string key,
            object value)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put(key, value);
            return map;
        }

        private static IDictionary<string, object> MakeMap(
            string key,
            object value,
            string key2,
            object value2)
        {
            var map = MakeMap(key, value);
            map.Put(key2, value2);
            return map;
        }

        private static void SendAndAssert(
            RegressionEnvironment env,
            string metaOne,
            string metaTwo,
            bool expected)
        {
            env.SendEventObjectArray(new object[] {metaOne.SplitCsv(), metaTwo.SplitCsv()}, "Event");
            Assert.AreEqual(expected, env.Listener("s0").IsInvokedAndReset());
        }

        internal class ExprEnumStringArrayIntersection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create objectarray schema Event(meta1 string[], meta2 string[]);\n" +
                          "@Name('s0') select * from Event(meta1.intersect(meta2).countOf() > 0);\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                SendAndAssert(env, "a,b", "a,b", true);
                SendAndAssert(env, "c,d", "a,b", false);
                SendAndAssert(env, "c,d", "a,d", true);
                SendAndAssert(env, "a,d,a,a", "b,c", false);
                SendAndAssert(env, "a,d,a,a", "b,d", true);

                env.UndeployAll();
            }
        }

        internal class ExprEnumSetLogicWithContained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "Contained.except(ContainedTwo) as val0," +
                          "Contained.intersect(ContainedTwo) as val1, " +
                          "Contained.union(ContainedTwo) as val2 " +
                          " from SupportBean_ST0_Container";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {typeof(ICollection<object>)});

                var first = SupportBean_ST0_Container.Make2ValueList("E1,1", "E2,10", "E3,1", "E4,10", "E5,11");
                var second = SupportBean_ST0_Container.Make2ValueList("E1,1", "E3,1", "E4,10");
                env.SendEventBean(new SupportBean_ST0_Container(first, second));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E2,E5");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1,E3,E4");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1,E2,E3,E4,E5,E1,E3,E4");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumSetLogicWithEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') expression last10A {" +
                    " (select * from SupportBean_ST0(Key0 like 'A%')#length(2)) " +
                    "}" +
                    "expression last10NonZero {" +
                    " (select * from SupportBean_ST0(P00 > 0)#length(2)) " +
                    "}" +
                    "select " +
                    "last10A().except(last10NonZero()) as val0," +
                    "last10A().intersect(last10NonZero()) as val1, " +
                    "last10A().union(last10NonZero()) as val2 " +
                    "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(new SupportBean_ST0("E1", "A1", 10)); // in both
                env.SendEventBean(new SupportBean());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E2", "A1", 0));
                env.SendEventBean(new SupportBean());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1,E2,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E3", "B1", 0));
                env.SendEventBean(new SupportBean());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1,E2,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E4", "A2", -1));
                env.SendEventBean(new SupportBean());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E2,E4");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E2,E4,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E5", "A3", -2));
                env.SendEventBean(new SupportBean());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E4,E5");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E4,E5,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean_ST0("E6", "A6", 11)); // in both
                env.SendEventBean(new SupportBean_ST0("E7", "A7", 12)); // in both
                env.SendEventBean(new SupportBean());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E6,E7");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E6,E7,E6,E7");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumSetLogicWithScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "Strvals.except(Strvalstwo) as val0," +
                          "Strvals.intersect(Strvalstwo) as val1, " +
                          "Strvals.union(Strvalstwo) as val2 " +
                          " from SupportCollection as bean";
                env.CompileDeploy(epl).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2", "E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E2");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", "E1", "E2", "E3", "E4");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString(null, "E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", null);
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", null);
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", null);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString("", "E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", "E3", "E4");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportCollection.MakeString("E1,E3,E5", "E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E5");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E3");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val2", "E1", "E3", "E5", "E3", "E4");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select Contained.union(true) from SupportBean_ST0_Container";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'Contained.union(true)': Enumeration method 'union' requires an expression yielding a collection of events of type");

                epl = "select Contained.union(prevwindow(S1)) from SupportBean_ST0_Container#lastevent, SupportBean#keepall S1";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'Contained.union(prevwindow(S1))': Enumeration method 'union' expects event type '" +
                    typeof(SupportBean_ST0).Name +
                    "' but receives event type 'SupportBean'");

                epl = "select (select * from SupportBean#keepall).union(Strvals) from SupportCollection";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'subselect_1.union(Strvals)': Enumeration method 'union' requires an expression yielding a collection of events of type 'SupportBean' as input parameter");

                epl = "select Strvals.union((select * from SupportBean#keepall)) from SupportCollection";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'Strvals.union(subselect_1)': Enumeration method 'union' requires an expression yielding a collection of values of type 'System.String' as input parameter");
            }
        }

        internal class ExprEnumUnionWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') expression one {" +
                          "  x => x.Contained.where(y -> P00 = 10)" +
                          "} " +
                          "" +
                          "expression two {" +
                          "  x => x.Contained.where(y -> P00 = 11)" +
                          "} " +
                          "" +
                          "select one(bean).union(two(bean)) as val0 from SupportBean_ST0_Container as bean";
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new [] { "val0" },
                    new[] {
                        typeof(ICollection<EventBean>)
                    });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,10", "E3,1", "E4,10", "E5,11"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E2,E4,E5");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,10", "E2,1", "E3,1"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,10", "E4,11"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E3,E4");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", null);
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumInheritance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    if (rep.IsMapEvent() || rep.IsObjectArrayEvent()) {
                        TryAssertionInheritance(env, rep);
                    }
                }
            }
        }
    }
} // end of namespace