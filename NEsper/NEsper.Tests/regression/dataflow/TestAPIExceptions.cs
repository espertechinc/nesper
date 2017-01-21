///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestAPIExceptions
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp).FullName);
            MyExceptionHandler.Contexts.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestExceptionHandler()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // test exception by graph source
            var stmtGraph = _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow DefaultSupportSourceOp -> outstream<SupportBean> {}");
    
            var op = new DefaultSupportSourceOp(new Object[] { new Exception("My-Exception-Is-Here") });
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(op));
            var handler = new MyExceptionHandler();
            options.ExceptionHandler(handler);
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
    
            df.Start();
            Thread.Sleep(100);
            Assert.AreEqual(EPDataFlowState.COMPLETE, df.State);
    
            Assert.AreEqual(1, MyExceptionHandler.Contexts.Count);
            EPDataFlowExceptionContext context = MyExceptionHandler.Contexts[0];
            Assert.AreEqual("MyDataFlow", context.DataFlowName);
            Assert.AreEqual("DefaultSupportSourceOp", context.OperatorName);
            Assert.AreEqual(0, context.OperatorNumber);
            Assert.AreEqual("DefaultSupportSourceOp#0() -> outstream<SupportBean>", context.OperatorPrettyPrint);
            Assert.AreEqual("Support-graph-source generated exception: My-Exception-Is-Here", context.Exception.Message);
            df.Cancel();
            stmtGraph.Dispose();
            MyExceptionHandler.Contexts.Clear();
    
            // test exception by operator
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyExceptionOp));
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow DefaultSupportSourceOp -> outstream<SupportBean> {}" +
                    "MyExceptionOp(outstream) {}");
    
            var opTwo = new DefaultSupportSourceOp(new Object[] {new SupportBean("E1", 1)});
            var optionsTwo = new EPDataFlowInstantiationOptions();
            optionsTwo.OperatorProvider(new DefaultSupportGraphOpProvider(opTwo));
            var handlerTwo = new MyExceptionHandler();
            optionsTwo.ExceptionHandler(handlerTwo);
            var dfTwo = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", optionsTwo);
    
            dfTwo.Start();
            Thread.Sleep(100);
    
            Assert.AreEqual(1, MyExceptionHandler.Contexts.Count);
            EPDataFlowExceptionContext contextTwo = MyExceptionHandler.Contexts[0];
            Assert.AreEqual("MyDataFlow", contextTwo.DataFlowName);
            Assert.AreEqual("MyExceptionOp", contextTwo.OperatorName);
            Assert.AreEqual(1, contextTwo.OperatorNumber);
            Assert.AreEqual("MyExceptionOp#1(outstream)", contextTwo.OperatorPrettyPrint);
            Assert.AreEqual("Operator-thrown-exception", contextTwo.Exception.Message);
        }

        public class MyExceptionHandler : EPDataFlowExceptionHandler
        {

            private static List<EPDataFlowExceptionContext> _contexts = new List<EPDataFlowExceptionContext>();

            public static List<EPDataFlowExceptionContext> Contexts
            {
                get { return _contexts; }
            }

            public static void SetContexts(List<EPDataFlowExceptionContext> contexts)
            {
                MyExceptionHandler._contexts = contexts;
            }

            public void Handle(EPDataFlowExceptionContext context)
            {
                _contexts.Add(context);
            }
        }

        [DataFlowOperator]
        public class MyExceptionOp
        {
            public void OnInput(SupportBean bean)
            {
                throw new Exception("Operator-thrown-exception");
            }
        }
    }
}
