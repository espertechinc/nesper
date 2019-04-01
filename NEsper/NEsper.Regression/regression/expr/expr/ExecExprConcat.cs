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
using com.espertech.esper.regression.epl.other;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprConcat : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string epl = "select p00 || p01 as c1, p00 || p01 || p02 as c2, p00 || '|' || p01 as c3" +
                    " from " + typeof(SupportBean_S0).FullName + "#length(10)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "b", "c"));
            AssertConcat(listener, "ab", "abc", "a|b");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, null, "b", "c"));
            AssertConcat(listener, null, null, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "", "b", "c"));
            AssertConcat(listener, "b", "bc", "|b");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "123", null, "c"));
            AssertConcat(listener, null, null, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "123", "456", "c"));
            AssertConcat(listener, "123456", "123456c", "123|456");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "123", "456", null));
            AssertConcat(listener, "123456", null, "123|456");
        }
    
        private void AssertConcat(SupportUpdateListener listener, string c1, string c2, string c3) {
            EventBean theEvent = listener.LastNewData[0];
            Assert.AreEqual(c1, theEvent.Get("c1"));
            Assert.AreEqual(c2, theEvent.Get("c2"));
            Assert.AreEqual(c3, theEvent.Get("c3"));
            listener.Reset();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
