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
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalAggregate : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            EventType evalEventType;
            if (inputEventType == null)
            {
                evalEventType = ExprDotNodeUtility.MakeTransientOAType(
                    enumMethodUsedName, goesToNames[1], collectionComponentType, eventAdapterService);
            }
            else
            {
                evalEventType = inputEventType;
            }

            Type initializationType = bodiesAndParameters[0].BodyEvaluator.ReturnType;
            EventType typeResult = ExprDotNodeUtility.MakeTransientOAType(
                enumMethodUsedName, goesToNames[0], initializationType, eventAdapterService);

            return new EventType[]
            {
                typeResult,
                evalEventType
            };
        }

        public override EnumEval GetEnumEval(EngineImportService engineImportService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            ExprDotEvalParam initValueParam = bodiesAndParameters[0];
            ExprEvaluator initValueEval = initValueParam.BodyEvaluator;
            base.TypeInfo = EPTypeHelper.SingleValue(initValueEval.ReturnType.GetBoxedType());

            var resultAndAdd = (ExprDotEvalParamLambda) bodiesAndParameters[1];

            if (inputEventType != null)
            {
                return new EnumEvalAggregateEvents(
                    initValueEval,
                    resultAndAdd.BodyEvaluator, resultAndAdd.StreamCountIncoming,
                    (ObjectArrayEventType) resultAndAdd.GoesToTypes[0]);
            }
            else
            {
                return new EnumEvalAggregateScalar(
                    initValueEval,
                    resultAndAdd.BodyEvaluator, resultAndAdd.StreamCountIncoming,
                    (ObjectArrayEventType) resultAndAdd.GoesToTypes[0],
                    (ObjectArrayEventType) resultAndAdd.GoesToTypes[1]);
            }
        }
    }
}