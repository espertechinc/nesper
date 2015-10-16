///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTIntervalOps
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
            _listener = new SupportUpdateListener();
    
            var configBean = new ConfigurationEventTypeLegacy();
            configBean.StartTimestampPropertyName = "MsecdateStart";
            configBean.EndTimestampPropertyName = "MsecdateEnd";
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).FullName, configBean);
            _epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB).FullName, configBean);
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestCreateSchema() {
            RunAssertionCreateSchema(EventRepresentationEnum.DEFAULT);
            RunAssertionCreateSchema(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionCreateSchema(EventRepresentationEnum.MAP);
        }
    
        private void RunAssertionCreateSchema(EventRepresentationEnum eventRepresentationEnum) {
    
            // test Map type Long-type timestamps
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeA as (startts long, endts long) starttimestamp startts endtimestamp endts");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeB as (startts long, endts long) starttimestamp startts endtimestamp endts");
    
            var stmt = _epService.EPAdministrator.CreateEPL("select a.includes(b) as val0 from TypeA.std:lastevent() as a, TypeB.std:lastevent() as b");
            stmt.Events += _listener.Update;
    
            MakeSendEvent("TypeA", eventRepresentationEnum, DateTimeParser.ParseDefaultMSec("2002-05-30T9:00:00.000"), DateTimeParser.ParseDefaultMSec("2002-05-30T9:00:01.000"));
            MakeSendEvent("TypeB", eventRepresentationEnum, DateTimeParser.ParseDefaultMSec("2002-05-30T9:00:00.500"), DateTimeParser.ParseDefaultMSec("2002-05-30T9:00:00.700"));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("val0"));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("TypeA", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("TypeB", true);
    
            // test Map type Calendar-type timestamps
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeA as (startts System.DateTime, endts System.DateTime) starttimestamp startts endtimestamp endts");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeB as (startts System.DateTime, endts System.DateTime) starttimestamp startts endtimestamp endts");
    
            stmt = _epService.EPAdministrator.CreateEPL("select a.includes(b) as val0 from TypeA.std:lastevent() as a, TypeB.std:lastevent() as b");
            stmt.Events += _listener.Update;

            MakeSendEvent("TypeA", eventRepresentationEnum, DateTimeParser.ParseDefault("2002-05-30T9:00:00.000"), DateTimeParser.ParseDefault("2002-05-30T9:00:01.000"));
            MakeSendEvent("TypeB", eventRepresentationEnum, DateTimeParser.ParseDefault("2002-05-30T9:00:00.500"), DateTimeParser.ParseDefault("2002-05-30T9:00:00.700"));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("val0"));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("TypeA", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("TypeB", true);
    
            // test Map type Date-type timestamps
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeA as (startts System.DateTime, endts System.DateTime) starttimestamp startts endtimestamp endts");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeB as (startts System.DateTime, endts System.DateTime) starttimestamp startts endtimestamp endts");
    
            stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select a.includes(b) as val0 from TypeA.std:lastevent() as a, TypeB.std:lastevent() as b");
            stmt.Events += _listener.Update;

            MakeSendEvent("TypeA", eventRepresentationEnum, DateTimeParser.ParseDefaultDate("2002-05-30T9:00:00.000"), DateTimeParser.ParseDefaultDate("2002-05-30T9:00:01.000"));
            MakeSendEvent("TypeB", eventRepresentationEnum, DateTimeParser.ParseDefaultDate("2002-05-30T9:00:00.500"), DateTimeParser.ParseDefaultDate("2002-05-30T9:00:00.700"));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("val0"));
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test Bean-type Date-type timestamps
            var epl = eventRepresentationEnum.GetAnnotationText() + " create schema SupportBean as " + typeof(SupportBean).FullName + " starttimestamp LongPrimitive endtimestamp LongBoxed";
            _epService.EPAdministrator.CreateEPL(epl);
    
            stmt = _epService.EPAdministrator.CreateEPL("select a.get('month') as val0 from SupportBean a");
            stmt.Events += _listener.Update;
    
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = DateTimeParser.ParseDefaultMSec("2002-05-30T9:00:00.000");
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(5, _listener.AssertOneGetNewAndReset().Get("val0"));
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl.Trim(), stmt.Text);
            
            // try XML
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "ABC";
            desc.StartTimestampPropertyName = "mystarttimestamp";
            desc.EndTimestampPropertyName = "myendtimestamp";
            desc.AddXPathProperty("mystarttimestamp", "/test/prop", XPathResultType.Number);
            try {
                _epService.EPAdministrator.Configuration.AddEventType("TypeXML", desc);
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual("Declared start timestamp property 'mystarttimestamp' is expected to return a DateTime or long-typed value but returns '" + Name.Of<double>() + "'", ex.Message);
            }
    
            _epService.Initialize();
        }
    
        private void MakeSendEvent(string typeName, EventRepresentationEnum eventRepresentationEnum, object startTs, object endTs) {
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("startts", startTs);
            theEvent.Put("endts", endTs);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), typeName);
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, typeName);
            }
        }
    
        [Test]
        public void TestCalendarOps() {
            var seedTime = "2002-05-30T9:00:00.000"; // seed is time for B
    
            object[][] expected = {
                    new object[] {"2999-01-01T9:00:00.001", 0, true} // sending in A
            };
            AssertExpression(seedTime, 0, "a.withDate(2001, 1, 1).before(b)", expected, null);
    
            expected = new object[][] {
                    new object[] {"2999-01-01T10:00:00.001", 0, false},
                    new object[] {"2999-01-01T8:00:00.001", 0, true}
            };
            AssertExpression(seedTime, 0, "a.withDate(2001, 1, 1).before(b.withDate(2001, 1, 1))", expected, null);
    
            // Test end-timestamp preserved when using calendar ops
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 2000, false}
            };
            AssertExpression(seedTime, 0, "a.before(b)", expected, null);
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 2000, false}
            };
            AssertExpression(seedTime, 0, "a.withTime(8, 59, 59, 0).before(b)", expected, null);
    
            // Test end-timestamp preserved when using calendar ops
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:01.000", 0, false},
                    new object[] {"2002-05-30T9:00:01.001", 0, true}
            };
            AssertExpression(seedTime, 1000, "a.after(b)", expected, null);
    
            // NOT YET SUPPORTED (a documented limitation of datetime methods)
            // assertExpression(seedTime, 0, "a.after(b.withTime(9, 0, 0, 0))", expected, null);   // the "b.withTime(...) must retain the end-timestamp correctness (a documented limitation)
        }
    
        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>("SupportBean");
    
            // wrong 1st parameter - string
            TryInvalid("select a.before('x') from A as a",
                       "Error starting statement: Failed to validate select-clause expression 'a.before('x')': Failed to resolve enumeration method, date-time method or mapped property 'a.before('x')': For date-time method 'before' the first parameter expression returns 'System.String', however requires a DateTime or Long-type return value or event (with timestamp) [select a.before('x') from A as a]");
    
            // wrong 1st parameter - event not defined with timestamp expression
            TryInvalid("select a.before(b) from A.std:lastevent() as a, SupportBean.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.before(b)': For date-time method 'before' the first parameter is event type 'SupportBean', however no timestamp property has been defined for this event type [select a.before(b) from A.std:lastevent() as a, SupportBean.std:lastevent() as b]");
    
            // wrong 1st parameter - boolean
            TryInvalid("select a.before(true) from A.std:lastevent() as a, SupportBean.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.before(true)': For date-time method 'before' the first parameter expression returns '" + Name.Of<bool>() + "', however requires a DateTime or Long-type return value or event (with timestamp) [select a.before(true) from A.std:lastevent() as a, SupportBean.std:lastevent() as b]");
    
            // wrong zero parameters
            TryInvalid("select a.before() from A.std:lastevent() as a, SupportBean.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.before()': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives no parameters [select a.before() from A.std:lastevent() as a, SupportBean.std:lastevent() as b]");
    
            // wrong target
            TryInvalid("select TheString.before(a) from A.std:lastevent() as a, SupportBean.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'TheString.before(a)': Date-time enumeration method 'before' requires either a DateTime or long value as input or events of an event type that declares a timestamp property but received System.String [select TheString.before(a) from A.std:lastevent() as a, SupportBean.std:lastevent() as b]");
            TryInvalid("select b.before(a) from A.std:lastevent() as a, SupportBean.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'b.before(a)': Date-time enumeration method 'before' requires either a DateTime or long value as input or events of an event type that declares a timestamp property [select b.before(a) from A.std:lastevent() as a, SupportBean.std:lastevent() as b]");
            TryInvalid("select a.get('month').before(a) from A.std:lastevent() as a, SupportBean.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.get(\"month\").before(a)': Invalid input for date-time method 'before' [select a.get('month').before(a) from A.std:lastevent() as a, SupportBean.std:lastevent() as b]");
    
            // test before/after
            TryInvalid("select a.before(b, 'abc') from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.before(b,\"abc\")': Error validating date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 1 but received System.String [select a.before(b, 'abc') from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.before(b, 1, 'def') from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.before(b,1,\"def\")': Error validating date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 2 but received System.String [select a.before(b, 1, 'def') from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.before(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.before(b,1,2,3)': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives 4 expressions [select a.before(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b]");
    
            // test coincides
            TryInvalid("select a.coincides(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.coincides(b,1,2,3)': Parameters mismatch for date-time method 'coincides', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing threshold for start and end value, or an expression providing timestamp or timestamped-event and an expression providing threshold for start value and an expression providing threshold for end value, but receives 4 expressions [select a.coincides(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.coincides(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.coincides(b,-1)': The coincides date-time method does not allow negative start and end values [select a.coincides(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
    
            // test during+interval
            TryInvalid("select a.during(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.during(b,1,2,3)': Parameters mismatch for date-time method 'during', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance start and an expression providing maximum distance start and an expression providing minimum distance end and an expression providing maximum distance end, but receives 4 expressions [select a.during(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b]");
    
            // test finishes+finished-by
            TryInvalid("select a.finishes(b, 1, 2) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.finishes(b,1,2)': Parameters mismatch for date-time method 'finishes', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between end timestamps, but receives 3 expressions [select a.finishes(b, 1, 2) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.finishes(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.finishes(b,-1)': The finishes date-time method does not allow negative threshold value [select a.finishes(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.finishedby(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.finishedby(b,-1)': The finishedby date-time method does not allow negative threshold value [select a.finishedby(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
    
            // test meets+met-by
            TryInvalid("select a.meets(b, 1, 2) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.meets(b,1,2)': Parameters mismatch for date-time method 'meets', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start and end timestamps, but receives 3 expressions [select a.meets(b, 1, 2) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.meets(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.meets(b,-1)': The meets date-time method does not allow negative threshold value [select a.meets(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.metBy(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.metBy(b,-1)': The metBy date-time method does not allow negative threshold value [select a.metBy(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
    
            // test overlaps+overlapped-by
            TryInvalid("select a.overlaps(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.overlaps(b,1,2,3)': Parameters mismatch for date-time method 'overlaps', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, but receives 4 expressions [select a.overlaps(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b]");
    
            // test start/startedby
            TryInvalid("select a.starts(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.starts(b,1,2,3)': Parameters mismatch for date-time method 'starts', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start timestamps, but receives 4 expressions [select a.starts(b, 1, 2, 3) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.starts(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.starts(b,-1)': The starts date-time method does not allow negative threshold value [select a.starts(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
            TryInvalid("select a.startedBy(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b",
                       "Error starting statement: Failed to validate select-clause expression 'a.startedBy(b,-1)': The startedBy date-time method does not allow negative threshold value [select a.startedBy(b, -1) from A.std:lastevent() as a, B.std:lastevent() as b]");
        }
    
        [Test]
        public void TestInvalidConfig() {
            var configBean = new ConfigurationEventTypeLegacy();
    
            configBean.StartTimestampPropertyName = null;
            configBean.EndTimestampPropertyName = "Utildate";
            TryInvalidConfig(typeof(SupportDateTime), configBean, "Declared end timestamp property requires that a start timestamp property is also declared");
    
            configBean.StartTimestampPropertyName = "xyz";
            configBean.EndTimestampPropertyName = null;
            TryInvalidConfig(typeof(SupportBean), configBean, "Declared start timestamp property name 'xyz' was not found");
    
            configBean.StartTimestampPropertyName = "LongPrimitive";
            configBean.EndTimestampPropertyName = "xyz";
            TryInvalidConfig(typeof(SupportBean), configBean, "Declared end timestamp property name 'xyz' was not found");
    
            configBean.EndTimestampPropertyName = null;
            configBean.StartTimestampPropertyName = "TheString";
            TryInvalidConfig(typeof(SupportBean), configBean, "Declared start timestamp property 'TheString' is expected to return a DateTime or long-typed value but returns 'System.String'");

            configBean.StartTimestampPropertyName = "LongPrimitive";
            configBean.EndTimestampPropertyName = "TheString";
            TryInvalidConfig(typeof(SupportBean), configBean, "Declared end timestamp property 'TheString' is expected to return a DateTime or long-typed value but returns 'System.String'");
    
            configBean.StartTimestampPropertyName = "Msecdate";
            configBean.EndTimestampPropertyName = "Utildate";
            TryInvalidConfig(typeof(SupportDateTime), configBean, "Declared end timestamp property 'Utildate' is expected to have the same property type as the start-timestamp property 'Msecdate'");
        }
    
        private void TryInvalidConfig(Type clazz, ConfigurationEventTypeLegacy config, string message) {
            try {
                _epService.EPAdministrator.Configuration.AddEventType(clazz.Name, clazz.FullName, config);
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void TryInvalid(string epl, string message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        [Test]
        public void TestBeforeInSelectClause() {
    
            var fields = "c0,c1".Split(',');
            var epl =
                    "select " +
                    "a.MsecdateStart.before(b.MsecdateStart) as c0," +
                    "a.before(b) as c1 " +
                    " from A.std:lastevent() as a, " +
                    "      B.std:lastevent() as b";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(bool?));
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("B1", "2002-05-30T9:00:00.000", 0));
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A1", "2002-05-30T8:59:59.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fields, true);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A2", "2002-05-30T8:59:59.950", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fields, true);
        }
    
        [Test]
        public void TestBeforeWhereClause() {
    
            Validator expectedValidator = new BeforeValidator(1L, long.MaxValue);
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 0, true},
                    new object[] {"2002-05-30T8:59:59.999", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            AssertExpression(seedTime, 0, "a.before(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.before(b, 1 millisecond)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.before(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.MsecdateStart.before(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.UtildateStart.before(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.before(b.MsecdateStart)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.before(b.UtildateStart)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.MsecdateStart.before(b.MsecdateStart)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.MsecdateStart.before(b.MsecdateStart)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.UtildateStart.before(b.UtildateStart)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.UtildateStart.before(b.MsecdateStart)", expected, expectedValidator);
    
            expectedValidator = new BeforeValidator(1L, long.MaxValue);
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, true},
                    new object[] {"2002-05-30T8:59:59.000", 999, true},
                    new object[] {"2002-05-30T8:59:59.000", 1000, false},
                    new object[] {"2002-05-30T8:59:59.000", 1001, false},
                    new object[] {"2002-05-30T8:59:59.999", 0, true},
                    new object[] {"2002-05-30T8:59:59.999", 1, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            AssertExpression(seedTime, 0, "a.before(b)", expected, expectedValidator);
            AssertExpression(seedTime, 100000, "a.before(b)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, true},
                    new object[] {"2002-05-30T8:59:59.899", 0, true},
                    new object[] {"2002-05-30T8:59:59.900", 0, true},
                    new object[] {"2002-05-30T8:59:59.901", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(100L, long.MaxValue);
            AssertExpression(seedTime, 0, "a.before(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 100000, "a.before(b, 100 milliseconds)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.499", 0, false},
                    new object[] {"2002-05-30T8:59:59.499", 1, true},
                    new object[] {"2002-05-30T8:59:59.500", 0, true},
                    new object[] {"2002-05-30T8:59:59.500", 1, true},
                    new object[] {"2002-05-30T8:59:59.500", 400, true},
                    new object[] {"2002-05-30T8:59:59.500", 401, false},
                    new object[] {"2002-05-30T8:59:59.899", 0, true},
                    new object[] {"2002-05-30T8:59:59.899", 2, false},
                    new object[] {"2002-05-30T8:59:59.900", 0, true},
                    new object[] {"2002-05-30T8:59:59.900", 1, false},
                    new object[] {"2002-05-30T8:59:59.901", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(100L, 500L);
            AssertExpression(seedTime, 0, "a.before(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 100000, "a.before(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            // test expression params
            _epService.EPAdministrator.CreateEPL("create variable long V_START = 100");
            _epService.EPAdministrator.CreateEPL("create variable long V_END = 500");
            AssertExpression(seedTime, 0, "a.before(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            _epService.EPRuntime.SetVariableValue("V_START", 200);
            _epService.EPRuntime.SetVariableValue("V_END", 800);
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.199", 0, false},
                    new object[] {"2002-05-30T8:59:59.199", 1, true},
                    new object[] {"2002-05-30T8:59:59.200", 0, true},
                    new object[] {"2002-05-30T8:59:59.800", 0, true},
                    new object[] {"2002-05-30T8:59:59.801", 0, false}
            };
            expectedValidator = new BeforeValidator(200L, 800L);
            AssertExpression(seedTime, 0, "a.before(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test negative and reversed max and min
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.500", 0, false},
                    new object[] {"2002-05-30T9:00:00.99", 0, false},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.500", 0, true},
                    new object[] {"2002-05-30T9:00:00.501", 0, false}
            };
            expectedValidator = new BeforeValidator(-500L, -100L);
            AssertExpression(seedTime, 0, "a.before(b, -100 milliseconds, -500 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.before(b, -500 milliseconds, -100 milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-03-01T9:00:00.000";
            expected = new object[][] {
                    new object[] {"2002-02-01T9:00:00.000", 0, true},
                    new object[] {"2002-02-01T9:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(GetMillisecForDays(28), long.MaxValue);
            AssertExpression(seedTime, 100, "a.before(b, 1 month)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-01-01T8:59:59.999", 0, false},
                    new object[] {"2002-01-01T9:00:00.000", 0, true},
                    new object[] {"2002-01-11T9:00:00.000", 0, true},
                    new object[] {"2002-02-01T9:00:00.000", 0, true},
                    new object[] {"2002-02-01T9:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(GetMillisecForDays(28), GetMillisecForDays(28+31));
            AssertExpression(seedTime, 100, "a.before(b, 1 month, 2 month)", expected, expectedValidator);
        }
    
        [Test]
        public void TestAfterWhereClause()
        {
            Validator expectedValidator = new AfterValidator(1L, long.MaxValue);
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, true}
            };
            AssertExpression(seedTime, 0, "a.after(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.after(b, 1 millisecond)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.after(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.after(b, 1000000000L, 1 millisecond)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.MsecdateStart.after(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.after(b.UtildateStart)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.002", 0, true}
            };
            AssertExpression(seedTime, 1, "a.after(b)", expected, expectedValidator);
            AssertExpression(seedTime, 1, "a.after(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.099", 0, false},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.101", 0, true}
            };
            expectedValidator = new AfterValidator(100L, long.MaxValue);
            AssertExpression(seedTime, 0, "a.after(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.after(b, 100 milliseconds, 1000000000L)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.099", 0, false},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.500", 0, true},
                    new object[] {"2002-05-30T9:00:00.501", 0, false}
            };
            expectedValidator = new AfterValidator(100L, 500L);
            AssertExpression(seedTime, 0, "a.after(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.after(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            // test expression params
            _epService.EPAdministrator.CreateEPL("create variable long V_START = 100");
            _epService.EPAdministrator.CreateEPL("create variable long V_END = 500");
            AssertExpression(seedTime, 0, "a.after(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            _epService.EPRuntime.SetVariableValue("V_START", 200);
            _epService.EPRuntime.SetVariableValue("V_END", 800);
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.199", 0, false},
                    new object[] {"2002-05-30T9:00:00.200", 0, true},
                    new object[] {"2002-05-30T9:00:00.800", 0, true},
                    new object[] {"2002-05-30T9:00:00.801", 0, false}
            };
            expectedValidator = new AfterValidator(200L, 800L);
            AssertExpression(seedTime, 0, "a.after(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test negative distances
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.599", 0, false},
                    new object[] {"2002-05-30T8:59:59.600", 0, true},
                    new object[] {"2002-05-30T8:59:59.1000", 0, true},
                    new object[] {"2002-05-30T8:59:59.1001", 0, false}
            };
            expectedValidator = new AfterValidator(-500L, -100L);
            AssertExpression(seedTime, 100, "a.after(b, -100 milliseconds, -500 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 100, "a.after(b, -500 milliseconds, -100 milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-02-01T9:00:00.000";
            expected = new object[][] {
                    new object[] {"2002-03-01T9:00:00.099", 0, false},
                    new object[] {"2002-03-01T9:00:00.100", 0, true}
            };
            expectedValidator = new AfterValidator(GetMillisecForDays(28), long.MaxValue);
            AssertExpression(seedTime, 100, "a.after(b, 1 month)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-03-01T9:00:00.099", 0, false},
                    new object[] {"2002-03-01T9:00:00.100", 0, true},
                    new object[] {"2002-04-01T9:00:00.100", 0, true},
                    new object[] {"2002-04-01T9:00:00.101", 0, false}
            };
            AssertExpression(seedTime, 100, "a.after(b, 1 month, 2 month)", expected, null);
        }
    
        [Test]
        public void TestCoincidesWhereClause() {
    
            Validator expectedValidator = new CoincidesValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            AssertExpression(seedTime, 0, "a.coincides(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.coincides(b, 0 millisecond)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.coincides(b, 0, 0)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.MsecdateStart.coincides(b)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.coincides(b.UtildateStart)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 1, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 1, false}
            };
            AssertExpression(seedTime, 1, "a.coincides(b)", expected, expectedValidator);
            AssertExpression(seedTime, 1, "a.coincides(b, 0, 0)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.899", 0, false},
                    new object[] {"2002-05-30T8:59:59.900", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 50, true},
                    new object[] {"2002-05-30T9:00:00.000", 100, true},
                    new object[] {"2002-05-30T9:00:00.000", 101, false},
                    new object[] {"2002-05-30T9:00:00.099", 0, true},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.101", 0, false}
            };
            expectedValidator = new CoincidesValidator(100L);
            AssertExpression(seedTime, 0, "a.coincides(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(seedTime, 0, "a.coincides(b, 100 milliseconds, 0.1 sec)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.799", 0, false},
                    new object[] {"2002-05-30T8:59:59.800", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.099", 0, true},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.200", 0, true},
                    new object[] {"2002-05-30T9:00:00.201", 0, false}
            };
            expectedValidator = new CoincidesValidator(200L, 500L);
            AssertExpression(seedTime, 0, "a.coincides(b, 200 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.799", 0, false},
                    new object[] {"2002-05-30T8:59:59.799", 200, false},
                    new object[] {"2002-05-30T8:59:59.799", 201, false},
                    new object[] {"2002-05-30T8:59:59.800", 0, false},
                    new object[] {"2002-05-30T8:59:59.800", 199, false},
                    new object[] {"2002-05-30T8:59:59.800", 200, true},
                    new object[] {"2002-05-30T8:59:59.800", 300, true},
                    new object[] {"2002-05-30T8:59:59.800", 301, false},
                    new object[] {"2002-05-30T9:00:00.050", 0, true},
                    new object[] {"2002-05-30T9:00:00.099", 0, true},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.101", 0, false}
            };
            expectedValidator = new CoincidesValidator(200L, 50L);
            AssertExpression(seedTime, 50, "a.coincides(b, 200 milliseconds, 50 milliseconds)", expected, expectedValidator);
    
            // test expression params
            _epService.EPAdministrator.CreateEPL("create variable long V_START = 200");
            _epService.EPAdministrator.CreateEPL("create variable long V_END = 50");
            AssertExpression(seedTime, 50, "a.coincides(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            _epService.EPRuntime.SetVariableValue("V_START", 200);
            _epService.EPRuntime.SetVariableValue("V_END", 70);
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.800", 0, false},
                    new object[] {"2002-05-30T8:59:59.800", 179, false},
                    new object[] {"2002-05-30T8:59:59.800", 180, true},
                    new object[] {"2002-05-30T8:59:59.800", 200, true},
                    new object[] {"2002-05-30T8:59:59.800", 320, true},
                    new object[] {"2002-05-30T8:59:59.800", 321, false}
            };
            expectedValidator = new CoincidesValidator(200L, 70L);
            AssertExpression(seedTime, 50, "a.coincides(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-02-01T9:00:00.000";    // lasts to "2002-04-01T9:00:00.000" (28+31 days)
            expected = new object[][] {
                    new object[] {"2002-02-15T9:00:00.099", GetMillisecForDays(28+14), true},
                    new object[] {"2002-01-01T8:00:00.00", GetMillisecForDays(28+30), false}
            };
            expectedValidator = new CoincidesValidator(GetMillisecForDays(28));
            AssertExpression(seedTime, GetMillisecForDays(28+31), "a.coincides(b, 1 month)", expected, expectedValidator);
        }
    
        [Test]
        public void TestDuringWhereClause() {
    
            Validator expectedValidator = new DuringValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, true},
                    new object[] {"2002-05-30T9:00:00.001", 98, true},
                    new object[] {"2002-05-30T9:00:00.001", 99, false},
                    new object[] {"2002-05-30T9:00:00.099", 0, true},
                    new object[] {"2002-05-30T9:00:00.099", 1, false},
                    new object[] {"2002-05-30T9:00:00.100", 0, false}
            };
            AssertExpression(seedTime, 100, "a.during(b)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 1, false}
            };
            AssertExpression(seedTime, 0, "a.during(b)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.001", 0, true},
                    new object[] {"2002-05-30T9:00:00.001", 2000000, true}
            };
            AssertExpression(seedTime, 100, "a.MsecdateStart.during(b)", expected, null);    // want to use null-validator here
    
            // test 1-parameter footprint
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 83, false},
                    new object[] {"2002-05-30T9:00:00.001", 84, true},
                    new object[] {"2002-05-30T9:00:00.001", 98, true},
                    new object[] {"2002-05-30T9:00:00.001", 99, false},
                    new object[] {"2002-05-30T9:00:00.015", 69, false},
                    new object[] {"2002-05-30T9:00:00.015", 70, true},
                    new object[] {"2002-05-30T9:00:00.015", 84, true},
                    new object[] {"2002-05-30T9:00:00.015", 85, false},
                    new object[] {"2002-05-30T9:00:00.016", 80, false},
                    new object[] {"2002-05-30T9:00:00.099", 0, false}
            };
            expectedValidator = new DuringValidator(15L);
            AssertExpression(seedTime, 100, "a.during(b, 15 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter footprint
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 78, false},
                    new object[] {"2002-05-30T9:00:00.001", 79, false},
                    new object[] {"2002-05-30T9:00:00.004", 85, false},
                    new object[] {"2002-05-30T9:00:00.005", 74, false},
                    new object[] {"2002-05-30T9:00:00.005", 75, true},
                    new object[] {"2002-05-30T9:00:00.005", 90, true},
                    new object[] {"2002-05-30T9:00:00.005", 91, false},
                    new object[] {"2002-05-30T9:00:00.006", 83, true},
                    new object[] {"2002-05-30T9:00:00.020", 76, false},
                    new object[] {"2002-05-30T9:00:00.020", 75, true},
                    new object[] {"2002-05-30T9:00:00.020", 60, true},
                    new object[] {"2002-05-30T9:00:00.020", 59, false},
                    new object[] {"2002-05-30T9:00:00.021", 68, false},
                    new object[] {"2002-05-30T9:00:00.099", 0, false}
            };
            expectedValidator = new DuringValidator(5L, 20L);
            AssertExpression(seedTime, 100, "a.during(b, 5 milliseconds, 20 milliseconds)", expected, expectedValidator);
    
            // test 4-parameter footprint
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.004", 85, false},
                    new object[] {"2002-05-30T9:00:00.005", 64, false},
                    new object[] {"2002-05-30T9:00:00.005", 65, true},
                    new object[] {"2002-05-30T9:00:00.005", 85, true},
                    new object[] {"2002-05-30T9:00:00.005", 86, false},
                    new object[] {"2002-05-30T9:00:00.020", 49, false},
                    new object[] {"2002-05-30T9:00:00.020", 50, true},
                    new object[] {"2002-05-30T9:00:00.020", 70, true},
                    new object[] {"2002-05-30T9:00:00.020", 71, false},
                    new object[] {"2002-05-30T9:00:00.021", 55, false}
            };
            expectedValidator = new DuringValidator(5L, 20L, 10L, 30L);
            AssertExpression(seedTime, 100, "a.during(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestFinishesWhereClause() {
    
            Validator expectedValidator = new FinishesValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 98, false},
                    new object[] {"2002-05-30T9:00:00.001", 99, true},
                    new object[] {"2002-05-30T9:00:00.001", 100, false},
                    new object[] {"2002-05-30T9:00:00.050", 50, true},
                    new object[] {"2002-05-30T9:00:00.099", 0, false},
                    new object[] {"2002-05-30T9:00:00.099", 1, true},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.101", 0, false}
            };
            AssertExpression(seedTime, 100, "a.finishes(b)", expected, expectedValidator);
            AssertExpression(seedTime, 100, "a.finishes(b, 0)", expected, expectedValidator);
            AssertExpression(seedTime, 100, "a.finishes(b, 0 milliseconds)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 99, false},
                    new object[] {"2002-05-30T9:00:00.001", 93, false},
                    new object[] {"2002-05-30T9:00:00.001", 94, true},
                    new object[] {"2002-05-30T9:00:00.001", 100, true},
                    new object[] {"2002-05-30T9:00:00.001", 104, true},
                    new object[] {"2002-05-30T9:00:00.001", 105, false},
                    new object[] {"2002-05-30T9:00:00.050", 50, true},
                    new object[] {"2002-05-30T9:00:00.104", 0, true},
                    new object[] {"2002-05-30T9:00:00.104", 1, true},
                    new object[] {"2002-05-30T9:00:00.105", 0, true},
                    new object[] {"2002-05-30T9:00:00.105", 1, false}
            };
            expectedValidator = new FinishesValidator(5L);
            AssertExpression(seedTime, 100, "a.finishes(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestFinishedByWhereClause() {
    
            Validator expectedValidator = new FinishedByValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.000", 1099, false},
                    new object[] {"2002-05-30T8:59:59.000", 1100, true},
                    new object[] {"2002-05-30T8:59:59.000", 1101, false},
                    new object[] {"2002-05-30T8:59:59.999", 100, false},
                    new object[] {"2002-05-30T8:59:59.999", 101, true},
                    new object[] {"2002-05-30T8:59:59.999", 102, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 50, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false}
            };
            AssertExpression(seedTime, 100, "a.finishedBy(b)", expected, expectedValidator);
            AssertExpression(seedTime, 100, "a.finishedBy(b, 0)", expected, expectedValidator);
            AssertExpression(seedTime, 100, "a.finishedBy(b, 0 milliseconds)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.000", 1094, false},
                    new object[] {"2002-05-30T8:59:59.000", 1095, true},
                    new object[] {"2002-05-30T8:59:59.000", 1105, true},
                    new object[] {"2002-05-30T8:59:59.000", 1106, false},
                    new object[] {"2002-05-30T8:59:59.999", 95, false},
                    new object[] {"2002-05-30T8:59:59.999", 96, true},
                    new object[] {"2002-05-30T8:59:59.999", 106, true},
                    new object[] {"2002-05-30T8:59:59.999", 107, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 95, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.000", 105, false}
            };
            expectedValidator = new FinishedByValidator(5L);
            AssertExpression(seedTime, 100, "a.finishedBy(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestIncludesByWhereClause() {
    
            Validator expectedValidator = new IncludesValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 1100, false},
                    new object[] {"2002-05-30T8:59:59.000", 1101, true},
                    new object[] {"2002-05-30T8:59:59.000", 3000, true},
                    new object[] {"2002-05-30T8:59:59.999", 101, false},
                    new object[] {"2002-05-30T8:59:59.999", 102, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 50, false},
                    new object[] {"2002-05-30T9:00:00.000", 102, false}
            };
            AssertExpression(seedTime, 100, "a.includes(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.000", 1100, false},
                    new object[] {"2002-05-30T8:59:59.000", 1105, false},
                    new object[] {"2002-05-30T8:59:59.994", 106, false},
                    new object[] {"2002-05-30T8:59:59.994", 110, false},
                    new object[] {"2002-05-30T8:59:59.995", 105, false},
                    new object[] {"2002-05-30T8:59:59.995", 106, true},
                    new object[] {"2002-05-30T8:59:59.995", 110, true},
                    new object[] {"2002-05-30T8:59:59.995", 111, false},
                    new object[] {"2002-05-30T8:59:59.999", 101, false},
                    new object[] {"2002-05-30T8:59:59.999", 102, true},
                    new object[] {"2002-05-30T8:59:59.999", 106, true},
                    new object[] {"2002-05-30T8:59:59.999", 107, false},
                    new object[] {"2002-05-30T9:00:00.000", 105, false},
                    new object[] {"2002-05-30T9:00:00.000", 106, false}
            };
            expectedValidator = new IncludesValidator(5L);
            AssertExpression(seedTime, 100, "a.includes(b, 5 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter form
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.000", 1100, false},
                    new object[] {"2002-05-30T8:59:59.000", 1105, false},
                    new object[] {"2002-05-30T8:59:59.979", 130, false},
                    new object[] {"2002-05-30T8:59:59.980", 124, false},
                    new object[] {"2002-05-30T8:59:59.980", 125, true},
                    new object[] {"2002-05-30T8:59:59.980", 140, true},
                    new object[] {"2002-05-30T8:59:59.980", 141, false},
                    new object[] {"2002-05-30T8:59:59.995", 109, false},
                    new object[] {"2002-05-30T8:59:59.995", 110, true},
                    new object[] {"2002-05-30T8:59:59.995", 125, true},
                    new object[] {"2002-05-30T8:59:59.995", 126, false},
                    new object[] {"2002-05-30T8:59:59.996", 112, false}
            };
            expectedValidator = new IncludesValidator(5L, 20L);
            AssertExpression(seedTime, 100, "a.includes(b, 5 milliseconds, 20 milliseconds)", expected, expectedValidator);
    
            // test 4-parameter form
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.000", 1100, false},
                    new object[] {"2002-05-30T8:59:59.000", 1105, false},
                    new object[] {"2002-05-30T8:59:59.979", 150, false},
                    new object[] {"2002-05-30T8:59:59.980", 129, false},
                    new object[] {"2002-05-30T8:59:59.980", 130, true},
                    new object[] {"2002-05-30T8:59:59.980", 150, true},
                    new object[] {"2002-05-30T8:59:59.980", 151, false},
                    new object[] {"2002-05-30T8:59:59.995", 114, false},
                    new object[] {"2002-05-30T8:59:59.995", 115, true},
                    new object[] {"2002-05-30T8:59:59.995", 135, true},
                    new object[] {"2002-05-30T8:59:59.995", 136, false},
                    new object[] {"2002-05-30T8:59:59.996", 124, false}
            };
            expectedValidator = new IncludesValidator(5L, 20L, 10L, 30L);
            AssertExpression(seedTime, 100, "a.includes(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestMeetsWhereClause() {
    
            Validator expectedValidator = new MeetsValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 1000, true},
                    new object[] {"2002-05-30T8:59:59.000", 1001, false},
                    new object[] {"2002-05-30T8:59:59.998", 1, false},
                    new object[] {"2002-05-30T8:59:59.999", 1, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 1, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            AssertExpression(seedTime, 0, "a.meets(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 0, false},
                    new object[] {"2002-05-30T8:59:59.000", 994, false},
                    new object[] {"2002-05-30T8:59:59.000", 995, true},
                    new object[] {"2002-05-30T8:59:59.000", 1005, true},
                    new object[] {"2002-05-30T8:59:59.000", 1006, false},
                    new object[] {"2002-05-30T8:59:59.994", 0, false},
                    new object[] {"2002-05-30T8:59:59.994", 1, true},
                    new object[] {"2002-05-30T8:59:59.995", 0, true},
                    new object[] {"2002-05-30T8:59:59.999", 0, true},
                    new object[] {"2002-05-30T8:59:59.999", 1, true},
                    new object[] {"2002-05-30T8:59:59.999", 6, true},
                    new object[] {"2002-05-30T8:59:59.999", 7, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 1, true},
                    new object[] {"2002-05-30T9:00:00.000", 5, true},
                    new object[] {"2002-05-30T9:00:00.005", 0, true},
                    new object[] {"2002-05-30T9:00:00.005", 1, false}
            };
            expectedValidator = new MeetsValidator(5L);
            AssertExpression(seedTime, 0, "a.meets(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestMetByWhereClause() {
    
            Validator expectedValidator = new MetByValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T9:00:00.99", 0, false},
                    new object[] {"2002-05-30T9:00:00.100", 0, true},
                    new object[] {"2002-05-30T9:00:00.100", 500, true},
                    new object[] {"2002-05-30T9:00:00.101", 0, false}
            };
            AssertExpression(seedTime, 100, "a.metBy(b)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.999", 1, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 1, true}
            };
            AssertExpression(seedTime, 0, "a.metBy(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.994", 0, false},
                    new object[] {"2002-05-30T8:59:59.994", 5, false},
                    new object[] {"2002-05-30T8:59:59.995", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 20, true},
                    new object[] {"2002-05-30T9:00:00.005", 0, true},
                    new object[] {"2002-05-30T9:00:00.005", 1000, true},
                    new object[] {"2002-05-30T9:00:00.006", 0, false}
            };
            expectedValidator = new MetByValidator(5L);
            AssertExpression(seedTime, 0, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);
    
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.994", 0, false},
                    new object[] {"2002-05-30T8:59:59.994", 5, false},
                    new object[] {"2002-05-30T8:59:59.995", 0, false},
                    new object[] {"2002-05-30T9:00:00.094", 0, false},
                    new object[] {"2002-05-30T9:00:00.095", 0, true},
                    new object[] {"2002-05-30T9:00:00.105", 0, true},
                    new object[] {"2002-05-30T9:00:00.105", 5000, true},
                    new object[] {"2002-05-30T9:00:00.106", 0, false}
            };
            expectedValidator = new MetByValidator(5L);
            AssertExpression(seedTime, 100, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestOverlapsWhereClause() {
    
            Validator expectedValidator = new OverlapsValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 1000, false},
                    new object[] {"2002-05-30T8:59:59.000", 1001, true},
                    new object[] {"2002-05-30T8:59:59.000", 1050, true},
                    new object[] {"2002-05-30T8:59:59.000", 1099, true},
                    new object[] {"2002-05-30T8:59:59.000", 1100, false},
                    new object[] {"2002-05-30T8:59:59.999", 1, false},
                    new object[] {"2002-05-30T8:59:59.999", 2, true},
                    new object[] {"2002-05-30T8:59:59.999", 100, true},
                    new object[] {"2002-05-30T8:59:59.999", 101, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false}
            };
            AssertExpression(seedTime, 100, "a.overlaps(b)", expected, expectedValidator);
    
            // test 1-parameter form (overlap by not more then X msec)
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 1000, false},
                    new object[] {"2002-05-30T8:59:59.000", 1001, true},
                    new object[] {"2002-05-30T8:59:59.000", 1005, true},
                    new object[] {"2002-05-30T8:59:59.000", 1006, false},
                    new object[] {"2002-05-30T8:59:59.000", 1100, false},
                    new object[] {"2002-05-30T8:59:59.999", 1, false},
                    new object[] {"2002-05-30T8:59:59.999", 2, true},
                    new object[] {"2002-05-30T8:59:59.999", 6, true},
                    new object[] {"2002-05-30T8:59:59.999", 7, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 5, false}
            };
            expectedValidator = new OverlapsValidator(5L);
            AssertExpression(seedTime, 100, "a.overlaps(b, 5 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter form (overlap by min X and not more then Y msec)
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 1004, false},
                    new object[] {"2002-05-30T8:59:59.000", 1005, true},
                    new object[] {"2002-05-30T8:59:59.000", 1010, true},
                    new object[] {"2002-05-30T8:59:59.000", 1011, false},
                    new object[] {"2002-05-30T8:59:59.999", 5, false},
                    new object[] {"2002-05-30T8:59:59.999", 6, true},
                    new object[] {"2002-05-30T8:59:59.999", 11, true},
                    new object[] {"2002-05-30T8:59:59.999", 12, false},
                    new object[] {"2002-05-30T8:59:59.999", 12, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 5, false}
            };
            expectedValidator = new OverlapsValidator(5L, 10L);
            AssertExpression(seedTime, 100, "a.overlaps(b, 5 milliseconds, 10 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestOverlappedByWhereClause() {
    
            Validator expectedValidator = new OverlappedByValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.000", 1000, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 1, false},
                    new object[] {"2002-05-30T9:00:00.001", 99, false},
                    new object[] {"2002-05-30T9:00:00.001", 100, true},
                    new object[] {"2002-05-30T9:00:00.099", 1, false},
                    new object[] {"2002-05-30T9:00:00.099", 2, true},
                    new object[] {"2002-05-30T9:00:00.100", 0, false},
                    new object[] {"2002-05-30T9:00:00.100", 1, false}
            };
            AssertExpression(seedTime, 100, "a.overlappedBy(b)", expected, expectedValidator);
    
            // test 1-parameter form (overlap by not more then X msec)
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 1000, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 1, false},
                    new object[] {"2002-05-30T9:00:00.001", 99, false},
                    new object[] {"2002-05-30T9:00:00.094", 7, false},
                    new object[] {"2002-05-30T9:00:00.094", 100, false},
                    new object[] {"2002-05-30T9:00:00.095", 5, false},
                    new object[] {"2002-05-30T9:00:00.095", 6, true},
                    new object[] {"2002-05-30T9:00:00.095", 100, true},
                    new object[] {"2002-05-30T9:00:00.099", 1, false},
                    new object[] {"2002-05-30T9:00:00.099", 2, true},
                    new object[] {"2002-05-30T9:00:00.099", 100, true},
                    new object[] {"2002-05-30T9:00:00.100", 100, false}
            };
            expectedValidator = new OverlappedByValidator(5L);
            AssertExpression(seedTime, 100, "a.overlappedBy(b, 5 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter form (overlap by min X and not more then Y msec)
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.000", 1000, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 1, false},
                    new object[] {"2002-05-30T9:00:00.001", 99, false},
                    new object[] {"2002-05-30T9:00:00.089", 14, false},
                    new object[] {"2002-05-30T9:00:00.090", 10, false},
                    new object[] {"2002-05-30T9:00:00.090", 11, true},
                    new object[] {"2002-05-30T9:00:00.090", 1000, true},
                    new object[] {"2002-05-30T9:00:00.095", 5, false},
                    new object[] {"2002-05-30T9:00:00.095", 6, true},
                    new object[] {"2002-05-30T9:00:00.096", 5, false},
                    new object[] {"2002-05-30T9:00:00.096", 100, false},
                    new object[] {"2002-05-30T9:00:00.100", 100, false}
            };
            expectedValidator = new OverlappedByValidator(5L, 10L);
            AssertExpression(seedTime, 100, "a.overlappedBy(b, 5 milliseconds, 10 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestStartsWhereClause() {
    
            Validator expectedValidator = new StartsValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.999", 100, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 1, true},
                    new object[] {"2002-05-30T9:00:00.000", 99, true},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, false}
            };
            AssertExpression(seedTime, 100, "a.starts(b)", expected, expectedValidator);
    
            // test 1-parameter form (max distance between start times)
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.994", 6, false},
                    new object[] {"2002-05-30T8:59:59.995", 0, true},
                    new object[] {"2002-05-30T8:59:59.995", 104, true},
                    new object[] {"2002-05-30T8:59:59.995", 105, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, true},
                    new object[] {"2002-05-30T9:00:00.000", 1, true},
                    new object[] {"2002-05-30T9:00:00.000", 99, true},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.001", 0, true},
                    new object[] {"2002-05-30T9:00:00.005", 94, true},
                    new object[] {"2002-05-30T9:00:00.005", 95, false},
                    new object[] {"2002-05-30T9:00:00.005", 100, false}
            };
            expectedValidator = new StartsValidator(5L);
            AssertExpression(seedTime, 100, "a.starts(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        [Test]
        public void TestStartedByWhereClause() {
    
            Validator expectedValidator = new StartedByValidator();
            var seedTime = "2002-05-30T9:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T8:59:59.999", 100, false},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.000", 101, true},
                    new object[] {"2002-05-30T9:00:00.001", 0, false},
                    new object[] {"2002-05-30T9:00:00.001", 101, false}
            };
            AssertExpression(seedTime, 100, "a.startedBy(b)", expected, expectedValidator);
    
            // test 1-parameter form (max distance between start times)
            expected = new object[][] {
                    new object[] {"2002-05-30T8:59:59.994", 6, false},
                    new object[] {"2002-05-30T8:59:59.995", 0, false},
                    new object[] {"2002-05-30T8:59:59.995", 105, false},
                    new object[] {"2002-05-30T8:59:59.995", 106, true},
                    new object[] {"2002-05-30T9:00:00.000", 0, false},
                    new object[] {"2002-05-30T9:00:00.000", 100, false},
                    new object[] {"2002-05-30T9:00:00.000", 101, true},
                    new object[] {"2002-05-30T9:00:00.001", 99, false},
                    new object[] {"2002-05-30T9:00:00.001", 100, true},
                    new object[] {"2002-05-30T9:00:00.005", 94, false},
                    new object[] {"2002-05-30T9:00:00.005", 95, false},
                    new object[] {"2002-05-30T9:00:00.005", 96, true}
            };
            expectedValidator = new StartedByValidator(5L);
            AssertExpression(seedTime, 100, "a.startedBy(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void AssertExpression(string seedTime, long seedDuration, string whereClause, object[][] timestampsAndResult, Validator validator) {
    
            var epl = "select * from A.std:lastevent() as a, B.std:lastevent() as b " +
                    "where " + whereClause;
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("B", seedTime, seedDuration));
    
            foreach (var test in timestampsAndResult) {
                var testtime = (string) test[0];
                var testduration = test[1].AsLong();
                var expected = (Boolean) test[2];
    
                var rightStart = DateTimeParser.ParseDefaultMSec(seedTime);
                var rightEnd = rightStart + seedDuration;
                var leftStart = DateTimeParser.ParseDefaultMSec(testtime);
                long leftEnd = leftStart + testduration;
                var message = "time " + testtime + " duration " + testduration + " for '" + whereClause + "'";
    
                if (validator != null)
                {
                    Assert.AreEqual(
                        expected, validator.Validate(leftStart, leftEnd, rightStart, rightEnd),
                        "Validation of expected result failed for " + message);
                }
    
                _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A", testtime, testduration));
    
                if (!_listener.IsInvoked && expected) {
                    Assert.Fail("Expected but not received for " + message);
                }
                if (_listener.IsInvoked && !expected) {
                    Assert.Fail("Not expected but received for " + message);
                }
                _listener.Reset();
            }
    
            stmt.Dispose();
        }
    
        private static long GetMillisecForDays(int days) {
            return days*24*60*60*1000L;
        }

        public interface Validator
        {
            bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd);
        }
    
        public class BeforeValidator : Validator
        {
            private readonly long? _start;
            private readonly long? _end;

            public BeforeValidator(long? start, long? end)
            {
                _start = start;
                _end = end;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                var delta = rightStart - leftEnd;
                return _start <= delta && delta <= _end;
            }
        }

        public class AfterValidator : Validator
        {
            private readonly long? _start;
            private readonly long? _end;

            public AfterValidator(long? start, long? end)
            {
                _start = start;
                _end = end;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                var delta = leftStart - rightEnd;
                return _start <= delta && delta <= _end;
            }
        }

        public class CoincidesValidator : Validator
        {
            private readonly long? _startThreshold;
            private readonly long? _endThreshold;

            public CoincidesValidator()
            {
                _startThreshold = 0L;
                _endThreshold = 0L;
            }

            public CoincidesValidator(long? startThreshold)
            {
                _startThreshold = startThreshold;
                _endThreshold = startThreshold;
            }

            public CoincidesValidator(long? startThreshold, long? endThreshold)
            {
                _startThreshold = startThreshold;
                _endThreshold = endThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                var startDelta = Math.Abs(leftStart - rightStart);
                var endDelta = Math.Abs(leftEnd - rightEnd);
                return ((startDelta <= _startThreshold) &&
                       (endDelta <= _endThreshold));
            }
        }

        public class DuringValidator : Validator
        {
            private readonly int _form;
            private readonly long? _threshold;
            private readonly long? _minThreshold;
            private readonly long? _maxThreshold;
            private readonly long? _minStartThreshold;
            private readonly long? _maxStartThreshold;
            private readonly long? _minEndThreshold;
            private readonly long? _maxEndThreshold;

            public DuringValidator()
            {
                _form = 1;
            }

            public DuringValidator(long? threshold)
            {
                _form = 2;
                _threshold = threshold;
            }

            public DuringValidator(long? minThreshold, long? maxThreshold)
            {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }

            public DuringValidator(long? minStartThreshold, long? maxStartThreshold, long? minEndThreshold, long? maxEndThreshold)
            {
                _form = 4;
                _minStartThreshold = minStartThreshold;
                _maxStartThreshold = maxStartThreshold;
                _minEndThreshold = minEndThreshold;
                _maxEndThreshold = maxEndThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_form == 1) {
                    return rightStart < leftStart &&
                           leftEnd < rightEnd;
                }
                else if (_form == 2) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart <= 0 || distanceStart > _threshold) {
                        return false;
                    }
                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd <= 0 || distanceEnd > _threshold);
                }
                else if (_form == 3) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart < _minThreshold || distanceStart > _maxThreshold) {
                        return false;
                    }
                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < _minThreshold || distanceEnd > _maxThreshold);
                }
                else if (_form == 4) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart < _minStartThreshold || distanceStart > _maxStartThreshold) {
                        return false;
                    }
                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < _minEndThreshold || distanceEnd > _maxEndThreshold);
                }
                throw new IllegalStateException("Invalid form: " + _form);
            }
        }

        public class FinishesValidator : Validator
        {
            private readonly long? _threshold;

            public FinishesValidator()
            {
            }

            public FinishesValidator(long? threshold)
            {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_threshold == null) {
                    return ((rightStart < leftStart) && (leftEnd == rightEnd));
                }
                else {
                    if (rightStart >= leftStart) {
                        return false;
                    }
                    var delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class FinishedByValidator : Validator
        {
            private readonly long? _threshold;

            public FinishedByValidator()
            {
            }

            public FinishedByValidator(long? threshold)
            {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_threshold == null) {
                    return ((leftStart < rightStart) && (leftEnd == rightEnd));
                }
                else {
                    if (leftStart >= rightStart) {
                        return false;
                    }
                    var delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class IncludesValidator : Validator
        {
    
            private readonly int _form;
            private readonly long? _threshold;
            private readonly long? _minThreshold;
            private readonly long? _maxThreshold;
            private readonly long? _minStartThreshold;
            private readonly long? _maxStartThreshold;
            private readonly long? _minEndThreshold;
            private readonly long? _maxEndThreshold;

            public IncludesValidator()
            {
                _form = 1;
            }

            public IncludesValidator(long? threshold)
            {
                _form = 2;
                _threshold = threshold;
            }

            public IncludesValidator(long? minThreshold, long? maxThreshold)
            {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }

            public IncludesValidator(long? minStartThreshold, long? maxStartThreshold, long? minEndThreshold, long? maxEndThreshold)
            {
                _form = 4;
                _minStartThreshold = minStartThreshold;
                _maxStartThreshold = maxStartThreshold;
                _minEndThreshold = minEndThreshold;
                _maxEndThreshold = maxEndThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                                    
                if (_form == 1) {
                    return leftStart < rightStart &&
                           rightEnd < leftEnd;
                }
                else if (_form == 2) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart <= 0 || distanceStart > _threshold) {
                        return false;
                    }
                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd <= 0 || distanceEnd > _threshold);
                }
                else if (_form == 3) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart < _minThreshold || distanceStart > _maxThreshold) {
                        return false;
                    }
                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < _minThreshold || distanceEnd > _maxThreshold);
                }
                else if (_form == 4) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart < _minStartThreshold || distanceStart > _maxStartThreshold) {
                        return false;
                    }
                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < _minEndThreshold || distanceEnd > _maxEndThreshold);
                }
                throw new IllegalStateException("Invalid form: " + _form);
            }
        }

        public class MeetsValidator : Validator
        {
            private readonly long? _threshold;

            public MeetsValidator()
            {
            }

            public MeetsValidator(long? threshold)
            {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_threshold == null) {
                    return rightStart == leftEnd;
                }
                else {
                    var delta = Math.Abs(rightStart - leftEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class MetByValidator : Validator
        {
            private readonly long? _threshold;

            public MetByValidator()
            {
            }

            public MetByValidator(long? threshold)
            {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_threshold == null) {
                    return leftStart == rightEnd;
                }
                else {
                    var delta = Math.Abs(leftStart - rightEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class OverlapsValidator : Validator
        {
            private readonly int _form;
            private readonly long? _threshold;
            private readonly long? _minThreshold;
            private readonly long? _maxThreshold;

            public OverlapsValidator()
            {
                _form = 1;
            }

            public OverlapsValidator(long? threshold)
            {
                _form = 2;
                _threshold = threshold;
            }

            public OverlapsValidator(long? minThreshold, long? maxThreshold)
            {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                var match = (leftStart < rightStart) &&
                           (rightStart < leftEnd) &&
                           (leftEnd < rightEnd);
    
                if (_form == 1) {
                    return match;
                }
                else if (_form == 2) {
                    if (!match) {
                        return false;
                    }
                    var delta = leftEnd - rightStart;
                    return 0 <= delta && delta <= _threshold;
                }
                else if (_form == 3) {
                    if (!match) {
                        return false;
                    }
                    var delta = leftEnd - rightStart;
                    return _minThreshold <= delta && delta <= _maxThreshold;
                }
                throw new ArgumentException("Invalid form " + _form);
            }
        }

        public class OverlappedByValidator : Validator
        {
            private readonly int _form;
            private readonly long? _threshold;
            private readonly long? _minThreshold;
            private readonly long? _maxThreshold;

            public OverlappedByValidator()
            {
                _form = 1;
            }

            public OverlappedByValidator(long? threshold)
            {
                _form = 2;
                _threshold = threshold;
            }

            public OverlappedByValidator(long? minThreshold, long? maxThreshold)
            {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                var match = (rightStart < leftStart) &&
                                (leftStart < rightEnd) &&
                                (rightEnd < leftEnd);
    
                if (_form == 1) {
                    return match;
                }
                else if (_form == 2) {
                    if (!match) {
                        return false;
                    }
                    var delta = rightEnd - leftStart;
                    return 0 <= delta && delta <= _threshold;
                }
                else if (_form == 3) {
                    if (!match) {
                        return false;
                    }
                    var delta = rightEnd - leftStart;
                    return _minThreshold <= delta && delta <= _maxThreshold;
                }
                throw new ArgumentException("Invalid form " + _form);
            }
        }

        public class StartsValidator : Validator
        {
            private readonly long? _threshold;

            public StartsValidator()
            {
            }

            public StartsValidator(long? threshold)
            {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_threshold == null) {
                    return (leftStart == rightStart) && (leftEnd < rightEnd);
                }
                else {
                    var delta = Math.Abs(leftStart - rightStart);
                    return (delta <= _threshold) && (leftEnd < rightEnd);
                }
            }
        }

        public class StartedByValidator : Validator
        {
            private readonly long? _threshold;

            public StartedByValidator()
            {
            }

            public StartedByValidator(long? threshold)
            {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_threshold == null) {
                    return (leftStart == rightStart) && (leftEnd > rightEnd);
                }
                else {
                    var delta = Math.Abs(leftStart - rightStart);
                    return (delta <= _threshold) && (leftEnd > rightEnd);
                }
            }
        }
    }
}
