///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     A factory for creating an instance of a parser that parses a String and returns a target type.
    /// </summary>
    public class SimpleTypeParserFactory
    {
        /// <summary>
        ///     Returns a parsers for the String value using the given built-in class for parsing.
        /// </summary>
        /// <param name="type">is the type to parse the value to</param>
        /// <returns>value matching the type passed in</returns>
        public static SimpleTypeParserSPI GetParser(Type type)
        {
            var typeBoxed = type.GetBoxedType();

            if (typeBoxed == typeof(string)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => value,
                    ProcCodegen = input => input
                };
            }

            if (typeBoxed == typeof(char?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseChar(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseChar", input)
                };
            }

            if (typeBoxed == typeof(bool?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseBoolean(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseBoolean", input)
                };
            }

            if (typeBoxed == typeof(Guid?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseGuid(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseGuid", input)
                };
            }

            // -- Integers
            if (typeBoxed == typeof(byte?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseByte(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseByte", input)
                };
            }

            if (typeBoxed == typeof(sbyte?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseSByte(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseSByte", input)
                };
            }

            if (typeBoxed == typeof(short?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseInt16(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseShort", input)
                };
            }

            if (typeBoxed == typeof(ushort?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseUInt16(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseUShort", input)
                };
            }

            if (typeBoxed == typeof(int?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseInt32(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseInt", input)
                };
            }

            if (typeBoxed == typeof(uint?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseUInt32(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseUInt", input)
                };
            }

            if (typeBoxed == typeof(long?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseInt64(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseLong", input)
                };
            }

            if (typeBoxed == typeof(ulong?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseUInt64(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseULong", input)
                };
            }

            // -- Floating Point
            if (typeBoxed == typeof(float?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseFloat(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseFloat", input)
                };
            }

            if (typeBoxed == typeof(double?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseDouble(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseDouble", input)
                };
            }

            if (typeBoxed == typeof(decimal?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseDecimal(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFactory), "ParseDecimal", input)
                };
            }

            var untyped = Nullable.GetUnderlyingType(typeBoxed);
            if (untyped != null && untyped.IsEnum) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => Enum.Parse(untyped, value, true),
                    ProcCodegen = input => StaticMethod(
                        typeof(SimpleTypeParserFactory), "ParseEnum", input,
                        Clazz(untyped))
                };
            }

            return null;
        }
    }
}