///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientExceptionHandler : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddPlugInAggregationFunctionFactory("myinvalidagg", typeof(ExecClientExceptionHandlerNoHandler.InvalidAggTestFactory));
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl = "@Name('ABCName') select Myinvalidagg() from SupportBean";
            epService.EPAdministrator.CreateEPL(epl);
    
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            } catch (EPException) {
                /* expected */
            }
        }
    }
} // end of namespace
