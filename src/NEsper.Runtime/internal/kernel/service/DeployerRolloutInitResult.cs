///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerRolloutInitResult
    {
        public DeployerRolloutInitResult(
            ISet<string> deploymentIdDependencies,
            DeployerModuleEPLObjects moduleEPLObjects,
            DeployerModulePaths modulePaths,
            string moduleName)
        {
            DeploymentIdDependencies = deploymentIdDependencies;
            ModuleEPLObjects = moduleEPLObjects;
            ModulePaths = modulePaths;
            ModuleName = moduleName;
        }

        public ISet<string> DeploymentIdDependencies { get; }

        public DeployerModuleEPLObjects ModuleEPLObjects { get; }

        public DeployerModulePaths ModulePaths { get; }

        public string ModuleName { get; }
    }
} // end of namespace