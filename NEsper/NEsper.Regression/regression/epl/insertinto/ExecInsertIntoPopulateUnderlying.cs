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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    using Map = IDictionary<string, object>;

    public class ExecInsertIntoPopulateUnderlying : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var legacy = new ConfigurationEventTypeLegacy();
            legacy.FactoryMethod = "GetInstance";
            configuration.AddEventType("SupportBeanString", typeof(SupportBeanString).AssemblyQualifiedName, legacy);
            configuration.AddImport(typeof(ExecInsertIntoPopulateUnderlying).Namespace);
    
            legacy = new ConfigurationEventTypeLegacy();
            legacy.FactoryMethod = typeof(SupportSensorEventFactory).FullName + ".GetInstance";
            configuration.AddEventType("SupportSensorEvent", typeof(SupportSensorEvent).AssemblyQualifiedName, legacy);
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
            mymapDef.Put("IntBoxed", typeof(int?));
            mymapDef.Put("FloatBoxed", typeof(float?));
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
            RunAssertionArrayPonoInsert(epService);
            RunAssertionArrayMapInsert(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionCtor(EPServiceProvider epService) {
    
            // simple type and null values
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorOne", typeof(SupportBeanCtorOne));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorTwo", typeof(SupportBeanCtorTwo));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
    
            var eplOne = "insert into SupportBeanCtorOne select TheString, IntBoxed, IntPrimitive, BoolPrimitive from SupportBean";
            var stmtOne = epService.EPAdministrator.CreateEPL(eplOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            SendReceive(epService, listener, "E1", 2, true, 100);
            SendReceive(epService, listener, "E2", 3, false, 101);
            SendReceive(epService, listener, null, 4, true, null);
            stmtOne.Dispose();
    
            // boxable type and null values
            var eplTwo = "insert into SupportBeanCtorOne select TheString, null, IntBoxed from SupportBean";
            var stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.Events += listener.Update;
            SendReceiveTwo(epService, listener, "E1", 100);
            stmtTwo.Dispose();
    
            // test join wildcard
            var eplThree = "insert into SupportBeanCtorTwo select * from SupportBean_ST0#lastevent, SupportBean_ST1#lastevent";
            var stmtThree = epService.EPAdministrator.CreateEPL(eplThree);
            stmtThree.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 2));
            var theEvent = (SupportBeanCtorTwo) listener.AssertOneGetNewAndReset().Underlying;
            Assert.IsNotNull(theEvent.St0);
            Assert.IsNotNull(theEvent.St1);
            stmtThree.Dispose();
    
            // test (should not use column names)
            var eplFour = "insert into SupportBeanCtorOne(TheString, IntPrimitive) select 'E1', 5 from SupportBean";
            var stmtFour = epService.EPAdministrator.CreateEPL(eplFour);
            stmtFour.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("x", -1));
            var eventOne = (SupportBeanCtorOne) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", eventOne.TheString);
            Assert.AreEqual(99, eventOne.IntPrimitive);
            Assert.AreEqual((int?) 5, eventOne.IntBoxed);
    
            // test Ctor accepting same types
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithCtorSameType));
            var epl = "insert into MyEventWithCtorSameType select c1,c2 from SupportBean(TheString='b1')#lastevent as c1, SupportBean(TheString='b2')#lastevent as c2";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("b1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("b2", 2));
            var result = (MyEventWithCtorSameType) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, result.B1.IntPrimitive);
            Assert.AreEqual(2, result.B2.IntPrimitive);
        }
    
        private void RunAssertionCtorWithPattern(EPServiceProvider epService) {
    
            // simple type and null values
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorThree", typeof(SupportBeanCtorThree));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
    
            // Test valid case of array insert
            var epl = "insert into SupportBeanCtorThree select s, e FROM PATTERN [" +
                    "every s=SupportBean_ST0 -> [2] e=SupportBean_ST1]";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E0", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("E2", 3));
            var three = (SupportBeanCtorThree) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E0", three.St0.Id);
            Assert.AreEqual(2, three.St1.Length);
            Assert.AreEqual("E1", three.St1[0].Id);
            Assert.AreEqual("E2", three.St1[1].Id);
        }
    
        private void RunAssertionBeanJoin(EPServiceProvider epService) {
            // test wildcard
            var stmtTextOne = "insert into SupportBeanObject select * from SupportBean_N#lastevent as One, SupportBean_S0#lastevent as Two";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            var n1 = new SupportBean_N(1, 10, 100d, 1000d, true, true);
            epService.EPRuntime.SendEvent(n1);
            var s01 = new SupportBean_S0(1);
            epService.EPRuntime.SendEvent(s01);
            var theEvent = (SupportBeanObject) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
    
            // test select stream names
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanObject select One, Two from SupportBean_N#lastevent as One, SupportBean_S0#lastevent as Two";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(n1);
            epService.EPRuntime.SendEvent(s01);
            theEvent = (SupportBeanObject) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
            stmtOne.Dispose();
    
            // test fully-qualified class name as target
            stmtTextOne = "insert into " + typeof(SupportBeanObject).FullName + " select One, Two from SupportBean_N#lastevent as One, SupportBean_S0#lastevent as Two";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(n1);
            epService.EPRuntime.SendEvent(s01);
            theEvent = (SupportBeanObject) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreSame(n1, theEvent.One);
            Assert.AreSame(s01, theEvent.Two);
    
            // test local class and auto-import
            stmtOne.Dispose();
            stmtTextOne = "insert into " + GetType().FullName + "$MyLocalTarget select 1 as Value from SupportBean_N";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
            epService.EPRuntime.SendEvent(n1);
            var eventLocal = (MyLocalTarget) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, eventLocal.Value);
            stmtOne.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanCtorOne", typeof(SupportBeanCtorOne));
    
            var text = "insert into SupportBeanCtorOne select 1 from SupportBean";
            TryInvalid(epService, text,
                string.Format(
                    "Error starting statement: Failed to find a suitable constructor for type \'{0}\': " +
                    "Could not find constructor in class \'{1}\' with matching parameter number and expected parameter type(s) \'{2}\' " +
                    "(nearest matching constructor taking type(s) \'System.String\') " +
                    "[insert into SupportBeanCtorOne select 1 from SupportBean]",
                    typeof(SupportBeanCtorOne).GetCleanName(),
                    typeof(SupportBeanCtorOne).GetCleanName(),
                    typeof(int).GetCleanName()
                    ));
    
            text = "insert into SupportBean(IntPrimitive) select 1L from SupportBean";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Invalid assignment of column 'IntPrimitive' of type '{0}' to event property 'IntPrimitive' typed as '{1}', column and parameter types mismatch [insert into SupportBean(IntPrimitive) select 1L from SupportBean]",
                typeof(long).GetCleanName(),
                typeof(int).GetCleanName()
                ));
    
            text = "insert into SupportBean(IntPrimitive) select null from SupportBean";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Invalid assignment of column 'IntPrimitive' of null type to event property 'IntPrimitive' typed as '{0}', nullable type mismatch [insert into SupportBean(IntPrimitive) select null from SupportBean]",
                typeof(int).GetCleanName()));
    
            text = "insert into SupportBeanReadOnly select 'a' as geom from SupportBean";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Failed to find a suitable constructor for type '{0}': Could not find constructor in class '{0}' with matching parameter number and expected parameter type(s) '{1}' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly select 'a' as geom from SupportBean]",
                typeof(SupportBeanReadOnly).GetCleanName(),
                typeof(string).GetCleanName()));
    
            text = "insert into SupportBean select 3 as dummyField from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Column 'dummyField' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 as dummyField from SupportBean]");
    
            text = "insert into SupportBean select 3 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Column '3' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 from SupportBean]");
    
            text = "insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Invalid assignment of column 'isa' of type '{0}' to event property 'isa' typed as '{1}', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap]", 
                typeof(ISupportBImpl).GetCleanName(),
                typeof(ISupportA).GetCleanName()));
    
            text = "insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Invalid assignment of column 'isg' of type '{0}' to event property 'isg' typed as '{1}', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap]", 
                typeof(ISupportBaseABImpl).GetCleanName(),
                typeof(ISupportAImplSuperG).GetCleanName()));
    
            text = "insert into SupportBean(dummy) select 3 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Column 'dummy' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean(dummy) select 3 from SupportBean]");
    
            text = "insert into SupportBeanReadOnly(side) select 'E1' from MyMap";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Failed to find a suitable constructor for type '{0}': Could not find constructor in class '{0}' with matching parameter number and expected parameter type(s) '{1}' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly(side) select 'E1' from MyMap]",
                typeof(SupportBeanReadOnly).GetCleanName(),
                typeof(string).GetCleanName()));
    
            epService.EPAdministrator.CreateEPL("insert into ABCStream select *, 1+1 from SupportBean");
            text = "insert into ABCStream(string) select 'E1' from MyMap";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Event type named 'ABCStream' has already been declared with differing column name or type information: Type by name 'ABCStream' is not a compatible type (target type underlying is '{0}') [insert into ABCStream(string) select 'E1' from MyMap]", 
                typeof(Pair<object, IDictionary<string, object>>).GetCleanName()));
    
            text = "insert into xmltype select 1 from SupportBean";
            TryInvalid(epService, text, string.Format(
                "Error starting statement: Event type named 'xmltype' has already been declared with differing column name or type information: Type by name 'xmltype' is not a compatible type (target type underlying is '{0}') [insert into xmltype select 1 from SupportBean]", 
                typeof(XmlNode).GetCleanName()));
    
            text = "insert into MyMap(dummy) select 1 from SupportBean";
            TryInvalid(epService, text, "Error starting statement: Event type named 'MyMap' has already been declared with differing column name or type information: Type by name 'MyMap' expects 10 properties but receives 1 properties [insert into MyMap(dummy) select 1 from SupportBean]");
    
            // setter throws exception
            var stmtTextOne = "insert into SupportBeanErrorTestingTwo(Value) select 'E1' from MyMap";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            var underlying = (SupportBeanErrorTestingTwo) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("default", underlying.Value);
            stmtOne.Dispose();
    
            // surprise - wrong type then defined
            stmtTextOne = "insert into SupportBean(IntPrimitive) select anint from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
            listener.Reset();
            var map = new Dictionary<string, object>();
            map.Put("anint", "notAnInt");
            epService.EPRuntime.SendEvent(map, "MyMap");
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
    
            // ctor throws exception
            epService.EPAdministrator.DestroyAllStatements();
            var stmtTextThree = "insert into SupportBeanCtorOne select 'E1' from SupportBean";
            var stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += listener.Update;
            try {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
                Assert.Fail(); // rethrowing handler registered
            } catch (Exception) {
                // expected
            }
    
            // allow automatic cast of same-type event
            epService.EPAdministrator.CreateEPL("create schema MapOne as (prop1 string)");
            epService.EPAdministrator.CreateEPL("create schema MapTwo as (prop1 string)");
            epService.EPAdministrator.CreateEPL("insert into MapOne select * from MapTwo");
        }
    
        private void RunAssertionPopulateBeanSimple(EPServiceProvider epService)
        {
            var container = epService.Container;

            // test select column names
            var stmtTextOne = "insert into SupportBean select " +
                    "'E1' as TheString, 1 as IntPrimitive, 2 as IntBoxed, 3L as LongPrimitive," +
                    "null as LongBoxed, true as BoolPrimitive, " +
                    "'x' as CharPrimitive, 0xA as BytePrimitive, " +
                    "8.0f as FloatPrimitive, 9.0d as DoublePrimitive, " +
                    "0x05 as ShortPrimitive, SupportEnum.ENUM_VALUE_2 as EnumValue " +
                    " from MyMap";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            var stmtTextTwo = "select * from SupportBean";
            var stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            var received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", received.TheString);
            EPAssertionUtil.AssertPropsPono(
                container, received,
                "IntPrimitive,IntBoxed,LongPrimitive,LongBoxed,BoolPrimitive,CharPrimitive,BytePrimitive,FloatPrimitive,DoublePrimitive,ShortPrimitive,EnumValue".Split(','),
                new object[]{1, 2, 3L, null, true, 'x', (byte) 10, 8f, 9d, (short) 5, SupportEnum.ENUM_VALUE_2});
    
            // test insert-into column names
            stmtOne.Dispose();
            stmtTwo.Dispose();
            listener.Reset();
            stmtTextOne = "insert into SupportBean(" +
                          "TheString, IntPrimitive, IntBoxed, LongPrimitive," +
                          "LongBoxed, BoolPrimitive, CharPrimitive, BytePrimitive, " +
                          "FloatPrimitive, DoublePrimitive, ShortPrimitive, EnumValue) " +
                          "select " +
                          "'E1', 1, 2, 3L," +
                          "null, true, " +
                          "'x', 0xA, " +
                          "8.0f, 9.0d, " +
                          "0x05 as ShortPrimitive, SupportEnum.ENUM_VALUE_2 " +
                          " from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("E1", received.TheString);
            EPAssertionUtil.AssertPropsPono(
                container, received,
                "IntPrimitive,IntBoxed,LongPrimitive,LongBoxed,BoolPrimitive,CharPrimitive,BytePrimitive,FloatPrimitive,DoublePrimitive,ShortPrimitive,EnumValue".Split(','),
                new object[]{1, 2, 3L, null, true, 'x', (byte) 10, 8f, 9d, (short) 5, SupportEnum.ENUM_VALUE_2});
    
            // test convert int? boxed to long boxed
            stmtOne.Dispose();
            listener.Reset();
            stmtTextOne = "insert into SupportBean(LongBoxed, DoubleBoxed) select IntBoxed, FloatBoxed from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            var vals = new Dictionary<string, object>();
            vals.Put("IntBoxed", 4);
            vals.Put("FloatBoxed", 0f);
            epService.EPRuntime.SendEvent(vals, "MyMap");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "LongBoxed,DoubleBoxed".Split(','), new object[]{4L, 0d});
            epService.EPAdministrator.DestroyAllStatements();
    
            // test new-to-map conversion
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithMapFieldSetter));
            var stmt = epService.EPAdministrator.CreateEPL("insert into MyEventWithMapFieldSetter(Id, Themap) " +
                    "select 'test' as id, new {somefield = TheString} as Themap from SupportBean");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap((Map) listener.AssertOneGetNew().Get("Themap"), "somefield".Split(','), "E1");
    
            stmt.Dispose();
        }
    
        private void RunAssertionBeanWildcard(EPServiceProvider epService) {
            var mapDef = new Dictionary<string, object>();
            mapDef.Put("IntPrimitive", typeof(int));
            mapDef.Put("LongBoxed", typeof(long));
            mapDef.Put("TheString", typeof(string));
            mapDef.Put("BoolPrimitive", typeof(bool?));
            epService.EPAdministrator.Configuration.AddEventType("MySupportMap", mapDef);
    
            var stmtTextOne = "insert into SupportBean select * from MySupportMap";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            var vals = new Dictionary<string, object>();
            vals.Put("IntPrimitive", 4);
            vals.Put("LongBoxed", 100L);
            vals.Put("TheString", "E1");
            vals.Put("BoolPrimitive", true);
    
            epService.EPRuntime.SendEvent(vals, "MySupportMap");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(),
                    "IntPrimitive,LongBoxed,TheString,BoolPrimitive".Split(','),
                    new object[]{4, 100L, "E1", true});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionPopulateBeanObjects(EPServiceProvider epService) {
            // arrays and maps
            var stmtTextOne = "insert into SupportBeanComplexProps(ArrayProperty,ObjectArray,MapProperty) " +
                              "select intArr,{10,20,30},mapProp" +
                              " from MyMap as m";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            var mymapVals = new Dictionary<string, object>();
            mymapVals.Put("intArr", new int[]{-1, -2});
            var inner = new Dictionary<string, object>();
            inner.Put("mykey", "myval");
            mymapVals.Put("mapProp", inner);
            epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            var theEvent = (SupportBeanComplexProps) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(-2, theEvent.ArrayProperty[1]);
            Assert.AreEqual(20, theEvent.ObjectArray[1]);
            Assert.AreEqual("myval", theEvent.MapProperty.Get("mykey"));
    
            // inheritance
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanInterfaceProps(isa,isg) select " +
                    "isaImpl,isgImpl" +
                    " from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            mymapVals = new Dictionary<string, object>();
            mymapVals.Put("mapProp", inner);
            epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            Assert.IsTrue(listener.AssertOneGetNewAndReset().Underlying is SupportBeanInterfaceProps);
            Assert.AreEqual(typeof(SupportBeanInterfaceProps), stmtOne.EventType.UnderlyingType);
    
            // object values from Map same type
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanComplexProps(Nested) select nested from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            mymapVals = new Dictionary<string, object>();
            mymapVals.Put("nested", new SupportBeanComplexProps.SupportBeanSpecialGetterNested("111", "222"));
            epService.EPRuntime.SendEvent(mymapVals, "MyMap");
            var eventThree = (SupportBeanComplexProps) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("111", eventThree.Nested.NestedValue);
    
            // object to Object
            stmtOne.Dispose();
            stmtTextOne = "insert into SupportBeanArrayCollMap(anyObject) select nested from SupportBeanComplexProps";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            var eventFour = (SupportBeanArrayCollMap) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual("NestedValue", ((SupportBeanComplexProps.SupportBeanSpecialGetterNested) eventFour.AnyObject).NestedValue);
    
            // test null value
            var stmtTextThree = "insert into SupportBean select 'B' as TheString, IntBoxed as IntPrimitive from SupportBean(TheString='A')";
            var stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            var received = (SupportBean) listener.AssertOneGetNewAndReset().Underlying;
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
            var types = new object[]{typeof(int), typeof(string), typeof(double?), null};
            epService.EPAdministrator.Configuration.AddEventType("MyOAType", props, types);
    
            var schema = SchemaBuilder.Record("MyAvroType",
                    TypeBuilder.RequiredInt("intVal"),
                    TypeBuilder.RequiredString("stringVal"),
                    TypeBuilder.RequiredDouble("doubleVal"),
                    TypeBuilder.Field("nullVal", TypeBuilder.NullType()));

            epService.EPAdministrator.Configuration.AddEventTypeAvro("MyAvroType", new ConfigurationEventTypeAvro(schema));
    
            TryAssertionPopulateUnderlying(epService, "MyMapType");
            TryAssertionPopulateUnderlying(epService, "MyOAType");
            TryAssertionPopulateUnderlying(epService, "MyAvroType");
        }

        private void RunAssertionCharSequenceCompat(EPServiceProvider epService) {
#if NOT_SUPPORTED_IN_DOTNET
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                epService.EPAdministrator.CreateEPL("create " + rep.GetOutputTypeCreateSchemaName() + " schema ConcreteType as (Value " + typeof(IEnumerable<char>).FullName + ")");
                epService.EPAdministrator.CreateEPL("insert into ConcreteType select \"Test\" as Value from SupportBean");
                epService.EPAdministrator.DestroyAllStatements();
                epService.EPAdministrator.Configuration.RemoveEventType("ConcreteType", false);
            }
