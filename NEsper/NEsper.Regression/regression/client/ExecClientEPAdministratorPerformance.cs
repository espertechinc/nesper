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

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertTrue;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPAdministratorPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertion1kValidStmtsPerformance(epService);
            RunAssertion1kInvalidStmts(epService);
        }
    
        private void RunAssertion1kValidStmtsPerformance(EPServiceProvider epService) {
            long start = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                string text = "select * from " + typeof(SupportBean).FullName;
                EPStatement stmt = epService.EPAdministrator.CreateEPL(text, "s1");
                Assert.AreEqual("s1", stmt.Name);
                stmt.Stop();
                stmt.Start();
                stmt.Stop();
                stmt.Dispose();
            }
            long end = DateTimeHelper.CurrentTimeMillis;
            long delta = end - start;
            Assert.IsTrue(".test10kValid delta=" + delta, delta < 5000);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion1kInvalidStmts(EPServiceProvider epService) {
            long start = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                try {
                    string text = "select xxx from " + typeof(SupportBean).FullName;
                    epService.EPAdministrator.CreateEPL(text, "s1");
                } catch (Exception ex) {
                    // expected
                }
            }
            long end = DateTimeHelper.CurrentTimeMillis;
            long delta = end - start;
            Assert.IsTrue(".test1kInvalid delta=" + delta, delta < 2500);
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
