///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
	public class TestNamedWindowConsumer 
	{
	    private EPServiceProviderSPI _epService;

        [SetUp]
	    public void SetUp()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
	        _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [Test]
	    public void TestLargeBatch()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}

	        _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
	                        "create schema IncomingEvent(id int);\n" +
	                        "create schema RetainedEvent(id int);\n" +
	                        "insert into RetainedEvent select * from IncomingEvent.win:expr_batch(current_count >= 10000);\n" +
	                        "create window RetainedEventWindow.win:keepall() as RetainedEvent;\n" +
	                        "insert into RetainedEventWindow select * from RetainedEvent;\n");

	        var @event = new Dictionary<string, object>();
	        @event.Put("id", 1);
	        for (int i = 0; i < 10000; i++) {
	            _epService.EPRuntime.SendEvent(@event, "IncomingEvent");
	        }
	    }
	}
} // end of namespace
