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

using SupportBean = com.espertech.esper.common.@internal.support.SupportBean;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherPatternEventProperties
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithWildcardSimplePattern(execs);
            WithWildcardOrPattern(execs);
            WithPropertiesSimplePattern(execs);
            WithPropertiesOrPattern(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPropertiesOrPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLOtherPropertiesOrPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertiesSimplePattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLOtherPropertiesSimplePattern());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardOrPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLOtherWildcardOrPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardSimplePattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLOtherWildcardSimplePattern());
            return execs;
        }

        private static void SetupSimplePattern(
            RegressionEnvironment env,
            string selectCriteria)
        {
            var stmtText = "@Name('s0') select " + selectCriteria + " from pattern [a=SupportBean]";
            env.CompileDeploy(stmtText).AddListener("s0");
        }

        private static void SetupOrPattern(
            RegressionEnvironment env,
            string selectCriteria)
        {
            var stmtText = "@Name('s0') select " +
                           selectCriteria +
                           " from pattern [every(a=SupportBean" +
                           " or b=SupportBeanComplexProps)]";
            env.CompileDeploy(stmtText).AddListener("s0");
        }

        internal class EPLOtherWildcardSimplePattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupSimplePattern(env, "*");

                object theEvent = new SupportBean();
                env.SendEventBean(theEvent);

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, eventBean.Get("a"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherWildcardOrPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupOrPattern(env, "*");

                object theEvent = new SupportBean();
                env.SendEventBean(theEvent);
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, eventBean.Get("a"));
                Assert.IsNull(eventBean.Get("b"));

                theEvent = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(theEvent);
                eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, eventBean.Get("b"));
                Assert.IsNull(eventBean.Get("a"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherPropertiesSimplePattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupSimplePattern(env, "a, a as myEvent, a.IntPrimitive as myInt, a.TheString");

                var theEvent = new SupportBean();
                theEvent.IntPrimitive = 1;
                theEvent.TheString = "test";
                env.SendEventBean(theEvent);

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, eventBean.Get("a"));
                Assert.AreSame(theEvent, eventBean.Get("myEvent"));
                Assert.AreEqual(1, eventBean.Get("myInt"));
                Assert.AreEqual("test", eventBean.Get("a.TheString"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherPropertiesOrPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupOrPattern(
                    env,
                    "a, a as myAEvent, b, b as myBEvent, a.IntPrimitive as myInt, " +
                    "a.TheString, b.SimpleProperty as simple, b.Indexed[0] as indexed, b.Nested.NestedValue as nestedVal");

                object theEvent = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(theEvent);
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, eventBean.Get("b"));
                Assert.AreEqual("Simple", eventBean.Get("simple"));
                Assert.AreEqual(1, eventBean.Get("indexed"));
                Assert.AreEqual("NestedValue", eventBean.Get("nestedVal"));
                Assert.IsNull(eventBean.Get("a"));
                Assert.IsNull(eventBean.Get("myAEvent"));
                Assert.IsNull(eventBean.Get("myInt"));
                Assert.IsNull(eventBean.Get("a.TheString"));

                var eventTwo = new SupportBean();
                eventTwo.IntPrimitive = 2;
                eventTwo.TheString = "test2";
                env.SendEventBean(eventTwo);
                eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(2, eventBean.Get("myInt"));
                Assert.AreEqual("test2", eventBean.Get("a.TheString"));
                Assert.IsNull(eventBean.Get("b"));
                Assert.IsNull(eventBean.Get("myBEvent"));
                Assert.IsNull(eventBean.Get("simple"));
                Assert.IsNull(eventBean.Get("indexed"));
                Assert.IsNull(eventBean.Get("nestedVal"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace