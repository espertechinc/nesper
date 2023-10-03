///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public class DeploymentRecoveryEntry
    {
        public DeploymentRecoveryEntry(
            int statementIdFirstStatement,
            EPCompiled compiled,
            IDictionary<int, object> userObjectsRuntime,
            IDictionary<int, string> statementNamesWhenProvidedByAPI,
            IDictionary<int, IDictionary<int, object>> substitutionParameters,
            string[] deploymentIdsConsumed)
        {
            StatementIdFirstStatement = statementIdFirstStatement;
            Compiled = compiled;
            UserObjectsRuntime = userObjectsRuntime;
            StatementNamesWhenProvidedByAPI = statementNamesWhenProvidedByAPI;
            SubstitutionParameters = substitutionParameters;
            DeploymentIdsConsumed = deploymentIdsConsumed;
        }

        public int StatementIdFirstStatement { get; }

        public EPCompiled Compiled { get; set; }

        public IDictionary<int, object> UserObjectsRuntime { get; }

        public IDictionary<int, string> StatementNamesWhenProvidedByAPI { get; }

        public IDictionary<int, IDictionary<int, object>> SubstitutionParameters { get; }
        
        public string[] DeploymentIdsConsumed { get; }
    }
} // end of namespace