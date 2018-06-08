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
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTRound : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
    
            RunAssertionInput(epService);
            RunAssertionRoundCeil(epService);
            RunAssertionRoundFloor(epService);
            RunAssertionRoundHalf(epService);
        }
    
        private void RunAssertionInput(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "utildate.RoundCeiling('hour') as val0," +
                    "longdate.RoundCeiling('hour') as val1" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(DateTimeOffset?), typeof(long?)
            });
    
            string startTime = "2002-05-30T09:01:02.003";
            string expectedTime = "2002-5-30T10:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                SupportDateTime.GetArrayCoerced(expectedTime, "util", "long"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionRoundCeil(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            string eplFragment = "select " +
                    "utildate.RoundCeiling('msec') as val0," +
                    "utildate.RoundCeiling('sec') as val1," +
                    "utildate.RoundCeiling('minutes') as val2," +
                    "utildate.RoundCeiling('hour') as val3," +
                    "utildate.RoundCeiling('day') as val4," +
                    "utildate.RoundCeiling('month') as val5," +
                    "utildate.RoundCeiling('year') as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?)
            });
    
            string[] expected = {
                    "2002-05-30T09:01:02.003",
                    "2002-05-30T09:01:03.000",
                    "2002-05-30T09:02:00.000",
                    "2002-05-30T10:00:00.000",
                    "2002-05-31T00:00:00.000",
                    "2002-06-1T00:00:00.000",
                    "2003-01-1T00:00:00.000",
            };
            string startTime = "2002-05-30T09:01:02.003";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionRoundFloor(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            string eplFragment = "select " +
                    "utildate.RoundFloor('msec') as val0," +
                    "utildate.RoundFloor('sec') as val1," +
                    "utildate.RoundFloor('minutes') as val2," +
                    "utildate.RoundFloor('hour') as val3," +
                    "utildate.RoundFloor('day') as val4," +
                    "utildate.RoundFloor('month') as val5," +
                    "utildate.RoundFloor('year') as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?)
            });
    
            string[] expected = {
                    "2002-05-30T09:01:02.003",
                    "2002-05-30T09:01:02.000",
                    "2002-05-30T09:01:00.000",
                    "2002-05-30T09:00:00.000",
                    "2002-05-30T00:00:00.000",
                    "2002-05-1T00:00:00.000",
                    "2002-01-1T00:00:00.000",
            };
            string startTime = "2002-05-30T09:01:02.003";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionRoundHalf(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            string eplFragment = "select " +
                    "utildate.RoundHalf('msec') as val0," +
                    "utildate.RoundHalf('sec') as val1," +
                    "utildate.RoundHalf('minutes') as val2," +
                    "utildate.RoundHalf('hour') as val3," +
                    "utildate.RoundHalf('day') as val4," +
                    "utildate.RoundHalf('month') as val5," +
                    "utildate.RoundHalf('year') as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?), typeof(DateTimeOffset?)
            });
    
            string[] expected = {
                    "2002-05-30T15:30:02.550",
                    "2002-05-30T15:30:03.000",
                    "2002-05-30T15:30:00.000",
                    "2002-05-30T16:00:00.00",
                    "2002-05-31T00:00:00.000",
                    "2002-06-01T00:00:00.000",
                    "2002-01-01T00:00:00.000",
            };
            string startTime = "2002-05-30T15:30:02.550";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
    
            // test rounding up/down
            stmtFragment.Dispose();
            fields = "val0".Split(',');
            eplFragment = "select Utildate.RoundHalf('min') as val0 from SupportDateTime";
            stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30T15:30:29.999"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{SupportDateTime.GetValueCoerced("2002-05-30T15:30:00.000", "util")});
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30T15:30:30.000"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{SupportDateTime.GetValueCoerced("2002-05-30T15:31:00.000", "util")});
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30T15:30:30.001"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{SupportDateTime.GetValueCoerced("2002-05-30T15:31:00.000", "util")});
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
