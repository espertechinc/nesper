///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.service
{
	/// <summary>
	/// On the level of enumeration method:
	///     If a enumeration method expression is invoked within another enumeration method expression (not counting expression declarations),
	///    for example "source.where(a => source.minBy(b => b.x))" the "source.minBy(b => b.x)" is not dependent on any other lambda so the result gets cached.
	///    The cache is keyed by the enumeration-method-node as an IdentityHashMap and verified by a context stack (Long[]) that is built in nested evaluation calls.
	/// 
	///	NOTE: ExpressionResultCacheEntry should not be held onto since the instance returned can be reused.
	/// </summary>
	public interface ExpressionResultCacheForEnumerationMethod
    {
	    void PushStack(ExpressionResultCacheStackEntry lambda);
	    bool PopLambda();
	    Deque<ExpressionResultCacheStackEntry> GetStack();

	    ExpressionResultCacheEntry<long?[], object> GetEnumerationMethodLastValue(object node);
	    void SaveEnumerationMethodLastValue(object node, object result);

	    void PushContext(long contextNumber);
	    void PopContext();
	}
} // end of namespace
