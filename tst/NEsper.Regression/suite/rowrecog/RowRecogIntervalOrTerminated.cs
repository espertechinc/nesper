///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;



namespace com.espertech.esper.regressionlib.suite.rowrecog
{
	public class RowRecogIntervalOrTerminated : RegressionExecution {

	    public void Run(RegressionEnvironment env) {

	        var milestone = new AtomicLong();
	        RunAssertionDocSample(env, milestone);

	        RunAssertion_A_Bstar(env, milestone, false);

	        RunAssertion_A_Bstar(env, milestone, true);

	        RunAssertion_Astar(env, milestone);

	        RunAssertion_A_Bplus(env, milestone);

	        RunAssertion_A_Bstar_or_Cstar(env, milestone);

	        RunAssertion_A_B_Cstar(env, milestone);

	        RunAssertion_A_B(env, milestone);

	        RunAssertion_A_Bstar_or_C(env, milestone);

	        RunAssertion_A_parenthesisBstar(env, milestone);
	    }

	    private void RunAssertion_A_Bstar_or_C(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        var fields = "a,b0,b1,b2,c".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, B[0].theString as b0, B[1].theString as b1, B[2].theString as b2, C.theString as c " +
	                   " pattern (A (B* | C))" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like 'A%'," +
	                   " B as B.theString like 'B%'," +
	                   " C as C.theString like 'C%'" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("C1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A1", null, null, null, "C1"});

	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A2", null, null, null, null});

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("X1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A2", "B1", "B2", null, null});

	        env.SendEventBean(new SupportRecogBean("A3"));
	        SendTimer(env, 10000);
	        env.AssertPropsNew("s0", fields, new object[]{"A3", null, null, null, null});

	        SendTimer(env, int.MaxValue);
	        env.AssertListenerNotInvoked("s0");

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_A_B(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        // the interval is not effective
	        var fields = "a,b".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, B.theString as b" +
	                   " pattern (A B)" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like 'A%'," +
	                   " B as B.theString like 'B%'" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A1", "B1"});

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.SendEventBean(new SupportRecogBean("A3"));
	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertPropsNew("s0", fields, new object[]{"A3", "B2"});

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertionDocSample(RegressionEnvironment env, AtomicLong milestone) {
	        SendTimer(env, 0);

	        var fields = "a_id,count_b,first_b,last_b".SplitCsv();
	        var text = "@name('s0') select * from TemperatureSensorEvent\n" +
	                   "match_recognize (\n" +
	                   "  partition by device\n" +
	                   "  measures A.id as a_id, count(B.id) as count_b, first(B.id) as first_b, last(B.id) as last_b\n" +
	                   "  pattern (A B*)\n" +
	                   "  interval 5 seconds or terminated\n" +
	                   "  define\n" +
	                   "    A as A.temp > 100,\n" +
	                   "    B as B.temp > 100)";

	        env.CompileDeploy(text).AddListener("s0");

	        SendTemperatureEvent(env, "E1", 1, 98);

	        env.MilestoneInc(milestone);

	        SendTemperatureEvent(env, "E2", 1, 101);
	        SendTemperatureEvent(env, "E3", 1, 102);
	        SendTemperatureEvent(env, "E4", 1, 101);   // falls below
	        env.AssertListenerNotInvoked("s0");

	        SendTemperatureEvent(env, "E5", 1, 100);   // falls below
	        env.AssertPropsNew("s0", fields, new object[]{"E2", 2L, "E3", "E4"});

	        env.MilestoneInc(milestone);

	        SendTimer(env, int.MaxValue);
	        env.AssertListenerNotInvoked("s0");

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_A_B_Cstar(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        var fields = "a,b,c0,c1,c2".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, B.theString as b, " +
	                   "C[0].theString as c0, C[1].theString as c1, C[2].theString as c2 " +
	                   " pattern (A B C*)" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like 'A%'," +
	                   " B as B.theString like 'B%'," +
	                   " C as C.theString like 'C%'" +
	                   ")";
	        env.CompileDeploy(text).AddListener("s0");

	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.SendEventBean(new SupportRecogBean("C1"));
	        env.SendEventBean(new SupportRecogBean("C2"));
	        env.AssertListenerNotInvoked("s0");

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertPropsNew("s0", fields, new object[]{"A1", "B1", "C1", "C2", null});

	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.SendEventBean(new SupportRecogBean("X1"));
	        env.SendEventBean(new SupportRecogBean("B3"));
	        env.SendEventBean(new SupportRecogBean("X2"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportRecogBean("A3"));
	        env.SendEventBean(new SupportRecogBean("B4"));
	        env.SendEventBean(new SupportRecogBean("X3"));
	        env.AssertPropsNew("s0", fields, new object[]{"A3", "B4", null, null, null});

	        env.MilestoneInc(milestone);

	        SendTimer(env, 20000);
	        env.SendEventBean(new SupportRecogBean("A4"));
	        env.SendEventBean(new SupportRecogBean("B5"));
	        env.SendEventBean(new SupportRecogBean("C3"));
	        env.AssertListenerNotInvoked("s0");

	        env.MilestoneInc(milestone);

	        SendTimer(env, 30000);
	        env.AssertPropsNew("s0", fields, new object[]{"A4", "B5", "C3", null, null});

	        SendTimer(env, int.MaxValue);
	        env.AssertListenerNotInvoked("s0");

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_A_Bstar_or_Cstar(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        var fields = "a,b0,b1,c0,c1".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, " +
	                   "B[0].theString as b0, B[1].theString as b1, " +
	                   "C[0].theString as c0, C[1].theString as c1 " +
	                   " pattern (A (B* | C*))" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like 'A%'," +
	                   " B as B.theString like 'B%'," +
	                   " C as C.theString like 'C%'" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("X1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A1", null, null, null, null});

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.SendEventBean(new SupportRecogBean("C1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A2", null, null, null, null});

	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.AssertPropsPerRowLastNew("s0", fields,
	            new object[][]{new object[]{"A2", null, null, "C1", null}});

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("C2"));
	        env.AssertListenerNotInvoked("s0");

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_A_Bplus(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        var fields = "a,b0,b1,b2".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, B[0].theString as b0, B[1].theString as b1, B[2].theString as b2" +
	                   " pattern (A B+)" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like 'A%'," +
	                   " B as B.theString like 'B%'" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("X1"));

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("X2"));
	        env.AssertPropsNew("s0", fields, new object[]{"A2", "B2", null, null});

	        env.SendEventBean(new SupportRecogBean("A3"));
	        env.SendEventBean(new SupportRecogBean("A4"));

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportRecogBean("B3"));
	        env.SendEventBean(new SupportRecogBean("B4"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("X3", -1));
	        env.AssertPropsNew("s0", fields, new object[]{"A4", "B3", "B4", null});

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_Astar(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        var fields = "a0,a1,a2,a3,a4".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A[0].theString as a0, A[1].theString as a1, A[2].theString as a2, A[3].theString as a3, A[4].theString as a4" +
	                   " pattern (A*)" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as theString like 'A%'" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A1", "A2", null, null, null});

	        env.MilestoneInc(milestone);

	        SendTimer(env, 2000);
	        env.SendEventBean(new SupportRecogBean("A3"));
	        env.SendEventBean(new SupportRecogBean("A4"));
	        env.SendEventBean(new SupportRecogBean("A5"));
	        env.AssertListenerNotInvoked("s0");
	        SendTimer(env, 12000);
	        env.AssertPropsNew("s0", fields, new object[]{"A3", "A4", "A5", null, null});

	        env.SendEventBean(new SupportRecogBean("A6"));
	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertPropsNew("s0", fields, new object[]{"A3", "A4", "A5", "A6", null});
	        env.SendEventBean(new SupportRecogBean("B3"));
	        env.AssertListenerNotInvoked("s0");

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_A_Bstar(RegressionEnvironment env, AtomicLong milestone, bool allMatches) {

	        SendTimer(env, 0);

	        var fields = "a,b0,b1,b2".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, B[0].theString as b0, B[1].theString as b1, B[2].theString as b2" +
	                   (allMatches ? " all matches" : "") +
	                   " pattern (A B*)" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like \"A%\"," +
	                   " B as B.theString like \"B%\"" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        // test output by terminated because of misfit event
	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("X1"));
	        if (!allMatches) {
	            env.AssertPropsNew("s0", fields, new object[]{"A1", "B1", null, null});
	        } else {
	            env.AssertListener("s0", listener => {
	                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields,
	                    new object[][]{new object[]{"A1", "B1", null, null}, new object[]{"A1", null, null, null}});
	            });
	        }

