///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.util;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class DeployerHelperDependencies
	{
		private readonly static Func<NameAndParamNum, string> SCRIPT_OBJECTNAME = nameParam => 
			nameParam.Name + "#" + nameParam.ParamNum.ToString();

		private readonly static Func<ModuleIndexMeta, string> INDEX_OBJECTNAME = idx =>
			idx.IndexName + (idx.IsNamedWindow ? " on named-window " : " on table ") + idx.InfraName;

		// what are the dependencies that the given deployment consumes from other modules?
		public static EPDeploymentDependencyConsumed GetDependenciesConsumed(
			string selfDeploymentId,
			EPServicesPath paths,
			DeploymentLifecycleService deploymentLifecycleService)
		{
			var selfDeployment = deploymentLifecycleService.GetDeploymentById(selfDeploymentId);
			if (selfDeployment == null) {
				return null;
			}

			string[] consumedDeploymentIds = selfDeployment.DeploymentIdDependencies;
			var consumed = new List<EPDeploymentDependencyConsumed.Item>(4);
			foreach (string providerDeploymentId in consumedDeploymentIds) {
				var providingDeployment = deploymentLifecycleService.GetDeploymentById(providerDeploymentId);
				if (providingDeployment == null) {
					continue;
				}

				string moduleName = providingDeployment.ModuleProvider.ModuleName;
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathNamedWindows,
					EPObjectType.NAMEDWINDOW,
					paths.NamedWindowPathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathTables,
					EPObjectType.TABLE,
					paths.TablePathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathVariables,
					EPObjectType.VARIABLE,
					paths.VariablePathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathContexts,
					EPObjectType.CONTEXT,
					paths.ContextPathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathEventTypes,
					EPObjectType.EVENTTYPE,
					paths.EventTypePathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathExprDecls,
					EPObjectType.EXPRESSION,
					paths.ExprDeclaredPathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathScripts,
					EPObjectType.SCRIPT,
					paths.ScriptPathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					SCRIPT_OBJECTNAME);
				HandleConsumed(
					providerDeploymentId,
					providingDeployment.PathClassProvideds,
					EPObjectType.CLASSPROVIDED,
					paths.ClassProvidedPathRegistry,
					moduleName,
					selfDeploymentId,
					consumed,
					name => name);

				foreach (ModuleIndexMeta objectName in providingDeployment.PathIndexes) {
					EventTableIndexMetadata indexMetadata = GetIndexMetadata(objectName, moduleName, paths);
					if (indexMetadata == null) {
						continue;
					}

					EventTableIndexMetadataEntry meta = indexMetadata.GetIndexEntryByName(objectName.IndexName);
					if (meta != null && meta.ReferringDeployments != null && meta.ReferringDeployments.Length > 0) {
						bool found = false;
						foreach (string dep in meta.ReferringDeployments) {
							if (dep.Equals(selfDeploymentId)) {
								found = true;
								break;
							}
						}

						if (found) {
							consumed.Add(
								new EPDeploymentDependencyConsumed.Item(providerDeploymentId, EPObjectType.INDEX, INDEX_OBJECTNAME.Invoke(objectName)));
						}
					}
				}
			}

			return new EPDeploymentDependencyConsumed(consumed);
		}

		// what are the dependencies that the given deployment provides to other modules?
		public static EPDeploymentDependencyProvided GetDependenciesProvided(
			string selfDeploymentId,
			EPServicesPath paths,
			DeploymentLifecycleService deploymentLifecycleService)
		{
			DeploymentInternal selfDeployment = deploymentLifecycleService.GetDeploymentById(selfDeploymentId);
			if (selfDeployment == null) {
				return null;
			}

			IList<EPDeploymentDependencyProvided.Item> dependencies = new List<EPDeploymentDependencyProvided.Item>(4);
			string moduleName = selfDeployment.ModuleProvider.ModuleName;
			HandleProvided(selfDeployment.PathNamedWindows, EPObjectType.NAMEDWINDOW, paths.NamedWindowPathRegistry, moduleName, dependencies, name => name);
			HandleProvided(selfDeployment.PathTables, EPObjectType.TABLE, paths.TablePathRegistry, moduleName, dependencies, name => name);
			HandleProvided(selfDeployment.PathVariables, EPObjectType.VARIABLE, paths.VariablePathRegistry, moduleName, dependencies, name => name);
			HandleProvided(selfDeployment.PathContexts, EPObjectType.CONTEXT, paths.ContextPathRegistry, moduleName, dependencies, name => name);
			HandleProvided(selfDeployment.PathEventTypes, EPObjectType.EVENTTYPE, paths.EventTypePathRegistry, moduleName, dependencies, name => name);
			HandleProvided(selfDeployment.PathExprDecls, EPObjectType.EXPRESSION, paths.ExprDeclaredPathRegistry, moduleName, dependencies, name => name);
			HandleProvided(selfDeployment.PathScripts, EPObjectType.SCRIPT, paths.ScriptPathRegistry, moduleName, dependencies, SCRIPT_OBJECTNAME);
			HandleProvided(
				selfDeployment.PathClassProvideds,
				EPObjectType.CLASSPROVIDED,
				paths.ClassProvidedPathRegistry,
				moduleName,
				dependencies,
				name => name);

			foreach (ModuleIndexMeta objectName in selfDeployment.PathIndexes) {
				EventTableIndexMetadata indexMetadata = GetIndexMetadata(objectName, moduleName, paths);
				if (indexMetadata == null) {
					continue;
				}

				EventTableIndexMetadataEntry meta = indexMetadata.GetIndexEntryByName(objectName.IndexName);
				if (meta != null && meta.ReferringDeployments != null && meta.ReferringDeployments.Length > 0) {
					ISet<string> referred = new HashSet<string>(Arrays.AsList(meta.ReferringDeployments));
					referred.Remove(selfDeploymentId);
					if (!referred.IsEmpty()) {
						dependencies.Add(new EPDeploymentDependencyProvided.Item(EPObjectType.INDEX, INDEX_OBJECTNAME.Invoke(objectName), referred));
					}
				}
			}

			return new EPDeploymentDependencyProvided(dependencies);
		}

		private static EventTableIndexMetadata GetIndexMetadata(
			ModuleIndexMeta objectName,
			string moduleName,
			EPServicesPath paths)
		{
			if (objectName.IsNamedWindow) {
				NamedWindowMetaData metaData = paths.NamedWindowPathRegistry.GetWithModule(objectName.InfraName, moduleName);
				return metaData?.IndexMetadata;
			}
			else {
				TableMetaData metaData = paths.TablePathRegistry.GetWithModule(objectName.InfraName, moduleName);
				return metaData?.IndexMetadata;
			}
		}

		private static void HandleConsumed<TK, TE>(
			string providerDeploymentId,
			TK[] objectNames,
			EPObjectType objectType,
			PathRegistry<TK, TE> registry,
			string moduleName,
			string selfDeploymentId,
			IList<EPDeploymentDependencyConsumed.Item> consumed,
			Func<TK, string> objectNameFunction)
			where TK : class
		{
			foreach (TK objectName in objectNames) {
				try {
					var ids = registry.GetDependencies(objectName, moduleName);
					if (ids != null && ids.Contains(selfDeploymentId)) {
						consumed.Add(
							new EPDeploymentDependencyConsumed.Item(
								providerDeploymentId,
								objectType,
								objectNameFunction.Invoke(objectName)));
					}
				}
				catch (ArgumentException) {
					// not handled
				}
			}
		}

		private static void HandleProvided<TK, TE>(
			TK[] objectNames,
			EPObjectType objectType,
			PathRegistry<TK, TE> registry,
			string moduleName,
			IList<EPDeploymentDependencyProvided.Item> dependencies,
			Func<TK, string> objectNameFunction)
			where TK : class
		{
			foreach (TK objectName in objectNames) {
				try {
					var ids = registry.GetDependencies(objectName, moduleName);
					if (ids != null) {
						dependencies.Add(
							new EPDeploymentDependencyProvided.Item(
								objectType,
								objectNameFunction.Invoke(objectName),
								new HashSet<string>(ids)));
					}
				}
				catch (ArgumentException) {
					// no need to handle
				}
			}
		}
	}
} // end of namespace
