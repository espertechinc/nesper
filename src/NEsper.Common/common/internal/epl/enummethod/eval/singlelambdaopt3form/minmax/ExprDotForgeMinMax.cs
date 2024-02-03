///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmax {
    public class ExprDotForgeMinMax : ExprDotForgeLambdaThreeForm {
        protected override EPChainableType InitAndNoParamsReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            var returnType = collectionComponentType.GetBoxedType();
            return new EPChainableTypeClass(returnType);
        }

        protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
            EnumMethodEnum enumMethod,
            EPChainableType type,
            StatementCompileTimeServices services)
        {
            return streamCountIncoming => new EnumMinMaxScalarNoParam(
                streamCountIncoming,
                enumMethod == EnumMethodEnum.MAX,
                type);
        }

        protected override ThreeFormInitFunction InitAndSingleParamReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            return lambda => {
                var returnType = ValidateNonNull(lambda.BodyForge.EvaluationType).GetBoxedType();
                return EPChainableTypeHelper.SingleValue(returnType);
            };
        }

        protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                typeInfo,
                services) => new EnumMinMaxEvent(lambda, enumMethod == EnumMethodEnum.MAX);
        }

        protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                fieldType,
                numParameters,
                typeInfo,
                services) => new EnumMinMaxEventPlus(
                lambda,
                fieldType,
                numParameters,
                enumMethod == EnumMethodEnum.MAX);
        }

        protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                eventType,
                numParams,
                typeInfo,
                services) => new EnumMinMaxScalar(lambda, eventType, numParams, enumMethod == EnumMethodEnum.MAX);
        }
    }
} // end of namespace