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
    public class TableAndLockGrouped
    {
        public TableAndLockGrouped(
            ILockable @lock,
            TableInstanceGrouped grouped)
        {
            Lock = @lock;
            Grouped = grouped;
        }

        public ILockable Lock { get; }

        public TableInstanceGrouped Grouped { get; }
    }
} // end of namespace