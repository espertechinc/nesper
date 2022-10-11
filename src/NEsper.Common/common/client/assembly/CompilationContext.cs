namespace com.espertech.esper.common.client.assembly
{
    /// <summary>
    /// CompilationContext contains information about a compilation request that is being processed.
    /// Currently, this is a placeholder until we can determine what information we can surface about
    /// the compilation request.
    /// </summary>
    public class CompilationContext
    {
        /// <summary>
        /// The module name (if known)
        /// </summary>
        public string ModuleName { get; set; }
        
        /// <summary>
        /// The target namespace (if applicable and known).
        /// </summary>
        public string Namespace { get; set; }
    }
}