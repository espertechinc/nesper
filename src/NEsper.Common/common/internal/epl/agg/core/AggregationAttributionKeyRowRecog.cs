///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationAttributionKeyRowRecog : AggregationAttributionKey
    {
        public AggregationAttributionKeyRowRecog(int aggregationNumber)
        {
            AggregationNumber = aggregationNumber;
        }

        public int AggregationNumber { get; }

        public T Accept<T>(AggregationAttributionKeyVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace