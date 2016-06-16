///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerPartitionedFilterCallback : FilterHandleCallback
    {
        private readonly AgentInstanceContext _agentInstanceContextCreateContext;
        private readonly EventPropertyGetter[] _getters;
        private readonly ContextControllerPartitionedInstanceCreateCallback _callback;
        private readonly EPStatementHandleCallback _filterHandle;
        private readonly FilterServiceEntry _filterServiceEntry;
    
        public ContextControllerPartitionedFilterCallback(EPServicesContext servicesContext, AgentInstanceContext agentInstanceContextCreateContext, ContextDetailPartitionItem partitionItem, ContextControllerPartitionedInstanceCreateCallback callback, ContextInternalFilterAddendum filterAddendum)
        {
            _agentInstanceContextCreateContext = agentInstanceContextCreateContext;
            _callback = callback;
    
            _filterHandle = new EPStatementHandleCallback(agentInstanceContextCreateContext.EpStatementAgentInstanceHandle, this);
    
            _getters = new EventPropertyGetter[partitionItem.PropertyNames.Count];
            for (int i = 0; i < partitionItem.PropertyNames.Count; i++)
            {
                var propertyName = partitionItem.PropertyNames[i];
                var getter = partitionItem.FilterSpecCompiled.FilterForEventType.GetGetter(propertyName);
                _getters[i] = getter;
            }
    
            var addendum = filterAddendum != null ? filterAddendum.GetFilterAddendum(partitionItem.FilterSpecCompiled) : null;
            var filterValueSet = partitionItem.FilterSpecCompiled.GetValueSet(null, null, addendum);

            _filterServiceEntry = servicesContext.FilterService.Add(filterValueSet, _filterHandle);
            var filtersVersion = servicesContext.FilterService.FiltersVersion;
            agentInstanceContextCreateContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;

        }
    
        public void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            Object key;
            if (_getters.Length > 1)
            {
                var keys = new Object[_getters.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                     keys[i] = _getters[i].Get(theEvent);
                }
                key = new MultiKeyUntyped(keys);
            }
            else
            {
                key = _getters[0].Get(theEvent);
            }
    
            _callback.Create(key, theEvent);
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
            long filtersVersion = _agentInstanceContextCreateContext.StatementContext.FilterService.FiltersVersion;
            _agentInstanceContextCreateContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
        }

        public EPStatementHandleCallback FilterHandle
        {
            get { return _filterHandle; }
        }
    }
}
