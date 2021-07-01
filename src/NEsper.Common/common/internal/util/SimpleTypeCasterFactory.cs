///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.util;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Factory for casters, which take an object and safely cast to a given type,
    ///     performing coercion or dropping precision if required.
    /// </summary>
    public class SimpleTypeCasterFactory
    {
        public static readonly SimpleTypeCaster Int16TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastInt16(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastInt16", input)
        };

        public static readonly SimpleTypeCaster Int32TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastInt32(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastInt32", input)
        };

        public static readonly SimpleTypeCaster Int64TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastInt64(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastInt64", input)
        };

        public static readonly SimpleTypeCaster UInt16TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastUInt16(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastUInt16", input)
        };

        public static readonly SimpleTypeCaster UInt32TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastUInt32(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastUInt32", input)
        };

        public static readonly SimpleTypeCaster UInt64TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastUInt64(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastUInt64", input)
        };

        public static readonly SimpleTypeCaster SingleTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastSingle(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastSingle", input)
        };

        public static readonly SimpleTypeCaster DoubleTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastDouble(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastDouble", input)
        };

        public static readonly SimpleTypeCaster DecimalTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastDecimal(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastDecimal", input)
        };

        public static readonly SimpleTypeCaster CharTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastChar(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastChar", input)
        };

        public static readonly SimpleTypeCaster ByteTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastByte(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastByte", input)
        };

        public static readonly SimpleTypeCaster SByteTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastSByte(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastSByte", input)
        };

        public static readonly SimpleTypeCaster BigIntegerTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastBigInteger(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastBigInteger", input)
        };
        
        // -----
        
        public static readonly SimpleTypeCaster NullableInt16TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableInt16(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableInt16", input)
        };

        public static readonly SimpleTypeCaster NullableInt32TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableInt32(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableInt32", input)
        };

        public static readonly SimpleTypeCaster NullableInt64TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableInt64(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableInt64", input)
        };

        public static readonly SimpleTypeCaster NullableUInt16TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableUInt16(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableUInt16", input)
        };

        public static readonly SimpleTypeCaster NullableUInt32TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableUInt32(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableUInt32", input)
        };

        public static readonly SimpleTypeCaster NullableUInt64TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableUInt64(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableUInt64", input)
        };

        public static readonly SimpleTypeCaster NullableSingleTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableSingle(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableSingle", input)
        };

        public static readonly SimpleTypeCaster NullableDoubleTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableDouble(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableDouble", input)
        };

        public static readonly SimpleTypeCaster NullableDecimalTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableDecimal(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableDecimal", input)
        };

        public static readonly SimpleTypeCaster NullableCharTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableChar(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableChar", input)
        };

        public static readonly SimpleTypeCaster NullableByteTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableByte(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableByte", input)
        };

        public static readonly SimpleTypeCaster NullableSByteTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableSByte(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableSByte", input)
        };

        public static readonly SimpleTypeCaster NullableBigIntegerTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = v => CastHelper.CastNullableBigInteger(v),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "CastNullableBigInteger", input)
        };
        
        // -----

        public static readonly SimpleTypeCaster BooleanTypeCaster = new ProxyTypeCaster {
            IsNumericCast = false,
            ProcCast = sourceObj => sourceObj == null ? (object) null : Convert.ToBoolean(sourceObj),
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(Convert), "ToBoolean", input)
        };

        public static readonly SimpleTypeCaster IdentityTypeCaster = new ProxyTypeCaster {
            IsNumericCast = false,
            ProcCast = value => value,
            ProcCodegenInput = input => input
        };
        
        public static readonly SimpleTypeCaster StringTypeCaster = new ProxyTypeCaster {
            IsNumericCast = false,
            ProcCast = sourceObj => {
                if (sourceObj == null) {
                    return null;
                }

                if (sourceObj is string stringValue) {
                    return stringValue;
                }

                return sourceObj.ToString();
            },
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(Convert), "ToString", input)
        };

        public static readonly IDictionary<Type, SimpleTypeCaster> TypeToTypeCaster =
            new Dictionary<Type, SimpleTypeCaster> {
                [typeof(string)] = StringTypeCaster,
                [typeof(int?)] = NullableInt32TypeCaster,
                [typeof(long?)] = NullableInt64TypeCaster,
                [typeof(short?)] = NullableInt16TypeCaster,
                [typeof(sbyte?)] = NullableSByteTypeCaster,
                [typeof(float?)] = NullableSingleTypeCaster,
                [typeof(double?)] = NullableDoubleTypeCaster,
                [typeof(decimal?)] = NullableDecimalTypeCaster,
                [typeof(uint?)] = NullableUInt32TypeCaster,
                [typeof(ulong?)] = NullableUInt64TypeCaster,
                [typeof(ushort?)] = NullableUInt16TypeCaster,
                [typeof(char?)] = NullableCharTypeCaster,
                [typeof(byte?)] = NullableByteTypeCaster,
                [typeof(BigInteger?)] = NullableBigIntegerTypeCaster,
                [typeof(int)] = Int32TypeCaster,
                [typeof(long)] = Int64TypeCaster,
                [typeof(short)] = Int16TypeCaster,
                [typeof(sbyte)] = SByteTypeCaster,
                [typeof(float)] = SingleTypeCaster,
                [typeof(double)] = DoubleTypeCaster,
                [typeof(decimal)] = DecimalTypeCaster,
                [typeof(uint)] = UInt32TypeCaster,
                [typeof(ulong)] = UInt64TypeCaster,
                [typeof(ushort)] = UInt16TypeCaster,
                [typeof(char)] = CharTypeCaster,
                [typeof(byte)] = ByteTypeCaster,
                [typeof(BigInteger)] = BigIntegerTypeCaster,
                [typeof(bool)] = BooleanTypeCaster
            };

        /// <summary>
        ///     Returns a caster that casts to a target type.
        /// </summary>
        /// <param name="fromType">can be null, if not known</param>
        /// <param name="targetType">to cast to</param>
        /// <returns>
        ///     caster for casting objects to the required type
        /// </returns>
        public static SimpleTypeCaster GetCaster(
            Type fromType,
            Type targetType)
        {
            if (fromType == targetType) {
                return IdentityTypeCaster;
            }

            bool isUnused;
            return GetCaster(targetType, out isUnused);
        }

        /// <summary>
        ///     Returns a caster that casts to a target type.
        /// </summary>
        /// <param name="targetType">to cast to</param>
        /// <returns>
        ///     caster for casting objects to the required type
        /// </returns>
        public static SimpleTypeCaster GetCaster(Type targetType)
        {
            bool isUnused;
            return GetCaster(targetType, out isUnused);
        }

        /// <summary>
        ///     Returns a caster that casts to a target type.
        /// </summary>
        /// <param name="targetType">to cast to</param>
        /// <param name="isNumeric">if set to <c>true</c> [is numeric].</param>
        /// <returns>
        ///     caster for casting objects to the required type
        /// </returns>
        public static SimpleTypeCaster GetCaster(
            Type targetType,
            out bool isNumeric)
        {
            isNumeric = true;

            if (TypeToTypeCaster.TryGetValue(targetType, out var targetTypeCaster)) {
                isNumeric = targetTypeCaster.IsNumericCast;
                return targetTypeCaster;
            }

            isNumeric = false;

            if (targetType.IsArray) {
                var arrayElementType = targetType.GetElementType();
                return new ProxyTypeCaster {
                    IsNumericCast = false,
                    ProcCast = sourceObj => CompatExtensions.UnwrapIntoArray(targetType, sourceObj),
                    ProcCodegenInput = input => CodegenExpressionBuilder
                        .StaticMethod(
                            typeof(CompatExtensions),
                            "UnwrapIntoArray",
                            new [] { arrayElementType },
                            input,
                            new CodegenExpressionConstant(true))
                };
            }

            if (targetType.IsEnum) {
                return new ProxyTypeCaster {
                    IsNumericCast = false,
                    ProcCast = v => CastHelper.CastEnum(targetType, v),
                    ProcCodegenInput = input => CodegenExpressionBuilder
                        .StaticMethod(typeof(CastHelper), "CastEnum", new [] { targetType }, input)
                };
            }
            
            return new ProxyTypeCaster {
                IsNumericCast = false,
                ProcCast = sourceObj => {
                    if (sourceObj == null) {
                        return null;
                    }

                    var sourceObjType = sourceObj.GetType();
                    return targetType.IsAssignableFrom(sourceObjType) ? sourceObj : null;
                },
                ProcCodegen = (
                    input,
                    inputType,
                    methodScope,
                    classScope) => CodegenExpressionBuilder.Cast(targetType, input)
            };
        }
    }
}