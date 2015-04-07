///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Placeholder for a long-typed value in an event expression.
    /// </summary>

    [Serializable]
    public sealed class LongValue : PrimitiveValueBase
    {
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        override public PrimitiveValueType Type
        {
            get { return PrimitiveValueType.LONG; }
        }

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        override public Object ValueObject
        {
            get { return longValue; }
        }

        private long? longValue;

        /// <summary>
        /// Parse the string literal value into the specific data type.
        /// </summary>
        /// <param name="value">is the textual value to parse</param>
        public override void Parse(String value)
        {
            longValue = ParseString(value);
        }

        /// <summary> Parse the string containing a long value.</summary>
        /// <param name="value">is the textual long value
        /// </param>
        /// <returns> long value
        /// </returns>
        public static long ParseString(String value)
        {
            if ((value.EndsWith("L")) || ((value.EndsWith("l"))))
            {
                value = value.Substring(0, value.Length - 1);
            }
            if (value.StartsWith("+"))
            {
                value = value.Substring(1);
            }
            return long.Parse(value);
        }

        /// <summary> Parse the string array returning a long array.</summary>
        /// <param name="values">string array
        /// </param>
        /// <returns> typed array
        /// </returns>
        public static long[] ParseString(String[] values)
        {
            long[] result = new long[values.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ParseString(values[i]);
            }
            return result;
        }

        /// <summary>
        /// Set a long value.
        /// </summary>
        /// <value></value>
        public override long _Long
        {
            set { this.longValue = value; }
        }

        /// <summary> Returns the long value.</summary>
        /// <returns> long value
        /// </returns>
        public long GetLong()
        {
            if (longValue == null)
            {
                throw new IllegalStateException();
            }
            return longValue.Value;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            if (longValue == null)
            {
                return "null";
            }
            return longValue.ToString();
        }
    }
}
