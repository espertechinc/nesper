///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.bookexample;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regression.epl.other
{
    using Map = IDictionary<string, object>;

    public class ExecEPLSplitStream : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionFromClause(epService);
            RunAssertionSplitPremptiveNamedWindow(epService);
            RunAssertion1SplitDefault(epService);
            RunAssertion2SplitNoDefaultOutputFirst(epService);
            RunAssertionSubquery(epService);
            RunAssertion2SplitNoDefaultOutputAll(epService);
            RunAssertion3And4SplitDefaultOutputFirst(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2",
                    "Error starting statement: Required insert-into clause is not provided, the clause is required for split-stream syntax");
    
            SupportMessageAssertUtil.TryInvalid(epService, "on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2",
                    "Error starting statement: A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax");
    
            SupportMessageAssertUtil.TryInvalid(epService, "on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select avg(IntPrimitive) where 1=2",
                    "Error starting statement: Aggregation functions are not allowed in this context");
        }
    
        private void RunAssertionFromClause(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OrderEvent", typeof(OrderBean));
    
            TryAssertionFromClauseBeginBodyEnd(epService);
            TryAssertionFromClauseAsMultiple(epService);
            TryAssertionFromClauseOutputFirstWhere(epService);
            TryAssertionFromClauseDocSample(epService);
        }
    
        private void RunAssertionSplitPremptiveNamedWindow(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionSplitPremptiveNamedWindow(epService, rep);
            }
        }
    
        private void RunAssertion1SplitDefault(EPServiceProvider epService) {
            // test wildcard
            var stmtOrigText = "on SupportBean insert into AStream select *";
            var stmt = epService.EPAdministrator.CreateEPL(stmtOrigText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var listeners = GetListeners();
            var stmtOne = epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += listeners[0].Update;
    
            SendSupportBean(epService, "E1", 1);
            AssertReceivedSingle(listeners, 0, "E1");
            Assert.IsFalse(listener.IsInvoked);
    
            // test select
            stmtOrigText = "on SupportBean insert into BStreamABC select 3*IntPrimitive as value";
            var stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
    
            stmtOne = epService.EPAdministrator.CreateEPL("select value from BStreamABC");
            stmtOne.Events += listeners[1].Update;
    
            SendSupportBean(epService, "E1", 6);
            Assert.AreEqual(18, listeners[1].AssertOneGetNewAndReset().Get("value"));
    
            // assert type is original type
            Assert.AreEqual(typeof(SupportBean), stmtOrig.EventType.UnderlyingType);
            Assert.IsFalse(stmtOrig.HasFirst());
    
            stmtOne.Dispose();
        }
    
        private void RunAssertion2SplitNoDefaultOutputFirst(EPServiceProvider epService) {
            var stmtOrigText = "@Audit on SupportBean " +
                    "insert into AStream2SP select * where IntPrimitive=1 " +
                    "insert into BStream2SP select * where IntPrimitive=1 or IntPrimitive=2";
            var stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
            TryAssertion(epService, stmtOrig);
    
            // statement object model
            var model = new EPStatementObjectModel();
            model.Annotations = Collections.SingletonList(new AnnotationPart("Audit"));
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean"));
            model.InsertInto = InsertIntoClause.Create("AStream2SP");
            model.SelectClause = SelectClause.CreateWildcard();
            model.WhereClause = Expressions.Eq("IntPrimitive", 1);
            var clause = OnClause.CreateOnInsertSplitStream();
            model.OnExpr = clause;
            var item = OnInsertSplitStreamItem.Create(
                    InsertIntoClause.Create("BStream2SP"),
                    SelectClause.CreateWildcard(),
                    Expressions.Or(Expressions.Eq("IntPrimitive", 1), Expressions.Eq("IntPrimitive", 2)));
            clause.AddItem(item);
            Assert.AreEqual(stmtOrigText, model.ToEPL());
            stmtOrig = epService.EPAdministrator.Create(model);
            TryAssertion(epService, stmtOrig);
    
            var newModel = epService.EPAdministrator.CompileEPL(stmtOrigText);
            stmtOrig = epService.EPAdministrator.Create(newModel);
            Assert.AreEqual(stmtOrigText, newModel.ToEPL());
            TryAssertion(epService, stmtOrig);
    
            SupportModelHelper.CompileCreate(epService, stmtOrigText + " output all");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubquery(EPServiceProvider epService) {
            var stmtOrigText = "on SupportBean " +
                    "insert into AStreamSub select (select p00 from S0#lastevent) as string where IntPrimitive=(select id from S0#lastevent) " +
                    "insert into BStreamSub select (select p01 from S0#lastevent) as string where IntPrimitive<>(select id from S0#lastevent) or (select id from S0#lastevent) is null";
            var stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
            var listener = new SupportUpdateListener();
            stmtOrig.Events += listener.Update;
    
            var stmtOne = epService.EPAdministrator.CreateEPL("select * from AStreamSub");
            var listenerAStream = new SupportUpdateListener();
            stmtOne.Events += listenerAStream.Update;
            var stmtTwo = epService.EPAdministrator.CreateEPL("select * from BStreamSub");
            var listenerBStream = new SupportUpdateListener();
            stmtTwo.Events += listenerBStream.Update;
    
            SendSupportBean(epService, "E1", 1);
            Assert.IsFalse(listenerAStream.GetAndClearIsInvoked());
            Assert.IsNull(listenerBStream.AssertOneGetNewAndReset().Get("string"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "x", "y"));
    
            SendSupportBean(epService, "E2", 10);
            Assert.AreEqual("x", listenerAStream.AssertOneGetNewAndReset().Get("string"));
            Assert.IsFalse(listenerBStream.GetAndClearIsInvoked());
    
            SendSupportBean(epService, "E3", 9);
            Assert.IsFalse(listenerAStream.GetAndClearIsInvoked());
            Assert.AreEqual("y", listenerBStream.AssertOneGetNewAndReset().Get("string"));
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertion2SplitNoDefaultOutputAll(EPServiceProvider epService) {
            var stmtOrigText = "on SupportBean " +
                    "insert into AStream2S select TheString where IntPrimitive=1 " +
                    "insert into BStream2S select TheString where IntPrimitive=1 or IntPrimitive=2 " +
                    "output all";
            var stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
            var listener = new SupportUpdateListener();
            stmtOrig.Events += listener.Update;
    
            var listeners = GetListeners();
            var stmtOne = epService.EPAdministrator.CreateEPL("select * from AStream2S");
            stmtOne.Events += listeners[0].Update;
            var stmtTwo = epService.EPAdministrator.CreateEPL("select * from BStream2S");
            stmtTwo.Events += listeners[1].Update;
    
            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);
    
            SendSupportBean(epService, "E1", 1);
            AssertReceivedEach(listeners, new[]{"E1", "E1"});
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E2", 2);
            AssertReceivedEach(listeners, new[]{null, "E2"});
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E3", 1);
            AssertReceivedEach(listeners, new[]{"E3", "E3"});
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E4", -999);
            AssertReceivedEach(listeners, new string[]{null, null});
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            stmtOrig.Dispose();
            stmtOrigText = "on SupportBean " +
                    "insert into AStream2S select TheString || '_1' as TheString where IntPrimitive in (1, 2) " +
                    "insert into BStream2S select TheString || '_2' as TheString where IntPrimitive in (2, 3) " +
                    "insert into CStream2S select TheString || '_3' as TheString " +
                    "output all";
            stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += listener.Update;
    
            var stmtThree = epService.EPAdministrator.CreateEPL("select * from CStream2S");
            stmtThree.Events += listeners[2].Update;
    
            SendSupportBean(epService, "E1", 2);
            AssertReceivedEach(listeners, new[]{"E1_1", "E1_2", "E1_3"});
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E2", 1);
            AssertReceivedEach(listeners, new[]{"E2_1", null, "E2_3"});
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E3", 3);
            AssertReceivedEach(listeners, new[]{null, "E3_2", "E3_3"});
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E4", -999);
            AssertReceivedEach(listeners, new[]{null, null, "E4_3"});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion3And4SplitDefaultOutputFirst(EPServiceProvider epService) {
            var stmtOrigText = "on SupportBean as mystream " +
                    "insert into AStream34 select mystream.TheString||'_1' as TheString where IntPrimitive=1 " +
                    "insert into BStream34 select mystream.TheString||'_2' as TheString where IntPrimitive=2 " +
                    "insert into CStream34 select TheString||'_3' as TheString";
            var stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
            var listener = new SupportUpdateListener();
            stmtOrig.Events += listener.Update;
    
            var listeners = GetListeners();
            var stmtOne = epService.EPAdministrator.CreateEPL("select * from AStream34");
            stmtOne.Events += listeners[0].Update;
            var stmtTwo = epService.EPAdministrator.CreateEPL("select * from BStream34");
            stmtTwo.Events += listeners[1].Update;
            var stmtThree = epService.EPAdministrator.CreateEPL("select * from CStream34");
            stmtThree.Events += listeners[2].Update;

            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);
    
            SendSupportBean(epService, "E1", 1);
            AssertReceivedSingle(listeners, 0, "E1_1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E2", 2);
            AssertReceivedSingle(listeners, 1, "E2_2");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E3", 1);
            AssertReceivedSingle(listeners, 0, "E3_1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E4", -999);
            AssertReceivedSingle(listeners, 2, "E4_3");
            Assert.IsFalse(listener.IsInvoked);
    
            stmtOrigText = "on SupportBean " +
                    "insert into AStream34 select TheString||'_1' as TheString where IntPrimitive=10 " +
                    "insert into BStream34 select TheString||'_2' as TheString where IntPrimitive=20 " +
                    "insert into CStream34 select TheString||'_3' as TheString where IntPrimitive<0 " +
                    "insert into DStream34 select TheString||'_4' as TheString";
            stmtOrig.Dispose();
            stmtOrig = epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += listener.Update;
    
            var stmtFour = epService.EPAdministrator.CreateEPL("select * from DStream34");
            stmtFour.Events += listeners[3].Update;
    
            SendSupportBean(epService, "E5", -999);
            AssertReceivedSingle(listeners, 2, "E5_3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E6", 9999);
            AssertReceivedSingle(listeners, 3, "E6_4");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E7", 20);
            AssertReceivedSingle(listeners, 1, "E7_2");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E8", 10);
            AssertReceivedSingle(listeners, 0, "E8_1");
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertReceivedEach(SupportUpdateListener[] listeners, string[] stringValue) {
            for (var i = 0; i < stringValue.Length; i++) {
                if (stringValue[i] != null) {
                    Assert.AreEqual(stringValue[i], listeners[i].AssertOneGetNewAndReset().Get("TheString"));
                } else {
                    Assert.IsFalse(listeners[i].IsInvoked);
                }
            }
        }
    
        private void AssertReceivedSingle(SupportUpdateListener[] listeners, int index, string stringValue) {
            for (var i = 0; i < listeners.Length; i++) {
                if (i == index) {
                    continue;
                }
                Assert.IsFalse(listeners[i].IsInvoked);
            }
            Assert.AreEqual(stringValue, listeners[index].AssertOneGetNewAndReset().Get("TheString"));
        }
    
        private void AssertReceivedNone(SupportUpdateListener[] listeners) {
            for (var i = 0; i < listeners.Length; i++) {
                Assert.IsFalse(listeners[i].IsInvoked);
            }
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void TryAssertion(EPServiceProvider epService, EPStatement stmtOrig) {
            var listener = new SupportUpdateListener();
            stmtOrig.Events += listener.Update;
    
            var listeners = GetListeners();
            var stmtOne = epService.EPAdministrator.CreateEPL("select * from AStream2SP");
            stmtOne.Events += listeners[0].Update;
            var stmtTwo = epService.EPAdministrator.CreateEPL("select * from BStream2SP");
            stmtTwo.Events += listeners[1].Update;

            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);
    
            SendSupportBean(epService, "E1", 1);
            AssertReceivedSingle(listeners, 0, "E1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E2", 2);
            AssertReceivedSingle(listeners, 1, "E2");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E3", 1);
            AssertReceivedSingle(listeners, 0, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "E4", -999);
            AssertReceivedNone(listeners);
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            stmtOrig.Dispose();
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void TryAssertionSplitPremptiveNamedWindow(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeTwo(col2 int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeTrigger(trigger int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window WinTwo#keepall as TypeTwo");
    
            var stmtOrigText = "on TypeTrigger " +
                    "insert into OtherStream select 1 " +
                    "insert into WinTwo(col2) select 2 " +
                    "output all";
            epService.EPAdministrator.CreateEPL(stmtOrigText);
    
            var stmt = epService.EPAdministrator.CreateEPL("on OtherStream select col2 from WinTwo");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // populate WinOne
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            // fire trigger
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new object[]{null});
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Dictionary<string, object>());
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(SchemaBuilder.Record("name", OptionalInt("trigger")));
                epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(@event);
            } else {
                Assert.Fail();
            }
    
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("col2"));
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "TypeTwo,TypeTrigger,WinTwo,OtherStream".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void TryAssertionFromClauseBeginBodyEnd(EPServiceProvider epService) {
            TryAssertionFromClauseBeginBodyEnd(epService, false);
            TryAssertionFromClauseBeginBodyEnd(epService, true);
        }
    
        private void TryAssertionFromClauseAsMultiple(EPServiceProvider epService) {
            TryAssertionFromClauseAsMultiple(epService, false);
            TryAssertionFromClauseAsMultiple(epService, true);
        }
    
        private void TryAssertionFromClauseAsMultiple(EPServiceProvider epService, bool soda) {
            var epl = "on OrderEvent as oe " +
                    "insert into StartEvent select oe.orderdetail.OrderId as oi " +
                    "insert into ThenEvent select * from [select oe.orderdetail.OrderId as oi, ItemId from orderdetail.Items] as item " +
                    "insert into MoreEvent select oe.orderdetail.OrderId as oi, item.ItemId as ItemId from [select oe, * from orderdetail.Items] as item " +
                    "output all";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
    
            var listeners = GetListeners();
            epService.EPAdministrator.CreateEPL("select * from StartEvent").Events += listeners[0].Update;
            epService.EPAdministrator.CreateEPL("select * from ThenEvent").Events += listeners[1].Update;
            epService.EPAdministrator.CreateEPL("select * from MoreEvent").Events += listeners[2].Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            var fieldsOrderId = "oi".Split(',');
            var fieldsItems = "oi,ItemId".Split(',');
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fieldsOrderId, new object[]{"PO200901"});
            var expected = new[] {new object[] {"PO200901", "A001"}, new object[] {"PO200901", "A002"}, new object[] {"PO200901", "A003"}};
            EPAssertionUtil.AssertPropsPerRow(listeners[1].GetAndResetDataListsFlattened().First, fieldsItems, expected);
            EPAssertionUtil.AssertPropsPerRow(listeners[2].GetAndResetDataListsFlattened().First, fieldsItems, expected);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionFromClauseBeginBodyEnd(EPServiceProvider epService, bool soda) {
            var epl = "on OrderEvent " +
                    "insert into BeginEvent select orderdetail.OrderId as OrderId " +
                    "insert into OrderItem select * from [select orderdetail.OrderId as OrderId, * from orderdetail.Items] " +
                    "insert into EndEvent select orderdetail.OrderId as OrderId " +
                    "output all";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
    
            var listeners = GetListeners();
            epService.EPAdministrator.CreateEPL("select * from BeginEvent").Events += listeners[0].Update;
            epService.EPAdministrator.CreateEPL("select * from OrderItem").Events += listeners[1].Update;
            epService.EPAdministrator.CreateEPL("select * from EndEvent").Events += listeners[2].Update;
    
            var typeOrderItem = epService.EPAdministrator.Configuration.GetEventType("OrderItem");
            Assert.AreEqual("[Amount, ItemId, OrderId, Price, ProductId]", CompatExtensions.Render(Enumerable.OrderBy(typeOrderItem.PropertyNames, propName => propName)));
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            AssertFromClauseWContained(listeners, "PO200901", new[] {new object[] {"PO200901", "A001"}, new object[] {"PO200901", "A002"}, new object[] {"PO200901", "A003"}});
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventTwo());
            AssertFromClauseWContained(listeners, "PO200902", new[] {new object[] {"PO200902", "B001"}});
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            AssertFromClauseWContained(listeners, "PO200904", new object[0][]);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionFromClauseOutputFirstWhere(EPServiceProvider epService) {
            TryAssertionFromClauseOutputFirstWhere(epService, false);
            TryAssertionFromClauseOutputFirstWhere(epService, true);
        }
    
        private void TryAssertionFromClauseOutputFirstWhere(EPServiceProvider epService, bool soda) {
            var fieldsOrderId = "oe.orderdetail.OrderId".Split(',');
            var epl = "on OrderEvent as oe " +
                    "insert into HeaderEvent select orderdetail.OrderId as OrderId where 1=2 " +
                    "insert into StreamOne select * from [select oe, * from orderdetail.Items] where productId=\"10020\" " +
                    "insert into StreamTwo select * from [select oe, * from orderdetail.Items] where productId=\"10022\" " +
                    "insert into StreamThree select * from [select oe, * from orderdetail.Items] where productId in (\"10020\",\"10025\",\"10022\")";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
    
            var listeners = GetListeners();
            var listenerEPL = new[]{"select * from StreamOne", "select * from StreamTwo", "select * from StreamThree"};
            for (var i = 0; i < listenerEPL.Length; i++) {
                epService.EPAdministrator.CreateEPL(listenerEPL[i]).Events += listeners[i].Update;
                listeners[i].Reset();
            }
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fieldsOrderId, new object[]{"PO200901"});
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventTwo());
            Assert.IsFalse(listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fieldsOrderId, new object[]{"PO200902"});
            Assert.IsFalse(listeners[2].IsInvoked);
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventThree());
            Assert.IsFalse(listeners[0].IsInvoked);
            Assert.IsFalse(listeners[1].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fieldsOrderId, new object[]{"PO200903"});
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            Assert.IsFalse(listeners[0].IsInvoked);
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionFromClauseDocSample(EPServiceProvider epService) {
            var epl =
                    "create schema MyOrderItem(ItemId string);\n" +
                            "create schema MyOrderEvent(OrderId string, Items MyOrderItem[]);\n" +
                            "on MyOrderEvent\n" +
                            "  insert into MyOrderBeginEvent select OrderId\n" +
                            "  insert into MyOrderItemEvent select * from [select OrderId, * from Items]\n" +
                            "  insert into MyOrderEndEvent select OrderId\n" +
                            "  output all;\n" +
                            "create context MyOrderContext \n" +
                            "  initiated by MyOrderBeginEvent as obe\n" +
                            "  terminated by MyOrderEndEvent(OrderId = obe.OrderId);\n" +
                            "@Name('count') context MyOrderContext select count(*) as orderItemCount from MyOrderItemEvent output when terminated;\n";
            var result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("count").Events += listener.Update;
    
            IDictionary<string, object> @event = new Dictionary<string, object>();
            @event.Put("OrderId", "1010");
            @event.Put("Items", new[] {Collections.SingletonDataMap("ItemId", "A0001")});
            epService.EPRuntime.SendEvent(@event, "MyOrderEvent");
    
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("orderItemCount"));
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(result.DeploymentId);
        }
    
        private void AssertFromClauseWContained(SupportUpdateListener[] listeners, string orderId, object[][] expected) {
            var fieldsOrderId = "OrderId".Split(',');
            var fieldsItems = "OrderId,ItemId".Split(',');
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fieldsOrderId, new object[]{orderId});
            EPAssertionUtil.AssertPropsPerRow(listeners[1].GetAndResetDataListsFlattened().First, fieldsItems, expected);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fieldsOrderId, new object[]{orderId});
        }
    
        private SupportUpdateListener[] GetListeners() {
            var listeners = new SupportUpdateListener[10];
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i] = new SupportUpdateListener();
            }
            return listeners;
        }
    }
} // end of namespace
