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
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestInstanceOfExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestInstanceofSimple()
        {
            String stmtText = "select Instanceof(TheString, string) as t0, " +
                              " Instanceof(IntBoxed, int) as t1, " +
                              " Instanceof(FloatBoxed, float) as t2, " +
                              " Instanceof(TheString, float, char, byte) as t3, " +
                              " Instanceof(IntPrimitive, int) as t4, " +
                              " Instanceof(IntPrimitive, long) as t5, " +
                              " Instanceof(IntPrimitive, long, long, System.Object) as t6, " +
                              " Instanceof(FloatBoxed, long, float) as t7 " +
                              " from " + typeof(SupportBean).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t" + i));
            }
    
            SupportBean bean = new SupportBean("abc", 100);
            bean.FloatBoxed = 100F;
            _epService.EPRuntime.SendEvent(bean);
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {true, false, true, false, true, false, true, true});
    
            bean = new SupportBean(null, 100);
            bean.FloatBoxed = null;
            _epService.EPRuntime.SendEvent(bean);
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, false, false, false, true, false, true, false});
    
            float? f = null;
            Assert.IsFalse(f is float?);
        }
    
        [Test]
        public void TestInstanceofStringAndNull_OM()
        {
            String stmtText = "select instanceof(TheString,string) as t0, " +
                              "instanceof(TheString,float,string,int) as t1 " +
                              "from " + typeof(SupportBean).FullName;
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.InstanceOf("TheString", "string"), "t0")
                .Add(Expressions.InstanceOf(Expressions.Property("TheString"), "float", "string", "int"), "t1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("abc", 100));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.IsTrue(theEvent.Get("t0").AsBoolean());
            Assert.IsTrue(theEvent.Get("t1").AsBoolean());
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 100));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.IsFalse(theEvent.Get("t0").AsBoolean());
            Assert.IsFalse(theEvent.Get("t1").AsBoolean());
        }
    
        [Test]
        public void TestInstanceofStringAndNull_Compile()
        {
            String stmtText = "select instanceof(TheString,System.String) as t0, " +
                              "instanceof(TheString,float,string,int) as t1 " +
                              "from " + typeof(SupportBean).FullName;
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("abc", 100));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.IsTrue(theEvent.Get("t0").AsBoolean());
            Assert.IsTrue(theEvent.Get("t1").AsBoolean());
    
            _epService.EPRuntime.SendEvent(new SupportBean(null, 100));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.IsFalse(theEvent.Get("t0").AsBoolean());
            Assert.IsFalse(theEvent.Get("t1").AsBoolean());
        }
    
        [Test]
        public void TestDynamicPropertyTypes()
        {
            String stmtText = "select instanceof(item?, string) as t0, " +
                              " instanceof(item?, int) as t1, " +
                              " instanceof(item?, System.Single) as t2, " +
                              " instanceof(item?, System.Single, char, byte) as t3, " +
                              " instanceof(item?, System.Int32) as t4, " +
                              " instanceof(item?, long) as t5, " +
                              " instanceof(item?, long, System.ValueType) as t6, " +
                              " instanceof(item?, long, float) as t7 " +
                              " from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {true, false, false, false, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(100f));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, false, true, true, false, false, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, false, false, false, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(10));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, true, false, false, true, false, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(99l));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, false, false, false, false, true, true, true});
        }
    
        [Test]
        public void TestDynamicSuperTypeAndInterface()
        {
            String stmtText = "select Instanceof(item?, " + typeof(SupportMarkerInterface).FullName + ") as t0, " +
                              " Instanceof(item?, " + typeof(ISupportA).FullName + ") as t1, " +
                              " Instanceof(item?, " + typeof(ISupportBaseAB).FullName + ") as t2, " +
                              " Instanceof(item?, " + typeof(ISupportBaseABImpl).FullName + ") as t3, " +
                              " Instanceof(item?, " + typeof(ISupportA).FullName + ", " + typeof(ISupportB).FullName + ") as t4, " +
                              " Instanceof(item?, " + typeof(ISupportBaseAB).FullName + ", " + typeof(ISupportB).FullName + ") as t5, " +
                              " Instanceof(item?, " + typeof(ISupportAImplSuperG).FullName + ", " + typeof(ISupportB).FullName + ") as t6, " +
                              " Instanceof(item?, " + typeof(ISupportAImplSuperGImplPlus).FullName + ", " + typeof(SupportBeanBase).FullName + ") as t7 " +
    
                              " from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBeanDynRoot("abc")));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {true, false, false, false, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportAImplSuperGImplPlus()));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, true, true, false, true, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportAImplSuperGImpl("", "", "")));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, true, true, false, true, true, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportBaseABImpl("")));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, false, true, true, false, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportBImpl("", "")));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, false, true, false, true, true, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportAImpl("", "")));
            AssertResults(_listener.AssertOneGetNewAndReset(), new bool[] {false, true, true, false, true, true, false, false});
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
