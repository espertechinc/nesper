///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.linq
{
    [TestFixture]
    public class TestLinqServiceProviderExtensions
    {
        private EPServiceProvider _serviceProvider;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var configuration = new Configuration(_container);
            _serviceProvider = EPServiceProviderManager.GetDefaultProvider(configuration);
            _serviceProvider.Initialize();
        }

        [Test]
        public void TestCreateNamedWindow()
        {
            var view = View.Create("keepall");
            var selectA = _serviceProvider.From<SupportBean>(typeof(SupportBean));

            _serviceProvider.EPAdministrator.CreateEPL(
                "create window testWindow1#keepall as select * from " + Name.Clean<SupportBean>());

            using (var statement = _serviceProvider.CreateWindow("testWindow2", view, selectA))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound create window testWindow2#keepall as select * from " + Name.Clean<SupportBean>() + " as s0");
            }
        }

        [Test]
        public void TestCreateVariable()
        {
            _serviceProvider.CreateVariable("varA1", typeof(int));
            using (var stream = _serviceProvider.From<EventBean>(typeof(SupportBean))
                .AddProperty("varA1").Select())
            {
                _serviceProvider.EPRuntime.SendEvent(new SupportBean());
                Assert.AreEqual(1, stream.Count);
                Assert.IsNull(stream[0].Get("varA1"));
            }

            _serviceProvider.CreateVariable("varA2", typeof(int), () => 10);
            using (var stream = _serviceProvider.From<EventBean>(typeof(SupportBean))
                .AddProperty("varA1")
                .AddProperty("varA2")
                .Select())
            {
                _serviceProvider.EPRuntime.SendEvent(new SupportBean());
                Assert.AreEqual(1, stream.Count);
                Assert.IsNull(stream[0].Get("varA1"));
                Assert.AreEqual(10, stream[0].Get("varA2"));
            }

            _serviceProvider.CreateVariable("varB1", "int");
            using (var stream = _serviceProvider.From<EventBean>(typeof(SupportBean))
                .AddProperty("varA1")
                .AddProperty("varA2")
                .AddProperty("varB1")
                .Select())
            {
                _serviceProvider.EPRuntime.SendEvent(new SupportBean());
                Assert.AreEqual(1, stream.Count);
                Assert.IsNull(stream[0].Get("varA1"));
                Assert.AreEqual(10, stream[0].Get("varA2"));
                Assert.IsNull(stream[0].Get("varB1"));
            }

            _serviceProvider.CreateVariable("varB2", "int", () => 20);
            using (var stream = _serviceProvider.From<EventBean>(typeof(SupportBean))
                .AddProperty("varA1")
                .AddProperty("varA2")
                .AddProperty("varB1")
                .AddProperty("varB2")
                .Select())
            {
                _serviceProvider.EPRuntime.SendEvent(new SupportBean());
                Assert.AreEqual(1, stream.Count);
                Assert.IsNull(stream[0].Get("varA1"));
                Assert.AreEqual(10, stream[0].Get("varA2"));
                Assert.IsNull(stream[0].Get("varB1"));
                Assert.AreEqual(20, stream[0].Get("varB2"));
            }
        }

        [Test]
        public void TestCreateSelectTrigger()
        {
            // Create a window
            _serviceProvider.CreateWindow(
                "testWindow",
                View.Create("keepall"),
                _serviceProvider.From<SupportBean>());

            // Create a select trigger - with no expression criteria
            using (var statement = _serviceProvider.CreateSelectTrigger(
                "testWindow",
                "testWindow",
                _serviceProvider.From<TriggerEvent>()))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound on " + typeof(TriggerEvent).FullName + " select * from testWindow as testWindow");
            }

            // Create a select trigger - with single expression criteria
            using (var statement = _serviceProvider.CreateSelectTrigger(
                "testWindow",
                "testWindow",
                _serviceProvider.From<TriggerEvent>(),
                testWindow => testWindow.IntPrimitive == 10))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound on " + typeof(TriggerEvent).FullName +
                    " select * from testWindow as testWindow where testWindow.IntPrimitive=10");
            }

            // Create a select trigger - with single expression criteria
            using (var statement = _serviceProvider.CreateSelectTrigger<TriggerEvent, SupportBean>(
                "testWindow",
                "testWindow",
                _serviceProvider.FromTypeAs<TriggerEvent>("triggerEvent"),
                (triggerEvent, testWindow) => triggerEvent.IntPrimitive == testWindow.IntPrimitive))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound on " + typeof(TriggerEvent).FullName +
                    " as triggerEvent select * from testWindow as testWindow where triggerEvent.IntPrimitive=testWindow.IntPrimitive");
            }
        }

        [Test]
        public void TestCreateDeleteTrigger()
        {
            // Create a window
            _serviceProvider.CreateWindow(
                "testWindow",
                View.Create("keepall"),
                _serviceProvider.From<SupportBean>());

            // Create a delete trigger - with no expression criteria
            using (var statement = _serviceProvider.CreateDeleteTrigger(
                "testWindow",
                "testWindow",
                _serviceProvider.From<TriggerEvent>()))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound on " + typeof(TriggerEvent).FullName + " delete from testWindow as testWindow");
            }

            // Create a delete trigger - with single expression criteria
            using (var statement = _serviceProvider.CreateDeleteTrigger(
                "testWindow",
                "testWindow",
                _serviceProvider.From<TriggerEvent>(),
                testWindow => testWindow.IntPrimitive == 10))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound on " + typeof(TriggerEvent).FullName +
                    " delete from testWindow as testWindow where testWindow.IntPrimitive=10");
            }

            // Create a delete trigger - with single expression criteria
            using (var statement = _serviceProvider.CreateDeleteTrigger<TriggerEvent, SupportBean>(
                "testWindow",
                "testWindow",
                _serviceProvider.FromTypeAs<TriggerEvent>("triggerEvent"),
                (triggerEvent, testWindow) => triggerEvent.IntPrimitive == testWindow.IntPrimitive))
            {
                Assert.AreEqual(
                    statement.Text,
                    "@IterableUnbound on " + typeof(TriggerEvent).FullName +
                    " as triggerEvent delete from testWindow as testWindow where triggerEvent.IntPrimitive=testWindow.IntPrimitive");
            }
        }

        internal class TriggerEvent
        {
            public int IntPrimitive { get; set; }
        }
    }
}
