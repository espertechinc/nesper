///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestLiteralConstants 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean).FullName);
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            updateListener = null;
        }
    
        [Test]
        public void TestLiteral()
        {
            String statement = (
                "select 0x23 as mybyte, " +
                "'\u0041' as myunicode," +
                "08 as zero8, " +
                "09 as zero9, " +
                "008 as zeroZero8 " +
                "from SupportBean"
                );

            EPStatement stmt = epService.EPAdministrator.CreateEPL(statement);
            stmt.Events += updateListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 100));

            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(),
                "mybyte,myunicode,zero8,zero9,zeroZero8".Split(','),
                new Object[] { (byte)35, "A", 8, 9, 8 });
        }
    }
}
