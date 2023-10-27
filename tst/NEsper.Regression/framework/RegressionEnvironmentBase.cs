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
using com.espertech.esper.common.client.scopetest;
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

using NEsper.Avro.Support;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.framework
{
	public abstract class RegressionEnvironmentBase : RegressionEnvironment
	{
		static RegressionEnvironmentBase()
		{
			RegressionCore.Initialize();
		}
		
		public RegressionEnvironmentBase(
			Configuration configuration,
			EPRuntime runtime)
		{
			Configuration = configuration;
			Runtime = runtime;
		}

		public abstract EPCompiler Compiler { get; }
		
		public Module ParseModule(string moduleText)
		{
			try {
				return Compiler.ParseModule(moduleText);
			}
			catch (Exception ex) {
				throw new EPRuntimeException(ex);
			}
		}

		public EPCompiled CompileFAF(
			string query,
			RegressionPath path)
		{
			var args = GetArgsNoExport(path);
			try {
				return Compiler.CompileQuery(query, args);
			}
			catch (Exception ex) {
				throw NotExpected(ex);
			}
		}

		public EPCompiled CompileFAF(
			EPStatementObjectModel model,
			RegressionPath path)
		{
			var args = GetArgsNoExport(path);
			try {
				return Compiler.CompileQuery(model, args);
			}
			catch (Exception ex) {
				throw NotExpected(ex);
			}
		}

		public EPFireAndForgetQueryResult CompileExecuteFAF(
			string query,
			RegressionPath path)
		{
			var compiled = CompileFAF(query, path);
			return Runtime.FireAndForgetService.ExecuteQuery(compiled);
		}

		public EPFireAndForgetQueryResult CompileExecuteFAF(string query)
		{
			var compiled = CompileFAF(query, new RegressionPath());
			return Runtime.FireAndForgetService.ExecuteQuery(compiled);
		}

		public void CompileExecuteFAFNoResult(
			string query,
			RegressionPath path)
		{
			CompileExecuteFAF(query, path);
		}

		public virtual RegressionEnvironment AddListener(string statementName)
		{
			return null;
		}

		public EPFireAndForgetQueryResult CompileExecuteFAF(
			EPStatementObjectModel model,
			RegressionPath path)
		{
			var compiled = CompileFAF(model, path);
			return Runtime.FireAndForgetService.ExecuteQuery(compiled);
		}

		public EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled)
		{
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

		public RegressionEnvironment SendEventBeanStage(
			string stageUri,
			object @event)
		{
			if (stageUri == null) {
				return SendEventBean(@event);
			}

			var stage = Runtime.StageService.GetExistingStage(stageUri);
			if (stage == null) {
				throw new EPRuntimeException("Failed to find stage '" + stageUri + "'");
			}

			stage.EventService.SendEventBean(@event, @event.GetType().Name);
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

		public RegressionEnvironment SendEventJson(
			string json,
			string typeName)
		{
			Runtime.EventService.SendEventJson(json, typeName);
			return this;
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

		public RegressionEnvironment AdvanceTimeSpan(
			long msec,
			long resolution)
		{
			Runtime.EventService.AdvanceTimeSpan(msec, resolution);
			return this;
		}

		public RegressionEnvironment AdvanceTime(long msec)
		{
			Runtime.EventService.AdvanceTime(msec);
			return this;
		}

		public RegressionEnvironment AdvanceTimeStage(
			string stageUri,
			long msec)
		{
			if (stageUri == null) {
				AdvanceTime(msec);
				return this;
			}

			var stage = Runtime.StageService.GetExistingStage(stageUri);
			if (stage == null) {
				throw new EPRuntimeException("Failed to find stage '" + stageUri + "'");
			}

			stage.EventService.AdvanceTime(msec);
			return this;
		}

		public SupportListener Listener(string statementName)
		{
			return GetRequireStatementListener(statementName, Runtime);
		}

		public SupportSubscriber Subscriber(string statementName)
		{
			return GetRequireStatementSubscriber(statementName, Runtime);
		}

		public SupportListener ListenerStage(
			string stageUri,
			string statementName)
		{
			return GetRequireStatementListener(statementName, stageUri, Runtime);
		}

		public string DeploymentId(string statementName)
		{
			var statement = (EPStatementSPI)GetRequireStatement(statementName, Runtime);
			return statement.StatementContext.DeploymentId;
		}

		public RegressionEnvironment UndeployAll()
		{
			try {
				Runtime.DeploymentService.UndeployAll();

				var stageURIs = Runtime.StageService.StageURIs;
				foreach (var uri in stageURIs) {
					var stage = Runtime.StageService.GetExistingStage(uri);
					stage.Destroy();
				}
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

		public RegressionEnvironment SetSubscriber(string statementName)
		{
			GetRequireStatement(statementName, Runtime).Subscriber = new SupportSubscriber();
			return this;
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
				Assert.AreEqual(epl, model.ToEPL());
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
				module.ModuleText = model.ToEPL();
				return Compiler.Compile(module, args);
			}
			catch (Exception ex) {
				throw NotExpected(ex);
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

		protected EPStatement GetAssertStatement(string statementName)
		{
			return GetRequireStatement(statementName, Runtime);
		}

		public EPCompiled Compile(
			string epl,
			CompilerArguments arguments)
		{
			try {
				arguments.Configuration = Configuration;
				return Compiler.Compile(epl, arguments);
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
				compilerOptions => compilerOptions
					.SetBusModifierEventType(ctx => EventTypeBusModifier.BUS)
					.SetAccessModifierEventType(ctx => NameAccessModifier.PUBLIC)
					.SetAccessModifierNamedWindow(ctx => NameAccessModifier.PUBLIC)
					.SetAccessModifierTable(ctx => NameAccessModifier.PUBLIC));
		}

		public EPCompiled Compile(string epl)
		{
			return TryCompile(epl, null);
		}

		public EPStatementObjectModel EplToModel(string epl)
		{
			try {
				var model = Compiler.EplToModel(epl, Configuration);
				return this.CopyMayFail(model); // copy to test serializability
				// return SerializableObjectCopier.CopyMayAssert.Fail(model); // copy to test serializability
			}
			catch (EPCompileException t) {
				throw NotExpected(t);
			}
		}

		public IContainer Container => Configuration.Container;
		
		public string RuntimeURI => Runtime.URI;

		public EPRuntime Runtime { get; }

		public EPDeploymentService Deployment => Runtime.DeploymentService;

		public EPEventService EventService => Runtime.EventService;

		public EPStageService StageService => Runtime.StageService;

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

			return Compiler.Compile(epl, args);
		}

		public EPCompiled CompileWRuntimePath(string epl)
		{
			return Compile(epl, new CompilerArguments(Runtime.RuntimePath));
		}

		public Module ReadModule(string filename)
		{
			try {
                return Compiler.ReadModule(filename, Container.ResourceManager());
			}
			catch (Exception ex) {
				throw new EPRuntimeException(ex);
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
				return Compiler.Compile(module, new CompilerArguments(Configuration));
			}
			catch (Exception ex) {
				throw new EPRuntimeException(ex);
			}
		}

		public RegressionEnvironment Rollout(
			IList<EPDeploymentRolloutCompiled> items,
			RolloutOptions options)
		{
			try {
				Runtime.DeploymentService.Rollout(items, options);
				return this;
			}
			catch (EPDeployException ex) {
				throw NotExpected(ex);
			}
		}

		public void AssertPropsPerRowIterator(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			EPAssertionUtil.AssertPropsPerRow(GetEnumerator(statementName), fields, expecteds);
		}

		public void AssertPropsPerRowIteratorAnyOrder(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			EPAssertionUtil.AssertPropsPerRowAnyOrder(GetEnumerator(statementName), fields, expecteds);
		}

		public void AssertPropsNew(
			string statementName,
			string[] fields,
			object[] expecteds)
		{
			EPAssertionUtil.AssertProps(Listener(statementName).AssertOneGetNewAndReset(), fields, expecteds);
		}

		public void AssertPropsOld(
			string statementName,
			string[] fields,
			object[] expecteds)
		{
			EPAssertionUtil.AssertProps(Listener(statementName).AssertOneGetOldAndReset(), fields, expecteds);
		}

		public void AssertPropsPerRowLastNew(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			EPAssertionUtil.AssertPropsPerRow(Listener(statementName).GetAndResetLastNewData(), fields, expecteds);
		}

		public void AssertPropsPerRowNewOnly(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			var listener = Listener(statementName);
			EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, expecteds);
			Assert.IsNull(listener.LastOldData);
			listener.Reset();
		}

		public void AssertPropsPerRowLastNewAnyOrder(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			EPAssertionUtil.AssertPropsPerRowAnyOrder(Listener(statementName).GetAndResetLastNewData(), fields, expecteds);
		}

		public void AssertPropsPerRowLastOld(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			EPAssertionUtil.AssertPropsPerRow(Listener(statementName).GetAndResetLastOldData(), fields, expecteds);
		}

		public void AssertPropsIRPair(
			string statementName,
			string[] fields,
			object[] newExpected,
			object[] oldExpected)
		{
			EPAssertionUtil.AssertProps(
				Listener(statementName).AssertPairGetIRAndReset(),
				fields,
				newExpected,
				oldExpected);
		}

		public void AssertListenerInvoked(string statementName)
		{
			Assert.IsTrue(Listener(statementName).IsInvokedAndReset());
		}

		public void AssertListenerNotInvoked(string statementName)
		{
			Assert.IsFalse(Listener(statementName).IsInvoked);
		}

		public void AssertListenerInvokedFlag(
			string statementName,
			bool expected)
		{
			Assert.AreEqual(expected, Listener(statementName).IsInvokedAndReset());
		}

		public void AssertListenerInvokedFlag(
			string statementName,
			bool expected,
			string message)
		{
			Assert.AreEqual(expected, Listener(statementName).IsInvokedAndReset(), message);
		}

		public void ListenerReset(string statementName)
		{
			Listener(statementName).Reset();
		}

		public void AssertPropsPerRowIRPair(
			string statementName,
			string[] fields,
			object[][] newExpected,
			object[][] oldExpected)
		{
			var listener = Listener("s0");
			Assert.AreEqual(1, listener.NewDataList.Count);
			Assert.AreEqual(1, listener.OldDataList.Count);
			EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, newExpected);
			EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, oldExpected);
			listener.Reset();
		}

		public void AssertPropsPerRowIRPairFlattened(
			string statementName,
			string[] fields,
			object[][] newExpected,
			object[][] oldExpected)
		{
			var listener = Listener("s0");
			EPAssertionUtil.AssertPropsPerRow(listener.NewDataListFlattened, fields, newExpected);
			EPAssertionUtil.AssertPropsPerRow(listener.OldDataListFlattened, fields, oldExpected);
			listener.Reset();
		}

		public void AssertPropsNV(
			string statementName,
			object[][] nameAndValuePairsNew,
			object[][] nameAndValuePairsOld)
		{
			var listener = Listener(statementName);
			Assert.AreEqual(1, listener.NewDataList.Count);
			Assert.AreEqual(1, listener.OldDataList.Count);
			EPAssertionUtil.AssertNameValuePairs(listener.LastNewData, nameAndValuePairsNew);
			EPAssertionUtil.AssertNameValuePairs(listener.LastOldData, nameAndValuePairsOld);
			listener.Reset();
		}

		public void AssertStatement(
			string statementName,
			Consumer<EPStatement> assertor)
		{
			assertor.Invoke(Statement(statementName));
		}

		public void AssertThat(Runnable runnable)
		{
			runnable.Invoke();
		}

		public void AssertRuntime(Consumer<EPRuntime> assertor)
		{
			assertor.Invoke(Runtime);
		}

		public void AssertListener(
			string statementName,
			Consumer<SupportListener> assertor)
		{
			assertor.Invoke(Listener(statementName));
		}

		public void AssertSubscriber(
			string statementName,
			Consumer<SupportSubscriber> assertor)
		{
			assertor.Invoke(Subscriber(statementName));
		}

		public void AssertIterator(
			string statementName,
			Consumer<IEnumerator<EventBean>> assertor)
		{
			assertor.Invoke(GetEnumerator(statementName));
		}

		public void AssertSafeEnumerator(
			string statementName,
			Consumer<IEnumerator<EventBean>> assertor)
		{
			assertor.Invoke(Statement(statementName).GetSafeEnumerator());
		}

		public void AssertEventNew(
			string statementName,
			Consumer<EventBean> assertor)
		{
			assertor.Invoke(Listener(statementName).AssertOneGetNewAndReset());
		}

		public void AssertEventOld(
			string statementName,
			Consumer<EventBean> assertor)
		{
			assertor.Invoke(Listener(statementName).AssertOneGetOldAndReset());
		}

		public void AssertPropsPerRowNewFlattened(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			var listener = Listener(statementName);
			EPAssertionUtil.AssertPropsPerRow(listener.NewDataListFlattened, fields, expecteds);
			listener.Reset();
		}

		public void AssertPropsPerRowOldFlattened(
			string statementName,
			string[] fields,
			object[][] expecteds)
		{
			var listener = Listener(statementName);
			EPAssertionUtil.AssertPropsPerRow(listener.OldDataListFlattened, fields, expecteds);
			listener.Reset();
		}

		public void AssertEqualsNew(
			string statementName,
			string fieldName,
			object expected)
		{
			Assert.AreEqual(expected, Listener(statementName).AssertOneGetNewAndReset().Get(fieldName));
		}

		public void AssertEqualsOld(
			string statementName,
			string fieldName,
			object expected)
		{
			Assert.AreEqual(expected, Listener(statementName).AssertOneGetOldAndReset().Get(fieldName));
		}

		public void TryInvalidCompile(
			string epl,
			string message)
		{
			try {
				CompileWCheckedEx(epl);
				Assert.Fail();
			}
			catch (EPCompileException ex) {
				AssertMessage(ex, message);
			}
		}

		public void TryInvalidCompile(
			RegressionPath path,
			string epl,
			string message)
		{
			try {
				CompileWCheckedEx(epl, path);
				Assert.Fail();
			}
			catch (EPCompileException ex) {
				AssertMessage(ex, message);
			}
		}

		public void TryInvalidCompileFAF(
			RegressionPath path,
			string faf,
			string expected)
		{
			try {
				var args = new CompilerArguments(Configuration);
				args.Path.AddAll(path.Compileds);
				Compiler.CompileQuery(faf, args);
				Assert.Fail();
			}
			catch (EPCompileException ex) {
				AssertMessage(ex, expected);
			}
		}

		public void RuntimeSetVariable(
			string statementNameOfDeployment,
			string variableName,
			object value)
		{
			var deploymentId = statementNameOfDeployment == null ? null : DeploymentId(statementNameOfDeployment);
			Runtime.VariableService.SetVariableValue(deploymentId, variableName, value);
		}

		public Schema RuntimeAvroSchemaPreconfigured(string eventTypeName)
		{
			return SupportAvroUtil.GetAvroSchema(Runtime.EventTypeService.GetEventTypePreconfigured(eventTypeName));
		}

		public Schema RuntimeAvroSchemaByDeployment(
			string statementNameToFind,
			string eventTypeName)
		{
			var deploymentId = DeploymentId(statementNameToFind);
			var eventType = Runtime.EventTypeService.GetEventType(deploymentId, eventTypeName);
			if (eventType == null) {
				throw new ArgumentException(
					"Failed to find event type '" + eventTypeName + "' at deployment Id '" + deploymentId + "'");
			}

			return SupportAvroUtil.GetAvroSchema(eventType);
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
					return Compiler.Compile(epl, new CompilerArguments(Configuration));
				}

				var args = new CompilerArguments(Configuration);
				options.Invoke(args.Options);

				return Compiler.Compile(epl, args);
			}
			catch (Exception ex) {
				throw NotExpected(ex);
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
			return args;
		}

		private Exception NotExpected(Exception ex)
		{
			throw new EPRuntimeException("Test failed due to exception: " + ex.Message, ex);
		}
	}
} // end of namespace
