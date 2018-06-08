///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterServiceProvider 
    {
        [Test]
        public void TestGetService()
        {
            var container = SupportContainer.Instance;
            FilterService serviceOne = FilterServiceProvider.NewService(
                container, ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY, false);
            FilterService serviceTwo = FilterServiceProvider.NewService(
                container, ConfigurationEngineDefaults.FilterServiceProfile.READMOSTLY, false);
    
            Assert.IsTrue(serviceOne != null);
            Assert.IsTrue(serviceOne != serviceTwo);
        }
    }
}
