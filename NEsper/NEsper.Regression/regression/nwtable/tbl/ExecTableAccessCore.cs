///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    using Map = IDictionary<string, object>;

    public class ExecTableAccessCore : RegressionExecution
    {

        public override void Run(EPServiceProvider epService)
        {
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }

            RunAssertionIntegerIndexedPropertyLookAlike(epService);
            RunAssertionFilterBehavior(epService);
            RunAssertionExprSelectClauseRenderingUnnamedCol(epService);
            RunAssertionTopLevelReadGrouped2Keys(epService);
            RunAssertionTopLevelReadUnGrouped(epService);
            RunAssertionExpressionAliasAndDecl(epService);
            RunAssertionGroupedTwoKeyNoContext(epService);
            RunAssertionGroupedThreeKeyNoContext(epService);
            RunAssertionGroupedSingleKeyNoContext(epService);
            RunAssertionUngroupedWContext(epService);
            RunAssertionOrderOfAggregationsAndPush(epService);
            RunAssertionMultiStmtContributing(epService);
            RunAssertionGroupedMixedMethodAndAccess(epService);
            RunAssertionNamedWindowAndFireAndForget(epService);
            RunAssertionSubquery(epService);
            RunAssertionOnMergeExpressions(epService);
        }

        private void RunAssertionIntegerIndexedPropertyLookAlike(EPServiceProvider epService)
        {
            TryAssertionIntegerIndexedPropertyLookAlike(epService, false);
            TryAssertionIntegerIndexedPropertyLookAlike(epService, true);
        }

        private void TryAssertionIntegerIndexedPropertyLookAlike(EPServiceProvider epService, bool soda)
        {
            var eplDeclare = "create table varaggIIP (key int primary key, myevents window(*) @Type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            var eplInto =
                "into table varaggIIP select window(*) as myevents from SupportBean#length(3) group by IntPrimitive";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplInto);

            var eplSelect =
                "select varaggIIP[1] as c0, varaggIIP[1].myevents as c1, varaggIIP[1].myevents.last(*) as c2 from SupportBean_S0";
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplSelect);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var e1 = MakeSendBean(epService, "E1", 1, 10L);
            var e2 = MakeSendBean(epService, "E2", 1, 20L);

            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            AssertIntegerIndexed(listener.AssertOneGetNewAndReset(), new SupportBean[] {e1, e2});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void AssertIntegerIndexed(EventBean @event, SupportBean[] events)
        {
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c0.myevents"));
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c1"));
            Assert.AreSame(events[events.Length - 1], @event.Get("c2"));
        }

        private void RunAssertionFilterBehavior(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create table varaggFB (total count(*))");
            epService.EPAdministrator.CreateEPL("into table varaggFB select count(*) as total from SupportBean_S0");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean(varaggFB.total = IntPrimitive)").Events +=
                listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(0));

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            epService.EPRuntime.SendEvent(new SupportBean_S0(0));

            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionExprSelectClauseRenderingUnnamedCol(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create table varaggESC (" +
                "key string primary key, theEvents window(*) @Type(SupportBean))");

            var stmtSelect = epService.EPAdministrator.CreateEPL(
                "select " +
                "varaggESC.keys()," +
                "varaggESC[p00].theEvents," +
                "varaggESC[p00]," +
                "varaggESC[p00].theEvents.last(*)," +
                "varaggESC[p00].theEvents.window(*).take(1) from SupportBean_S0");

            var expectedAggType = new object[][] {
                new object[] {"varaggESC.keys()", typeof(object[])},
                new object[] {"varaggESC[p00].theEvents", typeof(SupportBean[])},
                new object[] {"varaggESC[p00]", typeof(Map)},
                new object[] {"varaggESC[p00].theEvents.last(*)", typeof(SupportBean)},
                new object[] {"varaggESC[p00].theEvents.window(*).take(1)", typeof(ICollection<SupportBean>)},
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType, stmtSelect.EventType, SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionTopLevelReadGrouped2Keys(EPServiceProvider epService)
        {
            TryAssertionTopLevelReadGrouped2Keys(epService, false);
            TryAssertionTopLevelReadGrouped2Keys(epService, true);
        }

        private void TryAssertionTopLevelReadGrouped2Keys(EPServiceProvider epService, bool soda)
        {
            SupportModelHelper.CreateByCompileOrParse(
                epService, soda, "create objectarray schema MyEventOA as (c0 int, c1 string, c2 int)");
            SupportModelHelper.CreateByCompileOrParse(
                epService, soda, "create table windowAndTotalTLP2K (" +
                                 "keyi int primary key, keys string primary key, thewindow window(*) @Type('MyEventOA'), thetotal sum(int))");
            SupportModelHelper.CreateByCompileOrParse(
                epService, soda, "into table windowAndTotalTLP2K " +
                                 "select window(*) as thewindow, sum(c2) as thetotal from MyEventOA#length(2) group by c0, c1");

            var stmtSelect = SupportModelHelper.CreateByCompileOrParse(
                epService, soda, "select windowAndTotalTLP2K[id,p00] as val0 from SupportBean_S0");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            AssertTopLevelTypeInfo(stmtSelect);

            var e1 = new object[] {10, "G1", 100};
            epService.EPRuntime.SendEvent(e1, "MyEventOA");

            var fieldsInner = "thewindow,thetotal".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "G1"));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[][] {e1}, 100);

            var e2 = new object[] {20, "G2", 200};
            epService.EPRuntime.SendEvent(e2, "MyEventOA");

            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "G2"));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[][] {e2}, 200);

            var e3 = new object[] {20, "G2", 300};
            epService.EPRuntime.SendEvent(e3, "MyEventOA");

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "G1"));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, null, null);
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "G2"));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[][] {e2, e3}, 500);

            // test typable output
            stmtSelect.Dispose();
            var stmtConvert = epService.EPAdministrator.CreateEPL(
                "insert into OutStream select windowAndTotalTLP2K[20, 'G2'] as val0 from SupportBean_S0");
            stmtConvert.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0.thewindow,val0.thetotal".Split(','),
                new object[] {new object[][] {e2, e3}, 500});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionTopLevelReadUnGrouped(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(AggBean));
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventOATLRU(c0 int)");
            epService.EPAdministrator.CreateEPL(
                "create table windowAndTotalTLRUG (" +
                "thewindow window(*) @Type(MyEventOATLRU), thetotal sum(int))");
            epService.EPAdministrator.CreateEPL(
                "into table windowAndTotalTLRUG " +
                "select window(*) as thewindow, sum(c0) as thetotal from MyEventOATLRU#length(2)");

            var stmt =
                epService.EPAdministrator.CreateEPL("select windowAndTotalTLRUG as val0 from SupportBean_S0");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var e1 = new object[] {10};
            epService.EPRuntime.SendEvent(e1, "MyEventOATLRU");

            var fieldsInner = "thewindow,thetotal".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[][] {e1}, 10);

            var e2 = new object[] {20};
            epService.EPRuntime.SendEvent(e2, "MyEventOATLRU");

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[][] {e1, e2}, 30);

            var e3 = new object[] {30};
            epService.EPRuntime.SendEvent(e3, "MyEventOATLRU");

            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsMap(
                (Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[][] {e2, e3}, 50);

            // test typable output
            stmt.Dispose();
            var stmtConvert = epService.EPAdministrator.CreateEPL(
                "insert into AggBean select windowAndTotalTLRUG as val0 from SupportBean_S0");
            stmtConvert.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0.thewindow,val0.thetotal".Split(','),
                new object[] {new object[][] {e2, e3}, 50});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionExpressionAliasAndDecl(EPServiceProvider epService)
        {
            TryAssertionIntoTableFromExpression(epService);

            TryAssertionExpressionHasTableAccess(epService);

            TryAssertionSubqueryWithExpressionHasTableAccess(epService);
        }

        private void TryAssertionSubqueryWithExpressionHasTableAccess(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create table MyTableTwo(TheString string primary key, IntPrimitive int)");
            epService.EPAdministrator.CreateEPL(
                "create expression getMyValue{o => (select MyTableTwo[o.p00].IntPrimitive from SupportBean_S1#lastevent)}");
            epService.EPAdministrator.CreateEPL(
                "insert into MyTableTwo select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0");

            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("s0").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S1(1000));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));

            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] {2});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionExpressionHasTableAccess(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create table MyTableOne(TheString string primary key, IntPrimitive int)");
            epService.EPAdministrator.CreateEPL("create expression getMyValue{o => MyTableOne[o.p00].IntPrimitive}");
            epService.EPAdministrator.CreateEPL(
                "insert into MyTableOne select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0");

            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("s0").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));

            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] {2});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionIntoTableFromExpression(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create expression sumi {a -> sum(IntPrimitive)}");
            epService.EPAdministrator.CreateEPL("create expression sumd alias for {sum(DoublePrimitive)}");
            epService.EPAdministrator.CreateEPL(
                "create table varaggITFE (" +
                "sumi sum(int), sumd sum(double), sumf sum(float), suml sum(long))");
            epService.EPAdministrator.CreateEPL(
                "expression suml alias for {sum(LongPrimitive)} " +
                "into table varaggITFE " +
                "select suml, sum(FloatPrimitive) as sumf, sumd, sumi(sb) from SupportBean as sb");

            MakeSendBean(epService, "E1", 10, 100L, 1000d, 10000f);

            var fields = "varaggITFE.sumi,varaggITFE.sumd,varaggITFE.sumf,varaggITFE.suml";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select " + fields + " from SupportBean_S0").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields.Split(','), new object[] {10, 1000d, 10000f, 100L});

            MakeSendBean(epService, "E1", 11, 101L, 1001d, 10001f);

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields.Split(','), new object[] {21, 2001d, 20001f, 201L});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupedTwoKeyNoContext(EPServiceProvider epService)
        {
            var eplDeclare =
                "create table varTotalG2K (key0 string primary key, key1 int primary key, total sum(long), cnt count(*))";
            epService.EPAdministrator.CreateEPL(eplDeclare);

            var eplBind =
                "into table varTotalG2K " +
                "select sum(LongPrimitive) as total, count(*) as cnt " +
                "from SupportBean group by TheString, IntPrimitive";
            epService.EPAdministrator.CreateEPL(eplBind);

            var eplUse = "select varTotalG2K[p00, id].total as c0, varTotalG2K[p00, id].cnt as c1 from SupportBean_S0";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplUse).Events += listener.Update;

            MakeSendBean(epService, "E1", 10, 100);

            var fields = "c0,c1".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {100L, 1L});
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupedThreeKeyNoContext(EPServiceProvider epService)
        {
            var eplDeclare = "create table varTotalG3K (key0 string primary key, key1 int primary key," +
                             "key2 long primary key, total sum(double), cnt count(*))";
            epService.EPAdministrator.CreateEPL(eplDeclare);

            var eplBind = "into table varTotalG3K " +
                          "select sum(DoublePrimitive) as total, count(*) as cnt " +
                          "from SupportBean group by TheString, IntPrimitive, LongPrimitive";
            epService.EPAdministrator.CreateEPL(eplBind);

            var fields = "c0,c1".Split(',');
            var eplUse =
                "select varTotalG3K[p00, id, 100L].total as c0, varTotalG3K[p00, id, 100L].cnt as c1 from SupportBean_S0";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplUse).Events += listener.Update;

            MakeSendBean(epService, "E1", 10, 100, 1000);

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {1000.0, 1L});

            MakeSendBean(epService, "E1", 10, 100, 1001);

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {2001.0, 2L});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGroupedSingleKeyNoContext(EPServiceProvider epService)
        {
            TryAssertionGroupedSingleKeyNoContext(epService, false);
            TryAssertionGroupedSingleKeyNoContext(epService, true);
        }

        private void TryAssertionGroupedSingleKeyNoContext(EPServiceProvider epService, bool soda)
        {
            var eplDeclare = "create table varTotalG1K (key string primary key, total sum(int))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            var eplBind = "into table varTotalG1K " +
                          "select TheString, sum(IntPrimitive) as total from SupportBean group by TheString";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBind);

            var eplUse = "select p00 as c0, varTotalG1K[p00].total as c1 from SupportBean_S0";
            var listener = new SupportUpdateListener();
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplUse).Events += listener.Update;

            TryAssertionTopLevelSingle(epService, listener);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionUngroupedWContext(EPServiceProvider epService)
        {
            var eplPart =
                "create context PartitionedByString partition by TheString from SupportBean, p00 from SupportBean_S0;\n" +
                "context PartitionedByString create table varTotalUG (total sum(int));\n" +
                "context PartitionedByString into table varTotalUG select sum(IntPrimitive) as total from SupportBean;\n" +
                "@Name('L') context PartitionedByString select p00 as c0, varTotalUG.total as c1 from SupportBean_S0;\n";
            var result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplPart);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("L").Events += listener.Update;

            TryAssertionTopLevelSingle(epService, listener);

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }

        private void RunAssertionOrderOfAggregationsAndPush(EPServiceProvider epService)
        {
            TryAssertionOrderOfAggs(epService, true);
            TryAssertionOrderOfAggs(epService, false);
        }

        private void RunAssertionMultiStmtContributing(EPServiceProvider epService)
        {
            TryAssertionMultiStmtContributingDifferentAggs(epService, false);
            TryAssertionMultiStmtContributingDifferentAggs(epService, true);

            // contribute to the same aggregation
            epService.EPAdministrator.CreateEPL("create table sharedagg (total sum(int))");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "into table sharedagg " +
                "select p00 as c0, sum(id) as total from SupportBean_S0").Events += listener.Update;
            epService.EPAdministrator.CreateEPL(
                "into table sharedagg " +
                "select p10 as c0, sum(id) as total from SupportBean_S1").Events += listener.Update;
            epService.EPAdministrator.CreateEPL("select TheString as c0, sharedagg.total as total from SupportBean")
                .Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "A"));
            AssertMultiStmtContributingTotal(epService, listener, "A", 10);

            epService.EPRuntime.SendEvent(new SupportBean_S1(-5, "B"));
            AssertMultiStmtContributingTotal(epService, listener, "B", 5);

            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            AssertMultiStmtContributingTotal(epService, listener, "C", 7);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void AssertMultiStmtContributingTotal(
            EPServiceProvider epService, SupportUpdateListener listener, string c0, int total)
        {
            var fields = "c0,total".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {c0, total});

            epService.EPRuntime.SendEvent(new SupportBean(c0, 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {c0, total});
        }

        private void TryAssertionMultiStmtContributingDifferentAggs(EPServiceProvider epService, bool grouped)
        {
            var eplDeclare = "create table varaggMSC (" +
                             (grouped ? "key string primary key," : "") +
                             "s0sum sum(int), s0cnt count(*), s0win window(*) @Type(SupportBean_S0)," +
                             "s1sum sum(int), s1cnt count(*), s1win window(*) @Type(SupportBean_S1)" +
                             ")";
            epService.EPAdministrator.CreateEPL(eplDeclare);

            var fieldsSelect = "c0,c1,c2,c3,c4,c5".Split(',');
            var eplSelectUngrouped = "select varaggMSC.s0sum as c0, varaggMSC.s0cnt as c1," +
                                     "varaggMSC.s0win as c2, varaggMSC.s1sum as c3, varaggMSC.s1cnt as c4," +
                                     "varaggMSC.s1win as c5 from SupportBean";
            var eplSelectGrouped = "select varaggMSC[TheString].s0sum as c0, varaggMSC[TheString].s0cnt as c1," +
                                   "varaggMSC[TheString].s0win as c2, varaggMSC[TheString].s1sum as c3, varaggMSC[TheString].s1cnt as c4," +
                                   "varaggMSC[TheString].s1win as c5 from SupportBean";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(grouped ? eplSelectGrouped : eplSelectUngrouped).Events +=
                listener.Update;

            var listenerOne = new SupportUpdateListener();
            var fieldsOne = "s0sum,s0cnt,s0win".Split(',');
            var eplBindOne =
                "into table varaggMSC select sum(id) as s0sum, count(*) as s0cnt, window(*) as s0win from SupportBean_S0#length(2) " +
                (grouped ? "group by p00" : "");
            epService.EPAdministrator.CreateEPL(eplBindOne).Events += listenerOne.Update;

            var listenerTwo = new SupportUpdateListener();
            var fieldsTwo = "s1sum,s1cnt,s1win".Split(',');
            var eplBindTwo =
                "into table varaggMSC select sum(id) as s1sum, count(*) as s1cnt, window(*) as s1win from SupportBean_S1#length(2) " +
                (grouped ? "group by p10" : "");
            epService.EPAdministrator.CreateEPL(eplBindTwo).Events += listenerTwo.Update;

            // contribute S1
            var s1_1 = MakeSendS1(epService, 10, "G1");
            EPAssertionUtil.AssertProps(
                listenerTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {10, 1L, new object[] {s1_1}});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {null, 0L, null, 10, 1L, new object[] {s1_1}});

            // contribute S0
            var s0_1 = MakeSendS0(epService, 20, "G1");
            EPAssertionUtil.AssertProps(
                listenerOne.AssertOneGetNewAndReset(), fieldsOne, new object[] {20, 1L, new object[] {s0_1}});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {20, 1L, new object[] {s0_1}, 10, 1L, new object[] {s1_1}});

            // contribute S1 and S0
            var s1_2 = MakeSendS1(epService, 11, "G1");
            EPAssertionUtil.AssertProps(
                listenerTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {21, 2L, new object[] {s1_1, s1_2}});
            var s0_2 = MakeSendS0(epService, 21, "G1");
            EPAssertionUtil.AssertProps(
                listenerOne.AssertOneGetNewAndReset(), fieldsOne, new object[] {41, 2L, new object[] {s0_1, s0_2}});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {41, 2L, new object[] {s0_1, s0_2}, 21, 2L, new object[] {s1_1, s1_2}});

            // contribute S1 and S0 (leave)
            var s1_3 = MakeSendS1(epService, 12, "G1");
            EPAssertionUtil.AssertProps(
                listenerTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {23, 2L, new object[] {s1_2, s1_3}});
            var s0_3 = MakeSendS0(epService, 22, "G1");
            EPAssertionUtil.AssertProps(
                listenerOne.AssertOneGetNewAndReset(), fieldsOne, new object[] {43, 2L, new object[] {s0_2, s0_3}});

            epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {43, 2L, new object[] {s0_2, s0_3}, 23, 2L, new object[] {s1_2, s1_3}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggMSC__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggMSC__public", false);
        }

        private SupportBean_S1 MakeSendS1(EPServiceProvider epService, int id, string p10)
        {
            var bean = new SupportBean_S1(id, p10);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private SupportBean_S0 MakeSendS0(EPServiceProvider epService, int id, string p00)
        {
            var bean = new SupportBean_S0(id, p00);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private void TryAssertionOrderOfAggs(EPServiceProvider epService, bool ungrouped)
        {
            var eplDeclare = "create table varaggOOA (" + (ungrouped ? "" : "key string primary key, ") +
                             "sumint sum(int), " +
                             "sumlong sum(long), " +
                             "mysort sorted(IntPrimitive) @Type(SupportBean)," +
                             "mywindow window(*) @Type(SupportBean)" +
                             ")";
            epService.EPAdministrator.CreateEPL(eplDeclare);

            var fieldsTable = "sumint,sumlong,mywindow,mysort".Split(',');
            var listenerIntoTable = new SupportUpdateListener();
            var eplSelect = "into table varaggOOA select " +
                            "sum(LongPrimitive) as sumlong, " +
                            "sum(IntPrimitive) as sumint, " +
                            "window(*) as mywindow," +
                            "sorted() as mysort " +
                            "from SupportBean#length(2) " +
                            (ungrouped ? "" : "group by TheString ");
            epService.EPAdministrator.CreateEPL(eplSelect).Events += listenerIntoTable.Update;

            var fieldsSelect = "c0,c1,c2,c3".Split(',');
            var groupKey = ungrouped ? "" : "['E1']";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select " +
                "varaggOOA" + groupKey + ".sumint as c0, " +
                "varaggOOA" + groupKey + ".sumlong as c1," +
                "varaggOOA" + groupKey + ".mywindow as c2," +
                "varaggOOA" + groupKey + ".mysort as c3 from SupportBean_S0").Events += listener.Update;

            var e1 = MakeSendBean(epService, "E1", 10, 100);
            EPAssertionUtil.AssertProps(
                listenerIntoTable.AssertOneGetNewAndReset(), fieldsTable,
                new object[] {10, 100L, new object[] {e1}, new object[] {e1}});

            var e2 = MakeSendBean(epService, "E1", 5, 50);
            EPAssertionUtil.AssertProps(
                listenerIntoTable.AssertOneGetNewAndReset(), fieldsTable,
                new object[] {15, 150L, new object[] {e1, e2}, new object[] {e2, e1}});

            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {15, 150L, new object[] {e1, e2}, new object[] {e2, e1}});

            var e3 = MakeSendBean(epService, "E1", 12, 120);
            EPAssertionUtil.AssertProps(
                listenerIntoTable.AssertOneGetNewAndReset(), fieldsTable,
                new object[] {17, 170L, new object[] {e2, e3}, new object[] {e2, e3}});

            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {17, 170L, new object[] {e2, e3}, new object[] {e2, e3}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggOOA__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggOOA__public", false);
        }

        private void RunAssertionGroupedMixedMethodAndAccess(EPServiceProvider epService)
        {
            TryAssertionGroupedMixedMethodAndAccess(epService, false);
            TryAssertionGroupedMixedMethodAndAccess(epService, true);
        }

        private void RunAssertionNamedWindowAndFireAndForget(EPServiceProvider epService)
        {
            var epl = "create window MyWindow#length(2) as SupportBean;\n" +
                      "insert into MyWindow select * from SupportBean;\n" +
                      "create table varaggNWFAF (total sum(int));\n" +
                      "into table varaggNWFAF select sum(IntPrimitive) as total from MyWindow;\n";
            var deployment = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            var resultSelect = epService.EPRuntime.ExecuteQuery("select varaggNWFAF.total as c0 from MyWindow");
            Assert.AreEqual(10, resultSelect.Array[0].Get("c0"));

            var resultDelete = epService.EPRuntime.ExecuteQuery(
                "delete from MyWindow where varaggNWFAF.total = IntPrimitive");
            Assert.AreEqual(1, resultDelete.Array.Length);

            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            var resultUpdate = epService.EPRuntime.ExecuteQuery(
                "update MyWindow set DoublePrimitive = 100 where varaggNWFAF.total = IntPrimitive");
            Assert.AreEqual(100d, resultUpdate.Array[0].Get("DoublePrimitive"));

            var resultInsert = epService.EPRuntime.ExecuteQuery(
                "insert into MyWindow (TheString, IntPrimitive) values ('A', varaggNWFAF.total)");
            EPAssertionUtil.AssertProps(
                resultInsert.Array[0], "TheString,IntPrimitive".Split(','), new object[] {"A", 20});

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployment.DeploymentId);
        }

        private void RunAssertionSubquery(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create table subquery_var_agg (key string primary key, total count(*))");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select (select subquery_var_agg[p00].total from SupportBean_S0#lastevent) as c0 " +
                "from SupportBean_S1").Events += listener.Update;
            epService.EPAdministrator.CreateEPL(
                "into table subquery_var_agg select count(*) as total from SupportBean group by TheString");

            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("c0"));

            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("c0"));

            Assert.NotNull(epService.EPAdministrator.DeploymentAdmin);
        }

        private void RunAssertionOnMergeExpressions(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL(
                "create table the_table (key string primary key, total count(*), value int)");
            epService.EPAdministrator.CreateEPL(
                "into table the_table select count(*) as total from SupportBean group by TheString");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 as s0 " +
                "merge the_table as tt " +
                "where s0.p00 = tt.key " +
                "when matched and the_table[s0.p00].total > 0" +
                "  then update set value = 1");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select the_table[p10].value as c0 from SupportBean_S1").Events +=
                listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));

            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "E1"));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("c0"));

            Assert.NotNull(epService.EPAdministrator.DeploymentAdmin);
        }

        private void TryAssertionGroupedMixedMethodAndAccess(EPServiceProvider epService, bool soda)
        {
            var eplDeclare = "create table varMyAgg (" +
                             "key string primary key, " +
                             "c0 count(*), " +
                             "c1 count(distinct object), " +
                             "c2 window(*) @Type('SupportBean'), " +
                             "c3 sum(long)" +
                             ")";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            var eplBind = "into table varMyAgg select " +
                          "count(*) as c0, " +
                          "count(distinct IntPrimitive) as c1, " +
                          "window(*) as c2, " +
                          "sum(LongPrimitive) as c3 " +
                          "from SupportBean#length(3) group by TheString";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplBind);

            var eplSelect = "select " +
                            "varMyAgg[p00].c0 as c0, " +
                            "varMyAgg[p00].c1 as c1, " +
                            "varMyAgg[p00].c2 as c2, " +
                            "varMyAgg[p00].c3 as c3" +
                            " from SupportBean_S0";
            var stmtSelect = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            var fields = "c0,c1,c2,c3".Split(',');

            Assert.AreEqual(typeof(long), stmtSelect.EventType.GetPropertyType("c0"));
            Assert.AreEqual(typeof(long), stmtSelect.EventType.GetPropertyType("c1"));
            Assert.AreEqual(typeof(SupportBean[]), stmtSelect.EventType.GetPropertyType("c2"));
            Assert.AreEqual(typeof(long), stmtSelect.EventType.GetPropertyType("c3"));

            var b1 = MakeSendBean(epService, "E1", 10, 100);
            var b2 = MakeSendBean(epService, "E1", 11, 101);
            var b3 = MakeSendBean(epService, "E1", 10, 102);

            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {3L, 2L, new SupportBean[] {b1, b2, b3}, 303L});

            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {null, null, null, null});

            var b4 = MakeSendBean(epService, "E2", 20, 200);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields,
                new object[] {1L, 1L, new SupportBean[] {b4}, 200L});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionTopLevelSingle(EPServiceProvider epService, SupportUpdateListener listener)
        {
            SendEventsAndAssert(epService, listener, "A", 10, "A", 10);
            SendEventsAndAssert(epService, listener, "A", 11, "A", 21);
            SendEventsAndAssert(epService, listener, "B", 20, "A", 21);
            SendEventsAndAssert(epService, listener, "B", 21, "B", 41);
            SendEventsAndAssert(epService, listener, "C", 30, "A", 21);
            SendEventsAndAssert(epService, listener, "D", 40, "C", 30);

            var fields = "c0,c1".Split(',');
            var expected = new int[] {21, 41, 30, 40};
            var count = 0;
            foreach (var p00 in "A,B,C,D".Split(',')) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {p00, expected[count]});
                count++;
            }

            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 21});
        }

        private void SendEventsAndAssert(
            EPServiceProvider epService,
            SupportUpdateListener listener, 
            string theString, 
            int intPrimitive, 
            string p00,
            int total)
        {
            var fields = "c0,c1".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {p00, total});
        }

        private SupportBean MakeSendBean(
            EPServiceProvider epService,
            string theString, 
            int intPrimitive,
            long longPrimitive)
        {
            return MakeSendBean(epService, theString, intPrimitive, longPrimitive, -1);
        }

        private SupportBean MakeSendBean(
            EPServiceProvider epService, 
            string theString, 
            int intPrimitive, 
            long longPrimitive,
            double doublePrimitive)
        {
            return MakeSendBean(epService, theString, intPrimitive, longPrimitive, doublePrimitive, -1);
        }

        private SupportBean MakeSendBean(
            EPServiceProvider epService,
            string theString, 
            int intPrimitive, 
            long longPrimitive, 
            double doublePrimitive,
            float floatPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            bean.FloatPrimitive = floatPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private void AssertTopLevelTypeInfo(EPStatement stmt)
        {
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("val0"));
            var fragType = stmt.EventType.GetFragmentType("val0");
            Assert.IsFalse(fragType.IsIndexed);
            Assert.IsFalse(fragType.IsNative);
            Assert.AreEqual(typeof(object[][]), fragType.FragmentType.GetPropertyType("thewindow"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("thetotal"));
        }

        public class AggSubBean
        {
            private int _thetotal;
            private object[][] _thewindow;

            [PropertyName("thetotal")]
            public int Thetotal {
                get => _thetotal;
                set => _thetotal = value;
            }

            [PropertyName("thewindow")]
            public object[][] Thewindow {
                get => _thewindow;
                set => _thewindow = value;
            }
        }

        public class AggBean
        {
            private AggSubBean _val0;

            [PropertyName("val0")]
            public AggSubBean Val0 {
                get => _val0;
                set => _val0 = value;
            }
        }
    }
} // end of namespace
