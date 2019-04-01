using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

		public interface ExpressionResultCacheForEnumerationMethod {

	    ExpressionResultCacheEntryLongArrayAndObj GetEnumerationMethodLastValue(object node);

	    void SaveEnumerationMethodLastValue(object node, object result);

	    void PushContext(long contextNumber);

	    void PopContext();
	}
} // end of namespace