using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class CodegenSyntaxGenerator
    {
        private class AssemblyIndexCache
        {
            private readonly LinkedList<AssemblyCache> _indices;
            private readonly IDictionary<string, bool> _resolutions;
            
            public AssemblyIndexCache(Assembly[] assemblies)
            {
                _indices = new LinkedList<AssemblyCache>();
                _indices.AddAll(assemblies.Select(GetAssemblyCache));
                _resolutions = new Dictionary<string, bool>();
            }

            private bool DoesImportResolveType(
                Type type,
                ImportDecl import)
            {
                if (import.IsNamespaceImport)
                {
                    var importName = $"{import.Namespace}.{type.Name}".Replace("@", "");
                    var current = _indices.First;
                    while (current != null) {
                        var assemblyCacheReference = current.Value;
                        if (assemblyCacheReference.TryContainsType(importName, out var typeExists)) {
                            if (typeExists) {
                                return true;
                            }

                            current = current.Next;
                        }
                        else {
                            lock (_globalAssemblyCache) {
                                _globalAssemblyCache.Remove(assemblyCacheReference.AssemblyName);
                            }

                            _indices.Remove(current);
                            current = current.Next;
                        }
                    }

                    return false;
                }

                return import.TypeName == type.Name;
            }

            public bool IsAmbiguous(
                Type type,
                ISet<ImportDecl> imports)
            {
                //Console.WriteLine("IsAmbiguous: {0}", type.Name);

                if (_resolutions.TryGetValue(type.Name, out var isAmbiguous)) {
                    return isAmbiguous;
                }

                return (_resolutions[type.Name] = IsAmbiguousInternal(type, imports));
            }

            private bool IsAmbiguousInternal(
                Type type,
                ISet<ImportDecl> imports)
            {
                var count = 0;
                
                //return imports.Count(import => DoesImportResolveType(type, import)) > 1;
                foreach (var import in imports) {
                    if (DoesImportResolveType(type, import)) {
                        if (++count > 1) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}