///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
	///  Placeholder for an integer value in an event expression.
	/// </summary>

    [Serializable]
    public sealed class IntValue : PrimitiveValueBase
	{
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
		override public PrimitiveValueType Type
		{
			get { return PrimitiveValueType.INTEGER; }
		}

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
		override public Object ValueObject
		{
			get { return _intValue; }
		}

        /// <summary>
        /// Set an int value.
        /// </summary>
        /// <value></value>
		override public int _Int
		{
			set { _intValue = value; }
		}
		
		private int? _intValue;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		
		public IntValue()
		{
		}
		
		/// <summary> Constructor.</summary>
		/// <param name="intValue">is the value to set to
		/// </param>

		public IntValue(int intValue)
		{
			_intValue = intValue;
		}
		
		/// <summary> Parse the string array returning a int array.</summary>
		/// <param name="values">string array
		/// </param>
		/// <returns> typed array
		/// </returns>
		
		public static int[] ParseString(String[] values)
		{
			int[] result = new int[values.Length];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = ParseString(values[i]);
			}
			
			return result;
		}
		
		/// <summary> Parse string value returning a int.</summary>
		/// <param name="value">to parse
		/// </param>
		/// <returns> parsed value
		/// </returns>
		
		public static int ParseString(String value)
		{
            return Int32.Parse(value);
		}

        /// <summary>
        /// Parse the string literal value into the specific data type.
        /// </summary>
        /// <param name="value">is the textual value to parse</param>
        public override void Parse(String value)
        {
            _intValue = Int32.Parse(value);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override String ToString()
		{
			if (_intValue == null)
			{
				return "null";
			}
			return _intValue.ToString();
		}
	}
}
