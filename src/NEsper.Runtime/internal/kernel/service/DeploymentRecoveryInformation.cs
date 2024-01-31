///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeploymentRecoveryInformation
    {
        public DeploymentRecoveryInformation(
            IDictionary<int, object> statementUserObjectsRuntime,
            IDictionary<int, string> statementNamesWhenProvidedByAPI)
        {
            StatementUserObjectsRuntime = statementUserObjectsRuntime;
            StatementNamesWhenProvidedByAPI = statementNamesWhenProvidedByAPI;
        }

        public IDictionary<int, object> StatementUserObjectsRuntime { get; }

        public IDictionary<int, string> StatementNamesWhenProvidedByAPI { get; }
    }
} // end of namespace