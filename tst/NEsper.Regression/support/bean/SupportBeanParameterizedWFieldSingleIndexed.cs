///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.bean
{
	public class SupportBeanParameterizedWFieldSingleIndexed<T>
	{
		private readonly T[] _indexedArrayField;
		private readonly IList<T> _indexedListField;

		public SupportBeanParameterizedWFieldSingleIndexed(T value)
		{
			T[] array = new T[1];
			array[0] = value;
			IndexedArrayProperty = array;
			_indexedArrayField = array;

			IList<T> list = new List<T>();
			list.Add(value);
			IndexedListProperty = list;
			_indexedListField = list;
		}

		public T[] IndexedArrayProperty { get; }

		public IList<T> IndexedListProperty { get; }

		public T IndexedArrayAtIndex(int index)
		{
			return IndexedArrayProperty[index];
		}
	}
} // end of namespace
