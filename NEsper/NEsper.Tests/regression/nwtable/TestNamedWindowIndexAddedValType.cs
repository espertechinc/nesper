///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowIndexAddedValType 
    {
        [Test]
        public void TestRevision()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean_S0>();
            config.AddEventType<SupportBean_S1>();
    
            var revType = new ConfigurationRevisionEventType();
            revType.AddNameBaseEventType("SupportBean_S0");
            revType.AddNameDeltaEventType("SupportBean_S1");
            revType.KeyPropertyNames = new string[] {"Id"};
            revType.PropertyRevision = PropertyRevisionEnum.MERGE_EXISTS;
            config.AddRevisionEventType("RevType", revType);
    
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
    
            // widen to long
            var stmtTextCreate = "create window MyWindowOne.win:keepall() as select * from RevType";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean_S1");
    
            epService.EPAdministrator.CreateEPL("create index MyWindowOneIndex1 on MyWindowOne(P10)");
            epService.EPAdministrator.CreateEPL("create index MyWindowOneIndex2 on MyWindowOne(P00)");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P00"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "P10"));
    
            var result = epService.EPRuntime.ExecuteQuery("select * from MyWindowOne where P10='1'");
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    }
}
