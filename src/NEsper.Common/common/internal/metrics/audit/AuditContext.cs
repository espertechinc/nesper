///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public class AuditContext
    {
        public AuditContext(
            string runtimeURI,
            string deploymentId,
            string statementName,
            int agentInstanceId,
            AuditEnum category,
            long runtimeTime,
            string message)
        {
            RuntimeURI = runtimeURI;
            DeploymentId = deploymentId;
            StatementName = statementName;
            AgentInstanceId = agentInstanceId;
            Category = category;
            Message = message;
            RuntimeTime = runtimeTime;
        }

        public string RuntimeURI { get; }

        public string DeploymentId { get; }

        public string StatementName { get; }

        public AuditEnum Category { get; }

        public long RuntimeTime { get; }
        public int AgentInstanceId { get; }

        public string Message { get; }

        public string Format()
        {
            return DefaultFormat(StatementName, AgentInstanceId, Category, Message);
        }

        public static string DefaultFormat(
            string statementName,
            int partition,
            AuditEnum category,
            string message)
        {
            var buf = new StringBuilder();
            buf.Append("Statement ");
            buf.Append(statementName);
            buf.Append(" partition ");
            buf.Append(partition);
            buf.Append(" ");
            buf.Append(category.PrettyPrintText);
            buf.Append(" ");
            buf.Append(message);
            return buf.ToString();
        }
    }
}