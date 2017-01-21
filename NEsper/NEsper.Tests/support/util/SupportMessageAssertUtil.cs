///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.support.util
{
    public class SupportMessageAssertUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static void TryInvalid(EPServiceProvider engine, string epl, string message)
        {
            try
            {
                engine.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                AssertMessage(ex, message);
            }
        }

        public static void TryInvalidExecuteQuery(EPServiceProvider engine, String epl, String message)
        {
            try
            {
                engine.EPRuntime.ExecuteQuery(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                AssertMessage(ex, message);
            }
        }

        public static void AssertMessageContains(Exception ex, String message)
        {
            if (!ex.Message.Contains(message))
            {
                Assert.Fail("Does not contain text: '" + message + "' in text \n text:" + ex.Message);
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                Debug.WriteLine(ex.StackTrace);
                Assert.Fail("empty expected message");
            }
        }
    
        public static void AssertMessage(Exception ex, string message)
        {
            if (message.Equals("skip")) {
                return; // skip message validation
            }
            if (message.Length > 10) {
                Log.Error("Exception: " + ex.Message, ex);
                if (!ex.Message.StartsWith(message)) {
                    Assert.Fail("\nExpected:" + message + "\nReceived:" + ex.Message);
                }
            }
            else {
                Log.Error("Exception: " + ex.Message, ex);
                Assert.Fail("No assertion provided, received: " + ex.Message);
            }
        }
    }
}
