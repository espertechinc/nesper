///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestQueryWithEnum
    {
        [Test]
        public void TestEnum()
        {
            EPServiceProviderManager.PurgeAllProviders();
            var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();

            string statementText = "select * from " + Name.Clean<SupportBeanWithEnum>(false) + " where SupportEnum = " + Name.Clean<SupportEnum>(false) + ".ENUM_VALUE_2";
            using(var statement = epService.EPAdministrator.CreateEPL(statementText)) {
                var listener = new SupportUpdateListener();
                statement.Events += listener.Update;
                epService.EPRuntime.SendEvent(new SupportBeanWithEnum("", SupportEnum.ENUM_VALUE_3));
                Assert.IsFalse(listener.IsInvoked);
                epService.EPRuntime.SendEvent(new SupportBeanWithEnum("", SupportEnum.ENUM_VALUE_1));
                Assert.IsFalse(listener.IsInvoked);
                epService.EPRuntime.SendEvent(new SupportBeanWithEnum("", SupportEnum.ENUM_VALUE_2));
                Assert.IsTrue(listener.IsInvoked);
            }
        }
    }
}
