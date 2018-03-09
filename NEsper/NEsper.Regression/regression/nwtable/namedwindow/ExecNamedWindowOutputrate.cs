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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOutputrate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema SupportBean as " + typeof(SupportBean).FullName);
    
            epService.EPAdministrator.CreateEPL("create window MyWindowOne#keepall as (TheString string, intv int)");
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select TheString, IntPrimitive as intv from SupportBean");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            var fields = new string[]{"TheString", "c"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select irstream TheString, count(*) as c from MyWindowOne group by TheString output snapshot every 1 second");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B", 4));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A", 2L}, new object[] {"B", 1L}});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 5));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
    
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A", 2L}, new object[] {"B", 2L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
    
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A", 2L}, new object[] {"B", 2L}});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 5));
            epService.EPRuntime.SendEvent(new SupportBean("C", 1));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000));
    
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A", 3L}, new object[] {"B", 2L}, new object[] {"C", 1L}});
        }
    }
} // end of namespace
