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
        private readonly EPServicesContext _servicesContext;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly ContextDetailConditionFilter _endpointFilterSpec;
        private readonly ContextControllerConditionCallback _callback;
        private readonly ContextInternalFilterAddendum _filterAddendum;

        private EPStatementHandleCallback _filterHandle;
        private FilterServiceEntry _filterServiceEntry;

        public ContextControllerConditionFilter(EPServicesContext servicesContext, AgentInstanceContext agentInstanceContext, ContextDetailConditionFilter endpointFilterSpec, ContextControllerConditionCallback callback, ContextInternalFilterAddendum filterAddendum)
        {
            _servicesContext = servicesContext;
            _agentInstanceContext = agentInstanceContext;
            _endpointFilterSpec = endpointFilterSpec;
            _callback = callback;
            _filterAddendum = filterAddendum;
        }

        public void Activate(EventBean optionalTriggeringEvent, MatchedEventMap priorMatches, long timeOffset, bool isRecoveringResilient)
        {
            FilterHandleCallback filterCallback = new ProxyFilterHandleCallback
            {
                ProcStatementId = () => _agentInstanceContext.StatementContext.StatementId,
                ProcIsSubselect = () => false,
                ProcMatchFound = (theEvent, allStmtMatches) => FilterMatchFound(theEvent)
            };

            // determine addendum, if any
            FilterValueSetParam[][] addendum = null;
            if (_filterAddendum != null)
            {
                addendum = _filterAddendum.GetFilterAddendum(_endpointFilterSpec.FilterSpecCompiled);
            }

            _filterHandle = new EPStatementHandleCallback(_agentInstanceContext.EpStatementAgentInstanceHandle, filterCallback);
            FilterValueSet filterValueSet = _endpointFilterSpec.FilterSpecCompiled.GetValueSet(null, _agentInstanceContext, addendum);
            _filterServiceEntry = _servicesContext.FilterService.Add(filterValueSet, _filterHandle);

            if (optionalTriggeringEvent != null)
            {
                bool match = StatementAgentInstanceUtil.EvaluateFilterForStatement(_servicesContext, optionalTriggeringEvent, _agentInstanceContext, _filterHandle);

                if (match)
                {
                    FilterMatchFound(optionalTriggeringEvent);
                }
            }
        }

        private void FilterMatchFound(EventBean theEvent)
        {
            IDictionary<String, Object> props = Collections.GetEmptyMap<string, object>();
            if (_endpointFilterSpec.OptionalFilterAsName != null)
            {
                props = Collections.SingletonDataMap(_endpointFilterSpec.OptionalFilterAsName, theEvent);
            }
            _callback.RangeNotification(props, this, theEvent, null, _filterAddendum);
        }

        public void Deactivate()
        {
            if (_filterHandle != null)
            {
                _servicesContext.FilterService.Remove(_filterHandle, _filterServiceEntry);
                _filterHandle = null;
                _filterServiceEntry = null;
                long filtersVersion = _agentInstanceContext.StatementContext.FilterService.FiltersVersion;
                _agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
            }
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
}
