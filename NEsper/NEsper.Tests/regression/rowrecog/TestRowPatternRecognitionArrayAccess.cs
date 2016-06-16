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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionArrayAccess
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestSingleMultiMix()
        {
            String[] fields = "a,b0,c,d0,e".Split(',');
            String text = "select * from SupportBean " +
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
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
            _listener.Reset();
    
            SendEvents(new Object[][] { new Object[] {"A1", 100}, new Object[] {"B1", 50}, new Object[] {"B2", 49}, new Object[] {"C1", 49}, new Object[] {"D1", 2}, new Object[] {"D2", 2}, new Object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "B1", "C1", "D1", "E1"});
    
            SendEvents(new Object[][] { new Object[] {"A1", 100}, new Object[] {"B1", 50}, new Object[] {"C1", 49}, new Object[] {"D1", 2}, new Object[] {"D2", 2}, new Object[] {"E1", 2}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"A1", 100}, new Object[] {"B1", 50}, new Object[] {"B2", 49}, new Object[] {"C1", 49}, new Object[] {"D1", 2}, new Object[] {"D2", 3}, new Object[] {"E1", 2}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"A1", 100}, new Object[] {"B1", 50}, new Object[] {"B2", 49}, new Object[] {"C1", 49}, new Object[] {"D1", 2}, new Object[] {"D2", 2}, new Object[] {"D3", 99}, new Object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "B1", "C1", "D1", "E1"});
        }
    
        [Test]
        public void TestMultiDepends() {
            RunAssertionMultiDepends("A B A B C");
            RunAssertionMultiDepends("(A B)* C");
        }
    
        [Test]
        public void TestMeasuresClausePresence()
        {
            RunMeasuresClausePresence("A as a_array, B as b");
            RunMeasuresClausePresence("B as b");
            RunMeasuresClausePresence("A as a_array");
            RunMeasuresClausePresence("1 as one");
        }
    
        [Test]
        public void TestLambda() {
            String[] fieldsOne = "a0,a1,a2,b".Split(',');
            String eplOne = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b" +
                    " pattern (A* B)" +
                    " define" +
                    " B as (coalesce(A.SumOf(v => v.IntPrimitive), 0) + B.IntPrimitive) > 100" +
                    ")";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(eplOne);
            stmt.Events += _listener.Update;
            _listener.Reset();
    
            SendEvents(new Object[][] { new Object[] {"E1", 50}, new Object[] {"E2", 49}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"E3", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[] {"E1", "E2", null, "E3"});
    
            SendEvents(new Object[][] { new Object[] {"E4", 101}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[] {null, null, null, "E4"});
    
            SendEvents(new Object[][] { new Object[] {"E5", 50}, new Object[] {"E6", 51}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[] {"E5", null, null, "E6"});
    
            SendEvents(new Object[][] { new Object[] {"E7", 10}, new Object[] {"E8", 10}, new Object[] {"E9", 79}, new Object[] {"E10", 1}, new Object[] {"E11", 1}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new Object[] {"E7", "E8", "E9", "E11"});
            stmt.Dispose();
    
            String[] fieldsTwo = "a[0].TheString,a[1].TheString,b.TheString".Split(',');
            String eplTwo = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A as a, B as b " +
                    " pattern (A+ B)" +
                    " define" +
                    " A as TheString like 'A%', " +
                    " B as TheString like 'B%' and B.IntPrimitive > A.SumOf(v => v.IntPrimitive)" +
                    ")";
    
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.Events += _listener.Update;
            _listener.Reset();
    
            SendEvents(new Object[][] { new Object[] {"A1", 1}, new Object[] {"A2", 2}, new Object[] {"B1", 3}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] {"A2", null, "B1"});
    
            SendEvents(new Object[][] { new Object[] {"A3", 1}, new Object[] {"A4", 2}, new Object[] {"B2", 4}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] {"A3", "A4", "B2"});
    
            SendEvents(new Object[][] { new Object[] {"A5", -1}, new Object[] {"B3", 0}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] {"A5", null, "B3"});
    
            SendEvents(new Object[][] { new Object[] {"A6", 10}, new Object[] {"B3", 9}, new Object[] {"B4", 11}});
            SendEvents(new Object[][] { new Object[] {"A7", 10}, new Object[] {"A8", 9}, new Object[] {"A9", 8}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"B5", 18}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] {"A8", "A9", "B5"});
    
            SendEvents(new Object[][] { new Object[] {"A0", 10}, new Object[] {"A11", 9}, new Object[] {"A12", 8}, new Object[] {"B6", 8}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"A13", 1}, new Object[] {"A14", 1}, new Object[] {"A15", 1}, new Object[] {"A16", 1}, new Object[] {"B7", 5}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo, new Object[] {"A13", "A14", "B7"});
    
            SendEvents(new Object[][] { new Object[] {"A17", 1}, new Object[] {"A18", 1}, new Object[] {"B8", 1}});
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        private void RunMeasuresClausePresence(String measures)
        {
            String text = "select * from SupportBean " +
                    "match_recognize (" +
                    " partition by TheString " +
                    " measures " + measures +
                    " pattern (A+ B)" +
                    " define" +
                    " B as B.IntPrimitive = A[0].IntPrimitive" +
                    ")";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendEvents(new Object[][] { new Object[] {"A", 1}, new Object[] {"A", 0}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"B", 1}, new Object[] {"B", 1}});
            Assert.IsNotNull(_listener.AssertOneGetNewAndReset());
    
            SendEvents(new Object[][] { new Object[] {"A", 2}, new Object[] {"A", 3}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"B", 2}, new Object[] {"B", 2}});
            Assert.IsNotNull(_listener.AssertOneGetNewAndReset());
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiDepends(String pattern) {
            String[] fields = "a0,a1,b0,b1,c".Split(',');
            String text = "select * from SupportBean " +
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
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
            _listener.Reset();
    
            SendEvents(new Object[][] { new Object[] {"A1", 1}, new Object[] {"B1", 1}, new Object[] {"A2", 1}, new Object[] {"B2", 1}});
            _epService.EPRuntime.SendEvent(new SupportBean("C1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"A1", "A2", "B1", "B2", "C1"});
    
            SendEvents(new Object[][] { new Object[] {"A10", 1}, new Object[] {"B10", 1}, new Object[] {"A11", 1}, new Object[] {"B11", 2}, new Object[] {"C2", 2}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvents(new Object[][] { new Object[] {"A20", 2}, new Object[] {"B20", 2}, new Object[] {"A21", 1}, new Object[] {"B21", 2}, new Object[] {"C3", 2}});
            Assert.IsFalse(_listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void SendEvents(Object[][] objects) {
            foreach (Object[] @object in objects) {
                _epService.EPRuntime.SendEvent(new SupportBean((String) @object[0], (int) @object[1]));
            }
        }
    }
}
