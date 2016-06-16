///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraSubqCorrelCoerce 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("S0Bean", typeof(SupportBean_S0));
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestSubqueryIndex() {
            // named window tests
            RunAssertion(true, false, false, false); // no share
            RunAssertion(true, false, false, true); // no share create index
            RunAssertion(true, true, false, false); // share
            RunAssertion(true, true, false, true); // share create index
            RunAssertion(true, true, true, false); // disable share
            RunAssertion(true, true, true, true); // disable share create index
    
            // table tests
            RunAssertion(false, false, false, false); // table
            RunAssertion(false, false, false, true); // table + create index
        }
    
        private void RunAssertion(bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex) {
            _epService.EPAdministrator.CreateEPL("create schema EventSchema(e0 string, e1 int, e2 string)");
            _epService.EPAdministrator.CreateEPL("create schema WindowSchema(col0 string, col1 long, col2 string)");
    
            string createEpl = namedWindow ?
                    "create window MyInfra.win:keepall() as WindowSchema" :
                    "create table MyInfra (col0 string primary key, col1 long, col2 string)";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            _epService.EPAdministrator.CreateEPL(createEpl);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select * from WindowSchema");
    
            EPStatement stmtIndex = null;
            if (createExplicitIndex) {
                stmtIndex = _epService.EPAdministrator.CreateEPL("create index MyIndex on MyInfra (col2, col1)");
            }
    
            string[] fields = "e0,val".Split(',');
            string consumeEpl = "select e0, (select col0 from MyInfra where col2 = es.e2 and col1 = es.e1) as val from EventSchema es";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }
            EPStatement consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.AddListener(_listener);
    
            SendWindow("W1", 10L, "c31");
            SendEvent("E1", 10, "c31");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "W1"});
    
            SendEvent("E2", 11, "c32");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", null});
    
            SendWindow("W2", 11L, "c32");
            SendEvent("E3", 11, "c32");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", "W2"});
    
            SendWindow("W3", 11L, "c31");
            SendWindow("W4", 10L, "c32");
    
            SendEvent("E4", 11, "c31");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "W3"});
    
            SendEvent("E5", 10, "c31");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E5", "W1"});
    
            SendEvent("E6", 10, "c32");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E6", "W4"});
    
            // test late start
            consumeStmt.Dispose();
            consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.AddListener(_listener);
    
            SendEvent("E6", 10, "c32");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E6", "W4"});
    
            if (stmtIndex != null) {
                stmtIndex.Dispose();
            }
            consumeStmt.Dispose();
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void SendWindow(string col0, long col1, string col2) {
            var theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("col0", col0);
            theEvent.Put("col1", col1);
            theEvent.Put("col2", col2);
            if (EventRepresentationEnumExtensions.GetEngineDefault(_epService).IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "WindowSchema");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "WindowSchema");
            }
        }
    
        private void SendEvent(string e0, int e1, string e2) {
            var theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("e0", e0);
            theEvent.Put("e1", e1);
            theEvent.Put("e2", e2);
            if (EventRepresentationEnumExtensions.GetEngineDefault(_epService).IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "EventSchema");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "EventSchema");
            }
        }
    }
}
