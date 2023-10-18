///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
	public class EPLOuterInnerJoin3Stream {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLJoinFullJoinVariantThree());
	        execs.Add(new EPLJoinFullJoinVariantTwo());
	        execs.Add(new EPLJoinFullJoinVariantOne());
	        execs.Add(new EPLJoinLeftJoinVariantThree());
	        execs.Add(new EPLJoinLeftJoinVariantTwo());
	        execs.Add(new EPLJoinRightJoinVariantOne());
	        return execs;
	    }

	    private class EPLJoinFullJoinVariantThree : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select * from " +
	                                "SupportBean_S1#keepall as s1 inner join " +
	                                "SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
	                                "full outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10";

	            TryAssertionFull(env, joinStatement);
	        }
	    }

	    private class EPLJoinFullJoinVariantTwo : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select * from " +
	                                "SupportBean_S2#length(1000) as s2 " +
	                                "inner join " + "SupportBean_S1#keepall as s1 on s1.p10 = s2.p20" +
	                                " full outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10";

	            TryAssertionFull(env, joinStatement);
	        }
	    }

	    private class EPLJoinFullJoinVariantOne : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select * from " +
	                                "SupportBean_S0#length(1000) as s0 " +
	                                "full outer join " + "SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10" +
	                                " inner join " + "SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20";

	            TryAssertionFull(env, joinStatement);
	        }
	    }

	    private class EPLJoinLeftJoinVariantThree : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select * from " +
	                                "SupportBean_S1#keepall as s1 left outer join " +
	                                "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
	                                "inner join " + "SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20";

	            TryAssertionFull(env, joinStatement);
	        }
	    }

	    private class EPLJoinLeftJoinVariantTwo : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select * from " +
	                                "SupportBean_S2#length(1000) as s2 " +
	                                "inner join " + "SupportBean_S1#keepall as s1 on s1.p10 = s2.p20" +
	                                " left outer join " + "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10";

	            TryAssertionFull(env, joinStatement);
	        }
	    }

	    private class EPLJoinRightJoinVariantOne : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select * from " +
	                                "SupportBean_S0#length(1000) as s0 " +
	                                "right outer join " + "SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10" +
	                                " inner join " + "SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20";

	            TryAssertionFull(env, joinStatement);
	        }
	    }

	    private static void TryAssertionFull(RegressionEnvironment env, string expression) {
	        var fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20".SplitCsv();

	        env.EplToModelCompileDeploy(expression).AddListener("s0");

	        // s1, s2, s0
	        env.SendEventBean(new SupportBean_S1(100, "A_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(200, "A_1"));
	        env.AssertPropsNew("s0", fields, new object[]{null, null, 100, "A_1", 200, "A_1"});

	        env.SendEventBean(new SupportBean_S0(0, "A_1"));
	        env.AssertPropsNew("s0", fields, new object[]{0, "A_1", 100, "A_1", 200, "A_1"});

	        // s1, s0, s2
	        env.SendEventBean(new SupportBean_S1(103, "D_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(203, "D_1"));
	        env.AssertPropsNew("s0", fields, new object[]{null, null, 103, "D_1", 203, "D_1"});

	        env.SendEventBean(new SupportBean_S0(3, "D_1"));
	        env.AssertPropsNew("s0", fields, new object[]{3, "D_1", 103, "D_1", 203, "D_1"});

	        // s2, s1, s0
	        env.SendEventBean(new SupportBean_S2(201, "B_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S1(101, "B_1"));
	        env.AssertPropsNew("s0", fields, new object[]{null, null, 101, "B_1", 201, "B_1"});

	        env.SendEventBean(new SupportBean_S0(1, "B_1"));
	        env.AssertPropsNew("s0", fields, new object[]{1, "B_1", 101, "B_1", 201, "B_1"});

	        // s2, s0, s1
	        env.SendEventBean(new SupportBean_S2(202, "C_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S0(2, "C_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S1(102, "C_1"));
	        env.AssertPropsNew("s0", fields, new object[]{2, "C_1", 102, "C_1", 202, "C_1"});

	        // s0, s1, s2
	        env.SendEventBean(new SupportBean_S0(4, "E_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S1(104, "E_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(204, "E_1"));
	        env.AssertPropsNew("s0", fields, new object[]{4, "E_1", 104, "E_1", 204, "E_1"});

	        // s0, s2, s1
	        env.SendEventBean(new SupportBean_S0(5, "F_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(205, "F_1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S1(105, "F_1"));
	        env.AssertPropsNew("s0", fields, new object[]{5, "F_1", 105, "F_1", 205, "F_1"});

	        env.UndeployAll();
	    }
	}
} // end of namespace
