///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethodCacheLRU : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var joinStatement = "@name('s0') select Id, P00, TheString from " +
                                "SupportBean#length(100) as S1, " +
                                " method:SupportStaticMethodInvocations.FetchObjectLog(TheString, IntPrimitive)";
            env.CompileDeploy(joinStatement).AddListener("s0");

            // set sleep off
            SupportStaticMethodInvocations.GetInvocationSizeReset();

            // The LRU cache caches per same keys
            string[] fields = {"Id", "P00", "TheString"};
            SendBeanEvent(env, "E1", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, "|E1|", "E1"});

            SendBeanEvent(env, "E2", 2);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {2, "|E2|", "E2"});

            SendBeanEvent(env, "E3", 3);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {3, "|E3|", "E3"});
            Assert.AreEqual(3, SupportStaticMethodInvocations.GetInvocationSizeReset());

            // should be cached
            SendBeanEvent(env, "E3", 3);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {3, "|E3|", "E3"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeReset());

            // should not be cached
            SendBeanEvent(env, "E4", 4);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {4, "|E4|", "E4"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeReset());

            // should be cached
            SendBeanEvent(env, "E2", 2);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {2, "|E2|", "E2"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeReset());

            // should not be cached
            SendBeanEvent(env, "E1", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, "|E1|", "E1"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeReset());

            env.UndeployAll();
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace