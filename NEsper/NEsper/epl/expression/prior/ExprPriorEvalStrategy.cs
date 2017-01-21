///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.prior
{
    /// <summary>Represents the 'prior' prior event resolution strategy for use in an expression node tree. </summary>
    public interface ExprPriorEvalStrategy
    {
        Object Evaluate(EventBean[] eventsPerStream,
                        bool isNewData,
                        ExprEvaluatorContext exprEvaluatorContext,
                        int streamNumber,
                        ExprEvaluator evaluator,
                        int constantIndexNumber);
    }
}
