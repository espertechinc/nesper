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
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableFilters : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            epService.EPAdministrator.CreateEPL("create table MyTable(pkey string primary key, col0 int)");
            epService.EPAdministrator.CreateEPL("insert into MyTable select TheString as pkey, IntPrimitive as col0 from SupportBean");
    
            for (int i = 0; i < 5; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            string[] fields = "col0".Split(',');
    
            // test FAF filter
            EventBean[] events = epService.EPRuntime.ExecuteQuery("select col0 from MyTable(pkey='E1')").Array;
            EPAssertionUtil.AssertPropsPerRow(events, fields, new object[][]{new object[] {1}});
    
            // test iterate
            EPStatement stmtIterate = epService.EPAdministrator.CreateEPL("select col0 from MyTable(pkey='E2')");
            EPAssertionUtil.AssertPropsPerRow(stmtIterate.GetEnumerator(), fields, new object[][]{new object[] {2}});
            stmtIterate.Dispose();
    
            // test subquery
            EPStatement stmtSubquery = epService.EPAdministrator.CreateEPL("select (select col0 from MyTable(pkey='E3')) as col0 from SupportBean_S0");
            var listener = new SupportUpdateListener();
            stmtSubquery.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("col0"));
            stmtSubquery.Dispose();
    
            // test join
            SupportMessageAssertUtil.TryInvalid(epService, "select col0 from SupportBean_S0, MyTable(pkey='E4')",
                    "Error starting statement: Joins with tables do not allow table filter expressions, please add table filters to the where-clause instead [");
        }
    }
} // end of namespace
