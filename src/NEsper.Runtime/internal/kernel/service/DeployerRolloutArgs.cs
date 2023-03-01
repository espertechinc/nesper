using System;
using System.Collections.Generic;

#if NETCORE
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
        
#if NETCORE
        public Func<string, AssemblyLoadContext> GetDeploymentLoadContext;
#endif
    }
}