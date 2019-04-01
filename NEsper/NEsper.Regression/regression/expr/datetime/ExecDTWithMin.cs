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
    public class ExecDTWithMin : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInput(epService);
            RunAssertionFields(epService);
        }
    
        private void RunAssertionInput(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "utildate.WithMin('month') as val0," +
                    "longdate.WithMin('month') as val1" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(long?)
            });
    
            string startTime = "2002-05-30T09:00:00.000";
            string expectedTime = "2002-01-30T09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expectedTime, "util", "long"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionFields(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            string eplFragment = "select " +
                    "utildate.WithMin('msec') as val0," +
                    "utildate.WithMin('sec') as val1," +
                    "utildate.WithMin('minutes') as val2," +
                    "utildate.WithMin('hour') as val3," +
                    "utildate.WithMin('day') as val4," +
                    "utildate.WithMin('month') as val5," +
                    "utildate.WithMin('year') as val6," +
                    "utildate.WithMin('week') as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?),typeof(DateTimeOffset?),typeof(DateTimeOffset?),typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),typeof(DateTimeOffset?),typeof(DateTimeOffset?),typeof(DateTimeOffset?)
            });
    
            string[] expected = {
                    "2002-05-30T09:01:02.000",
                    "2002-05-30T09:01:00.003",
                    "2002-05-30T09:00:02.003",
                    "2002-05-30T00:01:02.003",
                    "2002-05-01T09:01:02.003",
                    "2002-01-30T09:01:02.003",
                    "0001-05-30T09:01:02.003",
                    "2002-01-03T09:01:02.003",
            };
            string startTime = "2002-05-30T09:01:02.003";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            //Log.Info("===> " + SupportDateTime.Print(listener.AssertOneGetNew().Get("val7")));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expected, "util"));
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
