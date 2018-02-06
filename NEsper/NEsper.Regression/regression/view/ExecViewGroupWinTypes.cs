///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewGroupWinTypes : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string viewStmt = "select * from " + typeof(SupportBean).FullName +
                    "#groupwin(IntPrimitive)#length(4)#groupwin(LongBoxed)#uni(DoubleBoxed)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(viewStmt);
    
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("LongBoxed"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("stddev"));
            Assert.AreEqual(8, stmt.EventType.PropertyNames.Length);
        }
    }
} // end of namespace
