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

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLComments : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string lineSeparator = Environment.NewLine;
            string statement = "select TheString, /* this is my string */\n" +
                    "IntPrimitive, // same line comment\n" +
                    "/* comment taking one line */\n" +
                    "// another comment taking a line\n" +
                    "IntPrimitive as /* rename */ myPrimitive\n" +
                    "from " + typeof(SupportBean).FullName + lineSeparator +
                    " where /* inside a where */ IntPrimitive /* */ = /* */ 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statement);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 100));
    
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("e1", theEvent.Get("TheString"));
            Assert.AreEqual(100, theEvent.Get("IntPrimitive"));
            Assert.AreEqual(100, theEvent.Get("myPrimitive"));
            updateListener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
        }
    }
} // end of namespace
