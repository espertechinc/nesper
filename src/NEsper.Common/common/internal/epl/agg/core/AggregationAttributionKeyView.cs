namespace com.espertech.esper.common.@internal.epl.agg.core
{
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////
/*
	 ***************************************************************************************
	 *  Copyright (C) 2006 EsperTech, Inc. All rights reserved.                            *
	 *  http://www.espertech.com/esper                                                     *
	 *  http://www.espertech.com                                                           *
	 *  ---------------------------------------------------------------------------------- *
	 *  The software in this package is published under the terms of the GPL license       *
	 *  a copy of which has been included with this distribution in the license.txt file.  *
	 ***************************************************************************************
	 */
    public class AggregationAttributionKeyView : AggregationAttributionKey
    {
        private readonly int streamNum;
        private readonly int? subqueryNum;
        private readonly int[] grouping;

        public AggregationAttributionKeyView(
            int streamNum,
            int? subqueryNum,
            int[] grouping)
        {
            this.streamNum = streamNum;
            this.subqueryNum = subqueryNum;
            this.grouping = grouping;
        }

        public T Accept<T>(AggregationAttributionKeyVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public int StreamNum => streamNum;

        public int? SubqueryNum => subqueryNum;

        public int[] Grouping => grouping;
    }
} // end of namespace