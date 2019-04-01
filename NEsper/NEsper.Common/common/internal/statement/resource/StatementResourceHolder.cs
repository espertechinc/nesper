///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.statement.resource
{
    public class StatementResourceHolder
    {
        public StatementResourceHolder(
            AgentInstanceContext agentInstanceContext, AgentInstanceStopCallback agentInstanceStopCallback,
            Viewable finalView, AggregationService aggregationService, PriorEvalStrategy[] priorEvalStrategies,
            PreviousGetterStrategy[] previousGetterStrategies, RowRecogPreviousStrategy rowRecogPreviousStrategy)
        {
            AgentInstanceContext = agentInstanceContext;
            AgentInstanceStopCallback = agentInstanceStopCallback;
            FinalView = finalView;
            AggregationService = aggregationService;
            PriorEvalStrategies = priorEvalStrategies;
            PreviousGetterStrategies = previousGetterStrategies;
            RowRecogPreviousStrategy = rowRecogPreviousStrategy;
        }

        public AgentInstanceStopCallback AgentInstanceStopCallback { get; }

        public Viewable FinalView { get; }

        public AgentInstanceContext AgentInstanceContext { get; }

        public Viewable[] TopViewables { get; set; }

        public Viewable[] EventStreamViewables { get; set; }

        public AggregationService AggregationService { get; set; }

        public NamedWindowInstance NamedWindowInstance { get; set; }

        public TableInstance TableInstance { get; set; }

        public StatementResourceExtension StatementResourceExtension { get; set; }

        public PriorEvalStrategy[] PriorEvalStrategies { get; }

        public ContextManagerRealization ContextManagerRealization { get; set; }

        public PreviousGetterStrategy[] PreviousGetterStrategies { get; }

        public RowRecogPreviousStrategy RowRecogPreviousStrategy { get; }

        public EvalRootState[] PatternRoots { get; set; }

        public IDictionary<int, SubSelectFactoryResult> SubselectStrategies { get; set; } =
            new Dictionary<int, SubSelectFactoryResult>();

        public IDictionary<int, ExprTableEvalStrategy> TableAccessStrategies { get; set; } =
            new Dictionary<int, ExprTableEvalStrategy>();
    }
} // end of namespace