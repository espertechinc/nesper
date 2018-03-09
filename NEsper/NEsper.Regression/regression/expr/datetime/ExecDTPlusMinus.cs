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
    public class ExecDTPlusMinus : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPlusMinus(epService);
            RunAssertionPlusMinusTimePeriod(epService);
        }
    
        private void RunAssertionPlusMinus(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create variable long varmsec");
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.Plus(varmsec) as val0," +
                    "utildate.Plus(varmsec) as val1," +
                    "longdate.Plus(varmsec) as val2," +
                    "current_timestamp.minus(varmsec) as val3," +
                    "utildate.Minus(varmsec) as val4," +
                    "longdate.Minus(varmsec) as val5" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{
                typeof(long?), typeof(DateTimeOffset?), typeof(long?),
                typeof(long?), typeof(DateTimeOffset?), typeof(long?)
            });
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{
                SupportDateTime.GetValueCoerced(startTime, "long"), null, null,
                SupportDateTime.GetValueCoerced(startTime, "long"), null, null
            });
    
            object[] expectedPlus = SupportDateTime.GetArrayCoerced(startTime, "long", "util", "long");
            object[] expectedMinus = SupportDateTime.GetArrayCoerced(startTime, "long", "util", "long");
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            epService.EPRuntime.SetVariableValue("varmsec", 1000);
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            //Log.Info("===> " + SupportDateTime.Print(listener.AssertOneGetNew().Get("val4")));
            expectedPlus = SupportDateTime.GetArrayCoerced("2002-05-30T09:00:01.000", "long", "util", "long");
            expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-30T08:59:59.000", "long", "util", "long");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            epService.EPRuntime.SetVariableValue("varmsec", 2 * 24 * 60 * 60 * 1000);
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-28T09:00:00.000", "long", "util", "long");
            expectedPlus = SupportDateTime.GetArrayCoerced("2002-06-1T09:00:00.000", "long", "util", "long");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionPlusMinusTimePeriod(EPServiceProvider epService) {
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.Plus(1 hour 10 sec 20 msec) as val0," +
                    "utildate.Plus(1 hour 10 sec 20 msec) as val1," +
                    "longdate.Plus(1 hour 10 sec 20 msec) as val2," +
                    "current_timestamp.Minus(1 hour 10 sec 20 msec) as val3," +
                    "utildate.Minus(1 hour 10 sec 20 msec) as val4," +
                    "longdate.Minus(1 hour 10 sec 20 msec) as val5" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(long?), typeof(DateTimeOffset?), typeof(long?),
                typeof(long?), typeof(DateTimeOffset?), typeof(long?)
            });
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            object[] expectedPlus = SupportDateTime.GetArrayCoerced("2002-05-30T010:00:10.020", "long", "util", "long");
            object[] expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-30T07:59:49.980", "long", "util", "long");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            expectedPlus = SupportDateTime.GetArrayCoerced("2002-05-30T010:00:10.020", "long", "null", "null");
            expectedMinus = SupportDateTime.GetArrayCoerced("2002-05-30T07:59:49.980", "long", "null", "null");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, EPAssertionUtil.ConcatenateArray(expectedPlus, expectedMinus));
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
