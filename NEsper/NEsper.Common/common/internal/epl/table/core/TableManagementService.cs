///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    /// <summary>
    ///     Service to manage named windows on an runtime level.
    /// </summary>
    public interface TableManagementService
    {
        int DeploymentCount { get; }

        TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        void AddTable(
            string tableName,
            TableMetaData tableMetaData,
            EPStatementInitServices services);

        Table GetTable(
            string deploymentId,
            string tableName);

        void DestroyTable(
            string deploymentId,
            string tableName);

        Table AllocateTable(TableMetaData metadata);

        TableSerdes GetTableSerdes<T>(
            Table table,
            DataInputOutputSerdeWCollation<T> aggregationSerde,
            StatementContext statementContext);

        TableInstance AllocateTableInstance(
            Table table,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace