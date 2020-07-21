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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.framework
{
    public interface RegressionEnvironment
    {
        IContainer Container { get; }

        Configuration Configuration { get; }

        EPCompiler Compiler { get; }

        bool IsHA { get; }

        bool IsHA_Releasing { get; }

        string RuntimeURI { get; }

        EPRuntime Runtime { get; }

        EPEventService EventService { get; }

        EPDeploymentService Deployment { get; }

        EPCompiled Compile(
            string epl,
            CompilerArguments arguments);

        EPCompiled Compile(string epl);

        EPCompiled Compile(
            string epl,
            RegressionPath path);

        EPCompiled Compile(
            EPStatementObjectModel model,
            CompilerArguments args);

        EPCompiled Compile(
            bool soda,
            string epl,
            CompilerArguments arguments);

        EPCompiled Compile(
            string epl,
            Consumer<CompilerOptions> options);

        EPCompiled CompileWBusPublicType(string epl);

        EPCompiled CompileWCheckedEx(string epl);

        EPCompiled CompileWCheckedEx(
            string epl,
            RegressionPath path);

        EPCompiled CompileFAF(
            string query,
            RegressionPath path);

        EPCompiled CompileFAF(
            EPStatementObjectModel model,
            RegressionPath path);

        EPCompiled Compile(Module module);

        Module ReadModule(string filename);

        EPCompiled ReadCompile(string filename);

        Module ParseModule(string moduleText);

        RegressionEnvironment CompileDeployAddListenerMileZero(
            string epl,
            string statementName);

        RegressionEnvironment CompileDeployAddListenerMile(
            string epl,
            string statementName,
            long milestone);

        RegressionEnvironment CompileDeploy(string epl);

        RegressionEnvironment CompileDeploy(
            bool soda,
            string epl);

        RegressionEnvironment CompileDeploy(
            string epl,
            Consumer<CompilerOptions> options);

        RegressionEnvironment CompileDeploy(
            bool soda,
            string epl,
            RegressionPath path);

        RegressionEnvironment CompileDeploy(
            string epl,
            RegressionPath path);

        RegressionEnvironment CompileDeploy(EPStatementObjectModel model);

        RegressionEnvironment CompileDeploy(
            EPStatementObjectModel model,
            RegressionPath path);

        RegressionEnvironment CompileDeployWBusPublicType(
            string epl,
            RegressionPath path);

        RegressionEnvironment Deploy(EPCompiled compiled);

        RegressionEnvironment Deploy(
            EPCompiled compiled,
            DeploymentOptions options);

        RegressionEnvironment Rollout(
            IList<EPDeploymentRolloutCompiled> items,
            RolloutOptions options);

        string DeployGetId(EPCompiled compiled);

        RegressionEnvironment UndeployAll();

        RegressionEnvironment UndeployModuleContaining(string statementName);

        RegressionEnvironment Undeploy(string deploymentId);

        EPFireAndForgetQueryResult CompileExecuteFAF(
            string query,
            RegressionPath path);

        EPFireAndForgetQueryResult CompileExecuteFAF(
            EPStatementObjectModel model,
            RegressionPath path);

        RegressionEnvironment SendEventObjectArray(
            object[] oa,
            string typeName);

        RegressionEnvironment SendEventBean(object @event);

        RegressionEnvironment SendEventBean(
            object @event,
            string typeName);

        RegressionEnvironment SendEventBeanStage(
            string stageUri,
            object @event);

        RegressionEnvironment SendEventMap(
            IDictionary<string, object> values,
            string typeName);

        RegressionEnvironment SendEventXMLDOM(
            XmlNode document,
            string typeName);

        RegressionEnvironment SendEventAvro(
            GenericRecord theEvent,
            string typeName);

        RegressionEnvironment SendEventJson(
            string json,
            string typeName);

        RegressionEnvironment AdvanceTime(long msec);

        RegressionEnvironment AdvanceTimeStage(
            string stageUri,
            long msec);

        RegressionEnvironment AdvanceTimeSpan(long msec);

        RegressionEnvironment AddListener(string statementName);

        RegressionEnvironment AddListener(
            string statementName,
            SupportListener listener);

        RegressionEnvironment Milestone(long num);

        RegressionEnvironment MilestoneInc(AtomicLong counter);

        EPStatement Statement(string statementName);

        IEnumerator<EventBean> GetEnumerator(string statementName);

        SupportListener Listener(string statementName);

        SupportListener ListenerStage(
            string stageUri,
            string statementName);

        string DeploymentId(string statementName);

        RegressionEnvironment EplToModelCompileDeploy(string epl);

        RegressionEnvironment EplToModelCompileDeploy(
            string epl,
            RegressionPath path);

        EPStatementObjectModel EplToModel(string epl);

        SupportListener ListenerNew();

        EPStageService StageService { get; }
    }

    public static class RegressionEnvironmentExtensions
    {
        public static T CopyMayFail<T>(
            this RegressionEnvironment env,
            T orig)
        {
            try {
                return SerializableObjectCopier
                    .GetInstance(env.Container)
                    .Copy(orig);
            }
            catch (Exception) {
                throw;
                //throw new AssertionException("Exception occurred during serialized copy", t);
            }
        }
    }
} // end of namespace