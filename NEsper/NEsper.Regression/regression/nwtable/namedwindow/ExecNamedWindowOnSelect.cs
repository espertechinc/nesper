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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOnSelect : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            SupportQueryPlanIndexHook.Reset();
            var fields = new string[]{"TheString", "IntPrimitive"};
    
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select * from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBean).FullName + "(TheString like 'E%')";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create on-select stmt
            string stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " insert into MyStream select mywin.* from MyWindow as mywin order by TheString asc";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
            Assert.AreEqual(StatementType.ON_INSERT, ((EPStatementSPI) stmtSelect).StatementMetadata.StatementType);
    
            // create consuming statement
            string stmtTextConsumer = "select * from default.MyStream";
            EPStatement stmtConsumer = epService.EPAdministrator.CreateEPL(stmtTextConsumer);
            var listenerConsumer = new SupportUpdateListener();
            stmtConsumer.Events += listenerConsumer.Update;
    
            // create second inserting statement
            string stmtTextInsertTwo = "insert into MyStream select * from " + typeof(SupportBean).FullName + "(TheString like 'I%')";
            epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            // send event
            SendSupportBean(epService, "E1", 1);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerConsumer.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // fire trigger
            SendSupportBean_A(epService, "A1");
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertProps(listenerConsumer.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            // insert via 2nd insert into
            SendSupportBean(epService, "I2", 2);
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertProps(listenerConsumer.AssertOneGetNewAndReset(), fields, new object[]{"I2", 2});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // send event
            SendSupportBean(epService, "E3", 3);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerConsumer.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
    
            // fire trigger
            SendSupportBean_A(epService, "A2");
            Assert.AreEqual(1, listenerSelect.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(listenerSelect.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
            listenerSelect.Reset();
            Assert.AreEqual(2, listenerConsumer.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(listenerConsumer.GetNewDataListFlattened(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
            listenerConsumer.Reset();
    
            // check type
            EventType consumerType = stmtConsumer.EventType;
            Assert.AreEqual(typeof(string), consumerType.GetPropertyType("TheString"));
            Assert.IsTrue(consumerType.PropertyNames.Length > 10);
            Assert.AreEqual(typeof(SupportBean), consumerType.UnderlyingType);
    
            // check type
            EventType onSelectType = stmtSelect.EventType;
            Assert.AreEqual(typeof(string), onSelectType.GetPropertyType("TheString"));
            Assert.IsTrue(onSelectType.PropertyNames.Length > 10);
            Assert.AreEqual(typeof(SupportBean), onSelectType.UnderlyingType);
    
            // delete all from named window
            string stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyWindow";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
            SendSupportBean_B(epService, "B1");
    
            // fire trigger - nothing to insert
            SendSupportBean_A(epService, "A3");
    
            stmtConsumer.Dispose();
            stmtSelect.Dispose();
            stmtCreate.Dispose();
        }
    
        private void SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean_B(EPServiceProvider epService, string id) {
            var bean = new SupportBean_B(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
