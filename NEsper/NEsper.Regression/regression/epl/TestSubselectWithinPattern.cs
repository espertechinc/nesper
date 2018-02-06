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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectWithinPattern
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestInvalid() {
    
            TryInvalid("select * from S0(exists (select * from S1))",
                    "Failed to validate subquery number 1 querying S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from S0(exists (select * from S1))]");
            
            _epService.EPAdministrator.CreateEPL("create window MyWindow#lastevent as select * from S0");
            TryInvalid("select * from S0(exists (select * from MyWindow#lastevent))",
                    "Failed to validate subquery number 1 querying MyWindow: Consuming statements to a named window cannot declare a data window view onto the named window [select * from S0(exists (select * from MyWindow#lastevent))]");
    
            TryInvalid("select * from S0(id in ((select p00 from MyWindow)))",
                    "Failed to validate filter expression 'id in (subselect_1)': Implicit conversion not allowed: Cannot coerce types " + typeof(int?).FullName + " and System.String [select * from S0(id in ((select p00 from MyWindow)))]");
        }

        [Test]
        public void TestSubqueryAgainstNamedWindowInUDFInPattern()
        {

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("supportSingleRowFunction", typeof(TestSubselectWithinPattern).FullName, "SupportSingleRowFunction");
            _epService.EPAdministrator.CreateEPL("create window MyWindow#unique(p00)#keepall as S0");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from pattern[S1(SupportSingleRowFunction((select * from MyWindow)))]");
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            _listener.AssertInvokedAndReset();
        }


        [Test]
        public void TestFilterPatternNamedWindowNoAlias()
        {
            // subselect in pattern
            String stmtTextOne = "select s.id as myid from pattern [every s=S0(p00 in (select p10 from S1#lastevent))]";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            RunAssertion();
            stmtOne.Dispose();
    
            // subselect in filter
            String stmtTextTwo = "select id as myid from S0(p00 in (select p10 from S1#lastevent))";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmtTwo.Events += _listener.Update;
            RunAssertion();
            stmtTwo.Dispose();
    
            // subselect in filter with named window
            EPStatement stmtNamedThree = _epService.EPAdministrator.CreateEPL("create window MyS1Window#lastevent as select * from S1");
            EPStatement stmtInsertThree = _epService.EPAdministrator.CreateEPL("insert into MyS1Window select * from S1");
            String stmtTextThree = "select id as myid from S0(p00 in (select p10 from MyS1Window))";
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += _listener.Update;
            RunAssertion();
            stmtThree.Dispose();
            stmtInsertThree.Dispose();
            stmtNamedThree.Dispose();
    
            // subselect in pattern with named window
            EPStatement stmtNamedFour = _epService.EPAdministrator.CreateEPL("create window MyS1Window#lastevent as select * from S1");
            EPStatement stmtInsertFour = _epService.EPAdministrator.CreateEPL("insert into MyS1Window select * from S1");
            String stmtTextFour = "select s.id as myid from pattern [every s=S0(p00 in (select p10 from MyS1Window))]";
            EPStatement stmtFour = _epService.EPAdministrator.CreateEPL(stmtTextFour);
            stmtFour.Events += _listener.Update;
            RunAssertion();
            stmtFour.Dispose();
            stmtInsertFour.Dispose();
            stmtNamedFour.Dispose();
        }
    
        [Test]
        public void TestCorrelated()
        {
            String stmtTextTwo = "select sp1.id as myid from pattern[every sp1=S0(exists (select * from S1#keepall as stream1 where stream1.p10 = sp1.P00))]";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmtTwo.Events += _listener.Update;
            RunAssertionTwo();
            stmtTwo.Dispose();
    
            String stmtTextOne = "select id as myid from S0(exists (select stream1.id from S1#keepall as stream1 where stream1.p10 = stream0.P00)) as stream0";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += _listener.Update;
            RunAssertionTwo();
            stmtOne.Dispose();
    
            // Correlated across two matches
            String stmtTextThree = "select sp0.P00||'+'||sp1.p10 as myid from pattern[" +
                    "every sp0=S0 -> sp1=S1(p11 = (select stream2.p21 from S2#keepall as stream2 where stream2.p20 = sp0.P00))]";
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(21, "X", "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(22, "Y", "B"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(23, "Z", "C"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "Y"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "C"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "B", "B"));
            Assert.AreEqual("Y+B", _listener.AssertOneGetNewAndReset().Get("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "B", "C"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(5, "C", "B"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(6, "X", "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(7, "A", "C"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            stmtThree.Dispose();
        }
    
        [Test]
        public void TestAggregation() {
    
            String stmtText = "select * from S0(id = (select sum(id) from S1#length(2)))";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3));  // now at 4
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));  // now at 13 (length window 2)
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(13));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionTwo() {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "B"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "C"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "C"));
            Assert.AreEqual(5, _listener.AssertOneGetNewAndReset().Get("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(6, "A"));
            Assert.AreEqual(6, _listener.AssertOneGetNewAndReset().Get("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(7, "D"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(8, "E"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(9, "C"));
            Assert.AreEqual(9, _listener.AssertOneGetNewAndReset().Get("myid"));
        }
    
        private void RunAssertion() {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "B"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "C"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
           
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "C"));
            Assert.AreEqual(5, _listener.AssertOneGetNewAndReset().Get("myid"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(6, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(7, "D"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(8, "E"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(9, "C"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E"));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("myid"));
        }
    
        private void TryInvalid(String epl, String message) {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }

        public static bool SupportSingleRowFunction(params object[] v)
        {
            return true;
        }
    }
}
