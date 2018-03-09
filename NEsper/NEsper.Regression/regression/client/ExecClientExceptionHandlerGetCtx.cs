///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientExceptionHandlerGetCtx : RegressionExecution {
        public override void Configure(Configuration configuration) {
            // add same factory twice
            SupportExceptionHandlerFactory.FactoryContexts.Clear();
            SupportExceptionHandlerFactory.Handlers.Clear();
            configuration.EngineDefaults.ExceptionHandling.HandlerFactories.Clear();
            configuration.EngineDefaults.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactory));
            configuration.EngineDefaults.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactory));
            configuration.AddEventType<SupportBean>();
            configuration.AddPlugInAggregationFunctionFactory("myinvalidagg", typeof(ExecClientExceptionHandlerNoHandler.InvalidAggTestFactory));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string epl = "@Name('ABCName') select Myinvalidagg() from SupportBean";
            epService.EPAdministrator.CreateEPL(epl);
    
            IList<ExceptionHandlerFactoryContext> contexts = SupportExceptionHandlerFactory.FactoryContexts;
            Assert.AreEqual(2, contexts.Count);
            Assert.AreEqual(epService.URI, contexts[0].EngineURI);
            Assert.AreEqual(epService.URI, contexts[1].EngineURI);
    
            SupportExceptionHandlerFactory.SupportExceptionHandler handlerOne = SupportExceptionHandlerFactory.Handlers[0];
            SupportExceptionHandlerFactory.SupportExceptionHandler handlerTwo = SupportExceptionHandlerFactory.Handlers[1];
            epService.EPRuntime.SendEvent(new SupportBean());
    
            Assert.AreEqual(1, handlerOne.Contexts.Count);
            Assert.AreEqual(1, handlerTwo.Contexts.Count);
            ExceptionHandlerContext ehc = handlerOne.Contexts[0];
            Assert.AreEqual(epService.URI, ehc.EngineURI);
            Assert.AreEqual(epl, ehc.Epl);
            Assert.AreEqual("ABCName", ehc.StatementName);
            Assert.AreEqual("Sample exception", ehc.Exception.Message);
            Assert.IsNotNull(ehc.CurrentEvent);
        }
    }
} // end of namespace
