///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.linq
{
    [TestFixture]
    public class TestLinqQueries
    {
        private EPServiceProvider _serviceProvider;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            Configuration configuration = new Configuration(_container);
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportPriceEvent>();
            configuration.AddEventType<SupportTradeEvent>();

            _serviceProvider = EPServiceProviderManager.GetDefaultProvider(configuration);
            _serviceProvider.Initialize();
        }

        [Test]
        public void TestSimpleQuery()
        {
            var testStream = _serviceProvider.From<SupportBean>();

            using (var testQuery = from atom in testStream select atom.IntPrimitive)
            {
                var eventInvocations = 0;
                testQuery.CollectionChanged += ((sender, args) => eventInvocations++);

                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 1));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 2));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(2, eventInvocations);
            }

            using (var testQuery = from atom in testStream
                                   where atom.IntPrimitive > 20 && atom.IntPrimitive < 30
                                   select atom.IntPrimitive)
            {
                var eventInvocations = 0;
                testQuery.CollectionChanged += ((sender, args) => eventInvocations++);

                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 1));
                Assert.AreEqual(0, testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 20));
                Assert.AreEqual(0, testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 25));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 30));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 29));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(2, eventInvocations);
            }
        }

        [Test]
        public void TestOrderByDescending()
        {
            var testStream = _serviceProvider.From<SupportBean>();

            using (var testQuery = from atom in testStream
                                   where atom.IntPrimitive > 20 && atom.IntPrimitive < 30
                                   orderby atom.IntPrimitive descending
                                   select atom.IntPrimitive)
            {
                var eventInvocations = 0;
                testQuery.CollectionChanged += ((sender, args) => eventInvocations++);

                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 1));
                Assert.AreEqual(0, testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 20));
                Assert.AreEqual(0, testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 25));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 30));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 29));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(2, eventInvocations);
            }
        }

        [Test]
        public void TestOrderByAscending()
        {
            var testStream = _serviceProvider.From<SupportBean>();

            using (var testQuery = from atom in testStream
                                   where atom.IntPrimitive > 20 && atom.IntPrimitive < 30
                                   orderby atom.IntPrimitive ascending
                                   select atom.IntPrimitive)
            {
                var eventInvocations = 0;
                testQuery.CollectionChanged += ((sender, args) => eventInvocations++);

                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 1));
                Assert.AreEqual(0, testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 20));
                Assert.AreEqual(0, testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 25));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 30));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 29));
                Assert.AreEqual(1, testQuery.Count);
                Assert.AreEqual(2, eventInvocations);
            }
        }


        [Test]
        public void TestGroupBy()
        {
            var testStream = _serviceProvider.From<SupportBean>();

            using (var testQuery = from atom in testStream
                                   where atom.IntPrimitive > 20 && atom.IntPrimitive < 30
                                   group atom by atom.TheString into name
                                   select name)
            {
                var eventInvocations = 0;
                testQuery.CollectionChanged += ((sender, args) => eventInvocations++);

                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 1));
                Assert.AreEqual((int) 0, (int) testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 20));
                Assert.AreEqual((int) 0, (int) testQuery.Count);
                Assert.AreEqual(0, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 25));
                Assert.AreEqual((int) 1, (int) testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 30));
                Assert.AreEqual((int) 1, (int) testQuery.Count);
                Assert.AreEqual(1, eventInvocations);
                _serviceProvider.EPRuntime.SendEvent(new SupportBean("A", 29));
                Assert.AreEqual((int) 1, (int) testQuery.Count);
                Assert.AreEqual(2, eventInvocations);
            }
        }

        [Test]
        public void TestOuterJoin()
        {
            var sample =
                "@IterableUnbound select irstream marketData.Amount, priceEvent.Sym from " + Name.Clean<SupportTradeEvent>() + " as marketData " +
                "left outer join " + Name.Clean<SupportPriceEvent>() + " as priceEvent on marketData.CCYPair = priceEvent.Sym";
            var sampleModel = _serviceProvider.EPAdministrator.CompileEPL(sample);
            var sampleModelEPL = sampleModel.ToEPL();

            var testStreamA = _serviceProvider.From<SupportPriceEvent>();
            var testStreamB = _serviceProvider.From<SupportTradeEvent>();
            var query = from marketData in testStreamB
                        join priceEvent in testStreamA on marketData.CCYPair equals priceEvent.Sym
                        select new { marketData.Amount, Symbol = priceEvent.Sym };

            var queryEPL = query.ObjectModel.ToEPL();

            Assert.That((object) queryEPL, Is.EqualTo(sampleModelEPL));
        }
    }
}
