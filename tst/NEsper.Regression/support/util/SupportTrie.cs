///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.util
{
	public interface SupportTrie<TK, TV>
	{
		TV Get(TK key);

		void Put(
			TK key,
			TV value);

		void Remove(TK key);
		void Clear();
		IOrderedDictionary<string, TV> PrefixMap(TK key);
	}
} // end of namespace
