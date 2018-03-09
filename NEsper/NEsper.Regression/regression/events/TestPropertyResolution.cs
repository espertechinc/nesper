///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
	public class TestPropertyResolution
    {
	    private EPServiceProvider epService;

        [Test]
	    public void TestReservedKeywordEscape() {
	        epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        epService.Initialize();

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }
	        epService.EPAdministrator.Configuration.AddEventType("SomeKeywords", typeof(SupportBeanReservedKeyword));
	        epService.EPAdministrator.Configuration.AddEventType("Order", typeof(SupportBeanReservedKeyword));
	        SupportUpdateListener listener = new SupportUpdateListener();

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select `seconds`, `order` from SomeKeywords");
	        stmt.Events += listener.Update;

	        object theEvent = new SupportBeanReservedKeyword(1, 2);
	        epService.EPRuntime.SendEvent(theEvent);
	        EventBean eventBean = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(1, eventBean.Get("seconds"));
	        Assert.AreEqual(2, eventBean.Get("order"));

	        stmt.Dispose();
	        stmt = epService.EPAdministrator.CreateEPL("select * from `Order`");
	        stmt.Events += listener.Update;

	        epService.EPRuntime.SendEvent(theEvent);
	        eventBean = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(1, eventBean.Get("seconds"));
	        Assert.AreEqual(2, eventBean.Get("order"));

	        stmt.Dispose();
	        stmt = epService.EPAdministrator.CreateEPL("select timestamp.`hour` as val from SomeKeywords");
	        stmt.Events += listener.Update;

	        SupportBeanReservedKeyword bean = new SupportBeanReservedKeyword(1, 2);
	        bean.Timestamp = new SupportBeanReservedKeyword.Inner();
	        bean.Timestamp.Hour = 10;
	        epService.EPRuntime.SendEvent(bean);
	        eventBean = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(10, eventBean.Get("val"));

	        // test back-tick with spaces etc
	        IDictionary<string, object> defType = new Dictionary<string, object>();
	        defType.Put("candidate book", typeof(string));
	        defType.Put("XML Message UnderlyingType", typeof(string));
	        defType.Put("select", typeof(int));
	        defType.Put("children's books", typeof(int[]));
	        defType.Put("my <> map", typeof(IDictionary<string, object>));
	        epService.EPAdministrator.Configuration.AddEventType("MyType", defType);
	        epService.EPAdministrator.CreateEPL("select `candidate book` as c0, `XML Message UnderlyingType` as c1, `select` as c2, `children's books`[0] as c3, `my <> map`('xx') as c4 from MyType").Events += listener.Update;

	        IDictionary<string, object> defValues = new Dictionary<string, object>();
	        defValues.Put("candidate book", "Enders Game");
	        defValues.Put("XML Message UnderlyingType", "book");
	        defValues.Put("select", 100);
	        defValues.Put("children's books", new int[] {50, 51});
	        defValues.Put("my <> map", Collections.SingletonMap("xx", "abc"));
	        epService.EPRuntime.SendEvent(defValues, "MyType");
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4".SplitCsv(), new object[] {"Enders Game", "book", 100, 50, "abc"});

	        try {
	            epService.EPAdministrator.CreateEPL("select `select` from " + typeof(SupportBean).FullName);
	            Assert.Fail();
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'select': Property named 'select' is not valid in any stream [");
	        }

	        try {
	            epService.EPAdministrator.CreateEPL("select `ab cd` from " + typeof(SupportBean).FullName);
	            Assert.Fail();
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'ab cd': Property named 'ab cd' is not valid in any stream [");
	        }

	        // test resolution as nested property
	        epService.EPAdministrator.CreateEPL("create schema MyEvent as (customer string, `from` string)");
	        epService.EPAdministrator.CreateEPL("insert into DerivedStream select customer,`from` from MyEvent");
	        epService.EPAdministrator.CreateEPL("create window TheWindow#firstunique(customer,`from`) as DerivedStream");
	        epService.EPAdministrator.CreateEPL("on pattern [a=TheWindow -> timer:interval(12 hours)] as s0 delete from TheWindow as s1 where s0.a.`from`=s1.`from`");

	        // test escape in column name
	        epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select TheString as `order`, TheString as `price.for.goods` from SupportBean");
	        stmtTwo.Events += listener.Update;
	        Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("order"));
	        Assert.AreEqual("price.for.goods", stmtTwo.EventType.PropertyDescriptors[1].PropertyName);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        IDictionary<string, object> @out = (IDictionary<string, object>) listener.AssertOneGetNew().Underlying;
	        Assert.AreEqual("E1", @out.Get("order"));
	        Assert.AreEqual("E1", @out.Get("price.for.goods"));

	        // try control character
	        TryInvalidControlCharacter(listener.AssertOneGetNew());

	        // try enum with keyword
	        TryEnumWithKeyword();

	        TryEnumItselfReserved();

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestWriteOnly() {
	        epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBeanWriteOnly).FullName);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.Events += listener.Update;

	        object theEvent = new SupportBeanWriteOnly();
	        epService.EPRuntime.SendEvent(theEvent);
	        EventBean eventBean = listener.AssertOneGetNewAndReset();
	        Assert.AreSame(theEvent, eventBean.Underlying);

	        EventType type = stmt.EventType;
	        Assert.AreEqual(0, type.PropertyNames.Length);

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestCaseSensitive() {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_SENSITIVE;

            epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select MYPROPERTY, myproperty, myProperty from " + typeof(SupportBeanDupProperty).FullName);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.Events += listener.Update;

            var uevent = new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower");

            epService.EPRuntime.SendEvent(uevent);
	        EventBean result = listener.AssertOneGetNewAndReset();

            Assert.AreEqual(uevent.MYPROPERTY, result.Get("MYPROPERTY"));
            Assert.AreEqual(uevent.myproperty, result.Get("myproperty"));
            Assert.AreEqual(uevent.myProperty, result.Get("myProperty"));

            try {
	            epService.EPAdministrator.CreateEPL("select MyProPerty from " + typeof(SupportBeanDupProperty).FullName);
	            Assert.Fail();
	        } catch (EPException ex) {
	            // expected
	        }
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestCaseInsensitive() {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select MYPROPERTY, myproperty, myProperty, MyProperty from " + typeof(SupportBeanDupProperty).FullName);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.Events += listener.Update;

	        epService.EPRuntime.SendEvent(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
	        EventBean result = listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result.EventType.PropertyNames.Length, 4);
            Assert.AreEqual(result.Get("MYPROPERTY"), "upper");
            Assert.AreEqual(result.Get("MyProperty"), "uppercamel");
            Assert.AreEqual(result.Get("myProperty"), "lowercamel");
            Assert.AreEqual(result.Get("myproperty"), "lower");

	        stmt = epService.EPAdministrator.CreateEPL("select " +
	                "NESTED.NESTEDVALUE as val1, " +
	                "ARRAYPROPERTY[0] as val2, " +
	                "MAPPED('keyOne') as val3, " +
	                "INDEXED[0] as val4 " +
	                " from " + typeof(SupportBeanComplexProps).FullName);
	        stmt.Events += listener.Update;
	        epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
	        EventBean theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("NestedValue", theEvent.Get("val1"));
	        Assert.AreEqual(10, theEvent.Get("val2"));
	        Assert.AreEqual("valueOne", theEvent.Get("val3"));
	        Assert.AreEqual(1, theEvent.Get("val4"));

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestAccessorStyleGlobalPublic() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.EventMeta.DefaultAccessorStyle = AccessorStyleEnum.PUBLIC;
	        configuration.AddEventType("SupportLegacyBean", typeof(SupportLegacyBean));
	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select fieldLegacyVal from SupportLegacyBean");
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.Events += listener.Update;

	        SupportLegacyBean theEvent = new SupportLegacyBean("E1");
	        theEvent.fieldLegacyVal = "val1";
	        epService.EPRuntime.SendEvent(theEvent);
	        Assert.AreEqual("val1", listener.AssertOneGetNewAndReset().Get("fieldLegacyVal"));

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestCaseDistinctInsensitive() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE;

	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL("select MYPROPERTY, myproperty, myProperty from " + typeof(SupportBeanDupProperty).FullName);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.Events += listener.Update;

	        epService.EPRuntime.SendEvent(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
	        EventBean result = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("upper", result.Get("MYPROPERTY"));
	        Assert.AreEqual("lower", result.Get("myproperty"));
	        Assert.IsTrue(result.Get("myProperty").Equals("lowercamel") || result.Get("myProperty").Equals("uppercamel")); // JDK6 versus JDK7 JavaBean inspector

	        try {
	            epService.EPAdministrator.CreateEPL("select MyProPerty from " + typeof(SupportBeanDupProperty).FullName);
	            Assert.Fail();
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Unexpected exception starting statement: Unable to determine which property to use for \"MyProPerty\" because more than one property matched [");
	            // expected
	        }

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestCaseInsensitiveEngineDefault() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	        configuration.AddEventType("Bean", typeof(SupportBean));

	        TryCaseInsensitive(configuration, "select THESTRING, INTPRIMITIVE from Bean where THESTRING='A'", "THESTRING", "INTPRIMITIVE");
	        TryCaseInsensitive(configuration, "select ThEsTrInG, INTprimitIVE from Bean where THESTRing='A'", "ThEsTrInG", "INTprimitIVE");
	    }

        [Test]
	    public void TestCaseInsensitiveTypeConfig() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        ConfigurationEventTypeLegacy legacyDef = new ConfigurationEventTypeLegacy();
	        legacyDef.PropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
	        configuration.AddEventType("Bean", typeof(SupportBean), legacyDef);

	        TryCaseInsensitive(configuration, "select theSTRING, INTPRIMITIVE from Bean where THESTRING='A'", "theSTRING", "INTPRIMITIVE");
	        TryCaseInsensitive(configuration, "select THEsTrInG, INTprimitIVE from Bean where theSTRing='A'", "THEsTrInG", "INTprimitIVE");
	    }

	    private void TryCaseInsensitive(Configuration configuration, string stmtText, string propOneName, string propTwoName) {
	        epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.Events += listener.Update;

	        epService.EPRuntime.SendEvent(new SupportBean("A", 10));
	        EventBean result = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("A", result.Get(propOneName));
	        Assert.AreEqual(10, result.Get(propTwoName));
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

	    private void TryEnumWithKeyword() {
	        epService.EPAdministrator.Configuration.AddEventType(typeof(LocalEventWithEnum));
	        epService.EPAdministrator.Configuration.AddImport(typeof(LocalEventEnum));
	        epService.EPAdministrator.CreateEPL("select * from LocalEventWithEnum(localEventEnum=LocalEventEnum.`NEW`)");
	    }

	    private void TryInvalidControlCharacter(EventBean eventBean) {
	        try {
	            eventBean.Get("a\u008F");
	            Assert.Fail();
	        } catch (PropertyAccessException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Property named 'a\u008F' is not a valid property name for this type");
	        }
	    }

	    private void TryEnumItselfReserved() {
	        epService.EPAdministrator.Configuration.AddEventType(typeof(LocalEventWithGroup));
	        epService.EPAdministrator.Configuration.AddImport(typeof(GROUP));
	        epService.EPAdministrator.CreateEPL("select * from LocalEventWithGroup(`GROUP`=`GROUP`.FOO)");
	    }

	    public class LocalEventWithEnum {
	        public LocalEventWithEnum(LocalEventEnum localEventEnum) {
	            this.LocalEventEnum = localEventEnum;
	        }

	        public LocalEventEnum LocalEventEnum { get; private set; }
	    }

        public enum LocalEventEnum
        {
            NEW
        }

        public class LocalEventWithGroup
        {
            public LocalEventWithGroup(GROUP GROUP)
            {
                this.GROUP = GROUP;
            }

            public GROUP GROUP { get; private set; }
        }

        public enum GROUP
        {
            FOO,
            BAR
        }
	}
} // end of namespace
