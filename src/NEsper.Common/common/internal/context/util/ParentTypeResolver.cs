using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ParentTypeResolver : TypeResolver
    {
        private readonly TypeResolver _parent;

        /// <summary>
        /// Dictionary that keeps track of all types (usually classes) allocated to a given deployment.
        /// In Java, types are bound to the ClassLoader, but in .NET we have to honor AppDomain boundaries.
        /// The expectation is that we will need to honor some AppDomain unloading of classes based on
        /// this dictionary.
        /// </summary>
        private readonly IDictionary<string, IList<string>> _deploymentToClazzMap;

        /// <summary>
        /// Dictionary that maps type names to their class instance.
        /// </summary>
        private readonly IDictionary<string, Type> _typeMap;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent"></param>
        public ParentTypeResolver(TypeResolver parent)
        {
            _parent = parent;
            _deploymentToClazzMap = new Dictionary<string, IList<string>>();
            _typeMap = new Dictionary<string, Type>();
        }

        public Type ResolveType(
            string typeName,
            bool resolve)
        {
            // See if the type exists in the list of injected types.
            if (_typeMap.TryGetValue(typeName, out var typeValue)) {
                return typeValue;
            }

            return _parent.ResolveType(typeName, resolve);
        }

        /// <summary>
        /// Mechanism that allows the classLoader to be "injected" with classes with the deployment. 
        /// </summary>
        /// <param name="underlyingClassName"></param>
        /// <param name="clazz"></param>
        /// <param name="optionalDeploymentId"></param>
        /// <param name="allowDuplicate"></param>
        /// <exception cref="IllegalStateException"></exception>
        public void Add(
            string underlyingClassName,
            Type clazz,
            string optionalDeploymentId,
            bool allowDuplicate)
        {
            if (!allowDuplicate && _typeMap.ContainsKey(underlyingClassName)) {
                throw new IllegalStateException("Attempt to add duplicate class " + underlyingClassName + " to parent class loader");
            }

            _typeMap[underlyingClassName] = clazz;

            if (optionalDeploymentId != null) {
                if (!_deploymentToClazzMap.TryGetValue(optionalDeploymentId, out var existing)) {
                    _deploymentToClazzMap[optionalDeploymentId] = new List<string>() {underlyingClassName};
                }
                else {
                    existing.Add(underlyingClassName);
                }
            }
        }

        public void Remove(string deploymentId)
        {
            if (_deploymentToClazzMap.TryRemove(deploymentId, out var existing)) {
                foreach (string className in existing) {
                    _typeMap.Remove(className);
                }
            }
        }
    }
}