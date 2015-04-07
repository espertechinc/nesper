///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        public ContextControllerFactoryContext(String outermostContextName,
                                               String contextName,
                                               EPServicesContext servicesContext,
                                               AgentInstanceContext agentInstanceContextCreate,
                                               int nestingLevel,
                                               bool isRecoveringResilient)
        {
            OutermostContextName = outermostContextName;
            ContextName = contextName;
            ServicesContext = servicesContext;
            AgentInstanceContextCreate = agentInstanceContextCreate;
            NestingLevel = nestingLevel;
            IsRecoveringResilient = isRecoveringResilient;
        }

        public string OutermostContextName { get; private set; }

        public string ContextName { get; private set; }

        public EPServicesContext ServicesContext { get; private set; }

        public AgentInstanceContext AgentInstanceContextCreate { get; private set; }

        public int NestingLevel { get; private set; }

        public bool IsRecoveringResilient { get; private set; }
    }
}