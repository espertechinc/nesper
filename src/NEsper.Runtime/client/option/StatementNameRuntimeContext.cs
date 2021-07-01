///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="StatementNameRuntimeOption" />.
    /// </summary>
    public class StatementNameRuntimeContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="statementName">statement name</param>
        /// <param name="statementId">statement number</param>
        /// <param name="epl">epl when attached or null</param>
        /// <param name="annotations">annotations</param>
        public StatementNameRuntimeContext(
            string deploymentId,
            string statementName,
            int statementId,
            string epl,
            Attribute[] annotations)
        {
            DeploymentId = deploymentId;
            StatementName = statementName;
            StatementId = statementId;
            Epl = epl;
            Annotations = annotations;
        }

        /// <summary>
        ///     Returns the deployment id
        /// </summary>
        /// <value>deployment id</value>
        public string DeploymentId { get; }

        /// <summary>
        ///     Returns the statement name
        /// </summary>
        /// <value>statement name</value>
        public string StatementName { get; }

        /// <summary>
        ///     Returns the statement number
        /// </summary>
        /// <value>statement number</value>
        public int StatementId { get; }

        /// <summary>
        ///     Returns the EPL when attached or null when not available
        /// </summary>
        /// <value>epl</value>
        public string Epl { get; }

        /// <summary>
        ///     Returns the annotations.
        /// </summary>
        /// <value>annotations</value>
        public Attribute[] Annotations { get; }
    }
} // end of namespace