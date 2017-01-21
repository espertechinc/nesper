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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestDeployAdmin 
    {
        private static readonly String Newline = Environment.NewLine;
    
        private EPServiceProvider _epService;
        private EPDeploymentAdmin _deploymentAdmin;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _deploymentAdmin = _epService.EPAdministrator.DeploymentAdmin;
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _deploymentAdmin = null;
        }
    
        [Test]
        public void TestUserObjectAndStatementNameResolver() {
            var module = _deploymentAdmin.Parse("select * from System.Object where 1=2; select * from System.Object where 3=4;");
            var options = new DeploymentOptions();
            options.StatementNameResolver = new ProxyStatementNameResolver
            {
                ProcGetStatementName = context => context.Epl.Contains("1=2") ? "StmtOne" : "StmtTwo"
            };
            options.StatementUserObjectResolver = new ProxyStatementUserObjectResolver
            {
                ProcGetUserObject = context => context.Epl.Contains("1=2") ? 100 : 200
            };

            _deploymentAdmin.Deploy(module, options);
    
            Assert.AreEqual(100, _epService.EPAdministrator.GetStatement("StmtOne").UserObject);
            Assert.AreEqual(200, _epService.EPAdministrator.GetStatement("StmtTwo").UserObject);
        }
    
        [Test]
        public void TestExplicitDeploymentId()
        {
            // try module-add
            var module = _deploymentAdmin.Parse("select * from System.Object");
            _deploymentAdmin.Add(module, "ABC01");
            Assert.AreEqual(DeploymentState.UNDEPLOYED, _deploymentAdmin.GetDeployment("ABC01").State);
            Assert.AreEqual(1, _deploymentAdmin.Deployments.Length);
    
            _deploymentAdmin.Deploy("ABC01", null);
            Assert.AreEqual(DeploymentState.DEPLOYED, _deploymentAdmin.GetDeployment("ABC01").State);
    
            try {
                _deploymentAdmin.Add(module, "ABC01");
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual("Assigned deployment id 'ABC01' is already in use", ex.Message);
            }
            _deploymentAdmin.UndeployRemove("ABC01");
            Assert.AreEqual(0, _deploymentAdmin.Deployments.Length);
    
            // try module-deploy
            var moduleTwo = _deploymentAdmin.Parse("select * from System.Object");
            _deploymentAdmin.Deploy(moduleTwo, null, "ABC02");
            Assert.AreEqual(DeploymentState.DEPLOYED, _deploymentAdmin.GetDeployment("ABC02").State);
            Assert.AreEqual(1, _deploymentAdmin.Deployments.Length);
    
            try {
                _deploymentAdmin.Add(module, "ABC02");
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual("Assigned deployment id 'ABC02' is already in use", ex.Message);
            }
            _deploymentAdmin.UndeployRemove("ABC02");
            Assert.AreEqual(0, _deploymentAdmin.Deployments.Length);
        }
    
        [Test]
        public void TestTransition()
        {
            // add module
            var module = MakeModule("com.testit", "create schema S1 as (col1 int)");
            var deploymentId = _deploymentAdmin.Add(module);
            var originalInfo = _deploymentAdmin.GetDeployment(deploymentId);
            var addedDate = originalInfo.AddedDate;
            var lastUpdDate = originalInfo.LastUpdateDate;
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);
            Assert.AreEqual("com.testit", originalInfo.Module.Name);
            Assert.AreEqual(0, originalInfo.Items.Length);
    
            // deploy added module
            var result = _deploymentAdmin.Deploy(deploymentId, null);
            Assert.AreEqual(deploymentId, result.DeploymentId);
            var info = _deploymentAdmin.GetDeployment(deploymentId);
            Assert.AreEqual(DeploymentState.DEPLOYED, info.State);
            Assert.AreEqual("com.testit", info.Module.Name);
            Assert.AreEqual(addedDate, info.AddedDate);
            Assert.IsTrue(info.LastUpdateDate.TimeInMillis() - lastUpdDate.TimeInMillis() < 5000);
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);
    
            // undeploy module
            _deploymentAdmin.Undeploy(deploymentId);
            Assert.AreEqual(deploymentId, result.DeploymentId);
            info = _deploymentAdmin.GetDeployment(deploymentId);
            Assert.AreEqual(DeploymentState.UNDEPLOYED, info.State);
            Assert.AreEqual("com.testit", info.Module.Name);
            Assert.AreEqual(addedDate, info.AddedDate);
            Assert.IsTrue(info.LastUpdateDate.TimeInMillis() - lastUpdDate.TimeInMillis() < 5000);
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);
    
            // remove module
            _deploymentAdmin.Remove(deploymentId);
            Assert.IsNull(_deploymentAdmin.GetDeployment(deploymentId));
            Assert.AreEqual(DeploymentState.UNDEPLOYED, originalInfo.State);
        }
    
        [Test]
        public void TestTransitionInvalid()
        {
            // invalid from deployed state
            var module = MakeModule("com.testit", "create schema S1 as (col1 int)");
            var deploymentResult = _deploymentAdmin.Deploy(module, null);
            try {
                _deploymentAdmin.Deploy(deploymentResult.DeploymentId, null);
                Assert.Fail();
            }
            catch (DeploymentStateException ex) {
                Assert.IsTrue(ex.Message.Contains("is already in deployed state"));
            }
    
            try {
                _deploymentAdmin.Remove(deploymentResult.DeploymentId);
                Assert.Fail();
            }
            catch (DeploymentStateException ex) {
                Assert.IsTrue(ex.Message.Contains("is in deployed state, please undeploy first"));
            }
    
            // invalid from undeployed state
            module = MakeModule("com.testit", "create schema S1 as (col1 int)");
            var deploymentId = _deploymentAdmin.Add(module);
            try {
                _deploymentAdmin.Undeploy(deploymentId);
                Assert.Fail();
            }
            catch (DeploymentStateException ex) {
                Assert.IsTrue(ex.Message.Contains("is already in undeployed state"));
            }
            _deploymentAdmin.UndeployRemove(deploymentId);
            Assert.IsNull(_deploymentAdmin.GetDeployment(deploymentId));
    
            // not found
            Assert.IsNull(_deploymentAdmin.GetDeployment("123"));
            try {
                _deploymentAdmin.Deploy("123", null);
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex) {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }
    
            try {
                _deploymentAdmin.Undeploy("123");
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex) {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }
    
            try {
                _deploymentAdmin.Remove("123");
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex) {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }
    
            try {
                _deploymentAdmin.UndeployRemove("123");
                Assert.Fail();
            }
            catch (DeploymentNotFoundException ex) {
                Assert.AreEqual("Deployment by id '123' could not be found", ex.Message);
            }
        }
    
        [Test]
        public void TestDeployImports()
        {
            var module = MakeModule("com.testit", "create schema S1 as SupportBean", "@Name('A') select SupportStaticMethodLib.PlusOne(IntPrimitive) as val from S1");
            module.Imports.Add(typeof(SupportBean).FullName);
            module.Imports.Add(typeof(SupportStaticMethodLib).Namespace);
            Assert.IsFalse(_deploymentAdmin.IsDeployed("com.testit"));
            _deploymentAdmin.Deploy(module, null);
            Assert.IsTrue(_deploymentAdmin.IsDeployed("com.testit"));
            _epService.EPAdministrator.GetStatement("A").Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            Assert.AreEqual(5, _listener.AssertOneGetNewAndReset().Get("val"));
        }
    
        [Test]
        public void TestDeploySingle()
        {
            var module = _deploymentAdmin.Read("regression/test_module_9.epl");
            var result = _deploymentAdmin.Deploy(module, new DeploymentOptions());
    
            Assert.NotNull(result.DeploymentId);
            Assert.AreEqual(2, result.Statements.Count);
            Assert.AreEqual(2, _epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual("@Name(\"StmtOne\")" + Newline +
                    "create schema MyEvent(id String, val1 int, val2 int)", _epService.EPAdministrator.GetStatement("StmtOne").Text);
            Assert.AreEqual("@Name(\"StmtTwo\")" + Newline +
                    "select * from MyEvent", _epService.EPAdministrator.GetStatement("StmtTwo").Text);
    
            Assert.AreEqual(1, _deploymentAdmin.Deployments.Length);
            Assert.AreEqual(result.DeploymentId, _deploymentAdmin.Deployments[0]);
    
            // test deploy with variable
            var moduleStr = "create variable integer snapshotOutputSecs = 10; " +
                    "create schema foo as (bar string); " +
                    "select bar from foo output snapshot every snapshotOutputSecs seconds;";
            _deploymentAdmin.ParseDeploy(moduleStr);
        }
    
        [Test]
        public void TestLineNumberAndComments()
        {
            var moduleText = Newline + Newline + "select * from ABC;" +
                             Newline + "select * from DEF";
            
            var module = _deploymentAdmin.Parse(moduleText);
            Assert.AreEqual(2, module.Items.Count);
            Assert.AreEqual(3, module.Items[0].LineNumber);
            Assert.AreEqual(4, module.Items[1].LineNumber);
    
            module = _deploymentAdmin.Parse("/* abc */");
    		_deploymentAdmin.Deploy(module, new DeploymentOptions());
    
            module = _deploymentAdmin.Parse("select * from System.Object; \r\n/* abc */\r\n");
    		_deploymentAdmin.Deploy(module, new DeploymentOptions());
        }
    
        [Test]
        public void TestShortcutReadDeploy()
        {
            var resource = "regression/test_module_12.epl";
            var input = ResourceManager.GetResourceAsStream(resource);
            Assert.NotNull(input);
            var resultOne = _deploymentAdmin.ReadDeploy(input, null, null, null);
            _deploymentAdmin.UndeployRemove(resultOne.DeploymentId);
            Assert.IsNull(_deploymentAdmin.GetDeployment(resultOne.DeploymentId));
    
            resultOne = _deploymentAdmin.ReadDeploy(resource, "uri1", "archive1", "obj1");
            Assert.AreEqual("regression.test", _deploymentAdmin.GetDeployment(resultOne.DeploymentId).Module.Name);
            Assert.AreEqual(2, resultOne.Statements.Count);
            Assert.AreEqual("create schema MyType(col1 integer)", resultOne.Statements[0].Text);
            Assert.IsTrue(_deploymentAdmin.IsDeployed("regression.test"));
            Assert.AreEqual("module regression.test;" + Newline + Newline +
                    "create schema MyType(col1 integer);" + Newline +
                    "select * from MyType;" + Newline, _deploymentAdmin.GetDeployment(resultOne.DeploymentId).Module.ModuleText);
    
            var moduleText = "module regression.test.two;" +
                    "uses regression.test;" +
                    "create schema MyTypeTwo(col1 integer, col2.col3 string);" +
                    "select * from MyTypeTwo;";
            var resultTwo = _deploymentAdmin.ParseDeploy(moduleText, "uri2", "archive2", "obj2");
            var infos = _deploymentAdmin.DeploymentInformation;
            Assert.AreEqual(2, infos.Length);

            IList<DeploymentInformation> infoList = infos.OrderBy(info => info.Module.Name).ToList();

            var infoOne = infoList[0];
            var infoTwo = infoList[1];
            Assert.AreEqual("regression.test", infoOne.Module.Name);
            Assert.AreEqual("uri1", infoOne.Module.Uri);
            Assert.AreEqual("archive1", infoOne.Module.ArchiveName);
            Assert.AreEqual("obj1", infoOne.Module.UserObject);
            Assert.NotNull(infoOne.AddedDate);
            Assert.NotNull(infoOne.LastUpdateDate);
            Assert.AreEqual(DeploymentState.DEPLOYED, infoOne.State);
            Assert.AreEqual("regression.test.two", infoTwo.Module.Name);
            Assert.AreEqual("uri2", infoTwo.Module.Uri);
            Assert.AreEqual("archive2", infoTwo.Module.ArchiveName);
            Assert.AreEqual("obj2", infoTwo.Module.UserObject);
            Assert.NotNull(infoTwo.AddedDate);
            Assert.NotNull(infoTwo.LastUpdateDate);
            Assert.AreEqual(DeploymentState.DEPLOYED, infoTwo.State);
        }
    
        [Test]
        public void TestDeployUndeploy() {
            var moduleOne = MakeModule("mymodule.one", "@Name('A1') create schema MySchemaOne (col1 int)", "@Name('B1') select * from MySchemaOne");
            var resultOne = _deploymentAdmin.Deploy(moduleOne, new DeploymentOptions());
            Assert.AreEqual(2, resultOne.Statements.Count);
            Assert.IsTrue(_deploymentAdmin.IsDeployed("mymodule.one"));
    
            var moduleTwo = MakeModule("mymodule.two", "@Name('A2') create schema MySchemaTwo (col1 int)", "@Name('B2') select * from MySchemaTwo");
            moduleTwo.UserObject = 100L;
            moduleTwo.ArchiveName = "archive";
            var resultTwo = _deploymentAdmin.Deploy(moduleTwo, new DeploymentOptions());
            Assert.AreEqual(2, resultTwo.Statements.Count);
            
            var info = _epService.EPAdministrator.DeploymentAdmin.DeploymentInformation;
            IList<DeploymentInformation> infoList = info.OrderBy(i => i.Module.Name).ToList();
            Assert.AreEqual(2, info.Length);
            Assert.AreEqual(resultOne.DeploymentId, infoList[0].DeploymentId);
            Assert.NotNull(infoList[0].LastUpdateDate);
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
            Assert.AreEqual(4, _epService.EPAdministrator.StatementNames.Count);
            
            var result = _deploymentAdmin.UndeployRemove(resultTwo.DeploymentId);
            Assert.AreEqual(2, _epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual(2, result.StatementInfo.Count);
            Assert.AreEqual("A2", result.StatementInfo[0].StatementName);
            Assert.AreEqual("@Name('A2') create schema MySchemaTwo (col1 int)", result.StatementInfo[0].Expression);
            Assert.AreEqual("B2", result.StatementInfo[1].StatementName);
            Assert.AreEqual("@Name('B2') select * from MySchemaTwo", result.StatementInfo[1].Expression);
    
            result = _deploymentAdmin.UndeployRemove(resultOne.DeploymentId);
            Assert.AreEqual(0, _epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual(2, result.StatementInfo.Count);
            Assert.AreEqual("A1", result.StatementInfo[0].StatementName);
        }
    
        [Test]
        public void TestInvalidExceptionList() {
            var moduleOne = MakeModule("mymodule.one", "create schema MySchemaOne (col1 Wrong)", "create schema MySchemaOne (col2 WrongTwo)");
            try {
                var options = new DeploymentOptions();
                options.IsFailFast = false;
                _deploymentAdmin.Deploy(moduleOne, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex) {
                Assert.AreEqual("Deployment failed in module 'mymodule.one' in expression 'create schema MySchemaOne (col1 Wrong)' : Error starting statement: Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema MySchemaOne (col1 Wrong)]", ex.Message);
                Assert.AreEqual(2,  ex.Exceptions.Count);
                Assert.AreEqual("create schema MySchemaOne (col1 Wrong)", ex.Exceptions[0].Expression);
                Assert.AreEqual("Error starting statement: Nestable type configuration encountered an unexpected property type name 'Wrong' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema MySchemaOne (col1 Wrong)]", ex.Exceptions[0].Inner.Message);
                Assert.AreEqual("create schema MySchemaOne (col2 WrongTwo)", ex.Exceptions[1].Expression);
                Assert.AreEqual("Error starting statement: Nestable type configuration encountered an unexpected property type name 'WrongTwo' for property 'col2', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema MySchemaOne (col2 WrongTwo)]", ex.Exceptions[1].Inner.Message);
            }

            // test newline as part of the failing expression - replaced by space
            try
            {
                _deploymentAdmin.ParseDeploy("XX\nX");
                Assert.Fail();
            }
            catch (DeploymentException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, "Compilation failed in expression 'XX X' : Incorrect syntax near 'XX' [");
            }
        }
    
        [Test]
        public void TestFlagRollbackFailfastCompile() {
    
            var textOne = "@Name('A') create schema MySchemaTwo (col1 int)";
            var textTwo = "@Name('B') create schema MySchemaTwo (col1 not_existing_type)";
            var errorTextTwo = "Error starting statement: Nestable type configuration encountered an unexpected property type name 'not_existing_type' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [@Name('B') create schema MySchemaTwo (col1 not_existing_type)]";
            var textThree = "@Name('C') create schema MySchemaTwo (col1 int)";
            var module = MakeModule("mymodule.two", textOne, textTwo, textThree);
    
            try {
                var options = new DeploymentOptions();
                _deploymentAdmin.Deploy(module, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex) {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(textTwo, first.Expression);
                Assert.AreEqual(errorTextTwo, first.Inner.Message);
            }
            Assert.AreEqual(0, _epService.EPAdministrator.StatementNames.Count);
    
            try {
                var options = new DeploymentOptions();
                options.IsRollbackOnFail = false;
                _deploymentAdmin.Deploy(module, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex) {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(textTwo, first.Expression);
                Assert.AreEqual(errorTextTwo, first.Inner.Message);
                EPAssertionUtil.AssertEqualsExactOrder(_epService.EPAdministrator.StatementNames, new String[]{"A"});
                _epService.EPAdministrator.GetStatement("A").Dispose();
            }
    
            try {
                var options = new DeploymentOptions();
                options.IsRollbackOnFail = false;
                options.IsFailFast = false;
                _deploymentAdmin.Deploy(module, options);
                Assert.Fail();
            }
            catch (DeploymentActionException ex) {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(textTwo, first.Expression);
                Assert.AreEqual(errorTextTwo, first.Inner.Message);
                EPAssertionUtil.AssertEqualsExactOrder(new String[]{"A", "C"}, _epService.EPAdministrator.StatementNames);
            }
        }
    
        [Test]
        public void TestFlagCompileOnly() {
            var text = "create schema SomeSchema (col1 NotExists)";
            var error = "Error starting statement: Nestable type configuration encountered an unexpected property type name 'NotExists' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [create schema SomeSchema (col1 NotExists)]";
    
            try {
                _deploymentAdmin.Deploy(MakeModule("test", text), null);
                Assert.Fail();
            }
            catch (DeploymentActionException ex) {
                Assert.AreEqual(1, ex.Exceptions.Count);
                var first = ex.Exceptions[0];
                Assert.AreEqual(error, first.Inner.Message);
            }
    
            var options = new DeploymentOptions();
            options.IsCompileOnly = true;
            Assert.IsNull(_deploymentAdmin.Deploy(MakeModule("test", text), options));
        }
    
        [Test]
        public void TestFlagValidateOnly() {
    
            var textOne = "@Name('A') create schema MySchemaTwo (col1 int)";
            var textTwo = "@Name('B') select * from MySchemaTwo";
            var module = MakeModule("mymodule.two", textOne, textTwo);
    
            var options = new DeploymentOptions();
            options.IsValidateOnly = true;
            var result = _deploymentAdmin.Deploy(module, options);
            Assert.IsNull(result);
            Assert.AreEqual(0, _epService.EPAdministrator.StatementNames.Count);
        }
    
        [Test]
        public void TestFlagIsolated() {
    
            var textOne = "@Name('A') create schema MySchemaTwo (col1 int)";
            var textTwo = "@Name('B') select * from MySchemaTwo";
            var module = MakeModule("mymodule.two", textOne, textTwo);
    
            var options = new DeploymentOptions();
            options.IsolatedServiceProvider = "iso1";
            var result = _deploymentAdmin.Deploy(module, options);
            Assert.NotNull(result);
            Assert.AreEqual(2, _epService.EPAdministrator.StatementNames.Count);
            Assert.AreEqual("iso1", _epService.EPAdministrator.GetStatement("A").ServiceIsolated);
            Assert.AreEqual("iso1", _epService.EPAdministrator.GetStatement("B").ServiceIsolated);
        }
    
        [Test]
        public void TestFlagUndeployNoDestroy() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var resultOne = _deploymentAdmin.ParseDeploy("@Name('S0') select * from SupportBean");
            var resultTwo = _deploymentAdmin.ParseDeploy("@Name('S1') select * from SupportBean");
    
            var options = new UndeploymentOptions();
            options.IsDestroyStatements = false;
            _deploymentAdmin.UndeployRemove(resultOne.DeploymentId, options);
            Assert.NotNull(_epService.EPAdministrator.GetStatement("S0"));
    
            _deploymentAdmin.Undeploy(resultTwo.DeploymentId, options);
            Assert.NotNull(_epService.EPAdministrator.GetStatement("S1"));
        }
    
        private Module MakeModule(String name, params String[] statements)
        {
            var items = new ModuleItem[statements.Length];
            for (var i = 0; i < statements.Length; i++) {
                items[i] = new ModuleItem(statements[i], false, 0, 0, 0);
            }
            return new Module(name, null, new HashSet<String>(), new HashSet<String>(), items, null);
        }
    }
}
