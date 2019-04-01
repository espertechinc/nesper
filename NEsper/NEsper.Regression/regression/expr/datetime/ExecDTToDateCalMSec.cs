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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTToDateCalMSec : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6,val7,val8".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.toDate() as val0," +
                    "utildate.toDate() as val1," +
                    "longdate.toDate() as val2," +
                    "current_timestamp.toCalendar() as val3," +
                    "utildate.toCalendar() as val4," +
                    "longdate.toCalendar() as val5," +
                    "current_timestamp.toMillisec() as val6," +
                    "utildate.toMillisec() as val7," +
                    "longdate.toMillisec() as val8" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{
                typeof(DateTimeOffset), typeof(DateTimeOffset), typeof(DateTimeOffset),
                typeof(DateTimeEx), typeof(DateTimeEx), typeof(DateTimeEx),
                typeof(long), typeof(long), typeof(long)
            });
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            object[] expectedUtil = SupportDateTime.GetArrayCoerced(startTime, "util", "util", "util");
            object[] expectedCal = SupportDateTime.GetArrayCoerced(startTime, "cal", "cal", "cal");
            object[] expectedMsec = SupportDateTime.GetArrayCoerced(startTime, "long", "long", "long");
            object[] expected = EPAssertionUtil.ConcatenateArray(expectedUtil, expectedCal, expectedMsec);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{
                SupportDateTime.GetValueCoerced(startTime, "util"), null, null,
                SupportDateTime.GetValueCoerced(startTime, "cal"), null, null,
                SupportDateTime.GetValueCoerced(startTime, "long"), null, null
            });
        }
    }
} // end of namespace
