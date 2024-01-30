///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationAttributionKeyView : AggregationAttributionKey
    {
        public AggregationAttributionKeyView(
            int streamNum,
            int? subqueryNum,
            int[] grouping)
        {
            StreamNum = streamNum;
            SubqueryNum = subqueryNum;
            Grouping = grouping;
        }

        public int StreamNum { get; }

        public int? SubqueryNum { get; }

        public int[] Grouping { get; }

        public T Accept<T>(AggregationAttributionKeyVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace