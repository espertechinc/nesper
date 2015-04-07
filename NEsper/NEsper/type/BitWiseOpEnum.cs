///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.collection;

namespace com.espertech.esper.type
{
	/// <summary>
	/// Enum representing relational types of operation.
	/// </summary>
    public enum BitWiseOpEnum
	{
		/// <summary>
		/// Bitwise and.
		/// </summary>
		BAND,

		/// <summary>
		/// Bitwise or.
		/// </summary>
		BOR,

		/// <summary>
		/// Bitwise xor.
		/// </summary>
		BXOR
	}

    [Serializable]
	public static class BitWiseOpEnumExtensions
	{
		private static readonly IDictionary<MultiKeyUntyped, Computer> Computers;

        /// <summary>Returns string rendering of enum.</summary>
        /// <returns>bitwise operator string</returns>

		public static String GetComputeDescription(this BitWiseOpEnum value)
        {
            return GetExpressionText(value);
        }


		public static String GetExpressionText(this BitWiseOpEnum value)
        {
            switch(value)
            {
                case BitWiseOpEnum.BAND:
                    return "&";
                case BitWiseOpEnum.BOR:
                    return "|";
                case BitWiseOpEnum.BXOR:
                    return "^";
            }

            throw new ArgumentException("invalid value", "value");
        }

        /// <summary>
        /// Initializes the <see cref="BitWiseOpEnum"/> class.
        /// </summary>

        static BitWiseOpEnumExtensions()
		{
			Computers = new Dictionary<MultiKeyUntyped, Computer>();
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(byte?), BitWiseOpEnum.BAND }), BAndByte);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short?), BitWiseOpEnum.BAND }), BAndShort);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int?), BitWiseOpEnum.BAND }), BAndInt);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long?), BitWiseOpEnum.BAND }), BAndLong);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(bool?), BitWiseOpEnum.BAND }), BAndBoolean);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(byte?), BitWiseOpEnum.BOR }), BOrByte);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short?), BitWiseOpEnum.BOR }), BOrShort);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int?), BitWiseOpEnum.BOR }), BOrInt);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long?), BitWiseOpEnum.BOR }), BOrLong);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(bool?), BitWiseOpEnum.BOR }), BOrBoolean);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(byte?), BitWiseOpEnum.BXOR }), BXorByte);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short?), BitWiseOpEnum.BXOR }), BXorShort);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int?), BitWiseOpEnum.BXOR }), BXorInt);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long?), BitWiseOpEnum.BXOR }), BXorLong);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(bool?), BitWiseOpEnum.BXOR }), BXorBoolean);
		}

        /// <summary>
        /// Returns number or bool computation for the target coercion type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="coercedType">target type</param>
        /// <returns>number cruncher</returns>

        public static Computer GetComputer(this BitWiseOpEnum value, Type coercedType)
		{
            if ((coercedType != typeof(byte?)) &&
                (coercedType != typeof(byte?)) &&
                (coercedType != typeof(short?)) &&
                (coercedType != typeof(int?)) &&
                (coercedType != typeof(long?)) &&
                (coercedType != typeof(bool?))) 
			{
				throw new ArgumentException( "Expected base numeric or bool type for computation result but got type " + coercedType );
			}

			MultiKeyUntyped key = new MultiKeyUntyped( new Object[] { coercedType, value } );
            return Computers[key];
		}

        /// <summary>Computer for relational op.</summary>
        
		public delegate Object Computer( Object objOne, Object objTwo );

        /// <summary>
        /// Bit Wise And.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

		public static Object BAndByte( Object objOne, Object objTwo )
		{
			byte? n1 = (byte?) objOne;
			byte? n2 = (byte?) objTwo;
			byte? result = (byte?) ( n1 & n2 );
			return result;
		}

        /// <summary>
        /// Bit Wise Or.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BOrByte(Object objOne, Object objTwo)
		{
			byte? n1 = (byte?) objOne;
			byte? n2 = (byte?) objTwo;
			byte? result = (byte?) ( n1 | n2);
			return result;
		}

        /// <summary>
        /// Bit Wise Xor.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BXorByte(Object objOne, Object objTwo)
		{
			byte? n1 = (byte?) objOne;
			byte? n2 = (byte?) objTwo;
			byte? result = (byte?) ( n1 ^ n2 );
			return result;
		}

        /// <summary>
        /// Bit Wise And.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BAndShort(Object objOne, Object objTwo)
		{
			short? n1 = (short?) objOne;
			short? n2 = (short?) objTwo;
			short? result = (short?) ( n1 & n2 );
			return result;
		}

        /// <summary>
        /// Bit Wise Or.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BOrShort(Object objOne, Object objTwo)
		{
			short? n1 = (short?) objOne;
			short? n2 = (short?) objTwo;
			short? result = (short?) ( n1 | n2 );
			return result;
		}

        /// <summary>
        /// Bit Wise Xor.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        
        public static Object BXorShort(Object objOne, Object objTwo)
		{
			short? n1 = (short?) objOne;
			short? n2 = (short?) objTwo;
			short? result = (short?) ( n1 ^ n2 );
			return result;
		}

        /// <summary>
        /// Bit Wise And.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static Object BAndInt(Object objOne, Object objTwo)
		{
			int? n1 = (int?) objOne;
			int? n2 = (int?) objTwo;
			int? result = n1 & n2;
			return result;
		}

        /// <summary>
        /// Bit Wise Or.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BOrInt(Object objOne, Object objTwo)
		{
			int? n1 = (int?) objOne;
			int? n2 = (int?) objTwo;
			int? result = n1 | n2;
			return result;
		}

        /// <summary>
        /// Bit Wise Xor.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static Object BXorInt(Object objOne, Object objTwo)
		{
			int? n1 = (int?) objOne;
			int? n2 = (int?) objTwo;
			int? result = n1 ^ n2;
			return result;
		}

        /// <summary>
        /// Bit Wise And.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static Object BAndLong(Object objOne, Object objTwo)
		{
			long? n1 = (long?) objOne;
			long? n2 = (long?) objTwo;
			long? result = n1 & n2;
			return result;
		}

        /// <summary>
        /// Bit Wise Or.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BOrLong(Object objOne, Object objTwo)
		{
			long? n1 = (long?) objOne;
			long? n2 = (long?) objTwo;
			long? result = n1 | n2;
			return result;
		}

        /// <summary>
        /// Bit Wise Xor.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static Object BXorLong(Object objOne, Object objTwo)
		{
			long? n1 = (long?) objOne;
			long? n2 = (long?) objTwo;
			long? result = n1 ^ n2;
			return result;
		}

        /// <summary>
        /// Bit Wise And.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static Object BAndBoolean(Object objOne, Object objTwo)
		{
			bool? b1 = (bool?) objOne;
			bool? b2 = (bool?) objTwo;
			bool? result = b1 & b2;
			return result;
		}

        /// <summary>
        /// Bit Wise Or.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BOrBoolean(Object objOne, Object objTwo)
		{
			bool? b1 = (bool?) objOne;
			bool? b2 = (bool?) objTwo;
			bool? result = b1 | b2;
			return result;
		}

        /// <summary>
        /// Bit Wise Xor.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>

        public static Object BXorBoolean(Object objOne, Object objTwo)
		{
			bool? b1 = (bool?) objOne;
			bool? b2 = (bool?) objTwo;
			bool? result = b1 ^ b2;
			return result;
		}
	}
}
