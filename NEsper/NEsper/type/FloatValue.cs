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
    /// Placeholder for a float value in an event expression.
    /// </summary>

    [Serializable]
    public sealed class FloatValue : PrimitiveValueBase
    {
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        override public PrimitiveValueType Type
        {
            get
            {
                return PrimitiveValueType.FLOAT;
            }

        }
        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        override public Object ValueObject
        {
            get
            {
                return floatValue;
            }

        }
        /// <summary>
        /// Set a float value.
        /// </summary>
        /// <value></value>
        override public float _Float
        {
            set
            {
                this.floatValue = value;
            }

        }
        private float? floatValue;

        /// <summary> Parse string value returning a float.</summary>
        /// <param name="value">to parse
        /// </param>
        /// <returns> parsed value
        /// </returns>
        public static float ParseString(String value)
        {
            if (value.EndsWith("f") || value.EndsWith("F"))
            {
                value = value.Substring(0, value.Length - 1);
            }

            return Single.Parse(value);
        }

        /// <summary> Parse the string array returning a float array.</summary>
        /// <param name="values">string array
        /// </param>
        /// <returns> typed array
        /// </returns>
        public static float[] ParseString(String[] values)
        {
            float[] result = new float[values.Length];
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
            floatValue = ParseString(value);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            if (floatValue == null)
            {
                return "null";
            }

            return floatValue.ToString();
        }
    }
}
