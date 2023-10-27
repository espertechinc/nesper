///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherPatternEventProperties
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithWildcardSimplePattern(execs);
            WithWildcardOrPattern(execs);
            WithPropertiesSimplePattern(execs);
            With(PropertiesOrPattern)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithPropertiesOrPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherPropertiesOrPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertiesSimplePattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherPropertiesSimplePattern());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardOrPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWildcardOrPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardSimplePattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherWildcardSimplePattern());
            return execs;
        }

        private class EPLOtherWildcardSimplePattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupSimplePattern(env, "*");

                object theEvent = new SupportBean();
                env.SendEventBean(theEvent);

                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("a")));

                env.UndeployAll();
            }
        }

        private class EPLOtherWildcardOrPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupOrPattern(env, "*");

                object eventOne = new SupportBean();
                env.SendEventBean(eventOne);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreSame(eventOne, eventBean.Get("a"));
                        Assert.IsNull(eventBean.Get("b"));
                    });

                object eventTwo = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(eventTwo);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreSame(eventTwo, eventBean.Get("b"));
                        Assert.IsNull(eventBean.Get("a"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherPropertiesSimplePattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupSimplePattern(env, "a, a as myEvent, a.IntPrimitive as myInt, a.TheString");

                var theEvent = new SupportBean();
                theEvent.IntPrimitive = 1;
                theEvent.TheString = "test";
                env.SendEventBean(theEvent);

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreSame(theEvent, eventBean.Get("a"));
                        Assert.AreSame(theEvent, eventBean.Get("myEvent"));
                        Assert.AreEqual(1, eventBean.Get("myInt"));
                        Assert.AreEqual("test", eventBean.Get("a.TheString"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherPropertiesOrPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupOrPattern(
                    env,
                    "a, a as myAEvent, b, b as myBEvent, a.IntPrimitive as myInt, " +
                    "a.TheString, b.SimpleProperty as simple, b.indexed[0] as indexed, b.Nested.NestedValue as nestedVal");

                object theEvent = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(theEvent);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreSame(theEvent, eventBean.Get("b"));
                        Assert.AreEqual("simple", eventBean.Get("simple"));
                        Assert.AreEqual(1, eventBean.Get("indexed"));
                        Assert.AreEqual("NestedValue", eventBean.Get("NestedVal"));
                        Assert.IsNull(eventBean.Get("a"));
                        Assert.IsNull(eventBean.Get("myAEvent"));
                        Assert.IsNull(eventBean.Get("myInt"));
                        Assert.IsNull(eventBean.Get("a.TheString"));
                    });

                var eventTwo = new SupportBean();
                eventTwo.IntPrimitive = 2;
                eventTwo.TheString = "test2";
                env.SendEventBean(eventTwo);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreEqual(2, eventBean.Get("myInt"));
                        Assert.AreEqual("test2", eventBean.Get("a.TheString"));
                        Assert.IsNull(eventBean.Get("b"));
                        Assert.IsNull(eventBean.Get("myBEvent"));
                        Assert.IsNull(eventBean.Get("simple"));
                        Assert.IsNull(eventBean.Get("indexed"));
                        Assert.IsNull(eventBean.Get("NestedVal"));
                    });

                env.UndeployAll();
            }
        }

        private static void SetupSimplePattern(
            RegressionEnvironment env,
            string selectCriteria)
        {
            var stmtText = "@name('s0') select " + selectCriteria + " from pattern [a=SupportBean]";
            env.CompileDeploy(stmtText).AddListener("s0");
        }

        private static void SetupOrPattern(
            RegressionEnvironment env,
            string selectCriteria)
        {
            var stmtText = "@name('s0') select " +
                           selectCriteria +
                           " from pattern [every(a=SupportBean" +
                           " or b=SupportBeanComplexProps)]";
            env.CompileDeploy(stmtText).AddListener("s0");
        }
    }
} // end of namespace