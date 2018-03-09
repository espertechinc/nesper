///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientDeployOrder : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOrder(epService);
            RunAssertionCircular(epService);
            RunAssertionUnresolvedUses(epService);
        }
    
        private void RunAssertionOrder(EPServiceProvider epService) {
    
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
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleC, moduleD, moduleB, moduleA}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleA, moduleB, moduleD, moduleC}, order);
    
            // Zero items
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{}, order);
    
            // 1 item
            moduleA = GetModule("A");
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleA}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleA}, order);
    
            // 2 item
            moduleA = GetModule("A", "B");
            moduleB = GetModule("B");
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleB, moduleA}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleA}, order);
    
            // 3 item
            moduleB = GetModule("B");
            moduleC = GetModule("C", "B");
            moduleD = GetModule("D");
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleB, moduleC, moduleD}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleC, moduleD}, order);
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleD, moduleC, moduleB}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleD, moduleC}, order);
    
            // 2 trees of 2 deep
            moduleA = GetModule("A", "B");
            moduleB = GetModule("B");
            moduleC = GetModule("C", "D");
            moduleD = GetModule("D");
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleC, moduleB, moduleA, moduleD}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleD, moduleC, moduleA}, order);
    
            // Tree of 5 deep
            moduleA = GetModule("A", "C");
            moduleB = GetModule("B");
            moduleC = GetModule("C", "B");
            moduleD = GetModule("D", "C", "E");
            moduleE = GetModule("E");
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleA, moduleB, moduleC, moduleD, moduleE}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleC, moduleE, moduleA, moduleD}, order);
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleB, moduleE, moduleC, moduleA, moduleD}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleE, moduleC, moduleA, moduleD}, order);
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleA, moduleD, moduleE, moduleC, moduleB}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleE, moduleC, moduleA, moduleD}, order);
    
            // Tree with null names
            moduleA = GetModule(null, "C", "A", "B", "D");
            moduleB = GetModule(null, "C");
            moduleC = GetModule("A");
            moduleD = GetModule("B", "A", "C");
            moduleE = GetModule("C");
            var options = new DeploymentOrderOptions();
            options.IsCheckUses = false;
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleA, moduleB, moduleC, moduleD, moduleE}), options);
            AssertOrder(new Module[]{moduleC, moduleE, moduleD, moduleA, moduleB}, order);
            Assert.IsFalse(epService.EPAdministrator.DeploymentAdmin.IsDeployed("C"));
    
            // Tree with duplicate names
            moduleA = GetModule("A", "C");
            moduleB = GetModule("B", "C");
            moduleC = GetModule("A", "B");
            moduleD = GetModule("D", "A");
            moduleE = GetModule("C");
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleA, moduleB, moduleC, moduleD, moduleE}), options);
            AssertOrder(new Module[]{moduleE, moduleB, moduleA, moduleC, moduleD}, order);
        }
    
        private void RunAssertionCircular(EPServiceProvider epService) {
    
            // Circular 3
            Module moduleB = GetModule("B", "C");
            Module moduleC = GetModule("C", "D");
            Module moduleD = GetModule("D", "B");
    
            try {
                epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleC, moduleD, moduleB}), new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'D' uses (depends on) module 'B'", ex.Message);
            }
    
            // Circular 1 - this is allowed
            moduleB = GetModule("B", "B");
            DeploymentOrder order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleC, moduleD, moduleB}), new DeploymentOrderOptions());
            AssertOrder(new Module[]{moduleB, moduleD, moduleC}, order);
    
            // Circular 2
            moduleB = GetModule("B", "C");
            moduleC = GetModule("C", "B");
            try {
                epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleC, moduleB}), new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'B'", ex.Message);
            }
    
            // turn off circular check
            var options = new DeploymentOrderOptions();
            options.IsCheckCircularDependency = false;
            order = epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleC, moduleB}), options);
            AssertOrder(new Module[]{moduleB, moduleC}, order);
        }
    
        private void RunAssertionUnresolvedUses(EPServiceProvider epService) {
    
            // Single module
            Module moduleB = GetModule("B", "C");
            try {
                epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(new Module[]{moduleB}), new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Module-dependency not found as declared by module 'B' for uses-declaration 'C'", ex.Message);
            }
    
            // multiple module
            var modules = new Module[]{GetModule("B", "C"), GetModule("C", "D"), GetModule("D", "x")};
            try {
                epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(modules), new DeploymentOrderOptions());
                Assert.Fail();
            } catch (DeploymentOrderException ex) {
                Assert.AreEqual("Module-dependency not found as declared by module 'D' for uses-declaration 'x'", ex.Message);
            }
    
            // turn off uses-checks
            var options = new DeploymentOrderOptions();
            options.IsCheckUses = false;
            epService.EPAdministrator.DeploymentAdmin.GetDeploymentOrder(Collections.List(modules), options);
        }
    
        private void AssertOrder(Module[] ordered, DeploymentOrder order) {
            EPAssertionUtil.AssertEqualsExactOrder(ordered, order.Ordered.ToArray());
        }
    
        private Module GetModule(string name, params string[] uses) {
            var usesSet = new HashSet<string>();
            usesSet.AddAll(Collections.List(uses));
            return new Module(name, null, usesSet, Collections.GetEmptySet<string>(), Collections.GetEmptyList<ModuleItem>(), null);
        }
    }
} // end of namespace
