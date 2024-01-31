///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.variable.compiletime;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class Variable
    {
        public Variable(
            int variableNumber,
            string deploymentId,
            VariableMetaData metaData,
            string optionalContextDeploymentId)
        {
            VariableNumber = variableNumber;
            DeploymentId = deploymentId;
            MetaData = metaData;
            OptionalContextDeploymentId = optionalContextDeploymentId;
        }

        public int VariableNumber { get; }

        public VariableMetaData MetaData { get; }

        public string DeploymentId { get; }

        public string OptionalContextDeploymentId { get; }
    }
} // end of namespace