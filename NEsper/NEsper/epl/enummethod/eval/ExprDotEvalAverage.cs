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
    public class ExprDotEvalAverage : ExprDotEvalEnumMethodBase
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


        public override EnumEval GetEnumEval(MethodResolutionService methodResolutionService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            if (bodiesAndParameters.IsEmpty())
            {
                if (collectionComponentType == typeof (decimal?))
                {
                    TypeInfo = EPTypeHelper.SingleValue(typeof(decimal?));
                    return new EnumEvalAverageDecimalScalar(
                        numStreamsIncoming, methodResolutionService.EngineImportService.DefaultMathContext);
                }
                TypeInfo = EPTypeHelper.SingleValue(typeof(double?));
                return new EnumEvalAverageScalar(numStreamsIncoming);
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            var returnType = first.BodyEvaluator.ReturnType.GetBoxedType();

            if (returnType == typeof (decimal?))
            {
                TypeInfo = EPTypeHelper.SingleValue(typeof(decimal?));
                if (inputEventType == null)
                {
                    return new EnumEvalAverageDecimalScalarLambda(
                        first.BodyEvaluator, first.StreamCountIncoming,
                        (ObjectArrayEventType) first.GoesToTypes[0],
                        methodResolutionService.EngineImportService.DefaultMathContext);
                }
                return new EnumEvalAverageDecimalEvents(
                    first.BodyEvaluator, first.StreamCountIncoming,
                    methodResolutionService.EngineImportService.DefaultMathContext);
            }
            TypeInfo = EPTypeHelper.SingleValue(typeof(double?));
            if (inputEventType == null)
            {
                return new EnumEvalAverageScalarLambda(
                    first.BodyEvaluator, first.StreamCountIncoming,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            return new EnumEvalAverageEvents(first.BodyEvaluator, first.StreamCountIncoming);
        }
    }
}