///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.expr
{
    /// <summary>
    ///     Invocation context for method invocations that invoke static methods or plug-in single-row functions.
    /// </summary>
    public class EPLMethodInvocationContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="statementName">the statement name</param>
        /// <param name="contextPartitionId">context partition id if using contexts, or -1 if not using context partitions</param>
        /// <param name="runtimeUri">the engine URI</param>
        /// <param name="functionName">
        ///     the name of the plug-in single row function, or the method name if not a plug-in single row
        ///     function
        /// </param>
        /// <param name="statementUserObject">the statement user object or null if not assigned</param>
        /// <param name="eventBeanService">event and event type services</param>
        public EPLMethodInvocationContext(
            string statementName,
            int contextPartitionId,
            string runtimeUri,
            string functionName,
            object statementUserObject,
            EventBeanService eventBeanService)
        {
            StatementName = statementName;
            ContextPartitionId = contextPartitionId;
            RuntimeURI = runtimeUri;
            FunctionName = functionName;
            StatementUserObject = statementUserObject;
            EventBeanService = eventBeanService;
        }

        /// <summary>
        ///     Returns the statement name, or null if the invocation context is not associated to a specific statement and is
        ///     shareable between statements
        /// </summary>
        /// <value>statement name or null</value>
        public string StatementName { get; private set; }

        /// <summary>
        ///     Returns the context partition id, or -1 if no contexts or if the invocation context is not associated to a specific
        ///     statement and is shareable between statements
        /// </summary>
        /// <value>context partition id</value>
        public int ContextPartitionId { get; private set; }

        /// <summary>
        ///     Returns the engine URI
        /// </summary>
        /// <value>engine URI</value>
        public string RuntimeURI { get; private set; }

        /// <summary>
        ///     Returns the function name that appears in the EPL statement.
        /// </summary>
        /// <value>function name</value>
        public string FunctionName { get; private set; }

        /// <summary>
        ///     Returns the statement user object or null if not assigned or if the invocation context is not associated to a
        ///     specific statement and is shareable between statements
        /// </summary>
        /// <value>statement user object or null</value>
        public object StatementUserObject { get; private set; }

        /// <summary>
        ///     Returns event and event type services.
        /// </summary>
        /// <value>eventBeanService</value>
        public EventBeanService EventBeanService { get; private set; }
    }
} // end of namespace