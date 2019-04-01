///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.eval;
using com.espertech.esper.common.@internal.epl.resultset.select.typable;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    /// <summary>
    /// Processor for select-clause expressions that handles a list of selection items represented by
    /// expression nodes. Computes results based on matching events.
    /// </summary>
    public class SelectExprProcessorHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IList<SelectClauseExprCompiledSpec> selectionList;
        private readonly IList<SelectExprStreamDesc> selectedStreams;
        private readonly SelectProcessorArgs args;
        private readonly InsertIntoDesc insertIntoDesc;

        public SelectExprProcessorHelper(
            IList<SelectClauseExprCompiledSpec> selectionList, IList<SelectExprStreamDesc> selectedStreams,
            SelectProcessorArgs args, InsertIntoDesc insertIntoDesc)
        {
            this.selectionList = selectionList;
            this.selectedStreams = selectedStreams;
            this.args = args;
            this.insertIntoDesc = insertIntoDesc;
        }

        public SelectExprProcessorForge Forge {
            get {
                bool isUsingWildcard = args.IsUsingWildcard;
                StreamTypeService typeService = args.TypeService;
                ImportServiceCompileTime importService = args.ImportService;
                BeanEventTypeFactory beanEventTypeFactoryProtected = args.BeanEventTypeFactoryPrivate;
                EventTypeNameGeneratorStatement eventTypeNameGeneratorStatement =
                    args.CompileTimeServices.EventTypeNameGeneratorStatement;
                string moduleName = args.ModuleName;

                // Get the named and un-named stream selectors (i.e. select s0.* from S0 as s0), if any
                IList<SelectClauseStreamCompiledSpec> namedStreams = new List<SelectClauseStreamCompiledSpec>();
                IList<SelectExprStreamDesc> unnamedStreams = new List<SelectExprStreamDesc>();
                foreach (SelectExprStreamDesc spec in selectedStreams) {
                    // handle special "transpose(...)" function
                    if ((spec.StreamSelected != null && spec.StreamSelected.OptionalName == null)
                        ||
                        (spec.ExpressionSelectedAsStream != null)) {
                        unnamedStreams.Add(spec);
                    }
                    else {
                        namedStreams.Add(spec.StreamSelected);
                        if (spec.StreamSelected.IsProperty) {
                            throw new ExprValidationException(
                                "The property wildcard syntax must be used without column name");
                        }
                    }
                }

                // Error if there are more then one un-named streams (i.e. select s0.*, s1.* from S0 as s0, S1 as s1)
                // Thus there is only 1 unnamed stream selector maximum.
                if (unnamedStreams.Count > 1) {
                    throw new ExprValidationException(
                        "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");
                }

                if (selectedStreams.IsEmpty() && selectionList.IsEmpty() && !isUsingWildcard) {
                    throw new ArgumentException("Empty selection list not supported");
                }

                foreach (SelectClauseExprCompiledSpec entry in selectionList) {
                    if (entry.AssignedName == null) {
                        throw new ArgumentException("Expected name for each expression has not been supplied");
                    }
                }

                // Verify insert into clause
                if (insertIntoDesc != null) {
                    VerifyInsertInto(insertIntoDesc, selectionList);
                }

                // Build a subordinate wildcard processor for joins
                SelectExprProcessorForge joinWildcardProcessor = null;
                if (typeService.StreamNames.Length > 1 && isUsingWildcard) {
                    joinWildcardProcessor = SelectExprJoinWildcardProcessorFactory.Create(
                        args, null, eventTypeName => eventTypeName + "_join");
                }

                // Resolve underlying event type in the case of wildcard select
                EventType eventType = null;
                bool singleStreamWrapper = false;
                if (isUsingWildcard) {
                    if (joinWildcardProcessor != null) {
                        eventType = joinWildcardProcessor.ResultEventType;
                    }
                    else {
                        eventType = typeService.EventTypes[0];
                        if (eventType is WrapperEventType) {
                            singleStreamWrapper = true;
                        }
                    }
                }

                // Find if there is any fragments selected
                EventType insertIntoTargetType = null;
                if (insertIntoDesc != null) {
                    if (args.OptionalInsertIntoEventType != null) {
                        insertIntoTargetType = args.OptionalInsertIntoEventType;
                    }
                    else {
                        insertIntoTargetType =
                            args.EventTypeCompileTimeResolver.GetTypeByName(insertIntoDesc.EventTypeName);
                        if (insertIntoTargetType == null) {
                            TableMetaData table = args.TableCompileTimeResolver.Resolve(insertIntoDesc.EventTypeName);
                            if (table != null) {
                                insertIntoTargetType = table.InternalEventType;
                                args.OptionalInsertIntoEventType = insertIntoTargetType;
                            }
                        }
                    }
                }

                // Obtain insert-into per-column type information, when available
                EPType[] insertIntoTargetsPerCol =
                    DetermineInsertedEventTypeTargets(insertIntoTargetType, selectionList);

                // Get expression nodes
                ExprForge[] exprForges = new ExprForge[selectionList.Count];
                ExprNode[] exprNodes = new ExprNode[selectionList.Count];
                object[] expressionReturnTypes = new object[selectionList.Count];
                for (int i = 0; i < selectionList.Count; i++) {
                    SelectClauseExprCompiledSpec spec = selectionList[i];
                    ExprNode expr = spec.SelectExpression;
                    ExprForge forge = expr.Forge;
                    exprNodes[i] = expr;

                    // if there is insert-into specification, use that
                    if (insertIntoDesc != null) {
                        // handle insert-into, with well-defined target event-typed column, and enumeration
                        TypeAndForgePair pair = HandleInsertIntoEnumeration(
                            spec.ProvidedName, insertIntoTargetsPerCol[i], forge);
                        if (pair != null) {
                            expressionReturnTypes[i] = pair.Type;
                            exprForges[i] = pair.Forge;
                            continue;
                        }

                        // handle insert-into with well-defined target event-typed column, and typable expression
                        pair = HandleInsertIntoTypableExpression(insertIntoTargetsPerCol[i], forge, args);
                        if (pair != null) {
                            expressionReturnTypes[i] = pair.Type;
                            exprForges[i] = pair.Forge;
                            continue;
                        }
                    }

                    // handle @eventbean annotation, i.e. well-defined type through enumeration
                    TypeAndForgePair pair = HandleAtEventbeanEnumeration(spec.IsEvents, forge);
                    if (pair != null) {
                        expressionReturnTypes[i] = pair.Type;
                        exprForges[i] = pair.Forge;
                        continue;
                    }

                    // handle typeable return, i.e. typable multi-column return without provided target type
                    pair = HandleTypableExpression(forge, i, eventTypeNameGeneratorStatement);
                    if (pair != null) {
                        expressionReturnTypes[i] = pair.Type;
                        exprForges[i] = pair.Forge;
                        continue;
                    }

                    // handle select-clause expressions that match group-by expressions with rollup and therefore should be boxed types as rollup can produce a null value
                    if (args.GroupByRollupInfo != null && args.GroupByRollupInfo.RollupDesc != null) {
                        Type returnType = forge.EvaluationType;
                        Type returnTypeBoxed = Boxing.GetBoxedType(returnType);
                        if (returnType != returnTypeBoxed &&
                            IsGroupByRollupNullableExpression(expr, args.GroupByRollupInfo)) {
                            exprForges[i] = forge;
                            expressionReturnTypes[i] = returnTypeBoxed;
                            continue;
                        }
                    }

                    // assign normal expected return type
                    exprForges[i] = forge;
                    expressionReturnTypes[i] = exprForges[i].EvaluationType;
                }

                // Get column names
                string[] columnNames;
                string[] columnNamesAsProvided;
                if ((insertIntoDesc != null) && (!insertIntoDesc.ColumnNames.IsEmpty())) {
                    columnNames = insertIntoDesc.ColumnNames.ToArray();
                    columnNamesAsProvided = columnNames;
                }
                else if (!selectedStreams.IsEmpty()) { // handle stream selection column names
                    int numStreamColumnsJoin = 0;
                    if (isUsingWildcard && typeService.EventTypes.Length > 1) {
                        numStreamColumnsJoin = typeService.EventTypes.Length;
                    }

                    columnNames = new string[selectionList.Count + namedStreams.Count + numStreamColumnsJoin];
                    columnNamesAsProvided = new string[columnNames.Length];
                    int count = 0;
                    foreach (SelectClauseExprCompiledSpec aSelectionList in selectionList) {
                        columnNames[count] = aSelectionList.AssignedName;
                        columnNamesAsProvided[count] = aSelectionList.ProvidedName;
                        count++;
                    }

                    foreach (SelectClauseStreamCompiledSpec aSelectionList in namedStreams) {
                        columnNames[count] = aSelectionList.OptionalName;
                        columnNamesAsProvided[count] = aSelectionList.OptionalName;
                        count++;
                    }

                    // for wildcard joins, add the streams themselves
                    if (isUsingWildcard && typeService.EventTypes.Length > 1) {
                        foreach (string streamName in typeService.StreamNames) {
                            columnNames[count] = streamName;
                            columnNamesAsProvided[count] = streamName;
                            count++;
                        }
                    }
                }
                else {
                    // handle regular column names
                    columnNames = new string[selectionList.Count];
                    columnNamesAsProvided = new string[selectionList.Count];
                    for (int i = 0; i < selectionList.Count; i++) {
                        columnNames[i] = selectionList[i].AssignedName;
                        columnNamesAsProvided[i] = selectionList[i].ProvidedName;
                    }
                }

                // Find if there is any fragment event types:
                // This is a special case for fragments: select a, b from pattern [a=A => b=B]
                // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
                for (int i = 0; i < selectionList.Count; i++) {
                    if (!(exprNodes[i] is ExprIdentNode)) {
                        continue;
                    }

                    ExprIdentNode identNode = (ExprIdentNode) exprNodes[i];
                    string propertyName = identNode.ResolvedPropertyName;
                    int streamNum = identNode.StreamId;

                    EventType eventTypeStream = typeService.EventTypes[streamNum];
                    if (eventTypeStream is NativeEventType) {
                        continue; // we do not transpose the native type for performance reasons
                    }

                    FragmentEventType fragmentType = eventTypeStream.GetFragmentType(propertyName);
                    if ((fragmentType == null) || (fragmentType.IsNative)) {
                        continue; // we also ignore native Java classes as fragments for performance reasons
                    }

                    // may need to unwrap the fragment if the target type has this underlying type
                    FragmentEventType targetFragment = null;
                    if (insertIntoTargetType != null) {
                        targetFragment = insertIntoTargetType.GetFragmentType(columnNames[i]);
                    }

                    if ((insertIntoTargetType != null) &&
                        (fragmentType.FragmentType.UnderlyingType == expressionReturnTypes[i]) &&
                        ((targetFragment == null) || (targetFragment != null && targetFragment.IsNative))) {
                        EventPropertyGetterSPI getter = ((EventTypeSPI) eventTypeStream).GetGetterSPI(propertyName);
                        Type returnType = eventTypeStream.GetPropertyType(propertyName);
                        exprForges[i] = new ExprEvalByGetter(streamNum, getter, returnType);
                    }
                    else if ((insertIntoTargetType != null) && expressionReturnTypes[i] is Type &&
                             (fragmentType.FragmentType.UnderlyingType ==
                              ((Type) expressionReturnTypes[i]).GetElementType()) &&
                             ((targetFragment == null) || (targetFragment != null && targetFragment.IsNative))) {
                        // same for arrays: may need to unwrap the fragment if the target type has this underlying type
                        EventPropertyGetterSPI getter = ((EventTypeSPI) eventTypeStream).GetGetterSPI(propertyName);
                        Type returnType = eventTypeStream.GetPropertyType(propertyName);
                        exprForges[i] = new ExprEvalByGetter(streamNum, getter, returnType);
                    }
                    else {
                        EventPropertyGetterSPI getter = ((EventTypeSPI) eventTypeStream).GetGetterSPI(propertyName);
                        FragmentEventType fragType = eventTypeStream.GetFragmentType(propertyName);
                        Type undType = fragType.FragmentType.UnderlyingType;
                        Type returnType = fragType.IsIndexed ? TypeHelper.GetArrayType(undType) : undType;
                        exprForges[i] = new ExprEvalByGetterFragment(streamNum, getter, returnType, fragmentType);
                        if (!fragmentType.IsIndexed) {
                            expressionReturnTypes[i] = fragmentType.FragmentType;
                        }
                        else {
                            expressionReturnTypes[i] = new EventType[] {fragmentType.FragmentType};
                        }
                    }
                }

                // Find if there is any stream expression (ExprStreamNode) :
                // This is a special case for stream selection: select a, b from A as a, B as b
                // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
                for (int i = 0; i < selectionList.Count; i++) {
                    Pair<ExprForge, object> pair = HandleUnderlyingStreamInsert(exprForges[i]);
                    if (pair != null) {
                        exprForges[i] = pair.First;
                        expressionReturnTypes[i] = pair.Second;
                    }
                }

                // Build event type that reflects all selected properties
                LinkedHashMap<string, object> selPropertyTypes = new LinkedHashMap<string, object>();
                int count = 0;
                for (int i = 0; i < exprForges.Length; i++) {
                    object expressionReturnType = expressionReturnTypes[count];
                    selPropertyTypes.Put(columnNames[count], expressionReturnType);
                    count++;
                }

                if (!selectedStreams.IsEmpty()) {
                    foreach (SelectClauseStreamCompiledSpec element in namedStreams) {
                        EventType eventTypeStream;
                        if (element.TableMetadata != null) {
                            eventTypeStream = element.TableMetadata.PublicEventType;
                        }
                        else {
                            eventTypeStream = typeService.EventTypes[element.StreamNumber];
                        }

                        selPropertyTypes.Put(columnNames[count], eventTypeStream);
                        count++;
                    }

                    if (isUsingWildcard && typeService.EventTypes.Length > 1) {
                        for (int i = 0; i < typeService.EventTypes.Length; i++) {
                            EventType eventTypeStream = typeService.EventTypes[i];
                            selPropertyTypes.Put(columnNames[count], eventTypeStream);
                            count++;
                        }
                    }
                }

                // Handle stream selection
                EventType underlyingEventType = null;
                int underlyingStreamNumber = 0;
                bool underlyingIsFragmentEvent = false;
                EventPropertyGetterSPI underlyingPropertyEventGetter = null;
                ExprForge underlyingExprForge = null;

                if (!selectedStreams.IsEmpty()) {
                    // Resolve underlying event type in the case of wildcard or non-named stream select.
                    // Determine if the we are considering a tagged event or a stream name.
                    if (isUsingWildcard || (!unnamedStreams.IsEmpty())) {
                        if (!unnamedStreams.IsEmpty()) {
                            if (unnamedStreams[0].StreamSelected != null) {
                                SelectClauseStreamCompiledSpec streamSpec = unnamedStreams[0].StreamSelected;

                                // the tag.* syntax for :  select tag.* from pattern [tag = A]
                                underlyingStreamNumber = streamSpec.StreamNumber;
                                if (streamSpec.IsFragmentEvent) {
                                    EventType compositeMap = typeService.EventTypes[underlyingStreamNumber];
                                    FragmentEventType fragment = compositeMap.GetFragmentType(streamSpec.StreamName);
                                    underlyingEventType = fragment.FragmentType;
                                    underlyingIsFragmentEvent = true;
                                }
                                else if (streamSpec.IsProperty) {
                                    // the property.* syntax for :  select property.* from A
                                    string propertyName = streamSpec.StreamName;
                                    Type propertyType = streamSpec.PropertyType;
                                    int streamNumber = streamSpec.StreamNumber;

                                    if (TypeHelper.IsJavaBuiltinDataType(streamSpec.PropertyType)) {
                                        throw new ExprValidationException(
                                            "The property wildcard syntax cannot be used on built-in types as returned by property '" +
                                            propertyName + "'");
                                    }

                                    // create or get an underlying type for that Class
                                    BeanEventTypeStem stem =
                                        args.CompileTimeServices.BeanEventTypeStemService.GetCreateStem(
                                            propertyType, null);
                                    NameAccessModifier visibility = GetVisibility(propertyType.Name);
                                    EventTypeMetadata metadata = new EventTypeMetadata(
                                        propertyType.Name, moduleName, EventTypeTypeClass.STREAM,
                                        EventTypeApplicationType.CLASS, visibility, EventTypeBusModifier.NONBUS, false,
                                        EventTypeIdPair.Unassigned());
                                    underlyingEventType = new BeanEventType(
                                        stem, metadata, beanEventTypeFactoryProtected, null, null, null, null);
                                    args.EventTypeCompileTimeRegistry.NewType(underlyingEventType);
                                    underlyingPropertyEventGetter =
                                        ((EventTypeSPI) typeService.EventTypes[streamNumber])
                                        .GetGetterSPI(propertyName);
                                    if (underlyingPropertyEventGetter == null) {
                                        throw new ExprValidationException(
                                            "Unexpected error resolving property getter for property " + propertyName);
                                    }
                                }
                                else {
                                    // the stream.* syntax for:  select a.* from A as a
                                    underlyingEventType = typeService.EventTypes[underlyingStreamNumber];
                                }
                            }
                            else {
                                // handle case where the unnamed stream is a "transpose" function, for non-insert-into
                                if (insertIntoDesc == null || insertIntoTargetType == null) {
                                    ExprNode expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                                    Type returnType = expression.Forge.EvaluationType;
                                    if (returnType == typeof(object[]) ||
                                        TypeHelper.IsImplementsInterface(
                                            returnType, typeof(IDictionary<object, object>)) ||
                                        TypeHelper.IsJavaBuiltinDataType(returnType)) {
                                        throw new ExprValidationException(
                                            "Invalid expression return type '" + returnType.Name +
                                            "' for transpose function");
                                    }

                                    BeanEventTypeStem stem =
                                        args.CompileTimeServices.BeanEventTypeStemService.GetCreateStem(
                                            returnType, null);
                                    NameAccessModifier visibility = GetVisibility(returnType.Name);
                                    EventTypeMetadata metadata = new EventTypeMetadata(
                                        returnType.Name, moduleName, EventTypeTypeClass.STREAM,
                                        EventTypeApplicationType.CLASS,
                                        visibility, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
                                    underlyingEventType = new BeanEventType(
                                        stem, metadata, beanEventTypeFactoryProtected, null, null, null, null);
                                    underlyingExprForge = expression.Forge;
                                    args.EventTypeCompileTimeRegistry.NewType(underlyingEventType);
                                }
                            }
                        }
                        else {
                            // no un-named stream selectors, but a wildcard was specified
                            if (typeService.EventTypes.Length == 1) {
                                // not a join, we are using the selected event
                                underlyingEventType = typeService.EventTypes[0];
                                if (underlyingEventType is WrapperEventType) {
                                    singleStreamWrapper = true;
                                }
                            }
                            else {
                                // For joins, all results are placed in a map with properties for each stream
                                underlyingEventType = null;
                            }
                        }
                    }
                }

                // obtains evaluators
                SelectExprForgeContext selectExprForgeContext = new SelectExprForgeContext(
                    exprForges, columnNames, null, typeService.EventTypes, args.EventTypeAvroHandler);

                if (insertIntoDesc == null) {
                    if (!selectedStreams.IsEmpty()) {
                        EventType resultEventType;
                        string eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                        if (underlyingEventType != null) {
                            TableMetaData table =
                                args.TableCompileTimeResolver.ResolveTableFromEventType(underlyingEventType);
                            if (table != null) {
                                underlyingEventType = table.PublicEventType;
                            }

                            EventTypeMetadata metadata = new EventTypeMetadata(
                                eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT,
                                EventTypeApplicationType.WRAPPER,
                                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                                EventTypeIdPair.Unassigned());
                            resultEventType = WrapperEventTypeUtil.MakeWrapper(
                                metadata, underlyingEventType, selPropertyTypes, null, beanEventTypeFactoryProtected,
                                args.EventTypeCompileTimeResolver);
                            args.EventTypeCompileTimeRegistry.NewType(resultEventType);

                            return new SelectEvalStreamWUnderlying(
                                selectExprForgeContext, resultEventType, namedStreams, isUsingWildcard,
                                unnamedStreams, singleStreamWrapper, underlyingIsFragmentEvent, underlyingStreamNumber,
                                underlyingPropertyEventGetter, underlyingExprForge, table, typeService.EventTypes);
                        }
                        else {
                            EventTypeMetadata metadata = new EventTypeMetadata(
                                eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT,
                                EventTypeApplicationType.MAP,
                                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                                EventTypeIdPair.Unassigned());
                            resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                metadata, selPropertyTypes, null, null, null, null, beanEventTypeFactoryProtected,
                                args.EventTypeCompileTimeResolver);
                            args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                            return new SelectEvalStreamNoUnderlyingMap(
                                selectExprForgeContext, resultEventType, namedStreams, isUsingWildcard);
                        }
                    }

                    if (isUsingWildcard) {
                        string eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                        EventTypeMetadata metadata = new EventTypeMetadata(
                            eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.WRAPPER,
                            NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                            EventTypeIdPair.Unassigned());
                        EventType resultEventType = WrapperEventTypeUtil.MakeWrapper(
                            metadata, eventType, selPropertyTypes, null, beanEventTypeFactoryProtected,
                            args.EventTypeCompileTimeResolver);
                        args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                        if (singleStreamWrapper) {
                            return new SelectEvalInsertWildcardSSWrapper(selectExprForgeContext, resultEventType);
                        }

                        if (joinWildcardProcessor == null) {
                            return new SelectEvalWildcard(selectExprForgeContext, resultEventType);
                        }

                        return new SelectEvalWildcardJoin(
                            selectExprForgeContext, resultEventType, joinWildcardProcessor);
                    }

                    EventType resultEventType;
                    EventUnderlyingType representation = EventRepresentationUtil.GetRepresentation(
                        args.Annotations, args.Configuration, CreateSchemaDesc.AssignedType.NONE);
                    string eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                    if (representation == EventUnderlyingType.OBJECTARRAY) {
                        EventTypeMetadata metadata = new EventTypeMetadata(
                            eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.OBJECTARR,
                            NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                            EventTypeIdPair.Unassigned());
                        resultEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                            metadata, selPropertyTypes, null, null, null, null, beanEventTypeFactoryProtected,
                            args.EventTypeCompileTimeResolver);
                    }
                    else if (representation == EventUnderlyingType.AVRO) {
                        EventTypeMetadata metadata = new EventTypeMetadata(
                            eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.AVRO,
                            NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                            EventTypeIdPair.Unassigned());
                        resultEventType = args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                            metadata, args.EventTypeCompileTimeResolver,
                            args.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory, selPropertyTypes,
                            args.Annotations,
                            null, null, null, args.StatementName);
                    }
                    else {
                        EventTypeMetadata metadata = new EventTypeMetadata(
                            eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP,
                            NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                            EventTypeIdPair.Unassigned());
                        IDictionary<string, object> propertyTypes =
                            EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                        resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                            metadata, propertyTypes, null, null, null, null, beanEventTypeFactoryProtected,
                            args.EventTypeCompileTimeResolver);
                    }

                    args.EventTypeCompileTimeRegistry.NewType(resultEventType);

                    if (selectExprForgeContext.ExprForges.Length == 0) {
                        return new SelectEvalNoWildcardEmptyProps(selectExprForgeContext, resultEventType);
                    }
                    else {
                        if (representation == EventUnderlyingType.OBJECTARRAY) {
                            return new SelectEvalNoWildcardObjectArray(selectExprForgeContext, resultEventType);
                        }
                        else if (representation == EventUnderlyingType.AVRO) {
                            return args.CompileTimeServices.EventTypeAvroHandler.OutputFactory.MakeSelectNoWildcard(
                                selectExprForgeContext, exprForges, resultEventType, args.TableCompileTimeResolver,
                                args.StatementName);
                        }

                        return new SelectEvalNoWildcardMap(selectExprForgeContext, resultEventType);
                    }
                }

                bool
                    singleColumnWrapOrBeanCoercion =
                        false; // Additional single-column coercion for non-wrapped type done by SelectExprInsertEventBeanFactory
                bool isVariantEvent = false;

                try {
                    if (!selectedStreams.IsEmpty()) {
                        EventType resultEventType;

                        // handle "transpose" special function with predefined target type
                        if (insertIntoTargetType != null && selectedStreams[0].ExpressionSelectedAsStream != null) {
                            if (exprForges.Length != 0) {
                                throw new ExprValidationException(
                                    "Cannot transpose additional properties in the select-clause to target event type '" +
                                    insertIntoTargetType.Name +
                                    "' with underlying type '" + insertIntoTargetType.UnderlyingType.Name + "', the " +
                                    ImportServiceCompileTime.EXT_SINGLEROW_FUNCTION_TRANSPOSE +
                                    " function must occur alone in the select clause");
                            }

                            ExprNode expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                            Type returnType = expression.Forge.EvaluationType;
                            if (insertIntoTargetType is ObjectArrayEventType && returnType == typeof(object[])) {
                                return new SelectExprInsertEventBeanFactory.
                                    SelectExprInsertNativeExpressionCoerceObjectArray(
                                        insertIntoTargetType, expression.Forge);
                            }
                            else if (insertIntoTargetType is MapEventType && TypeHelper.IsImplementsInterface(
                                         returnType, typeof(IDictionary<object, object>))) {
                                return new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceMap(
                                    insertIntoTargetType, expression.Forge);
                            }
                            else if (insertIntoTargetType is BeanEventType &&
                                     TypeHelper.IsSubclassOrImplementsInterface(
                                         returnType, insertIntoTargetType.UnderlyingType)) {
                                return new
                                    SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceNative(
                                        insertIntoTargetType, expression.Forge);
                            }
                            else if (insertIntoTargetType is AvroSchemaEventType &&
                                     returnType.Name.Equals(TypeHelper.APACHE_AVRO_GENERIC_RECORD_CLASSNAME)) {
                                return new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceAvro(
                                    insertIntoTargetType, expression.Forge);
                            }
                            else if (insertIntoTargetType is WrapperEventType) {
                                // for native event types as they got renamed, they become wrappers
                                // check if the proposed wrapper is compatible with the existing wrapper
                                WrapperEventType existing = (WrapperEventType) insertIntoTargetType;
                                if (existing.UnderlyingEventType is BeanEventType) {
                                    BeanEventType innerType = (BeanEventType) existing.UnderlyingEventType;
                                    ExprNode exprNode = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                                    if (!TypeHelper.IsSubclassOrImplementsInterface(
                                        exprNode.Forge.EvaluationType, innerType.UnderlyingType)) {
                                        throw new ExprValidationException(
                                            "Invalid expression return type '" + exprNode.Forge.EvaluationType +
                                            "' for transpose function, expected '" +
                                            innerType.UnderlyingType.GetSimpleName() + "'");
                                    }

                                    ExprForge evalExprForge = exprNode.Forge;
                                    return new SelectEvalStreamWUnderlying(
                                        selectExprForgeContext, insertIntoTargetType, namedStreams, isUsingWildcard,
                                        unnamedStreams, false, false, underlyingStreamNumber, null, evalExprForge, null,
                                        typeService.EventTypes);
                                }
                            }

                            throw SelectEvalInsertUtil.MakeEventTypeCastException(returnType, insertIntoTargetType);
                        }

                        if (underlyingEventType != null) {
                            // a single stream was selected via "stream.*" and there is no column name
                            // recast as a Map-type
                            if (underlyingEventType is MapEventType && insertIntoTargetType is MapEventType) {
                                return SelectEvalStreamWUndRecastMapFactory.Make(
                                    typeService.EventTypes, selectExprForgeContext,
                                    selectedStreams[0].StreamSelected.StreamNumber, insertIntoTargetType, exprNodes,
                                    importService, args.StatementName);
                            }

                            // recast as a Object-array-type
                            if (underlyingEventType is ObjectArrayEventType &&
                                insertIntoTargetType is ObjectArrayEventType) {
                                return SelectEvalStreamWUndRecastObjectArrayFactory.Make(
                                    typeService.EventTypes, selectExprForgeContext,
                                    selectedStreams[0].StreamSelected.StreamNumber, insertIntoTargetType, exprNodes,
                                    importService, args.StatementName);
                            }

                            // recast as a Avro-type
                            if (underlyingEventType is AvroSchemaEventType &&
                                insertIntoTargetType is AvroSchemaEventType) {
                                return args.EventTypeAvroHandler.OutputFactory.MakeRecast(
                                    typeService.EventTypes, selectExprForgeContext,
                                    selectedStreams[0].StreamSelected.StreamNumber,
                                    (AvroSchemaEventType) insertIntoTargetType, exprNodes, args.StatementName);
                            }

                            // recast as a Bean-type
                            if (underlyingEventType is BeanEventType && insertIntoTargetType is BeanEventType) {
                                return new SelectEvalInsertBeanRecast(
                                    insertIntoTargetType, selectedStreams[0].StreamSelected.StreamNumber,
                                    typeService.EventTypes);
                            }

                            // wrap if no recast possible
                            TableMetaData table =
                                args.TableCompileTimeResolver.ResolveTableFromEventType(underlyingEventType);
                            if (table != null) {
                                underlyingEventType = table.PublicEventType;
                            }

                            if (insertIntoTargetType == null || (!(insertIntoTargetType is WrapperEventType))) {
                                NameAccessModifier visibility = GetVisibility(insertIntoDesc.EventTypeName);
                                EventTypeMetadata metadata = new EventTypeMetadata(
                                    insertIntoDesc.EventTypeName, moduleName, EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.WRAPPER, visibility, EventTypeBusModifier.NONBUS, false,
                                    EventTypeIdPair.Unassigned());
                                resultEventType = WrapperEventTypeUtil.MakeWrapper(
                                    metadata, underlyingEventType, selPropertyTypes, null,
                                    beanEventTypeFactoryProtected,
                                    args.EventTypeCompileTimeResolver);
                                args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                            }
                            else {
                                resultEventType = insertIntoTargetType;
                            }

                            return new SelectEvalStreamWUnderlying(
                                selectExprForgeContext, resultEventType, namedStreams, isUsingWildcard,
                                unnamedStreams, singleStreamWrapper, underlyingIsFragmentEvent, underlyingStreamNumber,
                                underlyingPropertyEventGetter, underlyingExprForge, table, typeService.EventTypes);
                        }
                        else {
                            // there are one or more streams selected with column name such as "stream.* as columnOne"
                            if (insertIntoTargetType is BeanEventType) {
                                string name = selectedStreams[0].StreamSelected.StreamName;
                                string alias = selectedStreams[0].StreamSelected.OptionalName;
                                string syntaxUsed = name + ".*" + (alias != null ? " as " + alias : "");
                                string syntaxInstead = name + (alias != null ? " as " + alias : "");
                                throw new ExprValidationException(
                                    "The '" + syntaxUsed +
                                    "' syntax is not allowed when inserting into an existing bean event type, use the '" +
                                    syntaxInstead + "' syntax instead");
                            }

                            if (insertIntoTargetType == null || insertIntoTargetType is MapEventType) {
                                NameAccessModifier visibility = GetVisibility(insertIntoDesc.EventTypeName);
                                EventTypeMetadata metadata = new EventTypeMetadata(
                                    insertIntoDesc.EventTypeName, moduleName, EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.MAP, visibility, EventTypeBusModifier.NONBUS, false,
                                    EventTypeIdPair.Unassigned());
                                IDictionary<string, object> propertyTypes =
                                    EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                                MapEventType proposed = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                    metadata, propertyTypes, null, null, null, null, args.BeanEventTypeFactoryPrivate,
                                    args.EventTypeCompileTimeResolver);
                                if (insertIntoTargetType != null) {
                                    EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                }
                                else {
                                    insertIntoTargetType = proposed;
                                    args.EventTypeCompileTimeRegistry.NewType(proposed);
                                }

                                ISet<string> propertiesToUnwrap = GetEventBeanToObjectProps(
                                    selPropertyTypes, insertIntoTargetType);
                                if (propertiesToUnwrap.IsEmpty()) {
                                    return new SelectEvalStreamNoUnderlyingMap(
                                        selectExprForgeContext, insertIntoTargetType, namedStreams, isUsingWildcard);
                                }
                                else {
                                    return new SelectEvalStreamNoUndWEventBeanToObj(
                                        selectExprForgeContext, insertIntoTargetType, namedStreams, isUsingWildcard,
                                        propertiesToUnwrap);
                                }
                            }
                            else if (insertIntoTargetType is ObjectArrayEventType) {
                                ISet<string> propertiesToUnwrap = GetEventBeanToObjectProps(
                                    selPropertyTypes, insertIntoTargetType);
                                if (propertiesToUnwrap.IsEmpty()) {
                                    return new SelectEvalStreamNoUnderlyingObjectArray(
                                        selectExprForgeContext, insertIntoTargetType, namedStreams, isUsingWildcard);
                                }
                                else {
                                    return new SelectEvalStreamNoUndWEventBeanToObjObjArray(
                                        selectExprForgeContext, insertIntoTargetType, namedStreams, isUsingWildcard,
                                        propertiesToUnwrap);
                                }
                            }
                            else if (insertIntoTargetType is AvroSchemaEventType) {
                                throw new ExprValidationException("Avro event type does not allow contained beans");
                            }
                            else {
                                throw new IllegalStateException("Unrecognized event type " + insertIntoTargetType);
                            }
                        }
                    }

                    VariantEventType variantEventType = null;
                    if (insertIntoTargetType is VariantEventType) {
                        variantEventType = (VariantEventType) insertIntoTargetType;
                        isVariantEvent = true;
                        variantEventType.ValidateInsertedIntoEventType(eventType);
                    }

                    EventType resultEventType;
                    if (isUsingWildcard) {
                        if (variantEventType != null) {
                            resultEventType = variantEventType;
                        }
                        else {
                            if (insertIntoTargetType != null) {
                                // handle insert-into with fast coercion (no additional properties selected)
                                if (selPropertyTypes.IsEmpty()) {
                                    if (insertIntoTargetType is BeanEventType && eventType is BeanEventType) {
                                        return new SelectEvalInsertBeanRecast(
                                            insertIntoTargetType, 0, typeService.EventTypes);
                                    }

                                    if (insertIntoTargetType is ObjectArrayEventType &&
                                        eventType is ObjectArrayEventType) {
                                        ObjectArrayEventType target = (ObjectArrayEventType) insertIntoTargetType;
                                        ObjectArrayEventType source = (ObjectArrayEventType) eventType;
                                        ExprValidationException msg = BaseNestableEventType.IsDeepEqualsProperties(
                                            eventType.Name, source.Types, target.Types);
                                        if (msg == null) {
                                            return new SelectEvalInsertCoercionObjectArray(insertIntoTargetType);
                                        }
                                    }

                                    if (insertIntoTargetType is MapEventType && eventType is MapEventType) {
                                        return new SelectEvalInsertCoercionMap(insertIntoTargetType);
                                    }

                                    if (insertIntoTargetType is AvroSchemaEventType &&
                                        eventType is AvroSchemaEventType) {
                                        return new SelectEvalInsertCoercionAvro(insertIntoTargetType);
                                    }

                                    if (insertIntoTargetType is WrapperEventType && eventType is BeanEventType) {
                                        WrapperEventType wrapperType = (WrapperEventType) insertIntoTargetType;
                                        if (wrapperType.UnderlyingEventType is BeanEventType) {
                                            return new SelectEvalInsertBeanWrapRecast(
                                                wrapperType, 0, typeService.EventTypes);
                                        }
                                    }

                                    if (insertIntoTargetType is WrapperEventType) {
                                        WrapperEventType wrapperEventType = (WrapperEventType) insertIntoTargetType;
                                        if (EventTypeUtility.IsTypeOrSubTypeOf(
                                            eventType, wrapperEventType.UnderlyingEventType)) {
                                            return new SelectEvalInsertWildcardWrapper(
                                                selectExprForgeContext, insertIntoTargetType);
                                        }

                                        if (wrapperEventType.UnderlyingEventType is WrapperEventType) {
                                            WrapperEventType nestedWrapper =
                                                (WrapperEventType) wrapperEventType.UnderlyingEventType;
                                            if (EventTypeUtility.IsTypeOrSubTypeOf(
                                                eventType, nestedWrapper.UnderlyingEventType)) {
                                                return new SelectEvalInsertWildcardWrapperNested(
                                                    selectExprForgeContext, insertIntoTargetType, nestedWrapper);
                                            }
                                        }
                                    }
                                }

                                // handle insert-into by generating the writer with possible additional properties
                                SelectExprProcessorForge existingTypeProcessor =
                                    SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
                                        insertIntoTargetType, isUsingWildcard, typeService, exprForges, columnNames,
                                        expressionReturnTypes, insertIntoDesc, columnNamesAsProvided, true,
                                        args.StatementName,
                                        args.ImportService, args.EventTypeAvroHandler);
                                if (existingTypeProcessor != null) {
                                    return existingTypeProcessor;
                                }
                            }

                            NameAccessModifier visibility = GetVisibility(insertIntoDesc.EventTypeName);
                            if (selPropertyTypes.IsEmpty() && eventType is BeanEventType) {
                                BeanEventType beanEventType = (BeanEventType) eventType;
                                EventTypeMetadata metadata = new EventTypeMetadata(
                                    insertIntoDesc.EventTypeName, moduleName, EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.CLASS, visibility, EventTypeBusModifier.NONBUS, false,
                                    EventTypeIdPair.Unassigned());
                                BeanEventType newBeanType = new BeanEventType(
                                    beanEventType.Stem, metadata, beanEventTypeFactoryProtected, null, null, null,
                                    null);
                                resultEventType = newBeanType;
                                if (insertIntoTargetType != null) {
                                    EventTypeUtility.CompareExistingType(insertIntoTargetType, newBeanType);
                                }
                                else {
                                    args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                                }
                            }
                            else {
                                EventTypeMetadata metadata = new EventTypeMetadata(
                                    insertIntoDesc.EventTypeName, moduleName, EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.WRAPPER, visibility, EventTypeBusModifier.NONBUS, false,
                                    EventTypeIdPair.Unassigned());
                                WrapperEventType wrapperEventType = WrapperEventTypeUtil.MakeWrapper(
                                    metadata, eventType, selPropertyTypes, null, beanEventTypeFactoryProtected,
                                    args.EventTypeCompileTimeResolver);
                                resultEventType = wrapperEventType;
                                if (insertIntoTargetType != null) {
                                    EventTypeUtility.CompareExistingType(insertIntoTargetType, wrapperEventType);
                                }
                                else {
                                    args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                                }
                            }
                        }

                        if (singleStreamWrapper) {
                            if (!isVariantEvent) {
                                return new SelectEvalInsertWildcardSSWrapper(selectExprForgeContext, resultEventType);
                            }
                            else {
                                return new SelectEvalInsertWildcardSSWrapperRevision(
                                    selectExprForgeContext, resultEventType, variantEventType);
                            }
                        }

                        if (joinWildcardProcessor == null) {
                            if (!isVariantEvent) {
                                if (resultEventType is WrapperEventType) {
                                    return new SelectEvalInsertWildcardWrapper(selectExprForgeContext, resultEventType);
                                }
                                else {
                                    return new SelectEvalInsertWildcardBean(selectExprForgeContext, resultEventType);
                                }
                            }
                            else {
                                if (exprForges.Length == 0) {
                                    return new SelectEvalInsertWildcardVariant(
                                        selectExprForgeContext, resultEventType, variantEventType);
                                }
                                else {
                                    string eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                                    EventTypeMetadata metadata = new EventTypeMetadata(
                                        eventTypeName, moduleName, EventTypeTypeClass.STATEMENTOUT,
                                        EventTypeApplicationType.WRAPPER, NameAccessModifier.TRANSIENT,
                                        EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
                                    resultEventType = WrapperEventTypeUtil.MakeWrapper(
                                        metadata, eventType, selPropertyTypes, null, beanEventTypeFactoryProtected,
                                        args.EventTypeCompileTimeResolver);
                                    args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                                    return new SelectEvalInsertWildcardVariantWrapper(
                                        selectExprForgeContext, resultEventType, variantEventType, resultEventType);
                                }
                            }
                        }
                        else {
                            if (!isVariantEvent) {
                                return new SelectEvalInsertWildcardJoin(
                                    selectExprForgeContext, resultEventType, joinWildcardProcessor);
                            }
                            else {
                                return new SelectEvalInsertWildcardJoinVariant(
                                    selectExprForgeContext, resultEventType, joinWildcardProcessor, variantEventType);
                            }
                        }
                    }

                    // not using wildcard
                    resultEventType = null;
                    if ((columnNames.Length == 1) && (insertIntoDesc.ColumnNames.Count == 0)) {
                        if (insertIntoTargetType != null) {
                            // check if the existing type and new type are compatible
                            object columnOneType = expressionReturnTypes[0];
                            if (insertIntoTargetType is WrapperEventType) {
                                WrapperEventType wrapperType = (WrapperEventType) insertIntoTargetType;
                                // Map and Object both supported
                                if (wrapperType.UnderlyingEventType.UnderlyingType == columnOneType) {
                                    singleColumnWrapOrBeanCoercion = true;
                                    resultEventType = insertIntoTargetType;
                                }
                            }

                            if ((insertIntoTargetType is BeanEventType) && (columnOneType is Type)) {
                                BeanEventType beanType = (BeanEventType) insertIntoTargetType;
                                // Map and Object both supported
                                if (TypeHelper.IsSubclassOrImplementsInterface(
                                    (Type) columnOneType, beanType.UnderlyingType)) {
                                    singleColumnWrapOrBeanCoercion = true;
                                    resultEventType = insertIntoTargetType;
                                }
                            }
                        }
                    }

                    if (singleColumnWrapOrBeanCoercion) {
                        if (!isVariantEvent) {
                            if (resultEventType is WrapperEventType) {
                                WrapperEventType wrapper = (WrapperEventType) resultEventType;
                                if (wrapper.UnderlyingEventType is MapEventType) {
                                    return new SelectEvalInsertNoWildcardSingleColCoercionMapWrap(
                                        selectExprForgeContext, wrapper);
                                }
                                else if (wrapper.UnderlyingEventType is ObjectArrayEventType) {
                                    return new SelectEvalInsertNoWildcardSingleColCoercionObjectArrayWrap(
                                        selectExprForgeContext, wrapper);
                                }
                                else if (wrapper.UnderlyingEventType is AvroSchemaEventType) {
                                    return new SelectEvalInsertNoWildcardSingleColCoercionAvroWrap(
                                        selectExprForgeContext, wrapper);
                                }
                                else if (wrapper.UnderlyingEventType is VariantEventType) {
                                    variantEventType = (VariantEventType) wrapper.UnderlyingEventType;
                                    return new SelectEvalInsertNoWildcardSingleColCoercionBeanWrapVariant(
                                        selectExprForgeContext, wrapper, variantEventType);
                                }
                                else {
                                    return new SelectEvalInsertNoWildcardSingleColCoercionBeanWrap(
                                        selectExprForgeContext, wrapper);
                                }
                            }
                            else {
                                if (resultEventType is BeanEventType) {
                                    return new SelectEvalInsertNoWildcardSingleColCoercionBean(
                                        selectExprForgeContext, resultEventType);
                                }
                            }
                        }
                        else {
                            throw new UnsupportedOperationException(
                                "Single-column wrap conversion to variant type is not supported");
                        }
                    }

                    if (resultEventType == null) {
                        if (variantEventType != null) {
                            string eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                            EventTypeMetadata metadata = new EventTypeMetadata(
                                eventTypeName, moduleName, EventTypeTypeClass.STREAM, EventTypeApplicationType.MAP,
                                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                                EventTypeIdPair.Unassigned());
                            resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                metadata, selPropertyTypes, null, null, null, null, args.BeanEventTypeFactoryPrivate,
                                args.EventTypeCompileTimeResolver);
                            args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                        }
                        else {
                            EventType existingType = insertIntoTargetType;
                            if (existingType == null) {
                                // The type may however be an auto-import or fully-qualified class name
                                Type clazz = null;
                                try {
                                    clazz = importService.ResolveClass(insertIntoDesc.EventTypeName, false);
                                }
                                catch (ImportException e) {
                                    Log.Debug(
                                        "Target stream name '" + insertIntoDesc.EventTypeName +
                                        "' is not resolved as a class name");
                                }

                                if (clazz != null) {
                                    NameAccessModifier nameVisibility = GetVisibility(insertIntoDesc.EventTypeName);
                                    BeanEventTypeStem stem =
                                        args.CompileTimeServices.BeanEventTypeStemService.GetCreateStem(clazz, null);
                                    EventTypeMetadata metadata = new EventTypeMetadata(
                                        insertIntoDesc.EventTypeName, moduleName, EventTypeTypeClass.STREAM,
                                        EventTypeApplicationType.CLASS, nameVisibility, EventTypeBusModifier.NONBUS,
                                        false,
                                        EventTypeIdPair.Unassigned());
                                    existingType = new BeanEventType(
                                        stem, metadata, beanEventTypeFactoryProtected, null, null, null, null);
                                    args.EventTypeCompileTimeRegistry.NewType(existingType);
                                }
                            }

                            SelectExprProcessorForge selectExprInsertEventBean = null;
                            if (existingType != null) {
                                selectExprInsertEventBean = SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
                                    existingType, isUsingWildcard, typeService, exprForges, columnNames,
                                    expressionReturnTypes,
                                    insertIntoDesc, columnNamesAsProvided, false, args.StatementName,
                                    args.ImportService, args.EventTypeAvroHandler);
                            }

                            if (selectExprInsertEventBean != null) {
                                return selectExprInsertEventBean;
                            }
                            else {
                                // use the provided override-type if there is one
                                if (args.OptionalInsertIntoEventType != null) {
                                    resultEventType = insertIntoTargetType;
                                }
                                else if (existingType is AvroSchemaEventType) {
                                    args.EventTypeAvroHandler.AvroCompat(existingType, selPropertyTypes);
                                    resultEventType = existingType;
                                }
                                else {
                                    NameAccessModifier visibility = GetVisibility(insertIntoDesc.EventTypeName);
                                    EventUnderlyingType @out = EventRepresentationUtil.GetRepresentation(
                                        args.Annotations, args.Configuration, CreateSchemaDesc.AssignedType.NONE);
                                    Func<EventTypeApplicationType, EventTypeMetadata> metadata = appType =>
                                        new EventTypeMetadata(
                                            insertIntoDesc.EventTypeName, moduleName, EventTypeTypeClass.STREAM,
                                            appType,
                                            visibility, EventTypeBusModifier.NONBUS, false,
                                            EventTypeIdPair.Unassigned());
                                    IDictionary<string, object> propertyTypes =
                                        EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                                    if (@out == EventUnderlyingType.MAP) {
                                        MapEventType proposed = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                            metadata.Invoke(EventTypeApplicationType.MAP), propertyTypes, null, null,
                                            null, null,
                                            args.BeanEventTypeFactoryPrivate, args.EventTypeCompileTimeResolver);
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            args.EventTypeCompileTimeRegistry.NewType(proposed);
                                            resultEventType = proposed;
                                        }
                                    }
                                    else if (@out == EventUnderlyingType.OBJECTARRAY) {
                                        ObjectArrayEventType proposed = BaseNestableEventUtil.MakeOATypeCompileTime(
                                            metadata.Invoke(EventTypeApplicationType.OBJECTARR), propertyTypes, null,
                                            null, null,
                                            null, args.BeanEventTypeFactoryPrivate, args.EventTypeCompileTimeResolver);
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            args.EventTypeCompileTimeRegistry.NewType(proposed);
                                            resultEventType = proposed;
                                        }
                                    }
                                    else if (@out == EventUnderlyingType.AVRO) {
                                        AvroSchemaEventType proposed =
                                            args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                                                metadata.Invoke(EventTypeApplicationType.AVRO), null,
                                                args.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory,
                                                propertyTypes,
                                                args.Annotations, null, null, null, args.StatementName);
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            args.EventTypeCompileTimeRegistry.NewType(proposed);
                                            resultEventType = proposed;
                                        }
                                    }
                                    else {
                                        throw new IllegalStateException("Unrecognized code " + @out);
                                    }
                                }
                            }
                        }
                    }

                    if (variantEventType != null) {
                        variantEventType.ValidateInsertedIntoEventType(resultEventType);
                        isVariantEvent = true;
                    }

                    if (!isVariantEvent) {
                        if (resultEventType is MapEventType) {
                            return new SelectEvalNoWildcardMap(selectExprForgeContext, resultEventType);
                        }
                        else if (resultEventType is ObjectArrayEventType) {
                            return MakeObjectArrayConsiderReorder(
                                selectExprForgeContext, (ObjectArrayEventType) resultEventType, exprForges,
                                args.StatementRawInfo, args.CompileTimeServices);
                        }
                        else if (resultEventType is AvroSchemaEventType) {
                            return args.EventTypeAvroHandler.OutputFactory.MakeSelectNoWildcard(
                                selectExprForgeContext, exprForges, resultEventType, args.TableCompileTimeResolver,
                                args.StatementName);
                        }
                        else {
                            throw new IllegalStateException("Unrecognized output type " + resultEventType);
                        }
                    }
                    else {
                        return new SelectEvalInsertNoWildcardVariant(
                            selectExprForgeContext, resultEventType, variantEventType, resultEventType);
                    }
                }
                catch (EventAdapterException ex) {
                    Log.Debug("Exception provided by event adapter: " + ex.Message, ex);
                    throw new ExprValidationException(ex.Message, ex);
                }
            }
        }

        private bool IsGroupByRollupNullableExpression(ExprNode expr, GroupByRollupInfo groupByRollupInfo)
        {
            // if all levels include this key, we are fine
            foreach (AggregationGroupByRollupLevel level in groupByRollupInfo.RollupDesc.Levels) {
                if (level.IsAggregationTop) {
                    return true;
                }

                bool found = false;
                foreach (int rollupKeyIndex in level.RollupKeys) {
                    ExprNode groupExpression = groupByRollupInfo.ExprNodes[rollupKeyIndex];
                    if (ExprNodeUtilityCompare.DeepEquals(groupExpression, expr, false)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    return true;
                }
            }

            return false;
        }

        private SelectExprProcessorForge MakeObjectArrayConsiderReorder(
            SelectExprForgeContext selectExprForgeContext, ObjectArrayEventType resultEventType, ExprForge[] exprForges,
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices)
        {
            TypeWidenerSPI[] wideners = new TypeWidenerSPI[selectExprForgeContext.ColumnNames.Length];
            int[] remapped = new int[selectExprForgeContext.ColumnNames.Length];
            bool needRemap = false;
            for (int i = 0; i < selectExprForgeContext.ColumnNames.Length; i++) {
                string colName = selectExprForgeContext.ColumnNames[i];
                int index = CollectionUtil.FindItem(resultEventType.PropertyNames, colName);
                if (index == -1) {
                    throw new ExprValidationException(
                        "Could not find property '" + colName + "' in " + GetTypeNameConsiderTable(
                            resultEventType, compileTimeServices.TableCompileTimeResolver));
                }

                remapped[i] = index;
                if (index != i) {
                    needRemap = true;
                }

                ExprForge forge = exprForges[i];
                Type sourceColumnType;
                if (forge is SelectExprProcessorTypableForge) {
                    sourceColumnType = ((SelectExprProcessorTypableForge) forge).UnderlyingEvaluationType;
                }
                else if (forge is ExprEvalStreamInsertUnd) {
                    sourceColumnType = ((ExprEvalStreamInsertUnd) forge).UnderlyingReturnType;
                }
                else {
                    sourceColumnType = forge.EvaluationType;
                }

                Type targetPropType = resultEventType.GetPropertyType(colName);
                try {
                    TypeWidenerFactory.GetCheckPropertyAssignType(
                        colName, sourceColumnType, targetPropType, colName, false,
                        args.EventTypeAvroHandler.GetTypeWidenerCustomizer(resultEventType),
                        statementRawInfo.StatementName);
                }
                catch (TypeWidenerException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }
            }

            if (!needRemap) {
                return new SelectEvalInsertNoWildcardObjectArray(selectExprForgeContext, resultEventType);
            }

            if (CollectionUtil.IsAllNullArray(wideners)) {
                return new SelectEvalInsertNoWildcardObjectArrayRemap(
                    selectExprForgeContext, resultEventType, remapped);
            }

            throw new UnsupportedOperationException(
                "Automatic widening to columns of an object-array event type is not supported");
        }

        private string GetTypeNameConsiderTable(
            ObjectArrayEventType resultEventType, TableCompileTimeResolver tableCompileTimeResolver)
        {
            TableMetaData metadata = tableCompileTimeResolver.ResolveTableFromEventType(resultEventType);
            if (metadata != null) {
                return "table '" + metadata.TableName + "'";
            }

            return "type '" + resultEventType.Name + "'";
        }

        private Pair<ExprForge, object> HandleUnderlyingStreamInsert(ExprForge exprEvaluator)
        {
            if (!(exprEvaluator is ExprStreamUnderlyingNode)) {
                return null;
            }

            ExprStreamUnderlyingNode undNode = (ExprStreamUnderlyingNode) exprEvaluator;
            int streamNum = undNode.StreamId;
            Type returnType = undNode.Forge.EvaluationType;
            EventType namedWindowAsType = GetNamedWindowUnderlyingType(
                args.NamedWindowCompileTimeResolver, args.TypeService.EventTypes[streamNum]);
            TableMetaData tableMetadata =
                args.TableCompileTimeResolver.ResolveTableFromEventType(args.TypeService.EventTypes[streamNum]);

            EventType eventTypeStream;
            ExprForge forge;
            if (tableMetadata != null) {
                eventTypeStream = tableMetadata.PublicEventType;
                forge = new ExprEvalStreamInsertTable(streamNum, tableMetadata, returnType);
            }
            else if (namedWindowAsType == null) {
                eventTypeStream = args.TypeService.EventTypes[streamNum];
                forge = new ExprEvalStreamInsertUnd(undNode, streamNum, returnType);
            }
            else {
                eventTypeStream = namedWindowAsType;
                forge = new ExprEvalStreamInsertNamedWindow(streamNum, namedWindowAsType, returnType);
            }

            return new Pair<ExprForge, object>(forge, eventTypeStream);
        }

        private EventType GetNamedWindowUnderlyingType(
            NamedWindowCompileTimeResolver namedWindowCompileTimeResolver, EventType eventType)
        {
            NamedWindowMetaData nw = namedWindowCompileTimeResolver.Resolve(eventType.Name);
            if (nw == null) {
                return null;
            }

            return nw.OptionalEventTypeAs;
        }

        private static EPType[] DetermineInsertedEventTypeTargets(
            EventType targetType, IList<SelectClauseExprCompiledSpec> selectionList)
        {
            EPType[] targets = new EPType[selectionList.Count];
            if (targetType == null) {
                return targets;
            }

            for (int i = 0; i < selectionList.Count; i++) {
                SelectClauseExprCompiledSpec expr = selectionList[i];
                if (expr.ProvidedName == null) {
                    continue;
                }

                EventPropertyDescriptor desc = targetType.GetPropertyDescriptor(expr.ProvidedName);
                if (desc == null) {
                    continue;
                }

                if (!desc.IsFragment) {
                    continue;
                }

                FragmentEventType fragmentEventType = targetType.GetFragmentType(expr.ProvidedName);
                if (fragmentEventType == null) {
                    continue;
                }

                if (fragmentEventType.IsIndexed) {
                    targets[i] = EPTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
                }
                else {
                    targets[i] = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
                }
            }

            return targets;
        }

        private TypeAndForgePair HandleTypableExpression(
            ExprForge forge, int expressionNum, EventTypeNameGeneratorStatement eventTypeNameGeneratorStatement)
        {
            if (!(forge is ExprTypableReturnForge)) {
                return null;
            }

            ExprTypableReturnForge typable = (ExprTypableReturnForge) forge;
            LinkedHashMap<string, object> eventTypeExpr = typable.RowProperties;
            if (eventTypeExpr == null) {
                return null;
            }

            string eventTypeName = eventTypeNameGeneratorStatement.GetAnonymousTypeNameWithInner(expressionNum);
            EventTypeMetadata metadata = new EventTypeMetadata(
                eventTypeName, args.ModuleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            IDictionary<string, object> propertyTypes = EventTypeUtility.GetPropertyTypesNonPrimitive(eventTypeExpr);
            EventType mapType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata, propertyTypes, null, null, null, null, args.BeanEventTypeFactoryPrivate,
                args.EventTypeCompileTimeResolver);
            args.EventTypeCompileTimeRegistry.NewType(mapType);

            ExprForge newForge = new SelectExprProcessorTypableMapForge(mapType, forge);
            return new TypeAndForgePair(mapType, newForge);
        }

        private TypeAndForgePair HandleInsertIntoEnumeration(
            string insertIntoColName, EPType insertIntoTarget, ExprForge forge)
        {
            if (!(forge is ExprEnumerationForge) || insertIntoTarget == null ||
                (!EPTypeHelper.IsCarryEvent(insertIntoTarget))) {
                return null;
            }

            ExprEnumerationForge enumeration = (ExprEnumerationForge) forge;
            EventType eventTypeSingle = enumeration.GetEventTypeSingle(args.StatementRawInfo, args.CompileTimeServices);
            EventType eventTypeColl = enumeration.GetEventTypeCollection(
                args.StatementRawInfo, args.CompileTimeServices);
            EventType sourceType = eventTypeSingle != null ? eventTypeSingle : eventTypeColl;
            if (eventTypeColl == null && eventTypeSingle == null) {
                return null; // enumeration is untyped events (select-clause provided to subquery or 'new' operator)
            }

            if (sourceType.Metadata.TypeClass == EventTypeTypeClass.SUBQDERIVED) {
                return null; // we don't allow anonymous types here, thus excluding subquery multi-column selection
            }

            // check type info
            EventType targetType = EPTypeHelper.GetEventType(insertIntoTarget);
            CheckTypeCompatible(insertIntoColName, targetType, sourceType);

            // handle collection target - produce EventBean[]
            if (insertIntoTarget is EventMultiValuedEPType) {
                if (eventTypeColl != null) {
                    ExprEvalEnumerationCollForge enumerationCollForge =
                        new ExprEvalEnumerationCollForge(enumeration, targetType, false);
                    return new TypeAndForgePair(new EventType[] {targetType}, enumerationCollForge);
                }

                ExprEvalEnumerationSingleToCollForge singleToCollForge =
                    new ExprEvalEnumerationSingleToCollForge(enumeration, targetType);
                return new TypeAndForgePair(new EventType[] {targetType}, singleToCollForge);
            }

            // handle single-bean target
            // handle single-source
            if (eventTypeSingle != null) {
                ExprEvalEnumerationAtBeanSingleForge singleForge =
                    new ExprEvalEnumerationAtBeanSingleForge(enumeration, targetType);
                return new TypeAndForgePair(targetType, singleForge);
            }

            ExprEvalEnumerationCollForge enumerationCollForge =
                new ExprEvalEnumerationCollForge(enumeration, targetType, true);
            return new TypeAndForgePair(targetType, enumerationCollForge);
        }

        private void CheckTypeCompatible(string insertIntoCol, EventType targetType, EventType selectedType)
        {
            if (selectedType is BeanEventType && targetType is BeanEventType) {
                BeanEventType selected = (BeanEventType) selectedType;
                BeanEventType target = (BeanEventType) targetType;
                if (TypeHelper.IsSubclassOrImplementsInterface(selected.UnderlyingType, target.UnderlyingType)) {
                    return;
                }
            }

            if (!EventTypeUtility.IsTypeOrSubTypeOf(targetType, selectedType)) {
                throw new ExprValidationException(
                    "Incompatible type detected attempting to insert into column '" +
                    insertIntoCol + "' type '" + targetType.Name + "' compared to selected type '" + selectedType.Name +
                    "'");
            }
        }

        private TypeAndForgePair HandleInsertIntoTypableExpression(
            EPType insertIntoTarget, ExprForge forge, SelectProcessorArgs args)
        {
            if (!(forge is ExprTypableReturnForge)
                || insertIntoTarget == null
                || (!EPTypeHelper.IsCarryEvent(insertIntoTarget))) {
                return null;
            }

            EventType targetType = EPTypeHelper.GetEventType(insertIntoTarget);
            ExprTypableReturnForge typable = (ExprTypableReturnForge) forge;
            if (typable.IsMultirow == null) { // not typable after all
                return null;
            }

            LinkedHashMap<string, object> eventTypeExpr = typable.RowProperties;
            if (eventTypeExpr == null) {
                return null;
            }

            ISet<WriteablePropertyDescriptor> writables = EventTypeUtility.GetWriteableProperties(targetType, false);
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();
            IList<KeyValuePair<string, object>> writtenOffered = new List<KeyValuePair<string, object>>();

            // from Map<String, Object> determine properties and type widening that may be required
            foreach (KeyValuePair<string, object> offeredProperty in eventTypeExpr) {
                WriteablePropertyDescriptor writable = EventTypeUtility.FindWritable(offeredProperty.Key, writables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find property '" + offeredProperty.Key +
                        "' among properties for target event type '" + targetType.Name + "'");
                }

                written.Add(writable);
                writtenOffered.Add(offeredProperty);
            }

            // determine widening and column type compatibility
            TypeWidenerSPI[] wideners = new TypeWidenerSPI[written.Count];
            TypeWidenerCustomizer typeWidenerCustomizer =
                args.EventTypeAvroHandler.GetTypeWidenerCustomizer(targetType);
            for (int i = 0; i < written.Count; i++) {
                Type expected = written[i].PropertyType;
                KeyValuePair<string, object> provided = writtenOffered[i];
                if (provided.Value is Type) {
                    try {
                        wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(
                            provided.Key, (Type) provided.Value,
                            expected, written[i].PropertyName, false, typeWidenerCustomizer, args.StatementName);
                    }
                    catch (TypeWidenerException ex) {
                        throw new ExprValidationException(ex.Message, ex);
                    }
                }
            }

            bool hasWideners = !CollectionUtil.IsAllNullArray(wideners);

            // obtain factory
            WriteablePropertyDescriptor[] writtenArray = written.ToArray();
            EventBeanManufacturerForge manufacturer;
            try {
                manufacturer = EventTypeUtility.GetManufacturer(
                    targetType, writtenArray, args.ImportService, false, args.EventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException("Failed to obtain eventbean factory: " + e.Message, e);
            }

            // handle collection
            ExprForge typableForge;
            bool targetIsMultirow = insertIntoTarget is EventMultiValuedEPType;
            if (typable.IsMultirow) {
                if (targetIsMultirow) {
                    typableForge = new SelectExprProcessorTypableMultiForge(
                        typable, hasWideners, wideners, manufacturer, targetType, false);
                }
                else {
                    typableForge = new SelectExprProcessorTypableMultiForge(
                        typable, hasWideners, wideners, manufacturer, targetType, true);
                }
            }
            else {
                if (targetIsMultirow) {
                    typableForge = new SelectExprProcessorTypableSingleForge(
                        typable, hasWideners, wideners, manufacturer, targetType, false);
                }
                else {
                    typableForge = new SelectExprProcessorTypableSingleForge(
                        typable, hasWideners, wideners, manufacturer, targetType, true);
                }
            }

            object type = targetIsMultirow ? new EventType[] {targetType} : targetType;
            return new TypeAndForgePair(type, typableForge);
        }

        protected internal static void ApplyWideners(object[] row, TypeWidenerSPI[] wideners)
        {
            for (int i = 0; i < wideners.Length; i++) {
                if (wideners[i] != null) {
                    row[i] = wideners[i].Widen(row[i]);
                }
            }
        }

        public static CodegenExpression ApplyWidenersCodegen(
            CodegenExpressionRef row, TypeWidenerSPI[] wideners, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenBlock block = codegenMethodScope
                .MakeChild(typeof(void), typeof(SelectExprProcessorHelper), codegenClassScope)
                .AddParam(typeof(object[]), "row").Block;
            for (int i = 0; i < wideners.Length; i++) {
                if (wideners[i] != null) {
                    block.AssignArrayElement(
                        "row", Constant(i),
                        wideners[i].WidenCodegen(
                            ArrayAtIndex(@Ref("row"), Constant(i)), codegenMethodScope, codegenClassScope));
                }
            }

            return LocalMethodBuild(block.MethodEnd()).Pass(row).Call();
        }

        protected internal static void ApplyWideners(object[][] rows, TypeWidenerSPI[] wideners)
        {
            foreach (object[] row in rows) {
                ApplyWideners(row, wideners);
            }
        }

        public static CodegenExpression ApplyWidenersCodegenMultirow(
            CodegenExpressionRef rows, TypeWidenerSPI[] wideners, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod method = codegenMethodScope
                .MakeChild(typeof(void), typeof(SelectExprProcessorHelper), codegenClassScope)
                .AddParam(typeof(object[][]), "rows").Block
                .ForEach(typeof(object[]), "row", rows)
                .Expression(ApplyWidenersCodegen(@Ref("row"), wideners, codegenMethodScope, codegenClassScope))
                .BlockEnd()
                .MethodEnd();
            return LocalMethodBuild(method).Pass(rows).Call();
        }

        private TypeAndForgePair HandleAtEventbeanEnumeration(bool isEventBeans, ExprForge forge)
        {
            if (!(forge is ExprEnumerationForge) || !isEventBeans) {
                return null;
            }

            ExprEnumerationForge enumEval = (ExprEnumerationForge) forge;
            EventType eventTypeSingle = enumEval.GetEventTypeSingle(args.StatementRawInfo, args.CompileTimeServices);
            if (eventTypeSingle != null) {
                TableMetaData tableMetadata = args.TableCompileTimeResolver.ResolveTableFromEventType(eventTypeSingle);
                if (tableMetadata == null) {
                    ExprEvalEnumerationAtBeanSingleForge beanForge =
                        new ExprEvalEnumerationAtBeanSingleForge(enumEval, eventTypeSingle);
                    return new TypeAndForgePair(eventTypeSingle, beanForge);
                }

                throw new IllegalStateException("Unrecognized enumeration source returning table row-typed values");
            }

            EventType eventTypeColl = enumEval.GetEventTypeCollection(args.StatementRawInfo, args.CompileTimeServices);
            if (eventTypeColl != null) {
                TableMetaData tableMetadata = args.TableCompileTimeResolver.ResolveTableFromEventType(eventTypeColl);
                if (tableMetadata == null) {
                    ExprEvalEnumerationAtBeanColl
                        collForge = new ExprEvalEnumerationAtBeanColl(enumEval, eventTypeColl);
                    return new TypeAndForgePair(new EventType[] {eventTypeColl}, collForge);
                }

                ExprEvalEnumerationAtBeanCollTable tableForge =
                    new ExprEvalEnumerationAtBeanCollTable(enumEval, tableMetadata);
                return new TypeAndForgePair(new EventType[] {tableMetadata.PublicEventType}, tableForge);
            }

            return null;
        }

        // Determine which properties provided by the Map must be downcast from EventBean to Object
        private static ISet<string> GetEventBeanToObjectProps(
            IDictionary<string, object> selPropertyTypes, EventType resultEventType)
        {

            if (!(resultEventType is BaseNestableEventType)) {
                return Collections.GetEmptySet<string>();
            }

            BaseNestableEventType mapEventType = (BaseNestableEventType) resultEventType;
            ISet<string> props = null;
            foreach (KeyValuePair<string, object> entry in selPropertyTypes) {
                if (entry.Value is BeanEventType && mapEventType.Types.Get(entry.Key) is Type) {
                    if (props == null) {
                        props = new HashSet<string>();
                    }

                    props.Add(entry.Key);
                }
            }

            if (props == null) {
                return Collections.EmptySet();
            }

            return props;
        }

        private NameAccessModifier GetVisibility(string name)
        {
            return args.CompileTimeServices.ModuleVisibilityRules.GetAccessModifierEventType(
                args.StatementRawInfo, name);
        }

        private static void VerifyInsertInto(
            InsertIntoDesc insertIntoDesc,
            IList<SelectClauseExprCompiledSpec> selectionList)
        {
            // Verify all column names are unique
            ISet<string> names = new HashSet<string>();
            foreach (string element in insertIntoDesc.ColumnNames) {
                if (names.Contains(element)) {
                    throw new ExprValidationException(
                        "Property name '" + element + "' appears more then once in insert-into clause");
                }

                names.Add(element);
            }

            // Verify number of columns matches the select clause
            if ((!insertIntoDesc.ColumnNames.IsEmpty()) &&
                (insertIntoDesc.ColumnNames.Count != selectionList.Count)) {
                throw new ExprValidationException(
                    "Number of supplied values in the select or values clause does not match insert-into clause");
            }
        }

        private class TypeAndForgePair
        {
            internal TypeAndForgePair(object type, ExprForge forge)
            {
                this.Type = type;
                this.Forge = forge;
            }

            public object Type { get; private set; }
            public ExprForge Forge { get; private set; }
        }
    }
} // end of namespace