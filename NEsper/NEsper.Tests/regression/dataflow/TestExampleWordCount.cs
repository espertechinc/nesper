///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestExampleWordCount  {
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddNamespaceImport<MyTokenizerCounter>();
            _epService.EPAdministrator.Configuration.AddNamespaceImport<DefaultSupportCaptureOp>();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestEPLGraphOnly() {
    
            String epl = "create dataflow WordCount " +
                    "MyLineFeedSource -> LineOfTextStream {} " +
                    "MyTokenizerCounter(LineOfTextStream) -> SingleLineCountStream {}" +
                    "MyWordCountAggregator(SingleLineCountStream) -> WordCountStream {}" +
                    "DefaultSupportCaptureOp(WordCountStream) {}";
            _epService.EPAdministrator.CreateEPL(epl);
    
            RunAssertion();
        }
    
        private void RunAssertion()
        {
            var future = new DefaultSupportCaptureOp(1);
            var source = new MyLineFeedSource(Collections.List("Test this code", "Test line two").GetEnumerator());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future, source));
    
            _epService.EPRuntime.DataFlowRuntime.Instantiate("WordCount", options).Start();

            var received = future.GetValue(TimeSpan.FromSeconds(3));
            Assert.AreEqual(1, received.Length);
            var stats = (MyWordCountStats) received[0];
            EPAssertionUtil.AssertProps(stats, "Lines,Words,Chars".Split(','), new Object[] {2,6,23});
        }
    }
}
