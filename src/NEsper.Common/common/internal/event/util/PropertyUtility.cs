///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.util
{
    public class PropertyUtility
    {
        public static PropertyAccessException GetMemberAccessException(
            FieldInfo field,
            MemberAccessException e)
        {
            return GetAccessExceptionField(field, e);
        }

        public static PropertyAccessException GetMemberAccessException(
            MethodInfo method,
            MemberAccessException e)
        {
            return GetAccessExceptionField(method, e);
        }

        public static PropertyAccessException GetMemberAccessException(
            PropertyInfo property,
            MemberAccessException e)
        {
            return GetAccessExceptionField(property, e);
        }

        // -----
        
        public static PropertyAccessException GetArgumentException(
            FieldInfo field,
            ArgumentException e)
        {
            return GetAccessExceptionField(field, e);
        }

        public static PropertyAccessException GetArgumentException(
            MemberInfo member,
            ArgumentException e)
        {
            return GetAccessExceptionField(member, e);
        }

        private static PropertyAccessException GetAccessExceptionField(
            FieldInfo field,
            Exception e)
        {
            var declaring = field.DeclaringType;
            var message =
                $"Failed to obtain field value for field {field.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        private static PropertyAccessException GetAccessExceptionField(
            MemberInfo member,
            Exception e)
        {
            var declaring = member.DeclaringType;
            var message =
                $"Failed to obtain member value for member {member.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        // -----

        public static PropertyAccessException GetAccessException(
            PropertyInfo property,
            MemberAccessException e)
        {
            return GetAccessExceptionProperty(property, e);
        }

        public static PropertyAccessException GetAccessException(
            MethodInfo method,
            MemberAccessException e)
        {
            var declaring = method.DeclaringType;
            var message =
                $"Failed to obtain return value for method {method.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetAccessException(
            PropertyInfo property,
            ArgumentException e)
        {
            return GetAccessExceptionProperty(property, e);
        }

        private static PropertyAccessException GetAccessExceptionProperty(
            PropertyInfo property,
            Exception e)
        {
            var declaring = property.DeclaringType;
            var message =
                $"Failed to obtain property value for property {property.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        // -----

        private static PropertyAccessException GetMismatchException(
            Type declared,
            object @object,
            InvalidCastException e)
        {
            var classNameExpected = declared.CleanName();
            string classNameReceived;
            if (@object != null) {
                classNameReceived = @object.GetType().CleanName();
            }
            else {
                classNameReceived = "null";
            }

            if (classNameExpected.Equals(classNameReceived)) {
                classNameExpected = declared.CleanName();
                classNameReceived = @object != null
                    ? @object.GetType().CleanName()
                    : "null";
            }

            var message = "Mismatched getter instance to event bean type, expected " +
                          classNameExpected +
                          " but received " +
                          classNameReceived;
            throw new PropertyAccessException(message, e);
        }

        // -----

        public static PropertyAccessException GetIllegalAccessException(
            MethodInfo method,
            MemberAccessException e)
        {
            return GetAccessExceptionMethod(method, e);
        }

        public static PropertyAccessException GetIllegalArgumentException(
            MethodInfo method,
            ArgumentException e)
        {
            return GetAccessExceptionMethod(method, e);
        }

        private static PropertyAccessException GetAccessExceptionMethod(
            MethodInfo method,
            Exception e)
        {
            var declaring = method.DeclaringType;
            var message = $"Failed to invoke method {method.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        // -----

        public static PropertyAccessException GetMismatchException(
            MethodInfo method,
            object @object,
            InvalidCastException e)
        {
            return GetMismatchException(method.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetMismatchException(
            FieldInfo field,
            object @object,
            InvalidCastException e)
        {
            return GetMismatchException(field.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetMismatchException(
            PropertyInfo property,
            object @object,
            InvalidCastException e)
        {
            return GetMismatchException(property.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetInvocationTargetException(
            MemberInfo member,
            TargetException e)
        {
            var declaring = member.DeclaringType;
            var message =
                $"Failed to invoke member {member.Name} on class {declaring.CleanName()}: {e.InnerException.Message}";
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetTargetException(
            MemberInfo member,
            TargetException e)
        {
            var declaring = member.DeclaringType;
            var message =
                $"Failed to invoke member {member.Name} on class {declaring.CleanName()}: {e.InnerException.Message}";
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetTargetException(
            MemberInfo member,
            TargetInvocationException e)
        {
            var declaring = member.DeclaringType;
            var message =
                $"Failed to invoke member {member.Name} on class {declaring.CleanName()}: {e.InnerException.Message}";
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetGeneralException(
            MethodInfo method,
            Exception e)
        {
            var declaring = method.DeclaringType;
            var message = $"Failed to invoke method {method.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetGeneralException(
            FieldInfo field,
            Exception e)
        {
            var declaring = field.DeclaringType;
            var message =
                $"Failed to obtain field value for field {field.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetGeneralException(
            PropertyInfo property,
            Exception e)
        {
            var declaring = property.DeclaringType;
            var message =
                $"Failed to obtain property value for property {property.Name} on class {declaring.CleanName()}: {e.Message}";
            throw new PropertyAccessException(message, e);
        }
    }
} // end of namespace