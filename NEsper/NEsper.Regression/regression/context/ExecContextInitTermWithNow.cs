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
using com.espertech.esper.client.time;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextInitTermWithNow : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNonOverlapping(epService);
            RunAssertionOverlappingWithPattern(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionNonOverlapping(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            string contextExpr = "create context MyContext " +
                    "as start @now end after 10 seconds";
            epService.EPAdministrator.CreateEPL(contextExpr);
    
            var fields = new string[]{"cnt"};
            string streamExpr = "context MyContext " +
                    "select count(*) as cnt from SupportBean output last when terminated";
            EPStatement stream = epService.EPAdministrator.CreateEPL(streamExpr);
            var listener = new SupportUpdateListener();
            stream.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(8000));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(19999));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(30000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0L});
    
            SupportModelHelper.CompileCreate(epService, streamExpr);
    
            epService.EPAdministrator.DestroyAllStatements();
    
            SupportModelHelper.CompileCreate(epService, contextExpr);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOverlappingWithPattern(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            string contextExpr = "create context MyContext " +
                    "initiated by @Now and pattern [every timer:interval(10)] terminated after 10 sec";
            epService.EPAdministrator.CreateEPL(contextExpr);
    
            var fields = new string[]{"cnt"};
            string streamExpr = "context MyContext " +
                    "select count(*) as cnt from SupportBean output last when terminated";
            EPStatement stream = epService.EPAdministrator.CreateEPL(streamExpr);
            var listener = new SupportUpdateListener();
            stream.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(8000));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(9999));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10100));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(19999));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(30000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(40000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
    
            SupportModelHelper.CompileCreate(epService, streamExpr);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // for overlapping contexts, @now without condition is not allowed
            TryInvalid(epService, "create context TimedImmediate initiated @now terminated after 10 seconds",
                    "Incorrect syntax near 'terminated' (a reserved keyword) expecting 'and' but found 'terminated' at line 1 column 45 [create context TimedImmediate initiated @now terminated after 10 seconds]");
    
            // for non-overlapping contexts, @now with condition is not allowed
            TryInvalid(epService, "create context TimedImmediate start @now and after 5 seconds end after 10 seconds",
                    "Incorrect syntax near 'and' (a reserved keyword) at line 1 column 41 [create context TimedImmediate start @now and after 5 seconds end after 10 seconds]");
    
            // for overlapping contexts, @now together with a filter condition is not allowed
            TryInvalid(epService, "create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds",
                    "Invalid use of 'now' with initiated-by stream, this combination is not supported [create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds]");
        }
    }
} // end of namespace
