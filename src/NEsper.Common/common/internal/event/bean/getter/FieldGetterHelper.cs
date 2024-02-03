///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    public class FieldGetterHelper
    {
        public static object GetFieldSimple(
            FieldInfo field,
            object @object)
        {
            try {
                return field.GetValue(@object);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(field, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(field, e);
            }
        }

        public static object GetFieldMap(
            FieldInfo field,
            object @object,
            object key)
        {
            try {
                var result = field.GetValue(@object);
                return CollectionUtil.GetMapValueChecked(result, key);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(field, @object, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(field, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(field, e);
            }
        }

        public static object GetFieldArray(
            FieldInfo field,
            object @object,
            int index)
        {
            try {
                var value = field.GetValue(@object) as Array;
                if (value == null || value.Length <= index) {
                    return null;
                }

                return value.GetValue(index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(field, @object, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(field, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(field, e);
            }
        }
    }
} // end of namespace