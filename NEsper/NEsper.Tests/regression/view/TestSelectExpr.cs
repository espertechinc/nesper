///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    using DescriptionAttribute = esper.client.annotation.DescriptionAttribute;

    [TestFixture]
    public class TestSelectExpr
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            _testListener = null;
        }
    
        [Test]
        public void TestPrecedenceNoColumnName()
        {
            TryPrecedenceNoColumnName("3*2+1", "3*2+1", 7);
            TryPrecedenceNoColumnName("(3*2)+1", "3*2+1", 7);
            TryPrecedenceNoColumnName("3*(2+1)", "3*(2+1)", 9);
        }

        private void TryPrecedenceNoColumnName(String selectColumn, String expectedColumn, Object value)
        {
            String epl = "select " + selectColumn + " from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _testListener.Update;
            if (!stmt.EventType.PropertyNames[0].Equals(expectedColumn)) {
                Assert.Fail("Expected '" + expectedColumn + "' but was " + stmt.EventType.PropertyNames[0]);
            }

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean @event = _testListener.AssertOneGetNewAndReset();
            Assert.That(value, Is.EqualTo(@event.Get(expectedColumn)));
            stmt.Dispose();
        }

        [Test]
        public void TestGraphSelect()
        {
            _epService.EPAdministrator.CreateEPL("insert into MyStream select nested from " + typeof(SupportBeanComplexProps).FullName);
    
            String viewExpr = "select nested.nestedValue, nested.nestedNested.nestedNestedValue from MyStream";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            Assert.NotNull(_testListener.AssertOneGetNewAndReset());
        }
    
        [Test]
        public void TestKeywordsAllowed()
        {
            String fields = "count,escape,every,sum,avg,max,min,coalesce,median,stddev,avedev,events,first,last,unidirectional,pattern,sql,metadatasql,prev,prior,weekday,lastweekday,cast,snapshot,variable,window,left,right,full,outer,join";
            _epService.EPAdministrator.Configuration.AddEventType("Keywords", typeof(SupportBeanKeywords));
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select " + fields + " from Keywords");
            stmt.Events += _testListener.Update;
            _epService.EPRuntime.SendEvent(new SupportBeanKeywords());
            EPAssertionUtil.AssertEqualsExactOrder(stmt.EventType.PropertyNames, fields.Split(','));
    
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            String[] fieldsArr = fields.Split(',');
            for (int i = 0; i < fieldsArr.Length; i++) {
                Assert.AreEqual(1, theEvent.Get(fieldsArr[i]));
            }
            stmt.Dispose();
    
            stmt = _epService.EPAdministrator.CreateEPL("select escape as stddev, count(*) as count, last from Keywords");
            stmt.Events += _testListener.Update;
            _epService.EPRuntime.SendEvent(new SupportBeanKeywords());
    
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, theEvent.Get("stddev"));
            Assert.AreEqual(1L, theEvent.Get("count"));
            Assert.AreEqual(1, theEvent.Get("last"));
        }
    
        [Test]
        public void TestEscapeString()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
    
            // The following EPL syntax compiles but fails to match a string "A'B", we are looking into:
            // EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString='A\\\'B')");
    
            TryEscapeMatch("A'B", "\"A'B\"");       // opposite quotes
            TryEscapeMatch("A'B", "'A\\'B'");      // escape '
            TryEscapeMatch("A'B", "'A\\u0027B'");   // unicode
    
            TryEscapeMatch("A\"B", "'A\"B'");       // opposite quotes
            TryEscapeMatch("A\"B", "'A\\\"B'");      // escape "
            TryEscapeMatch("A\"B", "'A\\u0022B'");   // unicode
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("@Name('A\\\'B') @Description(\"A\\\"B\") select * from SupportBean");
            Assert.AreEqual("A\'B", stmt.Name);
            DescriptionAttribute desc = (DescriptionAttribute) stmt.Annotations.Skip(1).First();
            Assert.AreEqual("A\"B", desc.Value);
            stmt.Dispose();
    
            stmt = _epService.EPAdministrator.CreateEPL("select 'Volume' as field1, \"sleep\" as field2, \"\\u0041\" as unicodeA from SupportBean");
            stmt.Events += _testListener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(), new String[]{"field1", "field2", "unicodeA"}, new Object[]{"Volume", "sleep", "A"});
            stmt.Dispose();
    
            TryStatementMatch("John's", "select * from SupportBean(TheString='John\\'s')");
            TryStatementMatch("John's", "select * from SupportBean(TheString='John\\u0027s')");
            TryStatementMatch("Quote \"Hello\"", "select * from SupportBean(TheString like \"Quote \\\"Hello\\\"\")");
            TryStatementMatch("Quote \"Hello\"", "select * from SupportBean(TheString like \"Quote \\u0022Hello\\u0022\")");
        }
    
        private void TryEscapeMatch(String property, String escaped)
        {
            String epl = "select * from SupportBean(TheString=" + escaped + ")";
            String text = "trying >" + escaped + "< (" + escaped.Length + " chars) EPL " + epl;
            Log.Info("tryEscapeMatch for " + text);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _testListener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean(property, 1));
            Assert.AreEqual(_testListener.AssertOneGetNewAndReset().Get("IntPrimitive"), 1);
            stmt.Dispose();
        }
    
        private void TryStatementMatch(String property, String epl)
        {
            String text = "trying EPL " + epl;
            Log.Info("tryEscapeMatch for " + text);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _testListener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean(property, 1));
            Assert.AreEqual(_testListener.AssertOneGetNewAndReset().Get("IntPrimitive"), 1);
            stmt.Dispose();
        }
    
        [Test]
        public void TestGetEventType()
        {
            String viewExpr = "select TheString, BoolBoxed aBool, 3*IntPrimitive, FloatBoxed+FloatPrimitive result" +
                    " from " + typeof(SupportBean).FullName + ".win:length(3) " +
                    " where BoolBoxed = true";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            EventType type = selectTestView.EventType;
            Log.Debug(".testGetEventType properties=" + CompatExtensions.Render(type.PropertyNames));
            EPAssertionUtil.AssertEqualsAnyOrder(type.PropertyNames, new String[]{"3*IntPrimitive", "TheString", "result", "aBool"});
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(bool?), type.GetPropertyType("aBool"));
            Assert.AreEqual(typeof(float?), type.GetPropertyType("result"));
            Assert.AreEqual(typeof(int?), type.GetPropertyType("3*IntPrimitive"));
        }
    
        [Test]
        public void TestWindowStats()
        {
            String viewExpr = "select TheString, BoolBoxed as aBool, 3*IntPrimitive, FloatBoxed+FloatPrimitive as result" +
                    " from " + typeof(SupportBean).FullName + ".win:length(3) " +
                    " where BoolBoxed = true";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            _testListener.Reset();
    
            SendEvent("a", false, 0, 0, 0);
            SendEvent("b", false, 0, 0, 0);
            Assert.IsTrue(_testListener.LastNewData == null);
            SendEvent("c", true, 3, 10, 20);
    
            EventBean received = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("c", received.Get("TheString"));
            Assert.AreEqual(true, received.Get("aBool"));
            Assert.AreEqual(30f, received.Get("result"));
        }
    
        private void SendEvent(String s, bool b, int i, float f1, float f2)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = s;
            bean.BoolBoxed = b;
            bean.IntPrimitive = i;
            bean.FloatPrimitive = f1;
            bean.FloatBoxed = f2;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
