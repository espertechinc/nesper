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
    public class ExprDotEvalPropertyExprIndexed : ExprDotEvalPropertyExprBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly EventPropertyGetterIndexed _indexedGetter;
    
        public ExprDotEvalPropertyExprIndexed(string statementName, string propertyName, int streamNum, ExprEvaluator exprEvaluator, Type propertyType, EventPropertyGetterIndexed indexedGetter)
            : base(statementName, propertyName, streamNum, exprEvaluator, propertyType)
        {
            _indexedGetter = indexedGetter;
        }


        public override object Evaluate(EvaluateParams evaluateParams)
        {
            var eventInQuestion = evaluateParams.EventsPerStream[base.StreamNum];
            if (eventInQuestion == null)
            {
                return null;
            }
            var index = base.ExprEvaluator.Evaluate(evaluateParams);
            if (index == null || !index.IsInt())
            {
                Log.Warn(base.GetWarningText("integer", index));
                return null;
            }
            return _indexedGetter.Get(eventInQuestion, index.AsInt());
        }
    }
} // end of namespace
