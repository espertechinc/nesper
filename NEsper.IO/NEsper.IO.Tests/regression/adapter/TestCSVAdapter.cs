///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;
using NUnit.Framework;

namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestCSVAdapter
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            propertyTypes = new Dictionary<String, Object>();
            propertyTypes.Put("myInt", typeof (int?));
            propertyTypes.Put("myDouble", typeof (double?));
            propertyTypes.Put("myString", typeof (String));

            eventTypeName = "mapEvent";
            var configuration = new Configuration();
            configuration.AddEventType(eventTypeName, propertyTypes);
            configuration.AddEventType("myNonMapEvent", typeof (Type).FullName);

            epService = EPServiceProviderManager.GetProvider("CSVProvider", configuration);
            epService.Initialize();
            EPAdministrator administrator = epService.EPAdministrator;

            String statementText = "select * from mapEvent.win:length(5)";
            EPStatement statement = administrator.CreateEPL(statementText);

            listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            // Turn off external clocking
            epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));

            // Set the clock to 0
            currentTime = 0;
            SendTimeEvent(0);

            propertyOrderNoTimestamps = new[] {"myInt", "myDouble", "myString"};
            propertyOrderTimestamps = new[] {"timestamp", "myInt", "myDouble", "myString"};
        }

        #endregion

        private SupportUpdateListener listener;
        private String eventTypeName;
        private EPServiceProvider epService;
        private long currentTime;
        private InputAdapter adapter;
        private String[] propertyOrderTimestamps;
        private String[] propertyOrderNoTimestamps;
        private IDictionary<String, Object> propertyTypes;

        private void RunNullEPService(CSVInputAdapter adapter)
        {
            try {
                adapter.Start();
                Assert.Fail();
            }
            catch (EPException ex) {
                // Expected
            }

            try {
                adapter.EPService = null;
                Assert.Fail();
            }
            catch (ArgumentNullException) {
                // Expected
            }

            adapter.EPService = epService;
            adapter.Start();
            Assert.AreEqual(3, listener.GetNewDataList().Count);
        }

        private void AssertEvent(int howManyBack, int? myInt, double? myDouble, String myString)
        {
            Assert.IsTrue(listener.IsInvoked());
            Assert.IsTrue(howManyBack < listener.GetNewDataList().Count);
            EventBean[] data = listener.GetNewDataList()[howManyBack];
            Assert.AreEqual(1, data.Length);
            EventBean theEvent = data[0];
            Assert.AreEqual(myInt, theEvent.Get("myInt"));
            Assert.AreEqual(myDouble, theEvent.Get("myDouble"));
            Assert.AreEqual(myString, theEvent.Get("myString"));
        }

        private void SendTimeEvent(int timeIncrement)
        {
            currentTime += timeIncrement;
            var theEvent = new CurrentTimeEvent(currentTime);
            epService.EPRuntime.SendEvent(theEvent);
        }


        private void AssertEvents(bool isLooping, IEnumerable<object[]> events)
        {
            if (isLooping) {
                AssertLoopingEvents(events);
            }
            else {
                AssertNonLoopingEvents(events);
            }
        }


        private void AssertEvent(Object[] properties)
        {
            if (properties.Length == 1) {
                Assert.IsFalse(listener.GetAndClearIsInvoked());
            }
            else if (properties.Length == 4) {
                // properties = [callbackDelay, myInt, myDouble, myString]
                AssertEvent((int?) properties[1], (double?) properties[2], (String) properties[3]);
            }
            else {
                // properties = [callbackDelay, intOne, doubleOne, StringOne, intTwo, doubleTwo, stringTwo]
                AssertTwoEvents((int?) properties[1], (double?) properties[2], (String) properties[3],
                                (int?) properties[4], (double?) properties[5], (String) properties[6]);
            }
        }

        private void AssertEvent(Object myInt, Object myDouble, Object myString)
        {
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.GetLastNewData().Length);
            EventBean theEvent = listener.GetLastNewData()[0];
            Assert.AreEqual(myInt, theEvent.Get("myInt"));
            Assert.AreEqual(myDouble, theEvent.Get("myDouble"));
            Assert.AreEqual(myString, theEvent.Get("myString"));
            listener.Reset();
        }

        private void AssertTwoEvents(int? intOne, double? doubleOne, String stringOne,
                                     int? intTwo, double? doubleTwo, String stringTwo)
        {
            Assert.IsTrue(listener.IsInvoked());
            Assert.AreEqual(2, listener.GetNewDataList().Count);

            Assert.AreEqual(1, listener.GetNewDataList()[0].Length);
            EventBean theEvent = listener.GetNewDataList()[0][0];
            Assert.AreEqual(intOne, theEvent.Get("myInt"));
            Assert.AreEqual(doubleOne, theEvent.Get("myDouble"));
            Assert.AreEqual(stringOne, theEvent.Get("myString"));

            Assert.AreEqual(1, listener.GetNewDataList()[1].Length);
            theEvent = listener.GetNewDataList()[1][0];
            Assert.AreEqual(intTwo, theEvent.Get("myInt"));
            Assert.AreEqual(doubleTwo, theEvent.Get("myDouble"));
            Assert.AreEqual(stringTwo, theEvent.Get("myString"));
        }


        private void AssertNonLoopingEvents(IEnumerable<object[]> events)
        {
            AssertFlatEvents(events);

            SendTimeEvent(1000);
            AssertEvent(new Object[] {1000});
        }

        private void AssertLoopingEvents(IEnumerable<object[]> events)
        {
            AssertFlatEvents(events);
            AssertFlatEvents(events);
            AssertFlatEvents(events);
        }

        private void AssertFlatEvents(IEnumerable<object[]> events)
        {
            foreach (var theEvent in events) {
                SendTimeEvent((int) theEvent[0]);
                AssertEvent(theEvent);
                listener.Reset();
            }
        }

        private void StartAdapter(String filename, int eventsPerSec, bool isLooping, bool usingEngineThread,
                                  String timestampColumn, String[] propertyOrder)
        {
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource(filename), eventTypeName);
            if (eventsPerSec != -1) {
                adapterSpec.EventsPerSec = eventsPerSec;
            }
            adapterSpec.IsLooping = isLooping;
            adapterSpec.PropertyOrder = propertyOrder;
            adapterSpec.IsUsingEngineThread = usingEngineThread;
            adapterSpec.TimestampColumn = timestampColumn;

            adapter = new CSVInputAdapter(epService, adapterSpec);
            adapter.Start();
        }

        private void AssertFailedConstruction(String filename, String eventTypeName)
        {
            try {
                (new CSVInputAdapter(epService, new AdapterInputSource(filename), eventTypeName)).Start();
                Assert.Fail();
            }
            catch (EPException ) {
                // Expected
            }
        }

        [Test]
        public void TestAutoTyped()
        {
            var config = new Configuration();
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            var adapter = new CSVInputAdapter(
                epService,
                new AdapterInputSource(new StringReader("sym,price\nGOOG,22\nGOOG,33")),
                "MarketData"
                );
            try {
                epService.EPAdministrator.CreateEPL("select Sum(price) from MarketData.win:length(2)");
                Assert.Fail("should fail due to type conversion");
            }
            catch (EPStatementException e) {
                Assert.IsTrue(e.Message.Contains("Implicit conversion"));
            }

            var adapter2 = new CSVInputAdapter(
                epService,
                new AdapterInputSource(new StringReader("sym,long price\nGOOG,22\nGOOG,33")),
                "MarketData2"
                );
            epService.EPAdministrator.CreateEPL("select Sum(price) from MarketData2.win:length(2)");
        }

        [Test]
        public void TestComments()
        {
            const string filename = "regression/comments.csv";
            const int eventsPerSec = -1;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "one"});
            events.Add(new Object[] {200, 3, 3.3, "three"});
            events.Add(new Object[] {200, 5, 5.5, "five"});

            bool isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);
            AssertEvents(isLooping, events);

            isLooping = true;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestConflictingPropertyOrder()
        {
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource("regression/intsTitleRow.csv"),
                                                      "intsTitleRowEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] {"intTwo", "intOne"};
            adapterSpec.IsUsingEngineThread = true;
            adapter = new CSVInputAdapter(epService, adapterSpec);

            const string statementText = "select * from intsTitleRowEvent.win:length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;

            adapter.Start();

            SendTimeEvent(100);

            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.GetLastNewData().Length);
            Assert.AreEqual("1", listener.GetLastNewData()[0].Get("intTwo"));
            Assert.AreEqual("0", listener.GetLastNewData()[0].Get("intOne"));
        }

        [Test]
        public void TestDestroy()
        {
            const string filename = "regression/timestampOne.csv";
            StartAdapter(filename, -1, false, true, "timestamp", propertyOrderTimestamps);
            adapter.Destroy();
            Assert.AreEqual(AdapterState.DESTROYED, adapter.State);
        }

        [Test]
        public void TestEventsPerSecAndTimestamp()
        {
            const string filename = "regression/timestampOne.csv";
            const int eventsPerSec = 5;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {200, 1, 1.1, "timestampOne.one"});
            events.Add(new Object[] {200, 3, 3.3, "timestampOne.three"});
            events.Add(new Object[] {200, 5, 5.5, "timestampOne.five"});

            const bool isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestEventsPerSecInvalid()
        {
            const string filename = "regression/timestampOne.csv";

            try {
                StartAdapter(filename, 0, true, true, null, null);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // Expected
            }

            try {
                StartAdapter(filename, 1001, true, true, null, null);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // Expected
            }
        }

        [Test]
        public void TestFewerPropertiesToSend()
        {
            const string filename = "regression/moreProperties.csv";
            const int eventsPerSec = 10;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "moreProperties.one"});
            events.Add(new Object[] {100, 2, 2.2, "moreProperties.two"});
            events.Add(new Object[] {100, 3, 3.3, "moreProperties.three"});
            var propertyOrder = new[] {"someString", "myInt", "someInt", "myDouble", "myString"};

            const bool isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, null, propertyOrder);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestInputStream()
        {
            Stream stream = ResourceManager.GetResourceAsStream("regression/noTimestampOne.csv");
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource(stream), eventTypeName);
            adapterSpec.PropertyOrder = propertyOrderNoTimestamps;

            new CSVInputAdapter(epService, adapterSpec);

            adapterSpec.IsLooping = true;
            try {
                new CSVInputAdapter(epService, adapterSpec);
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
        }

        [Test]
        public void TestIsLoopingNoTitleRow()
        {
            const string filename = "regression/timestampOne.csv";
            const int eventsPerSec = -1;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "timestampOne.one"});
            events.Add(new Object[] {200, 3, 3.3, "timestampOne.three"});
            events.Add(new Object[] {200, 5, 5.5, "timestampOne.five"});

            const bool isLooping = true;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);
            AssertLoopingEvents(events);
        }

        [Test]
        public void TestIsLoopingTitleRow()
        {
            const string filename = "regression/titleRow.csv";
            const int eventsPerSec = -1;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "one"});
            events.Add(new Object[] {200, 3, 3.3, "three"});
            events.Add(new Object[] {200, 5, 5.5, "five"});

            const bool isLooping = true;
            propertyOrderNoTimestamps = null;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", null);
            AssertLoopingEvents(events);
        }

        [Test]
        public void TestNoPropertyTypes()
        {
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampOne.csv"),
                                                      "allStringEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] {"myInt", "myDouble", "myString"};
            adapterSpec.IsUsingEngineThread = true;
            adapter = new CSVInputAdapter(epService, adapterSpec);

            const string statementText = "select * from allStringEvent.win:length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;

            adapter.Start();

            SendTimeEvent(100);
            AssertEvent("1", "1.1", "noTimestampOne.one");

            SendTimeEvent(100);
            AssertEvent("2", "2.2", "noTimestampOne.two");

            SendTimeEvent(100);
            AssertEvent("3", "3.3", "noTimestampOne.three");
        }

        [Test]
        public void TestNoTimestampNoEventsPerSec()
        {
            const string filename = "regression/timestampOne.csv";

            StartAdapter(filename, -1, false, true, null, propertyOrderTimestamps);

            Assert.AreEqual(3, listener.GetNewDataList().Count);
            AssertEvent(0, 1, 1.1, "timestampOne.one");
            AssertEvent(1, 3, 3.3, "timestampOne.three");
            AssertEvent(2, 5, 5.5, "timestampOne.five");
        }

        [Test]
        public void TestNotUsingEngineThreadTimestamp()
        {
            const string filename = "regression/timestampOne.csv";

            long startTime = Environment.TickCount;
            StartAdapter(filename, -1, false, false, "timestamp", propertyOrderTimestamps);
            long endTime = Environment.TickCount;

            // The last event should be sent after 500 ms
            Assert.IsTrue(endTime - startTime > 500);

            Assert.AreEqual(3, listener.GetNewDataList().Count);
            AssertEvent(0, 1, 1.1, "timestampOne.one");
            AssertEvent(1, 3, 3.3, "timestampOne.three");
            AssertEvent(2, 5, 5.5, "timestampOne.five");
        }

        [Test]
        public void TestNotUsingEngineThreaNoTimestamp()
        {
            const string filename = "regression/noTimestampOne.csv";

            long startTime = Environment.TickCount;
            StartAdapter(filename, 5, false, false, null, propertyOrderNoTimestamps);
            long endTime = Environment.TickCount;

            // The last event should be sent after 600 ms
            Assert.IsTrue(endTime - startTime > 600);

            Assert.AreEqual(3, listener.GetNewDataList().Count);
            AssertEvent(0, 1, 1.1, "noTimestampOne.one");
            AssertEvent(1, 2, 2.2, "noTimestampOne.two");
            AssertEvent(2, 3, 3.3, "noTimestampOne.three");
        }

        [Test]
        public void TestNullEPService()
        {
            var adapter = new CSVInputAdapter(null, new AdapterInputSource("regression/titleRow.csv"), eventTypeName);
            RunNullEPService(adapter);

            listener.Reset();

            adapter = new CSVInputAdapter(new AdapterInputSource("regression/titleRow.csv"), eventTypeName);
            RunNullEPService(adapter);
        }

        [Test]
        public void TestPause()
        {
            const string filename = "regression/noTimestampOne.csv";
            StartAdapter(filename, 10, false, true, "timestamp", propertyOrderNoTimestamps);

            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            adapter.Pause();

            SendTimeEvent(100);
            Assert.AreEqual(AdapterState.PAUSED, adapter.State);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestResumePartialInterval()
        {
            const string filename = "regression/noTimestampOne.csv";
            StartAdapter(filename, 10, false, true, null, propertyOrderNoTimestamps);

            // time is 100
            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            // time is 150
            SendTimeEvent(50);

            adapter.Pause();
            // time is 200
            SendTimeEvent(50);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            adapter.Resume();

            AssertEvent(2, 2.2, "noTimestampOne.two");
        }

        [Test]
        public void TestResumeWholeInterval()
        {
            const string filename = "regression/noTimestampOne.csv";
            StartAdapter(filename, 10, false, true, null, propertyOrderNoTimestamps);

            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            adapter.Pause();
            SendTimeEvent(100);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            adapter.Resume();


            AssertEvent(2, 2.2, "noTimestampOne.two");
        }

        [Test]
        public void TestRunDecreasingTimestamps()
        {
            const string filename = "regression/decreasingTimestamps.csv";
            try {
                StartAdapter(filename, -1, false, true, null, null);

                SendTimeEvent(100);
                AssertEvent(1, 1.1, "one");

                SendTimeEvent(200);
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
        }

        [Test]
        public void TestRunEmptyFile()
        {
            const string filename = "regression/emptyFile.csv";
            StartAdapter(filename, -1, true, true, null, propertyOrderTimestamps);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestRunNegativeTimestamps()
        {
            const string filename = "regression/negativeTimestamps.csv";
            try {
                StartAdapter(filename, -1, false, true, null, null);

                SendTimeEvent(100);
                AssertEvent(1, 1.1, "one");

                SendTimeEvent(200);
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
        }

        [Test]
        public void TestRunNonexistentFile()
        {
            const string filename = "someNonexistentFile";
            AssertFailedConstruction(filename, eventTypeName);
        }

        [Test]
        public void TestRuntimePropertyTypes()
        {
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampOne.csv"),
                                                      "propertyTypeEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] { "myInt", "myDouble", "myString" };
            adapterSpec.PropertyTypes = propertyTypes;
            adapterSpec.IsUsingEngineThread = true;
            adapter = new CSVInputAdapter(epService, adapterSpec);

            const string statementText = "select * from propertyTypeEvent.win:length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += listener.Update;

            adapter.Start();

            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            SendTimeEvent(100);
            AssertEvent(2, 2.2, "noTimestampOne.two");

            SendTimeEvent(100);
            AssertEvent(3, 3.3, "noTimestampOne.three");
        }

        [Test]
        public void TestRuntimePropertyTypesInvalid()
        {
            var propertyTypesInvalid = new Dictionary<String, Object>(propertyTypes);
            propertyTypesInvalid.Put("anotherProperty", typeof (String));
            try {
                var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampOne.csv"),
                                                          "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(epService, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            propertyTypesInvalid = new Dictionary<String, Object>(propertyTypes);
            propertyTypesInvalid.Put("myInt", typeof (String));
            try {
                var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampOne.csv"),
                                                          "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(epService, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            propertyTypesInvalid = new Dictionary<String, Object>(propertyTypes);
            propertyTypesInvalid.Remove("myInt");
            propertyTypesInvalid.Put("anotherInt", typeof (int?));
            try {
                var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampOne.csv"),
                                                          "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(epService, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
        }

        [Test]
        public void TestRunTimestamps()
        {
            const string filename = "regression/timestampOne.csv";
            const int eventsPerSec = -1;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "timestampOne.one"});
            events.Add(new Object[] {200, 3, 3.3, "timestampOne.three"});
            events.Add(new Object[] {200, 5, 5.5, "timestampOne.five"});

            bool isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);
            AssertEvents(isLooping, events);

            isLooping = true;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestRunTitleRowOnly()
        {
            const string filename = "regression/titleRowOnly.csv";
            propertyOrderNoTimestamps = null;
            StartAdapter(filename, -1, true, true, "timestamp", null);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestRunWrongAlias()
        {
            const string filename = "regression/noTimestampOne.csv";
            AssertFailedConstruction(filename, "myNonMapEvent");
        }

        [Test]
        public void TestRunWrongMapType()
        {
            const string filename = "regression/differentMap.csv";
            AssertFailedConstruction(filename, eventTypeName);
        }

        [Test]
        public void TestStartOneRow()
        {
            const string filename = "regression/oneRow.csv";
            StartAdapter(filename, -1, false, true, "timestamp", propertyOrderTimestamps);

            SendTimeEvent(100);
            AssertEvent(1, 1.1, "one");
        }

        [Test]
        public void TestStop()
        {
            const string filename = "regression/timestampOne.csv";
            const int eventsPerSec = -1;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "timestampOne.one"});
            events.Add(new Object[] {200, 3, 3.3, "timestampOne.three"});

            const bool isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", propertyOrderTimestamps);

            AssertFlatEvents(events);

            adapter.Stop();

            SendTimeEvent(1000);
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            adapter.Start();
            AssertFlatEvents(events);
        }

        [Test]
        public void TestStopAfterEOF()
        {
            const string filename = "regression/timestampOne.csv";
            StartAdapter(filename, -1, false, false, "timestamp", propertyOrderTimestamps);
            Assert.AreEqual(AdapterState.OPENED, adapter.State);
        }

        [Test]
        public void TestTitleRowNoTimestamp()
        {
            const string filename = "regression/titleRowNoTimestamp.csv";
            int eventsPerSec = 10;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "one"});
            events.Add(new Object[] {100, 3, 3.3, "three"});
            events.Add(new Object[] {100, 5, 5.5, "five"});

            bool isLooping = true;
            propertyOrderNoTimestamps = null;
            StartAdapter(filename, eventsPerSec, isLooping, true, null, null);
            AssertLoopingEvents(events);
        }
    }
}