///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestDeployRedefinition
    {
        private EPServiceProvider _epService;
        private EPDeploymentAdmin _deploySvc;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _deploySvc = _epService.EPAdministrator.DeploymentAdmin;
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestCreateSchemaNamedWindowInsert()
        {
            String text = "module test.test1;\n" +
                    "create schema MyTypeOne(col1 string, col2 int);" +
                    "create window MyWindowOne#keepall as select * from MyTypeOne;" +
                    "insert into MyWindowOne select * from MyTypeOne;";

            DeploymentResult resultOne = _deploySvc.ParseDeploy(text, "uri1", "arch1", null);
            _deploySvc.UndeployRemove(resultOne.DeploymentId);

            DeploymentResult resultTwo = _deploySvc.ParseDeploy(text, "uri2", "arch2", null);
            _deploySvc.UndeployRemove(resultTwo.DeploymentId);
            text = "module test.test1;\n" +
                    "create schema MyTypeOne(col1 string, col2 int, col3 long);" +
                    "create window MyWindowOne#keepall as select * from MyTypeOne;" +
                    "insert into MyWindowOne select * from MyTypeOne;";

            DeploymentResult resultThree = _deploySvc.ParseDeploy(text, "uri1", "arch1", null);
            _deploySvc.UndeployRemove(resultThree.DeploymentId);

            FilterService filterService = ((EPServiceProviderSPI)_epService).FilterService;
            FilterServiceSPI filterSPI = (FilterServiceSPI)filterService;
            Assert.AreEqual(0, filterSPI.CountTypes);

            // test on-merge
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            String moduleString =
                    "@Name('S0') create window MyWindow#unique(IntPrimitive) as SupportBean;\n" +
                            "@Name('S1') on MyWindow insert into SecondStream select *;\n" +
                            "@Name('S2') on SecondStream merge MyWindow when matched then insert into ThirdStream select * then delete\n";
            Module module = _epService.EPAdministrator.DeploymentAdmin.Parse(moduleString);
            _epService.EPAdministrator.DeploymentAdmin.Deploy(module, null, "myid_101");
            _epService.EPAdministrator.DeploymentAdmin.UndeployRemove("myid_101");
            _epService.EPAdministrator.DeploymentAdmin.Deploy(module, null, "myid_101");

            // test table
            String moduleTableOne = "create table MyTable(c0 string, c1 string)";
            DeploymentResult d = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleTableOne);
            _epService.EPAdministrator.DeploymentAdmin.UndeployRemove(d.DeploymentId);
            String moduleTableTwo = "create table MyTable(c0 string, c1 string, c2 string)";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleTableTwo);
        }

        [Test]
        public void TestRedefDeployOrder()
        {
            String eplClientA = "" +
                    "create schema InputEvent as (col1 string, col2 string);" +
                    "\n" +
                    "@Name('A') " +
                    "insert into OutOne select col1||col2 as outOneCol from InputEvent;\n" +
                    "\n" +
                    "@Name('B') " +
                    "insert into OutTwo select outOneCol||'x'||outOneCol as finalOut from OutOne;";
            DeploymentResult deploymentResultOne = _deploySvc.ParseDeploy(eplClientA);

            String eplClientB = "@Name('C') select * from OutTwo;";   // implicily bound to PN1
            DeploymentResult deploymentResultTwo = _deploySvc.ParseDeploy(eplClientB);

            _deploySvc.Undeploy(deploymentResultOne.DeploymentId);
            _deploySvc.Undeploy(deploymentResultTwo.DeploymentId);

            String eplClientC = "" +
                    "create schema InputEvent as (col1 string, col2 string);" +
                    "\n" +
                    "@Name('A') " +
                    "insert into OutOne select col1||col2 as outOneCol from InputEvent;" +
                    "\n" +
                    "@Name('B') " +
                    "insert into OutTwo select col2||col1 as outOneCol from InputEvent;";
            _deploySvc.ParseDeploy(eplClientC);

            String eplClientD = "@Name('C') select * from OutOne;" +
                    "@Name('D') select * from OutTwo;";
            _deploySvc.ParseDeploy(eplClientD);
        }

        [Test]
        public void TestNamedWindow()
        {
            DeploymentResult result = _deploySvc.ParseDeploy("create window MyWindow#time(30) as (col1 int, col2 string)",
                    null, null, null);
            _deploySvc.UndeployRemove(result.DeploymentId);

            result = _deploySvc.ParseDeploy("create window MyWindow#time(30) as (col1 short, col2 long)");
            _deploySvc.UndeployRemove(result.DeploymentId);
        }

        [Test]
        public void TestInsertInto()
        {
            DeploymentResult result = _deploySvc.ParseDeploy("create schema MySchema (col1 int, col2 string);"
                    + "insert into MyStream select * from MySchema;",
                    null, null, null);
            _deploySvc.UndeployRemove(result.DeploymentId);

            result = _deploySvc.ParseDeploy("create schema MySchema (col1 short, col2 long);"
                    + "insert into MyStream select * from MySchema;",
                    null, null, null);
            _deploySvc.UndeployRemove(result.DeploymentId);
        }

        [Test]
        public void TestVariables()
        {
            DeploymentResult result = _deploySvc.ParseDeploy("create variable int MyVar;"
                    + "create schema MySchema (col1 short, col2 long);"
                    + "select MyVar from MySchema;",
                    null, null, null);
            _deploySvc.UndeployRemove(result.DeploymentId);

            result = _deploySvc.ParseDeploy("create variable string MyVar;"
                    + "create schema MySchema (col1 short, col2 long);"
                    + "select MyVar from MySchema;",
                    null, null, null);
            _deploySvc.UndeployRemove(result.DeploymentId);
        }
    }
}
