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

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorFilter : ViewableActivator
    {
        internal bool canIterate;
        internal FilterSpecActivatable filterSpec;
        internal bool isSubSelect;
        internal int? streamNumFromClause;
        internal int subselectNumber;

        public FilterSpecActivatable FilterSpec {
            get => filterSpec;
            set => filterSpec = value;
        }

        public bool CanIterate {
            get => canIterate;
            set => canIterate = value;
        }

        public int? StreamNumFromClause {
            get => streamNumFromClause;
            set => streamNumFromClause = value;
        }

        public bool IsSubSelect {
            get => isSubSelect;
            set => isSubSelect = value;
        }

        public int SubselectNumber {
            get => subselectNumber;
            set => subselectNumber = value;
        }

        public EventType EventType => filterSpec.ResultEventType;

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient)
        {
            FilterValueSetParam[][] addendum = null;
            if (agentInstanceContext.AgentInstanceFilterProxy != null) {
                addendum = agentInstanceContext.AgentInstanceFilterProxy.GetAddendumFilters(
                    filterSpec, agentInstanceContext);
            }

            FilterValueSetParam[][] filterValues = filterSpec.GetValueSet(
                null, addendum, agentInstanceContext, agentInstanceContext.StatementContextFilterEvalEnv);

            EventStream theStream;
            if (!agentInstanceContext.AuditProvider.Activated() &&
                !agentInstanceContext.InstrumentationProvider.Activated()) {
                theStream = canIterate
                    ? (EventStream) new ZeroDepthStreamIterable(filterSpec.ResultEventType)
                    : (EventStream) new ZeroDepthStreamNoIterate(filterSpec.ResultEventType);
            }
            else {
                int streamNum = streamNumFromClause ?? -1;
                theStream = canIterate
                    ? (EventStream) new ZeroDepthStreamIterableWAudit(
                        filterSpec.ResultEventType, agentInstanceContext, filterSpec, streamNum, isSubselect,
                        subselectNumber)
                    : (EventStream) new ZeroDepthStreamNoIterateWAudit(
                        filterSpec.ResultEventType, agentInstanceContext, filterSpec, streamNum, isSubselect,
                        subselectNumber);
            }

            var statementId = agentInstanceContext.StatementId;
            FilterHandleCallback filterCallback;
            if (filterSpec.OptionalPropertyEvaluator == null) {
                filterCallback = new ProxyFilterHandleCallback {
                    ProcStatementId = () => statementId,

                    ProcMatchFound = (theEvent, allStmtMatches) => theStream.Insert(theEvent),

                    ProcIsSubselect = () => isSubSelect
                };
            }
            else {
                filterCallback = new ProxyFilterHandleCallback {
                    ProcStatementId = () => statementId,

                    ProcMatchFound = (theEvent, allStmtMatches) => {
                        var result = filterSpec.OptionalPropertyEvaluator.GetProperty(theEvent, agentInstanceContext);
                        if (result == null) {
                            return;
                        }

                        theStream.Insert(result);
                    },

                    ProcIsSubselect = () => isSubSelect
                };
            }

            var filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle, filterCallback);
            agentInstanceContext.StatementContext.StatementContextRuntimeServices.FilterService.Add(
                filterSpec.FilterForEventType, filterValues, filterHandle);
            var stopCallback = new ViewableActivatorFilterStopCallback(filterHandle, filterSpec);
            return new ViewableActivationResult(theStream, stopCallback, null, false, false, null, null);
        }
    }
} // end of namespace