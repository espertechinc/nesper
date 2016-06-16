///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHashedFilterCallback : FilterHandleCallback
    {
        private readonly AgentInstanceContext _agentInstanceContextCreateContext;
        private readonly EventPropertyGetter _getter;
        private readonly ContextControllerHashedInstanceCallback _callback;
        private readonly EPStatementHandleCallback _filterHandle;
        private readonly FilterServiceEntry _filterServiceEntry;

        public ContextControllerHashedFilterCallback(
            EPServicesContext servicesContext,
            AgentInstanceContext agentInstanceContextCreateContext,
            ContextDetailHashItem hashItem,
            ContextControllerHashedInstanceCallback callback,
            ContextInternalFilterAddendum filterAddendum)
        {
            _agentInstanceContextCreateContext = agentInstanceContextCreateContext;
            _callback = callback;
            _getter = hashItem.Lookupable.Getter;
    
            _filterHandle = new EPStatementHandleCallback(agentInstanceContextCreateContext.EpStatementAgentInstanceHandle, this);
    
            FilterValueSetParam[][] addendum = filterAddendum != null ? filterAddendum.GetFilterAddendum(hashItem.FilterSpecCompiled) : null;
            FilterValueSet filterValueSet = hashItem.FilterSpecCompiled.GetValueSet(null, null, addendum);
            _filterServiceEntry = servicesContext.FilterService.Add(filterValueSet, _filterHandle);

            long filtersVersion = servicesContext.FilterService.FiltersVersion;
            agentInstanceContextCreateContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
        }
    
        public void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            int value = _getter.Get(theEvent).AsInt();
            _callback.Create(value, theEvent);
        }

        public bool IsSubSelect
        {
            get { return false; }
        }

        public int StatementId
        {
            get { return _agentInstanceContextCreateContext.StatementContext.StatementId; }
        }

        public void Destroy(FilterService filterService)
        {
            filterService.Remove(_filterHandle, _filterServiceEntry);
            var filtersVersion = _agentInstanceContextCreateContext.StatementContext.FilterService.FiltersVersion;
            _agentInstanceContextCreateContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
        }

        public EPStatementHandleCallback FilterHandle
        {
            get { return _filterHandle; }
        }
    }
}
