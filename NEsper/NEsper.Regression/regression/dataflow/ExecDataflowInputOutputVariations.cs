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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowInputOutputVariations : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionLargeNumOpsDataFlow(epService);
            RunAssertionFanInOut(epService);
            RunAssertionFactorial(epService);
        }
    
        private void RunAssertionLargeNumOpsDataFlow(EPServiceProvider epService) {
            string epl = "create dataflow MyGraph \n" +
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
            epService.EPAdministrator.CreateEPL(epl);
    
            var futureOneA = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var operators = new Dictionary<string, Object>();
            operators.Put("SupportOpCountFutureOneA", futureOneA);
    
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options).Start();
    
            object[] result = futureOneA.GetValue(3, TimeUnit.SECONDS);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] {new object[] {"A1"}}, result);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFanInOut(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MyCustomOp));
    
            string epl = "create dataflow MultiInMultiOutGraph \n" +
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
            epService.EPAdministrator.CreateEPL(epl);
    
            var futureOneA = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            var futureOneB = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            var futureTwoA = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            var futureTwoB = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
    
            var operators = new Dictionary<string, Object>();
            operators.Put("SupportOpCountFutureOneA", futureOneA);
            operators.Put("SupportOpCountFutureOneB", futureOneB);
            operators.Put("SupportOpCountFutureTwoA", futureTwoA);
            operators.Put("SupportOpCountFutureTwoB", futureTwoB);
    
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("MultiInMultiOutGraph", options).Start();
    
            EPAssertionUtil.AssertEqualsAnyOrder(new[] {new object[] {"S1-10"}, new object[] {"S1-20"}}, futureOneA.GetValue(3, TimeUnit.SECONDS));
            EPAssertionUtil.AssertEqualsAnyOrder(new[] {new object[] {"S1-10"}, new object[] {"S1-20"}}, futureOneB.GetValue(3, TimeUnit.SECONDS));
            EPAssertionUtil.AssertEqualsAnyOrder(new[] {new object[] {"S0-A1"}, new object[] {"S0-A2"}}, futureTwoA.GetValue(3, TimeUnit.SECONDS));
            EPAssertionUtil.AssertEqualsAnyOrder(new[] {new object[] {"S0-A1"}, new object[] {"S0-A2"}}, futureTwoB.GetValue(3, TimeUnit.SECONDS));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFactorial(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MyFactorialOp));
    
            string epl = "create dataflow FactorialGraph \n" +
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
            epService.EPAdministrator.CreateEPL(epl);
    
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future));
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("FactorialGraph", options).Start();
    
            object[] result = future.GetValue(3, TimeUnit.SECONDS);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual((long) 5 * 4 * 3 * 2, ((object[]) result[0])[0]);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        [DataFlowOperator]
        public class MyFactorialOp {

#pragma warning disable CS0649
            [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore CS0649

            public void OnInput(int number) {
                graphContext.SubmitPort(0, new object[]{number, (long) number});
            }
    
            public void OnTemp(int current, long temp) {
                if (current == 1) {
                    graphContext.SubmitPort(1, new object[]{temp});   // we are done
                } else {
                    current--;
                    long result = temp * current;
                    graphContext.SubmitPort(0, new object[]{current, result});
                }
            }
        }
    
        [DataFlowOperator]
        public class MyCustomOp {

#pragma warning disable CS0649
            [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore CS0649

            public void OnS0(string value) {
                string output = "S0-" + value;
                graphContext.SubmitPort(1, new object[]{output});
            }
    
            public void OnS1(int value) {
                string output = "S1-" + value;
                graphContext.SubmitPort(0, new object[]{output});
            }
        }
    }
} // end of namespace
