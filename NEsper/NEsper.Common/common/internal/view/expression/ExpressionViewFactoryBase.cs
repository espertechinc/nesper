///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    ///     Base factory for expression-based window and batch view.
    /// </summary>
    public abstract class ExpressionViewFactoryBase : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        public EventType EventType { get; set; }

        public EventType BuiltinMapType { get; set; }

        public ExprEvaluator ExpiryEval { get; set; }

        public int ScheduleCallbackId { get; set; }

        public AggregationServiceFactory AggregationServiceFactory { get; set; }

        public AggregationResultFutureAssignableWEval AggregationResultFutureAssignable { get; set; }

        public Variable[] Variables { get; set; }

        public void Init(ViewFactoryContext viewFactoryContext, EPStatementInitServices services)
        {
        }

        public abstract string ViewName { get; }
        public abstract View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);
        public abstract PreviousGetterStrategy MakePreviousGetter();
    }
} // end of namespace