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
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalTakeWhileAndLast : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            EventType firstParamType;
            if (inputEventType == null)
            {
                firstParamType = ExprDotNodeUtility.MakeTransientOAType(
                    enumMethodUsedName, goesToNames[0], collectionComponentType, eventAdapterService);
            }
            else
            {
                firstParamType = inputEventType;
            }

            if (goesToNames.Count == 1)
            {
                return new EventType[]
                {
                    firstParamType
                };
            }

            ObjectArrayEventType indexEventType = ExprDotNodeUtility.MakeTransientOAType(
                enumMethodUsedName, goesToNames[1], typeof(int), eventAdapterService);
            return new EventType[]
            {
                firstParamType,
                indexEventType
            };
        }

        public override EnumEval GetEnumEval(MethodResolutionService methodResolutionService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];

            if (inputEventType != null)
            {
                base.TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
                if (first.GoesToNames.Count == 1)
                {
                    if (EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST)
                    {
                        return new EnumEvalTakeWhileLastEvents(first.BodyEvaluator, first.StreamCountIncoming);
                    }
                    return new EnumEvalTakeWhileEvents(first.BodyEvaluator, first.StreamCountIncoming);
                }

                if (EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST)
                {
                    return new EnumEvalTakeWhileLastIndexEvents(
                        first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[1]);
                }
                return new EnumEvalTakeWhileIndexEvents(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[1]);
            }

            base.TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
            if (first.GoesToNames.Count == 1)
            {
                if (EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST)
                {
                    return new EnumEvalTakeWhileLastScalar(
                        first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
                }
                return new EnumEvalTakeWhileScalar(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
            }

            if (EnumMethodEnum == EnumMethodEnum.TAKEWHILELAST)
            {
                return new EnumEvalTakeWhileLastIndexScalar(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0],
                    (ObjectArrayEventType) first.GoesToTypes[1]);
            }
            return new EnumEvalTakeWhileIndexScalar(
                first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0],
                (ObjectArrayEventType) first.GoesToTypes[1]);
        }
    }
}