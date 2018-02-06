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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitAfter : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionAfterWithOutputLast(epService, false);
            RunAssertionAfterWithOutputLast(epService, true);
    
            RunAssertionEveryPolicy(epService);
            RunAssertionMonthScoped(epService);
            RunAssertionDirectNumberOfEvents(epService);
            RunAssertionDirectTimePeriod(epService);
            RunAssertionSnapshotVariable(epService);
            RunAssertionOutputWhenThen(epService);
        }
    
        private void RunAssertionAfterWithOutputLast(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            string epl = hint + "select sum(IntPrimitive) as thesum " +
                    "from SupportBean#keepall " +
                    "output after 4 events last every 2 events";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 40));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 50));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 60));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "thesum".Split(','), new object[]{210});
    
            stmt.Dispose();
        }
    
        private void RunAssertionEveryPolicy(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select TheString from SupportBean#keepall output after 0 days 0 hours 0 minutes 20 seconds 0 milliseconds every 0 days 0 hours 0 minutes 5 seconds 0 milliseconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionEveryPolicy(epService, listener);
    
            stmt.Dispose();
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("TheString");
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("keepall"));
            model.OutputLimitClause = OutputLimitClause.Create(Expressions.TimePeriod(0, 0, 0, 5, 0))
                .SetAfterTimePeriodExpression(Expressions.TimePeriod(0, 0, 0, 20, 0));
            Assert.AreEqual(stmtText, model.ToEPL());
        }
    
        private void RunAssertionMonthScoped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean output after 1 month").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E3"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionEveryPolicy(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "TheString".Split(',');
            SendTimer(epService, 1);
            SendEvent(epService, "E1");
    
            SendTimer(epService, 6000);
            SendEvent(epService, "E2");
            SendTimer(epService, 16000);
            SendEvent(epService, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 20000);
            SendEvent(epService, "E4");
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 24999);
            SendEvent(epService, "E5");
    
            SendTimer(epService, 25000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E4"}, new object[] {"E5"}});
            listener.Reset();
    
            SendTimer(epService, 27000);
            SendEvent(epService, "E6");
    
            SendTimer(epService, 29999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 30000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E6"});
        }
    
        private void RunAssertionDirectNumberOfEvents(EPServiceProvider epService) {
            string[] fields = "TheString".Split(',');
            string stmtText = "select TheString from SupportBean#keepall output after 3 events";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1");
            SendEvent(epService, "E2");
            SendEvent(epService, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E4");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendEvent(epService, "E5");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            stmt.Dispose();
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("TheString");
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("keepall"));
            model.OutputLimitClause = OutputLimitClause.CreateAfter(3);
            Assert.AreEqual("select TheString from SupportBean#keepall output after 3 events ", model.ToEPL());
    
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1");
            SendEvent(epService, "E2");
            SendEvent(epService, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E4");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendEvent(epService, "E5");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            model = epService.EPAdministrator.CompileEPL("select TheString from SupportBean#keepall output after 3 events");
            Assert.AreEqual("select TheString from SupportBean#keepall output after 3 events ", model.ToEPL());
    
            stmt.Dispose();
        }
    
        private void RunAssertionDirectTimePeriod(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string[] fields = "TheString".Split(',');
            string stmtText = "select TheString from SupportBean#keepall output after 20 seconds ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 1);
            SendEvent(epService, "E1");
    
            SendTimer(epService, 6000);
            SendEvent(epService, "E2");
    
            SendTimer(epService, 19999);
            SendEvent(epService, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 20000);
            SendEvent(epService, "E4");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendTimer(epService, 21000);
            SendEvent(epService, "E5");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionSnapshotVariable(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create variable int myvar = 1");
    
            SendTimer(epService, 0);
            string stmtText = "select TheString from SupportBean#keepall output after 20 seconds snapshot when myvar=1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSnapshotVar(epService, listener);
    
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtText, stmt.Text);
            stmt.Dispose();
        }
    
        private void RunAssertionOutputWhenThen(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create variable bool myvar0 = false");
            epService.EPAdministrator.CreateEPL("create variable bool myvar1 = false");
            epService.EPAdministrator.CreateEPL("create variable bool myvar2 = false");
    
            string epl = "@Name(\"select-streamstar+outputvar\")\n" +
                    "select a.* from SupportBean#time(10) a output after 3 events when myvar0=true then set myvar1=true, myvar2=true";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1");
            SendEvent(epService, "E2");
            SendEvent(epService, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("myvar0", true);
            SendEvent(epService, "E4");
            Assert.IsTrue(listener.IsInvoked);
    
            Assert.AreEqual(true, epService.EPRuntime.GetVariableValue("myvar1"));
            Assert.AreEqual(true, epService.EPRuntime.GetVariableValue("myvar2"));
    
            stmt.Dispose();
        }
    
        private void TryAssertionSnapshotVar(EPServiceProvider epService, SupportUpdateListener listener) {
            SendTimer(epService, 6000);
            SendEvent(epService, "E1");
            SendEvent(epService, "E2");
    
            SendTimer(epService, 19999);
            SendEvent(epService, "E3");
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 20000);
            SendEvent(epService, "E4");
            string[] fields = "TheString".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
            listener.Reset();
    
            SendTimer(epService, 21000);
            SendEvent(epService, "E5");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
            listener.Reset();
        }
    
        private void SendTimer(EPServiceProvider epService, long time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(time));
        }
    
        private void SendEvent(EPServiceProvider epService, string theString) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, 0));
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace
