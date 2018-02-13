///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTToDateCalMSec : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6,val7,val8,val9,val10,val11,val12,val13,val14,val15,val16,val17".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.ToDate() as val0," +
                    "utildate.ToDate() as val1," +
                    "longdate.ToDate() as val2," +
                    "caldate.ToDate() as val3," +
                    "localdate.ToDate() as val4," +
                    "zoneddate.ToDate() as val5," +
                    "current_timestamp.ToCalendar() as val6," +
                    "utildate.ToCalendar() as val7," +
                    "longdate.ToCalendar() as val8," +
                    "caldate.ToCalendar() as val9," +
                    "localdate.ToCalendar() as val10," +
                    "zoneddate.ToCalendar() as val11," +
                    "current_timestamp.ToMillisec() as val12," +
                    "utildate.ToMillisec() as val13," +
                    "longdate.ToMillisec() as val14," +
                    "caldate.ToMillisec() as val15," +
                    "localdate.ToMillisec() as val16," +
                    "zoneddate.ToMillisec() as val17" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(Date), typeof(Date), typeof(Date), typeof(Date), typeof(Date), typeof(Date),
                    typeof(Calendar), typeof(Calendar), typeof(Calendar), typeof(Calendar), typeof(Calendar), typeof(Calendar),
                    typeof(long), typeof(long), typeof(long), typeof(long), typeof(long), typeof(long)});
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            Object[] expectedUtil = SupportDateTime.GetArrayCoerced(startTime, "util", "util", "util", "util", "util", "util");
            Object[] expectedCal = SupportDateTime.GetArrayCoerced(startTime, "cal", "cal", "cal", "cal", "cal", "cal");
            Object[] expectedMsec = SupportDateTime.GetArrayCoerced(startTime, "long", "long", "long", "long", "long", "long");
            Object[] expected = EPAssertionUtil.ConcatenateArray(expectedUtil, expectedCal, expectedMsec);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{
                    SupportDateTime.GetValueCoerced(startTime, "util"), null, null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "cal"), null, null, null, null, null,
                    SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null, null, null});
        }
    }
} // end of namespace
