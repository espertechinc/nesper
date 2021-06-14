using com.espertech.esper.container;
using com.espertech.esperio.support.util;

using NUnit.Framework;

namespace com.espertech.esperio
{
    public class AbstractIOTest
    {
        protected IContainer container;

        [SetUp]
        public virtual void SetUpCommon()
        {
            container = SupportContainer.Reset();
        }
    }
}