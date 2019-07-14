///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOnMerge
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraUpdateNonPropertySet());
            execs.Add(new InfraMergeTriggeredByAnotherWindow());
            execs.Add(new InfraPropertyInsertBean());
            execs.Add(new InfraSubselect());
            execs.Add(new InfraDocExample());
            execs.Add(new InfraOnMergeWhere1Eq2InsertSelectStar());
            execs.Add(new InfraOnMergeNoWhereClauseInsertSelectStar());
            execs.Add(new InfraOnMergeNoWhereClauseInsertTranspose());
            return execs;
        }

        private static void TryAssertionSubselect(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var fields = "col1,col2".SplitCsv();
            var epl = eventRepresentationEnum.GetAnnotationText() +
                      " create schema MyEvent as (in1 string, in2 int);\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " create schema MySchema as (col1 string, col2 int);\n";
            epl += eventRepresentationEnum.GetAnnotationText() +
                   " @name('create') create window MyWindowSS#lastevent as MySchema;\n";
            epl += "on SupportBean_A delete from MyWindowSS;\n";
            epl += "on MyEvent me " +
                   "merge MyWindowSS mw " +
                   "when not matched and (select IntPrimitive>0 from SupportBean(theString like 'A%')#lastevent) then " +
                   "insert(col1, col2) select (select TheString from SupportBean(theString like 'A%')#lastevent), (select IntPrimitive from SupportBean(theString like 'A%')#lastevent) " +
                   "when matched and (select IntPrimitive>0 from SupportBean(theString like 'B%')#lastevent) then " +
                   "update set col1=(select TheString from SupportBean(theString like 'B%')#lastevent), col2=(select IntPrimitive from SupportBean(theString like 'B%')#lastevent) " +
                   "when matched and (select IntPrimitive>0 from SupportBean(theString like 'C%')#lastevent) then " +
                   "delete;\n";
            env.CompileDeployWBusPublicType(epl, new RegressionPath());

            // no action tests
            SendMyEvent(env, eventRepresentationEnum, "X1", 1);
            env.SendEventBean(new SupportBean("A1", 0)); // ignored
            SendMyEvent(env, eventRepresentationEnum, "X2", 2);
            env.SendEventBean(new SupportBean("A2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("create"), fields, null);

            SendMyEvent(env, eventRepresentationEnum, "X3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"A2", 20}});

            env.SendEventBean(new SupportBean_A("Y1"));
            env.SendEventBean(new SupportBean("A3", 30));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("create"), fields, null);

            SendMyEvent(env, eventRepresentationEnum, "X4", 4);
            env.SendEventBean(new SupportBean("A4", 40));
            SendMyEvent(env, eventRepresentationEnum, "X5", 5); // ignored as matched (no where clause, no B event)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"A3", 30}});

            env.SendEventBean(new SupportBean("B1", 50));
            SendMyEvent(env, eventRepresentationEnum, "X6", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"B1", 50}});

            env.SendEventBean(new SupportBean("B2", 60));
            SendMyEvent(env, eventRepresentationEnum, "X7", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"B2", 60}});

            env.SendEventBean(new SupportBean("B2", 0));
            SendMyEvent(env, eventRepresentationEnum, "X8", 8);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"B2", 60}});

            env.SendEventBean(new SupportBean("C1", 1));
            SendMyEvent(env, eventRepresentationEnum, "X9", 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("create"), fields, null);

            env.SendEventBean(new SupportBean("C1", 0));
            SendMyEvent(env, eventRepresentationEnum, "X10", 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"A4", 40}});

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
                env.SendEventObjectArray(new object[] {in1, in2}, "MyEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new Dictionary<string, object>();
                theEvent.Put("in1", in1);
                theEvent.Put("in2", in2);
                env.SendEventMap(theEvent, "MyEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("MyEvent"))
                        .AsRecordSchema());
                theEvent.Put("in1", in1);
                theEvent.Put("in2", in2);
                env.EventService.SendEventAvro(theEvent, "MyEvent");
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
            var baseModuleEPL = eventRepresentationEnum.GetAnnotationText() +
                                " create schema OrderEvent as (orderId string, productId string, price double, quantity int, deletedFlag boolean)";
            env.CompileDeployWBusPublicType(baseModuleEPL, path);

            var appModuleOne = eventRepresentationEnum.GetAnnotationText() +
                               " create schema ProductTotalRec as (productId string, totalPrice double);" +
                               "" +
                               eventRepresentationEnum.GetAnnotationText() +
                               " @Name('nwProd') create window ProductWindow#unique(productId) as ProductTotalRec;" +
                               "" +
                               "on OrderEvent oe\n" +
                               "merge ProductWindow pw\n" +
                               "where pw.productId = oe.productId\n" +
                               "when matched\n" +
                               "then update set totalPrice = totalPrice + oe.price\n" +
                               "when not matched\n" +
                               "then insert select productId, price as totalPrice;";
            env.CompileDeploy(appModuleOne, path);

            var appModuleTwo = eventRepresentationEnum.GetAnnotationText() +
                               " @Name('nwOrd') create window OrderWindow#keepall as OrderEvent;" +
                               "" +
                               "on OrderEvent oe\n" +
                               "  merge OrderWindow pw\n" +
                               "  where pw.orderId = oe.orderId\n" +
                               "  when not matched \n" +
                               "    then insert select *\n" +
                               "  when matched and oe.deletedFlag=true\n" +
                               "    then delete\n" +
                               "  when matched\n" +
                               "    then update set pw.quantity = oe.quantity, pw.price = oe.price";

            env.CompileDeploy(appModuleTwo, path);

            SendOrderEvent(env, eventRepresentationEnum, "O1", "P1", 10, 100, false);
            SendOrderEvent(env, eventRepresentationEnum, "O1", "P1", 11, 200, false);
            SendOrderEvent(env, eventRepresentationEnum, "O2", "P2", 3, 300, false);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("nwProd").GetEnumerator(),
                "productId,totalPrice".SplitCsv(),
                new[] {new object[] {"P1", 21d}, new object[] {"P2", 3d}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("nwOrd").GetEnumerator(),
                "orderId,quantity".SplitCsv(),
                new[] {new object[] {"O1", 200}, new object[] {"O2", 300}});

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
                env.SendEventObjectArray(new object[] {orderId, productId, price, quantity, deletedFlag}, "OrderEvent");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("orderId", orderId);
                theEvent.Put("productId", productId);
                theEvent.Put("price", price);
                theEvent.Put("quantity", quantity);
                theEvent.Put("deletedFlag", deletedFlag);
                env.SendEventMap(theEvent, "OrderEvent");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured("OrderEvent"))
                        .AsRecordSchema());
                theEvent.Put("orderId", orderId);
                theEvent.Put("productId", productId);
                theEvent.Put("price", price);
                theEvent.Put("quantity", quantity);
                theEvent.Put("deletedFlag", deletedFlag);
                env.EventService.SendEventAvro(theEvent, "OrderEvent");
            }
            else {
                Assert.Fail();
            }
        }

        private static void RunAssertionInsertSelectStar(
            RegressionEnvironment env,
            string onInsert)
        {
            var epl = "insert into SBStream select * from SupportBean_Container[beans];\n" +
                      "@Name('window') create window MyWindow#keepall as SupportBean;\n" +
                      onInsert;
            env.CompileDeploy(epl);

            env.SendEventBean(
                new SupportBean_Container(Arrays.AsList(new SupportBean("E1", 10), new SupportBean("E2", 20))));

            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("window"),
                "theString,intPrimitive".SplitCsv(),
                new[] {new object[] {"E1", 10}, new object[] {"E2", 20}});

            env.UndeployAll();
        }

        public static void IncreaseIntCopyDouble(
            SupportBean initialBean,
            SupportBean updatedBean)
        {
            updatedBean.IntPrimitive = initialBean.IntPrimitive + 1;
            updatedBean.DoubleBoxed = updatedBean.DoublePrimitive;
        }

        internal class InfraOnMergeWhere1Eq2InsertSelectStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionInsertSelectStar(
                    env,
                    "on SBStream merge MyWindow where 1=2 when not matched then insert select *;\n");
            }
        }

        internal class InfraOnMergeNoWhereClauseInsertSelectStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionInsertSelectStar(env, "on SBStream as sbs merge MyWindow insert select *;\n");
            }
        }

        internal class InfraOnMergeNoWhereClauseInsertTranspose : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionInsertSelectStar(env, "on SBStream as sbs merge MyWindow insert select transpose(sbs);\n");
            }
        }

        internal class InfraUpdateNonPropertySet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindowUNP#keepall as SupportBean", path);
                env.CompileDeploy("insert into MyWindowUNP select * from SupportBean", path);
                env.CompileDeploy(
                    "@Name('merge') on SupportBean_S0 as sb " +
                    "merge MyWindowUNP as mywin when matched then " +
                    "update set mywin.setDoublePrimitive(id), increaseIntCopyDouble(initial, mywin)",
                    path);
                env.AddListener("merge");
                var fields = "intPrimitive,doublePrimitive,doubleBoxed".SplitCsv();

                env.SendEventBean(MakeSupportBean("E1", 10, 2));
                env.SendEventBean(new SupportBean_S0(5, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("merge").GetAndResetLastNewData()[0],
                    fields,
                    new object[] {11, 5d, 5d});

                // try a case-statement
                var eplCase = "on SupportBean_S0 merge MyWindowUNP " +
                              "when matched then update set theString = " +
                              "case intPrimitive when 1 then 'a' else 'b' end";
                env.CompileDeploy(eplCase, path);

                env.UndeployAll();
            }
        }

        internal class InfraMergeTriggeredByAnotherWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // test dispatch between named windows
                env.CompileDeploy("@Name('A') create window A#unique(id) as (id int)", path);
                env.CompileDeploy("@Name('B') create window B#unique(id) as (id int)", path);
                env.CompileDeploy(
                    "@Name('C') on A merge B when not matched then insert select 1 as id when matched then insert select 1 as id",
                    path);

                env.CompileDeploy("@Name('D') select * from B", path).AddListener("D");
                env.CompileDeploy("@Name('E') insert into A select IntPrimitive as id from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("D").IsInvoked);
                env.UndeployAll();

                // test insert-stream only, no remove stream
                var fields = "c0,c1".SplitCsv();
                var epl = "create window W1#lastevent as SupportBean;\n" +
                          "insert into W1 select * from SupportBean;\n" +
                          "create window W2#lastevent as SupportBean;\n" +
                          "on W1 as a merge W2 as b when not matched then insert into OutStream select a.TheString as c0, istream() as c1;\n" +
                          "@Name('s0') select * from OutStream;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", true});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", true});

                env.UndeployAll();
            }
        }

        internal class InfraDocExample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionDocExample(env, rep);
                }
            }
        }

        internal class InfraPropertyInsertBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('window') create window MergeWindow#unique(TheString) as SupportBean", path);

                var epl =
                    "@Name('merge') on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select IntPrimitive";
                env.CompileDeploy(epl, path);
                env.SendEventBean(new SupportBean("E1", 10));

                var theEvent = env.GetEnumerator("window").Advance();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    "theString,intPrimitive".SplitCsv(),
                    new object[] {null, 10});
                env.UndeployModuleContaining("merge");

                epl =
                    "on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select TheString, intPrimitive";
                env.CompileDeploy(epl, path);
                env.SendEventBean(new SupportBean("E2", 20));

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("window"),
                    "theString,intPrimitive".SplitCsv(),
                    new[] {new object[] {null, 10}, new object[] {"E2", 20}});

                env.UndeployAll();
            }
        }

        internal class InfraSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionSubselect(env, rep);
                }
            }
        }
    }
} // end of namespace