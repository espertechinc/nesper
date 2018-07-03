///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowExampleRollingTopWords : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(GetType().FullName);
    
            var epl = "create dataflow RollingTopWords\n" +
                    "create objectarray schema WordEvent (word string),\n" +
                    "Emitter -> wordstream<WordEvent> {name:'a'} // Produces word stream\n" +
                    "Select(wordstream) -> wordcount { // Sliding time window count per word\n" +
                    "  select: (select word, count(*) as wordcount from wordstream#time(30) group by word)\n" +
                    "}\n" +
                    "Select(wordcount) -> wordranks { // Rank of words\n" +
                    "  select: (select window(*) as rankedWords from wordcount#sort(3, wordcount desc) output snapshot every 2 seconds)\n" +
                    "}\n" +
                    "DefaultSupportCaptureOp(wordranks) {}";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL(epl);
    
            // prepare test
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(capture));
    
            var instanceOne = epService.EPRuntime.DataFlowRuntime.Instantiate("RollingTopWords", options);
            var emitter = instanceOne.StartCaptive().Emitters.Get("a");
    
            foreach (var word in new string[]{"this", "is", "a", "test", "that", "is", "a", "word", "test"}) {
                emitter.Submit(new object[]{word});
            }
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.AreEqual(1, capture.Current.Length);
            var map = (IDictionary<string, object>) capture.Current[0];
            var rows = map.Get("rankedWords").UnwrapIntoList<object[]>();
            EPAssertionUtil.AssertPropsPerRow(
                epService.Container,
                rows,
                "word,count".Split(','),
                new object[][] {
                    new object[] {"is", 2L},
                    new object[] {"a", 2L},
                    new object[] {"test", 2L}
                });
    
            instanceOne.Cancel();
        }
    
        /// <summary>
        /// Comment-In for online flow-testing.
        /// <para>
        /// public void TestOnline() {
        /// epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockType.CLOCK_INTERNAL));
        /// </para>
        /// <para>
        /// string epl = "create dataflow RollingTopWords\n" +
        /// "create objectarray schema WordEvent (word string);\n" +
        /// "MyWordTestSource -> wordstream<WordEvent> {} // Produces word stream\n" +
        /// "Select(wordstream) -> wordcount { // Sliding time window count per word\n" +
        /// "  select: select word, count(*) as wordcount from wordstream#time(30) group by word;\n" +
        /// "}\n" +
        /// "Select(wordcount) -> wordranks { // Rank of words\n" +
        /// "  select: select prevwindow(wc) from wordcount#rank(word, 3, wordcount desc) as wc output snapshot every 2 seconds limit 1;\n" +
        /// "}\n" +
        /// "LogSink(wordranks) {format:'json';}";
        /// epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
        /// epService.EPAdministrator.CreateEPL(epl);
        /// </para>
        /// <para>
        /// // prepare test
        /// var capture = new DefaultSupportCaptureOp();
        /// var options = new EPDataFlowInstantiationOptions();
        /// options.OperatorProvider = new DefaultSupportGraphOpProvider(capture);
        /// </para>
        /// <para>
        /// EPDataFlowInstance instanceOne = epService.EPRuntime.DataFlowRuntime.Instantiate("RollingTopWords", options);
        /// instanceOne.Start();
        /// </para>
        /// <para>
        /// Thread.Sleep(100000000);
        /// }
        /// </para>
        /// </summary>
    
        public class MyWordTestSource : DataFlowSourceOperator {
#pragma warning disable CS0649
            [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore CS0649

            public void Next() {
                Thread.Sleep(100);
                var words = new string[]{"this", "is", "a", "test"};
                var rand = new Random();
                var word = words[rand.Next(words.Length)];
                graphContext.Submit(new object[]{word});
            }
    
            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                return null;
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
            }
    
            public void Close(DataFlowOpCloseContext openContext) {
            }
        }
    }
} // end of namespace