#endif
        }

        private void RunAssertionBeanFactoryMethod(EPServiceProvider epService) {
            // test factory method on the same event class
            var stmtTextOne = "insert into SupportBeanString select 'abc' as TheString from MyMap";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
            var subscriber = new SupportSubscriber();
            stmtOne.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            Assert.AreEqual("abc", listener.AssertOneGetNewAndReset().Get("TheString"));
            Assert.AreEqual("abc", subscriber.AssertOneGetNewAndReset());
            stmtOne.Dispose();
    
            // test factory method fully-qualified
            stmtTextOne = "insert into SupportSensorEvent(Id, Type, Device, Measurement, Confidence)" +
                    "select 2, 'A01', 'DHC1000', 100, 5 from MyMap";
            stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMap");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "Id,Type,Device,Measurement,Confidence".Split(','), 
                new object[]{2, "A01", "DHC1000", 100.0, 5.0});
    
            try {
                Activator.CreateInstance(typeof(SupportBeanString));
                Assert.Fail();
            } catch (MissingMethodException) {
                // expected
            } catch (Exception) {
                Assert.Fail();
            }
    
            try {
                Activator.CreateInstance(typeof(SupportSensorEvent));
                Assert.Fail();
            } catch (MissingMethodException) {
                // expected
            } catch (Exception) {
                Assert.Fail();
            }
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionArrayPonoInsert(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType("FinalEventInvalidNonArray", typeof(FinalEventInvalidNonArray));
            epService.EPAdministrator.Configuration.AddEventType("FinalEventInvalidArray", typeof(FinalEventInvalidArray));
            epService.EPAdministrator.Configuration.AddEventType("FinalEventValid", typeof(FinalEventValid));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            // Test valid case of array insert
            var validEpl = "INSERT INTO FinalEventValid SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]";
            var stmt = epService.EPAdministrator.CreateEPL(validEpl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
    
            var outEvent = (FinalEventValid) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(1, outEvent.StartEvent.Id);
            Assert.AreEqual("G1", outEvent.StartEvent.P00);
            Assert.AreEqual(2, outEvent.EndEvent.Length);
            Assert.AreEqual(2, outEvent.EndEvent[0].IntPrimitive);
            Assert.AreEqual(3, outEvent.EndEvent[1].IntPrimitive);
    
            // Test invalid case of non-array destination insert
            var invalidEpl = "INSERT INTO FinalEventInvalidNonArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Invalid assignment of column 'EndEvent' of type '" + typeof(SupportBean).GetCleanName() + "[]' to event property 'EndEvent' typed as '" + typeof(SupportBean).GetCleanName() + "', column and parameter types mismatch [INSERT INTO FinalEventInvalidNonArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]]", ex.Message);
            }
    
            // Test invalid case of array destination insert from non-array var
            var invalidEplTwo = "INSERT INTO FinalEventInvalidArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                    "every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEplTwo);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Invalid assignment of column 'StartEvent' of type '" + typeof(SupportBean_S0).GetCleanName() + "' to event property 'StartEvent' typed as '" + typeof(SupportBean_S0).GetCleanName() + "[]', column and parameter types mismatch [INSERT INTO FinalEventInvalidArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(TheString=s.p00) until timer:interval(10 sec)]]", ex.Message);
            }
    
            stmt.Dispose();
            foreach (var name in "FinalEventValid,FinalEventInvalidNonArray,FinalEventInvalidArray".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionArrayMapInsert(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionArrayMapInsert(epService, rep);
            }
        }
    
        private void TryAssertionArrayMapInsert(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventOne(id string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema EventTwo(id string, val int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema FinalEventValid (StartEvent EventOne, EndEvent EventTwo[])");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema FinalEventInvalidNonArray (StartEvent EventOne, EndEvent EventTwo)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema FinalEventInvalidArray (StartEvent EventOne, EndEvent EventTwo)");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            // Test valid case of array insert
            var validEpl = "INSERT INTO FinalEventValid SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
            var stmt = epService.EPAdministrator.CreateEPL(validEpl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventOne(epService, eventRepresentationEnum, "G1");
            SendEventTwo(epService, eventRepresentationEnum, "G1", 2);
            SendEventTwo(epService, eventRepresentationEnum, "G1", 3);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
    
            EventBean startEventOne;
            EventBean endEventOne;
            EventBean endEventTwo;
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                var outArray = (object[]) listener.AssertOneGetNewAndReset().Underlying;
                startEventOne = (EventBean) outArray[0];
                endEventOne = ((EventBean[]) outArray[1])[0];
                endEventTwo = ((EventBean[]) outArray[1])[1];
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var outMap = (Map) listener.AssertOneGetNewAndReset().Underlying;
                startEventOne = (EventBean) outMap.Get("StartEvent");
                endEventOne = ((EventBean[]) outMap.Get("EndEvent"))[0];
                endEventTwo = ((EventBean[]) outMap.Get("EndEvent"))[1];
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var received = listener.AssertOneGetNewAndReset();
                startEventOne = (EventBean) received.GetFragment("StartEvent");
                var endEvents = (EventBean[]) received.GetFragment("EndEvent");
                endEventOne = endEvents[0];
                endEventTwo = endEvents[1];
            } else {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
            Assert.AreEqual("G1", startEventOne.Get("id"));
            Assert.AreEqual(2, endEventOne.Get("val"));
            Assert.AreEqual(3, endEventTwo.Get("val"));
    
            // Test invalid case of non-array destination insert
            var invalidEpl = "INSERT INTO FinalEventInvalidNonArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            } catch (EPException ex) {
                string expected;
                if (eventRepresentationEnum.IsAvroEvent()) {
                    expected = "Error starting statement: Property 'EndEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                } else {
                    expected = "Error starting statement: Event type named 'FinalEventInvalidNonArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidNonArray' in property 'EndEvent' expected event type 'EventTwo' but receives event type 'EventTwo[]'";
                }
                SupportMessageAssertUtil.AssertMessage(ex, expected);
            }
    
            // Test invalid case of array destination insert from non-array var
            invalidEpl = "INSERT INTO FinalEventInvalidArray SELECT s as StartEvent, e as EndEvent FROM PATTERN [" +
                    "every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
            try {
                epService.EPAdministrator.CreateEPL(invalidEpl);
                Assert.Fail();
            } catch (EPException ex) {
                string expected;
                if (eventRepresentationEnum.IsAvroEvent()) {
                    expected = "Error starting statement: Property 'EndEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
                } else {
                    expected = "Error starting statement: Event type named 'FinalEventInvalidArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidArray' in property 'EndEvent' expected event type 'EventTwo' but receives event type 'EventTwo[]'";
                }
                SupportMessageAssertUtil.AssertMessage(ex, expected);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "EventOne,EventTwo,FinalEventValid,FinalEventInvalidNonArray,FinalEventInvalidArray".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void SendEventTwo(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string id, int val) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{id, val}, "EventTwo");
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
                epService.EPRuntime.SendEvent(new object[]{id}, "EventOne");
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
            var theEvent = (SupportBeanCtorOne) listener.AssertOneGetNewAndReset().Underlying;
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
            var theEvent = (SupportBeanCtorOne) listener.AssertOneGetNewAndReset().Underlying;
            Assert.AreEqual(theString, theEvent.TheString);
            Assert.AreEqual(intBoxed, theEvent.IntBoxed);
            Assert.AreEqual(boolPrimitive, theEvent.IsBoolPrimitive);
            Assert.AreEqual(intPrimitive, theEvent.IntPrimitive);
        }

        private void TryAssertionPopulateUnderlying(
            EPServiceProvider epService, 
            string typeName)
        {
            var stmtOrig = epService.EPAdministrator.CreateEPL("select * from " + typeName);
    
            var stmtTextOne = "insert into " + typeName + " select IntPrimitive as intVal, TheString as stringVal, DoubleBoxed as doubleVal from SupportBean";
            var stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
            Assert.AreSame(stmtOrig.EventType, stmtOne.EventType);
    
            var bean = new SupportBean();
            bean.IntPrimitive = 1000;
            bean.TheString = "E1";
            bean.DoubleBoxed = 1001d;
            epService.EPRuntime.SendEvent(bean);
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "intVal,stringVal,doubleVal".Split(','), new object[]{1000, "E1", 1001d});
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
