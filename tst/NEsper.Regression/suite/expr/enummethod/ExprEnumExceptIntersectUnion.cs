///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumExceptIntersectUnion
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumStringArrayIntersection());
            execs.Add(new ExprEnumSetLogicWithContained());
            execs.Add(new ExprEnumSetLogicWithScalar());
            execs.Add(new ExprEnumInheritance());
            execs.Add(new ExprEnumInvalid());
            execs.Add(new ExprEnumSetLogicWithEvents());
            execs.Add(new ExprEnumUnionWhere());
            return execs;
        }

        private class ExprEnumStringArrayIntersection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@buseventtype @public create objectarray schema Event(meta1 string[], meta2 string[]);\n" +
                          "@name('s0') select * from Event(meta1.intersect(meta2).countOf() > 0);\n";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                SendAndAssert(env, "a,b", "a,b", true);
                SendAndAssert(env, "c,d", "a,b", false);
                SendAndAssert(env, "c,d", "a,d", true);
                SendAndAssert(env, "a,d,a,a", "b,c", false);
                SendAndAssert(env, "a,d,a,a", "b,d", true);

                env.UndeployAll();
            }
        }

        private class ExprEnumSetLogicWithContained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "contained.except(containedTwo)");
                builder.WithExpression(fields[1], "contained.intersect(containedTwo)");
                builder.WithExpression(fields[2], "contained.union(containedTwo)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<SupportBean_ST0>)));

                var first = SupportBean_ST0_Container.Make2ValueList("E1,1", "E2,10", "E3,1", "E4,10", "E5,11");
                var second = SupportBean_ST0_Container.Make2ValueList("E1,1", "E3,1", "E4,10");
                builder.WithAssertion(new SupportBean_ST0_Container(first, second))
                    .Verify("c0", val => AssertST0Id(val, "E2,E5"))
                    .Verify("c1", val => AssertST0Id(val, "E1,E3,E4"))
                    .Verify("c2", val => AssertST0Id(val, "E1,E2,E3,E4,E5,E1,E3,E4"));

                builder.Run(env);
            }
        }

        private class ExprEnumSetLogicWithEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') expression last10A {" +
                    " (select * from SupportBean_ST0(key0 like 'A%')#length(2)) " +
                    "}" +
                    "expression last10NonZero {" +
                    " (select * from SupportBean_ST0(p00 > 0)#length(2)) " +
                    "}" +
                    "select " +
                    "last10A().except(last10NonZero()) as val0," +
                    "last10A().intersect(last10NonZero()) as val1, " +
                    "last10A().union(last10NonZero()) as val2 " +
                    "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtTypes("s0", "val0".SplitCsv(), new Type[] { typeof(ICollection<SupportBean_ST0>) });

                env.SendEventBean(new SupportBean_ST0("E1", "A1", 10)); // in both
                env.SendEventBean(new SupportBean());
                AssertST0Id(env, "val0", "");
                AssertST0Id(env, "val1", "E1");
                AssertST0Id(env, "val2", "E1,E1");
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean_ST0("E2", "A1", 0));
                env.SendEventBean(new SupportBean());
                AssertST0Id(env, "val0", "E2");
                AssertST0Id(env, "val1", "E1");
                AssertST0Id(env, "val2", "E1,E2,E1");
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean_ST0("E3", "B1", 0));
                env.SendEventBean(new SupportBean());
                AssertST0Id(env, "val0", "E2");
                AssertST0Id(env, "val1", "E1");
                AssertST0Id(env, "val2", "E1,E2,E1");
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean_ST0("E4", "A2", -1));
                env.SendEventBean(new SupportBean());
                AssertST0Id(env, "val0", "E2,E4");
                AssertST0Id(env, "val1", "");
                AssertST0Id(env, "val2", "E2,E4,E1");
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean_ST0("E5", "A3", -2));
                env.SendEventBean(new SupportBean());
                AssertST0Id(env, "val0", "E4,E5");
                AssertST0Id(env, "val1", "");
                AssertST0Id(env, "val2", "E4,E5,E1");
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean_ST0("E6", "A6", 11)); // in both
                env.SendEventBean(new SupportBean_ST0("E7", "A7", 12)); // in both
                env.SendEventBean(new SupportBean());
                AssertST0Id(env, "val0", "");
                AssertST0Id(env, "val1", "E6,E7");
                AssertST0Id(env, "val2", "E6,E7,E6,E7");
                env.ListenerReset("s0");

                env.UndeployAll();
            }
        }

        private class ExprEnumSetLogicWithScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.except(strvalstwo)");
                builder.WithExpression(fields[1], "strvals.intersect(strvalstwo)");
                builder.WithExpression(fields[2], "strvals.union(strvalstwo)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<string>)));

                builder.WithAssertion(SupportCollection.MakeString("E1,E2", "E3,E4"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E2"))
                    .Verify("c1", val => AssertValuesArrayScalar(val))
                    .Verify("c2", val => AssertValuesArrayScalar(val, "E1", "E2", "E3", "E4"));

                builder.WithAssertion(SupportCollection.MakeString(null, "E3,E4"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, null))
                    .Verify("c1", val => AssertValuesArrayScalar(val, null))
                    .Verify("c2", val => AssertValuesArrayScalar(val, null));

                builder.WithAssertion(SupportCollection.MakeString("", "E3,E4"))
                    .Verify("c0", val => AssertValuesArrayScalar(val))
                    .Verify("c1", val => AssertValuesArrayScalar(val))
                    .Verify("c2", val => AssertValuesArrayScalar(val, "E3", "E4"));

                builder.WithAssertion(SupportCollection.MakeString("E1,E3,E5", "E3,E4"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E5"))
                    .Verify("c1", val => AssertValuesArrayScalar(val, "E3"))
                    .Verify("c2", val => AssertValuesArrayScalar(val, "E1", "E3", "E5", "E3", "E4"));

                builder.Run(env);
            }
        }

        private class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select contained.union(true) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.union(true)': Enumeration method 'union' requires an expression yielding a collection of events of type");

                epl =
                    "select contained.union(prevwindow(s1)) from SupportBean_ST0_Container#lastevent, SupportBean#keepall s1";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.union(prevwindow(s1))': Enumeration method 'union' expects event type '" +
                    typeof(SupportBean_ST0).FullName +
                    "' but receives event type 'SupportBean'");

                epl = "select (select * from SupportBean#keepall).union(strvals) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'subselect_1.union(strvals)': Enumeration method 'union' requires an expression yielding a collection of events of type 'SupportBean' as input parameter");

                epl = "select strvals.union((select * from SupportBean#keepall)) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'strvals.union(subselect_1)': Enumeration method 'union' requires an expression yielding a collection of values of type 'String' as input parameter");
            }
        }

        private class ExprEnumUnionWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') expression one {" +
                          "  x => x.contained.where(y => p00 = 10)" +
                          "} " +
                          "" +
                          "expression two {" +
                          "  x => x.contained.where(y => p00 = 11)" +
                          "} " +
                          "" +
                          "select one(bean).union(two(bean)) as val0 from SupportBean_ST0_Container as bean";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStmtTypes("s0", "val0".SplitCsv(), new Type[] { typeof(ICollection<SupportBean_ST0>) });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,10", "E3,1", "E4,10", "E5,11"));
                AssertST0IdWReset(env, "val0", "E2,E4,E5");

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,10", "E2,1", "E3,1"));
                AssertST0IdWReset(env, "val0", "E1");

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,10", "E4,11"));
                AssertST0IdWReset(env, "val0", "E3,E4");

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                AssertST0IdWReset(env, "val0", null);

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                AssertST0IdWReset(env, "val0", "");

                env.UndeployAll();
            }
        }

        private class ExprEnumInheritance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    if (rep.IsMapEvent() || rep.IsObjectArrayEvent()) {
                        TryAssertionInheritance(env, rep);
                    }
                }
            }
        }

        private static void TryAssertionInheritance(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var epl = eventRepresentationEnum.GetAnnotationText() +
                      " @public @buseventtype create schema BaseEvent as (b1 string);\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " @public @buseventtype create schema SubEvent as (s1 string) inherits BaseEvent;\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " @public @buseventtype create schema OuterEvent as (bases BaseEvent[], subs SubEvent[]);\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " @name('s0') select bases.union(subs) as val from OuterEvent;\n";
            env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(
                    new object[]
                        { new object[][] { new object[] { "b10" } }, new object[][] { new object[] { "b10", "s10" } } },
                    "OuterEvent");
            }
            else {
                var baseEvent = MakeMap("b1", "b10");
                var subEvent = MakeMap("s1", "s10");
                var outerEvent = MakeMap(
                    "bases",
                    new IDictionary<string, object>[] { baseEvent },
                    "subs",
                    new IDictionary<string, object>[] { subEvent });
                env.SendEventMap(outerEvent, "OuterEvent");
            }

            env.AssertEventNew(
                "s0",
                @event => {
                    var result = (ICollection<object>)@event.Get("val");
                    Assert.AreEqual(2, result.Count);
                });

            env.UndeployAll();
        }

        private static IDictionary<string, object> MakeMap(
            string key,
            object value)
        {
            IDictionary<string, object> map = new LinkedHashMap<string, object>();
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
            env.SendEventObjectArray(new object[] { metaOne.SplitCsv(), metaTwo.SplitCsv() }, "Event");
            env.AssertListenerInvokedFlag("s0", expected);
        }
    }
} // end of namespace