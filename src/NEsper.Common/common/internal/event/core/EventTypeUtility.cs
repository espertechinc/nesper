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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.manufacturer;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        /// <summary>
        /// Compare types
        /// </summary>
        /// <param name="newEventType">the proposed new type</param>
        /// <param name="existingType">the existing type</param>
        /// <throws>ExprValidationException when validation fails</throws>
        public static void CompareExistingType(
            EventType newEventType,
            EventType existingType)
        {
            var compared = ((EventTypeSPI)newEventType).EqualsCompareType(existingType);
            if (compared != null) {
                throw new ExprValidationException(
                    "Event type named '" +
                    newEventType.Name +
                    "' has already been declared with differing column name or type information: " +
                    compared.Message,
                    compared);
            }
        }

        public static EventBeanSPI GetShellForType(EventType eventType)
        {
            if (eventType is BeanEventType) {
                return new BeanEventBean(null, eventType);
            }

            if (eventType is ObjectArrayEventType) {
                return new ObjectArrayEventBean(null, eventType);
            }

            if (eventType is MapEventType) {
                return new MapEventBean(null, eventType);
            }

            if (eventType is BaseXMLEventType) {
                return new XMLEventBean(null, eventType);
            }

            if (eventType is JsonEventType) {
                return new JsonEventBean(null, eventType);
            }

            if (eventType is WrapperEventType) {
                return new WrapperEventBean(null, null, eventType);
            }

            throw new EventAdapterException("Event type '" + eventType.Name + "' is not an runtime-native event type");
        }

        public static EventBeanFactory GetFactoryForType(
            EventType type,
            EventBeanTypedEventFactory factory,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            if (type is WrapperEventType wrapperType) {
                if (wrapperType.UnderlyingEventType is BeanEventType) {
                    return new EventBeanFactoryBeanWrapped(wrapperType.UnderlyingEventType, wrapperType, factory);
                }
            }

            if (type is BeanEventType) {
                return new EventBeanFactoryBean(type, factory);
            }

            if (type is MapEventType) {
                return new EventBeanFactoryMap(type, factory);
            }

            if (type is ObjectArrayEventType) {
                return new EventBeanFactoryObjectArray(type, factory);
            }

            if (type is BaseXMLEventType) {
                return new EventBeanFactoryXML(type, factory);
            }

            if (type is AvroSchemaEventType) {
                return eventTypeAvroHandler.GetEventBeanFactory(type, factory);
            }

            if (type is JsonEventType eventType) {
                return new EventBeanFactoryJson(eventType, factory);
            }

            throw new ArgumentException(
                "Cannot create event bean factory for event type '" +
                type.Name +
                "': " +
                type.GetType().Name +
                " is not a recognized event type or supported wrap event type");
        }

        /// <summary>
        /// Returns a factory for creating and populating event object instances for the given type.
        /// </summary>
        /// <param name="eventType">to create underlying objects for</param>
        /// <param name="properties">to write</param>
        /// <param name="importService">for resolving methods</param>
        /// <param name="allowAnyType">whether any type property can be populated</param>
        /// <param name="avroHandler">avro handler</param>
        /// <returns>factory</returns>
        /// <throws>EventBeanManufactureException if a factory cannot be created for the type</throws>
        public static EventBeanManufacturerForge GetManufacturer(
            EventType eventType,
            WriteablePropertyDescriptor[] properties,
            ImportService importService,
            bool allowAnyType,
            EventTypeAvroHandler avroHandler)
        {
            if (!(eventType is EventTypeSPI typeSpi)) {
                return null;
            }

            if (typeSpi is BeanEventType beanEventType) {
                return new EventBeanManufacturerBeanForge(beanEventType, properties, importService);
            }

            if (!allowAnyType && !AllowPopulate(typeSpi)) {
                return null;
            }

            if (typeSpi is MapEventType mapEventType) {
                return new EventBeanManufacturerMapForge(mapEventType, properties);
            }

            if (typeSpi is ObjectArrayEventType objectArrayEventType) {
                return new EventBeanManufacturerObjectArrayForge(objectArrayEventType, properties);
            }

            if (typeSpi is AvroSchemaEventType avroSchemaEventType) {
                return avroHandler.GetEventBeanManufacturer(avroSchemaEventType, properties);
            }

            if (typeSpi is JsonEventType jsonEventType) {
                if (jsonEventType.Detail.OptionalUnderlyingProvided != null) {
                    return new EventBeanManufacturerJsonProvidedForge(
                        jsonEventType,
                        properties,
                        importService);
                }

                return new EventBeanManufacturerJsonForge(jsonEventType, properties);
            }

            return null;
        }

        /// <summary>
        /// Returns descriptors for all writable properties.
        /// </summary>
        /// <param name="eventType">to reflect on</param>
        /// <param name="allowAnyType">whether any type property can be populated</param>
        /// <param name="allowFragmentType">whether to return writeable properties that are typed as event type</param>
        /// <returns>list of writable properties</returns>
        public static ISet<WriteablePropertyDescriptor> GetWriteableProperties(
            EventType eventType,
            bool allowAnyType,
            bool allowFragmentType)
        {
            if (!(eventType is EventTypeSPI typeSpi)) {
                return null;
            }

            if (typeSpi is BeanEventType beanEventType) {
                return PropertyHelper.GetWritableProperties(beanEventType.UnderlyingType);
            }

            if (!allowAnyType && !AllowPopulate(typeSpi)) {
                return null;
            }

            if (typeSpi is BaseNestableEventType type) {
                var mapdef = type.Types;
                ISet<WriteablePropertyDescriptor> writables = new LinkedHashSet<WriteablePropertyDescriptor>();
                foreach (var types in mapdef) {
                    if (types.Value is Type typesValue) {
                        writables.Add(new WriteablePropertyDescriptor(types.Key, typesValue, null, false));
                    }

                    if (types.Value is string) {
                        var typeName = types.Value.ToString();
                        var clazz = TypeHelper.GetPrimitiveTypeForName(typeName);
                        if (clazz != null) {
                            writables.Add(
                                new WriteablePropertyDescriptor(
                                    types.Key,
                                    clazz,
                                    null,
                                    false));
                        }
                        else if (allowFragmentType) {
                            writables.Add(new WriteablePropertyDescriptor(types.Key, null, null, true));
                        }
                    }

                    if (allowFragmentType && types.Value is TypeBeanOrUnderlying value) {
                        writables.Add(
                            new WriteablePropertyDescriptor(types.Key, value.EventType.UnderlyingType, null, true));
                    }

                    if (allowFragmentType && types.Value is TypeBeanOrUnderlying[] und) {
                        writables.Add(
                            new WriteablePropertyDescriptor(types.Key, und[0].EventType.UnderlyingType, null, true));
                    }
                }

                return writables;
            }
            else if (typeSpi is AvroSchemaEventType) {
                ISet<WriteablePropertyDescriptor> writables = new LinkedHashSet<WriteablePropertyDescriptor>();
                var desc = typeSpi.WriteableProperties;
                foreach (var prop in desc) {
                    writables.Add(new WriteablePropertyDescriptor(prop.PropertyName, prop.PropertyType, null, false));
                }

                return writables;
            }
            else {
                return null;
            }
        }

        public static CodegenExpression ResolveTypeCodegen(
            EventType eventType,
            CodegenExpression initServicesRef)
        {
            return ResolveTypeCodegenGivenResolver(
                eventType,
                ExprDotMethodChain(initServicesRef).Get(EPStatementInitServicesConstants.EVENTTYPERESOLVER));
        }

        public static CodegenExpression ResolveTypeCodegenGivenResolver(
            EventType eventType,
            CodegenExpression typeResolver)
        {
            if (eventType == null) {
                throw new ArgumentException("Null event type");
            }

            if (typeResolver == null) {
                throw new ArgumentException("Event type resolver not provided");
            }

            if (eventType is BeanEventType beanEventType &&
                eventType.Metadata.AccessModifier == NameAccessModifier.TRANSIENT) {
                var publicFields = beanEventType.Stem.IsPublicFields;
                return ExprDotMethod(
                    typeResolver,
                    EventTypeResolverConstants.RESOLVE_PRIVATE_BEAN_METHOD,
                    Constant(eventType.UnderlyingType),
                    Constant(publicFields));
            }

            return ExprDotMethod(
                typeResolver,
                EventTypeResolverConstants.RESOLVE_METHOD,
                Constant(eventType.Metadata.Name),
                Constant(eventType.Metadata.ModuleName),
                Constant(eventType.Metadata.AccessModifier));
        }

        public static CodegenExpression ResolveTypeArrayCodegen(
            EventType[] eventTypes,
            CodegenExpression initServicesRef)
        {
            var expressions = new CodegenExpression[eventTypes.Length];
            for (var i = 0; i < eventTypes.Length; i++) {
                expressions[i] = ResolveTypeCodegen(eventTypes[i], initServicesRef);
            }

            return NewArrayWithInit(typeof(EventType), expressions);
        }

        public static CodegenExpression ResolveTypeArrayCodegenMayNull(
            EventType[] eventTypes,
            CodegenExpression initServicesRef)
        {
            var expressions = new CodegenExpression[eventTypes.Length];
            for (var i = 0; i < eventTypes.Length; i++) {
                expressions[i] = eventTypes[i] == null
                    ? ConstantNull()
                    : ResolveTypeCodegen(eventTypes[i], initServicesRef);
            }

            return NewArrayWithInit(typeof(EventType), expressions);
        }

        public static CodegenExpression CodegenGetterWCoerce(
            EventPropertyGetterSPI getter,
            Type getterType,
            Type optionalCoercionType,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            getterType = getterType.GetBoxedType();
            
            var get = new CodegenExpressionLambda(method.Block)
                .WithParams(CodegenNamedParam.From(typeof(EventBean), "bean"));
            var anonymous = NewInstance<ProxyEventPropertyValueGetter>(get);
            
            // var anonymous = NewAnonymousClass(method.Block, typeof(EventPropertyValueGetter));
            // var get = CodegenMethod.MakeParentNode(typeof(object), generator, classScope)
            //     .AddParam(CodegenNamedParam.From(typeof(EventBean), "bean"));
            // anonymous.AddMethod("Get", get);

            var result = getter.EventBeanGetCodegen(Ref("bean"), method, classScope);
            if (optionalCoercionType is Type type &&
                getterType != optionalCoercionType &&
                getterType.IsTypeNumeric()) {
                var coercer = SimpleNumberCoercerFactory.GetCoercer(getterType, type.GetBoxedType());
                get.Block.DeclareVar(getterType, "prop", Cast(getterType, result));
                result = coercer.CoerceCodegen(Ref("prop"), getterType);
            }

            get.Block.BlockReturn(result);
            return anonymous;
        }

        public static CodegenExpression CodegenGetterWCoerceWArray(
            Type interfaceClass,
            EventPropertyGetterSPI getter,
            Type getterTypeUnboxed,
            Type optionalCoercionType,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            if (getterTypeUnboxed == null || optionalCoercionType == null) {
                return ConstantNull();
            }

            var getterType = getterTypeUnboxed.GetBoxedType();
            // CodegenExpressionNewAnonymousClass anonymous = NewAnonymousClass(method.Block, interfaceClass);

            Type proxyType;
            
            IList<CodegenNamedParam> parameters;
            if (interfaceClass == typeof(EventPropertyValueGetter)) {
                parameters = CodegenNamedParam.From(typeof(EventBean), "bean");
                proxyType = typeof(ProxyEventPropertyValueGetter);
            }
            else if (interfaceClass == typeof(ExprEventEvaluator)) {
                parameters = CodegenNamedParam.From(typeof(EventBean), "bean", typeof(ExprEvaluatorContext), "ctx");
                proxyType = typeof(ProxyExprEventEvaluator);
            }
            else {
                throw new IllegalStateException("Unrecognized interface class " + interfaceClass.Name);
            }

            // var getOrEval =
            //     CodegenMethod.MakeParentNode(typeof(object), generator, classScope).AddParam(parameters);
            // anonymous.AddMethod(interfaceClass == typeof(EventPropertyValueGetter) ? "Get" : "eval", getOrEval);
            //
            // var result = getter.EventBeanGetCodegen(Ref("bean"), method, classScope);
            // if (optionalCoercionType != null &&
            //     getterType != optionalCoercionType &&
            //     getterType.IsTypeNumeric()) {
            //     var coercer = SimpleNumberCoercerFactory.GetCoercer(
            //         getterType,
            //         optionalCoercionType.GetBoxedType());
            //     getOrEval.Block.DeclareVar(getterType, "prop", Cast(getterType, result));
            //     result = coercer.CoerceCodegen(Ref("prop"), getterType);
            // }
            //
            // if (getterType.IsArray) {
            //     var mkType = MultiKeyPlanner.GetMKClassForComponentType(GenericExtensions.GetComponentType(getterType));
            //     result = NewInstance(mkType, Cast(getterType, result));
            // }
            //
            // getOrEval.Block.MethodReturn(result);
            // return anonymous;

            var getOrEvalLambda = new CodegenExpressionLambda(method.Block)
                .WithParams(parameters)
                .WithBody(
                    block => {
                        var result = getter.EventBeanGetCodegen(Ref("bean"), method, classScope);
                        if (optionalCoercionType != null && getterType != optionalCoercionType && getterType.IsTypeNumeric()) {
                            var coercer = SimpleNumberCoercerFactory.GetCoercer(
                                getterType, optionalCoercionType.GetBoxedType());
                            block.DeclareVar(getterType, "prop", Cast(getterType, result));
                            result = coercer.CoerceCodegen(Ref("prop"), getterType);
                        }

                        if (getterType.IsArray) {
                            var mkType = MultiKeyPlanner.GetMKClassForComponentType(getterType.GetElementType());
                            result = NewInstance(mkType, Cast(getterType, result));
                        }

                        block.BlockReturn(result);
                    });
            
            return NewInstance(proxyType, getOrEvalLambda);
        }

        public static CodegenExpression CodegenWriter(
            EventType eventType,
            Type evaluationType,
            EventPropertyWriterSPI writer,
            CodegenMethod method,
            Type generator,
            CodegenClassScope classScope)
        {
            // CodegenExpressionNewAnonymousClass anonymous = NewAnonymousClass(method.Block, typeof(EventPropertyWriter));
            // var write = CodegenMethod
            //     .MakeParentNode(typeof(void), generator, classScope)
            //     .AddParam(CodegenNamedParam.From(typeof(object), "value", typeof(EventBean), "bean"));
            // anonymous.AddMethod("write", write);

            evaluationType = evaluationType.GetBoxedType();

            var write = new CodegenExpressionLambda(method.Block)
                .WithParam<object>("value")
                .WithParam<EventBean>("bean")
                .WithBody(
                    block => {
                        block
                            .DeclareVar(
                                eventType.UnderlyingType,
                                "und",
                                Cast(eventType.UnderlyingType, ExprDotUnderlying(Ref("bean"))));
                        
                        var value = ConstantNull();
                        if (evaluationType != null) {
                            block.DeclareVar(evaluationType, "eval", Cast(evaluationType, Ref("value")));
                            value = Ref("eval");
                        }

                        block.Expression(
                            writer.WriteCodegen(
                                value,
                                Ref("und"),
                                Ref("bean"),
                                method,
                                classScope));
                    });
            
            return NewInstance<ProxyEventPropertyWriter>(write);
        }

        public static IDictionary<string, object> GetPropertyTypesNonPrimitive(
            IDictionary<string, object> propertyTypesMayPrimitive)
        {
            var hasPrimitive = false;
            foreach (var entry in propertyTypesMayPrimitive) {
                if (entry.Value is Type type && type.IsPrimitive) {
                    hasPrimitive = true;
                    break;
                }
            }

            if (!hasPrimitive) {
                return propertyTypesMayPrimitive;
            }

            var props = new LinkedHashMap<string, object>(propertyTypesMayPrimitive);
            foreach (var entry in propertyTypesMayPrimitive) {
                if (entry.Value is Type clazz) {
                    if (clazz.IsPrimitive) {
                        props.Put(entry.Key, clazz.GetBoxedType());
                    }
                }
            }

            return props;
        }

        public static EventType RequireEventType(
            string aspectCamel,
            string aspectName,
            string optionalEventTypeName,
            EventTypeCompileTimeResolver eventTypeCompileTimeResolver)
        {
            if (optionalEventTypeName == null) {
                throw new ExprValidationException(
                    aspectCamel +
                    " '" +
                    aspectName +
                    "' returns EventBean-array but does not provide the event type name");
            }

            var eventType = eventTypeCompileTimeResolver.GetTypeByName(optionalEventTypeName);
            if (eventType == null) {
                throw new ExprValidationException(
                    aspectCamel +
                    " '" +
                    aspectName +
                    "' returns event type '" +
                    optionalEventTypeName +
                    "' and the event type cannot be found");
            }

            return eventType;
        }

        public static Pair<EventType[], ISet<EventType>> GetSuperTypesDepthFirst(
            ISet<string> superTypesSet,
            EventUnderlyingType representation,
            EventTypeNameResolver eventTypeNameResolver)
        {
            return GetSuperTypesDepthFirst(
                superTypesSet == null || superTypesSet.IsEmpty() ? null : superTypesSet.ToArray(),
                representation,
                eventTypeNameResolver);
        }

        public static Pair<EventType[], ISet<EventType>> GetSuperTypesDepthFirst(
            string[] superTypesSet,
            EventUnderlyingType representation,
            EventTypeNameResolver eventTypeNameResolver)
        {
            if (superTypesSet == null || superTypesSet.Length == 0) {
                return new Pair<EventType[], ISet<EventType>>(null, null);
            }

            var superTypes = new EventType[superTypesSet.Length];
            ISet<EventType> deepSuperTypes = new LinkedHashSet<EventType>();

            var count = 0;
            foreach (var superName in superTypesSet) {
                var type = eventTypeNameResolver.GetTypeByName(superName);
                if (type == null) {
                    throw new EventAdapterException("Supertype by name '" + superName + "' could not be found");
                }

                if (representation == EventUnderlyingType.MAP) {
                    if (!(type is MapEventType)) {
                        throw new EventAdapterException(
                            "Supertype by name '" +
                            superName +
                            "' is not a Map, expected a Map event type as a supertype");
                    }
                }
                else if (representation == EventUnderlyingType.OBJECTARRAY) {
                    if (!(type is ObjectArrayEventType)) {
                        throw new EventAdapterException(
                            "Supertype by name '" +
                            superName +
                            "' is not an Object-array type, expected a Object-array event type as a supertype");
                    }
                }
                else if (representation == EventUnderlyingType.AVRO) {
                    if (!(type is AvroSchemaEventType)) {
                        throw new EventAdapterException(
                            "Supertype by name '" +
                            superName +
                            "' is not an Avro type, expected a Avro event type as a supertype");
                    }
                }
                else if (representation == EventUnderlyingType.JSON) {
                    if (!(type is JsonEventType)) {
                        throw new EventAdapterException(
                            "Supertype by name '" +
                            superName +
                            "' is not a Json type, expected a Json event type as a supertype");
                    }
                }
                else {
                    throw new IllegalStateException("Unrecognized enum " + representation);
                }

                superTypes[count++] = type;
                deepSuperTypes.Add(type);
                AddRecursiveSupertypes(deepSuperTypes, type);
            }

            var superTypesListDepthFirst = new List<EventType>(deepSuperTypes);
            superTypesListDepthFirst.Reverse();

            return new Pair<EventType[], ISet<EventType>>(superTypes, new LinkedHashSet<EventType>(superTypesListDepthFirst));
        }

        public static EventPropertyDescriptor GetNestablePropertyDescriptor(
            EventType target,
            string propertyName)
        {
            var descriptor = target.GetPropertyDescriptor(propertyName);
            if (descriptor != null) {
                return descriptor;
            }

            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                return null;
            }

            // parse, can be an nested property
            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is PropertyBase propertyBase) {
                return target.GetPropertyDescriptor(propertyBase.PropertyNameAtomic);
            }

            if (!(property is NestedProperty nested)) {
                return null;
            }

            Deque<Property> properties = new ArrayDeque<Property>(nested.Properties);
            return GetNestablePropertyDescriptor(target, properties);
        }

        public static EventPropertyDescriptor GetNestablePropertyDescriptor(
            EventType target,
            Deque<Property> stack)
        {
            var topProperty = stack.RemoveFirst();
            if (stack.IsEmpty()) {
                return target.GetPropertyDescriptor(((PropertyBase)topProperty).PropertyNameAtomic);
            }

            if (!(topProperty is SimpleProperty simple)) {
                return null;
            }

            var fragmentEventType = target.GetFragmentType(simple.PropertyNameAtomic);
            if (fragmentEventType == null || fragmentEventType.FragmentType == null) {
                return null;
            }

            return GetNestablePropertyDescriptor(fragmentEventType.FragmentType, stack);
        }

        public static LinkedHashMap<string, object> BuildType(
            IList<ColumnDesc> columns,
            ICollection<string> copyFrom,
            ImportServiceCompileTime importService,
            ExtensionClass classpathExtension,
            EventTypeNameResolver eventTypeResolver)
        {
            var typing = new LinkedHashMap<string, object>();
            ISet<string> columnNames = new HashSet<string>();

            // Copy-from information gets added first as object-array appends information in well-defined order
            if (copyFrom != null && !copyFrom.IsEmpty()) {
                foreach (var copyFromName in copyFrom) {
                    var type = eventTypeResolver.GetTypeByName(copyFromName);
                    if (type == null) {
                        throw new ExprValidationException("Type by name '" + copyFromName + "' could not be located");
                    }

                    MergeType(typing, type, columnNames);
                }
            }

            foreach (var column in columns) {
                var added = columnNames.Add(column.Name);
                if (!added) {
                    throw new ExprValidationException("Duplicate column name '" + column.Name + "'");
                }

                var columnType = BuildType(column, importService, classpathExtension);
                typing.Put(column.Name, columnType);
            }

            return typing;
        }

        public static object BuildType(
            ColumnDesc column,
            ImportServiceCompileTime importService,
            ExtensionClass classpathExtension)
        {
            if (column.Type == null) {
                return null;
            }

            var classIdent = ClassDescriptor.ParseTypeText(column.Type);
            Type type = ImportTypeUtil.ResolveClassIdentifierToType(
                classIdent,
                false,
                importService,
                ExtensionClassEmpty.INSTANCE);
            
            if (type != null) {
                return type;
            }

            // Event types fall into here
            if (classIdent.ArrayDimensions > 1) {
                throw new EPException(
                    "Two-dimensional arrays are not supported for event-types (cannot find class '" +
                    classIdent.ClassIdentifier +
                    "')");
            }

            if (classIdent.ArrayDimensions == 1) {
                return column.Type + "[]";
            }

            return column.Type;
        }

        private static void MergeType(
            IDictionary<string, object> typing,
            EventType typeToMerge,
            ISet<string> columnNames)
        {
            foreach (var prop in typeToMerge.PropertyDescriptors) {
                var existing = typing.Get(prop.PropertyName);

                if (!prop.IsFragment) {
                    var assigned = prop.PropertyType;
                    if (existing is Type) {
                        if (((Type) existing).GetBoxedType() != assigned.GetBoxedType()) {
                            throw new ExprValidationException(
                                "Type by name '" +
                                typeToMerge.Name +
                                "' contributes property '" +
                                prop.PropertyName +
                                "' defined as '" +
                                assigned.CleanName() +
                                "' which overrides the same property of type '" +
                                ((Type) existing).CleanName() +
                                "'");
                        }
                    }

                    typing.Put(prop.PropertyName, prop.PropertyType);
                }
                else {
                    if (existing != null) {
                        throw new ExprValidationException(
                            "Property by name '" +
                            prop.PropertyName +
                            "' is defined twice by adding type '" +
                            typeToMerge.Name +
                            "'");
                    }

                    var fragment = typeToMerge.GetFragmentType(prop.PropertyName);
                    if (fragment == null) {
                        continue;
                    }

                    // for native-type fragments (classes) we use the original type as available from map or object-array
                    if (typeToMerge is BaseNestableEventType baseNestable && fragment.IsNative) {
                        typing.Put(prop.PropertyName, baseNestable.Types.Get(prop.PropertyName));
                    }
                    else {
                        if (fragment.IsIndexed) {
                            typing.Put(prop.PropertyName, new EventType[] { fragment.FragmentType });
                        }
                        else {
                            typing.Put(prop.PropertyName, fragment.FragmentType);
                        }
                    }
                }

                columnNames.Add(prop.PropertyName);
            }
        }

        public static void ValidateTimestampProperties(
            EventType eventType,
            string startTimestampProperty,
            string endTimestampProperty)
        {
            if (startTimestampProperty != null) {
                if (eventType.GetGetter(startTimestampProperty) == null) {
                    throw new ConfigurationException(
                        "Declared start timestamp property name '" + startTimestampProperty + "' was not found");
                }

                var type = eventType.GetPropertyType(startTimestampProperty);
                if (!TypeHelper.IsDateTime(type)) {
                    throw new ConfigurationException(
                        "Declared start timestamp property '" +
                        startTimestampProperty +
                        "' is expected to return a DateTimeEx, DateTime, DateTimeOffset or long-typed value but returns '" +
                        type.CleanName() +
                        "'");
                }
            }

            if (endTimestampProperty != null) {
                if (startTimestampProperty == null) {
                    throw new ConfigurationException(
                        "Declared end timestamp property requires that a start timestamp property is also declared");
                }

                if (eventType.GetGetter(endTimestampProperty) == null) {
                    throw new ConfigurationException(
                        "Declared end timestamp property name '" + endTimestampProperty + "' was not found");
                }

                var type = eventType.GetPropertyType(endTimestampProperty);
                if (!TypeHelper.IsDateTime(type)) {
                    throw new ConfigurationException(
                        "Declared end timestamp property '" +
                        endTimestampProperty +
                        "' is expected to return a DateTimeEx, DateTime, DateTimeOffset or long-typed value but returns '" +
                        type.CleanName() +
                        "'");
                }

                var startType = eventType.GetPropertyType(startTimestampProperty);
                if (startType.GetBoxedType() != type.GetBoxedType()) {
                    throw new ConfigurationException(
                        "Declared end timestamp property '" +
                        endTimestampProperty +
                        "' is expected to have the same property type as the start-timestamp property '" +
                        startTimestampProperty +
                        "'");
                }
            }
        }

        public static bool IsTypeOrSubTypeOf(
            EventType candidate,
            EventType superType)
        {
            if (Equals(candidate, superType)) {
                return true;
            }

            if (candidate.SuperTypes != null) {
                foreach (var type in candidate.DeepSuperTypes) {
                    if (Equals(type, superType)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determine among the Map-type properties which properties are Bean-type event type names,
        /// rewrites these as Class-type instead so that they are configured as native property and do not require wrapping,
        /// but may require unwrapping.
        /// </summary>
        /// <param name="typing">properties of map type</param>
        /// <param name="eventTypeResolver">resolver</param>
        /// <returns>compiled properties, same as original unless Bean-type event type names were specified.</returns>
        public static IDictionary<string, object> CompileMapTypeProperties(
            IDictionary<string, object> typing,
            EventTypeNameResolver eventTypeResolver)
        {
            var compiled = new LinkedHashMap<string, object>(typing);
            foreach (var specifiedEntry in typing) {
                var typeSpec = specifiedEntry.Value;
                var nameSpec = specifiedEntry.Key;

                if (typeSpec == null) {
                    compiled.Put(nameSpec, null);
                    continue;
                }

                if (typeSpec is Type typeSpecType) {
                    compiled.Put(nameSpec, typeSpecType.GetBoxedType());
                    continue;
                }

                if (typeSpec is IDictionary<string, object> typeSpecAsDictionary) {
                    var inner = CompileMapTypeProperties(
                        typeSpecAsDictionary,
                        eventTypeResolver);
                    compiled.Put(nameSpec, inner);
                }

                if (!(typeSpec is string typeNameSpec)) {
                    continue;
                }

                var isArray = IsPropertyArray(typeNameSpec);
                if (isArray) {
                    typeNameSpec = GetPropertyRemoveArray(typeNameSpec);
                }

                var eventType = eventTypeResolver.GetTypeByName(typeNameSpec);
                if (eventType == null || !(eventType is BeanEventType)) {
                    continue;
                }

                var beanEventType = (BeanEventType) eventType;
                var underlyingType = beanEventType.UnderlyingType;
                if (isArray) {
                    underlyingType = TypeHelper.GetArrayType(underlyingType);
                }

                compiled.Put(nameSpec, underlyingType);
            }

            return compiled;
        }

        /// <summary>
        /// Returns true if the name indicates that the type is an array type.
        /// </summary>
        /// <param name="name">the property name</param>
        /// <returns>true if array type</returns>
        public static bool IsPropertyArray(string name)
        {
            return name.Trim().EndsWith("[]");
        }

        public static bool IsTypeOrSubTypeOf(
            string typeName,
            EventType sameTypeOrSubtype)
        {
            if (sameTypeOrSubtype.Name.Equals(typeName)) {
                return true;
            }

            if (sameTypeOrSubtype.SuperTypes == null) {
                return false;
            }

            foreach (var superType in sameTypeOrSubtype.SuperTypes) {
                if (superType.Name.Equals(typeName)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the property name without the array type extension, if present.
        /// </summary>
        /// <param name="name">property name</param>
        /// <returns>property name with removed array extension name</returns>
        public static string GetPropertyRemoveArray(string name)
        {
            return name
                .RegexReplaceAll("\\[", "")
                .RegexReplaceAll("\\]", "");
        }

        public static PropertySetDescriptor GetNestableProperties(
            IDictionary<string, object> propertiesToAdd,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNestableGetterFactory factory,
            EventType[] optionalSuperTypes,
            BeanEventTypeFactory beanEventTypeFactory,
            bool publicFields)
        {
            IList<string> propertyNameList = new List<string>();
            IList<EventPropertyDescriptor> propertyDescriptors = new List<EventPropertyDescriptor>();
            IDictionary<string, object> nestableTypes = new LinkedHashMap<string, object>();
            IDictionary<string, PropertySetDescriptorItem> propertyItems =
                new Dictionary<string, PropertySetDescriptorItem>();

            // handle super-types first, such that the order of properties is well-defined from super-type to subtype
            if (optionalSuperTypes != null) {
                for (var i = 0; i < optionalSuperTypes.Length; i++) {
                    var superType = (BaseNestableEventType)optionalSuperTypes[i];
                    foreach (var propertyName in superType.PropertyNames) {
                        if (nestableTypes.ContainsKey(propertyName)) {
                            continue;
                        }

                        propertyNameList.Add(propertyName);
                    }

                    foreach (var descriptor in superType.PropertyDescriptors) {
                        if (nestableTypes.ContainsKey(descriptor.PropertyName)) {
                            continue;
                        }

                        propertyDescriptors.Add(descriptor);
                    }

                    propertyItems.PutAll(superType.PropertyItems);
                    nestableTypes.PutAll(superType.NestableTypes);
                }
            }

            nestableTypes.PutAll(propertiesToAdd);

            // Initialize getters and names array: at this time we do not care about nested types,
            // these are handled at the time someone is asking for them
            foreach (var entry in propertiesToAdd) {
                if (!(entry.Key is string)) {
                    throw new EPException("Invalid type configuration: property name is not a String-type value");
                }

                var name = entry.Key;

                // handle types that are String values
                if (entry.Value is string) {
                    var value = entry.Value.ToString().Trim();
                    var clazz = TypeHelper.GetPrimitiveTypeForName(value);
                    if (clazz != null) {
                        propertiesToAdd[entry.Key] = clazz;
                    }
                }

                if (entry.Value is Type asType) {
                    if (asType == typeof(FlexCollection)) {
                        asType = typeof(ICollection<object>);
                    }
                    
                    var componentType = asType.GetComponentType();
                    var isIndexed = componentType != null;


                    var isMapped = asType.IsGenericStringDictionary();
                    if (isMapped) {
                        componentType = asType.GetDictionaryValueType();
#if NOT_IN_JAVA
                        componentType = typeof(object); // Cannot determine the type at runtime
#endif
                }

                    var isFragment = asType.IsFragmentableType();
                    BeanEventType nativeFragmentType = null;
                    FragmentEventType fragmentType = null;
                    if (isFragment) {
                        fragmentType = EventBeanUtility.CreateNativeFragmentType(asType, beanEventTypeFactory, publicFields);
                        if (fragmentType != null) {
                            nativeFragmentType = (BeanEventType)fragmentType.FragmentType;
                        }
                        else {
                            isFragment = false;
                        }
                    }

                    var getter = factory.GetGetterProperty(name, nativeFragmentType, eventBeanTypedEventFactory, isFragment);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        asType,
                        false,
                        false,
                        isIndexed,
                        isMapped,
                        isFragment);
                    var item = new PropertySetDescriptorItem(descriptor, getter, fragmentType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                // A null-type is also allowed
                if (entry.Value == null) {
                    var getter = factory.GetGetterProperty(name, null, null, false);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        null,
                        false,
                        false,
                        false,
                        false,
                        false);
                    var item = new PropertySetDescriptorItem(descriptor, getter, null);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                // Add Map itself as a property
                if (entry.Value is IDictionary<string, object>) {
                    var getter = factory.GetGetterProperty(name, null, null, false);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        typeof(IDictionary<string, object>),
                        false,
                        false,
                        false,
                        true,
                        false);
                    var item = new PropertySetDescriptorItem(descriptor, getter, null);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entry.Value is EventType eventTypeX) {
                    // Add EventType itself as a property
                    var getter = factory.GetGetterEventBean(name, eventTypeX.UnderlyingType);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        eventTypeX.UnderlyingType,
                        false,
                        false,
                        false,
                        false,
                        true);
                    var fragmentEventType = new FragmentEventType(eventTypeX, false, false, true);
                    var item = new PropertySetDescriptorItem(
                        descriptor,
                        getter,
                        fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entry.Value is EventType[] eventTypeArray) {
                    // Add EventType array itself as a property, Type is expected to be first array element
                    var eventType = eventTypeArray[0];
                    var prototypeArray = Arrays.CreateInstanceChecked(eventType.UnderlyingType, 0);
                    var getter = factory.GetGetterEventBeanArray(name, eventType);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        prototypeArray.GetType(),
                        false,
                        false,
                        true,
                        false,
                        true);
                    var fragmentEventType = new FragmentEventType(eventType, true, false, true);
                    var item = new PropertySetDescriptorItem(
                        descriptor,
                        getter,
                        fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entry.Value is TypeBeanOrUnderlying typeBeanOrUnderlying) {
                    var eventType = typeBeanOrUnderlying.EventType;
                    if (!(eventType is BaseNestableEventType)) {
                        throw new EPException(
                            "Nestable type configuration encountered an unexpected property type name '" +
                            typeBeanOrUnderlying +
                            "' for property '" +
                            name +
                            "', expected Type or Dictionary or the name of a previously-declared event type");
                    }

                    var underlyingType = eventType.UnderlyingType;
                    var getter = factory.GetGetterBeanNested(
                        name,
                        eventType,
                        eventBeanTypedEventFactory);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        underlyingType,
                        false,
                        false,
                        false,
                        false,
                        true);
                    var fragmentEventType = new FragmentEventType(eventType, false, false, true);
                    var item = new PropertySetDescriptorItem(
                        descriptor,
                        getter,
                        fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entry.Value is TypeBeanOrUnderlying[] typeBeanOrUnderlyingArray) {
                    var eventType = typeBeanOrUnderlyingArray[0].EventType;
                    if (!(eventType is BaseNestableEventType) && !(eventType is BeanEventType)) {
                        throw new EPException(
                            "Nestable type configuration encountered an unexpected property type name '" +
                            typeBeanOrUnderlyingArray +
                            "' for property '" +
                            name +
                            "', expected System.Type or Map or the name of a previously-declared event type");
                    }

                    var underlyingType = eventType.UnderlyingType;
                    var propertyComponentType = underlyingType;
                    if (underlyingType != typeof(object[])) {
                        underlyingType = TypeHelper.GetArrayType(underlyingType);
                    }

                    var getter = factory.GetGetterBeanNestedArray(
                        name,
                        eventType,
                        eventBeanTypedEventFactory);
                    var descriptor = new EventPropertyDescriptor(
                        name,
                        underlyingType,
                        false,
                        false,
                        true,
                        false,
                        true);
                    var fragmentEventType = new FragmentEventType(eventType, true, false, true);
                    var item = new PropertySetDescriptorItem(
                        descriptor,
                        getter,
                        fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                GenerateExceptionNestedProp(name, entry.Value);
            }

            return new PropertySetDescriptor(propertyNameList, propertyDescriptors, propertyItems, nestableTypes);
        }

        private static void GenerateExceptionNestedProp(
            string name,
            object value)
        {
            var clazzName = value == null ? "null" : value.GetType().Name;
            throw new EPException(
                "Nestable type configuration encountered an unexpected property type of '" +
                clazzName +
                "' for property '" +
                name +
                "', expected Type or Dictionary or the name of a previously-declared event type");
        }

        public static Type GetNestablePropertyType(
            string propertyName,
            IDictionary<string, PropertySetDescriptorItem> simplePropertyTypes,
            IDictionary<string, object> nestableTypes,
            BeanEventTypeFactory beanEventTypeFactory,
            bool publicFields)
        {
            var propertyNameUnescape = StringValue.UnescapeDot(propertyName);
            var item = simplePropertyTypes.Get(propertyNameUnescape);
            if (item != null) {
                return item.PropertyDescriptor.PropertyType;
            }

            // see if this is a nested property
            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                // dynamic simple property
                if (propertyName.EndsWith("?")) {
                    return typeof(object);
                }

                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);

                if (property is SimpleProperty) {
                    var propitem = simplePropertyTypes.Get(property.PropertyNameAtomic);
                    if (propitem != null) {
                        return propitem.PropertyDescriptor.PropertyType;
                    }

                    return null;
                }

                if (property is IndexedProperty indexedProp) {
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    if (type is EventType[] eventTypeArray) {
                        var eventType = eventTypeArray[0];
                        return eventType.UnderlyingType;
                    }
                    if (type is TypeBeanOrUnderlying[] typeBeanOrUnderlyingArray) {
                        var innerType = typeBeanOrUnderlyingArray[0].EventType;
                        return innerType.UnderlyingType;
                    }
                    if (type is Type asType) {
                        return asType.GetComponentType();                    }  // remove this
                    return null;
                }

                if (property is MappedProperty mappedProp) {
                    var type = nestableTypes.Get(mappedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    if (type is Type asType) {
                        if (asType.IsGenericStringDictionary()) {
                            return typeof(object);
                        }
                    }

                    return null;
                }
                return null;
            }

            // Map event types allow 2 types of properties inside:
            //   - a property that is a object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters
            // The property getters therefore act on

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap =
                PropertyParser.UnescapeBacktickForProperty(StringValue.UnescapeDot(propertyName.Substring(0, index)));
            var propertyNested = propertyName.Substring(index + 1);
            var isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyMap.EndsWith("?")) {
                propertyMap = propertyMap.Substring(0, propertyMap.Length - 1);
                isRootedDynamic = true;
            }

            var nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null) {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty indexedProp) {
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    // handle map-in-map case
                    if (type is TypeBeanOrUnderlying[] typeBeanOrUnderlyingArray) {
                        var innerType = typeBeanOrUnderlyingArray[0].EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        return innerType.GetPropertyType(propertyNested);
                    }

                    if (type is EventType[] eventTypeArray) {
                        // handle eventType[] in map
                        var innerType = eventTypeArray[0];
                        return innerType.GetPropertyType(propertyNested);
                    }
                    if (type is Type asType) {
                        // handle array class in map case
                        var componentType = asType.GetComponentType();
                        if (componentType != null) {
                            var beanEventType = beanEventTypeFactory.GetCreateBeanType(componentType, publicFields);
                            return beanEventType.GetPropertyType(propertyNested);
                        }

                        return null;
                    }
                    return null;
                }

                if (property is MappedProperty) {
                    return null; // Since no type information is available for the property
                }
                
                return isRootedDynamic ? typeof(object) : null;
            }

            // If there is a map value in the map, return the Object value if this is a dynamic property
            if ((ReferenceEquals(nestedType, typeof(IDictionary<object, object>))) ||
                (ReferenceEquals(nestedType, typeof(IDictionary<string, object>)))) {
                var prop = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                return isRootedDynamic
                    ? typeof(object)
                    : prop.GetPropertyTypeMap(
                            null,
                            beanEventTypeFactory)
                        .GetBoxedType(); // we don't have a definition of the nested props
            }
            
            if (nestedType is IDictionary<string, object> nestedTypes) {
                var prop = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                return isRootedDynamic
                    ? typeof(object)
                    : prop.GetPropertyTypeMap(nestedTypes, beanEventTypeFactory).GetBoxedType();
            }

            if (nestedType is Type simpleClass) {
                if (simpleClass.IsBuiltinDataType()) {
                    return null;
                }

                if (simpleClass.IsArray &&
                    (simpleClass.GetElementType().IsBuiltinDataType() ||
                     simpleClass.GetElementType() == typeof(object))) {
                    return null;
                }

                EventType nestedEventType = beanEventTypeFactory.GetCreateBeanType(simpleClass, publicFields);
                return isRootedDynamic
                    ? typeof(object)
                    : nestedEventType.GetPropertyType(propertyNested).GetBoxedType();
            }

            if (nestedType is EventType innerType1) {
                return isRootedDynamic
                    ? typeof(object)
                    : innerType1.GetPropertyType(propertyNested).GetBoxedType();
            }

            if (nestedType is EventType[]) {
                return null; // requires indexed property
            }

            if (nestedType is TypeBeanOrUnderlying typeBeanOrUnderlying) {
                var innerType = typeBeanOrUnderlying.EventType;
                if (!(innerType is BaseNestableEventType)) {
                    return null;
                }

                return isRootedDynamic
                    ? typeof(object)
                    : innerType.GetPropertyType(propertyNested).GetBoxedType();
            }

            if (nestedType is TypeBeanOrUnderlying[]) {
                return null;
            }

            var message = "Nestable map type configuration encountered an unexpected value type of '" +
                          nestedType.GetType() +
                          "' for property '" +
                          propertyName +
                          "', expected Class, typeof(Map) or Map<String, Object> as value type";
            throw new PropertyAccessException(message);
        }

        public static EventPropertyGetterSPI GetNestableGetter(
            string propertyName,
            IDictionary<string, PropertySetDescriptorItem> propertyGetters,
            IDictionary<string, EventPropertyGetterSPI> propertyGetterCache,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNestableGetterFactory factory,
            bool isObjectArray,
            BeanEventTypeFactory beanEventTypeFactory,
            bool publicFields)
        {
            var cachedGetter = propertyGetterCache.Get(propertyName);
            if (cachedGetter != null) {
                return cachedGetter;
            }

            var unescapePropName = StringValue.UnescapeDot(propertyName);
            var item = propertyGetters.Get(unescapePropName);
            if (item != null) {
                var getter = item.PropertyGetter;
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }

            // see if this is a nested property
            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (prop is SimpleProperty) { // perhaps escaped
                    item = propertyGetters.Get(prop.PropertyNameAtomic);
                    if (item != null) {
                        var getter = item.PropertyGetter;
                        propertyGetterCache.Put(propertyName, getter);
                        return getter;
                    }

                    return null;
                }
                else if (prop is DynamicProperty dynamicProperty) {
                    var getterDyn = factory.GetPropertyDynamicGetter(
                        nestableTypes,
                        propertyName,
                        dynamicProperty,
                        eventBeanTypedEventFactory,
                        beanEventTypeFactory);
                    propertyGetterCache.Put(propertyName, getterDyn);
                    return getterDyn;
                }
                else if (prop is IndexedProperty indexedProp) {
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }
                    else if (type is EventType[]) {
                        var getterArr = factory.GetGetterIndexedEventBean(
                            indexedProp.PropertyNameAtomic,
                            indexedProp.Index);
                        propertyGetterCache.Put(propertyName, getterArr);
                        return getterArr;
                    }
                    else if (type is TypeBeanOrUnderlying underlying) {
                        var innerType = underlying.EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        var typeGetter = factory.GetGetterBeanNested(
                            indexedProp.PropertyNameAtomic,
                            innerType,
                            eventBeanTypedEventFactory);
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else if (type is TypeBeanOrUnderlying[] underlyings) {
                        var innerType = underlyings[0].EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        var typeGetter = factory.GetGetterIndexedUnderlyingArray(
                            indexedProp.PropertyNameAtomic,
                            indexedProp.Index,
                            eventBeanTypedEventFactory,
                            innerType,
                            beanEventTypeFactory);
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else if (type is Type eptype) {
                        if (!eptype.IsArray) {
                            return null;
                        }

                        Type componentType = eptype.GetComponentType();
                        var indexedGetter = factory.GetGetterIndexedClassArray(
                            indexedProp.PropertyNameAtomic,
                            indexedProp.Index,
                            eventBeanTypedEventFactory,
                            componentType,
                            beanEventTypeFactory);
                        propertyGetterCache.Put(propertyName, indexedGetter);
                        return indexedGetter;
                    }
                    else {
                        return null;
                    }
                }
                else if (prop is MappedProperty mappedProp) {
                    var type = nestableTypes.Get(mappedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    if (type is Type asType) {
                        return TypeHelper.IsImplementsInterface(asType, typeof(IDictionary<string, object>))
                            ? factory.GetGetterMappedProperty(mappedProp.PropertyNameAtomic, mappedProp.Key)
                            : null;
                    }

                    return null;
                }
                else {
                    return null;
                }
            }

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap =
                PropertyParser.UnescapeBacktickForProperty(StringValue.UnescapeDot(propertyName.Substring(0, index)));
            var propertyNested = propertyName.Substring(index + 1);
            var isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyMap.EndsWith("?")) {
                propertyMap = propertyMap.Substring(0, propertyMap.Length - 1);
                isRootedDynamic = true;
            }

            var nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null) {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty indexedProp) {
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    if (type is TypeBeanOrUnderlying[] underlyings) {
                        var innerType = (EventTypeSPI)underlyings[0].EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        var innerGetter = innerType.GetGetterSPI(propertyNested);
                        if (innerGetter == null) {
                            return null;
                        }

                        var typeGetter = factory.GetGetterNestedEntryBeanArray(
                            indexedProp.PropertyNameAtomic,
                            indexedProp.Index,
                            innerGetter,
                            innerType,
                            eventBeanTypedEventFactory);
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else if (type is EventType[] types) {
                        var componentType = (EventTypeSPI)types[0];
                        var nestedGetter = componentType.GetGetterSPI(propertyNested);
                        if (nestedGetter == null) {
                            return null;
                        }

                        var typeGetter = factory.GetGetterIndexedEntryEventBeanArrayElement(
                            indexedProp.PropertyNameAtomic,
                            indexedProp.Index,
                            nestedGetter);
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else if (type is Type type1) {
                        if (!type1.IsArray) {
                            return null;
                        }

                        Type componentType = type1.GetComponentType();
                        var nestedEventType =
                            beanEventTypeFactory.GetCreateBeanType(componentType, publicFields);
                        var nestedGetter =
                            (BeanEventPropertyGetter)nestedEventType.GetGetterSPI(propertyNested);
                        if (nestedGetter == null) {
                            return null;
                        }

                        var propertyTypeGetter = nestedEventType.GetPropertyType(propertyNested);
                        // construct getter for nested property
                        var indexGetter = factory.GetGetterIndexedEntryPONO(
                            indexedProp.PropertyNameAtomic,
                            indexedProp.Index,
                            nestedGetter,
                            eventBeanTypedEventFactory,
                            beanEventTypeFactory,
                            propertyTypeGetter);
                        propertyGetterCache.Put(propertyName, indexGetter);
                        return indexGetter;
                    }
                    else {
                        return null;
                    }
                }
                else if (property is MappedProperty) {
                    return null; // Since no type information is available for the property
                }
                else {
                    if (isRootedDynamic) {
                        var prop = PropertyParser.ParseAndWalk(propertyNested, true);
                        if (!isObjectArray) {
                            var getterNested = factory.GetGetterRootedDynamicNested(
                                prop,
                                eventBeanTypedEventFactory,
                                beanEventTypeFactory);
                            var dynamicGetter = factory.GetGetterNestedPropertyProvidedGetterDynamic(
                                nestableTypes,
                                propertyMap,
                                getterNested,
                                eventBeanTypedEventFactory);
                            propertyGetterCache.Put(propertyName, dynamicGetter);
                            return dynamicGetter;
                        }

                        return null;
                    }

                    return null;
                }
            }

            // The map contains another map, we resolve the property dynamically
            if (nestedType is Type nestedType1 && nestedType1 == typeof(IDictionary<string, object>)) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyNested);
                var getterNestedMap = prop.GetGetterMap(
                    null,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
                if (getterNestedMap == null) {
                    return null;
                }

                var mapGetter = factory.GetGetterNestedMapProp(propertyMap, getterNestedMap);
                propertyGetterCache.Put(propertyName, mapGetter);
                return mapGetter;
            }
            else if (nestedType is IDictionary<string, object> nestedTypes) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyNested);
                var getterNestedMap = prop.GetGetterMap(
                    nestedTypes,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
                if (getterNestedMap == null) {
                    return null;
                }

                var mapGetter = factory.GetGetterNestedMapProp(propertyMap, getterNestedMap);
                propertyGetterCache.Put(propertyName, mapGetter);
                return mapGetter;
            }
            else if (nestedType is Type simpleClass) {
                // ask the nested class to resolve the property
                if (simpleClass.IsArray) {
                    return null;
                }

                var nestedEventType = beanEventTypeFactory.GetCreateBeanType(simpleClass, publicFields);
                var nestedGetter =
                    (BeanEventPropertyGetter)nestedEventType.GetGetterSPI(propertyNested);
                if (nestedGetter == null) {
                    return null;
                }

                Type propertyType;
                var desc = nestedEventType.GetPropertyDescriptor(propertyNested);
                if (desc == null) {
                    propertyType = nestedEventType.GetPropertyType(propertyNested);
                }
                else {
                    propertyType = desc.PropertyType;
                }

                // construct getter for nested property
                var getter = factory.GetGetterNestedPONOProp(
                    propertyMap,
                    nestedGetter,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory,
                    propertyType);
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            else if (nestedType is EventType) {
                // ask the nested class to resolve the property
                var innerType = (EventTypeSPI)nestedType;
                var nestedGetter = innerType.GetGetterSPI(propertyNested);
                if (nestedGetter == null) {
                    return null;
                }

                // construct getter for nested property
                var getter = factory.GetGetterNestedEventBean(propertyMap, nestedGetter);
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            else if (nestedType is EventType[] typeArray) {
                var beanArrGetter = factory.GetGetterEventBeanArray(propertyMap, typeArray[0]);
                propertyGetterCache.Put(propertyName, beanArrGetter);
                return beanArrGetter;
            }
            else if (nestedType is TypeBeanOrUnderlying underlying) {
                var innerType = underlying.EventType;
                if (!(innerType is BaseNestableEventType)) {
                    return null;
                }

                var innerGetter = ((EventTypeSPI)innerType).GetGetterSPI(propertyNested);
                if (innerGetter == null) {
                    return null;
                }

                var outerGetter = factory.GetGetterNestedEntryBean(
                    propertyMap,
                    innerGetter,
                    innerType,
                    eventBeanTypedEventFactory);
                propertyGetterCache.Put(propertyName, outerGetter);
                return outerGetter;
            }
            else if (nestedType is TypeBeanOrUnderlying[] underlyings) {
                var innerType = underlyings[0].EventType;
                if (!(innerType is BaseNestableEventType)) {
                    return null;
                }

                var innerGetter = ((EventTypeSPI)innerType).GetGetterSPI(propertyNested);
                if (innerGetter == null) {
                    return null;
                }

                var outerGetter = factory.GetGetterNestedEntryBeanArray(
                    propertyMap,
                    0,
                    innerGetter,
                    innerType,
                    eventBeanTypedEventFactory);
                propertyGetterCache.Put(propertyName, outerGetter);
                return outerGetter;
            }
            else {
                var message = "Nestable type configuration encountered an unexpected value type of '" +
                              nestedType.GetType() +
                              " for property '" +
                              propertyName +
                              "', expected Class, Map.class or Map<String, Object> as value type";
                throw new PropertyAccessException(message);
            }
        }

        public static LinkedHashMap<string, object> ValidateObjectArrayDef(
            string[] propertyNames,
            object[] propertyTypes)
        {
            if (propertyNames.Length != propertyTypes.Length) {
                throw new ConfigurationException(
                    "Number of property names and property types do not match, found " +
                    propertyNames.Length +
                    " property names and " +
                    propertyTypes.Length +
                    " property types");
            }

            // validate property names for no-duplicates
            ISet<string> propertyNamesSet = new HashSet<string>();
            var propertyTypesMap = new LinkedHashMap<string, object>();
            for (var i = 0; i < propertyNames.Length; i++) {
                var propertyName = propertyNames[i];
                if (propertyNamesSet.Contains(propertyName)) { // duplicate prop check
                    throw new ConfigurationException(
                        "Property '" + propertyName + "' is listed twice in the type definition");
                }

                propertyNamesSet.Add(propertyName);
                propertyTypesMap.Put(propertyName, propertyTypes[i]);
            }

            return propertyTypesMap;
        }

        public static WriteablePropertyDescriptor FindWritable(
            string propertyName,
            ISet<WriteablePropertyDescriptor> writables)
        {
            foreach (var writable in writables) {
                if (writable.PropertyName.Equals(propertyName)) {
                    return writable;
                }
            }

            return null;
        }

        public static TimestampPropertyDesc ValidatedDetermineTimestampProps(
            EventType type,
            string startProposed,
            string endProposed,
            IList<EventType> superTypes)
        {
            // determine start&end timestamp as inherited
            var startTimestampPropertyName = startProposed;
            var endTimestampPropertyName = endProposed;

            if (superTypes != null && superTypes.Count > 0) {
                foreach (var superType in superTypes) {
                    if (superType.StartTimestampPropertyName != null) {
                        if (startTimestampPropertyName != null &&
                            !startTimestampPropertyName.Equals(superType.StartTimestampPropertyName)) {
                            throw GetExceptionTimestampInherited(
                                "start",
                                startTimestampPropertyName,
                                superType.StartTimestampPropertyName,
                                superType);
                        }

                        startTimestampPropertyName = superType.StartTimestampPropertyName;
                    }

                    if (superType.EndTimestampPropertyName != null) {
                        if (endTimestampPropertyName != null &&
                            !endTimestampPropertyName.Equals(superType.EndTimestampPropertyName)) {
                            throw GetExceptionTimestampInherited(
                                "end",
                                endTimestampPropertyName,
                                superType.EndTimestampPropertyName,
                                superType);
                        }

                        endTimestampPropertyName = superType.EndTimestampPropertyName;
                    }
                }
            }

            ValidateTimestampProperties(type, startTimestampPropertyName, endTimestampPropertyName);
            return new TimestampPropertyDesc(startTimestampPropertyName, endTimestampPropertyName);
        }

        private static EPException GetExceptionTimestampInherited(
            string tstype,
            string firstName,
            string secondName,
            EventType superType)
        {
            var message = "Event type declares " +
                          tstype +
                          " timestamp as property '" +
                          firstName +
                          "' however inherited event type '" +
                          superType.Name +
                          "' declares " +
                          tstype +
                          " timestamp as property '" +
                          secondName +
                          "'";
            return new EPException(message);
        }

        private static void AddRecursiveSupertypes(
            ISet<EventType> superTypes,
            EventType child)
        {
            if (child.SuperTypes != null) {
                for (var i = 0; i < child.SuperTypes.Count; i++) {
                    superTypes.Add(child.SuperTypes[i]);
                    AddRecursiveSupertypes(superTypes, child.SuperTypes[i]);
                }
            }
        }

        public static string DisallowedAtTypeMessage()
        {
            return "The @type annotation is only allowed when the invocation target returns EventBean instances";
        }

        public static void ValidateEventBeanClassVisibility(Type clazz)
        {
            if (!clazz.IsPublic) {
                throw new EventAdapterException("Event class '" + clazz.Name + "' does not have public visibility");
            }
        }

        public static EventPropertyGetterSPI[] GetGetters(
            EventType eventType,
            string[] props)
        {
            var getters = new EventPropertyGetterSPI[props.Length];
            var spi = (EventTypeSPI)eventType;
            for (var i = 0; i < getters.Length; i++) {
                getters[i] = spi.GetGetterSPI(props[i]);
            }

            return getters;
        }

        public static Type[] GetPropertyTypes(
            EventType eventType,
            string[] props)
        {
            var types = new Type[props.Length];
            for (var i = 0; i < props.Length; i++) {
                types[i] = eventType.GetPropertyType(props[i]);
            }

            return types;
        }

        public static EventType[] ShiftRight(EventType[] src)
        {
            var types = new EventType[src.Length + 1];
            Array.Copy(src, 0, types, 1, src.Length);
            return types;
        }

        public static string GetAdapterForMethodName(EventType eventType)
        {
            if (eventType is MapEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDMAP;
            }

            if (eventType is ObjectArrayEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDOBJECTARRAY;
            }

            if (eventType is BeanEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDBEAN;
            }

            if (eventType is BaseXMLEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDDOM;
            }

            if (eventType is AvroSchemaEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDAVRO;
            }

            if (eventType is JsonEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDJSON;
            }

            if (eventType is WrapperEventType) {
                return EventBeanTypedEventFactoryConstants.ADAPTERFORTYPEDWRAPPER;
            }

            throw new ArgumentException("Unrecognized event type " + eventType);
        }

        public static string GetMessageExpecting(
            string eventTypeName,
            EventType existingType,
            string typeOfEventType)
        {
            var message = "Event type named '" +
                          eventTypeName +
                          "' has not been defined or is not a " +
                          typeOfEventType +
                          " event type";
            if (existingType != null) {
                message += ", the name '" +
                           eventTypeName +
                           "' refers to a " +
                           existingType.UnderlyingType.CleanName() +
                           " event type";
            }
            else {
                message += ", the name '" + eventTypeName + "' has not been defined as an event type";
            }

            return message;
        }

        public static void ValidateTypeObjectArray(
            string eventTypeName,
            EventType type)
        {
            if (!(type is ObjectArrayEventType)) {
                throw new EPException(GetMessageExpecting(eventTypeName, type, "Object-array"));
            }
        }

        public static void ValidateTypeBean(
            string eventTypeName,
            EventType type)
        {
            if (!(type is BeanEventType)) {
                throw new EPException(GetMessageExpecting(eventTypeName, type, "Bean-type"));
            }
        }

        public static void ValidateTypeMap(
            string eventTypeName,
            EventType type)
        {
            if (!(type is MapEventType)) {
                throw new EPException(GetMessageExpecting(eventTypeName, type, "Map-type"));
            }
        }

        public static void ValidateTypeJson(
            string eventTypeName,
            EventType type)
        {
            if (!(type is JsonEventType)) {
                throw new EPException(GetMessageExpecting(eventTypeName, type, "Json-type"));
            }
        }

        public static void ValidateTypeXMLDOM(
            string eventTypeName,
            EventType type)
        {
            if (!(type is BaseXMLEventType)) {
                throw new EPException(GetMessageExpecting(eventTypeName, type, "XML-DOM-type"));
            }
        }

        public static void ValidateTypeAvro(
            string eventTypeName,
            EventType type)
        {
            if (!(type is AvroSchemaEventType)) {
                throw new EPException(GetMessageExpecting(eventTypeName, type, "Avro"));
            }
        }

        public static void ValidateModifiers(
            string eventTypeName,
            EventTypeBusModifier eventBusVisibility,
            NameAccessModifier nameAccessModifier)
        {
            if (eventBusVisibility != EventTypeBusModifier.BUS) {
                return;
            }

            if (nameAccessModifier != NameAccessModifier.PRECONFIGURED &&
                nameAccessModifier != NameAccessModifier.PUBLIC) {
                throw new ExprValidationException(
                    "Event type '" +
                    eventTypeName +
                    "' with bus-visibility requires the public access modifier for the event type");
            }
        }

        public static EventBean[] TypeCast(
            IList<EventBean> events,
            EventType targetType,
            EventBeanTypedEventFactory eventAdapterService,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            var convertedArray = new EventBean[events.Count];
            var count = 0;
            foreach (var theEvent in events) {
                EventBean converted;
                if (theEvent is DecoratingEventBean wrapper) {
                    if (targetType is MapEventType) {
                        IDictionary<string, object> props = new Dictionary<string, object>();
                        props.PutAll(wrapper.DecoratingProperties);
                        foreach (var propDesc in wrapper.UnderlyingEvent.EventType
                                     .PropertyDescriptors) {
                            props.Put(propDesc.PropertyName, wrapper.UnderlyingEvent.Get(propDesc.PropertyName));
                        }

                        converted = eventAdapterService.AdapterForTypedMap(props, targetType);
                    }
                    else {
                        converted = eventAdapterService.AdapterForTypedWrapper(
                            wrapper.UnderlyingEvent,
                            wrapper.DecoratingProperties,
                            targetType);
                    }
                }
                else if (theEvent.EventType is MapEventType && targetType is MapEventType) {
                    var mapEvent = (MappedEventBean)theEvent;
                    converted = eventAdapterService.AdapterForTypedMap(mapEvent.Properties, targetType);
                }
                else if (theEvent.EventType is MapEventType && targetType is WrapperEventType) {
                    converted = eventAdapterService.AdapterForTypedWrapper(theEvent, EmptyDictionary<string, object>.Instance, targetType);
                }
                else if (theEvent.EventType is BeanEventType && targetType is BeanEventType) {
                    converted = eventAdapterService.AdapterForTypedObject(theEvent.Underlying, targetType);
                }
                else if (theEvent.EventType is ObjectArrayEventType && targetType is ObjectArrayEventType type) {
                    var convertedObjectArray = ObjectArrayEventType.ConvertEvent(theEvent, type);
                    converted = eventAdapterService.AdapterForTypedObjectArray(convertedObjectArray, type);
                }
                else if (theEvent.EventType is AvroSchemaEventType && targetType is AvroSchemaEventType eventType) {
                    var convertedGenericRecord = eventTypeAvroHandler.ConvertEvent(theEvent, eventType);
                    converted = eventAdapterService.AdapterForTypedAvro(convertedGenericRecord, eventType);
                }
                else if (theEvent.EventType is JsonEventType && targetType is JsonEventType jsonEventType) {
                    var und = ConvertJsonEvents(theEvent, jsonEventType);
                    converted = eventAdapterService.AdapterForTypedJson(und, targetType);
                }
                else {
                    throw new EPException("Unknown event type " + theEvent.EventType);
                }

                convertedArray[count] = converted;
                count++;
            }

            return convertedArray;
        }

        private static object ConvertJsonEvents(
            EventBean theEvent,
            JsonEventType targetType)
        {
            var target = targetType.Delegate.Allocate();
            var source = theEvent.Underlying;
            var sourceType = (JsonEventType)theEvent.EventType;
            foreach (var entry in targetType.Detail.FieldDescriptors) {
                JsonUnderlyingField sourceField = entry.Value;
                var targetField = sourceType.Detail.FieldDescriptors.Get(entry.Key);
                if (targetField == null) {
                    continue;
                }

                sourceType.Delegate.TryGetProperty(sourceField.PropertyNumber, source, out var value);
                targetType.Delegate.TrySetProperty(targetField.PropertyNumber, value, target);
            }

            return target;
        }

        public static EventTypeForgeablesPair CreateNonVariantType(
            bool isAnonymous,
            CreateSchemaDesc spec,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            if (spec.AssignedType == AssignedType.VARIANT) {
                throw new IllegalStateException("Variant type is not allowed in this context");
            }

            var annotations = @base.StatementRawInfo.Annotations;

            NameAccessModifier visibility;
            EventTypeBusModifier eventBusVisibility;
            if (isAnonymous) {
                visibility = NameAccessModifier.TRANSIENT;
                eventBusVisibility = EventTypeBusModifier.NONBUS;
            }
            else {
                visibility = services.ModuleVisibilityRules.GetAccessModifierEventType(
                    @base.StatementRawInfo,
                    spec.SchemaName);
                eventBusVisibility =
                    services.ModuleVisibilityRules.GetBusModifierEventType(@base.StatementRawInfo, spec.SchemaName);
                ValidateModifiers(spec.SchemaName, eventBusVisibility, visibility);
            }

            EventType eventType;
            IList<StmtClassForgeableFactory> additionalForgeables = EmptyList<StmtClassForgeableFactory>.Instance;
            if (spec.Types.IsEmpty() && spec.AssignedType != AssignedType.XML) {
                var representation = EventRepresentationUtil.GetRepresentation(
                    annotations,
                    services.Configuration,
                    spec.AssignedType);
                IDictionary<string, object> typing = EventTypeUtility.BuildType(
                    spec.Columns,
                    spec.CopyFrom,
                    services.ImportServiceCompileTime,
                    services.ClassProvidedExtension,
                    services.EventTypeCompileTimeResolver);
                var compiledTyping =
                    CompileMapTypeProperties(typing, services.EventTypeCompileTimeResolver);

                ConfigurationCommonEventTypeWithSupertype config;
                if (representation == EventUnderlyingType.MAP) {
                    config = new ConfigurationCommonEventTypeMap();
                }
                else if (representation == EventUnderlyingType.OBJECTARRAY) {
                    config = new ConfigurationCommonEventTypeObjectArray();
                }
                else if (representation == EventUnderlyingType.AVRO) {
                    config = new ConfigurationCommonEventTypeAvro();
                }
                else if (representation == EventUnderlyingType.JSON) {
                    config = new ConfigurationCommonEventTypeJson();
                }
                else {
                    throw new IllegalStateException("Unrecognized representation '" + representation + "'");
                }

                if (spec.Inherits != null) {
                    config.SuperTypes.AddAll(spec.Inherits);
                    if (spec.Inherits.Count > 1 &&
                        (representation == EventUnderlyingType.OBJECTARRAY ||
                         representation == EventUnderlyingType.JSON)) {
                        throw new ExprValidationException(ConfigurationCommonEventTypeObjectArray.SINGLE_SUPERTYPE_MSG);
                    }
                }

                config.StartTimestampPropertyName = spec.StartTimestampProperty;
                config.EndTimestampPropertyName = spec.EndTimestampProperty;

                Func<EventTypeApplicationType, EventTypeMetadata> metadataFunc = appType => new EventTypeMetadata(
                    spec.SchemaName,
                    @base.ModuleName,
                    EventTypeTypeClass.STREAM,
                    appType,
                    visibility,
                    eventBusVisibility,
                    false,
                    EventTypeIdPair.Unassigned());
                if (representation == EventUnderlyingType.MAP) {
                    var st = GetSuperTypesDepthFirst(
                        config.SuperTypes,
                        EventUnderlyingType.MAP,
                        services.EventTypeCompileTimeResolver);
                    var metadata = metadataFunc.Invoke(EventTypeApplicationType.MAP);
                    eventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                        metadata,
                        compiledTyping,
                        st.First,
                        st.Second,
                        config.StartTimestampPropertyName,
                        config.EndTimestampPropertyName,
                        services.BeanEventTypeFactoryPrivate,
                        services.EventTypeCompileTimeResolver);
                }
                else if (representation == EventUnderlyingType.OBJECTARRAY) {
                    var st = GetSuperTypesDepthFirst(
                        config.SuperTypes,
                        EventUnderlyingType.OBJECTARRAY,
                        services.EventTypeCompileTimeResolver);
                    var metadata = metadataFunc.Invoke(EventTypeApplicationType.OBJECTARR);
                    eventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                        metadata,
                        compiledTyping,
                        st.First,
                        st.Second,
                        config.StartTimestampPropertyName,
                        config.EndTimestampPropertyName,
                        services.BeanEventTypeFactoryPrivate,
                        services.EventTypeCompileTimeResolver);
                }
                else if (representation == EventUnderlyingType.AVRO) {
                    var avroSuperTypes = GetSuperTypesDepthFirst(
                        config.SuperTypes,
                        EventUnderlyingType.AVRO,
                        services.EventTypeCompileTimeResolver);
                    var metadata = metadataFunc.Invoke(EventTypeApplicationType.AVRO);
                    eventType = services.EventTypeAvroHandler.NewEventTypeFromNormalized(
                        metadata,
                        services.EventTypeCompileTimeResolver,
                        services.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory,
                        compiledTyping,
                        annotations,
                        (ConfigurationCommonEventTypeAvro)config,
                        avroSuperTypes.First,
                        avroSuperTypes.Second,
                        @base.StatementName);
                }
                else if (representation == EventUnderlyingType.JSON) {
                    var st = GetSuperTypesDepthFirst(
                        config.SuperTypes,
                        EventUnderlyingType.JSON,
                        services.EventTypeCompileTimeResolver);
                    var metadata = metadataFunc.Invoke(EventTypeApplicationType.JSON);
                    var desc = JsonEventTypeUtility.MakeJsonTypeCompileTimeNewType(
                        metadata,
                        compiledTyping,
                        st,
                        config,
                        @base.StatementRawInfo,
                        services);
                    eventType = desc.EventType;
                    additionalForgeables = desc.AdditionalForgeables;
                }
                else {
                    throw new IllegalStateException("Unrecognized representation " + representation);
                }
            }
            else if (spec.AssignedType == AssignedType.XML) {
                if (!spec.Columns.IsEmpty()) {
                    throw new ExprValidationException(
                        "Create-XML-Schema does not allow specifying columns, use @" +
                        typeof(XMLSchemaFieldAttribute).Name +
                        " instead");
                }

                if (!spec.CopyFrom.IsEmpty()) {
                    throw new ExprValidationException("Create-XML-Schema does not allow copy-from");
                }

                if (!spec.Inherits.IsEmpty()) {
                    throw new ExprValidationException("Create-XML-Schema does not allow inherits");
                }

                var config = CreateSchemaXMLHelper.Configure(@base, services);
                SchemaModel schemaModel = null;
                if (config.SchemaResource != null || config.SchemaText != null) {
                    try {
                        schemaModel = services.EventTypeXMLXSDHandler.LoadAndMap(
                            config.SchemaResource,
                            config.SchemaText,
                            services.ImportServiceCompileTime);
                    }
                    catch (Exception ex) {
                        throw new ExprValidationException(ex.Message, ex);
                    }
                }

                var propertyAgnostic = schemaModel == null;
                var metadata = new EventTypeMetadata(
                    spec.SchemaName,
                    @base.ModuleName,
                    EventTypeTypeClass.STREAM,
                    EventTypeApplicationType.XML,
                    visibility,
                    eventBusVisibility,
                    propertyAgnostic,
                    EventTypeIdPair.Unassigned());
                config.StartTimestampPropertyName = spec.StartTimestampProperty;
                config.EndTimestampPropertyName = spec.EndTimestampProperty;
                eventType = EventTypeFactoryImpl
                    .GetInstance(services.Container)
                    .CreateXMLType(
                        metadata,
                        config,
                        schemaModel,
                        null,
                        metadata.Name,
                        services.BeanEventTypeFactoryPrivate,
                        services.XmlFragmentEventTypeFactory,
                        null,
                        services.EventTypeXMLXSDHandler);
            }
            else {
                // Object/Bean/PONO type definition
                if (spec.CopyFrom != null && !spec.CopyFrom.IsEmpty()) {
                    throw new ExprValidationException("Copy-from types are not allowed with class-provided types");
                }

                if (spec.Types.Count != 1) {
                    throw new IllegalStateException("Multiple types provided");
                }

                try {
                    // use the existing configuration, if any, possibly adding the start and end timestamps
                    var name = spec.Types.First();
                    var descriptor = ClassDescriptor.ParseTypeText(name);
                    var clazz =
                        services.ImportServiceCompileTime.ResolveClassForBeanEventType(
                            descriptor.ClassIdentifier);
                    Type parameterized = ImportTypeUtil.ParameterizeType(
                        false,
                        clazz,
                        descriptor,
                        services.ImportServiceCompileTime,
                        services.ClassProvidedExtension);
                    var stem = services.BeanEventTypeStemService.GetCreateStem(parameterized, null);
                    var metadata = new EventTypeMetadata(
                        spec.SchemaName,
                        @base.ModuleName,
                        EventTypeTypeClass.STREAM,
                        EventTypeApplicationType.CLASS,
                        visibility,
                        eventBusVisibility,
                        false,
                        EventTypeIdPair.Unassigned());
                    var superTypes = GetSuperTypes(stem.SuperTypes, services);
                    var deepSuperTypes = GetDeepSupertypes(stem.DeepSuperTypes, services);
                    eventType = new BeanEventType(
                        services.Container,
                        stem,
                        metadata,
                        services.BeanEventTypeFactoryPrivate,
                        superTypes,
                        deepSuperTypes,
                        spec.StartTimestampProperty,
                        spec.EndTimestampProperty);
                }
                catch (ImportException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }
            }

            services.EventTypeCompileTimeRegistry.NewType(eventType);
            return new EventTypeForgeablesPair(eventType, additionalForgeables);
        }

        private static bool AllowPopulate(EventTypeSPI typeSPI)
        {
            var metadata = typeSPI.Metadata;
            if (metadata.TypeClass == EventTypeTypeClass.STREAM ||
                metadata.TypeClass == EventTypeTypeClass.APPLICATION) {
                return true;
            }

            if (metadata.TypeClass != EventTypeTypeClass.STATEMENTOUT &&
                metadata.TypeClass != EventTypeTypeClass.TABLE_INTERNAL) {
                return false;
            }

            return true;
        }

        private static EventType[] GetSuperTypes(
            Type[] superTypes,
            StatementCompileTimeServices services)
        {
            if (superTypes == null || superTypes.Length == 0) {
                return null;
            }

            var types = new EventType[superTypes.Length];
            for (var i = 0; i < types.Length; i++) {
                types[i] = ResolveOrCreateType(superTypes[i], services);
            }

            return types;
        }

        private static ISet<EventType> GetDeepSupertypes(
            ISet<Type> superTypes,
            StatementCompileTimeServices services)
        {
            if (superTypes == null || superTypes.IsEmpty()) {
                return Collections.GetEmptySet<EventType>();
            }

            var supers = new LinkedHashSet<EventType>();
            foreach (var clazz in superTypes) {
                supers.Add(ResolveOrCreateType(clazz, services));
            }

            return supers;
        }

        private static EventType ResolveOrCreateType(
            Type clazz,
            StatementCompileTimeServices services)
        {
            var eventTypeCompileTimeResolver = services.EventTypeCompileTimeResolver;

            // find module-own type
            var moduleTypes = services.EventTypeCompileTimeRegistry.NewTypesAdded;
            foreach (var eventType in moduleTypes) {
                if (Matches(eventType, clazz)) {
                    return eventType;
                }
            }

            // find path types
            var pathRegistry = services.EventTypeCompileTimeResolver.Path;
            IList<EventType> found = new List<EventType>();
            pathRegistry.Traverse(
                eventType => {
                    if (Matches(eventType, clazz)) {
                        found.Add(eventType);
                    }
                });
            if (found.Count > 1) {
                throw new EPException("Found multiple parent types in path for classs '" + clazz + "'");
            }

            if (!found.IsEmpty()) {
                return found[0];
            }

            return services.BeanEventTypeFactoryPrivate.GetCreateBeanType(clazz, false);
        }

        private static bool Matches(
            EventType eventType,
            Type clazz)
        {
            if (!(eventType is BeanEventType beanEventType)) {
                return false;
            }

            return beanEventType.Stem.Clazz == clazz;
        }

        public static EventBeanAdapterFactory GetAdapterFactoryForType(
            EventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            if (eventType is BeanEventType) {
                return new EventBeanAdapterFactoryBean(eventType, eventBeanTypedEventFactory);
            }

            if (eventType is ObjectArrayEventType) {
                return new EventBeanAdapterFactoryObjectArray(eventType, eventBeanTypedEventFactory);
            }

            if (eventType is MapEventType) {
                return new EventBeanAdapterFactoryMap(eventType, eventBeanTypedEventFactory);
            }

            if (eventType is BaseXMLEventType) {
                return new EventBeanAdapterFactoryXml(eventType, eventBeanTypedEventFactory);
            }

            if (eventType is AvroSchemaEventType) {
                return new EventBeanAdapterFactoryAvro(eventType, eventTypeAvroHandler);
            }

            if (eventType is JsonEventType) {
                return new EventBeanAdapterFactoryJson(eventType, eventBeanTypedEventFactory);
            }

            if (eventType is WrapperEventType wrapperEventType) {
                var factoryWrapped = GetAdapterFactoryForType(
                    wrapperEventType.UnderlyingEventType,
                    eventBeanTypedEventFactory,
                    eventTypeAvroHandler);
                if (wrapperEventType.IsNoMapProperties) {
                    return new EventBeanAdapterFactoryWrapperNoProps(
                        wrapperEventType,
                        eventBeanTypedEventFactory,
                        factoryWrapped);
                }

                return new EventBeanAdapterFactoryWrapperWithProps(
                    wrapperEventType,
                    eventBeanTypedEventFactory,
                    factoryWrapped);
            }

            throw new EventAdapterException("Event type '" + eventType.Name + "' is not a runtime-native event type");
        }
    }
} // end of namespace