///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.view.ext;

namespace com.espertech.esper.view
{
    public interface ViewServicePreviousFactory
    {
        ViewUpdatedCollection GetOptPreviousExprRandomAccess(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);

        ViewUpdatedCollection GetOptPreviousExprRelativeAccess(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);

        IStreamSortRankRandomAccess GetOptPreviousExprSortedRankedAccess(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);
    }
} // end of namespace