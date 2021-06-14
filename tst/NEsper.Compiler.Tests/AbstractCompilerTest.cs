using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.compiler
{
    public class AbstractCompilerTest
    {
        protected IContainer container;

        [SetUp]
        public virtual void SetUpCommon()
        {
            container = SupportContainer.Reset();
        }
    }
}