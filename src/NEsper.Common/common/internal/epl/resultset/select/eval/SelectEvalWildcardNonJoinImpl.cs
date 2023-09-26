///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalWildcardNonJoinImpl : SelectExprProcessor
    {
        private readonly StatementResultService statementResultService;

        public SelectEvalWildcardNonJoinImpl(StatementResultService statementResultService)
        {
            this.statementResultService = statementResultService;
        }

        public EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvalCtx)
        {
            if (isSynthesize || statementResultService.IsMakeSynthetic) {
                return eventsPerStream[0];
            }

            return null;
        }
    }
} // end of namespace