	        env.MilestoneInc(milestone);

	        SendTimer(env, 20000);
	        env.AssertListenerNotInvoked("s0");

	        // test output by timer expiry
	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertListenerNotInvoked("s0");
	        SendTimer(env, 29999);

	        SendTimer(env, 30000);
	        if (!allMatches) {
	            env.AssertPropsNew("s0", fields, new object[]{"A2", "B2", null, null});
	        } else {
	            env.AssertListener("s0", listener => {
	                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields,
	                    new object[][]{new object[]{"A2", "B2", null, null}, new object[]{"A2", null, null, null}});
	            });
	        }

	        // destroy
	        env.UndeployAll();
	    }

	    private void RunAssertion_A_parenthesisBstar(RegressionEnvironment env, AtomicLong milestone) {

	        SendTimer(env, 0);

	        var fields = "a,b0,b1,b2".SplitCsv();
	        var text = "@name('s0') select * from SupportRecogBean#keepall " +
	                   "match_recognize (" +
	                   " measures A.theString as a, B[0].theString as b0, B[1].theString as b1, B[2].theString as b2" +
	                   " pattern (A (B)*)" +
	                   " interval 10 seconds or terminated" +
	                   " define" +
	                   " A as A.theString like \"A%\"," +
	                   " B as B.theString like \"B%\"" +
	                   ")";

	        env.CompileDeploy(text).AddListener("s0");

	        // test output by terminated because of misfit event
	        env.SendEventBean(new SupportRecogBean("A1"));
	        env.SendEventBean(new SupportRecogBean("B1"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportRecogBean("X1"));
	        env.AssertPropsNew("s0", fields, new object[]{"A1", "B1", null, null});

	        env.MilestoneInc(milestone);

	        SendTimer(env, 20000);
	        env.AssertListenerNotInvoked("s0");

	        // test output by timer expiry
	        env.SendEventBean(new SupportRecogBean("A2"));
	        env.SendEventBean(new SupportRecogBean("B2"));
	        env.AssertListenerNotInvoked("s0");
	        SendTimer(env, 29999);

	        SendTimer(env, 30000);
	        env.AssertPropsNew("s0", fields, new object[]{"A2", "B2", null, null});

	        // destroy
	        env.UndeployAll();
	    }

	    private void SendTemperatureEvent(RegressionEnvironment env, string id, int device, double temp) {
	        env.SendEventObjectArray(new object[]{id, device, temp}, "TemperatureSensorEvent");
	    }

	    private void SendTimer(RegressionEnvironment env, long time) {
	        env.AdvanceTime(time);
	    }
	}
} // end of namespace
