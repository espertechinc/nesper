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
    public class ExecViewMultipleExpiry : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            // Testing the two forms of the case expression
            // Furthermore the test checks the different when clauses and actions related.
            string caseExpr = "select volume " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#unique(symbol)#time(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendMarketDataEvent(epService, "DELL", 1, 50);
            SendMarketDataEvent(epService, "DELL", 2, 50);
            object[] values = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.AreEqual(1, values.Length);
        }
    
        private void SendMarketDataEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
