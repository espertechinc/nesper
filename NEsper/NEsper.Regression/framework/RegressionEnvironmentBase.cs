///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.framework
{
    public abstract class RegressionEnvironmentBase : RegressionEnvironment
    {
        public RegressionEnvironmentBase(
            Configuration configuration,
            EPRuntime runtime)
        {
            Configuration = configuration;
            Runtime = runtime;
        }

        public Module ParseModule(string moduleText)
        {
            try {
                return EPCompilerProvider.Compiler.ParseModule(moduleText);
            }
            catch (Exception t) {
                throw new EPException(t);
            }
        }

        public EPCompiled CompileFAF(
            string query,
            RegressionPath path)
        {
            var args = GetArgsNoExport(path);
            try {
                return EPCompilerProvider.Compiler.CompileQuery(query, args);
            }
            catch (Exception t) {
                throw NotExpected(t);
            }
        }

        public EPCompiled CompileFAF(
            EPStatementObjectModel model,
            RegressionPath path)
        {
            var args = GetArgsNoExport(path);
            try {
                return EPCompilerProvider.Compiler.CompileQuery(model, args);
            }
            catch (Exception t) {
                throw NotExpected(t);
            }
        }

        public EPFireAndForgetQueryResult CompileExecuteFAF(
            string query,
            RegressionPath path)
        {
            var compiled = CompileFAF(query, path);
            return Runtime.FireAndForgetService.ExecuteQuery(compiled);
        }

        public EPFireAndForgetQueryResult CompileExecuteFAF(
            EPStatementObjectModel model,
            RegressionPath path)
        {
            var compiled = CompileFAF(model, path);
            return Runtime.FireAndForgetService.ExecuteQuery(compiled);
        }

        public RegressionEnvironment Deploy(EPCompiled compiled)
        {
            TryDeploy(compiled);
            return this;
        }

        public RegressionEnvironment Deploy(
            EPCompiled compiled,
            DeploymentOptions options)
        {
            try {
                Runtime.DeploymentService.Deploy(compiled, options);
            }
            catch (EPDeployException ex) {
                throw NotExpected(ex);
            }

            return this;
        }

        public string DeployGetId(EPCompiled compiled)
        {
            try {
                return Runtime.DeploymentService.Deploy(compiled).DeploymentId;
            }
            catch (EPDeployException ex) {
                throw NotExpected(ex);
            }
        }

        public RegressionEnvironment SendEventObjectArray(
            object[] oa,
            string typeName)
        {
            Runtime.EventService.SendEventObjectArray(oa, typeName);
            return this;
        }

        public RegressionEnvironment SendEventBean(object @event)
        {
            Runtime.EventService.SendEventBean(@event, @event.GetType().Name);
            return this;
        }

        public RegressionEnvironment SendEventBean(
            object @event,
            string typeName)
        {
            Runtime.EventService.SendEventBean(@event, typeName);
            return this;
        }

        public RegressionEnvironment SendEventMap(
            IDictionary<string, object> values,
            string typeName)
        {
            Runtime.EventService.SendEventMap(values, typeName);
            return this;
        }

        public RegressionEnvironment SendEventXMLDOM(
            XmlNode document,
            string typeName)
        {
            Runtime.EventService.SendEventXMLDOM(document, typeName);
            return this;
        }

        public RegressionEnvironment SendEventAvro(
            GenericRecord theEvent,
            string typeName)
        {
            Runtime.EventService.SendEventAvro(theEvent, typeName);
            return this;
        }

        public virtual RegressionEnvironment AddListener(string statementName)
        {
            return null;
        }

        public virtual RegressionEnvironment Milestone(long num)
        {
            return null;
        }

        public virtual RegressionEnvironment MilestoneInc(AtomicLong counter)
        {
            return null;
        }

        public virtual bool IsHA => false;

        public virtual bool IsHA_Releasing => false;

        public virtual SupportListener ListenerNew()
        {
            return null;
        }

        public RegressionEnvironment AdvanceTimeSpan(long msec)
        {
            Runtime.EventService.AdvanceTimeSpan(msec);
            return this;
        }

        public RegressionEnvironment AdvanceTime(long msec)
        {
            Runtime.EventService.AdvanceTime(msec);
            return this;
        }

        public SupportListener Listener(string statementName)
        {
            return GetRequireStatementListener(statementName, Runtime);
        }

        public string DeploymentId(string statementName)
        {
            var statement = (EPStatementSPI) GetRequireStatement(statementName, Runtime);
            return statement.StatementContext.DeploymentId;
        }

        public RegressionEnvironment UndeployAll()
        {
            try {
                Runtime.DeploymentService.UndeployAll();
            }
            catch (EPUndeployException ex) {
                throw NotExpected(ex);
            }

            return this;
        }

        public RegressionEnvironment Undeploy(string deploymentId)
        {
            try {
                Runtime.DeploymentService.Undeploy(deploymentId);
            }
            catch (EPUndeployException ex) {
                throw NotExpected(ex);
            }

            return this;
        }

        public Configuration Configuration { get; }

        public EPStatement Statement(string statementName)
        {
            return GetStatement(statementName, Runtime);
        }

        public IEnumerator<EventBean> GetEnumerator(string statementName)
        {
            return GetRequireStatement(statementName, Runtime).GetEnumerator();
        }

        public RegressionEnvironment CompileDeployAddListenerMileZero(
            string epl,
            string statementName)
        {
            return CompileDeployAddListenerMile(epl, statementName, 0);
        }

        public RegressionEnvironment CompileDeploy(
            bool soda,
            string epl,
            RegressionPath path)
        {
            if (!soda) {
                CompileDeploy(epl, path);
            }
            else {
                var model = EplToModel(epl);
                CompileDeploy(model, path);
            }

            return this;
        }

        public EPCompiled Compile(
            string epl,
            RegressionPath path)
        {
            var args = GetArgsWithExportToPath(path);
            var compiled = Compile(epl, args);
            path.Add(compiled);
            return compiled;
        }

        public RegressionEnvironment CompileDeploy(
            EPStatementObjectModel model,
            RegressionPath path)
        {
            var args = GetArgsWithExportToPath(path);
            var compiled = Compile(model, args);
            path.Add(compiled);

            Deploy(compiled);
            return this;
        }

        public RegressionEnvironment CompileDeployWBusPublicType(
            string epl,
            RegressionPath path)
        {
            var compiled = CompileWBusPublicType(epl);
            path.Add(compiled);
            Deploy(compiled);
            return this;
        }

        public RegressionEnvironment CompileDeploy(EPStatementObjectModel model)
        {
            var args = new CompilerArguments(Configuration);
            var compiled = Compile(model, args);
            Deploy(compiled);
            return this;
        }

        public RegressionEnvironment CompileDeployAddListenerMile(
            string epl,
            string statementName,
            long milestone)
        {
            var compiled = TryCompile(epl, null);
            Deploy(compiled).AddListener(statementName);
            if (milestone != -1) {
                Milestone(milestone);
            }

            return this;
        }

        public EPCompiled Compile(
            bool soda,
            string epl,
            CompilerArguments arguments)
        {
            if (!soda) {
                Compile(epl, arguments);
            }

            var copy = EplToModel(epl);
            Assert.AreEqual(epl, copy.ToEPL());
            arguments.Configuration = Configuration;
            return Compile(copy, arguments);
        }

        public RegressionEnvironment CompileDeploy(
            bool soda,
            string epl)
        {
            if (!soda) {
                CompileDeploy(epl);
            }
            else {
                EplToModelCompileDeploy(epl);
            }

            return this;
        }

        public RegressionEnvironment CompileDeploy(string epl)
        {
            var compiled = TryCompile(epl, null);
            Deploy(compiled);
            return this;
        }

        public RegressionEnvironment CompileDeploy(
            string epl,
            Consumer<CompilerOptions> options)
        {
            var compiled = TryCompile(epl, options);
            Deploy(compiled);
            return this;
        }

        public RegressionEnvironment CompileDeploy(
            string epl,
            RegressionPath path)
        {
            var args = GetArgsWithExportToPath(path);
            var compiled = Compile(epl, args);
            path.Add(compiled);

            Deploy(compiled);
            return this;
        }

        public RegressionEnvironment EplToModelCompileDeploy(string epl)
        {
            var copy = EplToModel(epl);

            Assert.AreEqual(epl.Trim(), copy.ToEPL());

            var compiled = Compile(copy, new CompilerArguments(Configuration));
            var result = TryDeploy(compiled);

            var stmt = result.Statements[0];
            Assert.AreEqual(epl.Trim(), stmt.GetProperty(StatementProperty.EPL));
            return this;
        }

        public RegressionEnvironment EplToModelCompileDeploy(
            string epl,
            RegressionPath path)
        {
            var copy = EplToModel(epl);

            Assert.AreEqual(epl.Trim(), copy.ToEPL());

            var args = GetArgsWithExportToPath(path);

            var compiled = Compile(copy, args);
            path.Add(compiled);

            var result = TryDeploy(compiled);

            Assert.AreEqual(epl.Trim(), result.Statements[0].GetProperty(StatementProperty.EPL));
            return this;
        }

        public EPCompiled Compile(
            EPStatementObjectModel model,
            CompilerArguments args)
        {
            try {
                var module = new Module();
                module.Items.Add(new ModuleItem(model));
                return EPCompilerProvider.Compiler.Compile(module, args);
            }
            catch (Exception t) {
                throw NotExpected(t);
            }
        }

        public RegressionEnvironment UndeployModuleContaining(string statementName)
        {
            var deployments = Runtime.DeploymentService.Deployments;
            try {
                foreach (var deployment in deployments) {
                    var info = Runtime.DeploymentService.GetDeployment(deployment);
                    foreach (var stmt in info.Statements) {
                        if (stmt.Name.Equals(statementName)) {
                            Runtime.DeploymentService.Undeploy(deployment);
                            return this;
                        }
                    }
                }
            }
            catch (EPUndeployException ex) {
                throw NotExpected(ex);
            }

            Assert.Fail("Failed to find deployment with statement '" + statementName + "'");
            return this;
        }

        public RegressionEnvironment AddListener(
            string statementName,
            SupportListener listener)
        {
            GetAssertStatement(statementName).AddListener(listener);
            return this;
        }

        public EPCompiled Compile(
            string epl,
            CompilerArguments arguments)
        {
            try {
                arguments.Configuration = Configuration;
                return EPCompilerProvider.Compiler.Compile(epl, arguments);
            }
            catch (EPCompileException t) {
                throw NotExpected(t);
            }
        }

        public EPCompiled Compile(
            string epl,
            Consumer<CompilerOptions> options)
        {
            return TryCompile(epl, options);
        }

        public EPCompiled CompileWBusPublicType(string epl)
        {
            return TryCompile(
                epl,
                compilerOptions => {
                    compilerOptions.BusModifierEventType = ctx => EventTypeBusModifier.BUS;
                    compilerOptions.AccessModifierEventType = ctx => NameAccessModifier.PUBLIC;
                    compilerOptions.AccessModifierNamedWindow = ctx => NameAccessModifier.PUBLIC;
                    compilerOptions.AccessModifierTable = ctx => NameAccessModifier.PUBLIC;
                });
        }

        public EPCompiled Compile(string epl)
        {
            return TryCompile(epl, null);
        }

        public EPStatementObjectModel EplToModel(string epl)
        {
            try {
                var model = EPCompilerProvider.Compiler.EplToModel(epl, Configuration);
                return this.CopyMayFail(model); // copy to test serializability
            }
            catch (EPCompileException t) {
                throw NotExpected(t);
            }
        }

        public IContainer Container { get; }

        public string RuntimeURI => Runtime.URI;

        public EPRuntime Runtime { get; }

        public EPDeploymentService Deployment => Runtime.DeploymentService;

        public EPEventService EventService => Runtime.EventService;

        public EPCompiled CompileWCheckedEx(string epl)
        {
            return CompileWCheckedEx(epl, null);
        }

        public EPCompiled CompileWCheckedEx(
            string epl,
            RegressionPath path)
        {
            var args = new CompilerArguments(Configuration);
            if (path != null) {
                args.Path.AddAll(path.Compileds);
            }

            return EPCompilerProvider.Compiler.Compile(epl, args);
        }

        public Module ReadModule(string filename)
        {
            try {
                return EPCompilerProvider.Compiler.ReadModule(filename, Container.ResourceManager());
            }
            catch (Exception t) {
                throw new EPException(t);
            }
        }

        public EPCompiled ReadCompile(string fileName)
        {
            var module = ReadModule(fileName);
            return Compile(module);
        }

        public EPCompiled Compile(Module module)
        {
            try {
                return EPCompilerProvider.Compiler.Compile(module, new CompilerArguments(Configuration));
            }
            catch (Exception t) {
                throw new EPException(t);
            }
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled)
        {
            return Runtime.FireAndForgetService.ExecuteQuery(compiled);
        }

        public RegressionEnvironment CompileDeployWBusPublicType(string epl)
        {
            return null;
        }

        protected EPStatement GetAssertStatement(string statementName)
        {
            return GetRequireStatement(statementName, Runtime);
        }

        private EPDeployment TryDeploy(EPCompiled compiled)
        {
            try {
                return Runtime.DeploymentService.Deploy(compiled);
            }
            catch (EPDeployException ex) {
                throw NotExpected(ex);
            }
        }

        private EPCompiled TryCompile(
            string epl,
            Consumer<CompilerOptions> options)
        {
            try {
                if (options == null) {
                    return EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(Configuration));
                }

                var args = new CompilerArguments(Configuration);
                options.Invoke(args.Options);

                return EPCompilerProvider.Compiler.Compile(epl, args);
            }
            catch (Exception t) {
                throw NotExpected(t);
            }
        }

        private CompilerArguments GetArgsNoExport(RegressionPath path)
        {
            var args = new CompilerArguments(Configuration);
            args.Path.Compileds.AddAll(path.Compileds);
            return args;
        }

        private CompilerArguments GetArgsWithExportToPath(RegressionPath path)
        {
            var args = new CompilerArguments(Configuration);
            args.Path.Compileds.AddAll(path.Compileds);
            args.Options.AccessModifierNamedWindow = ctx => NameAccessModifier.PUBLIC;
            args.Options.AccessModifierVariable = ctx => NameAccessModifier.PUBLIC;
            args.Options.AccessModifierEventType = ctx => NameAccessModifier.PUBLIC;
            args.Options.AccessModifierContext = ctx => NameAccessModifier.PUBLIC;
            args.Options.AccessModifierExpression = ctx => NameAccessModifier.PUBLIC;
            args.Options.AccessModifierScript = ctx => NameAccessModifier.PUBLIC;
            args.Options.AccessModifierTable = ctx => NameAccessModifier.PUBLIC;
            return args;
        }

        private EPException NotExpected(Exception t)
        {
            throw new EPException("Test failed due to exception: " + t.Message, t);
        }
    }
} // end of namespace