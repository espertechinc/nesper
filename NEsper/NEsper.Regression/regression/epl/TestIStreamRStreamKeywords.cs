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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestIStreamRStreamKeywords 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _testListener;
	    private SupportUpdateListener _testListenerInsertInto;

        [SetUp]
	    public void SetUp()
	    {
	        _testListener = new SupportUpdateListener();
	        _testListenerInsertInto = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _testListener = null;
	        _testListenerInsertInto = null;
	    }

        [Test]
	    public void TestChangeEngineDefaultRStream()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.StreamSelection.DefaultStreamSelector = StreamSelector.RSTREAM_ONLY;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        string stmtText = "select * from " + typeof(SupportBean).FullName + "#length(3)";
	        EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_testListener);

	        object theEvent = SendEvent("a");
	        SendEvent("b");
	        SendEvent("c");
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("d");
	        Assert.IsTrue(_testListener.IsInvoked);
	        Assert.AreSame(theEvent, _testListener.LastNewData[0].Underlying);    // receive 'a' as new data
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	    }

        [Test]
	    public void TestChangeEngineDefaultIRStream()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.StreamSelection.DefaultStreamSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        string stmtText = "select * from " + typeof(SupportBean).FullName + "#length(3)";
	        EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_testListener);

	        object eventOld = SendEvent("a");
	        SendEvent("b");
	        SendEvent("c");
	        _testListener.Reset();

	        object eventNew = SendEvent("d");
	        Assert.IsTrue(_testListener.IsInvoked);
	        Assert.AreSame(eventNew, _testListener.LastNewData[0].Underlying);    // receive 'a' as new data
	        Assert.AreSame(eventOld, _testListener.LastOldData[0].Underlying);    // receive 'a' as new data
	    }

        [Test]
	    public void TestRStreamOnly_OM()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        string stmtText = "select rstream * from " + typeof(SupportBean).FullName + "#length(3)";
	        EPStatementObjectModel model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.CreateWildcard(StreamSelector.RSTREAM_ONLY);
	        FromClause fromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView(View.Create("length", Expressions.Constant(3))));
	        model.FromClause = fromClause;
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

	        Assert.AreEqual(stmtText, model.ToEPL());
	        EPStatement statement = _epService.EPAdministrator.Create(model);
	        statement.AddListener(_testListener);

	        object theEvent = SendEvent("a");
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvents(new string[] {"a", "b"});
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("d");
	        Assert.AreSame(theEvent, _testListener.LastNewData[0].Underlying);    // receive 'a' as new data
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        _testListener.Reset();
	    }

        [Test]
	    public void TestRStreamOnly_Compile()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        string stmtText = "select rstream * from " + typeof(SupportBean).FullName + "#length(3)";
	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

	        Assert.AreEqual(stmtText, model.ToEPL());
	        EPStatement statement = _epService.EPAdministrator.Create(model);
	        statement.AddListener(_testListener);

	        object theEvent = SendEvent("a");
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvents(new string[] {"a", "b"});
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("d");
	        Assert.AreSame(theEvent, _testListener.LastNewData[0].Underlying);    // receive 'a' as new data
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        _testListener.Reset();
	    }

        [Test]
	    public void TestRStreamOnly()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "select rstream * from " + typeof(SupportBean).FullName + "#length(3)");
	        statement.AddListener(_testListener);

	        object theEvent = SendEvent("a");
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvents(new string[] {"a", "b"});
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("d");
	        Assert.AreSame(theEvent, _testListener.LastNewData[0].Underlying);    // receive 'a' as new data
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        _testListener.Reset();
	    }

        [Test]
	    public void TestRStreamInsertInto()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "insert into NextStream " +
	                "select rstream s0.TheString as TheString from " + typeof(SupportBean).FullName + "#length(3) as s0");
	        statement.AddListener(_testListener);

	        statement = _epService.EPAdministrator.CreateEPL("select * from NextStream");
	        statement.AddListener(_testListenerInsertInto);

	        SendEvent("a");
	        Assert.IsFalse(_testListener.IsInvoked);
	        Assert.AreEqual("a", _testListenerInsertInto.AssertOneGetNewAndReset().Get("TheString"));    // insert into unchanged

	        SendEvents(new string[] {"b", "c"});
	        Assert.IsFalse(_testListener.IsInvoked);
	        Assert.AreEqual(2, _testListenerInsertInto.NewDataList.Count);    // insert into unchanged
	        _testListenerInsertInto.Reset();

	        SendEvent("d");
	        Assert.AreSame("a", _testListener.LastNewData[0].Get("TheString"));    // receive 'a' as new data
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        Assert.AreEqual("d", _testListenerInsertInto.LastNewData[0].Get("TheString"));    // insert into unchanged
	        Assert.IsNull(_testListenerInsertInto.LastOldData);  // receive no old data in insert into
	        _testListener.Reset();
	    }

        [Test]
	    public void TestRStreamInsertIntoRStream()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "insert rstream into NextStream " +
	                "select rstream s0.TheString as TheString from " + typeof(SupportBean).FullName + "#length(3) as s0");
	        statement.AddListener(_testListener);

	        statement = _epService.EPAdministrator.CreateEPL("select * from NextStream");
	        statement.AddListener(_testListenerInsertInto);

	        SendEvent("a");
	        Assert.IsFalse(_testListener.IsInvoked);
	        Assert.IsFalse(_testListenerInsertInto.IsInvoked);

	        SendEvents(new string[] {"b", "c"});
	        Assert.IsFalse(_testListener.IsInvoked);
	        Assert.IsFalse(_testListenerInsertInto.IsInvoked);

	        SendEvent("d");
	        Assert.AreSame("a", _testListener.LastNewData[0].Get("TheString"));    // receive 'a' as new data
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        Assert.AreEqual("a", _testListenerInsertInto.LastNewData[0].Get("TheString"));    // insert into unchanged
	        Assert.IsNull(_testListener.LastOldData);  // receive no old data in insert into
	        _testListener.Reset();
	    }

        [Test]
	    public void TestRStreamJoin()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "select rstream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
	                "from " + typeof(SupportBean).FullName + "(TheString='a')#length(2) as s1, "
	                        + typeof(SupportBean).FullName + "(TheString='b')#keepall as s2" +
	                " where s1.IntPrimitive = s2.IntPrimitive");
	        statement.AddListener(_testListener);

	        SendEvent("a", 1);
	        SendEvent("b", 1);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("a", 2);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("a", 3);
	        Assert.AreEqual(1, _testListener.LastNewData[0].Get("aID"));    // receive 'a' as new data
	        Assert.AreEqual(1, _testListener.LastNewData[0].Get("bID"));
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        _testListener.Reset();
	    }

        [Test]
	    public void TestIStreamOnly()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "select istream * from " + typeof(SupportBean).FullName + "#length(1)");
	        statement.AddListener(_testListener);

	        object theEvent = SendEvent("a");
	        Assert.AreSame(theEvent, _testListener.AssertOneGetNewAndReset().Underlying);

	        theEvent = SendEvent("b");
	        Assert.AreSame(theEvent, _testListener.LastNewData[0].Underlying);
	        Assert.IsNull(_testListener.LastOldData); // receive no old data, just istream events
	        _testListener.Reset();
	    }

        [Test]
	    public void TestIStreamInsertIntoRStream()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "insert rstream into NextStream " +
	                "select istream a.TheString as TheString from " + typeof(SupportBean).FullName + "#length(1) as a");
	        statement.AddListener(_testListener);

	        statement = _epService.EPAdministrator.CreateEPL("select * from NextStream");
	        statement.AddListener(_testListenerInsertInto);

	        SendEvent("a");
	        Assert.AreEqual("a", _testListener.AssertOneGetNewAndReset().Get("TheString"));
	        Assert.IsFalse(_testListenerInsertInto.IsInvoked);

	        SendEvent("b");
	        Assert.AreEqual("b", _testListener.LastNewData[0].Get("TheString"));
	        Assert.IsNull(_testListener.LastOldData);
	        Assert.AreEqual("a", _testListenerInsertInto.LastNewData[0].Get("TheString"));
	        Assert.IsNull(_testListenerInsertInto.LastOldData);
	    }

        [Test]
	    public void TestIStreamJoin()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(
	                "select istream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
	                "from " + typeof(SupportBean).FullName + "(TheString='a')#length(2) as s1, "
	                        + typeof(SupportBean).FullName + "(TheString='b')#keepall as s2" +
	                " where s1.IntPrimitive = s2.IntPrimitive");
	        statement.AddListener(_testListener);

	        SendEvent("a", 1);
	        SendEvent("b", 1);
	        Assert.AreEqual(1, _testListener.LastNewData[0].Get("aID"));    // receive 'a' as new data
	        Assert.AreEqual(1, _testListener.LastNewData[0].Get("bID"));
	        Assert.IsNull(_testListener.LastOldData);  // receive no more old data
	        _testListener.Reset();

	        SendEvent("a", 2);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent("a", 3);
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

	    private void SendEvents(string[] stringValue)
	    {
	        for (int i = 0; i < stringValue.Length; i++)
	        {
	            SendEvent(stringValue[i]);
	        }
	    }

	    private object SendEvent(string stringValue)
	    {
	        return SendEvent(stringValue, -1);
	    }

	    private object SendEvent(string stringValue, int IntPrimitive)
	    {
	        SupportBean theEvent = new SupportBean();
	        theEvent.TheString = stringValue;
	        theEvent.IntPrimitive = IntPrimitive;
	        _epService.EPRuntime.SendEvent(theEvent);
	        return theEvent;
	    }
	}
} // end of namespace
