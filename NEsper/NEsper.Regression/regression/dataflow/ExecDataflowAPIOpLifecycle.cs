///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIOpLifecycle : RegressionExecution
    {
    public override void Run(EPServiceProvider epService) {
            RunAssertionTypeEvent(epService);
            RunAssertionFlowGraphSource(epService);
            RunAssertionFlowGraphOperator(epService);
        }
    
        private void RunAssertionTypeEvent(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MyCaptureOutputPortOp));
            epService.EPAdministrator.CreateEPL("create schema MySchema(key string, value int)");
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne MyCaptureOutputPortOp -> outstream<EventBean<MySchema>> {}");
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne");
            Assert.AreEqual("MySchema", MyCaptureOutputPortOp.Port.OptionalDeclaredType.EventType.Name);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFlowGraphSource(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportGraphSource));
            SupportGraphSource.GetAndResetLifecycle();
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow @Name('Goodie') @Audit SupportGraphSource -> outstream<SupportBean> {propOne:'abc'}");
            Assert.AreEqual(0, SupportGraphSource.GetAndResetLifecycle().Count);
    
            // instantiate
            var options = new EPDataFlowInstantiationOptions().DataFlowInstanceId("id1").DataFlowInstanceUserObject("myobject");
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
    
            List<object> events = SupportGraphSource.GetAndResetLifecycle();
            Assert.AreEqual(3, events.Count);
            Assert.AreEqual("instantiated", events[0]);    // instantiated
            Assert.AreEqual("setPropOne=abc", events[1]);  // injected properties
    
            Assert.IsTrue(events[2] is DataFlowOpInitializateContext); // called initialize
            DataFlowOpInitializateContext initContext = (DataFlowOpInitializateContext) events[2];
            Assert.IsNotNull(initContext.AgentInstanceContext);
            Assert.IsNotNull(initContext.RuntimeEventSender);
            Assert.IsNotNull(initContext.ServicesContext);
            Assert.IsNotNull(initContext.StatementContext);
            Assert.AreEqual("id1", initContext.DataflowInstanceId);
            Assert.AreEqual("myobject", initContext.DataflowInstanceUserObject);
            Assert.AreEqual(0, initContext.InputPorts.Count);
            Assert.AreEqual(1, initContext.OutputPorts.Count);
            Assert.AreEqual("outstream", initContext.OutputPorts[0].StreamName);
            Assert.AreEqual("SupportBean", initContext.OutputPorts[0].OptionalDeclaredType.EventType.Name);
            Assert.AreEqual(2, initContext.OperatorAnnotations.Length);
            Assert.AreEqual("Goodie", ((NameAttribute) initContext.OperatorAnnotations[0]).Value);
            Assert.IsNotNull((AuditAttribute) initContext.OperatorAnnotations[1]);
    
            // run
            df.Run();
    
            events = SupportGraphSource.GetAndResetLifecycle();
            Assert.AreEqual(5, events.Count);
            Assert.IsTrue(events[0] is DataFlowOpOpenContext); // called open (GraphSource only)
            Assert.AreEqual("Next(numrows=0)", events[1]);
            Assert.AreEqual("Next(numrows=1)", events[2]);
            Assert.AreEqual("Next(numrows=2)", events[3]);
            Assert.IsTrue(events[4] is DataFlowOpCloseContext); // called close (GraphSource only)
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFlowGraphOperator(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddImport(typeof(MyLineFeedSource));
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportOperator));
            SupportGraphSource.GetAndResetLifecycle();
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow MyLineFeedSource -> outstream {} SupportOperator(outstream) {propOne:'abc'}");
            Assert.AreEqual(0, SupportOperator.GetAndResetLifecycle().Count);
    
            // instantiate
            var src = new MyLineFeedSource(Collections.List("abc", "def").GetEnumerator());
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src));
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
    
            IList<object> events = SupportOperator.GetAndResetLifecycle();
            Assert.AreEqual(3, events.Count);
            Assert.AreEqual("instantiated", events[0]);    // instantiated
            Assert.AreEqual("setPropOne=abc", events[1]);  // injected properties
    
            Assert.IsTrue(events[2] is DataFlowOpInitializateContext); // called initialize
            DataFlowOpInitializateContext initContext = (DataFlowOpInitializateContext) events[2];
            Assert.IsNotNull(initContext.AgentInstanceContext);
            Assert.IsNotNull(initContext.RuntimeEventSender);
            Assert.IsNotNull(initContext.ServicesContext);
            Assert.IsNotNull(initContext.StatementContext);
            Assert.IsNull(initContext.DataflowInstanceId);
            Assert.IsNull(initContext.DataflowInstanceUserObject);
            Assert.AreEqual(1, initContext.InputPorts.Count);
            Assert.AreEqual("[line]", CompatExtensions.Render(initContext.InputPorts[0].TypeDesc.EventType.PropertyNames));
            Assert.AreEqual("[outstream]", CompatExtensions.Render(initContext.InputPorts[0].StreamNames.ToArray()));
            Assert.AreEqual(0, initContext.OutputPorts.Count);
    
            // run
            df.Run();
    
            events = SupportOperator.GetAndResetLifecycle();
            Assert.AreEqual(4, events.Count);
            Assert.IsTrue(events[0] is DataFlowOpOpenContext); // called open (GraphSource only)
            Assert.AreEqual("abc", ((object[]) events[1])[0]);
            Assert.AreEqual("def", ((object[]) events[2])[0]);
            Assert.IsTrue(events[3] is DataFlowOpCloseContext); // called close (GraphSource only)
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class SupportGraphSource : DataFlowSourceOperator {
    
            private string propOne;
    
            private int numrows;
    
            [DataFlowContext]
            private EPDataFlowEmitter graphContext;
    
            private static List<object> lifecycle = new List<object>();
    
            public SupportGraphSource() {
                lifecycle.Add("instantiated");
            }
    
            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                lifecycle.Add(context);
                return null;
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
                lifecycle.Add(openContext);
            }
    
            public void Close(DataFlowOpCloseContext closeContext) {
                lifecycle.Add(closeContext);
            }
    
            public void Next() {
                lifecycle.Add("Next(numrows=" + numrows + ")");
                if (numrows < 2) {
                    numrows++;
                    graphContext.Submit("E" + numrows);
                } else {
                    graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                }
            }
    
            public static List<object> GetAndResetLifecycle() {
                var copy = new List<object>(lifecycle);
                lifecycle = new List<object>();
                return copy;
            }

            public string PropOne {
                get { return propOne; }
                set {
                    lifecycle.Add("setPropOne=" + value);
                    this.propOne = value;
                }
            }

            public EPDataFlowEmitter GraphContext {
                get => graphContext;
                set {
                    lifecycle.Add(value);
                    this.graphContext = value;
                }
            }
        }
    
        [DataFlowOperator]
        public class SupportOperator : DataFlowOpLifecycle
        {
            private string propOne;
    
            [DataFlowContext]
            private EPDataFlowEmitter graphContext;
    
            private static List<object> lifecycle = new List<object>();
    
            public static IList<object> GetAndResetLifecycle() {
                var copy = new List<object>(lifecycle);
                lifecycle = new List<object>();
                return copy;
            }
    
            public SupportOperator() {
                lifecycle.Add("instantiated");
            }
    
            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                lifecycle.Add(context);
                return null;
            }

            public string PropOne {
                get => propOne;
                set {
                    lifecycle.Add("setPropOne=" + value);
                    this.propOne = value;
                }
            }

            public EPDataFlowEmitter GraphContext {
                get => graphContext;
                set {
                    lifecycle.Add(value);
                    this.graphContext = value;
                }
            }

            public void OnInput(Object abc) {
                lifecycle.Add(abc);
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
                lifecycle.Add(openContext);
            }
    
            public void Close(DataFlowOpCloseContext closeContext) {
                lifecycle.Add(closeContext);
            }
        }
    
        [DataFlowOperator]
        public class MyCaptureOutputPortOp : DataFlowOpLifecycle {
            private static DataFlowOpOutputPort port;
    
            public static DataFlowOpOutputPort Port {
                get => port;
                set => MyCaptureOutputPortOp.port = value;
            }

            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                port = context.OutputPorts[0];
                return null;
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
    
            }
    
            public void Close(DataFlowOpCloseContext openContext) {
    
            }
        }
    }
} // end of namespace
