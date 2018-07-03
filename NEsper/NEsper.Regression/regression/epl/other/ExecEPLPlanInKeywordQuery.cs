///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLPlanInKeywordQuery : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
            epService.EPAdministrator.Configuration.AddEventType("S2", typeof(SupportBean_S2));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            RunAssertionNotIn(epService);
            RunAssertionMultiIdxMultipleInAndMultirow(epService);
            RunAssertionMultiIdxSubquery(epService);
            RunAssertionSingleIdxMultipleInAndMultirow(epService);
            RunAssertionSingleIdxSubquery(epService);
            RunAssertionSingleIdxConstants(epService);
            RunAssertionMultiIdxConstants(epService);
            RunAssertionQueryPlan3Stream(epService);
            RunAssertionQueryPlan2Stream(epService);
        }

        private void RunAssertionNotIn(EPServiceProvider epService)
        {
            SupportQueryPlanIndexHook.Reset();
            var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
                      "where P00 not in (P10, P11)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
            Assert.AreEqual("null", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

            stmt.Dispose();
        }

        private void RunAssertionMultiIdxMultipleInAndMultirow(EPServiceProvider epService)
        {
            // assert join
            SupportQueryPlanIndexHook.Reset();
            var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
                      "where P00 in (P10, P11) and P01 in (P12, P13)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
            Assert.AreEqual("[P10][P11]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

            TryAssertionMultiIdx(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();

            // assert named window
            epService.EPAdministrator.CreateEPL("create window S1Window#keepall as S1");
            epService.EPAdministrator.CreateEPL("insert into S1Window select * from S1");

            var eplNamedWindow = INDEX_CALLBACK_HOOK + "on S0 as s0 select * from S1Window as s1 " +
                                 "where P00 in (P10, P11) and P01 in (P12, P13)";
            var stmtNamedWindow = epService.EPAdministrator.CreateEPL(eplNamedWindow);
            stmtNamedWindow.Events += listener.Update;

            var onExprNamedWindow = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(
                typeof(SubordInKeywordMultiTableLookupStrategyFactory).Name, onExprNamedWindow.TableLookupStrategy);

            TryAssertionMultiIdx(epService, listener);

            // assert table
            epService.EPAdministrator.CreateEPL(
                "create table S1Table("
                + "Id int primary key, "
                + "P10 string primary key, "
                + "P11 string primary key, "
                + "P12 string primary key, "
                + "P13 string primary key)"
                );
            epService.EPAdministrator.CreateEPL("insert into S1Table select * from S1");
            epService.EPAdministrator.CreateEPL("create index S1Idx1 on S1Table(P10)");
            epService.EPAdministrator.CreateEPL("create index S1Idx2 on S1Table(P11)");
            epService.EPAdministrator.CreateEPL("create index S1Idx3 on S1Table(P12)");
            epService.EPAdministrator.CreateEPL("create index S1Idx4 on S1Table(P13)");

            var eplTable = INDEX_CALLBACK_HOOK + "on S0 as s0 select * from S1Table as s1 " +
                           "where P00 in (P10, P11) and P01 in (P12, P13)";
            var stmtTable = epService.EPAdministrator.CreateEPL(eplTable);
            stmtTable.Events += listener.Update;

            var onExprTable = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(
                typeof(SubordInKeywordMultiTableLookupStrategyFactory).Name, onExprTable.TableLookupStrategy);

            TryAssertionMultiIdx(epService, listener);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionMultiIdxSubquery(EPServiceProvider epService)
        {
            var epl = INDEX_CALLBACK_HOOK + "select s0.Id as c0," +
                      "(select * from S1#keepall as s1 " +
                      "  where s0.P00 in (s1.P10, s1.P11) and s0.P01 in (s1.P12, s1.P13))" +
                      ".selectFrom(a=>S1.Id) as c1 " +
                      "from S0 as s0";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(typeof(SubordInKeywordMultiTableLookupStrategyFactory).Name, subquery.TableLookupStrategy);

            // single row tests
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "a", "b", "c", "d"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "x"));
            AssertSubqueryC0C1(listener, 1, null);

            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x", "c"));
            AssertSubqueryC0C1(listener, 2, null);

            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "a", "c"));
            AssertSubqueryC0C1(listener, 3, new int?[] {101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "b", "d"));
            AssertSubqueryC0C1(listener, 4, new int?[] {101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "a", "d"));
            AssertSubqueryC0C1(listener, 5, new int?[] {101});

            // 2-row tests
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "a1", "a", "d1", "d"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "a", "x"));
            AssertSubqueryC0C1(listener, 10, null);

            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "x", "c"));
            AssertSubqueryC0C1(listener, 11, null);

            epService.EPRuntime.SendEvent(new SupportBean_S0(12, "a", "c"));
            AssertSubqueryC0C1(listener, 12, new int?[] {101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(13, "a", "d"));
            AssertSubqueryC0C1(listener, 13, new int?[] {101, 102});

            epService.EPRuntime.SendEvent(new SupportBean_S0(14, "a1", "d"));
            AssertSubqueryC0C1(listener, 14, new int?[] {102});

            epService.EPRuntime.SendEvent(new SupportBean_S0(15, "a", "d1"));
            AssertSubqueryC0C1(listener, 15, new int?[] {102});

            // 3-row tests
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "a", "a2", "d", "d2"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "a", "c"));
            AssertSubqueryC0C1(listener, 20, new int?[] {101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(21, "a", "d"));
            AssertSubqueryC0C1(listener, 21, new int?[] {101, 102, 103});

            epService.EPRuntime.SendEvent(new SupportBean_S0(22, "a2", "d"));
            AssertSubqueryC0C1(listener, 22, new int?[] {103});

            epService.EPRuntime.SendEvent(new SupportBean_S0(23, "a", "d2"));
            AssertSubqueryC0C1(listener, 23, new int?[] {103});

            stmt.Dispose();

            // test coercion absence - types the same
            var eplCoercion = INDEX_CALLBACK_HOOK + "select *," +
                              "(select * from S0#keepall as s0 where sb.LongPrimitive in (Id)) from SupportBean as sb";
            stmt = epService.EPAdministrator.CreateEPL(eplCoercion);
            var subqueryCoercion = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(
                typeof(SubordFullTableScanLookupStrategyFactory).Name, subqueryCoercion.TableLookupStrategy);
            stmt.Dispose();
        }

        private void RunAssertionSingleIdxMultipleInAndMultirow(EPServiceProvider epService)
        {
            // assert join
            SupportQueryPlanIndexHook.Reset();
            var epl = INDEX_CALLBACK_HOOK + "select * from S0#keepall as s0, S1 as s1 unidirectional " +
                      "where P00 in (P10, P11) and P01 in (P12, P13)";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[0].Items;
            Assert.AreEqual("[P00]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

            TryAssertionSingleIdx(epService, listener);
            epService.EPAdministrator.DestroyAllStatements();

            // assert named window
            epService.EPAdministrator.CreateEPL("create window S0Window#keepall as S0");
            epService.EPAdministrator.CreateEPL("insert into S0Window select * from S0");

            var eplNamedWindow = INDEX_CALLBACK_HOOK + "on S1 as s1 select * from S0Window as s0 " +
                                 "where P00 in (P10, P11) and P01 in (P12, P13)";
            var stmtNamedWindow = epService.EPAdministrator.CreateEPL(eplNamedWindow);
            stmtNamedWindow.Events += listener.Update;

            var onExprNamedWindow = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(
                typeof(SubordInKeywordSingleTableLookupStrategyFactory).Name, onExprNamedWindow.TableLookupStrategy);

            TryAssertionSingleIdx(epService, listener);

            // assert table
            epService.EPAdministrator.CreateEPL(
                "create table S0Table(Id int primary key, P00 string primary key, P01 string primary key, P02 string primary key, P03 string primary key)");
            epService.EPAdministrator.CreateEPL("insert into S0Table select * from S0");
            epService.EPAdministrator.CreateEPL("create index S0Idx1 on S0Table(P00)");
            epService.EPAdministrator.CreateEPL("create index S0Idx2 on S0Table(P01)");

            var eplTable = INDEX_CALLBACK_HOOK + "on S1 as s1 select * from S0Table as s0 " +
                           "where P00 in (P10, P11) and P01 in (P12, P13)";
            var stmtTable = epService.EPAdministrator.CreateEPL(eplTable);
            stmtTable.Events += listener.Update;

            var onExprTable = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(
                typeof(SubordInKeywordSingleTableLookupStrategyFactory).Name, onExprTable.TableLookupStrategy);

            TryAssertionSingleIdx(epService, listener);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSingleIdxSubquery(EPServiceProvider epService)
        {
            SupportQueryPlanIndexHook.Reset();
            var epl = INDEX_CALLBACK_HOOK + "select s1.Id as c0," +
                      "(select * from S0#keepall as s0 " +
                      "  where s0.P00 in (s1.P10, s1.P11) and s0.P01 in (s1.P12, s1.P13))" +
                      ".selectFrom(a=>S0.Id) as c1 " +
                      " from S1 as s1";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(typeof(SubordInKeywordSingleTableLookupStrategyFactory).Name, subquery.TableLookupStrategy);

            // single row tests
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "a", "c"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "a1", "b", "c", "d"));
            AssertSubqueryC0C1(listener, 1, null);

            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "a", "b", "x", "d"));
            AssertSubqueryC0C1(listener, 2, null);

            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "a", "b", "c", "d"));
            AssertSubqueryC0C1(listener, 3, new int?[] {100});

            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "x", "a", "x", "c"));
            AssertSubqueryC0C1(listener, 4, new int?[] {100});

            // 2-rows available tests
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "a", "d"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "a1", "b", "c", "d"));
            AssertSubqueryC0C1(listener, 10, null);

            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "a", "b", "x", "c1"));
            AssertSubqueryC0C1(listener, 11, null);

            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "a", "b", "c", "d"));
            AssertSubqueryC0C1(listener, 12, new int?[] {100, 101});

            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "x", "a", "x", "c"));
            AssertSubqueryC0C1(listener, 13, new int?[] {100});

            epService.EPRuntime.SendEvent(new SupportBean_S1(14, "x", "a", "d", "x"));
            AssertSubqueryC0C1(listener, 14, new int?[] {101});

            // 3-rows available tests
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "b", "c"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "a1", "b", "c1", "d"));
            AssertSubqueryC0C1(listener, 20, null);

            epService.EPRuntime.SendEvent(new SupportBean_S1(21, "a", "b", "x", "c1"));
            AssertSubqueryC0C1(listener, 21, null);

            epService.EPRuntime.SendEvent(new SupportBean_S1(22, "a", "b", "c", "d"));
            AssertSubqueryC0C1(listener, 22, new int?[] {100, 101, 102});

            epService.EPRuntime.SendEvent(new SupportBean_S1(23, "b", "a", "x", "c"));
            AssertSubqueryC0C1(listener, 23, new int?[] {100, 102});

            epService.EPRuntime.SendEvent(new SupportBean_S1(24, "b", "a", "d", "c"));
            AssertSubqueryC0C1(listener, 24, new int?[] {100, 101, 102});

            epService.EPRuntime.SendEvent(new SupportBean_S1(25, "b", "x", "x", "c"));
            AssertSubqueryC0C1(listener, 25, new int?[] {102});

            stmt.Dispose();

            // test coercion absence - types the same
            var eplCoercion = INDEX_CALLBACK_HOOK + "select *," +
                              "(select * from SupportBean#keepall as sb where sb.LongPrimitive in (s0.Id)) from S0 as s0";
            stmt = epService.EPAdministrator.CreateEPL(eplCoercion);
            var subqueryCoercion = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(
                typeof(SubordFullTableScanLookupStrategyFactory).Name, subqueryCoercion.TableLookupStrategy);
            stmt.Dispose();
        }

        private void TryAssertionSingleIdx(EPServiceProvider epService, SupportUpdateListener listener)
        {
            var fields = "s0.Id,s1.Id".Split(',');

            // single row tests
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "a", "c"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a1", "b", "c", "d"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a", "b", "x", "d"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "a", "b", "c", "d"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {100, 1}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "x", "a", "x", "c"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {100, 2}});

            // 2-rows available tests
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "a", "d"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a1", "b", "c", "d"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a", "b", "x", "c1"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "a", "b", "c", "d"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {100, 10}, new object[] {101, 10}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "x", "a", "x", "c"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {100, 11}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "x", "a", "d", "x"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {101, 12}});

            // 3-rows available tests
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "b", "c"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a1", "b", "c1", "d"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a", "b", "x", "c1"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "a", "b", "c", "d"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new[] {new object[] {100, 20}, new object[] {101, 20}, new object[] {102, 20}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(21, "b", "a", "x", "c"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {100, 21}, new object[] {102, 21}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(22, "b", "a", "d", "c"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields,
                new[] {new object[] {100, 22}, new object[] {101, 22}, new object[] {102, 22}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(23, "b", "x", "x", "c"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {102, 23}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionMultiIdx(EPServiceProvider epService, SupportUpdateListener listener)
        {
            var fields = "s0.Id,s1.Id".Split(',');

            // single row tests
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "a", "b", "c", "d"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "a", "x"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "x", "c"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "c"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {1, 101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "b", "d"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {2, 101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "a", "d"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {3, 101});

            // 2-row tests
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "a1", "a", "d1", "d"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "a", "x"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "x", "c"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "a", "c"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {10, 101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "a", "d"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {11, 101}, new object[] {11, 102}});

            epService.EPRuntime.SendEvent(new SupportBean_S0(12, "a1", "d"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {12, 102}});

            epService.EPRuntime.SendEvent(new SupportBean_S0(13, "a", "d1"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {13, 102}});

            // 3-row tests
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "a", "a2", "d", "d2"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "a", "c"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {20, 101});

            epService.EPRuntime.SendEvent(new SupportBean_S0(21, "a", "d"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields,
                new[] {new object[] {21, 101}, new object[] {21, 102}, new object[] {21, 103}});

            epService.EPRuntime.SendEvent(new SupportBean_S0(22, "a2", "d"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {22, 103}});

            epService.EPRuntime.SendEvent(new SupportBean_S0(23, "a", "d2"));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {23, 103}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSingleIdxConstants(EPServiceProvider epService)
        {
            SupportQueryPlanIndexHook.Reset();
            var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
                      "where P10 in ('a', 'b')";
            var fields = "s0.Id,s1.Id".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
            Assert.AreEqual("[P10]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "x"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "a"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {1, 101}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "b"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {2, 101}, new object[] {2, 102}});

            stmt.Dispose();
        }

        private void RunAssertionMultiIdxConstants(EPServiceProvider epService)
        {
            SupportQueryPlanIndexHook.Reset();
            var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
                      "where 'a' in (P10, P11)";
            var fields = "s0.Id,s1.Id".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
            Assert.AreEqual("[P10][P11]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "x", "y"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "x", "a"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {1, 101}});

            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "b", "a"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {2, 101}, new object[] {2, 102}});

            stmt.Dispose();
        }

        private void RunAssertionQueryPlan3Stream(EPServiceProvider epService)
        {
            var epl = "select * from S0 as s0 unidirectional, S1#keepall, S2#keepall ";

            // 3-stream join with in-multiindex directional
            var planInMidx = new InKeywordTableLookupPlanMultiIdx(
                0, 1, GetIndexKeys("i1a", "i1b"), SupportExprNodeFactory.MakeIdentExprNode("P00"));
            TryAssertion(
                epService, epl + " where P00 in (P10, P11)",
                SupportQueryPlanBuilder.Start(3)
                    .AddIndexHashSingleNonUnique(1, "i1a", "P10")
                    .AddIndexHashSingleNonUnique(1, "i1b", "P11")
                    .SetIndexFullTableScan(2, "i2")
                    .SetLookupPlanInstruction(
                        0, "s0", new[]
                        {
                            new LookupInstructionPlan(
                                0, "s0", new[] {1},
                                new TableLookupPlan[] {planInMidx}, null, new bool[3]),
                            new LookupInstructionPlan(
                                0, "s0", new[] {2},
                                new TableLookupPlan[] {new FullTableScanLookupPlan(1, 2, GetIndexKey("i2"))}, null,
                                new bool[3])
                        })
                    .Get());

            var planInMidxMulitiSrc = new InKeywordTableLookupPlanMultiIdx(
                0, 1, GetIndexKeys("i1", "i2"), SupportExprNodeFactory.MakeIdentExprNode("P00"));

            TryAssertion(
                epService, epl + " where P00 in (P10, P20)",
                SupportQueryPlanBuilder.Start(3)
                    .SetIndexFullTableScan(1, "i1")
                    .SetIndexFullTableScan(2, "i2")
                    .SetLookupPlanInstruction(
                        0, "s0", new[]
                        {
                            new LookupInstructionPlan(
                                0, "s0", new[] {1},
                                new TableLookupPlan[] {new FullTableScanLookupPlan(0, 1, GetIndexKey("i1"))}, null,
                                new bool[3]),
                            new LookupInstructionPlan(
                                0, "s0", new[] {2},
                                new TableLookupPlan[] {new FullTableScanLookupPlan(1, 2, GetIndexKey("i2"))}, null,
                                new bool[3])
                        })
                    .Get());

            // 3-stream join with in-singleindex directional
            var planInSidx = new InKeywordTableLookupPlanSingleIdx(
                0, 1, GetIndexKey("i1"), SupportExprNodeFactory.MakeIdentExprNodes("P00", "P01"));
            TryAssertion(epService, epl + " where P10 in (P00, P01)", GetSingleIndexPlan(planInSidx));

            // 3-stream join with in-singleindex multi-sourced
            var planInSingleMultiSrc = new InKeywordTableLookupPlanSingleIdx(
                0, 1, GetIndexKey("i1"), SupportExprNodeFactory.MakeIdentExprNodes("P00"));
            TryAssertion(epService, epl + " where P10 in (P00, P20)", GetSingleIndexPlan(planInSingleMultiSrc));
        }

        private void RunAssertionQueryPlan2Stream(EPServiceProvider epService)
        {
            var epl = "select * from S0 as s0 unidirectional, S1#keepall ";
            var fullTableScan = SupportQueryPlanBuilder.Start(2)
                .SetIndexFullTableScan(1, "a")
                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("a")))
                .Get();

            // 2-stream unidirectional joins
            TryAssertion(epService, epl, fullTableScan);

            var planEquals = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P10")
                .SetLookupPlanInner(
                    0,
                    new IndexedTableLookupPlanSingle(0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeKeyed("P00")))
                .Get();
            TryAssertion(epService, epl + "where P00 = P10", planEquals);
            TryAssertion(epService, epl + "where P00 = P10 and P00 in (P11, P12, P13)", planEquals);

            var planInMultiInner = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P11")
                .AddIndexHashSingleNonUnique(1, "b", "P12")
                .SetLookupPlanInner(
                    0,
                    new InKeywordTableLookupPlanMultiIdx(
                        0, 1, GetIndexKeys("a", "b"), SupportExprNodeFactory.MakeIdentExprNode("P00")))
                .Get();
            TryAssertion(epService, epl + "where P00 in (P11, P12)", planInMultiInner);
            TryAssertion(epService, epl + "where P00 = P11 or P00 = P12", planInMultiInner);

            var planInMultiOuter = SupportQueryPlanBuilder.Start(planInMultiInner)
                .SetLookupPlanOuter(
                    0,
                    new InKeywordTableLookupPlanMultiIdx(
                        0, 1, GetIndexKeys("a", "b"), SupportExprNodeFactory.MakeIdentExprNode("P00")))
                .Get();
            var eplOuterJoin = "select * from S0 as s0 unidirectional full outer join S1#keepall ";
            TryAssertion(epService, eplOuterJoin + "where P00 in (P11, P12)", planInMultiOuter);

            var planInMultiWConst = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P11")
                .AddIndexHashSingleNonUnique(1, "b", "P12")
                .SetLookupPlanInner(
                    0,
                    new InKeywordTableLookupPlanMultiIdx(
                        0, 1, GetIndexKeys("a", "b"), SupportExprNodeFactory.MakeConstExprNode("A")))
                .Get();
            TryAssertion(epService, epl + "where 'A' in (P11, P12)", planInMultiWConst);
            TryAssertion(epService, epl + "where 'A' = P11 or 'A' = P12", planInMultiWConst);

            var planInMultiWAddConst = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P12")
                .SetLookupPlanInner(
                    0,
                    new InKeywordTableLookupPlanMultiIdx(
                        0, 1, GetIndexKeys("a"), SupportExprNodeFactory.MakeConstExprNode("A")))
                .Get();
            TryAssertion(epService, epl + "where 'A' in ('B', P12)", planInMultiWAddConst);
            TryAssertion(epService, epl + "where 'A' in ('B', 'C')", fullTableScan);

            var planInSingle = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P10")
                .SetLookupPlanInner(
                    0,
                    new InKeywordTableLookupPlanSingleIdx(
                        0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeIdentExprNodes("P00", "P01")))
                .Get();
            TryAssertion(epService, epl + "where P10 in (P00, P01)", planInSingle);

            var planInSingleWConst = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P10")
                .SetLookupPlanInner(
                    0,
                    new InKeywordTableLookupPlanSingleIdx(
                        0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeConstAndIdentNode("A", "P01")))
                .Get();
            TryAssertion(epService, epl + "where P10 in ('A', P01)", planInSingleWConst);

            var planInSingleJustConst = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(1, "a", "P10")
                .SetLookupPlanInner(
                    0,
                    new InKeywordTableLookupPlanSingleIdx(
                        0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeConstAndConstNode("A", "B")))
                .Get();
            TryAssertion(epService, epl + "where P10 in ('A', 'B')", planInSingleJustConst);
        }

        private void TryAssertion(EPServiceProvider epService, string epl, QueryPlan expectedPlan)
        {
            SupportQueryPlanIndexHook.Reset();
            epl = INDEX_CALLBACK_HOOK + epl;
            epService.EPAdministrator.CreateEPL(epl);

            var actualPlan = SupportQueryPlanIndexHook.AssertJoinAndReset();
            SupportQueryPlanIndexHelper.CompareQueryPlans(expectedPlan, actualPlan);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void AssertSubqueryC0C1(SupportUpdateListener listener, int c0, int?[] c1)
        {
            var @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(c0, @event.Get("c0"));
            var c1Coll = @event.Get("c1").Unwrap<int?>(true);
            EPAssertionUtil.AssertEqualsAnyOrder(c1, c1Coll);
        }

        private QueryPlan GetSingleIndexPlan(InKeywordTableLookupPlanSingleIdx plan)
        {
            return SupportQueryPlanBuilder.Start(3)
                .AddIndexHashSingleNonUnique(1, "i1", "P10")
                .SetIndexFullTableScan(2, "i2")
                .SetLookupPlanInstruction(
                    0, "s0", new[]
                    {
                        new LookupInstructionPlan(
                            0, "s0", new[] {1},
                            new TableLookupPlan[] {plan}, null, new bool[3]),
                        new LookupInstructionPlan(
                            0, "s0", new[] {2},
                            new TableLookupPlan[] {new FullTableScanLookupPlan(1, 2, GetIndexKey("i2"))}, null,
                            new bool[3])
                    })
                .Get();
        }

        private static TableLookupIndexReqKey[] GetIndexKeys(params string[] names)
        {
            var keys = new TableLookupIndexReqKey[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                keys[i] = new TableLookupIndexReqKey(names[i]);
            }

            return keys;
        }

        private static TableLookupIndexReqKey GetIndexKey(string name)
        {
            return new TableLookupIndexReqKey(name);
        }
    }
} // end of namespace