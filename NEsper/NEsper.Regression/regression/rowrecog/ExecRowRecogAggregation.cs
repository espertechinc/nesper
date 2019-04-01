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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogAggregation : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
    
            RunAssertionMeasureAggregation(epService);
            RunAssertionMeasureAggregationPartitioned(epService);
        }
    
        private void RunAssertionMeasureAggregation(EPServiceProvider epService) {
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, " +
                    "       C.TheString as c_string, " +
                    "       max(B.value) as maxb, " +
                    "       min(B.value) as minb, " +
                    "       2*min(B.value) as minb2x, " +
                    "       last(B.value) as lastb, " +
                    "       first(B.value) as firstb," +
                    "       count(B.value) as countb " +
                    "  all matches pattern (A B* C) " +
                    "  define " +
                    "   A as (A.value = 0)," +
                    "   B as (B.value != 1)," +
                    "   C as (C.value = 1)" +
                    ") " +
                    "order by a_string";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "a_string,c_string,maxb,minb,minb2x,firstb,lastb,countb".Split(',');
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E1", "E2", null, null, null, null, null, 0L}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E1", "E2", null, null, null, null, null, 0L}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E3", "E6", 5, 3, 6, 5, 3, 2L}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E1", "E2", null, null, null, null, null, 0L}, new object[] {"E3", "E6", 5, 3, 6, 5, 3, 2L}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E7", "E12", 7, -1, -2, 4, 2, 4L}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E1", "E2", null, null, null, null, null, 0L},
                        new object[]{"E3", "E6", 5, 3, 6, 5, 3, 2L},
                        new object[]{"E7", "E12", 7, -1, -2, 4, 2, 4L},
                    });
    
            stmt.Dispose();
        }
    
        private void RunAssertionMeasureAggregationPartitioned(EPServiceProvider epService) {
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  partition by cat" +
                    "  measures A.cat as cat, A.TheString as a_string, " +
                    "       D.TheString as d_string, " +
                    "       sum(C.value) as sumc, " +
                    "       sum(B.value) as sumb, " +
                    "       sum(B.value + A.value) as sumaplusb, " +
                    "       sum(C.value + A.value) as sumaplusc " +
                    "  all matches pattern (A B B C C D) " +
                    "  define " +
                    "   A as (A.value >= 10)," +
                    "   B as (B.value > 1)," +
                    "   C as (C.value < -1)," +
                    "   D as (D.value = 999)" +
                    ") order by cat";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "a_string,d_string,sumb,sumc,sumaplusb,sumaplusc".Split(',');
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", "x", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", "y", 20));
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", "x", 7));     // B
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", "y", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "x", 8));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "y", 2));
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", "x", -2));    // C
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "y", -7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", "x", -5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", "y", -4));
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", "y", 999));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E2", "E11", 7, -11, 47, 29}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E11", 7, -11, 47, 29}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", "x", 999));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E1", "E12", 15, -7, 35, 13}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E1", "E12", 15, -7, 35, 13}, new object[] {"E2", "E11", 7, -11, 47, 29}});
    
            stmt.Dispose();
        }
    }
} // end of namespace
