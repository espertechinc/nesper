///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using XLR8.CGLib;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Internal abstraction around the assertion process.  This method can proxy its calls to NUnit, System.Diagnostics
    /// or any other assertion framework.
    /// </summary>
    public class AssertProxy
    {
        /// <summary>
        /// An action that is triggered when a condition fails.
        /// </summary>
        public static Action<string> AssertFail { get; set; }

        /// <summary>
        /// Static constructor
        /// </summary>
        static AssertProxy()
        {
            AssertFail = message => System.Diagnostics.Debug.Assert(true, message);

        }

        private static string SanitizeMessage(
            string message,
            string defaultMessage)
        {
            if (message == null) {
                return defaultMessage;
            }

            return message;
        }

        public static void True(
            bool condition,
            string message = null)
        {
            if (!condition) {
                AssertFail(SanitizeMessage(message, "Expected true, but received false"));
            }
        }

        public static void False(
            bool condition,
            string message = null)
        {
            if (condition) {
                AssertFail(SanitizeMessage(message, "Expected false, but received true"));
            }
        }

        public static void Fail(string message = null)
        {
            AssertFail(SanitizeMessage(message, string.Empty));
        }
    }
}