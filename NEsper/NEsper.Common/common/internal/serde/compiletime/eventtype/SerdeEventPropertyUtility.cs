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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.additional;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // constant;
using static com.espertech.esper.common.@internal.context.aifactory.createtable.StmtForgeMethodCreateTable; // INTERNAL_RESERVED_PROPERTY;
using static com.espertech.esper.common.@internal.@event.core.EventTypeUtility; // resolveTypeCodegenGivenResolver;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
	public class SerdeEventPropertyUtility {
	    public static SerdeEventPropertyDesc ForgeForEventProperty(EventType eventTypeSerde, string propertyName, object propertyType, StatementRawInfo raw, SerdeCompileTimeResolver resolver) {

	        DataInputOutputSerdeForge forge;
	        if (propertyType == null) {
	            return new SerdeEventPropertyDesc(new DataInputOutputSerdeForgeSingleton(typeof(DIOSkipSerde)), Collections.EmptySet());
	        }
	        if (propertyType is Type) {

	            // handle special Json catch-all types
	            if (eventTypeSerde is JsonEventType) {
	                forge = null;
	                if (propertyType == typeof(IDictionary)) {
	                    forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonObjectSerde));
	                } else if (propertyType == typeof(object[])) {
	                    forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonArraySerde));
	                } else if (propertyType == typeof(object)) {
	                    forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonAnyValueSerde));
	                }
	                if (forge != null) {
	                    return new SerdeEventPropertyDesc(forge, Collections.EmptySet());
	                }
	            }

	            // handle all Class-type properties
	            var typedProperty = (Type) propertyType;
	            if (typedProperty == typeof(object) && propertyName.Equals(INTERNAL_RESERVED_PROPERTY)) {
	                forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOSkipSerde)); // for expression data window or others that include transient references in the field
	            } else {
	                forge = resolver.SerdeForEventProperty(typedProperty, eventTypeSerde.Name, propertyName, raw);
	            }
	            return new SerdeEventPropertyDesc(forge, Collections.EmptySet());
	        }

	        if (propertyType is EventType) {
	            var eventType = (EventType) propertyType;
	            Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
	                ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
	            forge = new DataInputOutputSerdeForgeEventSerde("nullableEvent", func);
	            return new SerdeEventPropertyDesc(forge, Collections.Singleton(eventType));
	        } else if (propertyType is EventType[]) {
	            var eventType = ((EventType[]) propertyType)[0];
	            Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
	                ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
	            forge = new DataInputOutputSerdeForgeEventSerde("nullableEventArray", func);
	            return new SerdeEventPropertyDesc(forge, Collections.Singleton(eventType));
	        } else if (propertyType is TypeBeanOrUnderlying) {
	            var eventType = ((TypeBeanOrUnderlying) propertyType).EventType;
	            Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
	                ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
	            forge = new DataInputOutputSerdeForgeEventSerde("nullableEventOrUnderlying", func);
	            return new SerdeEventPropertyDesc(forge, Collections.Singleton(eventType));
	        } else if (propertyType is TypeBeanOrUnderlying[]) {
	            var eventType = ((TypeBeanOrUnderlying[]) propertyType)[0].EventType;
	            Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
	                ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
	            forge = new DataInputOutputSerdeForgeEventSerde("nullableEventArrayOrUnderlying", func);
	            return new SerdeEventPropertyDesc(forge, Collections.Singleton(eventType));
	        } else if (propertyType is IDictionary) {
	            var kv = (IDictionary<string, object>) propertyType;
	            var keys = new string[kv.Count];
	            var serdes = new DataInputOutputSerdeForge[kv.Count];
	            var index = 0;
	            var nestedTypes = new LinkedHashSet<EventType>();
	            foreach (KeyValuePair<string, object> entry in kv.EntrySet()) {
	                keys[index] = entry.Key;
	                if (entry.Value is string) {
	                    var value = entry.Value.ToString().Trim();
	                    Type clazz = TypeHelper.GetPrimitiveClassForName(value);
	                    if (clazz != null) {
	                        entry.Value = clazz;
	                    }
	                }
	                var desc = ForgeForEventProperty(eventTypeSerde, entry.Key, entry.Value, raw, resolver);
	                nestedTypes.AddAll(desc.NestedTypes);
	                serdes[index] = desc.Forge;
	                index++;
	            }
	            var functions = new Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[2];
	            functions[0] = vars => Constant(keys);
	            functions[1] = vars => DataInputOutputSerdeForge.CodegenArray(serdes, vars.Method, vars.Scope, vars.OptionalEventTypeResolver);
	            forge = new DataInputOutputSerdeForgeParameterized(typeof(DIOMapPropertySerde).Name, functions);
	            return new SerdeEventPropertyDesc(forge, nestedTypes);
	        } else {
	            throw new EPException("Failed to determine serde for unrecognized property value type '" + propertyType + "' for property '" + propertyName + "' of type '" + eventTypeSerde.Name + "'");
	        }
	    }
	}
} // end of namespace
