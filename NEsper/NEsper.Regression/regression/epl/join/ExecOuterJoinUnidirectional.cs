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

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecOuterJoinUnidirectional : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean_C)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            // all: unidirectional and full-outer-join
            RunAssertion2Stream(epService);
            RunAssertion3Stream(epService);
            RunAssertion3StreamMixed(epService);
            RunAssertion4StreamWhereClause(epService);
    
            // no-view-declared
            TryInvalid(epService,
                    "select * from SupportBean_A unidirectional full outer join SupportBean_B#keepall unidirectional",
                    "Error starting statement: The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");
    
            // not-all-unidirectional
            TryInvalid(epService,
                    "select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional full outer join SupportBean_C#keepall",
                    "Error starting statement: The unidirectional keyword must either apply to a single stream or all streams in a full outer join");
    
            // no iterate
            SupportMessageAssertUtil.TryInvalidIterate(epService,
                    "select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional",
                    "Iteration over a unidirectional join is not supported");
        }
    
        private void RunAssertion2Stream(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean_C), typeof(SupportBean_D)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select a.id as aid, b.id as bid from SupportBean_A as a unidirectional " +
                    "full outer join SupportBean_B as b unidirectional").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            AssertReceived2Stream(listener, "A1", null);
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            AssertReceived2Stream(listener, null, "B1");
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            AssertReceived2Stream(listener, null, "B2");
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            AssertReceived2Stream(listener, "A2", null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion3Stream(EPServiceProvider epService) {
            RunAssertion3StreamAllUnidirectional(epService, false);
            RunAssertion3StreamAllUnidirectional(epService, true);
        }
    
        private void RunAssertion3StreamAllUnidirectional(EPServiceProvider epService, bool soda) {
    
            string epl = "select * from SupportBean_A as a unidirectional " +
                    "full outer join SupportBean_B as b unidirectional " +
                    "full outer join SupportBean_C as c unidirectional";
            var listener = new SupportUpdateListener();
            SupportModelHelper.CreateByCompileOrParse(epService, soda, epl).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            AssertReceived3Stream(listener, "A1", null, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
            AssertReceived3Stream(listener, null, null, "C1");
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C2"));
            AssertReceived3Stream(listener, null, null, "C2");
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            AssertReceived3Stream(listener, "A2", null, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            AssertReceived3Stream(listener, null, "B1", null);
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            AssertReceived3Stream(listener, null, "B2", null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion3StreamMixed(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyCWindow#keepall as SupportBean_C");
            epService.EPAdministrator.CreateEPL("insert into MyCWindow select * from SupportBean_C");
            string epl = "select a.id as aid, b.id as bid, MyCWindow.id as cid, SupportBean_D.id as did " +
                    "from pattern[every a=SupportBean_A -> b=SupportBean_B] t1 unidirectional " +
                    "full outer join " +
                    "MyCWindow unidirectional " +
                    "full outer join " +
                    "SupportBean_D unidirectional";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_C("c1"));
            AssertReceived3StreamMixed(listener, null, null, "c1", null);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("a1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("b1"));
            AssertReceived3StreamMixed(listener, "a1", "b1", null, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("a2"));
            epService.EPRuntime.SendEvent(new SupportBean_B("b2"));
            AssertReceived3StreamMixed(listener, "a2", "b2", null, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_D("d1"));
            AssertReceived3StreamMixed(listener, null, null, null, "d1");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion4StreamWhereClause(EPServiceProvider epService) {
            string epl = "select * from SupportBean_A as a unidirectional " +
                    "full outer join SupportBean_B as b unidirectional " +
                    "full outer join SupportBean_C as c unidirectional " +
                    "full outer join SupportBean_D as d unidirectional " +
                    "where coalesce(a.id,b.id,c.id,d.id) in ('YES')";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            SendAssert(epService, listener, new SupportBean_A("A1"), false);
            SendAssert(epService, listener, new SupportBean_A("YES"), true);
            SendAssert(epService, listener, new SupportBean_C("YES"), true);
            SendAssert(epService, listener, new SupportBean_C("C1"), false);
            SendAssert(epService, listener, new SupportBean_D("YES"), true);
            SendAssert(epService, listener, new SupportBean_B("YES"), true);
            SendAssert(epService, listener, new SupportBean_B("B1"), false);
        }
    
        private void SendAssert(EPServiceProvider epService, SupportUpdateListener listener, SupportBeanBase @event, bool b) {
            epService.EPRuntime.SendEvent(@event);
            Assert.AreEqual(b, listener.GetAndClearIsInvoked());
        }
    
        private void AssertReceived2Stream(SupportUpdateListener listener, string a, string b) {
            string[] fields = "aid,bid".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{a, b});
        }
    
        private void AssertReceived3Stream(SupportUpdateListener listener, string a, string b, string c) {
            string[] fields = "a.id,b.id,c.id".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{a, b, c});
        }
    
        private void AssertReceived3StreamMixed(SupportUpdateListener listener, string a, string b, string c, string d) {
            string[] fields = "aid,bid,cid,did".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{a, b, c, d});
        }
    }
} // end of namespace
