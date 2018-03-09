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


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOM : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCompile(epService);
            RunAssertionOM(epService);
            RunAssertionOMCreateTableSyntax(epService);
        }
    
        private void RunAssertionCompile(EPServiceProvider epService) {
            var fields = new string[]{"key", "value"};
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            EPStatementObjectModel modelCreate = epService.EPAdministrator.CompileEPL(stmtTextCreate);
            EPStatement stmtCreate = epService.EPAdministrator.Create(modelCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            Assert.AreEqual("create window MyWindow#keepall as select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName, modelCreate.ToEPL());
    
            string stmtTextOnSelect = "on " + typeof(SupportBean_B).FullName + " select mywin.* from MyWindow as mywin";
            EPStatementObjectModel modelOnSelect = epService.EPAdministrator.CompileEPL(stmtTextOnSelect);
            EPStatement stmtOnSelect = epService.EPAdministrator.Create(modelOnSelect);
            var listenerOnSelect = new SupportUpdateListener();
            stmtOnSelect.Events += listenerOnSelect.Update;
    
            string stmtTextInsert = "insert into MyWindow select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            EPStatementObjectModel modelInsert = epService.EPAdministrator.CompileEPL(stmtTextInsert);
            EPStatement stmtInsert = epService.EPAdministrator.Create(modelInsert);
    
            string stmtTextSelectOne = "select irstream key, value*2 as value from MyWindow(key is not null)";
            EPStatementObjectModel modelSelect = epService.EPAdministrator.CompileEPL(stmtTextSelectOne);
            EPStatement stmtSelectOne = epService.EPAdministrator.Create(modelSelect);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
            Assert.AreEqual(stmtTextSelectOne, modelSelect.ToEPL());
    
            // send events
            SendSupportBean(epService, "E1", 10L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
    
            SendSupportBean(epService, "E2", 20L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol=s1.key";
            EPStatementObjectModel modelDelete = epService.EPAdministrator.CompileEPL(stmtTextDelete);
            epService.EPAdministrator.Create(modelDelete);
            Assert.AreEqual("on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol=s1.key", modelDelete.ToEPL());
    
            // send delete event
            SendMarketBean(epService, "E1");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
    
            // send delete event again, none deleted now
            SendMarketBean(epService, "E1");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            Assert.IsFalse(listenerWindow.IsInvoked);
    
            // send delete event
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
    
            // trigger on-select on empty window
            Assert.IsFalse(listenerOnSelect.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsFalse(listenerOnSelect.IsInvoked);
    
            SendSupportBean(epService, "E3", 30L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 60L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30L});
    
            // trigger on-select on the filled window
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertProps(listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30L});
    
            stmtSelectOne.Dispose();
            stmtInsert.Dispose();
            stmtCreate.Dispose();
        }
    
        private void RunAssertionOM(EPServiceProvider epService) {
            var fields = new string[]{"key", "value"};
    
            // create window object model
            var model = new EPStatementObjectModel();
            model.CreateWindow = CreateWindowClause.Create("MyWindow").AddView("keepall");
            model.SelectClause = SelectClause.Create()
                    .AddWithAsProvidedName("TheString", "key")
                    .AddWithAsProvidedName("LongBoxed", "value");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
    
            EPStatement stmtCreate = epService.EPAdministrator.Create(model);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            Assert.AreEqual(stmtTextCreate, model.ToEPL());
    
            string stmtTextInsert = "insert into MyWindow select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            EPStatementObjectModel modelInsert = epService.EPAdministrator.CompileEPL(stmtTextInsert);
            EPStatement stmtInsert = epService.EPAdministrator.Create(modelInsert);
    
            // Consumer statement object model
            model = new EPStatementObjectModel();
            Expression multi = Expressions.Multiply(Expressions.Property("value"), Expressions.Constant(2));
            model.SelectClause = SelectClause.Create(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add("key")
                    .Add(multi, "value");
            model.FromClause = FromClause.Create(FilterStream.Create("MyWindow", Expressions.IsNotNull("value")));
    
            EPStatement stmtSelectOne = epService.EPAdministrator.Create(model);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
            string stmtTextSelectOne = "select irstream key, value*2 as value from MyWindow(value is not null)";
            Assert.AreEqual(stmtTextSelectOne, model.ToEPL());
    
            // send events
            SendSupportBean(epService, "E1", 10L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
    
            SendSupportBean(epService, "E2", 20L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
    
            // create delete stmt
            model = new EPStatementObjectModel();
            model.OnExpr = OnClause.CreateOnDelete("MyWindow", "s1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName, "s0"));
            model.WhereClause = Expressions.EqProperty("s0.symbol", "s1.key");
            epService.EPAdministrator.Create(model);
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol=s1.key";
            Assert.AreEqual(stmtTextDelete, model.ToEPL());
    
            // send delete event
            SendMarketBean(epService, "E1");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
    
            // send delete event again, none deleted now
            SendMarketBean(epService, "E1");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            Assert.IsFalse(listenerWindow.IsInvoked);
    
            // send delete event
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
    
            // On-select object model
            model = new EPStatementObjectModel();
            model.OnExpr = OnClause.CreateOnSelect("MyWindow", "s1");
            model.WhereClause = Expressions.EqProperty("s0.id", "s1.key");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean_B).FullName, "s0"));
            model.SelectClause = SelectClause.CreateStreamWildcard("s1");
            EPStatement statement = epService.EPAdministrator.Create(model);
            var listenerOnSelect = new SupportUpdateListener();
            statement.Events += listenerOnSelect.Update;
            string stmtTextOnSelect = "on " + typeof(SupportBean_B).FullName + " as s0 select s1.* from MyWindow as s1 where s0.id=s1.key";
            Assert.AreEqual(stmtTextOnSelect, model.ToEPL());
    
            // send some more events
            SendSupportBean(epService, "E3", 30L);
            SendSupportBean(epService, "E4", 40L);
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsFalse(listenerOnSelect.IsInvoked);
    
            // trigger on-select
            epService.EPRuntime.SendEvent(new SupportBean_B("E3"));
            EPAssertionUtil.AssertProps(listenerOnSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30L});
    
            stmtSelectOne.Dispose();
            stmtInsert.Dispose();
            stmtCreate.Dispose();
        }
    
        private void RunAssertionOMCreateTableSyntax(EPServiceProvider epService) {
            string expected = "create window MyWindowOM#keepall as (a1 string, a2 double, a3 int)";
    
            // create window object model
            var model = new EPStatementObjectModel();
            CreateWindowClause clause = CreateWindowClause.Create("MyWindowOM").AddView("keepall");
            clause.AddColumn(new SchemaColumnDesc("a1", "string", false, false));
            clause.AddColumn(new SchemaColumnDesc("a2", "double", false, false));
            clause.AddColumn(new SchemaColumnDesc("a3", "int", false, false));
            model.CreateWindow = clause;
            Assert.AreEqual(expected, model.ToEPL());
    
            EPStatement stmtCreate = epService.EPAdministrator.Create(model);
            Assert.AreEqual(expected, stmtCreate.Text);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
