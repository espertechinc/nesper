///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.bookexample;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.bean.lrreport;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestEnumDataSources
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddEventType("SupportBean_A", typeof(SupportBean_A));
            config.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            config.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            config.AddEventType("SupportCollection", typeof(SupportCollection));
            config.AddEventType(typeof(MyCustomEnumEvent));
            config.AddImport(typeof(LocationReportFactory));
            config.EngineDefaults.ExpressionConfig.IsUdfCache = false;
            config.AddPlugInSingleRowFunction("MakeSampleList", typeof(SupportBean_ST0_Container).FullName, "MakeSampleList");
            config.AddPlugInSingleRowFunction("MakeSampleArray", typeof(SupportBean_ST0_Container).FullName, "MakeSampleArray");
            config.AddPlugInSingleRowFunction("MakeSampleListString", typeof(SupportCollection).FullName, "MakeSampleListString");
            config.AddPlugInSingleRowFunction("MakeSampleArrayString", typeof(SupportCollection).FullName, "MakeSampleArrayString");
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestSubstitutionParameter()
        {
            TrySubstitutionParameter(new int?[] {1, 10, 100});
            TrySubstitutionParameter(new Object[] {1, 10, 100});
            TrySubstitutionParameter(new int[] {1, 10, 100});
        }

        private void TrySubstitutionParameter(Object parameter)
        {
            SupportUpdateListener listener = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL("select * from SupportBean(?.sequenceEqual({1, IntPrimitive, 100}))");
            prepared.SetObject(1, parameter);
            _epService.EPAdministrator.Create(prepared).Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestTableRow()
        {
            // test table access expression
            _epService.EPAdministrator.CreateEPL("create table MyTableUnkeyed(theWindow window(*) @type(SupportBean))");
            _epService.EPAdministrator.CreateEPL("into table MyTableUnkeyed select window(*) as theWindow from SupportBean.win:time(30)");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));

            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select MyTableUnkeyed.theWindow.anyOf(v=>IntPrimitive=10) as c0 from SupportBean_A");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_A("A0"));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("c0"));
            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestPatternFilter()
        {
            var stmt = _epService.EPAdministrator.CreateEPL("select * from pattern [ ([2]a=SupportBean_ST0) -> b=SupportBean(IntPrimitive > a.max(i -> p00))]");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 15));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 15));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E4", 16));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a[0].Id,a[1].Id,b.TheString".Split(','), new Object[] { "E1", "E2", "E4" });
            stmt.Dispose();

            stmt = _epService.EPAdministrator.CreateEPL("select * from pattern [ a=SupportBean_ST0 until b=SupportBean -> c=SupportBean(IntPrimitive > a.sumOf(i => p00))]");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E10", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E11", 15));
            _epService.EPRuntime.SendEvent(new SupportBean("E12", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("E13", 25));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E14", 26));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a[0].Id,a[1].Id,b.TheString,c.TheString".Split(','), new Object[] { "E10", "E11", "E12", "E14" });
        }

        [Test]
        public void TestMatchRecognize()
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

            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            stmtOne.Events += _listener.Update;
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[] { "A1", "A2", "A3" });

            _epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A5", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("A6", 3));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("A7", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[] { "A4", "A5", "A7" });
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

            var stmtTwo = _epService.EPAdministrator.CreateEPL(textTwo);
            stmtTwo.Events += _listener.Update;
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] { true });

            _epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 15));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] { false });
        }

        [Test]
        public void TestEnumObject()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnumTwo));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportEnumTwoEvent));
    
            var fields = "c0,c1".Split(',');
            _epService.EPAdministrator.CreateEPL("select " +
                    "SupportEnumTwo.ENUM_VALUE_1.GetMystrings().anyOf(v => v = id) as c0, " +
                    "value.GetMystrings().anyOf(v => v = '2') as c1 " +
                    "from SupportEnumTwoEvent").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportEnumTwoEvent("0", SupportEnumTwo.ENUM_VALUE_1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false});
    
            _epService.EPRuntime.SendEvent(new SupportEnumTwoEvent("2", SupportEnumTwo.ENUM_VALUE_2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
        }
    
        [Test]
        public void TestSortedMaxMinBy()
        {
            var fields = "c0,c1,c2,c3,c4".Split(',');
    
            var eplWindowAgg = "select " +
                    "sorted(TheString).allOf(x => x.IntPrimitive < 5) as c0," +
                    "maxby(TheString).allOf(x => x.IntPrimitive < 5) as c1," +
                    "minby(TheString).allOf(x => x.IntPrimitive < 5) as c2," +
                    "maxbyever(TheString).allOf(x => x.IntPrimitive < 5) as c3," +
                    "minbyever(TheString).allOf(x => x.IntPrimitive < 5) as c4" +
                    " from SupportBean.win:length(5)";
            var stmtWindowAgg = _epService.EPAdministrator.CreateEPL(eplWindowAgg);
            stmtWindowAgg.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, true, true, true});
        }
    
        [Test]
        public void TestJoin()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SelectorEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(ContainerEvent));
    
            var stmt = _epService.EPAdministrator.CreateEPL("" +
                    "select * from SelectorEvent.win:keepall() as sel, ContainerEvent.win:keepall() as cont " +
                    "where cont.items.anyOf(i => sel.selector = i.selected)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SelectorEvent("S1", "sel1"));
            _epService.EPRuntime.SendEvent(new ContainerEvent("C1", new ContainedItem("I1", "sel1")));
            Assert.IsTrue(listener.IsInvoked);
        }
    
        [Test]
        public void TestPrevWindowSorted()
        {
            var stmt = _epService.EPAdministrator.CreateEPL("select Prevwindow(st0) as val0, Prevwindow(st0).EsperInternalNoop() as val1 " +
                    "from SupportBean_ST0.ext:sort(3, p00 asc) as st0");
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0,val1".Split(','), new Type[]
            {
                typeof(SupportBean_ST0[]), typeof(ICollection<object>)
            });
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 5));
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 6));
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1,E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", 4));
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E3,E1,E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E5", 3));
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E5,E3,E1");
            _listener.Reset();
            stmt.Dispose();
    
            // Scalar version
            String[] fields = {"val0"};
            var stmtScalar = _epService.EPAdministrator.CreateEPL("select Prevwindow(id).Where(x => x not like '%ignore%') as val0 " +
                    "from SupportBean_ST0.win:keepall() as st0");
            stmtScalar.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, fields, new Type[]{typeof(ICollection<object>)});
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 5));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E2ignore", 6));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", 4));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E3", "E1");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ignoreE5", 3));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E3", "E1");
            _listener.Reset();
        }
    
        [Test]
        public void TestNamedWindow()
        {
            // test named window
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean_ST0");
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_ST0");
            const string eplNamedWindow = "select MyWindow.allOf(x => x.p00 < 5) as allOfX from SupportBean.win:keepall()";
            var stmtNamedWindow = _epService.EPAdministrator.CreateEPL(eplNamedWindow);
            stmtNamedWindow.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtNamedWindow.EventType, "allOfX".Split(','), new Type[]{ typeof(bool) });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            stmtNamedWindow.Dispose();
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
    
            // test named window correlated
            const string eplNamedWindowCorrelated = "select MyWindow(key0 = sb.TheString).allOf(x => x.p00 < 5) as allOfX from SupportBean.win:keepall() sb";
            var stmtNamedWindowCorrelated = _epService.EPAdministrator.CreateEPL(eplNamedWindowCorrelated);
            stmtNamedWindowCorrelated.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", "KEY1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("KEY1", 0));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtNamedWindowCorrelated.Dispose();
        }
    
        [Test]
        public void TestSubselect()
        {
            // test subselect-wildcard
            const string eplSubselect = "select (select * from SupportBean_ST0.win:keepall()).allOf(x => x.p00 < 5) as allOfX from SupportBean.win:keepall()";
            var stmtSubselect = _epService.EPAdministrator.CreateEPL(eplSubselect);
            stmtSubselect.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselect.Dispose();
    
            // test subselect scalar return
            const string eplSubselectScalar = "select (select id from SupportBean_ST0.win:keepall()).allOf(x => x  like '%B%') as allOfX from SupportBean.win:keepall()";
            var stmtSubselectScalar = _epService.EPAdministrator.CreateEPL(eplSubselectScalar);
            stmtSubselectScalar.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("B1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("A1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselectScalar.Dispose();
    
            // test subselect-correlated scalar return
            const string eplSubselectScalarCorrelated = "select (select key0 from SupportBean_ST0.win:keepall() st0 where st0.id = sb.TheString).allOf(x => x  like '%hello%') as allOfX from SupportBean.win:keepall() sb";
            var stmtSubselectScalarCorrelated = _epService.EPAdministrator.CreateEPL(eplSubselectScalarCorrelated);
            stmtSubselectScalarCorrelated.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("A1", "hello", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("A2", "hello", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("A3", "test", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 1));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtSubselectScalarCorrelated.Dispose();

            // test subselect multivalue return
            var fields = new String[] { "id", "p00" };
            var eplSubselectMultivalue = "select (select id, p00 from SupportBean_ST0.win:keepall()).take(10) as c0 from SupportBean";
            var stmtSubselectMultivalue = _epService.EPAdministrator.CreateEPL(eplSubselectMultivalue);
            stmtSubselectMultivalue.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("B1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            assertPropsMapRows(_listener.AssertOneGetNewAndReset().Get("c0").Unwrap<DataMap>(), fields, new Object[][] { new object[] { "B1", 10 } });

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("B2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            assertPropsMapRows(_listener.AssertOneGetNewAndReset().Get("c0").Unwrap<DataMap>(), fields, new Object[][] { new object[] { "B1", 10 }, new object[] { "B2", 20 } });
            stmtSubselectMultivalue.Dispose();

            // test subselect that delivers events
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.CreateEPL("create schema AEvent (symbol string)");
            _epService.EPAdministrator.CreateEPL("create schema BEvent (a AEvent)");
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "select (select a from BEvent.win:keepall()).anyOf(v => symbol = 'GE') as flag from SupportBean");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(MakeBEvent("XX"), "BEvent");
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("flag"));

            _epService.EPRuntime.SendEvent(MakeBEvent("GE"), "BEvent");
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("flag"));

        }
    
        [Test]
        public void TestVariable()
        {
            _epService.EPAdministrator.CreateEPL("create variable string[] myvar = { 'E1', 'E3' }");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(myvar.anyOf(v => v = TheString))").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestAccessAggregation()
        {
            var fields = new String[] {"val0", "val1", "val2", "val3", "val4"};
    
            // test Window(*) and First(*)
            const string eplWindowAgg =
                "select " +
                "window(*).allOf(x => x.IntPrimitive < 5) as val0," +
                "first(*).allOf(x => x.IntPrimitive < 5) as val1," +
                "first(*, 1).allOf(x => x.IntPrimitive < 5) as val2," +
                "last(*).allOf(x => x.IntPrimitive < 5) as val3," +
                "last(*, 1).allOf(x => x.IntPrimitive < 5) as val4" +
                " from SupportBean.win:length(2)";
            var stmtWindowAgg = _epService.EPAdministrator.CreateEPL(eplWindowAgg);
            stmtWindowAgg.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, null, true, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, true, false, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false, true, true, false});
    
            stmtWindowAgg.Dispose();
    
            // test scalar: Window(*) and First(*)
            var eplWindowAggScalar = "select " +
                    "window(IntPrimitive).allOf(x => x < 5) as val0," +
                    "first(IntPrimitive).allOf(x => x < 5) as val1," +
                    "first(IntPrimitive, 1).allOf(x => x < 5) as val2," +
                    "last(IntPrimitive).allOf(x => x < 5) as val3," +
                    "last(IntPrimitive, 1).allOf(x => x < 5) as val4" +
                    " from SupportBean.win:length(2)";
            var stmtWindowAggScalar = _epService.EPAdministrator.CreateEPL(eplWindowAggScalar);
            stmtWindowAggScalar.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, null, true, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, true, false, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false, true, true, false});
    
            stmtWindowAggScalar.Dispose();
        }
    
        [Test]
        public void TestProperty()
        {
            // test fragment type - collection inside
            var eplFragment = "select contained.allOf(x => x.p00 < 5) as allOfX from SupportBean_ST0_Container.win:keepall()";
            var stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("ID1,KEY1,1"));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("allOfX"));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("ID1,KEY1,10"));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("allOfX"));
            stmtFragment.Dispose();
    
            // test array and iterable
            var fields = "val0,val1".Split(',');
            eplFragment = "select Intarray.sumOf() as val0, " +
                    "Intiterable.sumOf() as val1 " +
                    " from SupportCollection.win:keepall()";
            stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeNumeric("5,6,7"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{5 + 6 + 7, 5 + 6 + 7});
    
            // test map event type with object-array prop
            _epService.EPAdministrator.Configuration.AddEventType(typeof(BookDesc));
            _epService.EPAdministrator.CreateEPL("create schema MySchema (books BookDesc[])");
    
            var stmt = _epService.EPAdministrator.CreateEPL("select books.Max(i => i.price) as mymax from MySchema");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var @event = Collections.SingletonDataMap("books", new BookDesc[]{new BookDesc("1", "book1", "dave", 1.00, null)});
            _epService.EPRuntime.SendEvent(@event, "MySchema");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[] {1.0});
    
            // test method invocation variations returning list/array of string and test UDF +property as well
            RunAssertionMethodInvoke("select e.TheList.anyOf(v => v = selector) as flag from MyCustomEnumEvent e");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("convertToArray", typeof(MyCustomEnumEvent).FullName, "ConvertToArray");
            RunAssertionMethodInvoke("select convertToArray(TheList).anyOf(v => v = selector) as flag from MyCustomEnumEvent e");
            RunAssertionMethodInvoke("select TheArray.anyOf(v => v = selector) as flag from MyCustomEnumEvent e");
            RunAssertionMethodInvoke("select e.TheArray.anyOf(v => v = selector) as flag from MyCustomEnumEvent e");
            RunAssertionMethodInvoke("select e.TheList.anyOf(v => v = e.selector) as flag from pattern[every e=MyCustomEnumEvent]");
            RunAssertionMethodInvoke("select e.NestedMyEvent.MyNestedList.anyOf(v => v = e.selector) as flag from pattern[every e=MyCustomEnumEvent]");
            RunAssertionMethodInvoke("select " + typeof(MyCustomEnumEvent).FullName + ".ConvertToArray(TheList).anyOf(v => v = selector) as flag from MyCustomEnumEvent e");
        }
    
        private void RunAssertionMethodInvoke(String epl)
        {
            var fields = "flag".Split(',');
            var stmtMethodAnyOf = _epService.EPAdministrator.CreateEPL(epl);
            stmtMethodAnyOf.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new MyCustomEnumEvent("1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true});

            _epService.EPRuntime.SendEvent(new MyCustomEnumEvent("4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false});
    
            stmtMethodAnyOf.Dispose();
        }
    
        [Test]
        public void TestPrevFuncs()
        {
            // test Prevwindow(*) etc
            var fields = new String[] {"val0", "val1", "val2"};
            var epl = "select " +
                    "prevwindow(sb).allOf(x => x.IntPrimitive < 5) as val0," +
                    "prev(sb,1).allOf(x => x.IntPrimitive < 5) as val1," +
                    "prevtail(sb,1).allOf(x => x.IntPrimitive < 5) as val2" +
                    " from SupportBean.win:length(2) as sb";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, true});
            stmt.Dispose();
    
            // test scalar Prevwindow(property) etc
            var eplScalar = "select " +
                    "prevwindow(IntPrimitive).allOf(x => x < 5) as val0," +
                    "prev(IntPrimitive,1).allOf(x => x < 5) as val1," +
                    "prevtail(IntPrimitive,1).allOf(x => x < 5) as val2" +
                    " from SupportBean.win:length(2) as sb";
            var stmtScalar = _epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{false, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{true, true, true});
        }
    
        [Test]
        public void TestUDFStaticMethod()
        {
            var fields = "val1,val2,val3,val4".Split(',');
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean_ST0_Container));
            var epl = "select " +
                    "SupportBean_ST0_Container.MakeSampleList().Where(x => x.p00 < 5) as val1, " +
                    "SupportBean_ST0_Container.MakeSampleArray().Where(x => x.p00 < 5) as val2, " +
                    "MakeSampleList().Where(x => x.p00 < 5) as val3, " +
                    "MakeSampleArray().Where(x => x.p00 < 5) as val4 " +
                    "from SupportBean.win:length(2) as sb";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            
            SupportBean_ST0_Container.Samples = new String[] {"E1,1", "E2,20", "E3,3"};
            _epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields) {
                var result = _listener.AssertOneGetNew().Get(field).UnwrapIntoArray<SupportBean_ST0>();
                Assert.AreEqual(2, result.Length, "Failed for field " + field);
            }
            _listener.Reset();
    
            SupportBean_ST0_Container.Samples = null;
            _epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields) {
                Assert.IsNull(_listener.AssertOneGetNew().Get(field));
            }
            _listener.Reset();
    
            SupportBean_ST0_Container.Samples = new String[0];
            _epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields) {
                var result = _listener.AssertOneGetNew().Get(field).UnwrapIntoArray<SupportBean_ST0>();
                Assert.AreEqual(0, result.Length);
            }
            _listener.Reset();
            stmt.Dispose();
    
            // test UDF returning scalar values collection
            fields = "val0,val1,val2,val3".Split(',');
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportCollection));
            var eplScalar = "select " +
                    "SupportCollection.MakeSampleListString().Where(x => x != 'E1') as val0, " +
                    "SupportCollection.MakeSampleArrayString().Where(x => x != 'E1') as val1, " +
                    "makeSampleListString().Where(x => x != 'E1') as val2, " +
                    "makeSampleArrayString().Where(x => x != 'E1') as val3 " +
                    "from SupportBean.win:length(2) as sb";
            var stmtScalar = _epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtScalar.EventType, fields, new Type[]
            {
                typeof(ICollection<object>), typeof(ICollection<object>),
                typeof(ICollection<object>), typeof(ICollection<object>)
            });
    
            SupportCollection.SampleCSV = "E1,E2,E3";
            _epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields) {
                LambdaAssertionUtil.AssertValuesArrayScalar(_listener, field, "E2", "E3");
            }
            _listener.Reset();
    
            SupportCollection.SampleCSV = null;
            _epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields) {
                LambdaAssertionUtil.AssertValuesArrayScalar(_listener, field, null);
            }
            _listener.Reset();
    
            SupportCollection.SampleCSV = "";
            _epService.EPRuntime.SendEvent(new SupportBean());
            foreach (var field in fields) {
                LambdaAssertionUtil.AssertValuesArrayScalar(_listener, field);
            }
            _listener.Reset();
        }
    
        private SupportBean_ST0[] ToArray(ICollection<SupportBean_ST0> it)
        {
            if (!it.IsEmpty() && it.First() is EventBean)
            {
                Assert.Fail("GetEnumerator provides EventBean instances");
            }
            return it.ToArray();
        }

        private IDictionary<string,object> MakeBEvent(String symbol)
        {
            var map = new Dictionary<string, object>();
            map.Put("a", Collections.SingletonDataMap("symbol", symbol));
            return map;
        }

        private void assertPropsMapRows(ICollection<IDictionary<string,object>> rows, String[] fields, Object[][] objects)
        {
            var mapsColl = rows;
            var maps = mapsColl.ToArray();
            EPAssertionUtil.AssertPropsPerRow(maps, fields, objects);
        }
    
        public class SelectorEvent
        {
            public SelectorEvent(String selectorId, String selector)
            {
                SelectorId = selectorId;
                Selector = selector;
            }

            public string SelectorId { get; private set; }

            public string Selector { get; private set; }
        }
    
        public class ContainerEvent
        {
            public ContainerEvent(String containerId, params ContainedItem[] items)
            {
                ContainerId = containerId;
                Items = items;
            }

            public string ContainerId { get; private set; }

            public ContainedItem[] Items { get; private set; }
        }
    
        public class ContainedItem
        {
            public ContainedItem(String itemId, String selected)
            {
                ItemId = itemId;
                Selected = selected;
            }

            public string ItemId { get; private set; }

            public string Selected { get; private set; }
        }
    
        public class NestedMyEvent
        {
            public NestedMyEvent(IList<String> myList)
            {
                MyNestedList = myList;
            }

            public IList<string> MyNestedList { get; private set; }
        }
    
        public class SupportEnumTwoEvent
        {
            public SupportEnumTwoEvent(String id, SupportEnumTwo value)
            {
                Id = id;
                Value = value;
            }

            public string Id { get; private set; }

            public SupportEnumTwo Value { get; private set; }
        }
    }

    public class MyCustomEnumEvent
    {
        public MyCustomEnumEvent(String selector)
        {
            Selector = selector;
    
            TheList = new List<String>();
            TheList.Add("1");
            TheList.Add("2");
            TheList.Add("3");
        }

        public string Selector { get; private set; }

        public IList<string> TheList { get; private set; }

        public string[] TheArray
        {
            get { return TheList.ToArray(); }
        }

        public TestEnumDataSources.NestedMyEvent NestedMyEvent
        {
            get { return new TestEnumDataSources.NestedMyEvent(TheList); }
        }

        public static String[] ConvertToArray(IList<String> list)
        {
            return list.ToArray();
        }
    }
}
