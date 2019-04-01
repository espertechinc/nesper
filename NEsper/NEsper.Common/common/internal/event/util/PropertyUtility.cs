///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.util
{
    public class PropertyUtility
    {
        public static PropertyAccessException GetMemberAccessException(FieldInfo field, MemberAccessException e)
        {
            return GetAccessExceptionField(field, e);
        }

        public static PropertyAccessException GetArgumentException(FieldInfo field, ArgumentException e)
        {
            return GetAccessExceptionField(field, e);
        }

        private static PropertyAccessException GetAccessExceptionField(FieldInfo field, Exception e)
        {
            var declaring = field.DeclaringType;
            var message = "Failed to obtain field value for field " + field.Name + " on class " +
                          declaring.GetCleanName() + ": " + e.Message;
            throw new PropertyAccessException(message, e);
        }

        private static PropertyAccessException GetMismatchException(
            Type declared, object @object, InvalidCastException e)
        {
            var classNameExpected = declared.GetCleanName();
            string classNameReceived;
            if (@object != null) {
                classNameReceived = @object.GetType().GetCleanName();
            }
            else {
                classNameReceived = "null";
            }

            if (classNameExpected.Equals(classNameReceived)) {
                classNameExpected = declared.GetCleanName();
                classNameReceived = @object != null ? @object.GetType().GetCleanName() : "null";
            }

            var message = "Mismatched getter instance to event bean type, expected " + classNameExpected +
                          " but received " + classNameReceived;
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetMemberAccessException(MethodInfo method, MemberAccessException e)
        {
            return GetAccessExceptionMethod(method, e);
        }

        public static PropertyAccessException GetArgumentException(MethodInfo method, ArgumentException e)
        {
            return GetAccessExceptionMethod(method, e);
        }

        private static PropertyAccessException GetAccessExceptionMethod(MethodInfo method, Exception e)
        {
            var declaring = method.DeclaringType;
            var message = "Failed to invoke method " + method.Name + " on class " + declaring.GetCleanName() + ": " +
                          e.Message;
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetMismatchException(
            MethodInfo method, object @object, InvalidCastException e)
        {
            return GetMismatchException(method.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetMismatchException(
            FieldInfo field, object @object, InvalidCastException e)
        {
            return GetMismatchException(field.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetTargetException(MethodInfo method, TargetException e)
        {
            var declaring = method.DeclaringType;
            var message = "Failed to invoke method " + method.Name + " on class " + declaring.GetCleanName() + ": " +
                          e.InnerException.Message;
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetGeneralException(MethodInfo method, Exception t)
        {
            var declaring = method.DeclaringType;
            var message = "Failed to invoke method " + method.Name + " on class " + declaring.GetCleanName() + ": " +
                          t.Message;
            throw new PropertyAccessException(message, t);
        }
    }
} // end of namespace