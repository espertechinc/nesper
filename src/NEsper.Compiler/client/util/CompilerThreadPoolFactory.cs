using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compiler.client.util
{
    /// <summary>
    /// <para>
    ///     This is a delegate that can be supplied to the Container to override the default manner in which
    ///     the CompilerPool creates the IExecutorService that manages the ThreadPool.
    /// </para>
    /// <para>
    ///     Your factory will be provided with the compiler configuration and a thread factory.  The thread factory
    ///     should be used for creating individual threads within your pool.  However, you are free to use all of
    ///     the suggestions or none of the suggestions, but be aware of the ramifications.
    /// </para>
    /// <para>
    ///     Usage:
    ///     <code>
    ///     configuration.Container.Register&lt;CompilerThreadPoolFactory&gt;(myCompilerThreadPoolFactory)
    ///     </code>
    /// </para> 
    /// </summary>
    public delegate IExecutorService CompilerThreadPoolFactory(
        ConfigurationCompilerByteCode configuration,
        ThreadFactory threadFactory);
}