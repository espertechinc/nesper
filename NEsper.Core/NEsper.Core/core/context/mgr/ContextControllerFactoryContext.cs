///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerFactoryContext
    {
        public ContextControllerFactoryContext(
            String outermostContextName,
            String contextName,
            EPServicesContext servicesContext,
            AgentInstanceContext agentInstanceContextCreate,
            int nestingLevel,
            int numNestingLevels,
            bool isRecoveringResilient,
            ContextStateCache stateCache)
        {
            OutermostContextName = outermostContextName;
            ContextName = contextName;
            ServicesContext = servicesContext;
            AgentInstanceContextCreate = agentInstanceContextCreate;
            NestingLevel = nestingLevel;
            NumNestingLevels = numNestingLevels;
            IsRecoveringResilient = isRecoveringResilient;
            StateCache = stateCache;
        }

        public string OutermostContextName { get; private set; }

        public string ContextName { get; private set; }

        public EPServicesContext ServicesContext { get; private set; }

        public AgentInstanceContext AgentInstanceContextCreate { get; private set; }

        public int NestingLevel { get; private set; }

        public int NumNestingLevels { get; private set; }

        public bool IsRecoveringResilient { get; private set; }

        public ContextStateCache StateCache { get; private set; }
    }
}