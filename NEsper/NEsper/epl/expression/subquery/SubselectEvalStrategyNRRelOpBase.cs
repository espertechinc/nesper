///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.subquery
{
    using RelationalComputer = Func<object, object, bool>;

    public abstract class SubselectEvalStrategyNRRelOpBase : SubselectEvalStrategyNRBase
    {
        protected readonly RelationalComputer Computer;

        protected SubselectEvalStrategyNRRelOpBase(ExprEvaluator valueEval, ExprEvaluator selectEval, bool resultWhenNoMatchingEvents, RelationalComputer computer)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents)
        {
            Computer = computer;
        }
    }
} // end of namespace
