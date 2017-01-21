///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.pattern.guard.TimerWithinGuard" /> instances. 
    /// </summary>
    [Serializable]
    public class ExpressionGuardFactory 
        : GuardFactory
        , MetaDefItem
    {
        protected ExprNode Expression;
    
        /// <summary>For converting matched-events maps to events-per-stream. </summary>
        [NonSerialized] protected MatchedEventConvertor Convertor;
    
        public void SetGuardParameters(IList<ExprNode> paramList, MatchedEventConvertor convertor)
        {
            const string errorMessage = "Expression pattern guard requires a single expression as a parameter returning a true or false (bool) value";
            if (paramList.Count != 1)
            {
                throw new GuardParameterException(errorMessage);
            }
            Expression = paramList[0];
    
            if (paramList[0].ExprEvaluator.ReturnType.GetBoxedType() != typeof(bool?))
            {
                throw new GuardParameterException(errorMessage);
            }
    
            Convertor = convertor;
        }

        public Guard MakeGuard(PatternAgentInstanceContext context, MatchedEventMap beginState, Quitable quitable, EvalStateNodeNumber stateNodeId, Object guardState)
        {
            return new ExpressionGuard(Convertor, Expression.ExprEvaluator, quitable);
        }
    }
}
