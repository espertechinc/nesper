///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestVariablesCreate 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _listenerCreateOne;
        private SupportUpdateListener _listenerCreateTwo;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listenerCreateOne = new SupportUpdateListener();
            _listenerCreateTwo = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _listenerCreateOne = null;
            _listenerCreateTwo = null;
        }
    
        [Test]
        public void TestOM()
        {
            var model = new EPStatementObjectModel();
            model.CreateVariable = CreateVariableClause.Create("long", "var1", null);
            _epService.EPAdministrator.Create(model);
            Assert.AreEqual("create variable long var1", model.ToEPL());
    
            model = new EPStatementObjectModel();
            model.CreateVariable = CreateVariableClause.Create("string", "var2", Expressions.Constant("abc"));
            _epService.EPAdministrator.Create(model);
            Assert.AreEqual("create variable string var2 = \"abc\"", model.ToEPL());
    
            String stmtTextSelect = "select var1, var2 from " + typeof(SupportBean).FullName;
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.Events += _listener.Update;
    
            String[] fieldsVar = new String[] {"var1", "var2"};
            SendSupportBean("E1", 10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsVar, new Object[] {null, "abc"});

            SupportModelHelper.CompileCreate(_epService, "create variable double[] arrdouble = {1.0d,2.0d}");
        }
    
        [Test]
        public void TestCompileStartStop()
        {
            String text = "create variable long var1";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(text);
            _epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, model.ToEPL());
    
            text = "create variable string var2 = \"abc\"";
            model = _epService.EPAdministrator.CompileEPL(text);
            _epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, model.ToEPL());
    
            String stmtTextSelect = "select var1, var2 from " + typeof(SupportBean).FullName;
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.Events += _listener.Update;
    
            String[] fieldsVar = new String[] {"var1", "var2"};
            SendSupportBean("E1", 10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsVar, new Object[] {null, "abc"});
    
            // ESPER-545
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            String createText = "create variable int FOO = 0";
            _epService.EPAdministrator.CreateEPL(createText);
            _epService.EPAdministrator.CreateEPL("on pattern [every SupportBean] set FOO = FOO + 1");
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(1, _epService.EPRuntime.GetVariableValue("FOO"));
            
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.CreateEPL(createText);
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("FOO"));
        }
    
        [Test]
        public void TestSubscribeAndIterate()
        {
            String stmtCreateTextOne = "create variable long var1 = null";
            EPStatement stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtCreateTextOne);
            Assert.AreEqual(StatementType.CREATE_VARIABLE, ((EPStatementSPI) stmtCreateOne).StatementMetadata.StatementType);
            stmtCreateOne.Events += _listenerCreateOne.Update;
            String[] fieldsVar1 = new String[] {"var1"};
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new Object[][] { new Object[] {null}});
            Assert.IsFalse(_listenerCreateOne.IsInvoked);
    
            EventType typeSet = stmtCreateOne.EventType;
            Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var1"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, new String[] {"var1"}));
    
            String stmtCreateTextTwo = "create variable long var2 = 20";
            EPStatement stmtCreateTwo = _epService.EPAdministrator.CreateEPL(stmtCreateTextTwo);
            stmtCreateTwo.Events += _listenerCreateTwo.Update;
            String[] fieldsVar2 = new String[] {"var2"};
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new Object[][] { new Object[] {20L}});
            Assert.IsFalse(_listenerCreateTwo.IsInvoked);
    
            String stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1 = IntPrimitive * 2, var2 = var1 + 1";
            _epService.EPAdministrator.CreateEPL(stmtTextSet);
    
            SendSupportBean("E1", 100);
            EPAssertionUtil.AssertProps(_listenerCreateOne.LastNewData[0], fieldsVar1, new Object[] {200L});
            EPAssertionUtil.AssertProps(_listenerCreateOne.LastOldData[0], fieldsVar1, new Object[] {null});
            _listenerCreateOne.Reset();
            EPAssertionUtil.AssertProps(_listenerCreateTwo.LastNewData[0], fieldsVar2, new Object[] {201L});
            EPAssertionUtil.AssertProps(_listenerCreateTwo.LastOldData[0], fieldsVar2, new Object[] {20L});
            _listenerCreateOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new Object[][] { new Object[] {200L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new Object[][] { new Object[] {201L}});
    
            SendSupportBean("E2", 200);
            EPAssertionUtil.AssertProps(_listenerCreateOne.LastNewData[0], fieldsVar1, new Object[] {400L});
            EPAssertionUtil.AssertProps(_listenerCreateOne.LastOldData[0], fieldsVar1, new Object[] {200L});
            _listenerCreateOne.Reset();
            EPAssertionUtil.AssertProps(_listenerCreateTwo.LastNewData[0], fieldsVar2, new Object[] {401L});
            EPAssertionUtil.AssertProps(_listenerCreateTwo.LastOldData[0], fieldsVar2, new Object[] {201L});
            _listenerCreateOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new Object[][] { new Object[] {400L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new Object[][] { new Object[] {401L}});
    
            stmtCreateTwo.Stop();
            stmtCreateTwo.Start();
    
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new Object[][] { new Object[] {400L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new Object[][] { new Object[] {20L}});
        }
    
        [Test]
        public void TestDeclarationAndSelect()
        {
            Object[][] variables = new Object[][] {
                    new Object[] {"var1", "int", "1", 1},
                    new Object[] {"var2", "int", "'2'", 2},
                    new Object[] {"var3", "INTEGER", " 3+2 ", 5},
                    new Object[] {"var4", "bool", " true|false ", true},
                    new Object[] {"var5", "boolean", " var1=1 ", true},
                    new Object[] {"var6", "double", " 1.11 ", 1.11d},
                    new Object[] {"var7", "double", " 1.20d ", 1.20d},
                    new Object[] {"var8", "Double", " ' 1.12 ' ", 1.12d},
                    new Object[] {"var9", "float", " 1.13f*2f ", 2.26f},
                    new Object[] {"var10", "FLOAT", " -1.14f ", -1.14f},
                    new Object[] {"var11", "string", " ' XXXX ' ", " XXXX "},
                    new Object[] {"var12", "string", " \"a\" ", "a"},
                    new Object[] {"var13", "character", "'a'", 'a'},
                    new Object[] {"var14", "char", "'x'", 'x'},
                    new Object[] {"var15", "short", " 20 ", (short) 20},
                    new Object[] {"var16", "SHORT", " ' 9 ' ", (short)9},
                    new Object[] {"var17", "long", " 20*2 ", (long) 40},
                    new Object[] {"var18", "LONG", " ' 9 ' ", (long)9},
                    new Object[] {"var19", "byte", " 20*2 ", (byte) 40},
                    new Object[] {"var20", "BYTE", "9+1", (byte)10},
                    new Object[] {"var21", "int", null, null},
                    new Object[] {"var22", "bool", null, null},
                    new Object[] {"var23", "double", null, null},
                    new Object[] {"var24", "float", null, null},
                    new Object[] {"var25", "string", null, null},
                    new Object[] {"var26", "char", null, null},
                    new Object[] {"var27", "short", null, null},
                    new Object[] {"var28", "long", null, null},
                    new Object[] {"var29", "BYTE", null, null},
            };
    
            for (int i = 0; i < variables.Length; i++)
            {
                String text = "create variable " + variables[i][1] + " " + variables[i][0];
                if (variables[i][2] != null)
                {
                    text += " = " + variables[i][2];
                }
    
                _epService.EPAdministrator.CreateEPL(text);
            }
    
            // select all variables
            StringBuilder buf = new StringBuilder();
            String delimiter = "";
            buf.Append("select ");
            for (int i = 0; i < variables.Length; i++)
            {
                buf.Append(delimiter);
                buf.Append(variables[i][0]);
                delimiter = ",";
            }
            buf.Append(" from ");
            buf.Append(typeof(SupportBean).FullName);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(buf.ToString());
            stmt.Events += _listener.Update;
    
            // assert initialization values
            SendSupportBean("E1", 1);
            EventBean received = _listener.AssertOneGetNewAndReset();
            for (int i = 0; i < variables.Length; i++)
            {
                Assert.AreEqual(variables[i][3], received.Get((String)variables[i][0]));
            }
        }
    
        [Test]
        public void TestInvalid()
        {
            String stmt = "create variable somedummy myvar = 10";
            TryInvalid(stmt, "Error starting statement: Cannot create variable: Cannot create variable 'myvar', type 'somedummy' is not a recognized type [create variable somedummy myvar = 10]");
    
            stmt = "create variable string myvar = 5";
            TryInvalid(stmt, "Error starting statement: Cannot create variable: Variable 'myvar' of declared type System.String cannot be initialized by a value of type " + typeof(int).FullName + " [create variable string myvar = 5]");
    
            stmt = "create variable string myvar = 'a'";
            _epService.EPAdministrator.CreateEPL("create variable string myvar = 'a'");
            TryInvalid(stmt, "Error starting statement: Cannot create variable: Variable by name 'myvar' has already been created [create variable string myvar = 'a']");
    
            TryInvalid("select * from " + typeof(SupportBean).FullName + " output every somevar events",
                "Error starting statement: Error in the output rate limiting clause: Variable named 'somevar' has not been declared [select * from com.espertech.esper.support.bean.SupportBean output every somevar events]");
        }
    
        private void TryInvalid(String stmtText, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private SupportBean SendSupportBean(String stringValue, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean MakeSupportBean(String stringValue, int intPrimitive, int? intBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            return bean;
        }
    }
}
