///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using Newtonsoft.Json.Linq;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInsertIntoPopulateUnderlying
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportSubscriber _subscriber;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();

            var legacy = new ConfigurationEventTypeLegacy();
            legacy.FactoryMethod = "GetInstance";
            configuration.AddEventType("SupportBeanString", typeof(SupportBeanString).FullName, legacy);
            configuration.AddImport(typeof(TestInsertIntoPopulateUnderlying).Namespace);

            legacy = new ConfigurationEventTypeLegacy();
            legacy.FactoryMethod = typeof(SupportSensorEventFactory).FullName + ".GetInstance";
            configuration.AddEventType("SupportSensorEvent", typeof(SupportSensorEvent).FullName, legacy);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);
            }
            _listener = new SupportUpdateListener();
            _subscriber = new SupportSubscriber();

            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportTemperatureBean", typeof(SupportTemperatureBean));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanComplexProps", typeof(SupportBeanComplexProps));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanInterfaceProps", typeof(SupportBeanInterfaceProps));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanErrorTestingOne", typeof(SupportBeanErrorTestingOne));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanErrorTestingTwo", typeof(SupportBeanErrorTestingTwo));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanReadOnly", typeof(SupportBeanReadOnly));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBeanArrayCollMap", typeof(SupportBeanArrayCollMap));
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_N>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanObject", typeof(SupportBeanObject));
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));

            IDictionary<String, Object> mymapDef = new Dictionary<string, object>();
            mymapDef.Put("AnInt", typeof(int));
            mymapDef.Put("IntBoxed", typeof(int));
            mymapDef.Put("FloatBoxed", typeof(float));
            mymapDef.Put("IntArr", typeof(int[]));
            mymapDef.Put("MapProp", typeof(IDictionary<string, string>));
            mymapDef.Put("IsAImpl", typeof(ISupportAImpl));
            mymapDef.Put("IsBImpl", typeof(ISupportBImpl));
            mymapDef.Put("IsGImpl", typeof(ISupportAImplSuperGImpl));
            mymapDef.Put("IsABImpl", typeof(ISupportBaseABImpl));
            mymapDef.Put("Nested", typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested));
            _epService.EPAdministrator.Configuration.AddEventType("MyMap", mymapDef);

            var xml = new ConfigurationEventTypeXMLDOM();
            xml.RootElementName = "abc";
            _epService.EPAdministrator.Configuration.AddEventType("xmltype", xml);
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.EndTest();
            }
            _listener = null;
            _subscriber = null;
        }

        [Test]
        public void TestCtor()
        {
            // simple type and null values
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorOne", typeof(SupportBeanCtorOne));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorTwo", typeof(SupportBeanCtorTwo));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));

            var eplOne =
                "insert into SupportBeanCtorOne select TheString, IntBoxed, IntPrimitive, BoolPrimitive from SupportBean";
            var stmtOne = _epService.EPAdministrator.CreateEPL(eplOne);
            stmtOne.Events += _listener.Update;

            SendReceive("E1", 2, true, 100);
            SendReceive("E2", 3, false, 101);
            SendReceive(null, 4, true, null);
            stmtOne.Dispose();

            // boxable type and null values
            var eplTwo = "insert into SupportBeanCtorOne select TheString, null, IntBoxed from SupportBean";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.Events += _listener.Update;
            SendReceiveTwo("E1", 100);
            stmtTwo.Dispose();

            // test join wildcard
            var eplThree =
                "insert into SupportBeanCtorTwo select * from SupportBean_ST0#lastevent, SupportBean_ST1#lastevent";
            var stmtThree = _epService.EPAdministrator.CreateEPL(eplThree);
            stmtThree.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 2));
            var theEvent = (SupportBeanCtorTwo) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.NotNull(theEvent.St0);
            Assert.NotNull(theEvent.St1);
            stmtThree.Dispose();

            // test (should not use column names)
            var eplFour = "insert into SupportBeanCtorOne(TheString, IntPrimitive) select 'E1', 5 from SupportBean";
            var stmtFour = _epService.EPAdministrator.CreateEPL(eplFour);
            stmtFour.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("x", -1));
            var eventOne = (SupportBeanCtorOne) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", eventOne.TheString);
            Assert.AreEqual(99, eventOne.IntPrimitive);
            Assert.AreEqual((int?) 5, eventOne.IntBoxed);

            // test Ctor accepting same types
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithCtorSameType));
            var epl =
                "insert into MyEventWithCtorSameType select c1,c2 from SupportBean(TheString='b1')#lastevent as c1, SupportBean(TheString='b2')#lastevent as c2";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("b1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("b2", 2));
            var result = (MyEventWithCtorSameType) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, result.B1.IntPrimitive);
            Assert.AreEqual(2, result.B2.IntPrimitive);
        }

        [Test]
        public void TestCtorWithPattern()
        {

            // simple type and null values
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorThree", typeof(SupportBeanCtorThree));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));

            // Test valid case of array insert
            var epl = "insert into SupportBeanCtorThree select s, e FROM PATTERN [" +
                      "every s=SupportBean_ST0 -> [2] e=SupportBean_ST1]";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E0", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("E2", 3));
            var three = (SupportBeanCtorThree) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E0", three.St0.Id);
            Assert.AreEqual(2, three.St1.Length);
            Assert.AreEqual("E1", three.St1[0].Id);
            Assert.AreEqual("E2", three.St1[1].Id);
        }

        [Test]
        public void TestBeanJoin()
        {
            // test wildcard
            var stmtTextOne =
                "insert into SupportBeanObject select * from SupportBean_N#lastevent as One, SupportBean_S0#lastevent as Two";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            var n1 = new SupportBean_N(1, 10, 100d, 1000d, true, true);
            _epService.EPRuntime.SendEvent(n1);
            var s01 = new SupportBean_S0(1);
            _epService.EPRuntime.SendEvent(s01);
            var theEvent = (SupportBeanObject) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);

            // test select stream names
            stmtOne.Dispose();
            stmtTextOne =
                "insert into SupportBeanObject select One, Two from SupportBean_N#lastevent as One, SupportBean_S0#lastevent as Two";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(n1);
            _epService.EPRuntime.SendEvent(s01);
            theEvent = (SupportBeanObject) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
            stmtOne.Dispose();

            // test fully-qualified class name as target
            stmtTextOne = "insert into " + typeof(SupportBeanObject).FullName +
                          " select One, Two from SupportBean_N#lastevent as One, SupportBean_S0#lastevent as Two";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(n1);
            _epService.EPRuntime.SendEvent(s01);
            theEvent = (SupportBeanObject) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);

            // test local class and auto-import
            stmtOne.Dispose();
            stmtTextOne = "insert into `" + typeof(MyLocalTarget).FullName + "` select 1 as Value from SupportBean_N";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(n1);
            var eventLocal = (MyLocalTarget) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, eventLocal.Value);
        }

        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorOne", typeof(SupportBeanCtorOne));

            var text = "insert into SupportBeanCtorOne select 1 from SupportBean";
            TryInvalid(
                "Error starting statement: Failed to find a suitable constructor for type 'SupportBeanCtorOne': Could not find constructor in class '" +
                Name.Of<SupportBeanCtorOne>() + "' with matching parameter number and expected parameter type(s) '" +
                Name.Clean<int>() +
                "' (nearest matching constructor taking type(s) 'System.String') [insert into SupportBeanCtorOne select 1 from SupportBean]",
                text);

            text = "insert into SupportBean(IntPrimitive) select 1L from SupportBean";
            TryInvalid(
                "Error starting statement: Invalid assignment of column 'IntPrimitive' of type '" + Name.Of<long>() +
                "' to event property 'IntPrimitive' typed as '" + Name.Of<int>(false) +
                "', column and parameter types mismatch [insert into SupportBean(IntPrimitive) select 1L from SupportBean]",
                text);

            text = "insert into SupportBean(IntPrimitive) select null from SupportBean";
            TryInvalid(
                "Error starting statement: Invalid assignment of column 'IntPrimitive' of null type to event property 'IntPrimitive' typed as '" +
                Name.Of<int>(false) +
                "', nullable type mismatch [insert into SupportBean(IntPrimitive) select null from SupportBean]", text);

            text = "insert into SupportBeanReadOnly select 'a' as geom from SupportBean";
            TryInvalid(
                "Error starting statement: Failed to find a suitable constructor for type 'SupportBeanReadOnly': Could not find constructor in class '" +
                Name.Of<SupportBeanReadOnly>() +
                "' with matching parameter number and expected parameter type(s) 'System.String' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly select 'a' as geom from SupportBean]",
                text);

            text = "insert into SupportBean select 3 as dummyField from SupportBean";
            TryInvalid(
                "Error starting statement: Column 'dummyField' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 as dummyField from SupportBean]",
                text);

            text = "insert into SupportBean select 3 from SupportBean";
            TryInvalid(
                "Error starting statement: Column '3' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 from SupportBean]",
                text);

            text = "insert into SupportBeanInterfaceProps(ISA) select IsBImpl from MyMap";
            TryInvalid(
                "Error starting statement: Invalid assignment of column 'ISA' of type '" + Name.Of<ISupportBImpl>() +
                "' to event property 'ISA' typed as '" + Name.Of<ISupportA>() +
                "', column and parameter types mismatch [insert into SupportBeanInterfaceProps(ISA) select IsBImpl from MyMap]",
                text);

            text = "insert into SupportBeanInterfaceProps(ISG) select IsABImpl from MyMap";
            TryInvalid(
                "Error starting statement: Invalid assignment of column 'ISG' of type '" +
                Name.Of<ISupportBaseABImpl>() + "' to event property 'ISG' typed as '" +
                Name.Of<ISupportAImplSuperG>() +
                "', column and parameter types mismatch [insert into SupportBeanInterfaceProps(ISG) select IsABImpl from MyMap]",
                text);

            text = "insert into SupportBean(dummy) select 3 from SupportBean";
            TryInvalid(
                "Error starting statement: Column 'dummy' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean(dummy) select 3 from SupportBean]",
                text);

            text = "insert into SupportBeanReadOnly(side) select 'E1' from MyMap";
            TryInvalid(
                "Error starting statement: Failed to find a suitable constructor for type 'SupportBeanReadOnly': Could not find constructor in class '" +
                Name.Of<SupportBeanReadOnly>() +
                "' with matching parameter number and expected parameter type(s) 'System.String' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly(side) select 'E1' from MyMap]",
                text);

            _epService.EPAdministrator.CreateEPL("insert into ABCStream select *, 1+1 from SupportBean");
            text = "insert into ABCStream(string) select 'E1' from MyMap";
            TryInvalid(
                "Error starting statement: Event type named 'ABCStream' has already been declared with differing column name or type information: Type by name 'ABCStream' is not a compatible type (target type underlying is '" + Name.Of<Pair<object, IDictionary<string, object>>>() + "') [insert into ABCStream(string) select 'E1' from MyMap]",
                text);

            text = "insert into xmltype select 1 from SupportBean";
            TryInvalid(
                "Error starting statement: Event type named 'xmltype' has already been declared with differing column name or type information: Type by name 'xmltype' is not a compatible type (target type underlying is 'System.Xml.XmlNode') [insert into xmltype select 1 from SupportBean]",
                text);

            text = "insert into MyMap(dummy) select 1 from SupportBean";
            TryInvalid(
                "Error starting statement: Event type named 'MyMap' has already been declared with differing column name or type information: Type by name 'MyMap' expects 10 properties but receives 1 properties [insert into MyMap(dummy) select 1 from SupportBean]",
                text);

            // setter throws exception
            var stmtTextOne = "insert into SupportBeanErrorTestingTwo(Value) select 'E1' from MyMap";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            var underlying = (SupportBeanErrorTestingTwo) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("default", underlying.Value);
            stmtOne.Dispose();

            // surprise - wrong type then defined
            stmtTextOne = "insert into SupportBean(IntPrimitive) select AnInt from MyMap";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            _listener.Reset();
            var map = new Dictionary<string, object>();
            map.Put("AnInt", "NotAnInt");
            _epService.EPRuntime.SendEvent(map, "MyMap");
            Assert.AreEqual(0, _listener.AssertOneGetNewAndReset().Get("IntPrimitive"));

            // ctor throws exception
            _epService.EPAdministrator.DestroyAllStatements();
            var stmtTextThree = "insert into SupportBeanCtorOne select 'E1' from SupportBean";
            var stmtThree = _epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += _listener.Update;
            try
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail(); // rethrowing handler registered
            }
            catch (EPException)
            {
                // expected
            }

            // allow automatic cast of same-type event
            _epService.EPAdministrator.CreateEPL("create schema MapOne as (prop1 string)");
            _epService.EPAdministrator.CreateEPL("create schema MapTwo as (prop1 string)");
            _epService.EPAdministrator.CreateEPL("insert into MapOne select * from MapTwo");
        }

        [Test]
        public void TestPopulateBeanSimple()
        {
            // test select column names
            var stmtTextOne = "insert into SupportBean select " +
                              "'E1' as TheString, 1 as IntPrimitive, 2 as IntBoxed, 3L as LongPrimitive," +
                              "null as LongBoxed, true as BoolPrimitive, " +
                              "'x' as CharPrimitive, 0xA as BytePrimitive, " +
                              "8.0f as FloatPrimitive, 9.0d as DoublePrimitive, " +
                              "0x05 as ShortPrimitive, SupportEnum.ENUM_VALUE_2 as EnumValue " +
                              " from MyMap";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);

            var stmtTextTwo = "select * from SupportBean";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmtTwo.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            var received = (SupportBean) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", received.TheString);
            EPAssertionUtil.AssertPropsPONO(
                received,
                "IntPrimitive,IntBoxed,LongPrimitive,LongBoxed,BoolPrimitive,CharPrimitive,BytePrimitive,FloatPrimitive,DoublePrimitive,ShortPrimitive,EnumValue"
                    .Split(','),
                new Object[]
                {
                    1,
                    2,
                    3L,
                    null,
                    true,
                    'x',
                    (byte) 10,
                    8f,
                    9d,
                    (short) 5,
                    SupportEnum.ENUM_VALUE_2
                });

            // test insert-into column names
            stmtOne.Dispose();
            stmtTwo.Dispose();
            _listener.Reset();
            stmtTextOne = "insert into SupportBean(TheString, IntPrimitive, IntBoxed, LongPrimitive," +
                          "LongBoxed, BoolPrimitive, CharPrimitive, BytePrimitive, FloatPrimitive, DoublePrimitive, " +
                          "ShortPrimitive, EnumValue) select " +
                          "'E1', 1, 2, 3L," +
                          "null, true, " +
                          "'x', 0xA, " +
                          "8.0f, 9.0d, " +
                          "0x05 as ShortPrimitive, SupportEnum.ENUM_VALUE_2 " +
                          " from MyMap";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            received = (SupportBean) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", received.TheString);
            EPAssertionUtil.AssertPropsPONO(
                received,
                "IntPrimitive,IntBoxed,LongPrimitive,LongBoxed,BoolPrimitive,CharPrimitive,BytePrimitive,FloatPrimitive,DoublePrimitive,ShortPrimitive,EnumValue"
                    .Split(','),
                new Object[]
                {
                    1,
                    2,
                    3L,
                    null,
                    true,
                    'x',
                    (byte) 10,
                    8f,
                    9d,
                    (short) 5,
                    SupportEnum.ENUM_VALUE_2
                });

            // test convert Integer boxed to Long boxed
            stmtOne.Dispose();
            _listener.Reset();
            stmtTextOne = "insert into SupportBean(LongBoxed, DoubleBoxed) select IntBoxed, FloatBoxed from MyMap";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            IDictionary<String, Object> vals = new Dictionary<String, Object>();
            vals.Put("IntBoxed", 4);
            vals.Put("FloatBoxed", 0f);
            _epService.EPRuntime.SendEvent(vals, "MyMap");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), "LongBoxed,DoubleBoxed".Split(','), new Object[]
                {
                    4L,
                    0d
                });
            _epService.EPAdministrator.DestroyAllStatements();

            // test new-to-map conversion
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithMapFieldSetter));
            var stmt = _epService.EPAdministrator.CreateEPL(
                "insert into MyEventWithMapFieldSetter(Id, TheMap) " +
                "select 'test' as Id, new {somefield = TheString} as TheMap from SupportBean");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<String, Object>) _listener.AssertOneGetNew().Get("TheMap"), "somefield".Split(','), "E1");
        }

        [Test]
        public void TestBeanWildcard()
        {
            IDictionary<String, Object> mapDef = new Dictionary<String, Object>();
            mapDef.Put("IntPrimitive", typeof(int));
            mapDef.Put("LongBoxed", typeof(long));
            mapDef.Put("TheString", typeof(String));
            mapDef.Put("BoolPrimitive", typeof(Boolean));
            _epService.EPAdministrator.Configuration.AddEventType("MySupportMap", mapDef);

            var stmtTextOne = "insert into SupportBean select * from MySupportMap";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            IDictionary<String, Object> vals = new Dictionary<String, Object>();
            vals.Put("IntPrimitive", 4);
            vals.Put("LongBoxed", 100L);
            vals.Put("TheString", "E1");
            vals.Put("BoolPrimitive", true);

            _epService.EPRuntime.SendEvent(vals, "MySupportMap");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(),
                "IntPrimitive,LongBoxed,TheString,BoolPrimitive".Split(','),
                new Object[]
                {
                    4,
                    100L,
                    "E1",
                    true
                });
        }

        [Test]
        public void TestPopulateBeanObjects()
        {
            // arrays and maps
            var stmtTextOne = "insert into SupportBeanComplexProps(ArrayProperty,ObjectArray,MapProperty) select " +
                              "IntArr,{10,20,30},MapProp from MyMap as m";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            var mymapVals = new Dictionary<string, object>();
            mymapVals.Put(
                "IntArr", new int[]
                {
                    -1,
                    -2
                });

            var inner = new Dictionary<string, string>();
            inner["mykey"] = "myval";
            mymapVals["MapProp"] = inner;
            _epService.EPRuntime.SendEvent(mymapVals, "MyMap");

            var theEvent = (SupportBeanComplexProps) _listener.AssertOneGetNewAndReset().Underlying;

            Assert.AreEqual(-2, theEvent.ArrayProperty[1]);
            Assert.AreEqual(20, theEvent.ObjectArray[1]);
            Assert.AreEqual("myval", theEvent.MapProperty.Get("mykey"));

            // inheritance
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanInterfaceProps(ISA,ISG) select " +
                          "IsAImpl,IsGImpl" +
                          " from MyMap";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            mymapVals = new Dictionary<String, Object>();
            mymapVals.Put("mapProp", inner);
            _epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            var eventTwo = (SupportBeanInterfaceProps) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(typeof(SupportBeanInterfaceProps), stmtOne.EventType.UnderlyingType);

            // object values from Map same type
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanComplexProps(Nested) select Nested from MyMap";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            mymapVals = new Dictionary<String, Object>();
            mymapVals.Put("Nested", new SupportBeanComplexProps.SupportBeanSpecialGetterNested("111", "222"));
            _epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            var eventThree = (SupportBeanComplexProps) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("111", eventThree.Nested.NestedValue);

            // object to Object
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanArrayCollMap(anyObject) select Nested from SupportBeanComplexProps";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            var eventFour = (SupportBeanArrayCollMap) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(
                "NestedValue",
                ((SupportBeanComplexProps.SupportBeanSpecialGetterNested) eventFour.AnyObject).NestedValue);

            // test null value
            var stmtTextThree =
                "insert into SupportBean select 'B' as TheString, IntBoxed as IntPrimitive from SupportBean(TheString='A')";
            var stmtThree = _epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            var received = (SupportBean) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(0, received.IntPrimitive);

            var bean = new SupportBean("A", 1);
            bean.IntBoxed = 20;
            _epService.EPRuntime.SendEvent(bean);
            received = (SupportBean) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(20, received.IntPrimitive);
        }

        [Test]
        public void TestPopulateUnderlying()
        {
            var defMap = new Dictionary<string, Object>();
            defMap.Put("intVal", typeof(int));
            defMap.Put("stringVal", typeof(string));
            defMap.Put("doubleVal", typeof(double?));
            defMap.Put("nullVal", null);
            _epService.EPAdministrator.Configuration.AddEventType("MyMapType", defMap);

            var props = new string[]
            {
                "intVal",
                "stringVal",
                "doubleVal",
                "nullVal"
            };
            var types = new Object[]
            {
                typeof(int),
                typeof(string),
                typeof(double?),
                null
            };
            _epService.EPAdministrator.Configuration.AddEventType("MyOAType", props, types);

            Schema schema = SchemaBuilder.Record(
                "MyAvroType",
                TypeBuilder.RequiredInt("intVal"),
                TypeBuilder.RequiredString("stringVal"),
                TypeBuilder.RequiredDouble("doubleVal"),
                TypeBuilder.Field("nullVal", TypeBuilder.Null()));
            _epService.EPAdministrator.Configuration.AddEventTypeAvro(
                "MyAvroType", new ConfigurationEventTypeAvro(schema));

            RunAssertionPopulateUnderlying("MyMapType");
            RunAssertionPopulateUnderlying("MyOAType");
            RunAssertionPopulateUnderlying("MyAvroType");
        }

