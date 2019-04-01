///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.core
{
	public interface Viewable : IEnumerable<EventBean> {
        View Child { get; set; }

	    /// <summary>
	    /// Provides metadata information about the type of object the event collection contains.
	    /// </summary>
	    /// <returns>metadata for the objects in the collection</returns>
	    EventType EventType { get; }

#if REDUNDANT
        /// <summary>
        /// Allows iteration through all elements in this viewable.
        /// The iterator will return the elements in the collection in their natural order, or,
        /// if there is no natural ordering, in some unpredictable order.
        /// </summary>
        /// <returns>an enumerator which will go through all current elements in the collection.</returns>
        IEnumerator<EventBean> GetEnumerator();
#endif
	}
} // end of namespace