///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.strategy
{
    public class TableAndLockGrouped
    {
        public TableAndLockGrouped(ILockable ilock, TableStateInstanceGrouped grouped)
        {
            Lock = ilock;
            Grouped = grouped;
        }

        public ILockable Lock { get; private set; }

        public TableStateInstanceGrouped Grouped { get; private set; }
    }
} // end of namespace