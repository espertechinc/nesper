///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.util
{
	public interface SupportTrie<K, V>
	{
		V Get(K key);

		void Put(
			K key,
			V value);

		void Remove(K key);
		void Clear();
		SortedDictionary<K, V> PrefixMap(K key);
	}
} // end of namespace
