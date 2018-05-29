///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowExampleVwapFilterSelectJoin : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp).Namespace);
            epService.EPAdministrator.Configuration.AddImport(typeof(MyObjectArrayGraphSource).Namespace);
    
            string epl = "create dataflow VWAPSample\r\n" +
                    "create objectarray schema TradeQuoteType as (type string, ticker string, price double, volume long, askprice double, asksize long),\n" +
                    "MyObjectArrayGraphSource -> TradeQuoteStream<TradeQuoteType> {}\r\n" +
                    "Filter(TradeQuoteStream) -> TradeStream {\r\n" +
                    "filter: type=\"trade\"\r\n" +
                    "}\r\n" +
                    "Filter(TradeQuoteStream) -> QuoteStream {\r\n" +
                    "filter: type=\"quote\"\r\n" +
                    "}\r\n" +
                    "Select(TradeStream) -> VwapTrades {\r\n" +
                    "select: (select ticker, sum(price*volume)/sum(volume) as vwap, min(price) as minprice from TradeStream#groupwin(ticker)#length(4) group by ticker)\r\n" +
                    "}\r\n" +
                    "Select(VwapTrades as T, QuoteStream as Q) -> BargainIndex {\r\n" +
                    "select: " +
                    "(select case when vwap>askprice then asksize*(Math.Exp(vwap-askprice)) else 0.0d end as index " +
                    "from T#unique(ticker) as t, Q#lastevent as q " +
                    "where t.ticker=q.ticker)\r\n" +
                    "}\r\n" +
                    "DefaultSupportCaptureOp(BargainIndex) {}\r\n";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(epl);
    
            RunAssertion(epService);
    
            stmtGraph.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            string text = model.ToEPL(new EPStatementFormatter(true));
            Assert.AreEqual(RemoveNewlines(epl), RemoveNewlines(text));
            epService.EPAdministrator.Create(model);
    
            RunAssertion(epService);
        }
    
        private void RunAssertion(EPServiceProvider epService) {
    
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var source = new MyObjectArrayGraphSource(Collections.List(
                    new object[]{"trade", "GE", 100d, 1000L, null, null}, // vwap = 100, minPrice=100
                    new object[]{"quote", "GE", null, null, 99.5d, 2000L}  //
            ).GetEnumerator());
    
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future, source));
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("VWAPSample", options).Start();
    
            object[] received = future.GetValue(5, TimeUnit.SECONDS);
            Assert.AreEqual(1, received.Length);
            EPAssertionUtil.AssertProps(
                epService.Container, received[0], "index".Split(','), new object[]{2000 * Math.Exp(100 - 99.5)});
        }
    
        private string RemoveNewlines(string text) {
            return text.Replace("\n", "").Replace("\r", "");
        }
    }
} // end of namespace
