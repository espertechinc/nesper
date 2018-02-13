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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprMathDivisionRules : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Expression.IntegerDivision = true;
            configuration.EngineDefaults.Expression.DivisionByZeroReturnsNull = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string epl = "select intPrimitive/intBoxed as result from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("result"));
    
            SendEvent(epService, 100, 3);
            Assert.AreEqual(33, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, null);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
        }
    
        private void SendEvent(EPServiceProvider epService, int? intPrimitive, int? intBoxed) {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
