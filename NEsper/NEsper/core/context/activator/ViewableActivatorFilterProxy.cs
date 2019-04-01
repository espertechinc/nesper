///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;
using com.espertech.esper.view.stream;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorFilterProxy : ViewableActivator
    {
        private readonly EPServicesContext _services;
        private readonly FilterSpecCompiled _filterSpec;
        private readonly Attribute[] _annotations;
        private readonly bool _isSubSelect;
        private readonly InstrumentationAgent _instrumentationAgent;
        private readonly bool _isCanIterate;

        internal ViewableActivatorFilterProxy(EPServicesContext services, FilterSpecCompiled filterSpec, Attribute[] annotations, bool subSelect, InstrumentationAgent instrumentationAgent, bool isCanIterate)
        {
            _services = services;
            _filterSpec = filterSpec;
            _annotations = annotations;
            _isSubSelect = subSelect;
            _instrumentationAgent = instrumentationAgent;
            _isCanIterate = isCanIterate;
        }

        public ViewableActivationResult Activate(AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient)
        {
            // New event stream
            EventType resultEventType = _filterSpec.ResultEventType;
            EventStream zeroDepthStream = _isCanIterate ?
                (EventStream)new ZeroDepthStreamIterable(resultEventType) :
                (EventStream)new ZeroDepthStreamNoIterate(resultEventType);

            // audit proxy
            var inputStream = EventStreamProxy.GetAuditProxy(agentInstanceContext.StatementContext.EngineURI, agentInstanceContext.EpStatementAgentInstanceHandle.StatementHandle.StatementName, _annotations, _filterSpec, zeroDepthStream);

            var eventStream = inputStream;
            var statementId = agentInstanceContext.StatementContext.StatementId;

            var filterCallback = new ProxyFilterHandleCallback
            {
                ProcStatementId = () => statementId,
                ProcIsSubselect = () => _isSubSelect
            };

            if (_filterSpec.OptionalPropertyEvaluator != null)
            {
                filterCallback.ProcMatchFound = (theEvent, allStmtMatches) =>
                {
                    var result = _filterSpec.OptionalPropertyEvaluator.GetProperty(theEvent, agentInstanceContext);
                    if (result != null)
                    {
                        eventStream.Insert(result);
                    }
                };
            }
            else
            {
                filterCallback.ProcMatchFound = (theEvent, allStmtMatches) =>
                {
                    if (InstrumentationHelper.ENABLED) { _instrumentationAgent.IndicateQ(); }
                    eventStream.Insert(theEvent);
                    if (InstrumentationHelper.ENABLED) { _instrumentationAgent.IndicateA(); }
                };
            }

            var filterHandle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, filterCallback);

            FilterValueSetParam[][] addendum = null;
            if (agentInstanceContext.AgentInstanceFilterProxy != null)
            {
                addendum = agentInstanceContext.AgentInstanceFilterProxy.GetAddendumFilters(_filterSpec);
            }
            var filterValueSet = _filterSpec.GetValueSet(null, agentInstanceContext, addendum);
            var filterServiceEntry = _services.FilterService.Add(filterValueSet, filterHandle);

            var stopCallback = new ViewableActivatorFilterProxyStopCallback(this, filterHandle, filterServiceEntry);
            return new ViewableActivationResult(inputStream, stopCallback, null, null, null, false, false, null);
        }

        public EPServicesContext Services
        {
            get { return _services; }
        }

        public FilterSpecCompiled FilterSpec
        {
            get { return _filterSpec; }
        }
    }
}
