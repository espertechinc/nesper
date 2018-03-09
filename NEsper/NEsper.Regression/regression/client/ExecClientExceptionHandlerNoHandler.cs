///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.client
{
    public class ExecClientExceptionHandlerNoHandler : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ExceptionHandling.HandlerFactories.Clear();
            configuration.AddPlugInAggregationFunctionFactory("myinvalidagg", typeof(InvalidAggTestFactory));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl = "@Name('ABCName') select Myinvalidagg() from SupportBean";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.SendEvent(new SupportBean());
        }
    
        public class InvalidAggTestFactory : AggregationFunctionFactory
        {
    
            public void Validate(AggregationValidationContext validationContext) {
            }

            public Type ValueType
            {
                get { return null; }
            }

            public string FunctionName
            {
                set { }
            }

            public AggregationMethod NewAggregator() {
                return new InvalidAggTest();
            }
        }
    
        public class InvalidAggTest : AggregationMethod {
    
            public void Enter(Object value) {
                throw new EPRuntimeException("Sample exception");
            }
    
            public void Leave(Object value) {
            }

            public object Value
            {
                get { return null; }
            }

            public void Clear() {
            }
        }
    
    }
} // end of namespace
