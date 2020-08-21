///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSplitStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherSplitStream2SplitNoDefaultOutputFirst());
            execs.Add(new EPLOtherSplitStreamInvalid());
            execs.Add(new EPLOtherSplitStreamFromClause());
            execs.Add(new EPLOtherSplitStreamSplitPremptiveNamedWindow());
            execs.Add(new EPLOtherSplitStream1SplitDefault());
            execs.Add(new EPLOtherSplitStreamSubquery());
            execs.Add(new EPLOtherSplitStream2SplitNoDefaultOutputAll());
            execs.Add(new EPLOtherSplitStream3SplitOutputAll());
            execs.Add(new EPLOtherSplitStream3SplitDefaultOutputFirst());
            execs.Add(new EPLOtherSplitStream4Split());
            execs.Add(new EPLOtherSplitStreamSubqueryMultikeyWArray());
            return execs;
        }

        private static void AssertReceivedEach(
            SupportListener[] listeners,
            string[] stringValue)
        {
            for (var i = 0; i < stringValue.Length; i++) {
                if (stringValue[i] != null) {
                    Assert.AreEqual(stringValue[i], listeners[i].AssertOneGetNewAndReset().Get("TheString"));
                }
                else {
                    Assert.IsFalse(listeners[i].IsInvoked);
                }
            }
        }

        private static void AssertReceivedSingle(
            SupportListener[] listeners,
            int index,
            string stringValue)
        {
            for (var i = 0; i < listeners.Length; i++) {
                if (i == index) {
                    continue;
                }

                Assert.IsFalse(listeners[i].IsInvoked);
            }

            Assert.AreEqual(stringValue, listeners[index].AssertOneGetNewAndReset().Get("TheString"));
        }

        private static void AssertReceivedNone(SupportListener[] listeners)
        {
            for (var i = 0; i < listeners.Length; i++) {
                Assert.IsFalse(listeners[i].IsInvoked);
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            RegressionPath path)
        {
            var listeners = GetListeners(env);
            env.CompileDeploy("@Name('s0') select * from AStream2SP", path).AddListener("s0", listeners[0]);
            env.CompileDeploy("@Name('s1') select * from BStream2SP", path).AddListener("s1", listeners[1]);

            Assert.AreNotSame(env.Statement("s0").EventType, env.Statement("s1").EventType);
            Assert.AreSame(env.Statement("s0").EventType.UnderlyingType, env.Statement("s1").EventType.UnderlyingType);

            SendSupportBean(env, "E1", 1);
            AssertReceivedSingle(listeners, 0, "E1");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendSupportBean(env, "E2", 2);
            AssertReceivedSingle(listeners, 1, "E2");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendSupportBean(env, "E3", 1);
            AssertReceivedSingle(listeners, 0, "E3");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendSupportBean(env, "E4", -999);
            AssertReceivedNone(listeners);
            Assert.AreEqual("E4", env.Listener("split").AssertOneGetNewAndReset().Get("TheString"));

            env.UndeployAll();
        }

        private static void TryAssertionSplitPremptiveNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType(eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTrigger>() + " create schema TypeTrigger(trigger int)", path);
            env.CompileDeploy(eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTypeTwo>() + " create schema TypeTwo(col2 int)", path);
            env.CompileDeploy(eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedTypeTwo>() + " create window WinTwo#keepall as TypeTwo", path);

            var stmtOrigText = "on TypeTrigger " +
                               "insert into OtherStream select 1 " +
                               "insert into WinTwo(col2) select 2 " +
                               "output all";
            env.CompileDeploy(stmtOrigText, path);

            env.CompileDeploy("@Name('s0') on OtherStream select col2 from WinTwo", path).AddListener("s0");

            // populate WinOne
            env.SendEventBean(new SupportBean("E1", 2));

            // fire trigger
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {null}, "TypeTrigger");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(new Dictionary<string, object>(), "TypeTrigger");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(
                    SchemaBuilder.Record("name", TypeBuilder.OptionalInt("trigger")));
                env.SendEventAvro(@event, "TypeTrigger");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                env.SendEventJson("{}", "TypeTrigger");
            }
            else {
                Assert.Fail();
            }

            Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("col2"));

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseBeginBodyEnd(RegressionEnvironment env)
        {
            TryAssertionFromClauseBeginBodyEnd(env, false);
            TryAssertionFromClauseBeginBodyEnd(env, true);
        }

        private static void TryAssertionFromClauseAsMultiple(RegressionEnvironment env)
        {
            TryAssertionFromClauseAsMultiple(env, false);
            TryAssertionFromClauseAsMultiple(env, true);
        }

        private static void TryAssertionFromClauseAsMultiple(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var epl = "on OrderBean as oe " +
                      "insert into StartEvent select oe.OrderDetail.OrderId as oi " +
                      "insert into ThenEvent select * from [select oe.OrderDetail.OrderId as oi, ItemId from OrderDetail.Items] as item " +
                      "insert into MoreEvent select oe.OrderDetail.OrderId as oi, item.ItemId as ItemId from [select oe, * from OrderDetail.Items] as item " +
                      "output all";
            env.CompileDeploy(soda, epl, path);

            var listeners = GetListeners(env);
            env.CompileDeploy("@Name('s0') select * from StartEvent", path).AddListener("s0", listeners[0]);
            env.CompileDeploy("@Name('s1') select * from ThenEvent", path).AddListener("s1", listeners[1]);
            env.CompileDeploy("@Name('s2') select * from MoreEvent", path).AddListener("s2", listeners[2]);

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            var fieldsOrderId = new [] { "oi" };
            var fieldsItems = new [] { "oi","ItemId" };
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fieldsOrderId,
                new object[] {"PO200901"});
            object[][] expected = {
                new object[] {"PO200901", "A001"}, new object[] {"PO200901", "A002"}, new object[] {"PO200901", "A003"}
            };
            EPAssertionUtil.AssertPropsPerRow(
                listeners[1].GetAndResetDataListsFlattened().First,
                fieldsItems,
                expected);
            EPAssertionUtil.AssertPropsPerRow(
                listeners[2].GetAndResetDataListsFlattened().First,
                fieldsItems,
                expected);

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseBeginBodyEnd(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var epl = "@Name('split') on OrderBean " +
                      "insert into BeginEvent select OrderDetail.OrderId as OrderId " +
                      "insert into OrderItem select * from [select OrderDetail.OrderId as OrderId, * from OrderDetail.Items] " +
                      "insert into EndEvent select OrderDetail.OrderId as OrderId " +
                      "output all";
            env.CompileDeploy(soda, epl, path);
            Assert.AreEqual(
                StatementType.ON_SPLITSTREAM,
                env.Statement("split").GetProperty(StatementProperty.STATEMENTTYPE));

            var listeners = GetListeners(env);
            env.CompileDeploy("@Name('s0') select * from BeginEvent", path).AddListener("s0", listeners[0]);
            env.CompileDeploy("@Name('s1') select * from OrderItem", path).AddListener("s1", listeners[1]);
            env.CompileDeploy("@Name('s2') select * from EndEvent", path).AddListener("s2", listeners[2]);

            var orderItemType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("split"), "OrderItem");
            Assert.AreEqual("[\"Amount\", \"ItemId\", \"Price\", \"ProductId\", \"OrderId\"]", orderItemType.PropertyNames.RenderAny());

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            AssertFromClauseWContained(
                listeners,
                "PO200901",
                new[] {
                    new object[] {"PO200901", "A001"}, new object[] {"PO200901", "A002"},
                    new object[] {"PO200901", "A003"}
                });

            env.SendEventBean(OrderBeanFactory.MakeEventTwo());
            AssertFromClauseWContained(
                listeners,
                "PO200902",
                new[] {new object[] {"PO200902", "B001"}});

            env.SendEventBean(OrderBeanFactory.MakeEventFour());
            AssertFromClauseWContained(listeners, "PO200904", new object[0][]);

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseOutputFirstWhere(RegressionEnvironment env)
        {
            TryAssertionFromClauseOutputFirstWhere(env, false);
            TryAssertionFromClauseOutputFirstWhere(env, true);
        }

        private static void TryAssertionFromClauseOutputFirstWhere(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            var fieldsOrderId = new [] { "oe.OrderDetail.OrderId" };
            var epl = "on OrderBean as oe " +
                      "insert into HeaderEvent select OrderDetail.OrderId as OrderId where 1=2 " +
                      "insert into StreamOne select * from [select oe, * from OrderDetail.Items] where ProductId=\"10020\" " +
                      "insert into StreamTwo select * from [select oe, * from OrderDetail.Items] where ProductId=\"10022\" " +
                      "insert into StreamThree select * from [select oe, * from OrderDetail.Items] where ProductId in (\"10020\",\"10025\",\"10022\")";
            env.CompileDeploy(soda, epl, path);

            var listeners = GetListeners(env);
            string[] listenerEPL = {"select * from StreamOne", "select * from StreamTwo", "select * from StreamThree"};
            for (var i = 0; i < listenerEPL.Length; i++) {
                env.CompileDeploy("@Name('s" + i + "')" + listenerEPL[i], path).AddListener("s" + i, listeners[i]);
                listeners[i].Reset();
            }

            env.SendEventBean(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fieldsOrderId,
                new object[] {"PO200901"});
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);

            env.SendEventBean(OrderBeanFactory.MakeEventTwo());
            Assert.IsFalse(listeners[0].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[1].AssertOneGetNewAndReset(),
                fieldsOrderId,
                new object[] {"PO200902"});
            Assert.IsFalse(listeners[2].IsInvoked);

            env.SendEventBean(OrderBeanFactory.MakeEventThree());
            Assert.IsFalse(listeners[0].IsInvoked);
            Assert.IsFalse(listeners[1].IsInvoked);
            EPAssertionUtil.AssertProps(
                listeners[2].AssertOneGetNewAndReset(),
                fieldsOrderId,
                new object[] {"PO200903"});

            env.SendEventBean(OrderBeanFactory.MakeEventFour());
            Assert.IsFalse(listeners[0].IsInvoked);
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);

            env.UndeployAll();
        }

        private static void TryAssertionFromClauseDocSample(RegressionEnvironment env)
        {
            var epl =
                "create schema MyOrderItem(itemId string);\n" +
                "create schema MyOrderEvent(OrderId string, items MyOrderItem[]);\n" +
                "on MyOrderEvent\n" +
                "  insert into MyOrderBeginEvent select OrderId\n" +
                "  insert into MyOrderItemEvent select * from [select OrderId, * from items]\n" +
                "  insert into MyOrderEndEvent select OrderId\n" +
                "  output all;\n" +
                "create context MyOrderContext \n" +
                "  initiated by MyOrderBeginEvent as obe\n" +
                "  terminated by MyOrderEndEvent(OrderId = obe.OrderId);\n" +
                "@Name('count') context MyOrderContext select count(*) as orderItemCount from MyOrderItemEvent output when terminated;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("count");

            var @event = new Dictionary<string, object>();
            @event.Put("OrderId", "1010");
            @event.Put(
                "items",
                new[] {
                    Collections.SingletonDataMap("itemId", "A0001")
                });
            env.SendEventMap(@event, "MyOrderEvent");

            Assert.AreEqual(1L, env.Listener("count").AssertOneGetNewAndReset().Get("orderItemCount"));

            env.UndeployAll();
        }

        private static void AssertFromClauseWContained(
            SupportListener[] listeners,
            string orderId,
            object[][] expected)
        {
            var fieldsOrderId = new [] { "OrderId" };
            var fieldsItems = new [] { "OrderId","ItemId" };
            EPAssertionUtil.AssertProps(
                listeners[0].AssertOneGetNewAndReset(),
                fieldsOrderId,
                new object[] {orderId});
            EPAssertionUtil.AssertPropsPerRow(
                listeners[1].GetAndResetDataListsFlattened().First,
                fieldsItems,
                expected);
            EPAssertionUtil.AssertProps(
                listeners[2].AssertOneGetNewAndReset(),
                fieldsOrderId,
                new object[] {orderId});
        }

        private static SupportListener[] GetListeners(RegressionEnvironment env)
        {
            var listeners = new SupportListener[10];
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i] = env.ListenerNew();
            }

            return listeners;
        }

        internal class EPLOtherSplitStreamSubqueryMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create schema AValue(value int);\n" +
                             "on SupportBean\n" +
                             "  insert into AValue select (select sum(value) as c0 from SupportEventWithIntArray#keepall group by array) as value where IntPrimitive > 0\n" +
                             "  insert into AValue select 0 as value where IntPrimitive <= 0;\n" +
                             "@Name('s0') select * from AValue;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] {1, 2}, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] {1, 2}, 11));

                env.Milestone(0);
                AssertSplitResult(env, 21);

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] {1, 2}, 12));
                AssertSplitResult(env, 33);

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] {1}, 13));
                AssertSplitResult(env, null);

                env.UndeployAll();
            }

            private void AssertSplitResult(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventBean(new SupportBean("X", 0));
                Assert.AreEqual(0, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean("Y", 1));
                Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
            }
        }

        internal class EPLOtherSplitStreamInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "on SupportBean select * where IntPrimitive=1 insert into BStream select * where 1=2",
                    "Required insert-into clause is not provided, the clause is required for split-stream syntax");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "on SupportBean insert into AStream select * where IntPrimitive=1 group by string insert into BStream select * where 1=2",
                    "A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "on SupportBean insert into AStream select * where IntPrimitive=1 insert into BStream select avg(IntPrimitive) where 1=2",
                    "Aggregation functions are not allowed in this context");
            }
        }

        internal class EPLOtherSplitStreamFromClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionFromClauseBeginBodyEnd(env);
                TryAssertionFromClauseAsMultiple(env);
                TryAssertionFromClauseOutputFirstWhere(env);
                TryAssertionFromClauseDocSample(env);
            }
        }

        internal class EPLOtherSplitStreamSplitPremptiveNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionSplitPremptiveNamedWindow(env, rep);
                }
            }
        }

        internal class EPLOtherSplitStream1SplitDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // test wildcard
                var stmtOrigText = "@Name('insert') on SupportBean insert into AStream select *";
                env.CompileDeploy(stmtOrigText, path).AddListener("insert");

                var listeners = GetListeners(env);
                env.CompileDeploy("@Name('s0') select * from AStream", path).AddListener("s0", listeners[0]);

                SendSupportBean(env, "E1", 1);
                AssertReceivedSingle(listeners, 0, "E1");
                Assert.IsFalse(env.Listener("insert").IsInvoked);

                // test select
                stmtOrigText = "@Name('s1') on SupportBean insert into BStreamABC select 3*IntPrimitive as value";
                env.CompileDeploy(stmtOrigText, path);

                env.CompileDeploy("@Name('s2') select value from BStreamABC", path);
                env.AddListener("s2", listeners[1]);

                SendSupportBean(env, "E1", 6);
                Assert.AreEqual(18, listeners[1].AssertOneGetNewAndReset().Get("value"));

                // assert type is original type
                Assert.AreEqual(typeof(SupportBean), env.Statement("insert").EventType.UnderlyingType);
                Assert.IsFalse(env.GetEnumerator("insert").MoveNext());

                env.UndeployAll();
            }
        }

        internal class EPLOtherSplitStream2SplitNoDefaultOutputFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') on SupportBean " +
                                   "insert into AStream2SP select * where IntPrimitive=1 " +
                                   "insert into BStream2SP select * where IntPrimitive=1 or IntPrimitive=2";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");
                TryAssertion(env, path);
                path.Clear();

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
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("split"));
                Assert.AreEqual(stmtOrigText, model.ToEPL());
                env.CompileDeploy(model, path).AddListener("split");
                TryAssertion(env, path);
                path.Clear();

                env.EplToModelCompileDeploy(stmtOrigText, path).AddListener("split");
                TryAssertion(env, path);
            }
        }

        internal class EPLOtherSplitStreamSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') on SupportBean " +
                                   "insert into AStreamSub select (select P00 from SupportBean_S0#lastevent) as string where IntPrimitive=(select Id from SupportBean_S0#lastevent) " +
                                   "insert into BStreamSub select (select P01 from SupportBean_S0#lastevent) as string where IntPrimitive<>(select Id from SupportBean_S0#lastevent) or (select Id from SupportBean_S0#lastevent) is null";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                env.CompileDeploy("@Name('s0') select * from AStreamSub", path).AddListener("s0");
                env.CompileDeploy("@Name('s1') select * from BStreamSub", path).AddListener("s1");

                SendSupportBean(env, "E1", 1);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                Assert.IsNull(env.Listener("s1").AssertOneGetNewAndReset().Get("string"));

                env.SendEventBean(new SupportBean_S0(10, "x", "y"));

                SendSupportBean(env, "E2", 10);
                Assert.AreEqual("x", env.Listener("s0").AssertOneGetNewAndReset().Get("string"));
                Assert.IsFalse(env.Listener("s1").GetAndClearIsInvoked());

                SendSupportBean(env, "E3", 9);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual("y", env.Listener("s1").AssertOneGetNewAndReset().Get("string"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherSplitStream2SplitNoDefaultOutputAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') on SupportBean " +
                                   "insert into AStream2S select TheString where IntPrimitive=1 " +
                                   "insert into BStream2S select TheString where IntPrimitive=1 or IntPrimitive=2 " +
                                   "output all";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                var listeners = GetListeners(env);
                env.CompileDeploy("@Name('s0') select * from AStream2S", path).AddListener("s0", listeners[0]);
                env.CompileDeploy("@Name('s1') select * from BStream2S", path).AddListener("s1", listeners[1]);

                Assert.AreNotSame(env.Statement("s0").EventType, env.Statement("s1").EventType);
                Assert.AreSame(
                    env.Statement("s0").EventType.UnderlyingType,
                    env.Statement("s1").EventType.UnderlyingType);

                SendSupportBean(env, "E1", 1);
                AssertReceivedEach(listeners, new[] {"E1", "E1"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E2", 2);
                AssertReceivedEach(listeners, new[] {null, "E2"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E3", 1);
                AssertReceivedEach(listeners, new[] {"E3", "E3"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E4", -999);
                AssertReceivedEach(listeners, new string[] {null, null});
                Assert.AreEqual("E4", env.Listener("split").AssertOneGetNewAndReset().Get("TheString"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherSplitStream3SplitOutputAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') on SupportBean " +
                                   "insert into AStream2S select TheString || '_1' as TheString where IntPrimitive in (1, 2) " +
                                   "insert into BStream2S select TheString || '_2' as TheString where IntPrimitive in (2, 3) " +
                                   "insert into CStream2S select TheString || '_3' as TheString " +
                                   "output all";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                var listeners = GetListeners(env);
                env.CompileDeploy("@Name('s0') select * from AStream2S", path).AddListener("s0", listeners[0]);
                env.CompileDeploy("@Name('s1') select * from BStream2S", path).AddListener("s1", listeners[1]);
                env.CompileDeploy("@Name('s2') select * from CStream2S", path).AddListener("s2", listeners[2]);

                SendSupportBean(env, "E1", 2);
                AssertReceivedEach(listeners, new[] {"E1_1", "E1_2", "E1_3"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E2", 1);
                AssertReceivedEach(listeners, new[] {"E2_1", null, "E2_3"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E3", 3);
                AssertReceivedEach(listeners, new[] {null, "E3_2", "E3_3"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E4", -999);
                AssertReceivedEach(listeners, new[] {null, null, "E4_3"});
                Assert.IsFalse(env.Listener("split").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLOtherSplitStream3SplitDefaultOutputFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') on SupportBean as mystream " +
                                   "insert into AStream34 select mystream.TheString||'_1' as TheString where IntPrimitive=1 " +
                                   "insert into BStream34 select mystream.TheString||'_2' as TheString where IntPrimitive=2 " +
                                   "insert into CStream34 select TheString||'_3' as TheString";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                var listeners = GetListeners(env);
                env.CompileDeploy("@Name('s0') select * from AStream34", path).AddListener("s0", listeners[0]);
                env.CompileDeploy("@Name('s1') select * from BStream34", path).AddListener("s1", listeners[1]);
                env.CompileDeploy("@Name('s2') select * from CStream34", path).AddListener("s2", listeners[2]);

                Assert.AreNotSame(env.Statement("s0").EventType, env.Statement("s1").EventType);
                Assert.AreSame(
                    env.Statement("s0").EventType.UnderlyingType,
                    env.Statement("s1").EventType.UnderlyingType);

                SendSupportBean(env, "E1", 1);
                AssertReceivedSingle(listeners, 0, "E1_1");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E2", 2);
                AssertReceivedSingle(listeners, 1, "E2_2");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E3", 1);
                AssertReceivedSingle(listeners, 0, "E3_1");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E4", -999);
                AssertReceivedSingle(listeners, 2, "E4_3");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLOtherSplitStream4Split : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtOrigText = "@Name('split') on SupportBean " +
                                   "insert into AStream34 select TheString||'_1' as TheString where IntPrimitive=10 " +
                                   "insert into BStream34 select TheString||'_2' as TheString where IntPrimitive=20 " +
                                   "insert into CStream34 select TheString||'_3' as TheString where IntPrimitive<0 " +
                                   "insert into DStream34 select TheString||'_4' as TheString";
                env.CompileDeploy(stmtOrigText, path).AddListener("split");

                var listeners = GetListeners(env);
                env.CompileDeploy("@Name('s0') select * from AStream34", path).AddListener("s0", listeners[0]);
                env.CompileDeploy("@Name('s1') select * from BStream34", path).AddListener("s1", listeners[1]);
                env.CompileDeploy("@Name('s2') select * from CStream34", path).AddListener("s2", listeners[2]);
                env.CompileDeploy("@Name('s3') select * from DStream34", path).AddListener("s3", listeners[3]);

                SendSupportBean(env, "E5", -999);
                AssertReceivedSingle(listeners, 2, "E5_3");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E6", 9999);
                AssertReceivedSingle(listeners, 3, "E6_4");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E7", 20);
                AssertReceivedSingle(listeners, 1, "E7_2");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                SendSupportBean(env, "E8", 10);
                AssertReceivedSingle(listeners, 0, "E8_1");
                Assert.IsFalse(env.Listener("split").IsInvoked);

                env.UndeployAll();
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedTrigger
        {
            public int trigger;
        }

        [Serializable]
        public class MyLocalJsonProvidedTypeTwo
        {
            public int col2;
        }
    }
} // end of namespace