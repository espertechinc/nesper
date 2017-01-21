///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.type
{
	/// <summary>
    /// Factory class for PrimitiveValue for all fundamental types.
    /// </summary>
	
    public sealed class PrimitiveValueFactory
	{
        /// <summary>
        /// Create a placeholder instance for the primitive type passed in.
        /// Returns null if the type passed in is not a primitive type.
        /// </summary>
        /// <param name="type">a fundamental type</param>
        /// <returns>
        /// instance of placeholder representing the type, or null if not a primitive type
        /// </returns>
	
        public static PrimitiveValue Create(Type type)
		{
			if ((type == typeof(bool)) || type == typeof(bool?))
			{
				return new BoolValue();
			}
			if ((type == typeof(byte)) || (type == typeof(byte?)))
			{
				return new ByteValue();
			}
            if ((type == typeof(sbyte)) || (type == typeof(sbyte?)))
            {
                return new SByteValue();
            }
            if ((type == typeof(decimal)) || (type == typeof(decimal?)))
            {
                return new DecimalValue();
            }
			if ((type == typeof(double)) || (type == typeof(double?)))
			{
				return new DoubleValue();
			}
			if ((type == typeof(float)) || (type == typeof(float?)))
			{
				return new FloatValue();
			}
			if ((type == typeof(int)) || (type == typeof(int?)))
			{
				return new IntValue();
			}
			if ((type == typeof(long)) || (type == typeof(long?)))
			{
				return new LongValue();
			}
			if ((type == typeof(short)) || (type == typeof(short?)))
			{
				return new ShortValue();
			}
			if (type == typeof(String))
			{
				return new StringValue();
			}
			
			return null;
		}
	}
}
