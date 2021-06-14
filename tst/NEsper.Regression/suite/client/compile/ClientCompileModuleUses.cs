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
            WithOrder(execs);
            WithCircular(execs);
            WithUnresolvedUses(execs);
            WithIgnorableUses(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithIgnorableUses(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileModuleUsesIgnorableUses());
            return execs;
        }

        public static IList<RegressionExecution> WithUnresolvedUses(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileModuleUsesUnresolvedUses());
            return execs;
        }

        public static IList<RegressionExecution> WithCircular(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileModuleUsesCircular());
            return execs;
        }

        public static IList<RegressionExecution> WithOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileModuleUsesOrder());
            return execs;
        }

        private class ClientCompileModuleUsesIgnorableUses : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string eplObjects = "@public create variable int MYVAR;\n";
                env.Compile(eplObjects, path);

                env.Compile("uses dummy; select MYVAR from SupportBean", path);
            }
        }

        private class ClientCompileModuleUsesOrder : RegressionExecution
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
                        Arrays.AsList(new Module[] {moduleC, moduleD, moduleB, moduleA}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleA, moduleB, moduleD, moduleC}, order);

                    // Zero items
                    order = ModuleOrderUtil.GetModuleOrder(Arrays.AsList(new Module[] { }), EmptySet<string>.Instance, new ModuleOrderOptions());
                    AssertOrder(new Module[] { }, order);

                    // 1 item
                    moduleA = GetModule("A");
                    order = ModuleOrderUtil.GetModuleOrder(Arrays.AsList(new Module[] {moduleA}), EmptySet<string>.Instance, new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleA}, order);

                    // 2 item
                    moduleA = GetModule("A", "B");
                    moduleB = GetModule("B");
                    order = ModuleOrderUtil.GetModuleOrder(Arrays.AsList(new Module[] {moduleB, moduleA}), EmptySet<string>.Instance, new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleA}, order);

                    // 3 item
                    moduleB = GetModule("B");
                    moduleC = GetModule("C", "B");
                    moduleD = GetModule("D");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleB, moduleC, moduleD}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleC, moduleD}, order);
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleD, moduleC, moduleB}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleD, moduleC}, order);

                    // 2 trees of 2 deep
                    moduleA = GetModule("A", "B");
                    moduleB = GetModule("B");
                    moduleC = GetModule("C", "D");
                    moduleD = GetModule("D");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleC, moduleB, moduleA, moduleD}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleD, moduleC, moduleA}, order);

                    // Tree of 5 deep
                    moduleA = GetModule("A", "C");
                    moduleB = GetModule("B");
                    moduleC = GetModule("C", "B");
                    moduleD = GetModule("D", "C", "E");
                    moduleE = GetModule("E");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleA, moduleB, moduleC, moduleD, moduleE}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleC, moduleE, moduleA, moduleD}, order);
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleB, moduleE, moduleC, moduleA, moduleD}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleE, moduleC, moduleA, moduleD}, order);
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleA, moduleD, moduleE, moduleC, moduleB}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                    AssertOrder(new Module[] {moduleB, moduleE, moduleC, moduleA, moduleD}, order);

                    // Tree with null names
                    moduleA = GetModule(null, "C", "A", "B", "D");
                    moduleB = GetModule(null, "C");
                    moduleC = GetModule("A");
                    moduleD = GetModule("B", "A", "C");
                    moduleE = GetModule("C");
                    ModuleOrderOptions options = new ModuleOrderOptions();
                    options.IsCheckUses = false;
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleA, moduleB, moduleC, moduleD, moduleE}),
                        EmptySet<string>.Instance,
                        options);
                    AssertOrder(new Module[] {moduleC, moduleE, moduleD, moduleA, moduleB}, order);

                    // Tree with duplicate names
                    moduleA = GetModule("A", "C");
                    moduleB = GetModule("B", "C");
                    moduleC = GetModule("A", "B");
                    moduleD = GetModule("D", "A");
                    moduleE = GetModule("C");
                    order = ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleA, moduleB, moduleC, moduleD, moduleE}),
                        EmptySet<string>.Instance,
                        options);
                    AssertOrder(new Module[] {moduleE, moduleB, moduleA, moduleC, moduleD}, order);
                }
                catch (Exception ex) {
                    throw new EPRuntimeException(ex);
                }
            }
        }

        private class ClientCompileModuleUsesCircular : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Circular 3
                Module moduleB = GetModule("B", "C");
                Module moduleC = GetModule("C", "D");
                Module moduleD = GetModule("D", "B");

                try {
                    ModuleOrderUtil.GetModuleOrder(
                        Arrays.AsList(new Module[] {moduleC, moduleD, moduleB}),
                        EmptySet<string>.Instance,
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
                        Arrays.AsList(new Module[] {moduleC, moduleD, moduleB}),
                        EmptySet<string>.Instance,
                        new ModuleOrderOptions());
                }
                catch (ModuleOrderException e) {
                    throw new EPRuntimeException(e);
                }

                AssertOrder(new Module[] {moduleB, moduleD, moduleC}, order);

                // Circular 2
                moduleB = GetModule("B", "C");
                moduleC = GetModule("C", "B");
                try {
                    ModuleOrderUtil.GetModuleOrder(Arrays.AsList(new Module[] {moduleC, moduleB}), EmptySet<string>.Instance, new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual("Circular dependency detected in module uses-relationships: module 'C' uses (depends on) module 'B'", ex.Message);
                }

                // turn off circular check
                ModuleOrderOptions options = new ModuleOrderOptions();
                options.IsCheckCircularDependency = false;
                try {
                    order = ModuleOrderUtil.GetModuleOrder(Arrays.AsList(new Module[] {moduleC, moduleB}), EmptySet<string>.Instance, options);
                }
                catch (ModuleOrderException e) {
                    throw new EPRuntimeException(e);
                }

                AssertOrder(new Module[] {moduleB, moduleC}, order);
            }
        }

        private class ClientCompileModuleUsesUnresolvedUses : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Single module
                Module moduleB = GetModule("B", "C");
                try {
                    ModuleOrderUtil.GetModuleOrder(Arrays.AsList(new Module[] {moduleB}), EmptySet<string>.Instance, new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual("Module-dependency not found as declared by module 'B' for uses-declaration 'C'", ex.Message);
                }

                // multiple module
                Module[] modules = new Module[] {GetModule("B", "C"), GetModule("C", "D"), GetModule("D", "x")};
                try {
                    ModuleOrderUtil.GetModuleOrder(Arrays.AsList(modules), EmptySet<string>.Instance, new ModuleOrderOptions());
                    Assert.Fail();
                }
                catch (ModuleOrderException ex) {
                    Assert.AreEqual("Module-dependency not found as declared by module 'D' for uses-declaration 'x'", ex.Message);
                }

                // turn off uses-checks
                ModuleOrderOptions options = new ModuleOrderOptions();
                options.IsCheckUses = false;
                try {
                    ModuleOrderUtil.GetModuleOrder(Arrays.AsList(modules), EmptySet<string>.Instance, options);
                }
                catch (ModuleOrderException e) {
                    throw new EPRuntimeException(e);
                }
            }
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
            return new Module(
                name,
                null,
                usesSet,
                EmptySet<Import>.Instance,
                EmptyList<ModuleItem>.Instance,
                null);
        }
    }
} // end of namespace