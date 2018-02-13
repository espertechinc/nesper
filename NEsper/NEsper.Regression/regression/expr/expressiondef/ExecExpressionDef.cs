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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expressiondef
{
    using Map = IDictionary<string, object>;

    public class ExecExpressionDef : RegressionExecution {
    
        private static readonly string NEWLINE = Environment.NewLine;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNestedExpressionMultiSubquery(epService);
            RunAssertionWildcardAndPattern(epService);
            RunAssertionSequenceAndNested(epService);
            RunAssertionCaseNewMultiReturnNoElse(epService);
            RunAssertionAnnotationOrder(epService);
            RunAssertionSubqueryMultiresult(epService);
            RunAssertionSubqueryCross(epService);
            RunAssertionSubqueryJoinSameField(epService);
            RunAssertionSubqueryCorrelated(epService);
            RunAssertionSubqueryUncorrelated(epService);
            RunAssertionSubqueryNamedWindowUncorrelated(epService);
            RunAssertionSubqueryNamedWindowCorrelated(epService);
            RunAssertionAggregationNoAccess(epService);
            RunAssertionSplitStream(epService);
            RunAssertionAggregationAccess(epService);
            RunAssertionAggregatedResult(epService);
            RunAssertionScalarReturn(epService);
            RunAssertionEventTypeAndSODA(epService);
            RunAssertionOneParameterLambdaReturn(epService);
            RunAssertionNoParameterArithmetic(epService);
            RunAssertionNoParameterVariable(epService);
            RunAssertionWhereClauseExpression(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionNestedExpressionMultiSubquery(EPServiceProvider epService) {
            var fields = "c0".Split(',');
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL("create expression F1 { (select intPrimitive from SupportBean#lastevent)}");
            epService.EPAdministrator.CreateEPL("create expression F2 { param => (select a.intPrimitive from SupportBean#unique(theString) as a where a.theString = param.theString) }");
            epService.EPAdministrator.CreateEPL("create expression F3 { s => F1()+F2(s) }");
            epService.EPAdministrator.CreateEPL("select F3(myevent) as c0 from SupportBean as myevent").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{22});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWildcardAndPattern(EPServiceProvider epService) {
            var eplNonJoin =
                    "expression abc { x => intPrimitive } " +
                            "expression def { (x, y) => x.intPrimitive * y.intPrimitive }" +
                            "select Abc(*) as c0, Def(*, *) as c1 from SupportBean";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplNonJoin).AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0, c1".Split(','), new object[]{2, 4});
            epService.EPAdministrator.DestroyAllStatements();
    
            var eplPattern = "expression abc { x => intPrimitive * 2} " +
                    "select * from pattern [a=SupportBean -> b=SupportBean(intPrimitive = Abc(a))]";
            epService.EPAdministrator.CreateEPL(eplPattern).AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString, b.theString".Split(','), new object[]{"E1", "E2"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSequenceAndNested(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.CreateEPL("create window WindowOne#keepall as (col1 string, col2 string)");
            epService.EPAdministrator.CreateEPL("insert into WindowOne select p00 as col1, p01 as col2 from SupportBean_S0");
    
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            epService.EPAdministrator.CreateEPL("create window WindowTwo#keepall as (col1 string, col2 string)");
            epService.EPAdministrator.CreateEPL("insert into WindowTwo select p10 as col1, p11 as col2 from SupportBean_S1");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "B1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A", "B2"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A", "B1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A", "B2"));
    
            var epl =
                    "@Audit('exprdef') " +
                            "expression last2X {\n" +
                            "  p => WindowOne(WindowOne.col1 = p.theString).TakeLast(2)\n" +
                            "} " +
                            "expression last2Y {\n" +
                            "  p => WindowTwo(WindowTwo.col1 = p.theString).TakeLast(2).SelectFrom(q => q.col2)\n" +
                            "} " +
                            "select Last2X(sb).SelectFrom(a => a.col2).SequenceEqual(Last2Y(sb)) as val from SupportBean as sb";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCaseNewMultiReturnNoElse(EPServiceProvider epService) {
    
            var fieldsInner = "col1,col2".Split(',');
            var epl = "expression gettotal {" +
                    " x => case " +
                    "  when theString = 'A' then new { col1 = 'X', col2 = 10 } " +
                    "  when theString = 'B' then new { col1 = 'Y', col2 = 20 } " +
                    "end" +
                    "} " +
                    "insert into OtherStream select Gettotal(sb) as val0 from SupportBean sb";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("val0"));
    
            var listenerTwo = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select val0.col1 as c1, val0.col2 as c2 from OtherStream").AddListener(listenerTwo);
            var fieldsConsume = "c1,c2".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap((Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{null, null});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsConsume, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 2));
            EPAssertionUtil.AssertPropsMap((Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"X", 10});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsConsume, new object[]{"X", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 3));
            EPAssertionUtil.AssertPropsMap((Map) listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[]{"Y", 20});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsConsume, new object[]{"Y", 20});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAnnotationOrder(EPServiceProvider epService) {
            var epl = "expression scalar {1} @Name('test') select Scalar() from SupportBean_ST0";
            TryAssertionAnnotation(epService, epl);
    
            epl = "@Name('test') expression scalar {1} select Scalar() from SupportBean_ST0";
            TryAssertionAnnotation(epService, epl);
        }
    
        private void TryAssertionAnnotation(EPServiceProvider epService, string epl) {
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("Scalar()"));
            Assert.AreEqual("test", stmt.Name);
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "Scalar()".Split(','), new object[]{1});
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubqueryMultiresult(EPServiceProvider epService) {
            var eplOne = "" +
                    "expression maxi {" +
                    " (select max(intPrimitive) from SupportBean#keepall)" +
                    "} " +
                    "expression mini {" +
                    " (select min(intPrimitive) from SupportBean#keepall)" +
                    "} " +
                    "select p00/Maxi() as val0, p00/Mini() as val1 " +
                    "from SupportBean_ST0#lastevent";
            TryAssertionMultiResult(epService, eplOne);
    
            var eplTwo = "" +
                    "expression subq {" +
                    " (select max(intPrimitive) as maxi, min(intPrimitive) as mini from SupportBean#keepall)" +
                    "} " +
                    "select p00/Subq().maxi as val0, p00/Subq().mini as val1 " +
                    "from SupportBean_ST0#lastevent";
            TryAssertionMultiResult(epService, eplTwo);
    
            var eplTwoAlias = "" +
                    "expression subq alias for " +
                    " { (select max(intPrimitive) as maxi, min(intPrimitive) as mini from SupportBean#keepall) }" +
                    " " +
                    "select p00/Subq().maxi as val0, p00/Subq().mini as val1 " +
                    "from SupportBean_ST0#lastevent";
            TryAssertionMultiResult(epService, eplTwoAlias);
        }
    
        private void TryAssertionMultiResult(EPServiceProvider epService, string epl) {
            var fields = new string[]{"val0", "val1"};
    
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2 / 10d, 2 / 5d});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4 / 20d, 4 / 2d});
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubqueryCross(EPServiceProvider epService) {
            var eplDeclare = "expression subq {" +
                    " (x, y) => (select theString from SupportBean#keepall where theString = x.id and intPrimitive = y.p10)" +
                    "} " +
                    "select Subq(one, two) as val1 " +
                    "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
            TryAssertionSubqueryCross(epService, eplDeclare);
    
            var eplAlias = "expression subq alias for { (select theString from SupportBean#keepall where theString = one.id and intPrimitive = two.p10) }" +
                    "select subq as val1 " +
                    "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
            TryAssertionSubqueryCross(epService, eplAlias);
        }
    
        private void TryAssertionSubqueryCross(EPServiceProvider epService, string epl) {
            var fields = new string[]{"val1"};
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(string)});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null});
    
            epService.EPRuntime.SendEvent(new SupportBean("ST0", 20));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("x", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"ST0"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubqueryJoinSameField(EPServiceProvider epService) {
            var eplDeclare = "" +
                    "expression subq {" +
                    " x => (select intPrimitive from SupportBean#keepall where theString = x.pcommon)" +   // a common field
                    "} " +
                    "select Subq(one) as val1, Subq(two) as val2 " +
                    "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
            TryAssertionSubqueryJoinSameField(epService, eplDeclare);
    
            var eplAlias = "" +
                    "expression subq alias for {(select intPrimitive from SupportBean#keepall where theString = pcommon) }" +
                    "select subq as val1, subq as val2 " +
                    "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
            TryInvalid(epService, eplAlias,
                    "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Failed to validate filter expression 'theString=pcommon': Property named 'pcommon' is ambiguous as is valid for more then one stream [expression subq alias for {(select intPrimitive from SupportBean#keepall where theString = pcommon) }select subq as val1, subq as val2 from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two]");
        }
    
        private void TryAssertionSubqueryJoinSameField(EPServiceProvider epService, string epl) {
            var fields = new string[]{"val1", "val2"};
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 10));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 0, "E0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, 10});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0, "E0"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubqueryCorrelated(EPServiceProvider epService) {
            var eplDeclare = "expression subqOne {" +
                    " x => (select id from SupportBean_ST0#keepall where p00 = x.intPrimitive)" +
                    "} " +
                    "select theString as val0, SubqOne(t) as val1 from SupportBean as t";
            TryAssertionSubqueryCorrelated(epService, eplDeclare);
    
            var eplAlias = "expression subqOne alias for {(select id from SupportBean_ST0#keepall where p00 = t.intPrimitive)} " +
                    "select theString as val0, SubqOne(t) as val1 from SupportBean as t";
            TryAssertionSubqueryCorrelated(epService, eplAlias);
        }
    
        private void TryAssertionSubqueryCorrelated(EPServiceProvider epService, string epl) {
            var fields = new string[]{"val0", "val1"};
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(string), typeof(string)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E0", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 100));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 99));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "ST0"});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST1", 100));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", null});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubqueryUncorrelated(EPServiceProvider epService) {
            var eplDeclare = "expression subqOne {(select id from SupportBean_ST0#lastevent)} " +
                    "select theString as val0, SubqOne() as val1 from SupportBean as t";
            TryAssertionSubqueryUncorrelated(epService, eplDeclare);
    
            var eplAlias = "expression subqOne alias for {(select id from SupportBean_ST0#lastevent)} " +
                    "select theString as val0, subqOne as val1 from SupportBean as t";
            TryAssertionSubqueryUncorrelated(epService, eplAlias);
        }
    
        private void TryAssertionSubqueryUncorrelated(EPServiceProvider epService, string epl) {
    
            var fields = new string[]{"val0", "val1"};
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(string), typeof(string)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E0", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 99));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "ST0"});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "ST1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubqueryNamedWindowUncorrelated(EPServiceProvider epService) {
            var eplDeclare = "expression subqnamedwin { MyWindow.Where(x => x.val1 > 10).OrderBy(x => x.val0) } " +
                    "select Subqnamedwin() as c0, Subqnamedwin().Where(x => x.val1 < 100) as c1 from SupportBean_ST0 as t";
            TryAssertionSubqueryNamedWindowUncorrelated(epService, eplDeclare);
    
            var eplAlias = "expression subqnamedwin alias for {MyWindow.Where(x => x.val1 > 10).OrderBy(x => x.val0)}" +
                    "select subqnamedwin as c0, subqnamedwin.Where(x => x.val1 < 100) as c1 from SupportBean_ST0";
            TryAssertionSubqueryNamedWindowUncorrelated(epService, eplAlias);
        }
    
        private void TryAssertionSubqueryNamedWindowUncorrelated(EPServiceProvider epService, string epl) {
    
            var fieldsSelected = "c0,c1".Split(',');
            var fieldsInside = "val0".Split(',');
    
            epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create window MyWindow#keepall as (val0 string, val1 int)");
            epService.EPAdministrator.CreateEPL("insert into MyWindow (val0, val1) select theString, intPrimitive from SupportBean");
    
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fieldsSelected, new Type[]
            {
                typeof(ICollection<object>), typeof(ICollection<object>)
            });
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID0", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldsInside, null);
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c1")), fieldsInside, null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldsInside, new object[][]{new object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c1")), fieldsInside, new object[][]{new object[] {"E1"}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 500));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID2", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldsInside, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c1")), fieldsInside, new object[][]{new object[] {"E1"}});
            listener.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubqueryNamedWindowCorrelated(EPServiceProvider epService) {
    
            var epl = "expression subqnamedwin {" +
                    "  x => MyWindow(val0 = x.key0).Where(y => val1 > 10)" +
                    "} " +
                    "select Subqnamedwin(t) as c0 from SupportBean_ST0 as t";
            TryAssertionSubqNWCorrelated(epService, epl);
    
            // more or less prefixes
            epl = "expression subqnamedwin {" +
                    "  x => MyWindow(val0 = x.key0).Where(y => y.val1 > 10)" +
                    "} " +
                    "select Subqnamedwin(t) as c0 from SupportBean_ST0 as t";
            TryAssertionSubqNWCorrelated(epService, epl);
    
            // with property-explicit stream name
            epl = "expression subqnamedwin {" +
                    "  x => MyWindow(MyWindow.val0 = x.key0).Where(y => y.val1 > 10)" +
                    "} " +
                    "select Subqnamedwin(t) as c0 from SupportBean_ST0 as t";
            TryAssertionSubqNWCorrelated(epService, epl);
    
            // with alias
            epl = "expression subqnamedwin alias for {MyWindow(MyWindow.val0 = t.key0).Where(y => y.val1 > 10)}" +
                    "select subqnamedwin as c0 from SupportBean_ST0 as t";
            TryAssertionSubqNWCorrelated(epService, epl);
    
            // test ambiguous property names
            epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create window MyWindowTwo#keepall as (id string, p00 int)");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo (id, p00) select theString, intPrimitive from SupportBean");
            epl = "expression subqnamedwin {" +
                    "  x => MyWindowTwo(MyWindowTwo.id = x.id).Where(y => y.p00 > 10)" +
                    "} " +
                    "select Subqnamedwin(t) as c0 from SupportBean_ST0 as t";
            epService.EPAdministrator.CreateEPL(epl);
        }
    
        private void TryAssertionSubqNWCorrelated(EPServiceProvider epService, string epl) {
            var fieldSelected = "c0".Split(',');
            var fieldInside = "val0".Split(',');
    
            epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create window MyWindow#keepall as (val0 string, val1 int)");
            epService.EPAdministrator.CreateEPL("insert into MyWindow (val0, val1) select theString, intPrimitive from SupportBean");
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fieldSelected, new Type[]{typeof(ICollection<object>)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID0", "x", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldInside, null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", "x", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldInside, null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID2", "E2", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldInside, new object[][]{new object[] {"E2"}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 13));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", "E3", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) listener.AssertOneGetNew().Get("c0")), fieldInside, new object[][]{new object[] {"E3"}});
            listener.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAggregationNoAccess(EPServiceProvider epService) {
            var fields = new string[]{"val1", "val2", "val3", "val4"};
            var epl = "" +
                    "expression sumA {x => " +
                    "   sum(x.intPrimitive) " +
                    "} " +
                    "expression sumB {x => " +
                    "   sum(x.intBoxed) " +
                    "} " +
                    "expression countC {" +
                    "   count(*) " +
                    "} " +
                    "select SumA(t) as val1, SumB(t) as val2, SumA(t)/SumB(t) as val3, CountC() as val4 from SupportBean as t";
    
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?), typeof(double?), typeof(long)});
    
            epService.EPRuntime.SendEvent(GetSupportBean(5, 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, 6, 5 / 6d, 1L});
    
            epService.EPRuntime.SendEvent(GetSupportBean(8, 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5 + 8, 6 + 10, (5 + 8) / (6d + 10d), 2L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionSplitStream(EPServiceProvider epService) {
            var epl = "expression myLittleExpression { @event => false }" +
                    "on SupportBean as myEvent " +
                    " insert into ABC select * where MyLittleExpression(myEvent)" +
                    " insert into DEF select * where not MyLittleExpression(myEvent)";
            epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL("select * from DEF").AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.IsInvoked);
        }
    
        private void RunAssertionAggregationAccess(EPServiceProvider epService) {
            var eplDeclare = "expression wb {s => window(*).Where(y => y.intPrimitive > 2) }" +
                    "select Wb(t) as val1 from SupportBean#keepall as t";
            TryAssertionAggregationAccess(epService, eplDeclare);
    
            var eplAlias = "expression wb alias for {window(*).Where(y => y.intPrimitive > 2)}" +
                    "select wb as val1 from SupportBean#keepall as t";
            TryAssertionAggregationAccess(epService, eplAlias);
        }
    
        private void RunAssertionAggregatedResult(EPServiceProvider epService) {
            var fields = "c0,c1".Split(',');
            var epl =
                    "expression lambda1 { o => 1 * o.intPrimitive }\n" +
                            "expression lambda2 { o => 3 * o.intPrimitive }\n" +
                            "select sum(Lambda1(e)) as c0, sum(Lambda2(e)) as c1 from SupportBean as e";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 30});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{15, 45});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionAggregationAccess(EPServiceProvider epService, string epl) {
    
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val1".Split(','), new Type[]{typeof(ICollection<object>), typeof(ICollection<object>)});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            var outArray = ToArray((ICollection<object>) listener.AssertOneGetNewAndReset().Get("val1"));
            Assert.AreEqual(0, outArray.Length);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            outArray = ToArray((ICollection<object>) listener.AssertOneGetNewAndReset().Get("val1"));
            Assert.AreEqual(1, outArray.Length);
            Assert.AreEqual("E2", outArray[0].TheString);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionScalarReturn(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var eplScalarDeclare = "expression scalarfilter {s => strvals.Where(y => y != 'E1') } " +
                    "select Scalarfilter(t).Where(x => x != 'E2') as val1 from SupportCollection as t";
            TryAssertionScalarReturn(epService, eplScalarDeclare);
    
            var eplScalarAlias = "expression scalarfilter alias for {strvals.Where(y => y != 'E1')}" +
                    "select Scalarfilter.Where(x => x != 'E2') as val1 from SupportCollection";
            TryAssertionScalarReturn(epService, eplScalarAlias);
    
            // test with cast and with on-select and where-clause use
            var inner = "case when myEvent.myObject = 'X' then 0 else Cast(myEvent.myObject, long) end ";
            var eplCaseDeclare = "expression theExpression { myEvent => " + inner + "} " +
                    "on MyEvent as myEvent select mw.* from MyWindowFirst as mw where mw.myObject = TheExpression(myEvent)";
            TryAssertionNamedWindowCast(epService, eplCaseDeclare, "First");
    
            var eplCaseAlias = "expression theExpression alias for {" + inner + "}" +
                    "on MyEvent as myEvent select mw.* from MyWindowSecond as mw where mw.myObject = theExpression";
            TryAssertionNamedWindowCast(epService, eplCaseAlias, "Second");
        }
    
        private void TryAssertionNamedWindowCast(EPServiceProvider epService, string epl, string windowPostfix) {
    
            epService.EPAdministrator.CreateEPL("create window MyWindow" + windowPostfix + "#keepall as (myObject long)");
            epService.EPAdministrator.CreateEPL("insert into MyWindow" + windowPostfix + "(myObject) select Cast(intPrimitive, long) from SupportBean");
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            var props = new string[]{"myObject"};
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
    
            epService.EPRuntime.SendEvent(new MyEvent(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new MyEvent("X"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), props, new object[]{0L});
    
            epService.EPRuntime.SendEvent(new MyEvent(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), props, new object[]{1L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionScalarReturn(EPServiceProvider epService, string epl) {
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val1".Split(','), new Type[]{typeof(ICollection<object>)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E3", "E4");
            listener.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventTypeAndSODA(EPServiceProvider epService) {
    
            var fields = new string[]{"FZero()", "FOne(t)", "FTwo(t,t)", "FThree(t,t)"};
            var eplDeclared = "" +
                    "expression fZero {10} " +
                    "expression fOne {x => x.intPrimitive} " +
                    "expression fTwo {(x,y) => x.intPrimitive+y.intPrimitive} " +
                    "expression fThree {(x,y) => x.intPrimitive+100} " +
                    "select FZero(), FOne(t), FTwo(t,t), FThree(t,t) from SupportBean as t";
            var eplFormatted = "" +
                    "expression fZero {10}" + NEWLINE +
                    "expression fOne {x => x.intPrimitive}" + NEWLINE +
                    "expression fTwo {(x,y) => x.intPrimitive+y.intPrimitive}" + NEWLINE +
                    "expression fThree {(x,y) => x.intPrimitive+100}" + NEWLINE +
                    "select FZero(), FOne(t), FTwo(t,t), FThree(t,t)" + NEWLINE +
                    "from SupportBean as t";
            var stmt = epService.EPAdministrator.CreateEPL(eplDeclared);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            TryAssertionTwoParameterArithmetic(epService, listener, stmt, fields);
    
            stmt.Dispose();
            var model = epService.EPAdministrator.CompileEPL(eplDeclared);
            Assert.AreEqual(eplDeclared, model.ToEPL());
            Assert.AreEqual(eplFormatted, model.ToEPL(new EPStatementFormatter(true)));
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplDeclared, stmt.Text);
            stmt.AddListener(listener);
    
            TryAssertionTwoParameterArithmetic(epService, listener, stmt, fields);
            stmt.Dispose();
    
            var eplAlias = "" +
                    "expression fZero alias for {10} " +
                    "expression fOne alias for {intPrimitive} " +
                    "expression fTwo alias for {intPrimitive+intPrimitive} " +
                    "expression fThree alias for {intPrimitive+100} " +
                    "select fZero, fOne, fTwo, fThree from SupportBean";
            var stmtAlias = epService.EPAdministrator.CreateEPL(eplAlias);
            stmtAlias.AddListener(listener);
            TryAssertionTwoParameterArithmetic(epService, listener, stmtAlias, new string[]{"fZero", "fOne", "fTwo", "fThree"});
            stmtAlias.Dispose();
        }
    
        private void TryAssertionTwoParameterArithmetic(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt, string[] fields) {
            var props = stmt.EventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(props, fields);
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(fields[0]));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType(fields[1]));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(fields[2]));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(fields[3]));
            var getter = stmt.EventType.GetGetter(fields[3]);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{10, 11, 22, 111});
            Assert.AreEqual(111, getter.Get(listener.AssertOneGetNewAndReset()));
        }
    
        private void RunAssertionOneParameterLambdaReturn(EPServiceProvider epService) {
    
            var eplDeclare = "" +
                    "expression one {x1 => x1.contained.Where(y => y.p00 < 10) } " +
                    "expression two {x2 => One(x2).Where(y => y.p00 > 1)  } " +
                    "select One(s0c) as val1, Two(s0c) as val2 from SupportBean_ST0_Container as s0c";
            TryAssertionOneParameterLambdaReturn(epService, eplDeclare);
    
            var eplAliasWParen = "" +
                    "expression one alias for {contained.Where(y => y.p00 < 10)}" +
                    "expression two alias for {One().Where(y => y.p00 > 1)}" +
                    "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
            TryAssertionOneParameterLambdaReturn(epService, eplAliasWParen);
    
            var eplAliasNoParen = "" +
                    "expression one alias for {contained.Where(y => y.p00 < 10)}" +
                    "expression two alias for {one.Where(y => y.p00 > 1)}" +
                    "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
            TryAssertionOneParameterLambdaReturn(epService, eplAliasNoParen);
        }
    
        private void TryAssertionOneParameterLambdaReturn(EPServiceProvider epService, string epl) {
    
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val1,val2".Split(','), new Type[]
            {
                typeof(ICollection<object>), typeof(ICollection<object>)
            });
    
            var theEvent = SupportBean_ST0_Container.Make3Value("E1,K1,1", "E2,K2,2", "E20,K20,20");
            epService.EPRuntime.SendEvent(theEvent);
            var resultVal1 = ((ICollection<object>) listener.LastNewData[0].Get("val1")).ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{theEvent.Contained[0], theEvent.Contained[1]}, resultVal1
            );
            var resultVal2 = ((ICollection<object>) listener.LastNewData[0].Get("val2")).ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{theEvent.Contained[1]}, resultVal2
            );
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNoParameterArithmetic(EPServiceProvider epService) {
    
            var eplDeclared = "expression getEnumerationSource {1} " +
                    "select GetEnumerationSource() as val1, GetEnumerationSource()*5 as val2 from SupportBean";
            TryAssertionNoParameterArithmetic(epService, eplDeclared);
    
            var eplDeclaredNoParen = "expression getEnumerationSource {1} " +
                    "select getEnumerationSource as val1, getEnumerationSource*5 as val2 from SupportBean";
            TryAssertionNoParameterArithmetic(epService, eplDeclaredNoParen);
    
            var eplAlias = "expression getEnumerationSource alias for {1} " +
                    "select getEnumerationSource as val1, getEnumerationSource*5 as val2 from SupportBean";
            TryAssertionNoParameterArithmetic(epService, eplAlias);
        }
    
        private void TryAssertionNoParameterArithmetic(EPServiceProvider epService, string epl) {
    
            var fields = "val1,val2".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 5});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNoParameterVariable(EPServiceProvider epService) {
            var eplDeclared = "expression one {myvar} " +
                    "expression two {myvar * 10} " +
                    "select One() as val1, Two() as val2, One() * Two() as val3 from SupportBean";
            TryAssertionNoParameterVariable(epService, eplDeclared);
    
            var eplAlias = "expression one alias for {myvar} " +
                    "expression two alias for {myvar * 10} " +
                    "select One() as val1, Two() as val2, one * two as val3 from SupportBean";
            TryAssertionNoParameterVariable(epService, eplAlias);
        }
    
        private void TryAssertionNoParameterVariable(EPServiceProvider epService, string epl) {
    
            epService.EPAdministrator.CreateEPL("create variable int myvar = 2");
    
            var fields = "val1,val2,val3".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?), typeof(int?)});
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, 20, 40});
    
            epService.EPRuntime.SetVariableValue("myvar", 3);
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 30, 90});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWhereClauseExpression(EPServiceProvider epService) {
            var eplNoAlias = "expression one {x=>x.boolPrimitive} select * from SupportBean as sb where One(sb)";
            TryAssertionWhereClauseExpression(epService, eplNoAlias);
    
            var eplAlias = "expression one alias for {boolPrimitive} select * from SupportBean as sb where one";
            TryAssertionWhereClauseExpression(epService, eplAlias);
        }
    
        private void TryAssertionWhereClauseExpression(EPServiceProvider epService, string epl) {
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            var theEvent = new SupportBean();
            theEvent.BoolPrimitive = true;
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            var epl = "expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=intPrimitive)} select Abc() from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'p00=intPrimitive': Property named 'intPrimitive' is not valid in any stream [expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=intPrimitive)} select Abc() from SupportBean]");
    
            epl = "expression abc {x=>strvals.Where(x=> x != 'E1')} select Abc(str) from SupportCollection str";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc(str)': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'strvals.Where()': Error validating enumeration method 'where', the lambda-parameter name 'x' has already been declared in this context [expression abc {x=>strvals.Where(x=> x != 'E1')} select Abc(str) from SupportCollection str]");
    
            epl = "expression abc {avg(intPrimitive)} select Abc() from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'avg(intPrimitive)': Property named 'intPrimitive' is not valid in any stream [expression abc {avg(intPrimitive)} select Abc() from SupportBean]");
    
            epl = "expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=sb.intPrimitive)} select Abc() from SupportBean sb";
            TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'p00=sb.intPrimitive': Failed to find a stream named 'sb' (did you mean 'st0'?) [expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=sb.intPrimitive)} select Abc() from SupportBean sb]");
    
            epl = "expression abc {window(*)} select Abc() from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'window(*)': The 'window' aggregation function requires that at least one stream is provided [expression abc {window(*)} select Abc() from SupportBean]");
    
            epl = "expression abc {x => intPrimitive} select Abc() from SupportBean";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc()': Parameter count mismatches for declared expression 'abc', expected 1 parameters but received 0 parameters [expression abc {x => intPrimitive} select Abc() from SupportBean]");
    
            epl = "expression abc {intPrimitive} select Abc(sb) from SupportBean sb";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc(sb)': Parameter count mismatches for declared expression 'abc', expected 0 parameters but received 1 parameters [expression abc {intPrimitive} select Abc(sb) from SupportBean sb]");
    
            epl = "expression abc {x=>} select Abc(sb) from SupportBean sb";
            TryInvalid(epService, epl, "Incorrect syntax near '}' at line 1 column 19 near reserved keyword 'select' [expression abc {x=>} select Abc(sb) from SupportBean sb]");
    
            epl = "expression abc {intPrimitive} select Abc() from SupportBean sb";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'intPrimitive': Property named 'intPrimitive' is not valid in any stream [expression abc {intPrimitive} select Abc() from SupportBean sb]");
    
            epl = "expression abc {x=>x} select Abc(1) from SupportBean sb";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'Abc(1)': Expression 'abc' requires a stream name as a parameter [expression abc {x=>x} select Abc(1) from SupportBean sb]");
    
            epl = "expression abc {x=>intPrimitive} select * from SupportBean sb where Abc(sb)";
            TryInvalid(epService, epl, "Filter expression not returning a bool value: 'Abc(sb)' [expression abc {x=>intPrimitive} select * from SupportBean sb where Abc(sb)]");
    
            epl = "expression abc {x=>x.intPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where Abc(*)";
            TryInvalid(epService, epl, "Error validating expression: Failed to validate filter expression 'Abc(*)': Expression 'abc' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead [expression abc {x=>x.intPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where Abc(*)]");
        }
    
        private SupportBean GetSupportBean(int intPrimitive, int? intBoxed) {
            var b = new SupportBean(null, intPrimitive);
            b.IntBoxed = intBoxed;
            return b;
        }
    
        private SupportBean[] ToArray<T>(ICollection<T> it) {
            var result = new List<SupportBean>();
            foreach (object item in it) {
                result.Add((SupportBean) item);
            }
            return result.ToArray();
        }
    
        private Map[] ToArrayMap(ICollection<object> it) {
            if (it == null) {
                return null;
            }
            var result = new List<Map>();
            foreach (var item in it) {
                var map = (Map) item;
                result.Add(map);
            }
            return result.ToArray();
        }
    
        public class MyEvent {
            public object MyObject { get; }
            public MyEvent(object myObject) {
                this.MyObject = myObject;
            }
        }
    }
} // end of namespace
