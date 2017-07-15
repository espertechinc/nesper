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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestPropertyResolution
    {
        private EPServiceProvider _epService;
    
        [Test]
        public void TestReservedKeywordEscape()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.typeof(EventMetaConfig)PropertyResolutionStyle =
                PropertyResolutionStyle.DEFAULT;
            configuration.EngineDefaults.EventMetaConfig.DefaultAccessorStyle =
                AccessorStyleEnum.NATIVE;

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanReservedKeyword>("SomeKeywords");
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanReservedKeyword>("Order");

            var listener = new SupportUpdateListener();
            var stmt = _epService.EPAdministrator.CreateEPL("select `seconds`, `order` from SomeKeywords");
    
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBeanReservedKeyword(1, 2);
    
            _epService.EPRuntime.SendEvent(theEvent);
            var eventBean = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual(1, eventBean.Get("seconds"));
            Assert.AreEqual(2, eventBean.Get("order"));
    
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select * from `Order`");
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(theEvent);
            eventBean = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, eventBean.Get("seconds"));
            Assert.AreEqual(2, eventBean.Get("order"));
    
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL(
                    "select timestamp.`hour` as val from SomeKeywords");
            stmt.Events += listener.Update;
    
            var bean = new SupportBeanReservedKeyword(1, 2);
    
            bean.Timestamp = new SupportBeanReservedKeyword.Inner();
            bean.Timestamp.Hour = 10;
            _epService.EPRuntime.SendEvent(bean);
            eventBean = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10, eventBean.Get("val"));
    
            // test back-tick with spaces etc
            var defType = new Dictionary<string, object>();
            defType["candidate book"] = typeof(string);
            defType["XML Message Type"] = typeof(string);
            defType["select"] = typeof(int);
            defType["children's books"] = typeof(int[]);
            defType["my <> map"] = typeof(Map);

            _epService.EPAdministrator.Configuration.AddEventType("MyType", defType);
            _epService.EPAdministrator.CreateEPL("select `candidate book` as c0, `XML Message Type` as c1, `select` as c2, `children's books`[0] as c3, `my <> map`('xx') as c4 from MyType")
                .Events += listener.Update;
    
            var defValues = new Dictionary<string, object>();
            defValues["candidate book"] = "Enders Game";
            defValues["XML Message Type"] = "book";
            defValues["select"] = 100;
            defValues["children's books"] = new int[] { 50, 51 };
            defValues["my <> map"] = Collections.SingletonMap("xx", "abc");

            _epService.EPRuntime.SendEvent(defValues, "MyType");

            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(),
                "c0,c1,c2,c3,c4".Split(','), new Object[]
                {
                    "Enders Game", "book", 100, 50, "abc"
                });
            
            try {
                _epService.EPAdministrator.CreateEPL("select `select` from " + typeof(SupportBean).FullName);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(
                        "Error starting statement: Failed to validate select-clause expression 'select': Property named 'select' is not valid in any stream [select `select` from com.espertech.esper.support.bean.SupportBean]",
                        ex.Message);
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("select `ab cd` from " + typeof(SupportBean).FullName);
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'ab cd': Failed to find property 'ab cd', the property name does not parse (are you sure?): Incorrect syntax near 'cd' at line 1 column 3 [ab cd] [select `ab cd` from com.espertech.esper.support.bean.SupportBean]");
            }

            // test resolution as nested property
            _epService.EPAdministrator.CreateEPL("create schema MyEvent as (customer string, `from` string)");
            _epService.EPAdministrator.CreateEPL("insert into DerivedStream select customer,`from` from MyEvent");
            _epService.EPAdministrator.CreateEPL("create window TheWindow.std:firstunique(customer,`from`) as DerivedStream");
            _epService.EPAdministrator.CreateEPL("on pattern [a=TheWindow -> timer:interval(12 hours)] as s0 delete from TheWindow as s1 where s0.a.`from`=s1.`from`");

            // test escape in column name
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("select TheString as `order`, TheString as `price.for.goods` from SupportBean");
            stmtTwo.Events += listener.Update;
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("order"));
            Assert.AreEqual("price.for.goods", stmtTwo.EventType.PropertyDescriptors[1].PropertyName);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var @out = (Map) listener.AssertOneGetNew().Underlying;
            Assert.AreEqual("E1", @out.Get("order"));
            Assert.AreEqual("E1", @out.Get("price.for.goods"));

            // try control character
            TryInvalidControlCharacter(listener.AssertOneGetNew());

            // try enum with keyword
            TryEnumWithKeyword();

            TryEnumItselfReserved();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void TryEnumWithKeyword()
        {
            _epService.EPAdministrator.Configuration.AddEventType<LocalEventWithEnum>();
            _epService.EPAdministrator.Configuration.AddImport<LocalEventEnum>();
            _epService.EPAdministrator.CreateEPL("select * from LocalEventWithEnum(LocalEventEnum=LocalEventEnum.`NEW`)");
        }

        private void TryInvalidControlCharacter(EventBean eventBean)
        {
            try
            {
                eventBean.Get("a\u008F");
                Assert.Fail();
            }
            catch (PropertyAccessException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, "Unrecognized control characters found in text");
            }
        }
    
        [Test]
        public void TestWriteOnly()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(
                    SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBeanWriteOnly).FullName);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;
    
            Object theEvent = new SupportBeanWriteOnly();
    
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean eventBean = listener.AssertOneGetNewAndReset();
    
            Assert.AreSame(theEvent, eventBean.Underlying);
    
            EventType type = stmt.EventType;

            Assert.AreEqual(0, type.PropertyNames.Length);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestCaseSensitive()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.typeof(EventMetaConfig)PropertyResolutionStyle =
                PropertyResolutionStyle.CASE_SENSITIVE;

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var stmt = _epService.EPAdministrator.CreateEPL("select MYPROPERTY, myproperty, myProperty from " + typeof(SupportBeanDupProperty).FullName);
            var listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;

            var uevent = new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower");

            _epService.EPRuntime.SendEvent(uevent);
            EventBean result = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual(uevent.MYPROPERTY, result.Get("MYPROPERTY"));
            Assert.AreEqual(uevent.myproperty, result.Get("myproperty"));
            Assert.AreEqual(uevent.myProperty, result.Get("myProperty"));

            try {
                _epService.EPAdministrator.CreateEPL(
                        "select MyProPerty from " + typeof(SupportBeanDupProperty).FullName);
                Assert.Fail();
            } catch (EPException ex) {// expected
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestCaseInsensitive()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
    
            configuration.EngineDefaults.typeof(EventMetaConfig)PropertyResolutionStyle =
                    PropertyResolutionStyle.CASE_INSENSITIVE;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select MYPROPERTY, myproperty, myProperty, MyProperty from "
                            + typeof(SupportBeanDupProperty).FullName);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;

            var uevent = new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower");
            _epService.EPRuntime.SendEvent(uevent);

            EventBean result = listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result.EventType.PropertyNames.Length, 4);
            Assert.AreEqual(result.Get("MYPROPERTY"), "upper");
            Assert.AreEqual(result.Get("MyProperty"), "uppercamel");
            Assert.AreEqual(result.Get("myProperty"), "lowercamel");
            Assert.AreEqual(result.Get("myproperty"), "lower");

   
            stmt = _epService.EPAdministrator.CreateEPL(
                    "select " + "NESTED.NESTEDVALUE as val1, "
                    + "ARRAYPROPERTY[0] as val2, " + "MAPPED('keyOne') as val3, "
                    + "INDEXED[0] as val4 " + " from "
                    + typeof(SupportBeanComplexProps).FullName);
            stmt.Events += listener.Update;
            _epService.EPRuntime.SendEvent(
                    SupportBeanComplexProps.MakeDefaultBean());
            EventBean theEvent = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual("NestedValue", theEvent.Get("val1"));
            Assert.AreEqual(10, theEvent.Get("val2"));
            Assert.AreEqual("valueOne", theEvent.Get("val3"));
            Assert.AreEqual(1, theEvent.Get("val4"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestAccessorStyleGlobalPublic()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
    
            configuration.EngineDefaults.EventMetaConfig.DefaultAccessorStyle =
                    AccessorStyleEnum.PUBLIC;
            configuration.AddEventType("SupportLegacyBean", typeof(SupportLegacyBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select fieldLegacyVal from SupportLegacyBean");
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;
    
            SupportLegacyBean theEvent = new SupportLegacyBean("E1");
    
            theEvent.fieldLegacyVal = "val1";
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual("val1", listener.AssertOneGetNewAndReset().Get("fieldLegacyVal"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestCaseDistinctInsensitive() 
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
    
            configuration.EngineDefaults.typeof(EventMetaConfig)PropertyResolutionStyle =
                    PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select MYPROPERTY, myproperty, myProperty from " + typeof(SupportBeanDupProperty).FullName);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(
                    new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
            EventBean result = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual("upper", result.Get("MYPROPERTY"));
            Assert.AreEqual("lower", result.Get("myproperty"));
            Assert.AreEqual("lowercamel", result.Get("myProperty"));
    
            try {
                _epService.EPAdministrator.CreateEPL(
                        "select MyProPerty from " + typeof(SupportBeanDupProperty).FullName);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(
                        "Unexpected exception starting statement: Unable to determine which property to use for \"MyProPerty\" because more than one property matched [select MyProPerty from com.espertech.esper.support.bean.SupportBeanDupProperty]",
                        ex.Message);
                // expected
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestCaseInsensitiveEngineDefault()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
    
            configuration.EngineDefaults.typeof(EventMetaConfig)PropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.AddEventType("Bean", typeof(SupportBean));
    
            TryCaseInsensitive(configuration,
                    "select THESTRING, INTPRIMITIVE from Bean where THESTRING='A'",
                    "THESTRING", "INTPRIMITIVE");
            TryCaseInsensitive(configuration,
                    "select ThEsTrInG, INTprimitIVE from Bean where THESTRing='A'",
                    "ThEsTrInG", "INTprimitIVE");
        }
    
        [Test]
        public void TestCaseInsensitiveTypeConfig()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeLegacy legacyDef = new ConfigurationEventTypeLegacy();
    
            legacyDef.PropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.AddEventType("Bean", typeof(SupportBean).FullName, legacyDef);
    
            TryCaseInsensitive(configuration,
                    "select theSTRING, INTPRIMITIVE from Bean where THESTRING='A'",
                    "theSTRING", "INTPRIMITIVE");
            TryCaseInsensitive(configuration,
                    "select THEsTrInG, INTprimitIVE from Bean where theSTRing='A'",
                    "THEsTrInG", "INTprimitIVE");
        }
    
        private void TryCaseInsensitive(Configuration configuration, String stmtText, String propOneName, String propTwoName)
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("A", 10));
            EventBean result = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual("A", result.Get(propOneName));
            Assert.AreEqual(10, result.Get(propTwoName));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void TryEnumItselfReserved()
        {
            _epService.EPAdministrator.Configuration.AddEventType<LocalEventWithGroup>();
            _epService.EPAdministrator.Configuration.AddImport<GROUP>();
            _epService.EPAdministrator.CreateEPL("select * from LocalEventWithGroup(`GROUP`=`GROUP`.FOO)");
        }

        public class LocalEventWithEnum
        {
            public LocalEventEnum LocalEventEnum { get; private set; }
            public LocalEventWithEnum(LocalEventEnum localEventEnum)
            {
                LocalEventEnum = localEventEnum;
            }
        }

        public enum LocalEventEnum
        {
            NEW
        }

        public class LocalEventWithGroup
        {
            public LocalEventWithGroup(GROUP groupGroup)
            {
                GROUP = groupGroup;
            }

            public GROUP GROUP { get; private set; }
        }

        public enum GROUP
        {
            FOO,
            BAR
        }
    }
}
