///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowLateStartIndex 
    {
        private EPServiceProviderSPI _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyCountAccessEvent));
            _listener = new SupportUpdateListener();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestLateStartIndex()
        {
             // prepare
            PreloadData(false);
    
            // test join
            string eplJoin = "select * from SupportBean_S0 as s0 unidirectional, AWindow(p00='x') as aw where aw.id = s0.id";
            _epService.EPAdministrator.CreateEPL(eplJoin).AddListener(_listener);
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(2, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "x"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            // test subquery no-index-share
            string eplSubqueryNoIndexShare = "select (select id from AWindow(p00='x') as aw where aw.id = s0.id) " +
                    "from SupportBean_S0 as s0 unidirectional";
            _epService.EPAdministrator.CreateEPL(eplSubqueryNoIndexShare).AddListener(_listener);
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(2, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "x"));
    
            // test subquery with index share
            _epService.EPAdministrator.DestroyAllStatements();
            PreloadData(true);
    
            string eplSubqueryWithIndexShare = "select (select id from AWindow(p00='x') as aw where aw.id = s0.id) " +
                    "from SupportBean_S0 as s0 unidirectional";
            _epService.EPAdministrator.CreateEPL(eplSubqueryWithIndexShare).AddListener(_listener);
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(2, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "x"));
            Assert.IsTrue(_listener.IsInvoked);
        }
    
        private void PreloadData(bool indexShare)
        {
            string createEpl = "create window AWindow.win:keepall() as MyCountAccessEvent";
            if (indexShare) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
    
            _epService.EPAdministrator.CreateEPL(createEpl);
            _epService.EPAdministrator.CreateEPL("insert into AWindow select * from MyCountAccessEvent");
            _epService.EPAdministrator.CreateEPL("create index I1 on AWindow(p00)");
            MyCountAccessEvent.GetAndResetCountGetterCalled();
            for (int i = 0; i < 100; i++) {
                _epService.EPRuntime.SendEvent(new MyCountAccessEvent(i, "E" + i));
            }
            _epService.EPRuntime.SendEvent(new MyCountAccessEvent(-1, "x"));
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(101, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
        }
    
        public class MyCountAccessEvent
        {
            private static int _countGetterCalled;

            private readonly string _p00;
    
            public MyCountAccessEvent(int id, string p00) {
                Id = id;
                _p00 = p00;
            }
    
            public static int GetAndResetCountGetterCalled()
            {
                int value = _countGetterCalled;
                _countGetterCalled = 0;
                return value;
            }

            public int Id { get; private set; }

            public string P00
            {
                get
                {
                    _countGetterCalled++;
                    return _p00;
                }
            }
        }
    }
}
