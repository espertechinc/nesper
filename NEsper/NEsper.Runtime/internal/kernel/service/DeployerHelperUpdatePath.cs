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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class DeployerHelperUpdatePath
	{
		public static DeployerModulePaths UpdatePath(
			int rolloutItemNumber,
			DeployerModuleEPLObjects eplObjects,
			string moduleName,
			string deploymentId,
			EPServicesContext services)
		{
			// save path-visibility event types and named windows to the path
			var deploymentIdCrc32 = CRC32Util.ComputeCRC32(deploymentId);
			IDictionary<long, EventType> deploymentTypes = EmptyDictionary<long, EventType>.Instance;
			IList<string> pathEventTypes = new List<string>(eplObjects.ModuleEventTypes.Count);
			IList<string> pathNamedWindows = new List<string>(eplObjects.ModuleNamedWindows.Count);
			IList<string> pathTables = new List<string>(eplObjects.ModuleTables.Count);
			IList<string> pathContexts = new List<string>(eplObjects.ModuleContexts.Count);
			IList<string> pathVariables = new List<string>(eplObjects.ModuleVariables.Count);
			IList<string> pathExprDecl = new List<string>(eplObjects.ModuleExpressions.Count);
			IList<NameAndParamNum> pathScripts = new List<NameAndParamNum>(eplObjects.ModuleScripts.Count);
			IList<string> pathClasses = new List<string>(eplObjects.ModuleClasses.Count);

			try {
				foreach (var entry in eplObjects.ModuleNamedWindows) {
					if (entry.Value.EventType.Metadata.AccessModifier.IsNonPrivateNonTransient()) {
						try {
							services.NamedWindowPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathNamedWindows.Add(entry.Key);
					}
				}

				foreach (var entry in eplObjects.ModuleTables) {
					if (entry.Value.TableVisibility.IsNonPrivateNonTransient()) {
						try {
							services.TablePathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathTables.Add(entry.Key);
					}
				}

				foreach (var entry in eplObjects.ModuleEventTypes) {
					var eventTypeSPI = (EventTypeSPI) entry.Value;
					var nameTypeId = CRC32Util.ComputeCRC32(eventTypeSPI.Name);
					var eventTypeMetadata = entry.Value.Metadata;
					if (eventTypeMetadata.AccessModifier == NameAccessModifier.PRECONFIGURED) {
						// For XML all fragment event types are public
						if (eventTypeMetadata.ApplicationType != EventTypeApplicationType.XML) {
							throw new IllegalStateException("Unrecognized public visibility type in deployment");
						}
					}
					else if (eventTypeMetadata.AccessModifier.IsNonPrivateNonTransient()) {
						if (eventTypeMetadata.BusModifier == EventTypeBusModifier.BUS) {
							eventTypeSPI.SetMetadataId(nameTypeId, -1);
							services.EventTypeRepositoryBus.AddType(eventTypeSPI);
						}
						else {
							eventTypeSPI.SetMetadataId(deploymentIdCrc32, nameTypeId);
						}

						try {
							services.EventTypePathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}
					}
					else {
						eventTypeSPI.SetMetadataId(deploymentIdCrc32, nameTypeId);
					}

					if (eventTypeMetadata.AccessModifier.IsNonPrivateNonTransient()) {
						pathEventTypes.Add(entry.Key);
					}

					// we retain all types to enable variant-streams
					if (deploymentTypes.IsEmpty()) {
						deploymentTypes = new Dictionary<long, EventType>();
					}

					deploymentTypes.Put(nameTypeId, eventTypeSPI);
				}

				// add serde information to event types
				services.EventTypeSerdeRepository.AddSerdes(
					deploymentId,
					eplObjects.EventTypeSerdes,
					eplObjects.ModuleEventTypes,
					eplObjects.BeanEventTypeFactory);

				foreach (var entry in eplObjects.ModuleContexts) {
					if (entry.Value.ContextVisibility.IsNonPrivateNonTransient()) {
						try {
							services.ContextPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathContexts.Add(entry.Key);
					}
				}

				foreach (var entry in eplObjects.ModuleVariables) {
					if (entry.Value.VariableVisibility.IsNonPrivateNonTransient()) {
						try {
							services.VariablePathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathVariables.Add(entry.Key);
					}
				}

				foreach (var entry in eplObjects.ModuleExpressions) {
					if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
						try {
							services.ExprDeclaredPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathExprDecl.Add(entry.Key);
					}
				}

				foreach (var entry in eplObjects.ModuleScripts) {
					if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
						try {
							services.ScriptPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathScripts.Add(entry.Key);
					}
				}

				foreach (var index in eplObjects.ModuleIndexes) {
					if (index.IsNamedWindow) {
						var namedWindow = services.NamedWindowPathRegistry.GetWithModule(index.InfraName, index.InfraModuleName);
						if (namedWindow == null) {
							throw new IllegalStateException("Failed to find named window '" + index.InfraName + "'");
						}

						ValidateIndexPrecondition(rolloutItemNumber, namedWindow.IndexMetadata, index);
					}
					else {
						var table = services.TablePathRegistry.GetWithModule(index.InfraName, index.InfraModuleName);
						if (table == null) {
							throw new IllegalStateException("Failed to find table '" + index.InfraName + "'");
						}

						ValidateIndexPrecondition(rolloutItemNumber, table.IndexMetadata, index);
					}
				}

				foreach (var entry in eplObjects.ModuleClasses) {
					if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
						try {
							services.ClassProvidedPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
						}
						catch (PathExceptionAlreadyRegistered ex) {
							throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
						}

						pathClasses.Add(entry.Key);
					}
				}
			}
			catch (Exception) {
				Undeployer.DeleteFromEventTypeBus(services, deploymentTypes);
				Undeployer.DeleteFromPathRegistries(services, deploymentId);
				throw;
			}

			return new DeployerModulePaths(
				deploymentTypes,
				pathEventTypes,
				pathNamedWindows,
				pathTables,
				pathContexts,
				pathVariables,
				pathExprDecl,
				pathScripts,
				pathClasses);
		}

		private static void ValidateIndexPrecondition(
			int rolloutItemNumber,
			EventTableIndexMetadata indexMetadata,
			ModuleIndexMeta index)
		{
			if (indexMetadata.GetIndexByName(index.IndexName) != null) {
				var ex = new PathExceptionAlreadyRegistered(index.IndexName, PathRegistryObjectType.INDEX, index.IndexModuleName);
				throw new EPDeployPreconditionException(ex.Message, ex, rolloutItemNumber);
			}
		}
	}
} // end of namespace
