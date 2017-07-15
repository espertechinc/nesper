///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEvalPropertyExprMapped : ExprDotEvalPropertyExprBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly EventPropertyGetterMapped _mappedGetter;
    
        public ExprDotEvalPropertyExprMapped(string statementName, string propertyName, int streamNum, ExprEvaluator exprEvaluator, Type propertyType, EventPropertyGetterMapped mappedGetter)
            :  base(statementName, propertyName, streamNum, exprEvaluator, propertyType)
        {
            this._mappedGetter = mappedGetter;
        }

        public override object Evaluate(EvaluateParams evaluateParams)
        {
            var eventInQuestion = evaluateParams.EventsPerStream[base.StreamNum];
            if (eventInQuestion == null)
            {
                return null;
            }
            var result = base.ExprEvaluator.Evaluate(evaluateParams);
            if (result != null && (!(result is string)))
            {
                Log.Warn(base.GetWarningText("string", result));
                return null;
            }
            return _mappedGetter.Get(eventInQuestion, (string) result);
        }
    }
} // end of namespace
