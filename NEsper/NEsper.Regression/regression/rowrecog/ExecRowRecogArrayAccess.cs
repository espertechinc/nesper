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


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogArrayAccess : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionSingleMultiMix(epService);
            RunAssertionMultiDepends(epService);
            RunAssertionMeasuresClausePresence(epService);
            RunAssertionLambda(epService);
        }
    
        private void RunAssertionSingleMultiMix(EPServiceProvider epService) {
            string[] fields = "a,b0,c,d0,e".Split(',');
            string text = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, C.TheString as c, D[0].TheString as d0, E.TheString as e" +
                    " pattern (A B+ C D+ E)" +
                    " define" +
                    " A as A.TheString like 'A%', " +
                    " B as B.TheString like 'B%'," +
                    " C as C.TheString like 'C%' and C.IntPrimitive = B[1].IntPrimitive," +
                    " D as D.TheString like 'D%'," +
                    " E as E.TheString like 'E%' and E.IntPrimitive = D[1].IntPrimitive and E.IntPrimitive = D[0].IntPrimitive" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            listener.Reset();
    
            SendEvents(epService, new object[][]{new object[] {"A1", 100}, new object[] {"B1", 50}, new object[] {"B2", 49}, new object[] {"C1", 49}, new object[] {"D1", 2}, new object[] {"D2", 2}, new object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1", "C1", "D1", "E1"});
    
            SendEvents(epService, new object[][]{new object[] {"A1", 100}, new object[] {"B1", 50}, new object[] {"C1", 49}, new object[] {"D1", 2}, new object[] {"D2", 2}, new object[] {"E1", 2}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"A1", 100}, new object[] {"B1", 50}, new object[] {"B2", 49}, new object[] {"C1", 49}, new object[] {"D1", 2}, new object[] {"D2", 3}, new object[] {"E1", 2}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"A1", 100}, new object[] {"B1", 50}, new object[] {"B2", 49}, new object[] {"C1", 49}, new object[] {"D1", 2}, new object[] {"D2", 2}, new object[] {"D3", 99}, new object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1", "C1", "D1", "E1"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiDepends(EPServiceProvider epService) {
            TryMultiDepends(epService, "A B A B C");
            TryMultiDepends(epService, "(A B)* C");
        }
    
        private void RunAssertionMeasuresClausePresence(EPServiceProvider epService) {
            TryMeasuresClausePresence(epService, "A as a_array, B as b");
            TryMeasuresClausePresence(epService, "B as b");
            TryMeasuresClausePresence(epService, "A as a_array");
            TryMeasuresClausePresence(epService, "1 as one");
        }
    
        private void RunAssertionLambda(EPServiceProvider epService) {
            string[] fieldsOne = "a0,a1,a2,b".Split(',');
            string eplOne = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b" +
                    " pattern (A* B)" +
                    " define" +
                    " B as (coalesce(A.sumOf(v => v.IntPrimitive), 0) + B.IntPrimitive) > 100" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eplOne);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            listener.Reset();
    
            SendEvents(epService, new object[][]{new object[] {"E1", 50}, new object[] {"E2", 49}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"E3", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E1", "E2", null, "E3"});
    
            SendEvents(epService, new object[][]{new object[] {"E4", 101}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{null, null, null, "E4"});
    
            SendEvents(epService, new object[][]{new object[] {"E5", 50}, new object[] {"E6", 51}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E5", null, null, "E6"});
    
            SendEvents(epService, new object[][]{new object[] {"E7", 10}, new object[] {"E8", 10}, new object[] {"E9", 79}, new object[] {"E10", 1}, new object[] {"E11", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E7", "E8", "E9", "E11"});
            stmt.Dispose();
    
            string[] fieldsTwo = "a[0].TheString,a[1].TheString,b.TheString".Split(',');
            string eplTwo = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A as a, B as b " +
                    " pattern (A+ B)" +
                    " define" +
                    " A as TheString like 'A%', " +
                    " B as TheString like 'B%' and B.IntPrimitive > A.sumOf(v => v.IntPrimitive)" +
                    ")";
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.Events += listener.Update;
            listener.Reset();
    
            SendEvents(epService, new object[][]{new object[] {"A1", 1}, new object[] {"A2", 2}, new object[] {"B1", 3}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"A2", null, "B1"});
    
            SendEvents(epService, new object[][]{new object[] {"A3", 1}, new object[] {"A4", 2}, new object[] {"B2", 4}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"A3", "A4", "B2"});
    
            SendEvents(epService, new object[][]{new object[] {"A5", -1}, new object[] {"B3", 0}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"A5", null, "B3"});
    
            SendEvents(epService, new object[][]{new object[] {"A6", 10}, new object[] {"B3", 9}, new object[] {"B4", 11}});
            SendEvents(epService, new object[][]{new object[] {"A7", 10}, new object[] {"A8", 9}, new object[] {"A9", 8}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"B5", 18}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"A8", "A9", "B5"});
    
            SendEvents(epService, new object[][]{new object[] {"A0", 10}, new object[] {"A11", 9}, new object[] {"A12", 8}, new object[] {"B6", 8}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"A13", 1}, new object[] {"A14", 1}, new object[] {"A15", 1}, new object[] {"A16", 1}, new object[] {"B7", 5}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"A13", "A14", "B7"});
    
            SendEvents(epService, new object[][]{new object[] {"A17", 1}, new object[] {"A18", 1}, new object[] {"B8", 1}});
            Assert.IsFalse(listener.IsInvoked);
    
            stmtTwo.Dispose();
        }
    
        private void TryMeasuresClausePresence(EPServiceProvider epService, string measures) {
            string text = "select * from SupportBean " +
                    "match_recognize (" +
                    " partition by TheString " +
                    " measures " + measures +
                    " pattern (A+ B)" +
                    " define" +
                    " B as B.IntPrimitive = A[0].IntPrimitive" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvents(epService, new object[][]{new object[] {"A", 1}, new object[] {"A", 0}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"B", 1}, new object[] {"B", 1}});
            Assert.IsNotNull(listener.AssertOneGetNewAndReset());
    
            SendEvents(epService, new object[][]{new object[] {"A", 2}, new object[] {"A", 3}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"B", 2}, new object[] {"B", 2}});
            Assert.IsNotNull(listener.AssertOneGetNewAndReset());
    
            stmt.Dispose();
        }
    
        private void TryMultiDepends(EPServiceProvider epService, string pattern) {
            string[] fields = "a0,a1,b0,b1,c".Split(',');
            string text = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A[0].TheString as a0, A[1].TheString as a1, B[0].TheString as b0, B[1].TheString as b1, C.TheString as c" +
                    " pattern (" + pattern + ")" +
                    " define" +
                    " A as TheString like 'A%', " +
                    " B as TheString like 'B%'," +
                    " C as TheString like 'C%' and " +
                    "   C.IntPrimitive = A[0].IntPrimitive and " +
                    "   C.IntPrimitive = B[0].IntPrimitive and " +
                    "   C.IntPrimitive = A[1].IntPrimitive and " +
                    "   C.IntPrimitive = B[1].IntPrimitive" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            listener.Reset();
    
            SendEvents(epService, new object[][]{new object[] {"A1", 1}, new object[] {"B1", 1}, new object[] {"A2", 1}, new object[] {"B2", 1}});
            epService.EPRuntime.SendEvent(new SupportBean("C1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "A2", "B1", "B2", "C1"});
    
            SendEvents(epService, new object[][]{new object[] {"A10", 1}, new object[] {"B10", 1}, new object[] {"A11", 1}, new object[] {"B11", 2}, new object[] {"C2", 2}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvents(epService, new object[][]{new object[] {"A20", 2}, new object[] {"B20", 2}, new object[] {"A21", 1}, new object[] {"B21", 2}, new object[] {"C3", 2}});
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void SendEvents(EPServiceProvider epService, object[][] objects) {
            foreach (object[] @object in objects) {
                epService.EPRuntime.SendEvent(new SupportBean((string) @object[0], (int) @object[1]));
            }
        }
    }
} // end of namespace
