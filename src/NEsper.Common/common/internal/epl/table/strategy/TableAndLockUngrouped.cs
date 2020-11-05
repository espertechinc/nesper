///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class TableAndLockUngrouped
    {
        public TableAndLockUngrouped(
            ILockable @lock,
            TableInstanceUngrouped ungrouped)
        {
            Lock = @lock;
            Ungrouped = ungrouped;
        }

        public ILockable Lock { get; }

        public TableInstanceUngrouped Ungrouped { get; }
    }
} // end of namespace