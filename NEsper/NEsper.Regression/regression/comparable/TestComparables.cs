///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.comparable
{
    class TestComparables
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportVersionObject>();

            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestComparableProperties()
        {
            var admin = _epService.EPAdministrator;
            var runtime = _epService.EPRuntime;

            using (admin
                .CreateEPL("select * from SupportVersionObject where VersionA > VersionB")
                .AddListener(_listener))
            {
                Assert.That(_listener.IsInvoked, Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(2, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(2, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 1), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
            }

            using (admin
                .CreateEPL("select * from SupportVersionObject where VersionA >= VersionB")
                .AddListener(_listener))
            {
                Assert.That(_listener.IsInvoked, Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(2, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(2, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 1), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
            }

            using (admin
                .CreateEPL("select * from SupportVersionObject where VersionA < VersionB")
                .AddListener(_listener))
            {
                Assert.That(_listener.IsInvoked, Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(2, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(2, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 1), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
            }

            using (admin
                .CreateEPL("select * from SupportVersionObject where VersionA <= VersionB")
                .AddListener(_listener))
            {
                Assert.That(_listener.IsInvoked, Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(2, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(2, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 1), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.False);
                runtime.SendEvent(new SupportVersionObject(new SupportVersion(1, 0, 0), new SupportVersion(1, 0, 0)));
                Assert.That(_listener.IsInvokedAndReset(), Is.True);
            }
        }
    }
}
