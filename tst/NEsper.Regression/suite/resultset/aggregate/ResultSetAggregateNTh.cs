///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
	public class ResultSetAggregateNTh : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        var milestone = new AtomicLong();

	        var epl = "@name('s0') select " +
	                  "theString, " +
	                  "nth(intPrimitive,0) as int1, " +  // current
	                  "nth(intPrimitive,1) as int2 " +   // one before
	                  "from SupportBean#keepall group by theString output last every 3 events order by theString";
	        env.CompileDeploy(epl).AddListener("s0");

	        RunAssertion(env, milestone);

	        env.MilestoneInc(milestone);
	        env.UndeployAll();

	        env.EplToModelCompileDeploy(epl).AddListener("s0");

	        RunAssertion(env, milestone);

	        env.UndeployAll();

	        env.TryInvalidCompile("select nth() from SupportBean",
	            "Failed to validate select-clause expression 'nth(*)': The nth aggregation function requires two parameters, an expression returning aggregation values and a numeric index constant [select nth() from SupportBean]");
	    }

	    private static void RunAssertion(RegressionEnvironment env, AtomicLong milestone) {
	        var fields = "theString,int1,int2".SplitCsv();

	        env.SendEventBean(new SupportBean("G1", 10));
	        env.SendEventBean(new SupportBean("G2", 11));
	        env.AssertListenerNotInvoked("s0");

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportBean("G1", 12));
	        env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[] {"G1", 12, 10}, new object[] {"G2", 11, null}});

	        env.SendEventBean(new SupportBean("G2", 30));
	        env.SendEventBean(new SupportBean("G2", 20));
	        env.AssertListenerNotInvoked("s0");

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportBean("G2", 25));
	        env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[] {"G2", 25, 20}});

	        env.SendEventBean(new SupportBean("G1", -1));
	        env.SendEventBean(new SupportBean("G1", -2));
	        env.AssertListenerNotInvoked("s0");

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportBean("G2", 8));
	        env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[] {"G1", -2, -1}, new object[] {"G2", 8, 25}});
	    }
	}
} // end of namespace
