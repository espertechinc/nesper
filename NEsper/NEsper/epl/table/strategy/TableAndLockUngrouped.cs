///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.strategy
{
    public class TableAndLockUngrouped
    {
        public TableAndLockUngrouped(ILockable ilock, TableStateInstanceUngrouped ungrouped)
        {
            Lock = ilock;
            Ungrouped = ungrouped;
        }

        public ILockable Lock { get; private set; }

        public TableStateInstanceUngrouped Ungrouped { get; private set; }
    }
} // end of namespace