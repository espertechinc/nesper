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
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraExecuteQuery : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType(typeof(SupportBean_A));
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionInsert(epService, true);
            RunAssertionInsert(epService, false);

            RunAssertionParameterizedQuery(epService, true);
            RunAssertionParameterizedQuery(epService, false);

            RunAssertionUpdate(epService, true);
            RunAssertionUpdate(epService, false);

            RunAssertionDelete(epService, true);
            RunAssertionDelete(epService, false);

            RunAssertionDeleteContextPartitioned(epService, true);
            RunAssertionDeleteContextPartitioned(epService, false);

            RunAssertionSelectWildcard(epService, true);
            RunAssertionSelectWildcard(epService, false);

            RunAssertionSelectCountStar(epService, true);
            RunAssertionSelectCountStar(epService, false);

            RunAssertionAggUngroupedRowForAll(epService, true);
            RunAssertionAggUngroupedRowForAll(epService, false);

            RunAssertionInClause(epService, true);
            RunAssertionInClause(epService, false);

            RunAssertionAggUngroupedRowForGroup(epService, true);
            RunAssertionAggUngroupedRowForGroup(epService, false);

            RunAssertionJoin(epService, true, true);
            RunAssertionJoin(epService, false, false);
            RunAssertionJoin(epService, true, false);
            RunAssertionJoin(epService, false, true);

            RunAssertionAggUngroupedRowForEvent(epService, true);
            RunAssertionAggUngroupedRowForEvent(epService, false);

            RunAssertionJoinWhere(epService, true);
            RunAssertionJoinWhere(epService, false);

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                RunAssertion3StreamInnerJoin(epService, rep, true);
                RunAssertion3StreamInnerJoin(epService, rep, false);
            }

            RunAssertionExecuteFilter(epService, true);
            RunAssertionExecuteFilter(epService, false);

            RunAssertionInvalid(epService, true);
            RunAssertionInvalid(epService, false);
        }

        private void RunAssertionExecuteFilter(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));

            var query = "select * from MyInfra(IntPrimitive > 1, IntPrimitive < 10)";
            RunAssertionFilter(epService, query);

            query = "select * from MyInfra(IntPrimitive > 1) where IntPrimitive < 10";
            RunAssertionFilter(epService, query);

            query = "select * from MyInfra where IntPrimitive < 10 and IntPrimitive > 1";
            RunAssertionFilter(epService, query);

            DestroyInfra(epService);
        }

        private void RunAssertionFilter(EPServiceProvider epService, string query)
        {
            var fields = "TheString,IntPrimitive".Split(',');
            var result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {"E3", 5}});

            var prepared = epService.EPRuntime.PrepareQuery(query);
            EPAssertionUtil.AssertPropsPerRow(
                prepared.Execute().GetEnumerator(), fields, new[] {new object[] {"E3", 5}});
        }

        private void RunAssertionInvalid(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);
            string epl;

            epl = "insert into MyInfra select 1";
            TryInvalidFAF(
                epService, epl,
                "Error executing statement: Column '1' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into MyInfra select 1]");

            epl = "selectoo man";
            TryInvalidFAFSyntax(epService, epl, "Incorrect syntax near 'selectoo' [selectoo man]");

            epl = "select (select * from MyInfra) from MyInfra";
            TryInvalidFAF(
                epService, epl,
                "Subqueries are not a supported feature of on-demand queries [select (select * from MyInfra) from MyInfra]");

            epl = "select * from MyInfra output every 10 seconds";
            TryInvalidFAF(
                epService, epl,
                "Error executing statement: Output rate limiting is not a supported feature of on-demand queries [select * from MyInfra output every 10 seconds]");

            epl = "select prev(1, TheString) from MyInfra";
            TryInvalidFAF(
                epService, epl,
                "Error executing statement: Failed to validate select-clause expression 'prev(1,TheString)': Previous function cannot be used in this context [select prev(1, TheString) from MyInfra]");

            epl = "insert into MyInfra(IntPrimitive) select 'a'";
            if (isNamedWindow)
            {
                TryInvalidFAF(
                    epService, epl,
                    "Error executing statement: Invalid assignment of column 'IntPrimitive' of type 'System.String' to event property 'IntPrimitive' typed as '" + typeof(int).GetCleanName() + "', column and parameter types mismatch [insert into MyInfra(IntPrimitive) select 'a']");
            }
            else
            {
                TryInvalidFAF(
                    epService, epl,
                    "Error executing statement: Invalid assignment of column 'IntPrimitive' of type 'System.String' to event property 'IntPrimitive' typed as '" + typeof(int).GetCleanName() + "', column and parameter types mismatch [insert into MyInfra(IntPrimitive) select 'a']");
            }

            epl = "insert into MyInfra(IntPrimitive, TheString) select 1";
            TryInvalidFAF(
                epService, epl,
                "Error executing statement: Number of supplied values in the select or values clause does not match insert-into clause [insert into MyInfra(IntPrimitive, TheString) select 1]");

            epl = "insert into MyInfra select 1 as IntPrimitive from MyInfra";
            TryInvalidFAF(
                epService, epl,
                "Error executing statement: Insert-into fire-and-forget query can only consist of an insert-into clause and a select-clause [insert into MyInfra select 1 as IntPrimitive from MyInfra]");

            epl = "insert into MyInfra(IntPrimitive, TheString) values (1, 'a', 1)";
            TryInvalidFAF(
                epService, epl,
                "Error executing statement: Number of supplied values in the select or values clause does not match insert-into clause [insert into MyInfra(IntPrimitive, TheString) values (1, 'a', 1)]");

            if (isNamedWindow)
            {
                epl = "select * from pattern [every MyInfra]";
                TryInvalidFAF(
                    epService, epl,
                    "Error executing statement: On-demand queries require tables or named windows and do not allow event streams or patterns [select * from pattern [every MyInfra]]");

                epl = "select * from MyInfra#uni(IntPrimitive)";
                TryInvalidFAF(
                    epService, epl,
                    "Error executing statement: Views are not a supported feature of on-demand queries [select * from MyInfra#uni(IntPrimitive)]");
            }

            DestroyInfra(epService);
        }

        private void RunAssertion3StreamInnerJoin(
            EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, bool isNamedWindow)
        {
            var eplEvents = eventRepresentationEnum.GetAnnotationText() +
                            " create schema Product (productId string, categoryId string);" +
                            eventRepresentationEnum.GetAnnotationText() +
                            " create schema Category (categoryId string, owner string);" +
                            eventRepresentationEnum.GetAnnotationText() +
                            " create schema ProductOwnerDetails (productId string, owner string);";
            string epl;
            if (isNamedWindow)
            {
                epl = eplEvents +
                      eventRepresentationEnum.GetAnnotationText() +
                      " create window WinProduct#keepall as select * from Product;" +
                      eventRepresentationEnum.GetAnnotationText() +
                      " create window WinCategory#keepall as select * from Category;" +
                      eventRepresentationEnum.GetAnnotationText() +
                      " create window WinProductOwnerDetails#keepall as select * from ProductOwnerDetails;" +
                      "insert into WinProduct select * from Product;" +
                      "insert into WinCategory select * from Category;" +
                      "insert into WinProductOwnerDetails select * from ProductOwnerDetails;";
            }
            else
            {
                epl = eplEvents +
                      "create table WinProduct (productId string primary key, categoryId string primary key);" +
                      "create table WinCategory (categoryId string primary key, owner string primary key);" +
                      "create table WinProductOwnerDetails (productId string primary key, owner string);" +
                      "on Product t1 merge WinProduct t2 where t1.productId = t2.productId and t1.categoryId = t2.categoryId when not matched then insert select productId, categoryId;" +
                      "on Category t1 merge WinCategory t2 where t1.categoryId = t2.categoryId when not matched then insert select categoryId, owner;" +
                      "on ProductOwnerDetails t1 merge WinProductOwnerDetails t2 where t1.productId = t2.productId when not matched then insert select productId, owner;";
            }

            var dAdmin = epService.EPAdministrator.DeploymentAdmin;
            dAdmin.Deploy(dAdmin.Parse(epl), new DeploymentOptions());

            SendEvent(
                eventRepresentationEnum, epService, "Product", new[] {"productId=Product1", "categoryId=Category1"});
            SendEvent(
                eventRepresentationEnum, epService, "Product", new[] {"productId=Product2", "categoryId=Category1"});
            SendEvent(
                eventRepresentationEnum, epService, "Product", new[] {"productId=Product3", "categoryId=Category1"});
            SendEvent(eventRepresentationEnum, epService, "Category", new[] {"categoryId=Category1", "owner=Petar"});
            SendEvent(
                eventRepresentationEnum, epService, "ProductOwnerDetails", new[] {"productId=Product1", "owner=Petar"});

            var fields = "WinProduct.productId".Split(',');
            EventBean[] queryResults;
            queryResults = epService.EPRuntime.ExecuteQuery(
                "" +
                "select WinProduct.productId " +
                " from WinProduct" +
                " inner join WinCategory on WinProduct.categoryId=WinCategory.categoryId" +
                " inner join WinProductOwnerDetails on WinProduct.productId=WinProductOwnerDetails.productId"
            ).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new[] {new object[] {"Product1"}});

            queryResults = epService.EPRuntime.ExecuteQuery(
                "" +
                "select WinProduct.productId " +
                " from WinProduct" +
                " inner join WinCategory on WinProduct.categoryId=WinCategory.categoryId" +
                " inner join WinProductOwnerDetails on WinProduct.productId=WinProductOwnerDetails.productId" +
                " where WinCategory.owner=WinProductOwnerDetails.owner"
            ).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new[] {new object[] {"Product1"}});

            queryResults = epService.EPRuntime.ExecuteQuery(
                "" +
                "select WinProduct.productId " +
                " from WinProduct, WinCategory, WinProductOwnerDetails" +
                " where WinCategory.owner=WinProductOwnerDetails.owner" +
                " and WinProduct.categoryId=WinCategory.categoryId" +
                " and WinProduct.productId=WinProductOwnerDetails.productId"
            ).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new[] {new object[] {"Product1"}});

            var eplQuery = "select WinProduct.productId " +
                           " from WinProduct" +
                           " inner join WinCategory on WinProduct.categoryId=WinCategory.categoryId" +
                           " inner join WinProductOwnerDetails on WinProduct.productId=WinProductOwnerDetails.productId" +
                           " having WinCategory.owner=WinProductOwnerDetails.owner";
            queryResults = epService.EPRuntime.ExecuteQuery(eplQuery).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new[] {new object[] {"Product1"}});

            var model = epService.EPAdministrator.CompileEPL(eplQuery);
            queryResults = epService.EPRuntime.ExecuteQuery(model).Array;
            EPAssertionUtil.AssertPropsPerRow(queryResults, fields, new[] {new object[] {"Product1"}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("Product", false);
            epService.EPAdministrator.Configuration.RemoveEventType("Category", false);
            epService.EPAdministrator.Configuration.RemoveEventType("ProductOwnerDetails", false);
            epService.EPAdministrator.Configuration.RemoveEventType("WinProduct", false);
            epService.EPAdministrator.Configuration.RemoveEventType("WinCategory", false);
            epService.EPAdministrator.Configuration.RemoveEventType("WinProductOwnerDetails", false);
        }

        private void RunAssertionJoinWhere(EPServiceProvider epService, bool isNamedWindow)
        {
            var eplCreateOne = isNamedWindow
                ? EventRepresentationChoice.MAP.GetAnnotationText() +
                  " create window Infra1#keepall (key string, keyJoin string)"
                : "create table Infra1 (key string primary key, keyJoin string)";
            var eplCreateTwo = isNamedWindow
                ? EventRepresentationChoice.MAP.GetAnnotationText() +
                  " create window Infra2#keepall (keyJoin string, value double)"
                : "create table Infra2 (keyJoin string primary key, value double primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateOne);
            epService.EPAdministrator.CreateEPL(eplCreateTwo);

            var queryAgg =
                "select w1.key, sum(value) from Infra1 w1, Infra2 w2 WHERE w1.keyJoin = w2.keyJoin GROUP BY w1.key order by w1.key";
            var fieldsAgg = "w1.key,sum(value)".Split(',');
            var queryNoagg =
                "select w1.key, w2.value from Infra1 w1, Infra2 w2 where w1.keyJoin = w2.keyJoin and value = 1 order by w1.key";
            var fieldsNoagg = "w1.key,w2.value".Split(',');

            var result = epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            Assert.AreEqual(0, result.Length);
            result = epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            Assert.IsNull(result);

            InsertInfra1Event(epService, "key1", "keyJoin1");

            result = epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            Assert.AreEqual(0, result.Length);
            result = epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            Assert.IsNull(result);

            InsertInfra2Event(epService, "keyJoin1", 1d);

            result = epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsAgg, new[] {new object[] {"key1", 1d}});
            result = epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new[] {new object[] {"key1", 1d}});

            InsertInfra2Event(epService, "keyJoin2", 2d);

            result = epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsAgg, new[] {new object[] {"key1", 1d}});
            result = epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new[] {new object[] {"key1", 1d}});

            InsertInfra1Event(epService, "key2", "keyJoin2");

            result = epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(
                result, fieldsAgg, new[] {new object[] {"key1", 1d}, new object[] {"key2", 2d}});
            result = epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(result, fieldsNoagg, new[] {new object[] {"key1", 1d}});

            InsertInfra2Event(epService, "keyJoin2", 1d);

            result = epService.EPRuntime.ExecuteQuery(queryAgg).Array;
            EPAssertionUtil.AssertPropsPerRow(
                result, fieldsAgg, new[] {new object[] {"key1", 1d}, new object[] {"key2", 3d}});
            result = epService.EPRuntime.ExecuteQuery(queryNoagg).Array;
            EPAssertionUtil.AssertPropsPerRow(
                result, fieldsNoagg, new[] {new object[] {"key1", 1d}, new object[] {"key2", 1d}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("Infra1", false);
            epService.EPAdministrator.Configuration.RemoveEventType("Infra2", false);
        }

        private void InsertInfra1Event(EPServiceProvider epService, string key, string keyJoin)
        {
            epService.EPRuntime.ExecuteQuery("insert into Infra1 values ('" + key + "', '" + keyJoin + "')");
        }

        private void InsertInfra2Event(EPServiceProvider epService, string keyJoin, double value)
        {
            // BUG: if a double is 'rendered' as an integer, it will get stored in the underlying
            // map as an integer.  this can cause secondary problems if the data type is denoted
            // as something else.
            epService.EPRuntime.ExecuteQuery("insert into Infra2 values ('" + keyJoin + "', " + value.RenderAny() + ")");
        }

        private void RunAssertionAggUngroupedRowForEvent(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            var fields = new[] {"TheString", "total"};

            var query = "select TheString, sum(IntPrimitive) as total from MyInfra";
            var result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                result.GetEnumerator(), fields,
                new[] {new object[] {"E1", 16}, new object[] {"E2", 16}, new object[] {"E3", 16}});

            epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                result.GetEnumerator(), fields,
                new[]
                {
                    new object[] {"E1", 14}, new object[] {"E2", 14}, new object[] {"E3", 14}, new object[] {"E4", 14}
                });

            DestroyInfra(epService);
        }

        private void RunAssertionJoin(EPServiceProvider epService, bool isFirstNW, bool isSecondNW)
        {
            SetupInfra(epService, isFirstNW);

            var eplSecondCreate = isSecondNW
                ? "create window MySecondInfra#keepall as select * from SupportBean_A"
                : "create table MySecondInfra as (id string primary key)";
            epService.EPAdministrator.CreateEPL(eplSecondCreate);
            var eplSecondFill = isSecondNW
                ? "insert into MySecondInfra select * from SupportBean_A "
                : "on SupportBean_A sba merge MySecondInfra msi where msi.id = sba.id when not matched then insert select id";
            epService.EPAdministrator.CreateEPL(eplSecondFill);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            var fields = new[] {"TheString", "IntPrimitive", "id"};

            var query = "select TheString, IntPrimitive, id from MyInfra nw1, " +
                        "MySecondInfra nw2 where nw1.TheString = nw2.id";
            var result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {"E2", 11, "E2"}});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));

            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                result.GetEnumerator(), fields,
                new[] {new object[] {"E2", 11, "E2"}, new object[] {"E3", 1, "E3"}, new object[] {"E3", 2, "E3"}});

            DestroyInfra(epService);
            epService.EPAdministrator.Configuration.RemoveEventType("MySecondInfra", false);
        }

        private void RunAssertionAggUngroupedRowForGroup(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            var fields = new[] {"TheString", "total"};

            var query =
                "select TheString, sum(IntPrimitive) as total from MyInfra group by TheString order by TheString asc";
            var result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(
                result.GetEnumerator(), fields, new[] {new object[] {"E1", 6}, new object[] {"E2", 11}});

            epService.EPRuntime.SendEvent(new SupportBean("E2", -2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(
                result.GetEnumerator(), fields,
                new[] {new object[] {"E1", 6}, new object[] {"E2", 9}, new object[] {"E3", 3}});

            DestroyInfra(epService);
        }

        private void RunAssertionInClause(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            epService.EPRuntime.SendEvent(MakeBean("E1", 10, 100L));
            epService.EPRuntime.SendEvent(MakeBean("E2", 20, 200L));
            epService.EPRuntime.SendEvent(MakeBean("E3", 30, 300L));
            epService.EPRuntime.SendEvent(MakeBean("E4", 40, 400L));

            // try no index
            RunAssertionIn(epService);

            // try suitable index
            var stmtIdx1 = epService.EPAdministrator.CreateEPL("create index Idx1 on MyInfra(TheString, IntPrimitive)");
            RunAssertionIn(epService);
            stmtIdx1.Dispose();

            // backwards index
            var stmtIdx2 = epService.EPAdministrator.CreateEPL("create index Idx2 on MyInfra(IntPrimitive, TheString)");
            RunAssertionIn(epService);
            stmtIdx2.Dispose();

            // partial index
            var stmtIdx3 = epService.EPAdministrator.CreateEPL("create index Idx3 on MyInfra(IntPrimitive)");
            RunAssertionIn(epService);
            stmtIdx3.Dispose();

            DestroyInfra(epService);
        }

        private void RunAssertionIn(EPServiceProvider epService)
        {
            TryAssertionIn(epService, "TheString in ('E2', 'E3') and IntPrimitive in (10, 20)", new[] {200L});
            TryAssertionIn(epService, "IntPrimitive in (30, 20) and TheString in ('E4', 'E1')", new long[] { });
            TryAssertionIn(epService, "IntPrimitive in (30, 20) and TheString in ('E2', 'E1')", new[] {200L});
            TryAssertionIn(epService, "TheString in ('E2', 'E3') and IntPrimitive in (20, 30)", new[] {200L, 300L});
            TryAssertionIn(epService, "TheString in ('E2', 'E3') and IntPrimitive in (30, 20)", new[] {200L, 300L});
            TryAssertionIn(
                epService, "TheString in ('E1', 'E2', 'E3', 'E4') and IntPrimitive in (10, 20, 30)",
                new[] {100L, 200L, 300L});
        }

        private void TryAssertionIn(EPServiceProvider epService, string filter, long[] expected)
        {
            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfra where " + filter);
            Assert.AreEqual(result.Array.Length, expected.Length);
            var values = new List<long>();
            foreach (var @event in result.Array)
            {
                values.Add(@event.Get("LongPrimitive").AsLong());
            }

            EPAssertionUtil.AssertEqualsAnyOrder(expected, values.ToArray());
        }

        private void RunAssertionAggUngroupedRowForAll(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            var fields = new[] {"total"};

            var query = "select sum(IntPrimitive) as total from MyInfra";
            var result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {16}});

            epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {14}});

            DestroyInfra(epService);
        }

        private void RunAssertionSelectCountStar(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            var fields = new[] {"cnt"};
            var query = "select count(*) as cnt from MyInfra";
            var prepared = epService.EPRuntime.PrepareQuery(query);

            var result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {0L}});
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new[] {new object[] {0L}});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {1L}});
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new[] {new object[] {1L}});
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {1L}});
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new[] {new object[] {1L}});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {2L}});
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new[] {new object[] {2L}});

            var model = epService.EPAdministrator.CompileEPL(query);
            result = epService.EPRuntime.ExecuteQuery(model);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {2L}});
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, new[] {new object[] {2L}});

            var preparedFromModel = epService.EPRuntime.PrepareQuery(model);
            EPAssertionUtil.AssertPropsPerRow(
                preparedFromModel.Execute().GetEnumerator(), fields, new[] {new object[] {2L}});

            DestroyInfra(epService);
        }

        private void RunAssertionSelectWildcard(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            var query = "select * from MyInfra";
            var prepared = epService.EPRuntime.PrepareQuery(query);
            var result = epService.EPRuntime.ExecuteQuery(query);
            var fields = new[] {"TheString", "IntPrimitive"};
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(prepared.Execute().GetEnumerator(), fields, null);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), fields, new[] {new object[] {"E1", 1}});
            EPAssertionUtil.AssertPropsPerRow(
                prepared.Execute().GetEnumerator(), fields, new[] {new object[] {"E1", 1}});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                result.GetEnumerator(), fields, new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                prepared.Execute().GetEnumerator(), fields, new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

            DestroyInfra(epService);
        }

        private void RunAssertionDeleteContextPartitioned(EPServiceProvider epService, bool isNamedWindow)
        {
            // test hash-segmented context
            var eplCtx =
                "create context MyCtx coalesce Consistent_hash_crc32(TheString) from SupportBean granularity 4 preallocate";
            epService.EPAdministrator.CreateEPL(eplCtx);

            var eplCreate = isNamedWindow
                ? "context MyCtx create window CtxInfra#keepall as SupportBean"
                : "context MyCtx create table CtxInfra (TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            var eplPopulate = isNamedWindow
                ? "context MyCtx insert into CtxInfra select * from SupportBean"
                : "context MyCtx on SupportBean sb merge CtxInfra ci where sb.TheString = ci.TheString and sb.IntPrimitive = ci.IntPrimitive when not matched then insert select TheString, IntPrimitive";
            epService.EPAdministrator.CreateEPL(eplPopulate);

            var codeFunc = new SupportHashCodeFuncGranularCRC32(4);
            var codes = new int[5];
            for (var i = 0; i < 5; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
                codes[i] = codeFunc.CodeFor("E" + i);
            }

            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {3, 1, 3, 1, 2}, codes); // just to make sure CRC32 didn't change

            // assert counts individually per context partition
            AssertCtxInfraCountPerCode(epService, new long[] {0, 2, 1, 2});

            // delete per context partition (E0 ended up in '3')
            epService.EPRuntime.ExecuteQuery(
                "delete from CtxInfra where TheString = 'E0'",
                new ContextPartitionSelector[] {new SupportSelectorByHashCode(1)});
            AssertCtxInfraCountPerCode(epService, new long[] {0, 2, 1, 2});

            var result = epService.EPRuntime.ExecuteQuery(
                "delete from CtxInfra where TheString = 'E0'",
                new ContextPartitionSelector[] {new SupportSelectorByHashCode(3)});
            AssertCtxInfraCountPerCode(epService, new long[] {0, 2, 1, 1});
            if (isNamedWindow)
            {
                EPAssertionUtil.AssertPropsPerRow(result.Array, "TheString".Split(','), new[] {new object[] {"E0"}});
            }

            // delete per context partition (E1 ended up in '1')
            epService.EPRuntime.ExecuteQuery(
                "delete from CtxInfra where TheString = 'E1'",
                new ContextPartitionSelector[] {new SupportSelectorByHashCode(0)});
            AssertCtxInfraCountPerCode(epService, new long[] {0, 2, 1, 1});

            epService.EPRuntime.ExecuteQuery(
                "delete from CtxInfra where TheString = 'E1'",
                new ContextPartitionSelector[] {new SupportSelectorByHashCode(1)});
            AssertCtxInfraCountPerCode(epService, new long[] {0, 1, 1, 1});
            epService.EPAdministrator.DestroyAllStatements();

            // test category-segmented context
            var eplCtxCategory =
                "create context MyCtxCat group by IntPrimitive < 0 as negative, group by IntPrimitive > 0 as positive from SupportBean";
            epService.EPAdministrator.CreateEPL(eplCtxCategory);
            epService.EPAdministrator.CreateEPL("context MyCtxCat create window CtxInfraCat#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("context MyCtxCat insert into CtxInfraCat select * from SupportBean");

            epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E3", -3));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            Assert.AreEqual(2L, GetCtxInfraCatCount(epService, "positive"));
            Assert.AreEqual(2L, GetCtxInfraCatCount(epService, "negative"));

            result = epService.EPRuntime.ExecuteQuery(
                "context MyCtxCat delete from CtxInfraCat where context.label = 'negative'");
            Assert.AreEqual(2L, GetCtxInfraCatCount(epService, "positive"));
            Assert.AreEqual(0L, GetCtxInfraCatCount(epService, "negative"));
            EPAssertionUtil.AssertPropsPerRow(
                result.Array, "TheString".Split(','), new[] {new object[] {"E1"}, new object[] {"E3"}});

            DestroyInfra(epService);
            epService.EPAdministrator.Configuration.RemoveEventType("CtxInfra", false);
        }

        private void RunAssertionDelete(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            // test delete-all
            for (var i = 0; i < 10; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            Assert.AreEqual(10L, GetMyInfraCount(epService));
            var result = epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            Assert.AreEqual(0L, GetMyInfraCount(epService));
            if (isNamedWindow)
            {
                Assert.AreEqual(epService.EPAdministrator.Configuration.GetEventType("MyInfra"), result.EventType);
                Assert.AreEqual(10, result.Array.Length);
                Assert.AreEqual("E0", result.Array[0].Get("TheString"));
            }
            else
            {
                Assert.AreEqual(0, result.Array.Length);
            }

            // test SODA + where-clause
            for (var i = 0; i < 10; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            Assert.AreEqual(10L, GetMyInfraCount(epService));
            var eplWithWhere = "delete from MyInfra where TheString=\"E1\"";
            var modelWithWhere = epService.EPAdministrator.CompileEPL(eplWithWhere);
            Assert.AreEqual(eplWithWhere, modelWithWhere.ToEPL());
            result = epService.EPRuntime.ExecuteQuery(modelWithWhere);
            Assert.AreEqual(9L, GetMyInfraCount(epService));
            if (isNamedWindow)
            {
                Assert.AreEqual(epService.EPAdministrator.Configuration.GetEventType("MyInfra"), result.EventType);
                EPAssertionUtil.AssertPropsPerRow(result.Array, "TheString".Split(','), new[] {new object[] {"E1"}});
            }

            // test SODA delete-all
            var eplDelete = "delete from MyInfra";
            var modelDeleteOnly = epService.EPAdministrator.CompileEPL(eplDelete);
            Assert.AreEqual(eplDelete, modelDeleteOnly.ToEPL());
            epService.EPRuntime.ExecuteQuery(modelDeleteOnly);
            Assert.AreEqual(0L, GetMyInfraCount(epService));

            // test with index
            if (isNamedWindow)
            {
                epService.EPAdministrator.CreateEPL("create unique index Idx1 on MyInfra (TheString)");
            }

            for (var i = 0; i < 5; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            RunQueryAssertCount(
                epService, INDEX_CALLBACK_HOOK + "delete from MyInfra where TheString = 'E1' and IntPrimitive = 0", 5,
                isNamedWindow ? "Idx1" : "primary-MyInfra",
                isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCount(
                epService, INDEX_CALLBACK_HOOK + "delete from MyInfra where TheString = 'E1' and IntPrimitive = 1", 4,
                isNamedWindow ? "Idx1" : "primary-MyInfra",
                isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCount(
                epService, INDEX_CALLBACK_HOOK + "delete from MyInfra where TheString = 'E2'", 3,
                isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);
            RunQueryAssertCount(
                epService, INDEX_CALLBACK_HOOK + "delete from MyInfra where IntPrimitive = 4", 2, null, null);

            // test with alias
            RunQueryAssertCount(
                epService, INDEX_CALLBACK_HOOK + "delete from MyInfra as w1 where w1.TheString = 'E3'", 1,
                isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);

            // test consumption
            var stmt = epService.EPAdministrator.CreateEPL("select rstream * from MyInfra");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            var fields = new[] {"TheString", "IntPrimitive"};
            if (isNamedWindow)
            {
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E0", 0});
            }
            else
            {
                Assert.IsFalse(listener.IsInvoked);
            }

            DestroyInfra(epService);
        }

        private void RunQueryAssertCount(
            EPServiceProvider epService, string epl, int count, string indexName, string backingClass)
        {
            epService.EPRuntime.ExecuteQuery(epl);
            Assert.AreEqual(count, GetMyInfraCount(epService));
            SupportQueryPlanIndexHook.AssertFAFAndReset(indexName, backingClass);
        }

        private long GetMyInfraCount(EPServiceProvider epService)
        {
            return (long) epService.EPRuntime.ExecuteQuery("select count(*) as c0 from MyInfra").Array[0].Get("c0");
        }

        private void RunAssertionUpdate(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);
            var fields = new[] {"TheString", "IntPrimitive"};

            // test update-all
            for (var i = 0; i < 2; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            var result = epService.EPRuntime.ExecuteQuery("update MyInfra set TheString = 'ABC'");

            // Esper test in Java assumes order of rows.  However, tables are implemented with maps
            // (aka hashtables) and thus cannot be assumed to return a "fixed" order of rows.
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields,
                new[] {
                    new object[] {"ABC", 0},
                    new object[] {"ABC", 1}
                });

            if (isNamedWindow)
            {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    result.Array, fields, new[] {
                        new object[] {"ABC", 0},
                        new object[] {"ABC", 1}
                    });
            }

            // test update with where-clause
            epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            for (var i = 0; i < 3; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            result = epService.EPRuntime.ExecuteQuery(
                "update MyInfra set TheString = 'X', IntPrimitive=-1 where TheString = 'E1' and IntPrimitive = 1");
            if (isNamedWindow)
            {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"X", -1}});
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields,
                new[] {new object[] {"E0", 0}, new object[] {"E2", 2}, new object[] {"X", -1}});

            // test update with SODA
            var epl = "update MyInfra set IntPrimitive=IntPrimitive+10 where TheString=\"E2\"";
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            result = epService.EPRuntime.ExecuteQuery(model);
            if (isNamedWindow)
            {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E2", 12}});
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields,
                new[] {new object[] {"E0", 0}, new object[] {"X", -1}, new object[] {"E2", 12}});

            // test update with initial value
            result = epService.EPRuntime.ExecuteQuery(
                "update MyInfra set IntPrimitive=5, TheString='x', TheString = initial.TheString || 'y', IntPrimitive=initial.IntPrimitive+100 where TheString = 'E0'");
            if (isNamedWindow)
            {
                EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E0y", 100}});
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(), fields,
                new[] {new object[] {"X", -1}, new object[] {"E2", 12}, new object[] {"E0y", 100}});
            epService.EPRuntime.ExecuteQuery("delete from MyInfra");

            // test with index
            if (isNamedWindow)
            {
                epService.EPAdministrator.CreateEPL("create unique index Idx1 on MyInfra (TheString)");
            }

            for (var i = 0; i < 5; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            RunQueryAssertCountNonNegative(
                epService,
                INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where TheString = 'E1' and IntPrimitive = 0",
                5, isNamedWindow ? "Idx1" : "primary-MyInfra",
                isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCountNonNegative(
                epService,
                INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where TheString = 'E1' and IntPrimitive = 1",
                4, isNamedWindow ? "Idx1" : "primary-MyInfra",
                isNamedWindow ? BACKING_SINGLE_UNIQUE : BACKING_MULTI_UNIQUE);
            RunQueryAssertCountNonNegative(
                epService, INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where TheString = 'E2'", 3,
                isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);
            RunQueryAssertCountNonNegative(
                epService, INDEX_CALLBACK_HOOK + "update MyInfra set IntPrimitive=-1 where IntPrimitive = 4", 2, null,
                null);

            // test with alias
            RunQueryAssertCountNonNegative(
                epService, INDEX_CALLBACK_HOOK + "update MyInfra as w1 set IntPrimitive=-1 where w1.TheString = 'E3'",
                1, isNamedWindow ? "Idx1" : null, isNamedWindow ? BACKING_SINGLE_UNIQUE : null);

            // test consumption
            var stmt = epService.EPAdministrator.CreateEPL("select irstream * from MyInfra");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.ExecuteQuery("update MyInfra set IntPrimitive=1000 where TheString = 'E0'");
            if (isNamedWindow)
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertPairGetIRAndReset(), fields, new object[] {"E0", 1000}, new object[] {"E0", 0});
            }

            // test update via UDF and setter
            if (isNamedWindow)
            {
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("doubleInt", GetType(), "DoubleInt");
                epService.EPRuntime.ExecuteQuery("delete from MyInfra");
                epService.EPRuntime.SendEvent(new SupportBean("A", 10));
                epService.EPRuntime.ExecuteQuery("update MyInfra mw set mw.TheString = 'XYZ', doubleInt(mw)");
                EPAssertionUtil.AssertPropsPerRow(
                    epService.EPAdministrator.GetStatement("TheInfra").GetEnumerator(),
                    "TheString,IntPrimitive".Split(','), new[] {new object[] {"XYZ", 20}});
            }

            DestroyInfra(epService);
        }

        private void RunQueryAssertCountNonNegative(
            EPServiceProvider epService, string epl, int count, string indexName, string backingClass)
        {
            SupportQueryPlanIndexHook.Reset();
            epService.EPRuntime.ExecuteQuery(epl);
            var actual = (long) epService.EPRuntime
                .ExecuteQuery("select count(*) as c0 from MyInfra where IntPrimitive >= 0").Array[0].Get("c0");
            Assert.AreEqual(count, actual);
            SupportQueryPlanIndexHook.AssertFAFAndReset(indexName, backingClass);
        }

        private void RunAssertionParameterizedQuery(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            for (var i = 0; i < 10; i++)
            {
                epService.EPRuntime.SendEvent(MakeBean("E" + i, i, i * 1000));
            }

            // test one parameter
            var eplOneParam = "select * from MyInfra where IntPrimitive = ?";
            var pqOneParam = epService.EPRuntime.PrepareQueryWithParameters(eplOneParam);
            for (var i = 0; i < 10; i++)
            {
                RunParameterizedQuery(epService, pqOneParam, new object[] {i}, new[] {"E" + i});
            }

            RunParameterizedQuery(epService, pqOneParam, new object[] {-1}, null); // not found

            // test two parameter
            var eplTwoParam = "select * from MyInfra where IntPrimitive = ? and LongPrimitive = ?";
            var pqTwoParam = epService.EPRuntime.PrepareQueryWithParameters(eplTwoParam);
            for (var i = 0; i < 10; i++)
            {
                RunParameterizedQuery(epService, pqTwoParam, new object[] {i, (long) i * 1000}, new[] {"E" + i});
            }

            RunParameterizedQuery(epService, pqTwoParam, new object[] {-1, 1000}, null); // not found

            // test in-clause with string objects
            var eplInSimple = "select * from MyInfra where TheString in (?, ?, ?)";
            var pqInSimple = epService.EPRuntime.PrepareQueryWithParameters(eplInSimple);
            RunParameterizedQuery(epService, pqInSimple, new object[] {"A", "A", "A"}, null); // not found
            RunParameterizedQuery(epService, pqInSimple, new object[] {"A", "E3", "A"}, new[] {"E3"});

            // test in-clause with string array
            var eplInArray = "select * from MyInfra where TheString in (?)";
            var pqInArray = epService.EPRuntime.PrepareQueryWithParameters(eplInArray);
            RunParameterizedQuery(
                epService, pqInArray, new object[] {new[] {"E3", "E6", "E8"}}, new[] {"E3", "E6", "E8"});

            // various combinations
            RunParameterizedQuery(
                epService,
                epService.EPRuntime.PrepareQueryWithParameters(
                    "select * from MyInfra where TheString in (?) and LongPrimitive = 4000"),
                new object[] {new[] {"E3", "E4", "E8"}}, new[] {"E4"});
            RunParameterizedQuery(
                epService,
                epService.EPRuntime.PrepareQueryWithParameters("select * from MyInfra where LongPrimitive > 8000"),
                new object[] { }, new[] {"E9"});
            RunParameterizedQuery(
                epService,
                epService.EPRuntime.PrepareQueryWithParameters("select * from MyInfra where LongPrimitive < ?"),
                new object[] {2000}, new[] {"E0", "E1"});
            RunParameterizedQuery(
                epService,
                epService.EPRuntime.PrepareQueryWithParameters(
                    "select * from MyInfra where LongPrimitive between ? and ?"),
                new object[] {2000, 4000}, new[] {"E2", "E3", "E4"});

            DestroyInfra(epService);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunParameterizedQuery(
            EPServiceProvider epService, EPOnDemandPreparedQueryParameterized parameterizedQuery, object[] parameters,
            string[] expected)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                parameterizedQuery.SetObject(i + 1, parameters[i]);
            }

            var result = epService.EPRuntime.ExecuteQuery(parameterizedQuery);
            if (expected == null)
            {
                Assert.AreEqual(0, result.Array.Length);
            }
            else
            {
                Assert.AreEqual(expected.Length, result.Array.Length);
                var resultStrings = new string[result.Array.Length];
                for (var i = 0; i < resultStrings.Length; i++)
                {
                    resultStrings[i] = (string) result.Array[i].Get("TheString");
                }

                EPAssertionUtil.AssertEqualsAnyOrder(expected, resultStrings);
            }
        }

        private void RunAssertionInsert(EPServiceProvider epService, bool isNamedWindow)
        {
            SetupInfra(epService, isNamedWindow);

            var stmt = epService.EPAdministrator.GetStatement("TheInfra");
            var propertyNames = "TheString,IntPrimitive".Split(',');

            // try column name provided with insert-into
            var eplSelect = "insert into MyInfra (TheString, IntPrimitive) select 'a', 1";
            var resultOne = epService.EPRuntime.ExecuteQuery(eplSelect);
            AssertFAFInsertResult(resultOne, new object[] {"a", 1}, propertyNames, stmt, isNamedWindow);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), propertyNames, new[] {new object[] {"a", 1}});

            // try SODA and column name not provided with insert-into
            var eplTwo = "insert into MyInfra select \"b\" as TheString, 2 as IntPrimitive";
            var modelWSelect = epService.EPAdministrator.CompileEPL(eplTwo);
            Assert.AreEqual(eplTwo, modelWSelect.ToEPL());
            var resultTwo = epService.EPRuntime.ExecuteQuery(modelWSelect);
            AssertFAFInsertResult(resultTwo, new object[] {"b", 2}, propertyNames, stmt, isNamedWindow);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), propertyNames, new[] {new object[] {"a", 1}, new object[] {"b", 2}});

            // create unique index, insert duplicate row
            epService.EPAdministrator.CreateEPL("create unique index I1 on MyInfra (TheString)");
            try
            {
                var eplThree = "insert into MyInfra (TheString) select 'a' as TheString";
                epService.EPRuntime.ExecuteQuery(eplThree);
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Error executing statement: Unique index violation, index 'I1' is a unique index and key 'a' already exists [insert into MyInfra (TheString) select 'a' as TheString]",
                    ex.Message);
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), propertyNames, new[] {new object[] {"a", 1}, new object[] {"b", 2}});

            // try second no-column-provided version
            var eplMyInfraThree = isNamedWindow
                ? "create window MyInfraThree#keepall as (p0 string, p1 int)"
                : "create table MyInfraThree as (p0 string, p1 int)";
            var stmtMyInfraThree = epService.EPAdministrator.CreateEPL(eplMyInfraThree);
            epService.EPRuntime.ExecuteQuery("insert into MyInfraThree select 'a' as p0, 1 as p1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtMyInfraThree.GetEnumerator(), "p0,p1".Split(','), new[] {new object[] {"a", 1}});

            // try enum-value insert
            epService.EPAdministrator.CreateEPL("create schema MyMode (mode " + typeof(SupportEnum).FullName + ")");
            var eplInfraMode = isNamedWindow
                ? "create window MyInfraTwo#unique(mode) as MyMode"
                : "create table MyInfraTwo as (mode " + typeof(SupportEnum).FullName + ")";
            var stmtEnumWin = epService.EPAdministrator.CreateEPL(eplInfraMode);
            epService.EPRuntime.ExecuteQuery(
                "insert into MyInfraTwo select " + typeof(SupportEnum).FullName + "." +
                SupportEnum.ENUM_VALUE_2.GetName() + " as mode");
            EPAssertionUtil.AssertProps(
                stmtEnumWin.First(), "mode".Split(','), new object[] {SupportEnum.ENUM_VALUE_2});

            // try insert-into with values-keyword and explicit column names
            epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            var eplValuesKW = "insert into MyInfra(TheString, IntPrimitive) values (\"a\", 1)";
            var resultValuesKW = epService.EPRuntime.ExecuteQuery(eplValuesKW);
            AssertFAFInsertResult(resultValuesKW, new object[] {"a", 1}, propertyNames, stmt, isNamedWindow);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), propertyNames, new[] {new object[] {"a", 1}});

            // try insert-into with values model
            epService.EPRuntime.ExecuteQuery("delete from MyInfra");
            var modelWValuesKW = epService.EPAdministrator.CompileEPL(eplValuesKW);
            Assert.AreEqual(eplValuesKW, modelWValuesKW.ToEPL());
            epService.EPRuntime.ExecuteQuery(modelWValuesKW);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), propertyNames, new[] {new object[] {"a", 1}});

            // try insert-into with values-keyword and as-names
            epService.EPRuntime.ExecuteQuery("delete from MyInfraThree");
            var eplValuesWithoutCols = "insert into MyInfraThree values ('b', 2)";
            epService.EPRuntime.ExecuteQuery(eplValuesWithoutCols);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtMyInfraThree.GetEnumerator(), "p0,p1".Split(','), new[] {new object[] {"b", 2}});

            DestroyInfra(epService);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraTwo", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraThree", false);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void AssertFAFInsertResult(
            EPOnDemandQueryResult resultOne, object[] objects, string[] propertyNames, EPStatement stmt,
            bool isNamedWindow)
        {
            Assert.AreSame(resultOne.EventType, stmt.EventType);
            if (isNamedWindow)
            {
                Assert.AreEqual(1, resultOne.Array.Length);
                EPAssertionUtil.AssertPropsPerRow(resultOne.Array, propertyNames, new[] {objects});
            }
            else
            {
                Assert.AreEqual(0, resultOne.Array.Length);
            }
        }

        private void SetupInfra(EPServiceProvider epService, bool isNamedWindow)
        {
            var eplCreate = isNamedWindow
                ? "@Name('TheInfra') create window MyInfra#keepall as select * from SupportBean"
                : "@Name('TheInfra') create table MyInfra as (TheString string primary key, IntPrimitive int primary key, LongPrimitive long)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            var eplInsert = isNamedWindow
                ? "@Name('Insert') insert into MyInfra select * from SupportBean"
                : "@Name('Insert') on SupportBean sb merge MyInfra mi where mi.TheString = sb.TheString and mi.IntPrimitive=sb.IntPrimitive" +
                  " when not matched then insert select TheString, IntPrimitive, LongPrimitive";
            epService.EPAdministrator.CreateEPL(eplInsert);
        }

        private void DestroyInfra(EPServiceProvider epService)
        {
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }

        private SupportBean MakeBean(string theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        public static void DoubleInt(SupportBean bean)
        {
            bean.IntPrimitive = bean.IntPrimitive * 2;
        }

        private void AssertCtxInfraCountPerCode(EPServiceProvider epService, long[] expectedCountPerCode)
        {
            for (var i = 0; i < expectedCountPerCode.Length; i++)
            {
                Assert.AreEqual(expectedCountPerCode[i], GetCtxInfraCount(epService, i), "for code " + i);
            }
        }

        private void SendEvent(
            EventRepresentationChoice eventRepresentationEnum, EPServiceProvider epService, string eventName,
            string[] attributes)
        {
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                var eventObjectArray = new List<object>();
                foreach (var attribute in attributes)
                {
                    var value = attribute.Split('=')[1];
                    eventObjectArray.Add(value);
                }

                epService.EPRuntime.SendEvent(eventObjectArray.ToArray(), eventName);
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                var eventMap = new Dictionary<string, object>();
                foreach (var attribute in attributes)
                {
                    var key = attribute.Split('=')[0];
                    var value = attribute.Split('=')[1];
                    eventMap.Put(key, value);
                }

                epService.EPRuntime.SendEvent(eventMap, eventName);
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, eventName).AsRecordSchema());
                foreach (var attribute in attributes)
                {
                    var key = attribute.Split('=')[0];
                    var value = attribute.Split('=')[1];
                    record.Put(key, value);
                }

                epService.EPRuntime.SendEventAvro(record, eventName);
            }
            else
            {
                Assert.Fail();
            }
        }

        private long GetCtxInfraCount(EPServiceProvider epService, int hashCode)
        {
            var result = epService.EPRuntime.ExecuteQuery(
                "select count(*) as c0 from CtxInfra",
                new ContextPartitionSelector[] {new SupportSelectorByHashCode(hashCode)});
            return (long) result.Array[0].Get("c0");
        }

        private long GetCtxInfraCatCount(EPServiceProvider epService, string categoryName)
        {
            var result = epService.EPRuntime.ExecuteQuery(
                "select count(*) as c0 from CtxInfraCat",
                new ContextPartitionSelector[] {new SupportSelectorCategory(categoryName)});
            return (long) result.Array[0].Get("c0");
        }
    }
} // end of namespace