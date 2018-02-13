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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.execution;

// using static junit.framework.TestCase.assertNotNull;
// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIInstantiationOptions : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionParameterInjectionCallback(epService);
            RunAssertionOperatorInjectionCallback(epService);
        }
    
        private void RunAssertionParameterInjectionCallback(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne MyOp -> outstream<SomeType> {propOne:'abc', propThree:'xyz'}");
    
            var myOp = new MyOp("myid");
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(myOp));
            var myParameterProvider = new MyParameterProvider(Collections.SingletonDataMap("propTwo", "def"));
            options.ParameterProvider(myParameterProvider);
            Assert.AreEqual("myid", myOp.Id);
            Assert.IsNull(myOp.PropOne);
            Assert.IsNull(myOp.PropTwo);
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("abc", myOp.PropOne);
            Assert.AreEqual("def", myOp.PropTwo);
    
            Assert.AreEqual(3, myParameterProvider.contextMap.Count);
            Assert.IsNotNull(myParameterProvider.contextMap.Get("propOne"));
    
            EPDataFlowOperatorParameterProviderContext context = myParameterProvider.contextMap.Get("propTwo");
            Assert.AreEqual("propTwo", context.ParameterName);
            Assert.AreEqual("MyOp", context.OperatorName);
            Assert.AreSame(myOp, context.OperatorInstance);
            Assert.AreEqual(0, context.OperatorNum);
            Assert.AreEqual(null, context.ProvidedValue);
            Assert.AreEqual("MyDataFlowOne", context.DataFlowName);
    
            context = myParameterProvider.contextMap.Get("propThree");
            Assert.AreEqual("propThree", context.ParameterName);
            Assert.AreEqual("MyOp", context.OperatorName);
            Assert.AreSame(myOp, context.OperatorInstance);
            Assert.AreEqual(0, context.OperatorNum);
            Assert.AreEqual("xyz", context.ProvidedValue);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOperatorInjectionCallback(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema SomeType ()");
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne MyOp -> outstream<SomeType> {propOne:'abc', propThree:'xyz'}");
    
            var myOperatorProvider = new MyOperatorProvider();
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(myOperatorProvider);
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
    
            Assert.AreEqual(1, myOperatorProvider.contextMap.Count);
            EPDataFlowOperatorProviderContext context = myOperatorProvider.contextMap.Get("MyOp");
            Assert.AreEqual("MyOp", context.OperatorName);
            Assert.IsNotNull(context.Spec);
            Assert.AreEqual("MyDataFlowOne", context.DataFlowName);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class MyOp : DataFlowSourceOperator {
    
            private readonly string id;
    
            [DataFlowOpParameter]
            private string propOne;
    
            [DataFlowOpParameter]
            private string propTwo;
    
            [DataFlowOpParameter]
            private string propThree;
    
            public MyOp(string id) {
                this.id = id;
            }
    
            public void Next() {
            }
    
            public string GetPropOne() {
                return propOne;
            }
    
            public void SetPropOne(string propOne) {
                this.propOne = propOne;
            }
    
            public string GetId() {
                return id;
            }
    
            public string GetPropTwo() {
                return propTwo;
            }
    
            public string GetPropThree() {
                return propThree;
            }
    
            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
                return null;
            }
    
            public void Open(DataFlowOpOpenContext openContext) {
            }
    
            public void Close(DataFlowOpCloseContext openContext) {
            }
        }
    
        public class MyParameterProvider : EPDataFlowOperatorParameterProvider {
    
            private IDictionary<string, EPDataFlowOperatorParameterProviderContext> contextMap = new Dictionary<string, EPDataFlowOperatorParameterProviderContext>();
            private readonly IDictionary<string, Object> values;
    
            public MyParameterProvider(IDictionary<string, Object> values) {
                this.values = values;
            }
    
            public Object Provide(EPDataFlowOperatorParameterProviderContext context) {
                contextMap.Put(context.ParameterName, context);
                return Values.Get(context.ParameterName);
            }
        }
    
        public class MyOperatorProvider : EPDataFlowOperatorProvider {
            private IDictionary<string, EPDataFlowOperatorProviderContext> contextMap = new Dictionary<string, EPDataFlowOperatorProviderContext>();
    
            public Object Provide(EPDataFlowOperatorProviderContext context) {
                contextMap.Put(context.OperatorName, context);
                return new MyOp("test");
            }
        }
    }
} // end of namespace
