///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableMetadataColumnPairAggAccess : TableMetadataColumnPairBase
    {
        public TableMetadataColumnPairAggAccess(
            int dest,
            AggregationAccessorForge accessor)
            : base(dest)
        {
            Accessor = accessor;
        }

        public AggregationAccessorForge Accessor { get; }
    }
} // end of namespace