///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class StatementAgentInstanceFactoryResult
    {
        private readonly IList<StatementAgentInstancePreload> preloadList;

        protected StatementAgentInstanceFactoryResult(
            Viewable finalView,
            AgentInstanceStopCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            AggregationService optionalAggegationService,
            IDictionary<int, SubSelectFactoryResult> subselectStrategies,
            PriorEvalStrategy[] priorStrategies,
            PreviousGetterStrategy[] previousGetterStrategies,
            RowRecogPreviousStrategy rowRecogPreviousStrategy,
            IDictionary<int, ExprTableEvalStrategy> tableAccessStrategies,
            IList<StatementAgentInstancePreload> preloadList)
        {
            FinalView = finalView;
            StopCallback = stopCallback;
            AgentInstanceContext = agentInstanceContext;
            OptionalAggegationService = optionalAggegationService;
            SubselectStrategies = subselectStrategies;
            PriorStrategies = priorStrategies;
            PreviousGetterStrategies = previousGetterStrategies;
            RowRecogPreviousStrategy = rowRecogPreviousStrategy;
            TableAccessStrategies = tableAccessStrategies;
            this.preloadList = preloadList;
        }

        public Viewable FinalView { get; }

        public AgentInstanceStopCallback StopCallback { get; set; }

        public AgentInstanceContext AgentInstanceContext { get; }

        public AggregationService OptionalAggegationService { get; }

        public PriorEvalStrategy[] PriorStrategies { get; }

        public PreviousGetterStrategy[] PreviousGetterStrategies { get; }

        public ICollection<StatementAgentInstancePreload> PreloadList => preloadList;

        public RowRecogPreviousStrategy RowRecogPreviousStrategy { get; }

        public IDictionary<int, SubSelectFactoryResult> SubselectStrategies { get; }

        public IDictionary<int, ExprTableEvalStrategy> TableAccessStrategies { get; }
    }
} // end of namespace