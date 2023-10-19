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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherStreamExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithChainedParameterized(execs);
            WithStreamFunction(execs);
            WithInstanceMethodOuterJoin(execs);
            WithInstanceMethodStatic(execs);
            WithStreamInstanceMethodAliased(execs);
            WithStreamInstanceMethodNoAlias(execs);
            WithJoinStreamSelectNoWildcard(execs);
            WithPatternStreamSelectNoWildcard(execs);
            With(InvalidSelect)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternStreamSelectNoWildcard(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherPatternStreamSelectNoWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinStreamSelectNoWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinStreamSelectNoWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamInstanceMethodNoAlias(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherStreamInstanceMethodNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamInstanceMethodAliased(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherStreamInstanceMethodAliased());
            return execs;
        }

        public static IList<RegressionExecution> WithInstanceMethodStatic(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInstanceMethodStatic());
            return execs;
        }

        public static IList<RegressionExecution> WithInstanceMethodOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInstanceMethodOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamFunction(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherStreamFunction());
            return execs;
        }

        public static IList<RegressionExecution> WithChainedParameterized(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherChainedParameterized());
            return execs;
        }

        private class EPLOtherChainedParameterized : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subexpr = "top.getChildOne(\"abc\",10).getChildTwo(\"append\")";
                var epl = "@name('s0') select " + subexpr + " from SupportChainTop as top";
                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionChainedParam(env, subexpr);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                TryAssertionChainedParam(env, subexpr);
                env.UndeployAll();

                // test property hosts a method
                env.CompileDeploy(
                        "@name('s0') select inside.getMyString() as val," +
                        "inside.insideTwo.getMyOtherString() as val2 " +
                        "from SupportBeanStaticOuter")
                    .AddListener("s0");

                env.SendEventBean(new SupportBeanStaticOuter());
                env.AssertEventNew(
                    "s0",
                    result => {
                        Assert.AreEqual("hello", result.Get("val"));
                        Assert.AreEqual("hello2", result.Get("val2"));
                    });
                env.UndeployAll();
            }

            private static void TryAssertionChainedParam(
                RegressionEnvironment env,
                string subexpr)
            {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var rows = new object[][] {
                            new object[] { subexpr, typeof(SupportChainChildTwo) }
                        };
                        for (var i = 0; i < rows.Length; i++) {
                            var prop = statement.EventType.PropertyDescriptors[i];
                            Assert.AreEqual(rows[i][0], prop.PropertyName);
                            Assert.AreEqual(rows[i][1], prop.PropertyType);
                        }
                    });

                env.SendEventBean(new SupportChainTop());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var result = @event.Get(subexpr);
                        Assert.AreEqual("abcappend", ((SupportChainChildTwo)result).Text);
                    });
            }
        }

        private class EPLOtherStreamFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var prefix = "@name('s0') select * from SupportMarketDataBean as s0 where " +
                             typeof(SupportStaticMethodLib).FullName;
                TryAssertionStreamFunction(env, prefix + ".volumeGreaterZero(s0)");
                TryAssertionStreamFunction(env, prefix + ".volumeGreaterZero(*)");
                TryAssertionStreamFunction(env, prefix + ".volumeGreaterZeroEventBean(s0)");
                TryAssertionStreamFunction(env, prefix + ".volumeGreaterZeroEventBean(*)");
            }

            private static void TryAssertionStreamFunction(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("ACME", 0, 0L, null));
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(new SupportMarketDataBean("ACME", 0, 100L, null));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLOtherInstanceMethodOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne = "@name('s0') select symbol, s1.getTheString() as theString from " +
                              "SupportMarketDataBean#keepall as s0 " +
                              "left outer join " +
                              "SupportBean#keepall as s1 on s0.symbol=s1.theString";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);
                env.AssertPropsNew("s0", new string[] { "symbol", "theString" }, new object[] { "ACME", null });

                env.UndeployAll();
            }
        }

        private class EPLOtherInstanceMethodStatic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne =
                    "@name('s0') select symbol, s1.getSimpleProperty() as simpleprop, s1.makeDefaultBean() as def from " +
                    "SupportMarketDataBean#keepall as s0 " +
                    "left outer join " +
                    "SupportBeanComplexProps#keepall as s1 on s0.symbol=s1.simpleProperty";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        EPAssertionUtil.AssertProps(
                            theEvent,
                            new string[] { "symbol", "simpleprop" },
                            new object[] { "ACME", null });
                        Assert.IsNull(theEvent.Get("def"));
                    });

                var eventComplexProps = SupportBeanComplexProps.MakeDefaultBean();
                eventComplexProps.SimpleProperty = "ACME";
                env.SendEventBean(eventComplexProps);
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertProps(
                            @event,
                            new string[] { "symbol", "simpleprop" },
                            new object[] { "ACME", "ACME" });
                        Assert.IsNotNull(@event.Get("def"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherStreamInstanceMethodAliased : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne =
                    "@name('s0') select s0.getVolume() as volume, s0.getSymbol() as symbol, s0.getPriceTimesVolume(2) as pvf from " +
                    "SupportMarketDataBean as s0 ";
                env.CompileDeploy(textOne).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(3, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
                        Assert.AreEqual(typeof(string), type.GetPropertyType("symbol"));
                        Assert.AreEqual(typeof(double?), type.GetPropertyType("pvf"));
                    });

                var eventA = new SupportMarketDataBean("ACME", 4, 99L, null);
                env.SendEventBean(eventA);
                env.AssertPropsNew(
                    "s0",
                    new string[] { "volume", "symbol", "pvf" },
                    new object[] { 99L, "ACME", 4d * 99L * 2 });

                env.UndeployAll();
            }
        }

        private class EPLOtherStreamInstanceMethodNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var textOne = "@name('s0') select s0.getVolume(), s0.getPriceTimesVolume(3) from " +
                              "SupportMarketDataBean as s0 ";
                env.CompileDeploy(textOne).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(2, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("s0.getVolume()"));
                        Assert.AreEqual(typeof(double?), type.GetPropertyType("s0.getPriceTimesVolume(3)"));
                    });

                var eventA = new SupportMarketDataBean("ACME", 4, 2L, null);
                env.SendEventBean(eventA);
                env.AssertPropsNew(
                    "s0",
                    new string[] { "s0.getVolume()", "s0.getPriceTimesVolume(3)" },
                    new object[] { 2L, 4d * 2L * 3d });
                env.UndeployAll();

                // try instance method that accepts EventBean
                var epl = "@buseventtype @public create schema MyTestEvent as " +
                          typeof(MyTestEvent).FullName +
                          ";\n" +
                          "@name('s0') select " +
                          "s0.getValueAsInt(s0, 'id') as c0," +
                          "s0.getValueAsInt(*, 'id') as c1" +
                          " from MyTestEvent as s0";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                env.SendEventBean(new MyTestEvent(10));
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { 10, 10 });

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinStreamSelectNoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with alias
                var textOne = "@name('s0') select s0 as s0stream, s1 as s1stream from " +
                              "SupportMarketDataBean#keepall as s0, " +
                              "SupportBean#keepall as s1";

                // Attach listener to feed
                env.CompileDeploy(textOne).AddListener("s0");
                var model = env.EplToModel(textOne);
                Assert.AreEqual(textOne, model.ToEPL());

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(2, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0stream"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1stream"));
                    });

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);

                var eventB = new SupportBean();
                env.SendEventBean(eventB);
                env.AssertPropsNew("s0", new string[] { "s0stream", "s1stream" }, new object[] { eventA, eventB });

                env.UndeployAll();

                // try no alias
                textOne = "@name('s0') select s0, s1 from " +
                          "SupportMarketDataBean#keepall as s0, " +
                          "SupportBean#keepall as s1";
                env.CompileDeploy(textOne).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(2, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));
                    });

                env.SendEventBean(eventA);
                env.SendEventBean(eventB);
                env.AssertPropsNew("s0", new string[] { "s0", "s1" }, new object[] { eventA, eventB });

                env.UndeployAll();
            }
        }

        private class EPLOtherPatternStreamSelectNoWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with alias
                var textOne = "@name('s0') select * from pattern [every e1=SupportMarketDataBean -> e2=" +
                              "SupportBean(" +
                              typeof(SupportStaticMethodLib).FullName +
                              ".compareEvents(e1, e2))]";
                env.CompileDeploy(textOne).AddListener("s0");

                var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
                env.SendEventBean(eventA);

                var eventB = new SupportBean("ACME", 1);
                env.SendEventBean(eventB);
                env.AssertPropsNew("s0", new string[] { "e1", "e2" }, new object[] { eventA, eventB });

                env.UndeployAll();
            }
        }

        private class EPLOtherInvalidSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select s0.getString(1,2,3) from SupportBean as s0",
                    "skip");

                env.TryInvalidCompile(
                    "select s0.abc() from SupportBean as s0",
                    "Failed to validate select-clause expression 's0.abc()': Failed to solve 'abc' to either an date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'abc': Could not find enumeration method, date-time method, instance method or property named 'abc' in class '" +
                    typeof(SupportBean).FullName +
                    "' taking no parameters [");

                env.TryInvalidCompile(
                    "select s.theString from pattern [every [2] s=SupportBean] ee",
                    "Failed to validate select-clause expression 's.theString': Failed to resolve property 's.theString' (property 's' is an indexed property and requires an index or enumeration method to access values)");
            }
        }

        [Serializable]
        public class MyTestEvent
        {
            private int id;

            internal MyTestEvent(int id)
            {
                this.id = id;
            }

            public int Id => id;

            public int GetValueAsInt(
                EventBean @event,
                string propertyName)
            {
                return @event.Get(propertyName).AsInt32();
            }
        }
    }
} // end of namespace