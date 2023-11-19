///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExprStreamSelector
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithInvalidSelectWildcardProperty(execs);
            WithInsertTransposeNestedProperty(execs);
            WithInsertFromPattern(execs);
            WithObjectModelJoinAlias(execs);
            WithNoJoinWildcardNoAlias(execs);
            WithJoinWildcardNoAlias(execs);
            WithNoJoinWildcardWithAlias(execs);
            WithJoinWildcardWithAlias(execs);
            WithNoJoinWithAliasWithProperties(execs);
            WithJoinWithAliasWithProperties(execs);
            WithNoJoinNoAliasWithProperties(execs);
            WithJoinNoAliasWithProperties(execs);
            WithAloneNoJoinNoAlias(execs);
            WithAloneNoJoinAlias(execs);
            WithAloneJoinAlias(execs);
            WithAloneJoinNoAlias(execs);
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

        public static IList<RegressionExecution> WithAloneJoinNoAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherAloneJoinNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithAloneJoinAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherAloneJoinAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithAloneNoJoinAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherAloneNoJoinAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithAloneNoJoinNoAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherAloneNoJoinNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinNoAliasWithProperties(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinNoAliasWithProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinNoAliasWithProperties(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherNoJoinNoAliasWithProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinWithAliasWithProperties(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinWithAliasWithProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinWithAliasWithProperties(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherNoJoinWithAliasWithProperties());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinWildcardWithAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinWildcardWithAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinWildcardWithAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherNoJoinWildcardWithAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinWildcardNoAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherJoinWildcardNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinWildcardNoAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherNoJoinWildcardNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectModelJoinAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherObjectModelJoinAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertFromPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInsertFromPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertTransposeNestedProperty(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInsertTransposeNestedProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidSelectWildcardProperty(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidSelectWildcardProperty());
            return execs;
        }

        private class EPLOtherInvalidSelectWildcardProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select SimpleProperty.* as a from SupportBeanComplexProps as s0",
                    "The property wildcard syntax must be used without column name");
            }
        }

        private class EPLOtherInsertTransposeNestedProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOneText =
                    "@name('l1') @public insert into StreamA select Nested.* from SupportBeanComplexProps as s0";
                env.CompileDeploy(stmtOneText, path).AddListener("l1");
                env.AssertStatement(
                    "l1",
                    statement => Assert.AreEqual(
                        typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested),
                        statement.EventType.UnderlyingType));

                var stmtTwoText = "@name('l2') select NestedValue from StreamA";
                env.CompileDeploy(stmtTwoText, path).AddListener("l2");
                env.AssertStatement(
                    "l2",
                    statement => Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("NestedValue")));

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());

                env.AssertEqualsNew("l1", "NestedValue", "NestedValue");
                env.AssertEqualsNew("l2", "NestedValue", "NestedValue");

                env.UndeployAll();
                env.UndeployAll();
            }
        }

        private class EPLOtherInsertFromPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtOneText = "@name('l1') insert into streamA select a.* from pattern [every a=SupportBean]";
                env.CompileDeploy(stmtOneText).AddListener("l1");

                var stmtTwoText =
                    "@name('l2') insert into streamA select a.* from pattern [every a=SupportBean where timer:within(30 sec)]";
                env.CompileDeploy(stmtTwoText).AddListener("l2");

                env.AssertStatement(
                    "l1",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(SupportBean), eventType.UnderlyingType);
                    });

                object theEventOne = SendBeanEvent(env, "E1", 10);
                env.AssertEventNew("l2", @event => Assert.AreSame(theEventOne, @event.Underlying));

                object theEventTwo = SendBeanEvent(env, "E2", 10);
                env.AssertEventNew("l2", @event => Assert.AreSame(theEventTwo, @event.Underlying));

                var stmtThreeText =
                    "@name('l3') insert into streamB select a.*, 'abc' as abc from pattern [every a=SupportBean where timer:within(30 sec)]";
                env.CompileDeploy(stmtThreeText);
                env.AssertStatement(
                    "l3",
                    statement => {
                        Assert.AreEqual(
                            typeof(Pair<object, IDictionary<string, object>>),
                            statement.EventType.UnderlyingType);
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("abc"));
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("TheString"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherObjectModelJoinAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .AddStreamWildcard("s0")
                    .AddStreamWildcard("s1", "s1stream")
                    .AddWithAsProvidedName("TheString", "sym");
                model.FromClause = FromClause.Create()
                    .Add(FilterStream.Create("SupportBean", "s0").AddView("keepall"))
                    .Add(FilterStream.Create("SupportMarketDataBean", "s1").AddView("keepall"));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                var epl =
                    "@Name('s0') select s0.*, s1.* as s1stream, TheString as sym from SupportBean#keepall as s0, " +
                    "SupportMarketDataBean#keepall as s1";
                Assert.AreEqual(epl, model.ToEPL());
                var modelReverse = env.EplToModel(model.ToEPL());
                Assert.AreEqual(epl, modelReverse.ToEPL());

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
                        Assert.AreEqual(typeof(Pair<object, IDictionary<string, object>>), type.UnderlyingType);
                    });

                SendBeanEvent(env, "E1");
                env.AssertListenerNotInvoked("s0");

                object theEvent = SendMarketEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("s1stream")));

                env.UndeployAll();
            }
        }

        private class EPLOtherNoJoinWildcardNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select *, win.* from SupportBean#length(3) as win";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.IsTrue(type.PropertyNames.Length > 15);
                        Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
                    });

                object theEvent = SendBeanEvent(env, "E1", 16);
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Underlying));

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinWildcardNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select *, s1.* from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(7, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("Volume"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
                        Assert.AreEqual(typeof(Pair<object, IDictionary<string, object>>), type.UnderlyingType);
                    });

                object eventOne = SendBeanEvent(env, "E1", 13);
                env.AssertListenerNotInvoked("s0");

                object eventTwo = SendMarketEvent(env, "E2");
                var fields = new string[] { "s0", "s1", "Symbol", "Volume" };
                env.AssertPropsNew("s0", fields, new object[] { eventOne, eventTwo, "E2", 0L });

                env.UndeployAll();
            }
        }

        private class EPLOtherNoJoinWildcardWithAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select *, win.* as s0 from SupportBean#length(3) as win";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.IsTrue(type.PropertyNames.Length > 15);
                        Assert.AreEqual(typeof(Pair<object, IDictionary<string, object>>), type.UnderlyingType);
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                    });

                object theEvent = SendBeanEvent(env, "E1", 15);
                var fields = new string[] { "TheString", "IntPrimitive", "s0" };
                env.AssertPropsNew("s0", fields, new object[] { "E1", 15, theEvent });

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinWildcardWithAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select *, s1.* as s1stream, s0.* as s0stream from SupportBean#length(3) as s0, " +
                    "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(4, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                    });

                object eventOne = SendBeanEvent(env, "E1", 13);
                env.AssertListenerNotInvoked("s0");

                object eventTwo = SendMarketEvent(env, "E2");
                var fields = new string[] { "s0", "s1", "s0stream", "s1stream" };
                env.AssertPropsNew("s0", fields, new object[] { eventOne, eventTwo, eventOne, eventTwo });

                env.UndeployAll();
            }
        }

        private class EPLOtherNoJoinWithAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select TheString.* as s0, IntPrimitive as a, TheString.* as s1, IntPrimitive as b from SupportBean#length(3) as TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(4, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));
                    });

                object theEvent = SendBeanEvent(env, "E1", 12);
                var fields = new string[] { "s0", "s1", "a", "b" };
                env.AssertPropsNew("s0", fields, new object[] { theEvent, theEvent, 12, 12 });

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinWithAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select IntPrimitive, s1.* as s1stream, TheString, Symbol as sym, s0.* as s0stream from SupportBean#length(3) as s0, " +
                    "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(5, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("IntPrimitive"));
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
                        Assert.AreEqual(typeof(string), type.GetPropertyType("sym"));
                        Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                    });

                object eventOne = SendBeanEvent(env, "E1", 13);
                env.AssertListenerNotInvoked("s0");

                object eventTwo = SendMarketEvent(env, "E2");
                var fields = new string[] { "IntPrimitive", "sym", "TheString", "s0stream", "s1stream" };
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertProps(
                            @event,
                            fields,
                            new object[] { 13, "E2", "E1", eventOne, eventTwo });
                        var theEvent = (EventBean)((IDictionary<string, object>)@event.Underlying).Get("s0stream");
                        Assert.AreSame(eventOne, theEvent.Underlying);
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherNoJoinNoAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select IntPrimitive as a, string.*, IntPrimitive as b from SupportBean#length(3) as string";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(22, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(Pair<object, IDictionary<string, object>>), type.UnderlyingType);
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
                        Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                    });

                SendBeanEvent(env, "E1", 10);
                var fields = new string[] { "a", "TheString", "IntPrimitive", "b" };
                env.AssertPropsNew("s0", fields, new object[] { 10, "E1", 10, 10 });

                env.UndeployAll();
            }
        }

        private class EPLOtherJoinNoAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select IntPrimitive, s1.*, Symbol as sym from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(7, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("IntPrimitive"));
                        Assert.AreEqual(typeof(Pair<object, IDictionary<string, object>>), type.UnderlyingType);
                    });

                SendBeanEvent(env, "E1", 11);
                env.AssertListenerNotInvoked("s0");

                object theEvent = SendMarketEvent(env, "E1");
                var fields = new string[] { "IntPrimitive", "sym", "Symbol" };
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertProps(@event, fields, new object[] { 11, "E1", "E1" });
                        Assert.AreSame(theEvent, ((Pair<object, IDictionary<string, object>>)@event.Underlying).First);
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherAloneNoJoinNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select TheString.* from SupportBean#length(3) as TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.IsTrue(type.PropertyNames.Length > 10);
                        Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
                    });

                object theEvent = SendBeanEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Underlying));

                env.UndeployAll();
            }
        }

        private class EPLOtherAloneNoJoinAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select TheString.* as s0 from SupportBean#length(3) as TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(1, type.PropertyNames.Length);
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                    });

                object theEvent = SendBeanEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("s0")));

                env.UndeployAll();
            }
        }

        private class EPLOtherAloneJoinAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select s1.* as s1 from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                    });

                SendBeanEvent(env, "E1");
                env.AssertListenerNotInvoked("s0");

                object eventOne = SendMarketEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(eventOne, @event.Get("s1")));

                env.UndeployAll();

                // reverse streams
                epl = "@name('s0') select s0.* as szero from SupportBean#length(3) as s0, " +
                      "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("szero"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                    });

                SendMarketEvent(env, "E1");
                env.AssertListenerNotInvoked("s0");

                object eventTwo = SendBeanEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(eventTwo, @event.Get("szero")));

                env.UndeployAll();
            }
        }

        private class EPLOtherAloneJoinNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select s1.* from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(typeof(long?), type.GetPropertyType("Volume"));
                        Assert.AreEqual(typeof(SupportMarketDataBean), type.UnderlyingType);
                    });

                SendBeanEvent(env, "E1");
                env.AssertListenerNotInvoked("s0");

                object eventOne = SendMarketEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(eventOne, @event.Underlying));

                env.UndeployAll();

                // reverse streams
                epl = "@name('s0') select s0.* from SupportBean#length(3) as s0, " +
                      "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                        Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
                    });

                SendMarketEvent(env, "E1");
                env.AssertListenerNotInvoked("s0");

                object eventTwo = SendBeanEvent(env, "E1");
                env.AssertEventNew("s0", @event => Assert.AreSame(eventTwo, @event.Underlying));

                env.UndeployAll();
            }
        }

        private class EPLOtherInvalidSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select TheString.* as TheString, TheString from SupportBean#length(3) as TheString",
                    "Column name 'TheString' appears more then once in select clause");

                env.TryInvalidCompile(
                    "select s1.* as abc from SupportBean#length(3) as s0",
                    "Stream selector 's1.*' does not match any stream name in the from clause [");

                env.TryInvalidCompile(
                    "select s0.* as abc, s0.* as abc from SupportBean#length(3) as s0",
                    "Column name 'abc' appears more then once in select clause");

                env.TryInvalidCompile(
                    "select s0.*, s1.* from SupportBean#keepall as s0, SupportBean#keepall as s1",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");
            }
        }

        private static SupportBean SendBeanEvent(
            RegressionEnvironment env,
            string s)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean SendBeanEvent(
            RegressionEnvironment env,
            string s,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportMarketDataBean SendMarketEvent(
            RegressionEnvironment env,
            string s)
        {
            var bean = new SupportMarketDataBean(s, 0d, 0L, "");
            env.SendEventBean(bean);
            return bean;
        }
    }
} // end of namespace