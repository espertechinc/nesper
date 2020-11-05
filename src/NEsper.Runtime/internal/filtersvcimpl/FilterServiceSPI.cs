///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
	/// <summary>
	/// Service provider interface for filter service.
	/// </summary>
	public interface FilterServiceSPI : FilterService {
	    /// <summary>
	    /// Get a set of statements of out the active filters, returning filters.
	    /// </summary>
	    /// <param name="statementId">statement ids to remove</param>
	    /// <returns>filters</returns>
	    IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> Get(ISet<int> statementId);

	    /// <summary>
	    /// Add activity listener.void acquireWriteLock();
	    /// </summary>
	    /// <param name="filterServiceListener">to add</param>
	    void AddFilterServiceListener(FilterServiceListener filterServiceListener);

	    /// <summary>
	    /// Remove activity listener.
	    /// </summary>
	    /// <param name="filterServiceListener">to remove</param>
	    void RemoveFilterServiceListener(FilterServiceListener filterServiceListener);

	    int FilterCountApprox { get; }

	    int CountTypes { get; }

	    /// <summary>
	    /// Initialization is optional and provides a chance to preload things after statements are available.
	    /// </summary>
	    /// <param name="availableTypes">type information</param>
	    void Init(Supplier<ICollection<EventType>> availableTypes);
	}
} // end of namespace