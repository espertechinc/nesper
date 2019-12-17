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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class ExprDotForgeToMap : ExprDotForgeEnumMethodBase
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
            // TBD: Look at this closer
            // TypeInfo = EPTypeHelper.SingleValue(typeof(IDictionary<string, object>));
            TypeInfo = EPTypeHelper.SingleValue(typeof(IDictionary<object, object>));
            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            var second = (ExprDotEvalParamLambda) bodiesAndParameters[1];
            if (inputEventType == null) {
                return new EnumToMapScalarLambdaForge(
                    first.BodyForge,
                    first.StreamCountIncoming,
                    second.BodyForge,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }

            return new EnumToMapEventsForge(first.BodyForge, first.StreamCountIncoming, second.BodyForge);
        }
    }
} // end of namespace