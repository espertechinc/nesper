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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTSet : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInput(epService);
            RunAssertionFields(epService);
        }
    
        private void RunAssertionInput(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2".Split(',');
            string eplFragment = "select " +
                    "utildate.set('month', 1) as val0," +
                    "longdate.set('month', 1) as val1," +
                    "caldate.set('month', 1) as val2 " +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?),
                typeof(long?),
                typeof(DateTimeEx)
            });
    
            string startTime = "2002-05-30T09:00:00.000";
            string expectedTime = "2002-1-30T09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, 
                SupportDateTime.GetArrayCoerced(expectedTime, "util", "long", "cal"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionFields(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            string eplFragment = "select " +
                    "utildate.set('msec', 1) as val0," +
                    "utildate.set('sec', 2) as val1," +
                    "utildate.set('minutes', 3) as val2," +
                    "utildate.set('hour', 13) as val3," +
                    "utildate.set('day', 5) as val4," +
                    "utildate.set('month', 7) as val5," +
                    "utildate.set('year', 7) as val6," +
                    "utildate.set('week', 8) as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(DateTimeOffset?)

            });
    
            string[] expected = {
                    "2002-05-30T09:00:00.001",
                    "2002-05-30T09:00:02.000",
                    "2002-05-30T09:03:00.000",
                    "2002-05-30T13:00:00.000",
                    "2002-05-5T09:00:00.000",
                    "2002-07-30T09:00:00.000",
                    "0007-05-30T09:00:00.000",
                    "2002-02-21T09:00:00.000",
            };
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
