///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

// using static org.apache.avro.SchemaBuilder.record;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    using Map = IDictionary<string, object>;

    public class ExecInsertIntoTransposeStream : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("generateMap", GetType(), "LocalGenerateMap");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("generateOA", GetType(), "LocalGenerateOA");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("generateAvro", GetType(), "LocalGenerateAvro");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("custom", typeof(SupportStaticMethodLib), "MakeSupportBean");
    
            RunAssertionTransposeMapAndObjectArray(epService);
            RunAssertionTransposeFunctionToStreamWithProps(epService);
            RunAssertionTransposeFunctionToStream(epService);
            RunAssertionTransposeSingleColumnInsert(epService);
            RunAssertionTransposeEventJoinMap(epService);
            RunAssertionTransposeEventJoinPOJO(epService);
            RunAssertionTransposePOJOPropertyStream(epService);
            RunAssertionInvalidTranspose(epService);
        }
    
        private void RunAssertionTransposeMapAndObjectArray(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunTransposeMapAndObjectArray(epService, rep);
            }
        }
    
        private void RunTransposeMapAndObjectArray(EPServiceProvider epService, EventRepresentationChoice representation) {
    
            string[] fields = "p0,p1".Split(',');
            epService.EPAdministrator.CreateEPL("create " + representation.GetOutputTypeCreateSchemaName() + " schema MySchema(p0 string, p1 int)");
    
            string generateFunction;
            if (representation.IsObjectArrayEvent()) {
                generateFunction = "generateOA";
            } else if (representation.IsMapEvent()) {
                generateFunction = "generateMap";
            } else if (representation.IsAvroEvent()) {
                generateFunction = "generateAvro";
            } else {
                throw new IllegalStateException("Unrecognized code " + representation);
            }
            string epl = "insert into MySchema select Transpose(" + generateFunction + "(TheString, IntPrimitive)) from SupportBean";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl, "first").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            // MySchema already exists, start second statement
            epService.EPAdministrator.CreateEPL(epl, "second").Events += listener.Update;
            epService.EPAdministrator.GetStatement("first").Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
    
            epService.EPAdministrator.Configuration.RemoveEventType("MySchema", true);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTransposeFunctionToStreamWithProps(EPServiceProvider epService) {
            string stmtTextOne = "insert into MyStream select 1 as dummy, Transpose(Custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select * from MyStream";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(Pair<object, Map>), type.UnderlyingType);
    
            epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EventBean result = listener.AssertOneGetNewAndReset();
            var underlying = (Pair<object, Map>) result.Underlying;
            EPAssertionUtil.AssertProps(result, "dummy,TheString,IntPrimitive".Split(','), new object[]{1, "OI1", 10});
            Assert.AreEqual("OI1", ((SupportBean) underlying.First).TheString);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTransposeFunctionToStream(EPServiceProvider epService) {
            string stmtTextOne = "insert into OtherStream select Transpose(Custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
            epService.EPAdministrator.CreateEPL(stmtTextOne, "first");
    
            string stmtTextTwo = "select * from OtherStream(TheString like 'O%')";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
    
            epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EventBean result = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(result, "TheString,IntPrimitive".Split(','), new object[]{"OI1", 10});
            Assert.AreEqual("OI1", ((SupportBean) result.Underlying).TheString);
    
            // try second statement as "OtherStream" now already exists
            epService.EPAdministrator.CreateEPL(stmtTextOne, "second");
            epService.EPAdministrator.GetStatement("first").Dispose();
            epService.EPRuntime.SendEvent(new SupportBean("I2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString,IntPrimitive".Split(','), new object[]{"OI2", 10});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTransposeSingleColumnInsert(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanNumeric));
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("customOne", typeof(SupportStaticMethodLib), "MakeSupportBean");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("customTwo", typeof(SupportStaticMethodLib), "MakeSupportBeanNumeric");
    
            // with transpose and same input and output
            string stmtTextOne = "insert into SupportBean select Transpose(CustomOne('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            Assert.AreEqual(typeof(SupportBean), stmtOne.EventType.UnderlyingType);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EventBean resultOne = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(resultOne, "TheString,IntPrimitive".Split(','), new object[]{"OI1", 10});
            Assert.AreEqual("OI1", ((SupportBean) resultOne.Underlying).TheString);
            stmtOne.Dispose();
    
            // with transpose but different input and output (also test ignore column name)
            string stmtTextTwo = "insert into SupportBeanNumeric select Transpose(CustomTwo(IntPrimitive, IntPrimitive+1)) as col1 from SupportBean(TheString like 'I%')";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            Assert.AreEqual(typeof(SupportBeanNumeric), stmtTwo.EventType.UnderlyingType);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("I2", 10));
            EventBean resultTwo = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(resultTwo, "intOne,intTwo".Split(','), new object[]{10, 11});
            Assert.AreEqual(11, (int) ((SupportBeanNumeric) resultTwo.Underlying).IntTwo);
            stmtTwo.Dispose();
    
            // invalid wrong-bean target
            try {
                epService.EPAdministrator.CreateEPL("insert into SupportBeanNumeric select Transpose(CustomOne('O', 10)) from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Expression-returned value of type '" + Name.Clean<SupportBean>() + "' cannot be converted to target event type 'SupportBeanNumeric' with underlying type '" + Name.Clean<SupportBeanNumeric>() + "' [insert into SupportBeanNumeric select Transpose(CustomOne('O', 10)) from SupportBean]", ex.Message);
            }
    
            // invalid additional properties
            try {
                epService.EPAdministrator.CreateEPL("insert into SupportBean select 1 as dummy, Transpose(CustomOne('O', 10)) from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot transpose additional properties in the select-clause to target event type 'SupportBean' with underlying type '" + Name.Clean<SupportBean>() + "', the transpose function must occur alone in the select clause [insert into SupportBean select 1 as dummy, Transpose(CustomOne('O', 10)) from SupportBean]", ex.Message);
            }
    
            // invalid occurs twice
            try {
                epService.EPAdministrator.CreateEPL("insert into SupportBean select Transpose(CustomOne('O', 10)), Transpose(CustomOne('O', 11)) from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [insert into SupportBean select Transpose(CustomOne('O', 10)), Transpose(CustomOne('O', 11)) from SupportBean]", ex.Message);
            }
    
            // invalid wrong-type target
            try {
                epService.EPAdministrator.Configuration.AddEventType("SomeOtherStream", new Dictionary<string, object>());
                epService.EPAdministrator.CreateEPL("insert into SomeOtherStream select Transpose(CustomOne('O', 10)) from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Expression-returned value of type '" + Name.Clean<SupportBean>() + "' cannot be converted to target event type 'SomeOtherStream' with underlying type '" + Name.Clean<Map>() + "' [insert into SomeOtherStream select Transpose(CustomOne('O', 10)) from SupportBean]", ex.Message);
            }
    
            // invalid two parameters
            try {
                epService.EPAdministrator.CreateEPL("select Transpose(CustomOne('O', 10), CustomOne('O', 10)) from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'Transpose(CustomOne(\"O\",10),CustomO...(46 chars)': The transpose function requires a single parameter expression [select Transpose(CustomOne('O', 10), CustomOne('O', 10)) from SupportBean]", ex.Message);
            }
    
            // test not a top-level function or used in where-clause (possible but not useful)
            epService.EPAdministrator.CreateEPL("select * from SupportBean where Transpose(CustomOne('O', 10)) is not null");
            epService.EPAdministrator.CreateEPL("select Transpose(CustomOne('O', 10)) is not null from SupportBean");
    
            // invalid insert of object-array into undefined stream
            try {
                epService.EPAdministrator.CreateEPL("insert into SomeOther select Transpose(GenerateOA('a', 1)) from SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Invalid expression return type '" + Name.Clean<object[]>() + "' for transpose function [insert into SomeOther select Transpose(GenerateOA('a', 1)) from SupportBean]", ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTransposeEventJoinMap(EPServiceProvider epService) {
            IDictionary<string, Object> metadata = MakeMap(new object[][]{new object[] {"id", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("AEventTE", metadata);
            epService.EPAdministrator.Configuration.AddEventType("BEventTE", metadata);
    
            string stmtTextOne = "insert into MyStreamTE select a, b from AEventTE#keepall as a, BEventTE#keepall as b";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select a.id, b.id from MyStreamTE";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            IDictionary<string, Object> eventOne = MakeMap(new object[][]{new object[] {"id", "A1"}});
            IDictionary<string, Object> eventTwo = MakeMap(new object[][]{new object[] {"id", "B1"}});
            epService.EPRuntime.SendEvent(eventOne, "AEventTE");
            epService.EPRuntime.SendEvent(eventTwo, "BEventTE");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new object[]{"A1", "B1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTransposeEventJoinPOJO(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("AEventBean", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("BEventBean", typeof(SupportBean_B));
    
            string stmtTextOne = "insert into MyStream2Bean select a.* as a, b.* as b from AEventBean#keepall as a, BEventBean#keepall as b";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select a.id, b.id from MyStream2Bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new object[]{"A1", "B1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTransposePOJOPropertyStream(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("Complex", typeof(SupportBeanComplexProps));
    
            string stmtTextOne = "insert into MyStreamComplex select nested as inneritem from Complex";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select inneritem.nestedValue as result from MyStreamComplex";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "result".Split(','), new object[]{ "NestedValue" });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalidTranspose(EPServiceProvider epService) {
            IDictionary<string, Object> metadata = MakeMap(new object[][]{
                new object[] {"nested", MakeMap(new object[][]{new object[] {"nestedValue", typeof(string)}})}
            });
            epService.EPAdministrator.Configuration.AddEventType("ComplexMap", metadata);
    
            string stmtTextOne = "insert into MyStreamComplexMap select nested as inneritem from ComplexMap";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            try {
                string stmtTextTwo = "select inneritem.nestedValue as result from MyStreamComplexMap";
                epService.EPAdministrator.CreateEPL(stmtTextTwo);
            } catch (Exception ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'inneritem.nestedValue': Failed to resolve property 'inneritem.nestedValue' (property 'inneritem' is a mapped property and requires keyed access) [select inneritem.nestedValue as result from MyStreamComplexMap]", ex.Message);
            }
    
            // test invalid unwrap-properties
            epService.EPAdministrator.Configuration.AddEventType(typeof(E1));
            epService.EPAdministrator.Configuration.AddEventType(typeof(E2));
            epService.EPAdministrator.Configuration.AddEventType(typeof(EnrichedE2));
    
            try {
                epService.EPAdministrator.CreateEPL("@Resilient insert into EnrichedE2 " +
                        "select e2.* as event, e1.otherId as playerId " +
                        "from E1#length(20) as e1, E2#length(1) as e2 " +
                        "where e1.id = e2.id ");
            } catch (Exception ex) {
                Assert.AreEqual("Error starting statement: The 'e2.* as event' syntax is not allowed when inserting into an existing bean event type, use the 'e2 as event' syntax instead [@Resilient insert into EnrichedE2 select e2.* as event, e1.otherId as playerId from E1#length(20) as e1, E2#length(1) as e2 where e1.id = e2.id ]", ex.Message);
            }
        }
    
        public static Map LocalGenerateMap(string @string, int intPrimitive) {
            var @out = new Dictionary<string, Object>();
            @out.Put("p0", @string);
            @out.Put("p1", intPrimitive);
            return @out;
        }
    
        public static object[] LocalGenerateOA(string @string, int intPrimitive) {
            return new object[]{@string, intPrimitive};
        }
    
        public static GenericRecord LocalGenerateAvro(string @string, int intPrimitive) {
            var schema = SchemaBuilder.Record("name",
                TypeBuilder.RequiredString("p0"),
                TypeBuilder.RequiredInt("p1"));
            var record = new GenericRecord(schema);
            record.Put("p0", @string);
            record.Put("p1", intPrimitive);
            return record;
        }
    
        private IDictionary<string, Object> MakeMap(object[][] entries) {
            var result = new Dictionary<string, Object>();
            foreach (object[] entry in entries) {
                result.Put((string) entry[0], entry[1]);
            }
            return result;
        }
    
        [Serializable]
        public class E1
        {
            public string Id { get; }
            public string OtherId { get; }
            public E1(string id, string otherId)
            {
                this.Id = id;
                this.OtherId = otherId;
            }
        }
    
        [Serializable]
        public class E2
        {
            public string Id { get; }
            public string Value { get; }
            public E2(string id, string value)
            {
                this.Id = id;
                this.Value = value;
            }
        }
    
        [Serializable]
        public class EnrichedE2
        {
            public E2 Event { get; }
            public string OtherId { get; }

            public EnrichedE2(E2 @event, string playerId) {
                this.Event = @event;
                this.OtherId = playerId;
            }
        }
    }
} // end of namespace
