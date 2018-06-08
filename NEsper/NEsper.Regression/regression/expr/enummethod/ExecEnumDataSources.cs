///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.bookexample;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    using Map = IDictionary<string, object>;

    public class ExecEnumDataSources : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
            configuration.AddEventType<MyEvent>();
            configuration.AddImport(typeof(LocationReportFactory));
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            configuration.AddPlugInSingleRowFunction(
                "makeSampleList", typeof(SupportBean_ST0_Container), "MakeSampleList");
            configuration.AddPlugInSingleRowFunction(
                "makeSampleArray", typeof(SupportBean_ST0_Container), "MakeSampleArray");
            configuration.AddPlugInSingleRowFunction(
                "makeSampleListString", typeof(SupportCollection), "MakeSampleListString");
            configuration.AddPlugInSingleRowFunction(
                "makeSampleArrayString", typeof(SupportCollection), "MakeSampleArrayString");
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionSubstitutionParameter(epService);
            RunAssertionTableRow(epService);
            RunAssertionPatternFilter(epService);
            RunAssertionMatchRecognize(epService);
            RunAssertionEnumObject(epService);
            RunAssertionSortedMaxMinBy(epService);
            RunAssertionJoin(epService);
            RunAssertionPrevWindowSorted(epService);
            RunAssertionNamedWindow(epService);
            RunAssertionSubselect(epService);
            RunAssertionVariable(epService);
            RunAssertionAccessAggregation(epService);
            RunAssertionProperty(epService);
            RunAssertionPrevFuncs(epService);
            RunAssertionUDFStaticMethod(epService);
        }

        private void RunAssertionSubstitutionParameter(EPServiceProvider epService)
        {
            TrySubstitutionParameter(epService, new int?[] {1, 10, 100});
            TrySubstitutionParameter(epService, new object[] {1, 10, 100});
            TrySubstitutionParameter(epService, new[] {1, 10, 100});
        }

        private void RunAssertionTableRow(EPServiceProvider epService)
        {
            // test table access expression
            epService.EPAdministrator.CreateEPL("create table MyTableUnkeyed(theWindow window(*) @Type(SupportBean))");
            epService.EPAdministrator.CreateEPL(
                "into table MyTableUnkeyed select window(*) as theWindow from SupportBean#time(30)");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));

            var stmt = epService.EPAdministrator.CreateEPL(
                "select MyTableUnkeyed.theWindow.anyOf(v=>IntPrimitive=10) as c0 from SupportBean_A");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_A("A0"));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("c0"));
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionPatternFilter(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select * from pattern [ ([2]a=SupportBean_ST0) -> b=SupportBean(IntPrimitive > a.max(i -> p00))]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 15));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E4", 16));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "a[0].id,a[1].id,b.TheString".Split(','),
                new object[] {"E1", "E2", "E4"});
            stmt.Dispose();

            stmt = epService.EPAdministrator.CreateEPL(
                "select * from pattern [ a=SupportBean_ST0 until b=SupportBean -> c=SupportBean(IntPrimitive > a.sumOf(i => p00))]");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E10", 10));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E11", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E12", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E13", 25));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E14", 26));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "a[0].id,a[1].id,b.TheString,c.TheString".Split(','),
                new object[] {"E10", "E11", "E12", "E14"});

            stmt.Dispose();
        }

        private void RunAssertionMatchRecognize(EPServiceProvider epService)
        {
            // try define-clause
            var fieldsOne = "a_array[0].TheString,a_array[1].TheString,b.TheString".Split(',');
            var textOne = "select * from SupportBean " +
                          "match_recognize (" +
                          " measures A as a_array, B as b " +
                          " pattern (A* B)" +
                          " define" +
                          " B as A.anyOf(v=> v.IntPrimitive = B.IntPrimitive)" +
                          ")";

            var stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"A1", "A2", "A3"});

            epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A5", 2));
            epService.EPRuntime.SendEvent(new SupportBean("A6", 3));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("A7", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"A4", "A5", "A7"});
            stmtOne.Dispose();

            // try measures-clause
            var fieldsTwo = "c0".Split(',');
            var textTwo = "select * from SupportBean " +
                          "match_recognize (" +
                          " measures A.anyOf(v=> v.IntPrimitive = B.IntPrimitive) as c0 " +
                          " pattern (A* B)" +
                          " define" +
                          " A as A.TheString like 'A%'," +
                          " B as B.TheString like 'B%'" +
                          ")";

            var stmtTwo = epService.EPAdministrator.CreateEPL(textTwo);
            stmtTwo.Events += listener.Update;
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("B1", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {true});

            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 15));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[] {false});

            stmtTwo.Dispose();
        }

        private void RunAssertionEnumObject(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnumTwo));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportEnumTwoEvent));

            var fields = "c0,c1".Split(',');
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " +
                "SupportEnumTwo.ENUM_VALUE_1.GetMystrings().anyOf(v => v = id) as c0, " +
                "value.GetMystrings().anyOf(v => v = '2') as c1 " +
                "from SupportEnumTwoEvent");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportEnumTwoEvent("0", SupportEnumTwo.ENUM_VALUE_1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});

            epService.EPRuntime.SendEvent(new SupportEnumTwoEvent("2", SupportEnumTwo.ENUM_VALUE_2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true});

            stmt.Dispose();
        }

        private void RunAssertionSortedMaxMinBy(EPServiceProvider epService)
        {
            var fields = "c0,c1,c2,c3,c4".Split(',');

            var eplWindowAgg = "select " +
                               "sorted(TheString).allOf(x => x.IntPrimitive < 5) as c0," +
                               "maxby(TheString).allOf(x => x.IntPrimitive < 5) as c1," +
                               "minby(TheString).allOf(x => x.IntPrimitive < 5) as c2," +
                               "maxbyever(TheString).allOf(x => x.IntPrimitive < 5) as c3," +
                               "minbyever(TheString).allOf(x => x.IntPrimitive < 5) as c4" +
                               " from SupportBean#length(5)";
            var stmtWindowAgg = epService.EPAdministrator.CreateEPL(eplWindowAgg);
            var listener = new SupportUpdateListener();
            stmtWindowAgg.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {true, true, true, true, true});

            stmtWindowAgg.Dispose();
        }

        private void RunAssertionJoin(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SelectorEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(ContainerEvent));

            var stmt = epService.EPAdministrator.CreateEPL(
                "select * from SelectorEvent#keepall as sel, ContainerEvent#keepall as cont " +
                "where cont.items.anyOf(i => sel.selector = i.selected)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SelectorEvent("S1", "sel1"));
            epService.EPRuntime.SendEvent(new ContainerEvent("C1", new ContainedItem("I1", "sel1")));
            Assert.IsTrue(listener.IsInvoked);

            stmt.Dispose();
        }

        private void RunAssertionPrevWindowSorted(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select prevwindow(st0) as val0, prevwindow(st0).EsperInternalNoop() as val1 " +
                "from SupportBean_ST0#sort(3, p00 asc) as st0");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(
                stmt.EventType, "val0,val1".Split(','), new[] {
                    typeof(SupportBean_ST0[]),
                    typeof(ICollection<SupportBean_ST0>)
                });

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 5));
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 6));
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1,E2");
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", 4));
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E3,E1,E2");
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E5", 3));
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E5,E3,E1");
            listener.Reset();
            stmt.Dispose();

            // Scalar version
            var fields = new[] {"val0"};
            var stmtScalar = epService.EPAdministrator.CreateEPL(
                "select prevwindow(id).where(x => x not like '%ignore%') as val0 " +
                "from SupportBean_ST0#keepall as st0");
            stmtScalar.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, fields, new[] {
                typeof(ICollection<string>)
            });

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 5));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1");
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2ignore", 6));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1");
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", 4));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E3", "E1");
            listener.Reset();

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ignoreE5", 3));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E3", "E1");
            listener.Reset();

            stmtScalar.Dispose();
        }

        private void RunAssertionNamedWindow(EPServiceProvider epService)
        {
            // test named window
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean_ST0");
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_ST0");
            var eplNamedWindow = "select MyWindow.allOf(x => x.p00 < 5) as allOfX from SupportBean#keepall";
            var stmtNamedWindow = epService.EPAdministrator.CreateEPL(eplNamedWindow);
            var listener = new SupportUpdateListener();
            stmtNamedWindow.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtNamedWindow.EventType, "allOfX".Split(','), new[] {
                typeof(bool)
            });

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));

            stmtNamedWindow.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));

            // test named window correlated
            var eplNamedWindowCorrelated =
                "select MyWindow(key0 = sb.TheString).allOf(x => x.p00 < 5) as allOfX from SupportBean#keepall sb";
            var stmtNamedWindowCorrelated = epService.EPAdministrator.CreateEPL(eplNamedWindowCorrelated);
            stmtNamedWindowCorrelated.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", "KEY1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean("KEY1", 0));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtNamedWindowCorrelated.Dispose();
        }

        private void RunAssertionSubselect(EPServiceProvider epService)
        {
            // test subselect-wildcard
            var eplSubselect =
                "select (select * from SupportBean_ST0#keepall).allOf(x => x.p00 < 5) as allOfX from SupportBean#keepall";
            var stmtSubselect = epService.EPAdministrator.CreateEPL(eplSubselect);
            var listener = new SupportUpdateListener();
            stmtSubselect.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselect.Dispose();

            // test subselect scalar return
            var eplSubselectScalar =
                "select (select id from SupportBean_ST0#keepall).allOf(x => x  like '%B%') as allOfX from SupportBean#keepall";
            var stmtSubselectScalar = epService.EPAdministrator.CreateEPL(eplSubselectScalar);
            stmtSubselectScalar.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_ST0("B1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("A1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselectScalar.Dispose();

            // test subselect-correlated scalar return
            var eplSubselectScalarCorrelated =
                "select (select key0 from SupportBean_ST0#keepall st0 where st0.id = sb.TheString).allOf(x => x  like '%hello%') as allOfX from SupportBean#keepall sb";
            var stmtSubselectScalarCorrelated = epService.EPAdministrator.CreateEPL(eplSubselectScalarCorrelated);
            stmtSubselectScalarCorrelated.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_ST0("A1", "hello", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("A2", "hello", 0));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("A3", "test", 0));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 1));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselectScalarCorrelated.Dispose();

            // test subselect multivalue return
            var fields = new[] {"id", "p00"};
            var eplSubselectMultivalue =
                "select (select id, p00 from SupportBean_ST0#keepall).take(10) as c0 from SupportBean";
            var stmtSubselectMultivalue = epService.EPAdministrator.CreateEPL(eplSubselectMultivalue);
            stmtSubselectMultivalue.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_ST0("B1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPropsMapRows(
                listener.AssertOneGetNewAndReset().Get("c0").Unwrap<object>(), fields,
                new[] {new object[] {"B1", 10}});

            epService.EPRuntime.SendEvent(new SupportBean_ST0("B2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertPropsMapRows(
                listener.AssertOneGetNewAndReset().Get("c0").Unwrap<object>(), fields,
                new[] {new object[] {"B1", 10}, new object[] {"B2", 20}});
            stmtSubselectMultivalue.Dispose();

            // test subselect that delivers events
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL("create schema AEvent (symbol string)");
            epService.EPAdministrator.CreateEPL("create schema BEvent (a AEvent)");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select (select a from BEvent#keepall).anyOf(v => symbol = 'GE') as flag from SupportBean");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(MakeBEvent("XX"), "BEvent");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("flag"));

            epService.EPRuntime.SendEvent(MakeBEvent("GE"), "BEvent");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("flag"));

            stmt.Dispose();
        }

        private void RunAssertionVariable(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create variable string[] myvar = { 'E1', 'E3' }");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean(myvar.anyOf(v => v = TheString))")
                .Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionAccessAggregation(EPServiceProvider epService)
        {
            var fields = new[] {"val0", "val1", "val2", "val3", "val4"};

            // test window(*) and first(*)
            var eplWindowAgg = "select " +
                               "window(*).allOf(x => x.IntPrimitive < 5) as val0," +
                               "first(*).allOf(x => x.IntPrimitive < 5) as val1," +
                               "first(*, 1).allOf(x => x.IntPrimitive < 5) as val2," +
                               "last(*).allOf(x => x.IntPrimitive < 5) as val3," +
                               "last(*, 1).allOf(x => x.IntPrimitive < 5) as val4" +
                               " from SupportBean#length(2)";
            var stmtWindowAgg = epService.EPAdministrator.CreateEPL(eplWindowAgg);
            var listener = new SupportUpdateListener();
            stmtWindowAgg.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {true, true, null, true, null});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, false, false, true});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {false, false, true, true, false});

            stmtWindowAgg.Dispose();

            // test scalar: window(*) and first(*)
            var eplWindowAggScalar = "select " +
                                     "window(IntPrimitive).allOf(x => x < 5) as val0," +
                                     "first(IntPrimitive).allOf(x => x < 5) as val1," +
                                     "first(IntPrimitive, 1).allOf(x => x < 5) as val2," +
                                     "last(IntPrimitive).allOf(x => x < 5) as val3," +
                                     "last(IntPrimitive, 1).allOf(x => x < 5) as val4" +
                                     " from SupportBean#length(2)";
            var stmtWindowAggScalar = epService.EPAdministrator.CreateEPL(eplWindowAggScalar);
            stmtWindowAggScalar.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {true, true, null, true, null});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, false, false, true});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {false, false, true, true, false});

            stmtWindowAggScalar.Dispose();
        }

        private void RunAssertionProperty(EPServiceProvider epService)
        {
            // test fragment type - collection inside
            var eplFragment = "select Contained.allOf(x => x.p00 < 5) as allOfX from SupportBean_ST0_Container#keepall";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("ID1,KEY1,1"));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("ID1,KEY1,10"));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtFragment.Dispose();

            // test array and iterable
            var fields = "val0,val1".Split(',');
            eplFragment = "select Intarray.sumof() as val0, " +
                          "Intiterable.sumOf() as val1 " +
                          " from SupportCollection#keepall";
            stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += listener.Update;

            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("5,6,7"));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {5 + 6 + 7, 5 + 6 + 7});

            // test map event type with object-array prop
            epService.EPAdministrator.Configuration.AddEventType(typeof(BookDesc));
            epService.EPAdministrator.CreateEPL("create schema MySchema (books BookDesc[])");

            var stmt = epService.EPAdministrator.CreateEPL("select books.max(i => i.price) as mymax from MySchema");
            stmt.Events += listener.Update;

            var @event = Collections.SingletonDataMap(
                "books", new[] {new BookDesc("1", "book1", "dave", 1.00, null)});
            epService.EPRuntime.SendEvent(@event, "MySchema");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "mymax".Split(','), new object[] {1.0});

            // test method invocation variations returning list/array of string and test UDF +property as well
            RunAssertionMethodInvoke(epService, "select e.TheList.anyOf(v => v = selector) as flag from MyEvent e");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "convertToArray", typeof(MyEvent), "ConvertToArray");
            RunAssertionMethodInvoke(
                epService, "select convertToArray(theList).anyOf(v => v = selector) as flag from MyEvent e");
            RunAssertionMethodInvoke(epService, "select TheArray.anyOf(v => v = selector) as flag from MyEvent e");
            RunAssertionMethodInvoke(epService, "select e.TheArray.anyOf(v => v = selector) as flag from MyEvent e");
            RunAssertionMethodInvoke(
                epService, "select e.TheList.anyOf(v => v = e.selector) as flag from pattern[every e=MyEvent]");
            RunAssertionMethodInvoke(
                epService,
                "select e.NestedMyEvent.MyNestedList.anyOf(v => v = e.selector) as flag from pattern[every e=MyEvent]");
            RunAssertionMethodInvoke(
                epService,
                "select " + TypeHelper.MaskTypeName<MyEvent>() +
                ".ConvertToArray(TheList).anyOf(v => v = selector) as flag from MyEvent e");

            stmt.Dispose();
        }

        private void RunAssertionMethodInvoke(EPServiceProvider epService, string epl)
        {
            var fields = "flag".Split(',');
            var listener = new SupportUpdateListener();
            var stmtMethodAnyOf = epService.EPAdministrator.CreateEPL(epl);
            stmtMethodAnyOf.Events += listener.Update;

            epService.EPRuntime.SendEvent(new MyEvent("1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true});

            epService.EPRuntime.SendEvent(new MyEvent("4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false});

            stmtMethodAnyOf.Dispose();
        }

        private void RunAssertionPrevFuncs(EPServiceProvider epService)
        {
            // test prevwindow(*) etc
            var fields = new[] {"val0", "val1", "val2"};
            var epl = "select " +
                      "prevwindow(sb).allOf(x => x.IntPrimitive < 5) as val0," +
                      "prev(sb,1).allOf(x => x.IntPrimitive < 5) as val1," +
                      "prevtail(sb,1).allOf(x => x.IntPrimitive < 5) as val2" +
                      " from SupportBean#length(2) as sb";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, null, null});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, false});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, false, true});

            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, true, true});
            stmt.Dispose();

            // test scalar prevwindow(property) etc
            var eplScalar = "select " +
                            "prevwindow(IntPrimitive).allOf(x => x < 5) as val0," +
                            "prev(IntPrimitive,1).allOf(x => x < 5) as val1," +
                            "prevtail(IntPrimitive,1).allOf(x => x < 5) as val2" +
                            " from SupportBean#length(2) as sb";
            var stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, null, null});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, false});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, false, true});

            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, true, true});

            stmtScalar.Dispose();
        }

        private void RunAssertionUDFStaticMethod(EPServiceProvider epService)
        {
            var fields = "val1,val2,val3,val4".Split(',');
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean_ST0_Container));
            var epl = "select " +
                      "SupportBean_ST0_Container.MakeSampleList().where(x => x.p00 < 5) as val1, " +
                      "SupportBean_ST0_Container.MakeSampleArray().where(x => x.p00 < 5) as val2, " +
                      "MakeSampleList().where(x => x.p00 < 5) as val3, " +
                      "MakeSampleArray().where(x => x.p00 < 5) as val4 " +
                      "from SupportBean#length(2) as sb";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SupportBean_ST0_Container.Samples = new[] {"E1,1", "E2,20", "E3,3"};
            epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields)
            {
                var result = listener.AssertOneGetNew().Get(field).UnwrapIntoArray<SupportBean_ST0>();
                Assert.AreEqual(2, result.Length, "Failed for field " + field);
            }

            listener.Reset();

            SupportBean_ST0_Container.Samples = null;
            epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields)
            {
                Assert.IsNull(listener.AssertOneGetNew().Get(field));
            }

            listener.Reset();

            SupportBean_ST0_Container.Samples = new string[0];
            epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields)
            {
                var result = listener.AssertOneGetNew().Get(field).UnwrapIntoArray<SupportBean_ST0>();
                Assert.AreEqual(0, result.Length);
            }

            listener.Reset();
            stmt.Dispose();

            // test UDF returning scalar values collection
            fields = "val0,val1,val2,val3".Split(',');
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportCollection));
            var eplScalar = "select " +
                            "SupportCollection.MakeSampleListString().where(x => x != 'E1') as val0, " +
                            "SupportCollection.MakeSampleArrayString().where(x => x != 'E1') as val1, " +
                            "MakeSampleListString().where(x => x != 'E1') as val2, " +
                            "MakeSampleArrayString().where(x => x != 'E1') as val3 " +
                            "from SupportBean#length(2) as sb";
            var stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(
                stmtScalar.EventType, fields, new[]
                {
                    typeof(ICollection<string>),
                    typeof(ICollection<string>),
                    typeof(ICollection<string>),
                    typeof(ICollection<string>)
                });

            SupportCollection.SampleCSV = "E1,E2,E3";
            epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields)
            {
                LambdaAssertionUtil.AssertValuesArrayScalar(listener, field, "E2", "E3");
            }

            listener.Reset();

            SupportCollection.SampleCSV = null;
            epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields)
            {
                LambdaAssertionUtil.AssertValuesArrayScalar(listener, field, null);
            }

            listener.Reset();

            SupportCollection.SampleCSV = "";
            epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields)
            {
                LambdaAssertionUtil.AssertValuesArrayScalar(listener, field);
            }

            listener.Reset();

            stmtScalar.Dispose();
        }

        private void TrySubstitutionParameter(EPServiceProvider epService, object parameter)
        {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var prepared = epService.EPAdministrator.PrepareEPL(
                "select * from SupportBean(?.sequenceEqual({1, IntPrimitive, 100}))");
            prepared.SetObject(1, parameter);
            epService.EPAdministrator.Create(prepared).Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            epService.EPAdministrator.DestroyAllStatements();
        }

