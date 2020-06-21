///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeploymentInternal
    {
        public DeploymentInternal(
            string deploymentId,
            EPStatement[] statements,
            string[] deploymentIdDependencies,
            string[] pathNamedWindows,
            string[] pathTables,
            string[] pathVariables,
            string[] pathContexts,
            string[] pathEventTypes,
            string[] pathExprDecls,
            NameAndParamNum[] pathScripts,
            ModuleIndexMeta[] pathIndexes,
            string[] pathClassProvideds,
            ModuleProvider moduleProvider,
            IDictionary<ModuleProperty, object> modulePropertiesCached,
            IDictionary<long, EventType> deploymentTypes,
            long lastUpdateDate)
        {
            DeploymentId = deploymentId;
            Statements = statements;
            DeploymentIdDependencies = deploymentIdDependencies;
            PathNamedWindows = pathNamedWindows;
            PathTables = pathTables;
            PathVariables = pathVariables;
            PathContexts = pathContexts;
            PathEventTypes = pathEventTypes;
            PathExprDecls = pathExprDecls;
            PathScripts = pathScripts;
            PathIndexes = pathIndexes;
            PathClassProvideds = pathClassProvideds;
            
            ModuleProvider = moduleProvider;
            ModulePropertiesCached = modulePropertiesCached;
            DeploymentTypes = deploymentTypes;
            LastUpdateDate = lastUpdateDate;
        }

        public static DeploymentInternal From(
            string deploymentId,
            EPStatement[] statements,
            ISet<string> deploymentIdDependencies,
            DeployerModulePaths modulePaths,
            DeployerModuleEPLObjects moduleEPLObjects,
            ModuleProviderCLPair moduleProvider)
        {
            var deploymentIdDependenciesArray = deploymentIdDependencies.ToArray();
            return new DeploymentInternal(
                deploymentId,
                statements,
                deploymentIdDependenciesArray,
                modulePaths.PathNamedWindows.ToArray(),
                modulePaths.PathTables.ToArray(),
                modulePaths.PathVariables.ToArray(),
                modulePaths.PathContexts.ToArray(),
                modulePaths.PathEventTypes.ToArray(),
                modulePaths.PathExprDecl.ToArray(),
                modulePaths.PathScripts.ToArray(),
                moduleEPLObjects.ModuleIndexes.ToArray(),
                modulePaths.PathClassProvideds.ToArray(),
                moduleProvider.ModuleProvider,
                moduleProvider.ModuleProvider.ModuleProperties,
                modulePaths.DeploymentTypes,
                DateTimeHelper.CurrentTimeMillis);
        }
        
        public string DeploymentId { get; }

        public EPStatement[] Statements { get; }

        public string[] DeploymentIdDependencies { get; }

        public string[] PathNamedWindows { get; }

        public string[] PathTables { get; }

        public string[] PathVariables { get; }

        public string[] PathContexts { get; }

        public string[] PathEventTypes { get; }

        public string[] PathExprDecls { get; }

        public NameAndParamNum[] PathScripts { get; }

        public ModuleIndexMeta[] PathIndexes { get; }
        
        public string[] PathClassProvideds { get; }
        
        public ModuleProvider ModuleProvider { get; }

        public IDictionary<long, EventType> DeploymentTypes { get; }

        public IDictionary<ModuleProperty, object> ModulePropertiesCached { get; }

        public long LastUpdateDate { get; }
    }
} // end of namespace