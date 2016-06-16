///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionClausePresence  {
    
        [Test]
        public void TestMultimatchSelect() {    //When not measuring "B as b", B.size() is inaccessible.
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
    
            RunAssertionMeasurePresence(engine, 0, "B.size()", 1);
            RunAssertionMeasurePresence(engine, 0, "100+B.size()", 101);
            RunAssertionMeasurePresence(engine, 1000000, "B.anyOf(v=>TheString='E2')", true);
    
            RunAssertionDefineNotPresent(engine, true);
        }
    
        private void RunAssertionDefineNotPresent(EPServiceProvider engine, bool soda) {
            SupportUpdateListener listener = new SupportUpdateListener();
            string epl = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A as a, B as b" +
                    " pattern (A B)" +
                    ")";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(engine, soda, epl);
            stmt.AddListener(listener);
    
            string[] fields = "a,b".Split(',');
            SupportBean[] beans = new SupportBean[4];
            for (int i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }
    
            engine.EPRuntime.SendEvent(beans[0]);
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(beans[1]);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {beans[0], beans[1]});
    
            engine.EPRuntime.SendEvent(beans[2]);
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(beans[3]);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {beans[2], beans[3]});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMeasurePresence(EPServiceProvider engine, long baseTime, string select, object value) {
    
            engine.EPRuntime.SendEvent(new CurrentTimeEvent(baseTime));
            string epl = "select * from SupportBean  " +
                    "match_recognize (" +
                    "    measures A as a, A.TheString as id, " + select + " as val " +
                    "    pattern (A B*) " +
                    "    interval 1 minute " +
                    "    define " +
                    "        A as (A.IntPrimitive=1)," +
                    "        B as (B.IntPrimitive=2))";
            SupportUpdateListener listener = new SupportUpdateListener();
            engine.EPAdministrator.CreateEPL(epl).AddListener(listener);
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            engine.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            engine.EPRuntime.SendEvent(new CurrentTimeSpanEvent(baseTime + 60*1000*2));
            Assert.AreEqual(value, listener.GetNewDataListFlattened()[0].Get("val"));
    
            engine.EPAdministrator.DestroyAllStatements();
        }
    }
}
