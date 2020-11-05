///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class SortedAggregationStateDesc
    {
        public SortedAggregationStateDesc(
            bool max,
            ImportServiceCompileTime importService,
            ExprNode[] criteria,
            Type[] criteriaTypes,
            DataInputOutputSerdeForge[] criteriaSerdes,
            bool[] sortDescending,
            bool ever,
            int streamNum,
            ExprAggMultiFunctionSortedMinMaxByNode parent,
            ExprForge optionalFilter,
            EventType streamEventType)
        {
            IsMax = max;
            ImportService = importService;
            Criteria = criteria;
            CriteriaTypes = criteriaTypes;
            SortDescending = sortDescending;
            IsEver = ever;
            StreamNum = streamNum;
            Parent = parent;
            OptionalFilter = optionalFilter;
            StreamEventType = streamEventType;
            CriteriaSerdes = criteriaSerdes;
        }

        public ImportServiceCompileTime ImportService { get; }

        public ExprNode[] Criteria { get; }

        public bool[] SortDescending { get; }

        public int StreamNum { get; }

        public ExprAggMultiFunctionSortedMinMaxByNode Parent { get; }

        public ExprForge OptionalFilter { get; }

        public EventType StreamEventType { get; }

        public Type[] CriteriaTypes { get; }
        
        public DataInputOutputSerdeForge[] CriteriaSerdes { get; }

        public bool IsEver { get; }

        public bool IsSortUsingCollator => ImportService.IsSortUsingCollator;

        public bool IsMax { get; }
    }
} // end of namespace