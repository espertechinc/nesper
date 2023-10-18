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

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

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

        EPCompiled CompileWCheckedEx(string epl);

        EPCompiled CompileWCheckedEx(
            string epl,
            RegressionPath path);

        EPCompiled CompileWRuntimePath(string epl);
        
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

        EPFireAndForgetQueryResult CompileExecuteFAF(string query);
        
        void CompileExecuteFAFNoResult(string query, RegressionPath path);
        
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

        RegressionEnvironment AdvanceTimeSpan(long msec, long resolution);
        
        RegressionEnvironment AddListener(string statementName);

        RegressionEnvironment AddListener(
            string statementName,
            SupportListener listener);
        
        RegressionEnvironment SetSubscriber(string statementName);

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
        
        /// <summary>
        /// Assert iterator results with order as provided.
        /// Fails if the statement cannot be found.
        /// </summary>
        /// <param name="statementName">statement name</param>
        /// <param name="fields">property names</param>
        /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowIterator(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert iterator results without order by finding matches.
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowIteratorAnyOrder(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener last-invocation new-events, with order as provided, with old-events and other invocations ignored
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowLastNew(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener last-invocation new-events, with any order (finds matches), with old-events and other invocations ignored
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowLastNewAnyOrder(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener last-invocation old-events, with new-events and other invocations ignored
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowLastOld(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener last-invocation new-events and old-events must be none, and other invocations ignored
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowNewOnly(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener new-events of all invocations and old-events ignored
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowNewFlattened(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener old-events of all invocations and new-events ignored
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsPerRowOldFlattened(string statementName, string[] fields, object[][] expecteds);

	    /// <summary>
	    /// Assert all listener new-events and old-events expecting a single invocation that provided these events
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="newExpected">new-events expected values</param>
	    /// <param name="oldExpected">old-events expected values</param>
	    void AssertPropsPerRowIRPair(string statementName, string[] fields, object[][] newExpected, object[][] oldExpected);

	    /// <summary>
	    /// Assert all listener-accumulated new-events and old-events (any number of invocations).
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="newExpected">new-events expected values</param>
	    /// <param name="oldExpected">old-events expected values</param>
	    void AssertPropsPerRowIRPairFlattened(string statementName, string[] fields, object[][] newExpected, object[][] oldExpected);

	    /// <summary>
	    /// Assert listener that one new event and no old event is received and in a single invocation to the listener
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsNew(string statementName, string[] fields, object[] expecteds);

	    /// <summary>
	    /// Assert listener that one old event and no new event is received and in a single invocation to the listener
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="expecteds">expected values</param>
	    void AssertPropsOld(string statementName, string[] fields, object[] expecteds);

	    /// <summary>
	    /// Assert listener that one new event and one old event is received and in a single invocation to the listener
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="newExpected">new-events expected values</param>
	    /// <param name="oldExpected">old-events expected values</param>
	    void AssertPropsIRPair(string statementName, string[] fields, object[] newExpected, object[] oldExpected);

	    /// <summary>
	    /// Assert listener that one new event and one old event is received and in a single invocation to the listener
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="nameAndValuePairsNew">names and values to assert against in new events</param>
	    /// <param name="nameAndValuePairsOld">names and values to assert against in old events</param>
	    void AssertPropsNV(string statementName, object[][] nameAndValuePairsNew, object[][] nameAndValuePairsOld);

	    /// <summary>
	    /// Assert listener was invoked (not asserting the output received)
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    void AssertListenerInvoked(string statementName);

	    /// <summary>
	    /// Assert listener was not invoked
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    void AssertListenerNotInvoked(string statementName);

	    /// <summary>
	    /// Assert listener was or was not invoked
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="expected">flag indicating invoked or not</param>
	    void AssertListenerInvokedFlag(string statementName, bool expected);

	    /// <summary>
	    /// Assert listener was or was not invoked
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="expected">flag indicating invoked or not</param>
	    /// <param name="message">message</param>
	    void AssertListenerInvokedFlag(string statementName, bool expected, string message);

	    /// <summary>
	    /// Assert against a statement; Finds the statement by name and passes it to assertor.
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the statement</param>
	    void AssertStatement(string statementName, Consumer<EPStatement> assertor);

	    /// <summary>
	    /// Assert against a statement's listener; Finds the statement by name and passes its listener to assertor.
	    /// Fails if the statement cannot be found. Fails when there is no listener or there are multiple listeners for the statement.
	    /// Fails if the single listener is not a <seealso cref="SupportListener" />.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the listener</param>
	    void AssertListener(string statementName, Consumer<SupportListener> assertor);

	    /// <summary>
	    /// Assert against a statement's subscriber; Finds the statement by name and passes its subscriber to assertor.
	    /// Fails if the statement cannot be found. Fails when there is no subscriber.
	    /// Fails if the subscriber is not a <seealso cref="SupportSubscriber" />.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the listener</param>
	    void AssertSubscriber(string statementName, Consumer<SupportSubscriber> assertor);

	    /// <summary>
	    /// Assert listener that one new event and no old event is received and in a single invocation to the listener,
	    /// and passes the event to the assertor.
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the new event</param>
	    void AssertEventNew(string statementName, Consumer<EventBean> assertor);

	    /// <summary>
	    /// Assert listener that one old event and no new event is received and in a single invocation to the listener,
	    /// and passes the event to the assertor.
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the new event</param>
	    void AssertEventOld(string statementName, Consumer<EventBean> assertor);

	    /// <summary>
	    /// Assert against a statement's iterator; Finds the statement by name and passes its iterator to assertor.
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the listener</param>
	    void AssertIterator(string statementName, Consumer<IEnumerator<EventBean>> assertor);

	    /// <summary>
	    /// Assert against a statement's safe-iterator; Finds the statement by name and passes its safe-iterator to assertor.
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="assertor">receives the listener</param>
	    void AssertSafeEnumerator(string statementName, Consumer<IEnumerator<EventBean>> assertor);

	    /// <summary>
	    /// Assert listener that one new event and no old event is received and compares field value.
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fieldName">property name</param>
	    /// <param name="expected">expected value</param>
	    void AssertEqualsNew(string statementName, string fieldName, object expected);

	    /// <summary>
	    /// Assert listener that one old event and no new event is received and compares field value.
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fieldName">property name</param>
	    /// <param name="expected">expected value</param>
	    void AssertEqualsOld(string statementName, string fieldName, object expected);

	    /// <summary>
	    /// Assert against the current timetime, passing the runtime to the assertor.
	    /// Implementations may simply ignore this when not asserting.
	    /// </summary>
	    /// <param name="assertor">assertion logic</param>
	    void AssertRuntime(Consumer<EPRuntime> assertor);

	    /// <summary>
	    /// Assert when-available.
	    /// Implementations may simply ignore this when not asserting.
	    /// </summary>
	    /// <param name="runnable">assertion logic</param>
	    void AssertThat(Runnable runnable);

	    /// <summary>
	    /// Reset listener.
	    /// Fails if the statement cannot be found or the statement does not have a single <seealso cref="SupportListener" /> listener.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    void ListenerReset(string statementName);

	    /// <summary>
	    /// Assert statement property types
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="classes">property types</param>
	    void AssertStmtTypes(string statementName, string[] fields, Type[] classes) {
	        AssertStatement(statementName, statement => SupportEventPropUtil.AssertTypes(statement.EventType, fields, classes));
	    }

	    /// <summary>
	    /// Assert statement property type
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="field">property name</param>
	    /// <param name="clazz">property type</param>
	    void AssertStmtType(string statementName, string field, Type clazz) {
	        AssertStatement(statementName, statement => SupportEventPropUtil.AssertTypes(statement.EventType, field, clazz));
	    }

	    /// <summary>
	    /// Assert statement property types are all the same type
	    /// Fails if the statement cannot be found.
	    /// </summary>
	    /// <param name="statementName">statement name</param>
	    /// <param name="fields">property names</param>
	    /// <param name="clazz">property type</param>
	    void AssertStmtTypesAllSame(string statementName, string[] fields, Type clazz) {
	        AssertStatement(statementName, statement => SupportEventPropUtil.AssertTypesAllSame(statement.EventType, fields, clazz));
	    }

	    void RuntimeSetVariable(string statementNameOfDeployment, string variableName, object value);
	    Schema RuntimeAvroSchemaPreconfigured(string eventTypeName);
	    Schema RuntimeAvroSchemaByDeployment(string statementNameToFind, string eventTypeName);

	    void TryInvalidCompile(string epl, string message);
	    void TryInvalidCompile(RegressionPath path, string epl, string message);
	    void TryInvalidCompileFAF(RegressionPath path, string epl, string message);
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
        
        public static Configuration MinimalConfiguration(this RegressionEnvironment env)
        {
            var configuration = new Configuration();
            configuration.Common.Scripting.Engines.AddAll(env.Configuration.Common.Scripting.Engines);
            return configuration;
        }

    }
} // end of namespace