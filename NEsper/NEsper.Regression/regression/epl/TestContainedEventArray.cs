///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestContainedEventArray 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestContainedEventArrayAll() {
	        RunAssertion();
	        RunDocSample();
	    }

	    private void RunDocSample() {
	        _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
	                "create schema IdContainer(id int);" +
	                "create schema MyEvent(ids int[]);" +
	                "select * from MyEvent[ids@type(IdContainer)];");

	        _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
	                "create window MyWindow#keepall (id int);" +
	                "on MyEvent[ids@type(IdContainer)] as my_ids \n" +
	                "delete from MyWindow my_window \n" +
	                "where my_ids.id = my_window.id;");
	    }

	    private void RunAssertion() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanArrayCollMap));

	        string epl = "create objectarray schema DeleteId(id int);" +
	                     "create window MyWindow#keepall as SupportBean;" +
	                     "insert into MyWindow select * from SupportBean;" +
	                     "on SupportBeanArrayCollMap[intArr@type(DeleteId)] delete from MyWindow where intPrimitive = id";
	        DeploymentResult deployed = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

	        AssertCount(2);
	        _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[] {1, 2}));
	        AssertCount(0);

	        _epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
	    }

	    private void AssertCount(long i) {
	        Assert.AreEqual(i, _epService.EPRuntime.ExecuteQuery("select count(*) as c0 from MyWindow").Array[0].Get("c0"));
	    }
	}
} // end of namespace
