///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.exec.util
{
    public class RangeIndexLookupValueRange : RangeIndexLookupValue
    {
        private QueryGraphRangeEnum _operator;
        private bool _isAllowRangeReverse;

        public RangeIndexLookupValueRange(
            object value,
            QueryGraphRangeEnum @operator,
            bool allowRangeReverse)
            : base(value)
        {
            _operator = @operator;
            _isAllowRangeReverse = allowRangeReverse;
        }

        public QueryGraphRangeEnum Operator {
            get => _operator;
        }

        public bool IsAllowRangeReverse {
            get => _isAllowRangeReverse;
        }
    }
} // end of namespace