///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

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
            ProcCast = CastHelper.PrimitiveCastInt16,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastInt16", input)
        };

        public static readonly SimpleTypeCaster Int32TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastInt32,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastInt32", input)
        };

        public static readonly SimpleTypeCaster Int64TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastInt64,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastInt64", input)
        };

        public static readonly SimpleTypeCaster UInt16TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastUInt16,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastUInt16", input)
        };

        public static readonly SimpleTypeCaster UInt32TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastUInt32,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastUInt32", input)
        };

        public static readonly SimpleTypeCaster UInt64TypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastUInt64,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastUInt64", input)
        };

        public static readonly SimpleTypeCaster SingleTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastSingle,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastSingle", input)
        };

        public static readonly SimpleTypeCaster DoubleTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastDouble,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastDouble", input)
        };

        public static readonly SimpleTypeCaster DecimalTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastDecimal,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastDecimal", input)
        };

        public static readonly SimpleTypeCaster CharTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastChar,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastChar", input)
        };

        public static readonly SimpleTypeCaster ByteTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastByte,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastByte", input)
        };

        public static readonly SimpleTypeCaster SByteTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastSByte,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastSByte", input)
        };

        public static readonly SimpleTypeCaster BigIntegerTypeCaster = new ProxyTypeCaster {
            IsNumericCast = true,
            ProcCast = CastHelper.PrimitiveCastBigInteger,
            ProcCodegenInput = input => CodegenExpressionBuilder
                .StaticMethod(typeof(CastHelper), "PrimitiveCastBigInteger", input)
        };

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

            var baseType = Nullable.GetUnderlyingType(targetType);
            if (baseType != null) {
                targetType = baseType;
            }

            if (targetType == typeof(int)) {
                return Int32TypeCaster;
            }

            if (targetType == typeof(long)) {
                return Int64TypeCaster;
            }

            if (targetType == typeof(short)) {
                return Int16TypeCaster;
            }

            if (targetType == typeof(sbyte)) {
                return SByteTypeCaster;
            }

            if (targetType == typeof(float)) {
                return SingleTypeCaster;
            }

            if (targetType == typeof(double)) {
                return DoubleTypeCaster;
            }

            if (targetType == typeof(decimal)) {
                return DecimalTypeCaster;
            }

            if (targetType == typeof(uint)) {
                return UInt32TypeCaster;
            }

            if (targetType == typeof(ulong)) {
                return UInt64TypeCaster;
            }

            if (targetType == typeof(ushort)) {
                return UInt16TypeCaster;
            }

            if (targetType == typeof(char)) {
                return CharTypeCaster;
            }

            if (targetType == typeof(byte)) {
                return ByteTypeCaster;
            }

            if (targetType.IsBigInteger()) {
                return BigIntegerTypeCaster;
            }

            isNumeric = false;

            if (targetType == typeof(bool?)) {
                return BooleanTypeCaster;
            }

            if (targetType == typeof(string)) {
                return new ProxyTypeCaster {
                    IsNumericCast = false,
                    ProcCast = sourceObj => {
                        if (sourceObj == null) {
                            return null;
                        }

                        if (sourceObj is string) {
                            return (string) sourceObj;
                        }

                        return sourceObj.ToString();
                    },
                    ProcCodegen = (
                        input,
                        inputType,
                        methodScope,
                        classScope) => throw new NotImplementedException()
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
                    classScope) => throw new NotImplementedException()
            };
        }
    }
}