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
    public class TestAPIInstantiationOptions
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.CreateEPL("create schema SomeType ()");
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestParameterInjectionCallback() 
        {
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne MyOp -> outstream<SomeType> {propOne:'abc', propThree:'xyz'}");
    
            var myOp = new MyOp("myid");
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(myOp));
            var myParameterProvider = new MyParameterProvider(Collections.SingletonDataMap("propTwo", "def"));
            options.ParameterProvider(myParameterProvider);
            Assert.AreEqual("myid", myOp.Id);
            Assert.IsNull(myOp.PropOne);
            Assert.IsNull(myOp.PropTwo);
    
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.AreEqual("abc", myOp.PropOne);
            Assert.AreEqual("def", myOp.PropTwo);
    
            Assert.AreEqual(3, myParameterProvider.ContextMap.Count);
            Assert.NotNull(myParameterProvider.ContextMap.Get("propOne"));
    
            EPDataFlowOperatorParameterProviderContext context = myParameterProvider.ContextMap.Get("propTwo");
            Assert.AreEqual("propTwo", context.ParameterName);
            Assert.AreEqual("MyOp", context.OperatorName);
            Assert.AreSame(myOp, context.OperatorInstance);
            Assert.AreEqual(0, context.OperatorNum);
            Assert.AreEqual(null, context.ProvidedValue);
            Assert.AreEqual("MyDataFlowOne", context.DataFlowName);

            context = myParameterProvider.ContextMap.Get("propThree");
            Assert.AreEqual("propThree", context.ParameterName);
            Assert.AreEqual("MyOp", context.OperatorName);
            Assert.AreSame(myOp, context.OperatorInstance);
            Assert.AreEqual(0, context.OperatorNum);
            Assert.AreEqual("xyz", context.ProvidedValue);
        }
    
        [Test]
        public void TestOperatorInjectionCallback() {
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne MyOp -> outstream<SomeType> {propOne:'abc', propThree:'xyz'}");
    
            var myOperatorProvider = new MyOperatorProvider();
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(myOperatorProvider);
    
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
    
            Assert.AreEqual(1, myOperatorProvider.ContextMap.Count);
            EPDataFlowOperatorProviderContext context = myOperatorProvider.ContextMap.Get("MyOp");
            Assert.AreEqual("MyOp", context.OperatorName);
            Assert.NotNull(context.Spec);
            Assert.AreEqual("MyDataFlowOne", context.DataFlowName);
        }

        public class MyOp : DataFlowSourceOperator
        {
            private readonly String _id;

            [DataFlowOpParameter] private String propOne;
            [DataFlowOpParameter] private String propTwo;
            [DataFlowOpParameter] private String propThree;

            public MyOp(String id)
            {
                _id = id;
            }

            public void Next()
            {
            }

            public string PropOne
            {
                get { return propOne; }
                set { propOne = value; }
            }

            public string Id
            {
                get { return _id; }
            }

            public string PropTwo
            {
                get { return propTwo; }
            }

            public string PropThree
            {
                get { return propThree; }
            }

            public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
            {
                return null;
            }

            public void Open(DataFlowOpOpenContext openContext)
            {
            }

            public void Close(DataFlowOpCloseContext openContext)
            {
            }
        }

        public class MyParameterProvider : EPDataFlowOperatorParameterProvider
        {
            private readonly IDictionary<String, EPDataFlowOperatorParameterProviderContext> _contextMap = 
                new Dictionary<String, EPDataFlowOperatorParameterProviderContext>();
            private readonly IDictionary<String, Object> _values;

            internal IDictionary<string, EPDataFlowOperatorParameterProviderContext> ContextMap
            {
                get { return _contextMap; }
            }

            public MyParameterProvider(IDictionary<String, Object> values)
            {
                _values = values;
            }

            public Object Provide(EPDataFlowOperatorParameterProviderContext context)
            {
                _contextMap.Put(context.ParameterName, context);
                return _values.Get(context.ParameterName);
            }
        }

        public class MyOperatorProvider : EPDataFlowOperatorProvider
        {
            private readonly IDictionary<String, EPDataFlowOperatorProviderContext> _contextMap = 
                new Dictionary<String, EPDataFlowOperatorProviderContext>();

            internal IDictionary<string, EPDataFlowOperatorProviderContext> ContextMap
            {
                get { return _contextMap; }
            }

            public Object Provide(EPDataFlowOperatorProviderContext context)
            {
                _contextMap.Put(context.OperatorName, context);
                return new MyOp("test");
            }
        }
    }
}
