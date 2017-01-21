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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionIntervalOrTerminated
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType("TemperatureSensorEvent",
                    "id,device,temp".Split(','), new Object[] {typeof(string), typeof(int), typeof(double)});
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestOrTerminated()
        {
            RunAssertionDocSample();
    
            RunAssertion_A_Bstar(false);
    
            RunAssertion_A_Bstar(true);
    
            RunAssertion_Astar();
    
            RunAssertion_A_Bplus();
    
            RunAssertion_A_Bstar_or_Cstar();
    
            RunAssertion_A_B_Cstar();
    
            RunAssertion_A_B();
    
            RunAssertion_A_Bstar_or_C();
    
            RunAssertion_A_parenthesisBstar();
        }
    
        private void RunAssertion_A_Bstar_or_C()
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a,b0,b1,b2,c".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
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
            stmt.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A1", null, null, null, "C1"});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A2", null, null, null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A2", "B1", "B2", null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            SendTimer(isolated, 10000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A3", null, null, null, null});
    
            SendTimer(isolated, int.MaxValue);
            Assert.IsFalse(_listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_B()
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            // the interval is not effective
            String[] fields = "a,b".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    " measures A.TheString as a, B.TheString as b" +
                    " pattern (A B)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            stmt.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "B1"});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A3", "B2"});
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertionDocSample()
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a_id,count_b,first_b,last_b".Split(',');
            String text = "select * from TemperatureSensorEvent\n" +
                    "match_recognize (\n" +
                    "  partition by device\n" +
                    "  measures A.id as a_id, Count(B.id) as count_b, First(B.id) as first_b, Last(B.id) as last_b\n" +
                    "  pattern (A B*)\n" +
                    "  interval 5 seconds or terminated\n" +
                    "  define\n" +
                    "    A as A.temp > 100,\n" +
                    "    B as B.temp > 100)";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            stmt.Events += _listener.Update;
    
            SendTemperatureEvent(isolated, "E1", 1, 98);
            SendTemperatureEvent(isolated, "E2", 1, 101);
            SendTemperatureEvent(isolated, "E3", 1, 102);
            SendTemperatureEvent(isolated, "E4", 1, 101);   // falls below
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTemperatureEvent(isolated, "E5", 1, 100);   // falls below
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 2L, "E3", "E4"});
    
            SendTimer(isolated, int.MaxValue);
            Assert.IsFalse(_listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_B_Cstar()
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a,b,c0,c1,c2".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
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
            stmt.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C2"));
            Assert.IsFalse(_listener.IsInvoked);
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A1", "B1", "C1", "C2", null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X2"));
            Assert.IsFalse(_listener.IsInvoked);
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A3", "B4", null, null, null});
    
            SendTimer(isolated, 20000);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B5"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C3"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(isolated, 30000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A4", "B5", "C3", null, null});
    
            SendTimer(isolated, int.MaxValue);
            Assert.IsFalse(_listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_Bstar_or_Cstar()
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a,b0,b1,c0,c1".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
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
            stmt.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", null, null, null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A2", null, null, null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"A2", null, null, "C1", null}});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("C2"));
            Assert.IsFalse(_listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_Bplus() {
    
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a,b0,b1,b2".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, B[2].TheString as b2" +
                    " pattern (A B+)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like 'A%'," +
                    " B as B.TheString like 'B%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            stmt.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A2", "B2", null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B4"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X3", -1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A4", "B3", "B4", null});
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_Astar() {
    
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a0,a1,a2,a3,a4".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    " measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, A[3].TheString as a3, A[4].TheString as a4" +
                    " pattern (A*)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as TheString like 'A%'" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            stmt.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "A2", null, null, null});
    
            SendTimer(isolated, 2000);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A3"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A4"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A5"));
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(isolated, 12000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A3", "A4", "A5", null, null});
    
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A6"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A3", "A4", "A5", "A6", null});
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B3"));
            Assert.IsFalse(_listener.IsInvoked);
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
        }
    
        private void RunAssertion_A_Bstar(bool allMatches)
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a,b0,b1,b2".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
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
            stmt.Events += _listener.Update;
    
            // test output by terminated because of misfit event
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            if (!allMatches) {
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "B1", null, null});
            }
            else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields,
                        new Object[][] { new Object[] {"A1", "B1", null, null}, new Object[] {"A1", null, null, null}});
            }
    
            SendTimer(isolated, 20000);
            Assert.IsFalse(_listener.IsInvoked);
    
            // test output by timer expiry
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(isolated, 29999);
    
            SendTimer(isolated, 30000);
            if (!allMatches) {
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A2", "B2", null, null});
            }
            else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields,
                        new Object[][] { new Object[] {"A2", "B2", null, null}, new Object[] {"A2", null, null, null}});
            }
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
    
            EPStatement stmtFromModel = SupportModelHelper.CompileCreate(_epService, text);
            stmtFromModel.Dispose();
        }
    
        private void RunAssertion_A_parenthesisBstar()
        {
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            SendTimer(isolated, 0);
    
            String[] fields = "a,b0,b1,b2".Split(',');
            String text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1, B[2].TheString as b2" +
                    " pattern (A (B)*)" +
                    " interval 10 seconds or terminated" +
                    " define" +
                    " A as A.TheString like \"A%\"," +
                    " B as B.TheString like \"B%\"" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "stmt1", null);
            stmt.Events += _listener.Update;
    
            // test output by terminated because of misfit event
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A1"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B1"));
            Assert.IsFalse(_listener.IsInvoked);
            isolated.EPRuntime.SendEvent(new SupportRecogBean("X1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "B1", null, null});
    
            SendTimer(isolated, 20000);
            Assert.IsFalse(_listener.IsInvoked);
    
            // test output by timer expiry
            isolated.EPRuntime.SendEvent(new SupportRecogBean("A2"));
            isolated.EPRuntime.SendEvent(new SupportRecogBean("B2"));
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(isolated, 29999);
    
            SendTimer(isolated, 30000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A2", "B2", null, null});
    
            // destroy
            stmt.Dispose();
            isolated.Dispose();
    
            EPStatement stmtFromModel = SupportModelHelper.CompileCreate(_epService, text);
            stmtFromModel.Dispose();
        }
    
        private void SendTemperatureEvent(EPServiceProviderIsolated isolated, String id, int device, double temp)
        {
            isolated.EPRuntime.SendEvent(new Object[] {id, device, temp}, "TemperatureSensorEvent");
        }
    
        private void SendTimer(EPServiceProviderIsolated isolated, long time)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(time);
            isolated.EPRuntime.SendEvent(theEvent);
        }
    }
}
