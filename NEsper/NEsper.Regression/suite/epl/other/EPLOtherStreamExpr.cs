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
using com.espertech.esper.common.@internal.util;
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
                var subexpr = "top.GetChildOne(\"abc\",10).GetChildTwo(\"append\")";
                var epl = "@name('s0') select " + subexpr + " from SupportChainTop as top";
                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionChainedParam(env, subexpr);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                TryAssertionChainedParam(env, subexpr);
                env.UndeployAll();

                // test property hosts a method
                env.CompileDeploy(
                        "@name('s0') select " +
                        "Inside.GetMyString() as val," +
                        "Inside.InsideTwo.GetMyOtherString() as val2 " +
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
                var prefix = "@name('s0') select * from SupportMarketDataBean as S0 where " +
                             typeof(SupportStaticMethodLib).Name;
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZero(S0)");
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZero(*)");
                TryAssertionStreamFunction(env, prefix + ".VolumeGreaterZeroEventBean(S0)");
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
                var textOne = "@name('s0') select Symbol, S1.GetTheString() as TheString from " +
                              "SupportMarketDataBean#keepall as S0 " +
                              "left outer join " +
                              "SupportBean#keepall as S1 on S0.Symbol=S1.TheString";
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
                    "@name('s0') select Symbol, S1.GetSimpleProperty() as Simpleprop, S1.MakeDefaultBean() as def from " +
                    "SupportMarketDataBean#keepall as S0 " +
                    "left outer join " +
                    "SupportBeanComplexProps#keepall as S1 on S0.Symbol=S1.SimpleProperty";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new[] {"Symbol", "Simpleprop"},
                    new object[] {"ACME", null});
                Assert.IsNull(theEvent.Get("def"));

                var eventComplexProps = SupportBeanComplexProps.MakeDefaultBean();
                eventComplexProps.SimpleProperty = "ACME";
                env.SendEventBean(eventComplexProps);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new[] {"Symbol", "Simpleprop"},
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
                    "@name('s0') select S0.GetVolume() as Volume, S0.GetSymbol() as Symbol, S0.GetPriceTimesVolume(2) as pvf from " +
                    "SupportMarketDataBean as S0 ";
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
                var textOne = "@name('s0') select S0.GetVolume(), S0.GetPriceTimesVolume(3) from " +
                              "SupportMarketDataBean as S0 ";
                env.CompileDeploy(textOne).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(2, type.PropertyNames.Length);
                Assert.AreEqual(typeof(long?), type.GetPropertyType("S0.GetVolume()"));
                Assert.AreEqual(typeof(double?), type.GetPropertyType("S0.GetPriceTimesVolume(3)"));

                var eventA = new SupportMarketDataBean("ACME", 4, 2L, null);
                env.SendEventBean(eventA);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"S0.GetVolume()", "S0.GetPriceTimesVolume(3)"},
                    new object[] {2L, 4d * 2L * 3d});
                env.UndeployAll();

                // try instance method that accepts EventBean
                var epl = "create schema MyTestEvent as " +
                          typeof(MyTestEvent).MaskTypeName() +
                          ";\n" +
                          "@name('s0') select " +
                          "S0.GetValueAsInt(S0, 'Id') as c0," +
                          "S0.GetValueAsInt(*, 'Id') as c1" +
                          " from MyTestEvent as S0";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                env.SendEventBean(new MyTestEvent(10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1" },
                    new object[] {10, 10});

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinStreamSelectNoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with alias
                var textOne = "@name('s0') select S0 as S0stream, S1 as S1stream from " +
                              "SupportMarketDataBean#keepall as S0, " +
                              "SupportBean#keepall as S1";

                // Attach listener to feed
                env.CompileDeploy(textOne).AddListener("s0");
                var model = env.EplToModel(textOne);
                Assert.AreEqual(textOne, model.ToEPL());

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(2, type.PropertyNames.Length);
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("S0stream"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("S1stream"));

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);

                var eventB = new SupportBean();
                env.SendEventBean(eventB);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"S0stream", "S1stream"},
                    new object[] {eventA, eventB});

                env.UndeployAll();

                // try no alias
                textOne = "@name('s0') select S0, S1 from " +
                          "SupportMarketDataBean#keepall as S0, " +
                          "SupportBean#keepall as S1";
                env.CompileDeploy(textOne).AddListener("s0");

                type = env.Statement("s0").EventType;
                Assert.AreEqual(2, type.PropertyNames.Length);
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("S0"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("S1"));

                env.SendEventBean(eventA);
                env.SendEventBean(eventB);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"S0", "S1"},
                    new object[] {eventA, eventB});

                env.UndeployAll();
            }
        }

        internal class EPLOtherPatternStreamSelectNoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with alias
                var textOne = "@name('s0') select * from pattern [every e1=SupportMarketDataBean -> e2=" +
                              "SupportBean(" +
                              typeof(SupportStaticMethodLib).MaskTypeName() +
                              ".CompareEvents(e1, e2))]";
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
                    "select S0.GetString(1,2,3) from SupportBean as S0",
                    "skip");

                TryInvalidCompile(
                    env,
                    "select S0.abc() from SupportBean as S0",
                    "Failed to validate select-clause expression 'S0.abc()': Failed to solve 'abc' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'abc': Could not find enumeration method, date-time method, instance method or property named 'abc' in class '" +
                    typeof(SupportBean).MaskTypeName() +
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
                return @event.Get(propertyName).AsInt32();
            }
        }
    }
} // end of namespace