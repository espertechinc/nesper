///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Module ordering utility.
    /// </summary>
    public class ModuleOrderUtil
    {
        /// <summary>
        /// Compute a deployment order among the modules passed in considering their uses-dependency declarations.
        /// <para />The operation also checks and reports circular dependencies.
        /// <para />Pass in @{link ModuleOrderOptions} to customize the behavior if this method. When passing no options
        /// or passing default options, the default behavior checks uses-dependencies and circular dependencies.
        /// </summary>
        /// <param name="modules">to determine ordering for</param>
        /// <param name="options">operation options or null for default options</param>
        /// <param name="deployedModules">deployed modules</param>
        /// <returns>ordered modules</returns>
        /// <throws>ModuleOrderException when any module dependencies are not satisfied</throws>
        public static ModuleOrder GetModuleOrder(
            ICollection<Module> modules,
            ISet<string> deployedModules,
            ModuleOrderOptions options)
        {
            if (options == null) {
                options = new ModuleOrderOptions();
            }

            IList<Module> proposedModules = new List<Module>();
            proposedModules.AddAll(modules);

            ISet<string> availableModuleNames = new HashSet<string>();
            foreach (var proposedModule in proposedModules) {
                if (proposedModule.Name != null) {
                    availableModuleNames.Add(proposedModule.Name);
                }
            }

            // Collect all deployed modules
            ISet<string> allDeployedModules = new HashSet<string>();
            allDeployedModules.AddAll(deployedModules);
            foreach (var proposedModule in proposedModules) {
                allDeployedModules.Add(proposedModule.Name);
            }

            // Collect uses-dependencies of proposed modules
            IDictionary<string, ISet<string>> usesPerModuleName = new Dictionary<string, ISet<string>>();
            foreach (var proposedModule in proposedModules) {
                // check uses-dependency is available
                if (options.IsCheckUses) {
                    if (proposedModule.Uses != null) {
                        foreach (var uses in proposedModule.Uses) {
                            if (availableModuleNames.Contains(uses)) {
                                continue;
                            }

                            var deployed = allDeployedModules.Contains(uses);
                            if (deployed) {
                                continue;
                            }

                            var message = "Module-dependency not found";
                            if (proposedModule.Name != null) {
                                message += " as declared by module '" + proposedModule.Name + "'";
                            }

                            message += " for uses-declaration '" + uses + "'";
                            throw new ModuleOrderException(message);
                        }
                    }
                }

                if (proposedModule.Name == null || proposedModule.Uses == null) {
                    continue;
                }

                var usesSet = usesPerModuleName.Get(proposedModule.Name);
                if (usesSet == null) {
                    usesSet = new HashSet<string>();
                    usesPerModuleName.Put(proposedModule.Name, usesSet);
                }

                usesSet.AddAll(proposedModule.Uses);
            }

            var proposedModuleNames = new Dictionary<string, SortedSet<int>>()
                .WithNullKeySupport();

            var count = 0;
            foreach (var proposedModule in proposedModules) {
                var moduleNumbers = proposedModuleNames.Get(proposedModule.Name);
                if (moduleNumbers == null) {
                    moduleNumbers = new SortedSet<int>();
                    proposedModuleNames.Put(proposedModule.Name, moduleNumbers);
                }

                moduleNumbers.Add(count);
                count++;
            }

            var graph = new DependencyGraph(proposedModules.Count, false);
            var fromModule = 0;
            foreach (var proposedModule in proposedModules) {
                if (proposedModule.Uses == null || proposedModule.Uses.IsEmpty()) {
                    fromModule++;
                    continue;
                }

                var dependentModuleNumbers = new SortedSet<int>();
                foreach (var use in proposedModule.Uses) {
                    var moduleNumbers = proposedModuleNames.Get(use);
                    if (moduleNumbers == null) {
                        continue;
                    }

                    dependentModuleNumbers.AddAll(moduleNumbers);
                }

                dependentModuleNumbers.Remove(fromModule);
                graph.AddDependency(fromModule, dependentModuleNumbers);
                fromModule++;
            }

            if (options.IsCheckCircularDependency) {
                // Stack<int> 
                var circular = graph.FirstCircularDependency;
                if (circular != null) {
                    var message = "";
                    var delimiter = "";
                    foreach (var i in circular) {
                        message += delimiter;
                        message += "module '" + proposedModules[i].Name + "'";
                        delimiter = " uses (depends on) ";
                    }

                    throw new ModuleOrderException(
                        "Circular dependency detected in module uses-relationships: " + message);
                }
            }

            IList<Module> reverseDeployList = new List<Module>();
            ISet<int> ignoreList = new HashSet<int>();
            while (ignoreList.Count < proposedModules.Count) {
                // seconardy sort according to the order of listing
                ISet<int> rootNodes = new SortedSet<int>(
                    new ProxyComparer<int>() {
                        ProcCompare = (
                            o1,
                            o2) => {
                            return -1 * o1.CompareTo(o2);
                        }
                    });
                rootNodes.AddAll(graph.GetRootNodes(ignoreList));

                if (rootNodes.IsEmpty()) { // circular dependency could cause this
                    for (var i = 0; i < proposedModules.Count; i++) {
                        if (!ignoreList.Contains(i)) {
                            rootNodes.Add(i);
                            break;
                        }
                    }
                }

                foreach (var root in rootNodes) {
                    ignoreList.Add(root);
                    reverseDeployList.Add(proposedModules[root]);
                }
            }

            reverseDeployList.Reverse();
            return new ModuleOrder(reverseDeployList);
        }
    }
} // end of namespace