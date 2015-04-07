///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestInsertIntoTransposeStream
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("generateMap", GetType().FullName, "LocalGenerateMap");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("generateOA", GetType().FullName, "LocalGenerateOA");
  
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestTransposeMapAndObjectArray() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            RunTransposeMapAndObjectArray(EventRepresentationEnum.OBJECTARRAY);
            RunTransposeMapAndObjectArray(EventRepresentationEnum.MAP);
        }

        private void RunTransposeMapAndObjectArray(EventRepresentationEnum representation)
        {
            String[] fields = "p0,p1".Split(',');
            _epService.EPAdministrator.CreateEPL("create " + representation.GetOutputTypeCreateSchemaName() + " schema MySchema(p0 string, p1 int)");

            String generateFunction = representation == EventRepresentationEnum.MAP ? "generateMap" : "generateOA";
            String epl = "@Name('first') insert into MySchema select transpose(" + generateFunction + "(TheString, IntPrimitive)) from SupportBean";
            _epService.EPAdministrator.CreateEPL(epl).Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1});

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2});

            // MySchema already exists, start second statement
            _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);
            _epService.EPAdministrator.GetStatement("first").Dispose();

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E3", 3 });

            _epService.EPAdministrator.Configuration.RemoveEventType("MySchema", true);
            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestTransposeFunctionToStreamWithProps()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("custom", typeof(SupportStaticMethodLib).FullName, "MakeSupportBean");

            String stmtTextOne = "insert into MyStream select 1 as dummy, Transpose(custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);

            String stmtTextTwo = "select * from MyStream";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(Pair<string, object>), type.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EventBean result = _listener.AssertOneGetNewAndReset();
            Pair<object, DataMap> underlying = (Pair<object, DataMap>)result.Underlying;
            EPAssertionUtil.AssertProps(result, "dummy,TheString,IntPrimitive".Split(','), new Object[] { 1, "OI1", 10 });
            Assert.AreEqual("OI1", ((SupportBean)underlying.First).TheString);
        }

        [Test]
        public void TestTransposeFunctionToStream()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("custom", typeof(SupportStaticMethodLib).FullName, "MakeSupportBean");

            String stmtTextOne = "insert into OtherStream select transpose(custom('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
            _epService.EPAdministrator.CreateEPL(stmtTextOne, "first");

            String stmtTextTwo = "select * from OtherStream(TheString like 'O%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

            _epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EventBean result = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(result, "TheString,IntPrimitive".Split(','), new Object[] { "OI1", 10 });
            Assert.AreEqual("OI1", ((SupportBean)result.Underlying).TheString);

            // try second statement as "OtherStream" now already exists
            _epService.EPAdministrator.CreateEPL(stmtTextOne, "second");
            _epService.EPAdministrator.GetStatement("first").Dispose();
            _epService.EPRuntime.SendEvent(new SupportBean("I2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString,IntPrimitive".Split(','), new Object[] { "OI2", 10 });
        }

        [Test]
        public void TestTransposeSingleColumnInsert()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanNumeric>();
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("customOne", typeof(SupportStaticMethodLib).FullName, "MakeSupportBean");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("customTwo", typeof(SupportStaticMethodLib).FullName, "MakeSupportBeanNumeric");

            // with transpose and same input and output
            String stmtTextOne = "insert into SupportBean select Transpose(customOne('O' || TheString, 10)) from SupportBean(TheString like 'I%')";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            Assert.AreEqual(typeof(SupportBean), stmtOne.EventType.UnderlyingType);
            stmtOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EventBean resultOne = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(resultOne, "TheString,IntPrimitive".Split(','), new Object[] { "OI1", 10 });
            Assert.AreEqual("OI1", ((SupportBean)resultOne.Underlying).TheString);
            stmtOne.Dispose();

            // with transpose but different input and output (also test ignore column name)
            String stmtTextTwo = "insert into SupportBeanNumeric select Transpose(customTwo(IntPrimitive, IntPrimitive+1)) as col1 from SupportBean(TheString like 'I%')";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            Assert.AreEqual(typeof(SupportBeanNumeric), stmtTwo.EventType.UnderlyingType);
            stmtTwo.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("I2", 10));
            EventBean resultTwo = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(resultTwo, "intOne,intTwo".Split(','), new Object[] { 10, 11 });
            Assert.AreEqual(11, (int)((SupportBeanNumeric)resultTwo.Underlying).IntTwo);
            stmtTwo.Dispose();

            // invalid wrong-bean target
            try
            {
                _epService.EPAdministrator.CreateEPL("insert into SupportBeanNumeric select Transpose(customOne('O', 10)) from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Expression-returned value of type 'com.espertech.esper.support.bean.SupportBean' cannot be converted to target event type 'SupportBeanNumeric' with underlying type 'com.espertech.esper.support.bean.SupportBeanNumeric' [insert into SupportBeanNumeric select Transpose(customOne('O', 10)) from SupportBean]", ex.Message);
            }

            // invalid additional properties
            try
            {
                _epService.EPAdministrator.CreateEPL("insert into SupportBean select 1 as dummy, Transpose(customOne('O', 10)) from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Cannot transpose additional properties in the select-clause to target event type 'SupportBean' with underlying type 'com.espertech.esper.support.bean.SupportBean', the transpose function must occur alone in the select clause [insert into SupportBean select 1 as dummy, Transpose(customOne('O', 10)) from SupportBean]", ex.Message);
            }

            // invalid occurs twice
            try
            {
                _epService.EPAdministrator.CreateEPL("insert into SupportBean select Transpose(customOne('O', 10)), Transpose(customOne('O', 11)) from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [insert into SupportBean select Transpose(customOne('O', 10)), Transpose(customOne('O', 11)) from SupportBean]", ex.Message);
            }

            // invalid wrong-type target
            try
            {
                _epService.EPAdministrator.Configuration.AddEventType("SomeOtherStream", new Dictionary<String, Object>());
                _epService.EPAdministrator.CreateEPL("insert into SomeOtherStream select Transpose(customOne('O', 10)) from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Expression-returned value of type 'com.espertech.esper.support.bean.SupportBean' cannot be converted to target event type 'SomeOtherStream' with underlying type '" + Name.Of<IDictionary<string, object>>() + "' [insert into SomeOtherStream select Transpose(customOne('O', 10)) from SupportBean]", ex.Message);
            }

            // invalid two parameters
            try
            {
                _epService.EPAdministrator.CreateEPL("select transpose(customOne('O', 10), customOne('O', 10)) from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'transpose(customOne(\"O\",10),customO...(46 chars)': The transpose function requires a single parameter expression [select transpose(customOne('O', 10), customOne('O', 10)) from SupportBean]", ex.Message);
            }

            // test not a top-level function or used in where-clause (possible but not useful)
            _epService.EPAdministrator.CreateEPL("select * from SupportBean where Transpose(customOne('O', 10)) is not null");
            _epService.EPAdministrator.CreateEPL("select Transpose(customOne('O', 10)) is not null from SupportBean");


            // invalid insert of object-array into undefined stream
            try
            {
                _epService.EPAdministrator.CreateEPL("insert into SomeOther select transpose(generateOA('a', 1)) from SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Invalid expression return type '" + Name.Of<object[]>() + "' for transpose function [insert into SomeOther select transpose(generateOA('a', 1)) from SupportBean]", ex.Message);
            }
        }

        [Test]
        public void TestTransposeEventJoinMap()
        {
            IDictionary<String, Object> metadata = MakeMap(new Object[][] { new Object[] { "id", typeof(string) } });
            _epService.EPAdministrator.Configuration.AddEventType("AEvent", metadata);
            _epService.EPAdministrator.Configuration.AddEventType("BEvent", metadata);

            String stmtTextOne = "insert into MyStream select a, b from AEvent.win:keepall() as a, BEvent.win:keepall() as b";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);

            String stmtTextTwo = "select a.id, b.id from MyStream";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;

            IDictionary<String, Object> eventOne = MakeMap(new Object[][] { new Object[] { "id", "A1" } });
            IDictionary<String, Object> eventTwo = MakeMap(new Object[][] { new Object[] { "id", "B1" } });
            _epService.EPRuntime.SendEvent(eventOne, "AEvent");
            _epService.EPRuntime.SendEvent(eventTwo, "BEvent");

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new Object[] { "A1", "B1" });
        }

        [Test]
        public void TestTransposeEventJoinPONO()
        {
            _epService.EPAdministrator.Configuration.AddEventType("AEvent", typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType("BEvent", typeof(SupportBean_B));

            String stmtTextOne = "insert into MyStream select a.* as a, b.* as b from AEvent.win:keepall() as a, BEvent.win:keepall() as b";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);

            String stmtTextTwo = "select a.id, b.id from MyStream";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new Object[] { "A1", "B1" });
        }

        [Test]
        public void TestTransposePONOPropertyStream()
        {
            _epService.EPAdministrator.Configuration.AddEventType("Complex", typeof(SupportBeanComplexProps));

            String stmtTextOne = "insert into MyStream select Nested as inneritem from Complex";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);

            String stmtTextTwo = "select inneritem.NestedValue as result from MyStream";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "result".Split(','), new Object[] { "NestedValue" });
        }

        [Test]
        public void TestInvalidTranspose()
        {
            IDictionary<String, Object> metadata = MakeMap(new Object[][] {
                    new Object[] {"Nested", MakeMap(new Object[][] { new Object[] {"NestedValue", typeof(string)}}) }
            });
            _epService.EPAdministrator.Configuration.AddEventType("Complex", metadata);

            String stmtTextOne = "insert into MyStream select Nested as inneritem from Complex";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);

            try
            {
                String stmtTextTwo = "select inneritem.NestedValue as result from MyStream";
                _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'inneritem.NestedValue': Failed to resolve property 'inneritem.NestedValue' (property 'inneritem' is a mapped property and requires keyed access) [select inneritem.NestedValue as result from MyStream]", ex.Message);
            }

            // test invalid unwrap-properties
            _epService.EPAdministrator.Configuration.AddEventType<E1>();
            _epService.EPAdministrator.Configuration.AddEventType<E2>();
            _epService.EPAdministrator.Configuration.AddEventType<EnrichedE2>();

            try
            {
                _epService.EPAdministrator.CreateEPL("@Resilient insert into EnrichedE2 " +
                        "select e2.* as event, e1.otherId as playerId " +
                        "from E1.win:length(20) as e1, E2.win:length(1) as e2 " +
                        "where e1.id = e2.id ");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Error starting statement: The 'e2.* as event' syntax is not allowed when inserting into an existing bean event type, use the 'e2 as event' syntax instead [@Resilient insert into EnrichedE2 select e2.* as event, e1.otherId as playerId from E1.win:length(20) as e1, E2.win:length(1) as e2 where e1.id = e2.id ]", ex.Message);
            }
        }

        public static IDictionary<string, object> LocalGenerateMap(String @string, int intPrimitive)
        {
            var @out = new Dictionary<String, Object>();
            @out.Put("p0", @string);
            @out.Put("p1", intPrimitive);
            return @out;
        }

        public static Object[] LocalGenerateOA(String @string, int intPrimitive)
        {
            return new Object[] {@string, intPrimitive};
        }

        private IDictionary<String, Object> MakeMap(Object[][] entries)
        {
            var result = new Dictionary<String, Object>();
            foreach (Object[] entry in entries)
            {
                result.Put((string)entry[0], entry[1]);
            }
            return result;
        }

        [Serializable]
        public class E1
        {
            public E1(String id, String otherId)
            {
                Id = id;
                OtherId = otherId;
            }

            public string Id { get; private set; }
            public string OtherId { get; private set; }
        }

        [Serializable]
        public class E2
        {
            public E2(String id, String value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; private set; }
            public string Value { get; private set; }
        }

        [Serializable]
        public class EnrichedE2
        {
            public EnrichedE2(E2 @event, String playerId)
            {
                Event = @event;
                OtherId = playerId;
            }

            public E2 Event { get; private set; }
            public string OtherId { get; private set; }
        }
    }
}
