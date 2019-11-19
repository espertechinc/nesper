///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableColumnDescAgg : TableColumnDesc
    {
        public TableColumnDescAgg(
            int positionInDeclaration,
            string columnName,
            ExprAggregateNode aggregation,
            EventType optionalAssociatedType)
            : base(positionInDeclaration, columnName)
        {
            Aggregation = aggregation;
            OptionalAssociatedType = optionalAssociatedType;
        }

        public ExprAggregateNode Aggregation { get; }

        public EventType OptionalAssociatedType { get; }
    }
} // end of namespace