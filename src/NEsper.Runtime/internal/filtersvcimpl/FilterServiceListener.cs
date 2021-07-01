///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>Listener to filter activity. </summary>
    public interface FilterServiceListener
    {
        /// <summary>Indicates an event being filtered. </summary>
        /// <param name="theEvent">event</param>
        /// <param name="matches">matches found</param>
        /// <param name="statementId">optional statement id if for a statement</param>
        void Filtering(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            int? statementId);
    }
}