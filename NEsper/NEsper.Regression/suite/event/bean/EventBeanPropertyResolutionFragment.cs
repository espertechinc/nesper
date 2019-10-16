///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionFragment
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLBeanMapSimpleTypes());
            execs.Add(new EPLBeanObjectArraySimpleTypes());
            execs.Add(new EPLBeanWrapperFragmentWithMap());
            execs.Add(new EPLBeanWrapperFragmentWithObjectArray());
            execs.Add(new EPLBeanNativeBeanFragment());
            execs.Add(new EPLBeanMapFragmentMapNested());
            execs.Add(new EPLBeanObjectArrayFragmentObjectArrayNested());
            execs.Add(new EPLBeanMapFragmentMapUnnamed());
            execs.Add(new EPLBeanMapFragmentTransposedMapEventBean());
            execs.Add(new EPLBeanObjectArrayFragmentTransposedMapEventBean());
            execs.Add(new EPLBeanMapFragmentMapBeans());
            execs.Add(new EPLBeanObjectArrayFragmentBeans());
            execs.Add(new EPLBeanMapFragmentMap3Level());
            execs.Add(new EPLBeanObjectArrayFragment3Level());
            execs.Add(new EPLBeanFragmentMapMulti());
            return execs;
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

        internal class EPLBeanMapSimpleTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from MSTypeOne").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1someval", "A");

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", 99);
                dataRoot.Put("p0array", new[] {101, 102});
                dataRoot.Put("p0map", dataInner);

                // send event
                env.SendEventMap(dataRoot, "MSTypeOne");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    //comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                // resolve property via fragment
                Assert.IsNull(eventType.GetFragmentType("p0int"));
                Assert.IsNull(eventType.GetFragmentType("p0intarray"));
                Assert.IsNull(eventBean.GetFragment("p0map?"));
                Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
                Assert.IsNull(eventBean.GetFragment("p0map('a')?"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanObjectArraySimpleTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from OASimple").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1someval", "A");
                object[] dataRoot = {99, new[] {101, 102}, dataInner};

                // send event
                env.SendEventObjectArray(dataRoot, "OASimple");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    //comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                // resolve property via fragment
                Assert.IsNull(eventType.GetFragmentType("p0int"));
                Assert.IsNull(eventType.GetFragmentType("p0intarray"));
                Assert.IsNull(eventBean.GetFragment("p0map?"));
                Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
                Assert.IsNull(eventBean.GetFragment("p0map('a')?"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanWrapperFragmentWithMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select *, p0simple.p1id + 1 as plusone, p0bean as mybean from Frosty");
                env.AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1id", 10);

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);
                dataRoot.Put("p0bean", SupportBeanComplexProps.MakeDefaultBean());

                // send event
                env.SendEventMap(dataRoot, "Frosty");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  Console.WriteLine(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                // resolve property via fragment
                Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                Assert.AreEqual(11, eventBean.Get("plusone"));
                Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));

                var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
                Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

                var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");
                Assert.AreEqual(
                    "NestedNestedValue", 
                    innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanWrapperFragmentWithObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select *, p0simple.p1id + 1 as plusone, p0bean as mybean from WheatRoot");
                env.AddListener("s0");

                env.SendEventObjectArray(
                    new object[] {new object[] {10}, SupportBeanComplexProps.MakeDefaultBean()},
                    "WheatRoot");

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
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
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanNativeBeanFragment : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from SupportBeanComplexProps").AddListener("s0");

                // assert nested fragments
                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
                //Console.WriteLine(SupportEventTypeAssertionUtil.print(eventBean));

                Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Nested").IsFragment);
                var eventNested = (EventBean) eventBean.GetFragment("Nested");
                Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));
                eventNested = (EventBean) eventBean.GetFragment("Nested?");
                Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));

                Assert.IsTrue(eventNested.EventType.GetPropertyDescriptor("NestedNested").IsFragment);
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventNested.GetFragment("NestedNested")).Get("NestedNestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventNested.GetFragment("NestedNested?")).Get("NestedNestedValue"));

                var nestedFragment = (EventBean) eventBean.GetFragment("Nested.NestedNested");
                Assert.AreEqual("NestedNestedValue", nestedFragment.Get("NestedNestedValue"));
                env.UndeployAll();

                // assert indexed fragments
                env.CompileDeploy("@Name('s0') select * from SupportBeanCombinedProps").AddListener("s0");
                var eventObject = SupportBeanCombinedProps.MakeDefaultBean();
                env.SendEventBean(eventObject);
                eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
                //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));

                Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsFragment);
                Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsIndexed);
                var eventArray = (EventBean[]) eventBean.GetFragment("Array");
                Assert.AreEqual(3, eventArray.Length);

                var eventElement = eventArray[0];
                Assert.AreSame(
                    eventObject.Array[0].GetMapped("0ma"),
                    eventElement.Get("Mapped('0ma')"));
                Assert.AreSame(
                    eventObject.Array[0].GetMapped("0ma"),
                    eventBean.GetFragment("Array[0]").AsEventBean().Get("Mapped('0ma')"));
                Assert.AreSame(
                    eventObject.Array[0].GetMapped("0ma"),
                    eventBean.GetFragment("Array[0]?").AsEventBean().Get("Mapped('0ma')"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanMapFragmentMapNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from HomerunRoot").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1id", 10);

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);
                dataRoot.Put("p0array", new[] {dataInner, dataInner});

                // send event
                env.SendEventMap(dataRoot, "HomerunRoot");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
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

                env.UndeployAll();
            }
        }

        internal class EPLBeanObjectArrayFragmentObjectArrayNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from GoalRoot").AddListener("s0");

                Assert.AreEqual(typeof(object[]), env.Statement("s0").EventType.UnderlyingType);

                env.SendEventObjectArray(
                    new object[] {
                        new object[] {10},
                        new object[] {
                            new object[] {20},
                            new object[] {21}
                        }
                    },
                    "GoalRoot");

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
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

                env.UndeployAll();
            }
        }

        internal class EPLBeanMapFragmentMapUnnamed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from FlywheelRoot").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1id", 10);

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);

                // send event
                env.SendEventMap(dataRoot, "FlywheelRoot");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.IsFalse(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                Assert.IsNull(eventBean.GetFragment("p0simple"));

                // resolve property via getter
                Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanMapFragmentTransposedMapEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from pattern[one=GistMapOne until two=GistMapTwo]")
                    .AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p2Id", 2000);
                IDictionary<string, object> dataMap = new Dictionary<string, object>();
                dataMap.Put("Id", 1);
                dataMap.Put("bean", new SupportBean("E1", 100));
                dataMap.Put("beanarray", new[] {new SupportBean("E1", 100), new SupportBean("E2", 200)});
                dataMap.Put("complex", SupportBeanComplexProps.MakeDefaultBean());
                dataMap.Put("complexarray", new[] {SupportBeanComplexProps.MakeDefaultBean()});
                dataMap.Put("map", dataInner);
                dataMap.Put("maparray", new[] {dataInner, dataInner});

                // send event
                env.SendEventMap(dataMap, "GistMapOne");

                IDictionary<string, object> dataMapTwo = new Dictionary<string, object>(dataMap);
                dataMapTwo.Put("Id", 2);
                env.SendEventMap(dataMapTwo, "GistMapOne");

                IDictionary<string, object> dataMapThree = new Dictionary<string, object>(dataMap);
                dataMapThree.Put("Id", 3);
                env.SendEventMap(dataMapThree, "GistMapTwo");

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                // System.out.println(SupportEventTypeAssertionUtil.print(eventBean));
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("Id"));
                Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("Id"));
                Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("Id"));

                Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[0].bean")).Get("TheString"));
                Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[1].bean")).Get("TheString"));
                Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("two.bean")).Get("TheString"));

                Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("one[0].beanarray[1]")).Get("TheString"));
                Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("two.beanarray[1]")).Get("TheString"));

                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get("NestedNestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("two.complex.Nested.NestedNested")).Get("NestedNestedValue"));

                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get(
                        "NestedNestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get(
                        "NestedNestedValue"));

                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].map")).Get("p2Id"));
                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.map")).Get("p2Id"));

                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].maparray[1]")).Get("p2Id"));
                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.maparray[1]")).Get("p2Id"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanObjectArrayFragmentTransposedMapEventBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from pattern[one=CashMapOne until two=CashMapTwo]")
                    .AddListener("s0");

                object[] dataInner = {2000};
                object[] dataArray = {
                    1, new SupportBean("E1", 100),
                    new[] {new SupportBean("E1", 100), new SupportBean("E2", 200)},
                    SupportBeanComplexProps.MakeDefaultBean(),
                    new[] {SupportBeanComplexProps.MakeDefaultBean()},
                    dataInner,
                    new object[] {dataInner, dataInner}
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

                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                // System.out.println(SupportEventTypeAssertionUtil.print(eventBean));
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("Id"));
                Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("Id"));
                Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("Id"));

                Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[0].bean")).Get("TheString"));
                Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("one[1].bean")).Get("TheString"));
                Assert.AreEqual("E1", ((EventBean) eventBean.GetFragment("two.bean")).Get("TheString"));

                Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("one[0].beanarray[1]")).Get("TheString"));
                Assert.AreEqual("E2", ((EventBean) eventBean.GetFragment("two.beanarray[1]")).Get("TheString"));

                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get("NestedNestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("two.complex.Nested.NestedNested")).Get("NestedNestedValue"));

                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get(
                        "NestedNestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get(
                        "NestedNestedValue"));

                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].map")).Get("p2Id"));
                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.map")).Get("p2Id"));

                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("one[0].maparray[1]")).Get("p2Id"));
                Assert.AreEqual(2000, ((EventBean) eventBean.GetFragment("two.maparray[1]")).Get("p2Id"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanMapFragmentMapBeans : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from TXTypeRoot").AddListener("s0");

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1simple", new SupportBean("E1", 11));
                dataInner.Put("p1array", new[] {new SupportBean("A1", 21), new SupportBean("A2", 22)});
                dataInner.Put("p1complex", SupportBeanComplexProps.MakeDefaultBean());
                dataInner.Put(
                    "p1complexarray",
                    new[] {SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean()});

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataInner);
                dataRoot.Put("p0array", new[] {dataInner, dataInner});

                // send event
                env.SendEventMap(dataRoot, "TXTypeRoot");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
                Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
                Assert.AreEqual(
                    "simple",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("simpleProperty"));
                Assert.AreEqual(
                    "simple",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("simpleProperty"));
                Assert.AreEqual(
                    "NestedValue",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get(
                        "NestedNestedValue"));

                var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
                Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get("NestedNestedValue"));

                assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
                Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));

                assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
                Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanObjectArrayFragmentBeans : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from LocalTypeRoot").AddListener("s0");

                Assert.AreEqual(typeof(object[]), env.Statement("s0").EventType.UnderlyingType);

                object[] dataInner = {
                    new SupportBean("E1", 11), new[] {new SupportBean("A1", 21), new SupportBean("A2", 22)},
                    SupportBeanComplexProps.MakeDefaultBean(),
                    new[] {SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean()}
                };
                object[] dataRoot = {
                    dataInner,
                    new object[] {dataInner, dataInner}
                };

                // send event
                env.SendEventObjectArray(dataRoot, "LocalTypeRoot");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
                Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
                Assert.AreEqual(
                    "simple",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("simpleProperty"));
                Assert.AreEqual(
                    "simple",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("simpleProperty"));
                Assert.AreEqual(
                    "NestedValue",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get(
                        "NestedNestedValue"));

                var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
                Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));
                Assert.AreEqual(
                    "NestedNestedValue",
                    ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get("NestedNestedValue"));

                assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
                Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));

                assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
                Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
                Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("IntPrimitive"));
                Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("IntPrimitive"));

                env.UndeployAll();
            }
        }

        internal class EPLBeanMapFragmentMap3Level : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from JimTypeRoot").AddListener("s0");

                IDictionary<string, object> dataLev1 = new Dictionary<string, object>();
                dataLev1.Put("p2Id", 10);

                IDictionary<string, object> dataLev0 = new Dictionary<string, object>();
                dataLev0.Put("p1simple", dataLev1);
                dataLev0.Put("p1array", new[] {dataLev1, dataLev1});

                IDictionary<string, object> dataRoot = new Dictionary<string, object>();
                dataRoot.Put("p0simple", dataLev0);
                dataRoot.Put("p0array", new[] {dataLev0, dataLev0});

                // send event
                env.SendEventMap(dataRoot, "JimTypeRoot");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("p2Id"));
                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1simple")).Get("p2Id"));
                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2Id"));
                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1array[0]")).Get("p2Id"));

                // resolve property via fragment
                var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
                Assert.AreEqual(10, assertEvent.Get("p1simple.p2Id"));
                Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2Id"));

                assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
                Assert.AreEqual(10, assertEvent.Get("p1simple.p2Id"));
                Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2Id"));

                assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
                Assert.AreEqual(10, assertEvent.Get("p1simple.p2Id"));
                Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2Id"));

                Assert.AreEqual("JimTypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
                Assert.AreEqual(
                    typeof(int?),
                    eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2Id"));
                Assert.AreEqual(
                    typeof(int?),
                    eventType.GetFragmentType("p0array[0].p1array[0]")
                        .FragmentType.GetPropertyDescriptor("p2Id")
                        .PropertyType);
                Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
                Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

                TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");

                env.UndeployAll();
            }
        }

        internal class EPLBeanObjectArrayFragment3Level : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from JackTypeRoot").AddListener("s0");

                Assert.AreEqual(typeof(object[]), env.Statement("s0").EventType.UnderlyingType);

                object[] dataLev1 = {10};
                object[] dataLev0 = {
                    dataLev1,
                    new object[] {dataLev1, dataLev1}
                };
                object[] dataRoot = {
                    dataLev0,
                    new object[] {dataLev0, dataLev0}
                };

                // send event
                env.SendEventObjectArray(dataRoot, "JackTypeRoot");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("p2Id"));
                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1simple")).Get("p2Id"));
                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2Id"));
                Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1array[0]")).Get("p2Id"));

                // resolve property via fragment
                var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
                Assert.AreEqual(10, assertEvent.Get("p1simple.p2Id"));
                Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2Id"));

                assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
                Assert.AreEqual(10, assertEvent.Get("p1simple.p2Id"));
                Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2Id"));

                assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
                Assert.AreEqual(10, assertEvent.Get("p1simple.p2Id"));
                Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2Id"));

                Assert.AreEqual("JackTypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
                Assert.AreEqual(
                    typeof(int?),
                    eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2Id"));
                Assert.AreEqual(
                    typeof(int?),
                    eventType.GetFragmentType("p0array[0].p1array[0]")
                        .FragmentType.GetPropertyDescriptor("p2Id")
                        .PropertyType);
                Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
                Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

                TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");

                env.UndeployAll();
            }
        }

        internal class EPLBeanFragmentMapMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from MMOuterMap").AddListener("s0");

                IDictionary<string, object> dataInnerInner = new Dictionary<string, object>();
                dataInnerInner.Put("p2Id", 10);

                IDictionary<string, object> dataInner = new Dictionary<string, object>();
                dataInner.Put("p1bean", new SupportBean("string1", 2000));
                dataInner.Put("p1beanComplex", SupportBeanComplexProps.MakeDefaultBean());
                dataInner.Put("p1beanArray", new[] {new SupportBean("string2", 1), new SupportBean("string3", 2)});
                dataInner.Put("p1innerId", 50);
                dataInner.Put("p1innerMap", dataInnerInner);

                IDictionary<string, object> dataOuter = new Dictionary<string, object>();
                dataOuter.Put("p0simple", dataInner);
                dataOuter.Put("p0array", new[] {dataInner, dataInner});

                // send event
                env.SendEventMap(dataOuter, "MMOuterMap");
                var eventBean = env.Listener("s0").AssertOneGetNewAndReset();
                // System.out.println(SupportEventTypeAssertionUtil.print(eventBean));     comment me in
                var eventType = eventBean.EventType;
                SupportEventTypeAssertionUtil.AssertConsistency(eventType);

                // Fragment-to-simple
                Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
                Assert.AreEqual(
                    typeof(int?),
                    eventType.GetFragmentType("p0simple").FragmentType.GetPropertyDescriptor("p1innerId").PropertyType);
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

                env.UndeployAll();
            }
        }
    }
} // end of namespace