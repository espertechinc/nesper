///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorStreamReuseView 
        : ViewableActivator
        , StopCallback
    {
        private readonly EPServicesContext _services;
        private readonly StatementContext _statementContext;
        private readonly StatementSpecCompiled _statementSpec;
        private readonly FilterStreamSpecCompiled _filterStreamSpec;
        private readonly bool _join;
        private readonly ExprEvaluatorContextStatement _evaluatorContextStmt;
        private readonly bool _filterSubselectSameStream;
        private readonly int _streamNum;
        private readonly bool _isCanIterateUnbound;

        internal ViewableActivatorStreamReuseView(EPServicesContext services, StatementContext statementContext, StatementSpecCompiled statementSpec, FilterStreamSpecCompiled filterStreamSpec, bool join, ExprEvaluatorContextStatement evaluatorContextStmt, bool filterSubselectSameStream, int streamNum, bool isCanIterateUnbound)
        {
            _services = services;
            _statementContext = statementContext;
            _statementSpec = statementSpec;
            _filterStreamSpec = filterStreamSpec;
            _join = join;
            _evaluatorContextStmt = evaluatorContextStmt;
            _filterSubselectSameStream = filterSubselectSameStream;
            _streamNum = streamNum;
            _isCanIterateUnbound = isCanIterateUnbound;
        }
    
        public ViewableActivationResult Activate(AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient)
        {
            var pair = _services.StreamService.CreateStream(
                _statementContext.StatementId, _filterStreamSpec.FilterSpec,
                _statementContext.FilterService,
                agentInstanceContext.EpStatementAgentInstanceHandle,
                _join,
                agentInstanceContext,
                _statementSpec.OrderByList.Length > 0,
                _filterSubselectSameStream,
                _statementContext.Annotations,
                _statementContext.IsStatelessSelect,
                _streamNum,
                _isCanIterateUnbound);
            return new ViewableActivationResult(pair.First, this, pair.Second, null, null, false, false, null);
        }
    
        public void Stop()
        {
            _services.StreamService.DropStream(_filterStreamSpec.FilterSpec, _statementContext.FilterService, _join, _statementSpec.OrderByList.Length > 0, _filterSubselectSameStream, _statementContext.IsStatelessSelect);
        }

        public FilterStreamSpecCompiled FilterStreamSpec
        {
            get { return _filterStreamSpec; }
        }
    }
}
