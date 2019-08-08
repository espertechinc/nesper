///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherStreamExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherChainedParameterized());
            execs.Add(new EPLOtherStreamFunction());
            execs.Add(new EPLOtherInstanceMethodOuterJoin());
            execs.Add(new EPLOtherInstanceMethodStatic());
            execs.Add(new EPLOtherStreamInstanceMethodAliased());
            execs.Add(new EPLOtherStreamInstanceMethodNoAlias());
            execs.Add(new EPLOtherJoinStreamSelectNoWildcard());
            execs.Add(new EPLOtherPatternStreamSelectNoWildcard());
            execs.Add(new EPLOtherInvalidSelect());
            return execs;
        }

        internal class EPLOtherChainedParameterized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subexpr = "top.getChildOne(\"abc\",10).getChildTwo(\"append\")";
                var epl = "@Name('s0') select " + subexpr + " from SupportChainTop as top";
                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionChainedParam(env, subexpr);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                TryAssertionChainedParam(env, subexpr);
                env.UndeployAll();

                // test property hosts a method
                env.CompileDeploy(
                        "@Name('s0') select inside.getMyString() as val," +
                        "inside.insideTwo.getMyOtherString() as val2 " +
                        "from SupportBeanStaticOuter")
                    .AddListener("s0");

                env.SendEventBean(new SupportBeanStaticOuter());
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("hello", result.Get("val"));
                Assert.AreEqual("hello2", result.Get("val2"));
                env.UndeployAll();
            }

            private static void TryAssertionChainedParam(
                RegressionEnvironment env,
                string subexpr)
            {
                object[][] rows = {
                    new object[] {subexpr, typeof(SupportChainChildTwo)}
                };
                for (var i = 0; i < rows.Length; i++) {
                    var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
                    Assert.AreEqual(rows[i][0], prop.PropertyName);
                    Assert.AreEqual(rows[i][1], prop.PropertyType);
                }

                env.SendEventBean(new SupportChainTop());
                var result = env.Listener("s0").AssertOneGetNewAndReset().Get(subexpr);
                Assert.AreEqual("abcappend", ((SupportChainChildTwo) result).Text);
            }
        }

        internal class EPLOtherStreamFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var prefix = "@Name('s0') select * from SupportMarketDataBean as s0 where " +
                             typeof(SupportStaticMethodLib).Name;
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZero(s0)");
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZero(*)");
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZeroEventBean(s0)");
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZeroEventBean(*)");
            }

            private static void TryAssertionStreamFunction(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("ACME", 0, 0L, null));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportMarketDataBean("ACME", 0, 100L, null));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLOtherInstanceMethodOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne = "@Name('s0') select Symbol, s1.getTheString() as TheString from " +
                              "SupportMarketDataBean#keepall as s0 " +
                              "left outer join " +
                              "SupportBean#keepall as s1 on s0.Symbol=s1.TheString";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"Symbol", "TheString"},
                    new object[] {"ACME", null});

                env.UndeployAll();
            }
        }

        internal class EPLOtherInstanceMethodStatic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne =
                    "@Name('s0') select Symbol, s1.getSimpleProperty() as simpleprop, s1.makeDefaultBean() as def from " +
                    "SupportMarketDataBean#keepall as s0 " +
                    "left outer join " +
                    "SupportBeanComplexProps#keepall as s1 on s0.Symbol=s1.simpleProperty";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new[] {"Symbol", "simpleprop"},
                    new object[] {"ACME", null});
                Assert.IsNull(theEvent.Get("def"));

                var eventComplexProps = SupportBeanComplexProps.MakeDefaultBean();
                eventComplexProps.SimpleProperty = "ACME";
                env.SendEventBean(eventComplexProps);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new[] {"Symbol", "simpleprop"},
                    new object[] {"ACME", "ACME"});
                Assert.IsNotNull(theEvent.Get("def"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherStreamInstanceMethodAliased : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne =
                    "@Name('s0') select s0.getVolume() as Volume, s0.getSymbol() as Symbol, s0.getPriceTimesVolume(2) as pvf from " +
                    "SupportMarketDataBean as s0 ";
                env.CompileDeploy(textOne).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(3, type.PropertyNames.Length);
                Assert.AreEqual(typeof(long?), type.GetPropertyType("Volume"));
                Assert.AreEqual(typeof(string), type.GetPropertyType("Symbol"));
                Assert.AreEqual(typeof(double?), type.GetPropertyType("pvf"));

                var eventA = new SupportMarketDataBean("ACME", 4, 99L, null);
                env.SendEventBean(eventA);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"Volume", "Symbol", "pvf"},
                    new object[] {99L, "ACME", 4d * 99L * 2});

                env.UndeployAll();
            }
        }

        internal class EPLOtherStreamInstanceMethodNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne = "@Name('s0') select s0.getVolume(), s0.getPriceTimesVolume(3) from " +
                              "SupportMarketDataBean as s0 ";
                env.CompileDeploy(textOne).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(2, type.PropertyNames.Length);
                Assert.AreEqual(typeof(long?), type.GetPropertyType("s0.getVolume()"));
                Assert.AreEqual(typeof(double?), type.GetPropertyType("s0.getPriceTimesVolume(3)"));

                var eventA = new SupportMarketDataBean("ACME", 4, 2L, null);
                env.SendEventBean(eventA);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"s0.getVolume()", "s0.getPriceTimesVolume(3)"},
                    new object[] {2L, 4d * 2L * 3d});
                env.UndeployAll();

                // try instance method that accepts EventBean
                var epl = "create schema MyTestEvent as " +
                          typeof(MyTestEvent).Name +
                          ";\n" +
                          "@Name('s0') select " +
                          "s0.getValueAsInt(s0, 'Id') as c0," +
                          "s0.getValueAsInt(*, 'Id') as c1" +
                          " from MyTestEvent as s0";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                env.SendEventBean(new MyTestEvent(10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {10, 10});

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinStreamSelectNoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with alias
                var textOne = "@Name('s0') select s0 as s0stream, s1 as s1stream from " +
                              "SupportMarketDataBean#keepall as s0, " +
                              "SupportBean#keepall as s1";

                // Attach listener to feed
                env.CompileDeploy(textOne).AddListener("s0");
                var model = env.EplToModel(textOne);
                Assert.AreEqual(textOne, model.ToEPL());

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(2, type.PropertyNames.Length);
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0stream"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1stream"));

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);

                var eventB = new SupportBean();
                env.SendEventBean(eventB);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"s0stream", "s1stream"},
                    new object[] {eventA, eventB});

                env.UndeployAll();

                // try no alias
                textOne = "@Name('s0') select s0, s1 from " +
                          "SupportMarketDataBean#keepall as s0, " +
                          "SupportBean#keepall as s1";
                env.CompileDeploy(textOne).AddListener("s0");

                type = env.Statement("s0").EventType;
                Assert.AreEqual(2, type.PropertyNames.Length);
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));

                env.SendEventBean(eventA);
                env.SendEventBean(eventB);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"s0", "s1"},
                    new object[] {eventA, eventB});

                env.UndeployAll();
            }
        }

        internal class EPLOtherPatternStreamSelectNoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with alias
                var textOne = "@Name('s0') select * from pattern [every e1=SupportMarketDataBean -> e2=" +
                              "SupportBean(" +
                              typeof(SupportStaticMethodLib).Name +
                              ".compareEvents(e1, e2))]";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);

                var eventB = new SupportBean("ACME", 1);
                env.SendEventBean(eventB);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"e1", "e2"},
                    new object[] {eventA, eventB});

                env.UndeployAll();
            }
        }

        internal class EPLOtherInvalidSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select s0.getString(1,2,3) from SupportBean as s0",
                    "skip");

                TryInvalidCompile(
                    env,
                    "select s0.abc() from SupportBean as s0",
                    "Failed to validate select-clause expression 's0.abc()': Failed to solve 'abc' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'abc': Could not find enumeration method, date-time method or instance method named 'abc' in class '" +
                    typeof(SupportBean).Name +
                    "' taking no parameters [");

                TryInvalidCompile(
                    env,
                    "select s.TheString from pattern [every [2] s=SupportBean] ee",
                    "Failed to validate select-clause expression 's.TheString': Failed to resolve property 's.TheString' (property 's' is an indexed property and requires an index or enumeration method to access values)");
            }
        }

        [Serializable]
        public class MyTestEvent
        {
            public MyTestEvent(int id)
            {
                Id = id;
            }

            public int Id { get; }

            public int GetValueAsInt(
                EventBean @event,
                string propertyName)
            {
                return @event.Get(propertyName).AsInt();
            }
        }
    }
} // end of namespace