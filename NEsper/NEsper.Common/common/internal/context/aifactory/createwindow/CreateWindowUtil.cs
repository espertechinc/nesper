///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
	public class CreateWindowUtil {
	    // The create window command:
	    //      create window windowName[.window_view_list] as [select properties from] type
	    //
	    // This section expected s single FilterStreamSpecCompiled representing the selected type.
	    // It creates a new event type representing the window type and a sets the type selected on the filter stream spec.
	    protected internal static CreateWindowCompileResult HandleCreateWindow(StatementBaseInfo @base,
	                                                                  StatementCompileTimeServices services)
	            {

	        CreateWindowDesc createWindowDesc = @base.StatementSpec.Raw.CreateWindowDesc;
	        IList<ColumnDesc> columns = createWindowDesc.Columns;
	        string typeName = createWindowDesc.WindowName;
	        EventType targetType;

	        // determine that the window name is not already in use as an event type name
	        EventType existingType = services.EventTypeCompileTimeResolver.GetTypeByName(typeName);
	        if (existingType != null && existingType.Metadata.TypeClass != EventTypeTypeClass.NAMED_WINDOW) {
	            throw new ExprValidationException("Error starting statement: An event type or schema by name '" + typeName + "' already exists");
	        }

	        // Determine select-from
	        SelectFromInfo optionalSelectFrom = CreateWindowUtil.GetOptionalSelectFrom(createWindowDesc, services);

	        // Create Map or Wrapper event type from the select clause of the window.
	        // If no columns selected, simply create a wrapper type
	        // Build a list of properties
	        SelectClauseSpecRaw newSelectClauseSpecRaw = new SelectClauseSpecRaw();
	        LinkedHashMap<string, object> properties;
	        bool hasProperties = false;
	        if ((columns != null) && (!columns.IsEmpty())) {
	            properties = EventTypeUtility.BuildType(columns, null, services.ImportServiceCompileTime, services.EventTypeCompileTimeResolver);
	            hasProperties = true;
	        } else {
	            if (optionalSelectFrom == null) {
	                throw new IllegalStateException("Missing from-type information for create-window");
	            }
	            // Validate the select expressions which consists of properties only
	            IList<NamedWindowSelectedProps> select = CompileLimitedSelect(optionalSelectFrom, @base, services);

	            properties = new LinkedHashMap<>();
	            foreach (NamedWindowSelectedProps selectElement in select) {
	                if (selectElement.FragmentType != null) {
	                    properties.Put(selectElement.AssignedName, selectElement.FragmentType);
	                } else {
	                    properties.Put(selectElement.AssignedName, selectElement.SelectExpressionType);
	                }

	                // Add any properties to the new select clause for use by consumers to the statement itself
	                newSelectClauseSpecRaw.Add(new SelectClauseExprRawSpec(new ExprIdentNodeImpl(selectElement.AssignedName), null, false));
	                hasProperties = true;
	            }
	        }

	        // Create Map or Wrapper event type from the select clause of the window.
	        // If no columns selected, simply create a wrapper type
	        bool isOnlyWildcard = @base.StatementSpec.Raw.SelectClauseSpec.IsOnlyWildcard;
	        bool isWildcard = @base.StatementSpec.Raw.SelectClauseSpec.IsUsingWildcard;
	        NameAccessModifier namedWindowVisibility = services.ModuleVisibilityRules.GetAccessModifierNamedWindow(@base, typeName);
	        try {
	            if (isWildcard && !isOnlyWildcard) {
	                EventTypeMetadata metadata = new EventTypeMetadata(typeName, @base.ModuleName, EventTypeTypeClass.NAMED_WINDOW, EventTypeApplicationType.WRAPPER, namedWindowVisibility, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
	                targetType = WrapperEventTypeUtil.MakeWrapper(metadata, optionalSelectFrom.EventType, properties, null, services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
	            } else {
	                // Some columns selected, use the types of the columns
	                Function<EventTypeApplicationType, EventTypeMetadata> metadata = type => new EventTypeMetadata(typeName, @base.ModuleName, EventTypeTypeClass.NAMED_WINDOW, type, namedWindowVisibility, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());

	                if (hasProperties && !isOnlyWildcard) {
	                    IDictionary<string, object> compiledProperties = EventTypeUtility.CompileMapTypeProperties(properties, services.EventTypeCompileTimeResolver);
	                    EventUnderlyingType representation = EventRepresentationUtil.GetRepresentation(@base.StatementSpec.Annotations, services.Configuration, CreateSchemaDesc.AssignedType.NONE);

	                    if (representation == EventUnderlyingType.MAP) {
	                        targetType = BaseNestableEventUtil.MakeMapTypeCompileTime(metadata.Apply(EventTypeApplicationType.MAP), compiledProperties, null, null, null, null, services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
	                    } else if (representation == EventUnderlyingType.OBJECTARRAY) {
	                        targetType = BaseNestableEventUtil.MakeOATypeCompileTime(metadata.Apply(EventTypeApplicationType.OBJECTARR), compiledProperties, null, null, null, null, services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
	                    } else if (representation == EventUnderlyingType.AVRO) {
	                        targetType = services.EventTypeAvroHandler.NewEventTypeFromNormalized(metadata.Apply(EventTypeApplicationType.AVRO), services.EventTypeCompileTimeResolver, services.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory, compiledProperties, @base.StatementRawInfo.Annotations, null, null, null, @base.StatementName);
	                    } else {
	                        throw new IllegalStateException("Unrecognized representation " + representation);
	                    }
	                } else {
	                    if (optionalSelectFrom == null) {
	                        throw new IllegalStateException("Missing from-type information for create-window");
	                    }
	                    EventType selectFromType = optionalSelectFrom.EventType;

	                    // No columns selected, no wildcard, use the type as is or as a wrapped type
	                    if (selectFromType is ObjectArrayEventType) {
	                        ObjectArrayEventType oaType = (ObjectArrayEventType) selectFromType;
	                        targetType = BaseNestableEventUtil.MakeOATypeCompileTime(metadata.Apply(EventTypeApplicationType.OBJECTARR), oaType.Types, null, null, oaType.StartTimestampPropertyName, oaType.EndTimestampPropertyName, services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
	                    } else if (selectFromType is AvroSchemaEventType) {
	                        AvroSchemaEventType avroSchemaEventType = (AvroSchemaEventType) selectFromType;
	                        ConfigurationCommonEventTypeAvro avro = new ConfigurationCommonEventTypeAvro();
	                        avro.AvroSchema = avroSchemaEventType.Schema;
	                        targetType = services.EventTypeAvroHandler.NewEventTypeFromSchema(metadata.Apply(EventTypeApplicationType.AVRO), services.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory, avro, null, null);
	                    } else if (selectFromType is MapEventType) {
	                        MapEventType mapType = (MapEventType) selectFromType;
	                        targetType = BaseNestableEventUtil.MakeMapTypeCompileTime(metadata.Apply(EventTypeApplicationType.MAP), mapType.Types, null, null, mapType.StartTimestampPropertyName, mapType.EndTimestampPropertyName, services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
	                    } else if (selectFromType is BeanEventType) {
	                        BeanEventType beanType = (BeanEventType) selectFromType;
	                        targetType = new BeanEventType(beanType.Stem, metadata.Apply(EventTypeApplicationType.CLASS), services.BeanEventTypeFactoryPrivate, null, null, beanType.StartTimestampPropertyName, beanType.EndTimestampPropertyName);
	                    } else {
	                        targetType = WrapperEventTypeUtil.MakeWrapper(metadata.Apply(EventTypeApplicationType.WRAPPER), selectFromType, new Dictionary<>(), null, services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
	                    }
	                }
	            }
	            services.EventTypeCompileTimeRegistry.NewType(targetType);
	        } catch (EPException ex) {
	            throw new ExprValidationException(ex.Message, ex);
	        }

	        FilterSpecCompiled filter = new FilterSpecCompiled(targetType, typeName, new IList[0], null);
	        return new CreateWindowCompileResult(filter, newSelectClauseSpecRaw, optionalSelectFrom == null ? null : optionalSelectFrom.EventType);
	    }

	    private static IList<NamedWindowSelectedProps> CompileLimitedSelect(SelectFromInfo selectFromInfo, StatementBaseInfo @base, StatementCompileTimeServices compileTimeServices)
	            {
	        IList<NamedWindowSelectedProps> selectProps = new LinkedList<NamedWindowSelectedProps>();
	        StreamTypeService streams = new StreamTypeServiceImpl(new EventType[]{selectFromInfo.EventType}, new string[]{"stream_0"}, new bool[]{false}, false, false);

	        ExprValidationContext validationContext = new ExprValidationContextBuilder(streams, @base.StatementRawInfo, compileTimeServices).Build();
	        foreach (SelectClauseElementCompiled item in @base.StatementSpec.SelectClauseCompiled.SelectExprList) {
	            if (!(item is SelectClauseExprCompiledSpec)) {
	                continue;
	            }
	            SelectClauseExprCompiledSpec exprSpec = (SelectClauseExprCompiledSpec) item;
	            ExprNode validatedExpression = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, exprSpec.SelectExpression, validationContext);

	            // determine an element name if none assigned
	            string asName = exprSpec.ProvidedName;
	            if (asName == null) {
	                asName = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validatedExpression);
	            }

	            // check for fragments
	            EventType fragmentType = null;
	            if ((validatedExpression is ExprIdentNode) && (!(selectFromInfo.EventType is NativeEventType))) {
	                ExprIdentNode identNode = (ExprIdentNode) validatedExpression;
	                FragmentEventType fragmentEventType = selectFromInfo.EventType.GetFragmentType(identNode.FullUnresolvedName);
	                if ((fragmentEventType != null) && (!fragmentEventType.IsNative)) {
	                    fragmentType = fragmentEventType.FragmentType;
	                }
	            }

	            NamedWindowSelectedProps validatedElement = new NamedWindowSelectedProps(validatedExpression.Forge.EvaluationType, asName, fragmentType);
	            selectProps.Add(validatedElement);
	        }

	        return selectProps;
	    }

	    private static SelectFromInfo GetOptionalSelectFrom(CreateWindowDesc createWindowDesc, StatementCompileTimeServices compileTimeServices) {
	        if (createWindowDesc.AsEventTypeName == null) {
	            return null;
	        }
	        EventType eventType = StreamSpecCompiler.ResolveTypeName(createWindowDesc.AsEventTypeName, compileTimeServices.EventTypeCompileTimeResolver);
	        return new SelectFromInfo(eventType, createWindowDesc.AsEventTypeName);
	    }
	}
} // end of namespace