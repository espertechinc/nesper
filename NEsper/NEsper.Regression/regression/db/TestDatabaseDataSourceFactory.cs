///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
	public class TestDatabaseDataSourceFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [Test]
	    public void TestDBCP()
        {
	        var props = new Properties();
	        props.Put("driverClassName", SupportDatabaseService.DRIVER);
	        props.Put("url", SupportDatabaseService.FULLURL);
	        props.Put("username", SupportDatabaseService.DBUSER);
	        props.Put("password", SupportDatabaseService.DBPWD);

	        var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
	        configDB.SetDataSourceFactory(props, typeof(SupportDataSourceFactory).Name);
	        configDB.ConnectionLifecycle = ConnectionLifecycleEnum.POOLED;
	        configDB.LRUCache = 100;

	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddDatabaseReference("MyDB", configDB);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }

	        RunAssertion();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

	    private void RunAssertion() {
	        var stmtText = "select istream myint from " +
	                          " sql:MyDB ['select myint from mytesttable where ${intPrimitive} = mytesttable.mybigint'] as s0," +
	                          typeof(SupportBean).Name + " as s1";
	        var statement = _epService.EPAdministrator.CreateEPL(stmtText);

	        var fields = new string[] {"myint"};
	        _listener = new SupportUpdateListener();
	        statement.AddListener(_listener);

	        SendSupportBeanEvent(10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {100});

	        SendSupportBeanEvent(6);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {60});

            var startTime = DateTimeHelper.CurrentTimeMillis;
	        // Send 100 events which all fireStatementStopped a join
	        for (var i = 0; i < 100; i++) {
	            SendSupportBeanEvent(10);
	            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {100});
	        }
	        var endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info("delta=" + (endTime - startTime));
	        Assert.IsTrue(endTime - startTime < 5000);
	    }

	    private void SendSupportBeanEvent(int intPrimitive) {
	        var bean = new SupportBean();
	        bean.IntPrimitive = intPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
