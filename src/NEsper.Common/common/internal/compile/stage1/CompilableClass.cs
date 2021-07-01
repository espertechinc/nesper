namespace com.espertech.esper.common.@internal.compile.stage1
{
    public class CompilableClass
    {
        private readonly string _code;
        private readonly string _className;

        public string Code => _code;

        public string ClassName => _className;

        public CompilableClass(
            string code,
            string className)
        {
            _code = code;
            _className = className;
        }
    }
}