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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableAccessDotMethod
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestAggDatetimeAndEnumerationAndMethod() {
            RunAggregationWDatetimeEtc(false, false);
            RunAggregationWDatetimeEtc(true, false);
            RunAggregationWDatetimeEtc(false, true);
            RunAggregationWDatetimeEtc(true, true);
        }
    
        [Test]
        public void TestPlainPropDatetimeAndEnumerationAndMethod()
        {
            RunPlainPropertyWDatetimeEtc(false, false);
            RunPlainPropertyWDatetimeEtc(true, false);
            RunPlainPropertyWDatetimeEtc(false, true);
            RunPlainPropertyWDatetimeEtc(true, true);
        }
    
        private void RunPlainPropertyWDatetimeEtc(bool grouped, bool soda)
        {
            var myBean = typeof(MyBean).FullName.Replace('+', '$');
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create objectarray schema MyEvent as (p0 string)");
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create objectarray schema PopulateEvent as (" +
                    "key string" +
                    ", ts long" +
                    ", mb " + myBean +
                    ", mbarr " + myBean + "[]" +
                    ", me MyEvent, mearr MyEvent[])");
    
            var eplDeclare = "create table varagg (key string" + (grouped ? " primary key" : "") +
                    ", ts long" +
                    ", mb " + myBean +
                    ", mbarr " + myBean + "[]" +
                    ", me MyEvent, mearr MyEvent[])";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var key = grouped ? "[\"E1\"]" : "";
            var eplSelect = "select " +
                    "varagg" + key + ".ts.getMinuteOfHour() as c0, " +
                    "varagg" + key + ".mb.get_MyProperty() as c1, " +
                    "varagg" + key + ".mbarr.takeLast(1) as c2, " +
                    "varagg" + key + ".me.p0 as c3, " +
                    "varagg" + key + ".mearr.selectFrom(i => i.p0) as c4 " +
                    "from SupportBean_S0";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplSelect).AddListener(_listener);
    
            var eplMerge = "on PopulateEvent merge varagg " +
                    "when not matched then insert " +
                    "select key, ts, mb, mbarr, me, mearr";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplMerge);
    
            var @event = MakePopulateEvent();
            _epService.EPRuntime.SendEvent(@event, "PopulateEvent");
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            var output = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(output, "c0,c1,c3".Split(','),
                    new object[] {55, "x", "p0value"});
            Assert.AreEqual(1, ((ICollection<object>) output.Get("c2")).Count);
            Assert.AreEqual("[0_p0, 1_p0]", output.Get("c4").Render());
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private object[] MakePopulateEvent() {
            return new object[] {
                    "E1",
                    DateTimeParser.ParseDefaultMSec("2002-05-30T9:55:00.000"), // ts
                    new MyBean(),   // mb
                    new MyBean[] {new MyBean(), new MyBean()},   // mbarr
                    new object[] {"p0value"},   // me
                    new object[][] {new object[]{"0_p0"}, new object[]{"1_p0"}}    // mearr
            };
        }
    
        private void RunAggregationWDatetimeEtc(bool grouped, bool soda)
        {
            var eplDeclare = "create table varagg (" + (grouped ? "key string primary key, " : "") +
                    "a1 lastever(long), a2 window(*) @type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var eplInto = "into table varagg " +
                    "select lastever(LongPrimitive) as a1, window(*) as a2 from SupportBean.win:time(10 seconds)" +
                    (grouped ? " group by TheString" : "");
            EPStatement stmtInto = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplInto);
            var expectedAggType = new object[][] { new object[] { "a1", typeof(long?) }, new object[] { "a2", typeof(SupportBean[]) } };
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtInto.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
    
            var key = grouped ? "[\"E1\"]" : "";
            var eplGet = "select varagg" + key + ".a1.after(150L) as c0, " +
                    "varagg" + key + ".a2.countOf() as c1 from SupportBean_S0";
            EPStatement stmtGet = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplGet);
            stmtGet.AddListener(_listener);
            var expectedGetType = new object[][] { new object[] { "c0", typeof(bool?) }, new object[] { "c1", typeof(int) } };
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedGetType, stmtGet.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
    
            var fields = "c0,c1".Split(',');
            MakeSendBean("E1", 10, 100);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {false, 1});
    
            MakeSendBean("E1", 20, 200);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {true, 2});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        [Test]
        public void TestNestedDotMethod()
        {
            RunAssertionNestedDotMethod(true, false);
            RunAssertionNestedDotMethod(false, false);
            RunAssertionNestedDotMethod(true, true);
            RunAssertionNestedDotMethod(false, true);
        }
    
        private void RunAssertionNestedDotMethod(bool grouped, bool soda)
        {
            var eplDeclare = "create table varagg (" +
                    (grouped ? "key string primary key, " : "") +
                    "windowSupportBean window(*) @type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var eplInto = "into table varagg " +
                    "select window(*) as windowSupportBean from SupportBean.win:length(2)" +
                    (grouped ? " group by TheString" : "");
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplInto);
    
            var key = grouped ? "[\"E1\"]" : "";
            var eplSelect = "select " +
                    "varagg" + key + ".windowSupportBean.last(*).IntPrimitive as c0, " +
                    "varagg" + key + ".windowSupportBean.window(*).countOf() as c1, " +
                    "varagg" + key + ".windowSupportBean.window(IntPrimitive).take(1) as c2" +
                    " from SupportBean_S0";
            EPStatement stmtSelect = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplSelect);
            stmtSelect.AddListener(_listener);
            var expectedAggType = new object[][]
            {
                new object[] { "c0", typeof(int?) }, 
                new object[] { "c1", typeof(int) }, 
                new object[] { "c2", typeof(ICollection<object>) }
            };
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtSelect.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
    
            var fields = "c0,c1,c2".Split(',');
            MakeSendBean("E1", 10, 0);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, 1, Collections.SingletonList(10)});
    
            MakeSendBean("E1", 20, 0);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {20, 2, Collections.SingletonList(10)});
    
            MakeSendBean("E1", 30, 0);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {30, 2, Collections.SingletonList(20)});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private SupportBean MakeSendBean(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        public class MyBean
        {
            public string MyProperty
            {
                get { return "x"; }
            }
        }
    }
}
