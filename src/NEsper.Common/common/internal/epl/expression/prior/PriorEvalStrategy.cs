///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.prior
{
    /// <summary>
    /// Represents the 'prior' prior event resolution strategy for use in an expression node tree.
    /// </summary>
    public interface PriorEvalStrategy
    {
        EventBean GetSubstituteEvent(
            EventBean originalEvent,
            bool isNewData,
            int constantIndexNumber,
            int relativeIndex,
            ExprEvaluatorContext exprEvaluatorContext,
            int streamNum);
    }

    public static class PriorEvalStrategyConstants
    {
        public static readonly PriorEvalStrategy[] EMPTY_ARRAY = new PriorEvalStrategy[0];
    }
} // end of namespace