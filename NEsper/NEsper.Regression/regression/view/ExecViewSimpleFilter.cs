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

namespace com.espertech.esper.regression.view
{
    public class ExecViewSimpleFilter : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionNotEqualsOp(epService);
            RunAssertionCombinationEqualsOp(epService);
        }
    
        private void RunAssertionNotEqualsOp(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName +
                            "(TheString != 'a')");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "a");
            Assert.IsFalse(listener.IsInvoked);
    
            Object theEvent = SendEvent(epService, "b");
            Assert.AreSame(theEvent, listener.GetAndResetLastNewData()[0].Underlying);
    
            SendEvent(epService, "a");
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = SendEvent(epService, null);
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionCombinationEqualsOp(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName +
                            "(TheString != 'a', IntPrimitive=0)");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "b", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "a", 0);
            Assert.IsFalse(listener.IsInvoked);
    
            Object theEvent = SendEvent(epService, "x", 0);
            Assert.AreSame(theEvent, listener.GetAndResetLastNewData()[0].Underlying);
    
            SendEvent(epService, null, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private Object SendEvent(EPServiceProvider epService, string stringValue) {
            return SendEvent(epService, stringValue, -1);
        }
    
        private Object SendEvent(EPServiceProvider epService, string stringValue, int intPrimitive) {
            var theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            theEvent.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
} // end of namespace
