///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprMath : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string epl = "select IntPrimitive/IntBoxed as result from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("result"));
    
            SendEvent(epService, 100, 3);
            Assert.AreEqual(100 / 3d, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, null);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, 0);
            Assert.AreEqual(double.PositiveInfinity, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, -5, 0);
            Assert.AreEqual(double.NegativeInfinity, listener.AssertOneGetNewAndReset().Get("result"));
        }
    
        private void SendEvent(EPServiceProvider epService, int intPrimitive, int? intBoxed) {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
