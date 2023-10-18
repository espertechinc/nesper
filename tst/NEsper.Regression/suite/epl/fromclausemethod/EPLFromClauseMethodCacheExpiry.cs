///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

// assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
	public class EPLFromClauseMethodCacheExpiry : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        var joinStatement = "@name('s0') select id, p00, theString from " +
	                            "SupportBean#length(100) as s1, " +
	                            " method:SupportStaticMethodInvocations.fetchObjectLog(theString, intPrimitive)";
	        env.CompileDeploy(joinStatement).AddListener("s0");

	        // set sleep off
	        SupportStaticMethodInvocations.GetInvocationSizeReset();

	        SendTimer(env, 1000);
	        var fields = new string[]{"id", "p00", "theString"};
	        SendBeanEvent(env, "E1", 1);
	        env.AssertPropsNew("s0", fields, new object[]{1, "|E1|", "E1"});

	        SendTimer(env, 1500);
	        SendBeanEvent(env, "E2", 2);
	        env.AssertPropsNew("s0", fields, new object[]{2, "|E2|", "E2"});

	        SendTimer(env, 2000);
	        SendBeanEvent(env, "E3", 3);
	        env.AssertPropsNew("s0", fields, new object[]{3, "|E3|", "E3"});
	        env.AssertThat(() => Assert.AreEqual(3, SupportStaticMethodInvocations.GetInvocationSizeReset()));

	        // should be cached
	        SendBeanEvent(env, "E3", 3);
	        env.AssertPropsNew("s0", fields, new object[]{3, "|E3|", "E3"});
	        env.AssertThat(() => Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeReset()));

	        SendTimer(env, 2100);
	        // should not be cached
	        SendBeanEvent(env, "E4", 4);
	        env.AssertPropsNew("s0", fields, new object[]{4, "|E4|", "E4"});
	        env.AssertThat(() => Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeReset()));

	        // should be cached
	        SendBeanEvent(env, "E2", 2);
	        env.AssertPropsNew("s0", fields, new object[]{2, "|E2|", "E2"});
	        env.AssertThat(() => Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeReset()));

	        // should not be cached
	        SendBeanEvent(env, "E1", 1);
	        env.AssertPropsNew("s0", fields, new object[]{1, "|E1|", "E1"});
	        env.AssertThat(() => Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeReset()));

	        env.UndeployAll();
	    }

	    private static void SendTimer(RegressionEnvironment env, long timeInMSec) {
	        env.AdvanceTime(timeInMSec);
	    }

	    private static void SendBeanEvent(RegressionEnvironment env, string theString, int intPrimitive) {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        env.SendEventBean(bean);
	    }
	}
} // end of namespace
