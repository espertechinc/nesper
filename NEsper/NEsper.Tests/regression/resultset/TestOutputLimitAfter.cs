///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitAfter 
    {
        private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private string[] _fields;

        [SetUp]
	    public void SetUp()
	    {
            _fields = new string[] {"TheString"};
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
            _listener = new SupportUpdateListener();
        }

        [TearDown]
	    public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	        _fields = null;
	    }

        [Test]
        public void TestAfterWithOutputLast()
        {
            RunAssertionAfterWithOutputLast(false);
            RunAssertionAfterWithOutputLast(true);
        }

        private void RunAssertionAfterWithOutputLast(bool hinted)
        {
	         var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	         var epl = hint + "select sum(IntPrimitive) as thesum " +
	                 "from SupportBean.win:keepall() " +
	                 "output after 4 events last every 2 events";
	         var stmt = _epService.EPAdministrator.CreateEPL(epl);
	         stmt.AddListener(_listener);

	         _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	         _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	         _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
	         _epService.EPRuntime.SendEvent(new SupportBean("E4", 40));
	         _epService.EPRuntime.SendEvent(new SupportBean("E5", 50));
	         Assert.IsFalse(_listener.IsInvoked);

	         _epService.EPRuntime.SendEvent(new SupportBean("E6", 60));
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "thesum".Split(','), new object[] {210});

	         stmt.Dispose();
	     }

        [Test]
	     public void TestEveryPolicy()
	     {
	         SendTimer(0);
	         var stmtText = "select TheString from SupportBean.win:keepall() output after 0 days 0 hours 0 minutes 20 seconds 0 milliseconds every 0 days 0 hours 0 minutes 5 seconds 0 milliseconds";
	         var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	         stmt.AddListener(_listener);

	         RunAssertion();

	         var model = new EPStatementObjectModel();
	         model.SelectClause = SelectClause.Create("TheString");
	         model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("win", "keepall"));
	         model.OutputLimitClause = OutputLimitClause.Create(Expressions.TimePeriod(0, 0, 0, 5, 0)).SetAfterTimePeriodExpression(Expressions.TimePeriod(0, 0, 0, 20, 0));
	         Assert.AreEqual(stmtText, model.ToEPL());
	     }

        [Test]
	     public void TestMonthScoped() {
	         _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	         SendCurrentTime("2002-02-01T9:00:00.000");
	         _epService.EPAdministrator.CreateEPL("select * from SupportBean output after 1 month").AddListener(_listener);

	         _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	         SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
	         _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	         Assert.IsFalse(_listener.IsInvoked);

	         SendCurrentTime("2002-03-01T9:00:00.000");
	         _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[] {"E3"});
	     }

	     private void RunAssertion()
	     {
	         SendTimer(1);
	         SendEvent("E1");

	         SendTimer(6000);
	         SendEvent("E2");
	         SendTimer(16000);
	         SendEvent("E3");
	         Assert.IsFalse(_listener.IsInvoked);

	         SendTimer(20000);
	         SendEvent("E4");
	         Assert.IsFalse(_listener.IsInvoked);

	         SendTimer(24999);
	         SendEvent("E5");

	         SendTimer(25000);
	         EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, _fields, new object[][]{ new object[] {"E4"},  new object[] {"E5"}});
	         _listener.Reset();

	         SendTimer(27000);
	         SendEvent("E6");

	         SendTimer(29999);
	         Assert.IsFalse(_listener.IsInvoked);

	         SendTimer(30000);
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E6"});
	     }

        [Test]
	     public void TestDirectNumberOfEvents()
	     {
	         var stmtText = "select TheString from SupportBean.win:keepall() output after 3 events";
	         var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	         stmt.AddListener(_listener);

	         SendEvent("E1");
	         SendEvent("E2");
	         SendEvent("E3");
	         Assert.IsFalse(_listener.IsInvoked);

	         SendEvent("E4");
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E4"});

	         SendEvent("E5");
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E5"});

	         stmt.Dispose();

	         var model = new EPStatementObjectModel();
	         model.SelectClause = SelectClause.Create("TheString");
	         model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("win", "keepall"));
	         model.OutputLimitClause = OutputLimitClause.CreateAfter(3);
	         Assert.AreEqual("select TheString from SupportBean.win:keepall() output after 3 events ", model.ToEPL());

	         stmt = _epService.EPAdministrator.Create(model);
	         stmt.AddListener(_listener);

	         SendEvent("E1");
	         SendEvent("E2");
	         SendEvent("E3");
	         Assert.IsFalse(_listener.IsInvoked);

	         SendEvent("E4");
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E4"});

	         SendEvent("E5");
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E5"});

	         model = _epService.EPAdministrator.CompileEPL("select TheString from SupportBean.win:keepall() output after 3 events");
	         Assert.AreEqual("select TheString from SupportBean.win:keepall() output after 3 events ", model.ToEPL());
	     }

        [Test]
	     public void TestDirectTimePeriod()
	     {
	         SendTimer(0);
	         var stmtText = "select TheString from SupportBean.win:keepall() output after 20 seconds ";
	         var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	         stmt.AddListener(_listener);

	         SendTimer(1);
	         SendEvent("E1");

	         SendTimer(6000);
	         SendEvent("E2");

	         SendTimer(19999);
	         SendEvent("E3");
	         Assert.IsFalse(_listener.IsInvoked);

	         SendTimer(20000);
	         SendEvent("E4");
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E4"});

	         SendTimer(21000);
	         SendEvent("E5");
	         EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), _fields, new object[] {"E5"});
	     }

        [Test]
	     public void TestSnapshotVariable()
	     {
	         _epService.EPAdministrator.CreateEPL("create variable int myvar = 1");

	         SendTimer(0);
	         var stmtText = "select TheString from SupportBean.win:keepall() output after 20 seconds snapshot when myvar=1";
	         var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	         stmt.AddListener(_listener);

	         RunAssertionSnapshotVar();

	         stmt.Dispose();
	         var model = _epService.EPAdministrator.CompileEPL(stmtText);
	         Assert.AreEqual(stmtText, model.ToEPL());
	         stmt = _epService.EPAdministrator.Create(model);
	         Assert.AreEqual(stmtText, stmt.Text);
	     }

        [Test]
	     public void TestOutputWhenThen()
	     {
	         _epService.EPAdministrator.CreateEPL("create variable boolean myvar0 = false");
	         _epService.EPAdministrator.CreateEPL("create variable boolean myvar1 = false");
	         _epService.EPAdministrator.CreateEPL("create variable boolean myvar2 = false");

	         var epl = "@Name(\"select-streamstar+outputvar\")\n" +
	                 "select a.* from SupportBean.win:time(10) a output after 3 events when myvar0=true then set myvar1=true, myvar2=true";

	         var stmt = _epService.EPAdministrator.CreateEPL(epl);
	         stmt.AddListener(_listener);

	         SendEvent("E1");
	         SendEvent("E2");
	         SendEvent("E3");
	         Assert.IsFalse(_listener.IsInvoked);

	         _epService.EPRuntime.SetVariableValue("myvar0", true);
	         SendEvent("E4");
	         Assert.IsTrue(_listener.IsInvoked);

	         Assert.AreEqual(true, _epService.EPRuntime.GetVariableValue("myvar1"));
	         Assert.AreEqual(true, _epService.EPRuntime.GetVariableValue("myvar2"));
	     }

	     private void RunAssertionSnapshotVar()
	     {
	         SendTimer(6000);
	         SendEvent("E1");
	         SendEvent("E2");

	         SendTimer(19999);
	         SendEvent("E3");
	         Assert.IsFalse(_listener.IsInvoked);

	         SendTimer(20000);
	         SendEvent("E4");
	         EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, _fields, new object[][]{ new object[] {"E1"},  new object[] {"E2"},  new object[] {"E3"},  new object[] {"E4"}});
	         _listener.Reset();

	         SendTimer(21000);
	         SendEvent("E5");
	         EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, _fields, new object[][]{ new object[] {"E1"},  new object[] {"E2"},  new object[] {"E3"},  new object[] {"E4"},  new object[] {"E5"}});
	         _listener.Reset();
	     }

	     private void SendTimer(long time)
	     {
	         _epService.EPRuntime.SendEvent(new CurrentTimeEvent(time));
	     }

	     private void SendEvent(string theString)
	     {
	         _epService.EPRuntime.SendEvent(new SupportBean(theString, 0));
	     }

	     private void SendCurrentTime(string time) {
	         _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
	     }

	     private void SendCurrentTimeWithMinus(string time, long minus) {
             _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
	     }
	 }
} // end of namespace
