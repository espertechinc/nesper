///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;


namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionFragment
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMapSimpleTypes(execs);
            WithObjectArraySimpleTypes(execs);
            WithWrapperFragmentWithMap(execs);
            WithWrapperFragmentWithObjectArray(execs);
            WithNativeBeanFragment(execs);
            WithMapFragmentMapNested(execs);
            WithObjectArrayFragmentObjectArrayNested(execs);
            WithMapFragmentMapUnnamed(execs);
            WithMapFragmentTransposedMapEventBean(execs);
            WithObjectArrayFragmentTransposedMapEventBean(execs);
            WithMapFragmentMapBeans(execs);
            WithObjectArrayFragmentBeans(execs);
            WithMapFragmentMap3Level(execs);
            WithObjectArrayFragment3Level(execs);
            WithFragmentMapMulti(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFragmentMapMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanFragmentMapMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArrayFragment3Level(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanObjectArrayFragment3Level());
            return execs;
        }

        public static IList<RegressionExecution> WithMapFragmentMap3Level(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanMapFragmentMap3Level());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArrayFragmentBeans(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanObjectArrayFragmentBeans());
            return execs;
        }

        public static IList<RegressionExecution> WithMapFragmentMapBeans(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanMapFragmentMapBeans());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArrayFragmentTransposedMapEventBean(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanObjectArrayFragmentTransposedMapEventBean());
            return execs;
        }

        public static IList<RegressionExecution> WithMapFragmentTransposedMapEventBean(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanMapFragmentTransposedMapEventBean());
            return execs;
        }

        public static IList<RegressionExecution> WithMapFragmentMapUnnamed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanMapFragmentMapUnnamed());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArrayFragmentObjectArrayNested(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanObjectArrayFragmentObjectArrayNested());
            return execs;
        }

        public static IList<RegressionExecution> WithMapFragmentMapNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanMapFragmentMapNested());
            return execs;
        }

        public static IList<RegressionExecution> WithNativeBeanFragment(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanNativeBeanFragment());
            return execs;
        }

        public static IList<RegressionExecution> WithWrapperFragmentWithObjectArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanWrapperFragmentWithObjectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithWrapperFragmentWithMap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanWrapperFragmentWithMap());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArraySimpleTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanObjectArraySimpleTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithMapSimpleTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLBeanMapSimpleTypes());
            return execs;
        }

        private class EPLBeanMapSimpleTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from MSTypeOne").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1someval", "A");

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", 99);
                dataRoot.Put("p0array", new int[] { 101, 102 });
                dataRoot.Put("p0map", dataInner);

                // send event
                env.SendEventMap(dataRoot, "MSTypeOne");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // resolve property via fragment
                        ClassicAssert.IsNull(eventType.GetFragmentType("p0int"));
                        ClassicAssert.IsNull(eventType.GetFragmentType("p0intarray"));
                        ClassicAssert.IsNull(eventBean.GetFragment("p0map?"));
                        ClassicAssert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
                        ClassicAssert.IsNull(eventBean.GetFragment("p0map('a')?"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanObjectArraySimpleTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from OASimple").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1someval", "A");
                var dataRoot = new object[] { 99, new int[] { 101, 102 }, dataInner };

                // send event
                env.SendEventObjectArray(dataRoot, "OASimple");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // resolve property via fragment
                        ClassicAssert.IsNull(eventType.GetFragmentType("p0int"));
                        ClassicAssert.IsNull(eventType.GetFragmentType("p0intarray"));
                        ClassicAssert.IsNull(eventBean.GetFragment("p0map?"));
                        ClassicAssert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
                        ClassicAssert.IsNull(eventBean.GetFragment("p0map('a')?"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanWrapperFragmentWithMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select *, p0simple.p1id + 1 as plusone, p0bean as mybean from Frosty");
                env.AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1id", 10);

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);
                dataRoot.Put("p0bean", SupportBeanComplexProps.MakeDefaultBean());

                // send event
                env.SendEventMap(dataRoot, "Frosty");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // resolve property via fragment
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                        ClassicAssert.AreEqual(11, eventBean.Get("plusone"));
                        ClassicAssert.AreEqual(10, eventBean.Get("p0simple.p1id"));

                        var innerSimpleEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(10, innerSimpleEvent.Get("p1id"));

                        var innerBeanEvent = (EventBean)eventBean.GetFragment("mybean");
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanWrapperFragmentWithObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') select *, p0simple.p1id + 1 as plusone, p0bean as mybean from WheatRoot");
                env.AddListener("s0");

                env.SendEventObjectArray(
                    new object[] { new object[] { 10 }, SupportBeanComplexProps.MakeDefaultBean() },
                    "WheatRoot");

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // resolve property via fragment
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                        ClassicAssert.AreEqual(11, eventBean.Get("plusone"));
                        ClassicAssert.AreEqual(10, eventBean.Get("p0simple.p1id"));

                        var innerSimpleEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(10, innerSimpleEvent.Get("p1id"));

                        var innerBeanEvent = (EventBean)eventBean.GetFragment("mybean");
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanNativeBeanFragment : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from SupportBeanComplexProps").AddListener("s0");

                // assert nested fragments
                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
                        //Console.WriteLine(SupportEventTypeAssertionUtil.print(eventBean));

                        ClassicAssert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Nested").IsFragment);
                        var eventNested = (EventBean)eventBean.GetFragment("Nested");
                        ClassicAssert.AreEqual("NestedValue", eventNested.Get("NestedValue"));
                        eventNested = (EventBean)eventBean.GetFragment("nested?");
                        ClassicAssert.AreEqual("NestedValue", eventNested.Get("NestedValue"));

                        ClassicAssert.IsTrue(eventNested.EventType.GetPropertyDescriptor("NestedNested").IsFragment);
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventNested.GetFragment("NestedNested")).Get("NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventNested.GetFragment("NestedNested?")).Get("NestedNestedValue"));

                        var nestedFragment = (EventBean)eventBean.GetFragment("Nested.NestedNested");
                        ClassicAssert.AreEqual("NestedNestedValue", nestedFragment.Get("NestedNestedValue"));
                    });
                env.UndeployAll();

                // assert indexed fragments
                env.CompileDeploy("@name('s0') select * from SupportBeanCombinedProps").AddListener("s0");
                var eventObject = SupportBeanCombinedProps.MakeDefaultBean();
                env.SendEventBean(eventObject);
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
                        //Console.WriteLine(SupportEventTypeAssertionUtil.print(eventBean));

                        ClassicAssert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsFragment);
                        ClassicAssert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsIndexed);
                        var eventArray = (EventBean[])eventBean.GetFragment("Array");
                        ClassicAssert.AreEqual(3, eventArray.Length);

                        var eventElement = eventArray[0];
                        ClassicAssert.AreSame(eventObject.Array[0].GetMapped("0ma"), eventElement.Get("Mapped('0ma')"));
                        ClassicAssert.AreSame(
                            eventObject.Array[0].GetMapped("0ma"),
                            ((EventBean)eventBean.GetFragment("Array[0]")).Get("Mapped('0ma')"));
                        ClassicAssert.AreSame(
                            eventObject.Array[0].GetMapped("0ma"),
                            ((EventBean)eventBean.GetFragment("Array[0]?")).Get("Mapped('0ma')"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanMapFragmentMapNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from HomerunRoot").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1id", 10);

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);
                dataRoot.Put("p0array", new IDictionary<string, object>[] { dataInner, dataInner });

                // send event
                env.SendEventMap(dataRoot, "HomerunRoot");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // resolve property via fragment
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);

                        var innerSimpleEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(10, innerSimpleEvent.Get("p1id"));

                        var innerArrayAllEvent = (EventBean[])eventBean.GetFragment("p0array");
                        ClassicAssert.AreEqual(10, innerArrayAllEvent[0].Get("p1id"));

                        var innerArrayElementEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual(10, innerArrayElementEvent.Get("p1id"));

                        // resolve property via getter
                        ClassicAssert.AreEqual(10, eventBean.Get("p0simple.p1id"));
                        ClassicAssert.AreEqual(10, eventBean.Get("p0array[1].p1id"));

                        ClassicAssert.IsNull(eventType.GetFragmentType("p0array.p1id"));
                        ClassicAssert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanObjectArrayFragmentObjectArrayNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from GoalRoot").AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(object[]), statement.EventType.UnderlyingType));

                env.SendEventObjectArray(
                    new object[] { new object[] { 10 }, new object[] { new object[] { 20 }, new object[] { 21 } } },
                    "GoalRoot");

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // resolve property via fragment
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);

                        var innerSimpleEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(10, innerSimpleEvent.Get("p1id"));

                        var innerArrayAllEvent = (EventBean[])eventBean.GetFragment("p0array");
                        ClassicAssert.AreEqual(20, innerArrayAllEvent[0].Get("p1id"));

                        var innerArrayElementEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual(20, innerArrayElementEvent.Get("p1id"));

                        // resolve property via getter
                        ClassicAssert.AreEqual(10, eventBean.Get("p0simple.p1id"));
                        ClassicAssert.AreEqual(21, eventBean.Get("p0array[1].p1id"));

                        ClassicAssert.IsNull(eventType.GetFragmentType("p0array.p1id"));
                        ClassicAssert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanMapFragmentMapUnnamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from FlywheelRoot").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1id", 10);

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);

                // send event
                env.SendEventMap(dataRoot, "FlywheelRoot");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.IsFalse(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                        ClassicAssert.IsNull(eventBean.GetFragment("p0simple"));

                        // resolve property via getter
                        ClassicAssert.AreEqual(10, eventBean.Get("p0simple.p1id"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanMapFragmentTransposedMapEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from pattern[one=GistMapOne until two=GistMapTwo]")
                    .AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p2id", 2000);
                IDictionary<string, object> dataMap = new Dictionary<string, object>();
                dataMap.Put("id", 1);
                dataMap.Put("bean", new SupportBean("E1", 100));
                dataMap.Put("beanarray", new SupportBean[] { new SupportBean("E1", 100), new SupportBean("E2", 200) });
                dataMap.Put("complex", SupportBeanComplexProps.MakeDefaultBean());
                dataMap.Put(
                    "complexarray",
                    new SupportBeanComplexProps[] { SupportBeanComplexProps.MakeDefaultBean() });
                dataMap.Put("map", dataInner);
                dataMap.Put("maparray", new IDictionary<string, object>[] { dataInner, dataInner });

                // send event
                env.SendEventMap(dataMap, "GistMapOne");

                IDictionary<string, object> dataMapTwo = new Dictionary<string, object>(dataMap);
                dataMapTwo.Put("id", 2);
                env.SendEventMap(dataMapTwo, "GistMapOne");

                IDictionary<string, object> dataMapThree = new Dictionary<string, object>(dataMap);
                dataMapThree.Put("id", 3);
                env.SendEventMap(dataMapThree, "GistMapTwo");

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.AreEqual(1, ((EventBean)eventBean.GetFragment("one[0]")).Get("id"));
                        ClassicAssert.AreEqual(2, ((EventBean)eventBean.GetFragment("one[1]")).Get("id"));
                        ClassicAssert.AreEqual(3, ((EventBean)eventBean.GetFragment("two")).Get("id"));

                        ClassicAssert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[0].bean")).Get("TheString"));
                        ClassicAssert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[1].bean")).Get("TheString"));
                        ClassicAssert.AreEqual("E1", ((EventBean)eventBean.GetFragment("two.bean")).Get("TheString"));

                        ClassicAssert.AreEqual(
                            "E2",
                            ((EventBean)eventBean.GetFragment("one[0].beanarray[1]")).Get("TheString"));
                        ClassicAssert.AreEqual("E2", ((EventBean)eventBean.GetFragment("two.beanarray[1]")).Get("TheString"));

                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("two.complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].map")).Get("p2id"));
                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.map")).Get("p2id"));

                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].maparray[1]")).Get("p2id"));
                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.maparray[1]")).Get("p2id"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanObjectArrayFragmentTransposedMapEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from pattern[one=CashMapOne until two=CashMapTwo]")
                    .AddListener("s0");

                var dataInner = new object[] { 2000 };
                var dataArray = new object[] {
                    1, new SupportBean("E1", 100),
                    new SupportBean[] { new SupportBean("E1", 100), new SupportBean("E2", 200) },
                    SupportBeanComplexProps.MakeDefaultBean(),
                    new SupportBeanComplexProps[] { SupportBeanComplexProps.MakeDefaultBean() },
                    dataInner, new object[] { dataInner, dataInner }
                };

                // send event
                env.SendEventObjectArray(dataArray, "CashMapOne");

                var dataArrayTwo = new object[dataArray.Length];
                Array.Copy(dataArray, 0, dataArrayTwo, 0, dataArray.Length);
                dataArrayTwo[0] = 2;
                env.SendEventObjectArray(dataArrayTwo, "CashMapOne");

                var dataArrayThree = new object[dataArray.Length];
                Array.Copy(dataArray, 0, dataArrayThree, 0, dataArray.Length);
                dataArrayThree[0] = 3;
                env.SendEventObjectArray(dataArrayThree, "CashMapTwo");

                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.AreEqual(1, ((EventBean)eventBean.GetFragment("one[0]")).Get("id"));
                        ClassicAssert.AreEqual(2, ((EventBean)eventBean.GetFragment("one[1]")).Get("id"));
                        ClassicAssert.AreEqual(3, ((EventBean)eventBean.GetFragment("two")).Get("id"));

                        ClassicAssert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[0].bean")).Get("TheString"));
                        ClassicAssert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[1].bean")).Get("TheString"));
                        ClassicAssert.AreEqual("E1", ((EventBean)eventBean.GetFragment("two.bean")).Get("TheString"));

                        ClassicAssert.AreEqual(
                            "E2",
                            ((EventBean)eventBean.GetFragment("one[0].beanarray[1]")).Get("TheString"));
                        ClassicAssert.AreEqual("E2", ((EventBean)eventBean.GetFragment("two.beanarray[1]")).Get("TheString"));

                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("two.complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].map")).Get("p2id"));
                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.map")).Get("p2id"));

                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].maparray[1]")).Get("p2id"));
                        ClassicAssert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.maparray[1]")).Get("p2id"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanMapFragmentMapBeans : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from TXTypeRoot").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1simple", new SupportBean("E1", 11));
                dataInner.Put("p1array", new SupportBean[] { new SupportBean("A1", 21), new SupportBean("A2", 22) });
                dataInner.Put("p1complex", SupportBeanComplexProps.MakeDefaultBean());
                dataInner.Put(
                    "p1complexarray",
                    new SupportBeanComplexProps[]
                        { SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean() });

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);
                dataRoot.Put("p0array", new IDictionary<string, object>[] { dataInner, dataInner });

                // send event
                env.SendEventMap(dataRoot, "TXTypeRoot");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.AreEqual(
                            11,
                            ((EventBean)eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(
                            "A2",
                            ((EventBean)eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
                        ClassicAssert.AreEqual(
                            "Simple",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complex")).Get("SimpleProperty"));
                        ClassicAssert.AreEqual(
                            "Simple",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("SimpleProperty"));
                        ClassicAssert.AreEqual(
                            "NestedValue",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        var assertEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                        ClassicAssert.AreEqual(11, ((EventBean)assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(22, ((EventBean)assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        assertEvent = ((EventBean[])eventBean.GetFragment("p0array"))[0];
                        ClassicAssert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                        ClassicAssert.AreEqual(11, ((EventBean)assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(22, ((EventBean)assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));

                        assertEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                        ClassicAssert.AreEqual(11, ((EventBean)assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(22, ((EventBean)assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanObjectArrayFragmentBeans : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from LocalTypeRoot").AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(object[]), statement.EventType.UnderlyingType));

                object[] dataInner = {
                    new SupportBean("E1", 11),
                    new SupportBean[] { new SupportBean("A1", 21), new SupportBean("A2", 22) },
                    SupportBeanComplexProps.MakeDefaultBean(),
                    new SupportBeanComplexProps[]
                        { SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean() }
                };
                var dataRoot = new object[] { dataInner, new object[] { dataInner, dataInner } };

                // send event
                env.SendEventObjectArray(dataRoot, "LocalTypeRoot");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.AreEqual(
                            11,
                            ((EventBean)eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(
                            "A2",
                            ((EventBean)eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
                        ClassicAssert.AreEqual(
                            "Simple",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complex")).Get("SimpleProperty"));
                        ClassicAssert.AreEqual(
                            "Simple",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("SimpleProperty"));
                        ClassicAssert.AreEqual(
                            "NestedValue",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        var assertEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                        ClassicAssert.AreEqual(11, ((EventBean)assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(22, ((EventBean)assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(
                            "NestedNestedValue",
                            ((EventBean)assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

                        assertEvent = ((EventBean[])eventBean.GetFragment("p0array"))[0];
                        ClassicAssert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                        ClassicAssert.AreEqual(11, ((EventBean)assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(22, ((EventBean)assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));

                        assertEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                        ClassicAssert.AreEqual(11, ((EventBean)assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                        ClassicAssert.AreEqual(22, ((EventBean)assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanMapFragmentMap3Level : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from JimTypeRoot").AddListener("s0");

                IDictionary<string, object> dataLev1 = new Dictionary<string, object>();
                dataLev1.Put("p2id", 10);

                IDictionary<string, object> dataLev0 = new Dictionary<string, object>();
                dataLev0.Put("p1simple", dataLev1);
                dataLev0.Put("p1array", new IDictionary<string, object>[] { dataLev1, dataLev1 });

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataLev0);
                dataRoot.Put("p0array", new IDictionary<string, object>[] { dataLev0, dataLev0 });

                // send event
                env.SendEventMap(dataRoot, "JimTypeRoot");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));

                        // resolve property via fragment
                        var assertEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

                        assertEvent = ((EventBean[])eventBean.GetFragment("p0array"))[1];
                        ClassicAssert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

                        assertEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

                        ClassicAssert.AreEqual("JimTypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
                        ClassicAssert.AreEqual(
                            typeof(int?),
                            eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
                        ClassicAssert.AreEqual(
                            typeof(int?),
                            eventType.GetFragmentType("p0array[0].p1array[0]")
                                .FragmentType.GetPropertyDescriptor("p2id")
                                .PropertyType);
                        ClassicAssert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
                        ClassicAssert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

                        TryInvalid((EventBean)eventBean.GetFragment("p0simple"), "p1simple.p1id");
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanObjectArrayFragment3Level : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from JackTypeRoot").AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(object[]), statement.EventType.UnderlyingType));

                var dataLev1 = new object[] { 10 };
                var dataLev0 = new object[] { dataLev1, new object[] { dataLev1, dataLev1 } };
                var dataRoot = new object[] { dataLev0, new object[] { dataLev0, dataLev0 } };

                // send event
                env.SendEventObjectArray(dataRoot, "JackTypeRoot");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));

                        // resolve property via fragment
                        var assertEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

                        assertEvent = ((EventBean[])eventBean.GetFragment("p0array"))[1];
                        ClassicAssert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

                        assertEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
                        ClassicAssert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

                        ClassicAssert.AreEqual(
                            "JackTypeLev1",
                            eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
                        ClassicAssert.AreEqual(
                            typeof(int?),
                            eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
                        ClassicAssert.AreEqual(
                            typeof(int?),
                            eventType.GetFragmentType("p0array[0].p1array[0]")
                                .FragmentType.GetPropertyDescriptor("p2id")
                                .PropertyType);
                        ClassicAssert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
                        ClassicAssert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

                        TryInvalid((EventBean)eventBean.GetFragment("p0simple"), "p1simple.p1id");
                    });

                env.UndeployAll();
            }
        }

        private class EPLBeanFragmentMapMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from MMOuterMap").AddListener("s0");

                IDictionary<string, object> dataInnerInner = new Dictionary<string, object>();
                dataInnerInner.Put("p2id", 10);

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1bean", new SupportBean("string1", 2000));
                dataInner.Put("p1beanComplex", SupportBeanComplexProps.MakeDefaultBean());
                dataInner.Put(
                    "p1beanArray",
                    new SupportBean[] { new SupportBean("string2", 1), new SupportBean("string3", 2) });
                dataInner.Put("p1innerId", 50);
                dataInner.Put("p1innerMap", dataInnerInner);

                IDictionary<string, object> dataOuter = new Dictionary<string, object>();
                dataOuter.Put("p0simple", dataInner);
                dataOuter.Put("p0array", new IDictionary<string, object>[] { dataInner, dataInner });

                // send event
                env.SendEventMap(dataOuter, "MMOuterMap");
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        var eventType = eventBean.EventType;
                        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                        // Fragment-to-simple
                        ClassicAssert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                        ClassicAssert.AreEqual(
                            typeof(int?),
                            eventType.GetFragmentType("p0simple")
                                .FragmentType.GetPropertyDescriptor("p1innerId")
                                .PropertyType);
                        var p0simpleEvent = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual(50, p0simpleEvent.Get("p1innerId"));
                        p0simpleEvent = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual(50, p0simpleEvent.Get("p1innerId"));

                        // Fragment-to-bean
                        var p0arrayEvents = (EventBean[])eventBean.GetFragment("p0array");
                        ClassicAssert.AreSame(p0arrayEvents[0].EventType, p0simpleEvent.EventType);
                        ClassicAssert.AreEqual("string1", eventBean.Get("p0array[0].p1bean.TheString"));
                        ClassicAssert.AreEqual(
                            "string1",
                            ((EventBean)eventBean.GetFragment("p0array[0].p1bean")).Get("TheString"));

                        var innerOne = (EventBean)eventBean.GetFragment("p0array[0]");
                        ClassicAssert.AreEqual("string1", ((EventBean)innerOne.GetFragment("p1bean")).Get("TheString"));
                        ClassicAssert.AreEqual("string1", innerOne.Get("p1bean.TheString"));
                        innerOne = (EventBean)eventBean.GetFragment("p0simple");
                        ClassicAssert.AreEqual("string1", ((EventBean)innerOne.GetFragment("p1bean")).Get("TheString"));
                        ClassicAssert.AreEqual("string1", innerOne.Get("p1bean.TheString"));
                    });

                env.UndeployAll();
            }
        }

        private static void TryInvalid(
            EventBean theEvent,
            string property)
        {
            try {
                theEvent.Get(property);
                Assert.Fail();
            }
            catch (PropertyAccessException) {
                // expected
            }
        }
    }
} // end of namespace