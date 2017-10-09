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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseHintHook 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDatabaseHintHook"/> class.
        /// </summary>
        public TestDatabaseHintHook()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDatabaseHintHook"/> class.
        /// </summary>
        /// <param name="epService">The ep service.</param>
        public TestDatabaseHintHook(EPServiceProvider epService)
        {
            _epService = epService;
        }

        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;
            configDB.ConnectionAutoCommit = true;

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);

            _epService = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            SupportSQLColumnTypeConversion.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _epService.Dispose();
            SupportSQLColumnTypeConversion.Reset();
        }
    
        //@Hook(type=HookType.SQLCOL, hook="this is a sample and not used")
        [Test]
        public void TestOutputColumnConversion()
        {
            _epService.EPAdministrator.Configuration.AddVariable("myvariable", typeof(int), 10);
    
            var fields = new String[] {"myint"};
            var stmtText = "@Hook(Type=HookType.SQLCOL, Hook='" + typeof(SupportSQLColumnTypeConversion).FullName + "')" +
                    "select * from sql:MyDB ['select myint from mytesttable where myint = ${myvariable}']";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("myint"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { false } });
    
            // assert contexts
            var type = SupportSQLColumnTypeConversion.TypeContexts[0];
            Assert.AreEqual("Integer", type.ColumnSqlType);
            Assert.AreEqual("MyDB", type.Db);
            Assert.AreEqual("select myint from mytesttable where myint = ${myvariable}", type.Sql);
            Assert.AreEqual("myint", type.ColumnName);
            Assert.AreEqual(1, type.ColumnNumber);
            Assert.AreEqual(typeof(int?), type.ColumnClassType);
    
            SQLColumnValueContext val = SupportSQLColumnTypeConversion.ValueContexts[0];
            Assert.AreEqual(10, val.ColumnValue);
            Assert.AreEqual("myint", val.ColumnName);
            Assert.AreEqual(1, val.ColumnNumber);
    
            _epService.EPRuntime.SetVariableValue("myvariable", 60);    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { true } });
        }
    
        [Test]
        public void TestInputParameterConversion() {
    
            _epService.EPAdministrator.Configuration.AddVariable("myvariable", typeof(Object), "x10");
    
            String[] fields = new String[] {"myint"};
            String stmtText = "@Hook(Type=HookType.SQLCOL, Hook='" + typeof(SupportSQLColumnTypeConversion).FullName + "')" +
                    "select * from sql:MyDB ['select myint from mytesttable where myint = ${myvariable}']";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SetVariableValue("myvariable", "x60");    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { true } });
    
            SQLInputParameterContext param = SupportSQLColumnTypeConversion.ParamContexts[0];
            Assert.AreEqual(1, param.ParameterNumber);
            Assert.AreEqual("x60", param.ParameterValue);
        }
    
        [Test]
        public void TestOutputRowConversion() {
    
            _epService.EPAdministrator.Configuration.AddVariable("myvariable", typeof(int), 10);
    
            String[] fields = "TheString,IntPrimitive".Split(',');
            String stmtText = "@Hook(Type=HookType.SQLROW, Hook='" + typeof(SupportSQLOutputRowConversion).FullName + "')" +
                    "select * from sql:MyDB ['select * from mytesttable where myint = ${myvariable}']";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.UnderlyingType);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { ">10<", 99010 } });
    
            SQLOutputRowTypeContext type = SupportSQLOutputRowConversion.TypeContexts[0];
            Assert.AreEqual("MyDB", type.Db);
            Assert.AreEqual("select * from mytesttable where myint = ${myvariable}", type.Sql);
            Assert.AreEqual(typeof(int?), type.Fields.Get("myint"));
    
            SQLOutputRowValueContext val = SupportSQLOutputRowConversion.ValueContexts[0];
            Assert.AreEqual(10, val.Values.Get("myint"));
    
            _epService.EPRuntime.SetVariableValue("myvariable", 60);    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { ">60<", 99060 } });
    
            _epService.EPRuntime.SetVariableValue("myvariable", 90);    // greater 50 turns true
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
        }
    }
}
