///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIStartCaptive : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp).Name);
    
            string[] fields = "p0,p1".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("MyOAEventType", fields, new object[]{typeof(string), typeof(int)});
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow " +
                    "Emitter -> outstream<MyOAEventType> {name:'src1'}" +
                    "DefaultSupportCaptureOp(outstream) {}");

            var container = epService.Container;
            var captureOp = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
    
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
            EPDataFlowInstanceCaptive captiveStart = instance.StartCaptive();
            Assert.AreEqual(0, captiveStart.Runnables.Count);
            Assert.AreEqual(1, captiveStart.Emitters.Count);
            Emitter emitter = captiveStart.Emitters.Get("src1");
            Assert.AreEqual(EPDataFlowState.RUNNING, instance.State);
    
            emitter.Submit(new object[]{"E1", 10});
            EPAssertionUtil.AssertPropsPerRow(container, captureOp.Current, fields, new object[][]{new object[] {"E1", 10}});
    
            emitter.Submit(new object[]{"E2", 20});
            EPAssertionUtil.AssertPropsPerRow(container, captureOp.Current, fields, new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}});
    
            emitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            EPAssertionUtil.AssertPropsPerRow(container, captureOp.Current, fields, new Object[0][]);
            EPAssertionUtil.AssertPropsPerRow(container, captureOp.GetAndReset()[0].ToArray(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}});
    
            emitter.Submit(new object[]{"E3", 30});
            EPAssertionUtil.AssertPropsPerRow(container, captureOp.Current, fields, new object[][]{new object[] {"E3", 30}});
    
            // stays running until cancelled (no transition to complete)
            Assert.AreEqual(EPDataFlowState.RUNNING, instance.State);
    
            instance.Cancel();
            Assert.AreEqual(EPDataFlowState.CANCELLED, instance.State);
    
            // test doc sample
            string epl = "create dataflow HelloWorldDataFlow\n" +
                    "  create schema SampleSchema(text string),\t// sample type\t\t\n" +
                    "\t\n" +
                    "  Emitter -> helloworld.stream<SampleSchema> { name: 'myemitter' }\n" +
                    "  LogSink(helloworld.stream) {}";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.DataFlowRuntime.Instantiate("HelloWorldDataFlow");
        }
    }
} // end of namespace
