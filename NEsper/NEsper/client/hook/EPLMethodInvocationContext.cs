///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Invocation context for method invocations that invoke static methods or plug-in single-row functions.
    /// </summary>
    public class EPLMethodInvocationContext
    {
        private readonly String _statementName;
        private readonly int _contextPartitionId;
        private readonly String _engineURI;
        private readonly String _functionName;
        private readonly Object _statementUserObject;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">the statement name</param>
        /// <param name="contextPartitionId">context partition id if using contexts, or -1 if not using context partitions</param>
        /// <param name="engineURI">the engine URI</param>
        /// <param name="functionName">the name of the plug-in single row function, or the method name if not a plug-in single row function</param>
        /// <param name="statementUserObject">The statement user object.</param>
        public EPLMethodInvocationContext(
            String statementName,
            int contextPartitionId,
            String engineURI,
            String functionName,
            Object statementUserObject)
        {
            _statementName = statementName;
            _contextPartitionId = contextPartitionId;
            _engineURI = engineURI;
            _functionName = functionName;
            _statementUserObject = statementUserObject;
        }

        /// <summary>Returns the statement name. </summary>
        /// <value>statement name</value>
        public string StatementName
        {
            get { return _statementName; }
        }

        /// <summary>Returns the context partition id, or -1 if no contexts </summary>
        /// <value>context partition id</value>
        public int ContextPartitionId
        {
            get { return _contextPartitionId; }
        }

        /// <summary>Returns the engine URI </summary>
        /// <value>engine URI</value>
        public string EngineURI
        {
            get { return _engineURI; }
        }

        /// <summary>Returns the function name that appears in the EPL statement. </summary>
        /// <value>function name</value>
        public string FunctionName
        {
            get { return _functionName; }
        }

        /// <summary>Returns the statement user object.</summary>
        /// <value>The statement user object.</value>
        public object StatementUserObject
        {
            get { return _statementUserObject; }
        }
    }
}
