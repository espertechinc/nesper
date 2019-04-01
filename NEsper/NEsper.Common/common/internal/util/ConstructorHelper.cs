///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Helper class to find and invoke a class constructors that matches the types of arguments supplied.
    /// </summary>
    public class ConstructorHelper
    {
        private static Type[] EMPTY_OBJECT_ARRAY_TYPE = new[] { typeof(Object[]) };

        /// <summary>
        /// Find and invoke constructor matching the argument number and types returning an instance of given class.
        /// </summary>
        /// <param name="type">is the class of instance to construct</param>
        /// <param name="arguments">is the arguments for the constructor to match in number and type</param>
        /// <returns>instance of class</returns>
        /// <throws>MemberAccessException thrown if no access to class</throws>
        /// <throws>NoSuchMethodException thrown when the constructor is not found</throws>
        /// <throws>TargetInvocationException thrown when the ctor throws and exception</throws>
        /// <throws>InstantiationException thrown when the class cannot be loaded</throws>
        public static Object InvokeConstructor(Type type, Object[] arguments)
        {
            Type[] parameterTypes = new Type[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                parameterTypes[i] = arguments[i].GetType();
            }

            // Find a constructor that matches exactly
            ConstructorInfo ctor = GetRegularConstructor(type, parameterTypes);
            if (ctor != null)
            {
                return ctor.Invoke(arguments);
            }

            // Find a constructor with the same number of assignable parameters (such as int -> Integer).
            ctor = FindMatchingConstructor(type, parameterTypes);
            if (ctor != null)
            {
                return ctor.Invoke(arguments);
            }

            // Find an Object[] constructor, which always matches (throws an exception if not found)
            ctor = GetObjectArrayConstructor(type);
            if (ctor == null)
            {
                throw new MissingMethodException();
            }

            return ctor.Invoke(new Object[] { arguments });
        }

        private static ConstructorInfo FindMatchingConstructor(Type type, Type[] parameterTypes)
        {
            ConstructorInfo[] ctors = type.GetConstructors();

            for (int i = 0; i < ctors.Length; i++)
            {
                ParameterInfo[] ctorParams = ctors[i].GetParameters();

                if (IsAssignmentCompatible(parameterTypes, ctorParams))
                {
                    return ctors[i];
                }
            }

            return null;
        }

        private static bool IsAssignmentCompatible(Type[] parameterTypes, ParameterInfo[] ctorParams)
        {
            if (parameterTypes.Length != ctorParams.Length)
            {
                return false;
            }

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (!ctorParams[i].ParameterType.IsAssignmentCompatible(parameterTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static ConstructorInfo GetRegularConstructor(Type type, Type[] parameterTypes)
        {
            // Try to find the matching constructor
            ConstructorInfo ctor = type.GetConstructor(parameterTypes);
            return ctor;
        }

        // Try to find an Object[] constructor
        private static ConstructorInfo GetObjectArrayConstructor(Type clazz)
        {
            return clazz.GetConstructor(EMPTY_OBJECT_ARRAY_TYPE);
        }
    }
}