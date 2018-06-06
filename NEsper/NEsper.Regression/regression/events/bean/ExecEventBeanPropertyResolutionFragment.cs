///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    using Map = IDictionary<string, object>;

    public class ExecEventBeanPropertyResolutionFragment : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionMapSimpleTypes(epService);
            RunAssertionObjectArraySimpleTypes(epService);
            RunAssertionWrapperFragmentWithMap(epService);
            RunAssertionWrapperFragmentWithObjectArray(epService);
            RunAssertionNativeBeanFragment(epService);
            RunAssertionMapFragmentMapNested(epService);
            RunAssertionObjectArrayFragmentObjectArrayNested(epService);
            RunAssertionMapFragmentMapUnnamed(epService);
            RunAssertionMapFragmentTransposedMapEventBean(epService);
            RunAssertionObjectArrayFragmentTransposedMapEventBean(epService);
            RunAssertionMapFragmentMapBeans(epService);
            RunAssertionObjectArrayFragmentBeans(epService);
            RunAssertionMapFragmentMap3Level(epService);
            RunAssertionObjectArrayFragment3Level(epService);
            RunAssertionFragmentMapMulti(epService);
        }
    
        private void RunAssertionMapSimpleTypes(EPServiceProvider epService) {
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0int", typeof(int));
            mapOuter.Put("p0intarray", typeof(int[]));
            mapOuter.Put("p0map", typeof(Map));
            epService.EPAdministrator.Configuration.AddEventType("MSTypeOne", mapOuter);
            var listener = new SupportUpdateListener();
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from MSTypeOne");
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1someval", "A");
    
            var dataRoot = new Dictionary<string, Object>();
            dataRoot.Put("p0simple", 99);
            dataRoot.Put("p0array", new int[]{101, 102});
            dataRoot.Put("p0map", dataInner);
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "MSTypeOne");
            var eventBean = listener.AssertOneGetNewAndReset();
            //Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    //comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // resolve property via fragment
            Assert.IsNull(eventType.GetFragmentType("p0int"));
            Assert.IsNull(eventType.GetFragmentType("p0intarray"));
            Assert.IsNull(eventBean.GetFragment("p0map?"));
            Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
            Assert.IsNull(eventBean.GetFragment("p0map('a')?"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionObjectArraySimpleTypes(EPServiceProvider epService) {
            string[] props = {"p0int", "p0intarray", "p0map"};
            object[] types = {typeof(int), typeof(int[]), typeof(Map)};
            epService.EPAdministrator.Configuration.AddEventType("OASimple", props, types);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from OASimple");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1someval", "A");
            var dataRoot = new object[]{99, new int[]{101, 102}, dataInner};
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "OASimple");
            var eventBean = listener.AssertOneGetNewAndReset();
            //Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    //comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // resolve property via fragment
            Assert.IsNull(eventType.GetFragmentType("p0int"));
            Assert.IsNull(eventType.GetFragmentType("p0intarray"));
            Assert.IsNull(eventBean.GetFragment("p0map?"));
            Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
            Assert.IsNull(eventBean.GetFragment("p0map('a')?"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWrapperFragmentWithMap(EPServiceProvider epService) {
            var typeLev0 = new Dictionary<string, Object>();
            typeLev0.Put("p1id", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("FrostyLev0", typeLev0);
    
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0simple", "FrostyLev0");
            mapOuter.Put("p0bean", typeof(SupportBeanComplexProps));
            epService.EPAdministrator.Configuration.AddEventType("Frosty", mapOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select *, p0simple.p1id + 1 as plusone, p0bean as mybean from Frosty");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1id", 10);
    
            var dataRoot = new Dictionary<string, Object>();
            dataRoot.Put("p0simple", dataInner);
            dataRoot.Put("p0bean", SupportBeanComplexProps.MakeDefaultBean());
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "Frosty");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.AreEqual(11, eventBean.Get("plusone"));
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
    
            var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));
    
            var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");
            Assert.AreEqual("NestedNestedValue", innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWrapperFragmentWithObjectArray(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("WheatLev0", new string[]{"p1id"}, new object[]{typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("WheatRoot", new string[]{"p0simple", "p0bean"}, new object[]{"WheatLev0", typeof(SupportBeanComplexProps)});
    
            var stmt = epService.EPAdministrator.CreateEPL("select *, p0simple.p1id + 1 as plusone, p0bean as mybean from WheatRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{new object[]{10}, SupportBeanComplexProps.MakeDefaultBean()}, "WheatRoot");
    
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.AreEqual(11, eventBean.Get("plusone"));
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
    
            var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));
    
            var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");
            Assert.AreEqual("NestedNestedValue", innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));
    
            stmt.Dispose();
        }
    
        public void RunAssertionNativeBeanFragment(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBeanComplexProps).FullName);
            stmt.Events += listener.Update;
            stmt = epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBeanCombinedProps).FullName);
            stmt.Events += listener.Update;
    
            // assert Nested fragments
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            var eventBean = listener.AssertOneGetNewAndReset();
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
            //Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));
    
            Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Nested").IsFragment);
            var eventNested = (EventBean) eventBean.GetFragment("Nested");
            Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));
            eventNested = (EventBean) eventBean.GetFragment("Nested?");
            Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));
    
            Assert.IsTrue(eventNested.EventType.GetPropertyDescriptor("NestedNested").IsFragment);
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventNested.GetFragment("NestedNested")).Get("NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventNested.GetFragment("NestedNested?")).Get("NestedNestedValue"));
    
            var NestedFragment = (EventBean) eventBean.GetFragment("Nested.NestedNested");
            Assert.AreEqual("NestedNestedValue", NestedFragment.Get("NestedNestedValue"));
    
            // assert indexed fragments
            var eventObject = SupportBeanCombinedProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(eventObject);
            eventBean = listener.AssertOneGetNewAndReset();
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
            //Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));
    
            Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsFragment);
            Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsIndexed);
            var eventArray = (EventBean[]) eventBean.GetFragment("Array");
            Assert.AreEqual(3, eventArray.Length);
    
            var eventElement = eventArray[0];
            Assert.AreSame(eventObject.Array[0].GetMapped("0ma"), eventElement.Get("Mapped('0ma')"));
            Assert.AreSame(eventObject.Array[0].GetMapped("0ma"), ((EventBean) eventBean.GetFragment("Array[0]")).Get("Mapped('0ma')"));
            Assert.AreSame(eventObject.Array[0].GetMapped("0ma"), ((EventBean) eventBean.GetFragment("Array[0]?")).Get("Mapped('0ma')"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMapFragmentMapNested(EPServiceProvider epService) {
            var typeLev0 = new Dictionary<string, Object>();
            typeLev0.Put("p1id", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("HomerunLev0", typeLev0);
    
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0simple", "HomerunLev0");
            mapOuter.Put("p0array", "HomerunLev0[]");
            epService.EPAdministrator.Configuration.AddEventType("HomerunRoot", mapOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from HomerunRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1id", 10);
    
            var dataRoot = new Dictionary<string, Object>();
            dataRoot.Put("p0simple", dataInner);
            dataRoot.Put("p0array", new Map[]{dataInner, dataInner});
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "HomerunRoot");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);
    
            var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));
    
            var innerArrayAllEvent = (EventBean[]) eventBean.GetFragment("p0array");
            Assert.AreEqual(10, innerArrayAllEvent[0].Get("p1id"));
    
            var innerArrayElementEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(10, innerArrayElementEvent.Get("p1id"));
    
            // resolve property via getter
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
            Assert.AreEqual(10, eventBean.Get("p0array[1].p1id"));
    
            Assert.IsNull(eventType.GetFragmentType("p0array.p1id"));
            Assert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionObjectArrayFragmentObjectArrayNested(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("GoalLev0", new string[]{"p1id"}, new object[]{typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("GoalRoot", new string[]{"p0simple", "p0array"}, new object[]{"GoalLev0", "GoalLev0[]"});
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from GoalRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);
    
            epService.EPRuntime.SendEvent(new object[]{new object[]{10}, new object[]{new object[]{20}, new object[]{21}}}, "GoalRoot");
    
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);
    
            var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));
    
            var innerArrayAllEvent = (EventBean[]) eventBean.GetFragment("p0array");
            Assert.AreEqual(20, innerArrayAllEvent[0].Get("p1id"));
    
            var innerArrayElementEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(20, innerArrayElementEvent.Get("p1id"));
    
            // resolve property via getter
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
            Assert.AreEqual(21, eventBean.Get("p0array[1].p1id"));
    
            Assert.IsNull(eventType.GetFragmentType("p0array.p1id"));
            Assert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMapFragmentMapUnnamed(EPServiceProvider epService) {
            var typeLev0 = new Dictionary<string, Object>();
            typeLev0.Put("p1id", typeof(int));
    
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0simple", typeLev0);
            epService.EPAdministrator.Configuration.AddEventType("FlywheelRoot", mapOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from FlywheelRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1id", 10);
    
            var dataRoot = new Dictionary<string, Object>();
            dataRoot.Put("p0simple", dataInner);
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "FlywheelRoot");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.IsFalse(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.IsNull(eventBean.GetFragment("p0simple"));
    
            // resolve property via getter
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMapFragmentTransposedMapEventBean(EPServiceProvider epService) {
            var typeInner = new Dictionary<string, Object>();
            typeInner.Put("p2id", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("GistInner", typeInner);
    
            var typeMap = new Dictionary<string, Object>();
            typeMap.Put("id", typeof(int));
            typeMap.Put("bean", typeof(SupportBean));
            typeMap.Put("beanarray", typeof(SupportBean[]));
            typeMap.Put("complex", typeof(SupportBeanComplexProps));
            typeMap.Put("complexarray", typeof(SupportBeanComplexProps[]));
            typeMap.Put("map", "GistInner");
            typeMap.Put("maparray", "GistInner[]");
    
            epService.EPAdministrator.Configuration.AddEventType("GistMapOne", typeMap);
            epService.EPAdministrator.Configuration.AddEventType("GistMapTwo", typeMap);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from pattern[one=GistMapOne until two=GistMapTwo]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p2id", 2000);
            var dataMap = new Dictionary<string, Object>();
            dataMap.Put("id", 1);
            dataMap.Put("bean", new SupportBean("E1", 100));
            dataMap.Put("beanarray", new SupportBean[]{new SupportBean("E1", 100), new SupportBean("E2", 200)});
            dataMap.Put("complex", SupportBeanComplexProps.MakeDefaultBean());
            dataMap.Put("complexarray", new []{ SupportBeanComplexProps.MakeDefaultBean() });
            dataMap.Put("map", dataInner);
            dataMap.Put("maparray", new Map[]{dataInner, dataInner});
    
            // send event
            epService.EPRuntime.SendEvent(dataMap, "GistMapOne");
    
            var dataMapTwo = new Dictionary<string, Object>(dataMap);
            dataMapTwo.Put("id", 2);
            epService.EPRuntime.SendEvent(dataMapTwo, "GistMapOne");
    
            var dataMapThree = new Dictionary<string, Object>(dataMap);
            dataMapThree.Put("id", 3);
            epService.EPRuntime.SendEvent(dataMapThree, "GistMapTwo");
    
            var eventBean = listener.AssertOneGetNewAndReset();
            // Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("id"));
            Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("id"));
            Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("id"));
    
            Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[0].bean")).Get("TheString"));
            Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[1].bean")).Get("TheString"));
            Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("two.bean")).Get("TheString"));
    
            Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("one[0].beanarray[1]")).Get("TheString"));
            Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("two.beanarray[1]")).Get("TheString"));
    
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get("NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("two.complex.Nested.NestedNested")).Get("NestedNestedValue"));
    
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));
    
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].map")).Get("p2id"));
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.map")).Get("p2id"));
    
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].maparray[1]")).Get("p2id"));
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.maparray[1]")).Get("p2id"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionObjectArrayFragmentTransposedMapEventBean(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("CashInner", new string[]{"p2id"}, new object[]{typeof(int)});
    
            string[] props = {"id", "bean", "beanarray", "complex", "complexarray", "map", "maparray"};
            object[] types = {typeof(int), typeof(SupportBean), typeof(SupportBean[]), typeof(SupportBeanComplexProps), typeof(SupportBeanComplexProps[]), "CashInner", "CashInner[]"};
            epService.EPAdministrator.Configuration.AddEventType("CashMapOne", props, types);
            epService.EPAdministrator.Configuration.AddEventType("CashMapTwo", props, types);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from pattern[one=CashMapOne until two=CashMapTwo]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new object[]{2000};
            var dataArray = new object[]{1, new SupportBean("E1", 100),
                    new SupportBean[]{new SupportBean("E1", 100), new SupportBean("E2", 200)},
                    SupportBeanComplexProps.MakeDefaultBean(),
                    new SupportBeanComplexProps[]{SupportBeanComplexProps.MakeDefaultBean()},
                    dataInner, new object[]{dataInner, dataInner}};
    
            // send event
            epService.EPRuntime.SendEvent(dataArray, "CashMapOne");
    
            var dataArrayTwo = new Object[dataArray.Length];
            Array.Copy(dataArray, 0, dataArrayTwo, 0, dataArray.Length);
            dataArrayTwo[0] = 2;
            epService.EPRuntime.SendEvent(dataArrayTwo, "CashMapOne");
    
            var dataArrayThree = new Object[dataArray.Length];
            Array.Copy(dataArray, 0, dataArrayThree, 0, dataArray.Length);
            dataArrayThree[0] = 3;
            epService.EPRuntime.SendEvent(dataArrayThree, "CashMapTwo");
    
            var eventBean = listener.AssertOneGetNewAndReset();
            // Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("id"));
            Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("id"));
            Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("id"));
    
            Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[0].bean")).Get("TheString"));
            Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[1].bean")).Get("TheString"));
            Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("two.bean")).Get("TheString"));
    
            Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("one[0].beanarray[1]")).Get("TheString"));
            Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("two.beanarray[1]")).Get("TheString"));
    
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("one[0].complex.nested.nestedNested")).Get("NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("two.complex.nested.nestedNested")).Get("NestedNestedValue"));
    
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("one[0].complexarray[0].nested.nestedNested")).Get("NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("two.complexarray[0].nested.nestedNested")).Get("NestedNestedValue"));
    
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].map")).Get("p2id"));
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.map")).Get("p2id"));
    
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].maparray[1]")).Get("p2id"));
            Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.maparray[1]")).Get("p2id"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMapFragmentMapBeans(EPServiceProvider epService) {
            var typeLev0 = new Dictionary<string, Object>();
            typeLev0.Put("p1simple", typeof(SupportBean));
            typeLev0.Put("p1array", typeof(SupportBean[]));
            typeLev0.Put("p1complex", typeof(SupportBeanComplexProps));
            typeLev0.Put("p1complexarray", typeof(SupportBeanComplexProps[]));
            epService.EPAdministrator.Configuration.AddEventType("TXTypeLev0", typeLev0);
    
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0simple", "TXTypeLev0");
            mapOuter.Put("p0array", "TXTypeLev0[]");
            epService.EPAdministrator.Configuration.AddEventType("TXTypeRoot", mapOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from TXTypeRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1simple", new SupportBean("E1", 11));
            dataInner.Put("p1array", new SupportBean[]{new SupportBean("A1", 21), new SupportBean("A2", 22)});
            dataInner.Put("p1complex", SupportBeanComplexProps.MakeDefaultBean());
            dataInner.Put("p1complexarray", new SupportBeanComplexProps[]{SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean()});
    
            var dataRoot = new Dictionary<string, Object>();
            dataRoot.Put("p0simple", dataInner);
            dataRoot.Put("p0array", new Map[]{dataInner, dataInner});
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "TXTypeRoot");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
            Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
            Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("SimpleProperty"));
            Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("SimpleProperty"));
            Assert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));
    
            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
            Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get("NestedNestedValue"));
    
            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
            Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
    
            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
            Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionObjectArrayFragmentBeans(EPServiceProvider epService) {
            string[] propsLev0 = {"p1simple", "p1array", "p1complex", "p1complexarray"};
            object[] typesLev0 = {typeof(SupportBean), typeof(SupportBean[]), typeof(SupportBeanComplexProps), typeof(SupportBeanComplexProps[])};
            epService.EPAdministrator.Configuration.AddEventType("LocalTypeLev0", propsLev0, typesLev0);
    
            string[] propsOuter = {"p0simple", "p0array"};
            object[] typesOuter = {"LocalTypeLev0", "LocalTypeLev0[]"};
            epService.EPAdministrator.Configuration.AddEventType("LocalTypeRoot", propsOuter, typesOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from LocalTypeRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);
    
            object[] dataInner = {new SupportBean("E1", 11), new SupportBean[]{new SupportBean("A1", 21), new SupportBean("A2", 22)},
                    SupportBeanComplexProps.MakeDefaultBean(), new SupportBeanComplexProps[]{SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean()}};
            var dataRoot = new object[]{dataInner, new object[]{dataInner, dataInner}};
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "LocalTypeRoot");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
            Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
            Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("simpleProperty"));
            Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("simpleProperty"));
            Assert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));
    
            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
            Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get("NestedNestedValue"));
    
            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
            Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
    
            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
            Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMapFragmentMap3Level(EPServiceProvider epService) {
            var typeLev1 = new Dictionary<string, Object>();
            typeLev1.Put("p2id", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("JimTypeLev1", typeLev1);
    
            var typeLev0 = new Dictionary<string, Object>();
            typeLev0.Put("p1simple", "JimTypeLev1");
            typeLev0.Put("p1array", "JimTypeLev1[]");
            epService.EPAdministrator.Configuration.AddEventType("JimTypeLev0", typeLev0);
    
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0simple", "JimTypeLev0");
            mapOuter.Put("p0array", "JimTypeLev0[]");
            epService.EPAdministrator.Configuration.AddEventType("JimTypeRoot", mapOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from JimTypeRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataLev1 = new Dictionary<string, Object>();
            dataLev1.Put("p2id", 10);
    
            var dataLev0 = new Dictionary<string, Object>();
            dataLev0.Put("p1simple", dataLev1);
            dataLev0.Put("p1array", new Map[]{dataLev1, dataLev1});
    
            var dataRoot = new Dictionary<string, Object>();
            dataRoot.Put("p0simple", dataLev0);
            dataRoot.Put("p0array", new Map[]{dataLev0, dataLev0});
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "JimTypeRoot");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));
    
            // resolve property via fragment
            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));
    
            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));
    
            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));
    
            Assert.AreEqual("JimTypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array[0].p1array[0]").FragmentType.GetPropertyDescriptor("p2id").PropertyType);
            Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
            Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);
    
            TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionObjectArrayFragment3Level(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("JackTypeLev1", new string[]{"p2id"}, new object[]{typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("JackTypeLev0", new string[]{"p1simple", "p1array"}, new object[]{"JackTypeLev1", "JackTypeLev1[]"});
            epService.EPAdministrator.Configuration.AddEventType("JackTypeRoot", new string[]{"p0simple", "p0array"}, new object[]{"JackTypeLev0", "JackTypeLev0[]"});
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from JackTypeRoot");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);
    
            var dataLev1 = new object[]{10};
            var dataLev0 = new object[]{dataLev1, new object[]{dataLev1, dataLev1}};
            var dataRoot = new object[]{dataLev0, new object[]{dataLev0, dataLev0}};
    
            // send event
            epService.EPRuntime.SendEvent(dataRoot, "JackTypeRoot");
            var eventBean = listener.AssertOneGetNewAndReset();
            //  Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));    comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));
    
            // resolve property via fragment
            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));
    
            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));
    
            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));
    
            Assert.AreEqual("JackTypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array[0].p1array[0]").FragmentType.GetPropertyDescriptor("p2id").PropertyType);
            Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
            Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);
    
            TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFragmentMapMulti(EPServiceProvider epService) {
            var mapInnerInner = new Dictionary<string, Object>();
            mapInnerInner.Put("p2id", typeof(int));
    
            var mapInner = new Dictionary<string, Object>();
            mapInner.Put("p1bean", typeof(SupportBean));
            mapInner.Put("p1beanComplex", typeof(SupportBeanComplexProps));
            mapInner.Put("p1beanArray", typeof(SupportBean[]));
            mapInner.Put("p1innerId", typeof(int));
            mapInner.Put("p1innerMap", mapInnerInner);
            epService.EPAdministrator.Configuration.AddEventType("MMInnerMap", mapInner);
    
            var mapOuter = new Dictionary<string, Object>();
            mapOuter.Put("p0simple", "MMInnerMap");
            mapOuter.Put("p0array", "MMInnerMap[]");
            epService.EPAdministrator.Configuration.AddEventType("MMOuterMap", mapOuter);
    
            var stmt = epService.EPAdministrator.CreateEPL("select * from MMOuterMap");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var dataInnerInner = new Dictionary<string, Object>();
            dataInnerInner.Put("p2id", 10);
    
            var dataInner = new Dictionary<string, Object>();
            dataInner.Put("p1bean", new SupportBean("string1", 2000));
            dataInner.Put("p1beanComplex", SupportBeanComplexProps.MakeDefaultBean());
            dataInner.Put("p1beanArray", new SupportBean[]{new SupportBean("string2", 1), new SupportBean("string3", 2)});
            dataInner.Put("p1innerId", 50);
            dataInner.Put("p1innerMap", dataInnerInner);
    
            var dataOuter = new Dictionary<string, Object>();
            dataOuter.Put("p0simple", dataInner);
            dataOuter.Put("p0array", new Map[]{dataInner, dataInner});
    
            // send event
            epService.EPRuntime.SendEvent(dataOuter, "MMOuterMap");
            var eventBean = listener.AssertOneGetNewAndReset();
            // Log.Info(SupportEventTypeAssertionUtil.Print(eventBean));     comment me in
            var eventType = eventBean.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(eventType);
    
            // Fragment-to-simple
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0simple").FragmentType.GetPropertyDescriptor("p1innerId").PropertyType);
            var p0simpleEvent = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual(50, p0simpleEvent.Get("p1innerId"));
            p0simpleEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(50, p0simpleEvent.Get("p1innerId"));
    
            // Fragment-to-bean
            var p0arrayEvents = (EventBean[]) eventBean.GetFragment("p0array");
            Assert.AreSame(p0arrayEvents[0].EventType, p0simpleEvent.EventType);
            Assert.AreEqual("string1", eventBean.Get("p0array[0].p1bean.TheString"));
            Assert.AreEqual("string1", ((EventBean) eventBean.GetFragment("p0array[0].p1bean")).Get("TheString"));
    
            var innerOne = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual("string1", ((EventBean) innerOne.GetFragment("p1bean")).Get("TheString"));
            Assert.AreEqual("string1", innerOne.Get("p1bean.TheString"));
            innerOne = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual("string1", ((EventBean) innerOne.GetFragment("p1bean")).Get("TheString"));
            Assert.AreEqual("string1", innerOne.Get("p1bean.TheString"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryInvalid(EventBean theEvent, string property) {
            try {
                theEvent.Get(property);
                Assert.Fail();
            } catch (PropertyAccessException) {
                // expected
            }
        }
    }
} // end of namespace
