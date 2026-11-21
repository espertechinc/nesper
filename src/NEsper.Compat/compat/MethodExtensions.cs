///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace com.espertech.esper.compat
{
    public static class MethodExtensions
    {
        // Extension methods of the non-collectible assemblies, per assembly. Collectible assemblies
        // (such as those that hold compiled EPL and inlined classes) are never entered here, so the
        // cache cannot keep their load context alive.
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<Assembly, MethodInfo[]> CachedAssemblies =
            new Dictionary<Assembly, MethodInfo[]>();
        private static volatile MethodInfo[] _cachedMethods;
        private static int _assemblyLoadCount;
        private static int _cachedAssemblyLoadCount = -1;

        static MethodExtensions()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => Interlocked.Increment(ref _assemblyLoadCount);
        }

        /// <summary>
        /// Returns the extension methods declared by the non-collectible assemblies loaded in the
        /// application domain. Assemblies loaded after an earlier call are picked up on the next call.
        /// </summary>
        public static IEnumerable<MethodInfo> GetExtensionMethods()
        {
            var loadCount = Volatile.Read(ref _assemblyLoadCount);
            var cachedMethods = _cachedMethods;
            if (cachedMethods != null && loadCount == Volatile.Read(ref _cachedAssemblyLoadCount)) {
                return cachedMethods;
            }

            lock (CacheLock) {
                loadCount = Volatile.Read(ref _assemblyLoadCount);
                if (_cachedMethods != null && loadCount == _cachedAssemblyLoadCount) {
                    return _cachedMethods;
                }

                var changed = _cachedMethods == null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    // Dynamic assemblies can gain types after they are scanned, so they are not cached.
                    if (assembly.IsCollectible || assembly.IsDynamic || CachedAssemblies.ContainsKey(assembly)) {
                        continue;
                    }

                    CachedAssemblies[assembly] = FindExtensionMethods(assembly);
                    changed = true;
                }

                if (changed) {
                    _cachedMethods = CachedAssemblies.Values.SelectMany(methods => methods).ToArray();
                }

                Volatile.Write(ref _cachedAssemblyLoadCount, loadCount);
                return _cachedMethods;
            }
        }

        /// <summary>
        /// Returns the extension methods declared by the collectible assemblies that define the given
        /// types or their generic arguments. These are found on each call and never cached.
        /// </summary>
        private static IEnumerable<MethodInfo> GetCollectibleExtensionMethods(IEnumerable<Type> typeSet)
        {
            var assemblies = new HashSet<Assembly>();
            foreach (var type in typeSet) {
                AddCollectibleAssemblies(assemblies, type);
            }

            return assemblies.SelectMany(FindExtensionMethods);
        }

        private static void AddCollectibleAssemblies(
            ISet<Assembly> assemblies,
            Type type)
        {
            if (type == null || type.IsGenericParameter) {
                return;
            }

            if (type.HasElementType) {
                AddCollectibleAssemblies(assemblies, type.GetElementType());
                return;
            }

            if (type.Assembly.IsCollectible) {
                assemblies.Add(type.Assembly);
            }

            if (type.IsGenericType) {
                foreach (var argument in type.GetGenericArguments()) {
                    AddCollectibleAssemblies(assemblies, argument);
                }
            }
        }

        private static MethodInfo[] FindExtensionMethods(Assembly assembly)
        {
            Type[] types;
            try {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex) {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            return types
                .Where(t => t.IsDefined(typeof(ExtensionAttribute), true))
                .SelectMany(t => t.GetMethods().Where(m => m.IsDefined(typeof(ExtensionAttribute), true)))
                .ToArray();
        }

        public static ISet<Type> AddToTypeSet(
            ISet<Type> typeSet,
            Type declaringType)
        {
            if (declaringType == null) {
                return typeSet;
            }

            typeSet.Add(declaringType);
            AddToTypeSet(typeSet, declaringType.BaseType);
            foreach (var interfaceType in declaringType.GetInterfaces()) {
                AddToTypeSet(typeSet, interfaceType);
            }

            return typeSet;
        }

        public static bool IsMatchGenericParameter(
            Type parameterType,
            ISet<Type> typeSet)
        {
            if (typeSet.Contains(parameterType)) {
                return true;
            }

            if (parameterType.IsGenericType) {
                parameterType = parameterType.GetGenericTypeDefinition();
                foreach (var type in typeSet) {
                    if (type.IsGenericType) {
                        var genericTypeDefinition = type.GetGenericTypeDefinition();
                        if (genericTypeDefinition != null) {
                            if (parameterType == genericTypeDefinition) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        
        public static bool IsMatchGenericMethod(
            MethodInfo method,
            ISet<Type> typeSet)
        {
            var parameterZero = method.GetParameters()[0].ParameterType;
            return IsMatchGenericParameter(parameterZero, typeSet);
        }

        public static MethodInfo MakeConcreteMethod(
            MethodInfo method,
            ISet<Type> typeSet)
        {
            if (!method.IsGenericMethod) {
                return method;
            }
            
            var parameterZero = method.GetParameters()[0].ParameterType;
            var parameterArgs = parameterZero.GetGenericArguments();
            
            // Okay, we need N parameterArgs.  We will get those based on the item in
            // the typeSet that matches parameterZero.  This isn't perfect yet, and there
            // are many edge cases we are not handling.

            var parameterZeroGenericType = parameterZero.GetGenericTypeDefinition();
            foreach (var type in typeSet) {
                if (type.IsGenericType) {
                    var genericTypeDefinition = type.GetGenericTypeDefinition();
                    if (genericTypeDefinition != null) {
                        if (parameterZeroGenericType == genericTypeDefinition) {
                            // We have found a "concrete" (we think) class <type> that matches
                            // the parameter in the method signature.  We will take the args for
                            // the concrete.

                            var genericTypeArgs = type.GetGenericArguments();
                            if (genericTypeArgs.Length != parameterArgs.Length) {
                                throw new InvalidOperationException();
                            }

                            // No guarantee that the genericTypeArgs are in the same order
                            // as specified by the method.  This is something else we will
                            // have to address.  Example:
                            //
                            // void MyMethod<TV,TK>(this IDictionary<TK,TV> value)
                            // 
                            // Currently, this is going to be broken, we have more to do here.
                            
                            return method.MakeGenericMethod(genericTypeArgs);
                        }
                    }
                }
            }

            throw new InvalidOperationException("unable to find generic arguments to map to method");
        }
        
        public static IEnumerable<MethodInfo> GetExtensionMethods(
            this Type declaringType,
            string methodName)
        {
            var typeSet = AddToTypeSet(new HashSet<Type>(), declaringType);
            var extensionMethods = GetExtensionMethods()
                .Concat(GetCollectibleExtensionMethods(typeSet))
                .Where(m => m.Name == methodName)
                .Where(m => IsMatchGenericMethod(m, typeSet))
                .ToList();
            
            // Go through the identified extension methods and discover any methods
            // that are generic.  Identify the generic type that needs to be bound
            // to the generic method in order to derive the "correct" type.

            extensionMethods = extensionMethods
                .Select(m => MakeConcreteMethod(m, typeSet))
                .ToList();
            
            return extensionMethods;
        }

        public static bool IsExtensionMethod(this MethodInfo method)
        {
            return method.IsDefined(typeof(ExtensionAttribute), true);
        }
        
        public static void InvalidateExtensionMethodsCache()
        {
            lock (CacheLock) {
                CachedAssemblies.Clear();
                _cachedMethods = null;
                _cachedAssemblyLoadCount = -1;
            }
        }
    }
}