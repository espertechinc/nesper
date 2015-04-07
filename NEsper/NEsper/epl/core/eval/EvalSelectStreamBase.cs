///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalSelectStreamBase : SelectExprProcessor
    {
        protected readonly IList<SelectClauseStreamCompiledSpec> NamedStreams;
        protected readonly bool IsUsingWildcard;

        protected EvalSelectStreamBase(SelectExprContext selectExprContext, EventType resultEventType, IList<SelectClauseStreamCompiledSpec> namedStreams, bool usingWildcard)
        {
            SelectExprContext = selectExprContext;
            ResultEventType = resultEventType;
            NamedStreams = namedStreams;
            IsUsingWildcard = usingWildcard;
        }

        public EventType ResultEventType { get; protected set; }

        public SelectExprContext SelectExprContext { get; protected set; }

        public abstract EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext);
    }
}