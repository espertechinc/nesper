///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestExampleVwapFilterSelectJoin
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddNamespaceImport<MyObjectArrayGraphSource>();
            _epService.EPAdministrator.Configuration.AddNamespaceImport<DefaultSupportCaptureOp>();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestEPLGraphOnly()
        {
            var epl = "create dataflow VWAPSample\r\n" +
                    "create objectarray schema TradeQuoteType as (type string, ticker string, price double, volume long, askprice double, asksize long),\r\n" +
                    "MyObjectArrayGraphSource -> TradeQuoteStream<TradeQuoteType> {}\r\n" +
                    "filter(TradeQuoteStream) -> TradeStream {\r\n" +
                    "filter: type=\"trade\"\r\n" +
                    "}\r\n" +
                    "filter(TradeQuoteStream) -> QuoteStream {\r\n" +
                    "filter: type=\"quote\"\r\n" +
                    "}\r\n" +
                    "select(TradeStream) -> VwapTrades {\r\n" +
                    "select: (select ticker, sum(price*volume)/sum(volume) as vwap, min(price) as minprice from TradeStream.std:groupwin(ticker).win:length(4) group by ticker)\r\n" +
                    "}\r\n" +
                    "select(VwapTrades as T, QuoteStream as Q) -> BargainIndex {\r\n" +
                    "select: " +
                    "(select case when vwap>askprice then asksize*(Math.Exp(vwap-askprice)) else 0.0d end as index " +
                    "from T.std:unique(ticker) as t, Q.std:lastevent() as q " +
                    "where t.ticker=q.ticker)\r\n" +
                    "}\r\n" +
                    "DefaultSupportCaptureOp(BargainIndex) {}\r\n";
            var stmtGraph = _epService.EPAdministrator.CreateEPL(epl);
    
            RunAssertion();
    
            stmtGraph.Dispose();
            var model = _epService.EPAdministrator.CompileEPL(epl);
            var text = model.ToEPL(new EPStatementFormatter(true));
            Assert.AreEqual(RemoveNewlines(epl), RemoveNewlines(text));
            _epService.EPAdministrator.Create(model);
    
            RunAssertion();
        }
    
        private void RunAssertion()
        {
            var future = new DefaultSupportCaptureOp(1);
            var source = new MyObjectArrayGraphSource(Collections.List(
                    new Object[] {"trade", "GE", 100d, 1000L, null, null}, // vwap = 100, minPrice=100
                    new Object[] {"quote", "GE", null, null, 99.5d, 2000L}  //
                    ).GetEnumerator());
    
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future, source));
    
            _epService.EPRuntime.DataFlowRuntime.Instantiate("VWAPSample", options).Start();

            var received = future.GetValue(TimeSpan.FromSeconds(5));
            Assert.AreEqual(1, received.Length);
            EPAssertionUtil.AssertProps(received[0], "index".Split(','), new Object[] {2000*Math.Exp(100-99.5)});
        }

        private String RemoveNewlines(String text)
        {
            return text.Replace("\n", "").Replace("\r", "");
        }
    }
}
