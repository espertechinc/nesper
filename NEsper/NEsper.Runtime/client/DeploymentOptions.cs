///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client.option;
using com.espertech.esper.runtime.client.util;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Option holder for use with <seealso cref="EPDeploymentService.Deploy(EPCompiled)" />.
    /// </summary>
    public class DeploymentOptions
    {
        /// <summary>
        ///     Returns the deployment id if one should be assigned; A null value causes the runtime to generate and assign a
        ///     deployment id.
        /// </summary>
        /// <returns>deployment id</returns>
        public string DeploymentId { get; set; }

        /// <summary>
        ///     Returns the callback providing a runtime statement user object that can be obtained using
        ///     <see cref="EPStatement.UserObjectRuntime" />
        /// </summary>
        /// <returns>callback</returns>
        public StatementUserObjectRuntimeOption StatementUserObjectRuntime { get; set; }

        /// <summary>
        ///     Returns the callback overriding the statement name that identifies the statement within the deployment and that
        ///     can be obtained using <see cref="EPStatement.Name" />
        /// </summary>
        /// <returns>callback</returns>
        public StatementNameRuntimeOption StatementNameRuntime { get; set; }

        /// <summary>
        ///     Return the deployment lock strategy, the default is <seealso cref="LockStrategyDefault" />
        /// </summary>
        /// <returns>lock strategy</returns>
        public LockStrategy DeploymentLockStrategy { get; set; } = LockStrategyDefault.INSTANCE;

        /// <summary>
        ///     Returns the callback providing values for substitution parameters.
        /// </summary>
        /// <returns>callback</returns>
        public StatementSubstitutionParameterOption StatementSubstitutionParameter { get; set; }

        /// <summary>
        ///     Sets the deployment id if one should be assigned; A null value causes the runtime to generate and assign a
        ///     deployment id.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <returns>itself</returns>
        public DeploymentOptions WithDeploymentId(string deploymentId)
        {
            DeploymentId = deploymentId;
            return this;
        }

        /// <summary>
        ///     Sets the callback providing a runtime statement user object that can be obtained using
        ///     <see cref="EPStatement.UserObjectRuntime" />
        /// </summary>
        /// <param name="statementUserObjectRuntime">callback</param>
        /// <returns>itself</returns>
        public DeploymentOptions WithStatementUserObjectRuntime(StatementUserObjectRuntimeOption statementUserObjectRuntime)
        {
            StatementUserObjectRuntime = statementUserObjectRuntime;
            return this;
        }

        /// <summary>
        ///     Sets the callback overriding the statement name that identifies the statement within the deployment and that
        ///     can be obtained using <see cref="EPStatement.Name" />
        /// </summary>
        /// <param name="statementNameRuntime">callback</param>
        /// <returns>itself</returns>
        public DeploymentOptions WithStatementNameRuntime(StatementNameRuntimeOption statementNameRuntime)
        {
            StatementNameRuntime = statementNameRuntime;
            return this;
        }

        /// <summary>
        ///     Sets the deployment lock strategy, the default is <seealso cref="LockStrategyDefault" />
        /// </summary>
        /// <param name="deploymentLockStrategy">lock strategy</param>
        /// <returns>itself</returns>
        public DeploymentOptions WithDeploymentLockStrategy(LockStrategy deploymentLockStrategy)
        {
            DeploymentLockStrategy = deploymentLockStrategy;
            return this;
        }

        /// <summary>
        ///     Sets the callback providing values for substitution parameters.
        /// </summary>
        /// <param name="statementSubstitutionParameter">callback</param>
        /// <returns>itself</returns>
        public DeploymentOptions WithStatementSubstitutionParameter(StatementSubstitutionParameterOption statementSubstitutionParameter)
        {
            StatementSubstitutionParameter = statementSubstitutionParameter;
            return this;
        }
    }
} // end of namespace