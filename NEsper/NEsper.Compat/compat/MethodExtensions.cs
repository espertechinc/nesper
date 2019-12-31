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

        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type declaringType)
        {
            var extensionMethods = GetExtensionMethods()
                .Where(m => m.GetParameters()[0].ParameterType == declaringType);
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