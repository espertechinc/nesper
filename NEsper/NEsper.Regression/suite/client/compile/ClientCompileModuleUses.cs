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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileModuleUses
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientCompileModuleUsesOrder());
            execs.Add(new ClientCompileModuleUsesCircular());
            execs.Add(new ClientCompileModuleUsesUnresolvedUses());
            return execs;
        }

        private static void AssertOrder(
            Module[] ordered,
            ModuleOrder order)
        {
            EPAssertionUtil.AssertEqualsExactOrder(ordered, order.Ordered.ToArray());
        }

        private static Module GetModule(
            string name,
            params string[] uses)
        {
            ISet<string> usesSet = new HashSet<string>();
            usesSet.AddAll(Arrays.AsList(uses));
            return new Module(name, null, usesSet, new EmptySet<Import>(), new EmptyList<ModuleItem>(), null);
        }

        internal class ClientCompileModuleUsesOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    Module moduleA = null;
                    Module moduleB = null;
                    Module moduleC = null;
                    Module moduleD = null;
                    Module moduleE = null;
                    ModuleOrder order = null;

                    // Tree of 4 deep
                    moduleA = GetModule("A");
                    moduleB = GetModule("B", "A");
                    moduleC = GetModule("C", "A", "B", "D");
                    moduleD = GetModule("D", "A", "B");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleC, moduleD, moduleB, moduleA),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleA, moduleB, moduleD, moduleC}, order);

                    // Zero items
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] { }),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] { }, order);

                    // 1 item
                    moduleA = GetModule("A");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleA),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleA}, order);

                    // 2 item
                    moduleA = GetModule("A", "B");
                    moduleB = GetModule("B");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleB, moduleA),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleA}, order);

                    // 3 item
                    moduleB = GetModule("B");
                    moduleC = GetModule("C", "B");
                    moduleD = GetModule("D");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleB, moduleC, moduleD),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleC, moduleD}, order);
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleD, moduleC, moduleB),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleD, moduleC}, order);

                    // 2 trees of 2 deep
                    moduleA = GetModule("A", "B");
                    moduleB = GetModule("B");
                    moduleC = GetModule("C", "D");
                    moduleD = GetModule("D");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleC, moduleB, moduleA, moduleD),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleD, moduleC, moduleA}, order);

                    // Tree of 5 deep
                    moduleA = GetModule("A", "C");
                    moduleB = GetModule("B");
                    moduleC = GetModule("C", "B");
                    moduleD = GetModule("D", "C", "E");
                    moduleE = GetModule("E");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleA, moduleB, moduleC, moduleD, moduleE),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleC, moduleE, moduleA, moduleD}, order);
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleB, moduleE, moduleC, moduleA, moduleD),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleE, moduleC, moduleA, moduleD}, order);
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleA, moduleD, moduleE, moduleC, moduleB),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    AssertOrder(new[] {moduleB, moduleE, moduleC, moduleA, moduleD}, order);

                    // Tree with null names
                    moduleA = GetModule(null, "C", "A", "B", "D");
                    moduleB = GetModule(null, "C");
                    moduleC = GetModule("A");
                    moduleD = GetModule("B", "A", "C");
                    moduleE = GetModule("C");
                    var options = new ModuleOrderOptions();
                    options.IsCheckUses = false;
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleA, moduleB, moduleC, moduleD, moduleE),
                        new EmptySet<string>(),
                        options);
                    AssertOrder(new[] {moduleC, moduleE, moduleD, moduleA, moduleB}, order);

                    // Tree with duplicate names
                    moduleA = GetModule("A", "C");
                    moduleB = GetModule("B", "C");
                    moduleC = GetModule("A", "B");
                    moduleD = GetModule("D", "A");
                    moduleE = GetModule("C");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleA, moduleB, moduleC, moduleD, moduleE),
                        new EmptySet<string>(),
                        options);
                    AssertOrder(new[] {moduleE, moduleB, moduleA, moduleC, moduleD}, order);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }
            }
        }

        internal class ClientCompileModuleUsesCircular : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Circular 3
                var moduleB = GetModule("B", "C");
                var moduleC = GetModule("C", "D");
                var moduleD = GetModule("D", "B");

                try {
                    ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleC, moduleD, moduleB),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual(
                        "Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'D' uses (depends on) module 'B'",
                        ex.Message);
                }

                // Circular 1 - this is allowed
                moduleB = GetModule("B", "B");
                ModuleOrder order = null;
                try {
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleC, moduleD, moduleB),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                }
                catch (ModuleOrderException e) {
                    throw new EPException(e);
                }

                AssertOrder(new[] {moduleB, moduleD, moduleC}, order);

                // Circular 2
                moduleB = GetModule("B", "C");
                moduleC = GetModule("C", "B");
                try {
                    ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleC, moduleB),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual(
                        "Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'B'",
                        ex.Message);
                }

                // turn off circular check
                var options = new ModuleOrderOptions();
                options.IsCheckCircularDependency = false;
                try {
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleC, moduleB),
                        new EmptySet<string>(),
                        options);
                }
                catch (ModuleOrderException e) {
                    throw new EPException(e);
                }

                AssertOrder(new[] {moduleB, moduleC}, order);
            }
        }

        internal class ClientCompileModuleUsesUnresolvedUses : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Single module
                var moduleB = GetModule("B", "C");
                try {
                    ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(moduleB),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual(
                        "Module-dependency not found as declared by module 'B' for uses-declaration 'C'",
                        ex.Message);
                }

                // multiple module
                Module[] modules = {GetModule("B", "C"), GetModule("C", "D"), GetModule("D", "x")};
                try {
                    ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(modules),
                        new EmptySet<string>(),
                        new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual(
                        "Module-dependency not found as declared by module 'D' for uses-declaration 'x'",
                        ex.Message);
                }

                // turn off uses-checks
                var options = new ModuleOrderOptions();
                options.IsCheckUses = false;
                try {
                    ModuleOrderUtil.GetModuleOrder(Arrays.AsList(modules), new EmptySet<string>(), options);
                }
                catch (ModuleOrderException e) {
                    throw new EPException(e);
                }
            }
        }
    }
} // end of namespace