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

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
	public class RowRecogClausePresence : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        var milestone = new AtomicLong();
	        RunAssertionMeasurePresence(env, 0, "B.size()", 1, milestone);
	        RunAssertionMeasurePresence(env, 0, "100+B.size()", 101, milestone);
	        RunAssertionMeasurePresence(env, 1000000, "B.anyOf(v=>theString='E2')", true, milestone);

	        RunAssertionDefineNotPresent(env, true, milestone);
	        RunAssertionDefineNotPresent(env, false, milestone);
	    }

	    private void RunAssertionDefineNotPresent(RegressionEnvironment env, bool soda, AtomicLong milestone) {

	        var epl = "@name('s0') select * from SupportBean " +
	                  "match_recognize (" +
	                  " measures A as a, B as b" +
	                  " pattern (A B)" +
	                  ")";
	        env.CompileDeploy(soda, epl).AddListener("s0");

	        var fields = "a,b".SplitCsv();
	        var beans = new SupportBean[4];
	        for (var i = 0; i < beans.Length; i++) {
	            beans[i] = new SupportBean("E" + i, i);
	        }

	        env.SendEventBean(beans[0]);
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(beans[1]);
	        env.AssertPropsNew("s0", fields, new object[]{beans[0], beans[1]});

	        env.MilestoneInc(milestone);

	        env.SendEventBean(beans[2]);
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(beans[3]);
	        env.AssertPropsNew("s0", fields, new object[]{beans[2], beans[3]});

	        env.UndeployAll();
	    }

	    private void RunAssertionMeasurePresence(RegressionEnvironment env, long baseTime, string select, object value, AtomicLong milestone) {

	        env.AdvanceTime(baseTime);
	        var epl = "@name('s0') select * from SupportBean  " +
	                  "match_recognize (" +
	                  "    measures A as a, A.theString as id, " + select + " as val " +
	                  "    pattern (A B*) " +
	                  "    interval 1 minute " +
	                  "    define " +
	                  "        A as (A.intPrimitive=1)," +
	                  "        B as (B.intPrimitive=2))";
	        env.CompileDeploy(epl).AddListener("s0");

	        env.SendEventBean(new SupportBean("E1", 1));
	        env.SendEventBean(new SupportBean("E2", 2));

	        env.MilestoneInc(milestone);

	        env.AdvanceTimeSpan(baseTime + 60 * 1000 * 2);
	        env.AssertEqualsNew("s0", "val", value);

	        env.UndeployAll();
	    }
	}
} // end of namespace
