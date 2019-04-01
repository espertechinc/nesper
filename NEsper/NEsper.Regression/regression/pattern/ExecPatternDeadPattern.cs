///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternDeadPattern : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("A", typeof(SupportBean_A));
            configuration.AddEventType("B", typeof(SupportBean_B));
            configuration.AddEventType("C", typeof(SupportBean_C));
        }
    
        public override void Run(EPServiceProvider epService) {
            string pattern = "(A() -> B()) and not C()";
            // Adjust to 20000 to better test the limit
            for (int i = 0; i < 1000; i++) {
                epService.EPAdministrator.CreatePattern(pattern);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.IsTrue(delta < 20, "performance: delta=" + delta);
        }
    }
} // end of namespace
