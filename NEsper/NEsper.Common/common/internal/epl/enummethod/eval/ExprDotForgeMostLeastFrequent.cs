///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class ExprDotForgeMostLeastFrequent : ExprDotForgeEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            return ExprDotNodeUtility.GetSingleLambdaParamEventType(
                enumMethodUsedName, goesToNames, inputEventType, collectionComponentType, statementRawInfo, services);
        }

        public override EnumForge GetEnumForge(
            StreamTypeService streamTypeService,
            string enumMethodUsedName,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType,
            Type collectionComponentType,
            int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (bodiesAndParameters.IsEmpty()) {
                var returnTypeX = collectionComponentType.GetBoxedType();
                TypeInfo = EPTypeHelper.SingleValue(returnTypeX);
                return new EnumMostLeastFrequentScalarForge(numStreamsIncoming, EnumMethodEnum == EnumMethodEnum.MOSTFREQUENT, returnTypeX);
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            var returnType = first.BodyForge.EvaluationType.GetBoxedType();
            TypeInfo = EPTypeHelper.SingleValue(returnType);

            var mostFrequent = EnumMethodEnum == EnumMethodEnum.MOSTFREQUENT;
            if (inputEventType == null) {
                return new EnumMostLeastFrequentScalarLamdaForge(
                    first.BodyForge, first.StreamCountIncoming, mostFrequent,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }

            return new EnumMostLeastFrequentEventForge(first.BodyForge, numStreamsIncoming, mostFrequent);
        }
    }
} // end of namespace