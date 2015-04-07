///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class SupportDataFlowAssertionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static void TryInvalidRun(EPServiceProvider epService, String epl, String name, String message) {
            epService.EPAdministrator.CreateEPL(epl);
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate(name);
    
            try {
                df.Run();
                Assert.Fail();
            }
            catch (EPDataFlowExecutionException ex) {
                AssertException(message, ex.Message);
            }
        }
    
        public static void TryInvalidInstantiate(EPServiceProvider epService, String name, String epl, String message) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
    
            try {
                epService.EPRuntime.DataFlowRuntime.Instantiate(name);
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex) {
                Log.Info("Expected exception: " + ex.Message, ex);
                AssertException(message, ex.Message);
            }
            finally {
                stmt.Dispose();
            }
        }
    
        public static void TryInvalidCreate(EPServiceProvider epService, String epl, String message) {
            try {
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                AssertException(message, ex.Message);
            }
        }
    
        private static void AssertException(String expected, String message) {
            String received;
            if (message.LastIndexOf("[") != -1) {
                received = message.Substring(0, message.LastIndexOf("[") + 1);
            }
            else {
                received = message;
            }

            if (message == expected)
            {
                return;
            }

            if (message.StartsWith(expected)) {
                Assert.IsFalse(
                    string.IsNullOrEmpty(expected.Trim()),
                    "empty expected message, received:\n" + message);
                return;
            }
            Assert.Fail("Expected:\n" + expected + "\nbut received:\n" + received + "\n");
        }
    }
}
