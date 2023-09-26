///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof
{
    public class ExprDotForgeDistinctOf : ExprDotForgeLambdaThreeForm
    {
        protected override EPChainableType InitAndNoParamsReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            return EPChainableTypeHelper.CollectionOfSingleValue(collectionComponentType);
        }

        protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
            EnumMethodEnum enumMethod,
            EPChainableType type,
            StatementCompileTimeServices services)
        {
            return streamCountIncoming => {
                var classInfo = (EPChainableTypeClass)type;
                Type component = classInfo.Clazz.GetComponentType();
                return new EnumDistinctOfScalarNoParams(streamCountIncoming, component);
            };
            //return streamCountIncoming => new EnumDistinctOfScalarNoParams(streamCountIncoming, ((ClassMultiValuedEPType) type).Component);
        }

        protected override ThreeFormInitFunction InitAndSingleParamReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            return lambda => inputEventType == null
                ? EPChainableTypeHelper.CollectionOfSingleValue(collectionComponentType)
                : EPChainableTypeHelper.CollectionOfEvents(inputEventType);
        }

        protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                typeInfo,
                services) => new EnumDistinctOfEvent(lambda);
        }

        protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                fieldType,
                numParameters,
                typeInfo,
                services) => new EnumDistinctOfEventPlus(lambda, fieldType, numParameters);
        }

        protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                eventType,
                numParams,
                typeInfo,
                services) => new EnumDistinctOfScalar(lambda, eventType, numParams);
        }
    }
} // end of namespace