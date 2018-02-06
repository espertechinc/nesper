///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableAccessAggregationState : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionNestedMultivalueAccess(epService, false, false);
            RunAssertionNestedMultivalueAccess(epService, true, false);
            RunAssertionNestedMultivalueAccess(epService, false, true);
            RunAssertionNestedMultivalueAccess(epService, true, true);
    
            RunAssertionAccessAggShare(epService);
        }
    
        private void RunAssertionNestedMultivalueAccess(EPServiceProvider epService, bool grouped, bool soda) {
    
            string eplDeclare = "create table varagg (" +
                    (grouped ? "key string primary key, " : "") + "windowSupportBean window(*) @Type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplDeclare);
    
            string eplInto = "into table varagg " +
                    "select window(*) as windowSupportBean from SupportBean#length(2)" +
                    (grouped ? " group by TheString" : "");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplInto);
    
            string key = grouped ? "[\"E1\"]" : "";
            string eplSelect = "select " +
                    "varagg" + key + ".windowSupportBean.last(*) as c0, " +
                    "varagg" + key + ".windowSupportBean.window(*) as c1, " +
                    "varagg" + key + ".windowSupportBean.first(*) as c2, " +
                    "varagg" + key + ".windowSupportBean.last(IntPrimitive) as c3, " +
                    "varagg" + key + ".windowSupportBean.window(IntPrimitive) as c4, " +
                    "varagg" + key + ".windowSupportBean.first(IntPrimitive) as c5" +
                    " from SupportBean_S0";
            EPStatement stmtSelect = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            var expectedAggType = new object[][]{
                    new object[] {"c0", typeof(SupportBean)}, new object[] {"c1", typeof(SupportBean[])}, new object[] {"c2", typeof(SupportBean)},
                    new object[] {"c3", typeof(int)}, new object[] {"c4", typeof(int[])}, new object[] {"c5", typeof(int)}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtSelect.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            string[] fields = "c0,c1,c2,c3,c4,c5".Split(',');
            SupportBean b1 = MakeSendBean(epService, "E1", 10);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{b1, new object[]{b1}, b1, 10, new int[]{10}, 10});
    
            SupportBean b2 = MakeSendBean(epService, "E1", 20);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{b2, new object[]{b1, b2}, b1, 20, new int[]{10, 20}, 10});
    
            SupportBean b3 = MakeSendBean(epService, "E1", 30);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{b3, new object[]{b2, b3}, b2, 30, new int[]{20, 30}, 20});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private void RunAssertionAccessAggShare(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table varagg (" +
                    "mywin window(*) @Type(SupportBean))");
    
            EPStatement stmtAgg = epService.EPAdministrator.CreateEPL("into table varagg " +
                    "select window(sb.*) as mywin from SupportBean#time(10 sec) as sb");
            var listener = new SupportUpdateListener();
            stmtAgg.Events += listener.Update;
            Assert.AreEqual(typeof(SupportBean[]), stmtAgg.EventType.GetPropertyType("mywin"));
    
            EPStatement stmtGet = epService.EPAdministrator.CreateEPL("select varagg.mywin as c0 from SupportBean_S0");
            stmtGet.Events += listener.Update;
            Assert.AreEqual(typeof(SupportBean[]), stmtGet.EventType.GetPropertyType("c0"));
    
            SupportBean b1 = MakeSendBean(epService, "E1", 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "mywin".Split(','), new object[]{new SupportBean[]{b1}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[]{new object[]{b1}});
    
            SupportBean b2 = MakeSendBean(epService, "E2", 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "mywin".Split(','), new object[]{new SupportBean[]{b1, b2}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[]{new object[]{b1, b2}});
        }
    
        private SupportBean MakeSendBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace
