///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Factory for output process view that does not enforce any output policies and may simply
    ///     hand over events to child views, does not handle distinct.
    /// </summary>
    public class OutputProcessViewDirectFactory : OutputProcessViewFactory
    {
        protected internal OutputStrategyPostProcessFactory postProcessFactory;

        public OutputProcessViewDirectFactory()
        {
        }

        public OutputProcessViewDirectFactory(OutputStrategyPostProcessFactory postProcessFactory)
        {
            this.postProcessFactory = postProcessFactory;
        }

        public OutputStrategyPostProcessFactory PostProcessFactory {
            get => postProcessFactory;
            set => postProcessFactory = value;
        }

        public virtual OutputProcessView MakeView(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            var postProcess = postProcessFactory.Make(agentInstanceContext);
            return new OutputProcessViewDirectPostProcess(agentInstanceContext, resultSetProcessor, postProcess);
        }
    }
} // end of namespace