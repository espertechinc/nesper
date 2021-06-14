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
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.multikey.MultiKeyPlanner;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class SerdeCompileTimeResolverImpl : SerdeCompileTimeResolver
	{
		private readonly bool _allowExtended;
		private readonly bool allowSerializable;
		private readonly bool allowSerializationFallback;
		private readonly ICollection<SerdeProvider> serdeProviders;

		public SerdeCompileTimeResolverImpl(
			IList<SerdeProvider> serdeProviders,
			bool allowExtended,
			bool allowSerializable,
			bool allowSerializationFallback)
		{
			this.serdeProviders = new CopyOnWriteList<SerdeProvider>(serdeProviders);
			this._allowExtended = allowExtended;
			this.allowSerializable = allowSerializable;
			this.allowSerializationFallback = allowSerializationFallback;
		}

		public bool IsTargetHA {
			get { return true; }
		}

		public DataInputOutputSerdeForge SerdeForFilter(
			Type evaluationType,
			StatementRawInfo raw)
		{
			return SerdeMayArray(evaluationType, new SerdeProviderAdditionalInfoFilter(raw));
		}

		public DataInputOutputSerdeForge SerdeForKeyNonArray(
			Type paramType,
			StatementRawInfo raw)
		{
			return SerdeForClass(paramType, new SerdeProviderAdditionalInfoMultikey(raw));
		}

		public DataInputOutputSerdeForge[] SerdeForMultiKey(
			Type[] types,
			StatementRawInfo raw)
		{
			return SerdeForClasses(types, new SerdeProviderAdditionalInfoMultikey(raw));
		}

		public DataInputOutputSerdeForge[] SerdeForDataWindowSortCriteria(
			Type[] sortCriteriaExpressions,
			StatementRawInfo raw)
		{
			return SerdeForClasses(sortCriteriaExpressions, new SerdeProviderAdditionalInfoMultikey(raw));
		}

		public DataInputOutputSerdeForge SerdeForDerivedViewAddProp(
			Type evalType,
			StatementRawInfo raw)
		{
			return SerdeForClass(evalType, new SerdeProviderAdditionalInfoDerivedViewProperty(raw));
		}

		public DataInputOutputSerdeForge SerdeForIndexHashNonArray(
			Type propType,
			StatementRawInfo raw)
		{
			return SerdeForClass(propType, new SerdeProviderAdditionalInfoIndex(raw));
		}

		public DataInputOutputSerdeForge SerdeForBeanEventType(
			StatementRawInfo raw,
			Type underlyingType,
			string eventTypeName,
			IList<EventType> eventTypeSupertypes)
		{
			return SerdeForClass(underlyingType, new SerdeProviderAdditionalInfoEventType(raw, eventTypeName, eventTypeSupertypes));
		}

		public DataInputOutputSerdeForge SerdeForEventProperty(
			Type typedProperty,
			string eventTypeName,
			string propertyName,
			StatementRawInfo raw)
		{
			return SerdeForClass(typedProperty, new SerdeProviderAdditionalInfoEventProperty(raw, eventTypeName, propertyName));
		}

		public DataInputOutputSerdeForge SerdeForAggregation(
			Type type,
			StatementRawInfo raw)
		{
			return SerdeForClass(type, new SerdeProviderAdditionalInfoAggregation(raw));
		}

		public DataInputOutputSerdeForge SerdeForIndexBtree(
			Type rangeType,
			StatementRawInfo raw)
		{
			return SerdeForClass(rangeType, new SerdeProviderAdditionalInfoIndex(raw));
		}

		public DataInputOutputSerdeForge SerdeForAggregationDistinct(
			Type type,
			StatementRawInfo raw)
		{
			return SerdeMayArray(type, new SerdeProviderAdditionalInfoAggregationDistinct(raw));
		}

		public DataInputOutputSerdeForge SerdeForVariable(
			Type type,
			string variableName,
			StatementRawInfo raw)
		{
			return SerdeForClass(type, new SerdeProviderAdditionalInfoVariable(raw, variableName));
		}

		public DataInputOutputSerdeForge SerdeForEventTypeExternalProvider(
			BaseNestableEventType eventType,
			StatementRawInfo raw)
		{
			if (serdeProviders.IsEmpty()) {
				return null;
			}

			SerdeProviderEventTypeContext context = new SerdeProviderEventTypeContext(raw, eventType);
			foreach (SerdeProvider provider in serdeProviders) {
				try {
					SerdeProvision serde = provider.ResolveSerdeForEventType(context);
					if (serde != null) {
						return serde.ToForge();
					}
				}
				catch (DataInputOutputSerdeException) {
					throw;
				}
				catch (EPException) {
					throw;
				}
				catch (Exception ex) {
					throw new DataInputOutputSerdeException(
						"Unexpected exception invoking serde provider '" +
						provider.GetType().Name +
						"' passing for event type '" +
						eventType.Name +
						"': " +
						ex.Message,
						ex);
				}
			}

			return null;
		}

		private DataInputOutputSerdeForge SerdeMayArray(
			Type type,
			SerdeProviderAdditionalInfo info)
		{
			if (type.IsArray) {
				DataInputOutputSerde mkSerde = GetMKSerdeClassForComponentType(type.GetElementType());
				return new DataInputOutputSerdeForgeSingleton(mkSerde.GetType());
			}

			return SerdeForClass(type, info);
		}

		private DataInputOutputSerdeForge[] SerdeForClasses(
			Type[] sortCriteriaExpressions,
			SerdeProviderAdditionalInfo additionalInfo)
		{
			DataInputOutputSerdeForge[] forges = new DataInputOutputSerdeForge[sortCriteriaExpressions.Length];
			for (int i = 0; i < sortCriteriaExpressions.Length; i++) {
				forges[i] = SerdeForClass(sortCriteriaExpressions[i], additionalInfo);
			}

			return forges;
		}

		private DataInputOutputSerdeForge SerdeForClass(
			Type type,
			SerdeProviderAdditionalInfo additionalInfo)
		{
			if (IsBasicBuiltin(type)) {
				DataInputOutputSerde serde = VMBasicBuiltinSerdeFactory.GetSerde(type);
				if (serde == null) {
					throw new DataInputOutputSerdeException("Failed to find built-in serde for class " + type.Name);
				}

				return new DataInputOutputSerdeForgeSingleton(serde.GetType());
			}

			if (_allowExtended) {
				DataInputOutputSerde serde = VMExtendedBuiltinSerdeFactory.GetSerde(type);
				if (serde != null) {
					return new DataInputOutputSerdeForgeSingleton(serde.GetType());
				}
			}

			if (type == typeof(EPLMethodInvocationContext)) {
				return new DataInputOutputSerdeForgeSingleton(typeof(DIOSkipSerde));
			}

			SerdeProvision provision = SerdeCompileTimeResolverUtil.DetermineSerde(
				type,
				serdeProviders,
				allowSerializable,
				allowSerializationFallback,
				additionalInfo);
			return provision.ToForge();
		}

		private bool IsBasicBuiltin(Type type)
		{
			return TypeHelper.IsBuiltinDataType(type) && type.IsBigInteger();
		}
	}
} // end of namespace
