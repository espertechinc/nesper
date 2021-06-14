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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.espertech.esper.compat
{
    public static class MethodExtensions
    {
        private static List<MethodInfo> ExtensionMethods;

        public static IEnumerable<MethodInfo> GetExtensionMethods()
        {
            if (ExtensionMethods == null)
            {
                ExtensionMethods = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(
                        assembly => assembly.GetTypes().Where(t => t.IsDefined(typeof(ExtensionAttribute), true))
                            .SelectMany(
                                type => type.GetMethods().Where(m => m.IsDefined(typeof(ExtensionAttribute), true))))
                    .ToList();
            }

            return ExtensionMethods;
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
            ExtensionMethods = null;
        }
    }
}