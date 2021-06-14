///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public abstract class TableManagementServiceBase : TableManagementService
    {
        private readonly IDictionary<string, TableDeployment> deployments = new Dictionary<string, TableDeployment>();

        protected TableManagementServiceBase(TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            TableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        public int DeploymentCount => deployments.Count;

        public void AddTable(
            string tableName,
            TableMetaData tableMetaData,
            EPStatementInitServices services)
        {
            var deployment = deployments.Get(services.DeploymentId);
            if (deployment == null) {
                deployment = new TableDeployment();
                deployments.Put(services.DeploymentId, deployment);
            }

            deployment.Add(tableName, tableMetaData, services);
        }

        public Table GetTable(
            string deploymentId,
            string tableName)
        {
            var deployment = deployments.Get(deploymentId);
            return deployment == null ? null : deployment.GetTable(tableName);
        }

        public void DestroyTable(
            string deploymentId,
            string tableName)
        {
            var deployment = deployments.Get(deploymentId);
            if (deployment == null) {
                return;
            }

            deployment.Remove(tableName);
            if (deployment.IsEmpty()) {
                deployments.Remove(deploymentId);
            }
        }

        public abstract Table AllocateTable(TableMetaData metadata);

        public abstract TableInstance AllocateTableInstance(
            Table table,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace