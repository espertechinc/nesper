///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Record holding lookup resource references for use by <seealso cref="SubSelectActivationCollection"/>.
    /// </summary>
    public interface SubSelectStrategyFactory
    {
        SubSelectStrategyRealization Instantiate(
            EPServicesContext services,
            Viewable viewableRoot,
            AgentInstanceContext agentInstanceContext,
            IList<StopCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient);
    }
}
