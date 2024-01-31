///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean = com.espertech.esper.common.@internal.support.SupportBean;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

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

                env.AssertEventNew("s0", @event => ClassicAssert.AreSame(theEvent, @event.Get("a")));

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
                        ClassicAssert.AreSame(eventOne, eventBean.Get("a"));
                        ClassicAssert.IsNull(eventBean.Get("b"));
                    });

                object eventTwo = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(eventTwo);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        ClassicAssert.AreSame(eventTwo, eventBean.Get("b"));
                        ClassicAssert.IsNull(eventBean.Get("a"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherPropertiesSimplePattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupSimplePattern(env, "a, a as myEvent, a.IntPrimitive as MyInt, a.TheString");

                var theEvent = new SupportBean();
                theEvent.IntPrimitive = 1;
                theEvent.TheString = "test";
                env.SendEventBean(theEvent);

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        ClassicAssert.AreSame(theEvent, eventBean.Get("a"));
                        ClassicAssert.AreSame(theEvent, eventBean.Get("myEvent"));
                        ClassicAssert.AreEqual(1, eventBean.Get("MyInt"));
                        ClassicAssert.AreEqual("test", eventBean.Get("a.TheString"));
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
                    "a, a as myAEvent, b, b as myBEvent, a.IntPrimitive as MyInt, " +
                    "a.TheString, b.SimpleProperty as simple, b.Indexed[0] as Indexed, b.Nested.NestedValue as NestedValue");

                object theEvent = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(theEvent);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        ClassicAssert.AreSame(theEvent, eventBean.Get("b"));
                        ClassicAssert.AreEqual("Simple", eventBean.Get("simple"));
                        ClassicAssert.AreEqual(1, eventBean.Get("Indexed"));
                        ClassicAssert.AreEqual("NestedValue", eventBean.Get("NestedValue"));
                        ClassicAssert.IsNull(eventBean.Get("a"));
                        ClassicAssert.IsNull(eventBean.Get("myAEvent"));
                        ClassicAssert.IsNull(eventBean.Get("MyInt"));
                        ClassicAssert.IsNull(eventBean.Get("a.TheString"));
                    });

                var eventTwo = new SupportBean();
                eventTwo.IntPrimitive = 2;
                eventTwo.TheString = "test2";
                env.SendEventBean(eventTwo);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        ClassicAssert.AreEqual(2, eventBean.Get("MyInt"));
                        ClassicAssert.AreEqual("test2", eventBean.Get("a.TheString"));
                        ClassicAssert.IsNull(eventBean.Get("b"));
                        ClassicAssert.IsNull(eventBean.Get("myBEvent"));
                        ClassicAssert.IsNull(eventBean.Get("simple"));
                        ClassicAssert.IsNull(eventBean.Get("Indexed"));
                        ClassicAssert.IsNull(eventBean.Get("NestedValue"));
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