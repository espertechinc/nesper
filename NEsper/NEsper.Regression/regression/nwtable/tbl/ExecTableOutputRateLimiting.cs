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

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableOutputRateLimiting : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1), typeof(SupportBean_S2)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            var currentTime = new AtomicLong(0);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
    
            epService.EPAdministrator.CreateEPL("@Name('create') create table MyTable as (\n" +
                    "key string primary key, thesum sum(int))");
            epService.EPAdministrator.CreateEPL("@Name('select') into table MyTable " +
                    "select sum(IntPrimitive) as thesum from SupportBean group by TheString");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
            epService.EPAdministrator.GetStatement("create").Dispose();
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select key, thesum from MyTable output snapshot every 1 seconds");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            currentTime.Set(currentTime.Get() + 1000L);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), "key,thesum".Split(','),
                    new object[][]{new object[] {"E1", 40}, new object[] {"E2", 20}});
    
            currentTime.Set(currentTime.Get() + 1000L);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
    }
} // end of namespace
