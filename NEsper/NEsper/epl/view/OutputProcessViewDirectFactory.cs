///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Factory for output process view that does not enforce any output policies and may simply hand over events to child views, does not handle distinct.
    /// </summary>
    public class OutputProcessViewDirectFactory : OutputProcessViewFactory
    {
        protected readonly OutputStrategyPostProcessFactory PostProcessFactory;

        public OutputProcessViewDirectFactory(StatementContext statementContext, OutputStrategyPostProcessFactory postProcessFactory)
        {
            StatementContext = statementContext;
            StatementResultService = statementContext.StatementResultService;
            PostProcessFactory = postProcessFactory;
        }

        public virtual OutputProcessViewBase MakeView(ResultSetProcessor resultSetProcessor, AgentInstanceContext agentInstanceContext)
        {
            if (PostProcessFactory == null)
            {
                return new OutputProcessViewDirect(resultSetProcessor, this);
            }
            OutputStrategyPostProcess postProcess = PostProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectPostProcess(resultSetProcessor, this, postProcess);
        }

        public StatementResultService StatementResultService { get; private set; }

        public StatementContext StatementContext { get; private set; }
    }
}