#if false
        private SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> it) {
            if (!it.IsEmpty() && it.First() is EventBean) {
                Assert.Fail("Iterator provides EventBean instances");
            }
            return It.ToArray(new SupportBean_ST0[it.Count]);
        }
#endif

        private IDictionary<string, object> MakeBEvent(string symbol)
        {
            var map = new Dictionary<string, object>();
            map.Put("a", Collections.SingletonMap("symbol", symbol));
            return map;
        }

        private void AssertPropsMapRows(ICollection<object> rows, string[] fields, object[][] objects)
        {
            var maps = rows
                .Select(row => row.UnwrapStringDictionary())
                .ToArray();
            EPAssertionUtil.AssertPropsPerRow(maps, fields, objects);
        }

        public class SelectorEvent
        {
            public SelectorEvent(string selectorId, string selector)
            {
                SelectorId = selectorId;
                Selector = selector;
            }

            public string SelectorId { get; }

            public string Selector { get; }
        }

        public class ContainerEvent
        {
            public ContainerEvent(string containerId, params ContainedItem[] items)
            {
                ContainerId = containerId;
                Items = items;
            }

            public string ContainerId { get; }

            public ContainedItem[] Items { get; }
        }

        public class ContainedItem
        {
            public ContainedItem(string itemId, string selected)
            {
                ItemId = itemId;
                Selected = selected;
            }

            public string ItemId { get; }

            public string Selected { get; }
        }

        public class MyEvent
        {
            public MyEvent(string selector)
            {
                Selector = selector;

                TheList = new List<string>();
                TheList.Add("1");
                TheList.Add("2");
                TheList.Add("3");
            }

            public string Selector { get; }

            public List<string> TheList { get; }

            public NestedMyEvent NestedMyEvent => new NestedMyEvent(TheList);

            public string[] TheArray {
                get { return TheList.ToArray(); }
            }

            public static string[] ConvertToArray(IEnumerable<string> list)
            {
                return list.ToArray();
            }
        }

        public class NestedMyEvent
        {
            public NestedMyEvent(List<string> myList)
            {
                MyNestedList = myList;
            }

            public List<string> MyNestedList { get; }
        }

        public class SupportEnumTwoEvent
        {
            public SupportEnumTwoEvent(string id, SupportEnumTwo value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; }

            public SupportEnumTwo Value { get; }
        }
    }
} // end of namespace