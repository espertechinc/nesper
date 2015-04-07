///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableMetadataColumnPairAggAccess : TableMetadataColumnPairBase
    {
        public TableMetadataColumnPairAggAccess(int dest, AggregationAccessor accessor)
            : base(dest)
        {
            Accessor = accessor;
        }

        public AggregationAccessor Accessor { get; private set; }
    }
}