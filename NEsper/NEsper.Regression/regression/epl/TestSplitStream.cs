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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.bookexample;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using Avro.Generic;

namespace com.espertech.esper.regression.epl
{
    using DataMap = IDictionary<string, object>;
    using DataMapImpl = Dictionary<string, object>;

    [TestFixture]
    public class TestSplitStream
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener[] _listeners;

        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listeners = new SupportUpdateListener[10];
            for (var i = 0; i < _listeners.Length; i++)
            {
                _listeners[i] = new SupportUpdateListener();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _listeners = null;
        }

        [Test]
        public void TestInvalid()
        {
            SupportMessageAssertUtil.TryInvalid(
                _epService,
                "on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2",
                "Error starting statement: Required insert-into clause is not provided, the clause is required for split-stream syntax [on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2]");

            SupportMessageAssertUtil.TryInvalid(
                _epService,
                "on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2",
                "Error starting statement: A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax [on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2]");

            SupportMessageAssertUtil.TryInvalid(
                _epService,
                "on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select Avg(IntPrimitive) where 1=2",
                "Error starting statement: Aggregation functions are not allowed in this context [on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select Avg(IntPrimitive) where 1=2]");
        }

        [Test]
        public void TestFromClause()
        {
            _epService.EPAdministrator.Configuration.AddEventType<OrderBean>("OrderEvent");

            RunAssertionFromClauseBeginBodyEnd();
            RunAssertionFromClauseAsMultiple();
            RunAssertionFromClauseOutputFirstWhere();
            RunAssertionFromClauseDocSample();
        }

        [Test]
        public void TestSplitPremptiveNamedWindow()
        {
            EnumHelper.ForEach<EventRepresentationChoice>(
                rep => RunAssertionSplitPremptiveNamedWindow(rep));
        }

        [Test]
        public void Test1SplitDefault()
        {
            // test wildcard
            var stmtOrigText = "on SupportBean insert into AStream select *";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmt.Events += _listener.Update;

            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;

            SendSupportBean("E1", 1);
            AssertReceivedSingle(0, "E1");
            Assert.IsFalse(_listener.IsInvoked);

            // test select
            stmtOrigText = "on SupportBean insert into BStream select 3*IntPrimitive as value";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);

            stmtOne = _epService.EPAdministrator.CreateEPL("select value from BStream");
            stmtOne.Events += _listeners[1].Update;

            SendSupportBean("E1", 6);
            Assert.AreEqual(18, _listeners[1].AssertOneGetNewAndReset().Get("value"));

