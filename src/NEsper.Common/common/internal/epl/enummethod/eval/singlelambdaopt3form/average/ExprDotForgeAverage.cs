///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;


namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.average {
    public class ExprDotForgeAverage : ExprDotForgeLambdaThreeForm {
        protected override EPChainableType InitAndNoParamsReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            if (collectionComponentType.IsTypeBigInteger()) {
                return EPChainableTypeHelper.SingleValue(typeof(BigInteger?));
            }
            else if (collectionComponentType.IsTypeDecimal()) {
                return EPChainableTypeHelper.SingleValue(typeof(decimal?));
            }
            else {
                return EPChainableTypeHelper.SingleValue(typeof(double?));
            }
        }

        protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
            EnumMethodEnum enumMethod,
            EPChainableType type,
            StatementCompileTimeServices services)
        {
            var normalizedType = type.GetNormalizedType();
            if (normalizedType.IsTypeBigInteger()) {
                return streamCountIncoming => new EnumAverageBigIntegerScalarNoParam(streamCountIncoming);
                // services.ImportServiceCompileTime.DefaultMathContext
            }
            else if (normalizedType.IsTypeDecimal()) {
                return streamCountIncoming => new EnumAverageDecimalScalarNoParam(
                    streamCountIncoming);
            }
            else if (normalizedType.IsTypeDouble()) {
                return streamCountIncoming => new EnumAverageDoubleScalarNoParam(
                    streamCountIncoming);
            }
            else {
                throw new ArgumentException("Failed to find a suitable scalar no-param");
            }
        }

        protected override ThreeFormInitFunction InitAndSingleParamReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            return lambda => {
                var returnType = lambda.BodyForge.EvaluationType;
                if (returnType.IsTypeBigInteger()) {
                    return EPChainableTypeHelper.SingleValue(typeof(BigInteger?));
                }
                else if (returnType.IsTypeDecimal()) {
                    return EPChainableTypeHelper.SingleValue(typeof(decimal?));
                }
                else {
                    return EPChainableTypeHelper.SingleValue(typeof(double?));
                }
            };
        }

        protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                typeInfo,
                services) => {
                if (typeInfo.GetNormalizedType().IsTypeBigInteger()) {
                    return new EnumAverageBigIntegerEvent(lambda);
                    // services.ImportServiceCompileTime.DefaultMathContext
                }
                else if (typeInfo.GetNormalizedType().IsTypeDecimal()) {
                    return new EnumAverageDecimalEvent(lambda);
                }
                else if (typeInfo.GetNormalizedType().IsTypeDouble()) {
                    return new EnumAverageDoubleEvent(lambda);
                }
                else {
                    throw new ArgumentException("Failed to find a suitable event");
                }
            };
        }

        protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                fieldType,
                numParameters,
                typeInfo,
                services) => {
                if (typeInfo.GetNormalizedType().IsTypeBigInteger()) {
                    return new EnumAverageBigIntegerEventPlus(lambda, fieldType, numParameters);
                }
                else if (typeInfo.GetNormalizedType().IsTypeDecimal()) {
                    return new EnumAverageDecimalEventPlus(lambda, fieldType, numParameters);
                }
                else if (typeInfo.GetNormalizedType().IsTypeDouble()) {
                    return new EnumAverageDoubleEventPlus(lambda, fieldType, numParameters);
                }
                else {
                    throw new ArgumentException("Failed to find a suitable event");
                }
            };
        }

        protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                fieldType,
                numParams,
                typeInfo,
                services) => {
                if (typeInfo.GetNormalizedType().IsTypeBigInteger()) {
                    return new EnumAverageBigIntegerScalar(lambda, fieldType, numParams);
                }
                else if (typeInfo.GetNormalizedType().IsTypeDecimal()) {
                    return new EnumAverageDecimalScalar(lambda, fieldType, numParams);
                }
                else if (typeInfo.GetNormalizedType().IsTypeDouble()) {
                    return new EnumAverageDoubleScalar(lambda, fieldType, numParams);
                }
                else {
                    throw new ArgumentException("Failed to find a suitable scalar");
                }
            };
        }
    }
} // end of namespace