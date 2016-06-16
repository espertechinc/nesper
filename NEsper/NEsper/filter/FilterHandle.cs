///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.filter
{
	/// <summary>
	/// Marker interface for use with <see cref="FilterService"/>. Implementations serve as a filter match values when
	/// events match filters, and also serve to enter and remove a filter from the filter subscription set.
	/// </summary>
	public interface FilterHandle
	{
        /// <summary>
        /// Gets the statement id.
        /// </summary>
        /// <value>The statement id.</value>
        int StatementId { get; }
	}

} // End of namespace
