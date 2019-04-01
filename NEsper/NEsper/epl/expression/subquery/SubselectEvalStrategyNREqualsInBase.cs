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
    /// <summary>Represents a in-subselect evaluation strategy.</summary>
    public abstract class SubselectEvalStrategyNREqualsInBase : SubselectEvalStrategyNRBase
    {
        protected readonly Coercer Coercer;
        protected readonly bool IsNotIn;

        protected SubselectEvalStrategyNREqualsInBase(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool notIn,
            Coercer coercer)
            : base(valueEval, selectEval, notIn)
        {
            IsNotIn = notIn;
            Coercer = coercer;
        }
    }
} // end of namespace