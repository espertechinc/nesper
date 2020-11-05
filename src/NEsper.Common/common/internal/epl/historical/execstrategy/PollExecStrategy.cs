///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.historical.execstrategy
{
    /// <summary>
    ///     Interface for polling data from a data source such as a relational database.
    ///     <para />
    ///     Lifecycle methods are for managing connection resources.
    /// </summary>
    public interface PollExecStrategy
    {
        /// <summary>
        ///     Start the poll, called before any poll operation.
        /// </summary>
        void Start();

        /// <summary>
        ///     Poll events using the keys provided.
        /// </summary>
        /// <param name="lookupValues">is keys for exeuting a query or such</param>
        /// <param name="agentInstanceContext">context</param>
        /// <returns>a list of events for the keys</returns>
        IList<EventBean> Poll(
            object lookupValues,
            AgentInstanceContext agentInstanceContext);

        /// <summary>
        ///     Indicate we are done polling and can release resources.
        /// </summary>
        void Done();

        /// <summary>
        ///     Indicate we are no going to use this object again.
        /// </summary>
        void Destroy();
    }
} // end of namespace