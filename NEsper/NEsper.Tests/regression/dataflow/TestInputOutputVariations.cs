///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestInputOutputVariations
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestLargeNumOpsDataFlow()
        {
            String epl = "create dataflow MyGraph \n" +
                    "" +
                    "create objectarray schema SchemaOne (p1 string),\n" +
                    "\n" +
                    "BeaconSource -> InStream<SchemaOne> {p1:'A1', iterations:1}\n" +
                    "Select(InStream) -> out_1 { select: (select p1 from InStream) }\n" +
                    "Select(out_1) -> out_2 { select: (select p1 from out_1) }\n" +
                    "Select(out_2) -> out_3 { select: (select p1 from out_2) }\n" +
                    "Select(out_3) -> out_4 { select: (select p1 from out_3) }\n" +
                    "Select(out_4) -> out_5 { select: (select p1 from out_4) }\n" +
                    "Select(out_5) -> out_6 { select: (select p1 from out_5) }\n" +
                    "Select(out_6) -> out_7 { select: (select p1 from out_6) }\n" +
                    "Select(out_7) -> out_8 { select: (select p1 from out_7) }\n" +
                    "Select(out_8) -> out_9 { select: (select p1 from out_8) }\n" +
                    "Select(out_9) -> out_10 { select: (select p1 from out_9) }\n" +
                    "Select(out_10) -> out_11 { select: (select p1 from out_10) }\n" +
                    "Select(out_11) -> out_12 { select: (select p1 from out_11) }\n" +
                    "Select(out_12) -> out_13 { select: (select p1 from out_12) }\n" +
                    "Select(out_13) -> out_14 { select: (select p1 from out_13) }\n" +
                    "Select(out_14) -> out_15 { select: (select p1 from out_14) }\n" +
                    "Select(out_15) -> out_16 { select: (select p1 from out_15) }\n" +
                    "Select(out_16) -> out_17 { select: (select p1 from out_16) }\n" +
                    "\n" +
                    "SupportOpCountFutureOneA(out_17) {}\n";
            _epService.EPAdministrator.CreateEPL(epl);

            DefaultSupportCaptureOp futureOneA = new DefaultSupportCaptureOp(1);
            Dictionary<String, Object> operators = new Dictionary<String, Object>();
            operators.Put("SupportOpCountFutureOneA", futureOneA);

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));

            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options).Start();

            Object[] result = futureOneA.GetValue(TimeSpan.FromSeconds(3));
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[][]{ new Object[] { "A1" }}, result);
        }

        [Test]
        public void TestFanInOut()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyCustomOp));

            String epl = "create dataflow MultiInMultiOutGraph \n" +
                    "" +
                    "create objectarray schema SchemaOne (p1 string),\n" +
                    "create objectarray schema SchemaTwo (p2 int),\n" +
                    "\n" +
                    "BeaconSource -> InOne<SchemaOne> {p1:'A1', iterations:1}\n" +
                    "BeaconSource -> InTwo<SchemaOne> {p1:'A2', iterations:1}\n" +
                    "\n" +
                    "BeaconSource -> InThree<SchemaTwo> {p2:10, iterations:1}\n" +
                    "BeaconSource -> InFour<SchemaTwo> {p2:20, iterations:1}\n" +
                    "MyCustomOp((InOne, InTwo) as S0, (InThree, InFour) as S1) -> OutOne<SchemaTwo>, OutTwo<SchemaOne>{}\n" +
                    "\n" +
                    "SupportOpCountFutureOneA(OutOne) {}\n" +
                    "SupportOpCountFutureOneB(OutOne) {}\n" +
                    "SupportOpCountFutureTwoA(OutTwo) {}\n" +
                    "SupportOpCountFutureTwoB(OutTwo) {}\n";
            _epService.EPAdministrator.CreateEPL(epl);

            DefaultSupportCaptureOp futureOneA = new DefaultSupportCaptureOp(2);
            DefaultSupportCaptureOp futureOneB = new DefaultSupportCaptureOp(2);
            DefaultSupportCaptureOp futureTwoA = new DefaultSupportCaptureOp(2);
            DefaultSupportCaptureOp futureTwoB = new DefaultSupportCaptureOp(2);

            IDictionary<String, Object> operators = new Dictionary<String, Object>();
            operators["SupportOpCountFutureOneA"] = futureOneA;
            operators["SupportOpCountFutureOneB"] = futureOneB;
            operators["SupportOpCountFutureTwoA"] = futureTwoA;
            operators["SupportOpCountFutureTwoB"] = futureTwoB;

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));

            _epService.EPRuntime.DataFlowRuntime.Instantiate("MultiInMultiOutGraph", options).Start();

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[][] { new Object[] { "S1-10" }, new Object[] { "S1-20" } }, futureOneA.GetValue(TimeSpan.FromSeconds(3)));
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[][] { new Object[] { "S1-10" }, new Object[] { "S1-20" } }, futureOneB.GetValue(TimeSpan.FromSeconds(3)));
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[][] { new Object[] { "S0-A1" }, new Object[] { "S0-A2" } }, futureTwoA.GetValue(TimeSpan.FromSeconds(3)));
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[][] { new Object[] { "S0-A1" }, new Object[] { "S0-A2" } }, futureTwoB.GetValue(TimeSpan.FromSeconds(3)));
        }

        [Test]
        public void TestFactorial()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof (MyFactorialOp).FullName);
            _epService.EPAdministrator.Configuration.AddImport(typeof (DefaultSupportCaptureOp));

            String epl = "create dataflow FactorialGraph \n" +
                    "" +
                    "create objectarray schema InputSchema (number int),\n" +
                    "create objectarray schema TempSchema (current int, temp long),\n" +
                    "create objectarray schema FinalSchema (result long),\n" +
                    "\n" +
                    "BeaconSource -> InputData<InputSchema> {number:5, iterations:1}\n" +
                    "\n" +
                    "MyFactorialOp(InputData as Input, TempResult as Temp) -> TempResult<TempSchema>, FinalResult<FinalSchema>{}\n" +
                    "\n" +
                    "DefaultSupportCaptureOp(FinalResult) {}\n";
            _epService.EPAdministrator.CreateEPL(epl);

            var future = new DefaultSupportCaptureOp(1);
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));

            _epService.EPRuntime.DataFlowRuntime.Instantiate("FactorialGraph", options).Start();

            Object[] result = future.GetValue(TimeSpan.FromSeconds(3));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual((long)5 * 4 * 3 * 2, ((Object[])result[0])[0]);
        }

        [DataFlowOperator]
        public class MyFactorialOp
        {

            [DataFlowContext]
            private EPDataFlowEmitter graphContext;

            public void OnInput(int number)
            {
                graphContext.SubmitPort(0, new Object[] { number, (long)number });
            }

            public void OnTemp(int current, long temp)
            {
                if (current == 1)
                {
                    graphContext.SubmitPort(1, new Object[] { temp });   // we are done
                }
                else
                {
                    current--;
                    long result = temp * current;
                    graphContext.SubmitPort(0, new Object[] { current, result });
                }
            }
        }

        [DataFlowOperator]
        public class MyCustomOp
        {
            [DataFlowContext]
            private EPDataFlowEmitter graphContext;

            public void OnS0(String value)
            {
                String output = "S0-" + value;
                graphContext.SubmitPort(1, new Object[] { output });
            }

            public void OnS1(int value)
            {
                String output = "S1-" + value;
                graphContext.SubmitPort(0, new Object[] { output });
            }
        }
    }
}
