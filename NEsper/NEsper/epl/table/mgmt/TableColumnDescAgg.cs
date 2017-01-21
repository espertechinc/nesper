///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.baseagg;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableColumnDescAgg : TableColumnDesc
    {
        public TableColumnDescAgg(int positionInDeclaration, string columnName, ExprAggregateNode aggregation, EventType optionalAssociatedType)
            : base (positionInDeclaration, columnName)
        {
            Aggregation = aggregation;
            OptionalAssociatedType = optionalAssociatedType;
        }

        public ExprAggregateNode Aggregation { get; private set; }

        public EventType OptionalAssociatedType { get; private set; }
    }
}
