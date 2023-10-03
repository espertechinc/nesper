using System.Collections.Generic;
using System.Reflection;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class CodegenSyntaxGenerator
    {
        private class AssemblyCache
        {
            private readonly AssemblyName _assemblyName;
            private readonly System.WeakReference<Assembly> _assemblyReference;
            private readonly IDictionary<string, bool> _resolutions;

            public AssemblyName AssemblyName => _assemblyName;

            public AssemblyCache(Assembly assembly)
            {
                _assemblyName = assembly.GetName();
                _assemblyReference = new System.WeakReference<Assembly>(assembly);
                _resolutions = new Dictionary<string, bool>();
            }

            public bool TryContainsType(
                string typeName,
                out bool exists)
            {
                if (_assemblyReference.TryGetTarget(out var assembly)) {
                    if (!_resolutions.TryGetValue(typeName, out exists)) {
                        exists = (assembly.GetType(typeName, false) != null);
                        _resolutions[typeName] = exists;
                    }

                    return true;
                }

                exists = false;
                return false;
            }
        }
    }
}