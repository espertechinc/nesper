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
    public class ExprDotEvalWhere : ExprDotEvalEnumMethodBase
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
                enumMethodUsedName, goesToNames[1], typeof (int), eventAdapterService);
            return new EventType[]
            {
                firstParamType,
                indexEventType
            };
        }

        public override EnumEval GetEnumEval(EngineImportService engineImportService, EventAdapterService eventAdapterService, StreamTypeService streamTypeService, int statementId, string enumMethodUsedName, IList<ExprDotEvalParam> bodiesAndParameters, EventType inputEventType, Type collectionComponentType, int numStreamsIncoming, bool disablePropertyExpressionEventCollCache)
        {
            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];

            if (inputEventType != null)
            {
                TypeInfo = EPTypeHelper.CollectionOfEvents(inputEventType);
                if (first.GoesToNames.Count == 1)
                {
                    return new EnumEvalWhereEvents(first.BodyEvaluator, first.StreamCountIncoming);
                }
                return new EnumEvalWhereIndexEvents(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[1]);
            }

            TypeInfo = EPTypeHelper.CollectionOfSingleValue(collectionComponentType);
            if (first.GoesToNames.Count == 1)
            {
                return new EnumEvalWhereScalar(
                    first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0]);
            }
            return new EnumEvalWhereScalarIndex(
                first.BodyEvaluator, first.StreamCountIncoming, (ObjectArrayEventType) first.GoesToTypes[0],
                (ObjectArrayEventType) first.GoesToTypes[1]);
        }
    }
}