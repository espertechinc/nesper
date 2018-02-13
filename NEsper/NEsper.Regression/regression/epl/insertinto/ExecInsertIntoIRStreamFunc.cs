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

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    public class ExecInsertIntoIRStreamFunc : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            var listenerInsert = new SupportUpdateListener();
            var listenerSelect = new SupportUpdateListener();
    
            string[] fields = "c0,c1".Split(',');
            string stmtTextOne = "insert irstream into MyStream " +
                    "select irstream theString as c0, Istream() as c1 " +
                    "from SupportBean#lastevent";
            epService.EPAdministrator.CreateEPL(stmtTextOne).AddListener(listenerInsert);
    
            string stmtTextTwo = "select * from MyStream";
            epService.EPAdministrator.CreateEPL(stmtTextTwo).AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E1", true});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listenerInsert.AssertPairGetIRAndReset(), fields, new Object[]{"E2", true}, new Object[]{"E1", false});
            EPAssertionUtil.AssertPropsPerRow(listenerSelect.GetAndResetDataListsFlattened(), fields, new Object[][]{new object[] {"E2", true}, new object[] {"E1", false}}, new Object[0][]);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(listenerInsert.AssertPairGetIRAndReset(), fields, new Object[]{"E3", true}, new Object[]{"E2", false});
            EPAssertionUtil.AssertPropsPerRow(listenerSelect.GetAndResetDataListsFlattened(), fields, new Object[][]{new object[] {"E3", true}, new object[] {"E2", false}}, new Object[0][]);
    
            // test SODA
            string eplModel = "select Istream() from SupportBean";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplModel);
            Assert.AreEqual(eplModel, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplModel, stmt.Text);
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("Istream()"));
    
            // test join
            epService.EPAdministrator.DestroyAllStatements();
            fields = "c0,c1,c2".Split(',');
            string stmtTextJoin = "select irstream theString as c0, id as c1, Istream() as c2 " +
                    "from SupportBean#lastevent, SupportBean_S0#lastevent";
            epService.EPAdministrator.CreateEPL(stmtTextJoin).AddListener(listenerSelect);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 10, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listenerSelect.LastOldData[0], fields, new Object[]{"E1", 10, false});
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fields, new Object[]{"E2", 10, true});
        }
    }
} // end of namespace
