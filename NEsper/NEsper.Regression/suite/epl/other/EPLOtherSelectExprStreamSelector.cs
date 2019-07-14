///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBeanComplexProps = com.espertech.esper.common.@internal.support.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExprStreamSelector
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalidSelectWildcardProperty());
            execs.Add(new EPLOtherInsertTransposeNestedProperty());
            execs.Add(new EPLOtherInsertFromPattern());
            execs.Add(new EPLOtherObjectModelJoinAlias());
            execs.Add(new EPLOtherNoJoinWildcardNoAlias());
            execs.Add(new EPLOtherJoinWildcardNoAlias());
            execs.Add(new EPLOtherNoJoinWildcardWithAlias());
            execs.Add(new EPLOtherJoinWildcardWithAlias());
            execs.Add(new EPLOtherNoJoinWithAliasWithProperties());
            execs.Add(new EPLOtherJoinWithAliasWithProperties());
            execs.Add(new EPLOtherNoJoinNoAliasWithProperties());
            execs.Add(new EPLOtherJoinNoAliasWithProperties());
            execs.Add(new EPLOtherAloneNoJoinNoAlias());
            execs.Add(new EPLOtherAloneNoJoinAlias());
            execs.Add(new EPLOtherAloneJoinAlias());
            execs.Add(new EPLOtherAloneJoinNoAlias());
            execs.Add(new EPLOtherInvalidSelect());
            return execs;
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

        internal class EPLOtherInvalidSelectWildcardProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select simpleProperty.* as a from SupportBeanComplexProps as s0",
                    "The property wildcard syntax must be used without column name");
            }
        }

        internal class EPLOtherInsertTransposeNestedProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOneText = "@Name('l1') insert into StreamA select nested.* from SupportBeanComplexProps as s0";
                env.CompileDeploy(stmtOneText, path).AddListener("l1");
                Assert.AreEqual(
                    typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested),
                    env.Statement("l1").EventType.UnderlyingType);

                var stmtTwoText = "@Name('l2') select nestedValue from StreamA";
                env.CompileDeploy(stmtTwoText, path).AddListener("l2");
                Assert.AreEqual(typeof(string), env.Statement("l2").EventType.GetPropertyType("nestedValue"));

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());

                Assert.AreEqual("nestedValue", env.Listener("l1").AssertOneGetNewAndReset().Get("nestedValue"));
                Assert.AreEqual("nestedValue", env.Listener("l2").AssertOneGetNewAndReset().Get("nestedValue"));

                env.UndeployAll();
                env.UndeployAll();
            }
        }

        internal class EPLOtherInsertFromPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtOneText = "@Name('l1') insert into streamA select a.* from pattern [every a=SupportBean]";
                env.CompileDeploy(stmtOneText).AddListener("l1");

                var stmtTwoText =
                    "@Name('l2') insert into streamA select a.* from pattern [every a=SupportBean where timer:within(30 sec)]";
                env.CompileDeploy(stmtTwoText).AddListener("l2");

                var eventType = env.Statement("l1").EventType;
                Assert.AreEqual(typeof(SupportBean), eventType.UnderlyingType);

                object theEvent = SendBeanEvent(env, "E1", 10);
                Assert.AreSame(theEvent, env.Listener("l2").AssertOneGetNewAndReset().Underlying);

                theEvent = SendBeanEvent(env, "E2", 10);
                Assert.AreSame(theEvent, env.Listener("l2").AssertOneGetNewAndReset().Underlying);

                var stmtThreeText =
                    "@Name('l3') insert into streamB select a.*, 'abc' as abc from pattern [every a=SupportBean where timer:within(30 sec)]";
                env.CompileDeploy(stmtThreeText);
                Assert.AreEqual(typeof(Pair<object, object>), env.Statement("l3").EventType.UnderlyingType);
                Assert.AreEqual(typeof(string), env.Statement("l3").EventType.GetPropertyType("abc"));
                Assert.AreEqual(typeof(string), env.Statement("l3").EventType.GetPropertyType("TheString"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherObjectModelJoinAlias : RegressionExecution
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

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
                Assert.AreEqual(typeof(Pair<object, object>), type.UnderlyingType);

                SendBeanEvent(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object theEvent = SendMarketEvent(env, "E1");
                var outevent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, outevent.Get("s1stream"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherNoJoinWildcardNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select *, win.* from SupportBean#length(3) as win";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.IsTrue(type.PropertyNames.Length > 15);
                Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

                object theEvent = SendBeanEvent(env, "E1", 16);
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinWildcardNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select *, s1.* from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(7, type.PropertyNames.Length);
                Assert.AreEqual(typeof(long?), type.GetPropertyType("Volume"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
                Assert.AreEqual(typeof(Pair<object, object>), type.UnderlyingType);

                object eventOne = SendBeanEvent(env, "E1", 13);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object eventTwo = SendMarketEvent(env, "E2");
                string[] fields = {"s0", "s1", "Symbol", "Volume"};
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new[] {eventOne, eventTwo, "E2", 0L});

                env.UndeployAll();
            }
        }

        internal class EPLOtherNoJoinWildcardWithAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select *, win.* as s0 from SupportBean#length(3) as win";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.IsTrue(type.PropertyNames.Length > 15);
                Assert.AreEqual(typeof(Pair<object, object>), type.UnderlyingType);
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));

                object theEvent = SendBeanEvent(env, "E1", 15);
                string[] fields = {"TheString", "IntPrimitive", "s0"};
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", 15, theEvent});

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinWildcardWithAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select *, s1.* as s1stream, s0.* as s0stream from SupportBean#length(3) as s0, " +
                    "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(4, type.PropertyNames.Length);
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
                Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);

                object eventOne = SendBeanEvent(env, "E1", 13);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object eventTwo = SendMarketEvent(env, "E2");
                string[] fields = {"s0", "s1", "s0stream", "s1stream"};
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new[] {eventOne, eventTwo, eventOne, eventTwo});

                env.UndeployAll();
            }
        }

        internal class EPLOtherNoJoinWithAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select TheString.* as s0, IntPrimitive as a, theString.* as s1, IntPrimitive as b from SupportBean#length(3) as theString";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(4, type.PropertyNames.Length);
                Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);
                Assert.AreEqual(typeof(int?), type.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));

                object theEvent = SendBeanEvent(env, "E1", 12);
                string[] fields = {"s0", "s1", "a", "b"};
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {theEvent, theEvent, 12, 12});

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinWithAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select IntPrimitive, s1.* as s1stream, theString, symbol as sym, s0.* as s0stream from SupportBean#length(3) as s0, " +
                    "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(5, type.PropertyNames.Length);
                Assert.AreEqual(typeof(int?), type.GetPropertyType("IntPrimitive"));
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
                Assert.AreEqual(typeof(string), type.GetPropertyType("sym"));
                Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);

                object eventOne = SendBeanEvent(env, "E1", 13);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object eventTwo = SendMarketEvent(env, "E2");
                string[] fields = {"IntPrimitive", "sym", "TheString", "s0stream", "s1stream"};
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new[] {13, "E2", "E1", eventOne, eventTwo});
                var theEvent = (EventBean) ((IDictionary<string, object>) received.Underlying).Get("s0stream");
                Assert.AreSame(eventOne, theEvent.Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLOtherNoJoinNoAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select IntPrimitive as a, string.*, IntPrimitive as b from SupportBean#length(3) as string";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(23, type.PropertyNames.Length);
                Assert.AreEqual(typeof(Pair<object, object>), type.UnderlyingType);
                Assert.AreEqual(typeof(int?), type.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
                Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));

                SendBeanEvent(env, "E1", 10);
                string[] fields = {"a", "TheString", "IntPrimitive", "b"};
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, "E1", 10, 10});

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoinNoAliasWithProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select IntPrimitive, s1.*, symbol as sym from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(7, type.PropertyNames.Length);
                Assert.AreEqual(typeof(int?), type.GetPropertyType("IntPrimitive"));
                Assert.AreEqual(typeof(Pair<object, object>), type.UnderlyingType);

                SendBeanEvent(env, "E1", 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object theEvent = SendMarketEvent(env, "E1");
                string[] fields = {"IntPrimitive", "sym", "Symbol"};
                var received = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    received,
                    fields,
                    new object[] {11, "E1", "E1"});
                Assert.AreSame(theEvent, ((Pair<object, object>) received.Underlying).First);

                env.UndeployAll();
            }
        }

        internal class EPLOtherAloneNoJoinNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select TheString.* from SupportBean#length(3) as theString";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.IsTrue(type.PropertyNames.Length > 10);
                Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

                object theEvent = SendBeanEvent(env, "E1");
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLOtherAloneNoJoinAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select TheString.* as s0 from SupportBean#length(3) as theString";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(1, type.PropertyNames.Length);
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
                Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);

                object theEvent = SendBeanEvent(env, "E1");
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("s0"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherAloneJoinAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select s1.* as s1 from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl);
                var testListener = new SupportUpdateListener();
                env.Statement("s0").AddListener(testListener);

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
                Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);

                SendBeanEvent(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object theEvent = SendMarketEvent(env, "E1");
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("s1"));

                env.UndeployAll();

                // reverse streams
                epl = "@Name('s0') select s0.* as szero from SupportBean#length(3) as s0, " +
                      "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("szero"));
                Assert.AreEqual(typeof(IDictionary<string, object>), type.UnderlyingType);

                SendMarketEvent(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = SendBeanEvent(env, "E1");
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("szero"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherAloneJoinNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select s1.* from SupportBean#length(3) as s0, " +
                          "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(long?), type.GetPropertyType("Volume"));
                Assert.AreEqual(typeof(SupportMarketDataBean), type.UnderlyingType);

                SendBeanEvent(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                object theEvent = SendMarketEvent(env, "E1");
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployAll();

                // reverse streams
                epl = "@Name('s0') select s0.* from SupportBean#length(3) as s0, " +
                      "SupportMarketDataBean#keepall as s1";
                env.CompileDeploy(epl).AddListener("s0");

                type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

                SendMarketEvent(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = SendBeanEvent(env, "E1");
                Assert.AreSame(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLOtherInvalidSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select TheString.* as theString, theString from SupportBean#length(3) as theString",
                    "Column name 'theString' appears more then once in select clause");

                TryInvalidCompile(
                    env,
                    "select s1.* as abc from SupportBean#length(3) as s0",
                    "Stream selector 's1.*' does not match any stream name in the from clause [");

                TryInvalidCompile(
                    env,
                    "select s0.* as abc, s0.* as abc from SupportBean#length(3) as s0",
                    "Column name 'abc' appears more then once in select clause");

                TryInvalidCompile(
                    env,
                    "select s0.*, s1.* from SupportBean#keepall as s0, SupportBean#keepall as s1",
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");
            }
        }
    }
} // end of namespace