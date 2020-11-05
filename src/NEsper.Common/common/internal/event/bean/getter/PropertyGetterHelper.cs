///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	public class PropertyGetterHelper
	{
		public static object GetPropertySimple(
			PropertyInfo property,
			object @object)
		{
			try {
				return property.GetValue(@object);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(property, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(property, e);
			}
		}

		public static object GetPropertyMap(
			PropertyInfo property,
			object @object,
			object key)
		{
			try {
				var result = property.GetValue(@object);
				return CollectionUtil.GetMapValueChecked(result, key);
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(property, @object, e);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(property, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(property, e);
			}
		}

		public static object GetPropertyArray(
			PropertyInfo property,
			object @object,
			int index)
		{
			try {
				var value = property.GetValue(@object) as Array;
				if ((value == null) || (value.Length <= index)) {
					return null;
				}

				return value.GetValue(index);
			}
			catch (InvalidCastException e) {
				throw PropertyUtility.GetMismatchException(property, @object, e);
			}
			catch (ArgumentException e) {
				throw PropertyUtility.GetArgumentException(property, e);
			}
			catch (MemberAccessException e) {
				throw PropertyUtility.GetMemberAccessException(property, e);
			}
		}
	}
} // end of namespace