#if NOT_SUPPORTED_IN_DOTNET
        [Test]
        public void TestCharSequenceCompat()
        {
            EnumHelper.ForEach<EventRepresentationChoice>(
                rep =>
                {
                    _epService.EPAdministrator.CreateEPL(
                        "create " + rep.GetOutputTypeCreateSchemaName() +
                        " schema ConcreteType as (value " + typeof(IEnumerable<char>).FullName + ")");
                    _epService.EPAdministrator.CreateEPL(
                        "insert into ConcreteType select \"Test\" as value from SupportBean");
                    _epService.EPAdministrator.DestroyAllStatements();
                    _epService.EPAdministrator.Configuration.RemoveEventType("ConcreteType", false);
                });
        }
#endif

        [Test]
        public void TestPopulateObjectArray()
        {
            var props = new String[]
            {
                "intVal",
                "stringVal",
                "doubleVal",
                "nullVal"
            };
            var types = new Object[]
            {
                typeof(int),
                typeof(string),
                typeof(double),
                null
            };
            _epService.EPAdministrator.Configuration.AddEventType("MyOAType", props, types);
            var stmtOrig = _epService.EPAdministrator.CreateEPL("select * from MyOAType");

            var stmtTextOne =
                "insert into MyOAType select IntPrimitive as intVal, TheString as stringVal, DoubleBoxed as doubleVal from SupportBean";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            Assert.AreSame(stmtOrig.EventType, stmtOne.EventType);

            var bean = new SupportBean();
            bean.IntPrimitive = 1000;
            bean.TheString = "E1";
            bean.DoubleBoxed = 1001d;
            _epService.EPRuntime.SendEvent(bean);

            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), "intVal,stringVal,doubleVal,nullVal".Split(','), new Object[]
                {
                    1000,
                    "E1",
                    1001d,
                    null
                });
        }

        [Test]
        public void TestBeanFactoryMethod()
        {
            // test factory method on the same event class
            var stmtTextOne = "insert into SupportBeanString select 'abc' as TheString from MyMap";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            stmtOne.Subscriber = _subscriber;

            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            Assert.AreEqual("abc", _listener.AssertOneGetNewAndReset().Get("TheString"));
            Assert.AreEqual("abc", _subscriber.AssertOneGetNewAndReset());
            stmtOne.Dispose();

            // test factory method fully-qualified
            stmtTextOne = "insert into SupportSensorEvent(id, type, device, measurement, confidence)" +
                          "select 2, 'A01', 'DHC1000', 100, 5 from MyMap";
            stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), "Id,Type,Device,Measurement,Confidence".Split(','),
                new Object[]
                {
                    2,
                    "A01",
                    "DHC1000",
                    100.0,
                    5.0
                });

            try
            {
                Activator.CreateInstance(typeof(SupportBeanString));
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // expected
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            try
            {
                Activator.CreateInstance(typeof(SupportSensorEvent));
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // expected
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestArrayPONOInsert()
        {

            _epService.EPAdministrator.Configuration.AddEventType(
                "FinalEventInvalidNonArray", typeof(FinalEventInvalidNonArray));
            _epService.EPAdministrator.Configuration.AddEventType(
                "FinalEventInvalidArray", typeof(FinalEventInvalidArray));
            _epService.EPAdministrator.Configuration.AddEventType(
                "FinalEventValid", typeof(FinalEventValid));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

            // Test valid case of array insert
            var validEpl = "INSERT INTO FinalEventValid SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                           "every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]";
            var stmt = _epService.EPAdministrator.CreateEPL(validEpl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G1"));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));

            var outEvent = ((FinalEventValid) _listener.AssertOneGetNewAndReset().Underlying);
            Assert.AreEqual(1, outEvent.StartEvent.Id);
            Assert.AreEqual("G1", outEvent.StartEvent.P00);
            Assert.AreEqual(2, outEvent.EndEvent.Length);
            Assert.AreEqual(2, outEvent.EndEvent[0].IntPrimitive);
            Assert.AreEqual(3, outEvent.EndEvent[1].IntPrimitive);

            // Test invalid case of non-array destination insert
            var invalidEpl =
                "INSERT INTO FinalEventInvalidNonArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                "every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]";
            try
            {
                _epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Invalid assignment of column 'EndEvent' of type '" +
                    Name.Of<SupportBean[]>() + "' to event property 'EndEvent' typed as '" + Name.Of<SupportBean>() +
                    "', column and parameter types mismatch [INSERT INTO FinalEventInvalidNonArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]]",
                    ex.Message);
            }

            // Test invalid case of array destination insert from non-array var
            var invalidEplTwo =
                "INSERT INTO FinalEventInvalidArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                "every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]";
            try
            {
                _epService.EPAdministrator.CreateEPL(invalidEplTwo);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Invalid assignment of column 'StartEvent' of type '" +
                    Name.Of<SupportBean_S0>() + "' to event property 'StartEvent' typed as '" +
                    Name.Of<SupportBean_S0[]>() +
                    "', column and parameter types mismatch [INSERT INTO FinalEventInvalidArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]]",
                    ex.Message);
            }
        }

        [Test]
        public void TestArrayMapInsert()
        {
            EnumHelper.ForEach<EventRepresentationChoice>(
                rep => RunAssertionArrayMapInsert(rep));
        }

        private void RunAssertionArrayMapInsert(EventRepresentationChoice eventRepresentationEnum)
        {

            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() + " create schema EventOne(id string)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() + " create schema EventTwo(id string, val int)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " create schema FinalEventValid (startEvent EventOne, endEvent EventTwo[])");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " create schema FinalEventInvalidNonArray (startEvent EventOne, endEvent EventTwo)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " create schema FinalEventInvalidArray (startEvent EventOne, endEvent EventTwo)");

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

            // Test valid case of array insert
            var validEpl = "INSERT INTO FinalEventValid SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                           "every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
            var stmt = _epService.EPAdministrator.CreateEPL(validEpl);
            stmt.Events += _listener.Update;

            SendEventOne(_epService, eventRepresentationEnum, "G1");
            SendEventTwo(_epService, eventRepresentationEnum, "G1", 2);
            SendEventTwo(_epService, eventRepresentationEnum, "G1", 3);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));

            EventBean startEventOne;
            EventBean endEventOne;
            EventBean endEventTwo;
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                var outArray = ((Object[]) _listener.AssertOneGetNewAndReset().Underlying);
                startEventOne = (EventBean) outArray[0];
                endEventOne = ((EventBean[]) outArray[1])[0];
                endEventTwo = ((EventBean[]) outArray[1])[1];
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                var outMap = ((IDictionary<string, object>) _listener.AssertOneGetNewAndReset().Underlying);
                startEventOne = (EventBean) outMap.Get("startEvent");
                endEventOne = ((EventBean[]) outMap.Get("endEvent"))[0];
                endEventTwo = ((EventBean[]) outMap.Get("endEvent"))[1];
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                EventBean received = _listener.AssertOneGetNewAndReset();
                startEventOne = (EventBean) received.GetFragment("startEvent");
                EventBean[] endEvents = (EventBean[]) received.GetFragment("endEvent");
                endEventOne = endEvents[0];
                endEventTwo = endEvents[1];
            }
            else
            {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
            Assert.AreEqual("G1", startEventOne.Get("id"));

            Assert.AreEqual(2, endEventOne.Get("val"));
            Assert.AreEqual(3, endEventTwo.Get("val"));
    
            // Test invalid case of non-array destination insert
            var invalidEpl = "INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
            try {
                _epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            }
            catch (EPException ex) {
                String expected;
                if (eventRepresentationEnum.IsAvroEvent())
                {
                    expected = "Error starting statement: Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                }
                else
                {
                    expected = "Error starting statement: Event type named 'FinalEventInvalidNonArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidNonArray' in property 'endEvent' expected event type 'EventTwo' but receives event type 'EventTwo[]'";
                }
                SupportMessageAssertUtil.AssertMessage(ex, expected);
            }

            // Test invalid case of array destination insert from non-array var
            invalidEpl = "INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                         "every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
            try
            {
                _epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                String expected;
                if (eventRepresentationEnum.IsAvroEvent())
                {
                    expected = "Error starting statement: Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                }
                else
                {
                    expected = "Error starting statement: Event type named 'FinalEventInvalidArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidArray' in property 'endEvent' expected event type 'EventTwo' but receives event type 'EventTwo[]'";
                }
                SupportMessageAssertUtil.AssertMessage(ex, expected);
            }

            _epService.Initialize();
        }

        private void SendEventTwo(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string id, int val)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new Object[] { id, val }, "EventTwo");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("id", id);
                theEvent.Put("val", val);
                epService.EPRuntime.SendEvent(theEvent, "EventTwo");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var schema = SchemaBuilder.Record("name", 
                    TypeBuilder.RequiredString("id"),
                    TypeBuilder.RequiredInt("val"));
                var record = new GenericRecord(schema);
                record.Put("id", id);
                record.Put("val", val);
                epService.EPRuntime.SendEventAvro(record, "EventTwo");
            }
            else
            {
                Assert.Fail();
            }
        }

        private void SendEventOne(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string id)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new Object[] { id }, "EventOne");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("id", id);
                epService.EPRuntime.SendEvent(theEvent, "EventOne");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var schema = SchemaBuilder.Record("name", TypeBuilder.RequiredString("id"));
                var record = new GenericRecord(schema);
                record.Put("id", id);
                epService.EPRuntime.SendEventAvro(record, "EventOne");
            }
            else
            {
                Assert.Fail();
            }
        }

        private void TryInvalid(String msg, String stmt)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmt);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(msg, ex.Message);
            }
        }
    
        public class FinalEventInvalidNonArray
        {
            public SupportBean_S0 StartEvent { get; set; }
            public SupportBean EndEvent { get; set; }
        }
    
        public class FinalEventInvalidArray
        {
            public SupportBean_S0[] StartEvent { get; set; }
            public SupportBean[] EndEvent { get; set; }
        }
    
        public class FinalEventValid
        {
            public SupportBean_S0 StartEvent { get; set; }
            public SupportBean[] EndEvent { get; set; }
        }
    
        public class MyLocalTarget
        {
            public int Value { get; set; }
        }
    
        private void SendReceiveTwo(String theString, int? intBoxed)
        {
            var bean = new SupportBean(theString, -1);
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
            var theEvent = (SupportBeanCtorOne) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(null, theEvent.IntBoxed);
            Assert.AreEqual(intBoxed, (int?) theEvent.IntPrimitive);
        }
    
        private void SendReceive(String theString, int intPrimitive, bool boolPrimitive, int? intBoxed)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.BoolPrimitive = boolPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
            var theEvent = (SupportBeanCtorOne) _listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(intBoxed, theEvent.IntBoxed);
            Assert.AreEqual(boolPrimitive, theEvent.IsBoolPrimitive);
            Assert.AreEqual(intPrimitive, theEvent.IntPrimitive);
        }

        private void RunAssertionPopulateUnderlying(string typeName)
        {
            EPStatement stmtOrig = _epService.EPAdministrator.CreateEPL("select * from " + typeName);

            string stmtTextOne = "insert into " + typeName + " select intPrimitive as intVal, theString as stringVal, doubleBoxed as doubleVal from SupportBean";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(_listener);
            Assert.AreSame(stmtOrig.EventType, stmtOne.EventType);

            var bean = new SupportBean();
            bean.IntPrimitive = 1000;
            bean.TheString = "E1";
            bean.DoubleBoxed = 1001d;
            _epService.EPRuntime.SendEvent(bean);

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "intVal,stringVal,doubleVal".Split(','), new Object[] { 1000, "E1", 1001d });
            _epService.EPAdministrator.DestroyAllStatements();
        }

        public class MyEventWithMapFieldSetter
        {
            public string Id { get; set; }
            public IDictionary<string, object> TheMap { get; set; }
        }
    
        private class MyEventWithCtorSameType
        {
            public MyEventWithCtorSameType(SupportBean b1, SupportBean b2)
            {
                B1 = b1;
                B2 = b2;
            }

            public SupportBean B1 { get; private set; }
            public SupportBean B2 { get; private set; }
        }
    }
}
