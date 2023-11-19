///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
	public class SupportBeanParameterizedWFieldSingleMapped<T>
	{
		private readonly IDictionary<string, T> mapProperty;

		public readonly IDictionary<string, T> mapField;

		public SupportBeanParameterizedWFieldSingleMapped(T value)
		{
			this.mapProperty = new Dictionary<string, T>();
			this.mapProperty.Put("key", value);
			this.mapField = new Dictionary<string, T>();
			this.mapField.Put("key", value);
		}

		public IDictionary<string, T> MapProperty => mapProperty;

		public T MapKeyed(string key)
		{
			return mapProperty.Get(key);
		}
	}
} // end of namespace
