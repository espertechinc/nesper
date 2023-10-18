///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
	public class ResultSetQueryTypeRowPerGroupHaving {
	    private const string SYMBOL_DELL = "DELL";
	    private const string SYMBOL_IBM = "IBM";

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ResultSetQueryTypeHavingCount());
	        execs.Add(new ResultSetQueryTypeSumJoin());
	        execs.Add(new ResultSetQueryTypeSumOneView());
	        execs.Add(new ResultSetQueryTypeRowPerGroupBatch());
	        execs.Add(new ResultSetQueryTypeRowPerGroupDefinedExpr());
	        return execs;
	    }

	    private class ResultSetQueryTypeRowPerGroupDefinedExpr : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "expression F {v -> v} select sum(intPrimitive) from SupportBean group by theString having count(*) > F(1)";
	            env.Compile(epl);

	            var eplInvalid = "expression F {v -> v} select sum(intPrimitive) from SupportBean group by theString having count(*) > F(longPrimitive)";
	            env.TryInvalidCompile(eplInvalid, "Non-aggregated property 'longPrimitive' in the HAVING clause must occur in the group-by clause");
	        }
	    }

	    private class ResultSetQueryTypeRowPerGroupBatch : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.AdvanceTime(0);
	            env.CompileDeploy("@name('s0') select count(*) as y from SupportBean#time_batch(1 seconds) group by theString having count(*) > 0");
	            env.AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 0));
	            env.AdvanceTime(1000);
	            env.AssertPropsNew("s0", "y".SplitCsv(), new object[] {1L});

	            env.SendEventBean(new SupportBean("E2", 0));
	            env.AdvanceTime(2000);
	            env.AssertPropsNew("s0", "y".SplitCsv(), new object[] {1L});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetQueryTypeHavingCount : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select * from SupportBean(intPrimitive = 3)#length(10) as e1 group by theString having count(*) > 2";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(new SupportBean("A1", 3));

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean("A1", 3));
	            env.AssertListenerNotInvoked("s0");
	            env.SendEventBean(new SupportBean("A1", 3));
	            env.AssertListenerInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetQueryTypeSumJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select irstream symbol, sum(price) as mySum " +
	                      "from SupportBeanString#length(100) as one, " +
	                      " SupportMarketDataBean#length(3) as two " +
	                      "where (symbol='DELL' or symbol='IBM' or symbol='GE')" +
	                      "       and one.theString = two.symbol " +
	                      "group by symbol " +
	                      "having sum(price) >= 100";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
	            env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
	            env.SendEventBean(new SupportBeanString("AAA"));

	            TryAssertion(env);

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetQueryTypeSumOneView : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select irstream symbol, sum(price) as mySum " +
	                      "from SupportMarketDataBean#length(3) " +
	                      "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
	                      "group by symbol " +
	                      "having sum(price) >= 100";
	            env.CompileDeploy(epl).AddListener("s0");

	            TryAssertion(env);

	            env.UndeployAll();
	        }
	    }

	    private static void TryAssertion(RegressionEnvironment env) {
	        SendEvent(env, SYMBOL_DELL, 10);
	        env.AssertListenerNotInvoked("s0");

	        SendEvent(env, SYMBOL_DELL, 60);
	        env.AssertListenerNotInvoked("s0");

	        env.Milestone(0);

	        SendEvent(env, SYMBOL_DELL, 30);
	        AssertNewEvent(env, SYMBOL_DELL, 100);

	        SendEvent(env, SYMBOL_IBM, 30);
	        AssertOldEvent(env, SYMBOL_DELL, 100);

	        SendEvent(env, SYMBOL_IBM, 80);
	        AssertNewEvent(env, SYMBOL_IBM, 110);
	    }

	    private static void AssertNewEvent(RegressionEnvironment env, string symbol, double newSum) {
	        env.AssertListener("s0", listener => {
	            var oldData = listener.LastOldData;
	            var newData = listener.LastNewData;

	            Assert.IsNull(oldData);
	            Assert.AreEqual(1, newData.Length);

	            Assert.AreEqual(newSum, newData[0].Get("mySum"));
	            Assert.AreEqual(symbol, newData[0].Get("symbol"));

	            listener.Reset();
	        });
	    }

	    private static void AssertOldEvent(RegressionEnvironment env, string symbol, double newSum) {
	        env.AssertListener("s0", listener => {
	            var oldData = listener.LastOldData;
	            var newData = listener.LastNewData;

	            Assert.IsNull(newData);
	            Assert.AreEqual(1, oldData.Length);

	            Assert.AreEqual(newSum, oldData[0].Get("mySum"));
	            Assert.AreEqual(symbol, oldData[0].Get("symbol"));

	            listener.Reset();
	        });
	    }

	    private static void SendEvent(RegressionEnvironment env, string symbol, double price) {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        env.SendEventBean(bean);
	    }
	}
} // end of namespace
