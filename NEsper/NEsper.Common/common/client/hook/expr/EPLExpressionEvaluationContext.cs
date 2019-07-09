///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.expr
{
    /// <summary>
    /// Provides expression evaluation context information in an expression.
    /// </summary>
    public class EPLExpressionEvaluationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EPLExpressionEvaluationContext"/> class.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="contextPartitionId">The context partition identifier.</param>
        /// <param name="runtimeUri">The engine URI.</param>
        /// <param name="statementUserObject">The statement user object.</param>
        public EPLExpressionEvaluationContext(
            string statementName,
            int contextPartitionId,
            string runtimeUri,
            object statementUserObject)
        {
            StatementName = statementName;
            ContextPartitionId = contextPartitionId;
            RuntimeURI = runtimeUri;
            StatementUserObject = statementUserObject;
        }

        public string RuntimeURI { get; private set; }

        public string StatementName { get; private set; }

        public int ContextPartitionId { get; private set; }

        public object StatementUserObject { get; private set; }
    }
} // end of namespace