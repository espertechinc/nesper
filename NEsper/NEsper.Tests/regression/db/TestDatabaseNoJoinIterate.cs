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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseNoJoinIterate 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();

            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Unspecified;
            configDB.ConnectionAutoCommit = true;

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
            configuration.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;

            _epService = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _epService.Dispose();
        }
    
        [Test]
        public void TestExpressionPoll()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.CreateEPL("create variable boolean queryvar_bool");
            _epService.EPAdministrator.CreateEPL("create variable int queryvar_int");
            _epService.EPAdministrator.CreateEPL("create variable int lower");
            _epService.EPAdministrator.CreateEPL("create variable int upper");
            _epService.EPAdministrator.CreateEPL("on SupportBean set queryvar_int=IntPrimitive, queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed");
    
            // Test int and singlerow
            String stmtText = "select myint from sql:MyDB ['select myint from mytesttable where ${queryvar_int -2} = mytesttable.mybigint']";
            EPStatementSPI stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] { "myint" }, null);
    
            SendSupportBeanEvent(5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] { "myint" }, new Object[][] { new Object[] { 30 } });
    
            stmt.Dispose();
            Assert.IsFalse(_listener.IsInvoked);
    
            // Test multi-parameter and multi-row
            stmtText = "select myint from sql:MyDB ['select myint from mytesttable where mytesttable.mybigint between ${queryvar_int-2} and ${queryvar_int+2}'] order by myint";
            stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] { "myint" }, new Object[][] { new Object[] { 30 }, new Object[] { 40 }, new Object[] { 50 }, new Object[] { 60 }, new Object[] { 70 } });
            stmt.Dispose();
    
            // Test substitution parameters
            try {
                stmtText = "select myint from sql:MyDB ['select myint from mytesttable where mytesttable.mybigint between ${?} and ${queryvar_int+?}'] order by myint";
                _epService.EPAdministrator.PrepareEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("EPL substitution parameters are not allowed in SQL ${...} expressions, consider using a variable instead [select myint from sql:MyDB ['select myint from mytesttable where mytesttable.mybigint between ${?} and ${queryvar_int+?}'] order by myint]", ex.Message);
            }
        }
    
        [Test]
        public void TestVariablesPoll()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.CreateEPL("create variable boolean queryvar_bool");
            _epService.EPAdministrator.CreateEPL("create variable int queryvar_int");
            _epService.EPAdministrator.CreateEPL("create variable int lower");
            _epService.EPAdministrator.CreateEPL("create variable int upper");
            _epService.EPAdministrator.CreateEPL("on SupportBean set queryvar_int=IntPrimitive, queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed");
    
            // Test int and singlerow
            String stmtText = "select myint from sql:MyDB ['select myint from mytesttable where ${queryvar_int} = mytesttable.mybigint']";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] {"myint"}, null);
    
            SendSupportBeanEvent(5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new String[] {"myint"}, new Object[][] { new Object[] {50}});
    
            stmt.Dispose();
            Assert.IsFalse(_listener.IsInvoked);
    
            // Test boolean and multirow
            stmtText = "select * from sql:MyDB ['select mybigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by mybigint']";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            String[] fields = new String[] {"mybigint", "mybool"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(true, 10, 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {1L, true}, new Object[] {4L, true}});
    
            SendSupportBeanEvent(false, 30, 80);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {3L, false}, new Object[] {5L, false}, new Object[] {6L, false}});
    
            SendSupportBeanEvent(true, 20, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(true, 20, 60);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {4L, true}});
        }
    
        private void SendSupportBeanEvent(int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(bool boolPrimitive, int intPrimitive, int intBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
