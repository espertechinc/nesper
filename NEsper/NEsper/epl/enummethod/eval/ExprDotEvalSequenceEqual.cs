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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class ExprDotEvalSequenceEqual : ExprDotEvalEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventAdapterService eventAdapterService)
        {
            return new EventType[0];
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
            base.TypeInfo = EPTypeHelper.SingleValue(typeof(Boolean));
            ExprEvaluator body = bodiesAndParameters[0].BodyEvaluator;
            return new EnumEvalSequenceEqual(body, numStreamsIncoming);
        }
    }
}