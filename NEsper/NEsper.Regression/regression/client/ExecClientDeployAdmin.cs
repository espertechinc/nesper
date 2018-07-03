///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientDeployAdmin : RegressionExecution
    {
        private static readonly string NEWLINE = Environment.NewLine;

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionUserObjectAndStatementNameResolver(epService);
            RunAssertionExplicitDeploymentId(epService);
            RunAssertionTransition(epService);
            RunAssertionTransitionInvalid(epService);
            RunAssertionDeployImports(epService);
            RunAssertionDeploySingle(epService);
            RunAssertionLineNumberAndComments(epService);
            RunAssertionShortcutReadDeploy(epService);
            RunAssertionDeployUndeploy(epService);
            RunAssertionInvalidExceptionList(epService);
            RunAssertionFlagRollbackFailfastCompile(epService);
            RunAssertionFlagCompileOnly(epService);
            RunAssertionFlagValidateOnly(epService);
            RunAssertionFlagIsolated(epService);
            RunAssertionFlagUndeployNoDestroy(epService);
        }

        private void RunAssertionUserObjectAndStatementNameResolver(EPServiceProvider epService)
        {
            var module = epService.EPAdministrator.DeploymentAdmin.Parse(
                "select * from System.Object where 1=2; select * from System.Object where 3=4;");
            var options = new DeploymentOptions();
            options.StatementNameResolver = new ProxyStatementNameResolver
            {
                ProcGetStatementName = context => { return context.Epl.Contains("1=2") ? "StmtOne" : "StmtTwo"; }
            };
            options.StatementUserObjectResolver = new ProxyStatementUserObjectResolver
            {
                ProcGetUserObject = context => { return context.Epl.Contains("1=2") ? 100 : 200; }
            };

            epService.EPAdministrator.DeploymentAdmin.Deploy(module, options);

            Assert.AreEqual(100, epService.EPAdministrator.GetStatement("StmtOne").UserObject);
            Assert.AreEqual(200, epService.EPAdministrator.GetStatement("StmtTwo").UserObject);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionExplicitDeploymentId(EPServiceProvider epService)
        {
            // try module-add
            var module = epService.EPAdministrator.DeploymentAdmin.Parse("select * from System.Object");
            epService.EPAdministrator.DeploymentAdmin.Add(module, "ABC01");
            Assert.AreEqual(
                DeploymentState.UNDEPLOYED, epService.EPAdministrator.DeploymentAdmin.GetDeployment("ABC01").State);
            Assert.AreEqual(1, epService.EPAdministrator.DeploymentAdmin.Deployments.Length);

            epService.EPAdministrator.DeploymentAdmin.Deploy("ABC01", null);
            Assert.AreEqual(
                DeploymentState.DEPLOYED, epService.EPAdministrator.DeploymentAdmin.GetDeployment("ABC01").State);

            try
            {
                epService.EPAdministrator.DeploymentAdmin.Add(module, "ABC01");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Assigned deployment id 'ABC01' is already in use", ex.Message);
            }

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove("ABC01");
            Assert.AreEqual(0, epService.EPAdministrator.DeploymentAdmin.Deployments.Length);

            // try module-deploy
            var moduleTwo = epService.EPAdministrator.DeploymentAdmin.Parse("select * from System.Object");
            epService.EPAdministrator.DeploymentAdmin.Deploy(moduleTwo, null, "ABC02");
            Assert.AreEqual(
                DeploymentState.DEPLOYED, epService.EPAdministrator.DeploymentAdmin.GetDeployment("ABC02").State);
            Assert.AreEqual(1, epService.EPAdministrator.DeploymentAdmin.Deployments.Length);

            try
            {
                epService.EPAdministrator.DeploymentAdmin.Add(module, "ABC02");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Assigned deployment id 'ABC02' is already in use", ex.Message);
            }

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove("ABC02");
            Assert.AreEqual(0, epService.EPAdministrator.DeploymentAdmin.Deployments.Length);
        }

        private void RunAssertionTransition(EPServiceProvider epService)
        {
            // add module
            var module = MakeModule("com.testit", "create schema S1 as (col1 int)");
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.Add(module);
            var originalInfo = epService.EPAdministrator.DeploymentAdmin.GetDeployment(deploymentId);
            var addedDate = originalInfo.AddedDate;
            var lastUpdDate = originalInfo.LastUpdateDate;
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);
            Assert.AreEqual("com.testit", originalInfo.Module.Name);
            Assert.AreEqual(0, originalInfo.Items.Length);

            // deploy added module
            var result = epService.EPAdministrator.DeploymentAdmin.Deploy(deploymentId, null);
            Assert.AreEqual(deploymentId, result.DeploymentId);
            var info = epService.EPAdministrator.DeploymentAdmin.GetDeployment(deploymentId);
            Assert.AreEqual(DeploymentState.DEPLOYED, info.State);
            Assert.AreEqual("com.testit", info.Module.Name);
            Assert.AreEqual(addedDate, info.AddedDate);
            Assert.IsTrue(info.LastUpdateDate.TimeInMillis - lastUpdDate.TimeInMillis < 5000);
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);

            // undeploy module
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
            Assert.AreEqual(deploymentId, result.DeploymentId);
            info = epService.EPAdministrator.DeploymentAdmin.GetDeployment(deploymentId);
            Assert.AreEqual(DeploymentState.UNDEPLOYED, info.State);
            Assert.AreEqual("com.testit", info.Module.Name);
            Assert.AreEqual(addedDate, info.AddedDate);
            Assert.IsTrue(info.LastUpdateDate.TimeInMillis - lastUpdDate.TimeInMillis < 5000);
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);

            // remove module
            epService.EPAdministrator.DeploymentAdmin.Remove(deploymentId);
            Assert.IsNull(epService.EPAdministrator.DeploymentAdmin.GetDeployment(deploymentId));
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionTransitionInvalid(EPServiceProvider epService)
        {
            // invalid from deployed state
            var module = MakeModule("com.testit", "create schema S1 as (col1 int)");
            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.Deploy(module, null);
            try
            {
                epService.EPAdministrator.DeploymentAdmin.Deploy(deploymentResult.DeploymentId, null);
                Assert.Fail();
            }
            catch (DeploymentStateException ex)
            {
                Assert.IsTrue(ex.Message.Contains("is already in deployed state"));
            }

            try
            {
                epService.EPAdministrator.DeploymentAdmin.Remove(deploymentResult.DeploymentId);
                Assert.Fail();
            }
            catch (DeploymentStateException ex)
            {
                Assert.IsTrue(ex.Message.Contains("is in deployed state, please undeploy first"));
            }

            // invalid from undeployed state
            module = MakeModule("com.testit", "create schema S1 as (col1 int)");
            var deploymentId = epService.EPAdministrator.DeploymentAdmin.Add(module);
            try
            {
                epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentId);
                Assert.Fail();
            }
            catch (DeploymentStateException ex)
            {
                Assert.IsTrue(ex.Message.Contains("is already in undeployed state"));
            }

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deploymentId);
            Assert.IsNull(epService.EPAdministrator.DeploymentAdmin.GetDeployment(deploymentId));

            // not found
            Assert.IsNull(epService.EPAdministrator.DeploymentAdmin.GetDeployment("123"));
            try
            {
                epService.EPAdministrator.DeploymentAdmin.Deploy("123", null);
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex)
            {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }

            try
            {
                epService.EPAdministrator.DeploymentAdmin.Undeploy("123");
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex)
            {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }

            try
            {
                epService.EPAdministrator.DeploymentAdmin.Remove("123");
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex)
            {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }

            try
            {
                epService.EPAdministrator.DeploymentAdmin.UndeployRemove("123");
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex)
            {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }

            UndeployRemoveAll(epService);
        }

        private void RunAssertionDeployImports(EPServiceProvider epService)
        {
            var module = MakeModule(
                "com.testit", "create schema S1 as SupportBean",
                "@Name('A') select SupportStaticMethodLib.PlusOne(IntPrimitive) as val from S1");
            module.Imports.Add(typeof(SupportBean).FullName);
            module.Imports.Add(typeof(SupportStaticMethodLib).Namespace);
            Assert.IsFalse(epService.EPAdministrator.DeploymentAdmin.IsDeployed("com.testit"));
            epService.EPAdministrator.DeploymentAdmin.Deploy(module, null);
            Assert.IsTrue(epService.EPAdministrator.DeploymentAdmin.IsDeployed("com.testit"));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("A").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            Assert.AreEqual(5, listener.AssertOneGetNewAndReset().Get("val"));

            UndeployRemoveAll(epService);
        }

        private void RunAssertionDeploySingle(EPServiceProvider epService)
        {
            var module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_9.epl");
            var result = epService.EPAdministrator.DeploymentAdmin.Deploy(module, new DeploymentOptions());

            Assert.IsNotNull(result.DeploymentId);
            Assert.AreEqual(2, result.Statements.Count);
            Assert.AreEqual(2, epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual(
                "@Name(\"StmtOne\")" + NEWLINE +
                "create schema MyEvent(id String, val1 int, val2 int)",
                epService.EPAdministrator.GetStatement("StmtOne").Text);
            Assert.AreEqual(
                "@Name(\"StmtTwo\")" + NEWLINE +
                "select * from MyEvent", epService.EPAdministrator.GetStatement("StmtTwo").Text);

            Assert.AreEqual(1, epService.EPAdministrator.DeploymentAdmin.Deployments.Length);
            Assert.AreEqual(result.DeploymentId, epService.EPAdministrator.DeploymentAdmin.Deployments[0]);

            // test deploy with variable
            var moduleStr = "create variable integer snapshotOutputSecs = 10; " +
                            "create schema foo as (bar string); " +
                            "select bar from foo output snapshot every snapshotOutputSecs seconds;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleStr);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionLineNumberAndComments(EPServiceProvider epService)
        {
            var moduleText = NEWLINE + NEWLINE + "select * from ABC;" +
                             NEWLINE + "select * from DEF";

            var module = epService.EPAdministrator.DeploymentAdmin.Parse(moduleText);
            Assert.AreEqual(2, module.Items.Count);
            Assert.AreEqual(3, module.Items[0].LineNumber);
            Assert.AreEqual(4, module.Items[1].LineNumber);

            module = epService.EPAdministrator.DeploymentAdmin.Parse("/* abc */");
            epService.EPAdministrator.DeploymentAdmin.Deploy(module, new DeploymentOptions());

            module = epService.EPAdministrator.DeploymentAdmin.Parse(
                "select * from System.Object; \r\n/* abc */\r\n");
            epService.EPAdministrator.DeploymentAdmin.Deploy(module, new DeploymentOptions());

            UndeployRemoveAll(epService);
        }

        private void RunAssertionShortcutReadDeploy(EPServiceProvider epService)
        {
            var resource = "regression/test_module_12.epl";
            var input = SupportContainer.Instance.ResourceManager().GetResourceAsStream(resource);
            Assert.IsNotNull(input);
            var resultOne = epService.EPAdministrator.DeploymentAdmin.ReadDeploy(input, null, null, null);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultOne.DeploymentId);
            Assert.IsNull(epService.EPAdministrator.DeploymentAdmin.GetDeployment(resultOne.DeploymentId));

            resultOne = epService.EPAdministrator.DeploymentAdmin.ReadDeploy(resource, "uri1", "archive1", "obj1");
            Assert.AreEqual(
                "regression.test",
                epService.EPAdministrator.DeploymentAdmin.GetDeployment(resultOne.DeploymentId).Module.Name);
            Assert.AreEqual(2, resultOne.Statements.Count);
            Assert.AreEqual("create schema MyType(col1 integer)", resultOne.Statements[0].Text);
            Assert.IsTrue(epService.EPAdministrator.DeploymentAdmin.IsDeployed("regression.test"));
            Assert.AreEqual(
                "module regression.test;" + NEWLINE + NEWLINE +
                "create schema MyType(col1 integer);" + NEWLINE +
                "select * from MyType;" + NEWLINE,
                epService.EPAdministrator.DeploymentAdmin.GetDeployment(resultOne.DeploymentId).Module.ModuleText);

            var moduleText = "module regression.test.two;" +
                             "uses regression.test;" +
                             "create schema MyTypeTwo(col1 integer, col2.col3 string);" +
                             "select * from MyTypeTwo;";
            var resultTwo =
                epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleText, "uri2", "archive2", "obj2");
            var infos = epService.EPAdministrator.DeploymentAdmin.DeploymentInformation;
            Assert.AreEqual(2, infos.Length);

            var infoList = new List<DeploymentInformation>(infos);
            infoList.Sort((o1, o2) => o1.Module.Name.CompareTo(o2.Module.Name));

            var infoOne = infoList[0];
            var infoTwo = infoList[1];
            Assert.AreEqual("regression.test", infoOne.Module.Name);
            Assert.AreEqual("uri1", infoOne.Module.Uri);
            Assert.AreEqual("archive1", infoOne.Module.ArchiveName);
            Assert.AreEqual("obj1", infoOne.Module.UserObject);
            Assert.IsNotNull(infoOne.AddedDate);
            Assert.IsNotNull(infoOne.LastUpdateDate);
            Assert.AreEqual(DeploymentState.DEPLOYED, infoOne.State);
            Assert.AreEqual("regression.test.two", infoTwo.Module.Name);
            Assert.AreEqual("uri2", infoTwo.Module.Uri);
            Assert.AreEqual("archive2", infoTwo.Module.ArchiveName);
            Assert.AreEqual("obj2", infoTwo.Module.UserObject);
            Assert.IsNotNull(infoTwo.AddedDate);
            Assert.IsNotNull(infoTwo.LastUpdateDate);
            Assert.AreEqual(DeploymentState.DEPLOYED, infoTwo.State);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionDeployUndeploy(EPServiceProvider epService)
        {
            var moduleOne = MakeModule(
                "mymodule.one", "@Name('A1') create schema MySchemaOne (col1 int)",
                "@Name('B1') select * from MySchemaOne");
            var resultOne = epService.EPAdministrator.DeploymentAdmin.Deploy(moduleOne, new DeploymentOptions());
            Assert.AreEqual(2, resultOne.Statements.Count);
            Assert.IsTrue(epService.EPAdministrator.DeploymentAdmin.IsDeployed("mymodule.one"));

            var moduleTwo = MakeModule(
                "mymodule.two", "@Name('A2') create schema MySchemaTwo (col1 int)",
                "@Name('B2') select * from MySchemaTwo");
            moduleTwo.UserObject = 100L;
            moduleTwo.ArchiveName = "archive";
            var resultTwo = epService.EPAdministrator.DeploymentAdmin.Deploy(moduleTwo, new DeploymentOptions());
            Assert.AreEqual(2, resultTwo.Statements.Count);

            var info = epService.EPAdministrator.DeploymentAdmin.DeploymentInformation;
            var infoList = new List<DeploymentInformation>(info);
            infoList.Sort((o1, o2) => { return o1.Module.Name.CompareTo(o2.Module.Name); });

            Assert.AreEqual(2, info.Length);
            Assert.AreEqual(resultOne.DeploymentId, infoList[0].DeploymentId);
            Assert.IsNotNull(infoList[0].LastUpdateDate);
            Assert.AreEqual("mymodule.one", infoList[0].Module.Name);
            Assert.AreEqual(null, infoList[0].Module.Uri);
            Assert.AreEqual(0, infoList[0].Module.Uses.Count);
            Assert.AreEqual(resultTwo.DeploymentId, infoList[1].DeploymentId);
            Assert.AreEqual(100L, infoList[1].Module.UserObject);
            Assert.AreEqual("archive", infoList[1].Module.ArchiveName);
            Assert.AreEqual(2, infoList[1].Items.Length);
            Assert.AreEqual("A2", infoList[1].Items[0].StatementName);
            Assert.AreEqual("@Name('A2') create schema MySchemaTwo (col1 int)", infoList[1].Items[0].Expression);
            Assert.AreEqual("B2", infoList[1].Items[1].StatementName);
            Assert.AreEqual("@Name('B2') select * from MySchemaTwo", infoList[1].Items[1].Expression);
            Assert.AreEqual(4, epService.EPAdministrator.StatementNames.Count);

            var result = epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultTwo.DeploymentId);
            Assert.AreEqual(2, epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual(2, result.StatementInfo.Count);
            Assert.AreEqual("A2", result.StatementInfo[0].StatementName);
            Assert.AreEqual("@Name('A2') create schema MySchemaTwo (col1 int)", result.StatementInfo[0].Expression);
            Assert.AreEqual("B2", result.StatementInfo[1].StatementName);
            Assert.AreEqual("@Name('B2') select * from MySchemaTwo", result.StatementInfo[1].Expression);

            result = epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultOne.DeploymentId);
            Assert.AreEqual(0, epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual(2, result.StatementInfo.Count);
            Assert.AreEqual("A1", result.StatementInfo[0].StatementName);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionInvalidExceptionList(EPServiceProvider epService)
        {
            var moduleOne = MakeModule(
                "mymodule.one", "create schema MySchemaOne (col1 Wrong)", "create schema MySchemaOne (col2 WrongTwo)");
            try
            {
                var options = new DeploymentOptions();
                options.IsFailFast = false;
                epService.EPAdministrator.DeploymentAdmin.Deploy(moduleOne, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex)
            {
                Assert.AreEqual(
                    "Deployment failed in module 'mymodule.one' in expression 'create schema MySchemaOne (col1 Wrong)' : Error starting statement: Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema MySchemaOne (col1 Wrong)]",
                    ex.Message);
                Assert.AreEqual(2, ex.Exceptions.Count);
                Assert.AreEqual("create schema MySchemaOne (col1 Wrong)", ex.Exceptions[0].Expression);
                Assert.AreEqual(
                    "Error starting statement: Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema MySchemaOne (col1 Wrong)]",
                    ex.Exceptions[0].Inner.Message);
                Assert.AreEqual("create schema MySchemaOne (col2 WrongTwo)", ex.Exceptions[1].Expression);
                Assert.AreEqual(
                    "Error starting statement: Nestable type configuration encountered an unexpected property type name 'WrongTwo' for property 'col2', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema MySchemaOne (col2 WrongTwo)]",
                    ex.Exceptions[1].Inner.Message);
            }

            // test NEWLINE as part of the failing expression - replaced by space
            try
            {
                epService.EPAdministrator.DeploymentAdmin.ParseDeploy("XX\nX");
                Assert.Fail();
            }
            catch (DeploymentException ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex, "Compilation failed in expression 'XX X' : Incorrect syntax near 'XX' [");
            }
        }

        private void RunAssertionFlagRollbackFailfastCompile(EPServiceProvider epService)
        {
            var textOne = "@Name('A') create schema MySchemaTwo (col1 int)";
            var textTwo = "@Name('B') create schema MySchemaTwo (col1 not_existing_type)";
            var errorTextTwo =
                "Error starting statement: Nestable type configuration encountered an unexpected property type name 'not_existing_type' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [@Name('B') create schema MySchemaTwo (col1 not_existing_type)]";
            var textThree = "@Name('C') create schema MySchemaTwo (col1 int)";
            var module = MakeModule("mymodule.two", textOne, textTwo, textThree);

            try
            {
                var options = new DeploymentOptions();
                epService.EPAdministrator.DeploymentAdmin.Deploy(module, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex)
            {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(textTwo, first.Expression);
                Assert.AreEqual(errorTextTwo, first.Inner.Message);
            }

            Assert.AreEqual(0, epService.EPAdministrator.StatementNames.Count);

            try
            {
                var options = new DeploymentOptions();
                options.IsRollbackOnFail = false;
                epService.EPAdministrator.DeploymentAdmin.Deploy(module, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex)
            {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(textTwo, first.Expression);
                Assert.AreEqual(errorTextTwo, first.Inner.Message);
                EPAssertionUtil.AssertEqualsExactOrder(epService.EPAdministrator.StatementNames, new[] {"A"});
                epService.EPAdministrator.GetStatement("A").Dispose();
            }

            try
            {
                var options = new DeploymentOptions();
                options.IsRollbackOnFail = false;
                options.IsFailFast = false;
                epService.EPAdministrator.DeploymentAdmin.Deploy(module, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex)
            {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(textTwo, first.Expression);
                Assert.AreEqual(errorTextTwo, first.Inner.Message);
                EPAssertionUtil.AssertEqualsAnyOrder(
                    new[] {"A", "C"}, epService.EPAdministrator.StatementNames);
            }
        }

        private void RunAssertionFlagCompileOnly(EPServiceProvider epService)
        {
            var text = "create schema SomeSchema (col1 NotExists)";
            var error =
                "Error starting statement: Nestable type configuration encountered an unexpected property type name 'NotExists' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema SomeSchema (col1 NotExists)]";

            try
            {
                epService.EPAdministrator.DeploymentAdmin.Deploy(MakeModule("test", text), null);
                Assert.Fail();
            }
            catch (DeploymentActionException ex)
            {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(error, first.Inner.Message);
            }

            var options = new DeploymentOptions();
            options.IsCompileOnly = true;
            Assert.IsNull(epService.EPAdministrator.DeploymentAdmin.Deploy(MakeModule("test", text), options));
        }

        private void RunAssertionFlagValidateOnly(EPServiceProvider epService)
        {
            UndeployRemoveAll(epService);
            epService.EPAdministrator.DestroyAllStatements();

            var textOne = "@Name('A') create schema MySchemaTwo (col1 int)";
            var textTwo = "@Name('B') select * from MySchemaTwo";
            var module = MakeModule("mymodule.two", textOne, textTwo);

            var options = new DeploymentOptions();
            options.IsValidateOnly = true;
            var result = epService.EPAdministrator.DeploymentAdmin.Deploy(module, options);
            Assert.IsNull(result);
            Assert.AreEqual(0, epService.EPAdministrator.StatementNames.Count);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionFlagIsolated(EPServiceProvider epService)
        {
            var textOne = "@Name('A') create schema MySchemaTwo (col1 int)";
            var textTwo = "@Name('B') select * from MySchemaTwo";
            var module = MakeModule("mymodule.two", textOne, textTwo);

            var options = new DeploymentOptions();
            options.IsolatedServiceProvider = "iso1";
            var result = epService.EPAdministrator.DeploymentAdmin.Deploy(module, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual("iso1", epService.EPAdministrator.GetStatement("A").ServiceIsolated);
            Assert.AreEqual("iso1", epService.EPAdministrator.GetStatement("B").ServiceIsolated);

            UndeployRemoveAll(epService);
        }

        private void RunAssertionFlagUndeployNoDestroy(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            var resultOne =
                epService.EPAdministrator.DeploymentAdmin.ParseDeploy("@Name('S0') select * from SupportBean");
            var resultTwo =
                epService.EPAdministrator.DeploymentAdmin.ParseDeploy("@Name('S1') select * from SupportBean");

            var options = new UndeploymentOptions();
            options.IsDestroyStatements = false;
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultOne.DeploymentId, options);
            Assert.IsNotNull(epService.EPAdministrator.GetStatement("S0"));

            epService.EPAdministrator.DeploymentAdmin.Undeploy(resultTwo.DeploymentId, options);
            Assert.IsNotNull(epService.EPAdministrator.GetStatement("S1"));
        }

        private Module MakeModule(string name, params string[] statements)
        {
            var items = new ModuleItem[statements.Length];
            for (var i = 0; i < statements.Length; i++)
            {
                items[i] = new ModuleItem(statements[i], false, 0, 0, 0);
            }

            return new Module(
                name,
                null,
                new HashSet<string>(),
                new HashSet<string>(),
                items,
                null);
        }

        private void UndeployRemoveAll(EPServiceProvider epService)
        {
            var deployments = epService.EPAdministrator.DeploymentAdmin.DeploymentInformation;
            if (deployments != null) {
                foreach (var deployment in deployments) {
                    epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployment.DeploymentId);
                }
            }
        }
    }
} // end of namespace