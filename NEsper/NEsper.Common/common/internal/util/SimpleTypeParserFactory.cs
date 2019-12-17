///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.util;

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
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseChar", input)
                };
            }

            if (typeBoxed == typeof(bool?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseBoolean(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseBoolean", input)
                };
            }

            if (typeBoxed == typeof(Guid?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseGuid(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseGuid", input)
                };
            }

            // -- Integers
            if (typeBoxed == typeof(byte?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseByte(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseByte", input)
                };
            }

            if (typeBoxed == typeof(sbyte?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseSByte(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseSByte", input)
                };
            }

            if (typeBoxed == typeof(short?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseInt16(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseInt16", input)
                };
            }

            if (typeBoxed == typeof(ushort?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseUInt16(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseUInt16", input)
                };
            }

            if (typeBoxed == typeof(int?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseInt32(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseInt32", input)
                };
            }

            if (typeBoxed == typeof(uint?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseUInt32(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseUInt32", input)
                };
            }

            if (typeBoxed == typeof(long?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseInt64(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseInt64", input)
                };
            }

            if (typeBoxed == typeof(ulong?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseUInt64(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseUInt64", input)
                };
            }

            // -- Floating Point
            if (typeBoxed == typeof(float?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseFloat(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseFloat", input)
                };
            }

            if (typeBoxed == typeof(double?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseDouble(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseDouble", input)
                };
            }

            if (typeBoxed == typeof(decimal?)) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => SimpleTypeParserFunctions.ParseDecimal(value),
                    ProcCodegen = input => StaticMethod(typeof(SimpleTypeParserFunctions), "ParseDecimal", input)
                };
            }

            var untyped = Nullable.GetUnderlyingType(typeBoxed);
            if (untyped != null && untyped.IsEnum) {
                return new ProxySimpleTypeParserSPI {
                    ProcParse = value => Enum.Parse(untyped, value, true),
                    ProcCodegen = input => StaticMethod(
                        typeof(SimpleTypeParserFactory),
                        "ParseEnum",
                        input,
                        Clazz(untyped))
                };
            }

            return null;
        }

        public static CodegenExpression CodegenSimpleParser(
            SimpleTypeParserSPI parser,
            CodegenMethod method,
            Type originator,
            CodegenClassScope classScope)
        {
            var parse = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("value");
            var typeParser = NewInstance<ProxySimpleTypeParser>(parse);

            //var anonymousClass =
            //    NewAnonymousClass(method.Block, typeof(SimpleTypeParser));
            //var parse = CodegenMethod
            //    .MakeParentNode<object>(originator, classScope)
            //    .AddParam<string>("value");
            //anonymousClass.AddMethod("Parse", parse);

            parse.Block.BlockReturn(parser.Codegen(Ref("value")));
            return typeParser;
        }
    }
}