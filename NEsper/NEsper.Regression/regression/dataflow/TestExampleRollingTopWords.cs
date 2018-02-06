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
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestExampleRollingTopWords  {
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddImport(GetType().FullName);
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestGraph() {
            String epl = "create dataflow RollingTopWords\n" +
                    "create objectarray schema WordEvent (word string),\n" +
                    "Emitter -> wordstream<WordEvent> {name:'a'} // Produces word stream\n" +
                    "Select(wordstream) -> wordcount { // Sliding time window count per word\n" +
                    "  select: (select word, Count(*) as wordcount from wordstream#time(30) group by word)\n" +
                    "}\n" +
                    "Select(wordcount) -> wordranks { // Rank of words\n" +
                    "  select: (select Window(*) as rankedWords from wordcount#sort(3, wordcount desc) output snapshot every 2 seconds)\n" +
                    "}\n" +
                    "DefaultSupportCaptureOp(wordranks) {}";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL(epl);
    
            // prepare test
            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(capture));
    
            EPDataFlowInstance instanceOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("RollingTopWords", options);
            Emitter emitter = instanceOne.StartCaptive().Emitters.Get("a");
    
            foreach (String word in new String[] {"this", "is", "a", "test", "that", "is", "a", "word", "test"}) {
                emitter.Submit(new Object[] {word});
            }
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.AreEqual(1, capture.GetCurrent().Length);
            Map map = (Map) capture.GetCurrent()[0];
            IList<Object[]> rows = (Object[][]) map.Get("rankedWords");
            EPAssertionUtil.AssertPropsPerRow(
                rows, 
                "word,count".Split(','), 
                new Object[][]
                {
                    new Object[] {"is", 2L}, 
                    new Object[] {"a", 2L}, 
                    new Object[] {"test", 2L}
                });
    
            instanceOne.Cancel();
        }
    
        public class MyWordTestSource : DataFlowSourceOperator
        {
            [DataFlowContext]
            private EPDataFlowEmitter graphContext;
    
            private int count;
    
            public void Next() {
                Thread.Sleep(100);
                String[] words = new String[] {"this", "is", "a", "test"};
                Random rand = new Random();
                String word = words[rand.Next(0, words.Length)];
                graphContext.Submit(new Object[]{word});
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
}
