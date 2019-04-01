///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

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
    public class ExecJoin20Stream : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
    
            var buf = new StringWriter();
            buf.Write("select * from ");
    
            string delimiter = "";
            for (int i = 0; i < 20; i++) {
                buf.Write(delimiter);
                buf.Write("S0(id=" + i + ")#lastevent as s_" + i);
                delimiter = ", ";
            }
            EPStatement stmt = epService.EPAdministrator.CreateEPL(buf.ToString());
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 19; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i));
            }
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_S0(19));
            Assert.IsTrue(listener.IsInvoked);
        }
    }
} // end of namespace
