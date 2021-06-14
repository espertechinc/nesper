///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.dataflow;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowExampleWordCount : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('flow') create dataflow WordCount " +
                      "MyLineFeedSource -> LineOfTextStream {} " +
                      "MyTokenizerCounter(LineOfTextStream) -> SingleLineCountStream {}" +
                      "MyWordCountAggregator(SingleLineCountStream) -> WordCountStream {}" +
                      "DefaultSupportCaptureOp(WordCountStream) {}";
            env.CompileDeploy(epl);

            var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
            var source = new MyLineFeedSource(Arrays.AsList("Test this code", "Test line two").GetEnumerator());

            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future, source));

            env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "WordCount", options).Start();

            var received = new object[0];
            try {
                received = future.GetValue(3, TimeUnit.SECONDS);
            }
            catch (Exception t) {
                throw new EPException(t);
            }

            Assert.AreEqual(1, received.Length);
            var stats = (MyWordCountStats) received[0];
            Assert.AreEqual(2, stats.Lines);
            Assert.AreEqual(6, stats.Words);
            Assert.AreEqual(23, stats.Chars);

            env.UndeployAll();
        }
    }
} // end of namespace