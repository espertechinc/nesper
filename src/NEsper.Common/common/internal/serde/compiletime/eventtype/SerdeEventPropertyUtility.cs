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
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

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
            if (propertyType == null) {
                return new SerdeEventPropertyDesc(DataInputOutputSerdeForgeSkip.INSTANCE, EmptySet<EventType>.Instance);
            }

            if (propertyType is Type epType) {
                // handle special Json catch-all types
                if (eventTypeSerde is JsonEventType) {
                    forge = null;
                    if (epType.IsGenericDictionary()) {
                        forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonObjectSerde));
                    }
                    else if (epType == typeof(object[])) {
                        forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonArraySerde));
                    }
                    else if (epType == typeof(object)) {
                        forge = new DataInputOutputSerdeForgeSingleton(typeof(DIOJsonAnyValueSerde));
                    }

                    if (forge != null) {
                        return new SerdeEventPropertyDesc(forge, EmptySet<EventType>.Instance);
                    }
                }

                // handle all Class-type properties
                if (epType == typeof(object) && propertyName.Equals(INTERNAL_RESERVED_PROPERTY)) {
                    forge = new DataInputOutputSerdeForgeSingleton(
                        typeof(DIOSkipSerde)); // for expression data window or others that include transient references in the field
                }
                else {
                    forge = resolver.SerdeForEventProperty(epType, eventTypeSerde.Name, propertyName, raw);
                }

                return new SerdeEventPropertyDesc(forge, EmptySet<EventType>.Instance);
            }

            if (propertyType is EventType p0) {
                Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
                    ResolveTypeCodegenGivenResolver(p0, vars.OptionalEventTypeResolver);
                forge = new DataInputOutputSerdeForgeEventSerde(
                    DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENT,
                    p0,
                    func);
                return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(p0));
            }
            else if (propertyType is EventType[] types) {
                var eventType = types[0];
                Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
                    ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
                forge = new DataInputOutputSerdeForgeEventSerde(
                    DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENTARRAY,
                    eventType,
                    func);
                return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
            }
            else if (propertyType is TypeBeanOrUnderlying) {
                var eventType = ((TypeBeanOrUnderlying)propertyType).EventType;
                Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
                    ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
                forge = new DataInputOutputSerdeForgeEventSerde(
                    DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENTORUNDERLYING,
                    eventType,
                    func);
                return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
            }
            else if (propertyType is TypeBeanOrUnderlying[]) {
                var eventType = ((TypeBeanOrUnderlying[])propertyType)[0].EventType;
                Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression> func = vars =>
                    ResolveTypeCodegenGivenResolver(eventType, vars.OptionalEventTypeResolver);
                forge = new DataInputOutputSerdeForgeEventSerde(
                    DataInputOutputSerdeForgeEventSerdeMethod.NULLABLEEVENTARRAYORUNDERLYING,
                    eventType,
                    func);
                return new SerdeEventPropertyDesc(forge, Collections.SingletonSet(eventType));
            }
            else if (propertyType is IDictionary<string, object>) {
                var kv = (IDictionary<string, object>)propertyType;
                var keys = new string[kv.Count];
                var serdes = new DataInputOutputSerdeForge[kv.Count];
                var index = 0;
                var nestedTypes = new LinkedHashSet<EventType>();
                var postActions = new List<Action>();
                
                foreach (var entry in kv) {
                    keys[index] = entry.Key;
                    if (entry.Value is string stringValue) {
                        var value = stringValue.Trim();
                        var clazz = TypeHelper.GetPrimitiveTypeForName(value);
                        if (clazz != null) {
                            var key = entry.Key;
                            postActions.Add(() => kv[key] = clazz);
                        }
                    }

                    var desc = ForgeForEventProperty(eventTypeSerde, entry.Key, entry.Value, raw, resolver);
                    nestedTypes.AddAll(desc.NestedTypes);
                    serdes[index] = desc.Forge;
                    index++;
                }

                postActions.For(_ => _.Invoke());
                
                forge = new DataInputOutputSerdeForgeMap(keys, serdes);
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