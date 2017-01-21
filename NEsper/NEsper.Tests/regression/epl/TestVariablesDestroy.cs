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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestVariablesDestroy
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddVariable("MyPermanentVar", typeof(string), "thevalue");
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestDestroyReCreateChangeType() {
            String text = "@Name('ABC') create variable long var1 = 2";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(text);
    
            Assert.AreEqual(2L, _epService.EPRuntime.GetVariableValue("var1"));
            AssertStmtsRef("var1", new String[]{"ABC"});
    
            stmtOne.Dispose();
    
            AssertNotFound("var1");
            AssertStmtsRef("var1", null);
    
            text = "@Name('CDE') create variable string var1 = 'a'";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(text);
    
            AssertStmtsRef("var1", new String[]{"CDE"});
            Assert.AreEqual("a", _epService.EPRuntime.GetVariableValue("var1"));
    
            stmtTwo.Dispose();
            AssertNotFound("var1");
        }
    
        [Test]
        public void TestManageDependency() {
            // single variable
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("@Name('S0') create variable boolean var2 = true");
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('S1') select * from SupportBean(var2)");
            AssertStmtsRef("var2", new String[]{"S0", "S1"});
            Assert.AreEqual(true, _epService.EPRuntime.GetVariableValue("var2"));
    
            stmtOne.Dispose();
            AssertStmtsRef("var2", new String[]{"S1"});
            Assert.AreEqual(true, _epService.EPRuntime.GetVariableValue("var2"));
    
            stmtTwo.Dispose();
            AssertStmtsRef("var2", null);
            AssertNotFound("var2");
    
            // multiple variable
            EPStatement stmt1 = _epService.EPAdministrator.CreateEPL("@Name('T0') create variable boolean v1 = true");
            EPStatement stmt2 = _epService.EPAdministrator.CreateEPL("@Name('T1') create variable long v2 = 1");
            EPStatement stmt3 = _epService.EPAdministrator.CreateEPL("@Name('T2') create variable string v3 = 'a'");
            EPStatement stmtUseOne = _epService.EPAdministrator.CreateEPL("@Name('TX') select * from SupportBean(v1, v2=1, v3='a')");
            EPStatement stmtUseTwo = _epService.EPAdministrator.CreateEPL("@Name('TY') select * from SupportBean(v2=2)");
            EPStatement stmtUseThree = _epService.EPAdministrator.CreateEPL("@Name('TZ') select * from SupportBean(v3='A', v1)");
    
            AssertStmtsRef("v1", new String[]{"T0", "TX", "TZ"});
            AssertStmtsRef("v2", new String[]{"T1", "TX", "TY"});
            AssertStmtsRef("v3", new String[]{"T2", "TX", "TZ"});
    
            stmt2.Dispose();
            AssertStmtsRef("v2", new String[]{"TX", "TY"});
    
            stmtUseOne.Dispose();
            AssertStmtsRef("v2", new String[]{"TY"});
    
            stmtUseTwo.Dispose();
            AssertStmtsRef("v2", null);
            AssertNotFound("v2");
    
            EPStatement stmt4 = _epService.EPAdministrator.CreateEPL("@Name('T3') create variable boolean v4 = true");
            EPStatement stmtUseFour = _epService.EPAdministrator.CreateEPL("@Name('TQ') select * from SupportBean(v4)");
    
            AssertStmtsRef("v4", new String[]{"T3", "TQ"});
            Assert.AreEqual(true, _epService.EPRuntime.GetVariableValue("v4"));
    
            stmt1.Dispose();
            stmtUseThree.Dispose();
    
            AssertStmtsRef("v1", null);
            AssertNotFound("v1");
            Assert.AreEqual("a", _epService.EPRuntime.GetVariableValue("v3"));
            AssertStmtsRef("v3", new String[]{"T2"});
    
            stmt3.Dispose();
            AssertNotFound("v3");
    
            stmt4.Dispose();
            stmtUseFour.Dispose();
            AssertNotFound("v4");
    
            Assert.AreEqual(1, _epService.EPRuntime.VariableValueAll.Count);
        }
    
        [Test]
        public void TestConfigAPI() {
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("@Name('S0') create variable boolean var2 = true");
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('S1') select * from SupportBean(var2)");
    
            try {
                _epService.EPAdministrator.Configuration.RemoveVariable("var2", false);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Variable 'var2' is in use by one or more statements", ex.Message);
            }
    
            _epService.EPAdministrator.Configuration.RemoveVariable("var2", true);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
            AssertNotFound("var2");
    
            // try permanent variable
            Assert.AreEqual("thevalue", _epService.EPRuntime.GetVariableValue("MyPermanentVar"));
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL("@Name('S2') select * from SupportBean(MyPermanentVar = 'Y')");
            AssertStmtsRef("MyPermanentVar", new String[]{"S2"});
            stmtThree.Dispose();
            AssertStmtsRef("MyPermanentVar", null);
            Assert.AreEqual("thevalue", _epService.EPRuntime.GetVariableValue("MyPermanentVar"));
        }
    
        private void AssertStmtsRef(String variableName, String[] stmts) {
            EPAssertionUtil.AssertEqualsAnyOrder(stmts, _epService.EPAdministrator.Configuration.GetVariableNameUsedBy(variableName).ToArray());
        }
    
        private void AssertNotFound(String var) {
            try {
                _epService.EPRuntime.GetVariableValue(var);
                Assert.Fail();
            } catch (VariableNotFoundException ex) {
                // expected
            }
            AssertStmtsRef(var, null);
        }
    }
}
