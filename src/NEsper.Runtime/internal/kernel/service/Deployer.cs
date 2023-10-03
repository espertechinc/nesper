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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;
using com.espertech.esper.runtime.@internal.kernel.statement;

using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperInitStatement;
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperInitializeEPLObjects;
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperResolver;
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperStatement;
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperUpdatePath;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class Deployer
	{
		public static DeploymentInternal DeployFresh(
			string deploymentId,
			int statementIdFirstStatement,
			EPCompiled compiled,
			StatementNameRuntimeOption statementNameResolverRuntime,
			StatementUserObjectRuntimeOption userObjectResolverRuntime,
			StatementSubstitutionParameterOption substitutionParameterResolver,
			DeploymentClassLoaderOption deploymentClassLoaderOption,
			EPRuntimeSPI epRuntime)
		{
			return Deploy(
				false,
				deploymentId,
				statementIdFirstStatement,
				compiled,
				statementNameResolverRuntime,
				userObjectResolverRuntime,
				substitutionParameterResolver,
				deploymentClassLoaderOption,
				epRuntime);
		}

		public static DeploymentInternal DeployRecover(
			string deploymentId,
			int statementIdFirstStatement,
			EPCompiled compiled,
			StatementNameRuntimeOption statementNameResolverRuntime,
			StatementUserObjectRuntimeOption userObjectResolverRuntime,
			StatementSubstitutionParameterOption substitutionParameterResolver,
			DeploymentClassLoaderOption deploymentClassLoaderOption,
			EPRuntimeSPI epRuntime)
		{
			return Deploy(
				true,
				deploymentId,
				statementIdFirstStatement,
				compiled,
				statementNameResolverRuntime,
				userObjectResolverRuntime,
				substitutionParameterResolver,
				deploymentClassLoaderOption,
				epRuntime);
		}

		private static DeploymentInternal Deploy(
			bool recovery,
			string deploymentId,
			int statementIdFirstStatement,
			EPCompiled compiled,
			StatementNameRuntimeOption statementNameResolverRuntime,
			StatementUserObjectRuntimeOption userObjectResolverRuntime,
			StatementSubstitutionParameterOption substitutionParameterResolver,
			DeploymentClassLoaderOption deploymentClassLoaderOption,
			EPRuntimeSPI epRuntime)
		{
			// set variable local version
			epRuntime.ServicesContext.VariableManagementService.SetLocalVersion();

			try {
				return DeploySafe(
					recovery,
					deploymentId,
					statementIdFirstStatement,
					compiled,
					statementNameResolverRuntime,
					userObjectResolverRuntime,
					substitutionParameterResolver,
					deploymentClassLoaderOption,
					epRuntime);
			}
			catch (EPDeployException) {
				throw;
			}
			catch (Exception ex) {
				throw new EPDeployException(ex.Message, ex, -1);
			}
		}

		private static DeploymentInternal DeploySafe(
			bool recovery,
			string deploymentId,
			int statementIdFirstStatement,
			EPCompiled compiled,
			StatementNameRuntimeOption statementNameResolverRuntime,
			StatementUserObjectRuntimeOption userObjectResolverRuntime,
			StatementSubstitutionParameterOption substitutionParameterResolver,
			DeploymentClassLoaderOption deploymentClassLoaderOption,
			EPRuntimeSPI epRuntime)
		{
			var services = epRuntime.ServicesContext;
			var deploymentClassLoader = DeployerHelperResolver.GetClassLoader(-1, deploymentClassLoaderOption, services);
			var moduleProvider = ModuleProviderUtil.Analyze(
				compiled,
				deploymentClassLoader,
				services.ClassProvidedPathRegistry);
			if (moduleProvider.TypeResolver is ClassProvidedImportTypeResolver classProvidedImportClassLoader) {
				classProvidedImportClassLoader.Imported = moduleProvider.ModuleProvider.ModuleDependencies.PathClasses;
			}

			var moduleName = moduleProvider.ModuleProvider.ModuleName;

			// resolve external dependencies
			var moduleDependencies = moduleProvider.ModuleProvider.ModuleDependencies;
			var deploymentIdDependencies = ResolveDependencies(-1, moduleDependencies, services);

			// initialize EPL objects defined by module
			var moduleEPLObjects = InitializeEPLObjects(moduleProvider, deploymentId, services);

			// determine staged EPL object overlap
			ValidateStagedEPLObjects(moduleEPLObjects, moduleProvider.ModuleProvider.ModuleName, -1, epRuntime.StageService);

			// add EPL objects defined by module to path
			var modulePaths = UpdatePath(-1, moduleEPLObjects, moduleName, deploymentId, services);

			// obtain statement lightweights
			var stmtLightweights = InitializeStatements(
				-1,
				recovery,
				moduleEPLObjects,
				modulePaths,
				moduleName,
				moduleProvider,
				deploymentId,
				statementIdFirstStatement,
				userObjectResolverRuntime,
				statementNameResolverRuntime,
				substitutionParameterResolver,
				services);

			// start statements depending on context association
			var statements = DeployStatements(
				-1, stmtLightweights.Lightweights, recovery, modulePaths, moduleProvider, deploymentId, epRuntime);

			// add dependencies
			AddPathDependencies(deploymentId, moduleDependencies, services);

			// keep statement and deployment
			var deployed = DeploymentInternal.From(
				deploymentId,
				statements,
				deploymentIdDependencies,
				modulePaths,
				moduleEPLObjects,
				moduleProvider);
			services.DeploymentLifecycleService.AddDeployment(deploymentId, deployed);

			// register for recovery
			if (!recovery) {
				var recoveryInformation = GetRecoveryInformation(deployed);
				services.DeploymentRecoveryService.Add(
					deploymentId,
					statementIdFirstStatement,
					compiled,
					recoveryInformation.StatementUserObjectsRuntime,
					recoveryInformation.StatementNamesWhenProvidedByAPI,
					stmtLightweights.SubstitutionParameters,
					deployed.DeploymentIdDependencies);
			}

			return deployed;
		}

		internal static DeploymentRecoveryInformation GetRecoveryInformation(DeploymentInternal deployerResult)
		{
			IDictionary<int, object> userObjects = EmptyDictionary<int, object>.Instance;
			IDictionary<int, string> statementNamesWhenOverridden = EmptyDictionary<int, string>.Instance;

			foreach (var stmt in deployerResult.Statements) {
				var spi = (EPStatementSPI) stmt;
				if (stmt.UserObjectRuntime != null) {
					if (userObjects.IsEmpty()) {
						userObjects = new Dictionary<int, object>();
					}

					userObjects.Put(spi.StatementId, spi.StatementContext.UserObjectRuntime);
				}

				if (!spi.StatementContext.StatementInformationals.StatementNameCompileTime.Equals(spi.Name)) {
					if (statementNamesWhenOverridden.IsEmpty()) {
						statementNamesWhenOverridden = new Dictionary<int, string>();
					}

					statementNamesWhenOverridden.Put(spi.StatementId, spi.Name);
				}
			}

			return new DeploymentRecoveryInformation(userObjects, statementNamesWhenOverridden);
		}

		public static void AddPathDependencies(
			string deploymentId,
			ModuleDependenciesRuntime moduleDependencies,
			EPServicesContext services)
		{
			foreach (var eventType in moduleDependencies.PathEventTypes) {
				services.EventTypePathRegistry.AddDependency(eventType.Name, eventType.ModuleName, deploymentId);
			}

			foreach (var namedWindow in moduleDependencies.PathNamedWindows) {
				services.NamedWindowPathRegistry.AddDependency(namedWindow.Name, namedWindow.ModuleName, deploymentId);
			}

			foreach (var table in moduleDependencies.PathTables) {
				services.TablePathRegistry.AddDependency(table.Name, table.ModuleName, deploymentId);
			}

			foreach (var variable in moduleDependencies.PathVariables) {
				services.VariablePathRegistry.AddDependency(variable.Name, variable.ModuleName, deploymentId);
			}

			foreach (var context in moduleDependencies.PathContexts) {
				services.ContextPathRegistry.AddDependency(context.Name, context.ModuleName, deploymentId);
			}

			foreach (var exprDecl in moduleDependencies.PathExpressions) {
				services.ExprDeclaredPathRegistry.AddDependency(exprDecl.Name, exprDecl.ModuleName, deploymentId);
			}

			foreach (var script in moduleDependencies.PathScripts) {
				services.ScriptPathRegistry.AddDependency(new NameAndParamNum(script.Name, script.ParamNum), script.ModuleName, deploymentId);
			}

			foreach (var classProvided in moduleDependencies.PathClasses) {
				services.ClassProvidedPathRegistry.AddDependency(classProvided.Name, classProvided.ModuleName, deploymentId);
			}

			foreach (var index in moduleDependencies.PathIndexes) {
				EventTableIndexMetadata indexMetadata;
				if (index.IsNamedWindow) {
					var namedWindowName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathNamedWindows);
					var namedWindow = services.NamedWindowPathRegistry.GetWithModule(namedWindowName.Name, namedWindowName.ModuleName);
					indexMetadata = namedWindow.IndexMetadata;
				}
				else {
					var tableName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathTables);
					var table = services.TablePathRegistry.GetWithModule(tableName.Name, tableName.ModuleName);
					indexMetadata = table.IndexMetadata;
				}

				indexMetadata.AddIndexReference(index.IndexName, deploymentId);
			}
		}
	}
} // end of namespace
