///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Strategy for subselects with "=/!=/&gt;&lt; ALL".</summary>
    public abstract class SubselectEvalStrategyNREqualsBase : SubselectEvalStrategyNRBase
    {
        protected readonly bool IsNot;
        protected readonly Coercer Coercer;

        protected SubselectEvalStrategyNREqualsBase(ExprEvaluator valueEval, ExprEvaluator selectEval, bool resultWhenNoMatchingEvents, bool notIn, Coercer coercer)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents)
        {
            IsNot = notIn;
            Coercer = coercer;
        }
    }
} // end of namespace
