///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseNoJoinIterate : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Unspecified;
            configDB.ConnectionAutoCommit = true;

            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionExpressionPoll(epService);
            RunAssertionVariablesPoll(epService);
        }
    
        private void RunAssertionExpressionPoll(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variable bool queryvar_bool");
            epService.EPAdministrator.CreateEPL("create variable int queryvar_int");
            epService.EPAdministrator.CreateEPL("create variable int lower");
            epService.EPAdministrator.CreateEPL("create variable int upper");
            epService.EPAdministrator.CreateEPL("on SupportBean set queryvar_int=IntPrimitive, queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed");
    
            // Test int and singlerow
            string stmtText = "select myint from sql:MyDB ['select myint from mytesttable where ${queryvar_int -2} = mytesttable.mybigint']";
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"myint"}, null);
    
            SendSupportBeanEvent(epService, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"myint"}, new object[][]{new object[] {30}});
    
            stmt.Dispose();
            Assert.IsFalse(listener.IsInvoked);
    
            // Test multi-parameter and multi-row
            stmtText = "select myint from sql:MyDB ['select myint from mytesttable where mytesttable.mybigint between ${queryvar_int-2} and ${queryvar_int+2}'] order by myint";
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"myint"}, new object[][]{new object[] {30}, new object[] {40}, new object[] {50}, new object[] {60}, new object[] {70}});
            stmt.Dispose();
    
            // Test substitution parameters
            try {
                stmtText = "select myint from sql:MyDB ['select myint from mytesttable where mytesttable.mybigint between ${?} and ${queryvar_int+?}'] order by myint";
                epService.EPAdministrator.PrepareEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("EPL substitution parameters are not allowed in SQL ${...} expressions, consider using a variable instead [select myint from sql:MyDB ['select myint from mytesttable where mytesttable.mybigint between ${?} and ${queryvar_int+?}'] order by myint]", ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionVariablesPoll(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variable bool queryvar_bool");
            epService.EPAdministrator.CreateEPL("create variable int queryvar_int");
            epService.EPAdministrator.CreateEPL("create variable int lower");
            epService.EPAdministrator.CreateEPL("create variable int upper");
            epService.EPAdministrator.CreateEPL("on SupportBean set queryvar_int=IntPrimitive, queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed");
    
            // Test int and singlerow
            string stmtText = "select myint from sql:MyDB ['select myint from mytesttable where ${queryvar_int} = mytesttable.mybigint']";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"myint"}, null);
    
            SendSupportBeanEvent(epService, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"myint"}, new object[][]{new object[] {50}});
    
            stmt.Dispose();
            Assert.IsFalse(listener.IsInvoked);
    
            // Test bool and multirow
            stmtText = "select * from sql:MyDB ['select mybigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by mybigint']";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            var fields = new string[]{"mybigint", "mybool"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, true, 10, 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L, true}, new object[] {4L, true}});
    
            SendSupportBeanEvent(epService, false, 30, 80);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {3L, false}, new object[] {5L, false}, new object[] {6L, false}});
    
            SendSupportBeanEvent(epService, true, 20, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, true, 20, 60);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {4L, true}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, bool boolPrimitive, int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
