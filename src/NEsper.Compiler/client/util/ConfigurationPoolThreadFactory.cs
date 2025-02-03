using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compiler.client.util
{
    public delegate IExecutorService ConfigurationPoolThreadFactory(
        ConfigurationCompilerByteCode configuration,
        ThreadFactory threadFactory);
}