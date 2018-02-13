///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

// using static org.apache.avro.SchemaBuilder.record;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    using Map = IDictionary<string, object>;

    public class ExecInsertIntoPopulateUnderlying : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var legacy = new ConfigurationEventTypeLegacy();
            legacy.FactoryMethod = "GetInstance";
            configuration.AddEventType("SupportBeanString", typeof(SupportBeanString).FullName, legacy);
            configuration.AddImport(typeof(ExecInsertIntoPopulateUnderlying).Namespace);
    
            legacy = new ConfigurationEventTypeLegacy();
            legacy.FactoryMethod = typeof(SupportSensorEventFactory).FullName + ".GetInstance";
            configuration.AddEventType("SupportSensorEvent", typeof(SupportSensorEvent).Name, legacy);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportTemperatureBean", typeof(SupportTemperatureBean));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanInterfaceProps", typeof(SupportBeanInterfaceProps));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanErrorTestingOne", typeof(SupportBeanErrorTestingOne));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanErrorTestingTwo", typeof(SupportBeanErrorTestingTwo));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanReadOnly", typeof(SupportBeanReadOnly));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanArrayCollMap", typeof(SupportBeanArrayCollMap));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_N", typeof(SupportBean_N));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanObject", typeof(SupportBeanObject));
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));
    
            var mymapDef = new Dictionary<string, object>();
            mymapDef.Put("anint", typeof(int));
            mymapDef.Put("intBoxed", typeof(int?));
            mymapDef.Put("floatBoxed", typeof(float?));
            mymapDef.Put("intArr", typeof(int[]));
            mymapDef.Put("mapProp", typeof(Map));
            mymapDef.Put("isaImpl", typeof(ISupportAImpl));
            mymapDef.Put("isbImpl", typeof(ISupportBImpl));
            mymapDef.Put("isgImpl", typeof(ISupportAImplSuperGImpl));
            mymapDef.Put("isabImpl", typeof(ISupportBaseABImpl));
            mymapDef.Put("nested", typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested));
            epService.EPAdministrator.Configuration.AddEventType("MyMap", mymapDef);
    
            var xml = new ConfigurationEventTypeXMLDOM();
            xml.RootElementName = "abc";
            epService.EPAdministrator.Configuration.AddEventType("xmltype", xml);
    
            RunAssertionCtor(epService);
            RunAssertionCtorWithPattern(epService);
            RunAssertionBeanJoin(epService);
            RunAssertionPopulateBeanSimple(epService);
            RunAssertionBeanWildcard(epService);
            RunAssertionPopulateBeanObjects(epService);
            RunAssertionPopulateUnderlying(epService);
            RunAssertionCharSequenceCompat(epService);
            RunAssertionBeanFactoryMethod(epService);
            RunAssertionArrayPOJOInsert(epService);
            RunAssertionArrayMapInsert(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionCtor(EPServiceProvider epService) {
    
            // simple type and null values
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorOne", typeof(SupportBeanCtorOne));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorTwo", typeof(SupportBeanCtorTwo));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
    
            string eplOne = "insert into SupportBeanCtorOne select theString, intBoxed, intPrimitive, boolPrimitive from SupportBean";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(eplOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
    
            SendReceive(epService, listener, "E1", 2, true, 100);
            SendReceive(epService, listener, "E2", 3, false, 101);
            SendReceive(epService, listener, null, 4, true, null);
            stmtOne.Dispose();
    
            // boxable type and null values
            string eplTwo = "insert into SupportBeanCtorOne select theString, null, intBoxed from SupportBean";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.AddListener(listener);
            SendReceiveTwo(epService, listener, "E1", 100);
            stmtTwo.Dispose();
    
            // test join wildcard
            string eplThree = "insert into SupportBeanCtorTwo select * from SupportBean_ST0#lastevent, SupportBean_ST1#lastevent";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(eplThree);
            stmtThree.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 2));
            SupportBeanCtorTwo theEvent = (SupportBeanCtorTwo) listener.AssertOneGetNewAndReset().Underlying;
            Assert.IsNotNull(theEvent.St0);
            Assert.IsNotNull(theEvent.St1);
            stmtThree.Dispose();
    
            // test (should not use column names)
            string eplFour = "insert into SupportBeanCtorOne(theString, intPrimitive) select 'E1', 5 from SupportBean";
            EPStatement stmtFour = epService.EPAdministrator.CreateEPL(eplFour);
            stmtFour.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("x", -1));
            SupportBeanCtorOne eventOne = (SupportBeanCtorOne) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", eventOne.TheString);
            Assert.AreEqual(99, eventOne.IntPrimitive);
            Assert.AreEqual((int?) 5, eventOne.IntBoxed);
    
            // test Ctor accepting same types
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithCtorSameType));
            string epl = "insert into MyEventWithCtorSameType select c1,c2 from SupportBean(theString='b1')#lastevent as c1, SupportBean(theString='b2')#lastevent as c2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("b1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("b2", 2));
            MyEventWithCtorSameType result = (MyEventWithCtorSameType) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, result.B1.IntPrimitive);
            Assert.AreEqual(2, result.B2.IntPrimitive);
        }
    
        private void RunAssertionCtorWithPattern(EPServiceProvider epService) {
    
            // simple type and null values
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorThree", typeof(SupportBeanCtorThree));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
    
            // Test valid case of array insert
            string epl = "insert into SupportBeanCtorThree select s, e FROM PATTERN [" +
                    "every s=SupportBean_ST0 -> [2] e=SupportBean_ST1]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E0", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("E2", 3));
            SupportBeanCtorThree three = (SupportBeanCtorThree) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E0", three.St0.Id);
            Assert.AreEqual(2, three.St1.Length);
            Assert.AreEqual("E1", three.St1[0].Id);
            Assert.AreEqual("E2", three.St1[1].Id);
        }
    
        private void RunAssertionBeanJoin(EPServiceProvider epService) {
            // test wildcard
            string stmtTextOne = "insert into SupportBeanObject select * from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
    
            var n1 = new SupportBean_N(1, 10, 100d, 1000d, true, true);
            epService.EPRuntime.SendEvent(n1);
            var s01 = new SupportBean_S0(1);
            epService.EPRuntime.SendEvent(s01);
            SupportBeanObject theEvent = (SupportBeanObject) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
    
            // test select stream names
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanObject select one, two from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            epService.EPRuntime.SendEvent(n1);
            epService.EPRuntime.SendEvent(s01);
            theEvent = (SupportBeanObject) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
            stmtOne.Dispose();
    
            // test fully-qualified class name as target
            stmtTextOne = "insert into " + typeof(SupportBeanObject).Name + " select one, two from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            epService.EPRuntime.SendEvent(n1);
            epService.EPRuntime.SendEvent(s01);
            theEvent = (SupportBeanObject) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
    
            // test local class and auto-import
            stmtOne.Dispose();
            stmtTextOne = "insert into " + GetType().FullName + "$MyLocalTarget select 1 as value from SupportBean_N";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
            epService.EPRuntime.SendEvent(n1);
            MyLocalTarget eventLocal = (MyLocalTarget) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, eventLocal.Value);
            stmtOne.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorOne", typeof(SupportBeanCtorOne));
    
            string text = "insert into SupportBeanCtorOne select 1 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Failed to find a suitable constructor for class '" + typeof(SupportBeanCtorOne).Name + "': Could not find constructor in class '" + typeof(SupportBeanCtorOne).Name + "' with matching parameter number and expected parameter Type(s) 'int?' (nearest matching constructor taking Type(s) 'string, int?, int, bool') [insert into SupportBeanCtorOne select 1 from SupportBean]");
    
            text = "insert into SupportBean(intPrimitive) select 1L from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Invalid assignment of column 'intPrimitive' of type 'java.lang.long' to event property 'intPrimitive' typed as 'int', column and parameter types mismatch [insert into SupportBean(intPrimitive) select 1L from SupportBean]");
    
            text = "insert into SupportBean(intPrimitive) select null from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Invalid assignment of column 'intPrimitive' of null type to event property 'intPrimitive' typed as 'int', nullable type mismatch [insert into SupportBean(intPrimitive) select null from SupportBean]");
    
            text = "insert into SupportBeanReadOnly select 'a' as geom from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Failed to find a suitable constructor for class '" + typeof(SupportBeanReadOnly).Name + "': Could not find constructor in class '" + typeof(SupportBeanReadOnly).Name + "' with matching parameter number and expected parameter Type(s) 'string' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly select 'a' as geom from SupportBean]");
    
            text = "insert into SupportBean select 3 as dummyField from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Column 'dummyField' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 as dummyField from SupportBean]");
    
            text = "insert into SupportBean select 3 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Column '3' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 from SupportBean]");
    
            text = "insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap";
            TryInvalid(epService, text, "Error starting statement: Invalid assignment of column 'isa' of type '" + typeof(ISupportBImpl).Name + "' to event property 'isa' typed as '" + typeof(ISupportA).Name + "', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap]");
    
            text = "insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap";
            TryInvalid(epService, text, "Error starting statement: Invalid assignment of column 'isg' of type '" + typeof(ISupportBaseABImpl).Name + "' to event property 'isg' typed as '" + typeof(ISupportAImplSuperG).Name + "', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap]");
    
            text = "insert into SupportBean(dummy) select 3 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Column 'dummy' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean(dummy) select 3 from SupportBean]");
    
            text = "insert into SupportBeanReadOnly(side) select 'E1' from MyMap";
            TryInvalid(epService, text, "Error starting statement: Failed to find a suitable constructor for class '" + typeof(SupportBeanReadOnly).Name + "': Could not find constructor in class '" + typeof(SupportBeanReadOnly).Name + "' with matching parameter number and expected parameter Type(s) 'string' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly(side) select 'E1' from MyMap]");
    
            epService.EPAdministrator.CreateEPL("insert into ABCStream select *, 1+1 from SupportBean");
            text = "insert into ABCStream(string) select 'E1' from MyMap";
            TryInvalid(epService, text, "Error starting statement: Event type named 'ABCStream' has already been declared with differing column name or type information: Type by name 'ABCStream' is not a compatible type (target type underlying is '" + Name.Of<Pair<object, IDictionary<string, object>>>() + "') [insert into ABCStream(string) select 'E1' from MyMap]");
    
            text = "insert into xmltype select 1 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Event type named 'xmltype' has already been declared with differing column name or type information: Type by name 'xmltype' is not a compatible type (target type underlying is '" + Name.Of<XmlNode>() + "') [insert into xmltype select 1 from SupportBean]");
    
            text = "insert into MyMap(dummy) select 1 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Event type named 'MyMap' has already been declared with differing column name or type information: Type by name 'MyMap' expects 10 properties but receives 1 properties [insert into MyMap(dummy) select 1 from SupportBean]");
    
            // setter throws exception
            string stmtTextOne = "insert into SupportBeanErrorTestingTwo(value) select 'E1' from MyMap";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            SupportBeanErrorTestingTwo underlying = (SupportBeanErrorTestingTwo) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("default", underlying.Value);
            stmtOne.Dispose();
    
            // surprise - wrong type then defined
            stmtTextOne = "insert into SupportBean(intPrimitive) select anint from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
            listener.Reset();
            var map = new Dictionary<string, object>();
            map.Put("anint", "notAnInt");
            epService.EPRuntime.SendEvent(map, "MyMap");
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("intPrimitive"));
    
            // ctor throws exception
            epService.EPAdministrator.DestroyAllStatements();
            string stmtTextThree = "insert into SupportBeanCtorOne select 'E1' from SupportBean";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.AddListener(listener);
            try {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail(); // rethrowing handler registered
            } catch (Exception ex) {
                // expected
            }
    
            // allow automatic cast of same-type event
            epService.EPAdministrator.CreateEPL("create schema MapOne as (prop1 string)");
            epService.EPAdministrator.CreateEPL("create schema MapTwo as (prop1 string)");
            epService.EPAdministrator.CreateEPL("insert into MapOne select * from MapTwo");
        }
    
        private void RunAssertionPopulateBeanSimple(EPServiceProvider epService) {
            // test select column names
            string stmtTextOne = "insert into SupportBean select " +
                    "'E1' as theString, 1 as intPrimitive, 2 as intBoxed, 3L as longPrimitive," +
                    "null as longBoxed, true as boolPrimitive, " +
                    "'x' as charPrimitive, 0xA as bytePrimitive, " +
                    "8.0f as floatPrimitive, 9.0d as doublePrimitive, " +
                    "0x05 as shortPrimitive, SupportEnum.ENUM_VALUE_2 as enumValue " +
                    " from MyMap";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select * from SupportBean";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmtTwo.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            SupportBean received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", received.TheString);
            EPAssertionUtil.AssertPropsPono(received,
                    "intPrimitive,intBoxed,longPrimitive,longBoxed,boolPrimitive,charPrimitive,bytePrimitive,floatPrimitive,doublePrimitive,shortPrimitive,enumValue".Split(','),
                    new Object[]{1, 2, 3L, null, true, 'x', (byte) 10, 8f, 9d, (short) 5, SupportEnum.ENUM_VALUE_2});
    
            // test insert-into column names
            stmtOne.Dispose();
            stmtTwo.Dispose();
            listener.Reset();
            stmtTextOne = "insert into SupportBean(theString, intPrimitive, intBoxed, longPrimitive," +
                    "longBoxed, boolPrimitive, charPrimitive, bytePrimitive, floatPrimitive, doublePrimitive, " +
                    "shortPrimitive, enumValue) select " +
                    "'E1', 1, 2, 3L," +
                    "null, true, " +
                    "'x', 0xA, " +
                    "8.0f, 9.0d, " +
                    "0x05 as shortPrimitive, SupportEnum.ENUM_VALUE_2 " +
                    " from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", received.TheString);
            EPAssertionUtil.AssertPropsPono(received,
                    "intPrimitive,intBoxed,longPrimitive,longBoxed,boolPrimitive,charPrimitive,bytePrimitive,floatPrimitive,doublePrimitive,shortPrimitive,enumValue".Split(','),
                    new Object[]{1, 2, 3L, null, true, 'x', (byte) 10, 8f, 9d, (short) 5, SupportEnum.ENUM_VALUE_2});
    
            // test convert int? boxed to long boxed
            stmtOne.Dispose();
            listener.Reset();
            stmtTextOne = "insert into SupportBean(longBoxed, doubleBoxed) select intBoxed, floatBoxed from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            var vals = new Dictionary<string, object>();
            vals.Put("intBoxed", 4);
            vals.Put("floatBoxed", 0f);
            epService.EPRuntime.SendEvent(vals, "MyMap");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "longBoxed,doubleBoxed".Split(','), new Object[]{4L, 0d});
            epService.EPAdministrator.DestroyAllStatements();
    
            // test new-to-map conversion
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithMapFieldSetter));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into MyEventWithMapFieldSetter(id, themap) " +
                    "select 'test' as id, new {somefield = theString} as themap from SupportBean");
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap((Map) listener.AssertOneGetNew().Get("themap"), "somefield".Split(','), "E1");
    
            stmt.Dispose();
        }
    
        private void RunAssertionBeanWildcard(EPServiceProvider epService) {
            var mapDef = new Dictionary<string, object>();
            mapDef.Put("intPrimitive", typeof(int));
            mapDef.Put("longBoxed", typeof(long));
            mapDef.Put("theString", typeof(string));
            mapDef.Put("boolPrimitive", typeof(bool?));
            epService.EPAdministrator.Configuration.AddEventType("MySupportMap", mapDef);
    
            string stmtTextOne = "insert into SupportBean select * from MySupportMap";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
    
            var vals = new Dictionary<string, object>();
            vals.Put("intPrimitive", 4);
            vals.Put("longBoxed", 100L);
            vals.Put("theString", "E1");
            vals.Put("boolPrimitive", true);
    
            epService.EPRuntime.SendEvent(vals, "MySupportMap");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(),
                    "intPrimitive,longBoxed,theString,boolPrimitive".Split(','),
                    new Object[]{4, 100L, "E1", true});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionPopulateBeanObjects(EPServiceProvider epService) {
            // arrays and maps
            string stmtTextOne = "insert into SupportBeanComplexProps(arrayProperty,objectArray,mapProperty) select " +
                    "intArr,{10,20,30},mapProp" +
                    " from MyMap as m";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
    
            var mymapVals = new Dictionary<string, object>();
            mymapVals.Put("intArr", new int[]{-1, -2});
            var inner = new Dictionary<string, object>();
            inner.Put("mykey", "myval");
            mymapVals.Put("mapProp", inner);
            epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            SupportBeanComplexProps theEvent = (SupportBeanComplexProps) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(-2, theEvent.ArrayProperty[1]);
            Assert.AreEqual(20, theEvent.ObjectArray[1]);
            Assert.AreEqual("myval", theEvent.MapProperty.Get("mykey"));
    
            // inheritance
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanInterfaceProps(isa,isg) select " +
                    "isaImpl,isgImpl" +
                    " from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            mymapVals = new Dictionary<string, object>();
            mymapVals.Put("mapProp", inner);
            epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            Assert.IsTrue(listener.AssertOneGetNewAndReset().Underlying is SupportBeanInterfaceProps);
            Assert.AreEqual(typeof(SupportBeanInterfaceProps), stmtOne.EventType.UnderlyingType);
    
            // object values from Map same type
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanComplexProps(nested) select nested from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            mymapVals = new Dictionary<string, object>();
            mymapVals.Put("nested", new SupportBeanComplexProps.SupportBeanSpecialGetterNested("111", "222"));
            epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            SupportBeanComplexProps eventThree = (SupportBeanComplexProps) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("111", eventThree.Nested.NestedValue);
    
            // object to Object
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanArrayCollMap(anyObject) select nested from SupportBeanComplexProps";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            SupportBeanArrayCollMap eventFour = (SupportBeanArrayCollMap) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("nestedValue", ((SupportBeanComplexProps.SupportBeanSpecialGetterNested) eventFour.AnyObject).NestedValue);
    
            // test null value
            string stmtTextThree = "insert into SupportBean select 'B' as theString, intBoxed as intPrimitive from SupportBean(theString='A')";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            SupportBean received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(0, received.IntPrimitive);
    
            var bean = new SupportBean("A", 1);
            bean.IntBoxed = 20;
            epService.EPRuntime.SendEvent(bean);
            received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(20, received.IntPrimitive);
    
            stmtThree.Dispose();
        }
    
        private void RunAssertionPopulateUnderlying(EPServiceProvider epService) {
            var defMap = new Dictionary<string, object>();
            defMap.Put("intVal", typeof(int));
            defMap.Put("stringVal", typeof(string));
            defMap.Put("doubleVal", typeof(double?));
            defMap.Put("nullVal", null);
            epService.EPAdministrator.Configuration.AddEventType("MyMapType", defMap);
    
            var props = new string[]{"intVal", "stringVal", "doubleVal", "nullVal"};
            var types = new Object[]{typeof(int), typeof(string), typeof(double?), null};
            epService.EPAdministrator.Configuration.AddEventType("MyOAType", props, types);
    
            var schema = SchemaBuilder.Record("MyAvroType",
                    TypeBuilder.RequiredInt("intVal"),
                    TypeBuilder.RequiredString("stringVal"),
                    TypeBuilder.RequiredDouble("doubleVal"),
                    TypeBuilder.Field("nullVal", TypeBuilder.Null()));

            epService.EPAdministrator.Configuration.AddEventTypeAvro("MyAvroType", new ConfigurationEventTypeAvro(schema));
    
            TryAssertionPopulateUnderlying(epService, "MyMapType");
            TryAssertionPopulateUnderlying(epService, "MyOAType");
            TryAssertionPopulateUnderlying(epService, "MyAvroType");
        }
    
        private void RunAssertionCharSequenceCompat(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema ConcreteType as (value java.lang.CharSequence)");
                epService.EPAdministrator.CreateEPL("insert into ConcreteType select \"Test\" as value from SupportBean");
                epService.EPAdministrator.DestroyAllStatements();
                epService.EPAdministrator.Configuration.RemoveEventType("ConcreteType", false);
            }
        }
    
        private void RunAssertionBeanFactoryMethod(EPServiceProvider epService) {
            // test factory method on the same event class
            string stmtTextOne = "insert into SupportBeanString select 'abc' as theString from MyMap";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
            var subscriber = new SupportSubscriber();
            stmtOne.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            Assert.AreEqual("abc", listener.AssertOneGetNewAndReset().Get("theString"));
            Assert.AreEqual("abc", subscriber.AssertOneGetNewAndReset());
            stmtOne.Dispose();
    
            // test factory method fully-qualified
            stmtTextOne = "insert into SupportSensorEvent(id, type, device, measurement, confidence)" +
                    "select 2, 'A01', 'DHC1000', 100, 5 from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "id,type,device,measurement,confidence".Split(','), new Object[]{2, "A01", "DHC1000", 100.0, 5.0});
    
            try {
                typeof(SupportBeanString).NewInstance();
                Assert.Fail();
            } catch (InstantiationException ex) {
                // expected
            } catch (Exception ex) {
                Assert.Fail();
            }
    
            try {
                typeof(SupportSensorEvent).NewInstance();
                Assert.Fail();
            } catch (IllegalAccessException ex) {
                // expected
            } catch (InstantiationException e) {
                Assert.Fail();
            }
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionArrayPOJOInsert(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType("FinalEventInvalidNonArray", typeof(FinalEventInvalidNonArray));
            epService.EPAdministrator.Configuration.AddEventType("FinalEventInvalidArray", typeof(FinalEventInvalidArray));
            epService.EPAdministrator.Configuration.AddEventType("FinalEventValid", typeof(FinalEventValid));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            // Test valid case of array insert
            string validEpl = "INSERT INTO FinalEventValid SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:Interval(10 sec)]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(validEpl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
    
            FinalEventValid outEvent = (FinalEventValid) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, outEvent.StartEvent.Id);
            Assert.AreEqual("G1", outEvent.StartEvent.P00);
            Assert.AreEqual(2, outEvent.EndEvent.Length);
            Assert.AreEqual(2, outEvent.EndEvent[0].IntPrimitive);
            Assert.AreEqual(3, outEvent.EndEvent[1].IntPrimitive);
    
            // Test invalid case of non-array destination insert
            string invalidEpl = "INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:Interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Invalid assignment of column 'endEvent' of type '" + typeof(SupportBean).FullName + "[]' to event property 'endEvent' typed as '" + typeof(SupportBean).FullName + "', column and parameter types mismatch [INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:Interval(10 sec)]]", ex.Message);
            }
    
            // Test invalid case of array destination insert from non-array var
            string invalidEplTwo = "INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:Interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEplTwo);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Invalid assignment of column 'startEvent' of type '" + typeof(SupportBean_S0).Name + "' to event property 'startEvent' typed as '" + typeof(SupportBean_S0).Name + "[]', column and parameter types mismatch [INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:Interval(10 sec)]]", ex.Message);
            }
    
            stmt.Dispose();
            foreach (string name in "FinalEventValid,FinalEventInvalidNonArray,FinalEventInvalidArray".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionArrayMapInsert(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionArrayMapInsert(epService, rep);
            }
        }
    
        private void TryAssertionArrayMapInsert(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventOne(id string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTwo(id string, val int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema FinalEventValid (startEvent EventOne, endEvent EventTwo[])");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema FinalEventInvalidNonArray (startEvent EventOne, endEvent EventTwo)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema FinalEventInvalidArray (startEvent EventOne, endEvent EventTwo)");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            // Test valid case of array insert
            string validEpl = "INSERT INTO FinalEventValid SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:Interval(10 sec)]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(validEpl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendEventOne(epService, eventRepresentationEnum, "G1");
            SendEventTwo(epService, eventRepresentationEnum, "G1", 2);
            SendEventTwo(epService, eventRepresentationEnum, "G1", 3);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
    
            EventBean startEventOne;
            EventBean endEventOne;
            EventBean endEventTwo;
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                Object[] outArray = (Object[]) listener.AssertOneGetNewAndReset().Underlying;
                startEventOne = (EventBean) outArray[0];
                endEventOne = ((EventBean[]) outArray[1])[0];
                endEventTwo = ((EventBean[]) outArray[1])[1];
            } else if (eventRepresentationEnum.IsMapEvent()) {
                Map outMap = (Map) listener.AssertOneGetNewAndReset().Underlying;
                startEventOne = (EventBean) outMap.Get("startEvent");
                endEventOne = ((EventBean[]) outMap.Get("endEvent"))[0];
                endEventTwo = ((EventBean[]) outMap.Get("endEvent"))[1];
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                EventBean received = listener.AssertOneGetNewAndReset();
                startEventOne = (EventBean) received.GetFragment("startEvent");
                EventBean[] endEvents = (EventBean[]) received.GetFragment("endEvent");
                endEventOne = endEvents[0];
                endEventTwo = endEvents[1];
            } else {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
            Assert.AreEqual("G1", startEventOne.Get("id"));
            Assert.AreEqual(2, endEventOne.Get("val"));
            Assert.AreEqual(3, endEventTwo.Get("val"));
    
            // Test invalid case of non-array destination insert
            string invalidEpl = "INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:Interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            } catch (EPException ex) {
                string expected;
                if (eventRepresentationEnum.IsAvroEvent()) {
                    expected = "Error starting statement: Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                } else {
                    expected = "Error starting statement: Event type named 'FinalEventInvalidNonArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidNonArray' in property 'endEvent' expected event type 'EventTwo' but receives event type 'EventTwo[]'";
                }
                SupportMessageAssertUtil.AssertMessage(ex, expected);
            }
    
            // Test invalid case of array destination insert from non-array var
            invalidEpl = "INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:Interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            } catch (EPException ex) {
                string expected;
                if (eventRepresentationEnum.IsAvroEvent()) {
                    expected = "Error starting statement: Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                } else {
                    expected = "Error starting statement: Event type named 'FinalEventInvalidArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidArray' in property 'endEvent' expected event type 'EventTwo' but receives event type 'EventTwo[]'";
                }
                SupportMessageAssertUtil.AssertMessage(ex, expected);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "EventOne,EventTwo,FinalEventValid,FinalEventInvalidNonArray,FinalEventInvalidArray".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void SendEventTwo(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string id, int val) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new Object[]{id, val}, "EventTwo");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("id", id);
                theEvent.Put("val", val);
                epService.EPRuntime.SendEvent(theEvent, "EventTwo");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("name", 
                    TypeBuilder.RequiredString("id"),
                    TypeBuilder.RequiredInt("val"));
                var record = new GenericRecord(schema);
                record.Put("id", id);
                record.Put("val", val);
                epService.EPRuntime.SendEventAvro(record, "EventTwo");
            } else {
                Assert.Fail();
            }
        }
    
        private void SendEventOne(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string id) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new Object[]{id}, "EventOne");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("id", id);
                epService.EPRuntime.SendEvent(theEvent, "EventOne");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("name", TypeBuilder.RequiredString("id"));
                var record = new GenericRecord(schema);
                record.Put("id", id);
                epService.EPRuntime.SendEventAvro(record, "EventOne");
            } else {
                Assert.Fail();
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

        private void SendReceiveTwo(
            EPServiceProvider epService,
            SupportUpdateListener listener, 
            string theString, 
            int? intBoxed)
        {
            var bean = new SupportBean(theString, -1);
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
            SupportBeanCtorOne theEvent = (SupportBeanCtorOne) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(null, theEvent.IntBoxed);
            Assert.AreEqual(intBoxed, (int?) theEvent.IntPrimitive);
        }

        private void SendReceive(
            EPServiceProvider epService,
            SupportUpdateListener listener,
            string theString, 
            int intPrimitive,
            bool boolPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.BoolPrimitive = boolPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
            SupportBeanCtorOne theEvent = (SupportBeanCtorOne) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(intBoxed, theEvent.IntBoxed);
            Assert.AreEqual(boolPrimitive, theEvent.IsBoolPrimitive);
            Assert.AreEqual(intPrimitive, theEvent.IntPrimitive);
        }

        private void TryAssertionPopulateUnderlying(
            EPServiceProvider epService, 
            string typeName)
        {
            EPStatement stmtOrig = epService.EPAdministrator.CreateEPL("select * from " + typeName);
    
            string stmtTextOne = "insert into " + typeName + " select intPrimitive as intVal, theString as stringVal, doubleBoxed as doubleVal from SupportBean";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);
            Assert.AreSame(stmtOrig.EventType, stmtOne.EventType);
    
            var bean = new SupportBean();
            bean.IntPrimitive = 1000;
            bean.TheString = "E1";
            bean.DoubleBoxed = 1001d;
            epService.EPRuntime.SendEvent(bean);
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "intVal,stringVal,doubleVal".Split(','), new Object[]{1000, "E1", 1001d});
            epService.EPAdministrator.DestroyAllStatements();
        }

        public class MyEventWithMapFieldSetter
        {
            public string Id { get; set; }
            public Map Themap { get; set; }
        }

        private class MyEventWithCtorSameType
        {
            public SupportBean B1 { get; }
            public SupportBean B2 { get; }

            public MyEventWithCtorSameType(SupportBean b1, SupportBean b2)
            {
                this.B1 = b1;
                this.B2 = b2;
            }
        }
    }
} // end of namespace
