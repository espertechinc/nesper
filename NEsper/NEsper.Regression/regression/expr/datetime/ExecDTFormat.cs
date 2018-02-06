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
    public class ExecDTFormat : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFormat(epService);
            //RunAssertionFormatWString(epService);
        }
    
        private void RunAssertionFormat(EPServiceProvider epService) {
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "val0,val1,val2".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.Format() as val0," +
                    "utildate.Format() as val1," +
                    "longdate.Format() as val2" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(string), typeof(string), typeof(string)
            });
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            object[] expected = SupportDateTime.GetArrayCoerced(startTime, 
                "str[utc]", "str[utc]", "str[utc]");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {
                SupportDateTime.GetValueCoerced(startTime, "str[utc]"), null, null
            });
    
            stmtFragment.Dispose();
        }

#if REVISIT
        private void RunAssertionFormatWString(EPServiceProvider epService) {
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
            string sdfPattern = "yyyy.MM.dd G 'at' HH:mm:ss";
            var sdf = new SimpleDateFormat(sdfPattern);
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "longdate.Format(\"" + sdfPattern + "\") as val0," +
                    "utildate.Format(\"" + sdfPattern + "\") as val1" +
                    "utildate.Format(SimpleDateFormat.DateInstance) as val5," +
                    "localdate.Format(java.time.format.DateTimeFormatter.BASIC_ISO_DATE) as val6" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtFragment.EventType, fields, typeof(string));
    
            SupportDateTime sdt = SupportDateTime.Make(startTime);
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            var expected = new object[]{
                    sdf.Format(sdt.Longdate), sdf.Format(sdt.Utildate), 
            };
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, null});
    
            stmtFragment.Dispose();
        }
#endif
    }
} // end of namespace
