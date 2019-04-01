///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using com.espertech.esper.client;

using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.db;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseHintHook : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;
            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionOutputColumnConversion(epService);
            RunAssertionInputParameterConversion(epService);
            RunAssertionOutputRowConversion(epService);
        }
    
        //@Hook(type=HookType.SQLCOL, hook="this is a sample and not used")
        private void RunAssertionOutputColumnConversion(EPServiceProvider epService) {
            SupportSQLColumnTypeConversion.Reset();
            epService.EPAdministrator.Configuration.AddVariable("myvariableOCC", typeof(int), 10);
    
            var fields = new string[]{"myint"};
            string stmtText = "@Hook(Type=HookType.SQLCOL, Hook='" + typeof(SupportSQLColumnTypeConversion).FullName + "')" +
                    "select * from sql:MyDB ['select myint from mytesttable where myint = ${myvariableOCC}']";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("myint"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {false}});
    
            // assert contexts
            SQLColumnTypeContext type = SupportSQLColumnTypeConversion.TypeContexts[0];
            Assert.AreEqual("System.Int32", type.ColumnSqlType);
            Assert.AreEqual("MyDB", type.Db);
            Assert.AreEqual("select myint from mytesttable where myint = ${myvariableOCC}", type.Sql);
            Assert.AreEqual("myint", type.ColumnName);
            Assert.AreEqual(1, type.ColumnNumber);
            Assert.AreEqual(typeof(int?), type.ColumnClassType);
    
            SQLColumnValueContext val = SupportSQLColumnTypeConversion.ValueContexts[0];
            Assert.AreEqual(10, val.ColumnValue);
            Assert.AreEqual("myint", val.ColumnName);
            Assert.AreEqual(1, val.ColumnNumber);
    
            epService.EPRuntime.SetVariableValue("myvariableOCC", 60);    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {true}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInputParameterConversion(EPServiceProvider epService) {
            SupportSQLColumnTypeConversion.Reset();
            epService.EPAdministrator.Configuration.AddVariable("myvariableIPC", typeof(Object), "x10");
    
            var fields = new string[]{"myint"};
            string stmtText = "@Hook(Type=HookType.SQLCOL, Hook='" + typeof(SupportSQLColumnTypeConversion).FullName + "')" +
                    "select * from sql:MyDB ['select myint from mytesttable where myint = ${myvariableIPC}']";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SetVariableValue("myvariableIPC", "x60");    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {true}});
    
            SQLInputParameterContext param = SupportSQLColumnTypeConversion.ParamContexts[0];
            Assert.AreEqual(1, param.ParameterNumber);
            Assert.AreEqual("x60", param.ParameterValue);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputRowConversion(EPServiceProvider epService) {
            SupportSQLColumnTypeConversion.Reset();
            epService.EPAdministrator.Configuration.AddVariable("myvariableORC", typeof(int), 10);
    
            string[] fields = "TheString,IntPrimitive".Split(',');
            string stmtText = "@Hook(Type=HookType.SQLROW, Hook='" + typeof(SupportSQLOutputRowConversion).FullName + "')" +
                    "select * from sql:MyDB ['select * from mytesttable where myint = ${myvariableORC}']";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.UnderlyingType);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {">10<", 99010}});
    
            SQLOutputRowTypeContext type = SupportSQLOutputRowConversion.TypeContexts[0];
            Assert.AreEqual("MyDB", type.Db);
            Assert.AreEqual("select * from mytesttable where myint = ${myvariableORC}", type.Sql);
            Assert.AreEqual(typeof(int?), type.Fields.Get("myint"));
    
            SQLOutputRowValueContext val = SupportSQLOutputRowConversion.ValueContexts[0];
            Assert.AreEqual(10, val.Values.Get("myint"));
    
            epService.EPRuntime.SetVariableValue("myvariableORC", 60);    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {">60<", 99060}});
    
            epService.EPRuntime.SetVariableValue("myvariableORC", 90);    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            stmt.Dispose();
        }
    }
} // end of namespace
