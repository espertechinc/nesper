///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableAccessAnalysisResult
    {
        public TableAccessAnalysisResult(
            IDictionary<string, TableMetadataColumn> tableColumns, 
            ObjectArrayEventType internalEventType,
            ObjectArrayEventType publicEventType, 
            TableMetadataColumnPairPlainCol[] colsPlain,
            TableMetadataColumnPairAggMethod[] colsAggMethod, 
            TableMetadataColumnPairAggAccess[] colsAccess,
            AggregationRowStateForgeDesc aggDesc,
            string[] primaryKeyColumns,
            EventPropertyGetterSPI[] primaryKeyGetters, 
            Type[] primaryKeyTypes, 
            int[] primaryKeyColNums)
        {
            TableColumns = tableColumns;
            InternalEventType = internalEventType;
            PublicEventType = publicEventType;
            ColsPlain = colsPlain;
            ColsAggMethod = colsAggMethod;
            ColsAccess = colsAccess;
            AggDesc = aggDesc;
            PrimaryKeyColumns = primaryKeyColumns;
            PrimaryKeyGetters = primaryKeyGetters;
            PrimaryKeyTypes = primaryKeyTypes;
            PrimaryKeyColNums = primaryKeyColNums;
        }

        public IDictionary<string, TableMetadataColumn> TableColumns { get; }

        public ObjectArrayEventType InternalEventType { get; }

        public ObjectArrayEventType PublicEventType { get; }

        public TableMetadataColumnPairPlainCol[] ColsPlain { get; }

        public TableMetadataColumnPairAggMethod[] ColsAggMethod { get; }

        public TableMetadataColumnPairAggAccess[] ColsAccess { get; }

        public AggregationRowStateForgeDesc AggDesc { get; }

        public EventPropertyGetterSPI[] PrimaryKeyGetters { get; }

        public Type[] PrimaryKeyTypes { get; }

        public string[] PrimaryKeyColumns { get; }

        public int[] PrimaryKeyColNums { get; }
    }
} // end of namespace