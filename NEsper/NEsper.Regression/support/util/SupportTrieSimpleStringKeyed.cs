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
using com.espertech.esper.compat.collections.btree;

namespace com.espertech.esper.regressionlib.support.util
{
	public class SupportTrieSimpleStringKeyed<V> : SupportTrie<string, V>
	{
		private readonly IDictionary<string, V> simple = new Dictionary<string, V>();

		public V Get(string key)
		{
			return simple.Get(key);
		}

		public void Put(
			string key,
			V value)
		{
			simple.Put(key, value);
		}

		public void Remove(string key)
		{
			simple.Remove(key);
		}

		public void Clear()
		{
			simple.Clear();
		}

		public IOrderedDictionary<string, V> PrefixMap(string key)
		{
			var result = new OrderedListDictionary<string, V>();
			foreach (var entry in simple) {
				if (entry.Key.StartsWith(key)) {
					result.Put(entry.Key, entry.Value);
				}
			}

			return result;
		}
	}
} // end of namespace
