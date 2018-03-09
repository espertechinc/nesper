///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableIterate : RegressionExecution {
    
        private const string METHOD_NAME = "method:SupportStaticMethodLib.FetchTwoRows3Cols()";
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));
    
            epService.EPAdministrator.CreateEPL("@Resilient create table MyTable(pkey0 string primary key, pkey1 int primary key, c0 long)");
            epService.EPAdministrator.CreateEPL("@Resilient insert into MyTable select TheString as pkey0, IntPrimitive as pkey1, LongPrimitive as c0 from SupportBean");
    
            SendSupportBean(epService, "E1", 10, 100);
            SendSupportBean(epService, "E2", 20, 200);
    
            RunAssertion(epService, true);
            RunAssertion(epService, false);
        }
    
        private void RunAssertion(EPServiceProvider epService, bool useTable) {
            RunUnaggregatedUngroupedSelectStar(epService, useTable);
            RunFullyAggregatedAndUngrouped(epService, useTable);
            RunAggregatedAndUngrouped(epService, useTable);
            RunFullyAggregatedAndGrouped(epService, useTable);
            RunAggregatedAndGrouped(epService, useTable);
            RunAggregatedAndGroupedRollup(epService, useTable);
        }
    
        private void RunUnaggregatedUngroupedSelectStar(EPServiceProvider epService, bool useTable) {
            string epl = "select * from " + (useTable ? "MyTable" : METHOD_NAME);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,pkey1,c0".Split(','), new object[][]{new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 200L}});
        }
    
        private void RunFullyAggregatedAndUngrouped(EPServiceProvider epService, bool useTable) {
            string epl = "select count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            for (int i = 0; i < 2; i++) {
                EventBean @event = stmt.First();
                Assert.AreEqual(2L, @event.Get("thecnt"));
            }
        }
    
        private void RunAggregatedAndUngrouped(EPServiceProvider epService, bool useTable) {
            string epl = "select pkey0, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            for (int i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,thecnt".Split(','), new object[][]{new object[] {"E1", 2L}, new object[] {"E2", 2L}});
            }
        }
    
        private void RunFullyAggregatedAndGrouped(EPServiceProvider epService, bool useTable) {
            string epl = "select pkey0, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME) + " group by pkey0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            for (int i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,thecnt".Split(','), new object[][]{new object[] {"E1", 1L}, new object[] {"E2", 1L}});
            }
        }
    
        private void RunAggregatedAndGrouped(EPServiceProvider epService, bool useTable) {
            string epl = "select pkey0, pkey1, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME) + " group by pkey0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            for (int i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,pkey1,thecnt".Split(','), new object[][]{new object[] {"E1", 10, 1L}, new object[] {"E2", 20, 1L}});
            }
        }
    
        private void RunAggregatedAndGroupedRollup(EPServiceProvider epService, bool useTable) {
            string epl = "select pkey0, pkey1, count(*) as thecnt from " + (useTable ? "MyTable" : METHOD_NAME) + " group by rollup (pkey0, pkey1)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            for (int i = 0; i < 2; i++) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "pkey0,pkey1,thecnt".Split(','), new object[][]{
                        new object[] {"E1", 10, 1L},
                        new object[] {"E2", 20, 1L},
                        new object[] {"E1", null, 1L},
                        new object[] {"E2", null, 1L},
                        new object[] {null, null, 2L},
                });
            }
        }
    
        private SupportBean SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace
