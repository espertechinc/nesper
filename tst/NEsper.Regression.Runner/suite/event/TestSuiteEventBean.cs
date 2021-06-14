///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.@event.bean;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventBean
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            var myLegacyNestedEvent = new ConfigurationCommonEventTypeBean();
            myLegacyNestedEvent.AccessorStyle = AccessorStyle.EXPLICIT;
            myLegacyNestedEvent.AddFieldProperty("fieldNestedClassValue", "fieldNestedValue");
            myLegacyNestedEvent.AddMethodProperty("readNestedClassValue", "ReadNestedValue");
            configuration.Common.AddEventType(
                "MyLegacyNestedEvent",
                typeof(SupportLegacyBean.LegacyNested),
                myLegacyNestedEvent);

            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportOverrideOne),
                typeof(SupportOverrideOneA),
                typeof(SupportOverrideBase),
                typeof(SupportOverrideOneB),
                typeof(ISupportBaseAB),
                typeof(ISupportA),
                typeof(ISupportB),
                typeof(ISupportC),
                typeof(ISupportAImplSuperG),
                typeof(ISupportAImplSuperGImplPlus),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanWriteOnly),
                typeof(SupportBeanDupProperty),
                typeof(SupportBeanCombinedProps)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Common.AddEventType("SomeKeywords", typeof(SupportBeanReservedKeyword));
            configuration.Common.AddEventType("Order", typeof(SupportBeanReservedKeyword));

            var myFinalEvent = new ConfigurationCommonEventTypeBean();
            myFinalEvent.AccessorStyle = AccessorStyle.NATIVE;
            configuration.Common.AddEventType("MyFinalEvent", typeof(SupportBeanFinal), myFinalEvent);

            var myLegacyTwo = new ConfigurationCommonEventTypeBean();
            myLegacyTwo.AccessorStyle = AccessorStyle.NATIVE;
            myLegacyTwo.AddFieldProperty("explicitFInt", "fieldIntPrimitive");
            myLegacyTwo.AddMethodProperty("explicitMGetInt", "GetIntPrimitive");
            myLegacyTwo.AddMethodProperty("explicitMReadInt", "ReadIntPrimitive");
            configuration.Common.AddEventType("MyLegacyTwo", typeof(SupportLegacyBeanInt), myLegacyTwo);

            IDictionary<string, object> def = new Dictionary<string, object>();
            def.Put("Mapped", new Dictionary<string, object>());
            def.Put("Indexed", typeof(int[]));
            configuration.Common.AddEventType("MapEvent", def);

            IDictionary<string, object> defType = new Dictionary<string, object>();
            defType.Put("name", typeof(string));
            defType.Put("value", typeof(string));
            defType.Put("properties", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType("InputEvent", defType);

            configuration.Common.AddEventType(
                "ObjectArrayEvent",
                new[] {"Mapped", "Indexed"},
                new object[] {new Dictionary<string, object>(), typeof(int[])});

            var myEventWithField = new ConfigurationCommonEventTypeBean();
            myEventWithField.AccessorStyle = AccessorStyle.PUBLIC;
            configuration.Common.AddEventType(
                typeof(EventBeanPropertyIterableMapList.MyEventWithField).Name,
                typeof(EventBeanPropertyIterableMapList.MyEventWithField),
                myEventWithField);

            var configNoCglib = new ConfigurationCommonEventTypeBean();
            configuration.Common.AddEventType(
                typeof(EventBeanPropertyIterableMapList.MyEventWithMethod).Name,
                typeof(EventBeanPropertyIterableMapList.MyEventWithMethod),
                configNoCglib);

            IDictionary<string, object> mapOuter = new Dictionary<string, object>();
            mapOuter.Put("p0int", typeof(int));
            mapOuter.Put("p0intarray", typeof(int[]));
            mapOuter.Put("p0map", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType("MSTypeOne", mapOuter);

            string[] props = {"p0int", "p0intarray", "p0map"};
            object[] types = {typeof(int), typeof(int[]), typeof(IDictionary<string, object>)};
            configuration.Common.AddEventType("OASimple", props, types);

            IDictionary<string, object> frostyLev0 = new Dictionary<string, object>();
            frostyLev0.Put("p1id", typeof(int));
            configuration.Common.AddEventType("FrostyLev0", frostyLev0);

            IDictionary<string, object> frosty = new Dictionary<string, object>();
            frosty.Put("p0simple", "FrostyLev0");
            frosty.Put("p0bean", typeof(SupportBeanComplexProps));
            configuration.Common.AddEventType("Frosty", frosty);

            configuration.Common.AddEventType(
                "WheatLev0", new[] {"p1id"}, new object[] {typeof(int)});
            configuration.Common.AddEventType(
                "WheatRoot",
                new[] {"p0simple", "p0bean"},
                new object[] {"WheatLev0", typeof(SupportBeanComplexProps)});

            IDictionary<string, object> homerunLev0 = new Dictionary<string, object>();
            homerunLev0.Put("p1id", typeof(int));
            configuration.Common.AddEventType("HomerunLev0", homerunLev0);

            IDictionary<string, object> homerunRoot = new Dictionary<string, object>();
            homerunRoot.Put("p0simple", "HomerunLev0");
            homerunRoot.Put("p0array", "HomerunLev0[]");
            configuration.Common.AddEventType("HomerunRoot", homerunRoot);

            configuration.Common.AddEventType("GoalLev0", new[] {"p1id"}, new object[] {typeof(int)});
            configuration.Common.AddEventType(
                "GoalRoot",
                new[] {"p0simple", "p0array"},
                new object[] {"GoalLev0", "GoalLev0[]"});

            IDictionary<string, object> flywheelTypeLev0 = new Dictionary<string, object>();
            flywheelTypeLev0.Put("p1id", typeof(int));
            IDictionary<string, object> flywheelRoot = new Dictionary<string, object>();
            flywheelRoot.Put("p0simple", flywheelTypeLev0);
            configuration.Common.AddEventType("FlywheelRoot", flywheelRoot);

            IDictionary<string, object> gistInner = new Dictionary<string, object>();
            gistInner.Put("p2id", typeof(int));
            configuration.Common.AddEventType("GistInner", gistInner);

            IDictionary<string, object> typeMap = new Dictionary<string, object>();
            typeMap.Put("id", typeof(int));
            typeMap.Put("bean", typeof(SupportBean));
            typeMap.Put("beanarray", typeof(SupportBean[]));
            typeMap.Put("complex", typeof(SupportBeanComplexProps));
            typeMap.Put("complexarray", typeof(SupportBeanComplexProps[]));
            typeMap.Put("map", "GistInner");
            typeMap.Put("maparray", "GistInner[]");
            configuration.Common.AddEventType("GistMapOne", typeMap);
            configuration.Common.AddEventType("GistMapTwo", typeMap);

            configuration.Common.AddEventType("CashInner", new[] {"p2id"}, new object[] {typeof(int)});

            string[] propsCash = {"id", "bean", "beanarray", "complex", "complexarray", "map", "maparray"};
            object[] typesCash = {
                typeof(int), typeof(SupportBean), typeof(SupportBean[]), typeof(SupportBeanComplexProps),
                typeof(SupportBeanComplexProps[]), "CashInner", "CashInner[]"
            };
            configuration.Common.AddEventType("CashMapOne", propsCash, typesCash);
            configuration.Common.AddEventType("CashMapTwo", propsCash, typesCash);

            IDictionary<string, object> txTypeLev0 = new Dictionary<string, object>();
            txTypeLev0.Put("p1simple", typeof(SupportBean));
            txTypeLev0.Put("p1array", typeof(SupportBean[]));
            txTypeLev0.Put("p1complex", typeof(SupportBeanComplexProps));
            txTypeLev0.Put("p1complexarray", typeof(SupportBeanComplexProps[]));
            configuration.Common.AddEventType("TXTypeLev0", txTypeLev0);

            IDictionary<string, object> txTypeRoot = new Dictionary<string, object>();
            txTypeRoot.Put("p0simple", "TXTypeLev0");
            txTypeRoot.Put("p0array", "TXTypeLev0[]");
            configuration.Common.AddEventType("TXTypeRoot", txTypeRoot);

            string[] localTypeLev0 = {"p1simple", "p1array", "p1complex", "p1complexarray"};
            object[] typesLev0 = {
                typeof(SupportBean), typeof(SupportBean[]), typeof(SupportBeanComplexProps),
                typeof(SupportBeanComplexProps[])
            };
            configuration.Common.AddEventType("LocalTypeLev0", localTypeLev0, typesLev0);

            string[] localTypeRoot = {"p0simple", "p0array"};
            object[] typesOuter = {"LocalTypeLev0", "LocalTypeLev0[]"};
            configuration.Common.AddEventType("LocalTypeRoot", localTypeRoot, typesOuter);

            IDictionary<string, object> jimTypeLev1 = new Dictionary<string, object>();
            jimTypeLev1.Put("p2id", typeof(int));
            configuration.Common.AddEventType("JimTypeLev1", jimTypeLev1);

            IDictionary<string, object> jimTypeLev0 = new Dictionary<string, object>();
            jimTypeLev0.Put("p1simple", "JimTypeLev1");
            jimTypeLev0.Put("p1array", "JimTypeLev1[]");
            configuration.Common.AddEventType("JimTypeLev0", jimTypeLev0);

            IDictionary<string, object> jimTypeRoot = new Dictionary<string, object>();
            jimTypeRoot.Put("p0simple", "JimTypeLev0");
            jimTypeRoot.Put("p0array", "JimTypeLev0[]");
            configuration.Common.AddEventType("JimTypeRoot", jimTypeRoot);

            configuration.Common.AddEventType("JackTypeLev1", new[] {"p2id"}, new object[] {typeof(int)});
            configuration.Common.AddEventType(
                "JackTypeLev0",
                new[] {"p1simple", "p1array"},
                new object[] {"JackTypeLev1", "JackTypeLev1[]"});
            configuration.Common.AddEventType(
                "JackTypeRoot",
                new[] {"p0simple", "p0array"},
                new object[] {"JackTypeLev0", "JackTypeLev0[]"});

            IDictionary<string, object> mmInner = new Dictionary<string, object>();
            mmInner.Put("p2id", typeof(int));

            IDictionary<string, object> mmInnerMap = new Dictionary<string, object>();
            mmInnerMap.Put("p1bean", typeof(SupportBean));
            mmInnerMap.Put("p1beanComplex", typeof(SupportBeanComplexProps));
            mmInnerMap.Put("p1beanArray", typeof(SupportBean[]));
            mmInnerMap.Put("p1innerId", typeof(int));
            mmInnerMap.Put("p1innerMap", mmInner);
            configuration.Common.AddEventType("MMInnerMap", mmInnerMap);

            IDictionary<string, object> mmOuterMap = new Dictionary<string, object>();
            mmOuterMap.Put("p0simple", "MMInnerMap");
            mmOuterMap.Put("p0array", "MMInnerMap[]");
            configuration.Common.AddEventType("MMOuterMap", mmOuterMap);

            IDictionary<string, object> myTypeDef = new Dictionary<string, object>();
            myTypeDef.Put("candidate book", typeof(string));
            myTypeDef.Put("XML Message Type", typeof(string));
            myTypeDef.Put("select", typeof(int));
            myTypeDef.Put("children's books", typeof(int[]));
            myTypeDef.Put("my <> map", typeof(IDictionary<string, object>));
            configuration.Common.AddEventType("MyType", myTypeDef);

            configuration.Common.AddEventType(typeof(EventBeanPropertyResolutionWDefaults.LocalEventWithEnum));
            configuration.Common.AddEventType(typeof(EventBeanPropertyResolutionWDefaults.LocalEventWithGroup));

            var anotherLegacyNestedEvent = new ConfigurationCommonEventTypeBean();
            anotherLegacyNestedEvent.AccessorStyle = AccessorStyle.PUBLIC;
            configuration.Common.AddEventType(
                "AnotherLegacyNestedEvent",
                typeof(SupportLegacyBean.LegacyNested),
                anotherLegacyNestedEvent);

            configuration.Common.AddImportType(typeof(EventBeanPropertyResolutionWDefaults.LocalEventEnum));
            configuration.Common.AddImportType(typeof(EventBeanPropertyResolutionWDefaults.GROUP));
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanEventPropertyDynamicPerformance()
        {
            RegressionRunner.Run(session, new EventBeanEventPropertyDynamicPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanFinalClass()
        {
            RegressionRunner.Run(session, new EventBeanFinalClass());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanInheritAndInterface()
        {
            RegressionRunner.Run(session, EventBeanInheritAndInterface.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanNativeAccessor()
        {
            RegressionRunner.Run(session, new EventBeanNativeAccessor());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanMappedIndexedPropertyExpression()
        {
            RegressionRunner.Run(session, new EventBeanMappedIndexedPropertyExpression());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPrivateClass()
        {
            RegressionRunner.Run(session, new EventBeanPrivateClass());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyIterableMapList()
        {
            RegressionRunner.Run(session, new EventBeanPropertyIterableMapList());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionFragment()
        {
            RegressionRunner.Run(session, EventBeanPropertyResolutionFragment.Executions());
        }
        
        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyAccessPerformance() {
            RegressionRunner.Run(session, new EventBeanPropertyAccessPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionWDefaults()
        {
            RegressionRunner.Run(session, EventBeanPropertyResolutionWDefaults.Executions());
        }
    }
} // end of namespace