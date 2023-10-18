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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
	public class ViewTimeLengthBatch {

	    public static ICollection<RegressionExecution> Executions() {
	        var execs = new List<RegressionExecution>();
	        execs.Add(new ViewTimeLengthBatchSceneOne());
	        execs.Add(new ViewTimeLengthBatchSceneTwo());
	        execs.Add(new ViewTimeLengthBatchForceOutputOne());
	        execs.Add(new ViewTimeLengthBatchForceOutputTwo());
	        execs.Add(new ViewTimeLengthBatchForceOutputSum());
	        execs.Add(new ViewTimeLengthBatchStartEager());
	        execs.Add(new ViewTimeLengthBatchForceOutputStartEagerSum());
	        execs.Add(new ViewTimeLengthBatchForceOutputStartNoEagerSum());
	        execs.Add(new ViewTimeLengthBatchPreviousAndPrior());
	        execs.Add(new ViewTimeLengthBatchGroupBySumStartEager());
	        return execs;
	    }

	    internal class ViewTimeLengthBatchSceneOne : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = new string[] {"symbol"};
	            SendTimer(env, 1000);

	            var text = "@name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3)";
	            env.CompileDeployAddListenerMileZero(text, "s0");

	            SendTimer(env, 1000);
	            SendEvent(env, "E1");

	            env.Milestone(1);

	            SendTimer(env, 5000);
	            SendEvent(env, "E2");

	            env.Milestone(2);

	            SendTimer(env, 10999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, 11000);
	            env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[]{"E1"}, new object[] {"E2"}});

	            env.Milestone(3);

	            SendTimer(env, 12000);
	            SendEvent(env, "E3");
	            SendEvent(env, "E4");

	            env.Milestone(4);

	            SendTimer(env, 15000);
	            SendEvent(env, "E5");
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[]{"E3"}, new object[] {"E4"}, new object[] {"E5"}}, new object[][]{new object[]{"E1"}, new object[] {"E2"}});

	            env.Milestone(5);

	            SendTimer(env, 24999);
	            env.AssertListenerNotInvoked("s0");

	            // wait 10 second, check call
	            SendTimer(env, 25000);
	            env.AssertPropsPerRowIRPair("s0", fields, null, new object[][]{new object[]{"E3"}, new object[] {"E4"}, new object[] {"E5"}});

	            // wait 10 second, check no call received, no events
	            SendTimer(env, 35000);
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchSceneTwo : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            var events = Get100Events();

	            SendTimer(env, startTime);
	            var epl = "@name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3)";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            // Send 3 events in batch
	            env.SendEventBean(events[0]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[1]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[2]);
	            AssertUnderlyingIR(env, new object[]{events[0], events[1], events[2]}, Array.Empty<object>());

	            // Send another 3 events in batch
	            env.SendEventBean(events[3]);
	            env.SendEventBean(events[4]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[5]);
	            AssertUnderlyingIR(env, new object[]{events[3], events[4], events[5]}, new object[]{events[0], events[1], events[2]});

	            // Expire the last 3 events by moving time
	            SendTimer(env, startTime + 9999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 10000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), new object[]{events[3], events[4], events[5]});

	            SendTimer(env, startTime + 10001);
	            env.AssertListenerNotInvoked("s0");

	            // Send an event, let the timer send the batch
	            SendTimer(env, startTime + 10100);
	            env.SendEventBean(events[6]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 19999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 20000);
	            AssertUnderlyingIR(env, new object[]{events[6]}, new object[]{});

	            SendTimer(env, startTime + 20001);
	            env.AssertListenerNotInvoked("s0");

	            // Send two events, let the timer send the batch
	            SendTimer(env, startTime + 29998);
	            env.SendEventBean(events[7]);
	            env.SendEventBean(events[8]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 29999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 30000);
	            AssertUnderlyingIR(env, new object[]{events[7], events[8]}, new object[]{events[6]});

	            // Send three events, the the 3 events batch
	            SendTimer(env, startTime + 30001);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[9]);
	            env.SendEventBean(events[10]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[11]);
	            AssertUnderlyingIR(env, new object[]{events[9], events[10], events[11]}, new object[]{events[7], events[8]});

	            // Send 1 event, let the timer to do the batch
	            SendTimer(env, startTime + 39000 + 9999);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[12]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 10000);
	            AssertUnderlyingIR(env, new object[]{events[12]}, new object[]{events[9], events[10], events[11]});

	            SendTimer(env, startTime + 39000 + 10001);
	            env.AssertListenerNotInvoked("s0");

	            // Send no events, let the timer to do the batch
	            SendTimer(env, startTime + 39000 + 19999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 20000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), new object[]{events[12]});

	            SendTimer(env, startTime + 39000 + 20001);
	            env.AssertListenerNotInvoked("s0");

	            // Send no events, let the timer to do NO batch
	            SendTimer(env, startTime + 39000 + 29999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 30000);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 30001);
	            env.AssertListenerNotInvoked("s0");

	            // Send 1 more event
	            SendTimer(env, startTime + 90000);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[13]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 99999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 100000);
	            AssertUnderlyingIR(env, new object[]{events[13]}, Array.Empty<object>());

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchForceOutputOne : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = new string[] {"symbol"};
	            SendTimer(env, 1000);

	            var text = "@name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3, 'force_update')";
	            env.CompileDeployAddListenerMileZero(text, "s0");

	            SendTimer(env, 1000);
	            SendEvent(env, "E1");

	            env.Milestone(1);

	            SendTimer(env, 5000);
	            SendEvent(env, "E2");

	            env.Milestone(2);

	            SendTimer(env, 10999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, 11000);
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[]{"E1"}, new object[] {"E2"}}, null);

	            env.Milestone(3);

	            SendTimer(env, 12000);
	            SendEvent(env, "E3");
	            SendEvent(env, "E4");

	            env.Milestone(4);

	            SendTimer(env, 15000);
	            SendEvent(env, "E5");
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[]{"E3"}, new object[] {"E4"}, new object[] {"E5"}}, new object[][]{new object[]{"E1"}, new object[] {"E2"}});

	            env.Milestone(5);

	            SendTimer(env, 24999);
	            env.AssertListenerNotInvoked("s0");

	            // wait 10 second, check call
	            SendTimer(env, 25000);
	            env.AssertPropsPerRowIRPair("s0", fields, null, new object[][]{new object[]{"E3"}, new object[] {"E4"}, new object[] {"E5"}});

	            env.Milestone(6);

	            // wait 10 second, check call, should receive event
	            SendTimer(env, 35000);
	            env.AssertPropsPerRowIRPair("s0", fields, null, null);

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchForceOutputTwo : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            var events = Get100Events();
	            SendTimer(env, startTime);

	            var epl = "@name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3, 'FORCE_UPDATE')";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            // Send 3 events in batch
	            env.SendEventBean(events[0]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[1]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[2]);
	            AssertUnderlyingIR(env, new object[]{events[0], events[1], events[2]}, Array.Empty<object>());

	            // Send another 3 events in batch
	            env.SendEventBean(events[3]);
	            env.SendEventBean(events[4]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[5]);
	            AssertUnderlyingIR(env, new object[]{events[3], events[4], events[5]}, new object[]{events[0], events[1], events[2]});

	            // Expire the last 3 events by moving time
	            SendTimer(env, startTime + 9999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 10000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), new object[]{events[3], events[4], events[5]});

	            SendTimer(env, startTime + 10001);
	            env.AssertListenerNotInvoked("s0");

	            // Send an event, let the timer send the batch
	            SendTimer(env, startTime + 10100);
	            env.SendEventBean(events[6]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 19999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 20000);
	            AssertUnderlyingIR(env, new object[]{events[6]}, Array.Empty<object>());

	            SendTimer(env, startTime + 20001);
	            env.AssertListenerNotInvoked("s0");

	            // Send two events, let the timer send the batch
	            SendTimer(env, startTime + 29998);
	            env.SendEventBean(events[7]);
	            env.SendEventBean(events[8]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 29999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 30000);
	            AssertUnderlyingIR(env, new object[]{events[7], events[8]}, new object[]{events[6]});

	            // Send three events, the the 3 events batch
	            SendTimer(env, startTime + 30001);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[9]);
	            env.SendEventBean(events[10]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[11]);
	            AssertUnderlyingIR(env, new object[]{events[9], events[10], events[11]}, new object[]{events[7], events[8]});

	            // Send 1 event, let the timer to do the batch
	            SendTimer(env, startTime + 39000 + 9999);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[12]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 10000);
	            AssertUnderlyingIR(env, new object[]{events[12]}, new object[]{events[9], events[10], events[11]});

	            SendTimer(env, startTime + 39000 + 10001);
	            env.AssertListenerNotInvoked("s0");

	            // Send no events, let the timer to do the batch
	            SendTimer(env, startTime + 39000 + 19999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 20000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), new object[]{events[12]});

	            SendTimer(env, startTime + 39000 + 20001);
	            env.AssertListenerNotInvoked("s0");

	            // Send no events, let the timer do a batch
	            SendTimer(env, startTime + 39000 + 29999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 30000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), Array.Empty<object>());

	            SendTimer(env, startTime + 39000 + 30001);
	            env.AssertListenerNotInvoked("s0");

	            // Send no events, let the timer do a batch
	            SendTimer(env, startTime + 39000 + 39999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 39000 + 40000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), Array.Empty<object>());

	            SendTimer(env, startTime + 39000 + 40001);
	            env.AssertListenerNotInvoked("s0");

	            // Send 1 more event
	            SendTimer(env, startTime + 80000);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(events[13]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 88999);   // 10 sec from last batch
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 89000);
	            AssertUnderlyingIR(env, new object[]{events[13]}, Array.Empty<object>());

	            // Send 3 more events
	            SendTimer(env, startTime + 90000);
	            env.SendEventBean(events[14]);
	            env.SendEventBean(events[15]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 92000);
	            env.SendEventBean(events[16]);
	            AssertUnderlyingIR(env, new object[]{events[14], events[15], events[16]}, new object[]{events[13]});

	            // Send no events, let the timer do a batch
	            SendTimer(env, startTime + 101999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 102000);
	            AssertUnderlyingIR(env, Array.Empty<object>(), new object[]{events[14], events[15], events[16]});

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchForceOutputSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            SendTimer(env, startTime);
	            var events = Get100Events();

	            var epl = "@name('s0') select sum(price) from SupportMarketDataBean#time_length_batch(10 sec, 3, 'FORCE_UPDATE')";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            // Send 1 events in batch
	            env.SendEventBean(events[10]);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 10000);
	            AssertPrice(env, 10.0);

	            SendTimer(env, startTime + 20000);
	            AssertPrice(env, null);

	            SendTimer(env, startTime + 30000);
	            AssertPrice(env, null);

	            SendTimer(env, startTime + 40000);
	            AssertPrice(env, null);

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchStartEager : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = new string[] {"symbol"};
	            SendTimer(env, 1000);

	            var text = "@name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3, 'start_eager')";
	            env.CompileDeployAddListenerMileZero(text, "s0");

	            SendTimer(env, 10999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, 11000);
	            env.AssertPropsPerRowIRPair("s0", fields, null, null);

	            env.Milestone(1);

	            // Time period without events
	            SendTimer(env, 20999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, 21000);
	            env.AssertPropsPerRowIRPair("s0", fields, null, null);

	            // 3 events in batch
	            SendTimer(env, 22000);
	            SendEvent(env, "E1");
	            SendEvent(env, "E2");

	            env.Milestone(2);

	            SendTimer(env, 25000);
	            SendEvent(env, "E3");
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[]{"E1"}, new object[] {"E2"}, new object[] {"E3"}}, null);

	            env.Milestone(3);

	            // Time period without events
	            SendTimer(env, 34999);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, 35000);
	            env.AssertPropsPerRowIRPair("s0", fields, null, new object[][]{new object[]{"E1"}, new object[] {"E2"}, new object[] {"E3"}});

	            env.Milestone(4);

	            // 1 events in time period
	            SendTimer(env, 44999);
	            SendEvent(env, "E4");

	            env.Milestone(5);

	            SendTimer(env, 45000);
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[]{"E4"}}, null);

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchForceOutputStartEagerSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            SendTimer(env, startTime);
	            var events = Get100Events();

	            var epl = "@name('s0') select sum(price) from SupportMarketDataBean#time_length_batch(10 sec, 3, 'force_update, start_eager')";
	            env.CompileDeployAddListenerMileZero(epl, "s0");
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 9999);
	            env.AssertListenerNotInvoked("s0");

	            // Send batch off
	            SendTimer(env, startTime + 10000);
	            AssertPrice(env, null);

	            // Send batch off
	            SendTimer(env, startTime + 20000);
	            AssertPrice(env, null);

	            env.SendEventBean(events[11]);
	            env.SendEventBean(events[12]);
	            SendTimer(env, startTime + 30000);
	            AssertPrice(env, 23.0);

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchForceOutputStartNoEagerSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            SendTimer(env, startTime);

	            var epl = "@name('s0') select sum(price) from SupportMarketDataBean#time_length_batch(10 sec, 3, 'force_update')";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            // No batch as we are not start eager
	            SendTimer(env, startTime + 10000);
	            env.AssertListenerNotInvoked("s0");

	            // No batch as we are not start eager
	            SendTimer(env, startTime + 20000);
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchPreviousAndPrior : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            SendTimer(env, startTime);
	            var premades = Get100Events();

	            var epl = "@name('s0') select price, prev(1, price) as prevPrice, prior(1, price) as priorPrice from SupportMarketDataBean#time_length_batch(10 sec, 3)";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            // Send 3 events in batch
	            env.SendEventBean(premades[0]);
	            env.SendEventBean(premades[1]);
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(premades[2]);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(1, listener.NewDataList.Count);
	                var events = listener.LastNewData;
	                AssertData(events[0], 0, null, null);
	                AssertData(events[1], 1.0, 0.0, 0.0);
	                AssertData(events[2], 2.0, 1.0, 1.0);
	                listener.Reset();
	            });

	            env.UndeployAll();
	        }
	    }

	    internal class ViewTimeLengthBatchGroupBySumStartEager : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            long startTime = 1000;
	            SendTimer(env, startTime);

	            var epl = "@name('s0') select symbol, sum(price) as s from SupportMarketDataBean#time_length_batch(5, 10, \"START_EAGER\") group by symbol order by symbol asc";
	            env.CompileDeployAddListenerMileZero(epl, "s0");

	            SendTimer(env, startTime + 4000);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 6000);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(1, listener.NewDataList.Count);
	                var events = listener.LastNewData;
	                Assert.IsNull(events);
	                listener.Reset();
	            });

	            SendTimer(env, startTime + 7000);
	            env.SendEventBean(new SupportMarketDataBean("S1", "e1", 10d));

	            SendTimer(env, startTime + 8000);
	            env.SendEventBean(new SupportMarketDataBean("S2", "e2", 77d));

	            SendTimer(env, startTime + 9000);
	            env.SendEventBean(new SupportMarketDataBean("S1", "e3", 1d));

	            SendTimer(env, startTime + 10000);
	            env.AssertListenerNotInvoked("s0");

	            SendTimer(env, startTime + 11000);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(1, listener.NewDataList.Count);
	                var events = listener.LastNewData;
	                Assert.AreEqual(2, events.Length);
	                Assert.AreEqual("S1", events[0].Get("symbol"));
	                Assert.AreEqual(11d, events[0].Get("s"));
	                Assert.AreEqual("S2", events[1].Get("symbol"));
	                Assert.AreEqual(77d, events[1].Get("s"));
	                listener.Reset();
	            });

	            env.UndeployAll();
	        }
	    }

	    private static void SendTimer(RegressionEnvironment env, long timeInMSec) {
	        env.AdvanceTime(timeInMSec);
	    }

	    private static void AssertData(EventBean theEvent, double price, double? prevPrice, double? priorPrice) {
	        Assert.AreEqual(price, theEvent.Get("price"));
	        Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
	        Assert.AreEqual(priorPrice, theEvent.Get("priorPrice"));
	    }

	    private static SupportMarketDataBean[] Get100Events() {
	        var events = new SupportMarketDataBean[100];
	        for (var i = 0; i < events.Length; i++) {
	            events[i] = new SupportMarketDataBean($"S{i}", $"id_{i}", i);
	        }
	        return events;
	    }

	    private static SupportMarketDataBean SendEvent(RegressionEnvironment env, string symbol) {
	        var bean = new SupportMarketDataBean(symbol, 0, 0L, null);
	        env.SendEventBean(bean);
	        return bean;
	    }

	    private static void AssertUnderlyingIR(RegressionEnvironment env, object[] newUnd, object[] oldUnd) {
	        env.AssertListener("s0", listener => {
	            Assert.AreEqual(1, listener.NewDataList.Count);
	            Assert.AreEqual(1, listener.OldDataList.Count);
	            EPAssertionUtil.AssertEqualsExactOrderUnderlying(newUnd, listener.NewDataListFlattened);
	            EPAssertionUtil.AssertEqualsExactOrderUnderlying(oldUnd, listener.OldDataListFlattened);
	            listener.Reset();
	        });
	    }

	    private static void AssertPrice(RegressionEnvironment env, double? v) {
	        env.AssertEqualsNew("s0", "sum(price)", v);
	    }
	}
} // end of namespace
