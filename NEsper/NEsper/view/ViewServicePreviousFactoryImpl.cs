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
using com.espertech.esper.view.window;

namespace com.espertech.esper.view
{
    public class ViewServicePreviousFactoryImpl : ViewServicePreviousFactory
    {
        public ViewUpdatedCollection GetOptPreviousExprRandomAccess(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamRandomAccess randomAccess = null;
            if (agentInstanceViewFactoryContext.PreviousNodeGetter != null)
            {
                var getter = (RandomAccessByIndexGetter) agentInstanceViewFactoryContext.PreviousNodeGetter;
                randomAccess = new IStreamRandomAccess(getter);
                getter.Updated(randomAccess);
            }
            return randomAccess;
        }

        public ViewUpdatedCollection GetOptPreviousExprRelativeAccess(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamRelativeAccess relativeAccessByEvent = null;
            if (agentInstanceViewFactoryContext.PreviousNodeGetter != null)
            {
                var getter = (RelativeAccessByEventNIndexGetter) agentInstanceViewFactoryContext.PreviousNodeGetter;
                var observer = (IStreamRelativeAccessUpdateObserver) getter;
                relativeAccessByEvent = new IStreamRelativeAccess(observer);
                observer.Updated(relativeAccessByEvent, null);
            }

            return relativeAccessByEvent;
        }

        public IStreamSortRankRandomAccess GetOptPreviousExprSortedRankedAccess(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamSortRankRandomAccess rankedRandomAccess = null;
            if (agentInstanceViewFactoryContext.PreviousNodeGetter != null)
            {
                var getter = (RandomAccessByIndexGetter) agentInstanceViewFactoryContext.PreviousNodeGetter;
                rankedRandomAccess = new IStreamSortRankRandomAccessImpl(getter);
                getter.Updated(rankedRandomAccess);
            }

            return rankedRandomAccess;
        }
    }
} // end of namespace