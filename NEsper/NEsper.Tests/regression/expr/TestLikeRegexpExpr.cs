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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestLikeRegexpExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestRegexpFilterWithDanglingMetaCharacter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString regexp \"*any*\"");
            stmt.Events += _testListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_testListener.IsInvoked);
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestLikeRegexStringAndNull()
        {
            String caseExpr = "select p00 like p01 as r1, " +
                                    " p00 like p01 escape \"!\" as r2," +
                                    " p02 regexp p03 as r3 " +
                              " from " + typeof(SupportBean_S0).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
    
            RunLikeRegexStringAndNull();
        }
    
        [Test]
        public void TestLikeRegexEscapedChar()
        {
            String caseExpr = "select p00 regexp '\\\\w*-ABC' as result from " + typeof(SupportBean_S0).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "TBT-ABC"));
            Assert.IsTrue(_testListener.AssertOneGetNewAndReset().Get("result").AsBoolean());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "TBT-BC"));
            Assert.IsFalse(_testListener.AssertOneGetNewAndReset().Get("result").AsBoolean());
        }
    
        [Test]
        public void TestLikeRegexStringAndNull_OM()
        {
            String stmtText = "select p00 like p01 as r1, " +
                                    "p00 like p01 escape \"!\" as r2, " +
                                    "p02 regexp p03 as r3 " +
                              "from " + typeof(SupportBean_S0).FullName;
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.Like(Expressions.Property("p00"), Expressions.Property("p01")), "r1")
                .Add(Expressions.Like(Expressions.Property("p00"), Expressions.Property("p01"), Expressions.Constant("!")), "r2")
                .Add(Expressions.Regexp(Expressions.Property("p02"), Expressions.Property("p03")), "r3");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean_S0).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);        
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
    
            RunLikeRegexStringAndNull();

            String epl = "select * from " + typeof(SupportBean).FullName + "(TheString not like \"foo%\")";
            EPPreparedStatement eps = _epService.EPAdministrator.PrepareEPL(epl);
            EPStatement statement = _epService.EPAdministrator.Create(eps);
            Assert.AreEqual(epl, statement.Text);

            epl = "select * from " + typeof(SupportBean).FullName + "(TheString not regexp \"foo\")";
            eps = _epService.EPAdministrator.PrepareEPL(epl);
            statement = _epService.EPAdministrator.Create(eps);
            Assert.AreEqual(epl, statement.Text);
        }
    
        [Test]
        public void TestLikeRegexStringAndNull_Compile()
        {
            String stmtText = "select p00 like p01 as r1, " +
                                    "p00 like p01 escape \"!\" as r2, " +
                                    "p02 regexp p03 as r3 " +
                              "from " + typeof(SupportBean_S0).FullName;
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
    
            RunLikeRegexStringAndNull();
        }
    
        private void RunLikeRegexStringAndNull()
        {
            SendS0Event("a", "b", "c", "d");
            AssertReceived(new Object[][] { new Object[] {"r1", false}, new Object[]{"r2", false}, new Object[]{"r3", false}});
    
            SendS0Event(null, "b", null, "d");
            AssertReceived(new Object[][] { new Object[] {"r1", null}, new Object[]{"r2", null}, new Object[]{"r3", null}});
    
            SendS0Event("a", null, "c", null);
            AssertReceived(new Object[][] { new Object[] {"r1", null}, new Object[]{"r2", null}, new Object[]{"r3", null}});
    
            SendS0Event(null, null, null, null);
            AssertReceived(new Object[][] { new Object[] {"r1", null}, new Object[] {"r2", null}, new Object[]{"r3", null}});
    
            SendS0Event("abcdef", "%de_", "a", "[a-c]");
            AssertReceived(new Object[][] { new Object[] {"r1", true}, new Object[]{"r2", true}, new Object[]{"r3", true}});
    
            SendS0Event("abcdef", "b%de_", "d", "[a-c]");
            AssertReceived(new Object[][] { new Object[] {"r1", false}, new Object[]{"r2", false}, new Object[]{"r3", false}});
    
            SendS0Event("!adex", "!%de_", "", ".");
            AssertReceived(new Object[][] { new Object[] {"r1", true}, new Object[]{"r2", false}, new Object[]{"r3", false}});
    
            SendS0Event("%dex", "!%de_", "a", ".");
            AssertReceived(new Object[][] { new Object[] {"r1", false}, new Object[]{"r2", true}, new Object[]{"r3", true}});
        }
    
        [Test]
        public void TestInvalidLikeRegEx()
        {
            TryInvalid("IntPrimitive like 'a' escape null");
            TryInvalid("IntPrimitive like BoolPrimitive");
            TryInvalid("BoolPrimitive like string");
            TryInvalid("TheString like string escape IntPrimitive");
    
            TryInvalid("IntPrimitive regexp doublePrimitve");
            TryInvalid("IntPrimitive regexp BoolPrimitive");
            TryInvalid("BoolPrimitive regexp string");
            TryInvalid("TheString regexp IntPrimitive");
        }
    
        [Test]
        public void TestLikeRegexNumericAndNull()
        {
            String caseExpr = "select IntBoxed like '%01%' as r1, " +
                              " DoubleBoxed regexp '[0-9][0-9].[0-9][0-9]' as r2 " +
                              " from " + typeof (SupportBean).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
    
            SendSupportBeanEvent(101, 1.1);
            AssertReceived(new Object[][] { new Object[] {"r1", true}, new Object[]{"r2", false}});
    
            SendSupportBeanEvent(102, 11d);
            AssertReceived(new Object[][] { new Object[] {"r1", false}, new Object[]{"r2", true}});
    
            SendSupportBeanEvent(null, null);
            AssertReceived(new Object[][] { new Object[] {"r1", null}, new Object[]{"r2", null}});
        }
    
        private void TryInvalid(String expr)
        {
            try
            {
                String statement = "select " + expr + " from " + typeof(SupportBean).FullName;
                _epService.EPAdministrator.CreateEPL(statement);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
        }
    
        private void AssertReceived(Object[][] objects)
        {
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            for (int i = 0; i < objects.Length; i++)
            {
                String key = (String) objects[i][0];
                Object result = objects[i][1];
                Assert.AreEqual(result, theEvent.Get(key), "key=" + key + " result=" + result);
            }
        }
    
        private void SendS0Event(String p00, String p01, String p02, String p03)
        {
            SupportBean_S0 bean = new SupportBean_S0(-1, p00, p01, p02, p03);
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(int? intBoxed, double? doubleBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
