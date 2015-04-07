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
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerFactoryServiceContext
    {
        public ContextControllerFactoryServiceContext(String contextName,
                                                      EPServicesContext servicesContext,
                                                      ContextDetail detail,
                                                      AgentInstanceContext agentInstanceContextCreate,
                                                      bool isRecoveringResilient)
        {
            ContextName = contextName;
            ServicesContext = servicesContext;
            Detail = detail;
            AgentInstanceContextCreate = agentInstanceContextCreate;
            IsRecoveringResilient = isRecoveringResilient;
        }

        public string ContextName { get; private set; }

        public EPServicesContext ServicesContext { get; private set; }

        public ContextDetail Detail { get; private set; }

        public AgentInstanceContext AgentInstanceContextCreate { get; private set; }

        public bool IsRecoveringResilient { get; private set; }
    }
}