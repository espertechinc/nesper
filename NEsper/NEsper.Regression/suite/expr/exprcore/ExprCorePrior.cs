///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCorePrior {
	    public static ICollection<RegressionExecution> Executions() {
	        var execs = new List<RegressionExecution>();
	        execs.Add(new ExprCorePriorBoundedMultiple());
	        execs.Add(new ExprCorePriorExtTimedWindow());
	        execs.Add(new ExprCorePriorTimeBatchWindow());
	        execs.Add(new ExprCorePriorNoDataWindowWhere());
	        execs.Add(new ExprCorePriorLengthWindowWhere());
	        execs.Add(new ExprCorePriorStreamAndVariable());
	        execs.Add(new ExprCorePriorUnbound());
	        execs.Add(new ExprCorePriorUnboundSceneOne());
	        execs.Add(new ExprCorePriorUnboundSceneTwo());
	        execs.Add(new ExprCorePriorBoundedSingle());
	        execs.Add(new ExprCorePriorLongRunningSingle());
	        execs.Add(new ExprCorePriorLongRunningUnbound());
	        execs.Add(new ExprCorePriorLongRunningMultiple());
	        execs.Add(new ExprCorePriorTimewindowStats());
	        execs.Add(new ExprCorePriorTimeWindow());
	        execs.Add(new ExprCorePriorLengthWindow());
	        execs.Add(new ExprCorePriorLengthWindowSceneTwo());
	        execs.Add(new ExprCorePriorSortWindow());
	        execs.Add(new ExprCorePriorTimeBatchWindowJoin());
	        return execs;
	    }

	    public class ExprCorePriorUnboundSceneOne : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@Name('s0') select prior(1, Symbol) as prior1 from SupportMarketDataBean";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(MakeMarketDataEvent("E0"));
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1"}, new[] {
		            new object[] {null}
	            });
	            env.Listener("s0").Reset();

	            env.Milestone(1);

	            env.SendEventBean(MakeMarketDataEvent("E1"));
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1"}, new[] {
		            new object[] {"E0"}
	            });
	            env.Listener("s0").Reset();

	            env.Milestone(2);

	            for (var i = 2; i < 9; i++) {
	                env.SendEventBean(MakeMarketDataEvent("E" + i));
	                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1"}, new[] {
		                new object[] {"E" + (i - 1)}
	                });
	                env.Listener("s0").Reset();

	                if (i % 3 == 0) {
	                    env.Milestone(i + 1);
	                }
	            }

	            env.UndeployAll();
	        }
	    }

	    public class ExprCorePriorUnboundSceneTwo : RegressionExecution {

	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2".SplitCsv();

	            var epl = "@Name('s0') select TheString as c0, prior(1, IntPrimitive) as c1, prior(2, IntPrimitive) as c2 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(1);

	            SendSupportBean(env, "E1", 10);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E1", null, null);

	            env.Milestone(2);

	            SendSupportBean(env, "E2", 11);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E2", 10, null);

	            env.Milestone(3);

	            SendSupportBean(env, "E3", 12);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E3", 11, 10);

	            env.Milestone(4);

	            env.Milestone(5);

	            SendSupportBean(env, "E4", 13);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E4", 12, 11);

	            SendSupportBean(env, "E5", 14);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E5", 13, 12);

	            env.UndeployAll();
	        }
	    }

	    public class ExprCorePriorBoundedMultiple : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2".SplitCsv();
	            var epl = "@Name('s0') select irstream TheString as c0, prior(1, IntPrimitive) as c1, prior(2, IntPrimitive) as c2 from SupportBean#length(2)";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(1);

	            SendSupportBean(env, "E1", 10);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E1", null, null);

	            env.Milestone(2);

	            SendSupportBean(env, "E2", 11);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E2", 10, null);

	            env.Milestone(3);

	            SendSupportBean(env, "E3", 12);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertGetAndResetIRPair(), fields, new object[]{"E3", 11, 10}, new object[]{"E1", null, null});

	            env.Milestone(4);

	            env.Milestone(5);

	            SendSupportBean(env, "E4", 13);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertGetAndResetIRPair(), fields, new object[]{"E4", 12, 11}, new object[]{"E2", 10, null});

	            SendSupportBean(env, "E5", 14);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertGetAndResetIRPair(), fields, new object[]{"E5", 13, 12}, new object[]{"E3", 11, 10});

	            env.UndeployAll();
	        }
	    }

	    public class ExprCorePriorBoundedSingle : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1".SplitCsv();

	            env.Milestone(0);

	            var epl = "@Name('s0') select irstream TheString as c0, prior(1, IntPrimitive) as c1 from SupportBean#length(2)";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(1);

	            SendSupportBean(env, "E1", 10);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E1", null);

	            env.Milestone(2);

	            SendSupportBean(env, "E2", 11);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "E2", 10);

	            env.Milestone(3);

	            SendSupportBean(env, "E3", 12);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertGetAndResetIRPair(), fields, new object[]{"E3", 11}, new object[]{"E1", null});

	            env.Milestone(4);

	            env.Milestone(5);

	            SendSupportBean(env, "E4", 13);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertGetAndResetIRPair(), fields, new object[]{"E4", 12}, new object[]{"E2", 10});

	            SendSupportBean(env, "E5", 14);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertGetAndResetIRPair(), fields, new object[]{"E5", 13}, new object[]{"E3", 11});

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorTimewindowStats : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var epl = "@Name('s0') SELECT prior(1, average) as value FROM SupportBean()#time(5 minutes)#uni(IntPrimitive)";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

	            env.SendEventBean(new SupportBean("E1", 4));
	            Assert.AreEqual(1.0, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

	            env.SendEventBean(new SupportBean("E1", 5));
	            Assert.AreEqual(2.5, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorStreamAndVariable : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var path = new RegressionPath();
	            TryAssertionPriorStreamAndVariable(env, path, "1", milestone);

	            // try variable
	            TryAssertionPriorStreamAndVariable(env, path, "NUM_PRIOR", milestone);

	            // must be a constant-value expression
	            env.CompileDeploy("create variable int NUM_PRIOR_NONCONST = 1", path);
	            TryInvalidCompile(env, path, "@Name('s0') select prior(NUM_PRIOR_NONCONST, s0) as result from SupportBean_S0#length(2) as s0",
	                "Failed to validate select-clause expression 'prior(NUM_PRIOR_NONCONST,s0)': Prior function requires a constant-value integer-typed index expression as the first parameter");

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorTimeWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select irstream Symbol as currSymbol, " +
	                      " prior(2, Symbol) as priorSymbol, " +
	                      " prior(2, Price) as priorPrice " +
	                      "from SupportMarketDataBean#time(1 min)";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("priorSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("priorPrice"));

	            SendTimer(env, 0);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D1", 1);
	            AssertNewEvents(env, "D1", null, null);

	            SendTimer(env, 1000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D2", 2);
	            AssertNewEvents(env, "D2", null, null);

	            SendTimer(env, 2000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D3", 3);
	            AssertNewEvents(env, "D3", "D1", 1d);

	            SendTimer(env, 3000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D4", 4);
	            AssertNewEvents(env, "D4", "D2", 2d);

	            SendTimer(env, 4000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D5", 5);
	            AssertNewEvents(env, "D5", "D3", 3d);

	            SendTimer(env, 30000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D6", 6);
	            AssertNewEvents(env, "D6", "D4", 4d);

	            SendTimer(env, 60000);
	            AssertOldEvents(env, "D1", null, null);
	            SendTimer(env, 61000);
	            AssertOldEvents(env, "D2", null, null);
	            SendTimer(env, 62000);
	            AssertOldEvents(env, "D3", "D1", 1d);
	            SendTimer(env, 63000);
	            AssertOldEvents(env, "D4", "D2", 2d);
	            SendTimer(env, 64000);
	            AssertOldEvents(env, "D5", "D3", 3d);
	            SendTimer(env, 90000);
	            AssertOldEvents(env, "D6", "D4", 4d);

	            SendMarketEvent(env, "D7", 7);
	            AssertNewEvents(env, "D7", "D5", 5d);
	            SendMarketEvent(env, "D8", 8);
	            SendMarketEvent(env, "D9", 9);
	            SendMarketEvent(env, "D10", 10);
	            SendMarketEvent(env, "D11", 11);
	            env.Listener("s0").Reset();

	            // release batch
	            SendTimer(env, 150000);
	            var oldData = env.Listener("s0").LastOldData;
	            Assert.IsNull(env.Listener("s0").LastNewData);
	            Assert.AreEqual(5, oldData.Length);
	            AssertEvent(oldData[0], "D7", "D5", 5d);
	            AssertEvent(oldData[1], "D8", "D6", 6d);
	            AssertEvent(oldData[2], "D9", "D7", 7d);
	            AssertEvent(oldData[3], "D10", "D8", 8d);
	            AssertEvent(oldData[4], "D11", "D9", 9d);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorExtTimedWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select irstream Symbol as currSymbol, " +
	                      " prior(2, Symbol) as priorSymbol, " +
	                      " prior(3, Price) as priorPrice " +
	                      "from SupportMarketDataBean#ext_timed(Volume, 1 min) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("priorSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("priorPrice"));

	            SendMarketEvent(env, "D1", 1, 0);
	            AssertNewEvents(env, "D1", null, null);

	            SendMarketEvent(env, "D2", 2, 1000);
	            AssertNewEvents(env, "D2", null, null);

	            SendMarketEvent(env, "D3", 3, 3000);
	            AssertNewEvents(env, "D3", "D1", null);

	            SendMarketEvent(env, "D4", 4, 4000);
	            AssertNewEvents(env, "D4", "D2", 1d);

	            SendMarketEvent(env, "D5", 5, 5000);
	            AssertNewEvents(env, "D5", "D3", 2d);

	            SendMarketEvent(env, "D6", 6, 30000);
	            AssertNewEvents(env, "D6", "D4", 3d);

	            SendMarketEvent(env, "D7", 7, 60000);
	            AssertEvent(env.Listener("s0").LastNewData[0], "D7", "D5", 4d);
	            AssertEvent(env.Listener("s0").LastOldData[0], "D1", null, null);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "D8", 8, 61000);
	            AssertEvent(env.Listener("s0").LastNewData[0], "D8", "D6", 5d);
	            AssertEvent(env.Listener("s0").LastOldData[0], "D2", null, null);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "D9", 9, 63000);
	            AssertEvent(env.Listener("s0").LastNewData[0], "D9", "D7", 6d);
	            AssertEvent(env.Listener("s0").LastOldData[0], "D3", "D1", null);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "D10", 10, 64000);
	            AssertEvent(env.Listener("s0").LastNewData[0], "D10", "D8", 7d);
	            AssertEvent(env.Listener("s0").LastOldData[0], "D4", "D2", 1d);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "D10", 10, 150000);
	            var oldData = env.Listener("s0").LastOldData;
	            Assert.AreEqual(6, oldData.Length);
	            AssertEvent(oldData[0], "D5", "D3", 2d);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorTimeBatchWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select irstream Symbol as currSymbol, " +
	                      " prior(3, Symbol) as priorSymbol, " +
	                      " prior(2, Price) as priorPrice " +
	                      "from SupportMarketDataBean#time_batch(1 min) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("priorSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("priorPrice"));

	            SendTimer(env, 0);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "A", 1);
	            SendMarketEvent(env, "B", 2);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 60000);
	            Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
	            AssertEvent(env.Listener("s0").LastNewData[0], "A", null, null);
	            AssertEvent(env.Listener("s0").LastNewData[1], "B", null, null);
	            Assert.IsNull(env.Listener("s0").LastOldData);
	            env.Listener("s0").Reset();

	            SendTimer(env, 80000);
	            SendMarketEvent(env, "C", 3);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 120000);
	            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
	            AssertEvent(env.Listener("s0").LastNewData[0], "C", null, 1d);
	            Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
	            AssertEvent(env.Listener("s0").LastOldData[0], "A", null, null);
	            env.Listener("s0").Reset();

	            SendTimer(env, 300000);
	            SendMarketEvent(env, "D", 4);
	            SendMarketEvent(env, "E", 5);
	            SendMarketEvent(env, "F", 6);
	            SendMarketEvent(env, "G", 7);
	            SendTimer(env, 360000);
	            Assert.AreEqual(4, env.Listener("s0").LastNewData.Length);
	            AssertEvent(env.Listener("s0").LastNewData[0], "D", "A", 2d);
	            AssertEvent(env.Listener("s0").LastNewData[1], "E", "B", 3d);
	            AssertEvent(env.Listener("s0").LastNewData[2], "F", "C", 4d);
	            AssertEvent(env.Listener("s0").LastNewData[3], "G", "D", 5d);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorUnbound : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select Symbol as currSymbol, " +
	                      " prior(3, Symbol) as priorSymbol, " +
	                      " prior(2, Price) as priorPrice " +
	                      "from SupportMarketDataBean";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("priorSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("priorPrice"));

	            SendMarketEvent(env, "A", 1);
	            AssertNewEvents(env, "A", null, null);

	            env.Milestone(1);

	            SendMarketEvent(env, "B", 2);
	            AssertNewEvents(env, "B", null, null);

	            env.Milestone(2);

	            SendMarketEvent(env, "C", 3);
	            AssertNewEvents(env, "C", null, 1d);

	            env.Milestone(3);

	            SendMarketEvent(env, "D", 4);
	            AssertNewEvents(env, "D", "A", 2d);

	            env.Milestone(4);

	            SendMarketEvent(env, "E", 5);
	            AssertNewEvents(env, "E", "B", 3d);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorNoDataWindowWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@Name('s0') select * from SupportMarketDataBean where prior(1, Price) = 100";
	            env.CompileDeploy(text).AddListener("s0");

	            SendMarketEvent(env, "IBM", 75);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "IBM", 100);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "IBM", 120);
	            Assert.IsTrue(env.Listener("s0").IsInvoked);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorLongRunningSingle : RegressionExecution {
		    public bool IsExcludeWhenInstrumented => true;

		    public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select Symbol as currSymbol, " +
	                      " prior(3, Symbol) as prior0Symbol " +
	                      "from SupportMarketDataBean#sort(3, Symbol)";
	            env.CompileDeploy(epl).AddListener("s0");

	            var random = new Random();
	            // 200000 is a better number for a memory test, however for short unit tests this is 2000
	            for (var i = 0; i < 2000; i++) {
	                if (i % 10000 == 0) {
	                    //System.out.println(i);
	                }

	                SendMarketEvent(env, Convert.ToString(random.Next()), 4);

	                if (i % 1000 == 0) {
	                    env.Listener("s0").Reset();
	                }
	            }

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorLongRunningUnbound : RegressionExecution {
		    public bool IsExcludeWhenInstrumented => true;

		    public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select Symbol as currSymbol, " +
	                      " prior(3, Symbol) as prior0Symbol " +
	                      "from SupportMarketDataBean";
	            env.CompileDeploy(epl).AddListener("s0");
	            AssertStatelessStmt(env, "s0", false);

	            var random = new Random();
	            // 200000 is a better number for a memory test, however for short unit tests this is 2000
	            for (var i = 0; i < 2000; i++) {
	                if (i % 10000 == 0) {
	                    //System.out.println(i);
	                }

	                SendMarketEvent(env, Convert.ToString(random.Next()), 4);

	                if (i % 1000 == 0) {
	                    env.Listener("s0").Reset();
	                }
	            }

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorLongRunningMultiple : RegressionExecution {
		    public bool IsExcludeWhenInstrumented => true;

		    public void Run(RegressionEnvironment env) {

	            var epl = "@Name('s0') select Symbol as currSymbol, " +
	                      " prior(3, Symbol) as prior0Symbol, " +
	                      " prior(2, Symbol) as prior1Symbol, " +
	                      " prior(1, Symbol) as prior2Symbol, " +
	                      " prior(0, Symbol) as prior3Symbol, " +
	                      " prior(0, Price) as prior0Price, " +
	                      " prior(1, Price) as prior1Price, " +
	                      " prior(2, Price) as prior2Price, " +
	                      " prior(3, Price) as prior3Price " +
	                      "from SupportMarketDataBean#sort(3, Symbol)";
	            env.CompileDeploy(epl).AddListener("s0");

	            var random = new Random();
	            // 200000 is a better number for a memory test, however for short unit tests this is 2000
	            for (var i = 0; i < 2000; i++) {
	                if (i % 10000 == 0) {
	                    //System.out.println(i);
	                }

	                SendMarketEvent(env, Convert.ToString(random.Next()), 4);

	                if (i % 1000 == 0) {
	                    env.Listener("s0").Reset();
	                }
	            }

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorLengthWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select irstream Symbol as currSymbol, " +
	                      "prior(0, Symbol) as prior0Symbol, " +
	                      "prior(1, Symbol) as prior1Symbol, " +
	                      "prior(2, Symbol) as prior2Symbol, " +
	                      "prior(3, Symbol) as prior3Symbol, " +
	                      "prior(0, Price) as prior0Price, " +
	                      "prior(1, Price) as prior1Price, " +
	                      "prior(2, Price) as prior2Price, " +
	                      "prior(3, Price) as prior3Price " +
	                      "from SupportMarketDataBean#length(3) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prior0Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prior0Price"));

	            SendMarketEvent(env, "A", 1);
	            AssertNewEvents(env, "A", "A", 1d, null, null, null, null, null, null);
	            SendMarketEvent(env, "B", 2);
	            AssertNewEvents(env, "B", "B", 2d, "A", 1d, null, null, null, null);
	            SendMarketEvent(env, "C", 3);
	            AssertNewEvents(env, "C", "C", 3d, "B", 2d, "A", 1d, null, null);

	            SendMarketEvent(env, "D", 4);
	            var newEvent = env.Listener("s0").LastNewData[0];
	            var oldEvent = env.Listener("s0").LastOldData[0];
	            AssertEventProps(env, newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);
	            AssertEventProps(env, oldEvent, "A", "A", 1d, null, null, null, null, null, null);

	            SendMarketEvent(env, "E", 5);
	            newEvent = env.Listener("s0").LastNewData[0];
	            oldEvent = env.Listener("s0").LastOldData[0];
	            AssertEventProps(env, newEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
	            AssertEventProps(env, oldEvent, "B", "B", 2d, "A", 1d, null, null, null, null);

	            SendMarketEvent(env, "F", 6);
	            newEvent = env.Listener("s0").LastNewData[0];
	            oldEvent = env.Listener("s0").LastOldData[0];
	            AssertEventProps(env, newEvent, "F", "F", 6d, "E", 5d, "D", 4d, "C", 3d);
	            AssertEventProps(env, oldEvent, "C", "C", 3d, "B", 2d, "A", 1d, null, null);

	            SendMarketEvent(env, "G", 7);
	            newEvent = env.Listener("s0").LastNewData[0];
	            oldEvent = env.Listener("s0").LastOldData[0];
	            AssertEventProps(env, newEvent, "G", "G", 7d, "F", 6d, "E", 5d, "D", 4d);
	            AssertEventProps(env, oldEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);

	            SendMarketEvent(env, "G", 8);
	            oldEvent = env.Listener("s0").LastOldData[0];
	            AssertEventProps(env, oldEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);

	            env.UndeployAll();
	        }
	    }

	    public class ExprCorePriorLengthWindowSceneTwo : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@Name('s0') select prior(1, symbol) as prior1, prior(2, Symbol) as prior2 from SupportMarketDataBean#length(3)";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(MakeMarketDataEvent("E0"));
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1", "prior2"}, new[] {
		            new object[] {null, null}
	            });
	            env.Listener("s0").Reset();

	            env.Milestone(1);

	            env.SendEventBean(MakeMarketDataEvent("E1"));
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1", "prior2"}, new[] {
		            new object[] {"E0", null}
	            });
	            env.Listener("s0").Reset();

	            env.Milestone(2);

	            env.SendEventBean(MakeMarketDataEvent("E2"));
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1", "prior2"}, new[] {
		            new object[] {"E1", "E0"}
	            });
	            env.Listener("s0").Reset();

	            env.Milestone(3);

	            for (var i = 3; i < 9; i++) {
	                env.SendEventBean(MakeMarketDataEvent("E" + i));
	                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, new[]{"prior1", "prior2"},
	                    new[] {
		                    new object[] {"E" + (i - 1), "E" + (i - 2)}
	                    });
	                env.Listener("s0").Reset();

	                if (i % 3 == 0) {
	                    env.Milestone(i);
	                }
	            }

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorLengthWindowWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select prior(2, Symbol) as currSymbol " +
	                      "from SupportMarketDataBean#length(1) " +
	                      "where prior(2, Price) > 100";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendMarketEvent(env, "A", 1);
	            SendMarketEvent(env, "B", 130);
	            SendMarketEvent(env, "C", 10);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);
	            SendMarketEvent(env, "D", 5);
	            Assert.AreEqual("B", env.Listener("s0").AssertOneGetNewAndReset().Get("currSymbol"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePriorSortWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
		        var milestone = new AtomicLong();
	            var epl = "@Name('s0') select irstream Symbol as currSymbol, " +
	                      " prior(0, Symbol) as prior0Symbol, " +
	                      " prior(1, Symbol) as prior1Symbol, " +
	                      " prior(2, Symbol) as prior2Symbol, " +
	                      " prior(3, Symbol) as prior3Symbol, " +
	                      " prior(0, Price) as prior0Price, " +
	                      " prior(1, Price) as prior1Price, " +
	                      " prior(2, Price) as prior2Price, " +
	                      " prior(3, Price) as prior3Price " +
	                      "from SupportMarketDataBean#sort(3, Symbol)";
	            TryPriorSortWindow(env, epl, milestone);

	            epl = "@Name('s0') select irstream Symbol as currSymbol, " +
	                " prior(3, Symbol) as prior3Symbol, " +
	                " prior(1, Symbol) as prior1Symbol, " +
	                " prior(2, Symbol) as prior2Symbol, " +
	                " prior(0, Symbol) as prior0Symbol, " +
	                " prior(2, Price) as prior2Price, " +
	                " prior(1, Price) as prior1Price, " +
	                " prior(0, Price) as prior0Price, " +
	                " prior(3, Price) as prior3Price " +
	                "from SupportMarketDataBean#sort(3, Symbol)";
	            TryPriorSortWindow(env, epl, milestone);
	        }
	    }

	    private class ExprCorePriorTimeBatchWindowJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Name('s0') select TheString as currSymbol, " +
	                      "prior(2, Symbol) as priorSymbol, " +
	                      "prior(1, Price) as priorPrice " +
	                      "from SupportBean#keepall, SupportMarketDataBean#time_batch(1 min)";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("priorSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("priorPrice"));

	            SendTimer(env, 0);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "A", 1);
	            SendMarketEvent(env, "B", 2);
	            SendBeanEvent(env, "X1");
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 60000);
	            Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
	            AssertEvent(env.Listener("s0").LastNewData[0], "X1", null, null);
	            AssertEvent(env.Listener("s0").LastNewData[1], "X1", null, 1d);
	            Assert.IsNull(env.Listener("s0").LastOldData);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "C1", 11);
	            SendMarketEvent(env, "C2", 12);
	            SendMarketEvent(env, "C3", 13);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 120000);
	            Assert.AreEqual(3, env.Listener("s0").LastNewData.Length);
	            AssertEvent(env.Listener("s0").LastNewData[0], "X1", "A", 2d);
	            AssertEvent(env.Listener("s0").LastNewData[1], "X1", "B", 11d);
	            AssertEvent(env.Listener("s0").LastNewData[2], "X1", "C1", 12d);

	            env.UndeployAll();
	        }
	    }

	    private static void TryAssertionPriorStreamAndVariable(RegressionEnvironment env, RegressionPath path, string priorIndex, AtomicLong milestone) {
	        var text = "create constant variable int NUM_PRIOR = 1;\n @name('s0') select prior(" + priorIndex + ", s0) as result from SupportBean_S0#length(2) as s0";
	        env.CompileDeploy(text, path).AddListener("s0");

	        var e1 = new SupportBean_S0(3);
	        env.SendEventBean(e1);
	        Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

	        env.Milestone(milestone.GetAndIncrement());

	        env.SendEventBean(new SupportBean_S0(3));
	        Assert.AreEqual(e1, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));
	        Assert.AreEqual(typeof(SupportBean_S0), env.Statement("s0").EventType.GetPropertyType("result"));

	        env.UndeployAll();
	        path.Clear();
	    }

	    private static void TryPriorSortWindow(RegressionEnvironment env, string epl, AtomicLong milestone) {
	        env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

	        SendMarketEvent(env, "COX", 30);
	        AssertNewEvents(env, "COX", "COX", 30d, null, null, null, null, null, null);

	        SendMarketEvent(env, "IBM", 45);
	        AssertNewEvents(env, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);

	        SendMarketEvent(env, "MSFT", 33);
	        AssertNewEvents(env, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);

	        SendMarketEvent(env, "XXX", 55);
	        var newEvent = env.Listener("s0").LastNewData[0];
	        var oldEvent = env.Listener("s0").LastOldData[0];
	        AssertEventProps(env, newEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);
	        AssertEventProps(env, oldEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);

	        SendMarketEvent(env, "BOO", 20);
	        newEvent = env.Listener("s0").LastNewData[0];
	        oldEvent = env.Listener("s0").LastOldData[0];
	        AssertEventProps(env, newEvent, "BOO", "BOO", 20d, "XXX", 55d, "MSFT", 33d, "IBM", 45d);
	        AssertEventProps(env, oldEvent, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);

	        SendMarketEvent(env, "DOR", 1);
	        newEvent = env.Listener("s0").LastNewData[0];
	        oldEvent = env.Listener("s0").LastOldData[0];
	        AssertEventProps(env, newEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);
	        AssertEventProps(env, oldEvent, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);

	        SendMarketEvent(env, "AAA", 2);
	        newEvent = env.Listener("s0").LastNewData[0];
	        oldEvent = env.Listener("s0").LastOldData[0];
	        AssertEventProps(env, newEvent, "AAA", "AAA", 2d, "DOR", 1d, "BOO", 20d, "XXX", 55d);
	        AssertEventProps(env, oldEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);

	        SendMarketEvent(env, "AAB", 2);
	        oldEvent = env.Listener("s0").LastOldData[0];
	        AssertEventProps(env, oldEvent, "COX", "COX", 30d, null, null, null, null, null, null);
	        env.Listener("s0").Reset();

	        env.UndeployAll();
	    }

	    private static void AssertNewEvents(RegressionEnvironment env, string currSymbol,
	                                        string priorSymbol,
	                                        double? priorPrice) {
	        var oldData = env.Listener("s0").LastOldData;
	        var newData = env.Listener("s0").LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);

	        AssertEvent(newData[0], currSymbol, priorSymbol, priorPrice);

	        env.Listener("s0").Reset();
	    }

	    private static void AssertEvent(EventBean eventBean,
	                                    string currSymbol,
	                                    string priorSymbol,
	                                    double? priorPrice) {
	        Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
	        Assert.AreEqual(priorSymbol, eventBean.Get("priorSymbol"));
	        Assert.AreEqual(priorPrice, eventBean.Get("priorPrice"));
	    }

	    private static void AssertNewEvents(RegressionEnvironment env, string currSymbol,
	                                        string prior0Symbol,
	                                        double? prior0Price,
	                                        string prior1Symbol,
	                                        double? prior1Price,
	                                        string prior2Symbol,
	                                        double? prior2Price,
	                                        string prior3Symbol,
	                                        double? prior3Price) {
	        var oldData = env.Listener("s0").LastOldData;
	        var newData = env.Listener("s0").LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);
	        AssertEventProps(env, newData[0], currSymbol, prior0Symbol, prior0Price, prior1Symbol, prior1Price, prior2Symbol, prior2Price, prior3Symbol, prior3Price);

	        env.Listener("s0").Reset();
	    }

	    private static void AssertEventProps(RegressionEnvironment env, EventBean eventBean,
	                                         string currSymbol,
	                                         string prior0Symbol,
	                                         double? prior0Price,
	                                         string prior1Symbol,
	                                         double? prior1Price,
	                                         string prior2Symbol,
	                                         double? prior2Price,
	                                         string prior3Symbol,
	                                         double? prior3Price) {
	        Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
	        Assert.AreEqual(prior0Symbol, eventBean.Get("prior0Symbol"));
	        Assert.AreEqual(prior0Price, eventBean.Get("prior0Price"));
	        Assert.AreEqual(prior1Symbol, eventBean.Get("prior1Symbol"));
	        Assert.AreEqual(prior1Price, eventBean.Get("prior1Price"));
	        Assert.AreEqual(prior2Symbol, eventBean.Get("prior2Symbol"));
	        Assert.AreEqual(prior2Price, eventBean.Get("prior2Price"));
	        Assert.AreEqual(prior3Symbol, eventBean.Get("prior3Symbol"));
	        Assert.AreEqual(prior3Price, eventBean.Get("prior3Price"));

	        env.Listener("s0").Reset();
	    }

	    private static void SendTimer(RegressionEnvironment env, long timeInMSec) {
	        env.AdvanceTime(timeInMSec);
	    }

	    private static void SendMarketEvent(RegressionEnvironment env, string symbol, double price) {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        env.SendEventBean(bean);
	    }

	    private static void SendMarketEvent(RegressionEnvironment env, string symbol, double price, long volume) {
	        var bean = new SupportMarketDataBean(symbol, price, volume, null);
	        env.SendEventBean(bean);
	    }

	    private static void SendBeanEvent(RegressionEnvironment env, string theString) {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        env.SendEventBean(bean);
	    }

	    private static void SendSupportBean(RegressionEnvironment env, string theString, int intPrimitive) {
	        env.SendEventBean(new SupportBean(theString, intPrimitive));
	    }

	    private static void AssertOldEvents(RegressionEnvironment env, string currSymbol,
	                                        string priorSymbol,
	                                        double? priorPrice) {
	        var oldData = env.Listener("s0").LastOldData;
	        var newData = env.Listener("s0").LastNewData;

	        Assert.IsNull(newData);
	        Assert.AreEqual(1, oldData.Length);

	        Assert.AreEqual(currSymbol, oldData[0].Get("currSymbol"));
	        Assert.AreEqual(priorSymbol, oldData[0].Get("priorSymbol"));
	        Assert.AreEqual(priorPrice, oldData[0].Get("priorPrice"));

	        env.Listener("s0").Reset();
	    }

	    private static SupportMarketDataBean MakeMarketDataEvent(string symbol) {
	        return new SupportMarketDataBean(symbol, 0, 0L, "");
	    }
	}
} // end of namespace
