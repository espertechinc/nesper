///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.container;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;
using NUnit.Framework;

namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestCSVAdapter
    {
        private SupportUpdateListener _listener;
        private String _eventTypeName;
        private EPServiceProvider _epService;
        private long _currentTime;
        private InputAdapter _adapter;
        private String[] _propertyOrderTimestamps;
        private String[] _propertyOrderNoTimestamps;
        private IDictionary<String, Object> _propertyTypes;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _propertyTypes = new Dictionary<String, Object>();
            _propertyTypes.Put("myInt", typeof (int?));
            _propertyTypes.Put("myDouble", typeof (double?));
            _propertyTypes.Put("myString", typeof (String));

            _eventTypeName = "mapEvent";
            var configuration = new Configuration(_container);
            configuration.AddEventType(_eventTypeName, _propertyTypes);
            configuration.AddEventType("myNonMapEvent", typeof (Type).FullName);

            _epService = EPServiceProviderManager.GetProvider(_container, "CSVProvider", configuration);
            _epService.Initialize();
            EPAdministrator administrator = _epService.EPAdministrator;

            String statementText = "select * from mapEvent#length(5)";
            EPStatement statement = administrator.CreateEPL(statementText);

            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Turn off external clocking
            _epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));

            // Set the clock to 0
            _currentTime = 0;
            SendTimeEvent(0);

            _propertyOrderNoTimestamps = new[] {"myInt", "myDouble", "myString"};
            _propertyOrderTimestamps = new[] {"timestamp", "myInt", "myDouble", "myString"};
        }

        private void RunNullEPService(CSVInputAdapter adapter)
        {
            try {
                adapter.Start();
                Assert.Fail();
            }
            catch (EPException)
            {
                // Expected
            }

            try {
                adapter.EPService = null;
                Assert.Fail();
            }
            catch (ArgumentNullException) {
                // Expected
            }

            adapter.EPService = _epService;
            adapter.Start();
            Assert.AreEqual(3, _listener.GetNewDataList().Count);
        }

        private void AssertEvent(int howManyBack, int? myInt, double? myDouble, String myString)
        {
            Assert.IsTrue(_listener.IsInvoked());
            Assert.IsTrue(howManyBack < _listener.GetNewDataList().Count);
            EventBean[] data = _listener.GetNewDataList()[howManyBack];
            Assert.AreEqual(1, data.Length);
            EventBean theEvent = data[0];
            Assert.AreEqual(myInt, theEvent.Get("myInt"));
            Assert.AreEqual(myDouble, theEvent.Get("myDouble"));
            Assert.AreEqual(myString, theEvent.Get("myString"));
        }

        private void SendTimeEvent(int timeIncrement)
        {
            _currentTime += timeIncrement;
            var theEvent = new CurrentTimeEvent(_currentTime);
            _epService.EPRuntime.SendEvent(theEvent);
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
                Assert.IsFalse(_listener.GetAndClearIsInvoked());
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
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.GetLastNewData().Length);
            EventBean theEvent = _listener.GetLastNewData()[0];
            Assert.AreEqual(myInt, theEvent.Get("myInt"));
            Assert.AreEqual(myDouble, theEvent.Get("myDouble"));
            Assert.AreEqual(myString, theEvent.Get("myString"));
            _listener.Reset();
        }

        private void AssertTwoEvents(int? intOne, double? doubleOne, String stringOne,
                                     int? intTwo, double? doubleTwo, String stringTwo)
        {
            Assert.IsTrue(_listener.IsInvoked());
            Assert.AreEqual(2, _listener.GetNewDataList().Count);

            Assert.AreEqual(1, _listener.GetNewDataList()[0].Length);
            EventBean theEvent = _listener.GetNewDataList()[0][0];
            Assert.AreEqual(intOne, theEvent.Get("myInt"));
            Assert.AreEqual(doubleOne, theEvent.Get("myDouble"));
            Assert.AreEqual(stringOne, theEvent.Get("myString"));

            Assert.AreEqual(1, _listener.GetNewDataList()[1].Length);
            theEvent = _listener.GetNewDataList()[1][0];
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
                _listener.Reset();
            }
        }

        private void StartAdapter(String filename, int eventsPerSec, bool isLooping, bool usingEngineThread,
                                  String timestampColumn, String[] propertyOrder)
        {
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource(filename), _eventTypeName);
            if (eventsPerSec != -1) {
                adapterSpec.EventsPerSec = eventsPerSec;
            }
            adapterSpec.IsLooping = isLooping;
            adapterSpec.PropertyOrder = propertyOrder;
            adapterSpec.IsUsingEngineThread = usingEngineThread;
            adapterSpec.TimestampColumn = timestampColumn;

            _adapter = new CSVInputAdapter(_container, _epService, adapterSpec);
            _adapter.Start();
        }

        private void AssertFailedConstruction(String filename, String eventTypeName)
        {
            try {
                (new CSVInputAdapter(_container, _epService, new AdapterInputSource(filename), eventTypeName)).Start();
                Assert.Fail();
            }
            catch (EPException ) {
                // Expected
            }
        }

        [Test]
        public void TestAutoTyped()
        {
            var config = new Configuration(_container);
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            var adapter = new CSVInputAdapter(
                _container,
                epService,
                new AdapterInputSource(new StringReader("sym,price\nGOOG,22\nGOOG,33")),
                "MarketData"
                );
            try {
                epService.EPAdministrator.CreateEPL("select Sum(price) from MarketData#length(2)");
                Assert.Fail("should fail due to type conversion");
            }
            catch (EPStatementException e) {
                Assert.IsTrue(e.Message.Contains("Implicit conversion"));
            }

            var adapter2 = new CSVInputAdapter(
                _container,
                epService,
                new AdapterInputSource(new StringReader("sym,long price\nGOOG,22\nGOOG,33")),
                "MarketData2"
                );
            epService.EPAdministrator.CreateEPL("select Sum(price) from MarketData2#length(2)");
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
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
            AssertEvents(isLooping, events);

            isLooping = true;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
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
            _adapter = new CSVInputAdapter(_container, _epService, adapterSpec);

            const string statementText = "select * from intsTitleRowEvent#length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += _listener.Update;

            _adapter.Start();

            SendTimeEvent(100);

            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.GetLastNewData().Length);
            Assert.AreEqual("1", _listener.GetLastNewData()[0].Get("intTwo"));
            Assert.AreEqual("0", _listener.GetLastNewData()[0].Get("intOne"));
        }

        [Test]
        public void TestDestroy()
        {
            const string filename = "regression/timestampOne.csv";
            StartAdapter(filename, -1, false, true, "timestamp", _propertyOrderTimestamps);
            _adapter.Destroy();
            Assert.AreEqual(AdapterState.DESTROYED, _adapter.State);
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
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
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
            Stream stream = _container.ResourceManager().GetResourceAsStream("regression/noTimestampOne.csv");
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource(stream), _eventTypeName);
            adapterSpec.PropertyOrder = _propertyOrderNoTimestamps;

            new CSVInputAdapter(_container, _epService, adapterSpec);

            adapterSpec.IsLooping = true;
            try {
                new CSVInputAdapter(_container, _epService, adapterSpec);
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
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
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
            _propertyOrderNoTimestamps = null;
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
            _adapter = new CSVInputAdapter(_container, _epService, adapterSpec);

            const string statementText = "select * from allStringEvent#length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += _listener.Update;

            _adapter.Start();

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

            StartAdapter(filename, -1, false, true, null, _propertyOrderTimestamps);

            Assert.AreEqual(3, _listener.GetNewDataList().Count);
            AssertEvent(0, 1, 1.1, "timestampOne.one");
            AssertEvent(1, 3, 3.3, "timestampOne.three");
            AssertEvent(2, 5, 5.5, "timestampOne.five");
        }

        [Test]
        public void TestNotUsingEngineThreadTimestamp()
        {
            const string filename = "regression/timestampOne.csv";

            long startTime = Environment.TickCount;
            StartAdapter(filename, -1, false, false, "timestamp", _propertyOrderTimestamps);
            long endTime = Environment.TickCount;

            // The last event should be sent after 500 ms
            Assert.IsTrue(endTime - startTime > 500);

            Assert.AreEqual(3, _listener.GetNewDataList().Count);
            AssertEvent(0, 1, 1.1, "timestampOne.one");
            AssertEvent(1, 3, 3.3, "timestampOne.three");
            AssertEvent(2, 5, 5.5, "timestampOne.five");
        }

        [Test]
        public void TestNotUsingEngineThreaNoTimestamp()
        {
            const string filename = "regression/noTimestampOne.csv";

            long startTime = Environment.TickCount;
            StartAdapter(filename, 5, false, false, null, _propertyOrderNoTimestamps);
            long endTime = Environment.TickCount;

            // The last event should be sent after 600 ms
            Assert.IsTrue(endTime - startTime > 600);

            Assert.AreEqual(3, _listener.GetNewDataList().Count);
            AssertEvent(0, 1, 1.1, "noTimestampOne.one");
            AssertEvent(1, 2, 2.2, "noTimestampOne.two");
            AssertEvent(2, 3, 3.3, "noTimestampOne.three");
        }

        [Test]
        public void TestNullEPService()
        {
            var adapter = new CSVInputAdapter(
                _container, new AdapterInputSource("regression/titleRow.csv"), _eventTypeName);
            RunNullEPService(adapter);

            _listener.Reset();

            adapter = new CSVInputAdapter(_container, new AdapterInputSource("regression/titleRow.csv"), _eventTypeName);
            RunNullEPService(adapter);
        }

        [Test]
        public void TestPause()
        {
            const string filename = "regression/noTimestampOne.csv";
            StartAdapter(filename, 10, false, true, "timestamp", _propertyOrderNoTimestamps);

            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            _adapter.Pause();

            SendTimeEvent(100);
            Assert.AreEqual(AdapterState.PAUSED, _adapter.State);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestResumePartialInterval()
        {
            const string filename = "regression/noTimestampOne.csv";
            StartAdapter(filename, 10, false, true, null, _propertyOrderNoTimestamps);

            // time is 100
            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            // time is 150
            SendTimeEvent(50);

            _adapter.Pause();
            // time is 200
            SendTimeEvent(50);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            _adapter.Resume();

            AssertEvent(2, 2.2, "noTimestampOne.two");
        }

        [Test]
        public void TestResumeWholeInterval()
        {
            const string filename = "regression/noTimestampOne.csv";
            StartAdapter(filename, 10, false, true, null, _propertyOrderNoTimestamps);

            SendTimeEvent(100);
            AssertEvent(1, 1.1, "noTimestampOne.one");

            _adapter.Pause();
            SendTimeEvent(100);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            _adapter.Resume();


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
            StartAdapter(filename, -1, true, true, null, _propertyOrderTimestamps);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
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
            AssertFailedConstruction(filename, _eventTypeName);
        }

        [Test]
        public void TestRuntimePropertyTypes()
        {
            var adapterSpec = new CSVInputAdapterSpec(
                new AdapterInputSource("regression/noTimestampOne.csv"), "propertyTypeEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] { "myInt", "myDouble", "myString" };
            adapterSpec.PropertyTypes = _propertyTypes;
            adapterSpec.IsUsingEngineThread = true;
            _adapter = new CSVInputAdapter(_container, _epService, adapterSpec);

            const string statementText = "select * from propertyTypeEvent#length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            statement.Events += _listener.Update;

            _adapter.Start();

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
            var propertyTypesInvalid = new Dictionary<String, Object>(_propertyTypes);
            propertyTypesInvalid.Put("anotherProperty", typeof (String));
            try {
                var adapterSpec = new CSVInputAdapterSpec(
                    new AdapterInputSource("regression/noTimestampOne.csv"), "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(_container, _epService, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            propertyTypesInvalid = new Dictionary<String, Object>(_propertyTypes);
            propertyTypesInvalid.Put("myInt", typeof (String));
            try {
                var adapterSpec = new CSVInputAdapterSpec(
                    new AdapterInputSource("regression/noTimestampOne.csv"), "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(_container, _epService, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            propertyTypesInvalid = new Dictionary<String, Object>(_propertyTypes);
            propertyTypesInvalid.Remove("myInt");
            propertyTypesInvalid.Put("anotherInt", typeof (int?));
            try {
                var adapterSpec = new CSVInputAdapterSpec(
                    new AdapterInputSource("regression/noTimestampOne.csv"), "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(_container, _epService, adapterSpec)).Start();
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
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
            AssertEvents(isLooping, events);

            isLooping = true;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestRunTitleRowOnly()
        {
            const string filename = "regression/titleRowOnly.csv";
            _propertyOrderNoTimestamps = null;
            StartAdapter(filename, -1, true, true, "timestamp", null);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
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
            AssertFailedConstruction(filename, _eventTypeName);
        }

        [Test]
        public void TestStartOneRow()
        {
            const string filename = "regression/oneRow.csv";
            StartAdapter(filename, -1, false, true, "timestamp", _propertyOrderTimestamps);

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
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);

            AssertFlatEvents(events);

            _adapter.Stop();

            SendTimeEvent(1000);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            _adapter.Start();
            AssertFlatEvents(events);
        }

        [Test]
        public void TestStopAfterEOF()
        {
            const string filename = "regression/timestampOne.csv";
            StartAdapter(filename, -1, false, false, "timestamp", _propertyOrderTimestamps);
            Assert.AreEqual(AdapterState.OPENED, _adapter.State);
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
            _propertyOrderNoTimestamps = null;
            StartAdapter(filename, eventsPerSec, isLooping, true, null, null);
            AssertLoopingEvents(events);
        }
    }
}