///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestExistsExpr 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp()
        {
            listener = new SupportUpdateListener();
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            listener = null;
        }
    
        [Test]
        public void TestExistsSimple()
        {
            String stmtText = "select exists(TheString) as t0, " +
                              " exists(IntBoxed?) as t1, " +
                              " exists(dummy?) as t2, " +
                              " exists(IntPrimitive?) as t3, " +
                              " exists(IntPrimitive) as t4 " +
                              " from " + typeof(SupportBean).FullName;
    
            EPStatement selectTestCase = epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += listener.Update;
    
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t" + i));            
            }
    
            SupportBean bean = new SupportBean("abc", 100);
            bean.FloatBoxed = 9.5f;
            bean.IntBoxed = 3;
            epService.EPRuntime.SendEvent(bean);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[] {true, true, false, true, true});
        }
    
        [Test]
        public void TestExistsInner()
        {
            String stmtText = "select exists(item?.id) as t0, " +
                              " exists(item?.id?) as t1, " +
                              " exists(item?.item.IntBoxed) as t2, " +
                              " exists(item?.indexed[0]?) as t3, " +
                              " exists(item?.Mapped('keyOne')?) as t4, " +
                              " exists(item?.nested?) as t5, " +
                              " exists(item?.nested.nestedValue?) as t6, " +
                              " exists(item?.nested.nestedNested?) as t7, " +
                              " exists(item?.nested.nestedNested.nestedNestedValue?) as t8, " +
                              " exists(item?.nested.nestedNested.nestedNestedValue.dummy?) as t9, " +
                              " exists(item?.nested.nestedNested.dummy?) as t10 " +
                              " from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatement selectTestCase = epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += listener.Update;
    
            for (int i = 0; i < 11; i++)
            {
                Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t" + i));
            }
    
            // cannot exists if the inner is null
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[] {false, false, false, false, false, false, false, false, false, false, false});
    
            // try nested, indexed and mapped
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(SupportBeanComplexProps.MakeDefaultBean()));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[] {false, false, false, true, true, true, true, true, true, false, false});
    
            // try nested, indexed and mapped
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(SupportBeanComplexProps.MakeDefaultBean()));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[] {false, false, false, true, true, true, true, true, true, false, false});
    
            // try a boxed that returns null but does exists
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBeanDynRoot(new SupportBean())));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[] {false, false, true, false, false, false, false, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBean_A("10")));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[] {true, true, false, false, false, false, false, false, false, false, false});
        }
    
        [Test]
        public void TestCastDoubleAndNull_OM()
        {
            String stmtText = "select exists(item?.IntBoxed) as t0 " +
                              "from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.ExistsProperty("item?.IntBoxed"), "t0");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarkerInterface).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement selectTestCase = epService.EPAdministrator.Create(model);
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBean()));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
        }
    
        [Test]
        public void TestCastStringAndNull_Compile()
        {
            String stmtText = "select exists(item?.IntBoxed) as t0 " +
                              "from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement selectTestCase = epService.EPAdministrator.Create(model);
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBean()));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
        }
    
        private void AssertResults(EventBean theEvent, bool[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
}
