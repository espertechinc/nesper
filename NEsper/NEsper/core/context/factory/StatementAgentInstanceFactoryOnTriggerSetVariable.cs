///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.epl.view;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryOnTriggerSetVariable : StatementAgentInstanceFactoryOnTriggerBase
    {
        private readonly OnSetVariableViewFactory _onSetVariableViewFactory;
        private readonly ResultSetProcessorFactoryDesc _outputResultSetProcessorPrototype;
        private readonly OutputProcessViewFactory _outputProcessViewFactory;
    
        public StatementAgentInstanceFactoryOnTriggerSetVariable(StatementContext statementContext, StatementSpecCompiled statementSpec, EPServicesContext services, ViewableActivator activator, SubSelectStrategyCollection subSelectStrategyCollection, OnSetVariableViewFactory onSetVariableViewFactory, ResultSetProcessorFactoryDesc outputResultSetProcessorPrototype, OutputProcessViewFactory outputProcessViewFactory)
            : base(statementContext, statementSpec, services, activator, subSelectStrategyCollection)
        {
            _onSetVariableViewFactory = onSetVariableViewFactory;
            _outputResultSetProcessorPrototype = outputResultSetProcessorPrototype;
            _outputProcessViewFactory = outputProcessViewFactory;
        }
    
        public override OnExprViewResult DetermineOnExprView(AgentInstanceContext agentInstanceContext, IList<StopCallback> stopCallbacks)
        {
            OnSetVariableView view = _onSetVariableViewFactory.Instantiate(agentInstanceContext);
            return new OnExprViewResult(view, null);
        }
    
        public override View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView)
        {
            ResultSetProcessor outputResultSetProcessor = _outputResultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(null, null, agentInstanceContext);
            View outputView = _outputProcessViewFactory.MakeView(outputResultSetProcessor, agentInstanceContext);
            onExprView.AddView(outputView);
            return outputView;
        }
    }
}
