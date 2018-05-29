///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.linq
{
    [TestFixture]
    public class TestLinqWinViews
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
        public void TestViewWithKeepAll()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().KeepAll(),
                "select * from " + Name.Clean<SupportBean>() + "#keepall()");
        }

        [Test]
        public void TestViewWithLengthWindow()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithLength(100),
                "select * from " + Name.Clean<SupportBean>() + "#length(100)");
        }

        [Test]
        public void TestViewWithLengthBatchWindow()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithLength(100, true),
                "select * from " + Name.Clean<SupportBean>() + "#length_batch(100)");
        }

        [Test]
        public void TestViewWithTimeWindow()
        {
            // attempt with seconds
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithDuration(80),
                "select * from " + Name.Clean<SupportBean>() + "#time(1 minutes 20 seconds)");

            // attempt with a timespan
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithDuration(TimeSpan.FromSeconds(80)),
                "select * from " + Name.Clean<SupportBean>() + "#time(1 minutes 20 seconds)");
        }

        [Test]
        public void TestViewWithTimeBatchWindow()
        {
            // attempt with seconds
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithDuration(80, true),
                "select * from " + Name.Clean<SupportBean>() + "#time_batch(1 minutes 20 seconds)");

            // attempt with a timespan
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithDuration(TimeSpan.FromSeconds(80), true),
                "select * from " + Name.Clean<SupportBean>() + "#time_batch(1 minutes 20 seconds)");
        }

        [Test]
        public void TestViewWithTimeAccumulation()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().WithAccumlation(TimeSpan.FromSeconds(80)),
                "select * from " + Name.Clean<SupportBean>() + "#time_accum(1 minutes 20 seconds)");
        }

        [Test]
        public void TestViewWithFirstLength()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().KeepFirst(1000),
                "select * from " + Name.Clean<SupportBean>() + "#firstlength(1000)");
        }

        [Test]
        public void TestViewWithFirstDuration()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().KeepFirst(TimeSpan.FromSeconds(80)),
                "select * from " + Name.Clean<SupportBean>() + "#firsttime(1 minutes 20 seconds)");
        }

        [Test]
        public void TestViewWithExpression()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().KeepWhile(e => e.IntPrimitive > 100),
                "select * from " + Name.Clean<SupportBean>() + "#expr(e.IntPrimitive>100)");
        }

        [Test]
        public void TestViewUntilExpression()
        {
            AssertModelEquality(
                _serviceProvider.From<SupportBean>().KeepUntil(e => e.IntPrimitive > 100),
                "select * from " + Name.Clean<SupportBean>() + "#expr_batch(e.IntPrimitive>100)");
        }

        private void AssertModelEquality(EsperQuery<SupportBean> stream, string sample)
        {
            sample = "@IterableUnbound " + sample;

            var sampleModel = _serviceProvider.EPAdministrator.CompileEPL(sample);
            var sampleModelEPL = sampleModel.ToEPL();

            var streamEPL = stream.ObjectModel.ToEPL();

            Assert.That(streamEPL, Is.EqualTo(sampleModelEPL));
        }
    }
}
