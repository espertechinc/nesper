///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
    public class StageDeploymentHelper
    {
        public static void MovePath(
            DeploymentInternal deployment,
            EPServicesPath from,
            EPServicesPath to)
        {
            var moduleName = deployment.ModuleProvider.ModuleName;
            HandleProvided(deployment.DeploymentId, deployment.PathNamedWindows, from.NamedWindowPathRegistry, moduleName, to.NamedWindowPathRegistry);
            HandleProvided(deployment.DeploymentId, deployment.PathTables, from.TablePathRegistry, moduleName, to.TablePathRegistry);
            HandleProvided(deployment.DeploymentId, deployment.PathContexts, from.ContextPathRegistry, moduleName, to.ContextPathRegistry);
            HandleProvided(deployment.DeploymentId, deployment.PathVariables, from.VariablePathRegistry, moduleName, to.VariablePathRegistry);
            HandleProvided(deployment.DeploymentId, deployment.PathEventTypes, from.EventTypePathRegistry, moduleName, to.EventTypePathRegistry);
            HandleProvided(deployment.DeploymentId, deployment.PathExprDecls, from.ExprDeclaredPathRegistry, moduleName, to.ExprDeclaredPathRegistry);
            HandleProvided(deployment.DeploymentId, deployment.PathScripts, from.ScriptPathRegistry, moduleName, to.ScriptPathRegistry);
        }

        private static void HandleProvided<TK, TE>(
            string deploymentId,
            TK[] objectNames,
            PathRegistry<TK, TE> source,
            string moduleName,
            PathRegistry<TK, TE> target)
            where TK : class
        {
            foreach (var objectName in objectNames) {
                var e = source.GetEntryWithModule(objectName, moduleName);
                if (e != null) {
                    try {
                        target.AddEntry(objectName, moduleName, e);
                    }
                    catch (PathException ex) {
                        throw new EPException(ex);
                    }
                }
            }

            source.DeleteDeployment(deploymentId);
        }
    }
} // end of namespace