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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOutputLimitCrontabWhen 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MarketData", typeof(SupportMarketDataBean));
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestOutputCrontabAtVariable() {
    
            // every 15 minutes 8am to 5pm
            SendTimeEvent(1, 17, 10, 0, 0);
            _epService.EPAdministrator.CreateEPL("create variable int VFREQ = 15");
            _epService.EPAdministrator.CreateEPL("create variable int VMIN = 8");
            _epService.EPAdministrator.CreateEPL("create variable int VMAX = 17");
            String expression = "select * from MarketData.std:lastevent() output at (*/VFREQ, VMIN:VMAX, *, *, *)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            stmt.Events += _listener.Update;
            RunAssertionCrontab(1, stmt);
        }
    
        [Test]
        public void TestOutputCrontabAt() {
    
            // every 15 minutes 8am to 5pm
            SendTimeEvent(1, 17, 10, 0, 0);
            String expression = "select * from MarketData.std:lastevent() output at (*/15, 8:17, *, *, *)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            stmt.Events += _listener.Update;
            RunAssertionCrontab(1, stmt);
        }
    
        [Test]
        public void TestOutputCrontabAtOMCreate() {
    
            // every 15 minutes 8am to 5pm
            SendTimeEvent(1, 17, 10, 0, 0);
            String expression = "select * from MarketData.std:lastevent() output at (*/15, 8:17, *, *, *)";
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create("MarketData").AddView("std", "lastevent"));
            Expression[] crontabParams = new Expression[] {
                    Expressions.CrontabScheduleFrequency(15),
                    Expressions.CrontabScheduleRange(8, 17),
                    Expressions.CrontabScheduleWildcard(),
                    Expressions.CrontabScheduleWildcard(),
                    Expressions.CrontabScheduleWildcard()
                };
            model.OutputLimitClause = OutputLimitClause.CreateSchedule(crontabParams);
    
            String epl = model.ToEPL();
            Assert.AreEqual(expression, epl);
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            RunAssertionCrontab(1, stmt);
        }
    
        [Test]
        public void TestOutputCrontabAtOMCompile()
        {
            // every 15 minutes 8am to 5pm
            SendTimeEvent(1, 17, 10, 0, 0);
            String expression = "select * from MarketData.std:lastevent() output at (*/15, 8:17, *, *, *)";
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            RunAssertionCrontab(1, stmt);
        }
    
        private void RunAssertionCrontab(int days, EPStatement statement)
        {
            String[] fields = "symbol".Split(',');
            SendEvent("S1", 0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimeEvent(days, 17, 14, 59, 0);
            SendEvent("S2", 0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimeEvent(days, 17, 15, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S1"}, new Object[] {"S2"}});
    
            SendTimeEvent(days, 17, 18, 0, 00);
            SendEvent("S3", 0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimeEvent(days, 17, 30, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S3"}});
    
            SendTimeEvent(days, 17, 35, 0, 0);
            SendTimeEvent(days, 17, 45, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
    
            SendEvent("S4", 0);
            SendEvent("S5", 0);
            SendTimeEvent(days, 18, 0, 0, 0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimeEvent(days, 18, 1, 0, 0);
            SendEvent("S6", 0);
    
            SendTimeEvent(days, 18, 15, 0, 0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimeEvent(days+1, 7, 59, 59, 0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimeEvent(days+1, 8, 0, 0, 0);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S4"}, new Object[] {"S5"}, new Object[] {"S6"}});
    
            statement.Dispose();
            _listener.Reset();
        }
    
        [Test]
        public void TestOutputWhenThenExpression()
        {
            SendTimeEvent(1, 8, 0, 0, 0);
            _epService.EPAdministrator.Configuration.AddVariable("myvar", typeof(int), 0);
            _epService.EPAdministrator.Configuration.AddVariable("count_insert_var", typeof(int), 0);
            _epService.EPAdministrator.CreateEPL("on SupportBean set myvar = IntPrimitive");
    
            String expression = "select symbol from MarketData.win:length(2) output when myvar=1 then set myvar=0, count_insert_var=count_insert";
            EPStatement stmt =  _epService.EPAdministrator.CreateEPL(expression);
            RunAssertion(1, stmt);
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol");
            model.FromClause = FromClause.Create(FilterStream.Create("MarketData").AddView("win", "length", Expressions.Constant(2)));
            model.OutputLimitClause = OutputLimitClause.Create(Expressions.Eq("myvar", 1))
                .AddThenAssignment(Expressions.Eq(Expressions.Property("myvar"), Expressions.Constant(0)))
                .AddThenAssignment(Expressions.Eq(Expressions.Property("count_insert_var"), Expressions.Property("count_insert")));
    
            String epl = model.ToEPL();
            Assert.AreEqual(expression, epl);
            stmt = _epService.EPAdministrator.Create(model);
            RunAssertion(2, stmt);
    
            model = _epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            RunAssertion(3, stmt);
    
            String outputLast = "select symbol from MarketData.win:length(2) output last when myvar=1 ";
            model = _epService.EPAdministrator.CompileEPL(outputLast);
            Assert.AreEqual(outputLast.Trim(), model.ToEPL().Trim());
    
            // test same variable referenced multiple times JIRA-386
            SendTimer(0);
            SupportUpdateListener listenerOne = new SupportUpdateListener();
            SupportUpdateListener listenerTwo = new SupportUpdateListener();
            EPStatement stmtOne =  _epService.EPAdministrator.CreateEPL("select * from MarketData output last when myvar=100");
            stmtOne.Events += listenerOne.Update;
            EPStatement stmtTwo =  _epService.EPAdministrator.CreateEPL("select * from MarketData output last when myvar=100");
            stmtTwo.Events += listenerTwo.Update;
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", "E1", 100));
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", "E2", 100));
    
            SendTimer(1000);
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            _epService.EPRuntime.SetVariableValue("myvar", 100);
            SendTimer(2000);
            Assert.IsTrue(listenerTwo.IsInvoked);
            Assert.IsTrue(listenerOne.IsInvoked);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
    
            // test when-then with condition triggered by output events
            SendTimeEvent(2, 8, 0, 0, 0);
            String eplToDeploy = "create variable bool varOutputTriggered = false\n;" +
                    "@Audit @Name('out') select * from SupportBean.std:lastevent() output snapshot when (count_insert > 1 and varOutputTriggered = false) then set varOutputTriggered = true;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplToDeploy);
            _epService.EPAdministrator.GetStatement("out").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(_listener.IsInvoked);
            
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual("E2", _listener.AssertOneGetNewAndReset().Get("TheString"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SetVariableValue("varOutputTriggered", false); // turns true right away as triggering output
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            SendTimeEvent(2, 8, 0, 1, 0);
            Assert.AreEqual("E5", _listener.AssertOneGetNewAndReset().Get("TheString"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test count_total for insert and remove
            _epService.EPAdministrator.CreateEPL("create variable int var_cnt_total = 3");
            String expressionTotal = "select TheString from SupportBean.win:length(2) output when count_insert_total = var_cnt_total or count_remove_total > 2";
            EPStatement stmtTotal =  _epService.EPAdministrator.CreateEPL(expressionTotal);
            stmtTotal.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','), new Object[][] { new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPRuntime.SetVariableValue("var_cnt_total", -1);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','), new Object[][] { new Object[] {"E4"}, new Object[] {"E5"}});
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion(int days, EPStatement stmt)
        {
            SupportSubscriber subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent("S1", 0);
    
            // now scheduled for output
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("myvar"));
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendTimeEvent(days, 8, 0, 1, 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S1"}, subscriber.GetAndResetLastNewData());
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("myvar"));
            Assert.AreEqual(1, _epService.EPRuntime.GetVariableValue("count_insert_var"));
    
            SendEvent("S2", 0);
            SendEvent("S3", 0);
            SendTimeEvent(days, 8, 0, 2, 0);
            SendTimeEvent(days, 8, 0, 3, 0);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("myvar"));
            Assert.AreEqual(2, _epService.EPRuntime.GetVariableValue("count_insert_var"));
    
            Assert.IsFalse(subscriber.IsInvoked());
            SendTimeEvent(days, 8, 0, 4, 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S2", "S3"}, subscriber.GetAndResetLastNewData());
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("myvar"));
    
            SendTimeEvent(days, 8, 0, 5, 0);
            Assert.IsFalse(subscriber.IsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("myvar"));
            Assert.IsFalse(subscriber.IsInvoked());
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestOutputWhenExpression()
        {
            SendTimeEvent(1, 8, 0, 0, 0);
            _epService.EPAdministrator.Configuration.AddVariable("myint", typeof(int), 0);
            _epService.EPAdministrator.Configuration.AddVariable("mystring", typeof(String), "");
            _epService.EPAdministrator.CreateEPL("on SupportBean set myint = IntPrimitive, mystring = TheString");
    
            String expression = "select symbol from MarketData.win:length(2) output when myint = 1 and mystring like 'F%'";
            EPStatement stmt =  _epService.EPAdministrator.CreateEPL(expression);
            SupportSubscriber subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent("S1", 0);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, _epService.EPRuntime.GetVariableValue("myint"));
            Assert.AreEqual("E1", _epService.EPRuntime.GetVariableValue("mystring"));
    
            SendEvent("S2", 0);
            SendTimeEvent(1, 8, 0, 1, 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("F1", 0));
            Assert.AreEqual(0, _epService.EPRuntime.GetVariableValue("myint"));
            Assert.AreEqual("F1", _epService.EPRuntime.GetVariableValue("mystring"));
    
            SendTimeEvent(1, 8, 0, 2, 0);
            SendEvent("S3", 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("F2", 1));
            Assert.AreEqual(1, _epService.EPRuntime.GetVariableValue("myint"));
            Assert.AreEqual("F2", _epService.EPRuntime.GetVariableValue("mystring"));
    
            SendEvent("S4", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S1", "S2", "S3", "S4"}, subscriber.GetAndResetLastNewData());
        }
    
        [Test]
        public void TestOutputWhenBuiltInCountInsert()
        {
            String expression = "select symbol from MarketData.win:length(2) output when count_insert >= 3";
            EPStatement stmt =  _epService.EPAdministrator.CreateEPL(expression);
            SupportSubscriber subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent("S1", 0);
            SendEvent("S2", 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendEvent("S3", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S1", "S2", "S3"}, subscriber.GetAndResetLastNewData());
    
            SendEvent("S4", 0);
            SendEvent("S5", 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendEvent("S6", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S4", "S5", "S6"}, subscriber.GetAndResetLastNewData());
    
            SendEvent("S7", 0);
            Assert.IsFalse(subscriber.IsInvoked());
        }
    
        [Test]
        public void TestOutputWhenBuiltInCountRemove()
        {
            String expression = "select symbol from MarketData.win:length(2) output when count_remove >= 2";
            EPStatement stmt =  _epService.EPAdministrator.CreateEPL(expression);
            SupportSubscriber subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent("S1", 0);
            SendEvent("S2", 0);
            SendEvent("S3", 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendEvent("S4", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S1", "S2", "S3", "S4"}, subscriber.GetAndResetLastNewData());
    
            SendEvent("S5", 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendEvent("S6", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S5", "S6"}, subscriber.GetAndResetLastNewData());
    
            SendEvent("S7", 0);
            Assert.IsFalse(subscriber.IsInvoked());
        }
    
        [Test]
        public void TestOutputWhenBuiltInLastTimestamp()
        {
            SendTimeEvent(1, 8, 0, 0, 0);
            String expression = "select symbol from MarketData.win:length(2) output when current_timestamp - last_output_timestamp >= 2000";
            EPStatement stmt =  _epService.EPAdministrator.CreateEPL(expression);
            SupportSubscriber subscriber = new SupportSubscriber();
            stmt.Subscriber = subscriber;
    
            SendEvent("S1", 0);
    
            SendTimeEvent(1, 8, 0, 1, 900);
            SendEvent("S2", 0);
    
            SendTimeEvent(1, 8, 0, 2, 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendEvent("S3", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S1", "S2", "S3"}, subscriber.GetAndResetLastNewData());
    
            SendTimeEvent(1, 8, 0, 3, 0);
            SendEvent("S4", 0);
    
            SendTimeEvent(1, 8, 0, 3, 500);
            SendEvent("S5", 0);
            Assert.IsFalse(subscriber.IsInvoked());
    
            SendTimeEvent(1, 8, 0, 4, 0);
            SendEvent("S6", 0);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{"S4", "S5", "S6"}, subscriber.GetAndResetLastNewData());
        }
    
        private void SendEvent(String symbol, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimeEvent(int day, int hour, int minute, int second, int millis)
        {
            var dateTime = new DateTime(2008, 1, day, hour, minute, second, millis, DateTimeKind.Local);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(dateTime));
        }
    
        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddVariable("myvardummy", typeof(int), 0);
            _epService.EPAdministrator.Configuration.AddVariable("myvarlong", typeof(long), 0);
    
            TryInvalid("select * from MarketData output when sum(price) > 0",
                       "Error validating expression: Failed to validate output limit expression '(sum(price))>0': Property named 'price' is not valid in any stream [select * from MarketData output when sum(price) > 0]");
    
            TryInvalid("select * from MarketData output when sum(count_insert) > 0",
                       "Error validating expression: An aggregate function may not appear in a OUTPUT LIMIT clause [select * from MarketData output when sum(count_insert) > 0]");
    
            TryInvalid("select * from MarketData output when prev(1, count_insert) = 0",
                       "Error validating expression: Failed to validate output limit expression 'prev(1,count_insert)=0': Previous function cannot be used in this context [select * from MarketData output when prev(1, count_insert) = 0]");
    
            TryInvalid("select * from MarketData output when myvardummy",
                       "Error validating expression: The when-trigger expression in the OUTPUT WHEN clause must return a boolean-type value [select * from MarketData output when myvardummy]");
    
            TryInvalid("select * from MarketData output when true then set myvardummy = 'b'",
                       "Error starting statement: Error in the output rate limiting clause: Variable 'myvardummy' of declared type " + typeof(int?).FullName + " cannot be assigned a value of type System.String [select * from MarketData output when true then set myvardummy = 'b']");
    
            TryInvalid("select * from MarketData output when true then set myvardummy = sum(myvardummy)",
                       "Error validating expression: An aggregate function may not appear in a OUTPUT LIMIT clause [select * from MarketData output when true then set myvardummy = sum(myvardummy)]");
    
            TryInvalid("select * from MarketData output when true then set 1",
                        "Error starting statement: Error in the output rate limiting clause: Missing variable assignment expression in assignment number 0 [select * from MarketData output when true then set 1]");

            TryInvalid("select TheString, count(*) from SupportBean.win:length(2) group by TheString output all every 0 seconds",
                       "Error starting statement: Invalid time period expression returns a zero or negative time interval [select TheString, count(*) from SupportBean.win:length(2) group by TheString output all every 0 seconds]");
        }
    
        private void TryInvalid(String expression, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
