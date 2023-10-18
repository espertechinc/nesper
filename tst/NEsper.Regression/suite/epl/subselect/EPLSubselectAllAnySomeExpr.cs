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


namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
	public class EPLSubselectAllAnySomeExpr {
	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLSubselectRelationalOpAll());
	        execs.Add(new EPLSubselectRelationalOpNullOrNoRows());
	        execs.Add(new EPLSubselectRelationalOpSome());
	        execs.Add(new EPLSubselectEqualsNotEqualsAll());
	        execs.Add(new EPLSubselectEqualsAnyOrSome());
	        execs.Add(new EPLSubselectEqualsInNullOrNoRows());
	        execs.Add(new EPLSubselectInvalid());
	        return execs;
	    }

	    private class EPLSubselectRelationalOpAll : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "g,ge,l,le".SplitCsv();
	            var stmtText = "@name('s0') select " +
	                           "intPrimitive > all (select intPrimitive from SupportBean(theString like \"S%\")#keepall) as g, " +
	                           "intPrimitive >= all (select intPrimitive from SupportBean(theString like \"S%\")#keepall) as ge, " +
	                           "intPrimitive < all (select intPrimitive from SupportBean(theString like \"S%\")#keepall) as l, " +
	                           "intPrimitive <= all (select intPrimitive from SupportBean(theString like \"S%\")#keepall) as le " +
	                           "from SupportBean(theString like \"E%\")";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, true, true});

	            env.SendEventBean(new SupportBean("S1", 1));

	            env.SendEventBean(new SupportBean("E2", 1));
	            env.AssertPropsNew("s0", fields, new object[]{false, true, false, true});

	            env.SendEventBean(new SupportBean("E2", 2));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, false, false});

	            env.SendEventBean(new SupportBean("S2", 2));

	            env.SendEventBean(new SupportBean("E3", 3));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, false, false});

	            env.SendEventBean(new SupportBean("E4", 2));
	            env.AssertPropsNew("s0", fields, new object[]{false, true, false, false});

	            env.SendEventBean(new SupportBean("E5", 1));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, false, true});

	            env.SendEventBean(new SupportBean("E6", 0));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, true, true});

	            env.UndeployAll();

	            env.TryInvalidCompile("select intArr > all (select intPrimitive from SupportBean#keepall) from SupportBeanArrayCollMap",
	                "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords [select intArr > all (select intPrimitive from SupportBean#keepall) from SupportBeanArrayCollMap]");

	            // test OM
	            env.EplToModelCompileDeploy(stmtText).AddListener("s0");
	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, true, true});
	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectRelationalOpNullOrNoRows : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "vall,vany".SplitCsv();
	            var stmtText = "@name('s0') select " +
	                           "intBoxed >= all (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as vall, " +
	                           "intBoxed >= any (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as vany " +
	                           " from SupportBean(theString like 'E%')";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            // subs is empty
	            // select  null >= all (select val from subs), null >= any (select val from subs)
	            SendEvent(env, "E1", null, null);
	            env.AssertPropsNew("s0", fields, new object[]{true, false});

	            // select  1 >= all (select val from subs), 1 >= any (select val from subs)
	            SendEvent(env, "E2", 1, null);
	            env.AssertPropsNew("s0", fields, new object[]{true, false});

	            // subs is {null}
	            SendEvent(env, "S1", null, null);

	            SendEvent(env, "E3", null, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, null});
	            SendEvent(env, "E4", 1, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, null});

	            // subs is {null, 1}
	            SendEvent(env, "S2", null, 1d);

	            SendEvent(env, "E5", null, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, null});
	            SendEvent(env, "E6", 1, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, true});

	            SendEvent(env, "E7", 0, null);
	            env.AssertPropsNew("s0", fields, new object[]{false, false});

	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectRelationalOpSome : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "g,ge,l,le".SplitCsv();
	            var stmtText = "@name('s0') select " +
	                           "intPrimitive > any (select intPrimitive from SupportBean(theString like 'S%')#keepall) as g, " +
	                           "intPrimitive >= any (select intPrimitive from SupportBean(theString like 'S%')#keepall) as ge, " +
	                           "intPrimitive < any (select intPrimitive from SupportBean(theString like 'S%')#keepall) as l, " +
	                           "intPrimitive <= any (select intPrimitive from SupportBean(theString like 'S%')#keepall) as le " +
	                           " from SupportBean(theString like 'E%')";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, false, false});

	            env.SendEventBean(new SupportBean("S1", 1));

	            env.SendEventBean(new SupportBean("E2", 1));
	            env.AssertPropsNew("s0", fields, new object[]{false, true, false, true});

	            env.SendEventBean(new SupportBean("E2", 2));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, false, false});

	            env.SendEventBean(new SupportBean("E2a", 0));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, true, true});

	            env.SendEventBean(new SupportBean("S2", 2));

	            env.SendEventBean(new SupportBean("E3", 3));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, false, false});

	            env.SendEventBean(new SupportBean("E4", 2));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, false, true});

	            env.SendEventBean(new SupportBean("E5", 1));
	            env.AssertPropsNew("s0", fields, new object[]{false, true, true, true});

	            env.SendEventBean(new SupportBean("E6", 0));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, true, true});

	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectEqualsNotEqualsAll : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "eq,neq,sqlneq,nneq".SplitCsv();
	            var stmtText = "@name('s0') select " +
	                           "intPrimitive=all(select intPrimitive from SupportBean(theString like 'S%')#keepall) as eq, " +
	                           "intPrimitive != all (select intPrimitive from SupportBean(theString like 'S%')#keepall) as neq, " +
	                           "intPrimitive <> all (select intPrimitive from SupportBean(theString like 'S%')#keepall) as sqlneq, " +
	                           "not intPrimitive = all (select intPrimitive from SupportBean(theString like 'S%')#keepall) as nneq " +
	                           " from SupportBean(theString like 'E%')";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, true, false});

	            env.SendEventBean(new SupportBean("S1", 11));

	            env.SendEventBean(new SupportBean("E2", 11));
	            env.AssertPropsNew("s0", fields, new object[]{true, false, false, false});

	            env.SendEventBean(new SupportBean("E3", 10));
	            env.AssertPropsNew("s0", fields, new object[]{false, true, true, true});

	            env.SendEventBean(new SupportBean("S1", 12));

	            env.SendEventBean(new SupportBean("E4", 11));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, false, true});

	            env.SendEventBean(new SupportBean("E5", 14));
	            env.AssertPropsNew("s0", fields, new object[]{false, true, true, true});

	            env.UndeployAll();
	        }

	    }    // Test "value = SOME (subselect)" which is the same as "value IN (subselect)"

	    private class EPLSubselectEqualsAnyOrSome : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "r1,r2,r3,r4".SplitCsv();
	            var stmtText = "@name('s0') select " +
	                           "intPrimitive = SOME (select intPrimitive from SupportBean(theString like 'S%')#keepall) as r1, " +
	                           "intPrimitive = ANY (select intPrimitive from SupportBean(theString like 'S%')#keepall) as r2, " +
	                           "intPrimitive != SOME (select intPrimitive from SupportBean(theString like 'S%')#keepall) as r3, " +
	                           "intPrimitive <> ANY (select intPrimitive from SupportBean(theString like 'S%')#keepall) as r4 " +
	                           "from SupportBean(theString like 'E%')";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, false, false});

	            env.SendEventBean(new SupportBean("S1", 11));
	            env.SendEventBean(new SupportBean("E2", 11));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, false, false});

	            env.SendEventBean(new SupportBean("E3", 12));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, true, true});

	            env.SendEventBean(new SupportBean("S2", 12));
	            env.SendEventBean(new SupportBean("E4", 12));
	            env.AssertPropsNew("s0", fields, new object[]{true, true, true, true});

	            env.SendEventBean(new SupportBean("E5", 13));
	            env.AssertPropsNew("s0", fields, new object[]{false, false, true, true});

	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectEqualsInNullOrNoRows : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "eall,eany,neall,neany,isin".SplitCsv();
	            var stmtText = "@name('s0') select " +
	                           "intBoxed = all (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as eall, " +
	                           "intBoxed = any (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as eany, " +
	                           "intBoxed != all (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as neall, " +
	                           "intBoxed != any (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as neany, " +
	                           "intBoxed in (select doubleBoxed from SupportBean(theString like 'S%')#keepall) as isin " +
	                           " from SupportBean(theString like 'E%')";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            // subs is empty
	            // select  null = all (select val from subs), null = any (select val from subs), null != all (select val from subs), null != any (select val from subs), null in (select val from subs)
	            SendEvent(env, "E1", null, null);
	            env.AssertPropsNew("s0", fields, new object[]{true, false, true, false, false});

	            // select  1 = all (select val from subs), 1 = any (select val from subs), 1 != all (select val from subs), 1 != any (select val from subs), 1 in (select val from subs)
	            SendEvent(env, "E2", 1, null);
	            env.AssertPropsNew("s0", fields, new object[]{true, false, true, false, false});

	            // subs is {null}
	            SendEvent(env, "S1", null, null);

	            SendEvent(env, "E3", null, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, null, null, null, null});
	            SendEvent(env, "E4", 1, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, null, null, null, null});

	            // subs is {null, 1}
	            SendEvent(env, "S2", null, 1d);

	            SendEvent(env, "E5", null, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, null, null, null, null});
	            SendEvent(env, "E6", 1, null);
	            env.AssertPropsNew("s0", fields, new object[]{null, true, false, null, true});
	            SendEvent(env, "E7", 0, null);
	            env.AssertPropsNew("s0", fields, new object[]{false, null, null, true, null});

	            env.UndeployAll();
	        }
	    }

	    private class EPLSubselectInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.TryInvalidCompile(
	                "select intArr = all (select intPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap",
	                "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords [select intArr = all (select intPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap]");
	        }
	    }

	    private static void SendEvent(RegressionEnvironment env, string theString, int? intBoxed, double? doubleBoxed) {
	        var bean = new SupportBean(theString, -1);
	        bean.IntBoxed = intBoxed;
	        bean.DoubleBoxed = doubleBoxed;
	        env.SendEventBean(bean);
	    }
	}
} // end of namespace
