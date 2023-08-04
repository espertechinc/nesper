using System;
using System.Collections.Generic;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Loader;
#endif

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public struct DeployerRolloutArgs
    {
        public int CurrentStatementId;
        public ICollection<EPDeploymentRolloutCompiled> ItemsProvided;
        public EPRuntimeSPI Runtime;
    }
}