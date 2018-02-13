///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateLeaving : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            string epl = "select leaving() as val from SupportBean#length(3)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            RunAssertion(epService, listener);
    
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            stmt = epService.EPAdministrator.Create(model);
            stmt.AddListener(listener);
            Assert.AreEqual(epl, model.ToEPL());
    
            RunAssertion(epService, listener);
    
            TryInvalid(epService, "select leaving(1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'leaving(1)': The 'leaving' function expects no parameters");
        }
    
        private void RunAssertion(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "val".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{true});
        }
    }
} // end of namespace
