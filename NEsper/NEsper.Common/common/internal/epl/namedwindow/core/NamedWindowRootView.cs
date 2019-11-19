///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    ///     The root window in a named window plays multiple roles: It holds the indexes for deleting rows, if any on-delete
    ///     statement
    ///     requires such indexes. Such indexes are updated when events arrive, or remove from when a data window
    ///     or on-delete statement expires events. The view keeps track of on-delete statements their indexes used.
    /// </summary>
    public class NamedWindowRootView
    {
        private readonly NamedWindowMetaData namedWindowMetaData;

        public NamedWindowRootView(NamedWindowMetaData vo)
        {
            namedWindowMetaData = vo;
        }

        public bool IsChildBatching => namedWindowMetaData.IsChildBatching;

        public EventType EventType => namedWindowMetaData.EventType;

        public string ContextName => namedWindowMetaData.ContextName;

        public bool IsVirtualDataWindow => namedWindowMetaData.IsVirtualDataWindow;
    }
} // end of namespace