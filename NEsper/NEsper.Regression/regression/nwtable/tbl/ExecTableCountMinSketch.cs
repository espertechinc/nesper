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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableCountMinSketch : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S2));
    
            RunAssertionDocSamples(epService);
            RunAssertionNonStringType(epService);
            RunAssertionFrequencyAndTopk(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionDocSamples(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema WordEvent (word string)");
            epService.EPAdministrator.CreateEPL("create schema EstimateWordCountEvent (word string)");
    
            epService.EPAdministrator.CreateEPL("create table WordCountTable(wordcms countMinSketch())");
            epService.EPAdministrator.CreateEPL("create table WordCountTable2(wordcms countMinSketch({\n" +
                    "  epsOfTotalCount: 0.000002,\n" +
                    "  confidence: 0.999,\n" +
                    "  seed: 38576,\n" +
                    "  topk: 20,\n" +
                    "  agent: '" + typeof(CountMinSketchAgentStringUTF16).FullName + "'" +
                    "}))");
            epService.EPAdministrator.CreateEPL("into table WordCountTable select countMinSketchAdd(word) as wordcms from WordEvent");
            epService.EPAdministrator.CreateEPL("select WordCountTable.wordcms.countMinSketchFrequency(word) from EstimateWordCountEvent");
            epService.EPAdministrator.CreateEPL("select WordCountTable.wordcms.countMinSketchTopk() from pattern[every timer:interval(10 sec)]");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNonStringType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyByteArrayEventRead));
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyByteArrayEventCount));
    
            var eplTable = "create table MyApproxNS(bytefreq countMinSketch({" +
                    "  epsOfTotalCount: 0.02," +
                    "  confidence: 0.98," +
                    "  topk: null," +
                    "  agent: '" + typeof(MyBytesPassthruAgent).FullName + "'" +
                    "}))";
            epService.EPAdministrator.CreateEPL(eplTable);
    
            var eplInto = "into table MyApproxNS select countMinSketchAdd(data) as bytefreq from MyByteArrayEventCount";
            epService.EPAdministrator.CreateEPL(eplInto);
    
            var listener = new SupportUpdateListener();
            var eplRead = "select MyApproxNS.bytefreq.countMinSketchFrequency(data) as freq from MyByteArrayEventRead";
            var stmtRead = epService.EPAdministrator.CreateEPL(eplRead);
            stmtRead.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new MyByteArrayEventCount(new byte[]{1, 2, 3}));
            epService.EPRuntime.SendEvent(new MyByteArrayEventRead(new byte[]{0, 2, 3}));
            Assert.AreEqual(0L, listener.AssertOneGetNewAndReset().Get("freq"));
    
            epService.EPRuntime.SendEvent(new MyByteArrayEventRead(new byte[]{1, 2, 3}));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("freq"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFrequencyAndTopk(EPServiceProvider epService) {
            var epl =
                    "create table MyApproxFT(wordapprox countMinSketch({topk:3}));\n" +
                    "into table MyApproxFT select countMinSketchAdd(TheString) as wordapprox from SupportBean;\n" +
                    "@Name('frequency') select MyApproxFT.wordapprox.countMinSketchFrequency(p00) as freq from SupportBean_S0;\n" +
                    "@Name('topk') select MyApproxFT.wordapprox.countMinSketchTopk() as topk from SupportBean_S1;\n";
            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var listenerFreq = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("frequency").Events += listenerFreq.Update;
            var listenerTopk = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("topk").Events += listenerTopk.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertOutput(epService, listenerFreq, "E1=1", listenerTopk, "E1=1");
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertOutput(epService, listenerFreq, "E1=1,E2=1", listenerTopk, "E1=1,E2=1");
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertOutput(epService, listenerFreq, "E1=1,E2=2", listenerTopk, "E1=1,E2=2");
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            AssertOutput(epService, listenerFreq, "E1=1,E2=2,E3=1", listenerTopk, "E1=1,E2=2,E3=1");
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertOutput(epService, listenerFreq, "E1=1,E2=2,E3=1,E4=1", listenerTopk, "E1=1,E2=2,E3=1");
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertOutput(epService, listenerFreq, "E1=1,E2=2,E3=1,E4=2", listenerTopk, "E1=1,E2=2,E4=2");
    
            // test join
            var eplJoin = "select wordapprox.countMinSketchFrequency(s2.p20) as c0 from MyApproxFT, SupportBean_S2 s2 unidirectional";
            var stmtJoin = epService.EPAdministrator.CreateEPL(eplJoin);
            stmtJoin.Events += listenerFreq.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "E3"));
            Assert.AreEqual(1L, listenerFreq.AssertOneGetNewAndReset().Get("c0"));
            stmtJoin.Dispose();
    
            // test subquery
            var eplSubquery = "select (select wordapprox.countMinSketchFrequency(s2.p20) from MyApproxFT) as c0 from SupportBean_S2 s2";
            var stmtSubquery = epService.EPAdministrator.CreateEPL(eplSubquery);
            stmtSubquery.Events += listenerFreq.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "E3"));
            Assert.AreEqual(1L, listenerFreq.AssertOneGetNewAndReset().Get("c0"));
            stmtSubquery.Dispose();
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deploymentResult.DeploymentId);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyByteArrayEventCount));
            epService.EPAdministrator.CreateEPL("create table MyCMS(wordcms countMinSketch())");
    
            // invalid "countMinSketch" declarations
            //
            TryInvalid(epService, "select countMinSketch() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketch()': Count-min-sketch aggregation function 'countMinSketch' can only be used in create-table statements [");
            TryInvalid(epService, "create table MyTable(cms countMinSketch(5))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch(5)': Count-min-sketch aggregation function 'countMinSketch'  expects either no parameter or a single json parameter object [");
            TryInvalid(epService, "create table MyTable(cms countMinSketch({xxx:3}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({xxx=3})': Unrecognized parameter 'xxx' [");
            TryInvalid(epService, "create table MyTable(cms countMinSketch({epsOfTotalCount:'a'}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({epsOfTotalCount=a})': Property 'epsOfTotalCount' expects an " + typeof(double).GetCleanName() + " but receives a value of type System.String [");
            TryInvalid(epService, "create table MyTable(cms countMinSketch({agent:'a'}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({agent=a})': Failed to instantiate agent provider: Could not load class by name 'a', please check imports [");
            TryInvalid(epService, "create table MyTable(cms countMinSketch({agent:'System.String'}))",
                    "Error starting statement: Failed to validate table-column expression 'countMinSketch({agent=System.String})': Failed to instantiate agent provider: Type 'System.String' does not implement interface 'com.espertech.esper.client.util.CountMinSketchAgent' [");
    
            // invalid "countMinSketchAdd" declarations
            //
            TryInvalid(epService, "select countMinSketchAdd(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchAdd(TheString)': Count-min-sketch aggregation function 'countMinSketchAdd' can only be used with into-table");
            TryInvalid(epService, "into table MyCMS select countMinSketchAdd() as wordcms from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchAdd()': Count-min-sketch aggregation function 'countMinSketchAdd' requires a single parameter expression");
            TryInvalid(epService, "into table MyCMS select countMinSketchAdd(data) as wordcms from MyByteArrayEventCount",
                    "Error starting statement: Incompatible aggregation function for table 'MyCMS' column 'wordcms', expecting 'countMinSketch()' and received 'countMinSketchAdd(data)': Mismatching parameter return type, expected any of [System.String] but received System.Byte[] [");
            TryInvalid(epService, "into table MyCMS select countMinSketchAdd(distinct 'abc') as wordcms from MyByteArrayEventCount",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchAdd(distinct \"abc\")': Count-min-sketch aggregation function 'countMinSketchAdd' is not supported with distinct [");
    
            // invalid "countMinSketchFrequency" declarations
            //
            TryInvalid(epService, "into table MyCMS select countMinSketchFrequency(TheString) as wordcms from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchFrequency(TheString)': Count-min-sketch aggregation function 'countMinSketchFrequency' requires the use of a table-access expression [");
            TryInvalid(epService, "select countMinSketchFrequency() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchFrequency()': Count-min-sketch aggregation function 'countMinSketchFrequency' requires a single parameter expression");
    
            // invalid "countMinSketchTopk" declarations
            //
            TryInvalid(epService, "select countMinSketchTopk() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countMinSketchTopk()': Count-min-sketch aggregation function 'countMinSketchTopk' requires the use of a table-access expression");
            TryInvalid(epService, "select MyCMS.wordcms.countMinSketchTopk(TheString) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'MyCMS.wordcms.countMinSketchTopk(Th...(43 chars)': Count-min-sketch aggregation function 'countMinSketchTopk' requires a no parameter expressions [");
        }
    
        private void AssertOutput(EPServiceProvider epService, SupportUpdateListener listenerFrequency, string frequencyList,
                                  SupportUpdateListener listenerTopk, string topkList) {
            AssertFrequencies(epService, listenerFrequency, frequencyList);
            AssertTopk(epService, listenerTopk, topkList);
        }
    
        private void AssertFrequencies(EPServiceProvider epService, SupportUpdateListener listenerFrequency, string frequencyList) {
            var pairs = frequencyList.Split(',');
            for (var i = 0; i < pairs.Length; i++) {
                var split = pairs[i].Split('=');
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, split[0].Trim()));
                var value = listenerFrequency.AssertOneGetNewAndReset().Get("freq");
                Assert.AreEqual(Int64.Parse(split[1]), value, "failed at index" + i);
            }
        }
    
        private void AssertTopk(EPServiceProvider epService, SupportUpdateListener listenerTopk, string topkList) {
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            var @event = listenerTopk.AssertOneGetNewAndReset();
            var arr = (CountMinSketchTopK[]) @event.Get("topk");
    
            var pairs = topkList.Split(',');
            Assert.AreEqual(pairs.Length, arr.Length, "received " + CompatExtensions.Render(arr));
    
            foreach (var pair in pairs) {
                var pairArr = pair.Split('=');
                var expectedFrequency = Int64.Parse(pairArr[1]);
                var expectedValue = pairArr[0].Trim();
                var foundIndex = Find(expectedFrequency, expectedValue, arr);
                Assert.IsFalse(foundIndex == -1, "failed to find '" + expectedValue + "=" + expectedFrequency + "' among remaining " + CompatExtensions.Render(arr));
                arr[foundIndex] = null;
            }
        }
    
        private int Find(long expectedFrequency, string expectedValue, CountMinSketchTopK[] arr) {
            for (var i = 0; i < arr.Length; i++) {
                var item = arr[i];
                if (item != null && item.Frequency == expectedFrequency && item.Value.Equals(expectedValue)) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>An agent that expects byte[] values.</summary>
        public class MyBytesPassthruAgent : CountMinSketchAgent
        {
            public Type[] AcceptableValueTypes => new Type[] {typeof(byte[])};

            public void Add(CountMinSketchAgentContextAdd ctx)
            {
                if (ctx.Value == null)
                {
                    return;
                }

                var value = (byte[]) ctx.Value;
                ctx.State.Add(value, 1);
            }

            public long? Estimate(CountMinSketchAgentContextEstimate ctx)
            {
                if (ctx.Value == null)
                {
                    return null;
                }

                var value = (byte[]) ctx.Value;
                return ctx.State.Frequency(value);
            }

            public Object FromBytes(CountMinSketchAgentContextFromBytes ctx)
            {
                return ctx.Bytes;
            }
        }

        public abstract class MyByteArrayEvent
        {
            protected MyByteArrayEvent(byte[] data)
            {
                Data = data;
            }

            public byte[] Data { get; }
        }
    
        public class MyByteArrayEventRead : MyByteArrayEvent
        {
            public MyByteArrayEventRead(byte[] data) : base(data) { }
        }
    
        public class MyByteArrayEventCount : MyByteArrayEvent
        {
            public MyByteArrayEventCount(byte[] data) : base(data) { }
        }
    }
} // end of namespace
