///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseNoJoinIteratePerf 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.LRUCache = 100000;
            configDB.ConnectionCatalog = "test";

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);

            _epService = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epService.Dispose();
        }
    
        [Test]
        public void TestVariablesPollPerformanceCache()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.CreateEPL("create variable boolean queryvar_bool");
            _epService.EPAdministrator.CreateEPL("create variable int lower");
            _epService.EPAdministrator.CreateEPL("create variable int upper");
            _epService.EPAdministrator.CreateEPL("on SupportBean set queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed");
    
            const string stmtText = "select * from sql:MyDB ['select mybigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by mybigint']";
            var fields = new[] {"mybigint", "mybool"};
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            SendSupportBeanEvent(true, 20, 60);
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i ++)
            {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new[] { new Object[] {4L, true}});
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 1000, "delta=" + delta);
    
            stmt.Dispose();
        }
    
        private void SendSupportBeanEvent(bool boolPrimitive, int intPrimitive, int intBoxed)
        {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
