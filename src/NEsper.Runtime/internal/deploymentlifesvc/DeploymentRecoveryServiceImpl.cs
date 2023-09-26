///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public class DeploymentRecoveryServiceImpl : DeploymentRecoveryService
    {
        public static readonly DeploymentRecoveryServiceImpl INSTANCE = new DeploymentRecoveryServiceImpl();

        public IEnumerator<KeyValuePair<string, DeploymentRecoveryEntry>> Deployments()
        {
            return EnumerationHelper.Empty<KeyValuePair<string, DeploymentRecoveryEntry>>();
        }

        public void Remove(string deploymentId)
        {
            // no action
        }

        public void Add(
            string deploymentId,
            int statementIdFirstStatement,
            EPCompiled compiled,
            IDictionary<int, object> userObjectsRuntime,
            IDictionary<int, string> statementNamesWhenProvidedByAPI,
            IDictionary<int, IDictionary<int, object>> substitutionParameters,
            string[] deploymentIdsConsumed)
        {
            // no action
        }
    }
} // end of namespace