///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorFilter : ViewableActivator
    {
        public FilterSpecActivatable FilterSpec { get; set; }

        public bool CanIterate { get; set; }

        public int? StreamNumFromClause { get; set; }

        public bool IsSubSelect { get; set; }

        public int SubselectNumber { get; set; }

        public EventType EventType => FilterSpec.ResultEventType;

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            FilterValueSetParam[][] addendum = null;
            if (agentInstanceContext.AgentInstanceFilterProxy != null) {
                addendum = agentInstanceContext.AgentInstanceFilterProxy.GetAddendumFilters(
                    FilterSpec,
                    agentInstanceContext);
            }

            var filterValues = FilterSpec.GetValueSet(
                null,
                addendum,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);

            EventStream theStream;
            if (filterValues == null) {
                theStream = new ZeroDepthStreamNoIterate(FilterSpec.ResultEventType);
            }
            else {
                if (!agentInstanceContext.AuditProvider.Activated() &&
                    !agentInstanceContext.InstrumentationProvider.Activated()) {
                    theStream = CanIterate
                        ? new ZeroDepthStreamIterable(FilterSpec.ResultEventType)
                        : (EventStream)new ZeroDepthStreamNoIterate(FilterSpec.ResultEventType);
                }
                else {
                    var streamNum = StreamNumFromClause ?? -1;
                    theStream = CanIterate
                        ? new ZeroDepthStreamIterableWAudit(
                            FilterSpec.ResultEventType,
                            agentInstanceContext,
                            FilterSpec,
                            streamNum,
                            isSubselect,
                            SubselectNumber)
                        : (EventStream)new ZeroDepthStreamNoIterateWAudit(
                            FilterSpec.ResultEventType,
                            agentInstanceContext,
                            FilterSpec,
                            streamNum,
                            isSubselect,
                            SubselectNumber);
                }
            }

            FilterHandleCallback filterCallback;
            if (FilterSpec.OptionalPropertyEvaluator == null) {
                filterCallback = new ProxyFilterHandleCallback {
                    ProcMatchFound = (
                        theEvent,
                        allStmtMatches) => theStream.Insert(theEvent),

                    ProcIsSubselect = () => IsSubSelect
                };
            }
            else {
                filterCallback = new ProxyFilterHandleCallback {
                    ProcMatchFound = (
                        theEvent,
                        allStmtMatches) => {
                        var result = FilterSpec.OptionalPropertyEvaluator.GetProperty(theEvent, agentInstanceContext);
                        if (result == null) {
                            return;
                        }

                        theStream.Insert(result);
                    },

                    ProcIsSubselect = () => IsSubSelect
                };
            }

            var filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                filterCallback);
            if (filterValues != null) {
                agentInstanceContext
                    .StatementContext
                    .FilterService
                    .Add(
                        FilterSpec.FilterForEventType,
                        filterValues,
                        filterHandle);
            }

            var stopCallback = new ViewableActivatorFilterMgmtCallback(
                agentInstanceContext.StatementContext.Container,
                filterHandle,
                FilterSpec);
            
            return new ViewableActivationResult(theStream, stopCallback, null, false, false, null, null, null);
        }
    }
} // end of namespace