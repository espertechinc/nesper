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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestVariantStreamDefault
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerOne;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listenerOne = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listenerOne = null;
        }

        [Test]
        public void TestSingleColumnConversion()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantStream", typeof(SupportBeanVariantStream));
            _epService.EPAdministrator.Configuration.AddImport(GetType().FullName);

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBean");
            variant.AddEventTypeName("SupportBeanVariantStream");
            _epService.EPAdministrator.Configuration.AddVariantStream("AllEvents", variant);

            _epService.EPAdministrator.CreateEPL("insert into AllEvents select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("create window MainEventWindow#length(10000) as AllEvents");
            _epService.EPAdministrator.CreateEPL("insert into MainEventWindow select " + GetType().Name + ".PreProcessEvent(event) from AllEvents as event");

            EPStatement statement = _epService.EPAdministrator.CreateEPL("select * from MainEventWindow where TheString = 'E'");
            statement.AddEventHandlerWithReplay(new SupportUpdateListener().Update);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
        }

        public static Object PreProcessEvent(Object o)
        {
            return new SupportBean("E2", 0);
        }

        [Test]
        public void TestCoercionBoxedTypeMatch()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantStream", typeof(SupportBeanVariantStream));

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBean");
            variant.AddEventTypeName("SupportBeanVariantStream");
            _epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStream", variant);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyVariantStream");
            stmt.Events += _listenerOne.Update;
            EventType typeSelectAll = stmt.EventType;
            AssertEventTypeDefault(typeSelectAll);
            Assert.AreEqual(typeof(Object), stmt.EventType.UnderlyingType);

            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBeanVariantStream");

            // try wildcard
            Object eventOne = new SupportBean("E0", -1);
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, _listenerOne.AssertOneGetNewAndReset().Underlying);

            Object eventTwo = new SupportBeanVariantStream("E1");
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, _listenerOne.AssertOneGetNewAndReset().Underlying);

            stmt.Dispose();
            String fields = "TheString,BoolBoxed,IntPrimitive,LongPrimitive,DoublePrimitive,EnumValue";
            stmt = _epService.EPAdministrator.CreateEPL("select " + fields + " from MyVariantStream");
            stmt.Events += _listenerOne.Update;
            AssertEventTypeDefault(stmt.EventType);

            // coerces to the higher resolution type, accepts boxed versus not boxed
            _epService.EPRuntime.SendEvent(new SupportBeanVariantStream("s1", true, 1, 20, 30, SupportEnum.ENUM_VALUE_1));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new Object[] { "s1", true, 1, 20L, 30d, SupportEnum.ENUM_VALUE_1 });

            SupportBean bean = new SupportBean("s2", 99);
            bean.LongPrimitive = 33;
            bean.DoublePrimitive = 50;
            bean.EnumValue = SupportEnum.ENUM_VALUE_3;
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new Object[] { "s2", null, 99, 33L, 50d, SupportEnum.ENUM_VALUE_3 });

            // make sure a property is not known since the property is not found on SupportBeanVariantStream
            try
            {
                _epService.EPAdministrator.CreateEPL("select CharBoxed from MyVariantStream");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'CharBoxed': Property named 'CharBoxed' is not valid in any stream [select CharBoxed from MyVariantStream]", ex.Message);
            }

            // try dynamic property: should exists but not show up as a declared property
            stmt.Dispose();
            fields = "v1,v2,v3";
            stmt = _epService.EPAdministrator.CreateEPL("select LongBoxed? as v1,CharBoxed? as v2,DoubleBoxed? as v3 from MyVariantStream");
            stmt.Events += _listenerOne.Update;
            AssertEventTypeDefault(typeSelectAll);  // asserts prior "select *" event type

            bean = new SupportBean();
            bean.LongBoxed = 33L;
            bean.CharBoxed = 'a';
            bean.DoubleBoxed = Double.NaN;
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new Object[] { 33L, 'a', Double.NaN });

            _epService.EPRuntime.SendEvent(new SupportBeanVariantStream("s2"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new Object[] { null, null, null });
        }

        [Test]
        public void TestSuperTypesInterfaces()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantOne", typeof(SupportBeanVariantOne));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantTwo", typeof(SupportBeanVariantTwo));

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBeanVariantOne");
            variant.AddEventTypeName("SupportBeanVariantTwo");
            _epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStream", variant);
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBeanVariantOne");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBeanVariantTwo");

            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyVariantStream");
            stmt.Events += _listenerOne.Update;
            EventType eventType = stmt.EventType;

            var expected = "P0,P1,P2,P3,P4,P5,Indexed,Mapped,Inneritem".Split(',');
            var propertyNames = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
            Assert.AreEqual(typeof(ISupportBaseAB), eventType.GetPropertyType("P0"));
            Assert.AreEqual(typeof(ISupportAImplSuperG), eventType.GetPropertyType("P1"));
            Assert.AreEqual(typeof(object), eventType.GetPropertyType("P2"));
            Assert.AreEqual(typeof(IList<object>), eventType.GetPropertyType("P3"));
            Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("P4"));
            Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("P5"));
            Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("Indexed"));
            Assert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("Mapped"));
            Assert.AreEqual(typeof(SupportBeanVariantOne.SupportBeanVariantOneInner), eventType.GetPropertyType("Inneritem"));

            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select P0,P1,P2,P3,P4,P5,Indexed[0] as P6,indexArr[1] as P7,mappedKey('a') as P8,Inneritem as P9,Inneritem.val as P10 from MyVariantStream");
            stmt.Events += _listenerOne.Update;
            eventType = stmt.EventType;
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("P6"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("P7"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("P8"));
            Assert.AreEqual(typeof(SupportBeanVariantOne.SupportBeanVariantOneInner), eventType.GetPropertyType("P9"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("P10"));

            SupportBeanVariantOne ev1 = new SupportBeanVariantOne();
            _epService.EPRuntime.SendEvent(ev1);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), "P6,P7,P8,P9,P10".Split(','), new Object[] { 1, 2, "val1", ev1.Inneritem, ev1.Inneritem.Val });

            SupportBeanVariantTwo ev2 = new SupportBeanVariantTwo();
            _epService.EPRuntime.SendEvent(ev2);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), "P6,P7,P8,P9,P10".Split(','), new Object[] { 10, 20, "val2", ev2.Inneritem, ev2.Inneritem.Val });
        }

        private void AssertEventTypeDefault(EventType eventType)
        {
            var expected = "TheString,BoolBoxed,IntPrimitive,LongPrimitive,DoublePrimitive,EnumValue".Split(',');
            var propertyNames = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("BoolBoxed"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("LongPrimitive"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("DoublePrimitive"));
            Assert.AreEqual(typeof(SupportEnum?), eventType.GetPropertyType("EnumValue"));
            foreach (String expectedProp in expected)
            {
                Assert.NotNull(eventType.GetGetter(expectedProp));
                Assert.IsTrue(eventType.IsProperty(expectedProp));
            }

            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("TheString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("BoolBoxed", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("IntPrimitive", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("LongPrimitive", typeof(long?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("DoublePrimitive", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("EnumValue", typeof(SupportEnum?), null, false, false, false, false, false),
            }, eventType.PropertyDescriptors);
        }

        [Test]
        public void TestNamedWin()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantStream", typeof(SupportBeanVariantStream));

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBeanVariantStream");
            variant.AddEventTypeName("SupportBean");
            _epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStream", variant);

            // test named window
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window MyVariantWindow#unique(TheString) as select * from MyVariantStream");
            stmt.Events += _listenerOne.Update;
            _epService.EPAdministrator.CreateEPL("insert into MyVariantWindow select * from MyVariantStream");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBeanVariantStream");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBean");

            Object eventOne = new SupportBean("E1", -1);
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, _listenerOne.AssertOneGetNewAndReset().Underlying);

            Object eventTwo = new SupportBeanVariantStream("E2");
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, _listenerOne.AssertOneGetNewAndReset().Underlying);

            Object eventThree = new SupportBean("E2", -1);
            _epService.EPRuntime.SendEvent(eventThree);
            Assert.AreSame(eventThree, _listenerOne.LastNewData[0].Underlying);
            Assert.AreSame(eventTwo, _listenerOne.LastOldData[0].Underlying);
            _listenerOne.Reset();

            Object eventFour = new SupportBeanVariantStream("E1");
            _epService.EPRuntime.SendEvent(eventFour);
            Assert.AreSame(eventFour, _listenerOne.LastNewData[0].Underlying);
            Assert.AreSame(eventOne, _listenerOne.LastOldData[0].Underlying);
            _listenerOne.Reset();
        }

        [Test]
        public void TestPatternSubquery()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantStream", typeof(SupportBeanVariantStream));

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBeanVariantStream");
            variant.AddEventTypeName("SupportBean");
            _epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStream", variant);

            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBeanVariantStream");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from SupportBean");

            // test pattern
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from pattern [a=MyVariantStream -> b=MyVariantStream]");
            stmt.Events += _listenerOne.Update;
            Object[] events = { new SupportBean("E1", -1), new SupportBeanVariantStream("E2") };
            _epService.EPRuntime.SendEvent(events[0]);
            _epService.EPRuntime.SendEvent(events[1]);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), "a,b".Split(','), events);

            // test subquery
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean_A as a where Exists(select * from MyVariantStream#lastevent as b where b.TheString=a.id)");
            stmt.Events += _listenerOne.Update;
            events = new Object[] { new SupportBean("E1", -1), new SupportBeanVariantStream("E2"), new SupportBean_A("E2") };

            _epService.EPRuntime.SendEvent(events[0]);
            _epService.EPRuntime.SendEvent(events[2]);
            Assert.IsFalse(_listenerOne.IsInvoked);

            _epService.EPRuntime.SendEvent(events[1]);
            _epService.EPRuntime.SendEvent(events[2]);
            Assert.IsTrue(_listenerOne.IsInvoked);
        }

        [Test]
        public void TestDynamicMapType()
        {
            IDictionary<String, Object> types = new Dictionary<String, Object>();
            types["someprop"] = typeof(string);

            _epService.EPAdministrator.Configuration.AddEventType("MyEvent", types);
            _epService.EPAdministrator.Configuration.AddEventType("MySecondEvent", types);

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("MyEvent");
            variant.AddEventTypeName("MySecondEvent");
            _epService.EPAdministrator.Configuration.AddVariantStream("MyVariant", variant);

            _epService.EPAdministrator.CreateEPL("insert into MyVariant select * from MyEvent");
            _epService.EPAdministrator.CreateEPL("insert into MyVariant select * from MySecondEvent");

            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyVariant");
            stmt.Events += _listenerOne.Update;
            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyEvent");
            Assert.NotNull(_listenerOne.AssertOneGetNewAndReset());
            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MySecondEvent");
            Assert.NotNull(_listenerOne.AssertOneGetNewAndReset());
        }

        [Test]
        public void TestInvalidInsertInto()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantStream", typeof(SupportBeanVariantStream));

            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBean");
            variant.AddEventTypeName("SupportBeanVariantStream");
            _epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStream", variant);

            SupportMessageAssertUtil.TryInvalid(_epService, "insert into MyVariantStream select * from " + Name.Of<SupportBean_A>(),
                "Error starting statement: Selected event type is not a valid event type of the variant stream 'MyVariantStream'");

            SupportMessageAssertUtil.TryInvalid(_epService, "insert into MyVariantStream select intPrimitive as k0 from " + Name.Of<SupportBean>(),
                "Error starting statement: Selected event type is not a valid event type of the variant stream 'MyVariantStream' ");

        }

        [Test]
        public void TestInvalidConfig()
        {
            ConfigurationVariantStream config = new ConfigurationVariantStream();
            TryInvalidConfig("abc", config, "Invalid variant stream configuration, no event type name has been added and default type variance requires at least one type, for name 'abc'");

            config.AddEventTypeName("dummy");
            TryInvalidConfig("abc", config, "Event type by name 'dummy' could not be found for use in variant stream configuration by name 'abc'");
            _epService.EPAdministrator.Configuration.AddEventType("MyEvent", typeof(SupportBean));
        }

        private void TryInvalidConfig(String name, ConfigurationVariantStream config, String message)
        {
            try
            {
                _epService.EPAdministrator.Configuration.AddVariantStream(name, config);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}