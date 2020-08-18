///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableManagementServiceImpl : TableManagementServiceBase
    {
        public TableManagementServiceImpl(TableExprEvaluatorContext tableExprEvaluatorContext)
            : base(
                tableExprEvaluatorContext)
        {
        }

        public override Table AllocateTable(TableMetaData metadata)
        {
            return new TableImpl(metadata);
        }

        public TableSerdes GetTableSerdes<T>(
            Table table,
            DataInputOutputSerde<T> aggregationSerde,
            StatementContext statementContext)
        {
            return null; // this implementation does not require serdes
        }

        public override TableInstance AllocateTableInstance(
            Table table,
            AgentInstanceContext agentInstanceContext)
        {
            if (!table.MetaData.IsKeyed) {
                return new TableInstanceUngroupedImpl(table, agentInstanceContext);
            }

            return new TableInstanceGroupedImpl(table, agentInstanceContext);
        }
    }
} // end of namespace