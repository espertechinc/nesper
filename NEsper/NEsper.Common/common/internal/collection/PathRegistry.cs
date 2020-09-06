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
    public class PathRegistry<TK, TE> where TK : class
    {
        private readonly IDictionary<TK, PathModuleEntry<TE>> _entities;

        public PathRegistry(PathRegistryObjectType objectType)
        {
            ObjectType = objectType;
            _entities = new HashMap<TK, PathModuleEntry<TE>>();
        }

        public PathRegistry(
            PathRegistryObjectType objectType,
            IDictionary<TK, PathModuleEntry<TE>> entities)
        {
            ObjectType = objectType;
            _entities = entities;
        }


        public PathRegistryObjectType ObjectType { get; }

        public int Count => _entities.Count;

        public bool IsEmpty() 
        {
            return _entities.Count == 0;
        }
        
        public void Add(
            TK entityKey,
            string moduleName,
            TE entity,
            string deploymentId)
        {
            CheckModuleNameParameter(moduleName);
            var existing = _entities.Get(entityKey);
            if (existing == null) {
                existing = new PathModuleEntry<TE>();
                _entities.Put(entityKey, existing);
            }
            else {
                var existingDeploymentId = existing.GetDeploymentId(moduleName);
                if (existingDeploymentId != null) {
                    throw new PathExceptionAlreadyRegistered(entityKey.ToString(), ObjectType, moduleName);
                }
            }

            existing.Add(moduleName, entity, deploymentId);
        }

        public void AddEntry(TK entityKey,
            String moduleName,
            PathDeploymentEntry<TE> entity)
        {
            CheckModuleNameParameter(moduleName);
            if (!_entities.TryGetValue(entityKey, out var existing)) {
                existing = new PathModuleEntry<TE>();
                _entities[entityKey] = existing;
            }
            else {
                var existingDeploymentId = existing.GetDeploymentId(moduleName);
                if (existingDeploymentId != null) {
                    throw new PathExceptionAlreadyRegistered(entityKey.ToString(), ObjectType, moduleName);
                }
            }

            existing.Add(moduleName, entity);
        }

        public Pair<TE, string> GetAnyModuleExpectSingle(
            TK entityKey,
            ICollection<string> moduleUses)
        {
            var existing = _entities.Get(entityKey);
            return existing?.GetAnyModuleExpectSingle(entityKey.ToString(), ObjectType, moduleUses);
        }

        public TE GetWithModule(
            TK entityKey,
            string moduleName)
        {
            CheckModuleNameParameter(moduleName);
            var existing = _entities.Get(entityKey);
            return existing == null ? default(TE) : existing.GetWithModule(moduleName);
        }
        
        public PathDeploymentEntry<TE> GetEntryWithModule(TK entityKey, String moduleName) {
            CheckModuleNameParameter(moduleName);
            return _entities.TryGetValue(entityKey, out var existing)
                ? existing.GetEntryWithModule(moduleName)
                : null;
        }

        public string GetDeploymentId(
            TK entityEntity,
            string moduleName)
        {
            CheckModuleNameParameter(moduleName);
            var existing = _entities.Get(entityEntity);
            return existing?.GetDeploymentId(moduleName);
        }

        public void DeleteDeployment(string deploymentId)
        {
            var keysToRemove = _entities
                .Where(entry => entry.Value.DeleteDeployment(deploymentId))
                .ToList();

            keysToRemove.ForEach(key => _entities.Remove(key));
        }

        public void AddDependency(
            TK entityKey,
            string moduleName,
            string deploymentIdDep)
        {
            CheckModuleNameParameter(moduleName);
            var existing = _entities.Get(entityKey);
            if (existing == null) {
                throw new ArgumentException("Failed to find " + ObjectType.Name + " '" + entityKey + "'");
            }

            existing.AddDependency(entityKey.ToString(), moduleName, deploymentIdDep, ObjectType);
        }

        public ISet<string> GetDependencies(
            TK entityKey,
            string moduleName)
        {
            CheckModuleNameParameter(moduleName);
            var existing = _entities.Get(entityKey);
            return existing?.GetDependencies(entityKey.ToString(), moduleName, ObjectType);
        }

        public void RemoveDependency(
            TK entityKey,
            string moduleName,
            string deploymentId)
        {
            CheckModuleNameParameter(moduleName);
            var existing = _entities.Get(entityKey);
            existing?.RemoveDependency(moduleName, deploymentId);
        }

        public void Traverse(Consumer<TE> consumer)
        {
            foreach (var entry in _entities) {
                entry.Value.Traverse(consumer);
            }
        }

        public void TraverseWithModule(BiConsumer<String, TE> consumer) {
            foreach (var entry in _entities) {
                entry.Value.TraverseWithModule(consumer);
            }
        }
        
        private void CheckModuleNameParameter(string moduleName)
        {
            if (moduleName != null && moduleName.Length == 0) { 
                throw new ArgumentException("Invalid empty module name, use null or a non-empty value");
            }
        }

        public void MergeFrom(PathRegistry<TK, TE> other)
        {
            if (other.ObjectType != ObjectType) {
                throw new ArgumentException("Invalid object type " + other.ObjectType + " expected " + ObjectType);
            }

            foreach (var entry in other._entities) {
                if (_entities.ContainsKey(entry.Key)) {
                    continue;
                }

                _entities.Put(entry.Key, entry.Value);
            }
        }
        
        public PathRegistry<TK, TE> Copy() {
            var copy = new HashMap<TK, PathModuleEntry<TE>>();
            foreach (var entry in _entities) {
                PathModuleEntry<TE> entryCopy = entry.Value.Copy();
                copy[entry.Key] = entryCopy;
            }
            return new PathRegistry<TK, TE>(ObjectType, copy);
        }
    }
} // end of namespace