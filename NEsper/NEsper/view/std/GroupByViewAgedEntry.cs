///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.view.std
{
    public class GroupByViewAgedEntry
    {
        public GroupByViewAgedEntry(object subviews, long lastUpdateTime)
        {
            SubviewHolder = subviews;
            LastUpdateTime = lastUpdateTime;
        }

        public object SubviewHolder { get; private set; }

        public long LastUpdateTime { get; set; }
    }
}
