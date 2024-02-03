using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
    public abstract class ClientStageRegressionExecution : RegressionExecution
    {
        public abstract void Run(RegressionEnvironment env);

        public virtual ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.RUNTIMEOPS);
        }
    }
}