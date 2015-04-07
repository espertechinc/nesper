///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalMinMax : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            return ExprDotNodeUtility.GetSingleLambdaParamEventType(
                enumMethodUsedName, goesToNames, inputEventType, collectionComponentType,
                eventAdapterService);
        }

        public override EnumEval GetEnumEval(
            MethodResolutionService methodResolutionService,
            EventAdapterService eventAdapterService,
            StreamTypeService streamTypeService,
            String statementId,
            String enumMethodUsedName,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType,
            Type collectionComponentType,
            int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache)
        {
            bool max = this.EnumMethodEnum == EnumMethodEnum.MAX;

            Type returnType;
            if (bodiesAndParameters.IsEmpty())
            {
                returnType = collectionComponentType.GetBoxedType();
                base.TypeInfo = EPTypeHelper.SingleValue(returnType);
                return new EnumEvalMinMaxScalar(numStreamsIncoming, max);
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            returnType = first.BodyEvaluator.ReturnType.GetBoxedType();
            base.TypeInfo = EPTypeHelper.SingleValue(returnType);

            if (inputEventType == null)
            {
                return new EnumEvalMinMaxScalarLambda(
                    first.BodyEvaluator, first.StreamCountIncoming, max,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            return new EnumEvalMinMaxEvents(first.BodyEvaluator, first.StreamCountIncoming, max);
        }
    }
}