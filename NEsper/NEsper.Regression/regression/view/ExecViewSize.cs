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
    public class ExecViewSize : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string statementText = "select irstream size from " + typeof(SupportMarketDataBean).FullName + "#size";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "DELL", 1L);
            AssertSize(listener, 1, 0);
    
            SendEvent(epService, "DELL", 1L);
            AssertSize(listener, 2, 1);
    
            stmt.Dispose();
            statementText = "select size, symbol, feed from " + typeof(SupportMarketDataBean).FullName + "#size(symbol, feed)";
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
            string[] fields = "size,symbol,feed".Split(',');
    
            SendEvent(epService, "DELL", 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, "DELL", "f1"});
    
            SendEvent(epService, "DELL", 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L, "DELL", "f1"});
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "f1");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void AssertSize(SupportUpdateListener listener, long newSize, long oldSize) {
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "size", new object[]{newSize}, new object[]{oldSize});
        }
    }
} // end of namespace
