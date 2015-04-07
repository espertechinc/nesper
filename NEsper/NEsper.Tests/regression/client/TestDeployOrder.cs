///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestDeployOrder
    {
        private EPServiceProvider _epService;
        private EPDeploymentAdmin _deploymentAdmin;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _deploymentAdmin = _epService.EPAdministrator.DeploymentAdmin;
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
        
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestOrder()
        {
            Module moduleA = null;
            Module moduleB = null;
            Module moduleC = null;
            Module moduleD = null;
            Module moduleE = null;
            DeploymentOrder order = null;
    
            // Tree of 4 deep
            moduleA = GetModule("A");
            moduleB = GetModule("B", "A");
            moduleC = GetModule("C", "A", "B", "D");
            moduleD = GetModule("D", "A", "B");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleC, moduleD, moduleB, moduleA}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleA, moduleB, moduleD, moduleC}, order);
    
            // Zero items
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{}, order);
    
            // 1 item
            moduleA = GetModule("A");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleA}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleA}, order);
    
            // 2 item
            moduleA = GetModule("A", "B");
            moduleB = GetModule("B");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleB, moduleA}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleA}, order);
    
            // 3 item
            moduleB = GetModule("B");
            moduleC = GetModule("C", "B");
            moduleD = GetModule("D");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleB, moduleC, moduleD}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleC, moduleD}, order);
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleD, moduleC, moduleB}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleD, moduleC}, order);
    
            // 2 trees of 2 deep
            moduleA = GetModule("A", "B");
            moduleB = GetModule("B");
            moduleC = GetModule("C", "D");
            moduleD = GetModule("D");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleC, moduleB, moduleA, moduleD}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleD, moduleC, moduleA}, order);
    
            // Tree of 5 deep
            moduleA = GetModule("A", "C");
            moduleB = GetModule("B");
            moduleC = GetModule("C", "B");
            moduleD = GetModule("D", "C", "E");
            moduleE = GetModule("E");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleA, moduleB, moduleC, moduleD, moduleE}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleC, moduleE, moduleA, moduleD}, order);
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleB, moduleE, moduleC, moduleA, moduleD}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleE, moduleC, moduleA, moduleD}, order);
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleA, moduleD, moduleE, moduleC, moduleB}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleE, moduleC, moduleA, moduleD}, order);
    
            // Tree with null names
            moduleA = GetModule(null, "C", "A", "B", "D");
            moduleB = GetModule(null, "C");
            moduleC = GetModule("A");
            moduleD = GetModule("B", "A", "C");
            moduleE = GetModule("C");
            DeploymentOrderOptions options = new DeploymentOrderOptions();
            options.IsCheckUses = false;
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleA, moduleB, moduleC, moduleD, moduleE}, options);
            AssertOrder(new Module[]{moduleC, moduleE, moduleD, moduleA, moduleB}, order);
            Assert.IsFalse(_deploymentAdmin.IsDeployed("C"));
    
            // Tree with duplicate names
            moduleA = GetModule("A", "C");
            moduleB = GetModule("B", "C");
            moduleC = GetModule("A", "B");
            moduleD = GetModule("D", "A");
            moduleE = GetModule("C");
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleA, moduleB, moduleC, moduleD, moduleE}, options);
            AssertOrder(new Module[]{moduleE, moduleB, moduleA, moduleC, moduleD}, order);
        }
    
        [Test]
        public void TestCircular() {
    
            // Circular 3
            Module moduleB = GetModule("B", "C");
            Module moduleC = GetModule("C", "D");
            Module moduleD = GetModule("D", "B");
    
            try {
                _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleC, moduleD, moduleB}, new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'D' uses (depends on) module 'B'", ex.Message);
            }
    
            // Circular 1 - this is allowed
            moduleB = GetModule("B", "B");
            DeploymentOrder order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleC, moduleD, moduleB}, new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleD, moduleC,}, order);
    
            // Circular 2
            moduleB = GetModule("B", "C");
            moduleC = GetModule("C", "B");
            try {
                _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleC, moduleB}, new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'B'", ex.Message);
            }
    
            // turn off circular check
            DeploymentOrderOptions options = new DeploymentOrderOptions();
            options.IsCheckCircularDependency = false;
            order = _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleC, moduleB}, options);
            AssertOrder(new Module[]{moduleB, moduleC}, order);
        }
    
        [Test]
        public void TestUnresolvedUses() {
    
            // Single module
            Module moduleB = GetModule("B", "C");
            try {
                _deploymentAdmin.GetDeploymentOrder(new Module[]{moduleB}, new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Module-dependency not found as declared by module 'B' for uses-declaration 'C'", ex.Message);
            }
    
            // multiple module
            Module[] modules = new Module[]{GetModule("B", "C"), GetModule("C", "D"), GetModule("D", "x")};
            try {
                _deploymentAdmin.GetDeploymentOrder(modules, new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Module-dependency not found as declared by module 'D' for uses-declaration 'x'", ex.Message);
            }
    
            // turn off uses-checks
            DeploymentOrderOptions options = new DeploymentOrderOptions();
            options.IsCheckUses = false;
            _deploymentAdmin.GetDeploymentOrder(modules, options);
        }
    
        private void AssertOrder(Module[] ordered, DeploymentOrder order) {
            EPAssertionUtil.AssertEqualsExactOrder(ordered, order.Ordered.ToArray());
        }
    
        private Module GetModule(String name, params String[] uses) {
            ICollection<String> usesSet = new HashSet<String>();
            usesSet.AddAll(uses);
            return new Module(name, null, usesSet, new string[0], new ModuleItem[0], null);
        }
    }
}
