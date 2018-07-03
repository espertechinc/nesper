///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Factory for output process view that does not enforce any output policies and may simply
    /// hand over events to child views, does not handle distinct.
    /// </summary>
    public class OutputProcessViewDirectFactory : OutputProcessViewFactory
    {
        private readonly StatementContext _statementContext;
        private readonly StatementResultService _statementResultService;
        protected readonly OutputStrategyPostProcessFactory PostProcessFactory;
        protected readonly ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory;

        public OutputProcessViewDirectFactory(
            StatementContext statementContext,
            OutputStrategyPostProcessFactory postProcessFactory,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory)
        {
            _statementContext = statementContext;
            _statementResultService = statementContext.StatementResultService;
            PostProcessFactory = postProcessFactory;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
        }

        public virtual OutputProcessViewBase MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            if (PostProcessFactory == null)
            {
                return new OutputProcessViewDirect(resultSetProcessor, this);
            }
            OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectPostProcess(resultSetProcessor, this, postProcess);
        }

        public StatementResultService StatementResultService => _statementResultService;

        public StatementContext StatementContext => _statementContext;
    }
} // end of namespace
