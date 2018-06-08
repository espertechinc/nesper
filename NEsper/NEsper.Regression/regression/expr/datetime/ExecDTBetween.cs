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
    public class ExecDTBetween : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
            configuration.AddEventType("SupportTimeStartEndA", typeof(SupportTimeStartEndA));
            configuration.AddImport(typeof(DateTimeParser));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionIncludeEndpoints(epService);
            RunAssertionExcludeEndpoints(epService);
        }
    
        private void RunAssertionIncludeEndpoints(EPServiceProvider epService) {
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fieldsCurrentTs = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            string eplCurrentTS = "select " +
                    "current_timestamp.after(longdateStart) as val0, " +
                    "current_timestamp.between(longdateStart, longdateEnd) as val1, " +
                    "current_timestamp.between(utildateStart, caldateEnd) as val2, " +
                    "current_timestamp.between(caldateStart, utildateEnd) as val3, " +
                    "current_timestamp.between(utildateStart, utildateEnd) as val4, " +
                    "current_timestamp.between(caldateStart, caldateEnd) as val5, " +
                    "current_timestamp.between(caldateEnd, caldateStart) as val6 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtCurrentTs = epService.EPAdministrator.CreateEPL(eplCurrentTS);
            var listener = new SupportUpdateListener();
            stmtCurrentTs.Events += listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtCurrentTs.EventType, fieldsCurrentTs, typeof(bool?));
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[] {
                true, false, false, false, false, false, false
            });
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[] {
                true, true, true, true, true, true, true
            });
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[] {
                true, true, true, true, true, true, true
            });
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.000", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[] {
                false, true, true, true, true, true, true
            });
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.000", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[] {
                false, true, true, true, true, true, true
            });
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.001", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[] {
                false, false, false, false, false, false, false
            });
            stmtCurrentTs.Dispose();
    
            // test calendar field and constants
            epService.EPAdministrator.Configuration.AddImport(typeof(DateTime));
            string[] fieldsConstants = "val0,val1,val2,val5".Split(',');
            string eplConstants = "select " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000')) as val0, " +
                    "utildateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000')) as val1, " +
                    "caldateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000')) as val2, " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:01:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:00:00.000')) as val5 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtConstants = epService.EPAdministrator.CreateEPL(eplConstants);
            stmtConstants.Events += listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtConstants.EventType, fieldsConstants, typeof(bool?));
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsConstants, false);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:00.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:05.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:59.999", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.001", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsConstants, false);
    
            stmtConstants.Dispose();
        }
    
        private void RunAssertionExcludeEndpoints(EPServiceProvider epService) {
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
            epService.EPAdministrator.CreateEPL("create variable bool VAR_TRUE = true");
            epService.EPAdministrator.CreateEPL("create variable bool VAR_FALSE = false");
    
            TryAssertionExcludeEndpoints(epService, "longdateStart, longdateEnd");
            TryAssertionExcludeEndpoints(epService, "utildateStart, utildateEnd");
            TryAssertionExcludeEndpoints(epService, "caldateStart, caldateEnd");
        }
    
        private void TryAssertionExcludeEndpoints(EPServiceProvider epService, string fields) {
    
            string[] fieldsCurrentTs = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            string eplCurrentTS = "select " +
                    "current_timestamp.between(" + fields + ", true, true) as val0, " +
                    "current_timestamp.between(" + fields + ", true, false) as val1, " +
                    "current_timestamp.between(" + fields + ", false, true) as val2, " +
                    "current_timestamp.between(" + fields + ", false, false) as val3, " +
                    "current_timestamp.between(" + fields + ", VAR_TRUE, VAR_TRUE) as val4, " +
                    "current_timestamp.between(" + fields + ", VAR_TRUE, VAR_FALSE) as val5, " +
                    "current_timestamp.between(" + fields + ", VAR_FALSE, VAR_TRUE) as val6, " +
                    "current_timestamp.between(" + fields + ", VAR_FALSE, VAR_FALSE) as val7 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtCurrentTs = epService.EPAdministrator.CreateEPL(eplCurrentTS);
            var listener = new SupportUpdateListener();
            stmtCurrentTs.Events += listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtCurrentTs.EventType, fieldsCurrentTs, typeof(bool?));
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, false);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[]{true, false, true, false, true, false, true, false});
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 2));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, true);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.000", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new object[]{true, true, false, false, true, true, false, false});
    
            stmtCurrentTs.Dispose();
    
            // test calendar field and constants
            epService.EPAdministrator.Configuration.AddImport(typeof(DateTime));
            string[] fieldsConstants = "val0,val1,val2,val3".Split(',');
            string eplConstants = "select " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000'), true, true) as val0, " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000'), true, false) as val1, " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000'), false, true) as val2, " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30T09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30T09:01:00.000'), false, false) as val3 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtConstants = epService.EPAdministrator.CreateEPL(eplConstants);
            stmtConstants.Events += listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtConstants.EventType, fieldsConstants, typeof(bool?));
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsConstants, new object[]{false, false, false, false});
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:00.000", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsConstants, new object[]{true, true, false, false});
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:05.000", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsConstants, new object[]{true, true, true, true});
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:59.999", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsConstants, new object[]{true, true, true, true});
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.000", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsConstants, new object[]{true, false, true, false});
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.001", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsConstants, new object[]{false, false, false, false});
    
            stmtConstants.Dispose();
        }
    }
} // end of namespace
