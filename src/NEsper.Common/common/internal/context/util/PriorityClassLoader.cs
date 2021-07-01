using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.util
{
    public class PriorityClassLoader : ClassLoader
    {
        private readonly ClassLoader _parent;

        /// <summary>
        /// Priority assemblies
        /// </summary>
        private readonly IEnumerable<Assembly> _priorityAssemblies;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="priorityAssemblies"></param>
        public PriorityClassLoader(ClassLoader parent, IEnumerable<Assembly> priorityAssemblies)
        {
            _parent = parent;
            _priorityAssemblies = priorityAssemblies;
        }

        public Type GetClass(string typeName)
        {
            foreach (var priorityAssembly in _priorityAssemblies) {
                var priorityType = priorityAssembly.GetType(typeName, false, false);
                if (priorityType != null) {
                    return priorityType;
                }
            }

            return _parent.GetClass(typeName);
        }
    }
}