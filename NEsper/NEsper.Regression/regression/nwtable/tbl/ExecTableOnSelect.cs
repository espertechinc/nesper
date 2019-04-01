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
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableOnSelect : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            epService.EPAdministrator.CreateEPL("create table varagg as (" +
                    "key string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table varagg " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 select total as value from varagg where key = p00").Events += listener.Update;
    
            AssertValues(epService, listener, "G1,G2", new int?[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 100));
            AssertValues(epService, listener, "G1,G2", new int?[]{100, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 200));
            AssertValues(epService, listener, "G1,G2", new int?[]{100, 200});
    
            epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into MyStream select total from varagg where key = p10");
            epService.EPAdministrator.CreateEPL("select * from MyStream").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 300));
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G2"));
            Assert.AreEqual(500, listener.AssertOneGetNewAndReset().Get("total"));
        }
    
        private static void AssertValues(EPServiceProvider engine, SupportUpdateListener listener, string keys, int?[] values) {
            string[] keyarr = keys.Split(',');
            for (int i = 0; i < keyarr.Length; i++) {
                engine.EPRuntime.SendEvent(new SupportBean_S0(0, keyarr[i]));
                if (values[i] == null) {
                    Assert.IsFalse(listener.IsInvoked);
                } else {
                    EventBean @event = listener.AssertOneGetNewAndReset();
                    Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keyarr[i] + "'");
                }
            }
        }
    }
} // end of namespace
