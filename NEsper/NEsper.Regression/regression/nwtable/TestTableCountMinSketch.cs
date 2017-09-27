///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.util;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableCountMinSketch 
    {
        private EPServiceProviderSPI _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S2));
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestDocSamples()
        {
            _epService.EPAdministrator.CreateEPL("create schema WordEvent (word string)");
            _epService.EPAdministrator.CreateEPL("create schema EstimateWordCountEvent (word string)");
    
            _epService.EPAdministrator.CreateEPL("create table WordCountTable(wordcms countMinSketch())");
            _epService.EPAdministrator.CreateEPL("create table WordCountTable2(wordcms countMinSketch({\n" +
                    "  epsOfTotalCount: 0.000002,\n" +
                    "  confidence: 0.999,\n" +
                    "  seed: 38576,\n" +
                    "  topk: 20,\n" +
                    "  agent: '" + typeof(CountMinSketchAgentStringUTF16).MaskTypeName() + "'" +
                    "}))");
            _epService.EPAdministrator.CreateEPL("into table WordCountTable select countMinSketchAdd(word) as wordcms from WordEvent");
            _epService.EPAdministrator.CreateEPL("select WordCountTable.wordcms.countMinSketchFrequency(word) from EstimateWordCountEvent");
            _epService.EPAdministrator.CreateEPL("select WordCountTable.wordcms.countMinSketchTopk() from pattern[every timer:interval(10 sec)]");
        }
    
        [Test]
        public void TestNonStringType()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyByteArrayEventRead));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyByteArrayEventCount));
    
            var eplTable = "create table MyApprox(bytefreq countMinSketch({" +
                    "  epsOfTotalCount: 0.02," +
                    "  confidence: 0.98," +
                    "  topk: null," +
                    "  agent: '" + typeof(MyBytesPassthruAgent).MaskTypeName() + "'" +
                    "}))";
            _epService.EPAdministrator.CreateEPL(eplTable);
    
            var eplInto = "into table MyApprox select countMinSketchAdd(data) as bytefreq from MyByteArrayEventCount";
            _epService.EPAdministrator.CreateEPL(eplInto);
    
            var listener = new SupportUpdateListener();
            var eplRead = "select MyApprox.bytefreq.countMinSketchFrequency(data) as freq from MyByteArrayEventRead";
            var stmtRead = _epService.EPAdministrator.CreateEPL(eplRead);
            stmtRead.AddListener(listener);
    
            _epService.EPRuntime.SendEvent(new MyByteArrayEventCount(new byte[] {1, 2, 3}));
            _epService.EPRuntime.SendEvent(new MyByteArrayEventRead(new byte[] {0, 2, 3}));
            Assert.AreEqual(0L, listener.AssertOneGetNewAndReset().Get("freq"));
    
            _epService.EPRuntime.SendEvent(new MyByteArrayEventRead(new byte[] {1, 2, 3}));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("freq"));
        }
    
        [Test]
        public void TestFrequencyAndTopk() {
            var epl =
                    "create table MyApprox(wordapprox countMinSketch({topk:3}));\n" +
                    "into table MyApprox select countMinSketchAdd(TheString) as wordapprox from SupportBean;\n" +
                    "@Name('frequency') select MyApprox.wordapprox.countMinSketchFrequency(p00) as freq from SupportBean_S0;\n" +
                    "@Name('topk') select MyApprox.wordapprox.countMinSketchTopk() as topk from SupportBean_S1;\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var listenerFreq = new SupportUpdateListener();
            _epService.EPAdministrator.GetStatement("frequency").AddListener(listenerFreq);
            var listenerTopk = new SupportUpdateListener();
            _epService.EPAdministrator.GetStatement("topk").AddListener(listenerTopk);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertOutput(listenerFreq, "E1=1", listenerTopk, "E1=1");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertOutput(listenerFreq, "E1=1,E2=1", listenerTopk, "E1=1,E2=1");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertOutput(listenerFreq, "E1=1,E2=2", listenerTopk, "E1=1,E2=2");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            AssertOutput(listenerFreq, "E1=1,E2=2,E3=1", listenerTopk, "E1=1,E2=2,E3=1");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertOutput(listenerFreq, "E1=1,E2=2,E3=1,E4=1", listenerTopk, "E1=1,E2=2,E3=1");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertOutput(listenerFreq, "E1=1,E2=2,E3=1,E4=2", listenerTopk, "E1=1,E2=2,E4=2");
    
            // test join
            var eplJoin = "select wordapprox.countMinSketchFrequency(s2.p20) as c0 from MyApprox, SupportBean_S2 s2 unidirectional";
            var stmtJoin = _epService.EPAdministrator.CreateEPL(eplJoin);
            stmtJoin.AddListener(listenerFreq);
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "E3"));
            Assert.AreEqual(1L, listenerFreq.AssertOneGetNewAndReset().Get("c0"));
            stmtJoin.Dispose();
    
            // test subquery
            var eplSubquery = "select (select wordapprox.countMinSketchFrequency(s2.p20) from MyApprox) as c0 from SupportBean_S2 s2";
            var stmtSubquery = _epService.EPAdministrator.CreateEPL(eplSubquery);
            stmtSubquery.AddListener(listenerFreq);
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "E3"));
            Assert.AreEqual(1L, listenerFreq.AssertOneGetNewAndReset().Get("c0"));
            stmtSubquery.Dispose();
        }
    
        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType<MyByteArrayEventCount>();
            _epService.EPAdministrator.CreateEPL("create table MyCMS(wordcms countMinSketch())");
    
            // invalid "countMinSketch" declarations
            //
            TryInvalid("select countMinSketch() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketch()': Count-min-sketch aggregation function 'countMinSketch' can only be used in create-table statements [");
            TryInvalid("create table MyTable(cms countMinSketch(5))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch(5)': Count-min-sketch aggregation function 'countMinSketch'  expects either no parameter or a single json parameter object [");
            TryInvalid("create table MyTable(cms countMinSketch({xxx:3}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({xxx=3})': Unrecognized parameter 'xxx' [");
            TryInvalid("create table MyTable(cms countMinSketch({epsOfTotalCount:'a'}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({epsOfTotalCount=a})': Property 'epsOfTotalCount' expects an System.Double but receives a value of type System.String [");
            TryInvalid("create table MyTable(cms countMinSketch({agent:'a'}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({agent=a})': Failed to instantiate agent provider: Could not load class by name 'a', please check imports [");
            TryInvalid("create table MyTable(cms countMinSketch({agent:'System.String'}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({agent=System.String})': Failed to instantiate agent provider: Class 'System.String' does not implement interface 'com.espertech.esper.client.util.CountMinSketchAgent' [");
    
            // invalid "countMinSketchAdd" declarations
            //
            TryInvalid("select countMinSketchAdd(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchAdd(TheString)': Count-min-sketch aggregation function 'countMinSketchAdd' can only be used with into-table");
            TryInvalid("into table MyCMS select countMinSketchAdd() as wordcms from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchAdd()': Count-min-sketch aggregation function 'countMinSketchAdd' requires a single parameter expression");
            TryInvalid("into table MyCMS select countMinSketchAdd(data) as wordcms from MyByteArrayEventCount",
                    "Error starting statement: Incompatible aggregation function for table 'MyCMS' column 'wordcms', expecting 'countMinSketch()' and received 'countMinSketchAdd(data)': Mismatching parameter return type, expected any of [System.String] but received System.Byte(Array) [");
            TryInvalid("into table MyCMS select countMinSketchAdd(distinct 'abc') as wordcms from MyByteArrayEventCount",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchAdd(distinct \"abc\")': Count-min-sketch aggregation function 'countMinSketchAdd' is not supported with distinct [");
    
            // invalid "countMinSketchFrequency" declarations
            //
            TryInvalid("into table MyCMS select countMinSketchFrequency(TheString) as wordcms from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchFrequency(TheString)': Count-min-sketch aggregation function 'countMinSketchFrequency' requires the use of a table-access expression [");
            TryInvalid("select countMinSketchFrequency() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchFrequency()': Count-min-sketch aggregation function 'countMinSketchFrequency' requires a single parameter expression");
    
            // invalid "countMinSketchTopk" declarations
            //
            TryInvalid("select countMinSketchTopk() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchTopk()': Count-min-sketch aggregation function 'countMinSketchTopk' requires the use of a table-access expression");
            TryInvalid("select MyCMS.wordcms.countMinSketchTopk(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'MyCMS.wordcms.countMinSketchTopk(Th...(43 chars)': Count-min-sketch aggregation function 'countMinSketchTopk' requires a no parameter expressions [");
        }
    
        private void TryInvalid(string epl, string expected)
        {
            SupportMessageAssertUtil.TryInvalid(_epService, epl, expected);
        }
    
        private void AssertOutput(SupportUpdateListener listenerFrequency, string frequencyList,
                                  SupportUpdateListener listenerTopk, string topkList)
        {
            AssertFrequencies(listenerFrequency, frequencyList);
            AssertTopk(listenerTopk, topkList);
        }
    
        private void AssertFrequencies(SupportUpdateListener listenerFrequency, string frequencyList)
        {
            var pairs = frequencyList.Split(',');
            for (var i = 0; i < pairs.Length; i++) {
                var split = pairs[i].Split('=');
                _epService.EPRuntime.SendEvent(new SupportBean_S0(0, split[0].Trim()));
                var value = listenerFrequency.AssertOneGetNewAndReset().Get("freq");
                Assert.AreEqual(long.Parse(split[1]), value, "failed at index" + i);
            }
        }
    
        private void AssertTopk(SupportUpdateListener listenerTopk, string topkList)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            var @event = listenerTopk.AssertOneGetNewAndReset();
            var arr = (CountMinSketchTopK[]) @event.Get("topk");
    
            var pairs = topkList.Split(',');
            Assert.AreEqual(pairs.Length, arr.Length, "received " + arr);
    
            foreach (var pair in pairs) {
                var pairArr = pair.Split('=');
                long expectedFrequency = long.Parse(pairArr[1]);
                var expectedValue = pairArr[0].Trim();
                var foundIndex = Find(expectedFrequency, expectedValue, arr);
                Assert.IsFalse(foundIndex == -1, "failed to find '" + expectedValue + "=" + expectedFrequency + "' among remaining " + arr);
                arr[foundIndex] = null;
            }
        }
    
        private int Find(long expectedFrequency, string expectedValue, CountMinSketchTopK[] arr)
        {
            for (var i = 0; i < arr.Length; i++) {
                var item = arr[i];
                if (item != null && item.Frequency == expectedFrequency && item.Value.Equals(expectedValue)) {
                    return i;
                }
            }
            return -1;
        }
    
        /// <summary>
        /// An agent that expects byte[] values.
        /// </summary>
        public class MyBytesPassthruAgent : CountMinSketchAgent
        {
            public Type[] AcceptableValueTypes
            {
                get
                {
                    return new Type[]
                    {
                        typeof (byte[])
                    };
                }
            }

            public void Add(CountMinSketchAgentContextAdd ctx)
            {
                if (ctx.Value == null) {
                    return;
                }
                var value = (byte[]) ctx.Value;
                ctx.State.Add(value, 1);
            }
    
            public long? Estimate(CountMinSketchAgentContextEstimate ctx)
            {
                if (ctx.Value == null) {
                    return null;
                }
                var value = (byte[]) ctx.Value;
                return ctx.State.Frequency(value);
            }
    
            public object FromBytes(CountMinSketchAgentContextFromBytes ctx)
            {
                return ctx.Bytes;
            }
        }

        internal abstract class MyByteArrayEvent
        {
            private readonly byte[] _data;

            internal MyByteArrayEvent(byte[] data)
            {
                _data = data;
            }

            public byte[] Data
            {
                get { return _data; }
            }
        }

        internal class MyByteArrayEventRead : MyByteArrayEvent
        {
            internal MyByteArrayEventRead(byte[] data) : base(data)
            {
            }
        }

        internal class MyByteArrayEventCount : MyByteArrayEvent
        {
            internal MyByteArrayEventCount(byte[] data) : base(data)
            {
            }
        }
    }
}
