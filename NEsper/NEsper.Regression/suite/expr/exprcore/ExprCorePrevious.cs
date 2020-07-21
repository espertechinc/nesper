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

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCorePrevious {
	    public static ICollection<RegressionExecution> Executions() {
	        var execs = new List<RegressionExecution>();
	        execs.Add(new ExprCorePreviousLengthWindowWhere());
	        execs.Add(new ExprCorePreviousPrevStream());
	        execs.Add(new ExprCorePreviousLengthWindow());
	        execs.Add(new ExprCorePreviousTimeBatch());
	        execs.Add(new ExprCorePreviousPrevCountStar());
	        execs.Add(new ExprCorePreviousPerGroupTwoCriteria());
	        execs.Add(new ExprCorePreviousExprNameAndTypeAndSODA());
	        execs.Add(new ExprCorePreviousSortWindowPerGroup());
	        execs.Add(new ExprCorePreviousTimeBatchPerGroup());
	        execs.Add(new ExprCorePreviousLengthBatchPerGroup());
	        execs.Add(new ExprCorePreviousTimeWindowPerGroup());
	        execs.Add(new ExprCorePreviousExtTimeWindowPerGroup());
	        execs.Add(new ExprCorePreviousLengthWindowPerGroup());
	        execs.Add(new ExprCorePreviousTimeWindow());
	        execs.Add(new ExprCorePreviousExtTimedWindow());
	        execs.Add(new ExprCorePreviousTimeBatchWindow());
	        execs.Add(new ExprCorePreviousLengthBatch());
	        execs.Add(new ExprCorePreviousLengthWindowDynamic());
	        execs.Add(new ExprCorePreviousSortWindow());
	        execs.Add(new ExprCorePreviousExtTimedBatch());
	        execs.Add(new ExprCorePreviousPrevCountStarWithStaticMethod());
	        execs.Add(new ExprCorePreviousTimeBatchWindowJoin());
	        execs.Add(new ExprCorePreviousInvalid());
	        return execs;
	    }

	    private class ExprCorePreviousTimeBatch : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.AdvanceTime(0);

	            var text = "@name('s0') select irstream Symbol, " +
	                       "prev(1, Symbol) as prev1, " +
	                       "prevtail(Symbol) as prevtail, " +
	                       "prevcount(Symbol) as prevCountSym, " +
	                       "prevwindow(Symbol) as prevWindowSym " +
	                       "from SupportMarketDataBean#time_batch(1 sec)";
	            env.CompileDeploy(text).AddListener("s0");
	            var fields = new[]{"Symbol", "prev1", "prevtail", "prevCountSym", "prevWindowSym"};

	            env.AdvanceTime(1500);
	            env.SendEventBean(MakeMarketDataEvent("E1"));
	            env.AdvanceTime(1700);
	            env.SendEventBean(MakeMarketDataEvent("E2"));
	            env.AdvanceTime(2499);

	            env.Milestone(1);

	            env.AdvanceTime(2500);
	            var newEvents = env.Listener("s0").NewDataListFlattened;
	            EPAssertionUtil.AssertPropsPerRow(newEvents, fields,
	                new[] {
		                new object[]{"E1", null, "E1", 2L, new object[]{"E2", "E1"}},
		                new object[]{"E2", "E1", "E1", 2L, new object[]{"E2", "E1"}}
	                });
	            Assert.IsNull(env.Listener("s0").LastOldData);
	            env.Listener("s0").Reset();

	            env.Milestone(2);

	            env.AdvanceTime(2500);
	            env.SendEventBean(MakeMarketDataEvent("E3"));
	            env.SendEventBean(MakeMarketDataEvent("E4"));

	            env.Milestone(3);

	            env.AdvanceTime(2600);
	            env.SendEventBean(MakeMarketDataEvent("E5"));

	            env.Milestone(4);

	            env.AdvanceTime(3500);
	            var win = new object[]{"E5", "E4", "E3"};
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, fields,
	                new[] {
		                new object[]{"E3", null, "E3", 3L, win}, 
		                new object[]{"E4", "E3", "E3", 3L, win}, 
		                new object[]{"E5", "E4", "E3", 3L, win}
	                });
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").OldDataListFlattened, fields,
	                new[] {
		                new object[]{"E1", null, null, null, null},
		                new object[]{"E2", null, null, null, null}
	                });
	            env.Listener("s0").Reset();

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousExprNameAndTypeAndSODA : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select " +
	                      "prev(1,IntPrimitive), " +
	                      "prev(1,sb), " +
	                      "prevtail(1,IntPrimitive), " +
	                      "prevtail(1,sb), " +
	                      "prevwindow(IntPrimitive), " +
	                      "prevwindow(sb), " +
	                      "prevcount(IntPrimitive), " +
	                      "prevcount(sb) " +
	                      "from SupportBean#time(1 minutes) as sb";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E2", 2));
	            var resultBean = env.Listener("s0").NewDataListFlattened[1];

	            var rows = new[] {
		            new object[]{"prev(1,IntPrimitive)", typeof(int?)},
	                new object[]{"prev(1,sb)", typeof(SupportBean)},
	                new object[]{"prevtail(1,IntPrimitive)", typeof(int?)},
	                new object[]{"prevtail(1,sb)", typeof(SupportBean)},
	                new object[]{"prevwindow(IntPrimitive)", typeof(int?[])},
	                new object[]{"prevwindow(sb)", typeof(SupportBean[])},
	                new object[]{"prevcount(IntPrimitive)", typeof(long?)},
	                new object[]{"prevcount(sb)", typeof(long?)}
	            };
	            for (var i = 0; i < rows.Length; i++) {
	                var message = "For prop '" + rows[i][0] + "'";
	                var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
	                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
	                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
	                var result = resultBean.Get(prop.PropertyName);
	                Assert.AreEqual(prop.PropertyType, result.GetType(), message);
	            }

	            env.UndeployAll();

	            env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(1);
	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousPrevStream : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select prev(1, s0) as result, " +
	                      "prevtail(0, s0) as tailresult," +
	                      "prevwindow(s0) as windowresult," +
	                      "prevcount(s0) as countresult " +
	                      "from SupportBean_S0#length(2) as s0";
	            env.CompileDeploy(epl).AddListener("s0");

	            var fields = "result,tailresult,windowresult,countresult".SplitCsv();

	            var e1 = new SupportBean_S0(1);
	            env.SendEventBean(e1);

	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields,
	                null,
	                e1,
	                new object[]{e1},
	                1L);

	            var e2 = new SupportBean_S0(2);
	            env.SendEventBean(e2);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields,
	                e1,
	                e1,
	                new object[]{e2, e1},
	                2L);
	            Assert.AreEqual(typeof(SupportBean_S0), env.Statement("s0").EventType.GetPropertyType("result"));

	            var e3 = new SupportBean_S0(3);
	            env.SendEventBean(e3);
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields,
	                e2,
	                e2,
	                new object[]{e3, e2},
	                2L);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousPrevCountStarWithStaticMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream count(*) as total, " +
	                      "prev(" + typeof(ExprCorePrevious).Name + ".intToLong(count(*)) - 1, Price) as firstPrice from SupportMarketDataBean#time(60)";
	            env.CompileDeploy(epl).AddListener("s0");

	            AssertPrevCount(env);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousPrevCountStar : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream count(*) as total, " +
	                      "prev(count(*) - 1, Price) as firstPrice from SupportMarketDataBean#time(60)";
	            env.CompileDeploy(epl).AddListener("s0");

	            AssertPrevCount(env);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousPerGroupTwoCriteria : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select Symbol, feed, " +
	                      "prev(1, Price) as prevPrice, " +
	                      "prevtail(Price) as tailPrice, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol, feed)#length(2)";
	            env.CompileDeploy(epl).AddListener("s0");

	            var fields = "Symbol,feed,prevPrice,tailPrice,countPrice,windowPrice".SplitCsv();

	            env.SendEventBean(new SupportMarketDataBean("IBM", 10, 0L, "F1"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "IBM", "F1", null, 10d, 1L, SplitDoubles("10d"));

	            env.SendEventBean(new SupportMarketDataBean("IBM", 11, 0L, "F1"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "IBM", "F1", 10d, 10d, 2L, SplitDoubles("11d,10d"));

	            env.SendEventBean(new SupportMarketDataBean("MSFT", 100, 0L, "F2"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "MSFT", "F2", null, 100d, 1L, SplitDoubles("100d"));

	            env.SendEventBean(new SupportMarketDataBean("IBM", 12, 0L, "F2"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "IBM", "F2", null, 12d, 1L, SplitDoubles("12d"));

	            env.SendEventBean(new SupportMarketDataBean("IBM", 13, 0L, "F1"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "IBM", "F1", 11d, 11d, 2L, SplitDoubles("13d,11d"));

	            env.SendEventBean(new SupportMarketDataBean("MSFT", 101, 0L, "F2"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "MSFT", "F2", 100d, 100d, 2L, SplitDoubles("101d,100d"));

	            env.SendEventBean(new SupportMarketDataBean("IBM", 17, 0L, "F2"));
	            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "IBM", "F2", 12d, 12d, 2L, SplitDoubles("17d,12d"));

	            env.UndeployAll();

	            // test length window overflow
	            env.CompileDeployAddListenerMile("@name('s0') select prev(5,IntPrimitive) as val0 from SupportBean#groupwin(TheString)#length(5)", "s0", 1);

	            env.SendEventBean(new SupportBean("A", 11));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("A", 12));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("A", 13));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("A", 14));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("A", 15));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("C", 20));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("C", 21));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("C", 22));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("C", 23));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("C", 24));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("B", 31));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("C", 25));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.SendEventBean(new SupportBean("A", 16));
	            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousSortWindowPerGroup : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // descending sort
	            var epl = "@name('s0')select " +
	                      "Symbol, " +
	                      "prev(1, Price) as prevPrice, " +
	                      "prev(2, Price) as prevPrevPrice, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol)#sort(10, Price asc)";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrevPrice"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail0Price"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail1Price"));
	            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("countPrice"));
	            Assert.AreEqual(typeof(double?[]), env.Statement("s0").EventType.GetPropertyType("windowPrice"));

	            SendMarketEvent(env, "IBM", 75);
	            AssertReceived(env, "IBM", null, null, 75d, null, 1L, SplitDoubles("75d"));
	            SendMarketEvent(env, "IBM", 80);
	            AssertReceived(env, "IBM", 80d, null, 80d, 75d, 2L, SplitDoubles("75d,80d"));
	            SendMarketEvent(env, "IBM", 79);
	            AssertReceived(env, "IBM", 79d, 80d, 80d, 79d, 3L, SplitDoubles("75d,79d,80d"));
	            SendMarketEvent(env, "IBM", 81);
	            AssertReceived(env, "IBM", 79d, 80d, 81d, 80d, 4L, SplitDoubles("75d,79d,80d,81d"));
	            SendMarketEvent(env, "IBM", 79.5);
	            AssertReceived(env, "IBM", 79d, 79.5d, 81d, 80d, 5L, SplitDoubles("75d,79d,79.5,80d,81d"));    // 75, 79, 79.5, 80, 81

	            SendMarketEvent(env, "MSFT", 10);
	            AssertReceived(env, "MSFT", null, null, 10d, null, 1L, SplitDoubles("10d"));
	            SendMarketEvent(env, "MSFT", 20);
	            AssertReceived(env, "MSFT", 20d, null, 20d, 10d, 2L, SplitDoubles("10d,20d"));
	            SendMarketEvent(env, "MSFT", 21);
	            AssertReceived(env, "MSFT", 20d, 21d, 21d, 20d, 3L, SplitDoubles("10d,20d,21d")); // 10, 20, 21

	            SendMarketEvent(env, "IBM", 74d);
	            AssertReceived(env, "IBM", 75d, 79d, 81d, 80d, 6L, SplitDoubles("74d,75d,79d,79.5,80d,81d"));  // 74, 75, 79, 79.5, 80, 81

	            SendMarketEvent(env, "MSFT", 19);
	            AssertReceived(env, "MSFT", 19d, 20d, 21d, 20d, 4L, SplitDoubles("10d,19d,20d,21d")); // 10, 19, 20, 21

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousTimeBatchPerGroup : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SendTimer(env, 0);

	            var epl = "@name('s0')select " +
	                      "Symbol, " +
	                      "prev(1, Price) as prevPrice, " +
	                      "prev(2, Price) as prevPrevPrice, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol)#time_batch(1 sec)";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrevPrice"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail0Price"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail1Price"));

	            SendMarketEvent(env, "IBM", 75);
	            SendMarketEvent(env, "MSFT", 40);
	            SendMarketEvent(env, "IBM", 76);
	            SendMarketEvent(env, "CIC", 1);
	            SendTimer(env, 1000);

	            var events = env.Listener("s0").LastNewData;
	            // order not guaranteed as timed batch, however for testing the order is reliable as schedule buckets are created
	            // in a predictable order
	            // Previous is looking at the same batch, doesn't consider outside of window
	            AssertReceived(events[0], "IBM", null, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
	            AssertReceived(events[1], "IBM", 75d, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
	            AssertReceived(events[2], "MSFT", null, null, 40d, null, 1L, SplitDoubles("40d"));
	            AssertReceived(events[3], "CIC", null, null, 1d, null, 1L, SplitDoubles("1d"));

	            // Next batch, previous is looking only within the same batch
	            SendMarketEvent(env, "MSFT", 41);
	            SendMarketEvent(env, "IBM", 77);
	            SendMarketEvent(env, "IBM", 78);
	            SendMarketEvent(env, "CIC", 2);
	            SendMarketEvent(env, "MSFT", 42);
	            SendMarketEvent(env, "CIC", 3);
	            SendMarketEvent(env, "CIC", 4);
	            SendTimer(env, 2000);

	            events = env.Listener("s0").LastNewData;
	            AssertReceived(events[0], "IBM", null, null, 77d, 78d, 2L, SplitDoubles("78d,77d"));
	            AssertReceived(events[1], "IBM", 77d, null, 77d, 78d, 2L, SplitDoubles("78d,77d"));
	            AssertReceived(events[2], "MSFT", null, null, 41d, 42d, 2L, SplitDoubles("42d,41d"));
	            AssertReceived(events[3], "MSFT", 41d, null, 41d, 42d, 2L, SplitDoubles("42d,41d"));
	            AssertReceived(events[4], "CIC", null, null, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
	            AssertReceived(events[5], "CIC", 2d, null, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
	            AssertReceived(events[6], "CIC", 3d, 2d, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousLengthBatchPerGroup : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // Also testing the alternative syntax here of "prev(property)" and "prev(property, index)" versus "prev(index, property)"
	            var epl = "@name('s0')select irstream " +
	                      "Symbol, " +
	                      "prev(Price) as prevPrice, " +
	                      "prev(Price, 2) as prevPrevPrice, " +
	                      "prevtail(Price, 0) as prevTail0Price, " +
	                      "prevtail(Price, 1) as prevTail1Price, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol)#length_batch(3)";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrevPrice"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail0Price"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail1Price"));

	            SendMarketEvent(env, "IBM", 75);
	            SendMarketEvent(env, "MSFT", 50);
	            SendMarketEvent(env, "IBM", 76);
	            SendMarketEvent(env, "CIC", 1);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);
	            SendMarketEvent(env, "IBM", 77);

	            var eventsNew = env.Listener("s0").LastNewData;
	            Assert.AreEqual(3, eventsNew.Length);
	            AssertReceived(eventsNew[0], "IBM", null, null, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
	            AssertReceived(eventsNew[1], "IBM", 75d, null, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
	            AssertReceived(eventsNew[2], "IBM", 76d, 75d, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
	            env.Listener("s0").Reset();

	            // Next batch, previous is looking only within the same batch
	            SendMarketEvent(env, "MSFT", 51);
	            SendMarketEvent(env, "IBM", 78);
	            SendMarketEvent(env, "IBM", 79);
	            SendMarketEvent(env, "CIC", 2);
	            SendMarketEvent(env, "CIC", 3);

	            eventsNew = env.Listener("s0").LastNewData;
	            Assert.AreEqual(3, eventsNew.Length);
	            AssertReceived(eventsNew[0], "CIC", null, null, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
	            AssertReceived(eventsNew[1], "CIC", 1d, null, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
	            AssertReceived(eventsNew[2], "CIC", 2d, 1d, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "MSFT", 52);

	            eventsNew = env.Listener("s0").LastNewData;
	            Assert.AreEqual(3, eventsNew.Length);
	            AssertReceived(eventsNew[0], "MSFT", null, null, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
	            AssertReceived(eventsNew[1], "MSFT", 50d, null, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
	            AssertReceived(eventsNew[2], "MSFT", 51d, 50d, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "IBM", 80);

	            eventsNew = env.Listener("s0").LastNewData;
	            var eventsOld = env.Listener("s0").LastOldData;
	            Assert.AreEqual(3, eventsNew.Length);
	            Assert.AreEqual(3, eventsOld.Length);
	            AssertReceived(eventsNew[0], "IBM", null, null, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
	            AssertReceived(eventsNew[1], "IBM", 78d, null, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
	            AssertReceived(eventsNew[2], "IBM", 79d, 78d, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
	            AssertReceived(eventsOld[0], "IBM", null, null, null, null, null, null);
	            AssertReceived(eventsOld[1], "IBM", null, null, null, null, null, null);
	            AssertReceived(eventsOld[2], "IBM", null, null, null, null, null, null);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousTimeWindowPerGroup : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select " +
	                      "Symbol, " +
	                      "prev(1, Price) as prevPrice, " +
	                      "prev(2, Price) as prevPrevPrice, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol)#time(20 sec) ";
	            AssertPerGroup(epl, env);
	        }
	    }

	    private class ExprCorePreviousExtTimeWindowPerGroup : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select " +
	                      "Symbol, " +
	                      "prev(1, Price) as prevPrice, " +
	                      "prev(2, Price) as prevPrevPrice, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol)#ext_timed(Volume, 20 sec) ";
	            AssertPerGroup(epl, env);
	        }
	    }

	    private class ExprCorePreviousLengthWindowPerGroup : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select Symbol, " +
	                      "prev(1, Price) as prevPrice, " +
	                      "prev(2, Price) as prevPrevPrice, " +
	                      "prevtail(Price, 0) as prevTail0Price, " +
	                      "prevtail(Price, 1) as prevTail1Price, " +
	                      "prevcount(Price) as countPrice, " +
	                      "prevwindow(Price) as windowPrice " +
	                      "from SupportMarketDataBean#groupwin(Symbol)#length(10) ";
	            AssertPerGroup(epl, env);
	        }
	    }

	    private class ExprCorePreviousTimeWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream Symbol as currSymbol, " +
	                      " prev(2, Symbol) as prevSymbol, " +
	                      " prev(2, Price) as prevPrice, " +
	                      " prevtail(0, Symbol) as prevTailSymbol, " +
	                      " prevtail(0, Price) as prevTailPrice, " +
	                      " prevtail(1, Symbol) as prevTail1Symbol, " +
	                      " prevtail(1, Price) as prevTail1Price, " +
	                      " prevcount(Price) as prevCountPrice, " +
	                      " prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#time(1 min) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prevSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));

	            SendTimer(env, 0);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D1", 1);
	            AssertNewEventWTail(env, "D1", null, null, "D1", 1d, null, null, 1L, SplitDoubles("1d"));

	            SendTimer(env, 1000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D2", 2);
	            AssertNewEventWTail(env, "D2", null, null, "D1", 1d, "D2", 2d, 2L, SplitDoubles("2d,1d"));

	            SendTimer(env, 2000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D3", 3);
	            AssertNewEventWTail(env, "D3", "D1", 1d, "D1", 1d, "D2", 2d, 3L, SplitDoubles("3d,2d,1d"));

	            SendTimer(env, 3000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D4", 4);
	            AssertNewEventWTail(env, "D4", "D2", 2d, "D1", 1d, "D2", 2d, 4L, SplitDoubles("4d,3d,2d,1d"));

	            SendTimer(env, 4000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D5", 5);
	            AssertNewEventWTail(env, "D5", "D3", 3d, "D1", 1d, "D2", 2d, 5L, SplitDoubles("5d,4d,3d,2d,1d"));

	            SendTimer(env, 30000);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "D6", 6);
	            AssertNewEventWTail(env, "D6", "D4", 4d, "D1", 1d, "D2", 2d, 6L, SplitDoubles("6d,5d,4d,3d,2d,1d"));

	            // Test remove stream, always returns null as previous function
	            // returns null for remove stream for time windows
	            SendTimer(env, 60000);
	            AssertOldEventWTail(env, "D1", null, null, null, null, null, null, null, null);
	            SendTimer(env, 61000);
	            AssertOldEventWTail(env, "D2", null, null, null, null, null, null, null, null);
	            SendTimer(env, 62000);
	            AssertOldEventWTail(env, "D3", null, null, null, null, null, null, null, null);
	            SendTimer(env, 63000);
	            AssertOldEventWTail(env, "D4", null, null, null, null, null, null, null, null);
	            SendTimer(env, 64000);
	            AssertOldEventWTail(env, "D5", null, null, null, null, null, null, null, null);
	            SendTimer(env, 90000);
	            AssertOldEventWTail(env, "D6", null, null, null, null, null, null, null, null);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousExtTimedWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream Symbol as currSymbol, " +
	                      " prev(2, Symbol) as prevSymbol, " +
	                      " prev(2, Price) as prevPrice, " +
	                      " prevtail(0, Symbol) as prevTailSymbol, " +
	                      " prevtail(0, Price) as prevTailPrice, " +
	                      " prevtail(1, Symbol) as prevTail1Symbol, " +
	                      " prevtail(1, Price) as prevTail1Price, " +
	                      " prevcount(Price) as prevCountPrice, " +
	                      " prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#ext_timed(Volume, 1 min) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prevSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prevTailSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTailPrice"));

	            SendMarketEvent(env, "D1", 1, 0);
	            AssertNewEventWTail(env, "D1", null, null, "D1", 1d, null, null, 1L, SplitDoubles("1d"));

	            SendMarketEvent(env, "D2", 2, 1000);
	            AssertNewEventWTail(env, "D2", null, null, "D1", 1d, "D2", 2d, 2L, SplitDoubles("2d,1d"));

	            SendMarketEvent(env, "D3", 3, 3000);
	            AssertNewEventWTail(env, "D3", "D1", 1d, "D1", 1d, "D2", 2d, 3L, SplitDoubles("3d,2d,1d"));

	            SendMarketEvent(env, "D4", 4, 4000);
	            AssertNewEventWTail(env, "D4", "D2", 2d, "D1", 1d, "D2", 2d, 4L, SplitDoubles("4d,3d,2d,1d"));

	            SendMarketEvent(env, "D5", 5, 5000);
	            AssertNewEventWTail(env, "D5", "D3", 3d, "D1", 1d, "D2", 2d, 5L, SplitDoubles("5d,4d,3d,2d,1d"));

	            SendMarketEvent(env, "D6", 6, 30000);
	            AssertNewEventWTail(env, "D6", "D4", 4d, "D1", 1d, "D2", 2d, 6L, SplitDoubles("6d,5d,4d,3d,2d,1d"));

	            SendMarketEvent(env, "D7", 7, 60000);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "D7", "D5", 5d, "D2", 2d, "D3", 3d, 6L, SplitDoubles("7d,6d,5d,4d,3d,2d"));
	            AssertEventWTail(env.Listener("s0").LastOldData[0], "D1", null, null, null, null, null, null, null, null);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "D8", 8, 61000);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "D8", "D6", 6d, "D3", 3d, "D4", 4d, 6L, SplitDoubles("8d,7d,6d,5d,4d,3d"));
	            AssertEventWTail(env.Listener("s0").LastOldData[0], "D2", null, null, null, null, null, null, null, null);
	            env.Listener("s0").Reset();

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousTimeBatchWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream Symbol as currSymbol, " +
	                      " prev(2, Symbol) as prevSymbol, " +
	                      " prev(2, Price) as prevPrice, " +
	                      " prevtail(0, Symbol) as prevTailSymbol, " +
	                      " prevtail(0, Price) as prevTailPrice, " +
	                      " prevtail(1, Symbol) as prevTail1Symbol, " +
	                      " prevtail(1, Price) as prevTail1Price, " +
	                      " prevcount(Price) as prevCountPrice, " +
	                      " prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#time_batch(1 min) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prevSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));

	            SendTimer(env, 0);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "A", 1);
	            SendMarketEvent(env, "B", 2);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 60000);
	            Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "A", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[1], "B", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
	            Assert.IsNull(env.Listener("s0").LastOldData);
	            env.Listener("s0").Reset();

	            SendTimer(env, 80000);
	            SendMarketEvent(env, "C", 3);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 120000);
	            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "C", null, null, "C", 3d, null, null, 1L, SplitDoubles("3d"));
	            Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
	            AssertEventWTail(env.Listener("s0").LastOldData[0], "A", null, null, null, null, null, null, null, null);
	            env.Listener("s0").Reset();

	            SendTimer(env, 300000);
	            SendMarketEvent(env, "D", 4);
	            SendMarketEvent(env, "E", 5);
	            SendMarketEvent(env, "F", 6);
	            SendMarketEvent(env, "G", 7);
	            SendTimer(env, 360000);
	            Assert.AreEqual(4, env.Listener("s0").LastNewData.Length);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "D", null, null, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[1], "E", null, null, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[2], "F", "D", 4d, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[3], "G", "E", 5d, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousTimeBatchWindowJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select TheString as currSymbol, " +
	                      " prev(2, Symbol) as prevSymbol, " +
	                      " prev(1, Price) as prevPrice, " +
	                      " prevtail(0, Symbol) as prevTailSymbol, " +
	                      " prevtail(0, Price) as prevTailPrice, " +
	                      " prevtail(1, Symbol) as prevTail1Symbol, " +
	                      " prevtail(1, Price) as prevTail1Price, " +
	                      " prevcount(Price) as prevCountPrice, " +
	                      " prevwindow(Price) as prevWindowPrice " +
	                      "from SupportBean#keepall, SupportMarketDataBean#time_batch(1 min)";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prevSymbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));

	            SendTimer(env, 0);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "A", 1);
	            SendMarketEvent(env, "B", 2);
	            SendBeanEvent(env, "X1");
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 60000);
	            Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "X1", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[1], "X1", null, 1d, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
	            Assert.IsNull(env.Listener("s0").LastOldData);
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "C1", 11);
	            SendMarketEvent(env, "C2", 12);
	            SendMarketEvent(env, "C3", 13);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendTimer(env, 120000);
	            Assert.AreEqual(3, env.Listener("s0").LastNewData.Length);
	            AssertEventWTail(env.Listener("s0").LastNewData[0], "X1", null, null, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[1], "X1", null, 11d, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
	            AssertEventWTail(env.Listener("s0").LastNewData[2], "X1", "C1", 12d, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousLengthWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream Symbol as currSymbol, " +
	                      "prev(0, Symbol) as prev0Symbol, " +
	                      "prev(1, Symbol) as prev1Symbol, " +
	                      "prev(2, Symbol) as prev2Symbol, " +
	                      "prev(0, Price) as prev0Price, " +
	                      "prev(1, Price) as prev1Price, " +
	                      "prev(2, Price) as prev2Price," +
	                      "prevtail(0, Symbol) as prevTail0Symbol, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Symbol) as prevTail1Symbol, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as prevCountPrice, " +
	                      "prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#length(3) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prev0Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prev0Price"));

	            SendMarketEvent(env, "A", 1);
	            AssertNewEvents(env, "A", "A", 1d, null, null, null, null, "A", 1d, null, null, 1L, SplitDoubles("1d"));

	            env.Milestone(1);

	            SendMarketEvent(env, "B", 2);
	            AssertNewEvents(env, "B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));

	            env.Milestone(2);

	            SendMarketEvent(env, "C", 3);
	            AssertNewEvents(env, "C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));

	            env.Milestone(3);

	            SendMarketEvent(env, "D", 4);
	            var newEvent = env.Listener("s0").LastNewData[0];
	            var oldEvent = env.Listener("s0").LastOldData[0];
	            AssertEventProps(env, newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "B", 2d, "C", 3d, 3L, SplitDoubles("4d,3d,2d"));
	            AssertEventProps(env, oldEvent, "A", null, null, null, null, null, null, null, null, null, null, null, null);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousLengthBatch : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select irstream Symbol as currSymbol, " +
	                      "prev(0, Symbol) as prev0Symbol, " +
	                      "prev(1, Symbol) as prev1Symbol, " +
	                      "prev(2, Symbol) as prev2Symbol, " +
	                      "prev(0, Price) as prev0Price, " +
	                      "prev(1, Price) as prev1Price, " +
	                      "prev(2, Price) as prev2Price, " +
	                      "prevtail(0, Symbol) as prevTail0Symbol, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Symbol) as prevTail1Symbol, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as prevCountPrice, " +
	                      "prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#length_batch(3) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            // assert select result type
	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prev0Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prev0Price"));

	            SendMarketEvent(env, "A", 1);
	            SendMarketEvent(env, "B", 2);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "C", 3);
	            var newEvents = env.Listener("s0").LastNewData;
	            Assert.AreEqual(3, newEvents.Length);
	            AssertEventProps(env, newEvents[0], "A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
	            AssertEventProps(env, newEvents[1], "B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
	            AssertEventProps(env, newEvents[2], "C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
	            env.Listener("s0").Reset();

	            SendMarketEvent(env, "D", 4);
	            SendMarketEvent(env, "E", 5);
	            Assert.IsFalse(env.Listener("s0").IsInvoked);

	            SendMarketEvent(env, "F", 6);
	            newEvents = env.Listener("s0").LastNewData;
	            var oldEvents = env.Listener("s0").LastOldData;
	            Assert.AreEqual(3, newEvents.Length);
	            Assert.AreEqual(3, oldEvents.Length);
	            AssertEventProps(env, newEvents[0], "D", "D", 4d, null, null, null, null, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
	            AssertEventProps(env, newEvents[1], "E", "E", 5d, "D", 4d, null, null, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
	            AssertEventProps(env, newEvents[2], "F", "F", 6d, "E", 5d, "D", 4d, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
	            AssertEventProps(env, oldEvents[0], "A", null, null, null, null, null, null, null, null, null, null, null, null);
	            AssertEventProps(env, oldEvents[1], "B", null, null, null, null, null, null, null, null, null, null, null, null);
	            AssertEventProps(env, oldEvents[2], "C", null, null, null, null, null, null, null, null, null, null, null, null);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousLengthWindowWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select prev(2, Symbol) as currSymbol " +
	                      "from SupportMarketDataBean#length(100) " +
	                      "where prev(2, Price) > 100";
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

	    private class ExprCorePreviousLengthWindowDynamic : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select prev(IntPrimitive, TheString) as sPrev " +
	                      "from SupportBean#length(100)";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendBeanEvent(env, "A", 1);
	            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
	            Assert.AreEqual(null, theEvent.Get("sPrev"));

	            SendBeanEvent(env, "B", 0);
	            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
	            Assert.AreEqual("B", theEvent.Get("sPrev"));

	            SendBeanEvent(env, "C", 2);
	            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
	            Assert.AreEqual("A", theEvent.Get("sPrev"));

	            SendBeanEvent(env, "D", 1);
	            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
	            Assert.AreEqual("C", theEvent.Get("sPrev"));

	            SendBeanEvent(env, "E", 4);
	            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
	            Assert.AreEqual("A", theEvent.Get("sPrev"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousSortWindow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')select Symbol as currSymbol, " +
	                      " prev(0, Symbol) as prev0Symbol, " +
	                      " prev(1, Symbol) as prev1Symbol, " +
	                      " prev(2, Symbol) as prev2Symbol, " +
	                      " prev(0, Price) as prev0Price, " +
	                      " prev(1, Price) as prev1Price, " +
	                      " prev(2, Price) as prev2Price, " +
	                      " prevtail(0, Symbol) as prevTail0Symbol, " +
	                      " prevtail(0, Price) as prevTail0Price, " +
	                      " prevtail(1, Symbol) as prevTail1Symbol, " +
	                      " prevtail(1, Price) as prevTail1Price, " +
	                      " prevcount(Price) as prevCountPrice, " +
	                      " prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#sort(100, Symbol asc)";
	            env.CompileDeploy(epl).AddListener("s0");

	            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("prev0Symbol"));
	            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prev0Price"));

	            SendMarketEvent(env, "COX", 30);
	            AssertNewEvents(env, "COX", "COX", 30d, null, null, null, null, "COX", 30d, null, null, 1L, SplitDoubles("30d"));

	            SendMarketEvent(env, "IBM", 45);
	            AssertNewEvents(env, "IBM", "COX", 30d, "IBM", 45d, null, null, "IBM", 45d, "COX", 30d, 2L, SplitDoubles("30d,45d"));

	            SendMarketEvent(env, "MSFT", 33);
	            AssertNewEvents(env, "MSFT", "COX", 30d, "IBM", 45d, "MSFT", 33d, "MSFT", 33d, "IBM", 45d, 3L, SplitDoubles("30d,45d,33d"));

	            SendMarketEvent(env, "XXX", 55);
	            AssertNewEvents(env, "XXX", "COX", 30d, "IBM", 45d, "MSFT", 33d, "XXX", 55d, "MSFT", 33d, 4L, SplitDoubles("30d,45d,33d,55d"));

	            SendMarketEvent(env, "CXX", 56);
	            AssertNewEvents(env, "CXX", "COX", 30d, "CXX", 56d, "IBM", 45d, "XXX", 55d, "MSFT", 33d, 5L, SplitDoubles("30d,56d,45d,33d,55d"));

	            SendMarketEvent(env, "GE", 1);
	            AssertNewEvents(env, "GE", "COX", 30d, "CXX", 56d, "GE", 1d, "XXX", 55d, "MSFT", 33d, 6L, SplitDoubles("30d,56d,1d,45d,33d,55d"));

	            SendMarketEvent(env, "AAA", 1);
	            AssertNewEvents(env, "AAA", "AAA", 1d, "COX", 30d, "CXX", 56d, "XXX", 55d, "MSFT", 33d, 7L, SplitDoubles("1d,30d,56d,1d,45d,33d,55d"));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousExtTimedBatch : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "currSymbol,prev0Symbol,prev0Price,prev1Symbol,prev1Price,prev2Symbol,prev2Price,prevTail0Symbol,prevTail0Price,prevTail1Symbol,prevTail1Price,prevCountPrice,prevWindowPrice".SplitCsv();
	            var epl = "@name('s0')select irstream Symbol as currSymbol, " +
	                      "prev(0, Symbol) as prev0Symbol, " +
	                      "prev(0, Price) as prev0Price, " +
	                      "prev(1, Symbol) as prev1Symbol, " +
	                      "prev(1, Price) as prev1Price, " +
	                      "prev(2, Symbol) as prev2Symbol, " +
	                      "prev(2, Price) as prev2Price," +
	                      "prevtail(0, Symbol) as prevTail0Symbol, " +
	                      "prevtail(0, Price) as prevTail0Price, " +
	                      "prevtail(1, Symbol) as prevTail1Symbol, " +
	                      "prevtail(1, Price) as prevTail1Price, " +
	                      "prevcount(Price) as prevCountPrice, " +
	                      "prevwindow(Price) as prevWindowPrice " +
	                      "from SupportMarketDataBean#ext_timed_batch(Volume, 10, 0L) ";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendMarketEvent(env, "A", 1, 1000);
	            SendMarketEvent(env, "B", 2, 1001);
	            SendMarketEvent(env, "C", 3, 1002);
	            SendMarketEvent(env, "D", 4, 10000);

	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), fields,
	                new[] {
	                    new object[]{"A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")},
	                    new object[]{"B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")},
	                    new object[]{"C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")}
	                },
	                null);

	            SendMarketEvent(env, "E", 5, 20000);

	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), fields,
	                new[] {
		                new object[]{"D", "D", 4d, null, null, null, null, "D", 4d, null, null, 1L, SplitDoubles("4d")},
	                },
	                new[] {
		                new object[]{"A", null, null, null, null, null, null, null, null, null, null, null, null},
		                new object[]{"B", null, null, null, null, null, null, null, null, null, null, null, null},
	                    new object[]{"C", null, null, null, null, null, null, null, null, null, null, null, null},
	                }
	            );

	            env.UndeployAll();
	        }
	    }

	    private class ExprCorePreviousInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            SupportMessageAssertUtil.TryInvalidCompile(env, "select prev(0, average) " +
	                    "from SupportMarketDataBean#length(100)#uni(Price)",
	                "Previous function requires a single data window view onto the stream [");

	            SupportMessageAssertUtil.TryInvalidCompile(env, "select count(*) from SupportBean#keepall where prev(0, IntPrimitive) = 5",
	                "The 'prev' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead [select count(*) from SupportBean#keepall where prev(0, IntPrimitive) = 5]");

	            SupportMessageAssertUtil.TryInvalidCompile(env, "select count(*) from SupportBean#keepall having prev(0, IntPrimitive) = 5",
	                "The 'prev' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead [select count(*) from SupportBean#keepall having prev(0, IntPrimitive) = 5]");
	        }
	    }

	    private static void AssertEventWTail(EventBean eventBean,
	                                         string currSymbol,
	                                         string prevSymbol,
	                                         double? prevPrice,
	                                         string prevTailSymbol,
	                                         double? prevTailPrice,
	                                         string prevTail1Symbol,
	                                         double? prevTail1Price,
	                                         long? prevcount,
	                                         object[] prevwindow) {
	        Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
	        Assert.AreEqual(prevSymbol, eventBean.Get("prevSymbol"));
	        Assert.AreEqual(prevPrice, eventBean.Get("prevPrice"));
	        Assert.AreEqual(prevTailSymbol, eventBean.Get("prevTailSymbol"));
	        Assert.AreEqual(prevTailPrice, eventBean.Get("prevTailPrice"));
	        Assert.AreEqual(prevTail1Symbol, eventBean.Get("prevTail1Symbol"));
	        Assert.AreEqual(prevTail1Price, eventBean.Get("prevTail1Price"));
	        Assert.AreEqual(prevcount, eventBean.Get("prevCountPrice"));
	        EPAssertionUtil.AssertEqualsExactOrder((object[]) eventBean.Get("prevWindowPrice"), prevwindow);
	    }

	    private static void AssertNewEvents(RegressionEnvironment env, string currSymbol,
	                                        string prev0Symbol,
	                                        double? prev0Price,
	                                        string prev1Symbol,
	                                        double? prev1Price,
	                                        string prev2Symbol,
	                                        double? prev2Price,
	                                        string prevTail0Symbol,
	                                        double? prevTail0Price,
	                                        string prevTail1Symbol,
	                                        double? prevTail1Price,
	                                        long? prevCount,
	                                        object[] prevWindow) {
	        var oldData = env.Listener("s0").LastOldData;
	        var newData = env.Listener("s0").LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);
	        AssertEventProps(env, newData[0], currSymbol, prev0Symbol, prev0Price, prev1Symbol, prev1Price, prev2Symbol, prev2Price,
	            prevTail0Symbol, prevTail0Price, prevTail1Symbol, prevTail1Price, prevCount, prevWindow);

	        env.Listener("s0").Reset();
	    }

	    private static void AssertEventProps(RegressionEnvironment env,
	                                         EventBean eventBean,
	                                         string currSymbol,
	                                         string prev0Symbol,
	                                         double? prev0Price,
	                                         string prev1Symbol,
	                                         double? prev1Price,
	                                         string prev2Symbol,
	                                         double? prev2Price,
	                                         string prevTail0Symbol,
	                                         double? prevTail0Price,
	                                         string prevTail1Symbol,
	                                         double? prevTail1Price,
	                                         long? prevCount,
	                                         object[] prevWindow) {
	        Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
	        Assert.AreEqual(prev0Symbol, eventBean.Get("prev0Symbol"));
	        Assert.AreEqual(prev0Price, eventBean.Get("prev0Price"));
	        Assert.AreEqual(prev1Symbol, eventBean.Get("prev1Symbol"));
	        Assert.AreEqual(prev1Price, eventBean.Get("prev1Price"));
	        Assert.AreEqual(prev2Symbol, eventBean.Get("prev2Symbol"));
	        Assert.AreEqual(prev2Price, eventBean.Get("prev2Price"));
	        Assert.AreEqual(prevTail0Symbol, eventBean.Get("prevTail0Symbol"));
	        Assert.AreEqual(prevTail0Price, eventBean.Get("prevTail0Price"));
	        Assert.AreEqual(prevTail1Symbol, eventBean.Get("prevTail1Symbol"));
	        Assert.AreEqual(prevTail1Price, eventBean.Get("prevTail1Price"));
	        Assert.AreEqual(prevCount, eventBean.Get("prevCountPrice"));
	        EPAssertionUtil.AssertEqualsExactOrder((object[]) eventBean.Get("prevWindowPrice"), prevWindow);

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

	    private static void SendBeanEvent(RegressionEnvironment env, string theString, int intPrimitive) {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        env.SendEventBean(bean);
	    }

	    private static void AssertNewEventWTail(RegressionEnvironment env, string currSymbol,
	                                            string prevSymbol,
	                                            double? prevPrice,
	                                            string prevTailSymbol,
	                                            double? prevTailPrice,
	                                            string prevTail1Symbol,
	                                            double? prevTail1Price,
	                                            long? prevcount,
	                                            object[] prevwindow) {
	        var oldData = env.Listener("s0").LastOldData;
	        var newData = env.Listener("s0").LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);

	        AssertEventWTail(newData[0], currSymbol, prevSymbol, prevPrice, prevTailSymbol, prevTailPrice, prevTail1Symbol, prevTail1Price, prevcount, prevwindow);

	        env.Listener("s0").Reset();
	    }

	    private static void AssertOldEventWTail(RegressionEnvironment env,
	                                            string currSymbol,
	                                            string prevSymbol,
	                                            double? prevPrice,
	                                            string prevTailSymbol,
	                                            double? prevTailPrice,
	                                            string prevTail1Symbol,
	                                            double? prevTail1Price,
	                                            long? prevcount,
	                                            object[] prevwindow) {
	        var oldData = env.Listener("s0").LastOldData;
	        var newData = env.Listener("s0").LastNewData;

	        Assert.IsNull(newData);
	        Assert.AreEqual(1, oldData.Length);

	        AssertEventWTail(oldData[0], currSymbol, prevSymbol, prevPrice, prevTailSymbol, prevTailPrice, prevTail1Symbol, prevTail1Price, prevcount, prevwindow);

	        env.Listener("s0").Reset();
	    }

	    private static void AssertPerGroup(string statement, RegressionEnvironment env) {
	        env.CompileDeploy(statement).AddListener("s0");

	        // assert select result type
	        Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrice"));
	        Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevPrevPrice"));
	        Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail0Price"));
	        Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("prevTail1Price"));
	        Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("countPrice"));
	        Assert.AreEqual(typeof(double?[]), env.Statement("s0").EventType.GetPropertyType("windowPrice"));

	        SendMarketEvent(env, "IBM", 75);
	        AssertReceived(env, "IBM", null, null, 75d, null, 1L, SplitDoubles("75d"));

	        SendMarketEvent(env, "MSFT", 40);
	        AssertReceived(env, "MSFT", null, null, 40d, null, 1L, SplitDoubles("40d"));

	        SendMarketEvent(env, "IBM", 76);
	        AssertReceived(env, "IBM", 75d, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));

	        SendMarketEvent(env, "CIC", 1);
	        AssertReceived(env, "CIC", null, null, 1d, null, 1L, SplitDoubles("1d"));

	        SendMarketEvent(env, "MSFT", 41);
	        AssertReceived(env, "MSFT", 40d, null, 40d, 41d, 2L, SplitDoubles("41d,40d"));

	        SendMarketEvent(env, "IBM", 77);
	        AssertReceived(env, "IBM", 76d, 75d, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));

	        SendMarketEvent(env, "IBM", 78);
	        AssertReceived(env, "IBM", 77d, 76d, 75d, 76d, 4L, SplitDoubles("78d,77d,76d,75d"));

	        SendMarketEvent(env, "CIC", 2);
	        AssertReceived(env, "CIC", 1d, null, 1d, 2d, 2L, SplitDoubles("2d,1d"));

	        SendMarketEvent(env, "MSFT", 42);
	        AssertReceived(env, "MSFT", 41d, 40d, 40d, 41d, 3L, SplitDoubles("42d,41d,40d"));

	        SendMarketEvent(env, "CIC", 3);
	        AssertReceived(env, "CIC", 2d, 1d, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));

	        env.UndeployAll();
	    }

	    private static void AssertReceived(RegressionEnvironment env, string symbol, double? prevPrice, double? prevPrevPrice,
	                                       double? prevTail1Price, double? prevTail2Price,
	                                       long? countPrice, object[] windowPrice) {
	        var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
	        AssertReceived(theEvent, symbol, prevPrice, prevPrevPrice, prevTail1Price, prevTail2Price, countPrice, windowPrice);
	    }

	    private static void AssertReceived(EventBean theEvent, string symbol, double? prevPrice, double? prevPrevPrice,
	                                       double? prevTail0Price, double? prevTail1Price,
	                                       long? countPrice, object[] windowPrice) {
	        Assert.AreEqual(symbol, theEvent.Get("Symbol"));
	        Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
	        Assert.AreEqual(prevPrevPrice, theEvent.Get("prevPrevPrice"));
	        Assert.AreEqual(prevTail0Price, theEvent.Get("prevTail0Price"));
	        Assert.AreEqual(prevTail1Price, theEvent.Get("prevTail1Price"));
	        Assert.AreEqual(countPrice, theEvent.Get("countPrice"));
	        EPAssertionUtil.AssertEqualsExactOrder(windowPrice, (object[]) theEvent.Get("windowPrice"));
	    }

	    private static void AssertCountAndPrice(EventBean theEvent, long? total, double? price) {
	        Assert.AreEqual(total, theEvent.Get("total"));
	        Assert.AreEqual(price, theEvent.Get("firstPrice"));
	    }

	    private static void AssertPrevCount(RegressionEnvironment env) {
	        SendTimer(env, 0);
	        SendMarketEvent(env, "IBM", 75);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 1L, 75D);

	        SendMarketEvent(env, "IBM", 76);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 2L, 75D);

	        SendTimer(env, 10000);
	        SendMarketEvent(env, "IBM", 77);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 3L, 75D);

	        SendTimer(env, 20000);
	        SendMarketEvent(env, "IBM", 78);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 4L, 75D);

	        SendTimer(env, 50000);
	        SendMarketEvent(env, "IBM", 79);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 5L, 75D);

	        SendTimer(env, 60000);
	        Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
	        var oldData = env.Listener("s0").LastOldData;
	        Assert.AreEqual(2, oldData.Length);
	        AssertCountAndPrice(oldData[0], 3L, null);
	        env.Listener("s0").Reset();

	        SendMarketEvent(env, "IBM", 80);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 4L, 77D);

	        SendTimer(env, 65000);
	        Assert.IsFalse(env.Listener("s0").IsInvoked);

	        SendTimer(env, 70000);
	        Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
	        oldData = env.Listener("s0").LastOldData;
	        Assert.AreEqual(1, oldData.Length);
	        AssertCountAndPrice(oldData[0], 3L, null);
	        env.Listener("s0").Reset();

	        SendTimer(env, 80000);
	        env.Listener("s0").Reset();

	        SendMarketEvent(env, "IBM", 81);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 3L, 79D);

	        SendTimer(env, 120000);
	        env.Listener("s0").Reset();

	        SendMarketEvent(env, "IBM", 82);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 2L, 81D);

	        SendTimer(env, 300000);
	        env.Listener("s0").Reset();

	        SendMarketEvent(env, "IBM", 83);
	        AssertCountAndPrice(env.Listener("s0").AssertOneGetNewAndReset(), 1L, 83D);
	    }

	    private static SupportMarketDataBean MakeMarketDataEvent(string symbol) {
	        return new SupportMarketDataBean(symbol, 0, 0L, null);
	    }

	    // Don't remove me, I'm dynamically referenced by EPL
	    public static int? IntToLong(long? longValue) {
	        if (longValue == null) {
	            return null;
	        } else {
		        return longValue.AsInt32();
	        }
	    }

	    private static object[] SplitDoubles(string doubleList) {
	        var doubles = doubleList.SplitCsv();
	        var result = new object[doubles.Length];
	        for (var i = 0; i < result.Length; i++) {
	            result[i] = Double.Parse(doubles[i]);
	        }
	        return result;
	    }
	}
} // end of namespace
