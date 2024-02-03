///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public interface DeploymentRecoveryService
    {
        void Add(
            string deploymentId,
            int statementIdFirstStatement,
            EPCompiled compiled,
            IDictionary<int, object> userObjectsRuntime,
            IDictionary<int, string> statementNamesWhenProvidedByAPI,
            IDictionary<int, IDictionary<int, object>> substitutionParameters,
            string[] deploymentIdsConsumed);

        IEnumerator<KeyValuePair<string, DeploymentRecoveryEntry>> Deployments();

        void Remove(string deploymentId);
    }
} // end of namespace