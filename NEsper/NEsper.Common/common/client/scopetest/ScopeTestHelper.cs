///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.common.client.scopetest
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
        public static void AssertFalse(
            string message,
            bool condition)
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
        public static void AssertTrue(
            string message,
            bool condition)
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        private static bool AreCollectionsEqual(
            object expected,
            object actual)
        {
            var magicMarker = MagicMarker.SingletonInstance;
            var magicExpected = magicMarker.GetCollectionFactory(expected.GetType())
                .Invoke(expected);
            var magicActual = magicMarker.GetCollectionFactory(actual.GetType())
                .Invoke(actual);
            if (magicExpected.Count == magicActual.Count)
            {
                using (var magicExpectedEnum = magicExpected.GetEnumerator())
                {
                    using (var magicActualEnum = magicActual.GetEnumerator())
                    {
                        while (true)
                        {
                            var mvExpected = magicExpectedEnum.MoveNext();
                            var mvActual = magicActualEnum.MoveNext();
                            if (mvExpected && mvActual)
                            {
                                if (!Equals(magicExpectedEnum.Current, magicActualEnum.Current))
                                {
                                    break;
                                }
                            }
                            else if (!mvExpected && !mvActual)
                            {
                                return true;
                            }
                            else
                            {
                                throw new IllegalStateException("collection has been modified");
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>Assert that two values equal. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertEquals(
            string message,
            object expected,
            object actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            if (expected != null && expected.Equals(actual))
            {
                return;
            }

            if (expected != null && actual != null)
            {
                if (expected.GetType().IsGenericCollection() && actual.GetType().IsGenericCollection())
                {
                    if (AreCollectionsEqual(expected, actual))
                    {
                        return;
                    }
                }
            }

            FailNotEquals(message, expected, actual);
        }

        /// <summary>Assert that two values equal. </summary>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertEquals(
            object expected,
            object actual)
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
        public static void AssertSame(
            string message,
            object expected,
            object actual)
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
        public static void AssertSame(
            object expected,
            object actual)
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
        public static void AssertNull(
            string message,
            object @object)
        {
            AssertTrue(message, @object == null);
        }

        /// <summary>Assert that a value is not null. </summary>
        /// <param name="object">the object to check</param>
        public static void AssertNotNull(object @object)
        {
            AssertTrue(@object != null);
        }

        /// <summary>
        /// Assert that a value is not null.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="object">the object to check</param>
        public static void AssertNotNull(
            string message,
            object @object)
        {
            AssertTrue(message, @object != null);
        }

        /// <summary>Assert that a value is null. </summary>
        /// <param name="object">the object to check</param>
        public static void AssertNull(object @object)
        {
            AssertTrue(@object == null);
        }

        /// <summary>Fail assertion formatting a message for not-same. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void FailNotSame(
            string message,
            object expected,
            object actual)
        {
            Fail(Format(message, expected, actual, true));
        }

        /// <summary>Fail assertion formatting a message for not-equals. </summary>
        /// <param name="message">an optional message</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void FailNotEquals(
            string message,
            object expected,
            object actual)
        {
            Fail(Format(message, expected, actual, false));
        }

        /// <summary>Fail assertion. </summary>
        /// <param name="message">an optional message</param>
        public static void Fail(string message)
        {
            AssertProxy.Fail(message);
        }

        private static string Format(
            string message,
            object expected,
            object actual,
            bool isSame)
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