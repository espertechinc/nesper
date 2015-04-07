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
using com.espertech.esper.client.hook;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestExceptionHandler 
    {
        private EPServiceProvider _epService;
    
        [Test]
        public void TestHandler()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();

            // add same factory twice
            config.EngineDefaults.ExceptionHandlingConfig.HandlerFactories.Clear();
            config.EngineDefaults.ExceptionHandlingConfig.AddClass(typeof(SupportExceptionHandlerFactory));
            config.EngineDefaults.ExceptionHandlingConfig.AddClass(typeof(SupportExceptionHandlerFactory));
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddPlugInAggregationFunctionFactory("myinvalidagg", typeof(InvalidAggTestFactory).FullName);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            SupportExceptionHandlerFactory.FactoryContexts.Clear();
            SupportExceptionHandlerFactory.Handlers.Clear();
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            const string epl = "@Name('ABCName') select Myinvalidagg() from SupportBean";
            _epService.EPAdministrator.CreateEPL(epl);
    
            IList<ExceptionHandlerFactoryContext> contexts = SupportExceptionHandlerFactory.FactoryContexts;
            Assert.AreEqual(2, contexts.Count);
            Assert.AreEqual(_epService.URI, contexts[0].EngineURI);
            Assert.AreEqual(_epService.URI, contexts[1].EngineURI);
    
            SupportExceptionHandlerFactory.SupportExceptionHandler handlerOne = SupportExceptionHandlerFactory.Handlers[0];
            SupportExceptionHandlerFactory.SupportExceptionHandler handlerTwo = SupportExceptionHandlerFactory.Handlers[1];
            _epService.EPRuntime.SendEvent(new SupportBean());
    
            Assert.AreEqual(1, handlerOne.Contexts.Count);
            Assert.AreEqual(1, handlerTwo.Contexts.Count);
            ExceptionHandlerContext ehc = handlerOne.Contexts[0];
            Assert.AreEqual(_epService.URI, ehc.EngineURI);
            Assert.AreEqual(epl, ehc.Epl);
            Assert.AreEqual("ABCName", ehc.StatementName);
            Assert.AreEqual("Sample exception", ehc.Exception.Message);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        /// <summary>
        /// Ensure the support configuration has an exception handler that rethrows exceptions.
        /// </summary>
        [Test]
        public void TestSupportConfigHandlerRethrow()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddPlugInAggregationFunctionFactory("myinvalidagg", typeof(InvalidAggTestFactory).FullName);

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            const string epl = "@Name('ABCName') select myinvalidagg() from SupportBean";
            _epService.EPAdministrator.CreateEPL(epl);

            try {
                _epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        public class InvalidAggTestFactory : AggregationFunctionFactory
        {
            public string FunctionName
            {
                set { }
            }

            public void Validate(AggregationValidationContext validationContext)
            {
            }

            public AggregationMethod NewAggregator()
            {
                return new InvalidAggTest();
            }

            public Type ValueType
            {
                get { return null; }
            }
        }

        public class InvalidAggTest : AggregationMethod
        {
            public void Validate(AggregationValidationContext validationContext)
            {
            }
    
            public void Enter(Object value)
            {
                throw new Exception("Sample exception");
            }
    
            public void Leave(Object value) {
            }

            public object Value
            {
                get { return null; }
            }

            public Type ValueType
            {
                get { return null; }
            }

            public void Clear()
            {
            }
        }
    }
}
