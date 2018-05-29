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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableAccessDotMethod : RegressionExecution
    {

        public override void Run(EPServiceProvider epService)
        {
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }

            RunAssertionPlainPropDatetimeAndEnumerationAndMethod(epService);
            RunAssertionAggDatetimeAndEnumerationAndMethod(epService);
            RunAssertionNestedDotMethod(epService);
        }

        private void RunAssertionAggDatetimeAndEnumerationAndMethod(EPServiceProvider epService)
        {
            RunAggregationWDatetimeEtc(epService, false, false);
            RunAggregationWDatetimeEtc(epService, true, false);
            RunAggregationWDatetimeEtc(epService, false, true);
            RunAggregationWDatetimeEtc(epService, true, true);
        }

        private void RunAssertionPlainPropDatetimeAndEnumerationAndMethod(EPServiceProvider epService)
        {
            RunPlainPropertyWDatetimeEtc(epService, false, false);
            RunPlainPropertyWDatetimeEtc(epService, true, false);
            RunPlainPropertyWDatetimeEtc(epService, false, true);
            RunPlainPropertyWDatetimeEtc(epService, true, true);
        }

        private void RunPlainPropertyWDatetimeEtc(EPServiceProvider epService, bool grouped, bool soda)
        {
            string myBean = typeof(MyBean).MaskTypeName();
            SupportModelHelper.CreateByCompileOrParse(
                epService, soda, "create objectarray schema MyEvent as (p0 string)");
            SupportModelHelper.CreateByCompileOrParse(
                epService, soda, "create objectarray schema PopulateEvent as (" +
                                 "key string" +
                                 ", ts long" +
                                 ", mb " + myBean +
                                 ", mbarr " + myBean + "[]" +
                                 ", me MyEvent" +
                                 ", mearr MyEvent[])");

            string eplDeclare = "create table varaggPWD (key string" + (grouped ? " primary key" : "") +
                                ", ts long" +
                                ", mb " + myBean +
                                ", mbarr " + myBean + "[]" +
                                ", me MyEvent" +
                                ", mearr MyEvent[])";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            string key = grouped ? "[\"E1\"]" : "";
            string eplSelect = "select " +
                               "varaggPWD" + key + ".ts.getMinuteOfHour() as c0, " +
                               "varaggPWD" + key + ".mb.MyProperty as c1, " +
                               "varaggPWD" + key + ".mbarr.takeLast(1) as c2, " +
                               "varaggPWD" + key + ".me.p0 as c3, " +
                               "varaggPWD" + key + ".mearr.selectFrom(i => i.p0) as c4 " +
                               "from SupportBean_S0";
            var listener = new SupportUpdateListener();
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplSelect).Events += listener.Update;

            string eplMerge = "on PopulateEvent merge varaggPWD " +
                              "when not matched then insert " +
                              "select key, ts, mb, mbarr, me, mearr";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplMerge);

            object[] @event = MakePopulateEvent();
            epService.EPRuntime.SendEvent(@event, "PopulateEvent");
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EventBean output = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                output, "c0,c1,c3".Split(','),
                new object[] {55, "x", "p0value"});
            Assert.AreEqual(1, ((ICollection<object>) output.Get("c2")).Count);
            Assert.AreEqual("[0_p0, 1_p0]", output.Get("c4").RenderAny());

            epService.EPAdministrator.DestroyAllStatements();
        }

        private object[] MakePopulateEvent()
        {
            return new object[] {
                "E1",
                DateTimeParser.ParseDefaultMSec("2002-05-30T09:55:00.000"), // ts
                new MyBean(), // mb
                new MyBean[] {new MyBean(), new MyBean()}, // mbarr
                new object[] {"p0value"}, // me
                new object[][] {new object[] {"0_p0"}, new object[] {"1_p0"}} // mearr
            };
        }

        private void RunAggregationWDatetimeEtc(EPServiceProvider epService, bool grouped, bool soda)
        {
            string eplDeclare = "create table varaggWDE (" + (grouped ? "key string primary key, " : "") +
                                "a1 lastever(long), a2 window(*) @Type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            string eplInto = "into table varaggWDE " +
                             "select lastever(LongPrimitive) as a1, window(*) as a2 from SupportBean#time(10 seconds)" +
                             (grouped ? " group by TheString" : "");
            EPStatement stmtInto = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplInto);
            var expectedAggType = new object[][] {
                new object[] {"a1", typeof(long)},
                new object[] {"a2", typeof(SupportBean[])}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType, stmtInto.EventType, SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            string key = grouped ? "[\"E1\"]" : "";
            string eplGet = "select varaggWDE" + key + ".a1.after(150L) as c0, " +
                            "varaggWDE" + key + ".a2.countOf() as c1 from SupportBean_S0";
            EPStatement stmtGet = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplGet);
            var listener = new SupportUpdateListener();
            stmtGet.Events += listener.Update;
            var expectedGetType =
                new object[][] {
                    new object[] {"c0", typeof(bool?)},
                    new object[] {"c1", typeof(int)}
                };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedGetType, stmtGet.EventType, SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            string[] fields = "c0,c1".Split(',');
            MakeSendBean(epService, "E1", 10, 100);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, 1});

            MakeSendBean(epService, "E1", 20, 200);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, 2});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggWDE__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggWDE__public", false);
        }

        private void RunAssertionNestedDotMethod(EPServiceProvider epService)
        {
            TryAssertionNestedDotMethod(epService, true, false);
            TryAssertionNestedDotMethod(epService, false, false);
            TryAssertionNestedDotMethod(epService, true, true);
            TryAssertionNestedDotMethod(epService, false, true);
        }

        private void TryAssertionNestedDotMethod(EPServiceProvider epService, bool grouped, bool soda)
        {
            string eplDeclare = "create table varaggNDM (" +
                                (grouped ? "key string primary key, " : "") +
                                "windowSupportBean window(*) @Type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);

            string eplInto = "into table varaggNDM " +
                             "select window(*) as windowSupportBean from SupportBean#length(2)" +
                             (grouped ? " group by TheString" : "");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplInto);

            string key = grouped ? "[\"E1\"]" : "";
            string eplSelect = "select " +
                               "varaggNDM" + key + ".windowSupportBean.last(*).IntPrimitive as c0, " +
                               "varaggNDM" + key + ".windowSupportBean.window(*).countOf() as c1, " +
                               "varaggNDM" + key + ".windowSupportBean.window(IntPrimitive).take(1) as c2" +
                               " from SupportBean_S0";
            EPStatement stmtSelect = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            var expectedAggType = new object[][] {
                new object[] {"c0", typeof(int?)},
                new object[] {"c1", typeof(int)},
                new object[] {"c2", typeof(ICollection<int>)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedAggType, stmtSelect.EventType, SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            string[] fields = "c0,c1,c2".Split(',');
            MakeSendBean(epService, "E1", 10, 0);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {10, 1, Collections.SingletonList(10)});

            MakeSendBean(epService, "E1", 20, 0);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {20, 2, Collections.SingletonList(10)});

            MakeSendBean(epService, "E1", 30, 0);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), fields, new object[] {30, 2, Collections.SingletonList(20)});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggNDM__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varaggNDM__public", false);
        }

        private void MakeSendBean(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }

        public class MyBean
        {
            public string MyProperty => "x";
        }
    }
} // end of namespace
