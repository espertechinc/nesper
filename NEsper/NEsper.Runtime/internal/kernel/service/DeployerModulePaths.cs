///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.script.core;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerModulePaths
    {
        public DeployerModulePaths(
            IDictionary<long, EventType> deploymentTypes,
            IList<string> pathEventTypes,
            IList<string> pathNamedWindows,
            IList<string> pathTables,
            IList<string> pathContexts,
            IList<string> pathVariables,
            IList<string> pathExprDecl,
            IList<NameAndParamNum> pathScripts,
            IList<string> pathClassProvideds)
        {
            DeploymentTypes = deploymentTypes;
            PathEventTypes = pathEventTypes;
            PathNamedWindows = pathNamedWindows;
            PathTables = pathTables;
            PathContexts = pathContexts;
            PathVariables = pathVariables;
            PathExprDecl = pathExprDecl;
            PathScripts = pathScripts;
            PathClassProvideds = pathClassProvideds;
        }

        public IDictionary<long, EventType> DeploymentTypes { get; }

        public IList<string> PathEventTypes { get; }

        public IList<string> PathNamedWindows { get; }

        public IList<string> PathTables { get; }

        public IList<string> PathContexts { get; }

        public IList<string> PathVariables { get; }

        public IList<string> PathExprDecl { get; }

        public IList<NameAndParamNum> PathScripts { get; }

        public IList<string> PathClassProvideds { get; }
    }
} // end of namespace