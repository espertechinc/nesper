///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;

using NUnit.Framework;

namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestCSVAdapterUseCases
    {
        private static readonly String NEW_LINE = Environment.NewLine;

        internal const String CSV_FILENAME_ONELINE_TRADE = "regression/csvtest_tradedata.csv";

        private const string CSV_FILENAME_ONELINE_TRADE_MULTIPLE = "regression/csvtest_tradedata_multiple.csv";
        private const string CSV_FILENAME_TIMESTAMPED_PRICES = "regression/csvtest_timestamp_prices.csv";
        private const string CSV_FILENAME_TIMESTAMPED_TRADES = "regression/csvtest_timestamp_trades.csv";

        private readonly bool _useBean;

        private IContainer _container;
        private EPRuntime _runtime;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        public TestCSVAdapterUseCases() : this(false)
        {
        }

        public TestCSVAdapterUseCases(bool ub)
        {
            _useBean = ub;
        }

        private Configuration MakeConfig(String typeName)
        {
            return MakeConfig(typeName, false);
        }

        private Configuration MakeConfig(String typeName, bool useBean)
        {
            var configuration = new Configuration(_container);
            if (useBean) {
                configuration.Common.AddEventType(typeName, typeof (ExampleMarketDataBean));
            }
            else {
                IDictionary<String, Object> eventProperties = new Dictionary<String, Object>();
                eventProperties.Put("symbol", typeof (String));
                eventProperties.Put("price", typeof (double));
                eventProperties.Put("volume", typeof (int?));
                configuration.Common.AddEventType(typeName, eventProperties);
            }

            return configuration;
        }

        private void TrySource(AdapterInputSource source)
        {
            var spec = new CSVInputAdapterSpec(source, "TypeC");

            _runtime = EPRuntimeProvider.GetRuntime("testPlayFromInputStream", MakeConfig("TypeC"));
            _runtime.Initialize();
            InputAdapter feed = new CSVInputAdapter(_runtime, spec);

            var stmt = CompileUtil.CompileDeploy(_runtime, "select * from TypeC#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            feed.Start();
            Assert.AreEqual(1, listener.GetNewDataList().Count);
        }

        /// <summary>
        /// Bean with same properties as map type used in this test
        /// </summary>
        public class ExampleMarketDataBean
        {
            [PropertyName("symbol")]
            public string Symbol { get; set; }
            [PropertyName("price")]
            public double Price { get; set; }
            [PropertyName("volume")]
            public int? Volume { get; set; }
        }

        /// <summary>
        /// Play a CSV file using the application thread
        /// </summary>
        [Test]
        public void TestAppThread()
        {
            _runtime = EPRuntimeProvider.GetRuntime("testExistingTypeNoOptions", MakeConfig("TypeA"));
            _runtime.Initialize();

            var stmt = CompileUtil.CompileDeploy(_runtime, "select symbol, price, volume from TypeA#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var spec = new CSVInputAdapterSpec(new AdapterInputSource(_container, CSV_FILENAME_ONELINE_TRADE), "TypeA");
            spec.EventsPerSec = 1000;

            InputAdapter inputAdapter = new CSVInputAdapter(_runtime, spec);
            inputAdapter.Start();

            Assert.AreEqual(1, listener.GetNewDataList().Count);
        }

        [Test]
        public void TestCoordinated()
        {
            IDictionary<String, Object> priceProps = new Dictionary<String, Object>();
            priceProps.Put("timestamp", typeof (long?));
            priceProps.Put("symbol", typeof (String));
            priceProps.Put("price", typeof (double?));

            IDictionary<String, Object> tradeProps = new Dictionary<String, Object>();
            tradeProps.Put("timestamp", typeof (long?));
            tradeProps.Put("symbol", typeof (String));
            tradeProps.Put("notional", typeof (double?));

            var config = new Configuration(_container);
            config.Common.AddEventType("TradeEvent", tradeProps);
            config.Common.AddEventType("PriceEvent", priceProps);

            _runtime = EPRuntimeProvider.GetRuntime("testCoordinated", config);
            _runtime.Initialize();
            _runtime.EventService.ClockExternal();
            _runtime.EventService.AdvanceTime(0);

            var sourcePrices = new AdapterInputSource(_container, CSV_FILENAME_TIMESTAMPED_PRICES);
            var inputPricesSpec = new CSVInputAdapterSpec(sourcePrices, "PriceEvent");
            inputPricesSpec.TimestampColumn = "timestamp";
            inputPricesSpec.PropertyTypes = priceProps;
            var inputPrices = new CSVInputAdapter(inputPricesSpec);

            var sourceTrades = new AdapterInputSource(_container, CSV_FILENAME_TIMESTAMPED_TRADES);
            var inputTradesSpec = new CSVInputAdapterSpec(sourceTrades, "TradeEvent");
            inputTradesSpec.TimestampColumn = "timestamp";
            inputTradesSpec.PropertyTypes = tradeProps;
            var inputTrades = new CSVInputAdapter(inputTradesSpec);

            var stmtPrices = CompileUtil.CompileDeploy(_runtime, "select symbol, price from PriceEvent#length(100)").Statements[0];
            var listenerPrice = new SupportUpdateListener();
            stmtPrices.Events += listenerPrice.Update;
            var stmtTrade = CompileUtil.CompileDeploy(_runtime, "select symbol, notional from TradeEvent#length(100)").Statements[0];
            var listenerTrade = new SupportUpdateListener();
            stmtTrade.Events += listenerTrade.Update;

            AdapterCoordinator coordinator = new AdapterCoordinatorImpl(_runtime, true);
            coordinator.Coordinate(inputPrices);
            coordinator.Coordinate(inputTrades);
            coordinator.Start();

            _runtime.EventService.AdvanceTime(400);
            Assert.IsFalse(listenerTrade.IsInvoked());
            Assert.IsFalse(listenerPrice.IsInvoked());

            // invoke read of events at 500 (see CSV)
            _runtime.EventService.AdvanceTime(1000);
            Assert.AreEqual(1, listenerTrade.GetNewDataList().Count);
            Assert.AreEqual(1, listenerPrice.GetNewDataList().Count);
            listenerTrade.Reset();
            listenerPrice.Reset();

            // invoke read of price events at 1500 (see CSV)
            _runtime.EventService.AdvanceTime(2000);
            Assert.AreEqual(0, listenerTrade.GetNewDataList().Count);
            Assert.AreEqual(1, listenerPrice.GetNewDataList().Count);
            listenerTrade.Reset();
            listenerPrice.Reset();

            // invoke read of trade events at 2500 (see CSV)
            _runtime.EventService.AdvanceTime(3000);
            Assert.AreEqual(1, listenerTrade.GetNewDataList().Count);
            Assert.AreEqual(0, listenerPrice.GetNewDataList().Count);
            listenerTrade.Reset();
            listenerPrice.Reset();
        }

        /// <summary>
        /// Play a CSV file using no existing (dynamic) event type (no timestamp)
        /// </summary>
        [Test]
        public void TestDynamicType()
        {
            var spec = new CSVInputAdapterSpec(
                new AdapterInputSource(_container, CSV_FILENAME_ONELINE_TRADE), "TypeB");

            var config = new Configuration(_container);
            config.Runtime.Threading.IsInternalTimerEnabled = false;
            _runtime = EPRuntimeProvider.GetDefaultRuntime(config);
            _runtime.Initialize();

            InputAdapter feed = new CSVInputAdapter(_runtime, spec);

            var stmt = CompileUtil.CompileDeploy(_runtime, "select symbol, price, volume from TypeB#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            Assert.AreEqual(typeof (String), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof (String), stmt.EventType.GetPropertyType("price"));
            Assert.AreEqual(typeof (String), stmt.EventType.GetPropertyType("volume"));

            feed.Start();
            Assert.AreEqual(1, listener.GetNewDataList().Count);
        }

        /// <summary>
        /// Play a CSV file using an engine thread
        /// </summary>
        [Test]
        public void TestEngineThread1000PerSec()
        {
            _runtime = EPRuntimeProvider.GetRuntime("testExistingTypeNoOptions", MakeConfig("TypeA"));
            _runtime.Initialize();

            var stmt = CompileUtil.CompileDeploy(_runtime, "select symbol, price, volume from TypeA#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var spec = new CSVInputAdapterSpec(
                new AdapterInputSource(_container, CSV_FILENAME_ONELINE_TRADE), "TypeA");
            spec.EventsPerSec = 1000;
            spec.IsUsingEngineThread = true;

            InputAdapter inputAdapter = new CSVInputAdapter(_runtime, spec);
            inputAdapter.Start();
            Thread.Sleep(1000);

            Assert.AreEqual(1, listener.GetNewDataList().Count);
        }

        /// <summary>
        /// Play a CSV file using an engine thread.
        /// </summary>
        [Test]
        public void TestEngineThread1PerSec()
        {
            _runtime = EPRuntimeProvider.GetRuntime("testExistingTypeNoOptions", MakeConfig("TypeA"));
            _runtime.Initialize();

            var stmt = CompileUtil.CompileDeploy(_runtime, "select symbol, price, volume from TypeA#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var spec = new CSVInputAdapterSpec(new AdapterInputSource(_container, CSV_FILENAME_ONELINE_TRADE_MULTIPLE), "TypeA");
            spec.EventsPerSec = 1;
            spec.IsUsingEngineThread = true;

            InputAdapter inputAdapter = new CSVInputAdapter(_runtime, spec);
            inputAdapter.Start();

            Thread.Sleep(1500);
            Assert.AreEqual(1, listener.GetNewDataList().Count);
            listener.Reset();
            Thread.Sleep(300);
            Assert.AreEqual(0, listener.GetNewDataList().Count);

            Thread.Sleep(2000);
            Assert.IsTrue(listener.GetNewDataList().Count >= 2);
        }

        /// <summary>
        /// Play a CSV file using an existing event type definition (no timestamps).  Should
        /// not require a timestamp column, should block thread until played in.
        /// </summary>
        [Test]
        public void TestExistingTypeNoOptions()
        {
            _runtime = EPRuntimeProvider.GetRuntime("testExistingTypeNoOptions", MakeConfig("TypeA", _useBean));
            _runtime.Initialize();

            var stmt = CompileUtil.CompileDeploy(_runtime, "select symbol, price, volume from TypeA#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            (new CSVInputAdapter(_runtime, new AdapterInputSource(_container, CSV_FILENAME_ONELINE_TRADE), "TypeA")).Start();

            Assert.AreEqual(1, listener.GetNewDataList().Count);
        }

        /// <summary>
        /// Play a CSV file that is from memory.
        /// </summary>
        [Test]
        public void TestPlayFromInputStream()
        {
            var myCSV = "symbol, price, volume" + NEW_LINE + "IBM, 10.2, 10000";
            Stream inputStream = new MemoryStream(Encoding.ASCII.GetBytes(myCSV));
            TrySource(new AdapterInputSource(_container, inputStream));
        }

        /// <summary>
        /// Play a CSV file that is from memory.
        /// </summary>
        [Test]
        public void TestPlayFromStringReader()
        {
            var myCSV = "symbol, price, volume" + NEW_LINE + "IBM, 10.2, 10000";
            var reader = new StringReader(myCSV);
            TrySource(new AdapterInputSource(_container, reader));
        }
    }
}