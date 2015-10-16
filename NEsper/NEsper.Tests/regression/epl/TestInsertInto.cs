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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string,object>;

    [TestFixture]
    public class TestInsertInto
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _feedListener;
        private SupportUpdateListener _resultListenerDelta;
        private SupportUpdateListener _resultListenerProduct;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _feedListener = new SupportUpdateListener();
            _resultListenerDelta = new SupportUpdateListener();
            _resultListenerProduct = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _resultListenerDelta = null;
            _feedListener = null;
            _resultListenerProduct = null;
        }

        [Test]
        public void TestAssertionWildcardRecast()
        {
            // bean to OA/Map/bean
            RunAssertionWildcardRecast(true, null, false, EventRepresentationEnum.OBJECTARRAY);
            RunAssertionWildcardRecast(true, null, false, EventRepresentationEnum.MAP);
            try {
                RunAssertionWildcardRecast(true, null, true, null);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Expression-returned event type 'SourceSchema' with underlying type 'com.espertech.esper.regression.epl.TestInsertInto+MyP0P1EventSource' cannot be converted to target event type 'TargetSchema' with underlying type ");
            }

            // OA to OA/Map/bean
            RunAssertionWildcardRecast(false, EventRepresentationEnum.OBJECTARRAY, false, EventRepresentationEnum.OBJECTARRAY);
            RunAssertionWildcardRecast(false, EventRepresentationEnum.OBJECTARRAY, false, EventRepresentationEnum.MAP);
            RunAssertionWildcardRecast(false, EventRepresentationEnum.OBJECTARRAY, true, null);

            // Map to OA/Map/bean
            RunAssertionWildcardRecast(false, EventRepresentationEnum.MAP, false, EventRepresentationEnum.OBJECTARRAY);
            RunAssertionWildcardRecast(false, EventRepresentationEnum.MAP, false, EventRepresentationEnum.MAP);
            RunAssertionWildcardRecast(false, EventRepresentationEnum.MAP, true, null);
        }

        private void RunAssertionWildcardRecast(
            bool sourceBean, EventRepresentationEnum? sourceType,
            bool targetBean, EventRepresentationEnum? targetType)
        {
            try {
                RunAssertionWildcardRecastInternal(sourceBean, sourceType, targetBean, targetType);
            }
            finally {
                // cleanup
                _epService.EPAdministrator.DestroyAllStatements();
                _epService.EPAdministrator.Configuration.RemoveEventType("TargetSchema", false);
                _epService.EPAdministrator.Configuration.RemoveEventType("SourceSchema", false);
                _epService.EPAdministrator.Configuration.RemoveEventType("TargetContainedSchema", false);
            }
        }

        private void RunAssertionWildcardRecastInternal(
            bool sourceBean, EventRepresentationEnum? sourceType,
            bool targetBean, EventRepresentationEnum? targetType)
        {
            // declare source type
            if (sourceBean)
            {
                _epService.EPAdministrator.CreateEPL(
                    "create schema SourceSchema as com.espertech.esper.regression.epl.TestInsertInto$MyP0P1EventSource");
            }
            else if (sourceType == null) {
                Assert.Fail();
            }
            else {
                _epService.EPAdministrator.CreateEPL("create " + sourceType.Value.GetOutputTypeCreateSchemaName() + " schema SourceSchema as (P0 string, P1 int)");
            }

            // declare target type
            if (targetBean) {
                _epService.EPAdministrator.CreateEPL("create schema TargetSchema as " + typeof(MyP0P1EventTarget).MaskTypeName());
            }
            else if (targetType == null) {
                Assert.Fail();
            }
            else {
                _epService.EPAdministrator.CreateEPL("create " + targetType.Value.GetOutputTypeCreateSchemaName() + " schema TargetContainedSchema as (C0 int)");
                _epService.EPAdministrator.CreateEPL("create " + targetType.Value.GetOutputTypeCreateSchemaName() + " schema TargetSchema (P0 string, P1 int, C0 TargetContainedSchema)");
            }

            // insert-into and select
            _epService.EPAdministrator.CreateEPL("insert into TargetSchema select * from SourceSchema");
            var stmt = _epService.EPAdministrator.CreateEPL("select * from TargetSchema");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            // send event
            if (sourceBean) {
                _epService.EPRuntime.SendEvent(new MyP0P1EventSource("a", 10));
            }
            else if (sourceType == EventRepresentationEnum.MAP) {
                var map = new Dictionary<String, Object>();
                map.Put("P0", "a");
                map.Put("P1", 10);
                _epService.EPRuntime.SendEvent(map, "SourceSchema");
            }
            else {
                _epService.EPRuntime.SendEvent(new Object[] {"a", 10}, "SourceSchema");
            }

            // assert
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "P0,P1,C0".Split(','), new Object[]{"a", 10, null});
        }
    
        [Test]
        public void TestVariantRStreamOMToStmt() {
            var model = new EPStatementObjectModel();
            model.InsertInto = InsertIntoClause.Create("Event_1", new String[0], StreamSelector.RSTREAM_ONLY);
            model.SelectClause = SelectClause.Create().Add("IntPrimitive", "IntBoxed");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            var stmt = _epService.EPAdministrator.Create(model, "s1");
    
            var epl = "insert rstream into Event_1 " +
                    "select IntPrimitive, IntBoxed " +
                    "from " + typeof(SupportBean).FullName;
            Assert.AreEqual(epl, model.ToEPL());
            Assert.AreEqual(epl, stmt.Text);
    
            var modelTwo = _epService.EPAdministrator.CompileEPL(model.ToEPL());
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(epl, modelTwo.ToEPL());
    
            // assert statement-type reference
            var spi = (EPServiceProviderSPI) _epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("Event_1"));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsTrue(stmtNames.Contains("s1"));
    
            stmt.Dispose();
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("Event_1"));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsFalse(stmtNames.Contains("s1"));
        }
    
        [Test]
        public void TestVariantOneOMToStmt() {
            var model = new EPStatementObjectModel();
            model.InsertInto = InsertIntoClause.Create("Event_1", "delta", "product");
            model.SelectClause = SelectClause.Create().Add(Expressions.Minus("IntPrimitive", "IntBoxed"), "deltaTag")
                .Add(Expressions.Multiply("IntPrimitive", "IntBoxed"), "productTag");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView(View.Create("win", "length", Expressions.Constant(100))));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            var stmt = RunAsserts(null, model);
    
            var epl = "insert into Event_1(delta, product) " +
                    "select IntPrimitive-IntBoxed as deltaTag, IntPrimitive*IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100)";
            Assert.AreEqual(epl, model.ToEPL());
            Assert.AreEqual(epl, stmt.Text);
        }
    
        [Test]
        public void TestVariantOneEPLToOMStmt() {
            var epl = "insert into Event_1(delta, product) " +
                    "select IntPrimitive-IntBoxed as deltaTag, IntPrimitive*IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100)";
    
            var model = _epService.EPAdministrator.CompileEPL(epl);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(epl, model.ToEPL());
    
            var stmt = RunAsserts(null, model);
            Assert.AreEqual(epl, stmt.Text);
        }
    
        [Test]
        public void TestVariantOne() {
            var stmtText = "insert into Event_1 (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100)";
    
            RunAsserts(stmtText, null);
        }
    
        [Test]
        public void TestVariantOneStateless()
        {
            String stmtTextStateless = "insert into Event_1 (delta, product) " +
                    "select intPrimitive - intBoxed as deltaTag, intPrimitive * intBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName;
            RunAsserts(stmtTextStateless, null);
        }

        [Test]
        public void TestVariantOneWildcard() {
            var stmtText = "insert into Event_1 (delta, product) " +
                    "select * from " + typeof(SupportBean).FullName + ".win:length(100)";
    
            try {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                // Expected
            }
    
            // assert statement-type reference
            var spi = (EPServiceProviderSPI) _epService;
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("Event_1"));
    
            // test insert wildcard to wildcard
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            var listener = new SupportUpdateListener();
    
            var stmtSelectText = "insert into ABCStream select * from SupportBean";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtSelectText, "resilient i0");
            stmtSelect.Events += listener.Update;
            Assert.IsTrue(stmtSelect.EventType is BeanEventType);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNew().Get("TheString"));
            Assert.IsTrue(listener.AssertOneGetNew() is BeanEventBean);
        }
    
        [Test]
        public void TestVariantOneJoin() {
            var stmtText = "insert into Event_1 (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100) as s0," +
                    typeof(SupportBean_A).FullName + ".win:length(100) as s1 " +
                    " where s0.TheString = s1.id";
    
            RunAsserts(stmtText, null);
        }
    
        [Test]
        public void TestVariantOneJoinWildcard() {
            var stmtText = "insert into Event_1 (delta, product) " +
                    "select * " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100) as s0," +
                    typeof(SupportBean_A).FullName + ".win:length(100) as s1 " +
                    " where s0.TheString = s1.id";
    
            try {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                // Expected
            }
        }
    
        [Test]
        public void TestVariantTwo() {
            var stmtText = "insert into Event_1 " +
                    "select IntPrimitive - IntBoxed as delta, IntPrimitive * IntBoxed as product " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100)";
    
            RunAsserts(stmtText, null);
        }
    
        [Test]
        public void TestVariantTwoWildcard() {
            var stmtText = "insert into event1 select * from " + typeof(SupportBean).FullName + ".win:length(100)";
            var otherText = "select * from default.event1.win:length(10)";
    
            // Attach listener to feed
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtText, "stmt1");
            Assert.AreEqual(StatementType.INSERT_INTO, ((EPStatementSPI) stmtOne).StatementMetadata.StatementType);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL(otherText, "stmt2");
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            var theEvent = SendEvent(10, 11);
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
            Assert.AreEqual(1, listenerOne.LastNewData.Length);
            Assert.AreEqual(10, listenerOne.LastNewData[0].Get("IntPrimitive"));
            Assert.AreEqual(11, listenerOne.LastNewData[0].Get("IntBoxed"));
            Assert.AreEqual(20, listenerOne.LastNewData[0].EventType.PropertyNames.Length);
            Assert.AreSame(theEvent, listenerOne.LastNewData[0].Underlying);
    
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
            Assert.AreEqual(1, listenerTwo.LastNewData.Length);
            Assert.AreEqual(10, listenerTwo.LastNewData[0].Get("IntPrimitive"));
            Assert.AreEqual(11, listenerTwo.LastNewData[0].Get("IntBoxed"));
            Assert.AreEqual(20, listenerTwo.LastNewData[0].EventType.PropertyNames.Length);
            Assert.AreSame(theEvent, listenerTwo.LastNewData[0].Underlying);
    
            // assert statement-type reference
            var spi = (EPServiceProviderSPI) _epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("event1"));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("event1");
            EPAssertionUtil.AssertEqualsAnyOrder(stmtNames.ToArray(), new String[]{"stmt1", "stmt2"});
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtNames.ToArray(), new String[]{"stmt1"});
    
            stmtOne.Dispose();
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("event1"));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("event1");
            EPAssertionUtil.AssertEqualsAnyOrder(new String[]{"stmt2"}, stmtNames.ToArray());
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
    
            stmtTwo.Dispose();
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("event1"));
        }
    
        [Test]
        public void TestVariantTwoJoin() {
            var stmtText = "insert into Event_1 " +
                    "select IntPrimitive - IntBoxed as delta, IntPrimitive * IntBoxed as product " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100) as s0," +
                    typeof(SupportBean_A).FullName + ".win:length(100) as s1 " +
                    " where s0.TheString = s1.id";
    
            RunAsserts(stmtText, null);
    
            // assert type metadata
            var type = (EventTypeSPI) ((EPServiceProviderSPI) _epService).EventAdapterService.GetEventTypeByName("Event_1");
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("Event_1", type.Metadata.PrimaryName);
            Assert.AreEqual("Event_1", type.Metadata.PublicName);
            Assert.AreEqual("Event_1", type.Name);
            Assert.AreEqual(TypeClass.STREAM, type.Metadata.TypeClass);
            Assert.AreEqual(false, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
        }
    
        [Test]
        public void TestVariantTwoJoinWildcard() {
            var textOne = "insert into event2 select * " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100) as s0, " +
                    typeof(SupportBean_A).FullName + ".win:length(5) as s1 " +
                    "where s0.TheString = s1.id";
            var textTwo = "select * from event2.win:length(10)";
    
            // Attach listener to feed
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL(textTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            // send event for joins to match on
            var eventA = new SupportBean_A("myId");
            _epService.EPRuntime.SendEvent(eventA);
    
            var eventOne = SendEvent(10, 11);
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
            Assert.AreEqual(1, listenerOne.LastNewData.Length);
            Assert.AreEqual(2, listenerOne.LastNewData[0].EventType.PropertyNames.Length);
            Assert.IsTrue(listenerOne.LastNewData[0].EventType.IsProperty("s0"));
            Assert.IsTrue(listenerOne.LastNewData[0].EventType.IsProperty("s1"));
            Assert.AreSame(eventOne, listenerOne.LastNewData[0].Get("s0"));
            Assert.AreSame(eventA, listenerOne.LastNewData[0].Get("s1"));
    
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
            Assert.AreEqual(1, listenerTwo.LastNewData.Length);
            Assert.AreEqual(2, listenerTwo.LastNewData[0].EventType.PropertyNames.Length);
            Assert.IsTrue(listenerTwo.LastNewData[0].EventType.IsProperty("s0"));
            Assert.IsTrue(listenerTwo.LastNewData[0].EventType.IsProperty("s1"));
            Assert.AreSame(eventOne, listenerOne.LastNewData[0].Get("s0"));
            Assert.AreSame(eventA, listenerOne.LastNewData[0].Get("s1"));
        }
    
        [Test]
        public void TestInvalidStreamUsed() {
            var stmtText = "insert into Event_1 (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100)";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            try {
                stmtText = "insert into Event_1(delta) " +
                        "select (IntPrimitive - IntBoxed) as deltaTag " +
                        "from " + typeof(SupportBean).FullName + ".win:length(100)";
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                // expected
                Assert.AreEqual("Error starting statement: Event type named 'Event_1' has already been declared with differing column name or type information: Type by name 'Event_1' expects 2 properties but receives 1 properties [insert into Event_1(delta) select (IntPrimitive - IntBoxed) as deltaTag from com.espertech.esper.support.bean.SupportBean.win:length(100)]", ex.Message);
            }
        }
    
        [Test]
        public void TestWithOutputLimitAndSort() {
            // NOTICE: we are inserting the RSTREAM (removed events)
            var stmtText = "insert rstream into StockTicks(mySymbol, myPrice) " +
                    "select Symbol, Price from " + typeof(SupportMarketDataBean).FullName + ".win:time(60) " +
                    "output every 5 seconds " +
                    "order by Symbol asc";
            _epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "select mySymbol, sum(myPrice) as Pricesum from StockTicks.win:length(100)";
            var statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _feedListener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SendEvent("IBM", 50);
            SendEvent("CSC", 10);
            SendEvent("GE", 20);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10 * 1000));
            SendEvent("DEF", 100);
            SendEvent("ABC", 11);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(20 * 1000));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(30 * 1000));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(40 * 1000));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(50 * 1000));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(55 * 1000));
    
            Assert.IsFalse(_feedListener.IsInvoked);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(60 * 1000));
    
            Assert.IsTrue(_feedListener.IsInvoked);
            Assert.AreEqual(3, _feedListener.NewDataList.Count);
            Assert.AreEqual("CSC", _feedListener.NewDataList[0][0].Get("mySymbol"));
            Assert.AreEqual(10.0, _feedListener.NewDataList[0][0].Get("Pricesum"));
            Assert.AreEqual("GE", _feedListener.NewDataList[1][0].Get("mySymbol"));
            Assert.AreEqual(30.0, _feedListener.NewDataList[1][0].Get("Pricesum"));
            Assert.AreEqual("IBM", _feedListener.NewDataList[2][0].Get("mySymbol"));
            Assert.AreEqual(80.0, _feedListener.NewDataList[2][0].Get("Pricesum"));
            _feedListener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(65 * 1000));
            Assert.IsFalse(_feedListener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(70 * 1000));
            Assert.AreEqual("ABC", _feedListener.NewDataList[0][0].Get("mySymbol"));
            Assert.AreEqual(91.0, _feedListener.NewDataList[0][0].Get("Pricesum"));
            Assert.AreEqual("DEF", _feedListener.NewDataList[1][0].Get("mySymbol"));
            Assert.AreEqual(191.0, _feedListener.NewDataList[1][0].Get("Pricesum"));
        }
    
        [Test]
        public void TestStaggeredWithWildcard() {
            var statementOne = "insert into streamA select * from " + typeof(SupportBeanSimple).FullName + ".win:length(5)";
            var statementTwo = "insert into streamB select *, MyInt+MyInt as summed, myString||myString as concat from streamA.win:length(5)";
            var statementThree = "insert into streamC select * from streamB.win:length(5)";
    
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            var listenerThree = new SupportUpdateListener();
    
            _epService.EPAdministrator.CreateEPL(statementOne).Events += listenerOne.Update;
            _epService.EPAdministrator.CreateEPL(statementTwo).Events += listenerTwo.Update;
            _epService.EPAdministrator.CreateEPL(statementThree).Events += listenerThree.Update;
    
            SendSimpleEvent("one", 1);
            AssertSimple(listenerOne, "one", 1, null, 0);
            AssertSimple(listenerTwo, "one", 1, "oneone", 2);
            AssertSimple(listenerThree, "one", 1, "oneone", 2);
    
            SendSimpleEvent("two", 2);
            AssertSimple(listenerOne, "two", 2, null, 0);
            AssertSimple(listenerTwo, "two", 2, "twotwo", 4);
            AssertSimple(listenerThree, "two", 2, "twotwo", 4);
        }
    
        [Test]
        public void TestInsertFromPattern() {
            var stmtOneText = "insert into streamA select * from pattern [every " + typeof(SupportBean).FullName + "]";
            var listenerOne = new SupportUpdateListener();
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtOneText);
            stmtOne.Events += listenerOne.Update;
    
            var stmtTwoText = "insert into streamA select * from pattern [every " + typeof(SupportBean).FullName + "]";
            var listenerTwo = new SupportUpdateListener();
            var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTwoText);
            stmtTwo.Events += listenerTwo.Update;
    
            var eventType = stmtOne.EventType;
            Assert.AreEqual(typeof(Map), eventType.UnderlyingType);
        }
    
        [Test]
        public void TestInsertIntoPlusPattern() {
            var stmtOneTxt = "insert into InZone " +
                    "select 111 as statementId, mac, locationReportId " +
                    "from " + typeof(SupportRFIDEvent).FullName + " " +
                    "where mac in ('1','2','3') " +
                    "and zoneID = '10'";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtOneTxt);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var stmtTwoTxt = "insert into OutOfZone " +
                    "select 111 as statementId, mac, locationReportId " +
                    "from " + typeof(SupportRFIDEvent).FullName + " " +
                    "where mac in ('1','2','3') " +
                    "and zoneID != '10'";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTwoTxt);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            var stmtThreeTxt = "select 111 as eventSpecId, A.locationReportId as locationReportId " +
                    " from pattern [every A=InZone -> (timer:interval(1 sec) and not OutOfZone(mac=A.mac))]";
            var stmtThree = _epService.EPAdministrator.CreateEPL(stmtThreeTxt);
            var listener = new SupportUpdateListener();
            stmtThree.Events += listener.Update;
    
            // try the alert case with 1 event for the mac in question
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPRuntime.SendEvent(new SupportRFIDEvent("LR1", "1", "10"));
            Assert.IsFalse(listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("LR1", theEvent.Get("locationReportId"));
    
            listenerOne.Reset();
            listenerTwo.Reset();
    
            // try the alert case with 2 events for zone 10 within 1 second for the mac in question
            _epService.EPRuntime.SendEvent(new SupportRFIDEvent("LR2", "2", "10"));
            Assert.IsFalse(listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            _epService.EPRuntime.SendEvent(new SupportRFIDEvent("LR3", "2", "10"));
            Assert.IsFalse(listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
    
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("LR2", theEvent.Get("locationReportId"));
        }
    
        [Test]
        public void TestNullType() {
            var stmtOneTxt = "insert into InZone select null as dummy from System.String";
            var stmtOne = _epService.EPAdministrator.CreateEPL(stmtOneTxt);
            Assert.IsTrue(stmtOne.EventType.IsProperty("dummy"));
    
            var stmtTwoTxt = "select dummy from InZone";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTwoTxt);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent("a");
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("dummy"));
        }
    
        private void AssertSimple(SupportUpdateListener listener, String myString, int myInt, String additionalString, int additionalInt) {
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            var eventBean = listener.LastNewData[0];
            Assert.AreEqual(myString, eventBean.Get("MyString"));
            Assert.AreEqual(myInt, eventBean.Get("MyInt"));
            if (additionalString != null) {
                Assert.AreEqual(additionalString, eventBean.Get("concat"));
                Assert.AreEqual(additionalInt, eventBean.Get("summed"));
            }
        }
    
        private void SendEvent(String symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, null, null);
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSimpleEvent(String stringValue, int val) {
            _epService.EPRuntime.SendEvent(new SupportBeanSimple(stringValue, val));
        }
    
        private EPStatement RunAsserts(String stmtText, EPStatementObjectModel model) {
            // Attach listener to feed
            EPStatement stmt = null;
            if (model != null) {
                stmt = _epService.EPAdministrator.Create(model, "s1");
            } else {
                stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            }
            stmt.Events += _feedListener.Update;
    
            // send event for joins to match on
            _epService.EPRuntime.SendEvent(new SupportBean_A("myId"));
    
            // Attach delta statement to statement and add listener
            stmtText = "select min(delta) as minD, max(delta) as maxD " +
                    "from Event_1.win:time(60)";
            var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtText);
            stmtTwo.Events += _resultListenerDelta.Update;
    
            // Attach prodict statement to statement and add listener
            stmtText = "select min(product) as minP, max(product) as maxP " +
                    "from Event_1.win:time(60)";
            var stmtThree = _epService.EPAdministrator.CreateEPL(stmtText);
            stmtThree.Events += _resultListenerProduct.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0)); // Set the time to 0 seconds
    
            // send events
            SendEvent(20, 10);
            AssertReceivedFeed(10, 200);
            AssertReceivedMinMax(10, 10, 200, 200);
    
            SendEvent(50, 25);
            AssertReceivedFeed(25, 25 * 50);
            AssertReceivedMinMax(10, 25, 200, 1250);
    
            SendEvent(5, 2);
            AssertReceivedFeed(3, 2 * 5);
            AssertReceivedMinMax(3, 25, 10, 1250);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10 * 1000)); // Set the time to 10 seconds
    
            SendEvent(13, 1);
            AssertReceivedFeed(12, 13);
            AssertReceivedMinMax(3, 25, 10, 1250);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(61 * 1000)); // Set the time to 61 seconds
            AssertReceivedMinMax(12, 12, 13, 13);
    
            return stmt;
        }
    
        private void AssertReceivedMinMax(int minDelta, int maxDelta, int minProduct, int maxProduct) {
            Assert.AreEqual(1, _resultListenerDelta.NewDataList.Count);
            Assert.AreEqual(1, _resultListenerDelta.LastNewData.Length);
            Assert.AreEqual(1, _resultListenerProduct.NewDataList.Count);
            Assert.AreEqual(1, _resultListenerProduct.LastNewData.Length);
            Assert.AreEqual(minDelta, _resultListenerDelta.LastNewData[0].Get("minD"));
            Assert.AreEqual(maxDelta, _resultListenerDelta.LastNewData[0].Get("maxD"));
            Assert.AreEqual(minProduct, _resultListenerProduct.LastNewData[0].Get("minP"));
            Assert.AreEqual(maxProduct, _resultListenerProduct.LastNewData[0].Get("maxP"));
            _resultListenerDelta.Reset();
            _resultListenerProduct.Reset();
        }
    
        private void AssertReceivedFeed(int delta, int product) {
            Assert.AreEqual(1, _feedListener.NewDataList.Count);
            Assert.AreEqual(1, _feedListener.LastNewData.Length);
            Assert.AreEqual(delta, _feedListener.LastNewData[0].Get("delta"));
            Assert.AreEqual(product, _feedListener.LastNewData[0].Get("product"));
            _feedListener.Reset();
        }
    
        private SupportBean SendEvent(int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.TheString = "myId";
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        public class MyP0P1EventSource
        {
            public MyP0P1EventSource(String p0, int p1)
            {
                P0 = p0;
                P1 = p1;
            }

            public string P0 { get; private set; }
            public int P1 { get; private set; }
        }

        public class MyP0P1EventTarget
        {
            public MyP0P1EventTarget()
            {
            }

            public MyP0P1EventTarget(String p0, int p1, Object c0)
            {
                P0 = p0;
                P1 = p1;
                C0 = c0;
            }

            public string P0 { get; set; }
            public int P1 { get; set; }
            public object C0 { get; set; }
        }
    }
}
