///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
	/// <summary>
	/// Test for populating an empty type:
	/// - an empty insert-into property list is allowed, i.e. "insert into EmptySchema()"
	/// - an empty select-clause is not allowed, i.e. "select from xxx" fails
	/// - we require "select null from" (unnamed null column) for populating an empty type
	/// </summary>
    [TestFixture]
	public class TestInsertIntoEmptyPropType 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNamedWindowModelAfter() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S1>();

	        _epService.EPAdministrator.CreateEPL("create schema EmptyPropSchema()");
	        var stmtCreateWindow = _epService.EPAdministrator.CreateEPL("create window EmptyPropWin#keepall as EmptyPropSchema");
	        _epService.EPAdministrator.CreateEPL("insert into EmptyPropWin() select null from SupportBean");

	        _epService.EPRuntime.SendEvent(new SupportBean());

            EventBean[] events = EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator());
	        Assert.AreEqual(1, events.Length);
	        Assert.AreEqual("EmptyPropWin", events[0].EventType.Name);

	        // try fire-and-forget query
	        _epService.EPRuntime.ExecuteQuery("insert into EmptyPropWin select null");
            Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator()).Length);
	        _epService.EPRuntime.ExecuteQuery("delete from EmptyPropWin"); // empty window

	        // try on-merge
	        _epService.EPAdministrator.CreateEPL("on SupportBean_S0 merge EmptyPropWin " +
	                "when not matched then insert select null");
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(1, EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator()).Length);

	        // try on-insert
	        _epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into EmptyPropWin select null");
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(stmtCreateWindow.GetEnumerator()).Length);
	    }

        [Test]
	    public void TestCreateSchemaInsertInto() {
	        RunAssertionInsertMap(true);
	        RunAssertionInsertMap(false);
	        RunAssertionInsertOA();
	        RunAssertionInsertBean();
	    }

	    private void RunAssertionInsertBean() {
	        _epService.EPAdministrator.CreateEPL("create schema MyBeanWithoutProps as " + typeof(MyBeanWithoutProps).MaskTypeName());
	        _epService.EPAdministrator.CreateEPL("insert into MyBeanWithoutProps select null from SupportBean");

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from MyBeanWithoutProps");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        Assert.IsTrue(_listener.AssertOneGetNewAndReset().Underlying is MyBeanWithoutProps);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionInsertMap(bool soda) {
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create map schema EmptyMapSchema as ()");
	        _epService.EPAdministrator.CreateEPL("insert into EmptyMapSchema() select null from SupportBean");

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from EmptyMapSchema");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        var @event = _listener.AssertOneGetNewAndReset();
	        Assert.IsTrue(@event.Underlying.AsBasicDictionary<string, object>().IsEmpty());
	        Assert.AreEqual(0, @event.EventType.PropertyDescriptors.Count);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionInsertOA() {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema EmptyOASchema()");
	        _epService.EPAdministrator.CreateEPL("insert into EmptyOASchema select null from SupportBean");

	        var supportSubscriber = new SupportSubscriber();
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from EmptyOASchema");
	        stmt.AddListener(_listener);
	        stmt.Subscriber = supportSubscriber;

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        Assert.AreEqual(0, ((object[]) _listener.AssertOneGetNewAndReset().Underlying).Length);

	        var lastNewSubscriberData = supportSubscriber.GetLastNewData();
	        Assert.AreEqual(1, lastNewSubscriberData.Length);
	        Assert.AreEqual(0, ((object[]) lastNewSubscriberData[0]).Length);
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    public class MyBeanWithoutProps
        {
	    }
	}
} // end of namespace
