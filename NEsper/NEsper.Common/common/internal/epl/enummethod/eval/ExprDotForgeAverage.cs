///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class ExprDotForgeAverage : ExprDotForgeEnumMethodBase
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
                enumMethodUsedName,
                goesToNames,
                inputEventType,
                collectionComponentType,
                statementRawInfo,
                services);
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
                if (collectionComponentType.IsDecimal() || collectionComponentType.IsBigInteger()) {
                    TypeInfo = EPTypeHelper.SingleValue(typeof(decimal));
                    return new EnumAverageDecimalScalarForge(
                        numStreamsIncoming,
                        services.ImportServiceCompileTime.DefaultMathContext);
                }

                TypeInfo = EPTypeHelper.SingleValue(typeof(double?));
                return new EnumAverageScalarForge(numStreamsIncoming);
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            var returnType = first.BodyForge.EvaluationType;

            if (returnType == typeof(decimal) || returnType.IsBigInteger()) {
                TypeInfo = EPTypeHelper.SingleValue(typeof(decimal));
                if (inputEventType == null) {
                    return new EnumAverageDecimalScalarLambdaForge(
                        first.BodyForge,
                        first.StreamCountIncoming,
                        (ObjectArrayEventType) first.GoesToTypes[0],
                        services.ImportServiceCompileTime.DefaultMathContext);
                }

                return new EnumAverageDecimalEventsForge(
                    first.BodyForge,
                    first.StreamCountIncoming,
                    services.ImportServiceCompileTime.DefaultMathContext);
            }

            TypeInfo = EPTypeHelper.SingleValue(typeof(double?));
            if (inputEventType == null) {
                return new EnumAverageScalarLambdaForge(
                    first.BodyForge,
                    first.StreamCountIncoming,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }

            return new EnumAverageEventsForge(first.BodyForge, first.StreamCountIncoming);
        }
    }
} // end of namespace