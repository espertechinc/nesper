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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.where {
    public class ExprDotForgeWhere : ExprDotForgeLambdaThreeForm {
        protected override EPChainableType InitAndNoParamsReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            throw new IllegalStateException();
        }

        protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
            EnumMethodEnum enumMethod,
            EPChainableType type,
            StatementCompileTimeServices services)
        {
            throw new IllegalStateException();
        }

        protected override ThreeFormInitFunction InitAndSingleParamReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            return lambda => {
                if (inputEventType != null) {
                    return EPChainableTypeHelper.CollectionOfEvents(inputEventType);
                }

                return EPChainableTypeHelper.CollectionOfSingleValue(collectionComponentType);
            };
        }

        protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                typeInfo,
                services) => new EnumWhereEvent(lambda);
        }

        protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                indexEventType,
                numParameters,
                typeInfo,
                services) => new EnumWhereEventPlus(lambda, indexEventType, numParameters);
        }

        protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                fieldType,
                numParams,
                typeInfo,
                services) => new EnumWhereScalar(lambda, fieldType, numParams);
        }
    }
} // end of namespace