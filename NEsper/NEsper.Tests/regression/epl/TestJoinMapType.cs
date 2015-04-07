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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinMapType
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            IDictionary<String, Object> typeInfo = new Dictionary<String, Object>();
            typeInfo["Id"] = typeof (string);
            typeInfo["P00"] = typeof (int);

            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MapS0", typeInfo);
            config.AddEventType("MapS1", typeInfo);

            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void RunAssertion()
        {
            SendMapEvent("MapS0", "a", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendMapEvent("MapS1", "a", 2);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("a", theEvent.Get("S0.Id"));
            Assert.AreEqual("a", theEvent.Get("S1.Id"));
            Assert.AreEqual(1, theEvent.Get("S0.P00"));
            Assert.AreEqual(2, theEvent.Get("S1.P00"));

            SendMapEvent("MapS1", "b", 3);
            SendMapEvent("MapS0", "c", 4);
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void SendMapEvent(String name, String id, int p00)
        {
            IDictionary<String, Object> theEvent = new Dictionary<String, Object>();
            theEvent["Id"] = id;
            theEvent["P00"] = p00;
            _epService.EPRuntime.SendEvent(theEvent, name);
        }

        [Test]
        public void TestJoinMapEvent()
        {
            String joinStatement =
                "select S0.Id, S1.Id, S0.P00, S1.P00 from MapS0.win:keepall() as S0, MapS1.win:keepall() as S1" +
                " where S0.Id = S1.Id";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;

            RunAssertion();

            stmt.Dispose();
            joinStatement = "select * from MapS0.win:keepall() as S0, MapS1.win:keepall() as S1 where S0.Id = S1.Id";
            stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;

            RunAssertion();
        }

        [Test]
        public void TestJoinMapEventNotUnique()
        {
            // Test for Esper-122 
            String joinStatement =
                "select S0.Id, S1.Id, S0.P00, S1.P00 from MapS0.win:keepall() as S0, MapS1.win:keepall() as S1" +
                " where S0.Id = S1.Id";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;

            for (int i = 0; i < 100; i++)
            {
                if (i%2 == 1)
                {
                    SendMapEvent("MapS0", "a", 1);
                }
                else
                {
                    SendMapEvent("MapS1", "a", 1);
                }
            }
        }

        [Test]
        public void TestJoinWrapperEventNotUnique()
        {
            // Test for Esper-122
            _epService.EPAdministrator.CreateEPL("insert into S0 select 's0' as streamone, * from " +
                                                 typeof (SupportBean).FullName);
            _epService.EPAdministrator.CreateEPL("insert into S1 select 's1' as streamtwo, * from " +
                                                 typeof (SupportBean).FullName);
            String joinStatement =
                "select * from S0.win:keepall() as a, S1.win:keepall() as b where a.IntBoxed = b.IntBoxed";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;

            for (int i = 0; i < 100; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean());
            }
        }
    }
}
