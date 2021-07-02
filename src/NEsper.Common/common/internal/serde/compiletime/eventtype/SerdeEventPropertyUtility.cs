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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.additional;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.aifactory.createtable.StmtForgeMethodCreateTable;
using static com.espertech.esper.common.@internal.@event.core.EventTypeUtility;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
	public class SerdeEventPropertyUtility
	{
		public static SerdeEventPropertyDesc ForgeForEventProperty(
			EventType eventTypeSerde,
			string propertyName,
			object propertyType,
			StatementRawInfo raw,
			SerdeCompileTimeResolver resolver)
		{
			DataInputOutputSerdeForge forge;
			if (propertyType.IsNullType()) {
				return new SerdeEventPropertyDesc(new DataInputOutputSerdeForgeSingleton(typeof(DIOSkipSerde)), EmptySet<EventType>.Instance);
			}

			if (propertyType is Type propertyTypeType) {
				// handle special Json catch-all types
				if (eventTypeSerde is JsonEventType) {
					forge = null;
					if (propertyTypeType == typeof(IDictionary<string, object>)) {
						forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonObjectSerde));
					}
					else if (propertyTypeType == typeof(object[])) {
						forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonArraySerde));
					}
					else if (propertyTypeType == typeof(object)) {
						forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonAnyValueSerde));
					}

					if (forge != null) {
						return new SerdeEventPropertyDesc(forge, EmptySet<EventType>.Instance);
					}
				}

				// handle all Class-type properties
				var typedProperty = (Type) propertyType;
				if (typedProperty == typeof(object) && propertyName.Equals(INTERNAL_RESERVED_PROPERTY)) {
					forge = new DataInputOutputSerdeForgeSingleton(
						typeof(DIOSkipSerde)); // for expression data window or others that include transient references in the field
				}
				else {
					forge = resolver.SerdeForEventProperty(typedProperty, eventTypeSerde.Name, propertyName, raw);
				}

				return new SerdeEventPropertyDesc(forge, EmptySet<EventType>.Instance);
			}

			if (propertyType is EventType) {
				var eventType = (EventType) propertyType;
				Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
					ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
				forge = new DataInputOutputSerdeForgeEventSerde("NullableEvent", func);
				return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
			}
			else if (propertyType is EventType[]) {
				var eventType = ((EventType[]) propertyType)[0];
				Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
					ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
				forge = new DataInputOutputSerdeForgeEventSerde("NullableEventArray", func);
				return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
			}
			else if (propertyType is TypeBeanOrUnderlying) {
				var eventType = ((TypeBeanOrUnderlying) propertyType).EventType;
				Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
					ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
				forge = new DataInputOutputSerdeForgeEventSerde("NullableEventOrUnderlying", func);
				return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
			}
			else if (propertyType is TypeBeanOrUnderlying[]) {
				var eventType = ((TypeBeanOrUnderlying[]) propertyType)[0].EventType;
				Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
					ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
				forge = new DataInputOutputSerdeForgeEventSerde("NullableEventArrayOrUnderlying", func);
				return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
			}
			else if (propertyType is IDictionary<string, object> keyValueProperties) {
				var keys = new string[keyValueProperties.Count];
				var serdes = new DataInputOutputSerdeForge[keyValueProperties.Count];
				var index = 0;
				var nestedTypes = new LinkedHashSet<EventType>();

				// Rewrite all properties where the value is a string.  First, gather all instances that need
				// to be rewritten into the class that matches the type.
				keyValueProperties
					.Where(entry => entry.Value is string)
					.ToList()
					.ForEach(
						entry => {
							var value = entry.Value.ToString()?.Trim();
							var clazz = TypeHelper.GetPrimitiveTypeForName(value);
							if (clazz != null) {
								keyValueProperties[entry.Key] = clazz;
							}
						});
				
				foreach (var entry in keyValueProperties) {
					keys[index] = entry.Key;
					var desc = ForgeForEventProperty(eventTypeSerde, entry.Key, entry.Value, raw, resolver);
					nestedTypes.AddAll(desc.NestedTypes);
					serdes[index] = desc.Forge;
					index++;
				}

				var functions = new Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[2];
				functions[0] = vars => Constant(keys);
				functions[1] = vars => DataInputOutputSerdeForgeExtensions.CodegenArray(serdes, vars.Method, vars.Scope, vars.OptionalEventTypeResolver);
				forge = new DataInputOutputSerdeForgeParameterized(typeof(DIOMapPropertySerde).Name, functions);
				return new SerdeEventPropertyDesc(forge, nestedTypes);
			}
			else {
				throw new EPException(
					"Failed to determine serde for unrecognized property value type '" +
					propertyType +
					"' for property '" +
					propertyName +
					"' of type '" +
					eventTypeSerde.Name +
					"'");
			}
		}
	}
} // end of namespace
