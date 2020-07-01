﻿using com.espertech.esper.compat.function;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.compiler
{
    public class AbstractRuntimeTest
    {
        protected IContainer container;

        [SetUp]
        public virtual void SetUpCommon()
        {
            container = SupportContainer.Reset();
        }
    }
}