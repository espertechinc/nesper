///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
	public class SupportBeanParameterizedWFieldSingleIndexed<T>
	{
		private readonly T[] indexedArrayProperty;
		private readonly IList<T> indexedListProperty;

		public readonly T[] indexedArrayField;
		public readonly IList<T> indexedListField;

		public SupportBeanParameterizedWFieldSingleIndexed(T value)
		{
			var array = new T[1];
			array[0] = value;
			this.indexedArrayProperty = array;
			this.indexedArrayField = array;
			IList<T> list = new List<T>();
			list.Add(value);
			this.indexedListProperty = list;
			this.indexedListField = list;
		}

		public T[] IndexedArrayProperty => indexedArrayProperty;

		public IList<T> IndexedListProperty => indexedListProperty;

		public T IndexedArrayAtIndex(int index)
		{
			return indexedArrayProperty[index];
		}
	}
} // end of namespace
