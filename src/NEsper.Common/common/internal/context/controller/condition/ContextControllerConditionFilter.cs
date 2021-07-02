///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextControllerConditionFilter : ContextControllerConditionNonHA
    {
        private readonly IntSeqKey conditionPath;
        private readonly object[] partitionKeys;
        private readonly ContextConditionDescriptorFilter filter;
        private readonly ContextControllerConditionCallback callback;
        private readonly ContextController controller;

        private EPStatementHandleCallbackFilter filterHandle;
        private EventBean lastEvent;

        public ContextControllerConditionFilter(
            IntSeqKey conditionPath,
            object[] partitionKeys,
            ContextConditionDescriptorFilter filter,
            ContextControllerConditionCallback callback,
            ContextController controller)
        {
            this.conditionPath = conditionPath;
            this.partitionKeys = partitionKeys;
            this.filter = filter;
            this.callback = callback;
            this.controller = controller;
        }

        public bool Activate(
            EventBean optionalTriggeringEvent,
            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;

            FilterHandleCallback filterCallback = new ProxyFilterHandleCallback() {
                ProcMatchFound = (
                    theEvent,
                    allStmtMatches) => FilterMatchFound(theEvent),
                ProcIsSubselect = () => false,
            };

            filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                filterCallback);
            var filterValueSet = ComputeFilterValues(agentInstanceContext);
            if (filterValueSet != null) {
                agentInstanceContext.FilterService.Add(
                    filter.FilterSpecActivatable.FilterForEventType,
                    filterValueSet,
                    filterHandle);
                var filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
                agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
            }

            var match = false;
            if (optionalTriggeringEvent != null) {
                match = AgentInstanceUtil.EvaluateFilterForStatement(
                    optionalTriggeringEvent,
                    agentInstanceContext,
                    filterHandle);
            }

            return match;
        }

        public void Deactivate()
        {
            if (filterHandle == null) {
                return;
            }

            var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
            var filterValueSet = ComputeFilterValues(agentInstanceContext);
            if (filterValueSet != null) {

                agentInstanceContext.FilterService.Remove(
                    filterHandle,
                    filter.FilterSpecActivatable.FilterForEventType,
                    filterValueSet);
                var filtersVersion = agentInstanceContext.StatementContext.FilterService.FiltersVersion;
                agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                    filtersVersion;
            }

            filterHandle = null;
        }

        private FilterValueSetParam[][] ComputeFilterValues(AgentInstanceContext agentInstanceContext)
        {
            var addendum = ContextManagerUtil.ComputeAddendumNonStmt(
                partitionKeys,
                filter.FilterSpecActivatable,
                controller.Realization);
            return filter.FilterSpecActivatable.GetValueSet(
                null,
                addendum,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
        }

        public bool IsImmediate {
            get => false;
        }

        public bool IsRunning {
            get => filterHandle != null;
        }

        public ContextConditionDescriptor Descriptor {
            get => filter;
        }

        public long? ExpectedEndTime {
            get => null;
        }


        public void Transfer(AgentInstanceTransferServices xfer)
        {
            if (filterHandle == null) {
                return;
            }

            var filterValueSet = ComputeFilterValues(xfer.AgentInstanceContext);
            if (filterValueSet != null) {
                xfer.AgentInstanceContext.FilterService.Remove(filterHandle, filter.FilterSpecActivatable.FilterForEventType, filterValueSet);
                xfer.TargetFilterService.Add(filter.FilterSpecActivatable.FilterForEventType, filterValueSet, filterHandle);
            }
        }

        private void FilterMatchFound(EventBean theEvent)
        {
            // For OR-type filters we de-duplicate here by keeping the last event instance
            if (filter.FilterSpecActivatable.Plan.Paths.Length > 1) {
                if (theEvent == lastEvent) {
                    return;
                }

                lastEvent = theEvent;
            }

            IDictionary<string, object> terminationProperties = null;
            if (filter.OptionalFilterAsName != null) {
                terminationProperties = Collections.SingletonDataMap(filter.OptionalFilterAsName, theEvent);
            }
            
            callback.RangeNotification(conditionPath, this, theEvent, null, null, null, terminationProperties);
        }
    }
} // end of namespace