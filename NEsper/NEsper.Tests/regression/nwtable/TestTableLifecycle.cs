///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableLifecycle
    {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp() {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestLifecycleIntoTable()
        {
            RunAssertionIntoTable();
        }

        [Test]
        public void TestLifecycleCreateIndex()
        {
            RunAssertionDependent("create index IDX on abc (p)");
        }

        [Test]
        public void TestLifecycleJoin()
        {
            RunAssertionDependent("select * from SupportBean, abc");
        }

        [Test]
        public void TestLifecycleSubquery()
        {
            RunAssertionDependent("select * from SupportBean where exists (select * from abc)");
        }

        [Test]
        public void TestLifecycleInsertInto() 
        {
            RunAssertionDependent("insert into abc select 'a' as id, 'a' as p from SupportBean");
        }

        private void RunAssertionIntoTable() 
        {
            string eplCreate = "create table abc (total count(*))";
            string eplUse = "select abc from SupportBean";
            string eplInto = "into table abc select count(*) as total from SupportBean";
    
            // typical select-use-destroy
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(eplCreate);
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(eplUse);
            EPStatement stmtInto = epService.EPAdministrator.CreateEPL(eplInto);
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__public"));
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__internal"));
    
            stmtCreate.Dispose();
            stmtSelect.Dispose();
            AssertFailCreate(eplCreate);
            stmtInto.Dispose();
    
            // destroy-all
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL(eplInto);
            epService.EPAdministrator.CreateEPL(eplUse);
            epService.EPAdministrator.DestroyAllStatements();
    
            stmtCreate = epService.EPAdministrator.CreateEPL(eplCreate);
            stmtCreate.Dispose();
    
            // deploy and undeploy as module
            string module = eplCreate + ";\n" + eplUse + ";\n" + eplInto + ";\n";
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(module);
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__public"));
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__internal"));
    
            AssertFailCreate(eplCreate);
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
            Assert.IsNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__public"));
            Assert.IsNull(epService.EPAdministrator.Configuration.GetEventType("table_abc__internal"));
    
            // stop and start
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(eplCreate);
            stmtCreateTwo.Stop();
            AssertFailCreate(eplCreate);
            stmtCreateTwo.Start();
            AssertFailCreate(eplCreate);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL(eplCreate);
        }

        private void RunAssertionDependent(String eplDependent)
        {
            String eplCreate = "create table abc (id string primary key, p string)";

            // typical select-use-destroy
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(eplCreate);
            EPStatement stmtDependent = epService.EPAdministrator.CreateEPL(eplDependent);

            stmtCreate.Dispose();
            AssertFailCreate(eplCreate);
            stmtDependent.Dispose();
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertFailCreate(string create) {
            try {
                epService.EPAdministrator.CreateEPL(create);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                // expected
            }
        }
    }
}
