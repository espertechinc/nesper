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

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regression.epl.insertinto
{
    using Map = IDictionary<string, object>;

    public class ExecInsertInto : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionAssertionWildcardRecast(epService);
            RunAssertionVariantRStreamOMToStmt(epService);
            RunAssertionVariantOneOMToStmt(epService);
            RunAssertionVariantOneEPLToOMStmt(epService);
            RunAssertionVariantOne(epService);
            RunAssertionVariantOneStateless(epService);
            RunAssertionVariantOneWildcard(epService);
            RunAssertionVariantOneJoin(epService);
            RunAssertionVariantOneJoinWildcard(epService);
            RunAssertionVariantTwo(epService);
            RunAssertionVariantTwoWildcard(epService);
            RunAssertionVariantTwoJoin(epService);
            RunAssertionJoinWildcard(epService);
            RunAssertionInvalidStreamUsed(epService);
            RunAssertionWithOutputLimitAndSort(epService);
            RunAssertionStaggeredWithWildcard(epService);
            RunAssertionInsertFromPattern(epService);
            RunAssertionInsertIntoPlusPattern(epService);
            RunAssertionNullType(epService);
        }
    
        private void RunAssertionAssertionWildcardRecast(EPServiceProvider epService) {
            // bean to OA/Map/bean
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionWildcardRecast(epService, true, null, false, rep);
            }
    
            try {
                TryAssertionWildcardRecast(epService, true, null, true, null);
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Expression-returned event type 'SourceSchema' with underlying type 'com.espertech.esper.regression.epl.insertinto.ExecInsertInto+MyP0P1EventSource' cannot be converted to target event type 'TargetSchema' with underlying type ");
            }
    
            // OA
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.ARRAY, false, EventRepresentationChoice.ARRAY);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.ARRAY, false, EventRepresentationChoice.MAP);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.ARRAY, false, EventRepresentationChoice.AVRO);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.ARRAY, true, null);
    
            // Map
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.MAP, false, EventRepresentationChoice.ARRAY);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.MAP, false, EventRepresentationChoice.MAP);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.MAP, false, EventRepresentationChoice.AVRO);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.MAP, true, null);
    
            // Avro
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.AVRO, false, EventRepresentationChoice.ARRAY);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.AVRO, false, EventRepresentationChoice.MAP);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.AVRO, false, EventRepresentationChoice.AVRO);
            TryAssertionWildcardRecast(epService, false, EventRepresentationChoice.AVRO, true, null);
        }
    
        private void RunAssertionVariantRStreamOMToStmt(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.InsertInto = InsertIntoClause.Create("Event_1_RSOM", new string[0], StreamSelector.RSTREAM_ONLY);
            model.SelectClause = SelectClause.Create().Add("IntPrimitive", "IntBoxed");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            EPStatement stmt = epService.EPAdministrator.Create(model, "s1");
    
            string epl = "insert rstream into Event_1_RSOM " +
                    "select IntPrimitive, IntBoxed " +
                    "from " + typeof(SupportBean).FullName;
            Assert.AreEqual(epl, model.ToEPL());
            Assert.AreEqual(epl, stmt.Text);
    
            EPStatementObjectModel modelTwo = epService.EPAdministrator.CompileEPL(model.ToEPL());
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, modelTwo.ToEPL());
    
            // assert statement-type reference
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("Event_1_RSOM"));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsTrue(stmtNames.Contains("s1"));
    
            stmt.Dispose();
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("Event_1_RSOM"));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsFalse(stmtNames.Contains("s1"));
        }
    
        private void RunAssertionVariantOneOMToStmt(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.InsertInto = InsertIntoClause.Create("Event_1_OMS", "delta", "product");
            model.SelectClause = SelectClause.Create().Add(Expressions.Minus("IntPrimitive", "IntBoxed"), "deltaTag")
                    .Add(Expressions.Multiply("IntPrimitive", "IntBoxed"), "productTag");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView(View.Create("length", Expressions.Constant(100))));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            EPStatement stmt = TryAssertsVariant(epService, null, model, "Event_1_OMS");
    
            string epl = "insert into Event_1_OMS(delta, product) " +
                    "select IntPrimitive-IntBoxed as deltaTag, IntPrimitive*IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + "#length(100)";
            Assert.AreEqual(epl, model.ToEPL());
            Assert.AreEqual(epl, stmt.Text);
    
            stmt.Dispose();
        }
    
        private void RunAssertionVariantOneEPLToOMStmt(EPServiceProvider epService) {
            string epl = "insert into Event_1_EPL(delta, product) " +
                    "select IntPrimitive-IntBoxed as deltaTag, IntPrimitive*IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = TryAssertsVariant(epService, null, model, "Event_1_EPL");
            Assert.AreEqual(epl, stmt.Text);
            stmt.Dispose();
        }
    
        private void RunAssertionVariantOne(EPServiceProvider epService) {
            string stmtText = "insert into Event_1VO (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + "#length(100)";
    
            TryAssertsVariant(epService, stmtText, null, "Event_1VO").Dispose();
        }
    
        private void RunAssertionVariantOneStateless(EPServiceProvider epService) {
            string stmtTextStateless = "insert into Event_1VOS (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName;
            TryAssertsVariant(epService, stmtTextStateless, null, "Event_1VOS").Dispose();
        }
    
        private void RunAssertionVariantOneWildcard(EPServiceProvider epService) {
            string stmtText = "insert into Event_1W (delta, product) " +
                    "select * from " + typeof(SupportBean).FullName + "#length(100)";
    
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException) {
                // Expected
            }
    
            // assert statement-type reference
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("Event_1W"));
    
            // test insert wildcard to wildcard
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var listener = new SupportUpdateListener();
    
            string stmtSelectText = "insert into ABCStream select * from SupportBean";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtSelectText, "resilient i0");
            stmtSelect.Events += listener.Update;
            Assert.IsTrue(stmtSelect.EventType is BeanEventType);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNew().Get("TheString"));
            Assert.IsTrue(listener.AssertOneGetNew().Underlying is SupportBean);
    
            stmtSelect.Dispose();
        }
    
        private void RunAssertionVariantOneJoin(EPServiceProvider epService) {
            string stmtText = "insert into Event_1J (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + "#length(100) as s0," +
                    typeof(SupportBean_A).FullName + "#length(100) as s1 " +
                    " where s0.TheString = s1.id";
    
            TryAssertsVariant(epService, stmtText, null, "Event_1J").Dispose();
        }
    
        private void RunAssertionVariantOneJoinWildcard(EPServiceProvider epService) {
            string stmtText = "insert into Event_1JW (delta, product) " +
                    "select * " +
                    "from " + typeof(SupportBean).FullName + "#length(100) as s0," +
                    typeof(SupportBean_A).FullName + "#length(100) as s1 " +
                    " where s0.TheString = s1.id";
    
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException) {
                // Expected
            }
        }
    
        private void RunAssertionVariantTwo(EPServiceProvider epService) {
            string stmtText = "insert into Event_1_2 " +
                    "select IntPrimitive - IntBoxed as delta, IntPrimitive * IntBoxed as product " +
                    "from " + typeof(SupportBean).FullName + "#length(100)";
    
            TryAssertsVariant(epService, stmtText, null, "Event_1_2").Dispose();
        }
    
        private void RunAssertionVariantTwoWildcard(EPServiceProvider epService) {
            string stmtText = "insert into event1 select * from " + typeof(SupportBean).FullName + "#length(100)";
            string otherText = "select * from default.event1#length(10)";
    
            // Attach listener to feed
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtText, "stmt1");
            Assert.AreEqual(StatementType.INSERT_INTO, ((EPStatementSPI) stmtOne).StatementMetadata.StatementType);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(otherText, "stmt2");
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            SupportBean theEvent = SendEvent(epService, 10, 11);
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
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("event1"));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("event1");
            EPAssertionUtil.AssertEqualsAnyOrder(stmtNames.ToArray(), new string[]{"stmt1", "stmt2"});
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtNames.ToArray(), new string[]{"stmt1"});
    
            stmtOne.Dispose();
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("event1"));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("event1");
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmt2"}, stmtNames.ToArray());
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
    
            stmtTwo.Dispose();
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("event1"));
        }
    
        private void RunAssertionVariantTwoJoin(EPServiceProvider epService) {
            string stmtText = "insert into Event_1_2J " +
                    "select IntPrimitive - IntBoxed as delta, IntPrimitive * IntBoxed as product " +
                    "from " + typeof(SupportBean).FullName + "#length(100) as s0," +
                    typeof(SupportBean_A).FullName + "#length(100) as s1 " +
                    " where s0.TheString = s1.id";
    
            EPStatement stmt = TryAssertsVariant(epService, stmtText, null, "Event_1_2J");
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("Event_1_2J");
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("Event_1_2J", type.Metadata.PrimaryName);
            Assert.AreEqual("Event_1_2J", type.Metadata.PublicName);
            Assert.AreEqual("Event_1_2J", type.Name);
            Assert.AreEqual(TypeClass.STREAM, type.Metadata.TypeClass);
            Assert.AreEqual(false, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinWildcard(EPServiceProvider epService) {
            TryAssertionJoinWildcard(epService, true, null);
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionJoinWildcard(epService, false, rep);
            }
        }
    
        private void RunAssertionInvalidStreamUsed(EPServiceProvider epService) {
            string stmtText = "insert into Event_1IS (delta, product) " +
                    "select IntPrimitive - IntBoxed as deltaTag, IntPrimitive * IntBoxed as productTag " +
                    "from " + typeof(SupportBean).FullName + "#length(100)";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            try {
                stmtText = "insert into Event_1IS(delta) " +
                        "select (IntPrimitive - IntBoxed) as deltaTag " +
                        "from " + typeof(SupportBean).FullName + "#length(100)";
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                // expected
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Event type named 'Event_1IS' has already been declared with differing column name or type information: Type by name 'Event_1IS' expects 2 properties but receives 1 properties ");
            }
        }
    
        private void RunAssertionWithOutputLimitAndSort(EPServiceProvider epService) {
            // NOTICE: we are inserting the RSTREAM (removed events)
            string stmtText = "insert rstream into StockTicks(mySymbol, myPrice) " +
                    "select symbol, price from " + typeof(SupportMarketDataBean).FullName + "#time(60) " +
                    "output every 5 seconds " +
                    "order by symbol asc";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "select mySymbol, sum(myPrice) as pricesum from StockTicks#length(100)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SendEvent(epService, "IBM", 50);
            SendEvent(epService, "CSC", 10);
            SendEvent(epService, "GE", 20);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10 * 1000));
            SendEvent(epService, "DEF", 100);
            SendEvent(epService, "ABC", 11);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20 * 1000));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(30 * 1000));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(40 * 1000));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(50 * 1000));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(55 * 1000));
    
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(60 * 1000));
    
            Assert.IsTrue(listener.IsInvoked);
            Assert.AreEqual(3, listener.NewDataList.Count);
            Assert.AreEqual("CSC", listener.NewDataList[0][0].Get("mySymbol"));
            Assert.AreEqual(10.0, listener.NewDataList[0][0].Get("pricesum"));
            Assert.AreEqual("GE", listener.NewDataList[1][0].Get("mySymbol"));
            Assert.AreEqual(30.0, listener.NewDataList[1][0].Get("pricesum"));
            Assert.AreEqual("IBM", listener.NewDataList[2][0].Get("mySymbol"));
            Assert.AreEqual(80.0, listener.NewDataList[2][0].Get("pricesum"));
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(65 * 1000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(70 * 1000));
            Assert.AreEqual("ABC", listener.NewDataList[0][0].Get("mySymbol"));
            Assert.AreEqual(91.0, listener.NewDataList[0][0].Get("pricesum"));
            Assert.AreEqual("DEF", listener.NewDataList[1][0].Get("mySymbol"));
            Assert.AreEqual(191.0, listener.NewDataList[1][0].Get("pricesum"));
    
            statement.Dispose();
        }
    
        private void RunAssertionStaggeredWithWildcard(EPServiceProvider epService) {
            string statementOne = "insert into streamA select * from " + typeof(SupportBeanSimple).FullName + "#length(5)";
            string statementTwo = "insert into streamB select *, myInt+myInt as summed, myString||myString as concat from streamA#length(5)";
            string statementThree = "insert into streamC select * from streamB#length(5)";
    
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            var listenerThree = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL(statementOne).Events += listenerOne.Update;
            epService.EPAdministrator.CreateEPL(statementTwo).Events += listenerTwo.Update;
            epService.EPAdministrator.CreateEPL(statementThree).Events += listenerThree.Update;
    
            SendSimpleEvent(epService, "one", 1);
            AssertSimple(listenerOne, "one", 1, null, 0);
            AssertSimple(listenerTwo, "one", 1, "oneone", 2);
            AssertSimple(listenerThree, "one", 1, "oneone", 2);
    
            SendSimpleEvent(epService, "two", 2);
            AssertSimple(listenerOne, "two", 2, null, 0);
            AssertSimple(listenerTwo, "two", 2, "twotwo", 4);
            AssertSimple(listenerThree, "two", 2, "twotwo", 4);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInsertFromPattern(EPServiceProvider epService) {
            string stmtOneText = "insert into streamA1 select * from pattern [every " + typeof(SupportBean).FullName + "]";
            var listenerOne = new SupportUpdateListener();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtOneText);
            stmtOne.Events += listenerOne.Update;
    
            string stmtTwoText = "insert into streamA1 select * from pattern [every " + typeof(SupportBean).FullName + "]";
            var listenerTwo = new SupportUpdateListener();
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTwoText);
            stmtTwo.Events += listenerTwo.Update;
    
            EventType eventType = stmtOne.EventType;
            Assert.AreEqual(typeof(Map), eventType.UnderlyingType);
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionInsertIntoPlusPattern(EPServiceProvider epService) {
            string stmtOneTxt = "insert into InZone " +
                    "select 111 as statementId, mac, locationReportId " +
                    "from " + typeof(SupportRFIDEvent).FullName + " " +
                    "where mac in ('1','2','3') " +
                    "and zoneID = '10'";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtOneTxt);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            string stmtTwoTxt = "insert into OutOfZone " +
                    "select 111 as statementId, mac, locationReportId " +
                    "from " + typeof(SupportRFIDEvent).FullName + " " +
                    "where mac in ('1','2','3') " +
                    "and zoneID != '10'";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTwoTxt);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            string stmtThreeTxt = "select 111 as eventSpecId, A.locationReportId as locationReportId " +
                    " from pattern [every A=InZone -> (timer:interval(1 sec) and not OutOfZone(mac=A.mac))]";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtThreeTxt);
            var listener = new SupportUpdateListener();
            stmtThree.Events += listener.Update;
    
            // try the alert case with 1 event for the mac in question
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPRuntime.SendEvent(new SupportRFIDEvent("LR1", "1", "10"));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("LR1", theEvent.Get("locationReportId"));
    
            listenerOne.Reset();
            listenerTwo.Reset();
    
            // try the alert case with 2 events for zone 10 within 1 second for the mac in question
            epService.EPRuntime.SendEvent(new SupportRFIDEvent("LR2", "2", "10"));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            epService.EPRuntime.SendEvent(new SupportRFIDEvent("LR3", "2", "10"));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
    
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("LR2", theEvent.Get("locationReportId"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNullType(EPServiceProvider epService) {
            string stmtOneTxt = "insert into InZoneTwo select null as dummy from System.String";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtOneTxt);
            Assert.IsTrue(stmtOne.EventType.IsProperty("dummy"));
    
            string stmtTwoTxt = "select dummy from InZoneTwo";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTwoTxt);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent("a");
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("dummy"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertSimple(SupportUpdateListener listener, string myString, int myInt, string additionalString, int additionalInt) {
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            EventBean eventBean = listener.LastNewData[0];
            Assert.AreEqual(myString, eventBean.Get("myString"));
            Assert.AreEqual(myInt, eventBean.Get("myInt"));
            if (additionalString != null) {
                Assert.AreEqual(additionalString, eventBean.Get("concat"));
                Assert.AreEqual(additionalInt, eventBean.Get("summed"));
            }
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, null, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSimpleEvent(EPServiceProvider epService, string theString, int val) {
            epService.EPRuntime.SendEvent(new SupportBeanSimple(theString, val));
        }
    
        private EPStatement TryAssertsVariant(EPServiceProvider epService, string stmtText, EPStatementObjectModel model, string typeName) {
            // Attach listener to feed
            EPStatement stmt;
            if (model != null) {
                stmt = epService.EPAdministrator.Create(model, "s1");
            } else {
                stmt = epService.EPAdministrator.CreateEPL(stmtText);
            }
            var feedListener = new SupportUpdateListener();
            stmt.Events += feedListener.Update;
    
            // send event for joins to match on
            epService.EPRuntime.SendEvent(new SupportBean_A("myId"));
    
            // Attach delta statement to statement and add listener
            stmtText = "select MIN(delta) as minD, max(delta) as maxD " +
                    "from " + typeName + "#time(60)";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtText);
            var resultListenerDelta = new SupportUpdateListener();
            stmtTwo.Events += resultListenerDelta.Update;
    
            // Attach prodict statement to statement and add listener
            stmtText = "select min(product) as minP, max(product) as maxP " +
                    "from " + typeName + "#time(60)";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtText);
            var resultListenerProduct = new SupportUpdateListener();
            stmtThree.Events += resultListenerProduct.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0)); // Set the time to 0 seconds
    
            // send events
            SendEvent(epService, 20, 10);
            AssertReceivedFeed(feedListener, 10, 200);
            AssertReceivedMinMax(resultListenerDelta, resultListenerProduct, 10, 10, 200, 200);
    
            SendEvent(epService, 50, 25);
            AssertReceivedFeed(feedListener, 25, 25 * 50);
            AssertReceivedMinMax(resultListenerDelta, resultListenerProduct, 10, 25, 200, 1250);
    
            SendEvent(epService, 5, 2);
            AssertReceivedFeed(feedListener, 3, 2 * 5);
            AssertReceivedMinMax(resultListenerDelta, resultListenerProduct, 3, 25, 10, 1250);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10 * 1000)); // Set the time to 10 seconds
    
            SendEvent(epService, 13, 1);
            AssertReceivedFeed(feedListener, 12, 13);
            AssertReceivedMinMax(resultListenerDelta, resultListenerProduct, 3, 25, 10, 1250);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(61 * 1000)); // Set the time to 61 seconds
            AssertReceivedMinMax(resultListenerDelta, resultListenerProduct, 12, 12, 13, 13);
    
            return stmt;
        }
    
        private void AssertReceivedMinMax(SupportUpdateListener resultListenerDelta, SupportUpdateListener resultListenerProduct, int minDelta, int maxDelta, int minProduct, int maxProduct) {
            Assert.AreEqual(1, resultListenerDelta.NewDataList.Count);
            Assert.AreEqual(1, resultListenerDelta.LastNewData.Length);
            Assert.AreEqual(1, resultListenerProduct.NewDataList.Count);
            Assert.AreEqual(1, resultListenerProduct.LastNewData.Length);
            Assert.AreEqual(minDelta, resultListenerDelta.LastNewData[0].Get("minD"));
            Assert.AreEqual(maxDelta, resultListenerDelta.LastNewData[0].Get("maxD"));
            Assert.AreEqual(minProduct, resultListenerProduct.LastNewData[0].Get("minP"));
            Assert.AreEqual(maxProduct, resultListenerProduct.LastNewData[0].Get("maxP"));
            resultListenerDelta.Reset();
            resultListenerProduct.Reset();
        }
    
        private void AssertReceivedFeed(SupportUpdateListener feedListener, int delta, int product) {
            Assert.AreEqual(1, feedListener.NewDataList.Count);
            Assert.AreEqual(1, feedListener.LastNewData.Length);
            Assert.AreEqual(delta, feedListener.LastNewData[0].Get("delta"));
            Assert.AreEqual(product, feedListener.LastNewData[0].Get("product"));
            feedListener.Reset();
        }
    
        private void AssertJoinWildcard(
            EventRepresentationChoice? rep, 
            SupportUpdateListener listener,
            Object eventS0, 
            Object eventS1)
        {
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(2, listener.LastNewData[0].EventType.PropertyNames.Length);
            Assert.IsTrue(listener.LastNewData[0].EventType.IsProperty("s0"));
            Assert.IsTrue(listener.LastNewData[0].EventType.IsProperty("s1"));
            Assert.AreSame(eventS0, listener.LastNewData[0].Get("s0"));
            Assert.AreSame(eventS1, listener.LastNewData[0].Get("s1"));
            Assert.IsTrue(rep == null || rep.Value.MatchesClass(listener.LastNewData[0].Underlying.GetType()));
        }
    
        private void TryAssertionJoinWildcard(
            EPServiceProvider epService, 
            bool bean, 
            EventRepresentationChoice? rep)
        {
            if (bean)
            {
                epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean));
                epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_A));
            } else if (rep == null) {
                Assert.Fail();
            } else if (rep.Value.IsMapEvent()) {
                epService.EPAdministrator.Configuration.AddEventType("S0", Collections.SingletonDataMap("TheString", typeof(string)));
                epService.EPAdministrator.Configuration.AddEventType("S1", Collections.SingletonDataMap("id", typeof(string)));
            } else if (rep.Value.IsObjectArrayEvent()) {
                epService.EPAdministrator.Configuration.AddEventType("S0", new string[]{"TheString"}, new object[]{typeof(string)});
                epService.EPAdministrator.Configuration.AddEventType("S1", new string[]{"id"}, new object[]{typeof(string)});
            } else if (rep.Value.IsAvroEvent()) {
                epService.EPAdministrator.Configuration.AddEventTypeAvro("S0", new ConfigurationEventTypeAvro() {
                    AvroSchema = SchemaBuilder.Record("S0", RequiredString("TheString"))
                });
                epService.EPAdministrator.Configuration.AddEventTypeAvro("S1", new ConfigurationEventTypeAvro() {
                    AvroSchema = SchemaBuilder.Record("S1", RequiredString("id"))
                });
            } else {
                Assert.Fail();
            }
    
            string textOne = (bean ? "" : rep.Value.GetAnnotationText()) + "insert into event2 select * " +
                    "from S0#length(100) as s0, S1#length(5) as s1 " +
                    "where s0.TheString = s1.id";
            string textTwo = (bean ? "" : rep.Value.GetAnnotationText()) + "select * from event2#length(10)";
    
            // Attach listener to feed
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(textTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            // send event for joins to match on
            Object eventS1;
            if (bean) {
                eventS1 = new SupportBean_A("myId");
                epService.EPRuntime.SendEvent(eventS1);
            } else if (rep.Value.IsMapEvent()) {
                eventS1 = Collections.SingletonDataMap("id", "myId");
                epService.EPRuntime.SendEvent((Map) eventS1, "S1");
            } else if (rep.Value.IsObjectArrayEvent()) {
                eventS1 = new object[]{"myId"};
                epService.EPRuntime.SendEvent((object[]) eventS1, "S1");
            } else if (rep.Value.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "S1").AsRecordSchema());
                theEvent.Put("id", "myId");
                eventS1 = theEvent;
                epService.EPRuntime.SendEventAvro(theEvent, "S1");
            } else {
                throw new ArgumentException();
            }
    
            Object eventS0;
            if (bean) {
                eventS0 = new SupportBean("myId", -1);
                epService.EPRuntime.SendEvent(eventS0);
            } else if (rep.Value.IsMapEvent()) {
                eventS0 = Collections.SingletonDataMap("TheString", "myId");
                epService.EPRuntime.SendEvent((Map) eventS0, "S0");
            } else if (rep.Value.IsObjectArrayEvent()) {
                eventS0 = new object[]{"myId"};
                epService.EPRuntime.SendEvent((object[]) eventS0, "S0");
            } else if (rep.Value.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "S0").AsRecordSchema());
                theEvent.Put("TheString", "myId");
                eventS0 = theEvent;
                epService.EPRuntime.SendEventAvro(theEvent, "S0");
            } else {
                throw new ArgumentException();
            }
    
            AssertJoinWildcard(rep, listenerOne, eventS0, eventS1);
            AssertJoinWildcard(rep, listenerTwo, eventS0, eventS1);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
            epService.EPAdministrator.Configuration.RemoveEventType("S0", false);
            epService.EPAdministrator.Configuration.RemoveEventType("S1", false);
            epService.EPAdministrator.Configuration.RemoveEventType("event2", false);
        }
    
        private SupportBean SendEvent(EPServiceProvider epService, int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.TheString = "myId";
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void TryAssertionWildcardRecast(
            EPServiceProvider epService,
            bool sourceBean, 
            EventRepresentationChoice? sourceType,
            bool targetBean,
            EventRepresentationChoice? targetType)
        {
            try {
                TryAssertionWildcardRecastInternal(epService, sourceBean, sourceType, targetBean, targetType);
            } finally {
                // cleanup
                epService.EPAdministrator.DestroyAllStatements();
                epService.EPAdministrator.Configuration.RemoveEventType("TargetSchema", false);
                epService.EPAdministrator.Configuration.RemoveEventType("SourceSchema", false);
                epService.EPAdministrator.Configuration.RemoveEventType("TargetContainedSchema", false);
            }
        }
    
        private void TryAssertionWildcardRecastInternal(
            EPServiceProvider epService, 
            bool sourceBean, 
            EventRepresentationChoice? sourceType,
            bool targetBean, 
            EventRepresentationChoice? targetType)
        {
            // declare source type
            if (sourceBean) {
                epService.EPAdministrator.CreateEPL("create schema SourceSchema as " + TypeHelper.MaskTypeName<MyP0P1EventSource>());
            } else {
                epService.EPAdministrator.CreateEPL("create " + sourceType.Value.GetOutputTypeCreateSchemaName() + " schema SourceSchema as (P0 string, P1 int)");
            }
    
            // declare target type
            if (targetBean) {
                epService.EPAdministrator.CreateEPL("create schema TargetSchema as " + TypeHelper.MaskTypeName<MyP0P1EventTarget>());
            } else {
                epService.EPAdministrator.CreateEPL("create " + targetType.Value.GetOutputTypeCreateSchemaName() + " schema TargetContainedSchema as (C0 int)");
                epService.EPAdministrator.CreateEPL("create " + targetType.Value.GetOutputTypeCreateSchemaName() + " schema TargetSchema (P0 string, P1 int, C0 TargetContainedSchema)");
            }
    
            // insert-into and select
            epService.EPAdministrator.CreateEPL("insert into TargetSchema select * from SourceSchema");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from TargetSchema");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // send event
            if (sourceBean) {
                epService.EPRuntime.SendEvent(new MyP0P1EventSource("a", 10));
            } else if (sourceType.Value.IsMapEvent()) {
                var map = new Dictionary<string, object>();
                map.Put("P0", "a");
                map.Put("P1", 10);
                epService.EPRuntime.SendEvent(map, "SourceSchema");
            } else if (sourceType.Value.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"a", 10}, "SourceSchema");
            } else if (sourceType.Value.IsAvroEvent()) {
                var schema = SchemaBuilder.Record("schema",
                    RequiredString("P0"),
                    RequiredString("P1"),
                    RequiredString("C0"));
                var record = new GenericRecord(schema);
                record.Put("P0", "a");
                record.Put("P1", 10);
                epService.EPRuntime.SendEventAvro(record, "SourceSchema");
            } else {
                Assert.Fail();
            }
    
            // assert
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "P0,P1,C0".Split(','), new object[]{"a", 10, null});
        }
    
        public class MyP0P1EventSource {
            public MyP0P1EventSource(string p0, int p1) {
                P0 = p0;
                P1 = p1;
            }

            public string P0 { get; }

            public int P1 { get; }
        }
    
        public class MyP0P1EventTarget {
            public MyP0P1EventTarget() {
            }

            public MyP0P1EventTarget(string p0, int p1, Object c0) {
                P0 = p0;
                P1 = p1;
                C0 = c0;
            }

            public string P0 { get; set; }

            public int P1 { get; set; }

            public object C0 { get; set; }
        }
    }
} // end of namespace
