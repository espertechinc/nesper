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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabase3StreamOuterJoin  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);

            var configuration = SupportConfigFactory.GetConfiguration();
    
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.EngineDefaults.Logging.IsEnableADO = true;
            configuration.AddDatabaseReference("MyDB", configDB);
    
            _epService = EPServiceProviderManager.GetProvider(
                    "TestDatabaseJoinRetained", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBeanTwo", typeof(SupportBeanTwo));
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _epService.Dispose();
        }
    
        [Test]
        public void TestInnerJoinLeftS0() {
            String stmtText = "select * from SupportBean#lastevent sb"
                    + " inner join " + " SupportBeanTwo#lastevent sbt"
                    + " on sb.TheString = sbt.stringTwo " + " inner join "
                    + " sql:MyDB ['select myint from mytesttable'] as s1 "
                    + "  on s1.myint = sbt.IntPrimitiveTwo";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                    stmtText);
    
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("T1", -1));
    
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T2", 30));
            _epService.EPRuntime.SendEvent(new SupportBean("T2", -1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[]
                    {
                "T2", "T2", 30
            }
                    );
    
            _epService.EPRuntime.SendEvent(new SupportBean("T3", -1));
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T3", 40));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[]
                    {
                "T3", "T3", 40
            }
                    );
        }
    
        [Test]
        public void TestOuterJoinLeftS0() {
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBeanTwo", typeof(SupportBeanTwo));
            String stmtText = "select * from SupportBean#lastevent sb"
                    + " left outer join " + " SupportBeanTwo#lastevent sbt"
                    + " on sb.TheString = sbt.stringTwo " + " left outer join "
                    + " sql:MyDB ['select myint from mytesttable'] as s1 "
                    + "  on s1.myint = sbt.IntPrimitiveTwo";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                    stmtText);
    
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("T1", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[]
                    {
                "T1", "T1", null
            }
                    );
    
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T2", 30));
            _epService.EPRuntime.SendEvent(new SupportBean("T2", -2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[]
                    {
                "T2", "T2", 30
            }
                    );
    
            _epService.EPRuntime.SendEvent(new SupportBean("T3", -1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[]
                    {
                "T3", null, null
            }
                    );
    
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T3", 40));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[]
                    {
                "T3", "T3", 40
            }
                    );
        }
    }
}