            // assert type is original type
            Assert.AreEqual(typeof(SupportBean), stmtOrig.EventType.UnderlyingType);
            Assert.IsFalse(stmtOrig.HasFirst());
        }

        [Test]
        public void Test2SplitNoDefaultOutputFirst()
        {
            var stmtOrigText = "@Audit on SupportBean " +
                        "insert into AStream select * where IntPrimitive=1 " +
                        "insert into BStream select * where IntPrimitive=1 or IntPrimitive=2";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            RunAssertion(stmtOrig);

            // statement object model
            var model = new EPStatementObjectModel();
            model.Annotations = Collections.SingletonList(new AnnotationPart("Audit"));
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean"));
            model.InsertInto = InsertIntoClause.Create("AStream");
            model.SelectClause = SelectClause.CreateWildcard();
            model.WhereClause = Expressions.Eq("IntPrimitive", 1);
            var clause = OnClause.CreateOnInsertSplitStream();
            model.OnExpr = clause;
            var item = OnInsertSplitStreamItem.Create(
                    InsertIntoClause.Create("BStream"),
                    SelectClause.CreateWildcard(),
                    Expressions.Or(Expressions.Eq("IntPrimitive", 1), Expressions.Eq("IntPrimitive", 2)));
            clause.AddItem(item);
            Assert.AreEqual(stmtOrigText, model.ToEPL());
            stmtOrig = _epService.EPAdministrator.Create(model);
            RunAssertion(stmtOrig);

            var newModel = _epService.EPAdministrator.CompileEPL(stmtOrigText);
            stmtOrig = _epService.EPAdministrator.Create(newModel);
            Assert.AreEqual(stmtOrigText, newModel.ToEPL());
            RunAssertion(stmtOrig);

            SupportModelHelper.CompileCreate(_epService, stmtOrigText + " output all");
        }

        [Test]
        public void TestSubquery()
        {
            var stmtOrigText = "on SupportBean " +
                                  "insert into AStream select (select p00 from S0#lastevent) as string where IntPrimitive=(select id from S0#lastevent) " +
                                  "insert into BStream select (select p01 from S0#lastevent) as string where IntPrimitive<>(select id from S0#lastevent) or (select id from S0#lastevent) is null";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;

            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;

            SendSupportBean("E1", 1);
            Assert.IsFalse(_listeners[0].GetAndClearIsInvoked());
            Assert.IsNull(_listeners[1].AssertOneGetNewAndReset().Get("string"));

            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "x", "y"));

            SendSupportBean("E2", 10);
            Assert.AreEqual("x", _listeners[0].AssertOneGetNewAndReset().Get("string"));
            Assert.IsFalse(_listeners[1].GetAndClearIsInvoked());

            SendSupportBean("E3", 9);
            Assert.IsFalse(_listeners[0].GetAndClearIsInvoked());
            Assert.AreEqual("y", _listeners[1].AssertOneGetNewAndReset().Get("string"));
        }

        [Test]
        public void Test2SplitNoDefaultOutputAll()
        {
            var stmtOrigText = "on SupportBean " +
                                  "insert into AStream select TheString where IntPrimitive=1 " +
                                  "insert into BStream select TheString where IntPrimitive=1 or IntPrimitive=2 " +
                                  "output all";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;

            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;

            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);

            SendSupportBean("E1", 1);
            AssertReceivedEach(new String[] { "E1", "E1" });
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E2", 2);
            AssertReceivedEach(new String[] { null, "E2" });
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E3", 1);
            AssertReceivedEach(new String[] { "E3", "E3" });
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E4", -999);
            AssertReceivedEach(new String[] { null, null });
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("TheString"));

            stmtOrig.Dispose();
            stmtOrigText = "on SupportBean " +
                                  "insert into AStream select TheString || '_1' as TheString where IntPrimitive in (1, 2) " +
                                  "insert into BStream select TheString || '_2' as TheString where IntPrimitive in (2, 3) " +
                                  "insert into CStream select TheString || '_3' as TheString " +
                                  "output all";
            stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;

            var stmtThree = _epService.EPAdministrator.CreateEPL("select * from CStream");
            stmtThree.Events += _listeners[2].Update;

            SendSupportBean("E1", 2);
            AssertReceivedEach(new String[] { "E1_1", "E1_2", "E1_3" });
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E2", 1);
            AssertReceivedEach(new String[] { "E2_1", null, "E2_3" });
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E3", 3);
            AssertReceivedEach(new String[] { null, "E3_2", "E3_3" });
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E4", -999);
            AssertReceivedEach(new String[] { null, null, "E4_3" });
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void Test3And4SplitDefaultOutputFirst()
        {
            var stmtOrigText = "on SupportBean as mystream " +
                                  "insert into AStream select mystream.TheString||'_1' as TheString where IntPrimitive=1 " +
                                  "insert into BStream select mystream.TheString||'_2' as TheString where IntPrimitive=2 " +
                                  "insert into CStream select TheString||'_3' as TheString";
            var stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;

            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;
            var stmtThree = _epService.EPAdministrator.CreateEPL("select * from CStream");
            stmtThree.Events += _listeners[2].Update;

            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);

            SendSupportBean("E1", 1);
            AssertReceivedSingle(0, "E1_1");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E2", 2);
            AssertReceivedSingle(1, "E2_2");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E3", 1);
            AssertReceivedSingle(0, "E3_1");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E4", -999);
            AssertReceivedSingle(2, "E4_3");
            Assert.IsFalse(_listener.IsInvoked);

            stmtOrigText = "on SupportBean " +
                                  "insert into AStream select TheString||'_1' as TheString where IntPrimitive=10 " +
                                  "insert into BStream select TheString||'_2' as TheString where IntPrimitive=20 " +
                                  "insert into CStream select TheString||'_3' as TheString where IntPrimitive<0 " +
                                  "insert into DStream select TheString||'_4' as TheString";
            stmtOrig.Dispose();
            stmtOrig = _epService.EPAdministrator.CreateEPL(stmtOrigText);
            stmtOrig.Events += _listener.Update;

            var stmtFour = _epService.EPAdministrator.CreateEPL("select * from DStream");
            stmtFour.Events += _listeners[3].Update;

            SendSupportBean("E5", -999);
            AssertReceivedSingle(2, "E5_3");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E6", 9999);
            AssertReceivedSingle(3, "E6_4");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E7", 20);
            AssertReceivedSingle(1, "E7_2");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E8", 10);
            AssertReceivedSingle(0, "E8_1");
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void AssertReceivedEach(String[] stringValue)
        {
            for (var i = 0; i < stringValue.Length; i++)
            {
                if (stringValue[i] != null)
                {
                    Assert.AreEqual(stringValue[i], _listeners[i].AssertOneGetNewAndReset().Get("TheString"));
                }
                else
                {
                    Assert.IsFalse(_listeners[i].IsInvoked);
                }
            }
        }

        private void AssertReceivedSingle(int index, String stringValue)
        {
            for (var i = 0; i < _listeners.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                Assert.IsFalse(_listeners[i].IsInvoked);
            }
            Assert.AreEqual(stringValue, _listeners[index].AssertOneGetNewAndReset().Get("TheString"));
        }

        private void AssertReceivedNone()
        {
            for (var i = 0; i < _listeners.Length; i++)
            {
                Assert.IsFalse(_listeners[i].IsInvoked);
            }
        }

        private SupportBean SendSupportBean(String theString, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private void RunAssertion(EPStatement stmtOrig)
        {
            stmtOrig.Events += _listener.Update;

            var stmtOne = _epService.EPAdministrator.CreateEPL("select * from AStream");
            stmtOne.Events += _listeners[0].Update;
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select * from BStream");
            stmtTwo.Events += _listeners[1].Update;

            Assert.AreNotSame(stmtOne.EventType, stmtTwo.EventType);
            Assert.AreSame(stmtOne.EventType.UnderlyingType, stmtTwo.EventType.UnderlyingType);

            SendSupportBean("E1", 1);
            AssertReceivedSingle(0, "E1");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E2", 2);
            AssertReceivedSingle(1, "E2");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E3", 1);
            AssertReceivedSingle(0, "E3");
            Assert.IsFalse(_listener.IsInvoked);

            SendSupportBean("E4", -999);
            AssertReceivedNone();
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("TheString"));

            stmtOrig.Dispose();
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }

        public void RunAssertionSplitPremptiveNamedWindow(EventRepresentationChoice eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeTwo(col2 int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeTrigger(trigger int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window WinTwo#keepall as TypeTwo");

            var stmtOrigText = "on TypeTrigger " +
                        "insert into OtherStream select 1 " +
                        "insert into WinTwo(col2) select 2 " +
                        "output all";
            _epService.EPAdministrator.CreateEPL(stmtOrigText);

            var stmt = _epService.EPAdministrator.CreateEPL("on OtherStream select col2 from WinTwo");
            stmt.Events += _listener.Update;

            // populate WinOne
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));

            // fire trigger
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Object[] { null });
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Dictionary<string, object>());
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var @event = new GenericRecord(SchemaBuilder.Record("name", TypeBuilder.OptionalInt("trigger")));
                _epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(@event);
            }
            else
            {
                Assert.Fail();
            }

            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("col2"));
            _epService.Initialize();
        }

        private void RunAssertionFromClauseBeginBodyEnd()
        {
            RunAssertionFromClauseBeginBodyEnd(false);
            RunAssertionFromClauseBeginBodyEnd(true);
        }

        private void RunAssertionFromClauseAsMultiple()
        {
            RunAssertionFromClauseAsMultiple(false);
            RunAssertionFromClauseAsMultiple(true);
        }

        private void RunAssertionFromClauseAsMultiple(bool soda)
        {
            String epl = "on OrderEvent as oe " +
                         "insert into StartEvent select oe.orderdetail.OrderId as oi " +
                         "insert into ThenEvent select * from [select oe.orderdetail.OrderId as oi, ItemId from orderdetail.Items] as Item " +
                         "insert into MoreEvent select oe.orderdetail.OrderId as oi, Item.ItemId as ItemId from [select oe, * from orderdetail.Items] as Item " +
                         "output all";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);

            _epService.EPAdministrator.CreateEPL("select * from StartEvent").AddListener(_listeners[0]);
            _epService.EPAdministrator.CreateEPL("select * from ThenEvent").AddListener(_listeners[1]);
            _epService.EPAdministrator.CreateEPL("select * from MoreEvent").AddListener(_listeners[2]);

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            String[] fieldsOrderId = "oi".SplitCsv();
            String[] fieldsItems = "oi,ItemId".SplitCsv();
            EPAssertionUtil.AssertProps(_listeners[0].AssertOneGetNewAndReset(), fieldsOrderId, new Object[] { "PO200901" });
            Object[][] expected = new Object[][] { new Object[] { "PO200901", "A001" }, new Object[] { "PO200901", "A002" }, new Object[] { "PO200901", "A003" } };
            EPAssertionUtil.AssertPropsPerRow(_listeners[1].GetAndResetDataListsFlattened().First, fieldsItems, expected);
            EPAssertionUtil.AssertPropsPerRow(_listeners[2].GetAndResetDataListsFlattened().First, fieldsItems, expected);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFromClauseBeginBodyEnd(bool soda)
        {
            String epl = "on OrderEvent " +
                         "insert into BeginEvent select orderdetail.OrderId as OrderId " +
                         "insert into OrderItem select * from [select orderdetail.OrderId as OrderId, * from orderdetail.Items] " +
                         "insert into EndEvent select orderdetail.OrderId as OrderId " +
                         "output all";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);

            _epService.EPAdministrator.CreateEPL("select * from BeginEvent").AddListener(_listeners[0]);
            _epService.EPAdministrator.CreateEPL("select * from OrderItem").AddListener(_listeners[1]);
            _epService.EPAdministrator.CreateEPL("select * from EndEvent").AddListener(_listeners[2]);

            EventType typeOrderItem = _epService.EPAdministrator.Configuration.GetEventType("OrderItem");
            Assert.AreEqual("[Amount, ItemId, OrderId, Price, ProductId]", CompatExtensions.Render(Enumerable.OrderBy(typeOrderItem.PropertyNames, propName => propName)));

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            AssertFromClauseWContained("PO200901", new Object[][] { new Object[] { "PO200901", "A001" }, new Object[] { "PO200901", "A002" }, new Object[] { "PO200901", "A003" } });

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventTwo());
            AssertFromClauseWContained("PO200902", new Object[][] { new Object[] { "PO200902", "B001" } });

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            AssertFromClauseWContained("PO200904", new Object[0][]);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFromClauseOutputFirstWhere()
        {
            RunAssertionFromClauseOutputFirstWhere(false);
            RunAssertionFromClauseOutputFirstWhere(true);
        }

        private void RunAssertionFromClauseOutputFirstWhere(bool soda)
        {
            String[] fieldsOrderId = "oe.orderdetail.OrderId".SplitCsv();
            String epl = "on OrderEvent as oe " +
                         "insert into HeaderEvent select orderdetail.OrderId as OrderId where 1=2 " +
                         "insert into StreamOne select * from [select oe, * from orderdetail.Items] where productId=\"10020\" " +
                         "insert into StreamTwo select * from [select oe, * from orderdetail.Items] where productId=\"10022\" " +
                         "insert into StreamThree select * from [select oe, * from orderdetail.Items] where productId in (\"10020\",\"10025\",\"10022\")";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);

            String[] listenerEPL = new String[] { "select * from StreamOne", "select * from StreamTwo", "select * from StreamThree" };
            for (int i = 0; i < listenerEPL.Length; i++)
            {
                _epService.EPAdministrator.CreateEPL(listenerEPL[i]).AddListener(_listeners[i]);
                _listeners[i].Reset();
            }

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertProps(_listeners[0].AssertOneGetNewAndReset(), fieldsOrderId, new Object[] { "PO200901" });
            Assert.IsFalse(_listeners[1].IsInvoked);
            Assert.IsFalse(_listeners[2].IsInvoked);

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventTwo());
            Assert.IsFalse(_listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(_listeners[1].AssertOneGetNewAndReset(), fieldsOrderId, new Object[] { "PO200902" });
            Assert.IsFalse(_listeners[2].IsInvoked);

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventThree());
            Assert.IsFalse(_listeners[0].IsInvoked);
            Assert.IsFalse(_listeners[1].IsInvoked);
            EPAssertionUtil.AssertProps(_listeners[2].AssertOneGetNewAndReset(), fieldsOrderId, new Object[] { "PO200903" });

            _epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventFour());
            Assert.IsFalse(_listeners[0].IsInvoked);
            Assert.IsFalse(_listeners[1].IsInvoked);
            Assert.IsFalse(_listeners[2].IsInvoked);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFromClauseDocSample()
        {
            String epl =
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
                "@Name('count') context MyOrderContext select count(*) as OrderItemCount from MyOrderItemEvent output when terminated;\n";

            var result = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            _listener.Reset();
            _epService.EPAdministrator.GetStatement("count").AddListener(_listener);

            var @event = new Dictionary<string, object>();
            @event.Put("OrderId", "1010");
            @event.Put("Items", new IDictionary<string,object>[] {Collections.SingletonDataMap("ItemId", "A0001")});
            _epService.EPRuntime.SendEvent(@event, "MyOrderEvent");

            Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("OrderItemCount"));

            _epService.EPAdministrator.DeploymentAdmin.Undeploy(result.DeploymentId);
        }

        private void AssertFromClauseWContained(String orderId, Object[][] expected)
        {
            String[] fieldsOrderId = "OrderId".SplitCsv();
            String[] fieldsItems = "OrderId,ItemId".SplitCsv();
            EPAssertionUtil.AssertProps(_listeners[0].AssertOneGetNewAndReset(), fieldsOrderId, new Object[] { orderId });
            EPAssertionUtil.AssertPropsPerRow(_listeners[1].GetAndResetDataListsFlattened().First, fieldsItems, expected);
            EPAssertionUtil.AssertProps(_listeners[2].AssertOneGetNewAndReset(), fieldsOrderId, new Object[] { orderId });
        }
    }
}
