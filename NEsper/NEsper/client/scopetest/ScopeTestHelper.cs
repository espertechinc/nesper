///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.client.scopetest
{
    /// <summary>
    /// Helper for asserting conditions.
    /// </summary>
    public class ScopeTestHelper
    {
        /// <summary>Assert a condition is false. </summary>
        /// <param name="condition">to assert</param>
        public static void AssertFalse(bool condition)
        {
            AssertTrue(!condition);
        }

        /// <summary>Assert a condition is false. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="condition">to assert</param>
        public static void AssertFalse(String message, bool condition)
        {
            if (condition)
            {
                Fail(message);
            }
        }

        /// <summary>Assert a condition is true. </summary>
        /// <param name="condition">to assert</param>
        public static void AssertTrue(bool condition)
        {
            AssertTrue(null, condition);
        }

        /// <summary>Assert a condition is true. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="condition">to assert</param>
        public static void AssertTrue(String message, bool condition)
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        /// <summary>Assert that two values equal. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertEquals(String message, Object expected, Object actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }
            if (expected != null && expected.Equals(actual))
            {
                return;
            }
            FailNotEquals(message, expected, actual);
        }

        /// <summary>Assert that two values equal. </summary>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertEquals(Object expected, Object actual)
        {
            AssertEquals(null, expected, actual);
        }

        /// <summary>Fail assertion. </summary>
        public static void Fail()
        {
            Fail(null);
        }

        /// <summary>Assert that two values are the same. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertSame(String message, Object expected, Object actual)
        {
            if (expected == actual)
            {
                return;
            }
            FailNotSame(message, expected, actual);
        }

        /// <summary>Assert that two values are the same. </summary>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertSame(Object expected, Object actual)
        {
            if (expected == actual)
            {
                return;
            }
            FailNotSame(null, expected, actual);
        }

        /// <summary>Assert that a value is null. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="object">the object to check</param>
        public static void AssertNull(String message, Object @object)
        {
            AssertTrue(message, @object == null);
        }

        /// <summary>Assert that a value is not null. </summary>
        /// <param name="object">the object to check</param>
        public static void AssertNotNull(Object @object)
        {
            AssertTrue(@object != null);
        }

        /// <summary>Assert that a value is null. </summary>
        /// <param name="object">the object to check</param>
        public static void AssertNull(Object @object)
        {
            AssertTrue(@object == null);
        }

        /// <summary>Fail assertion formatting a message for not-same. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void FailNotSame(String message, Object expected, Object actual)
        {
            Fail(Format(message, expected, actual, true));
        }

        /// <summary>Fail assertion formatting a message for not-equals. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void FailNotEquals(String message, Object expected, Object actual)
        {
            Fail(Format(message, expected, actual, false));
        }

        /// <summary>Fail assertion. </summary>
        /// <param name="message">an optional message</param>
        public static void Fail(String message)
        {
            AssertProxy.Fail(message);
        }

        private static String Format(String message, Object expected, Object actual, bool isSame)
        {
            var buf = new StringBuilder();
            if (!string.IsNullOrEmpty(message))
            {
                buf.Append(message);
                buf.Append(' ');
            }
            buf.Append("expected");
            if (isSame)
            {
                buf.Append(" same");
            }
            buf.Append(":<");
            buf.Append(expected == null ? "null" : expected.ToString());
            buf.Append("> but was:<");
            buf.Append(actual == null ? "null" : actual.ToString());
            buf.Append(">");
            return buf.ToString();
        }
    }
}
