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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertFalse;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogClausePresence : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionMeasurePresence(epService, 0, "B.Count", 1);
            RunAssertionMeasurePresence(epService, 0, "100+B.Count", 101);
            RunAssertionMeasurePresence(epService, 1000000, "B.AnyOf(v=>theString='E2')", true);
    
            RunAssertionDefineNotPresent(epService, true);
            RunAssertionDefineNotPresent(epService, false);
        }
    
        private void RunAssertionDefineNotPresent(EPServiceProvider engine, bool soda) {
            var listener = new SupportUpdateListener();
            string epl = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A as a, B as b" +
                    " pattern (A B)" +
                    ")";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(engine, soda, epl);
            stmt.AddListener(listener);
    
            string[] fields = "a,b".Split(',');
            var beans = new SupportBean[4];
            for (int i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }
    
            engine.EPRuntime.SendEvent(beans[0]);
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(beans[1]);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{beans[0], beans[1]});
    
            engine.EPRuntime.SendEvent(beans[2]);
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(beans[3]);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{beans[2], beans[3]});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMeasurePresence(EPServiceProvider engine, long baseTime, string select, Object value) {
    
            engine.EPRuntime.SendEvent(new CurrentTimeEvent(baseTime));
            string epl = "select * from SupportBean  " +
                    "match_recognize (" +
                    "    measures A as a, A.theString as id, " + select + " as val " +
                    "    pattern (A B*) " +
                    "    interval 1 minute " +
                    "    define " +
                    "        A as (A.intPrimitive=1)," +
                    "        B as (B.intPrimitive=2))";
            var listener = new SupportUpdateListener();
            engine.EPAdministrator.CreateEPL(epl).AddListener(listener);
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            engine.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            engine.EPRuntime.SendEvent(new CurrentTimeSpanEvent(baseTime + 60 * 1000 * 2));
            Assert.AreEqual(value, listener.GetNewDataListFlattened()[0].Get("val"));
    
            engine.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
