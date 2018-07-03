///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewLengthBatch : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            var events = new SupportBean[10];
            for (int i = 0; i < events.Length; i++) {
                events[i] = new SupportBean();
            }
    
            RunAssertionLengthBatchSize2(epService, events);
            RunAssertionLengthBatchSize1(epService, events);
            RunAssertionLengthBatchSize3(epService, events);
            RunAssertionLengthBatchSize3And2Staggered(epService, events);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionLengthBatchSize2(EPServiceProvider epService, SupportBean[] events) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + "#length_batch(2)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(events[0], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[0]}, stmt.GetEnumerator());
    
            SendEvent(events[1], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[0], events[1]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[2], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[2]}, stmt.GetEnumerator());
    
            SendEvent(events[3], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[2], events[3]},
                new SupportBean[]{events[0], events[1]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[4], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[4]}, stmt.GetEnumerator());
    
            SendEvent(events[5], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[4], events[5]},
                new SupportBean[]{events[2], events[3]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            stmt.Dispose();
        }
    
        private void RunAssertionLengthBatchSize1(EPServiceProvider epService, SupportBean[] events) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + "#length_batch(1)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(events[0], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[0]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[1], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[1]},
                new SupportBean[]{events[0]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[2], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[2]}, 
                new SupportBean[]{events[1]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            stmt.Dispose();
        }
    
        private void RunAssertionLengthBatchSize3(EPServiceProvider epService, SupportBean[] events) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + "#length_batch(3)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(events[0], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[0]}, stmt.GetEnumerator());
    
            SendEvent(events[1], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[0], events[1]}, stmt.GetEnumerator());
    
            SendEvent(events[2], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[0], events[1], events[2]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[3], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[3]}, stmt.GetEnumerator());
    
            SendEvent(events[4], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{events[3], events[4]}, stmt.GetEnumerator());
    
            SendEvent(events[5], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[3], events[4], events[5]},
                new SupportBean[]{events[0], events[1], events[2]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            stmt.Dispose();
        }
    
        private void RunAssertionLengthBatchSize3And2Staggered(EPServiceProvider epService, SupportBean[] events) {
            if (SupportConfigFactory.SkipTest(typeof(ExecViewLengthBatch))) {
                return;
            }
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + "#length_batch(3)#length_batch(2)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(events[0], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[1], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[2], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(), new SupportBean[]{events[0], events[1], events[2]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[3], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[4], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(events[5], epService);
            EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(),
                new SupportBean[]{events[3], events[4], events[5]},
                new SupportBean[]{events[0], events[1], events[2]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            try {
                epService.EPAdministrator.CreateEPL(
                        "select * from " + typeof(SupportMarketDataBean).FullName + "#length_batch(0)");
                Assert.Fail();
            } catch (Exception) {
                // expected
            }
        }
    
        private void SendEvent(SupportBean theEvent, EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
