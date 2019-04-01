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
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableOnDelete : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionDeleteFlow(epService);
            RunAssertionDeleteSecondaryIndexUpd(epService);
        }
    
        private void RunAssertionDeleteSecondaryIndexUpd(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTable as (pkey0 string primary key, " +
                    "pkey1 int primary key, thesum sum(long))");
            epService.EPAdministrator.CreateEPL("into table MyTable select sum(LongPrimitive) as thesum from SupportBean group by TheString, IntPrimitive");
    
            MakeSendSupportBean(epService, "E1", 10, 2L);
            MakeSendSupportBean(epService, "E2", 20, 3L);
            MakeSendSupportBean(epService, "E1", 11, 4L);
            MakeSendSupportBean(epService, "E2", 21, 5L);
    
            epService.EPAdministrator.CreateEPL("create index MyIdx on MyTable(pkey0)");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('select') on SupportBean_S0 select sum(thesum) as c0 from MyTable where pkey0=p00").Events += listener.Update;
    
            AssertSum(epService, listener, "E1,E2,E3", new long?[]{6L, 8L, null});
    
            MakeSendSupportBean(epService, "E3", 30, 77L);
            MakeSendSupportBean(epService, "E2", 21, 2L);
    
            AssertSum(epService, listener, "E1,E2,E3", new long[]{6L, 10L, 77L});
    
            epService.EPAdministrator.CreateEPL("@Name('on-delete') on SupportBean_S1 delete from MyTable where pkey0=p10 and pkey1=id");
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "E1"));   // deletes {"E1", 11, 4L}
            AssertSum(epService, listener, "E1,E2,E3", new long[]{2L, 10L, 77L});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E2"));   // deletes {"E2", 20, 3L}
            AssertSum(epService, listener, "E1,E2,E3", new long[]{2L, 7L, 77L});
        }

        private void AssertSum(EPServiceProvider epService, SupportUpdateListener listener, string listOfP00, long?[] sums)
        {
            string[] p00s = listOfP00.Split(',');
            Assert.AreEqual(p00s.Length, sums.Length);
            for (int i = 0; i < p00s.Length; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00s[i]));
                Assert.AreEqual(sums[i], listener.AssertOneGetNewAndReset().Get("c0"));
            }
        }

        private void AssertSum(EPServiceProvider epService, SupportUpdateListener listener, string listOfP00, long[] sums) {
            string[] p00s = listOfP00.Split(',');
            Assert.AreEqual(p00s.Length, sums.Length);
            for (int i = 0; i < p00s.Length; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00s[i]));
                Assert.AreEqual(sums[i], listener.AssertOneGetNewAndReset().Get("c0"));
            }
        }
    
        private void RunAssertionDeleteFlow(EPServiceProvider epService) {
            var listenerDeleteFiltered = new SupportUpdateListener();
            var listenerDeleteAll = new SupportUpdateListener();
    
            string[] fields = "key,thesum".Split(',');
            epService.EPAdministrator.CreateEPL("create table varagg as (key string primary key, thesum sum(int))");
            epService.EPAdministrator.CreateEPL("into table varagg select sum(IntPrimitive) as thesum from SupportBean group by TheString");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select varagg[p00].thesum as value from SupportBean_S0").Events += listener.Update;
            EPStatement stmtDeleteFiltered = epService.EPAdministrator.CreateEPL("on SupportBean_S1(id = 1) delete from varagg where key = p10");
            EPStatement stmtDeleteAll = epService.EPAdministrator.CreateEPL("on SupportBean_S1(id = 2) delete from varagg");
    
            var expectedType = new object[][]{new object[] {"key", typeof(string)}, new object[] {"thesum", typeof(int)}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtDeleteAll.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            stmtDeleteFiltered.Events += listenerDeleteFiltered.Update;
            stmtDeleteAll.Events += listenerDeleteAll.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            AssertValues(epService, listener, "G1,G2", new int?[]{10, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            AssertValues(epService, listener, "G1,G2", new int?[]{10, 20});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "G1"));
            AssertValues(epService, listener, "G1,G2", new int?[]{null, 20});
            EPAssertionUtil.AssertProps(listenerDeleteFiltered.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, null));
            AssertValues(epService, listener, "G1,G2", new int?[]{null, null});
            EPAssertionUtil.AssertProps(listenerDeleteAll.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private static void AssertValues(EPServiceProvider engine, SupportUpdateListener listener, string keys, int?[] values) {
            string[] keyarr = keys.Split(',');
            Assert.AreEqual(keyarr.Length, values.Length);
            for (int i = 0; i < keyarr.Length; i++) {
                engine.EPRuntime.SendEvent(new SupportBean_S0(0, keyarr[i]));
                EventBean @event = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keyarr[i] + "'");
            }
        }
    
        private void MakeSendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(b);
        }
    }
} // end of namespace
