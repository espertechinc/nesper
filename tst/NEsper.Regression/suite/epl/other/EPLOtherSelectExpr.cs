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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithPrecedenceNoColumnName(execs);
            WithGraphSelect(execs);
            WithKeywordsAllowed(execs);
            WithEscapeString(execs);
            WithGetEventType(execs);
            With(WindowStats)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithWindowStats(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWindowStats());
            return execs;
        }

        public static IList<RegressionExecution> WithGetEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherGetEventType());
            return execs;
        }

        public static IList<RegressionExecution> WithEscapeString(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherEscapeString());
            return execs;
        }

        public static IList<RegressionExecution> WithKeywordsAllowed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherKeywordsAllowed());
            return execs;
        }

        public static IList<RegressionExecution> WithGraphSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherGraphSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecedenceNoColumnName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherPrecedenceNoColumnName());
            return execs;
        }

        private class EPLOtherPrecedenceNoColumnName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryPrecedenceNoColumnName(env, "3*2+1", "3*2+1", 7);
                TryPrecedenceNoColumnName(env, "(3*2)+1", "3*2+1", 7);
                TryPrecedenceNoColumnName(env, "3*(2+1)", "3*(2+1)", 9);
            }

            private static void TryPrecedenceNoColumnName(
                RegressionEnvironment env,
                string selectColumn,
                string expectedColumn,
                object value)
            {
                var epl = "@name('s0') select " + selectColumn + " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => {
                        if (!statement.EventType.PropertyNames[0].Equals(expectedColumn)) {
                            Assert.Fail(
                                "Expected '" + expectedColumn + "' but was " + statement.EventType.PropertyNames[0]);
                        }
                    });

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", expectedColumn, value);
                env.UndeployAll();
            }
        }

        private class EPLOtherGraphSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into MyStream select nested from SupportBeanComplexProps", path);
                var epl = "@name('s0') select nested.NestedValue, nested.NestedNested.NestedNestedValue from MyStream";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                env.AssertEventNew("s0", @event => { });

                env.UndeployAll();
            }
        }

        private class EPLOtherKeywordsAllowed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields =
                    "count,escape,every,sum,avg,max,min,coalesce,median,stddev,avedev,events,first,last,unidirectional,pattern,sql,metadatasql,prev,prior,weekday,lastweekday,cast,snapshot,variable,window,left,right,full,outer,join";
                env.CompileDeploy("@name('s0') select " + fields + " from SupportBeanKeywords").AddListener("s0");

                env.SendEventBean(new SupportBeanKeywords());
                env.AssertStatement(
                    "s0",
                    statement => EPAssertionUtil.AssertEqualsExactOrder(
                        statement.EventType.PropertyNames,
                        fields.SplitCsv()));

                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        var fieldsArr = fields.SplitCsv();
                        foreach (var aFieldsArr in fieldsArr) {
                            Assert.AreEqual(1, theEvent.Get(aFieldsArr));
                        }
                    });
                env.UndeployAll();

                env.CompileDeploy(
                    "@name('s0') select escape as stddev, count(*) as count, last from SupportBeanKeywords");
                env.AddListener("s0");
                env.SendEventBean(new SupportBeanKeywords());

                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(1, theEvent.Get("stddev"));
                        Assert.AreEqual(1L, theEvent.Get("count"));
                        Assert.AreEqual(1, theEvent.Get("last"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherEscapeString : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // The following EPL syntax compiles but fails to match a string "A'B", we are looking into:
                // env.compileDeploy("@name('s0') select * from SupportBean(string='A\\\'B')");

                TryEscapeMatch(env, "A'B", "\"A'B\""); // opposite quotes
                TryEscapeMatch(env, "A'B", "'A\\'B'"); // escape '
                TryEscapeMatch(env, "A'B", "'A\\u0027B'"); // unicode

                TryEscapeMatch(env, "A\"B", "'A\"B'"); // opposite quotes
                TryEscapeMatch(env, "A\"B", "'A\\\"B'"); // escape "
                TryEscapeMatch(env, "A\"B", "'A\\u0022B'"); // unicode

                env.CompileDeploy("@name('A\\\'B') @Description(\"A\\\"B\") select * from SupportBean");
                env.AssertStatement(
                    "A\'B",
                    statement => {
                        Assert.AreEqual("A\'B", statement.Name);
                        var desc =
                            (com.espertech.esper.common.client.annotation.DescriptionAttribute)statement.Annotations[1];
                        Assert.AreEqual("A\"B", desc.Value);
                    });
                env.UndeployAll();

                env.CompileDeploy(
                    "@name('s0') select 'Volume' as field1, \"sleep\" as field2, \"\\u0041\" as unicodeA from SupportBean");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertPropsNew(
                    "s0",
                    new string[] { "field1", "field2", "unicodeA" },
                    new object[] { "Volume", "sleep", "A" });
                env.UndeployAll();

                TryStatementMatch(env, "John's", "select * from SupportBean(TheString='John\\'s')");
                TryStatementMatch(env, "John's", "select * from SupportBean(TheString='John\\u0027s')");
                TryStatementMatch(
                    env,
                    "Quote \"Hello\"",
                    "select * from SupportBean(TheString like \"Quote \\\"Hello\\\"\")");
                TryStatementMatch(
                    env,
                    "Quote \"Hello\"",
                    "select * from SupportBean(TheString like \"Quote \\u0022Hello\\u0022\")");

                env.UndeployAll();
            }

            private static void TryEscapeMatch(
                RegressionEnvironment env,
                string property,
                string escaped)
            {
                var epl = "@name('s0') select * from SupportBean(TheString=" + escaped + ")";
                var text = "trying >" + escaped + "< (" + escaped.Length + " chars) EPL " + epl;
                log.Info("tryEscapeMatch for " + text);
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean(property, 1));
                env.AssertEqualsNew("s0", "IntPrimitive", 1);
                env.UndeployAll();
            }

            private static void TryStatementMatch(
                RegressionEnvironment env,
                string property,
                string epl)
            {
                var text = "trying EPL " + epl;
                log.Info("tryEscapeMatch for " + text);
                env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
                env.SendEventBean(new SupportBean(property, 1));
                env.AssertEqualsNew("s0", "IntPrimitive", 1);
                env.UndeployAll();
            }
        }

        private class EPLOtherGetEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select TheString, BoolBoxed aBool, 3*IntPrimitive, FloatBoxed+FloatPrimitive result" +
                    " from SupportBean#length(3) " +
                    " where BoolBoxed = true";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        log.Debug(".testGetEventType properties=" + CompatExtensions.Render(type.PropertyNames));
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            type.PropertyNames,
                            new string[] { "3*IntPrimitive", "TheString", "result", "aBool" });
                        Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                        Assert.AreEqual(typeof(bool?), type.GetPropertyType("aBool"));
                        Assert.AreEqual(typeof(float?), type.GetPropertyType("result"));
                        Assert.AreEqual(typeof(int?), type.GetPropertyType("3*IntPrimitive"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherWindowStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select TheString, BoolBoxed as aBool, 3*IntPrimitive, FloatBoxed+FloatPrimitive as result" +
                    " from SupportBean#length(3) " +
                    " where BoolBoxed = true";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "a", false, 0, 0, 0);
                SendEvent(env, "b", false, 0, 0, 0);
                env.AssertListener("s0", listener => Assert.IsNull(listener.LastNewData));
                SendEvent(env, "c", true, 3, 10, 20);

                env.AssertListener(
                    "s0",
                    listener => {
                        var received = listener.GetAndResetLastNewData()[0];
                        Assert.AreEqual("c", received.Get("TheString"));
                        Assert.AreEqual(true, received.Get("aBool"));
                        Assert.AreEqual(30f, received.Get("result"));
                    });

                env.UndeployAll();
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            bool b,
            int i,
            float f1,
            float f2)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.BoolBoxed = b;
            bean.IntPrimitive = i;
            bean.FloatPrimitive = f1;
            bean.FloatBoxed = f2;
            env.SendEventBean(bean);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(EPLOtherSelectExpr));
    }
} // end of namespace