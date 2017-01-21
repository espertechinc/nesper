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
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestAPIOpLifecycle
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
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
        public void TestTypeEvent() {
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyCaptureOutputPortOp));
            _epService.EPAdministrator.CreateEPL("create schema MySchema(key string, value int)");
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne MyCaptureOutputPortOp -> outstream<EventBean<MySchema>> {}");
    
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne");
            Assert.AreEqual("MySchema", MyCaptureOutputPortOp.Port.OptionalDeclaredType.EventType.Name);
        }
    
        [Test]
        public void TestFlowGraphSource() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportGraphSource));
            SupportGraphSource.GetAndResetLifecycle();
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow @Name('Goodie') @Audit SupportGraphSource -> outstream<SupportBean> {propOne:'abc'}");
            Assert.AreEqual(0, SupportGraphSource.GetAndResetLifecycle().Count);
    
            // instantiate
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().DataFlowInstanceId("id1").DataFlowInstanceUserObject("myobject");
            EPDataFlowInstance df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
    
            List<Object> events = SupportGraphSource.GetAndResetLifecycle();
            Assert.AreEqual(3, events.Count);
            Assert.AreEqual("instantiated", events[0]);    // instantiated
            Assert.AreEqual("setPropOne=abc", events[1]);  // injected properties
    
            Assert.IsTrue(events[2] is DataFlowOpInitializateContext); // called initialize
            DataFlowOpInitializateContext initContext = (DataFlowOpInitializateContext) events[2];
            Assert.NotNull(initContext.AgentInstanceContext);
            Assert.NotNull(initContext.RuntimeEventSender);
            Assert.NotNull(initContext.ServicesContext);
            Assert.NotNull(initContext.StatementContext);
            Assert.AreEqual("id1", initContext.DataflowInstanceId);
            Assert.AreEqual("myobject", initContext.DataflowInstanceUserObject);
            Assert.AreEqual(0, initContext.InputPorts.Count);
            Assert.AreEqual(1, initContext.OutputPorts.Count);
            Assert.AreEqual("outstream", initContext.OutputPorts[0].StreamName);
            Assert.AreEqual("SupportBean", initContext.OutputPorts[0].OptionalDeclaredType.EventType.Name);
            Assert.AreEqual(2, initContext.OperatorAnnotations.Length);
            Assert.AreEqual("Goodie", ((NameAttribute) initContext.OperatorAnnotations[0]).Value);
            Assert.NotNull((AuditAttribute) initContext.OperatorAnnotations[1]);
    
            // run
            df.Run();
    
            events = SupportGraphSource.GetAndResetLifecycle();
            Assert.AreEqual(5, events.Count);
            Assert.IsTrue(events[0] is DataFlowOpOpenContext); // called open (GraphSource only)
            Assert.AreEqual("next(numrows=0)", events[1]);
            Assert.AreEqual("next(numrows=1)", events[2]);
            Assert.AreEqual("next(numrows=2)", events[3]);
            Assert.IsTrue(events[4] is DataFlowOpCloseContext); // called close (GraphSource only)
        }
    
        [Test]
        public void TestFlowGraphOperator() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyLineFeedSource));
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportOperator));
            SupportGraphSource.GetAndResetLifecycle();
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow MyLineFeedSource -> outstream {} SupportOperator(outstream) {propOne:'abc'}");
            Assert.AreEqual(0, SupportOperator.GetAndResetLifecycle().Count);
    
            // instantiate
            MyLineFeedSource src = new MyLineFeedSource(Collections.List("abc", "def").GetEnumerator());
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(src));
            EPDataFlowInstance df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
    
            List<Object> events = SupportOperator.GetAndResetLifecycle();
            Assert.AreEqual(3, events.Count);
            Assert.AreEqual("instantiated", events[0]);    // instantiated
            Assert.AreEqual("setPropOne=abc", events[1]);  // injected properties
    
            Assert.IsTrue(events[2] is DataFlowOpInitializateContext); // called initialize
            DataFlowOpInitializateContext initContext = (DataFlowOpInitializateContext) events[2];
            Assert.NotNull(initContext.AgentInstanceContext);
            Assert.NotNull(initContext.RuntimeEventSender);
            Assert.NotNull(initContext.ServicesContext);
            Assert.NotNull(initContext.StatementContext);
            Assert.IsNull(initContext.DataflowInstanceId);
            Assert.IsNull(initContext.DataflowInstanceUserObject);
            Assert.AreEqual(1, initContext.InputPorts.Count);
            Assert.AreEqual("[line]", initContext.InputPorts[0].TypeDesc.EventType.PropertyNames.Render());
            Assert.AreEqual("[outstream]", initContext.InputPorts[0].StreamNames.Render());
            Assert.AreEqual(0, initContext.OutputPorts.Count);
    
            // run
            df.Run();
    
            events = SupportOperator.GetAndResetLifecycle();
            Assert.AreEqual(4, events.Count);
            Assert.IsTrue(events[0] is DataFlowOpOpenContext); // called open (GraphSource only)
            Assert.AreEqual("abc", ((Object[]) events[1])[0]);
            Assert.AreEqual("def", ((Object[]) events[2])[0]);
            Assert.IsTrue(events[3] is DataFlowOpCloseContext); // called close (GraphSource only)
        }
    
        public class SupportGraphSource : DataFlowSourceOperator {
    
            private String _propOne;
    
            private int _numrows;
    
            [DataFlowContext]
            private EPDataFlowEmitter _graphContext;
    
            private static List<Object> _lifecycle = new List<Object>();
    
            public SupportGraphSource() {
                _lifecycle.Add("instantiated");
            }
    
            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                _lifecycle.Add(context);
                return null;
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
                _lifecycle.Add(openContext);
            }
    
            public void Close(DataFlowOpCloseContext closeContext) {
                _lifecycle.Add(closeContext);
            }
    
            public void Next() {
                _lifecycle.Add("next(numrows=" + _numrows + ")");
                if (_numrows < 2) {
                    _numrows++;
                    _graphContext.Submit("E" + _numrows);
                }
                else {
                    _graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl() { });
                }
            }
    
            public static List<Object> GetAndResetLifecycle() {
                List<Object> copy = new List<Object>(_lifecycle);
                _lifecycle = new List<Object>();
                return copy;
            }

            public string PropOne
            {
                get { return _propOne; }
                set
                {
                    _lifecycle.Add("setPropOne=" + value);
                    _propOne = value;
                }
            }

            public void SetGraphContext(EPDataFlowEmitter graphContext) {
                _lifecycle.Add(graphContext);
                _graphContext = graphContext;
            }
        }
    
        [DataFlowOperator]
        public class SupportOperator : DataFlowOpLifecycle
        {
            private String _propOne;
    
            [DataFlowContext]
            private EPDataFlowEmitter _graphContext;
    
            private static List<Object> _lifecycle = new List<Object>();
    
            public static List<Object> GetAndResetLifecycle() {
                var copy = new List<Object>(_lifecycle);
                _lifecycle = new List<Object>();
                return copy;
            }
    
            public SupportOperator() {
                _lifecycle.Add("instantiated");
            }
    
            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                _lifecycle.Add(context);
                return null;
            }

            public string PropOne
            {
                get { return _propOne; }
                set
                {
                    _lifecycle.Add("setPropOne=" + value);
                    _propOne = value;
                }
            }

            public void SetGraphContext(EPDataFlowEmitter graphContext) {
                _lifecycle.Add(graphContext);
                _graphContext = graphContext;
            }
    
            public void OnInput(Object abc) {
                _lifecycle.Add(abc);
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
                _lifecycle.Add(openContext);
            }
    
            public void Close(DataFlowOpCloseContext closeContext) {
                _lifecycle.Add(closeContext);
            }
        }

        [DataFlowOperator]
        public class MyCaptureOutputPortOp : DataFlowOpLifecycle
        {
            public static DataFlowOpOutputPort Port { get; set; }

            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
            {
                Port = context.OutputPorts[0];
                return null;
            }

            public void Open(DataFlowOpOpenContext openContext)
            {

            }

            public void Close(DataFlowOpCloseContext openContext)
            {

            }
        }
    }
}
