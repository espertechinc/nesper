///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Placeholder for a decimal value in an event expression.
    /// </summary>

    [Serializable]
    public class DecimalValue : PrimitiveValueBase
    {
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        override public PrimitiveValueType Type
        {
            get { return PrimitiveValueType.DECIMAL; }
        }

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        override public Object ValueObject
        {
            get { return decimalValue; }
        }

        private decimal? decimalValue;

        /// <summary> Constructor.</summary>
        public DecimalValue()
        {
        }

        /// <summary>
        /// Constructor setting the value.
        /// </summary>
        /// <param name="decimalValue">The decimal value.</param>
        public DecimalValue(decimal decimalValue)
        {
            this.decimalValue = decimalValue;
        }

        /// <summary>
        /// Parse string value returning a decimal.
        /// </summary>
        /// <param name="value">value to parse</param>
        /// <returns>parsed value</returns>
        public static decimal ParseString(String value)
        {
            if (value.EndsWith("m"))
            {
                value = value.Substring(0, value.Length - 1);
            }

            return Decimal.Parse(value);
        }

        /// <summary>
        /// Parse the string literal value into the specific data type.
        /// </summary>
        /// <param name="value">is the textual value to parse</param>
        public override void Parse(String value)
        {
            decimalValue = ParseString(value);
        }

        /// <summary> Parse the string array returning a decimal array.</summary>
        /// <param name="values">string array
        /// </param>
        /// <returns> typed array
        /// </returns>
        public static decimal[] ParseString(String[] values)
        {
            decimal[] result = new decimal[values.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ParseString(values[i]);
            }
            return result;
        }

        /// <summary> Return the value as an unboxed.</summary>
        /// <returns> value
        /// </returns>

        public decimal GetDecimal()
        {
            if (decimalValue == null)
            {
                throw new IllegalStateException();
            }
            return decimalValue.Value;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            if (decimalValue == null)
            {
                return "null";
            }
            return decimalValue.ToString();
        }
    }
}
