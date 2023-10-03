namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPRuntimeSPIRunAfterDestroyCtx
    {
        public EPRuntimeSPIRunAfterDestroyCtx(string runtimeURI) {
            this.RuntimeUri = runtimeURI;
        }

        public string RuntimeUri { get; }
    }
}