///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
	/// <summary>
	/// A comparator on multikeys. The multikeys must contain the same number of values.
	/// </summary>
	public sealed class ComparatorHashableMultiKeyCasting : IComparer<object> {
	    private readonly IComparer<HashableMultiKey> comparator;

	    public ComparatorHashableMultiKeyCasting(IComparer<HashableMultiKey> comparator) {
	        this.comparator = comparator;
	    }

	    public int Compare(object firstValues, object secondValues) {
	        return comparator.Compare((HashableMultiKey) firstValues, (HashableMultiKey) secondValues);
	    }
	}
} // end of namespace