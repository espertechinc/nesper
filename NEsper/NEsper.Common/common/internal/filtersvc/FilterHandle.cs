///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.filtersvc
{
    /// <summary>
    ///     Marker interface for use with <see cref="FilterService" />. Implementations serve as a filter match values when
    ///     events match filters, and also serve to enter and remove a filter from the filter subscription set.
    /// </summary>
    public interface FilterHandle
    {
        /// <summary>Gets the statement id.</summary>
        /// <value>The statement id.</value>
        int StatementId { get; }

        /// <summary>Gets the agent instance identifier.</summary>
        /// <value>The agent instance identifier.</value>
        int AgentInstanceId { get; }
    }
} // End of namespace