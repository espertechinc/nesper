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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIExceptions : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            MyExceptionHandler.Contexts.Clear();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // test exception by graph source
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow DefaultSupportSourceOp -> outstream<SupportBean> {}");
    
            var op = new DefaultSupportSourceOp(new object[]{new EPRuntimeException("My-Exception-Is-Here")});
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(op));
            var handler = new MyExceptionHandler();
            options.ExceptionHandler(handler);
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options);
    
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
            epService.EPAdministrator.Configuration.AddImport(typeof(MyExceptionOp));
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow DefaultSupportSourceOp -> outstream<SupportBean> {}" +
                    "MyExceptionOp(outstream) {}");
    
            var opTwo = new DefaultSupportSourceOp(new object[]{new SupportBean("E1", 1)});
            var optionsTwo = new EPDataFlowInstantiationOptions();
            optionsTwo.OperatorProvider(new DefaultSupportGraphOpProvider(opTwo));
            var handlerTwo = new MyExceptionHandler();
            optionsTwo.ExceptionHandler(handlerTwo);
            EPDataFlowInstance dfTwo = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", optionsTwo);
    
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
    
        public class MyExceptionHandler : EPDataFlowExceptionHandler {
    
            private static List<EPDataFlowExceptionContext> contexts = new List<EPDataFlowExceptionContext>();

            public static List<EPDataFlowExceptionContext> Contexts {
                get { return contexts; }
            }

            public static void SetContexts(List<EPDataFlowExceptionContext> contexts) {
                MyExceptionHandler.contexts = contexts;
            }
    
            public void Handle(EPDataFlowExceptionContext context) {
                contexts.Add(context);
            }
        }
    
        [DataFlowOperator]
        public class MyExceptionOp {
            public void OnInput(SupportBean bean) {
                throw new EPRuntimeException("Operator-thrown-exception");
            }
        }
    }
} // end of namespace
