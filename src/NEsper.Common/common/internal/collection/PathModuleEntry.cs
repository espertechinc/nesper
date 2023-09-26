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

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.collection
{
    public class PathModuleEntry<TE>
    {
        private readonly IDictionary<string, PathDeploymentEntry<TE>> _modules;

        public PathModuleEntry()
        {
            _modules = new Dictionary<string, PathDeploymentEntry<TE>>().WithNullKeySupport();
        }

        public PathModuleEntry(IDictionary<string, PathDeploymentEntry<TE>> modules)
        {
            _modules = modules;
        }

        public void Add(
            string moduleName,
            TE entity,
            string deploymentId)
        {
            _modules[moduleName] = new PathDeploymentEntry<TE>(deploymentId, entity);
        }

        public void Add(
            string moduleName,
            PathDeploymentEntry<TE> entity)
        {
            _modules[moduleName] = entity;
        }

        public Pair<TE, string> GetAnyModuleExpectSingle(
            string entityName,
            PathRegistryObjectType objectType,
            ICollection<string> moduleNames)
        {
            if (_modules.IsEmpty()) {
                return null;
            }

            if (moduleNames == null || moduleNames.IsEmpty()) {
                if (_modules.Count > 1) {
                    throw new PathExceptionAmbiguous(entityName, objectType);
                }

                var moduleName = _modules.Keys.First();
                var entry = _modules.Get(moduleName);
                if (entry == null) {
                    return null;
                }

                return new Pair<TE, string>(entry.Entity, moduleName);
            }

            if (_modules.Count == 1) {
                var entry = _modules.First();
                return new Pair<TE, string>(entry.Value.Entity, entry.Key);
            }

            var found = _modules
                .Where(e => moduleNames.Contains(e.Key))
                .Take(3)
                .ToList();

            switch (found.Count) {
                case 0:
                    return null;

                case 1:
                    var entry = found[0];
                    return new Pair<TE, string>(entry.Value.Entity, entry.Key);

                default:
                    throw new PathExceptionAmbiguous(entityName, objectType);
            }
        }

        public string GetDeploymentId(string moduleName)
        {
            var existing = _modules.Get(moduleName);
            return existing?.DeploymentId;
        }

        public TE GetWithModule(string moduleName)
        {
            var entry = _modules.Get(moduleName);
            return entry == null ? default : entry.Entity;
        }

        public PathDeploymentEntry<TE> GetEntryWithModule(string moduleName)
        {
            return _modules.Get(moduleName);
        }

        public bool DeleteDeployment(string deploymentId)
        {
            foreach (var entry in _modules) {
                if (entry.Value.DeploymentId.Equals(deploymentId)) {
                    _modules.Remove(entry.Key);
                    return _modules.IsEmpty();
                }
            }

            return _modules.IsEmpty();
        }

        public void AddDependency(
            string entityName,
            string moduleName,
            string deploymentIdDep,
            PathRegistryObjectType objectType)
        {
            var existing = _modules.Get(moduleName);
            if (existing == null) {
                throw new ArgumentException(
                    "Failed to find " + objectType.Name + " '" + entityName + "' under module '" + moduleName + "'");
            }

            existing.AddDependency(deploymentIdDep);
        }

        public ISet<string> GetDependencies(
            string entityName,
            string moduleName,
            PathRegistryObjectType objectType)
        {
            var existing = _modules.Get(moduleName);
            if (existing == null) {
                throw new ArgumentException(
                    "Failed to find " + objectType.Name + " '" + entityName + "' under module '" + moduleName + "'");
            }

            return existing.Dependencies;
        }

        public void RemoveDependency(
            string moduleName,
            string deploymentId)
        {
            var existing = _modules.Get(moduleName);
            existing?.RemoveDependency(deploymentId);
        }

        public void Traverse(Consumer<TE> consumer)
        {
            foreach (var entry in _modules) {
                consumer.Invoke(entry.Value.Entity);
            }
        }

        public void TraverseWithModule(BiConsumer<string, TE> consumer)
        {
            foreach (var entry in _modules) {
                consumer.Invoke(entry.Key, entry.Value.Entity);
            }
        }

        public PathModuleEntry<TE> Copy()
        {
            var copy = new HashMap<string, PathDeploymentEntry<TE>>();
            foreach (var entry in _modules) {
                var copyEntry = entry.Value.Copy();
                copy[entry.Key] = copyEntry;
            }

            return new PathModuleEntry<TE>(copy);
        }
    }
} // end of namespace