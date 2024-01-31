///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.dataflow;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowExampleVwapFilterSelectJoin : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            if (env.IsHA) {
                return;
            }

            var epl = "@name('flow')create dataflow VWAPSample\r\n" +
                      "create objectarray schema TradeQuoteType as (type string, ticker string, Price double, Volume long, askPrice double, asksize long),\n" +
                      "MyObjectArrayGraphSource -> TradeQuoteStream<TradeQuoteType> {}\r\n" +
                      "filter(TradeQuoteStream) -> TradeStream {\r\n" +
                      "filter: type=\"trade\"\r\n" +
                      "}\r\n" +
                      "filter(TradeQuoteStream) -> QuoteStream {\r\n" +
                      "filter: type=\"quote\"\r\n" +
                      "}\r\n" +
                      "select(TradeStream) -> VwapTrades {\r\n" +
                      "select: (select ticker, sum(Price*Volume)/sum(Volume) as vwap, min(Price) as minPrice from TradeStream#groupwin(ticker)#length(4) group by ticker)\r\n" +
                      "}\r\n" +
                      "select(VwapTrades as T, QuoteStream as Q) -> BargainIndex {\r\n" +
                      "select: " +
                      "(select case when vwap>askPrice then asksize*(Math.Exp(vwap-askPrice)) else 0.0d end as index " +
                      "from T#unique(ticker) as t, Q#lastevent as q " +
                      "where t.ticker=q.ticker)\r\n" +
                      "}\r\n" +
                      "DefaultSupportCaptureOp(BargainIndex) {}\r\n";
            env.CompileDeploy(epl);

            RunAssertion(env);

            env.UndeployAll();
            var model = env.EplToModel(epl);
            var text = model.ToEPL(new EPStatementFormatter(true));
            ClassicAssert.AreEqual(RemoveNewlines(epl), RemoveNewlines(text));
            env.CompileDeploy(model);

            RunAssertion(env);
            env.UndeployAll();
        }

        private static void RunAssertion(RegressionEnvironment env)
        {
            var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
            var source = new MyObjectArrayGraphSource(
                Arrays.AsList(
                        new object[] { "trade", "GE", 100d, 1000L, null, null }, // vwap = 100, minPrice=100
                        new object[] { "quote", "GE", null, null, 99.5d, 2000L } //
                    )
                    .GetEnumerator());

            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future, source));

            env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "VWAPSample", options).Start();

            object[] received;
            try {
                received = future.GetValue(5, TimeUnit.SECONDS);
            }
            catch (Exception t) {
                throw new EPException(t);
            }

            ClassicAssert.AreEqual(1, received.Length);

            var receivedArray = received[0].UnwrapIntoArray<object>();
            EPAssertionUtil.AssertProps(
                env.Container,
                receivedArray,
                new string[] { "index" },
                new object[] { 2000 * Math.Exp(100 - 99.5) });

            env.UndeployAll();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.DATAFLOW);
        }

        private string RemoveNewlines(string text)
        {
            return text.Replace("\n", "").Replace("\r", "");
        }
    }
} // end of namespace