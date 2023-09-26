///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    public class GroupByViewAgedEntry
    {
        public GroupByViewAgedEntry(
            View subview,
            long lastUpdateTime)
        {
            Subview = subview;
            LastUpdateTime = lastUpdateTime;
        }

        public View Subview { get; }

        public long LastUpdateTime { get; private set; }

        public void SetLastUpdateTime(long lastUpdateTime)
        {
            LastUpdateTime = lastUpdateTime;
        }
    }
} // end of namespace