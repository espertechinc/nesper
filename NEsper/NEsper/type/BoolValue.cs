///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Placeholder for a bool value in an event expression.
    /// </summary>

    [Serializable]
    public sealed class BoolValue : PrimitiveValueBase
    {
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        override public PrimitiveValueType Type
        {
            get { return PrimitiveValueType.BOOL; }
        }

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        override public Object ValueObject
        {
            get { return boolValue; }
        }

        /// <summary>
        /// Set a bool value.
        /// </summary>
        /// <value></value>
        override public bool _Boolean
        {
            set { this.boolValue = value; }
        }

        private bool? boolValue;

        /// <summary> Constructor.</summary>
        /// <param name="boolValue">sets the value.
        /// </param>
        public BoolValue(Boolean boolValue)
        {
            this.boolValue = boolValue;
        }

        /// <summary> Constructor.</summary>
        public BoolValue()
        {
        }

        /// <summary> Parse the bool string.</summary>
        /// <param name="value">is a bool value
        /// </param>
        /// <returns> parsed bool
        /// </returns>
        public static bool ParseString(String value)
        {
            bool rvalue;
            value = value.ToLower();
            if (!Boolean.TryParse(value, out rvalue))
            {
                throw new ArgumentException("Boolean value '" + value + "' cannot be converted to bool");
            }

            return rvalue;
        }

        /// <summary> Parse the string array returning a bool array.</summary>
        /// <param name="values">string array
        /// </param>
        /// <returns> typed array
        /// </returns>
        public static bool[] ParseString(String[] values)
        {
            bool[] result = new bool[values.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ParseString(values[i]);
            }
            return result;
        }

        /// <summary>
        /// Parse the string literal value into the specific data type.
        /// </summary>
        /// <param name="value">is the textual value to parse</param>
        public override void Parse(String value)
        {
            boolValue = ParseString(value);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            if (boolValue == null)
            {
                return "null";
            }

            return boolValue.ToString();
        }
    }
}
