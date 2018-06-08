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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateRate : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            RunAssertionRateDataNonWindowed(epService);
            RunAssertionRateDataWindowed(epService);
        }

        // rate implementation does not require a data window (may have one)
        // advantage: not retaining events, only timestamp data points
        // disadvantage: output rate limiting without snapshot may be less accurate rate
        private void RunAssertionRateDataNonWindowed(EPServiceProvider epService)
        {
            SendTimer(epService, 0);

            var epl = "select rate(10) as myrate from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            TryAssertion(epService, listener);

            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(epl);
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(epl, model.ToEPL());

            TryAssertion(epService, listener);

            TryInvalid(
                epService, "select rate() from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'rate(*)': The rate aggregation function minimally requires a numeric constant or expression as a parameter. [select rate() from SupportBean]");
            TryInvalid(
                epService, "select rate(true) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'rate(true)': The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation [select rate(true) from SupportBean]");

            stmt.Dispose();
        }

        private void RunAssertionRateDataWindowed(EPServiceProvider epService)
        {
            var fields = "myrate,myqtyrate".Split(',');
            var epl =
                "select RATE(LongPrimitive) as myrate, RATE(LongPrimitive, IntPrimitive) as myqtyrate from SupportBean#length(3)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEvent(epService, 1000, 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEvent(epService, 1200, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEvent(epService, 1300, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            SendEvent(epService, 1500, 14);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {3 * 1000 / 500d, 14 * 1000 / 500d});

            SendEvent(epService, 2000, 11);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {3 * 1000 / 800d, 25 * 1000 / 800d});

            TryInvalid(
                epService, "select rate(LongPrimitive) as myrate from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'rate(LongPrimitive)': The rate aggregation function in the timestamp-property notation requires data windows [select rate(LongPrimitive) as myrate from SupportBean]");
            TryInvalid(
                epService, "select rate(current_timestamp) as myrate from SupportBean#time(20)",
                "Error starting statement: Failed to validate select-clause expression 'rate(current_timestamp())': The rate aggregation function does not allow the current engine timestamp as a parameter [select rate(current_timestamp) as myrate from SupportBean#time(20)]");
            TryInvalid(
                epService, "select rate(TheString) as myrate from SupportBean#time(20)",
                "Error starting statement: Failed to validate select-clause expression 'rate(TheString)': The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation [select rate(TheString) as myrate from SupportBean#time(20)]");

            stmt.Dispose();
        }

        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener)
        {
            var fields = "myrate".Split(',');

            SendTimer(epService, 1000);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 1200);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 1600);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 1600);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 9000);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 9200);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 10999);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(epService, 11100);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {0.7});

            SendTimer(epService, 11101);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {0.8});

            SendTimer(epService, 11200);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {0.8});

            SendTimer(epService, 11600);
            SendEvent(epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {0.7});
        }

#if DEPRECATED
        [Test]
        public void TestRateThreaded() {
    
            var config = new Configuration();
            config.AddEventType<SupportBean>();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            var runnable = new RateSendRunnable(epService.EPRuntime);
            var timer = new ScheduledThreadPoolExecutor(1);
    
            //string viewExpr = "select RATE(LongPrimitive) as myrate from SupportBean#time(10) output every 1 sec";
            var viewExpr = "select RATE(10) as myrate from SupportBean output snapshot every 1 sec";
            var stmt = epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += (sender, args) => Log.Info(newEvents[0].Get("myrate"));
    
            var rateDelay = 133;   // <== change here
            var future = timer.ScheduleAtFixedRate(runnable, 0, rateDelay, TimeUnit.MILLISECONDS);
            System.Threading.Thread.Sleep(2 * 60 * 1000);
            future.Cancel(true);
        }
#endif

        private void SendTimer(EPServiceProvider epService, long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendEvent(EPServiceProvider epService, long longPrimitive, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.LongPrimitive = longPrimitive;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(EPServiceProvider epService)
        {
            var bean = new SupportBean();
            epService.EPRuntime.SendEvent(bean);
        }

        public class RateSendRunnable
        {
            private readonly EPRuntime _runtime;

            public RateSendRunnable(EPRuntime runtime)
            {
                _runtime = runtime;
            }

            public void Run()
            {
                var bean = new SupportBean();
                bean.LongPrimitive = DateTimeHelper.CurrentTimeMillis;
                _runtime.SendEvent(bean);
            }
        }
    }
} // end of namespace