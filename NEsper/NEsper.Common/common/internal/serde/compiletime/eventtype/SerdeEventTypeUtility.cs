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
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationIDGenerator;
using static com.espertech.esper.common.@internal.serde.compiletime.eventtype.SerdeEventPropertyUtility;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
	public class SerdeEventTypeUtility
	{
		public static IList<StmtClassForgeableFactory> Plan(
			EventType eventType,
			StatementRawInfo raw,
			SerdeEventTypeCompileTimeRegistry registry,
			SerdeCompileTimeResolver resolver)
		{
			// there is no need to register a serde when not using HA, or when it is already registered, or for table-internal type
			var typeClass = eventType.Metadata.TypeClass;
			if (!registry.IsTargetHA || registry.EventTypes.ContainsKey(eventType) || typeClass == EventTypeTypeClass.TABLE_INTERNAL) {
				return EmptyList<StmtClassForgeableFactory>.Instance;
			}

			// there is also no need to register a serde when using a public object
			var statementType = raw.StatementType;
			if ((typeClass == EventTypeTypeClass.NAMED_WINDOW && statementType != StatementType.CREATE_WINDOW) ||
			    (typeClass == EventTypeTypeClass.TABLE_PUBLIC && statementType != StatementType.CREATE_TABLE)) {
				return EmptyList<StmtClassForgeableFactory>.Instance;
			}

			IList<StmtClassForgeableFactory> forgeables = new List<StmtClassForgeableFactory>(2);
			PlanRecursive(forgeables, eventType, raw, registry, resolver);
			return forgeables;
		}

		private static void PlanRecursive(
			IList<StmtClassForgeableFactory> additionalForgeables,
			EventType eventType,
			StatementRawInfo raw,
			SerdeEventTypeCompileTimeRegistry registry,
			SerdeCompileTimeResolver resolver)
		{
			if (!registry.IsTargetHA) {
				return;
			}

			if (registry.EventTypes.ContainsKey(eventType)) {
				return;
			}

			SerdeAndForgeables pair;
			if (eventType is BeanEventType) {
				pair = PlanBean((BeanEventType) eventType, raw, resolver);
			}
			else if (eventType is BaseNestableEventType) {
				pair = PlanBaseNestable((BaseNestableEventType) eventType, raw, resolver);
				PlanPropertiesMayRecurse(eventType, additionalForgeables, raw, registry, resolver);
			}
			else if (eventType is WrapperEventType) {
				var wrapperEventType = (WrapperEventType) eventType;
				PlanRecursive(additionalForgeables, wrapperEventType.UnderlyingEventType, raw, registry, resolver);
				pair = PlanBaseNestable(wrapperEventType.UnderlyingMapType, raw, resolver);
			}
			else if (eventType is VariantEventType || eventType is AvroSchemaEventType || eventType is BaseXMLEventType) {
				// no serde generation
				pair = null;
			}
			else {
				throw new UnsupportedOperationException("Event type not yet handled: " + eventType);
			}

			if (pair != null) {
				registry.AddSerdeFor(eventType, pair.Forge);
				additionalForgeables.AddAll(pair.AdditionalForgeables);
			}
		}

		private static void PlanPropertiesMayRecurse(
			EventType eventType,
			IList<StmtClassForgeableFactory> additionalForgeables,
			StatementRawInfo raw,
			SerdeEventTypeCompileTimeRegistry registry,
			SerdeCompileTimeResolver resolver)
		{
			foreach (var desc in eventType.PropertyDescriptors) {
				if (!desc.IsFragment) {
					continue;
				}

				var fragmentEventType = eventType.GetFragmentType(desc.PropertyName);
				if (fragmentEventType == null || registry.EventTypes.ContainsKey(fragmentEventType.FragmentType)) {
					continue;
				}

				PlanRecursive(additionalForgeables, fragmentEventType.FragmentType, raw, registry, resolver);
			}
		}

		private static SerdeAndForgeables PlanBaseNestable(
			BaseNestableEventType eventType,
			StatementRawInfo raw,
			SerdeCompileTimeResolver resolver)
		{
			string className;
			if (eventType is JsonEventType) {
				var classNameFull = ((JsonEventType) eventType).Detail.SerdeClassName;
				var lastDotIndex = classNameFull.LastIndexOf('.');
				className = lastDotIndex == -1 ? classNameFull : classNameFull.Substring(lastDotIndex + 1);

			}
			else {
				var uuid = GenerateClassNameUUID();
				className = GenerateClassNameWithUUID(typeof(DataInputOutputSerde), eventType.Metadata.Name, uuid);
			}

			var optionalApplicationSerde = resolver.SerdeForEventTypeExternalProvider(eventType, raw);
			if (optionalApplicationSerde != null) {
				return new SerdeAndForgeables(optionalApplicationSerde, EmptyList<StmtClassForgeableFactory>.Instance);
			}

			var forges = new DataInputOutputSerdeForge[eventType.Types.Count];
			var count = 0;
			foreach (var property in eventType.Types) {
				var desc = ForgeForEventProperty(eventType, property.Key, property.Value, raw, resolver);
				forges[count] = desc.Forge;
				count++;
			}

			StmtClassForgeableFactory forgeable = new ProxyStmtClassForgeableFactory() {
				ProcMake = (
					namespaceScope,
					classPostfix) => {
					return new StmtClassForgeableBaseNestableEventTypeSerde(className, namespaceScope, eventType, forges);
				},
			};

			var forge = new DataInputOutputSerdeForgeForClassName(className);

			return new SerdeAndForgeables(forge, Collections.SingletonList(forgeable));
		}

		private static SerdeAndForgeables PlanBean(
			BeanEventType eventType,
			StatementRawInfo raw,
			SerdeCompileTimeResolver resolver)
		{
			var forge = resolver.SerdeForBeanEventType(raw, eventType.UnderlyingType, eventType.Name, eventType.SuperTypes);
			return new SerdeAndForgeables(forge, EmptyList<StmtClassForgeableFactory>.Instance);
		}

		private class SerdeAndForgeables
		{
			private readonly DataInputOutputSerdeForge forge;
			private readonly IList<StmtClassForgeableFactory> additionalForgeables;

			public SerdeAndForgeables(
				DataInputOutputSerdeForge forge,
				IList<StmtClassForgeableFactory> additionalForgeables)
			{
				this.forge = forge;
				this.additionalForgeables = additionalForgeables;
			}

			public DataInputOutputSerdeForge Forge => forge;

			public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;
		}
	}
} // end of namespace
