///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class DeployerHelperResolver
	{
		public static TypeResolver GetTypeResolver(
			int rolloutItemNumber,
			DeploymentTypeResolverOption deploymentTypeResolverOption,
			EPServicesContext servicesContext)
		{
			TypeResolver deploymentTypeResolver = servicesContext.TypeResolverParent;
			if (deploymentTypeResolverOption != null) {
				deploymentTypeResolver = deploymentTypeResolverOption(
					new DeploymentClassLoaderContext(
						servicesContext.TypeResolverParent,
						servicesContext.ConfigSnapshot));
				if (deploymentTypeResolver == null) {
					throw new EPDeployException("Deployment classloader option returned a null value for the classloader", rolloutItemNumber);
				}
			}

			return deploymentTypeResolver;
		}

		public static string DetermineDeploymentIdCheckExists(
			int rolloutItemNumber,
			DeploymentOptions optionsMayNull,
			DeploymentLifecycleService deploymentLifecycleService)
		{
			string deploymentId;
			if (optionsMayNull == null || optionsMayNull.DeploymentId == null) {
				// the CRC may already exists, however this is very unlikely
				long crc;
				do {
					deploymentId = Guid.NewGuid().ToString();
					crc = CRC32Util.ComputeCRC32(deploymentId);
				} while (deploymentLifecycleService.GetDeploymentByCRC(crc) != null);
			}
			else {
				deploymentId = optionsMayNull.DeploymentId;
			}

			if (deploymentLifecycleService.GetDeploymentById(deploymentId) != null) {
				throw new EPDeployDeploymentExistsException("Deployment by id '" + deploymentId + "' already exists", rolloutItemNumber);
			}

			return deploymentId;
		}

		public static ISet<string> ResolveDependencies(
			int rolloutItemNumber,
			ModuleDependenciesRuntime moduleDependencies,
			EPServicesContext services)
		{
			ISet<string> dependencies = new HashSet<string>();

			foreach (var publicEventType in moduleDependencies.PublicEventTypes) {
				if (services.EventTypeRepositoryBus.GetTypeByName(publicEventType) == null) {
					throw MakePreconditionExceptionPreconfigured(rolloutItemNumber, PathRegistryObjectType.EVENTTYPE, publicEventType);
				}
			}

			foreach (var publicVariable in moduleDependencies.PublicVariables) {
				if (services.ConfigSnapshot.Common.Variables.Get(publicVariable) == null) {
					throw MakePreconditionExceptionPreconfigured(rolloutItemNumber, PathRegistryObjectType.VARIABLE, publicVariable);
				}
			}

			foreach (var pathNamedWindow in moduleDependencies.PathNamedWindows) {
				var depIdNamedWindow = services.NamedWindowPathRegistry.GetDeploymentId(pathNamedWindow.Name, pathNamedWindow.ModuleName);
				if (depIdNamedWindow == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.NAMEDWINDOW, pathNamedWindow);
				}

				dependencies.Add(depIdNamedWindow);
			}

			foreach (var pathTable in moduleDependencies.PathTables) {
				var depIdTable = services.TablePathRegistry.GetDeploymentId(pathTable.Name, pathTable.ModuleName);
				if (depIdTable == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.TABLE, pathTable);
				}

				dependencies.Add(depIdTable);
			}

			foreach (var pathEventType in moduleDependencies.PathEventTypes) {
				var depIdEventType = services.EventTypePathRegistry.GetDeploymentId(pathEventType.Name, pathEventType.ModuleName);
				if (depIdEventType == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.EVENTTYPE, pathEventType);
				}

				dependencies.Add(depIdEventType);
			}

			foreach (var pathVariable in moduleDependencies.PathVariables) {
				var depIdVariable = services.VariablePathRegistry.GetDeploymentId(pathVariable.Name, pathVariable.ModuleName);
				if (depIdVariable == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.VARIABLE, pathVariable);
				}

				dependencies.Add(depIdVariable);
			}

			foreach (var pathContext in moduleDependencies.PathContexts) {
				var depIdContext = services.ContextPathRegistry.GetDeploymentId(pathContext.Name, pathContext.ModuleName);
				if (depIdContext == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.CONTEXT, pathContext);
				}

				dependencies.Add(depIdContext);
			}

			foreach (var pathExpression in moduleDependencies.PathExpressions) {
				var depIdExpression = services.ExprDeclaredPathRegistry.GetDeploymentId(pathExpression.Name, pathExpression.ModuleName);
				if (depIdExpression == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.EXPRDECL, pathExpression);
				}

				dependencies.Add(depIdExpression);
			}

			foreach (var pathScript in moduleDependencies.PathScripts) {
				var depIdExpression = services.ScriptPathRegistry.GetDeploymentId(
					new NameAndParamNum(pathScript.Name, pathScript.ParamNum),
					pathScript.ModuleName);
				if (depIdExpression == null) {
					throw MakePreconditionExceptionPath(
						rolloutItemNumber,
						PathRegistryObjectType.SCRIPT,
						new NameAndModule(pathScript.Name, pathScript.ModuleName));
				}

				dependencies.Add(depIdExpression);
			}

			foreach (var index in moduleDependencies.PathIndexes) {
				string depIdIndex;
				if (index.IsNamedWindow) {
					var namedWindowName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathNamedWindows);
					var namedWindow = services.NamedWindowPathRegistry.GetWithModule(namedWindowName.Name, namedWindowName.ModuleName);
					depIdIndex = namedWindow.IndexMetadata.GetIndexDeploymentId(index.IndexName);
				}
				else {
					var tableName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathTables);
					var table = services.TablePathRegistry.GetWithModule(tableName.Name, tableName.ModuleName);
					depIdIndex = table.IndexMetadata.GetIndexDeploymentId(index.IndexName);
				}

				if (depIdIndex == null) {
					throw MakePreconditionExceptionPath(
						rolloutItemNumber,
						PathRegistryObjectType.INDEX,
						new NameAndModule(index.IndexName, index.IndexModuleName));
				}

				dependencies.Add(depIdIndex);
			}

			foreach (var pathClass in moduleDependencies.PathClasses) {
				var depIdClass = services.ClassProvidedPathRegistry.GetDeploymentId(pathClass.Name, pathClass.ModuleName);
				if (depIdClass == null) {
					throw MakePreconditionExceptionPath(rolloutItemNumber, PathRegistryObjectType.CLASSPROVIDED, pathClass);
				}

				dependencies.Add(depIdClass);
			}

			return dependencies;
		}

		private static EPDeployPreconditionException MakePreconditionExceptionPath(
			int rolloutItemNumber,
			PathRegistryObjectType objectType,
			NameAndModule nameAndModule)
		{
			var message = "Required dependency ";
			message += objectType.Name + " '" + nameAndModule.Name + "'";
			if (!string.IsNullOrEmpty(nameAndModule.ModuleName)) {
				message += " module '" + nameAndModule.ModuleName + "'";
			}

			message += " cannot be found";
			return new EPDeployPreconditionException(message, rolloutItemNumber);
		}

		private static EPDeployPreconditionException MakePreconditionExceptionPreconfigured(
			int rolloutItemNumber,
			PathRegistryObjectType objectType,
			string name)
		{
			var message = "Required pre-configured ";
			message += objectType.Name + " '" + name + "'";
			message += " cannot be found";
			return new EPDeployPreconditionException(message, rolloutItemNumber);
		}

		public static void ReverseDeployment(
			string deploymentId,
			IDictionary<long, EventType> deploymentTypes,
			IList<StatementLightweight> lightweights,
			EPStatement[] statements,
			ModuleProviderCLPair provider,
			EPServicesContext services)
		{
			var revert = new List<StatementContext>();
			foreach (var stmtToRemove in lightweights) {
				revert.Add(stmtToRemove.StatementContext);
			}

			revert.Reverse();
			var reverted = revert.ToArray();
			Undeployer.Disassociate(statements);
			Undeployer.Undeploy(deploymentId, deploymentTypes, reverted, provider.ModuleProvider, services);
		}
	}
} // end of namespace
