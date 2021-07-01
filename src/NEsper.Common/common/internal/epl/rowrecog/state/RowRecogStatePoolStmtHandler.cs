///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public class RowRecogStatePoolStmtHandler
    {
        public int Count { get; private set; }

        public void DecreaseCount()
        {
            Count--;
            if (Count < 0) {
                Count = 0;
            }
        }

        public void DecreaseCount(int num)
        {
            Count -= num;
            if (Count < 0) {
                Count = 0;
            }
        }

        public void IncreaseCount()
        {
            Count++;
        }
    }
} // end of namespace