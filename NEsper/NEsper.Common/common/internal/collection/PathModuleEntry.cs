///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.collection
{
    public class PathModuleEntry<TE>
    {
        private readonly IDictionary<string, PathDeploymentEntry<TE>> modules =
            new Dictionary<string, PathDeploymentEntry<TE>>(4);

        public void Add(string moduleName, TE entity, string deploymentId)
        {
            modules.Put(moduleName, new PathDeploymentEntry<TE>(deploymentId, entity));
        }

        public Pair<TE, string> GetAnyModuleExpectSingle(
            string entityName, PathRegistryObjectType objectType, ISet<string> moduleNames)
        {
            if (modules.IsEmpty()) {
                return null;
            }

            if (moduleNames == null || moduleNames.IsEmpty()) {
                if (modules.Count > 1) {
                    throw new PathExceptionAmbiguous(entityName, objectType);
                }

                var moduleName = modules.Keys.First();
                var entry = modules.Get(moduleName);
                if (entry == null) {
                    return null;
                }

                return new Pair<TE,string>(entry.Entity, moduleName);
            }

            PathDeploymentEntry<TE> found = null;
            string moduleNameFound = null;
            foreach (var moduleName in moduleNames) {
                var entry = modules.Get(moduleName);
                if (entry != null) {
                    if (found != null) {
                        throw new PathExceptionAmbiguous(entityName, objectType);
                    }

                    found = entry;
                    moduleNameFound = moduleName;
                }
            }

            return found == null ? null : new Pair<TE, string>(found.Entity, moduleNameFound);
        }

        public string GetDeploymentId(string moduleName)
        {
            var existing = modules.Get(moduleName);
            return existing?.DeploymentId;
        }

        public TE GetWithModule(string moduleName)
        {
            var entry = modules.Get(moduleName);
            return entry == null ? default(TE) : entry.Entity;
        }

        public bool DeleteDeployment(string deploymentId)
        {
            foreach (var entry in modules) {
                if (entry.Value.DeploymentId.Equals(deploymentId)) {
                    modules.Remove(entry.Key);
                    return modules.IsEmpty();
                }
            }

            return modules.IsEmpty();
        }

        public void AddDependency(
            string entityName, string moduleName, string deploymentIdDep, PathRegistryObjectType objectType)
        {
            var existing = modules.Get(moduleName);
            if (existing == null) {
                throw new ArgumentException(
                    "Failed to find " + objectType.Name + " '" + entityName + "' under module '" + moduleName + "'");
            }

            existing.AddDependency(deploymentIdDep);
        }

        public ISet<string> GetDependencies(string entityName, string moduleName, PathRegistryObjectType objectType)
        {
            var existing = modules.Get(moduleName);
            if (existing == null) {
                throw new ArgumentException(
                    "Failed to find " + objectType.Name + " '" + entityName + "' under module '" + moduleName + "'");
            }

            return existing.Dependencies;
        }

        public void RemoveDependency(string moduleName, string deploymentId)
        {
            var existing = modules.Get(moduleName);
            existing?.RemoveDependency(deploymentId);
        }

        public void Traverse(Consumer<TE> consumer)
        {
            foreach (var entry in modules) {
                consumer.Invoke(entry.Value.Entity);
            }
        }
    }
} // end of namespace