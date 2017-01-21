///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.epl.join.exec.@base
{
    public class RangeIndexLookupValueRange : RangeIndexLookupValue
    {
        public QueryGraphRangeEnum Operator { get; private set; }

        public bool IsAllowRangeReverse { get; private set; }

        public RangeIndexLookupValueRange(Object value, QueryGraphRangeEnum @operator, bool allowRangeReverse) 
            : base(value)
        {
            Operator = @operator;
            IsAllowRangeReverse = allowRangeReverse;
        }
    }
}
