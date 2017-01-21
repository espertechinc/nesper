///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
	/// <summary>
	/// On the level of indexed event properties: Properties that are contained in EventBean instances, such as for Enumeration Methods, get wrapped only once for the same event.
	/// NOTE: ExpressionResultCacheEntry should not be held onto since the instance returned can be reused.
	/// </summary>
	public interface ExpressionResultCacheForPropUnwrap
    {
	    ExpressionResultCacheEntry<EventBean, ICollection<EventBean>> GetPropertyColl(string propertyNameFullyQualified, EventBean reference);
	    void SavePropertyColl(string propertyNameFullyQualified, EventBean reference, ICollection<EventBean> events);
	}
} // end of namespace
