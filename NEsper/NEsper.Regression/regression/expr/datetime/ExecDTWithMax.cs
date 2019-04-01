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
    public class ExecDTWithMax : RegressionExecution {
    
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
                    "utildate.WithMax('month') as val0," +
                    "longdate.WithMax('month') as val1" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?), typeof(long?)
            });
    
            string startTime = "2002-05-30T09:00:00.000";
            string expectedTime = "2002-12-30T09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, SupportDateTime.GetArrayCoerced(expectedTime, "util", "long"));
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionFields(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            string eplFragment = "select " +
                    "utildate.WithMax('msec') as val0," +
                    "utildate.WithMax('sec') as val1," +
                    "utildate.WithMax('minutes') as val2," +
                    "utildate.WithMax('hour') as val3," +
                    "utildate.WithMax('day') as val4," +
                    "utildate.WithMax('month') as val5," +
                    "utildate.WithMax('year') as val6," +
                    "utildate.WithMax('week') as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]
            {
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?),
                typeof(DateTimeOffset?)
            });
    
            string[] expected = {
                    "2002-5-30T09:00:00.999",
                    "2002-5-30T09:00:59.000",
                    "2002-5-30T09:59:00.000",
                    "2002-5-30T23:00:00.000",
                    "2002-5-31T09:00:00.000",
                    "2002-12-30T09:00:00.000",
                    "9999-05-30T09:00:00.000",
                    "2002-12-26T09:00:00.000"
            };
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                SupportDateTime.GetArrayCoerced(expected, "util"));
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
