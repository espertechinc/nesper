///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;
using com.espertech.esper.common.@internal.@event.json.parser.forge;
using com.espertech.esper.common.@internal.@event.json.write;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


using static com.espertech.esper.common.@internal.bytecodemodel.util.IdentifierUtil; // getIdentifierMayStartNumeric
using static com.espertech.esper.common.@internal.@event.core.BaseNestableEventUtil; // resolvePropertyTypes

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class JsonEventTypeUtility {
	    public static JsonEventType MakeJsonTypeCompileTimeExistingType(EventTypeMetadata metadata, JsonEventType existingType, StatementCompileTimeServices services) {
	        EventTypeNestableGetterFactoryJson getterFactoryJson = new EventTypeNestableGetterFactoryJson(existingType.Detail);
	        return new JsonEventType(metadata, existingType.Types,
	            null, Collections.EmptySet(), existingType.StartTimestampPropertyName, existingType.EndTimestampPropertyName,
	            getterFactoryJson, services.BeanEventTypeFactoryPrivate, existingType.Detail, existingType.UnderlyingType);
	    }

	    public static EventTypeForgablesPair MakeJsonTypeCompileTimeNewType(EventTypeMetadata metadata, IDictionary<string, object> compiledTyping, Pair<EventType[], ISet<EventType>> superTypes, ConfigurationCommonEventTypeWithSupertype config, StatementRawInfo raw, StatementCompileTimeServices services) {
	        if (metadata.ApplicationType != EventTypeApplicationType.JSON) {
	            throw new IllegalStateException("Expected Json application type");
	        }

	        // determine supertype
	        JsonEventType optionalSuperType = (JsonEventType) (superTypes == null ? null : (superTypes.First == null || superTypes.First.Length == 0 ? null : superTypes.First[0]));
	        int numFieldsSuperType = optionalSuperType == null ? 0 : optionalSuperType.Detail.FieldDescriptors.Count;

	        // determine dynamic
	        JsonSchema jsonSchema = (JsonSchema) AnnotationUtil.FindAnnotation(raw.Annotations, typeof(JsonSchema));
	        bool dynamic = DetermineDynamic(jsonSchema, optionalSuperType, raw);

	        // determine json underlying type class
	        Type optionalUnderlyingProvided = DetermineUnderlyingProvided(jsonSchema, services);

	        // determine properties
	        IDictionary<string, object> properties;
	        IDictionary<string, string> fieldNames;
	        IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses;
	        IDictionary<string, Field> fields;
	        if (optionalUnderlyingProvided == null) {
	            properties = ResolvePropertyTypes(compiledTyping, services.EventTypeCompileTimeResolver);
	            properties = RemoveEventBeanTypes(properties);
	            fieldNames = ComputeFieldNames(properties);
	            deepClasses = JsonEventTypeUtilityReflective.ComputeClassesDeep(properties, metadata.Name, raw.Annotations, services);
	            fields = Collections.EmptyMap();
	        } else {
	            if (dynamic) {
	                throw new ExprValidationException("The dynamic flag is not supported when used with a provided JSON event class");
	            }
	            if (optionalSuperType != null) {
	                throw new ExprValidationException("Specifying a supertype is not supported with a provided JSON event class");
	            }
	            if (!Modifier.IsPublic(optionalUnderlyingProvided.Modifiers)) {
	                throw new ExprValidationException("Provided JSON event class is not public");
	            }
	            if (!ConstructorHelper.HasDefaultConstructor(optionalUnderlyingProvided)) {
	                throw new ExprValidationException("Provided JSON event class does not have a public default constructor or is a non-static inner class");
	            }
	            deepClasses = JsonEventTypeUtilityReflective.ComputeClassesDeep(optionalUnderlyingProvided, metadata.Name, raw.Annotations, services);
	            fields = new LinkedHashMap<>();
	            deepClasses.Get(optionalUnderlyingProvided).Fields.ForEach(field -> fields.Put(field.Name, field));
	            properties = ResolvePropertiesFromFields(fields);
	            fieldNames = ComputeFieldNamesFromProperties(properties);
	            compiledTyping = ResolvePropertyTypes(compiledTyping, services.EventTypeCompileTimeResolver);
	            ValidateFieldTypes(optionalUnderlyingProvided, fields, compiledTyping);

	            // use the rich-type definition for properties that may come from events
	            foreach (KeyValuePair<string, object> compiledTypingEntry in compiledTyping.EntrySet()) {
	                if (compiledTypingEntry.Value is TypeBeanOrUnderlying || compiledTypingEntry.Value is TypeBeanOrUnderlying[]) {
	                    properties.Put(compiledTypingEntry.Key, compiledTypingEntry.Value);
	                }
	            }
	        }

	        IDictionary<string, JsonUnderlyingField> fieldDescriptors = ComputeFields(properties, fieldNames, optionalSuperType, fields);
	        IDictionary<string, JsonForgeDesc> forges = ComputeValueForges(properties, fields, deepClasses, raw.Annotations, services);

	        string jsonClassNameSimple;
	        if (optionalUnderlyingProvided != null) {
	            jsonClassNameSimple = optionalUnderlyingProvided.SimpleName;
	        } else {
	            jsonClassNameSimple = metadata.Name;
	            if (metadata.AccessModifier.IsPrivateOrTransient) {
	                string uuid = CodeGenerationIDGenerator.GenerateClassNameUUID();
	                jsonClassNameSimple = jsonClassNameSimple + "__" + uuid;
	            } else if (raw.ModuleName != null) {
	                jsonClassNameSimple = jsonClassNameSimple + "__" + "module" + "_" + raw.ModuleName;
	            }
	        }

	        StmtClassForgeableJsonDesc forgeableDesc = new StmtClassForgeableJsonDesc(properties, fieldDescriptors, dynamic, numFieldsSuperType, optionalSuperType, forges);

	        string underlyingClassNameSimple = jsonClassNameSimple;
	        string underlyingClassNameForReference = optionalUnderlyingProvided != null ? optionalUnderlyingProvided.Name : underlyingClassNameSimple;
	        StmtClassForgeableFactory underlying = new ProxyStmtClassForgeableFactory() {
	            ProcMake = (namespaceScope, classPostfix) =>  {
	                return new StmtClassForgeableJsonUnderlying(underlyingClassNameSimple, namespaceScope, forgeableDesc);
	            },
	        };

	        string delegateClassNameSimple = jsonClassNameSimple + "__Delegate";
	        StmtClassForgeableFactory delegate = new ProxyStmtClassForgeableFactory() {
	            ProcMake = (namespaceScope, classPostfix) =>  {
	                return new StmtClassForgeableJsonDelegate(CodegenClassType.JSONDELEGATE, delegateClassNameSimple, namespaceScope, underlyingClassNameForReference, forgeableDesc);
	            },
	        };

	        string delegateFactoryClassNameSimple = jsonClassNameSimple + "__Factory";
	        StmtClassForgeableFactory delegateFactory = new ProxyStmtClassForgeableFactory() {
	            ProcMake = (namespaceScope, classPostfix) =>  {
	                return new StmtClassForgeableJsonDelegateFactory(CodegenClassType.JSONDELEGATEFACTORY, delegateFactoryClassNameSimple, optionalUnderlyingProvided != null, namespaceScope, delegateClassNameSimple, underlyingClassNameForReference, forgeableDesc);
	            },
	        };

	        string underlyingClassNameFull = optionalUnderlyingProvided == null ? services.PackageName + "." + underlyingClassNameSimple : optionalUnderlyingProvided.Name;
	        string delegateClassNameFull = services.PackageName + "." + delegateClassNameSimple;
	        string delegateFactoryClassNameFull = services.PackageName + "." + delegateFactoryClassNameSimple;
	        string serdeClassNameFull = services.PackageName + "." + jsonClassNameSimple + "__" + metadata.Name + "__Serde"; // include event type name as underlying-class may occur multiple times

	        JsonEventTypeDetail detail = new JsonEventTypeDetail(underlyingClassNameFull, optionalUnderlyingProvided, delegateClassNameFull, delegateFactoryClassNameFull, serdeClassNameFull, fieldDescriptors, dynamic, numFieldsSuperType);
	        EventTypeNestableGetterFactoryJson getterFactoryJson = new EventTypeNestableGetterFactoryJson(detail);

	        Type standIn = optionalUnderlyingProvided == null ? services.CompilerServices.CompileStandInClass(CodegenClassType.JSONEVENT, underlyingClassNameSimple, services.Services)
	            : optionalUnderlyingProvided;

	        JsonEventType eventType = new JsonEventType(metadata, properties,
	            superTypes == null ? new EventType[0] : superTypes.First,
	            superTypes == null ? Collections.EmptySet() : superTypes.Second,
	            config == null ? null : config.StartTimestampPropertyName,
	            config == null ? null : config.EndTimestampPropertyName,
	            getterFactoryJson, services.BeanEventTypeFactoryPrivate, detail, standIn);

	        IList<StmtClassForgeableFactory> additionalForgeables = new List<>(3);

	        // generate delegate and factory forgables for application classes
	        GenerateApplicationClassForgables(optionalUnderlyingProvided, deepClasses, additionalForgeables, raw.Annotations, services);

	        if (optionalUnderlyingProvided == null) {
	            additionalForgeables.Add(underlying);
	        }
	        additionalForgeables.Add(delegate);
	        additionalForgeables.Add(delegateFactory);

	        return new EventTypeForgablesPair(eventType, additionalForgeables);
	    }

	    private static void ValidateFieldTypes(Type declaredClass, IDictionary<string, Field> targetFields, IDictionary<string, object> insertedFields) {
	        foreach (KeyValuePair<string, object> inserted in insertedFields.EntrySet()) {
	            string insertedName = inserted.Key;
	            object insertedType = inserted.Value;
	            FieldInfo field = targetFields.Get(insertedName);

	            if (field == null) {
	                throw new ExprValidationException("Failed to find public field '" + insertedName + "' on class '" + declaredClass.Name + "'");
	            }

	            Type fieldClass = TypeHelper.GetBoxedType(field.Type);
	            if (insertedType is Type) {
	                Type insertedClass = TypeHelper.GetBoxedType((Type) insertedType);
	                if (!TypeHelper.IsSubclassOrImplementsInterface(insertedClass, fieldClass)) {
	                    throw MakeInvalidField(insertedName, insertedClass, declaredClass, field);
	                }
	            } else if (insertedType is TypeBeanOrUnderlying || insertedType is EventType) {
	                EventType eventType = (insertedType is TypeBeanOrUnderlying) ? ((TypeBeanOrUnderlying) insertedType).EventType : (EventType) insertedType;
	                if (!TypeHelper.IsSubclassOrImplementsInterface(eventType.UnderlyingType, fieldClass)) {
	                    throw MakeInvalidField(insertedName, eventType.UnderlyingType, declaredClass, field);
	                }
	            } else if (insertedType is TypeBeanOrUnderlying[] || insertedType is EventType[]) {
	                EventType eventType = (insertedType is TypeBeanOrUnderlying[]) ? ((TypeBeanOrUnderlying[]) insertedType)[0].EventType : ((EventType[]) insertedType)[0];
	                if (!fieldClass.IsArray || !TypeHelper.IsSubclassOrImplementsInterface(eventType.UnderlyingType, fieldClass.ComponentType)) {
	                    throw MakeInvalidField(insertedName, eventType.UnderlyingType, declaredClass, field);
	                }
	            } else {
	                throw new IllegalStateException("Unrecognized type '" + insertedType + "'");
	            }
	        }
	    }

	    private static ExprValidationException MakeInvalidField(string insertedName, Type insertedClass, Type declaredClass, FieldInfo field) {
	        return new ExprValidationException("Public field '" + insertedName + "' of class '" + TypeHelper.GetClassNameFullyQualPretty(declaredClass) + "' declared as type " +
	            "'" + TypeHelper.GetClassNameFullyQualPretty(field.Type) + "' cannot receive a value of type '" + TypeHelper.GetClassNameFullyQualPretty(insertedClass) + "'");
	    }

	    private static void GenerateApplicationClassForgables(Type optionalUnderlyingProvided, IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses, IList<StmtClassForgeableFactory> additionalForgeables, Attribute[] annotations, StatementCompileTimeServices services)
	        {
	        foreach (KeyValuePair<Type, JsonApplicationClassDelegateDesc> entry in deepClasses.EntrySet()) {
	            if (entry.Key == optionalUnderlyingProvided) {
	                continue;
	            }

	            LinkedHashMap<string, Field> fields = new LinkedHashMap<>();
	            entry.Value.Fields.ForEach(field -> fields.Put(field.Name, field));

	            IDictionary<string, object> properties = ResolvePropertiesFromFields(fields);
	            IDictionary<string, string> fieldNames = ComputeFieldNamesFromProperties(properties);
	            IDictionary<string, JsonForgeDesc> forges = ComputeValueForges(properties, fields, deepClasses, annotations, services);
	            IDictionary<string, JsonUnderlyingField> fieldDescriptors = ComputeFields(properties, fieldNames, null, fields);

	            string delegateClassNameSimple = entry.Value.DelegateClassName;
	            StmtClassForgeableJsonDesc forgeableDesc = new StmtClassForgeableJsonDesc(properties, fieldDescriptors, false, 0, null, forges);
	            StmtClassForgeableFactory delegate = new ProxyStmtClassForgeableFactory() {
	                ProcMake = (namespaceScope, classPostfix) =>  {
	                    return new StmtClassForgeableJsonDelegate(CodegenClassType.JSONNESTEDCLASSDELEGATEANDFACTORY, delegateClassNameSimple, namespaceScope, entry.Key.Name, forgeableDesc);
	                },
	            };

	            string delegateFactoryClassNameSimple = entry.Value.DelegateFactoryClassName;
	            StmtClassForgeableFactory delegateFactory = new ProxyStmtClassForgeableFactory() {
	                ProcMake = (namespaceScope, classPostfix) =>  {
	                    return new StmtClassForgeableJsonDelegateFactory(CodegenClassType.JSONNESTEDCLASSDELEGATEANDFACTORY, delegateFactoryClassNameSimple, true, namespaceScope, delegateClassNameSimple, entry.Key.Name, forgeableDesc);
	                },
	            };

	            additionalForgeables.Add(delegate);
	            additionalForgeables.Add(delegateFactory);
	        }
	    }

	    private static IDictionary<string, string> ComputeFieldNamesFromProperties(IDictionary<string, object> properties) {
	        Dictionary<string, string> fieldNames = new LinkedHashMap<>();
	        foreach (string key in properties.KeySet()) {
	            fieldNames.Put(key, key);
	        }
	        return fieldNames;
	    }

	    private static Type DetermineUnderlyingProvided(JsonSchema jsonSchema, StatementCompileTimeServices services)
	        {
	        if (jsonSchema != null && !jsonSchema.ClassName().Trim().IsEmpty()) {
	            try {
	                return services.ClasspathImportServiceCompileTime.ResolveClass(jsonSchema.ClassName(), true, ClasspathExtensionClassEmpty.INSTANCE);
	            } catch (ClasspathImportException e) {
	                throw new ExprValidationException("Failed to resolve JSON event class '" + jsonSchema.ClassName() + "': " + e.Message, e);
	            }
	        }
	        return null;
	    }

	    private static bool DetermineDynamic(JsonSchema jsonSchema, JsonEventType optionalSuperType, StatementRawInfo raw) {
	        if (optionalSuperType != null && optionalSuperType.Detail.IsDynamic) {
	            return true;
	        }
	        return jsonSchema != null && jsonSchema.Dynamic();
	    }

	    private static IDictionary<string, object> RemoveEventBeanTypes(IDictionary<string, object> properties) {
	        LinkedHashMap<string, object> verified = new LinkedHashMap<>();
	        foreach (KeyValuePair<string, object> prop in properties.EntrySet()) {
	            string propertyName = prop.Key;
	            object propertyType = prop.Value;
	            verified.Put(propertyName, propertyType);

	            if (propertyType is EventType) {
	                EventType eventType = (EventType) propertyType;
	                verified.Put(propertyName, new TypeBeanOrUnderlying(eventType));
	            } else if (propertyType is EventType[]) {
	                EventType eventType = ((EventType[]) propertyType)[0];
	                verified.Put(propertyName, new TypeBeanOrUnderlying[]{new TypeBeanOrUnderlying(eventType)});
	            }
	        }
	        return verified;
	    }

	    private static IDictionary<string, JsonForgeDesc> ComputeValueForges(IDictionary<string, object> compiledTyping, IDictionary<string, Field> fields, IDictionary<Type, JsonApplicationClassDelegateDesc> deepClasses, Attribute[] annotations, StatementCompileTimeServices services) {
	        IDictionary<string, JsonForgeDesc> valueForges = new Dictionary<>();
	        foreach (KeyValuePair<string, object> entry in compiledTyping.EntrySet()) {
	            object type = entry.Value;
	            Field optionalField = fields.Get(entry.Key);
	            JsonForgeDesc forgeDesc;
	            if (type == null) {
	                forgeDesc = new JsonForgeDesc(entry.Key, null, null, JsonEndValueForgeNull.INSTANCE, JsonWriteForgeNull.INSTANCE);
	            } else if (type is Type) {
	                Type clazz = (Type) type;
	                forgeDesc = JsonForgeFactoryBuiltinClassTyped.Forge(clazz, entry.Key, optionalField, deepClasses, annotations, services);
	            } else if (type is TypeBeanOrUnderlying) {
	                EventType eventType = ((TypeBeanOrUnderlying) type).EventType;
	                ValidateJsonOrMapType(eventType);
	                if (eventType is JsonEventType) {
	                    forgeDesc = JsonForgeFactoryEventTypeTyped.ForgeNonArray(entry.Key, (JsonEventType) eventType);
	                } else {
	                    forgeDesc = JsonForgeFactoryBuiltinClassTyped.Forge(typeof(IDictionary), entry.Key, optionalField, deepClasses, annotations, services);
	                }
	            } else if (type is TypeBeanOrUnderlying[]) {
	                EventType eventType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
	                ValidateJsonOrMapType(eventType);
	                if (eventType is JsonEventType) {
	                    forgeDesc = JsonForgeFactoryEventTypeTyped.ForgeArray(entry.Key, (JsonEventType) eventType);
	                } else {
	                    forgeDesc = JsonForgeFactoryBuiltinClassTyped.Forge(typeof(IDictionary[]), entry.Key, optionalField, deepClasses, annotations, services);
	                }
	            } else {
	                throw new IllegalStateException("Unrecognized type " + type);
	            }
	            valueForges.Put(entry.Key, forgeDesc);
	        }
	        return valueForges;
	    }

	    private static void ValidateJsonOrMapType(EventType eventType) {
	        if (!(eventType is JsonEventType) && !(eventType is MapEventType)) {
	            throw new ExprValidationException("Failed to validate event type '" + eventType.Metadata.Name + "', expected a Json or Map event type");
	        }
	    }

	    private static IDictionary<string, JsonUnderlyingField> ComputeFields(IDictionary<string, object> compiledTyping, IDictionary<string, string> fieldNames, JsonEventType optionalSuperType, IDictionary<string, Field> fields) {
	        IDictionary<string, JsonUnderlyingField> allFieldsInclSupertype = new LinkedHashMap<>();

	        int index = 0;
	        if (optionalSuperType != null) {
	            allFieldsInclSupertype.PutAll(optionalSuperType.Detail.FieldDescriptors);
	            index = allFieldsInclSupertype.Count;
	        }

	        foreach (KeyValuePair<string, object> entry in compiledTyping.EntrySet()) {
	            string fieldName = fieldNames.Get(entry.Key);

	            object type = entry.Value;
	            Type assignedType;
	            if (type == null) {
	                assignedType = typeof(object);
	            } else if (type is Type) {
	                assignedType = (Type) type;
	            } else if (type is TypeBeanOrUnderlying) {
	                EventType other = ((TypeBeanOrUnderlying) type).EventType;
	                ValidateJsonOrMapType(other);
	                assignedType = GetAssignedType(other);
	            } else if (type is TypeBeanOrUnderlying[]) {
	                EventType other = ((TypeBeanOrUnderlying[]) type)[0].EventType;
	                ValidateJsonOrMapType(other);
	                assignedType = TypeHelper.GetArrayType(GetAssignedType(other));
	            } else {
	                throw new IllegalStateException("Unrecognized type " + type);
	            }

	            allFieldsInclSupertype.Put(entry.Key, new JsonUnderlyingField(fieldName, index, assignedType, fields.Get(fieldName)));
	            index++;
	        }
	        return allFieldsInclSupertype;
	    }

	    private static Type GetAssignedType(EventType type) {
	        if (type is JsonEventType) {
	            return type.UnderlyingType;
	        }
	        if (type is MapEventType) {
	            return typeof(IDictionary);
	        }
	        throw new ExprValidationException("Incompatible type '" + type.Name + "' encountered, expected a Json or Map event type");
	    }

	    private static IDictionary<string, string> ComputeFieldNames(IDictionary<string, object> compiledTyping) {
	        IDictionary<string, string> fields = new Dictionary<>();
	        ISet<string> assignedNames = new HashSet<>();
	        foreach (string name in compiledTyping.KeySet()) {
	            string assigned = "_" + GetIdentifierMayStartNumeric(name.ToLowerCase(Locale.ENGLISH));
	            if (!assignedNames.Add(assigned)) {
	                int suffix = 0;
	                while (true) {
	                    string withSuffix = assigned + "_" + suffix;
	                    if (!assignedNames.Contains(withSuffix)) {
	                        assigned = withSuffix;
	                        assignedNames.Add(assigned);
	                        break;
	                    }
	                    suffix++;
	                }
	            }
	            fields.Put(name, assigned);
	        }
	        return fields;
	    }

	    public static void AddJsonUnderlyingClass(IDictionary<string, EventType> moduleTypes, ParentClassLoader classLoaderParent, string optionalDeploymentId) {
	        foreach (KeyValuePair<string, EventType> eventType in moduleTypes.EntrySet()) {
	            AddJsonUnderlyingClassInternal(eventType.Value, classLoaderParent, optionalDeploymentId);
	        }
	    }

	    public static void AddJsonUnderlyingClass(PathRegistry<string, EventType> pathEventTypes, ParentClassLoader classLoaderParent) {
	        pathEventTypes.Traverse(type -> AddJsonUnderlyingClassInternal(type, classLoaderParent, null));
	    }

	    private static void AddJsonUnderlyingClassInternal(EventType eventType, ParentClassLoader classLoaderParent, string optionalDeploymentId) {
	        if (!(eventType is JsonEventType)) {
	            return;
	        }
	        JsonEventType jsonEventType = (JsonEventType) eventType;
	        // for named-window the same underlying is used and we ignore duplicate add
	        bool allowDuplicate = eventType.Metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW;
	        if (jsonEventType.Detail.OptionalUnderlyingProvided == null) {
	            classLoaderParent.Add(jsonEventType.Detail.UnderlyingClassName, jsonEventType.UnderlyingType, optionalDeploymentId, allowDuplicate);
	        } else {
	            allowDuplicate = true;
	        }
	        classLoaderParent.Add(jsonEventType.Detail.DelegateClassName, jsonEventType.DelegateType, optionalDeploymentId, allowDuplicate);
	        classLoaderParent.Add(jsonEventType.Detail.DelegateFactoryClassName, jsonEventType.DelegateFactory.GetType(), optionalDeploymentId, allowDuplicate);
	    }

	    private static IDictionary<string, object> ResolvePropertiesFromFields(IDictionary<string, Field> fields) {
	        LinkedHashMap<string, object> properties = new LinkedHashMap<>(CollectionUtil.CapacityHashMap(fields.Count));
	        foreach (KeyValuePair<string, Field> field in fields.EntrySet()) {
	            properties.Put(field.Key, field.Value.Type);
	        }
	        return properties;
	    }
	}
} // end of namespace
