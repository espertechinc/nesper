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


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinInheritAndInterface : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string epl = "select a, b from " +
                    typeof(ISupportA).FullName + "#length(10), " +
                    typeof(ISupportB).FullName + "#length(10)" +
                    " where a = b";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new ISupportAImpl("1", "ab1"));
            epService.EPRuntime.SendEvent(new ISupportBImpl("2", "ab2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new ISupportBImpl("1", "ab3"));
            Assert.IsTrue(listener.IsInvoked);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("1", theEvent.Get("a"));
            Assert.AreEqual("1", theEvent.Get("b"));
        }
    }
} // end of namespace
