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

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectWithinPattern : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionSubqueryAgainstNamedWindowInUDFInPattern(epService);
            RunAssertionFilterPatternNamedWindowNoAlias(epService);
            RunAssertionCorrelated(epService);
            RunAssertionAggregation(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            TryInvalid(epService, "select * from S0(exists (select * from S1))",
                    "Failed to validate subquery number 1 querying S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from S0(exists (select * from S1))]");
    
            epService.EPAdministrator.CreateEPL("create window MyWindowInvalid#lastevent as select * from S0");
            TryInvalid(epService, "select * from S0(exists (select * from MyWindowInvalid#lastevent))",
                    "Failed to validate subquery number 1 querying MyWindowInvalid: Consuming statements to a named window cannot declare a data window view onto the named window [select * from S0(exists (select * from MyWindowInvalid#lastevent))]");
    
            TryInvalid(epService, "select * from S0(id in ((select p00 from MyWindowInvalid)))",
                    "Failed to validate filter expression 'id in (subselect_1)': Implicit conversion not allowed: Cannot coerce types " + Name.Clean<int>() + " and System.String [select * from S0(id in ((select p00 from MyWindowInvalid)))]");
        }
    
        private void RunAssertionSubqueryAgainstNamedWindowInUDFInPattern(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("supportSingleRowFunction", typeof(ExecSubselectWithinPattern), "SupportSingleRowFunction");
            epService.EPAdministrator.CreateEPL("create window MyWindowSNW#unique(p00)#keepall as S0");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from pattern[S1(supportSingleRowFunction((select * from MyWindowSNW)))]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            listener.AssertInvokedAndReset();
        }
    
        private void RunAssertionFilterPatternNamedWindowNoAlias(EPServiceProvider epService) {
            // subselect in pattern
            string stmtTextOne = "select s.id as myid from pattern [every s=S0(p00 in (select p10 from S1#lastevent))]";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
            TryAssertion(epService, listener);
            stmtOne.Dispose();
    
            // subselect in filter
            string stmtTextTwo = "select id as myid from S0(p00 in (select p10 from S1#lastevent))";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmtTwo.Events += listener.Update;
            TryAssertion(epService, listener);
            stmtTwo.Dispose();
    
            // subselect in filter with named window
            EPStatement stmtNamedThree = epService.EPAdministrator.CreateEPL("create window MyS1Window#lastevent as select * from S1");
            EPStatement stmtInsertThree = epService.EPAdministrator.CreateEPL("insert into MyS1Window select * from S1");
            string stmtTextThree = "select id as myid from S0(p00 in (select p10 from MyS1Window))";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += listener.Update;
            TryAssertion(epService, listener);
            stmtThree.Dispose();
            stmtInsertThree.Dispose();
            stmtNamedThree.Dispose();
    
            // subselect in pattern with named window
            EPStatement stmtNamedFour = epService.EPAdministrator.CreateEPL("create window MyS1Window#lastevent as select * from S1");
            EPStatement stmtInsertFour = epService.EPAdministrator.CreateEPL("insert into MyS1Window select * from S1");
            string stmtTextFour = "select s.id as myid from pattern [every s=S0(p00 in (select p10 from MyS1Window))]";
            EPStatement stmtFour = epService.EPAdministrator.CreateEPL(stmtTextFour);
            stmtFour.Events += listener.Update;
            TryAssertion(epService, listener);
            stmtFour.Dispose();
            stmtInsertFour.Dispose();
            stmtNamedFour.Dispose();
        }
    
        private void RunAssertionCorrelated(EPServiceProvider epService) {
    
            string stmtTextTwo = "select sp1.id as myid from pattern[every sp1=S0(exists (select * from S1#keepall as stream1 where stream1.p10 = sp1.p00))]";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
            TryAssertionCorrelated(epService, listener);
            stmtTwo.Dispose();
    
            string stmtTextOne = "select id as myid from S0(exists (select stream1.id from S1#keepall as stream1 where stream1.p10 = stream0.p00)) as stream0";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmtOne.Events += listener.Update;
            TryAssertionCorrelated(epService, listener);
            stmtOne.Dispose();
    
            // Correlated across two matches
            string stmtTextThree = "select sp0.p00||'+'||sp1.p10 as myid from pattern[" +
                    "every sp0=S0 -> sp1=S1(p11 = (select stream2.p21 from S2#keepall as stream2 where stream2.p20 = sp0.p00))]";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(21, "X", "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(22, "Y", "B"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(23, "Z", "C"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "Y"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "C"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "B", "B"));
            Assert.AreEqual("Y+B", listener.AssertOneGetNewAndReset().Get("myid"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "B", "C"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(5, "C", "B"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(6, "X", "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(7, "A", "C"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmtThree.Dispose();
        }
    
        private void RunAssertionAggregation(EPServiceProvider epService) {
    
            string stmtText = "select * from S0(id = (select sum(id) from S1#length(2)))";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));  // now at 4
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));  // now at 13 (length window 2)
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(13));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void TryAssertionCorrelated(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "B"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "C"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "C"));
            Assert.AreEqual(5, listener.AssertOneGetNewAndReset().Get("myid"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(6, "A"));
            Assert.AreEqual(6, listener.AssertOneGetNewAndReset().Get("myid"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(7, "D"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(8, "E"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(9, "C"));
            Assert.AreEqual(9, listener.AssertOneGetNewAndReset().Get("myid"));
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "B"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "C"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "C"));
            Assert.AreEqual(5, listener.AssertOneGetNewAndReset().Get("myid"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(6, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(7, "D"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(8, "E"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(9, "C"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E"));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("myid"));
        }
    
        public static bool SupportSingleRowFunction(params object[] v) {
            return true;
        }
    }
} // end of namespace
