///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.pattern.pool
{
    public class PatternSubexpressionPoolStmtHandler
    {
        private int _count;

        public int Count => _count;

        public void DecreaseCount()
        {
            _count--;
            if (_count < 0) {
                _count = 0;
            }
        }

        public void IncreaseCount()
        {
            _count++;
        }
    }
}