///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestCurrentEvaluationContextExpr 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestExecutionContext()
	    {
	        RunAssertionExecCtx(false);
	        RunAssertionExecCtx(true);
	    }

	    private void RunAssertionExecCtx(bool soda) {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
	        SendTimer(0);

	        string epl = "select " +
	                "current_evaluation_context() as c0, " +
	                "current_evaluation_context(), " +
	                "current_evaluation_context().get_EngineURI() as c2 from SupportBean";
	        EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl, "my_user_object");
	        stmt.AddListener(_listener);

	        Assert.AreEqual(typeof(EPLExpressionEvaluationContext), stmt.EventType.GetPropertyType("current_evaluation_context()"));

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EventBean @event = _listener.AssertOneGetNewAndReset();
	        EPLExpressionEvaluationContext ctx = (EPLExpressionEvaluationContext) @event.Get("c0");
	        Assert.AreEqual(_epService.URI, ctx.EngineURI);
	        Assert.AreEqual(stmt.Name, ctx.StatementName);
	        Assert.AreEqual(-1, ctx.ContextPartitionId);
	        Assert.AreEqual("my_user_object", ctx.StatementUserObject);
	        Assert.AreEqual(_epService.URI, @event.Get("c2"));

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void SendTimer(long timeInMSec)
	    {
	        CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
	        EPRuntime runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }
	}
} // end of namespace
