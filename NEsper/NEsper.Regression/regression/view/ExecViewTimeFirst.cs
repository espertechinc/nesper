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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeFirst : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#firsttime(1 month)");
    
            SendCurrentTime(epService, "2002-02-15T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
    
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "TheString".Split(','), new object[][]{new object[] {"E1"}, new object[] {"E2"}});
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace
