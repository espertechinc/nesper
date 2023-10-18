///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;
using NUnit.Framework;

using static com.espertech.esperio.support.util.CompileUtil;

namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestCSVAdapter
    {
        private SupportUpdateListener _listener;
        private String _eventTypeName;
        private EPRuntime _runtime;
        private EPRuntimeProvider _runtimeProvider;
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
            _propertyTypes.Put("MyInt", typeof(int?));
            _propertyTypes.Put("MyDouble", typeof(double?));
            _propertyTypes.Put("MyString", typeof(String));

            _eventTypeName = "mapEvent";
            var configuration = new Configuration(_container);
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Common.AddEventType(_eventTypeName, _propertyTypes);
            configuration.Common.AddEventType("myNonMapEvent", typeof(Type).FullName);

            _runtimeProvider = new EPRuntimeProvider();
            _runtime = _runtimeProvider.GetRuntimeInstance("CSVProvider", configuration);
            _runtime.Initialize();

            var statementText = "select * from mapEvent#length(5)";
            var statement = CompileDeploy(_runtime, statementText).Statements[0];

            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Set the clock to 0
            _currentTime = 0;
            SendTimeEvent(0);

            _propertyOrderNoTimestamps = new[] {"MyInt", "MyDouble", "MyString"};
            _propertyOrderTimestamps = new[] {"timestamp", "MyInt", "MyDouble", "MyString"};
        }

        private void RunNullEPService(CSVInputAdapter adapter)
        {
            try {
                adapter.Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            try {
                adapter.Runtime = null;
                Assert.Fail();
            }
            catch (ArgumentNullException) {
                // Expected
            }

            adapter.Runtime = _runtime;
            adapter.Start();
            Assert.AreEqual(3, _listener.GetNewDataList().Count);
        }

        private void AssertEvent(int howManyBack,
            int? myInt,
            double? myDouble,
            String myString)
        {
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsTrue(howManyBack < _listener.GetNewDataList().Count);
            var data = _listener.GetNewDataList()[howManyBack];
            Assert.AreEqual(1, data.Length);
            var theEvent = data[0];
            Assert.AreEqual(myInt, theEvent.Get("MyInt"));
            Assert.AreEqual(myDouble, theEvent.Get("MyDouble"));
            Assert.AreEqual(myString, theEvent.Get("MyString"));
        }

        private void SendTimeEvent(int timeIncrement)
        {
            _currentTime += timeIncrement;
            _runtime.EventService.AdvanceTime(_currentTime);
        }


        private void AssertEvents(bool isLooping,
            IEnumerable<object[]> events)
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
                AssertTwoEvents(
                    (int?) properties[1],
                    (double?) properties[2],
                    (String) properties[3],
                    (int?) properties[4],
                    (double?) properties[5],
                    (String) properties[6]);
            }
        }

        private void AssertEvent(Object myInt,
            Object myDouble,
            Object myString)
        {
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.GetLastNewData().Length);
            var theEvent = _listener.GetLastNewData()[0];
            Assert.AreEqual(myInt, theEvent.Get("MyInt"));
            Assert.AreEqual(myDouble, theEvent.Get("MyDouble"));
            Assert.AreEqual(myString, theEvent.Get("MyString"));
            _listener.Reset();
        }

        private void AssertTwoEvents(int? intOne,
            double? doubleOne,
            String stringOne,
            int? intTwo,
            double? doubleTwo,
            String stringTwo)
        {
            Assert.IsTrue(_listener.IsInvoked);
            Assert.AreEqual(2, _listener.GetNewDataList().Count);

            Assert.AreEqual(1, _listener.GetNewDataList()[0].Length);
            var theEvent = _listener.GetNewDataList()[0][0];
            Assert.AreEqual(intOne, theEvent.Get("MyInt"));
            Assert.AreEqual(doubleOne, theEvent.Get("MyDouble"));
            Assert.AreEqual(stringOne, theEvent.Get("MyString"));

            Assert.AreEqual(1, _listener.GetNewDataList()[1].Length);
            theEvent = _listener.GetNewDataList()[1][0];
            Assert.AreEqual(intTwo, theEvent.Get("MyInt"));
            Assert.AreEqual(doubleTwo, theEvent.Get("MyDouble"));
            Assert.AreEqual(stringTwo, theEvent.Get("MyString"));
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

        private void StartAdapter(String filename,
            int eventsPerSec,
            bool isLooping,
            bool usingEngineThread,
            String timestampColumn,
            String[] propertyOrder)
        {
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource(_container, filename), _eventTypeName);
            if (eventsPerSec != -1) {
                adapterSpec.EventsPerSec = eventsPerSec;
            }

            adapterSpec.IsLooping = isLooping;
            adapterSpec.PropertyOrder = propertyOrder;
            adapterSpec.IsUsingEngineThread = usingEngineThread;
            adapterSpec.TimestampColumn = timestampColumn;

            _adapter = new CSVInputAdapter(_runtime, adapterSpec);
            _adapter.Start();
        }

        private void AssertFailedConstruction(String filename,
            String eventTypeName)
        {
            try {
                (new CSVInputAdapter(_runtime, new AdapterInputSource(_container, filename), eventTypeName)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
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

            var isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
            AssertEvents(isLooping, events);

            isLooping = true;
            StartAdapter(filename, eventsPerSec, isLooping, true, "timestamp", _propertyOrderTimestamps);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestConflictingPropertyOrder()
        {
            CompileDeploy(_runtime, "@public @buseventtype create schema intsTitleRowEvent(intOne string, intTwo string)");

            var adapterSpec = new CSVInputAdapterSpec(
                new AdapterInputSource(_container, "regression/intsTitleRow.csv"),
                "intsTitleRowEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] {"intTwo", "intOne"};
            adapterSpec.IsUsingEngineThread = true;
            _adapter = new CSVInputAdapter(_runtime, adapterSpec);

            const string statementText = "select * from intsTitleRowEvent#length(5)";
            var statement = CompileDeploy(_runtime, statementText).Statements[0];
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
            var propertyOrder = new[] {"SomeString", "MyInt", "SomeInt", "MyDouble", "MyString"};

            const bool isLooping = false;
            StartAdapter(filename, eventsPerSec, isLooping, true, null, propertyOrder);
            AssertEvents(isLooping, events);
        }

        [Test]
        public void TestInputStream()
        {
            var stream = _container.ResourceManager().GetResourceAsStream("regression/noTimestampOne.csv");
            var adapterSpec = new CSVInputAdapterSpec(new AdapterInputSource(_container, stream), _eventTypeName);
            adapterSpec.PropertyOrder = _propertyOrderNoTimestamps;

            new CSVInputAdapter(_runtime, adapterSpec);

            adapterSpec.IsLooping = true;
            try {
                new CSVInputAdapter(_runtime, adapterSpec);
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
            CompileDeploy(_runtime, "@public @buseventtype create schema allStringEvent(MyInt string, MyDouble string, MyString string)");

            var adapterSpec = new CSVInputAdapterSpec(
                new AdapterInputSource(_container, "regression/noTimestampOne.csv"),
                "allStringEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] {"MyInt", "MyDouble", "MyString"};
            adapterSpec.IsUsingEngineThread = true;
            _adapter = new CSVInputAdapter(_runtime, adapterSpec);

            const string statementText = "select * from allStringEvent#length(5)";
            var statement = CompileDeploy(_runtime, statementText).Statements[0];
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
                new AdapterInputSource(_container, "regression/titleRow.csv"),
                _eventTypeName);
            RunNullEPService(adapter);

            _listener.Reset();

            adapter = new CSVInputAdapter(new AdapterInputSource(_container, "regression/titleRow.csv"), _eventTypeName);
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
            CompileDeploy(_runtime, "@public @buseventtype create schema propertyTypeEvent(MyInt int, MyDouble double, MyString string)");

            var adapterSpec = new CSVInputAdapterSpec(
                new AdapterInputSource(_container, "regression/noTimestampOne.csv"),
                "propertyTypeEvent");
            adapterSpec.EventsPerSec = 10;
            adapterSpec.PropertyOrder = new[] {"MyInt", "MyDouble", "MyString"};
            adapterSpec.PropertyTypes = _propertyTypes;
            adapterSpec.IsUsingEngineThread = true;
            _adapter = new CSVInputAdapter(_runtime, adapterSpec);

            const string statementText = "select * from propertyTypeEvent#length(5)";
            var statement = CompileDeploy(_runtime, statementText).Statements[0];
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
            propertyTypesInvalid.Put("anotherProperty", typeof(String));
            try {
                var adapterSpec = new CSVInputAdapterSpec(
                    new AdapterInputSource(_container, "regression/noTimestampOne.csv"),
                    "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(_runtime, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            propertyTypesInvalid = new Dictionary<String, Object>(_propertyTypes);
            propertyTypesInvalid.Put("MyInt", typeof(String));
            try {
                var adapterSpec = new CSVInputAdapterSpec(
                    new AdapterInputSource(_container, "regression/noTimestampOne.csv"),
                    "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(_runtime, adapterSpec)).Start();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }

            propertyTypesInvalid = new Dictionary<String, Object>(_propertyTypes);
            propertyTypesInvalid.Remove("MyInt");
            propertyTypesInvalid.Put("anotherInt", typeof(int?));
            try {
                var adapterSpec = new CSVInputAdapterSpec(
                    new AdapterInputSource(_container, "regression/noTimestampOne.csv"),
                    "mapEvent");
                adapterSpec.PropertyTypes = propertyTypesInvalid;
                (new CSVInputAdapter(_runtime, adapterSpec)).Start();
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

            var isLooping = false;
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
            var eventsPerSec = 10;

            IList<Object[]> events = new List<Object[]>();
            events.Add(new Object[] {100, 1, 1.1, "one"});
            events.Add(new Object[] {100, 3, 3.3, "three"});
            events.Add(new Object[] {100, 5, 5.5, "five"});

            var isLooping = true;
            _propertyOrderNoTimestamps = null;
            StartAdapter(filename, eventsPerSec, isLooping, true, null, null);
            AssertLoopingEvents(events);
        }
    }
}