///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraEventType 
    {
        private EPServiceProviderSPI _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
        
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestEventType() {
            RunAssertionType(true);
            RunAssertionType(false);

            // name cannot be the same as an existing event type
            _epService.EPAdministrator.CreateEPL("create schema SchemaOne as (p0 string)");
            SupportMessageAssertUtil.TryInvalid(_epService, "create window SchemaOne.win:keepall as SchemaOne",
                "Error starting statement: An event type or schema by name 'SchemaOne' already exists"
            );

            _epService.EPAdministrator.CreateEPL("create schema SchemaTwo as (p0 string)");
            SupportMessageAssertUtil.TryInvalid(_epService, "create table SchemaTwo(c0 int)",
                "Error starting statement: An event type or schema by name 'SchemaTwo' already exists"
            );
        }

        private void RunAssertionType(bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as (c0 int[], c1 int[primitive])" :
                    "create table MyInfra (c0 int[], c1 int[primitive])";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(_epService, false, eplCreate);

            object[][] expectedType = new object[][] { new object[] { "c0", typeof(int[]) }, new object[] { "c1", typeof(int[]) } };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmt.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
}
