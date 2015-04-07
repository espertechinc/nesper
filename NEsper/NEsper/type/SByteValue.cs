///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Globalization;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Placeholder for a sbyte value in an event expression.
    /// </summary>

    [Serializable]
    public sealed class SByteValue : PrimitiveValueBase
    {
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        override public PrimitiveValueType Type
        {
            get { return PrimitiveValueType.BYTE; }
        }

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        override public Object ValueObject
        {
            get { return _byteValue; }
        }

        /// <summary>
        /// Set a byte value.
        /// </summary>
        /// <value></value>
        override public sbyte _SByte
        {
            set { _byteValue = value; }
        }

        private sbyte? _byteValue;

        /// <summary>Parses a string value as a byte.</summary>
        /// <param name="value">to parse</param>
        /// <returns>byte value</returns>
        public static sbyte ParseString(String value)
        {
            if (value.ToUpper().StartsWith("0X"))
            {
                return SByte.Parse(value.Substring(2), NumberStyles.HexNumber);
            }

            return SByte.Parse(value);
        }

        /// <summary>
        /// Parse the string literal value into the specific data type.
        /// </summary>
        /// <param name="value">is the textual value to parse</param>
        public override void Parse(String value)
        {
            if (value.ToUpper().StartsWith("0X"))
            {
                _byteValue = SByte.Parse(value.Substring(2), NumberStyles.HexNumber);
            }
            else
            {
                _byteValue = SByte.Parse(value);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            if (_byteValue == null)
            {
                return "null";
            }
            return _byteValue.ToString();
        }
    }
}
