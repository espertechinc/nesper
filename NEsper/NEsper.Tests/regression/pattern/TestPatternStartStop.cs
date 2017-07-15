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
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestPatternStartStop
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
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
        public void TestStartStop()
        {
            String viewExpr = "@IterableUnbound every tag=" + typeof(SupportBean).FullName;
            EPStatement patternStmt = _epService.EPAdministrator.CreatePattern(viewExpr, "MyPattern");
            Assert.AreEqual(StatementType.PATTERN, ((EPStatementSPI)patternStmt).StatementMetadata.StatementType);

            // Pattern started when created
            Assert.IsFalse(patternStmt.HasFirst());
            using (IEnumerator<EventBean> safe = patternStmt.GetSafeEnumerator())
            {
                Assert.IsFalse(safe.MoveNext());
            }

            // Stop pattern
            patternStmt.Stop();
            SendEvent();
            Assert.That(patternStmt.GetEnumerator(), Is.InstanceOf<NullEnumerator<EventBean>>());

            // Start pattern
            patternStmt.Start();
            Assert.IsFalse(patternStmt.HasFirst());

            // Send event
            SupportBean theEvent = SendEvent();
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
            using (var safe = patternStmt.GetSafeEnumerator())
            {
                Assert.That(safe.MoveNext(), Is.True);
                Assert.AreSame(theEvent, safe.Current.Get("tag"));
            }

            // Stop pattern
            patternStmt.Stop();
            Assert.That(patternStmt.GetEnumerator(), Is.InstanceOf<NullEnumerator<EventBean>>());

            // Start again, iterator is zero
            patternStmt.Start();
            Assert.IsFalse(patternStmt.HasFirst());

            // assert statement-eventtype reference INFO
            EPServiceProviderSPI spi = (EPServiceProviderSPI)_epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            ICollection<String> stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsTrue(stmtNames.Contains("MyPattern"));

            patternStmt.Dispose();

            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsFalse(stmtNames.Contains("MyPattern"));
        }

        [Test]
        public void TestAddRemoveListener()
        {
            String viewExpr = "@IterableUnbound every tag=" + typeof(SupportBean).FullName;
            EPStatement patternStmt = _epService.EPAdministrator.CreatePattern(viewExpr, "MyPattern");
            Assert.AreEqual(StatementType.PATTERN, ((EPStatementSPI)patternStmt).StatementMetadata.StatementType);

            // Pattern started when created

            // Add listener
            patternStmt.Events += _listener.Update;
            Assert.IsNull(_listener.LastNewData);
            Assert.IsFalse(patternStmt.HasFirst());

            // Send event
            SupportBean theEvent = SendEvent();
            Assert.AreEqual(theEvent, _listener.GetAndResetLastNewData()[0].Get("tag"));
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));

            // Remove listener
            patternStmt.Events -= _listener.Update;
            theEvent = SendEvent();
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
            Assert.IsNull(_listener.LastNewData);

            // Add listener back
            patternStmt.Events += _listener.Update;
            theEvent = SendEvent();
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
            Assert.AreEqual(theEvent, _listener.GetAndResetLastNewData()[0].Get("tag"));
        }

        private SupportBean SendEvent()
        {
            SupportBean theEvent = new SupportBean();
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
}
