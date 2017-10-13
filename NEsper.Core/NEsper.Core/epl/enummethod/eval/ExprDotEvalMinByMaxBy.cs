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
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalMinByMaxBy : ExprDotEvalEnumMethodBase
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
            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];

            var max = EnumMethodEnum == EnumMethodEnum.MAXBY;
            if (inputEventType == null)
            {
                base.TypeInfo = EPTypeHelper.SingleValue(collectionComponentType);
                return new EnumEvalMinMaxByScalarLambda(
                    first.BodyEvaluator, first.StreamCountIncoming, max,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            base.TypeInfo = EPTypeHelper.SingleEvent(inputEventType);
            return new EnumEvalMinMaxByEvents(first.BodyEvaluator, first.StreamCountIncoming, max);
        }
    }
}