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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionVariantStream
    {
        [Test]
        public void TestInstanceOfDynamicVariantStream()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            
            epService.EPAdministrator.CreateEPL("create schema S0 as " + typeof(SupportBean_S0).FullName);
            epService.EPAdministrator.CreateEPL("create schema S1 as " + typeof(SupportBean_S1).FullName);
            epService.EPAdministrator.CreateEPL("create variant schema MyVariantType as S0, S1");
    
            String[] fields = "a,b".Split(',');
            String text = "select * from MyVariantType.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.id? as a, B.id? as b" +
                    "  pattern (A B) " +
                    "  define " +
                    "    A as Typeof(A) = 'S0'," +
                    "    B as Typeof(B) = 'S1'" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPAdministrator.CreateEPL("insert into MyVariantType select * from S0");
            epService.EPAdministrator.CreateEPL("insert into MyVariantType select * from S1");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "S1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][] { new Object[] {1, 2}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][] { new Object[] {1, 2}});
    
            String epl = "// Declare one sample type\n" +
                    "create schema ST0 as (col string)\n;" +
                    "// Declare second sample type\n" +
                    "create schema ST1 as (col string)\n;" +
                    "// Declare variant stream holding either type\n" +
                    "create variant schema MyVariantStream as ST0, ST1\n;" +
                    "// Populate variant stream\n" +
                    "insert into MyVariantStream select * from ST0\n;" +
                    "// Populate variant stream\n" +
                    "insert into MyVariantStream select * from ST1\n;" +
                    "// Simple pattern to match ST0 ST1 pairs\n" +
                    "select * from MyVariantType.win:time(1 min)\n" +
                    "match_recognize (\n" +
                    "measures A.id? as a, B.id? as b\n" +
                    "pattern (A B)\n" +
                    "define\n" +
                    "A as Typeof(A) = 'ST0',\n" +
                    "B as Typeof(B) = 'ST1'\n" +
                    ");";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
