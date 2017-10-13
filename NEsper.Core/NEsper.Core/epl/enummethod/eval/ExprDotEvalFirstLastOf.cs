///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalFirstLastOf : ExprDotEvalEnumMethodBase
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


        public override EnumEval GetEnumEval(EngineImportService engineImportService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            if (bodiesAndParameters.IsEmpty())
            {
                if (inputEventType != null)
                {
                    base.TypeInfo = EPTypeHelper.SingleEvent(inputEventType);
                }
                else
                {
                    base.TypeInfo = EPTypeHelper.SingleValue(collectionComponentType);
                }
                if (EnumMethodEnum == EnumMethodEnum.FIRST)
                {
                    return new EnumEvalFirstOfNoPredicate(numStreamsIncoming);
                }
                else
                {
                    return new EnumEvalLastOfNoPredicate(numStreamsIncoming);
                }
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            if (inputEventType != null)
            {
                base.TypeInfo = EPTypeHelper.SingleEvent(inputEventType);
                if (EnumMethodEnum == EnumMethodEnum.FIRST)
                {
                    return new EnumEvalFirstOfPredicateEvents(first.BodyEvaluator, first.StreamCountIncoming);
                }
                else
                {
                    return new EnumEvalLastOfPredicateEvents(first.BodyEvaluator, first.StreamCountIncoming);
                }
            }
            base.TypeInfo = EPTypeHelper.SingleValue(collectionComponentType);
            if (EnumMethodEnum == EnumMethodEnum.FIRST)
            {
                return new EnumEvalFirstOfPredicateScalar(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            else
            {
                return new EnumEvalLastOfPredicateScalar(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
            }
        }
    }
}