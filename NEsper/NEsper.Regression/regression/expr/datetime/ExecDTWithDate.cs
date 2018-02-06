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
    public class ExecDTWithDate : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create variable int varyear");
            epService.EPAdministrator.CreateEPL("create variable int varmonth");
            epService.EPAdministrator.CreateEPL("create variable int varday");
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.withDate(varyear, varmonth, varday) as val0," +
                    "utildate.withDate(varyear, varmonth, varday) as val1," +
                    "longdate.withDate(varyear, varmonth, varday) as val2," +
                    "caldate.withDate(varyear, varmonth, varday) as val3 " +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(long?), typeof(DateTimeOffset?), typeof(long?), typeof(DateTimeEx)
            });
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {
                SupportDateTime.GetValueCoerced(startTime, "long"), null, null, null
            });
    
            string expectedTime = "2004-09-03T09:00:00.000";
            epService.EPRuntime.SetVariableValue("varyear", 2004);
            epService.EPRuntime.SetVariableValue("varmonth", 9);
            epService.EPRuntime.SetVariableValue("varday", 3);
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "util", "long", "cal"));
    
            expectedTime = "2002-09-30T09:00:00.000";
            epService.EPRuntime.SetVariableValue("varyear", null);
            epService.EPRuntime.SetVariableValue("varmonth", 9);
            epService.EPRuntime.SetVariableValue("varday", null);
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "long", "util", "long", "cal"));
        }
    }
} // end of namespace
