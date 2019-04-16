///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.util
{
    public class RangeIndexLookupValueRange : RangeIndexLookupValue
    {
        private QueryGraphRangeEnum @operator;
        private bool isAllowRangeReverse;

        public RangeIndexLookupValueRange(
            object value,
            QueryGraphRangeEnum @operator,
            bool allowRangeReverse)
            : base(value)
        {
            this.@operator = @operator;
            isAllowRangeReverse = allowRangeReverse;
        }

        public QueryGraphRangeEnum Operator {
            get => @operator;
        }

        public bool IsAllowRangeReverse {
            get => isAllowRangeReverse;
        }
    }
} // end of namespace