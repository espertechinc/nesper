///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventPropertyDynamicBean 
    {
        private SupportUpdateListener _listener;
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestGetValueDynamic()
        {
            RunAssertionGetDynamicWObjectArr(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionGetDynamicWObjectArr(EventRepresentationEnum.MAP);
            RunAssertionGetDynamicWObjectArr(EventRepresentationEnum.DEFAULT);
        }

        [Test]
        public void TestGetValueNested()
        {
            String stmtText = "select item.Nested?.NestedValue as n1, " +
                              " item.Nested?.NestedValue? as n2, " +
                              " item.Nested?.NestedNested.NestedNestedValue as n3, " +
                              " item.Nested?.NestedNested?.NestedNestedValue as n4, " +
                              " item.Nested?.NestedNested.NestedNestedValue? as n5, " +
                              " item.Nested?.NestedNested?.NestedNestedValue? as n6 " +
                              " from " + typeof(SupportBeanDynRoot).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("n1"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("n2"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("n3"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("n4"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("n5"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("n6"));
    
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
    
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(bean.Nested.NestedValue, theEvent.Get("n1"));
            Assert.AreEqual(bean.Nested.NestedValue, theEvent.Get("n2"));
            Assert.AreEqual(bean.Nested.NestedNested.NestedNestedValue, theEvent.Get("n3"));
            Assert.AreEqual(bean.Nested.NestedNested.NestedNestedValue, theEvent.Get("n4"));
            Assert.AreEqual(bean.Nested.NestedNested.NestedNestedValue, theEvent.Get("n5"));
            Assert.AreEqual(bean.Nested.NestedNested.NestedNestedValue, theEvent.Get("n6"));
    
            bean = SupportBeanComplexProps.MakeDefaultBean();
            bean.Nested.NestedValue = "Nested1";
            bean.Nested.NestedNested.SetNestedNestedValue("Nested2");
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
    
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("Nested1", theEvent.Get("n1"));
            Assert.AreEqual("Nested1", theEvent.Get("n2"));
            Assert.AreEqual("Nested2", theEvent.Get("n3"));
            Assert.AreEqual("Nested2", theEvent.Get("n4"));
            Assert.AreEqual("Nested2", theEvent.Get("n5"));
            Assert.AreEqual("Nested2", theEvent.Get("n6"));
        }
    
        [Test]
        public void TestGetValueTop()
        {
            String stmtText = "select id? as myid from " + typeof(SupportMarkerInterface).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportMarkerImplA("e1"));
            Assert.AreEqual("e1", _listener.AssertOneGetNewAndReset().Get("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportMarkerImplB(1));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportMarkerImplC());
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("myid"));
        }
    
        [Test]
        public void TestGetValueTopNested()
        {
            String stmtText = "select SimpleProperty? as Simple, "+
                              " Nested?.NestedValue as Nested, " +
                              " Nested?.NestedNested.NestedNestedValue as NestedNested " +
                              "from " + typeof(SupportMarkerInterface).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Simple"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Nested"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("NestedNested"));
    
            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("Simple", theEvent.Get("Simple"));
            Assert.AreEqual("NestedValue", theEvent.Get("Nested"));
            Assert.AreEqual("NestedNestedValue", theEvent.Get("NestedNested"));
        }
    
        [Test]
        public void TestGetValueTopComplex()
        {
            String stmtText = "select item?.Indexed[0] as Indexed1, " +
                              "item?.Indexed[1]? as Indexed2, " +
                              "item?.ArrayProperty[1]? as Array, " +
                              "item?.Mapped('keyOne') as Mapped1, " +
                              "item?.Mapped('keyTwo')? as Mapped2,  " +
                              "item?.MapProperty('xOne')? as map " +
                              "from " + typeof(SupportBeanDynRoot).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Indexed1"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Indexed2"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Mapped1"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Mapped2"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Array"));
    
            SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(inner));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(inner.GetIndexed(0), theEvent.Get("Indexed1"));
            Assert.AreEqual(inner.GetIndexed(1), theEvent.Get("Indexed2"));
            Assert.AreEqual(inner.GetMapped("keyOne"), theEvent.Get("Mapped1"));
            Assert.AreEqual(inner.GetMapped("keyTwo"), theEvent.Get("Mapped2"));
            Assert.AreEqual(inner.MapProperty.Get("xOne"), theEvent.Get("map"));
            Assert.AreEqual(inner.ArrayProperty[1], theEvent.Get("Array"));
        }
    
        [Test]
        public void TestGetValueRootComplex()
        {
            String stmtText = "select Indexed[0]? as Indexed1, " +
                              "Indexed[1]? as Indexed2, " +
                              "Mapped('keyOne')? as Mapped1, " +
                              "Mapped('keyTwo')? as Mapped2  " +
                              "from " + typeof(SupportBeanComplexProps).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Indexed1"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Indexed2"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Mapped1"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Mapped2"));
    
            SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(inner);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(inner.GetIndexed(0), theEvent.Get("Indexed1"));
            Assert.AreEqual(inner.GetIndexed(1), theEvent.Get("Indexed2"));
            Assert.AreEqual(inner.GetMapped("keyOne"), theEvent.Get("Mapped1"));
            Assert.AreEqual(inner.GetMapped("keyTwo"), theEvent.Get("Mapped2"));
        }
    
        [Test]
        public void TestPerformance()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // exclude test

            String stmtText = "select SimpleProperty?, " +
                              "Indexed[1]? as Indexed, " +
                              "Mapped('keyOne')? as Mapped " +
                              "from " + typeof(SupportBeanComplexProps).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(Object), type.GetPropertyType("SimpleProperty?"));
            Assert.AreEqual(typeof(Object), type.GetPropertyType("Indexed"));
            Assert.AreEqual(typeof(Object), type.GetPropertyType("Mapped"));
    
            SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(inner);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(inner.SimpleProperty, theEvent.Get("SimpleProperty?"));
            Assert.AreEqual(inner.GetIndexed(1), theEvent.Get("Indexed"));
            Assert.AreEqual(inner.GetMapped("keyOne"), theEvent.Get("Mapped"));
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(inner);
                if (i % 1000 == 0)
                {
                    _listener.Reset();
                }
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 1000, "delta=" + delta);
        }

        private void RunAssertionGetDynamicWObjectArr(EventRepresentationEnum eventRepresentationEnum)
        {
            String stmtText = eventRepresentationEnum.GetAnnotationText()
                              + " select item.id? as myid from "
                              + typeof (SupportBeanDynRoot).FullName;
            using (var stmt = _epService.EPAdministrator.CreateEPL(stmtText))
            {

                stmt.Events += _listener.Update;

                // check type
                Assert.AreEqual(typeof (object), stmt.EventType.GetPropertyType("myid"));

                // check value with an object that has the property as an int
                var runtime = _epService.EPRuntime;
                runtime.SendEvent(
                    new SupportBeanDynRoot(new SupportBean_S0(101)));
                Assert.AreEqual(101, _listener.AssertOneGetNewAndReset().Get("myid"));

                // check value with an object that doesn't have the property
                runtime.SendEvent(new SupportBeanDynRoot("abc"));
                Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("myid"));

                // check value with an object that has the property as a string
                runtime.SendEvent(
                    new SupportBeanDynRoot(new SupportBean_A("e1")));
                Assert.AreEqual("e1", _listener.AssertOneGetNewAndReset().Get("myid"));

                runtime.SendEvent(
                    new SupportBeanDynRoot(new SupportBean_B("e2")));
                Assert.AreEqual("e2", _listener.AssertOneGetNewAndReset().Get("myid"));

                runtime.SendEvent(
                    new SupportBeanDynRoot(new SupportBean_S1(102)));
                Assert.AreEqual(102, _listener.AssertOneGetNewAndReset().Get("myid"));

                if (eventRepresentationEnum.IsObjectArrayEvent())
                {
                    Assert.AreEqual(typeof (object[]), stmt.EventType.UnderlyingType);
                }
                else
                {
                    Assert.AreEqual(typeof (IDictionary<string, object>), stmt.EventType.UnderlyingType);
                }
            }
        }
    }
}
