///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewParameterizedByContext
    {
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestContextParams() {
	        foreach (Type clazz in Collections.List(typeof(MyInitEventWLength), typeof(SupportBean))) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }

	        RunAssertionLengthWindow();
	        RunAssertionDocSample();

	        _epService.EPAdministrator.CreateEPL("create context CtxInitToTerm initiated by MyInitEventWLength as miewl terminated after 1 year");
	        RunAssertionWindow("length_batch(context.miewl.intSize)");
	        RunAssertionWindow("time(context.miewl.intSize)");
	        RunAssertionWindow("ext_timed(longPrimitive, context.miewl.intSize)");
	        RunAssertionWindow("time_batch(context.miewl.intSize)");
	        RunAssertionWindow("ext_timed_batch(longPrimitive, context.miewl.intSize)");
	        RunAssertionWindow("time_length_batch(context.miewl.intSize, context.miewl.intSize)");
	        RunAssertionWindow("time_accum(context.miewl.intSize)");
	        RunAssertionWindow("firstlength(context.miewl.intSize)");
	        RunAssertionWindow("firsttime(context.miewl.intSize)");
	        RunAssertionWindow("sort(context.miewl.intSize, intPrimitive)");
	        RunAssertionWindow("rank(theString, context.miewl.intSize, theString)");
	        RunAssertionWindow("time_order(longPrimitive, context.miewl.intSize)");
	    }

	    private void RunAssertionDocSample() {
	        string epl = "create schema ParameterEvent(windowSize int);" +
	                     "create context MyContext initiated by ParameterEvent as params terminated after 1 year;" +
	                     "context MyContext select * from SupportBean#length(context.params.windowSize);";
	        DeploymentResult deployed = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployed.DeploymentId);
	    }

	    private void RunAssertionWindow(string window) {
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("context CtxInitToTerm select * from SupportBean#" + window);
	        _epService.EPRuntime.SendEvent(new MyInitEventWLength("P1", 2));
	        stmt.Dispose();
	    }

	    private void RunAssertionLengthWindow() {
	        _epService.EPAdministrator.CreateEPL("create context CtxInitToTerm initiated by MyInitEventWLength as miewl terminated after 1 year");
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("context CtxInitToTerm select context.miewl.id as id, count(*) as cnt from SupportBean(theString=context.miewl.id)#length(context.miewl.intSize)");

	        _epService.EPRuntime.SendEvent(new MyInitEventWLength("P1", 2));
	        _epService.EPRuntime.SendEvent(new MyInitEventWLength("P2", 4));
	        _epService.EPRuntime.SendEvent(new MyInitEventWLength("P3", 3));
	        for (int i = 0; i < 10; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean("P1", 0));
	            _epService.EPRuntime.SendEvent(new SupportBean("P2", 0));
	            _epService.EPRuntime.SendEvent(new SupportBean("P3", 0));
	        }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "id,cnt".SplitCsv(), new object[][] { new object[] { "P1", 2L }, new object[] { "P2", 4L }, new object[] { "P3", 3L } });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    public class MyInitEventWLength
        {
	        public MyInitEventWLength(string id, int intSize)
            {
	            Id = id;
	            IntSize = intSize;
	        }

	        public string Id { get; private set; }

	        public int IntSize { get; private set; }
        }
	}
} // end of namespace
