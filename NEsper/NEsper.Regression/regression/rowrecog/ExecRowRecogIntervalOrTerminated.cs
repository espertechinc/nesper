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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogIntervalOrTerminated : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("TemperatureSensorEvent",
                    "id,device,temp".Split(','), new object[]{typeof(string), typeof(int), typeof(double)});
    
            RunAssertionDocSample(epService);
    
            RunAssertion_A_Bstar(epService, false);
    
            RunAssertion_A_Bstar(epService, true);
    
            RunAssertion_Astar(epService);
    
            RunAssertion_A_Bplus(epService);
    
            RunAssertion_A_Bstar_or_Cstar(epService);
    
            RunAssertion_A_B_Cstar(epService);
    
            RunAssertion_A_B(epService);
    
            RunAssertion_A_Bstar_or_C(epService);
    
            RunAssertion_A_parenthesisBstar(epService);
        }
    
        private void RunAssertion_A_Bstar_or_C(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a,b0,b1,b2,c".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, B[2].TheString as b2, C.TheString as c " +
                    " pattern (A (B* | C))" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'," +
                    " C as C.TheString like 'C%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", null, null, null, "C1"});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", null, null, null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", "B1", "B2", null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            SendTimer(isolated, 10000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", null, null, null, null});
    
            SendTimer(isolated, int.MaxValue);
            Assert.IsFalse(listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_B(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            // the interval is not effective
            string[] fields = "a,b".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, B.TheString as b" +
                    " pattern (A B)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1"});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", "B2"});
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertionDocSample(EPServiceProvider epService) {
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a_id,count_b,first_b,last_b".Split(',');
            string text = "select * from TemperatureSensorEvent\n" +
                    "match_recognize (\n" +
                    "  partition by device\n" +
                    "  measures A.id as a_id, count(B.id) as count_b, first(B.id) as first_b, last(B.id) as last_b\n" +
                    "  pattern (A B*)\n" +
                    "  interval 5 seconds or terminated\n" +
                    "  define\n" +
                    "    A as A.temp > 100,\n" +
                    "    B as B.temp > 100)";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTemperatureEvent(isolated, "E1", 1, 98);
            SendTemperatureEvent(isolated, "E2", 1, 101);
            SendTemperatureEvent(isolated, "E3", 1, 102);
            SendTemperatureEvent(isolated, "E4", 1, 101);   // falls below
            Assert.IsFalse(listener.IsInvoked);
    
            SendTemperatureEvent(isolated, "E5", 1, 100);   // falls below
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L, "E3", "E4"});
    
            SendTimer(isolated, int.MaxValue);
            Assert.IsFalse(listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_B_Cstar(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a,b,c0,c1,c2".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, B.TheString as b, " +
                    "C[0].TheString as c0, C[1].TheString as c1, C[2].TheString as c2 " +
                    " pattern (A B C*)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'," +
                    " C as C.TheString like 'C%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C2"));
            Assert.IsFalse(listener.IsInvoked);
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1", "C1", "C2", null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X2"));
            Assert.IsFalse(listener.IsInvoked);
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", "B4", null, null, null});
    
            SendTimer(isolated, 20000);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B5"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C3"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(isolated, 30000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A4", "B5", "C3", null, null});
    
            SendTimer(isolated, int.MaxValue);
            Assert.IsFalse(listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_Bstar_or_Cstar(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a,b0,b1,c0,c1".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, " +
                    "B[0].TheString as b0, B[1].TheString as b1, " +
                    "C[0].TheString as c0, C[1].TheString as c1 " +
                    " pattern (A (B* | C*))" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'," +
                    " C as C.TheString like 'C%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", null, null, null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", null, null, null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"A2", null, null, "C1", null}});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C2"));
            Assert.IsFalse(listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_Bplus(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a,b0,b1,b2".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, B[2].TheString as b2" +
                    " pattern (A B+)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", "B2", null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B4"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X3", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A4", "B3", "B4", null});
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_Astar(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a0,a1,a2,a3,a4".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, A[3].TheString as a3, A[4].TheString as a4" +
                    " pattern (A*)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as TheString like 'A%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "A2", null, null, null});
    
            SendTimer(isolated, 2000);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A5"));
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(isolated, 12000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", "A4", "A5", null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A6"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", "A4", "A5", "A6", null});
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B3"));
            Assert.IsFalse(listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_Bstar(EPServiceProvider epService, bool allMatches) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a,b0,b1,b2".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, B[2].TheString as b2" +
                    (allMatches ? " all matches" : "") +
                    " pattern (A B*)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like \"A%\"," +
                    " B as B.TheString like \"B%\"" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // test output by terminated because of misfit event
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            if (!allMatches) {
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1", null, null});
            } else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields,
                        new object[][]{new object[] {"A1", "B1", null, null}, new object[] {"A1", null, null, null}});
            }
    
            SendTimer(isolated, 20000);
            Assert.IsFalse(listener.IsInvoked);
    
            // test output by timer expiry
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(isolated, 29999);
    
            SendTimer(isolated, 30000);
            if (!allMatches) {
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", "B2", null, null});
            } else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields,
                        new object[][]{new object[] {"A2", "B2", null, null}, new object[] {"A2", null, null, null}});
            }
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
    
            EPStatement stmtFromModel = SupportModelHelper.CompileCreate(epService, text);
            stmtFromModel.Dispose();
        }
    
        private void RunAssertion_A_parenthesisBstar(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            string[] fields = "a,b0,b1,b2".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, B[2].TheString as b2" +
                    " pattern (A (B)*)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like \"A%\"," +
                    " B as B.TheString like \"B%\"" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // test output by terminated because of misfit event
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            Assert.IsFalse(listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1", null, null});
    
            SendTimer(isolated, 20000);
            Assert.IsFalse(listener.IsInvoked);
    
            // test output by timer expiry
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(isolated, 29999);
    
            SendTimer(isolated, 30000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", "B2", null, null});
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
    
            EPStatement stmtFromModel = SupportModelHelper.CompileCreate(epService, text);
            stmtFromModel.Dispose();
        }
    
        private void SendTemperatureEvent(EPServiceProviderIsolated isolated, string id, int device, double temp) {
            isolated.EPRuntime.SendEvent(new object[]{id, device, temp}, "TemperatureSensorEvent");
        }
    
        private void SendTimer(EPServiceProviderIsolated isolated, long time) {
            var theEvent = new CurrentTimeEvent(time);
            isolated.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
