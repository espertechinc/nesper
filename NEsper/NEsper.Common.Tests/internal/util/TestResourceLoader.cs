///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestResourceLoader : AbstractCommonTest
    {
        private const string TEST_RESOURCE = "regression/esper.test.readconfig.cfg.xml";

        [Test, RunInApplicationDomain]
        public void TestResolveResourceAsURL()
        {
            Assert.That(container.ResourceManager().ResolveResourceURL("somefile"), Is.Null);
            Assert.That(container.ResourceManager().ResolveResourceURL(TEST_RESOURCE), Is.Not.Null);
        }
    }
} // end of namespace
