///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
	public class ResultSetAggregateMaxMinGroupBy {
	    private const string SYMBOL_DELL = "DELL";
	    private const string SYMBOL_IBM = "IBM";

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ResultSetAggregateMinMax());
	        execs.Add(new ResultSetAggregateMinMaxOM());
	        execs.Add(new ResultSetAggregateMinMaxViewCompile());
	        execs.Add(new ResultSetAggregateMinMaxJoin());
	        execs.Add(new ResultSetAggregateMinNoGroupHaving());
	        execs.Add(new ResultSetAggregateMinNoGroupSelectHaving());
	        return execs;
	    }

	    private class ResultSetAggregateMinMax : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select irstream symbol, " +
	                      "min(all volume) as minVol," +
	                      "max(all volume) as maxVol," +
	                      "min(distinct volume) as minDistVol," +
	                      "max(distinct volume) as maxDistVol" +
	                      " from SupportMarketDataBean#length(3) " +
	                      "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
	                      "group by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            TryAssertionMinMax(env, milestone);

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggregateMinMaxOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create()
		            .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
		            .Add("symbol")
		            .Add(Expressions.Min("volume"), "minVol")
		            .Add(Expressions.Max("volume"), "maxVol")
		            .Add(Expressions.MinDistinct("volume"), "minDistVol")
		            .Add(Expressions.MaxDistinct("volume"), "maxDistVol");

	            model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportMarketDataBean)).AddView("length", Expressions.Constant(3)));
	            model.WhereClause = Expressions.Or()
	                .Add(Expressions.Eq("symbol", "DELL"))
	                .Add(Expressions.Eq("symbol", "IBM"))
	                .Add(Expressions.Eq("symbol", "GE"));
	            model.GroupByClause = GroupByClause.Create("symbol");
	            model = env.CopyMayFail(model);

	            var epl = "select irstream symbol, " +
	                      "min(volume) as minVol, " +
	                      "max(volume) as maxVol, " +
	                      "min(distinct volume) as minDistVol, " +
	                      "max(distinct volume) as maxDistVol " +
	                      "from SupportMarketDataBean#length(3) " +
	                      "where symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\" " +
	                      "group by symbol";
	            Assert.AreEqual(epl, model.ToEPL());

	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
	            env.CompileDeploy(model).AddListener("s0");

	            TryAssertionMinMax(env, new AtomicLong());

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggregateMinMaxViewCompile : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select irstream symbol, " +
	                      "min(volume) as minVol, " +
	                      "max(volume) as maxVol, " +
	                      "min(distinct volume) as minDistVol, " +
	                      "max(distinct volume) as maxDistVol " +
	                      "from SupportMarketDataBean#length(3) " +
	                      "where symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\" " +
	                      "group by symbol";
	            env.EplToModelCompileDeploy(epl).AddListener("s0");

	            TryAssertionMinMax(env, new AtomicLong());

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggregateMinMaxJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select irstream symbol, " +
	                      "min(volume) as minVol," +
	                      "max(volume) as maxVol," +
	                      "min(distinct volume) as minDistVol," +
	                      "max(distinct volume) as maxDistVol" +
	                      " from SupportBeanString#length(100) as one, " +
	                      "SupportMarketDataBean#length(3) as two " +
	                      "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
	                      "  and one.theString = two.symbol " +
	                      "group by symbol";
	            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

	            env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
	            env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

	            TryAssertionMinMax(env, milestone);

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggregateMinNoGroupHaving : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select symbol from SupportMarketDataBean#time(5 sec) " +
	                           "having volume > min(volume) * 1.3";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            SendEvent(env, "DELL", 100L);
	            SendEvent(env, "DELL", 105L);
	            SendEvent(env, "DELL", 100L);
	            env.AssertListenerNotInvoked("s0");

	            env.Milestone(1);

	            SendEvent(env, "DELL", 131L);
	            env.AssertEqualsNew("s0", "symbol", "DELL");

	            SendEvent(env, "DELL", 132L);
	            env.AssertEqualsNew("s0", "symbol", "DELL");

	            env.Milestone(2);

	            SendEvent(env, "DELL", 129L);
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggregateMinNoGroupSelectHaving : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "symbol,mymin".SplitCsv();
	            var stmtText = "@name('s0') select symbol, min(volume) as mymin from SupportMarketDataBean#length(5) " +
	                           "having volume > min(volume) * 1.3";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            SendEvent(env, "DELL", 100L);
	            SendEvent(env, "DELL", 105L);
	            SendEvent(env, "DELL", 100L);
	            env.AssertListenerNotInvoked("s0");

	            SendEvent(env, "DELL", 131L);
	            env.AssertPropsNew("s0", fields, new object[] {"DELL", 100L});

	            env.Milestone(1);

	            SendEvent(env, "DELL", 132L);
	            env.AssertPropsNew("s0", fields, new object[] {"DELL", 100L});

	            SendEvent(env, "DELL", 129L);
	            SendEvent(env, "DELL", 125L);
	            SendEvent(env, "DELL", 125L);
	            env.AssertListenerNotInvoked("s0");

	            SendEvent(env, "DELL", 170L);
	            env.AssertPropsNew("s0", fields, new object[] {"DELL", 125L});

	            env.UndeployAll();
	        }
	    }

	    private static void TryAssertionMinMax(RegressionEnvironment env, AtomicLong milestone) {
	        // assert select result type
	        env.AssertStatement("s0", statement => {
	            Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("symbol"));
	            Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("minVol"));
	            Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("maxVol"));
	            Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("minDistVol"));
	            Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("maxDistVol"));
	        });

	        SendEvent(env, SYMBOL_DELL, 50L);
	        AssertEvents(env, SYMBOL_DELL, null, null, null, null,
	            SYMBOL_DELL, 50L, 50L, 50L, 50L
	        );

	        env.MilestoneInc(milestone);

	        SendEvent(env, SYMBOL_DELL, 30L);
	        AssertEvents(env, SYMBOL_DELL, 50L, 50L, 50L, 50L,
	            SYMBOL_DELL, 30L, 50L, 30L, 50L
	        );

	        SendEvent(env, SYMBOL_DELL, 30L);
	        AssertEvents(env, SYMBOL_DELL, 30L, 50L, 30L, 50L,
	            SYMBOL_DELL, 30L, 50L, 30L, 50L
	        );

	        env.MilestoneInc(milestone);

	        SendEvent(env, SYMBOL_DELL, 90L);
	        AssertEvents(env, SYMBOL_DELL, 30L, 50L, 30L, 50L,
	            SYMBOL_DELL, 30L, 90L, 30L, 90L
	        );

	        SendEvent(env, SYMBOL_DELL, 100L);
	        AssertEvents(env, SYMBOL_DELL, 30L, 90L, 30L, 90L,
	            SYMBOL_DELL, 30L, 100L, 30L, 100L
	        );

	        SendEvent(env, SYMBOL_IBM, 20L);
	        SendEvent(env, SYMBOL_IBM, 5L);
	        SendEvent(env, SYMBOL_IBM, 15L);
	        SendEvent(env, SYMBOL_IBM, 18L);
	        AssertEvents(env, SYMBOL_IBM, 5L, 20L, 5L, 20L,
	            SYMBOL_IBM, 5L, 18L, 5L, 18L
	        );

	        env.MilestoneInc(milestone);

	        SendEvent(env, SYMBOL_IBM, null);
	        AssertEvents(env, SYMBOL_IBM, 5L, 18L, 5L, 18L,
	            SYMBOL_IBM, 15L, 18L, 15L, 18L
	        );

	        SendEvent(env, SYMBOL_IBM, null);
	        AssertEvents(env, SYMBOL_IBM, 15L, 18L, 15L, 18L,
	            SYMBOL_IBM, 18L, 18L, 18L, 18L
	        );

	        SendEvent(env, SYMBOL_IBM, null);
	        AssertEvents(env, SYMBOL_IBM, 18L, 18L, 18L, 18L,
	            SYMBOL_IBM, null, null, null, null
	        );
	    }

	    private static void AssertEvents(RegressionEnvironment env, string symbolOld, long? minVolOld, long? maxVolOld, long? minDistVolOld, long? maxDistVolOld,
	                                     string symbolNew, long? minVolNew, long? maxVolNew, long? minDistVolNew, long? maxDistVolNew) {
	        env.AssertListener("s0", listener => {
	            var oldData = listener.LastOldData;
	            var newData = listener.LastNewData;

	            Assert.AreEqual(1, oldData.Length);
	            Assert.AreEqual(1, newData.Length);

	            Assert.AreEqual(symbolOld, oldData[0].Get("symbol"));
	            Assert.AreEqual(minVolOld, oldData[0].Get("minVol"));
	            Assert.AreEqual(maxVolOld, oldData[0].Get("maxVol"));
	            Assert.AreEqual(minDistVolOld, oldData[0].Get("minDistVol"));
	            Assert.AreEqual(maxDistVolOld, oldData[0].Get("maxDistVol"));

	            Assert.AreEqual(symbolNew, newData[0].Get("symbol"));
	            Assert.AreEqual(minVolNew, newData[0].Get("minVol"));
	            Assert.AreEqual(maxVolNew, newData[0].Get("maxVol"));
	            Assert.AreEqual(minDistVolNew, newData[0].Get("minDistVol"));
	            Assert.AreEqual(maxDistVolNew, newData[0].Get("maxDistVol"));

	            listener.Reset();
	        });
	    }

	    private static void SendEvent(RegressionEnvironment env, string symbol, long? volume) {
	        var bean = new SupportMarketDataBean(symbol, 0, volume, null);
	        env.SendEventBean(bean);
	    }

	    private static readonly ILog log = LogManager.GetLogger(typeof(ResultSetAggregateMaxMinGroupBy));
	}
} // end of namespace
