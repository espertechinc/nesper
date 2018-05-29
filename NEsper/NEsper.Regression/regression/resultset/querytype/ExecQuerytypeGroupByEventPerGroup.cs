///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeGroupByEventPerGroup : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionCriteriaByDotMethod(epService);
            RunAssertionUnboundStreamIterate(epService);
            RunAssertionNamedWindowDelete(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionUnboundStreamUnlimitedKey(epService);
            }
            RunAssertionAggregateGroupedProps(epService);
            RunAssertionAggregateGroupedPropsPerGroup(epService);
            RunAssertionAggregationOverGroupedProps(epService);
            RunAssertionSumOneView(epService);
            RunAssertionSumJoin(epService);
            RunAssertionUniqueInBatch(epService);
        }
    
        private void RunAssertionCriteriaByDotMethod(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string epl = "select sb.TheString as c0, sum(IntPrimitive) as c1 from SupportBean#length_batch(2) as sb group by sb.TheString";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[]{"E1", 30});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnboundStreamIterate(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // with output snapshot
            string[] fields = "c0,c1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 30));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}, new object[] {"E0", 30}});
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
    
            // with order-by
            stmt = epService.EPAdministrator.CreateEPL("select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events order by TheString asc");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 30));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E0", 30}, new object[] {"E1", 21}, new object[] {"E2", 20}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 40));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E0", 30}, new object[] {"E1", 21}, new object[] {"E2", 20}, new object[] {"E3", 40}});
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
    
            // test un-grouped case
            stmt = epService.EPAdministrator.CreateEPL("select null as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 3 events");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {null, 10}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {null, 30}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {null, 41}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {null, 41}});
    
            stmt.Dispose();
    
            // test reclaim
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            stmt = epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=1,reclaim_group_freq=1') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            epService.EPRuntime.SendEvent(new SupportBean("E0", 11));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1800));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E0", 11}, new object[] {"E2", 12}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E0", 11}, new object[] {"E2", 12}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2200));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 13));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E0", 11}, new object[] {"E2", 25}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionNamedWindowDelete(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_A a delete from MyWindow w where w.TheString = a.id");
            epService.EPAdministrator.CreateEPL("on SupportBean_B delete from MyWindow");
    
            string[] fields = "TheString,mysum".Split(',');
            string epl = "@Hint('DISABLE_RECLAIM_GROUP') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionNamedWindowDelete(epService, listener, stmt, fields);
    
            stmt.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean_B("delete"));
    
            epl = "select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionNamedWindowDelete(epService, listener, stmt, fields);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnboundStreamUnlimitedKey(EPServiceProvider epService) {
            // ESPER-396 Unbound stream and aggregating/grouping by unlimited key (i.e. timestamp) configurable state drop
            SendTimer(epService, 0);
    
            // After the oldest group is 60 second old, reclaim group older then  30 seconds
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30,reclaim_group_freq=5') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            for (int i = 0; i < 1000; i++) {
                SendTimer(epService, 1000 + i * 1000); // reduce factor if sending more events
                var theEvent = new SupportBean();
                theEvent.LongPrimitive = i * 1000;
                epService.EPRuntime.SendEvent(theEvent);
    
                //if (i % 100000 == 0)
                //{
                //    Log.Info("Sending event number " + i);
                //}
            }
    
            listener.Reset();
    
            for (int i = 0; i < 964; i++) {
                var theEvent = new SupportBean();
                theEvent.LongPrimitive = i * 1000;
                epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }
    
            for (int i = 965; i < 1000; i++) {
                var theEvent = new SupportBean();
                theEvent.LongPrimitive = i * 1000;
                epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }
    
            // no frequency provided
            epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
            epService.EPRuntime.SendEvent(new SupportBean());
    
            epService.EPAdministrator.CreateEPL("create variable int myAge = 10");
            epService.EPAdministrator.CreateEPL("create variable int myFreq = 10");
    
            stmtOne.Dispose();
            stmtOne = epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=myAge,reclaim_group_freq=myFreq') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
            stmtOne.Events += listener.Update;
    
            for (int i = 0; i < 1000; i++) {
                SendTimer(epService, 2000000 + 1000 + i * 1000); // reduce factor if sending more events
                var theEvent = new SupportBean();
                theEvent.LongPrimitive = i * 1000;
                epService.EPRuntime.SendEvent(theEvent);
    
                if (i == 500) {
                    epService.EPRuntime.SetVariableValue("myAge", 60);
                    epService.EPRuntime.SetVariableValue("myFreq", 90);
                }
    
                /*
                if (i % 100000 == 0)
                new object[] {
                    Log.Info("Sending event number " + i);
                }
                */
            }
    
            listener.Reset();
    
            for (int i = 0; i < 900; i++) {
                var theEvent = new SupportBean();
                theEvent.LongPrimitive = i * 1000;
                epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }
    
            for (int i = 900; i < 1000; i++) {
                var theEvent = new SupportBean();
                theEvent.LongPrimitive = i * 1000;
                epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }
    
            stmtOne.Dispose();
    
            // invalid tests
            TryInvalid(epService, "@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            TryInvalid(epService, "@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            epService.EPAdministrator.Configuration.AddVariable("MyVar", typeof(string), "");
            TryInvalid(epService, "@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Variable type of variable 'MyVar' is not numeric [@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            TryInvalid(epService, "@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Hint parameter value '-30' is an invalid value, expecting a double-typed seconds value or variable name [@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
    
            /// <summary>Test natural timer - long running test to be commented out.</summary>
            /*
            epService = EPServiceProviderManager.GetProvider(GetType().FullName);
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=1,reclaim_group_freq=1') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
    
            int count = 0;
            While(true)
            new object[] {
                SupportBean @event = new SupportBean();
                event.LongPrimitive = DateTimeHelper.CurrentTimeMillis;
                epService.EPRuntime.SendEvent(new SupportBean());
                count++;
                if (count % 100000 == 0)
                new object[] {
                    Log.Info("Sending event number " + count);
                }
            }
            */
        }
    
        private void TryAssertionNamedWindowDelete(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt, string[] fields) {
            epService.EPRuntime.SendEvent(new SupportBean("A", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 100});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 20});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 101));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 201});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 21));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 41});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"A", 201}, new object[] {"B", 41}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", null});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"B", 41}});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 102));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 102});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"A", 102}, new object[] {"B", 41}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", null});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"A", 102}});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 22));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 22});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"A", 102}, new object[] {"B", 22}});
        }
    
        private void RunAssertionAggregateGroupedProps(EPServiceProvider epService) {
            // test for ESPER-185
            string[] fields = "mycount".Split(',');
            string epl = "select irstream count(price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by price";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 11);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L}, new object[] {1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{2L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {2L}, new object[] {1L}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregateGroupedPropsPerGroup(EPServiceProvider epService) {
            // test for ESPER-185
            string[] fields = "mycount".Split(',');
            string epl = "select irstream count(price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol, price";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 11);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L}, new object[] {1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{2L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {2L}, new object[] {1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {2L}, new object[] {1L}, new object[] {1L}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregationOverGroupedProps(EPServiceProvider epService) {
            // test for ESPER-185
            string[] fields = "symbol,price,mycount".Split(',');
            string epl = "select irstream symbol,price,count(price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol, price order by symbol asc";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"DELL", 10.0, 1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"DELL", 10.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10.0, 1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 11);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"DELL", 11.0, 1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"DELL", 11.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10.0, 1L}, new object[] {"DELL", 11.0, 1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"DELL", 10.0, 2L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"DELL", 10.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10.0, 2L}, new object[] {"DELL", 11.0, 1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 5);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"IBM", 5.0, 1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"IBM", 5.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10.0, 2L}, new object[] {"DELL", 11.0, 1L}, new object[] {"IBM", 5.0, 1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 5);
            Assert.AreEqual(1, listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"IBM", 5.0, 2L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"IBM", 5.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10.0, 2L}, new object[] {"DELL", 11.0, 1L}, new object[] {"IBM", 5.0, 2L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 5);
            Assert.AreEqual(2, listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[]{"IBM", 5.0, 3L});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[]{"IBM", 5.0, 2L});
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"DELL", 10.0, 1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"DELL", 10.0, 2L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 11.0, 1L}, new object[] {"DELL", 10.0, 1L}, new object[] {"IBM", 5.0, 3L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 5);
            Assert.AreEqual(2, listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[]{"IBM", 5.0, 4L});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[]{"IBM", 5.0, 3L});
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"DELL", 11.0, 0L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"DELL", 11.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10.0, 1L}, new object[] {"IBM", 5.0, 4L}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            string epl = "select irstream symbol," +
                    "sum(price) as mySum," +
                    "avg(price) as myAvg " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSum(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            string epl = "select irstream symbol," +
                    "sum(price) as mySum," +
                    "avg(price) as myAvg " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "       and one.TheString = two.symbol " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));
    
            TryAssertionSum(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUniqueInBatch(EPServiceProvider epService) {
            string stmtOne = "insert into MyStream select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#time_batch(1 sec)";
            epService.EPAdministrator.CreateEPL(stmtOne);
            SendTimer(epService, 0);
    
            string epl = "select symbol " +
                    "from MyStream#time_batch(1 sec)#unique(symbol) " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "IBM", 100);
            SendEvent(epService, "IBM", 101);
            SendEvent(epService, "IBM", 102);
            SendTimer(epService, 1000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 2000);
            UniformPair<EventBean[]> received = listener.GetDataListsFlattened();
            Assert.AreEqual("IBM", received.First[0].Get("symbol"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionSum(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            var fields = new string[]{"symbol", "mySum", "myAvg"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myAvg"));
    
            SendEvent(epService, SYMBOL_DELL, 10);
            AssertEvents(listener, SYMBOL_DELL,
                    null, null,
                    10d, 10d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 10d, 10d}});
    
            SendEvent(epService, SYMBOL_DELL, 20);
            AssertEvents(listener, SYMBOL_DELL,
                    10d, 10d,
                    30d, 15d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 30d, 15d}});
    
            SendEvent(epService, SYMBOL_DELL, 100);
            AssertEvents(listener, SYMBOL_DELL,
                    30d, 15d,
                    130d, 130d / 3d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 130d, 130d / 3d}});
    
            SendEvent(epService, SYMBOL_DELL, 50);
            AssertEvents(listener, SYMBOL_DELL,
                    130d, 130 / 3d,
                    170d, 170 / 3d);    // 20 + 100 + 50
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 170d, 170d / 3d}});
    
            SendEvent(epService, SYMBOL_DELL, 5);
            AssertEvents(listener, SYMBOL_DELL,
                    170d, 170 / 3d,
                    155d, 155 / 3d);    // 100 + 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 155d, 155d / 3d}});
    
            SendEvent(epService, "AAA", 1000);
            AssertEvents(listener, SYMBOL_DELL,
                    155d, 155d / 3,
                    55d, 55d / 2);    // 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"DELL", 55d, 55d / 2d}});
    
            SendEvent(epService, SYMBOL_IBM, 70);
            AssertEvents(listener, SYMBOL_DELL,
                    55d, 55 / 2d,
                    5, 5,
                    SYMBOL_IBM,
                    null, null,
                    70, 70);    // Dell:5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                    new object[] {"DELL", 5d, 5d}, new object[] {"IBM", 70d, 70d}});
    
            SendEvent(epService, "AAA", 2000);
            AssertEvents(listener, SYMBOL_DELL,
                    5d, 5d,
                    null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                    new object[] {"IBM", 70d, 70d}});
    
            SendEvent(epService, "AAA", 3000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "AAA", 4000);
            AssertEvents(listener, SYMBOL_IBM,
                    70d, 70d,
                    null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbol,
                                  double? oldSum, double? oldAvg,
                                  double? newSum, double? newAvg) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
            Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
            Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbolOne,
                                  double? oldSumOne, double? oldAvgOne,
                                  double newSumOne, double newAvgOne,
                                  string symbolTwo,
                                  double? oldSumTwo, double? oldAvgTwo,
                                  double newSumTwo, double newAvgTwo) {
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(),
                    "mySum,myAvg".Split(','),
                    new object[][]{new object[] {newSumOne, newAvgOne}, new object[] {newSumTwo, newAvgTwo}},
                    new object[][]{new object[] {oldSumOne, oldAvgOne}, new object[] {oldSumTwo, oldAvgTwo}});
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
