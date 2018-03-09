///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestStatementEvents
    {
        private EPServiceProvider epService;

        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean));
            epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
        }

        [Test]
        public void TestSimpleEvents()
        {
            var invocations = new int[] {0};
            var stmtText = "select * from Bean";
            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += delegate { invocations[0]++; };

            for (int ii = 0; ii < 100; ii++) {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.AreEqual(invocations[0], ii + 1);
            }
        }
    }
}
