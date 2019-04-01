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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogEmptyPartition : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            string[] fields = "value".Split(',');
            string text = "select * from MyEvent#length(10) " +
                    "match_recognize (" +
                    "  partition by value" +
                    "  measures E1.value as value" +
                    "  pattern (E1 E2 | E2 E1 ) " +
                    "  define " +
                    "    E1 as E1.TheString = 'A', " +
                    "    E2 as E2.TheString = 'B' " +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 6));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 8));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{7});
    
            /// <summary>Comment-in for testing partition removal.</summary>
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("A", i));
                //Log.Info(i);
                //epService.EPRuntime.SendEvent(new SupportRecogBean("B", i));
                //EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {i});
            }
        }
    }
} // end of namespace
