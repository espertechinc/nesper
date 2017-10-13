///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

namespace com.espertech.esper.util
{
    /// <summary>
    /// A factory for creating an instance of a parser that parses a String and returns a target type.
    /// </summary>
    public class SimpleTypeParserFactory
    {
        /// <summary>
        /// Returns a parsers for the String value using the given built-in class for parsing.
        /// </summary>
        /// <param name="type">is the type to parse the value to</param>
        /// <returns>value matching the type passed in</returns>
        public static SimpleTypeParser GetParser(Type type)
        {
            Type typeBoxed = TypeHelper.GetBoxedType(type);

            if (typeBoxed == typeof(string))
            {
                return value => value;
            }
            if (typeBoxed == typeof(char?))
            {
                return value => value[0];
            }
            if (typeBoxed == typeof(bool?))
            {
                return value => Boolean.Parse(value);
            }
            if (typeBoxed == typeof(Guid?))
            {
                return value => new Guid(value);
            }

            // -- Integers
            if (typeBoxed == typeof(byte?))
            {
                return delegate(String value)
                {
                    value = value.Trim();
                    if (value.StartsWith("0x"))
                    {
                        return Byte.Parse(value.Substring(2), NumberStyles.HexNumber);
                    }

                    return Byte.Parse(value.Trim());
                };
            }
            if (typeBoxed == typeof(sbyte?))
            {
                return value => SByte.Parse(value.Trim());
            }
            if (typeBoxed == typeof(short?))
            {
                return value => Int16.Parse(value);
            }
            if (typeBoxed == typeof(ushort?))
            {
                return value => UInt16.Parse(value);
            }
            if (typeBoxed == typeof(int?))
            {
                return value => Int32.Parse(value);
            }
            if (typeBoxed == typeof(uint?))
            {
                return value => UInt32.Parse(value);
            }
            if (typeBoxed == typeof(long?))
            {
                return delegate(String value)
                {
                    value = value.TrimEnd('l', 'L', ' ');
                    return Int64.Parse(value);
                };
            }
            if (typeBoxed == typeof(ulong?))
            {
                return delegate(String value)
                {
                    value = value.TrimEnd('l', 'L', ' ');
                    return UInt64.Parse(value);
                };
            }

            // -- Floating Point
            if (typeBoxed == typeof(float?))
            {
                return delegate(String value)
                {
                    value = value.TrimEnd('f', 'F', ' ');
                    return Single.Parse(value);
                };
            }
            if (typeBoxed == typeof(double?))
            {
                return delegate(String value)
                {
                    value = value.TrimEnd('f', 'F', 'd', 'D', ' ');
                    return Double.Parse(value);
                };
            }
            if (typeBoxed == typeof(decimal?))
            {
                return delegate(String value)
                {
                    value = value.TrimEnd('f', 'F', 'd', 'D', 'M', 'm', ' ');
                    return Decimal.Parse(value);
                };
            }

            var untyped = Nullable.GetUnderlyingType(typeBoxed);
            if ((untyped != null) && untyped.IsEnum)
            {
                return value => Enum.Parse(untyped, value, true);
            }

            return null;
        }
    }
}
