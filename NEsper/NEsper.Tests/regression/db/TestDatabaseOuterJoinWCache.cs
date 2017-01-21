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
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
	public class TestDatabaseOuterJoinWCache 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        ConfigurationDBRef configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
	        configDB.ConnectionCatalog = "test";
	        configDB.SetExpiryTimeCache(60, 120);

	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
	        configuration.AddDatabaseReference("MyDB", configDB);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	        _epService.Dispose();
	    }

        [Test]
	    public void TestOuterJoinWithCache()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        string stmtText = "select * from SupportBean as sb " +
	                "left outer join " +
	                "sql:MyDB ['select myint from mytesttable'] as t " +
	                "on sb.intPrimitive = t.myint " +
	                "where myint is null";

	        EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        _listener = new SupportUpdateListener();
	        statement.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	    }
	}
} // end of namespace
