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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnMerge
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithUpdateNonPropertySet(execs);
            WithMergeTriggeredByAnotherWindow(execs);
            WithPropertyInsertBean(execs);
            WithSubselect(execs);
            WithDocExample(execs);
            WithOnMergeWhere1Eq2InsertSelectStar(execs);
            WithOnMergeNoWhereClauseInsertSelectStar(execs);
            WithOnMergeNoWhereClauseInsertTranspose(execs);
            WithOnMergeSetRHSEvent(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeSetRHSEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeSetRHSEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeNoWhereClauseInsertTranspose(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeNoWhereClauseInsertTranspose());
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeNoWhereClauseInsertSelectStar(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeNoWhereClauseInsertSelectStar());
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeWhere1Eq2InsertSelectStar(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeWhere1Eq2InsertSelectStar());
            return execs;
        }

        public static IList<RegressionExecution> WithDocExample(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDocExample());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyInsertBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPropertyInsertBean());
            return execs;
        }

        public static IList<RegressionExecution> WithMergeTriggeredByAnotherWindow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMergeTriggeredByAnotherWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateNonPropertySet(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowOnMergeUpdateNonPropertySet());
            return execs;
        }

        private class InfraOnMergeSetRHSEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('window') create window RecentWindow#time(30 seconds) (Id string, currentSewid SimpleEventWithId, prevSewid SimpleEventWithId);\n" +
                    "on SimpleEventWithId as sewid\n" +
                    "  merge RecentWindow as recent where recent.Id = sewid.Id\n" +
                    "  when not matched then insert select sewid.Id as Id, sewid as currentSewid, sewid as prevSewid\n" +
                    "  when matched then update set prevSewid = currentSewid, currentSewid = sewid;\n";
                env.CompileDeploy(epl);

                var sewidOne = new object[] { "Id", "A" };
                env.SendEventObjectArray(sewidOne, "SimpleEventWithId");
                AssertWindow(env, "Id", sewidOne, sewidOne);

                var sewidTwo = new object[] { "Id", "B" };
                env.SendEventObjectArray(sewidTwo, "SimpleEventWithId");
                AssertWindow(env, "Id", sewidTwo, sewidOne);

                var sewidThree = new object[] { "Id", "B" };
                env.SendEventObjectArray(sewidThree, "SimpleEventWithId");
                AssertWindow(env, "Id", sewidThree, sewidTwo);

                env.UndeployAll();
            }

            private void AssertWindow(
                RegressionEnvironment env,
                string id,
                object[] currentSewid,
                object[] prevSewid)
            {
                env.AssertIterator(
                    "window",
                    iterator => {
                        var @event = iterator.Advance();
                        Assert.AreEqual(id, @event.Get("Id"));
                        Assert.AreSame(currentSewid, ((EventBean)@event.Get("currentSewid")).Underlying);
                        Assert.AreSame(prevSewid, ((EventBean)@event.Get("prevSewid")).Underlying);
                    });
            }
        }

        private class InfraOnMergeWhere1Eq2InsertSelectStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionInsertSelectStar(
                    env,
                    "on SBStream merge MyWindow where 1=2 when not matched then insert select *;\n");
            }
        }

        private class InfraOnMergeNoWhereClauseInsertSelectStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionInsertSelectStar(env, "on SBStream as sbs merge MyWindow insert select *;\n");
            }
        }

        private class InfraOnMergeNoWhereClauseInsertTranspose : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionInsertSelectStar(env, "on SBStream as sbs merge MyWindow insert select transpose(sbs);\n");
            }
        }

        private class InfraNamedWindowOnMergeUpdateNonPropertySet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindowUNP#keepall as SupportBean", path);
                env.CompileDeploy("insert into MyWindowUNP select * from SupportBean", path);
                env.CompileDeploy(
                    "@name('merge') on SupportBean_S0 as sb " +
                    "merge MyWindowUNP as mywin when matched then " +
                    "update set mywin.setDoublePrimitive(Id), increaseIntCopyDouble(initial, mywin)",
                    path);
                env.AddListener("merge");
                var fields = "IntPrimitive,DoublePrimitive,DoubleBoxed".SplitCsv();

                env.SendEventBean(MakeSupportBean("E1", 10, 2));
                env.SendEventBean(new SupportBean_S0(5, "E1"));
                env.AssertPropsPerRowLastNew("merge", fields, new object[][] { new object[] { 11, 5d, 5d } });

                // try a case-statement
                var eplCase = "on SupportBean_S0 merge MyWindowUNP " +
                              "when matched then update set TheString = " +
                              "case IntPrimitive when 1 then 'a' else 'b' end";
                env.CompileDeploy(eplCase, path);

                env.UndeployAll();
            }
        }

        private class InfraMergeTriggeredByAnotherWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // test dispatch between named windows
                env.CompileDeploy("@name('A') @public create window A#unique(id) as (Id int)", path);
                env.CompileDeploy("@name('B') @public create window B#unique(id) as (Id int)", path);
                env.CompileDeploy(
                    "@name('C') on A merge B when not matched then insert select 1 as id when matched then insert select 1 as Id",
                    path);

                env.CompileDeploy("@name('D') select * from B", path).AddListener("D");
                env.CompileDeploy("@name('E') insert into A select IntPrimitive as Id from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerInvoked("D");
                env.UndeployAll();

                // test insert-stream only, no remove stream
                var fields = "c0,c1".SplitCsv();
                var epl = "create window W1#lastevent as SupportBean;\n" +
                          "insert into W1 select * from SupportBean;\n" +
                          "create window W2#lastevent as SupportBean;\n" +
                          "on W1 as a merge W2 as b when not matched then insert into OutStream select a.TheString as c0, istream() as c1;\n" +
                          "@name('s0') select * from OutStream;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1", true });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E2", true });

                env.UndeployAll();
            }
        }

        private class InfraDocExample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionDocExample(env, rep);
                }
            }
        }

        private class InfraPropertyInsertBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('window') @public create window MergeWindow#unique(TheString) as SupportBean",
                    path);

                var epl =
                    "@name('merge') on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select IntPrimitive";
                env.CompileDeploy(epl, path);
                env.SendEventBean(new SupportBean("E1", 10));

                env.AssertIterator(
                    "window",
                    iterator => {
                        var theEvent = iterator.Advance();
                        EPAssertionUtil.AssertProps(
                            theEvent,
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[] { null, 10 });
                    });
                env.UndeployModuleContaining("merge");

                epl =
                    "on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select TheString, IntPrimitive";
                env.CompileDeploy(epl, path);
                env.SendEventBean(new SupportBean("E2", 20));

                env.AssertPropsPerRowIterator(
                    "window",
                    "TheString,IntPrimitive".SplitCsv(),
                    new object[][] { new object[] { null, 10 }, new object[] { "E2", 20 } });

                env.UndeployAll();
            }
        }

        private class InfraSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionSubselect(env, rep);
                }
            }
        }

        private static void TryAssertionSubselect(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var fields = "col1,col2".SplitCsv();
            var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMyEvent)) +
                      " @public @buseventtype create schema MyEvent as (in1 string, in2 int);\n";
            epl += eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMySchema)) +
                   " @public @buseventtype create schema MySchema as (col1 string, col2 int);\n";
            epl += "@name('create') @public create window MyWindowSS#lastevent as MySchema;\n";
            epl += "on SupportBean_A delete from MyWindowSS;\n";
            epl += "on MyEvent me " +
                   "merge MyWindowSS mw " +
                   "when not matched and (select IntPrimitive>0 from SupportBean(TheString like 'A%')#lastevent) then " +
                   "insert(col1, col2) select (select TheString from SupportBean(TheString like 'A%')#lastevent), (select IntPrimitive from SupportBean(TheString like 'A%')#lastevent) " +
                   "when matched and (select IntPrimitive>0 from SupportBean(TheString like 'B%')#lastevent) then " +
                   "update set col1=(select TheString from SupportBean(TheString like 'B%')#lastevent), col2=(select IntPrimitive from SupportBean(TheString like 'B%')#lastevent) " +
                   "when matched and (select IntPrimitive>0 from SupportBean(TheString like 'C%')#lastevent) then " +
                   "delete;\n";
            env.CompileDeploy(epl, new RegressionPath());

            // no action tests
            SendMyEvent(env, eventRepresentationEnum, "X1", 1);
            env.SendEventBean(new SupportBean("A1", 0)); // ignored
            SendMyEvent(env, eventRepresentationEnum, "X2", 2);
            env.SendEventBean(new SupportBean("A2", 20));
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, null);

            SendMyEvent(env, eventRepresentationEnum, "X3", 3);
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "A2", 20 } });

            env.SendEventBean(new SupportBean_A("Y1"));
            env.SendEventBean(new SupportBean("A3", 30));
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, null);

            SendMyEvent(env, eventRepresentationEnum, "X4", 4);
            env.SendEventBean(new SupportBean("A4", 40));
            SendMyEvent(env, eventRepresentationEnum, "X5", 5); // ignored as matched (no where clause, no B event)
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "A3", 30 } });

            env.SendEventBean(new SupportBean("B1", 50));
            SendMyEvent(env, eventRepresentationEnum, "X6", 6);
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "B1", 50 } });

            env.SendEventBean(new SupportBean("B2", 60));
            SendMyEvent(env, eventRepresentationEnum, "X7", 7);
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "B2", 60 } });

            env.SendEventBean(new SupportBean("B2", 0));
            SendMyEvent(env, eventRepresentationEnum, "X8", 8);
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "B2", 60 } });

            env.SendEventBean(new SupportBean("C1", 1));
            SendMyEvent(env, eventRepresentationEnum, "X9", 9);
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, null);

            env.SendEventBean(new SupportBean("C1", 0));
            SendMyEvent(env, eventRepresentationEnum, "X10", 10);
            env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "A4", 40 } });

            env.UndeployAll();
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }

        private static void SendMyEvent(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string in1,
            int in2)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { in1, in2 }, "MyEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("in1", in1);
                theEvent.Put("in2", in2);
                env.SendEventMap(theEvent, "MyEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(env.RuntimeAvroSchemaPreconfigured("MyEvent").AsRecordSchema());
                theEvent.Put("in1", in1);
                theEvent.Put("in2", in2);
                env.SendEventAvro(theEvent, "MyEvent");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                env.SendEventJson("{\"in1\": \"" + in1 + "\", \"in2\": " + in2 + "}", "MyEvent");
            }
            else {
                Assert.Fail();
            }
        }

        private static void TryAssertionDocExample(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            var baseModuleEPL =
                eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedOrderEvent)) +
                " @public @buseventtype create schema OrderEvent as (OrderId string, ProductId string, Price double, quantity int, deletedFlag boolean)";
            env.CompileDeploy(baseModuleEPL, path);

            var appModuleOne =
                eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedProductTotalRec)) +
                " create schema ProductTotalRec as (ProductId string, totalPrice double);" +
                "" +
                "@name('nwProd') @public create window ProductWindow#unique(ProductId) as ProductTotalRec;" +
                "" +
                "on OrderEvent oe\n" +
                "merge ProductWindow pw\n" +
                "where pw.productId = oe.ProductId\n" +
                "when matched\n" +
                "then update set totalPrice = totalPrice + oe.Price\n" +
                "when not matched\n" +
                "then insert select ProductId, Price as totalPrice;";
            env.CompileDeploy(appModuleOne, path);

            var appModuleTwo = "@name('nwOrd') create window OrderWindow#keepall as OrderEvent;" +
                               "" +
                               "on OrderEvent oe\n" +
                               "  merge OrderWindow pw\n" +
                               "  where pw.OrderId = oe.OrderId\n" +
                               "  when not matched \n" +
                               "    then insert select *\n" +
                               "  when matched and oe.deletedFlag=true\n" +
                               "    then delete\n" +
                               "  when matched\n" +
                               "    then update set pw.quantity = oe.quantity, pw.Price = oe.Price";

            env.CompileDeploy(appModuleTwo, path);

            SendOrderEvent(env, eventRepresentationEnum, "O1", "P1", 10, 100, false);
            SendOrderEvent(env, eventRepresentationEnum, "O1", "P1", 11, 200, false);
            SendOrderEvent(env, eventRepresentationEnum, "O2", "P2", 3, 300, false);
            env.AssertPropsPerRowIteratorAnyOrder(
                "nwProd",
                "ProductId,totalPrice".SplitCsv(),
                new object[][] { new object[] { "P1", 21d }, new object[] { "P2", 3d } });
            env.AssertPropsPerRowIteratorAnyOrder(
                "nwOrd",
                "OrderId,quantity".SplitCsv(),
                new object[][] { new object[] { "O1", 200 }, new object[] { "O2", 300 } });

            var module = "create schema StreetCarCountSchema (streetid string, carcount int);" +
                         "    create schema StreetChangeEvent (streetid string, action string);" +
                         "    create window StreetCarCountWindow#unique(streetid) as StreetCarCountSchema;" +
                         "    on StreetChangeEvent ce merge StreetCarCountWindow w where ce.streetid = w.streetid\n" +
                         "    when not matched and ce.action = 'ENTER' then insert select streetid, 1 as carcount\n" +
                         "    when matched and ce.action = 'ENTER' then update set StreetCarCountWindow.carcount = carcount + 1\n" +
                         "    when matched and ce.action = 'LEAVE' then update set StreetCarCountWindow.carcount = carcount - 1;" +
                         "    select * from StreetCarCountWindow;";
            env.CompileDeploy(module, path);

            env.UndeployAll();
        }

        private static void SendOrderEvent(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string orderId,
            string productId,
            double price,
            int quantity,
            bool deletedFlag)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(
                    new object[] { orderId, productId, price, quantity, deletedFlag },
                    "OrderEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("OrderId", orderId);
                theEvent.Put("ProductId", productId);
                theEvent.Put("Price", price);
                theEvent.Put("quantity", quantity);
                theEvent.Put("deletedFlag", deletedFlag);
                env.SendEventMap(theEvent, "OrderEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(env.RuntimeAvroSchemaPreconfigured("OrderEvent").AsRecordSchema());
                theEvent.Put("OrderId", orderId);
                theEvent.Put("ProductId", productId);
                theEvent.Put("Price", price);
                theEvent.Put("quantity", quantity);
                theEvent.Put("deletedFlag", deletedFlag);
                env.SendEventAvro(theEvent, "OrderEvent");
            }
            else {
                var @object = new JObject();
                @object.Add("OrderId", orderId);
                @object.Add("ProductId", productId);
                @object.Add("Price", price);
                @object.Add("quantity", quantity);
                @object.Add("deletedFlag", deletedFlag);
                env.SendEventJson(@object.ToString(), "OrderEvent");
            }
        }

        private static void RunAssertionInsertSelectStar(
            RegressionEnvironment env,
            string onInsert)
        {
            var epl = "insert into SBStream select * from SupportBean_Container[beans];\n" +
                      "@name('window') create window MyWindow#keepall as SupportBean;\n" +
                      onInsert;
            env.CompileDeploy(epl);

            env.SendEventBean(
                new SupportBean_Container(Arrays.AsList(new SupportBean("E1", 10), new SupportBean("E2", 20))));

            env.AssertPropsPerRowIterator(
                "window",
                "TheString,IntPrimitive".SplitCsv(),
                new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });

            env.UndeployAll();
        }

        public static void IncreaseIntCopyDouble(
            SupportBean initialBean,
            SupportBean updatedBean)
        {
            updatedBean.IntPrimitive = initialBean.IntPrimitive + 1;
            updatedBean.DoubleBoxed = updatedBean.DoublePrimitive;
        }

        [Serializable]
        public class MyLocalJsonProvidedMyEvent
        {
            public string in1;
            public int in2;
        }

        [Serializable]
        public class MyLocalJsonProvidedMySchema
        {
            public string col1;
            public int col2;
        }

        [Serializable]
        public class MyLocalJsonProvidedOrderEvent
        {
            public string orderId;
            public string productId;
            public double price;
            public int quantity;
            public bool deletedFlag;
        }

        [Serializable]
        public class MyLocalJsonProvidedProductTotalRec
        {
            public string productId;
            public double totalPrice;
        }
    }
} // end of namespace