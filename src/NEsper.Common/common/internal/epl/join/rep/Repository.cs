///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.join.rep
{
    /// <summary>
    ///     An interface for a repository of events in a lookup/join scheme
    ///     that supplies events for event stream table lookups and receives results of lookups.
    /// </summary>
    public interface Repository
    {
        /// <summary>
        ///     Supply events for performing look ups for a given stream.
        /// </summary>
        /// <param name="lookupStream">is the stream to perform lookup for</param>
        /// <returns>an iterator over events with additional positioning information</returns>
        IEnumerator<Cursor> GetCursors(int lookupStream);

        /// <summary>
        ///     Add a lookup result.
        /// </summary>
        /// <param name="cursor">provides result position and parent event and node information</param>
        /// <param name="lookupResults">is the events found</param>
        /// <param name="resultStream">is the stream number of the stream providing the results</param>
        void AddResult(
            Cursor cursor,
            ICollection<EventBean> lookupResults,
            int resultStream);
    }
} // end of namespace