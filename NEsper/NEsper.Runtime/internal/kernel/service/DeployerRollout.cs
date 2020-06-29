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

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

using static com.espertech.esper.runtime.@internal.kernel.service.Deployer; // addPathDependencies, getRecoveryInformation
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperInitStatement; // initializeStatements
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperInitializeEPLObjects; // initializeEPLObjects, validateStagedEPLObjects
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperResolver; // resolveDependencies
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperUpdatePath; // updatePath;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class DeployerRollout
	{
		public static DeployerRolloutDeploymentResult Rollout(
			int currentStatementId,
			ICollection<EPDeploymentRolloutCompiled> itemsProvided,
			EPRuntimeSPI runtime)
		{
			var items = itemsProvided.ToArray();

			// per-deployment: determine deployment id
			var deploymentIds = new string[items.Length];
			var deploymentIdSet = new HashSet<string>();
			for (var i = 0; i < items.Length; i++) {
				deploymentIds[i] = DeployerHelperResolver.DetermineDeploymentIdCheckExists(
					i,
					items[i].Options,
					runtime.ServicesContext.DeploymentLifecycleService);
				if (!deploymentIdSet.Add(deploymentIds[i])) {
					throw new EPDeployException("Deployment id '" + deploymentIds[i] + "' occurs multiple times in the rollout", i);
				}
			}

			// per-deployment: obtain module providers
			var moduleProviders = new ModuleProviderCLPair[items.Length];
			for (var i = 0; i < items.Length; i++) {
				var classLoader = DeployerHelperResolver.GetClassLoader(i, items[i].Options.DeploymentClassLoaderOption, runtime.ServicesContext);
				try {
					moduleProviders[i] = ModuleProviderUtil.Analyze(items[i].Compiled, classLoader, runtime.ServicesContext.ClassProvidedPathRegistry);
				}
				catch (Exception) {
					RolloutCleanClassloader(deploymentIds, runtime.ServicesContext);
				}
			}

			// per-deployment: check dependencies and initialize EPL objects
			var inits = new DeployerRolloutInitResult[items.Length];
			for (var i = 0; i < items.Length; i++) {
				try {
					inits[i] = ResolveDependenciesInitEPLObjects(i, deploymentIds[i], moduleProviders[i], runtime.ServicesContext, runtime.StageService);
				}
				catch (EPDeployException ex) {
					RolloutCleanPathAndTypes(inits, deploymentIds, runtime.ServicesContext);
					throw ex;
				}
				catch (Exception ex) {
					RolloutCleanPathAndTypes(inits, deploymentIds, runtime.ServicesContext);
					throw new EPDeployException(ex.Message, ex, i);
				}
			}

			// per-deployment - obtain statement lightweights
			var stmtLightweights = new DeployerModuleStatementLightweights[items.Length];
			var numStatements = 0;
			for (var i = 0; i < items.Length; i++) {
				var statementIdStart = currentStatementId + numStatements;
				try {
					stmtLightweights[i] = InitializeStatements(
						i,
						false,
						inits[i].ModuleEPLObjects,
						inits[i].ModulePaths,
						inits[i].ModuleName,
						moduleProviders[i],
						deploymentIds[i],
						statementIdStart,
						items[i].Options.StatementUserObjectRuntime,
						items[i].Options.StatementNameRuntime,
						items[i].Options.StatementSubstitutionParameter,
						runtime.ServicesContext);
				}
				catch (EPDeployException ex) {
					RolloutCleanLightweights(stmtLightweights, inits, deploymentIds, moduleProviders, runtime.ServicesContext);
					throw ex;
				}
				catch (Exception ex) {
					RolloutCleanLightweights(stmtLightweights, inits, deploymentIds, moduleProviders, runtime.ServicesContext);
					throw new EPDeployException(ex.Message, ex, i);
				}

				numStatements += stmtLightweights[i].Lightweights.Count;
			}

			// per-deployment: start statements depending on context association
			var statements = new EPStatement[items.Length][];
			for (var i = 0; i < items.Length; i++) {
				try {
					statements[i] = DeployerHelperStatement.DeployStatements(
						i,
						stmtLightweights[i].Lightweights,
						false,
						inits[i].ModulePaths,
						moduleProviders[i],
						deploymentIds[i],
						runtime);
				}
				catch (EPDeployException ex) {
					RolloutCleanStatements(statements, stmtLightweights, inits, deploymentIds, moduleProviders, runtime.ServicesContext);
					throw ex;
				}
				catch (Exception t) {
					RolloutCleanStatements(statements, stmtLightweights, inits, deploymentIds, moduleProviders, runtime.ServicesContext);
					throw new EPDeployException(t.Message, t, i);
				}
			}

			// per-deployment: add paths dependency information and add deployment
			var deployments = new DeploymentInternal[items.Length];
			for (var i = 0; i < items.Length; i++) {
				try {
					// add dependencies
					AddPathDependencies(deploymentIds[i], moduleProviders[i].ModuleProvider.ModuleDependencies, runtime.ServicesContext);

					// keep statement and deployment
					deployments[i] = DeploymentInternal.From(
						deploymentIds[i],
						statements[i],
						inits[i].DeploymentIdDependencies,
						inits[i].ModulePaths,
						inits[i].ModuleEPLObjects,
						moduleProviders[i]);
					runtime.ServicesContext.DeploymentLifecycleService.AddDeployment(deploymentIds[i], deployments[i]);

					// register for recovery
					DeploymentRecoveryInformation recoveryInformation = GetRecoveryInformation(deployments[i]);
					runtime.ServicesContext.DeploymentRecoveryService.Add(
						deploymentIds[i],
						stmtLightweights[i].StatementIdFirstStatement,
						items[i].Compiled,
						recoveryInformation.StatementUserObjectsRuntime,
						recoveryInformation.StatementNamesWhenProvidedByAPI,
						stmtLightweights[i].SubstitutionParameters);
				}
				catch (Exception t) {
					RolloutCleanStatements(statements, stmtLightweights, inits, deploymentIds, moduleProviders, runtime.ServicesContext);
					throw new EPDeployException(t.Message, t, i);
				}
			}

			return new DeployerRolloutDeploymentResult(numStatements, deployments);
		}

		private static void RolloutCleanClassloader(
			string[] deploymentIds,
			EPServicesContext services)
		{
			for (var i = 0; i < deploymentIds.Length; i++) {
				services.ClassLoaderParent.Remove(deploymentIds[i]);
			}
		}

		private static void RolloutCleanPathAndTypes(
			DeployerRolloutInitResult[] inits,
			string[] deploymentIds,
			EPServicesContext services)
		{
			RolloutCleanClassloader(deploymentIds, services);

			for (var i = 0; i < inits.Length; i++) {
				Undeployer.DeleteFromPathRegistries(services, deploymentIds[i]);
				if (inits[i] != null) {
					Undeployer.DeleteFromEventTypeBus(services, inits[i].ModulePaths.DeploymentTypes);
				}
			}
		}

		private static void RolloutCleanLightweights(
			DeployerModuleStatementLightweights[] stmtLightweights,
			DeployerRolloutInitResult[] inits,
			string[] deploymentIds,
			ModuleProviderCLPair[] moduleProviders,
			EPServicesContext services)
		{
			for (var i = stmtLightweights.Length - 1; i >= 0; i--) {
				if (stmtLightweights[i] != null) {
					DeployerHelperResolver.ReverseDeployment(
						deploymentIds[i],
						inits[i].ModulePaths.DeploymentTypes,
						stmtLightweights[i].Lightweights,
						new EPStatement[0],
						moduleProviders[i],
						services);
					inits[i] = null;
				}
			}

			RolloutCleanPathAndTypes(inits, deploymentIds, services);
		}

		private static void RolloutCleanStatements(
			EPStatement[][] statements,
			DeployerModuleStatementLightweights[] stmtLightweights,
			DeployerRolloutInitResult[] inits,
			string[] deploymentIds,
			ModuleProviderCLPair[] moduleProviders,
			EPServicesContext services)
		{
			for (var i = statements.Length - 1; i >= 0; i--) {
				if (statements[i] != null) {
					DeployerHelperResolver.ReverseDeployment(
						deploymentIds[i],
						inits[i].ModulePaths.DeploymentTypes,
						stmtLightweights[i].Lightweights,
						statements[i],
						moduleProviders[i],
						services);
					stmtLightweights[i] = null;
					inits[i] = null;
				}
			}

			RolloutCleanLightweights(stmtLightweights, inits, deploymentIds, moduleProviders, services);
		}

		private static DeployerRolloutInitResult ResolveDependenciesInitEPLObjects(
			int rolloutItemNumber,
			string deploymentId,
			ModuleProviderCLPair moduleProvider,
			EPServicesContext services,
			EPStageService stageService)
		{
			var moduleDependencies = moduleProvider.ModuleProvider.ModuleDependencies;
			var deploymentIdDependencies = DeployerHelperResolver.ResolveDependencies(rolloutItemNumber, moduleDependencies, services);

			// initialize EPL objects defined by module
			var moduleEPLObjects = InitializeEPLObjects(moduleProvider, deploymentId, services);

			// determine staged EPL object overlap
			ValidateStagedEPLObjects(moduleEPLObjects, moduleProvider.ModuleProvider.ModuleName, rolloutItemNumber, stageService);

			// add EPL objects defined by module to path
			var moduleName = moduleProvider.ModuleProvider.ModuleName;
			var modulePaths = UpdatePath(rolloutItemNumber, moduleEPLObjects, moduleName, deploymentId, services);

			return new DeployerRolloutInitResult(deploymentIdDependencies, moduleEPLObjects, modulePaths, moduleName);
		}
	}
} // end of namespace
