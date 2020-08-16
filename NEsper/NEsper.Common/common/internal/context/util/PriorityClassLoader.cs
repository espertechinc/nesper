using System;
using System.Collections.Generic;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.util
{
    public class PriorityClassLoader : ClassLoader
    {
        private readonly ClassLoader _parent;

        /// <summary>
        /// Priority types
        /// </summary>
        private readonly IDictionary<string, Type> _priorityTypes;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="priorityTypes"></param>
        public PriorityClassLoader(ClassLoader parent, IDictionary<string, Type> priorityTypes)
        {
            _parent = parent;
            _priorityTypes = priorityTypes;
        }

        /// <inheritdoc />
        public Type GetClass(string typeName)
        {
            return _priorityTypes.TryGetValue(typeName, out var typeValue)
                ? typeValue 
                : _parent.GetClass(typeName);
        }
    }
}