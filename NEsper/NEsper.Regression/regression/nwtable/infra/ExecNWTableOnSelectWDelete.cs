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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableOnSelectWDelete : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
    
            RunAssertionWindowAgg(epService, true);
            RunAssertionWindowAgg(epService, false);
        }
    
        private void RunAssertionWindowAgg(EPServiceProvider epService, bool namedWindow) {
    
            string[] fieldsWin = "TheString,IntPrimitive".Split(',');
            string[] fieldsSelect = "c0".Split(',');
    
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as SupportBean" :
                    "create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
            EPStatement stmtWin = epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            string eplSelectDelete = "on S0 as s0 " +
                    "select and delete window(win.*).aggregate(0,(result,value) => result+value.IntPrimitive) as c0 " +
                    "from MyInfra as win where s0.p00=win.TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eplSelectDelete);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fieldsWin, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            } else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            }
    
            // select and delete bean E1
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{1});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[][]{new object[] {"E2", 2}});
    
            // add some E2 events
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[][]{new object[] {"E2", 2}, new object[] {"E2", 3}, new object[] {"E2", 4}});
    
            // select and delete beans E2
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{2 + 3 + 4});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new Object[0][]);
    
            // test SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplSelectDelete);
            Assert.AreEqual(eplSelectDelete, model.ToEPL());
            EPStatement stmtSD = epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplSelectDelete, stmtSD.Text);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
} // end of namespace
