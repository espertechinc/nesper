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
            configuration.AddEventType(typeof(MyEvent));
            configuration.AddImport(typeof(LocationReportFactory));
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            configuration.AddPlugInSingleRowFunction(
                "makeSampleList", typeof(SupportBean_ST0_Container).FullName, "MakeSampleList");
            configuration.AddPlugInSingleRowFunction(
                "makeSampleArray", typeof(SupportBean_ST0_Container).FullName, "MakeSampleArray");
            configuration.AddPlugInSingleRowFunction(
                "makeSampleListString", typeof(SupportCollection).FullName, "MakeSampleListString");
            configuration.AddPlugInSingleRowFunction(
                "makeSampleArrayString", typeof(SupportCollection).FullName, "MakeSampleArrayString");
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
                "into table MyTableUnkeyed select window(*) as theWindow from SupportBean#Time(30)");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));

            var stmt = epService.EPAdministrator.CreateEPL(
                "select MyTableUnkeyed.theWindow.AnyOf(v=>intPrimitive=10) as c0 from SupportBean_A");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean_A("A0"));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("c0"));
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionPatternFilter(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select * from pattern [ ([2]a=SupportBean_ST0) -> b=SupportBean(intPrimitive > a.max(i -> p00))]");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 15));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E4", 16));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "a[0].id,a[1].id,b.theString".Split(','),
                new object[] {"E1", "E2", "E4"});
            stmt.Dispose();

            stmt = epService.EPAdministrator.CreateEPL(
                "select * from pattern [ a=SupportBean_ST0 until b=SupportBean -> c=SupportBean(intPrimitive > a.SumOf(i => p00))]");
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean_ST0("E10", 10));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E11", 15));
            epService.EPRuntime.SendEvent(new SupportBean("E12", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E13", 25));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E14", 26));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "a[0].id,a[1].id,b.theString,c.theString".Split(','),
                new object[] {"E10", "E11", "E12", "E14"});

            stmt.Dispose();
        }

        private void RunAssertionMatchRecognize(EPServiceProvider epService)
        {
            // try define-clause
            var fieldsOne = "a_array[0].theString,a_array[1].theString,b.theString".Split(',');
            var textOne = "select * from SupportBean " +
                          "match_recognize (" +
                          " measures A as a_array, B as b " +
                          " pattern (A* B)" +
                          " define" +
                          " B as A.AnyOf(v=> v.intPrimitive = B.intPrimitive)" +
                          ")";

            var stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listener = new SupportUpdateListener();
            stmtOne.AddListener(listener);

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
                          " measures A.AnyOf(v=> v.intPrimitive = B.intPrimitive) as c0 " +
                          " pattern (A* B)" +
                          " define" +
                          " A as A.theString like 'A%'," +
                          " B as B.theString like 'B%'" +
                          ")";

            var stmtTwo = epService.EPAdministrator.CreateEPL(textTwo);
            stmtTwo.AddListener(listener);
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
                "SupportEnumTwo.ENUM_VALUE_1.Mystrings.AnyOf(v => v = id) as c0, " +
                "value.Mystrings.AnyOf(v => v = '2') as c1 " +
                "from SupportEnumTwoEvent");
            stmt.AddListener(listener);

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
                               "sorted(theString).AllOf(x => x.intPrimitive < 5) as c0," +
                               "maxby(theString).AllOf(x => x.intPrimitive < 5) as c1," +
                               "minby(theString).AllOf(x => x.intPrimitive < 5) as c2," +
                               "maxbyever(theString).AllOf(x => x.intPrimitive < 5) as c3," +
                               "minbyever(theString).AllOf(x => x.intPrimitive < 5) as c4" +
                               " from SupportBean#length(5)";
            var stmtWindowAgg = epService.EPAdministrator.CreateEPL(eplWindowAgg);
            var listener = new SupportUpdateListener();
            stmtWindowAgg.AddListener(listener);

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
                "" +
                "select * from SelectorEvent#keepall as sel, ContainerEvent#keepall as cont " +
                "where Cont.items.AnyOf(i => sel.selector = i.selected)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SelectorEvent("S1", "sel1"));
            epService.EPRuntime.SendEvent(new ContainerEvent("C1", new ContainedItem("I1", "sel1")));
            Assert.IsTrue(listener.IsInvoked);

            stmt.Dispose();
        }

        private void RunAssertionPrevWindowSorted(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select Prevwindow(st0) as val0, Prevwindow(st0).EsperInternalNoop() as val1 " +
                "from SupportBean_ST0#Sort(3, p00 asc) as st0");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(
                stmt.EventType, "val0,val1".Split(','), new[] {typeof(SupportBean_ST0[]), typeof(ICollection<object>)});

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
                "select Prevwindow(id).Where(x => x not like '%ignore%') as val0 " +
                "from SupportBean_ST0#keepall as st0");
            stmtScalar.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, fields, new[] {typeof(ICollection<object>)});

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
            var eplNamedWindow = "select MyWindow.AllOf(x => x.p00 < 5) as allOfX from SupportBean#keepall";
            var stmtNamedWindow = epService.EPAdministrator.CreateEPL(eplNamedWindow);
            var listener = new SupportUpdateListener();
            stmtNamedWindow.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtNamedWindow.EventType, "allOfX".Split(','), new[] {typeof(bool?)});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));

            stmtNamedWindow.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));

            // test named window correlated
            var eplNamedWindowCorrelated =
                "select MyWindow(key0 = sb.theString).AllOf(x => x.p00 < 5) as allOfX from SupportBean#keepall sb";
            var stmtNamedWindowCorrelated = epService.EPAdministrator.CreateEPL(eplNamedWindowCorrelated);
            stmtNamedWindowCorrelated.AddListener(listener);

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
                "select (select * from SupportBean_ST0#keepall).AllOf(x => x.p00 < 5) as allOfX from SupportBean#keepall";
            var stmtSubselect = epService.EPAdministrator.CreateEPL(eplSubselect);
            var listener = new SupportUpdateListener();
            stmtSubselect.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselect.Dispose();

            // test subselect scalar return
            var eplSubselectScalar =
                "select (select id from SupportBean_ST0#keepall).AllOf(x => x  like '%B%') as allOfX from SupportBean#keepall";
            var stmtSubselectScalar = epService.EPAdministrator.CreateEPL(eplSubselectScalar);
            stmtSubselectScalar.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean_ST0("B1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(new SupportBean_ST0("A1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselectScalar.Dispose();

            // test subselect-correlated scalar return
            var eplSubselectScalarCorrelated =
                "select (select key0 from SupportBean_ST0#keepall st0 where st0.id = sb.theString).AllOf(x => x  like '%hello%') as allOfX from SupportBean#keepall sb";
            var stmtSubselectScalarCorrelated = epService.EPAdministrator.CreateEPL(eplSubselectScalarCorrelated);
            stmtSubselectScalarCorrelated.AddListener(listener);

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
                "select (select id, p00 from SupportBean_ST0#keepall).Take(10) as c0 from SupportBean";
            var stmtSubselectMultivalue = epService.EPAdministrator.CreateEPL(eplSubselectMultivalue);
            stmtSubselectMultivalue.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean_ST0("B1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPropsMapRows(
                (ICollection<object>) listener.AssertOneGetNewAndReset().Get("c0"), fields,
                new[] {new object[] {"B1", 10}});

            epService.EPRuntime.SendEvent(new SupportBean_ST0("B2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertPropsMapRows(
                (ICollection<object>) listener.AssertOneGetNewAndReset().Get("c0"), fields,
                new[] {new object[] {"B1", 10}, new object[] {"B2", 20}});
            stmtSubselectMultivalue.Dispose();

            // test subselect that delivers events
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL("create schema AEvent (symbol string)");
            epService.EPAdministrator.CreateEPL("create schema BEvent (a AEvent)");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select (select a from BEvent#keepall).AnyOf(v => symbol = 'GE') as flag from SupportBean");
            stmt.AddListener(listener);

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
            epService.EPAdministrator.CreateEPL("select * from SupportBean(myvar.AnyOf(v => v = theString))")
                .AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionAccessAggregation(EPServiceProvider epService)
        {
            var fields = new[] {"val0", "val1", "val2", "val3", "val4"};

            // test window(*) and First(*)
            var eplWindowAgg = "select " +
                               "window(*).AllOf(x => x.intPrimitive < 5) as val0," +
                               "First(*).AllOf(x => x.intPrimitive < 5) as val1," +
                               "First(*, 1).AllOf(x => x.intPrimitive < 5) as val2," +
                               "last(*).AllOf(x => x.intPrimitive < 5) as val3," +
                               "last(*, 1).AllOf(x => x.intPrimitive < 5) as val4" +
                               " from SupportBean#length(2)";
            var stmtWindowAgg = epService.EPAdministrator.CreateEPL(eplWindowAgg);
            var listener = new SupportUpdateListener();
            stmtWindowAgg.AddListener(listener);

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

            // test scalar: window(*) and First(*)
            var eplWindowAggScalar = "select " +
                                     "window(intPrimitive).AllOf(x => x < 5) as val0," +
                                     "First(intPrimitive).AllOf(x => x < 5) as val1," +
                                     "First(intPrimitive, 1).AllOf(x => x < 5) as val2," +
                                     "last(intPrimitive).AllOf(x => x < 5) as val3," +
                                     "last(intPrimitive, 1).AllOf(x => x < 5) as val4" +
                                     " from SupportBean#length(2)";
            var stmtWindowAggScalar = epService.EPAdministrator.CreateEPL(eplWindowAggScalar);
            stmtWindowAggScalar.AddListener(listener);

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
            var eplFragment = "select Contained.AllOf(x => x.p00 < 5) as allOfX from SupportBean_ST0_Container#keepall";
            var stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("ID1,KEY1,1"));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("allOfX"));

            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("ID1,KEY1,10"));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtFragment.Dispose();

            // test array and iterable
            var fields = "val0,val1".Split(',');
            eplFragment = "select Intarray.Sumof() as val0, " +
                          "intiterable.SumOf() as val1 " +
                          " from SupportCollection#keepall";
            stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.AddListener(listener);

            epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("5,6,7"));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {5 + 6 + 7, 5 + 6 + 7});

            // test map event type with object-array prop
            epService.EPAdministrator.Configuration.AddEventType(typeof(BookDesc));
            epService.EPAdministrator.CreateEPL("create schema MySchema (books BookDesc[])");

            var stmt = epService.EPAdministrator.CreateEPL("select Books.max(i => i.price) as mymax from MySchema");
            stmt.AddListener(listener);

            var @event = Collections.SingletonDataMap(
                "books", new[] {new BookDesc("1", "book1", "dave", 1.00, null)});
            epService.EPRuntime.SendEvent(@event, "MySchema");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "mymax".Split(','), new object[] {1.0});

            // test method invocation variations returning list/array of string and test UDF +property as well
            RunAssertionMethodInvoke(epService, "select E.TheList.AnyOf(v => v = selector) as flag from MyEvent e");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "convertToArray", typeof(MyEvent).FullName, "ConvertToArray");
            RunAssertionMethodInvoke(
                epService, "select ConvertToArray(theList).AnyOf(v => v = selector) as flag from MyEvent e");
            RunAssertionMethodInvoke(epService, "select TheArray.AnyOf(v => v = selector) as flag from MyEvent e");
            RunAssertionMethodInvoke(epService, "select E.TheArray.AnyOf(v => v = selector) as flag from MyEvent e");
            RunAssertionMethodInvoke(
                epService, "select E.theList.AnyOf(v => v = e.selector) as flag from pattern[every e=MyEvent]");
            RunAssertionMethodInvoke(
                epService,
                "select E.nestedMyEvent.myNestedList.AnyOf(v => v = e.selector) as flag from pattern[every e=MyEvent]");
            RunAssertionMethodInvoke(
                epService,
                "select " + TypeHelper.MaskTypeName<MyEvent>() +
                ".ConvertToArray(theList).AnyOf(v => v = selector) as flag from MyEvent e");

            stmt.Dispose();
        }

        private void RunAssertionMethodInvoke(EPServiceProvider epService, string epl)
        {
            var fields = "flag".Split(',');
            var listener = new SupportUpdateListener();
            var stmtMethodAnyOf = epService.EPAdministrator.CreateEPL(epl);
            stmtMethodAnyOf.AddListener(listener);

            epService.EPRuntime.SendEvent(new MyEvent("1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true});

            epService.EPRuntime.SendEvent(new MyEvent("4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false});

            stmtMethodAnyOf.Dispose();
        }

        private void RunAssertionPrevFuncs(EPServiceProvider epService)
        {
            // test Prevwindow(*) etc
            var fields = new[] {"val0", "val1", "val2"};
            var epl = "select " +
                      "Prevwindow(sb).AllOf(x => x.intPrimitive < 5) as val0," +
                      "Prev(sb,1).AllOf(x => x.intPrimitive < 5) as val1," +
                      "Prevtail(sb,1).AllOf(x => x.intPrimitive < 5) as val2" +
                      " from SupportBean#length(2) as sb";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, null, null});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true, false});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, false, true});

            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, true, true});
            stmt.Dispose();

            // test scalar Prevwindow(property) etc
            var eplScalar = "select " +
                            "Prevwindow(intPrimitive).AllOf(x => x < 5) as val0," +
                            "Prev(intPrimitive,1).AllOf(x => x < 5) as val1," +
                            "Prevtail(intPrimitive,1).AllOf(x => x < 5) as val2" +
                            " from SupportBean#length(2) as sb";
            var stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.AddListener(listener);

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
                      "SupportBean_ST0_Container.MakeSampleList().Where(x => x.p00 < 5) as val1, " +
                      "SupportBean_ST0_Container.MakeSampleArray().Where(x => x.p00 < 5) as val2, " +
                      "MakeSampleList().Where(x => x.p00 < 5) as val3, " +
                      "MakeSampleArray().Where(x => x.p00 < 5) as val4 " +
                      "from SupportBean#length(2) as sb";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

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
                            "SupportCollection.MakeSampleListString().Where(x => x != 'E1') as val0, " +
                            "SupportCollection.MakeSampleArrayString().Where(x => x != 'E1') as val1, " +
                            "MakeSampleListString().Where(x => x != 'E1') as val2, " +
                            "MakeSampleArrayString().Where(x => x != 'E1') as val3 " +
                            "from SupportBean#length(2) as sb";
            var stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(
                stmtScalar.EventType, fields, new[]
                {
                    typeof(ICollection<object>),
                    typeof(ICollection<object>),
                    typeof(ICollection<object>),
                    typeof(ICollection<object>)
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
                "select * from SupportBean(?.SequenceEqual({1, intPrimitive, 100}))");
            prepared.SetObject(1, parameter);
            epService.EPAdministrator.Create(prepared).AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            epService.EPAdministrator.DestroyAllStatements();
        }

#if false
        private SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> it) {
            if (!it.IsEmpty() && it.First() is EventBean) {
                Fail("Iterator provides EventBean instances");
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
            var mapsColl = (ICollection<Map>) rows;
            var maps = mapsColl.ToArray();
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

                MyList = new List<string>();
                MyList.Add("1");
                MyList.Add("2");
                MyList.Add("3");
            }

            public string Selector { get; }

            public List<string> MyList { get; }

            public NestedMyEvent NestedMyEvent => new NestedMyEvent(MyList);

            public string[] GetTheArray()
            {
                return MyList.ToArray();
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