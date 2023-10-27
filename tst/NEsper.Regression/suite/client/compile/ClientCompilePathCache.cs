///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompilePathCache
    {
        private const string EPL_PROVIDE = "@public create variable int myvariable = 10;\n" +
                                           "@public create schema MySchema();\n" +
                                           "@public create expression myExpr { 'abc' };\n" +
                                           "@public create window MyWindow#keepall as SupportBean_S0;\n" +
                                           "@public create table MyTable(y string);\n" +
                                           "@public create context MyContext start SupportBean_S0 end SupportBean_S1;\n" +
                                           "@public create expression myScript() [ 2 ];\n" +
                                           "@public create inlined_class \"\"\" public class MyClass { public static String doIt(String parameter) { return \"def\"; } }\"\"\";\n" +
                                           "@public @buseventtype create json schema CarLocUpdateEvent(CarId string, Direction int);\n";

        private const string EPL_CONSUME = "@name('s0') select myvariable as c0, myExpr() as c1, myScript() as c2," +
                                           "MyClass.doIt(TheString) as c4 from SupportBean;\n" +
                                           "select * from MySchema;" +
                                           "on SupportBean_S1 delete from MyWindow;\n" +
                                           "on SupportBean_S1 delete from MyTable;\n" +
                                           "context MyContext select * from SupportBean;\n" +
                                           "select CarId, Direction, count(*) as cnt from CarLocUpdateEvent(Direction = 1)#time(1 min);\n";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithObjectTypes(execs);
            WithProtected(execs);
            WithFillByCompile(execs);
            WithEventTypeChain(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompilePathCacheInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypeChain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompilePathCacheEventTypeChain());
            return execs;
        }

        public static IList<RegressionExecution> WithFillByCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompilePathCacheFillByCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithProtected(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompilePathCacheProtected());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompilePathCacheObjectTypes());
            return execs;
        }

        private class ClientCompilePathCacheEventTypeChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cache = CompilerPathCache.GetInstance();
                IList<EPCompiled> path = new List<EPCompiled>();
                CompileAdd(env, cache, path, new SupportModuleLoadDetector(), "@public create schema L0()");

                for (var i = 1; i < 10; i++) {
                    var epl = string.Format("@public create schema L%d(l%d L%d)", i, i - 1, i - 1);
                    CompileAdd(env, cache, path, new SupportModuleLoadDetector(), epl);
                }
            }
        }

        private class ClientCompilePathCacheInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                AssertDuplicate(env, false);
                AssertDuplicate(env, true);
            }

            private void AssertDuplicate(
                RegressionEnvironment env,
                bool withCache)
            {
                var cache = CompilerPathCache.GetInstance();
                IList<EPCompiled> pathOne = new List<EPCompiled>();
                CompileAdd(env, cache, pathOne, new SupportModuleLoadDetector(), "@public create schema A()");

                IList<EPCompiled> pathTwo = new List<EPCompiled>();
                CompileAdd(env, cache, pathTwo, new SupportModuleLoadDetector(), "@public create schema A()");

                var args = new CompilerArguments();
                if (withCache) {
                    args.Options.PathCache = cache;
                }

                args.Path.AddAll(pathOne);
                args.Path.AddAll(pathTwo);

                try {
                    EPCompilerProvider.Compiler.Compile("create schema B()", args);
                    Assert.Fail();
                }
                catch (EPCompileException e) {
                    SupportMessageAssertUtil.AssertMessage(
                        e,
                        "Invalid path: An event type by name 'A' has already been created for module '(unnamed)'");
                }
            }
        }

        private class ClientCompilePathCacheFillByCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var e1 = env.Compile(EPL_PROVIDE);

                var cache = CompilerPathCache.GetInstance();
                IList<EPCompiled> path = new List<EPCompiled>();
                path.Add(e1);

                var detectorOne = new SupportModuleLoadDetector();
                CompileAdd(env, cache, path, detectorOne, "create schema X()");
                Assert.IsTrue(detectorOne.IsLoadedModuleProvider);

                var detectorTwo = new SupportModuleLoadDetector();
                CompileAdd(env, cache, path, detectorTwo, EPL_CONSUME);
                Assert.IsFalse(detectorTwo.IsLoadedModuleProvider);
            }
        }

        private class ClientCompilePathCacheProtected : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cache = CompilerPathCache.GetInstance();
                var classLoaderProvider = new SupportModuleLoadDetector();
                IList<EPCompiled> path = new List<EPCompiled>();

                var eplProvide = "module a.b.c; @protected create variable int myvariable = 10;\n";
                CompileAdd(env, cache, path, classLoaderProvider, eplProvide);

                var eplConsume = "module a.b.c; select myvariable from SupportBean;\n";
                CompileAdd(env, cache, path, classLoaderProvider, eplConsume);
                Assert.IsFalse(classLoaderProvider.IsLoadedModuleProvider);
            }
        }

        private class ClientCompilePathCacheObjectTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cache = CompilerPathCache.GetInstance();
                var classLoaderProvider = new SupportModuleLoadDetector();
                IList<EPCompiled> path = new List<EPCompiled>();

                CompileAdd(env, cache, path, classLoaderProvider, EPL_PROVIDE);

                CompileAdd(env, cache, path, classLoaderProvider, EPL_CONSUME);

                CompileFAF(cache, path, classLoaderProvider, "select * from MyWindow");
                Assert.IsFalse(classLoaderProvider.IsLoadedModuleProvider);
            }
        }

        private static void CompileAdd(
            RegressionEnvironment env,
            CompilerPathCache cache,
            IList<EPCompiled> path,
            SupportModuleLoadDetector classLoaderProvider,
            string epl)
        {
            Configuration configuration;
            try {
                configuration = env.CopyMayFail(env.Configuration);
            }
            catch (Exception ex) {
                throw new EPRuntimeException(ex.Message, ex);
            }

            configuration.Common.TransientConfiguration = Collections.SingletonDataMap(
                ClassLoaderProviderConstants.NAME,
                classLoaderProvider);

            var args = new CompilerArguments(configuration);
            args.Options.PathCache = cache;
            args.Path.AddAll(path);

            try {
                var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
                path.Add(compiled);
            }
            catch (EPCompileException e) {
                throw new EPRuntimeException(e.Message, e);
            }
        }

        private static void CompileFAF(
            CompilerPathCache cache,
            IList<EPCompiled> path,
            SupportModuleLoadDetector classLoaderProvider,
            string epl)
        {
            var configuration = new Configuration();
            configuration.Common.TransientConfiguration.Put(
                ClassLoaderProviderConstants.NAME,
                classLoaderProvider);

            var args = new CompilerArguments(configuration);
            args.Options.PathCache = cache;
            args.Path.AddAll(path);

            try {
                EPCompilerProvider.Compiler.CompileQuery(epl, args);
            }
            catch (EPCompileException e) {
                throw new EPRuntimeException(e.Message, e);
            }
        }

        private class SupportModuleLoadDetector : TypeResolver
        {
            bool loadedModuleProvider = false;

            public TypeResolver TypeResolver()
            {
                return this;
            }

            public Type ResolveType(
                string typeName,
                bool resolve = false)
            {
                if (typeName.Contains(nameof(ModuleProvider))) {
                    loadedModuleProvider = true;
                }

                return null;
            }

            public bool IsLoadedModuleProvider => loadedModuleProvider;
        }
    }
} // end of namespace