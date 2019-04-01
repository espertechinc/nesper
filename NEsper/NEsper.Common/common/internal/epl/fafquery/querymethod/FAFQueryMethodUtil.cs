///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.fafquery.processor;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodUtil
    {
        protected internal static ICollection<int> AgentInstanceIds(
            FireAndForgetProcessor processor,
            ContextPartitionSelector optionalSelector,
            ContextManagementService contextManagementService)
        {
            var contextManager = contextManagementService.GetContextManager(
                processor.ContextDeploymentId, processor.ContextName);
            return contextManager.Realization.GetAgentInstanceIds(
                optionalSelector == null ? ContextPartitionSelectorAll.INSTANCE : optionalSelector);
        }

        public static EPException RuntimeDestroyed()
        {
            return new EPException("Runtime has already been destroyed");
        }
    }
} // end of namespace