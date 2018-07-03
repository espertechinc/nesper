///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    /// <summary>
    /// This test uses unique and sort views to obtain from a set of market data events the 3 currently most expensive stocks
    /// and their symbols.
    /// The unique view plays the role of filtering only the most recent events and making prior events for a symbol 'old'
    /// data to the sort view, which removes these prior events for a symbol from the sorted window.
    /// </summary>
    public class ExecViewUniqueSorted : RegressionExecution {
        private const string SYMBOL_CSCO = "CSCO.O";
        private const string SYMBOL_IBM = "IBM.N";
        private const string SYMBOL_MSFT = "MSFT.O";
        private const string SYMBOL_C = "C.N";

        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            RunAssertionExpressionParameter(epService);
            RunAssertionWindowStats(epService);
            RunAssertionSensorPerEvent(epService);
            RunAssertionReuseUnique(epService);
        }
    
        private void RunAssertionExpressionParameter(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#unique(Math.Abs(IntPrimitive))");
            SendEvent(epService, "E1", 10);
            SendEvent(epService, "E2", -10);
            SendEvent(epService, "E3", -5);
            SendEvent(epService, "E4", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "TheString".Split(','), new[] {new object[] {"E2"}, new object[] {"E4"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionWindowStats(EPServiceProvider epService) {
            // Get the top 3 volumes for each symbol
            EPStatement top3Prices = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportMarketDataBean).FullName +
                            "#unique(symbol)#sort(3, price desc)");
            var testListener = new SupportUpdateListener();
            top3Prices.Events += testListener.Update;
    
            var beans = new object[10];
    
            beans[0] = MakeEvent(SYMBOL_CSCO, 50);
            epService.EPRuntime.SendEvent(beans[0]);

            object[] result = top3Prices
                .Select(x => x.Underlying)
                .ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(new[]{beans[0]}, result);
            Assert.IsTrue(testListener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrder((object[]) null, testListener.LastOldData);
            EPAssertionUtil.AssertEqualsExactOrder(new[]{beans[0]}, new[]{testListener.LastNewData[0].Underlying});
            testListener.Reset();
    
            beans[1] = MakeEvent(SYMBOL_CSCO, 20);
            beans[2] = MakeEvent(SYMBOL_IBM, 50);
            beans[3] = MakeEvent(SYMBOL_MSFT, 40);
            beans[4] = MakeEvent(SYMBOL_C, 100);
            beans[5] = MakeEvent(SYMBOL_IBM, 10);
    
            epService.EPRuntime.SendEvent(beans[1]);
            epService.EPRuntime.SendEvent(beans[2]);
            epService.EPRuntime.SendEvent(beans[3]);
            epService.EPRuntime.SendEvent(beans[4]);
            epService.EPRuntime.SendEvent(beans[5]);

            result = top3Prices
                .Select(x => x.Underlying)
                .ToArray();

            EPAssertionUtil.AssertEqualsExactOrder(new[]{beans[4], beans[3], beans[5]}, result);
    
            beans[6] = MakeEvent(SYMBOL_CSCO, 110);
            beans[7] = MakeEvent(SYMBOL_C, 30);
            beans[8] = MakeEvent(SYMBOL_CSCO, 30);
    
            epService.EPRuntime.SendEvent(beans[6]);
            epService.EPRuntime.SendEvent(beans[7]);
            epService.EPRuntime.SendEvent(beans[8]);
    
            result = top3Prices
                .Select(x => x.Underlying)
                .ToArray();

            EPAssertionUtil.AssertEqualsExactOrder(new[]{beans[3], beans[8], beans[7]}, result);
    
            top3Prices.Dispose();
        }
    
        private void RunAssertionSensorPerEvent(EPServiceProvider epService) {
            string stmtString =
                    "SELECT irstream * " +
                            "FROM\n " +
                            typeof(SupportSensorEvent).FullName + "#groupwin(type)#time(1 hour)#unique(device)#sort(1, measurement desc) as high ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            EPRuntime runtime = epService.EPRuntime;
    
            var eventOne = new SupportSensorEvent(1, "Temperature", "Device1", 5.0, 96.5);
            runtime.SendEvent(eventOne);
            EPAssertionUtil.AssertUnderlyingPerRow(testListener.AssertInvokedAndReset(), new object[]{eventOne}, null);
    
            var eventTwo = new SupportSensorEvent(2, "Temperature", "Device2", 7.0, 98.5);
            runtime.SendEvent(eventTwo);
            EPAssertionUtil.AssertUnderlyingPerRow(testListener.AssertInvokedAndReset(), new object[]{eventTwo}, new object[]{eventOne});
    
            var eventThree = new SupportSensorEvent(3, "Temperature", "Device2", 4.0, 99.5);
            runtime.SendEvent(eventThree);
            EPAssertionUtil.AssertUnderlyingPerRow(testListener.AssertInvokedAndReset(), new object[]{eventThree}, new object[]{eventTwo});
    
            SupportSensorEvent theEvent = (SupportSensorEvent) stmt.First().Underlying;
            Assert.AreEqual(3, theEvent.Id);
    
            stmt.Dispose();
        }
    
        private void RunAssertionReuseUnique(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#unique(IntBoxed)");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var beanOne = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(beanOne);
            testListener.Reset();
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#unique(IntBoxed)");
            var testListenerTwo = new SupportUpdateListener();
            stmtTwo.Events += testListenerTwo.Update;
            stmt.Start(); // no effect
    
            var beanTwo = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(beanTwo);
    
            Assert.AreSame(beanTwo, testListener.LastNewData[0].Underlying);
            Assert.AreSame(beanOne, testListener.LastOldData[0].Underlying);
            Assert.AreSame(beanTwo, testListenerTwo.LastNewData[0].Underlying);
            Assert.IsNull(testListenerTwo.LastOldData);
    
            stmt.Dispose();
        }
    
        private object MakeEvent(string symbol, double price) {
            return new SupportMarketDataBean(symbol, price, 0L, "");
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
        }

#if false
        private object[] ToObjectArray(IEnumerator<EventBean> it) {
            var result = new LinkedList<object>();
            for (; it.HasNext(); ) {
                EventBean theEvent = it.Next();
                result.Add(theEvent.Underlying);
            }
            return Result.ToArray();
        }
#endif
    }
} // end of namespace
