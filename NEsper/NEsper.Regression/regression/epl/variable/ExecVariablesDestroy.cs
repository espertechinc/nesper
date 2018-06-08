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
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    public class ExecVariablesDestroy : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddVariable("MyPermanentVar", typeof(string), "thevalue");
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionDestroyReCreateChangeType(epService);
            RunAssertionManageDependency(epService);
            RunAssertionConfigAPI(epService);
        }
    
        private void RunAssertionDestroyReCreateChangeType(EPServiceProvider epService) {
            string text = "@Name('ABC') create variable long var1 = 2";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(text);
    
            Assert.AreEqual(2L, epService.EPRuntime.GetVariableValue("var1"));
            AssertStmtsRef(epService, "var1", new string[]{"ABC"});
    
            stmtOne.Dispose();
    
            AssertNotFound(epService, "var1");
            AssertStmtsRef(epService, "var1", null);
    
            text = "@Name('CDE') create variable string var1 = 'a'";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(text);
    
            AssertStmtsRef(epService, "var1", new string[]{"CDE"});
            Assert.AreEqual("a", epService.EPRuntime.GetVariableValue("var1"));
    
            stmtTwo.Dispose();
            AssertNotFound(epService, "var1");
        }
    
        private void RunAssertionManageDependency(EPServiceProvider epService) {
            // single variable
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('S0') create variable bool var2 = true");
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("@Name('S1') select * from SupportBean(var2)");
            AssertStmtsRef(epService, "var2", new string[]{"S0", "S1"});
            Assert.AreEqual(true, epService.EPRuntime.GetVariableValue("var2"));
    
            stmtOne.Dispose();
            AssertStmtsRef(epService, "var2", new string[]{"S1"});
            Assert.AreEqual(true, epService.EPRuntime.GetVariableValue("var2"));
    
            stmtTwo.Dispose();
            AssertStmtsRef(epService, "var2", null);
            AssertNotFound(epService, "var2");
    
            // multiple variable
            EPStatement stmt1 = epService.EPAdministrator.CreateEPL("@Name('T0') create variable bool v1 = true");
            EPStatement stmt2 = epService.EPAdministrator.CreateEPL("@Name('T1') create variable long v2 = 1");
            EPStatement stmt3 = epService.EPAdministrator.CreateEPL("@Name('T2') create variable string v3 = 'a'");
            EPStatement stmtUseOne = epService.EPAdministrator.CreateEPL("@Name('TX') select * from SupportBean(v1, v2=1, v3='a')");
            EPStatement stmtUseTwo = epService.EPAdministrator.CreateEPL("@Name('TY') select * from SupportBean(v2=2)");
            EPStatement stmtUseThree = epService.EPAdministrator.CreateEPL("@Name('TZ') select * from SupportBean(v3='A', v1)");
    
            AssertStmtsRef(epService, "v1", new string[]{"T0", "TX", "TZ"});
            AssertStmtsRef(epService, "v2", new string[]{"T1", "TX", "TY"});
            AssertStmtsRef(epService, "v3", new string[]{"T2", "TX", "TZ"});
    
            stmt2.Dispose();
            AssertStmtsRef(epService, "v2", new string[]{"TX", "TY"});
    
            stmtUseOne.Dispose();
            AssertStmtsRef(epService, "v2", new string[]{"TY"});
    
            stmtUseTwo.Dispose();
            AssertStmtsRef(epService, "v2", null);
            AssertNotFound(epService, "v2");
    
            EPStatement stmt4 = epService.EPAdministrator.CreateEPL("@Name('T3') create variable bool v4 = true");
            EPStatement stmtUseFour = epService.EPAdministrator.CreateEPL("@Name('TQ') select * from SupportBean(v4)");
    
            AssertStmtsRef(epService, "v4", new string[]{"T3", "TQ"});
            Assert.AreEqual(true, epService.EPRuntime.GetVariableValue("v4"));
    
            stmt1.Dispose();
            stmtUseThree.Dispose();
    
            AssertStmtsRef(epService, "v1", null);
            AssertNotFound(epService, "v1");
            Assert.AreEqual("a", epService.EPRuntime.GetVariableValue("v3"));
            AssertStmtsRef(epService, "v3", new string[]{"T2"});
    
            stmt3.Dispose();
            AssertNotFound(epService, "v3");
    
            stmt4.Dispose();
            stmtUseFour.Dispose();
            AssertNotFound(epService, "v4");
    
            Assert.AreEqual(1, epService.EPRuntime.VariableValueAll.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionConfigAPI(EPServiceProvider epService) {
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('S0') create variable bool var2 = true");
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("@Name('S1') select * from SupportBean(var2)");
    
            try {
                epService.EPAdministrator.Configuration.RemoveVariable("var2", false);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Variable 'var2' is in use by one or more statements", ex.Message);
            }
    
            epService.EPAdministrator.Configuration.RemoveVariable("var2", true);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
            AssertNotFound(epService, "var2");
    
            // try permanent variable
            Assert.AreEqual("thevalue", epService.EPRuntime.GetVariableValue("MyPermanentVar"));
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL("@Name('S2') select * from SupportBean(MyPermanentVar = 'Y')");
            AssertStmtsRef(epService, "MyPermanentVar", new string[]{"S2"});
            stmtThree.Dispose();
            AssertStmtsRef(epService, "MyPermanentVar", null);
            Assert.AreEqual("thevalue", epService.EPRuntime.GetVariableValue("MyPermanentVar"));
        }
    
        private void AssertStmtsRef(EPServiceProvider epService, string variableName, string[] stmts) {
            EPAssertionUtil.AssertEqualsAnyOrder(stmts, epService.EPAdministrator.Configuration.GetVariableNameUsedBy(variableName).ToArray());
        }
    
        private void AssertNotFound(EPServiceProvider epService, string var) {
            try {
                epService.EPRuntime.GetVariableValue(var);
                Assert.Fail();
            } catch (VariableNotFoundException) {
                // expected
            }
            AssertStmtsRef(epService, var, null);
        }
    }
} // end of namespace
