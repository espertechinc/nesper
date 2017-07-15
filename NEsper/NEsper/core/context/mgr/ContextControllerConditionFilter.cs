///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerConditionFilter : ContextControllerCondition
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly ContextControllerConditionCallback _callback;
        private readonly ContextDetailConditionFilter _endpointFilterSpec;
        private readonly ContextInternalFilterAddendum _filterAddendum;
        private readonly EPServicesContext _servicesContext;

        private EPStatementHandleCallback _filterHandle;
        private FilterServiceEntry _filterServiceEntry;
        private EventBean _lastEvent;

        public ContextControllerConditionFilter(
            EPServicesContext servicesContext,
            AgentInstanceContext agentInstanceContext,
            ContextDetailConditionFilter endpointFilterSpec,
            ContextControllerConditionCallback callback,
            ContextInternalFilterAddendum filterAddendum)
        {
            _servicesContext = servicesContext;
            _agentInstanceContext = agentInstanceContext;
            _endpointFilterSpec = endpointFilterSpec;
            _callback = callback;
            _filterAddendum = filterAddendum;
        }

        public void Activate(
            EventBean optionalTriggeringEvent,
            MatchedEventMap priorMatches,
            long timeOffset,
            bool isRecoveringResilient)
        {
            var filterCallback = new ProxyFilterHandleCallback
            {
                ProcStatementId = () => { return AgentInstanceContext.StatementContext.StatementId; },
                ProcMatchFound = (theEvent, allStmtMatches) => { FilterMatchFound(theEvent); },
                ProcIsSubselect = () => { return false; }
            };

            // determine addendum, if any
            FilterValueSetParam[][] addendum = null;
            if (_filterAddendum != null)
            {
                addendum = _filterAddendum.GetFilterAddendum(_endpointFilterSpec.FilterSpecCompiled);
            }

            _filterHandle = new EPStatementHandleCallback(
                _agentInstanceContext.EpStatementAgentInstanceHandle, filterCallback);
            FilterValueSet filterValueSet = _endpointFilterSpec.FilterSpecCompiled.GetValueSet(
                null, _agentInstanceContext, addendum);
            _filterServiceEntry = _servicesContext.FilterService.Add(filterValueSet, _filterHandle);
            long filtersVersion = _servicesContext.FilterService.FiltersVersion;
            _agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;

            if (optionalTriggeringEvent != null)
            {
                bool match = StatementAgentInstanceUtil.EvaluateFilterForStatement(
                    _servicesContext, optionalTriggeringEvent, _agentInstanceContext, _filterHandle);

                if (match)
                {
                    FilterMatchFound(optionalTriggeringEvent);
                }
            }
        }

        public void Deactivate()
        {
            if (_filterHandle != null)
            {
                _servicesContext.FilterService.Remove(_filterHandle, _filterServiceEntry);
                _filterHandle = null;
                _filterServiceEntry = null;
                long filtersVersion = _agentInstanceContext.StatementContext.FilterService.FiltersVersion;
                _agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                    filtersVersion;
            }
        }

        private void FilterMatchFound(EventBean theEvent)
        {
            // For OR-type filters we de-duplicate here by keeping the last event instance
            if (_endpointFilterSpec.FilterSpecCompiled.Parameters.Length > 1)
            {
                if (theEvent == _lastEvent)
                {
                    return;
                }
                _lastEvent = theEvent;
            }
            IDictionary<string, Object> props = Collections.EmptyDataMap;
            if (_endpointFilterSpec.OptionalFilterAsName != null)
            {
                props = Collections.SingletonDataMap(_endpointFilterSpec.OptionalFilterAsName, theEvent);
            }
            _callback.RangeNotification(props, this, theEvent, null, _filterAddendum);
        }

        public bool IsRunning
        {
            get { return _filterHandle != null; }
        }

        public long? ExpectedEndTime
        {
            get { return null; }
        }

        public bool IsImmediate
        {
            get { return false; }
        }

        public ContextDetailConditionFilter EndpointFilterSpec
        {
            get { return _endpointFilterSpec; }
        }
    }
} // end of namespace