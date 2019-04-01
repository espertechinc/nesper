///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientDeployRedefinition : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCreateSchemaNamedWindowInsert(epService);
            RunAssertionRedefDeployOrder(epService);
            RunAssertionNamedWindow(epService);
            RunAssertionInsertInto(epService);
            RunAssertionVariables(epService);
        }
    
        private void RunAssertionCreateSchemaNamedWindowInsert(EPServiceProvider epService) {
    
            string text = "module test.test1;\n" +
                    "create schema MyTypeOne(col1 string, col2 int);" +
                    "create window MyWindowOne#keepall as select * from MyTypeOne;" +
                    "insert into MyWindowOne select * from MyTypeOne;";
    
            DeploymentResult resultOne = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(text, "uri1", "arch1", null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultOne.DeploymentId);
    
            DeploymentResult resultTwo = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(text, "uri2", "arch2", null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultTwo.DeploymentId);
            text = "module test.test1;\n" +
                    "create schema MyTypeOne(col1 string, col2 int, col3 long);" +
                    "create window MyWindowOne#keepall as select * from MyTypeOne;" +
                    "insert into MyWindowOne select * from MyTypeOne;";
    
            DeploymentResult resultThree = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(text, "uri1", "arch1", null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultThree.DeploymentId);
    
            FilterService filterService = ((EPServiceProviderSPI) epService).FilterService;
            FilterServiceSPI filterSPI = (FilterServiceSPI) filterService;
            Assert.AreEqual(0, filterSPI.CountTypes);
    
            // test on-merge
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string moduleString =
                    "@Name('S0') create window MyWindow#unique(IntPrimitive) as SupportBean;\n" +
                            "@Name('S1') on MyWindow insert into SecondStream select *;\n" +
                            "@Name('S2') on SecondStream merge MyWindow when matched then insert into ThirdStream select * then delete\n";
            Module module = epService.EPAdministrator.DeploymentAdmin.Parse(moduleString);
            epService.EPAdministrator.DeploymentAdmin.Deploy(module, null, "myid_101");
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove("myid_101");
            epService.EPAdministrator.DeploymentAdmin.Deploy(module, null, "myid_101");
    
            // test table
            string moduleTableOne = "create table MyTable(c0 string, c1 string)";
            DeploymentResult d = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleTableOne);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(d.DeploymentId);
            string moduleTableTwo = "create table MyTable(c0 string, c1 string, c2 string)";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleTableTwo);
    
            UndeployRemoveAll(epService);
        }
    
        private void RunAssertionRedefDeployOrder(EPServiceProvider epService) {
            string eplClientA = "" +
                    "create schema InputEvent as (col1 string, col2 string);" +
                    "\n" +
                    "@Name('A') " +
                    "insert into OutOne select col1||col2 as outOneCol from InputEvent;\n" +
                    "\n" +
                    "@Name('B') " +
                    "insert into OutTwo select outOneCol||'x'||outOneCol as finalOut from OutOne;";
            DeploymentResult deploymentResultOne = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplClientA);
    
            string eplClientB = "@Name('C') select * from OutTwo;";   // implicily bound to PN1
            DeploymentResult deploymentResultTwo = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplClientB);
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResultOne.DeploymentId);
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResultTwo.DeploymentId);
    
            string eplClientC = "" +
                    "create schema InputEvent as (col1 string, col2 string);" +
                    "\n" +
                    "@Name('A') " +
                    "insert into OutOne select col1||col2 as outOneCol from InputEvent;" +
                    "\n" +
                    "@Name('B') " +
                    "insert into OutTwo select col2||col1 as outOneCol from InputEvent;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplClientC);
    
            string eplClientD = "@Name('C') select * from OutOne;" +
                    "@Name('D') select * from OutTwo;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplClientD);
    
            UndeployRemoveAll(epService);
        }
    
        private void RunAssertionNamedWindow(EPServiceProvider epService) {
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy("create window MyWindow#time(30) as (col1 int, col2 string)",
                    null, null, null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
    
            result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy("create window MyWindow#time(30) as (col1 short, col2 long)");
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionInsertInto(EPServiceProvider epService) {
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy("create schema MySchema (col1 int, col2 string);"
                            + "insert into MyStream select * from MySchema;",
                    null, null, null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
    
            result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy("create schema MySchema (col1 short, col2 long);"
                            + "insert into MyStream select * from MySchema;",
                    null, null, null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionVariables(EPServiceProvider epService) {
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy("create variable int MyVar;"
                            + "create schema MySchema (col1 short, col2 long);"
                            + "select MyVar from MySchema;",
                    null, null, null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
    
            result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy("create variable string MyVar;"
                            + "create schema MySchema (col1 short, col2 long);"
                            + "select MyVar from MySchema;",
                    null, null, null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void UndeployRemoveAll(EPServiceProvider epService) {
            DeploymentInformation[] deployments = epService.EPAdministrator.DeploymentAdmin.DeploymentInformation;
            foreach (DeploymentInformation deployment in deployments) {
                epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployment.DeploymentId);
            }
        }
    }
} // end of namespace
