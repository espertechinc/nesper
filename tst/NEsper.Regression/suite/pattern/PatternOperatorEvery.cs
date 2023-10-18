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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework; // assertFalse

namespace com.espertech.esper.regressionlib.suite.pattern
{
	public class PatternOperatorEvery {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new PatternEverySimple());
	        execs.Add(new PatternOp());
	        execs.Add(new PatternEveryWithAnd());
	        execs.Add(new PatternEveryFollowedByWithin());
	        execs.Add(new PatternEveryAndNot());
	        execs.Add(new PatternEveryFollowedBy());
	        return execs;
	    }

	    public class PatternEverySimple : RegressionExecution {

	        public void Run(RegressionEnvironment env) {

	            var fields = "c0".SplitCsv();

	            var epl = "@name('s0') select a.theString as c0 from pattern [every a=SupportBean]";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendSupportBean(env, "E1", 0);
	            env.AssertPropsNew("s0", fields, new object[]{"E1"});

	            env.Milestone(1);

	            SendSupportBean(env, "E2", 0);
	            env.AssertPropsNew("s0", fields, new object[]{"E2"});

	            env.Milestone(2);

	            SendSupportBean(env, "E3", 0);
	            env.AssertPropsNew("s0", fields, new object[]{"E3"});

	            env.Milestone(3);

	            SendSupportBean(env, "E4", 0);
	            env.AssertPropsNew("s0", fields, new object[]{"E4"});

	            var listener = env.Listener("s0");
	            env.UndeployModuleContaining("s0");

	            SendSupportBean(env, "E5", 0);
	            Assert.IsFalse(listener.IsInvoked);

	            env.Milestone(4);

	            SendSupportBean(env, "E6", 0);
	            Assert.IsFalse(listener.IsInvoked);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.OBSERVEROPS);
	        }
	    }

	    public class PatternEveryFollowedBy : RegressionExecution {

	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2".SplitCsv();

	            var epl = "@name('s0') select a.theString as c0, a.intPrimitive as c1, b.intPrimitive as c2 " +
	                      "from pattern [every a=SupportBean -> b=SupportBean(theString=a.theString)]";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(0);

	            SendSupportBean(env, "E1", 1);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(1);

	            SendSupportBean(env, "E2", 10);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(2);

	            SendSupportBean(env, "E1", 2);
	            env.AssertPropsNew("s0", fields, new object[]{"E1", 1, 2});

	            env.Milestone(3);

	            env.UndeployAll();
	        }
	    }

	    public class PatternEveryFollowedByWithin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var fields = "c0,c1,c2".SplitCsv();

	            env.AdvanceTime(0);
	            var epl = "@name('s0') select a.theString as c0, a.intPrimitive as c1, b.intPrimitive as c2 " +
	                      "from pattern [every a=SupportBean -> b=SupportBean(theString=a.theString) where timer:within(10 sec)]";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(0);

	            env.AdvanceTime(5000);
	            SendSupportBean(env, "E1", 1);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(1);

	            env.AdvanceTime(8000);
	            SendSupportBean(env, "E2", 10);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(2);

	            env.AdvanceTime(15000);   // expires E1 subexpression
	            env.Milestone(3);

	            SendSupportBean(env, "E1", 2);
	            env.AssertListenerNotInvoked("s0");

	            SendSupportBean(env, "E2", 11);
	            env.AssertPropsNew("s0", fields, new object[]{"E2", 10, 11});

	            env.UndeployAll();
	        }
	    }

	    public class PatternEveryWithAnd : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var fields = "c0,c1".SplitCsv();

	            var epl = "@name('s0') select a.theString as c0, b.theString as c1 from pattern [every (a=SupportBean(intPrimitive>0) and b=SupportBean(intPrimitive<0))]";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(0);

	            SendSupportBean(env, "E1", 1);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(1);

	            SendSupportBean(env, "E2", 1);
	            env.AssertListenerNotInvoked("s0");
	            SendSupportBean(env, "E3", -1);
	            env.AssertPropsNew("s0", fields, new object[]{"E1", "E3"});

	            env.Milestone(2);

	            SendSupportBean(env, "E4", -2);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(3);

	            SendSupportBean(env, "E5", 2);
	            env.AssertPropsNew("s0", fields, new object[]{"E5", "E4"});

	            env.UndeployAll();
	        }
	    }

	    private class PatternOp : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var events = EventCollectionFactory.GetEventSetOne(0, 1000);
	            var testCaseList = new CaseList();
	            EventExpressionCase testCase;

	            testCase = new EventExpressionCase("every b=SupportBean_B");
	            testCase.Add("B1", "b", events.GetEvent("B1"));
	            testCase.Add("B2", "b", events.GetEvent("B2"));
	            testCase.Add("B3", "b", events.GetEvent("B3"));
	            testCaseList.AddTest(testCase);

	            testCase = new EventExpressionCase("b=SupportBean_B");
	            testCase.Add("B1", "b", events.GetEvent("B1"));
	            testCaseList.AddTest(testCase);

	            testCase = new EventExpressionCase("every (every (every b=SupportBean_B))");
	            testCase.Add("B1", "b", events.GetEvent("B1"));
	            for (var i = 0; i < 3; i++) {
	                testCase.Add("B2", "b", events.GetEvent("B2"));
	            }
	            for (var i = 0; i < 9; i++) {
	                testCase.Add("B3", "b", events.GetEvent("B3"));
	            }
	            testCaseList.AddTest(testCase);

	            testCase = new EventExpressionCase("every (every b=SupportBean_B())");
	            testCase.Add("B1", "b", events.GetEvent("B1"));
	            testCase.Add("B2", "b", events.GetEvent("B2"));
	            testCase.Add("B2", "b", events.GetEvent("B2"));
	            for (var i = 0; i < 4; i++) {
	                testCase.Add("B3", "b", events.GetEvent("B3"));
	            }
	            testCaseList.AddTest(testCase);

	            testCase = new EventExpressionCase("every( every (every (every b=SupportBean_B())))");
	            testCase.Add("B1", "b", events.GetEvent("B1"));
	            for (var i = 0; i < 4; i++) {
	                testCase.Add("B2", "b", events.GetEvent("B2"));
	            }
	            for (var i = 0; i < 16; i++) {
	                testCase.Add("B3", "b", events.GetEvent("B3"));
	            }
	            testCaseList.AddTest(testCase);

	            var util = new PatternTestHarness(events, testCaseList);
	            util.RunTest(env);
	        }
	    }

	    private class PatternEveryAndNot : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SendTimer(env, 0);
	            var expression = "@name('s0') select 'No event within 6 seconds' as alert\n" +
	                             "from pattern [ every (timer:interval(6) and not SupportBean)]";
	            env.CompileDeploy(expression).AddListener("s0");

	            SendTimer(env, 2000);
	            env.SendEventBean(new SupportBean());

	            SendTimer(env, 6000);
	            SendTimer(env, 7000);
	            SendTimer(env, 7999);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(0);

	            SendTimer(env, 8000);
	            env.AssertEqualsNew("s0", "alert", "No event within 6 seconds");

	            SendTimer(env, 12000);
	            env.SendEventBean(new SupportBean());

	            env.Milestone(1);

	            SendTimer(env, 13000);
	            env.SendEventBean(new SupportBean());

	            SendTimer(env, 18999);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(2);

	            SendTimer(env, 19000);
	            env.AssertEqualsNew("s0", "alert", "No event within 6 seconds");

	            env.UndeployAll();
	        }

	        private static void SendTimer(RegressionEnvironment env, long timeInMSec) {
	            env.AdvanceTime(timeInMSec);
	        }
	    }

	    private static void SendSupportBean(RegressionEnvironment env, string theString, int intPrimitive) {
	        env.SendEventBean(new SupportBean(theString, intPrimitive));
	    }
	}
} // end of namespace
