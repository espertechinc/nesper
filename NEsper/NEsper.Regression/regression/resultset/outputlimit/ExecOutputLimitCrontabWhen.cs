///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitCrontabWhen : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionOutputCrontabAtVariable(epService);
            RunAssertionOutputCrontabAt(epService);
            RunAssertionOutputCrontabAtOMCreate(epService);
            RunAssertionOutputCrontabAtOMCompile(epService);
            RunAssertionOutputWhenThenExpression(epService);
            RunAssertionOutputWhenExpression(epService);
            RunAssertionOutputWhenBuiltInCountInsert(epService);
            RunAssertionOutputWhenBuiltInCountRemove(epService);
            RunAssertionOutputWhenBuiltInLastTimestamp(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionOutputCrontabAtVariable(EPServiceProvider epService) {
    
            // every 15 minutes 8am to 5pm
            SendTimeEvent(epService, 1, 17, 10, 0, 0);
            epService.EPAdministrator.CreateEPL("create variable int VFREQ = 15");
            epService.EPAdministrator.CreateEPL("create variable int VMIN = 8");
            epService.EPAdministrator.CreateEPL("create variable int VMAX = 17");
            string expression = "select * from MarketData#lastevent output at (*/VFREQ, VMIN:VMAX, *, *, *)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryAssertionCrontab(epService, 1, stmt, listener);
        }
    
        private void RunAssertionOutputCrontabAt(EPServiceProvider epService) {
    
            // every 15 minutes 8am to 5pm
            SendTimeEvent(epService, 1, 17, 10, 0, 0);
            string expression = "select * from MarketData#lastevent output at (*/15, 8:17, *, *, *)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryAssertionCrontab(epService, 1, stmt, listener);
        }
    
        private void RunAssertionOutputCrontabAtOMCreate(EPServiceProvider epService) {
    
            // every 15 minutes 8am to 5pm
            SendTimeEvent(epService, 1, 17, 10, 0, 0);
            string expression = "select * from MarketData#lastevent output at (*/15, 8:17, *, *, *)";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create("MarketData")
                .AddView("lastevent"));
            var crontabParams = new Expression[]{
                    Expressions.CrontabScheduleFrequency(15),
                    Expressions.CrontabScheduleRange(8, 17),
                    Expressions.CrontabScheduleWildcard(),
                    Expressions.CrontabScheduleWildcard(),
                    Expressions.CrontabScheduleWildcard()
            };
            model.OutputLimitClause = OutputLimitClause.CreateSchedule(crontabParams);
    
            string epl = model.ToEPL();
            Assert.AreEqual(expression, epl);
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryAssertionCrontab(epService, 1, stmt, listener);
        }
    
        private void RunAssertionOutputCrontabAtOMCompile(EPServiceProvider epService) {
            // every 15 minutes 8am to 5pm
            SendTimeEvent(epService, 1, 17, 10, 0, 0);
            string expression = "select * from MarketData#lastevent output at (*/15, 8:17, *, *, *)";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryAssertionCrontab(epService, 1, stmt, listener);
        }
    
        private void TryAssertionCrontab(EPServiceProvider epService, int days, EPStatement statement, SupportUpdateListener listener) {
            string[] fields = "symbol".Split(',');
            SendEvent(epService, "S1", 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, days, 17, 14, 59, 0);
            SendEvent(epService, "S2", 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, days, 17, 15, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S1"}, new object[] {"S2"}});
    
            SendTimeEvent(epService, days, 17, 18, 0, 0);
            SendEvent(epService, "S3", 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, days, 17, 30, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S3"}});
    
            SendTimeEvent(epService, days, 17, 35, 0, 0);
            SendTimeEvent(epService, days, 17, 45, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
    
            SendEvent(epService, "S4", 0);
            SendEvent(epService, "S5", 0);
            SendTimeEvent(epService, days, 18, 0, 0, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, days, 18, 1, 0, 0);
            SendEvent(epService, "S6", 0);
    
            SendTimeEvent(epService, days, 18, 15, 0, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, days + 1, 7, 59, 59, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, days + 1, 8, 0, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S4"}, new object[] {"S5"}, new object[] {"S6"}});
    
            statement.Dispose();
            listener.Reset();
        }
    
        private void RunAssertionOutputWhenThenExpression(EPServiceProvider epService) {
            SendTimeEvent(epService, 1, 8, 0, 0, 0);
            epService.EPAdministrator.Configuration.AddVariable("myvar", typeof(int), 0);
            epService.EPAdministrator.Configuration.AddVariable("count_insert_var", typeof(int), 0);
            epService.EPAdministrator.CreateEPL("on SupportBean set myvar = IntPrimitive");
    
            string expression = "select symbol from MarketData#length(2) output when myvar=1 then set myvar=0, count_insert_var=count_insert";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            TryAssertion(epService, 1, stmt);
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol");
            model.FromClause = FromClause.Create(FilterStream.Create("MarketData")
                .AddView("length", Expressions.Constant(2)));
            model.OutputLimitClause = OutputLimitClause.Create(Expressions.Eq("myvar", 1))
                    .AddThenAssignment(Expressions.Eq(Expressions.Property("myvar"), Expressions.Constant(0)))
                    .AddThenAssignment(Expressions.Eq(Expressions.Property("count_insert_var"), Expressions.Property("count_insert")));
    
            string epl = model.ToEPL();
            Assert.AreEqual(expression, epl);
            stmt = epService.EPAdministrator.Create(model);
            TryAssertion(epService, 2, stmt);
    
            model = epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            TryAssertion(epService, 3, stmt);
    
            string outputLast = "select symbol from MarketData#length(2) output last when myvar=1 ";
            model = epService.EPAdministrator.CompileEPL(outputLast);
            Assert.AreEqual(outputLast.Trim(), model.ToEPL().Trim());
    
            // test same variable referenced multiple times JIRA-386
            SendTimer(epService, 0);
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select * from MarketData output last when myvar=100");
            stmtOne.Events += listenerOne.Update;
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select * from MarketData output last when myvar=100");
            stmtTwo.Events += listenerTwo.Update;
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", "E1", 100));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", "E2", 100));
    
            SendTimer(epService, 1000);
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("myvar", 100);
            SendTimer(epService, 2000);
            Assert.IsTrue(listenerTwo.IsInvoked);
            Assert.IsTrue(listenerOne.IsInvoked);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
    
            // test when-then with condition triggered by output events
            SendTimeEvent(epService, 2, 8, 0, 0, 0);
            string eplToDeploy = "create variable bool varOutputTriggered = false\n;" +
                    "@Audit @Name('out') select * from SupportBean#lastevent output snapshot when (count_insert > 1 and varOutputTriggered = false) then set varOutputTriggered = true;";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplToDeploy);
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual("E2", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("varOutputTriggered", false); // turns true right away as triggering output
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            SendTimeEvent(epService, 2, 8, 0, 1, 0);
            Assert.AreEqual("E5", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test count_total for insert and remove
            epService.EPAdministrator.CreateEPL("create variable int var_cnt_total = 3");
            string expressionTotal = "select TheString from SupportBean#length(2) output when count_insert_total = var_cnt_total or count_remove_total > 2";
            EPStatement stmtTotal = epService.EPAdministrator.CreateEPL(expressionTotal);
            stmtTotal.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "TheString".Split(','), new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            epService.EPRuntime.SetVariableValue("var_cnt_total", -1);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "TheString".Split(','), new object[][]{new object[] {"E4"}, new object[] {"E5"}});
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertion(EPServiceProvider epService, int days, EPStatement stmt) {
            var subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent(epService, "S1", 0);
    
            // now scheduled for output
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("myvar"));
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendTimeEvent(epService, days, 8, 0, 1, 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1"}, subscriber.GetAndResetLastNewData());
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("myvar"));
            Assert.AreEqual(1, epService.EPRuntime.GetVariableValue("count_insert_var"));
    
            SendEvent(epService, "S2", 0);
            SendEvent(epService, "S3", 0);
            SendTimeEvent(epService, days, 8, 0, 2, 0);
            SendTimeEvent(epService, days, 8, 0, 3, 0);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("myvar"));
            Assert.AreEqual(2, epService.EPRuntime.GetVariableValue("count_insert_var"));
    
            Assert.IsFalse(subscriber.IsInvoked);
            SendTimeEvent(epService, days, 8, 0, 4, 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S2", "S3"}, subscriber.GetAndResetLastNewData());
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("myvar"));
    
            SendTimeEvent(epService, days, 8, 0, 5, 0);
            Assert.IsFalse(subscriber.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("myvar"));
            Assert.IsFalse(subscriber.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputWhenExpression(EPServiceProvider epService) {
            SendTimeEvent(epService, 1, 8, 0, 0, 0);
            epService.EPAdministrator.Configuration.AddVariable("myint", typeof(int), 0);
            epService.EPAdministrator.Configuration.AddVariable("mystring", typeof(string), "");
            epService.EPAdministrator.CreateEPL("on SupportBean set myint = IntPrimitive, mystring = TheString");
    
            string expression = "select symbol from MarketData#length(2) output when myint = 1 and mystring like 'F%'";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent(epService, "S1", 0);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, epService.EPRuntime.GetVariableValue("myint"));
            Assert.AreEqual("E1", epService.EPRuntime.GetVariableValue("mystring"));
    
            SendEvent(epService, "S2", 0);
            SendTimeEvent(epService, 1, 8, 0, 1, 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("F1", 0));
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("myint"));
            Assert.AreEqual("F1", epService.EPRuntime.GetVariableValue("mystring"));
    
            SendTimeEvent(epService, 1, 8, 0, 2, 0);
            SendEvent(epService, "S3", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("F2", 1));
            Assert.AreEqual(1, epService.EPRuntime.GetVariableValue("myint"));
            Assert.AreEqual("F2", epService.EPRuntime.GetVariableValue("mystring"));
    
            SendEvent(epService, "S4", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1", "S2", "S3", "S4"}, subscriber.GetAndResetLastNewData());
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputWhenBuiltInCountInsert(EPServiceProvider epService) {
            string expression = "select symbol from MarketData#length(2) output when count_insert >= 3";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent(epService, "S1", 0);
            SendEvent(epService, "S2", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendEvent(epService, "S3", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1", "S2", "S3"}, subscriber.GetAndResetLastNewData());
    
            SendEvent(epService, "S4", 0);
            SendEvent(epService, "S5", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendEvent(epService, "S6", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S4", "S5", "S6"}, subscriber.GetAndResetLastNewData());
    
            SendEvent(epService, "S7", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputWhenBuiltInCountRemove(EPServiceProvider epService) {
            string expression = "select symbol from MarketData#length(2) output when count_remove >= 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent(epService, "S1", 0);
            SendEvent(epService, "S2", 0);
            SendEvent(epService, "S3", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendEvent(epService, "S4", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1", "S2", "S3", "S4"}, subscriber.GetAndResetLastNewData());
    
            SendEvent(epService, "S5", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendEvent(epService, "S6", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S5", "S6"}, subscriber.GetAndResetLastNewData());
    
            SendEvent(epService, "S7", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputWhenBuiltInLastTimestamp(EPServiceProvider epService) {
            SendTimeEvent(epService, 1, 8, 0, 0, 0);
            string expression = "select symbol from MarketData#length(2) output when current_timestamp - last_output_timestamp >= 2000";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent(epService, "S1", 0);
    
            SendTimeEvent(epService, 1, 8, 0, 1, 900);
            SendEvent(epService, "S2", 0);
    
            SendTimeEvent(epService, 1, 8, 0, 2, 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendEvent(epService, "S3", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1", "S2", "S3"}, subscriber.GetAndResetLastNewData());
    
            SendTimeEvent(epService, 1, 8, 0, 3, 0);
            SendEvent(epService, "S4", 0);
    
            SendTimeEvent(epService, 1, 8, 0, 3, 500);
            SendEvent(epService, "S5", 0);
            Assert.IsFalse(subscriber.IsInvoked);
    
            SendTimeEvent(epService, 1, 8, 0, 4, 0);
            SendEvent(epService, "S6", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S4", "S5", "S6"}, subscriber.GetAndResetLastNewData());
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimeEvent(EPServiceProvider epService, int day, int hour, int minute, int second, int millis) {
            var dateTime = new DateTime(2008, 1, day, hour, minute, second, millis, DateTimeKind.Local);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(dateTime));
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("myvardummy", typeof(int), 0);
            epService.EPAdministrator.Configuration.AddVariable("myvarlong", typeof(long), 0);
    
            TryInvalid(epService, "select * from MarketData output when sum(price) > 0",
                    "Error validating expression: Failed to validate output limit expression '(sum(price))>0': Property named 'price' is not valid in any stream [select * from MarketData output when sum(price) > 0]");
    
            TryInvalid(epService, "select * from MarketData output when sum(count_insert) > 0",
                    "Error validating expression: An aggregate function may not appear in a OUTPUT LIMIT clause [select * from MarketData output when sum(count_insert) > 0]");
    
            TryInvalid(epService, "select * from MarketData output when prev(1, count_insert) = 0",
                    "Error validating expression: Failed to validate output limit expression 'prev(1,count_insert)=0': Previous function cannot be used in this context [select * from MarketData output when prev(1, count_insert) = 0]");
    
            TryInvalid(epService, "select * from MarketData output when myvardummy",
                    "Error validating expression: The when-trigger expression in the OUTPUT WHEN clause must return a boolean-type value [select * from MarketData output when myvardummy]");
    
            TryInvalid(epService, "select * from MarketData output when true then set myvardummy = 'b'",
                    "Error starting statement: Error in the output rate limiting clause: Variable 'myvardummy' of declared type " + Name.Clean<int>() + " cannot be assigned a value of type System.String [select * from MarketData output when true then set myvardummy = 'b']");
    
            TryInvalid(epService, "select * from MarketData output when true then set myvardummy = sum(myvardummy)",
                    "Error validating expression: An aggregate function may not appear in a OUTPUT LIMIT clause [select * from MarketData output when true then set myvardummy = sum(myvardummy)]");
    
            TryInvalid(epService, "select * from MarketData output when true then set 1",
                    "Error starting statement: Error in the output rate limiting clause: Missing variable assignment expression in assignment number 0 [select * from MarketData output when true then set 1]");
    
            TryInvalid(epService, "select TheString, count(*) from SupportBean#length(2) group by TheString output all every 0 seconds",
                    "Error starting statement: Invalid time period expression returns a zero or negative time interval [select TheString, count(*) from SupportBean#length(2) group by TheString output all every 0 seconds]");
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
