///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    ///     Context provided to <see cref="ConditionHandler" /> implementations providing
    ///     engine-condition-contextual information.
    ///     <para />
    ///     Statement information pertains to the statement currently being processed when
    ///     the condition occured.
    /// </summary>
    public class ConditionHandlerContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="runtimeUri">engine URI</param>
        /// <param name="statementName">statement name</param>
        /// <param name="deploymentId">statement deployment id</param>
        /// <param name="engineCondition">condition reported</param>
        public ConditionHandlerContext(
            string runtimeUri,
            string statementName,
            string deploymentId,
            BaseCondition engineCondition)
        {
            RuntimeURI = runtimeUri;
            StatementName = statementName;
            DeploymentId = deploymentId;
            EngineCondition = engineCondition;
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string RuntimeURI { get; }

        /// <summary>
        ///     Returns the statement name, if provided, or the statement id assigned to the statement if no name was
        ///     provided.
        /// </summary>
        /// <value>statement name or id</value>
        public string StatementName { get; }

        /// <summary>Returns the deployment id of the statement. </summary>
        public string DeploymentId { get; }

        /// <summary>Returns the condition reported. </summary>
        /// <value>condition reported</value>
        public BaseCondition EngineCondition { get; }
    }
}