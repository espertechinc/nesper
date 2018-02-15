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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTGet : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFields(epService);
            RunAssertionInput(epService);
        }
    
        private void RunAssertionInput(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4".Split(',');
            string epl = "select " +
                    "utildate.Get('month') as val0," +
                    "longdate.Get('month') as val1," +
                    "caldate.Get('month') as val2, " +
                    "localdate.Get('month') as val3, " +
                    "zoneddate.Get('month') as val4 " +
                    " from SupportDateTime";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(int?)});
    
            string startTime = "2002-05-30T09:00:00.000";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, 4, 4, 5, 5});
    
            // try event as input
            var configBean = new ConfigurationEventTypeLegacy();
            configBean.StartTimestampPropertyName = "longdateStart";
            configBean.EndTimestampPropertyName = "longdateEnd";
            epService.EPAdministrator.Configuration.AddEventType("SupportTimeStartEndA", typeof(SupportTimeStartEndA).Name, configBean);
    
            stmt.Dispose();
            epl = "select Abc.Get('month') as val0 from SupportTimeStartEndA as abc";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A0", startTime, 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[]{4});
    
            // test "get" method on object is preferred
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.CreateEPL("select E.Get() as c0, e.Get('abc') as c1 from MyEvent as e").Events += listener.Update;
            epService.EPRuntime.SendEvent(new MyEvent());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[]{1, 2});
    
            stmt.Dispose();
        }
    
        private void RunAssertionFields(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            string eplFragment = "select " +
                    "utildate.Get('msec') as val0," +
                    "utildate.Get('sec') as val1," +
                    "utildate.Get('minutes') as val2," +
                    "utildate.Get('hour') as val3," +
                    "utildate.Get('day') as val4," +
                    "utildate.Get('month') as val5," +
                    "utildate.Get('year') as val6," +
                    "utildate.Get('week') as val7" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(int?), typeof(int?)});
    
            string startTime = "2002-05-30T09:01:02.003";
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 2, 1, 9, 30, 4, 2002, 22});
    
            stmtFragment.Dispose();
        }
    
        public class MyEvent {
            public int Get() {
                return 1;
            }
    
            public int Get(string abc) {
                return 2;
            }
        }
    }
} // end of namespace
