///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowExampleRollingTopWords : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            if (env.IsHA) {
                return;
            }

            var epl = "@Name('flow') create dataflow RollingTopWords\n" +
                      "create objectarray schema WordEvent (word string),\n" +
                      "Emitter -> wordstream:<WordEvent> {name:'a'} // Produces word stream\n" +
                      "Select(wordstream) -> wordcount { // Sliding time window count per word\n" +
                      "  select: (select word, count(*) as wordcount from wordstream#time(30) group by word)\n" +
                      "}\n" +
                      "Select(wordcount) -> wordranks { // Rank of words\n" +
                      "  select: (select window(*) as rankedWords from wordcount#sort(3, wordcount desc) output snapshot every 2 seconds)\n" +
                      "}\n" +
                      "DefaultSupportCaptureOp(wordranks) {}";
            env.EventService.AdvanceTime(0);
            env.CompileDeploy(epl);

            // prepare test
            var capture = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions();
            options.WithOperatorProvider(new DefaultSupportGraphOpProvider(capture));

            var instanceOne = env.Runtime.DataFlowService.Instantiate(
                env.DeploymentId("flow"),
                "RollingTopWords",
                options);
            var emitter = instanceOne.StartCaptive().Emitters.Get("a");

            foreach (var word in new[] {"this", "is", "a", "test", "that", "is", "a", "word", "test"}) {
                emitter.Submit(new object[] {word});
            }

            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);

            env.AdvanceTime(2000);
            Assert.AreEqual(1, capture.Current.Length);
            var row = capture.Current[0].UnwrapIntoArray<object>();
            var rows = row[0].UnwrapIntoArray<object>();
            EPAssertionUtil.AssertPropsPerRow(
                env.Container,
                rows,
                new[] {"word", "count"},
                new[] {
                    new object[] {"is", 2L},
                    new object[] {"a", 2L},
                    new object[] {"test", 2L}
                });

            instanceOne.Cancel();

            env.UndeployAll();
        }

        public class MyWordTestSource : DataFlowSourceOperator
        {
            private int count;

            [DataFlowContext] private EPDataFlowEmitter graphContext;

            public void Next()
            {
                Thread.Sleep(100);
                string[] words = {"this", "is", "a", "test"};
                var rand = new Random();
                var word = words[rand.Next(words.Length)];
                graphContext.Submit(new object[] {word});
            }

            public void Open(DataFlowOpOpenContext openContext)
            {
            }

            public void Close(DataFlowOpCloseContext openContext)
            {
            }
        }
    }
} // end of namespace