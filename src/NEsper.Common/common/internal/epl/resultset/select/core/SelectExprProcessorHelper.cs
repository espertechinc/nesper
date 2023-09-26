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
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.eval;
using com.espertech.esper.common.@internal.epl.resultset.select.typable;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    /// <summary>
    /// Processor for select-clause expressions that handles a list of selection items represented by
    /// expression nodes. Computes results based on matching events.
    /// </summary>
    public partial class SelectExprProcessorHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IContainer _container;
        private readonly IList<SelectClauseExprCompiledSpec> _selectionList;
        private readonly IList<SelectExprStreamDesc> _selectedStreams;
        private readonly SelectProcessorArgs _args;
        private readonly InsertIntoDesc _insertIntoDesc;

        public SelectExprProcessorHelper(
            IList<SelectClauseExprCompiledSpec> selectionList,
            IList<SelectExprStreamDesc> selectedStreams,
            SelectProcessorArgs args,
            InsertIntoDesc insertIntoDesc)
        {
            this._container = args.Container;
            this._selectionList = selectionList;
            this._selectedStreams = selectedStreams;
            this._args = args;
            this._insertIntoDesc = insertIntoDesc;
        }

        private EventType AllocateBeanTransposeUnderlyingType(
            Type returnType,
            string moduleName,
            BeanEventTypeFactory beanEventTypeFactoryProtected)
        {
            // check if the module has already registered the same bean type.
            // since private bean-types are registered by fully-qualified class name this prevents name-duplicate.
            foreach (var eventType in _args.EventTypeCompileTimeRegistry.NewTypesAdded) {
                if (!(eventType is BeanEventType beanEventType)) {
                    continue;
                }

                if (beanEventType.UnderlyingType == returnType) {
                    return beanEventType;
                }
            }

            // the bean-type have not been allocated
            var stem = _args.CompileTimeServices.BeanEventTypeStemService.GetCreateStem(returnType, null);
            var visibility = GetVisibility(returnType.Name);
            var metadata = new EventTypeMetadata(
                returnType.Name,
                moduleName,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.CLASS,
                visibility,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var type = new BeanEventType(
                _container,
                stem,
                metadata,
                beanEventTypeFactoryProtected,
                null,
                null,
                null,
                null);
            _args.EventTypeCompileTimeRegistry.NewType(type);
            return type;
        }

        private bool IsGroupByRollupNullableExpression(
            ExprNode expr,
            GroupByRollupInfo groupByRollupInfo)
        {
            // if all levels include this key, we are fine
            foreach (var level in groupByRollupInfo.RollupDesc.Levels) {
                if (level.IsAggregationTop) {
                    return true;
                }

                var found = false;
                foreach (var rollupKeyIndex in level.RollupKeys) {
                    var groupExpression = groupByRollupInfo.ExprNodes[rollupKeyIndex];
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
            SelectExprForgeContext selectExprForgeContext,
            ObjectArrayEventType resultEventType,
            ExprForge[] exprForges,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var wideners = new TypeWidenerSPI[selectExprForgeContext.ColumnNames.Length];
            var remapped = new int[selectExprForgeContext.ColumnNames.Length];
            var needRemap = false;
            for (var i = 0; i < selectExprForgeContext.ColumnNames.Length; i++) {
                var colName = selectExprForgeContext.ColumnNames[i];
                var index = CollectionUtil.FindItem(resultEventType.PropertyNames, colName);
                if (index == -1) {
                    throw new ExprValidationException(
                        "Could not find property '" +
                        colName +
                        "' in " +
                        GetTypeNameConsiderTable(resultEventType, compileTimeServices.TableCompileTimeResolver));
                }

                remapped[i] = index;
                if (index != i) {
                    needRemap = true;
                }

                var forge = exprForges[i];
                Type sourceColumnType;
                if (forge is SelectExprProcessorTypableForge typableForge) {
                    sourceColumnType = typableForge.UnderlyingEvaluationType;
                }
                else if (forge is ExprEvalStreamInsertBean bean) {
                    sourceColumnType = bean.UnderlyingReturnType;
                }
                else {
                    sourceColumnType = forge.EvaluationType;
                }

                var targetPropType = resultEventType.GetPropertyType(colName);
                try {
                    TypeWidenerFactory.GetCheckPropertyAssignType(
                        colName,
                        sourceColumnType,
                        targetPropType,
                        colName,
                        false,
                        _args.EventTypeAvroHandler.GetTypeWidenerCustomizer(resultEventType),
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
                    selectExprForgeContext,
                    resultEventType,
                    remapped);
            }

            throw new UnsupportedOperationException(
                "Automatic widening to columns of an object-array event type is not supported");
        }

        private string GetTypeNameConsiderTable(
            ObjectArrayEventType resultEventType,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            var metadata = tableCompileTimeResolver.ResolveTableFromEventType(resultEventType);
            if (metadata != null) {
                return "table '" + metadata.TableName + "'";
            }

            return "type '" + resultEventType.Name + "'";
        }

        private Pair<ExprForge, object> HandleUnderlyingStreamInsert(
            ExprForge exprEvaluator,
            EventPropertyDescriptor optionalInsertedTargetProp,
            EPChainableType optionalInsertedTargetEPType)
        {
            if (!(exprEvaluator is ExprStreamUnderlyingNode undNode)) {
                return null;
            }

            var streamNum = undNode.StreamId;
            var returnType = undNode.Forge.EvaluationType;
            var namedWindowAsType = GetNamedWindowUnderlyingType(
                _args.NamedWindowCompileTimeResolver,
                _args.TypeService.EventTypes[streamNum]);
            var tableMetadata =
                _args.TableCompileTimeResolver.ResolveTableFromEventType(_args.TypeService.EventTypes[streamNum]);
            if (returnType == null) {
                throw new ExprValidationException("Null-type value is not allowed");
            }

            var returnClass = returnType;
            EventType eventTypeStream;
            ExprForge forge;
            if (tableMetadata != null) {
                eventTypeStream = tableMetadata.PublicEventType;
                forge = new ExprEvalStreamInsertTable(streamNum, tableMetadata, returnClass);
            }
            else if (namedWindowAsType == null) {
                eventTypeStream = _args.TypeService.EventTypes[streamNum];
                if (optionalInsertedTargetProp != null &&
                    TypeHelper.IsSubclassOrImplementsInterface(
                        eventTypeStream.UnderlyingType,
                        optionalInsertedTargetProp.PropertyType) &&
                    (optionalInsertedTargetEPType == null ||
                     !EventTypeUtility.IsTypeOrSubTypeOf(
                         eventTypeStream,
                         optionalInsertedTargetEPType.GetEventType()))) {
                    return new Pair<ExprForge, object>(new ExprEvalStreamInsertUnd(undNode, streamNum, returnClass), returnClass);
                }
                else {
                    forge = new ExprEvalStreamInsertBean(undNode, streamNum, returnClass);
                }
            }
            else {
                eventTypeStream = namedWindowAsType;
                forge = new ExprEvalStreamInsertNamedWindow(streamNum, namedWindowAsType, returnClass);
            }

            return new Pair<ExprForge, object>(forge, eventTypeStream);
        }

        private EventType GetNamedWindowUnderlyingType(
            NamedWindowCompileTimeResolver namedWindowCompileTimeResolver,
            EventType eventType)
        {
            var nw = namedWindowCompileTimeResolver.Resolve(eventType.Name);
            if (nw == null) {
                return null;
            }

            return nw.OptionalEventTypeAs;
        }

        private static TypesAndPropertyDescPair DetermineInsertedEventTypeTargets(
            EventType targetType,
            IList<SelectClauseExprCompiledSpec> selectionList,
            InsertIntoDesc insertIntoDesc)
        {
            var targets = new EPChainableType[selectionList.Count];
            var propertyDescriptors = new EventPropertyDescriptor[selectionList.Count];
            var canInsertEventBean = new bool[selectionList.Count];
            if (targetType == null) {
                return new TypesAndPropertyDescPair(targets, propertyDescriptors, canInsertEventBean);
            }

            for (var i = 0; i < selectionList.Count; i++) {
                var expr = selectionList[i];
                string providedName = null;
                if (expr.ProvidedName != null) {
                    providedName = expr.ProvidedName;
                }
                else if (insertIntoDesc.ColumnNames.Count > i) {
                    providedName = insertIntoDesc.ColumnNames[i];
                }

                if (providedName == null) {
                    continue;
                }

                var desc = targetType.GetPropertyDescriptor(providedName);
                propertyDescriptors[i] = desc;
                if (desc == null) {
                    continue;
                }

                if (!desc.IsFragment) {
                    continue;
                }

                var fragmentEventType = targetType.GetFragmentType(providedName);
                if (fragmentEventType == null) {
                    continue;
                }

                if (fragmentEventType.IsIndexed) {
                    targets[i] = EPChainableTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
                }
                else {
                    targets[i] = EPChainableTypeHelper.SingleEvent(fragmentEventType.FragmentType);
                }

                canInsertEventBean[i] = fragmentEventType.IsCanInsertEventBean;
            }

            return new TypesAndPropertyDescPair(targets, propertyDescriptors, canInsertEventBean);
        }

        private TypeAndForgePair HandleTypableExpression(
            ExprForge forge,
            int expressionNum,
            EventTypeNameGeneratorStatement eventTypeNameGeneratorStatement)
        {
            if (!(forge is ExprTypableReturnForge typable)) {
                return null;
            }

            var eventTypeExpr = typable.RowProperties;
            if (eventTypeExpr == null) {
                return null;
            }

            var eventTypeName = eventTypeNameGeneratorStatement.GetAnonymousTypeNameWithInner(expressionNum);
            var metadata = new EventTypeMetadata(
                eventTypeName,
                _args.ModuleName,
                EventTypeTypeClass.STATEMENTOUT,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var propertyTypes = EventTypeUtility.GetPropertyTypesNonPrimitive(eventTypeExpr);
            EventType mapType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata,
                propertyTypes,
                null,
                null,
                null,
                null,
                _args.BeanEventTypeFactoryPrivate,
                _args.EventTypeCompileTimeResolver);
            _args.EventTypeCompileTimeRegistry.NewType(mapType);
            ExprForge newForge = new SelectExprProcessorTypableMapForge(mapType, typable);
            return new TypeAndForgePair(mapType, newForge);
        }

        private TypeAndForgePair HandleInsertIntoEnumeration(
            string insertIntoColName,
            EPChainableType insertIntoTarget,
            bool canInsertEventBean,
            ExprNode expr,
            ExprForge forge)
        {
            if (!insertIntoTarget.IsCarryEvent()) {
                return null;
            }

            var targetType = insertIntoTarget.GetEventType();
            ExprEnumerationForge enumeration;
            if (forge is ExprEnumerationForge enumerationForge) {
                enumeration = enumerationForge;
            }
            else if (expr is ExprEnumerationForgeProvider provider && !(provider is ExprStreamUnderlyingNode)) {
                // ExprStreamUnderlyingNode specifically is handled elsewhere
                var desc = provider.GetEnumerationForge(_args.TypeService, _args.ContextDescriptor);
                if (desc == null || desc.Forge == null) {
                    return null;
                }

                enumeration = desc.Forge;
                var sourceTypeX = enumeration.GetEventTypeSingle(_args.StatementRawInfo, _args.CompileTimeServices);
                if (sourceTypeX == null || !EventTypeUtility.IsTypeOrSubTypeOf(sourceTypeX, targetType)) {
                    return null;
                }
            }
            else {
                return null;
            }

            var eventTypeSingle = enumeration.GetEventTypeSingle(_args.StatementRawInfo, _args.CompileTimeServices);
            var eventTypeColl = enumeration.GetEventTypeCollection(_args.StatementRawInfo, _args.CompileTimeServices);
            var sourceType = eventTypeSingle ?? eventTypeColl;
            if (eventTypeColl == null && eventTypeSingle == null) {
                return null; // enumeration is untyped events (select-clause provided to subquery or 'new' operator)
            }

            if (sourceType.Metadata.TypeClass == EventTypeTypeClass.SUBQDERIVED) {
                return null; // we don't allow anonymous types here, thus excluding subquery multi-column selection
            }

            // check type info
            CheckTypeCompatible(insertIntoColName, targetType, sourceType);
            // handle collection target - produce EventBean[] or Underlying[]
            if (insertIntoTarget is EPChainableTypeEventMulti) {
                ExprForge exprForgeX;
                if (eventTypeColl != null) {
                    if (canInsertEventBean) {
                        // conversion collection-of-eventbean to array-of-eventbean
                        exprForgeX = new ExprEvalEnumerationCollToEventBeanArrayForge(enumeration, targetType);
                    }
                    else {
                        // conversion collection-of-eventbean to array-of-underlying
                        exprForgeX = new ExprEvalEnumerationCollToUnderlyingArrayForge(enumeration, targetType);
                    }
                }
                else {
                    if (canInsertEventBean) {
                        // conversion single-eventbean to array-of-eventbean
                        exprForgeX = new ExprEvalEnumerationEventBeanToEventBeanArrayForge(enumeration, targetType);
                    }
                    else {
                        // conversion single-eventbean to array-of-underlying
                        exprForgeX = new ExprEvalEnumerationEventBeanToUnderlyingArrayForge(enumeration, targetType);
                    }
                }

                return new TypeAndForgePair(new EventType[] { targetType }, exprForgeX);
            }

            // handle single-bean-source
            if (eventTypeSingle != null) {
                if (canInsertEventBean) {
                    // passing of eventbean as eventbean
                    forge = new ExprEvalEnumerationAtBeanSingleForge(enumeration, targetType);
                }

                // when inserting class-bean no need to transform
                return new TypeAndForgePair(targetType, forge);
            }

            // conversion of collection-of-eventbean to single
            ExprForge exprForge;
            if (canInsertEventBean) {
                // conversion collection-of-eventbean to eventbean
                exprForge = new ExprEvalEnumerationCollToEventBeanForge(enumeration, targetType);
            }
            else {
                // conversion collection-of-eventbean to eventbean
                exprForge = new ExprEvalEnumerationCollToUnderlyingForge(enumeration, targetType);
            }

            return new TypeAndForgePair(targetType, exprForge);
        }

        private void CheckTypeCompatible(
            string insertIntoCol,
            EventType targetType,
            EventType selectedType)
        {
            if (selectedType is BeanEventType selected && targetType is BeanEventType target) {
                if (TypeHelper.IsSubclassOrImplementsInterface(selected.UnderlyingType, target.UnderlyingType)) {
                    return;
                }
            }

            if (!EventTypeUtility.IsTypeOrSubTypeOf(targetType, selectedType)) {
                throw new ExprValidationException(
                    "Incompatible type detected attempting to insert into column '" +
                    insertIntoCol +
                    "' type '" +
                    targetType.Name +
                    "' compared to selected type '" +
                    selectedType.Name +
                    "'");
            }
        }

        private TypeAndForgePair HandleInsertIntoTypableExpression(
            EPChainableType insertIntoTarget,
            ExprForge forge,
            SelectProcessorArgs args)
        {
            if (!(forge is ExprTypableReturnForge typable) ||
                insertIntoTarget == null ||
                !insertIntoTarget.IsCarryEvent()) {
                return null;
            }

            var targetType = insertIntoTarget.GetEventType();
            if (typable.IsMultirow == null) { // not typable after all
                return null;
            }

            var eventTypeExpr = typable.RowProperties;
            if (eventTypeExpr == null) {
                return null;
            }

            var writables = EventTypeUtility.GetWriteableProperties(targetType, false, false);
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();
            IList<KeyValuePair<string, object>> writtenOffered = new List<KeyValuePair<string, object>>();
            // from Map<String, Object> determine properties and type widening that may be required
            foreach (var offeredProperty in eventTypeExpr) {
                var writable = EventTypeUtility.FindWritable(offeredProperty.Key, writables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find property '" +
                        offeredProperty.Key +
                        "' among properties for target event type '" +
                        targetType.Name +
                        "'");
                }

                written.Add(writable);
                writtenOffered.Add(offeredProperty);
            }

            // determine widening and column type compatibility
            var wideners = new TypeWidenerSPI[written.Count];
            var typeWidenerCustomizer = args.EventTypeAvroHandler.GetTypeWidenerCustomizer(targetType);
            for (var i = 0; i < written.Count; i++) {
                var expected = written[i].PropertyType;
                var provided = writtenOffered[i];
                if (provided.Value is Type value) {
                    try {
                        wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(
                            provided.Key,
                            value,
                            expected,
                            written[i].PropertyName,
                            false,
                            typeWidenerCustomizer,
                            args.StatementName);
                    }
                    catch (TypeWidenerException ex) {
                        throw new ExprValidationException(ex.Message, ex);
                    }
                }
            }

            var hasWideners = !CollectionUtil.IsAllNullArray(wideners);
            // obtain factory
            var writtenArray = written.ToArray();
            EventBeanManufacturerForge manufacturer;
            try {
                manufacturer = EventTypeUtility.GetManufacturer(
                    targetType,
                    writtenArray,
                    args.ImportService,
                    false,
                    args.EventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException("Failed to obtain eventbean factory: " + e.Message, e);
            }

            // handle collection
            ExprForge typableForge;
            var targetIsMultirow = insertIntoTarget is EPChainableTypeEventMulti;
            if (typable.IsMultirow.GetValueOrDefault()) {
                if (targetIsMultirow) {
                    typableForge = new SelectExprProcessorTypableMultiForge(
                        typable,
                        hasWideners,
                        wideners,
                        manufacturer,
                        targetType,
                        false);
                }
                else {
                    typableForge = new SelectExprProcessorTypableMultiForge(
                        typable,
                        hasWideners,
                        wideners,
                        manufacturer,
                        targetType,
                        true);
                }
            }
            else {
                if (targetIsMultirow) {
                    typableForge = new SelectExprProcessorTypableSingleForge(
                        typable,
                        hasWideners,
                        wideners,
                        manufacturer,
                        targetType,
                        false);
                }
                else {
                    typableForge = new SelectExprProcessorTypableSingleForge(
                        typable,
                        hasWideners,
                        wideners,
                        manufacturer,
                        targetType,
                        true);
                }
            }

            var type = targetIsMultirow
                ? (object) new [] { targetType }
                : targetType;
            return new TypeAndForgePair(type, typableForge);
        }

        internal static void ApplyWideners(
            object[] row,
            TypeWidenerSPI[] wideners)
        {
            for (var i = 0; i < wideners.Length; i++) {
                if (wideners[i] != null) {
                    row[i] = wideners[i].Widen(row[i]);
                }
            }
        }

        public static CodegenExpression ApplyWidenersCodegen(
            CodegenExpressionRef row,
            TypeWidenerSPI[] wideners,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope.MakeChild(typeof(void), typeof(SelectExprProcessorHelper), codegenClassScope)
                .AddParam<object[]>("row")
                .Block;
            for (var i = 0; i < wideners.Length; i++) {
                if (wideners[i] != null) {
                    block.AssignArrayElement(
                        "row",
                        Constant(i),
                        wideners[i]
                            .WidenCodegen(
                                ArrayAtIndex(Ref("row"), Constant(i)),
                                codegenMethodScope,
                                codegenClassScope));
                }
            }

            return LocalMethodBuild(block.MethodEnd()).Pass(row).Call();
        }

        public static CodegenExpression ApplyWidenersCodegenMultirow(
            CodegenExpressionRef rows,
            TypeWidenerSPI[] wideners,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(void), typeof(SelectExprProcessorHelper), codegenClassScope)
                .AddParam<object[][]>("rows")
                .Block.ForEach(typeof(object[]), "row", rows)
                .Expression(ApplyWidenersCodegen(Ref("row"), wideners, codegenMethodScope, codegenClassScope))
                .BlockEnd()
                .MethodEnd();
            return LocalMethodBuild(method).Pass(rows).Call();
        }

        private TypeAndForgePair HandleAtEventbeanEnumeration(
            bool isEventBeans,
            ExprForge forge)
        {
            if (!(forge is ExprEnumerationForge enumEval) || !isEventBeans) {
                return null;
            }

            var eventTypeColl = enumEval.GetEventTypeCollection(_args.StatementRawInfo, _args.CompileTimeServices);
            if (eventTypeColl != null) {
                var tableMetadata = _args.TableCompileTimeResolver.ResolveTableFromEventType(eventTypeColl);
                if (tableMetadata == null) {
                    var collForge = new ExprEvalEnumerationAtBeanColl(enumEval, eventTypeColl);
                    return new TypeAndForgePair(new EventType[] { eventTypeColl }, collForge);
                }

                var tableForge = new ExprEvalEnumerationAtBeanCollTable(enumEval, tableMetadata);
                return new TypeAndForgePair(new EventType[] { tableMetadata.PublicEventType }, tableForge);
            }

            var eventTypeSingle = enumEval.GetEventTypeSingle(_args.StatementRawInfo, _args.CompileTimeServices);
            if (eventTypeSingle != null) {
                var tableMetadata = _args.TableCompileTimeResolver.ResolveTableFromEventType(eventTypeSingle);
                if (tableMetadata == null) {
                    var beanForge = new ExprEvalEnumerationAtBeanSingleForge(enumEval, eventTypeSingle);
                    return new TypeAndForgePair(eventTypeSingle, beanForge);
                }

                throw new IllegalStateException("Unrecognized enumeration source returning table row-typed values");
            }

            return null;
        }

        // Determine which properties provided by the Map must be downcast from EventBean to Object
        private static ISet<string> GetEventBeanToObjectProps(
            IDictionary<string, object> selPropertyTypes,
            EventType resultEventType)
        {
            if (!(resultEventType is BaseNestableEventType mapEventType)) {
                return EmptySet<string>.Instance;
            }

            ISet<string> props = null;
            foreach (var entry in selPropertyTypes) {
                if (entry.Value is BeanEventType && mapEventType.Types.Get(entry.Key) is Type) {
                    if (props == null) {
                        props = new HashSet<string>();
                    }

                    props.Add(entry.Key);
                }
            }

            if (props == null) {
                return EmptySet<string>.Instance;
            }

            return props;
        }

        private NameAccessModifier GetVisibility(string name)
        {
            return _args.CompileTimeServices.ModuleVisibilityRules.GetAccessModifierEventType(
                _args.StatementRawInfo,
                name);
        }

        private static void VerifyInsertInto(
            InsertIntoDesc insertIntoDesc,
            IList<SelectClauseExprCompiledSpec> selectionList)
        {
            // Verify all column names are unique
            ISet<string> names = new HashSet<string>();
            foreach (var element in insertIntoDesc.ColumnNames) {
                if (names.Contains(element)) {
                    throw new ExprValidationException(
                        "Property name '" + element + "' appears more then once in insert-into clause");
                }

                names.Add(element);
            }

            // Verify number of columns matches the select clause
            if (!insertIntoDesc.ColumnNames.IsEmpty() && insertIntoDesc.ColumnNames.Count != selectionList.Count) {
                throw new ExprValidationException(
                    "Number of supplied values in the select or values clause does not match insert-into clause");
            }
        }

        public SelectExprProcessorWInsertTarget Forge {
            get {
                var isUsingWildcard = _args.IsUsingWildcard;
                var typeService = _args.TypeService;
                var importService = _args.ImportService;
                BeanEventTypeFactory beanEventTypeFactoryProtected = _args.BeanEventTypeFactoryPrivate;
                var eventTypeNameGeneratorStatement = _args.CompileTimeServices.EventTypeNameGeneratorStatement;
                var moduleName = _args.ModuleName;
                var additionalForgeables = new List<StmtClassForgeableFactory>();
                // Get the named and un-named stream selectors (i.e. select s0.* from S0 as s0), if any
                IList<SelectClauseStreamCompiledSpec> namedStreams = new List<SelectClauseStreamCompiledSpec>();
                IList<SelectExprStreamDesc> unnamedStreams = new List<SelectExprStreamDesc>();
                foreach (var spec in _selectedStreams) {
                    // handle special "transpose(...)" function
                    if (spec.StreamSelected is { OptionalName: null } ||
                        spec.ExpressionSelectedAsStream != null) {
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

                if (_selectedStreams.IsEmpty() && _selectionList.IsEmpty() && !isUsingWildcard) {
                    throw new ArgumentException("Empty selection list not supported");
                }

                foreach (var entry in _selectionList) {
                    if (entry.AssignedName == null) {
                        throw new ArgumentException("Expected name for each expression has not been supplied");
                    }
                }

                // Verify insert into clause
                if (_insertIntoDesc != null) {
                    VerifyInsertInto(_insertIntoDesc, _selectionList);
                }

                // Build a subordinate wildcard processor for joins
                SelectExprProcessorForge joinWildcardProcessor = null;
                if (typeService.StreamNames.Length > 1 && isUsingWildcard) {
                    var pair = SelectExprJoinWildcardProcessorFactory.Create(
                        _args,
                        null,
                        eventTypeName => eventTypeName + "_join");
                    joinWildcardProcessor = pair.Forge;
                    additionalForgeables.AddAll(pair.AdditionalForgeables);
                }

                // Resolve underlying event type in the case of wildcard select
                EventType eventType = null;
                var singleStreamWrapper = false;
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
                if (_insertIntoDesc != null) {
                    if (_args.OptionalInsertIntoEventType != null) {
                        insertIntoTargetType = _args.OptionalInsertIntoEventType;
                    }
                    else {
                        insertIntoTargetType =
                            _args.EventTypeCompileTimeResolver.GetTypeByName(_insertIntoDesc.EventTypeName);
                        if (insertIntoTargetType == null) {
                            var table = _args.TableCompileTimeResolver.Resolve(_insertIntoDesc.EventTypeName);
                            if (table != null) {
                                insertIntoTargetType = table.InternalEventType;
                                _args.OptionalInsertIntoEventType = insertIntoTargetType;
                            }
                        }
                    }
                }

                // Obtain insert-into per-column type information, when available
                var insertInfo = DetermineInsertedEventTypeTargets(insertIntoTargetType, _selectionList, _insertIntoDesc);
                var insertIntoTargetsPerCol = insertInfo.InsertIntoTargetsPerCol;
                var insertIntoPropertyDescriptors = insertInfo.PropertyDescriptors;
                var canHandleInsertEventBean = insertInfo.CanInsertEventBean;
                // Get expression nodes
                var exprForges = new ExprForge[_selectionList.Count];
                var exprNodes = new ExprNode[_selectionList.Count];
                var expressionReturnTypes = new object[_selectionList.Count];
                for (var i = 0; i < _selectionList.Count; i++) {
                    var spec = _selectionList[i];
                    var expr = spec.SelectExpression;
                    var forge = expr.Forge;
                    exprNodes[i] = expr;
                    // if there is insert-into specification, use that
                    if (_insertIntoDesc != null) {
                        // handle insert-into, with well-defined target event-typed column, and enumeration
                        var pairInner = HandleInsertIntoEnumeration(
                            spec.ProvidedName,
                            insertIntoTargetsPerCol[i],
                            canHandleInsertEventBean[i],
                            expr,
                            forge);
                        if (pairInner != null) {
                            expressionReturnTypes[i] = pairInner.Type;
                            exprForges[i] = pairInner.Forge;
                            continue;
                        }

                        // handle insert-into with well-defined target event-typed column, and typable expression
                        pairInner = HandleInsertIntoTypableExpression(insertIntoTargetsPerCol[i], forge, _args);
                        if (pairInner != null) {
                            expressionReturnTypes[i] = pairInner.Type;
                            exprForges[i] = pairInner.Forge;
                            continue;
                        }
                    }

                    // handle @eventbean annotation, i.e. well-defined type through enumeration
                    var pair = HandleAtEventbeanEnumeration(spec.IsEvents, forge);
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
                    if (_args.GroupByRollupInfo != null && _args.GroupByRollupInfo.RollupDesc != null) {
                        var returnType = forge.EvaluationType;
                        var returnTypeBoxed = returnType.GetBoxedType();
                        if (returnType != returnTypeBoxed &&
                            IsGroupByRollupNullableExpression(expr, _args.GroupByRollupInfo)) {
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
                if (_insertIntoDesc != null && !_insertIntoDesc.ColumnNames.IsEmpty()) {
                    columnNames = _insertIntoDesc.ColumnNames.ToArray();
                    columnNamesAsProvided = columnNames;
                }
                else if (!_selectedStreams.IsEmpty()) { // handle stream selection column names
                    var numStreamColumnsJoin = 0;
                    if (isUsingWildcard && typeService.EventTypes.Length > 1) {
                        numStreamColumnsJoin = typeService.EventTypes.Length;
                    }

                    columnNames = new string[_selectionList.Count + namedStreams.Count + numStreamColumnsJoin];
                    columnNamesAsProvided = new string[columnNames.Length];
                    var countInner = 0;
                    foreach (var aSelectionList in _selectionList) {
                        columnNames[countInner] = aSelectionList.AssignedName;
                        columnNamesAsProvided[countInner] = aSelectionList.ProvidedName;
                        countInner++;
                    }

                    foreach (var aSelectionList in namedStreams) {
                        columnNames[countInner] = aSelectionList.OptionalName;
                        columnNamesAsProvided[countInner] = aSelectionList.OptionalName;
                        countInner++;
                    }

                    // for wildcard joins, add the streams themselves
                    if (isUsingWildcard && typeService.EventTypes.Length > 1) {
                        foreach (var streamName in typeService.StreamNames) {
                            columnNames[countInner] = streamName;
                            columnNamesAsProvided[countInner] = streamName;
                            countInner++;
                        }
                    }
                }
                else {
                    // handle regular column names
                    columnNames = new string[_selectionList.Count];
                    columnNamesAsProvided = new string[_selectionList.Count];
                    for (var i = 0; i < _selectionList.Count; i++) {
                        columnNames[i] = _selectionList[i].AssignedName;
                        columnNamesAsProvided[i] = _selectionList[i].ProvidedName;
                    }
                }

                // Find if there is any fragment event types:
                // This is a special case for fragments: select a, b from pattern [a=A -> b=B]
                // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
                for (var i = 0; i < _selectionList.Count; i++) {
                    if (!(exprNodes[i] is ExprIdentNode)) {
                        continue;
                    }

                    var identNode = (ExprIdentNode)exprNodes[i];
                    var propertyName = identNode.ResolvedPropertyName;
                    var streamNum = identNode.StreamId;
                    var eventTypeStream = typeService.EventTypes[streamNum];
                    if (eventTypeStream is NativeEventType) {
                        continue; // we do not transpose the native type for performance reasons
                    }

                    var fragmentType = eventTypeStream.GetFragmentType(propertyName);
                    if (fragmentType == null || fragmentType.IsNative) {
                        continue; // we also ignore native classes as fragments for performance reasons
                    }

                    // may need to unwrap the fragment if the target type has this underlying type
                    FragmentEventType targetFragment = null;
                    if (insertIntoTargetType != null) {
                        targetFragment = insertIntoTargetType.GetFragmentType(columnNames[i]);
                    }

                    var exprRetType = expressionReturnTypes[i];
                    var exprTypeClass = exprRetType is Type asType ? asType : null;
                    if (insertIntoTargetType != null &&
                        fragmentType.FragmentType.UnderlyingType == exprTypeClass &&
                        (targetFragment == null || targetFragment?.IsNative == true)) {
                        var getter = ((EventTypeSPI)eventTypeStream).GetGetterSPI(propertyName);
                        var returnType = eventTypeStream.GetPropertyType(propertyName);
                        exprForges[i] = new ExprEvalByGetter(streamNum, getter, returnType);
                    }
                    else if (insertIntoTargetType != null &&
                             exprTypeClass != null &&
                             fragmentType.FragmentType.UnderlyingType == exprTypeClass.GetElementType() &&
                             (targetFragment == null || targetFragment?.IsNative == true)) {
                        // same for arrays: may need to unwrap the fragment if the target type has this underlying type
                        var getter = ((EventTypeSPI)eventTypeStream).GetGetterSPI(propertyName);
                        var returnType = eventTypeStream.GetPropertyType(propertyName);
                        exprForges[i] = new ExprEvalByGetter(streamNum, getter, returnType);
                    }
                    else {
                        var getter = ((EventTypeSPI)eventTypeStream).GetGetterSPI(propertyName);
                        var fragType = eventTypeStream.GetFragmentType(propertyName);
                        var undType = fragType.FragmentType.UnderlyingType;
                        var returnType = fragType.IsIndexed ? TypeHelper.GetArrayType(undType) : undType;
                        exprForges[i] = new ExprEvalByGetterFragment(streamNum, getter, returnType, fragmentType);
                        if (!fragmentType.IsIndexed) {
                            expressionReturnTypes[i] = fragmentType.FragmentType;
                        }
                        else {
                            expressionReturnTypes[i] = new[] {
                                fragmentType.FragmentType
                            };
                        }
                    }
                }

                // Find if there is any stream expression (ExprStreamNode) :
                // This is a special case for stream selection: select a, b from A as a, B as b
                // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
                for (var i = 0; i < _selectionList.Count; i++) {
                    var pair = HandleUnderlyingStreamInsert(
                        exprForges[i],
                        insertIntoPropertyDescriptors[i],
                        insertIntoTargetsPerCol[i]);
                    if (pair != null) {
                        exprForges[i] = pair.First;
                        expressionReturnTypes[i] = pair.Second;
                    }
                }

                // Build event type that reflects all selected properties
                var selPropertyTypes = new LinkedHashMap<string, object>();
                var count = 0;
                for (var i = 0; i < exprForges.Length; i++) {
                    var expressionReturnType = expressionReturnTypes[count];
                    selPropertyTypes.Put(columnNames[count], expressionReturnType);
                    count++;
                }

                if (!_selectedStreams.IsEmpty()) {
                    foreach (var element in namedStreams) {
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
                        for (var i = 0; i < typeService.EventTypes.Length; i++) {
                            var eventTypeStream = typeService.EventTypes[i];
                            selPropertyTypes.Put(columnNames[count], eventTypeStream);
                            count++;
                        }
                    }
                }

                // Handle stream selection
                EventType underlyingEventType = null;
                var underlyingStreamNumber = 0;
                var underlyingIsFragmentEvent = false;
                EventPropertyGetterSPI underlyingPropertyEventGetter = null;
                ExprForge underlyingExprForge = null;
                if (!_selectedStreams.IsEmpty()) {
                    // Resolve underlying event type in the case of wildcard or non-named stream select.
                    // Determine if the we are considering a tagged event or a stream name.
                    if (isUsingWildcard || !unnamedStreams.IsEmpty()) {
                        if (!unnamedStreams.IsEmpty()) {
                            if (unnamedStreams[0].StreamSelected != null) {
                                var streamSpec = unnamedStreams[0].StreamSelected;
                                // the tag.* syntax for :  select tag.* from pattern [tag = A]
                                underlyingStreamNumber = streamSpec.StreamNumber;
                                if (streamSpec.IsFragmentEvent) {
                                    var compositeMap = typeService.EventTypes[underlyingStreamNumber];
                                    var fragment = compositeMap.GetFragmentType(streamSpec.StreamName);
                                    underlyingEventType = fragment.FragmentType;
                                    underlyingIsFragmentEvent = true;
                                }
                                else if (streamSpec.IsProperty) {
                                    // the property.* syntax for :  select property.* from A
                                    var propertyName = streamSpec.StreamName;
                                    var propertyType = streamSpec.PropertyType;
                                    var streamNumber = streamSpec.StreamNumber;
                                    if (propertyType.IsBuiltinDataType()) {
                                        throw new ExprValidationException(
                                            "The property wildcard syntax cannot be used on built-in types as returned by property '" +
                                            propertyName +
                                            "'");
                                    }

                                    // create or get an underlying type for that Class
                                    var stem = _args.CompileTimeServices.BeanEventTypeStemService.GetCreateStem(
                                        propertyType,
                                        null);
                                    var visibility = GetVisibility(propertyType.Name);
                                    var metadata = new EventTypeMetadata(
                                        propertyType.Name,
                                        moduleName,
                                        EventTypeTypeClass.STREAM,
                                        EventTypeApplicationType.CLASS,
                                        visibility,
                                        EventTypeBusModifier.NONBUS,
                                        false,
                                        EventTypeIdPair.Unassigned());
                                    underlyingEventType = new BeanEventType(
                                        _container,
                                        stem,
                                        metadata,
                                        beanEventTypeFactoryProtected,
                                        null,
                                        null,
                                        null,
                                        null);
                                    _args.EventTypeCompileTimeRegistry.NewType(underlyingEventType);
                                    underlyingPropertyEventGetter =
                                        ((EventTypeSPI)typeService.EventTypes[streamNumber]).GetGetterSPI(propertyName);
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
                                if (_insertIntoDesc == null || insertIntoTargetType == null) {
                                    var expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                                    var returnType = expression.Forge.EvaluationType;
                                    if (returnType == null ||
                                        returnType == typeof(object[]) ||
                                        returnType.IsGenericDictionary() ||
                                        returnType.IsBuiltinDataType()) {
                                        throw new ExprValidationException(
                                            "Invalid expression return type '" +
                                            returnType.CleanName() +
                                            "' for transpose function");
                                    }

                                    underlyingEventType = AllocateBeanTransposeUnderlyingType(
                                        returnType,
                                        moduleName,
                                        beanEventTypeFactoryProtected);
                                    underlyingExprForge = expression.Forge;
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
                var selectExprForgeContext = new SelectExprForgeContext(
                    exprForges,
                    columnNames,
                    null,
                    typeService.EventTypes,
                    _args.EventTypeAvroHandler);
                if (_insertIntoDesc == null) {
                    if (!_selectedStreams.IsEmpty()) {
                        EventType resultEventTypeX;
                        var eventTypeNameInner = eventTypeNameGeneratorStatement.AnonymousTypeName;
                        if (underlyingEventType != null) {
                            var table = _args.TableCompileTimeResolver.ResolveTableFromEventType(underlyingEventType);
                            if (table != null) {
                                underlyingEventType = table.PublicEventType;
                            }

                            var metadata = new EventTypeMetadata(
                                eventTypeNameInner,
                                moduleName,
                                EventTypeTypeClass.STATEMENTOUT,
                                EventTypeApplicationType.WRAPPER,
                                NameAccessModifier.TRANSIENT,
                                EventTypeBusModifier.NONBUS,
                                false,
                                EventTypeIdPair.Unassigned());
                            resultEventTypeX = WrapperEventTypeUtil.MakeWrapper(
                                metadata,
                                underlyingEventType,
                                selPropertyTypes,
                                null,
                                beanEventTypeFactoryProtected,
                                _args.EventTypeCompileTimeResolver);
                            _args.EventTypeCompileTimeRegistry.NewType(resultEventTypeX);
                            var forgeX = new SelectEvalStreamWUnderlying(
                                selectExprForgeContext,
                                resultEventTypeX,
                                namedStreams,
                                isUsingWildcard,
                                unnamedStreams,
                                singleStreamWrapper,
                                underlyingIsFragmentEvent,
                                underlyingStreamNumber,
                                underlyingPropertyEventGetter,
                                underlyingExprForge,
                                table,
                                typeService.EventTypes);
                            return new SelectExprProcessorWInsertTarget(
                                forgeX,
                                insertIntoTargetType,
                                additionalForgeables);
                        }
                        else {
                            var metadata = new EventTypeMetadata(
                                eventTypeNameInner,
                                moduleName,
                                EventTypeTypeClass.STATEMENTOUT,
                                EventTypeApplicationType.MAP,
                                NameAccessModifier.TRANSIENT,
                                EventTypeBusModifier.NONBUS,
                                false,
                                EventTypeIdPair.Unassigned());
                            resultEventTypeX = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                metadata,
                                selPropertyTypes,
                                null,
                                null,
                                null,
                                null,
                                beanEventTypeFactoryProtected,
                                _args.EventTypeCompileTimeResolver);
                            _args.EventTypeCompileTimeRegistry.NewType(resultEventTypeX);
                            var forgeX = new SelectEvalStreamNoUnderlyingMap(
                                selectExprForgeContext,
                                resultEventTypeX,
                                namedStreams,
                                isUsingWildcard);
                            return new SelectExprProcessorWInsertTarget(
                                forgeX,
                                insertIntoTargetType,
                                additionalForgeables);
                        }
                    }

                    if (isUsingWildcard) {
                        var eventTypeNameInner = eventTypeNameGeneratorStatement.AnonymousTypeName;
                        var metadata = new EventTypeMetadata(
                            eventTypeNameInner,
                            moduleName,
                            EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.WRAPPER,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned());
                        EventType resultEventTypeInner = WrapperEventTypeUtil.MakeWrapper(
                            metadata,
                            eventType,
                            selPropertyTypes,
                            null,
                            beanEventTypeFactoryProtected,
                            _args.EventTypeCompileTimeResolver);
                        _args.EventTypeCompileTimeRegistry.NewType(resultEventTypeInner);
                        SelectExprProcessorForge forgeX;
                        if (singleStreamWrapper) {
                            forgeX = new SelectEvalInsertWildcardSSWrapper(selectExprForgeContext, resultEventTypeInner);
                        }
                        else if (joinWildcardProcessor == null) {
                            forgeX = new SelectEvalWildcard(selectExprForgeContext, resultEventTypeInner);
                        }
                        else {
                            forgeX = new SelectEvalWildcardJoin(
                                selectExprForgeContext,
                                resultEventTypeInner,
                                joinWildcardProcessor);
                        }

                        return new SelectExprProcessorWInsertTarget(forgeX, insertIntoTargetType, additionalForgeables);
                    }

                    EventType resultEventType;
                    var representation = EventRepresentationUtil.GetRepresentation(
                        _args.Annotations,
                        _args.Configuration,
                        AssignedType.NONE);
                    var eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                    if (representation == EventUnderlyingType.OBJECTARRAY) {
                        var metadata = new EventTypeMetadata(
                            eventTypeName,
                            moduleName,
                            EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.OBJECTARR,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned());
                        resultEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                            metadata,
                            selPropertyTypes,
                            null,
                            null,
                            null,
                            null,
                            beanEventTypeFactoryProtected,
                            _args.EventTypeCompileTimeResolver);
                    }
                    else if (representation == EventUnderlyingType.AVRO) {
                        var metadata = new EventTypeMetadata(
                            eventTypeName,
                            moduleName,
                            EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.AVRO,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned());
                        resultEventType = _args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                            metadata,
                            _args.EventTypeCompileTimeResolver,
                            _args.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory,
                            selPropertyTypes,
                            _args.Annotations,
                            null,
                            null,
                            null,
                            _args.StatementName);
                    }
                    else if (representation == EventUnderlyingType.JSON) {
                        var metadata = new EventTypeMetadata(
                            eventTypeName,
                            moduleName,
                            EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.JSON,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned());
                        var pair = JsonEventTypeUtility.MakeJsonTypeCompileTimeNewType(
                            metadata,
                            selPropertyTypes,
                            null,
                            null,
                            _args.StatementRawInfo,
                            _args.CompileTimeServices);
                        resultEventType = pair.EventType;
                        additionalForgeables.AddAll(pair.AdditionalForgeables);
                    }
                    else {
                        var metadata = new EventTypeMetadata(
                            eventTypeName,
                            moduleName,
                            EventTypeTypeClass.STATEMENTOUT,
                            EventTypeApplicationType.MAP,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned());
                        var propertyTypes =
                            EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                        resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                            metadata,
                            propertyTypes,
                            null,
                            null,
                            null,
                            null,
                            beanEventTypeFactoryProtected,
                            _args.EventTypeCompileTimeResolver);
                    }

                    _args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                    
                    SelectExprProcessorForge forge;
                    if (selectExprForgeContext.ExprForges.Length == 0) {
                        forge = new SelectEvalNoWildcardEmptyProps(selectExprForgeContext, resultEventType);
                    }
                    else {
                        if (representation == EventUnderlyingType.OBJECTARRAY) {
                            forge = new SelectEvalNoWildcardObjectArray(selectExprForgeContext, resultEventType);
                        }
                        else if (representation == EventUnderlyingType.AVRO) {
                            forge = _args.CompileTimeServices.EventTypeAvroHandler.OutputFactory.MakeSelectNoWildcard(
                                selectExprForgeContext,
                                exprForges,
                                resultEventType,
                                _args.TableCompileTimeResolver,
                                _args.StatementName);
                        }
                        else if (representation == EventUnderlyingType.JSON) {
                            forge = new SelectEvalNoWildcardJson(
                                selectExprForgeContext,
                                (JsonEventType)resultEventType);
                        }
                        else {
                            forge = new SelectEvalNoWildcardMap(selectExprForgeContext, resultEventType);
                        }
                    }

                    return new SelectExprProcessorWInsertTarget(forge, insertIntoTargetType, additionalForgeables);
                }

                // Additional single-column coercion for non-wrapped type done by SelectExprInsertEventBeanFactory                var singleColumnWrapOrBeanCoercion = false;
                var singleColumnWrapOrBeanCoercion = false;
                var isVariantEvent = false;
                try {
                    if (!_selectedStreams.IsEmpty()) {
                        EventType resultEventTypeX;
                        // handle "transpose" special function with predefined target type
                        if (insertIntoTargetType != null && _selectedStreams[0].ExpressionSelectedAsStream != null) {
                            if (exprForges.Length != 0) {
                                throw new ExprValidationException(
                                    "Cannot transpose additional properties in the select-clause to target event type '" +
                                    insertIntoTargetType.Name +
                                    "' with underlying type '" +
                                    insertIntoTargetType.UnderlyingType.CleanName() +
                                    "', the " +
                                    ImportServiceCompileTimeConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE +
                                    " function must occur alone in the select clause");
                            }

                            var expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                            var returnType = expression.Forge.EvaluationType;
                            if (returnType == null) {
                                throw new ExprValidationException("Cannot transpose a null-type value");
                            }

                            if (insertIntoTargetType is ObjectArrayEventType && typeof(object[]).Equals(returnType)) {
                                SelectExprProcessorForge forgeX =
                                    new SelectExprInsertEventBeanFactory.
                                        SelectExprInsertNativeExpressionCoerceObjectArray(
                                            insertIntoTargetType,
                                            expression.Forge);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else if (insertIntoTargetType is MapEventType && returnType.IsGenericStringDictionary()) {
                                SelectExprProcessorForge forgeX =
                                    new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceMap(
                                        insertIntoTargetType,
                                        expression.Forge);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else if (insertIntoTargetType is BeanEventType &&
                                     TypeHelper.IsSubclassOrImplementsInterface(
                                         returnType,
                                         insertIntoTargetType.UnderlyingType)) {
                                SelectExprProcessorForge forgeX =
                                    new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceNative(
                                        insertIntoTargetType,
                                        expression.Forge);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else if (insertIntoTargetType is AvroSchemaEventType &&
                                     Equals(returnType.FullName, TypeHelper.AVRO_GENERIC_RECORD_CLASSNAME)) {
                                SelectExprProcessorForge forgeX =
                                    new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceAvro(
                                        insertIntoTargetType,
                                        expression.Forge);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else if (insertIntoTargetType is JsonEventType && typeof(string).Equals(returnType)) {
                                SelectExprProcessorForge forgeX =
                                    new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceJson(
                                        insertIntoTargetType,
                                        expression.Forge);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else if (insertIntoTargetType is WrapperEventType existing) {
                                // for native event types as they got renamed, they become wrappers
                                // check if the proposed wrapper is compatible with the existing wrapper
                                if (existing.UnderlyingEventType is BeanEventType innerType) {
                                    var exprNode = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                                    if (!TypeHelper.IsSubclassOrImplementsInterface(
                                            exprNode.Forge.EvaluationType,
                                            innerType.UnderlyingType)) {
                                        throw new ExprValidationException(
                                            "Invalid expression return type '" +
                                            exprNode.Forge.EvaluationType +
                                            "' for transpose function, expected '" +
                                            innerType.UnderlyingType.Name +
                                            "'");
                                    }

                                    var evalExprForge = exprNode.Forge;
                                    SelectExprProcessorForge forgeX = new SelectEvalStreamWUnderlying(
                                        selectExprForgeContext,
                                        existing,
                                        namedStreams,
                                        isUsingWildcard,
                                        unnamedStreams,
                                        false,
                                        false,
                                        underlyingStreamNumber,
                                        null,
                                        evalExprForge,
                                        null,
                                        typeService.EventTypes);
                                    return new SelectExprProcessorWInsertTarget(forgeX, existing, additionalForgeables);
                                }
                            }

                            throw SelectEvalInsertUtil.MakeEventTypeCastException(
                                returnType,
                                insertIntoTargetType);
                        }

                        if (underlyingEventType != null) {
                            // a single stream was selected via "stream.*" and there is no column name
                            // recast as a Map-type
                            if (underlyingEventType is MapEventType && insertIntoTargetType is MapEventType) {
                                var forgeX = SelectEvalStreamWUndRecastMapFactory.Make(
                                    typeService.EventTypes,
                                    selectExprForgeContext,
                                    _selectedStreams[0].StreamSelected.StreamNumber,
                                    insertIntoTargetType,
                                    exprNodes,
                                    importService,
                                    _args.StatementName);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }

                            // recast as a Object-array-type
                            if (underlyingEventType is ObjectArrayEventType &&
                                insertIntoTargetType is ObjectArrayEventType) {
                                var forgeX = SelectEvalStreamWUndRecastObjectArrayFactory.Make(
                                    typeService.EventTypes,
                                    selectExprForgeContext,
                                    _selectedStreams[0].StreamSelected.StreamNumber,
                                    insertIntoTargetType,
                                    exprNodes,
                                    importService,
                                    _args.StatementName);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }

                            // recast as a Avro-type
                            if (underlyingEventType is AvroSchemaEventType &&
                                insertIntoTargetType is AvroSchemaEventType type) {
                                var forgeX = _args.EventTypeAvroHandler.OutputFactory.MakeRecast(
                                    typeService.EventTypes,
                                    selectExprForgeContext,
                                    _selectedStreams[0].StreamSelected.StreamNumber,
                                    type,
                                    exprNodes,
                                    _args.StatementName);
                                return new SelectExprProcessorWInsertTarget(forgeX, type, additionalForgeables);
                            }

                            // recast as a Bean-type
                            if (underlyingEventType is BeanEventType && insertIntoTargetType is BeanEventType) {
                                var forgeX = new SelectEvalInsertBeanRecast(
                                    insertIntoTargetType,
                                    _selectedStreams[0].StreamSelected.StreamNumber,
                                    typeService.EventTypes);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }

                            if (underlyingEventType is JsonEventType && insertIntoTargetType is JsonEventType) {
                                var forgeX = SelectEvalStreamWUndRecastJsonFactory.Make(
                                    typeService.EventTypes,
                                    selectExprForgeContext,
                                    _selectedStreams[0].StreamSelected.StreamNumber,
                                    insertIntoTargetType,
                                    exprNodes,
                                    importService,
                                    _args.StatementName);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }

                            // wrap if no recast possible
                            var table = _args.TableCompileTimeResolver.ResolveTableFromEventType(underlyingEventType);
                            if (table != null) {
                                underlyingEventType = table.PublicEventType;
                            }

                            if (insertIntoTargetType == null || !(insertIntoTargetType is WrapperEventType)) {
                                var visibility = GetVisibility(_insertIntoDesc.EventTypeName);
                                var metadata = new EventTypeMetadata(
                                    _insertIntoDesc.EventTypeName,
                                    moduleName,
                                    EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.WRAPPER,
                                    visibility,
                                    EventTypeBusModifier.NONBUS,
                                    false,
                                    EventTypeIdPair.Unassigned());
                                resultEventTypeX = WrapperEventTypeUtil.MakeWrapper(
                                    metadata,
                                    underlyingEventType,
                                    selPropertyTypes,
                                    null,
                                    beanEventTypeFactoryProtected,
                                    _args.EventTypeCompileTimeResolver);
                                _args.EventTypeCompileTimeRegistry.NewType(resultEventTypeX);
                            }
                            else {
                                resultEventTypeX = insertIntoTargetType;
                            }

                            var forgeBranch1 = new SelectEvalStreamWUnderlying(
                                selectExprForgeContext,
                                resultEventTypeX,
                                namedStreams,
                                isUsingWildcard,
                                unnamedStreams,
                                singleStreamWrapper,
                                underlyingIsFragmentEvent,
                                underlyingStreamNumber,
                                underlyingPropertyEventGetter,
                                underlyingExprForge,
                                table,
                                typeService.EventTypes);
                            return new SelectExprProcessorWInsertTarget(
                                forgeBranch1,
                                insertIntoTargetType,
                                additionalForgeables);
                        }
                        else {
                            // there are one or more streams selected with column name such as "stream.* as columnOne"
                            if (insertIntoTargetType is BeanEventType) {
                                var name = _selectedStreams[0].StreamSelected.StreamName;
                                var alias = _selectedStreams[0].StreamSelected.OptionalName;
                                var syntaxUsed = name + ".*" + (alias != null ? " as " + alias : "");
                                var syntaxInstead = name + (alias != null ? " as " + alias : "");
                                throw new ExprValidationException(
                                    "The '" +
                                    syntaxUsed +
                                    "' syntax is not allowed when inserting into an existing bean event type, use the '" +
                                    syntaxInstead +
                                    "' syntax instead");
                            }

                            if (insertIntoTargetType == null || insertIntoTargetType is MapEventType) {
                                var visibility = GetVisibility(_insertIntoDesc.EventTypeName);
                                var metadata = new EventTypeMetadata(
                                    _insertIntoDesc.EventTypeName,
                                    moduleName,
                                    EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.MAP,
                                    visibility,
                                    EventTypeBusModifier.NONBUS,
                                    false,
                                    EventTypeIdPair.Unassigned());
                                var propertyTypes =
                                    EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                                var proposed = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                    metadata,
                                    propertyTypes,
                                    null,
                                    null,
                                    null,
                                    null,
                                    _args.BeanEventTypeFactoryPrivate,
                                    _args.EventTypeCompileTimeResolver);
                                if (insertIntoTargetType != null) {
                                    EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                }
                                else {
                                    insertIntoTargetType = proposed;
                                    _args.EventTypeCompileTimeRegistry.NewType(proposed);
                                }

                                var propertiesToUnwrap = GetEventBeanToObjectProps(
                                    selPropertyTypes,
                                    insertIntoTargetType);
                                SelectExprProcessorForge forgeBranch2;
                                if (propertiesToUnwrap.IsEmpty()) {
                                    forgeBranch2 = new SelectEvalStreamNoUnderlyingMap(
                                        selectExprForgeContext,
                                        insertIntoTargetType,
                                        namedStreams,
                                        isUsingWildcard);
                                }
                                else {
                                    forgeBranch2 = new SelectEvalStreamNoUndWEventBeanToObj(
                                        selectExprForgeContext,
                                        insertIntoTargetType,
                                        namedStreams,
                                        isUsingWildcard,
                                        propertiesToUnwrap);
                                }

                                return new SelectExprProcessorWInsertTarget(
                                    forgeBranch2,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else if (insertIntoTargetType is ObjectArrayEventType) {
                                var propertiesToUnwrap = GetEventBeanToObjectProps(
                                    selPropertyTypes,
                                    insertIntoTargetType);
                                SelectExprProcessorForge forgeBranch2;
                                if (propertiesToUnwrap.IsEmpty()) {
                                    forgeBranch2 = new SelectEvalStreamNoUnderlyingObjectArray(
                                        selectExprForgeContext,
                                        insertIntoTargetType,
                                        namedStreams,
                                        isUsingWildcard);
                                }
                                else {
                                    forgeBranch2 = new SelectEvalStreamNoUndWEventBeanToObjObjArray(
                                        selectExprForgeContext,
                                        insertIntoTargetType,
                                        namedStreams,
                                        isUsingWildcard,
                                        propertiesToUnwrap);
                                }

                                return new SelectExprProcessorWInsertTarget(
                                    forgeBranch2,
                                    insertIntoTargetType,
                                    additionalForgeables);
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
                    if (insertIntoTargetType is VariantEventType targetType) {
                        variantEventType = targetType;
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
                                    if (insertIntoTargetType is BeanEventType && 
                                        eventType is BeanEventType) {
                                        SelectExprProcessorForge forgeX = new SelectEvalInsertBeanRecast(
                                            insertIntoTargetType,
                                            0,
                                            typeService.EventTypes);
                                        return new SelectExprProcessorWInsertTarget(
                                            forgeX,
                                            insertIntoTargetType,
                                            additionalForgeables);
                                    }

                                    if (insertIntoTargetType is ObjectArrayEventType type &&
                                        eventType is ObjectArrayEventType arrayEventType) {
                                        var msg = BaseNestableEventType.IsDeepEqualsProperties(
                                            eventType.Name,
                                            arrayEventType.Types,
                                            type.Types,
                                            false);
                                        if (msg == null) {
                                            SelectExprProcessorForge forgeX =
                                                new SelectEvalInsertCoercionObjectArray(type);
                                            return new SelectExprProcessorWInsertTarget(
                                                forgeX,
                                                type,
                                                additionalForgeables);
                                        }
                                    }

                                    if (insertIntoTargetType is MapEventType && eventType is MapEventType) {
                                        SelectExprProcessorForge forgeX =
                                            new SelectEvalInsertCoercionMap(insertIntoTargetType);
                                        return new SelectExprProcessorWInsertTarget(
                                            forgeX,
                                            insertIntoTargetType,
                                            additionalForgeables);
                                    }

                                    if (insertIntoTargetType is AvroSchemaEventType &&
                                        eventType is AvroSchemaEventType) {
                                        SelectExprProcessorForge forgeX =
                                            new SelectEvalInsertCoercionAvro(insertIntoTargetType);
                                        return new SelectExprProcessorWInsertTarget(
                                            forgeX,
                                            insertIntoTargetType,
                                            additionalForgeables);
                                    }

                                    if (insertIntoTargetType is JsonEventType target &&
                                        eventType is JsonEventType source) {
                                        var msg = BaseNestableEventType.IsDeepEqualsProperties(
                                            eventType.Name,
                                            source.Types,
                                            target.Types,
                                            false);
                                        if (msg == null) {
                                            SelectExprProcessorForge forgeX =
                                                new SelectEvalInsertCoercionJson(source, target);
                                            return new SelectExprProcessorWInsertTarget(
                                                forgeX,
                                                insertIntoTargetType,
                                                additionalForgeables);
                                        }
                                    }

                                    if (insertIntoTargetType is WrapperEventType wrapperType &&
                                        eventType is BeanEventType) {
                                        if (wrapperType.UnderlyingEventType is BeanEventType) {
                                            SelectExprProcessorForge forgeX = new SelectEvalInsertBeanWrapRecast(
                                                wrapperType,
                                                0,
                                                typeService.EventTypes);
                                            return new SelectExprProcessorWInsertTarget(
                                                forgeX,
                                                wrapperType,
                                                additionalForgeables);
                                        }
                                    }

                                    if (insertIntoTargetType is WrapperEventType wrapperEventType) {
                                        if (EventTypeUtility.IsTypeOrSubTypeOf(
                                                eventType,
                                                wrapperEventType.UnderlyingEventType)) {
                                            SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardWrapper(
                                                selectExprForgeContext,
                                                wrapperEventType);
                                            return new SelectExprProcessorWInsertTarget(
                                                forgeX,
                                                wrapperEventType,
                                                additionalForgeables);
                                        }

                                        if (wrapperEventType.UnderlyingEventType is WrapperEventType nestedWrapper) {
                                            if (EventTypeUtility.IsTypeOrSubTypeOf(
                                                    eventType,
                                                    nestedWrapper.UnderlyingEventType)) {
                                                SelectExprProcessorForge forgeX =
                                                    new SelectEvalInsertWildcardWrapperNested(
                                                        selectExprForgeContext,
                                                        wrapperEventType,
                                                        nestedWrapper);
                                                return new SelectExprProcessorWInsertTarget(
                                                    forgeX,
                                                    wrapperEventType,
                                                    additionalForgeables);
                                            }
                                        }
                                    }
                                }

                                // handle insert-into by generating the writer with possible additional properties
                                var existingTypeProcessor =
                                    SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
                                        insertIntoTargetType,
                                        isUsingWildcard,
                                        typeService,
                                        exprForges,
                                        columnNames,
                                        expressionReturnTypes,
                                        _insertIntoDesc,
                                        columnNamesAsProvided,
                                        true,
                                        _args.StatementName,
                                        _args.ImportService,
                                        _args.EventTypeAvroHandler);
                                if (existingTypeProcessor != null) {
                                    return new SelectExprProcessorWInsertTarget(
                                        existingTypeProcessor,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                            }

                            var visibility = GetVisibility(_insertIntoDesc.EventTypeName);
                            if (selPropertyTypes.IsEmpty() && eventType is BeanEventType beanEventType) {
                                var metadata = new EventTypeMetadata(
                                    _insertIntoDesc.EventTypeName,
                                    moduleName,
                                    EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.CLASS,
                                    visibility,
                                    EventTypeBusModifier.NONBUS,
                                    false,
                                    EventTypeIdPair.Unassigned());
                                var newBeanType = new BeanEventType(
                                    _container,
                                    beanEventType.Stem,
                                    metadata,
                                    beanEventTypeFactoryProtected,
                                    null,
                                    null,
                                    null,
                                    null);
                                resultEventType = newBeanType;
                                if (insertIntoTargetType != null) {
                                    EventTypeUtility.CompareExistingType(newBeanType, insertIntoTargetType);
                                }
                                else {
                                    _args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                                }
                            }
                            else {
                                var metadata = new EventTypeMetadata(
                                    _insertIntoDesc.EventTypeName,
                                    moduleName,
                                    EventTypeTypeClass.STREAM,
                                    EventTypeApplicationType.WRAPPER,
                                    visibility,
                                    EventTypeBusModifier.NONBUS,
                                    false,
                                    EventTypeIdPair.Unassigned());
                                var wrapperEventType = WrapperEventTypeUtil.MakeWrapper(
                                    metadata,
                                    eventType,
                                    selPropertyTypes,
                                    null,
                                    beanEventTypeFactoryProtected,
                                    _args.EventTypeCompileTimeResolver);
                                resultEventType = wrapperEventType;
                                if (insertIntoTargetType != null) {
                                    EventTypeUtility.CompareExistingType(wrapperEventType, insertIntoTargetType);
                                }
                                else {
                                    _args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                                }
                            }
                        }

                        if (singleStreamWrapper) {
                            if (!isVariantEvent) {
                                SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardSSWrapper(
                                    selectExprForgeContext,
                                    resultEventType);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else {
                                SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardSSWrapperRevision(
                                    selectExprForgeContext,
                                    resultEventType,
                                    variantEventType);
                                return new SelectExprProcessorWInsertTarget(
                                    forgeX,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                        }

                        if (joinWildcardProcessor == null) {
                            if (!isVariantEvent) {
                                if (resultEventType is WrapperEventType) {
                                    SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardWrapper(
                                        selectExprForgeContext,
                                        resultEventType);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else {
                                    SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardBean(
                                        selectExprForgeContext,
                                        resultEventType);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                            }
                            else {
                                if (exprForges.Length == 0) {
                                    SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardVariant(
                                        selectExprForgeContext,
                                        resultEventType,
                                        variantEventType);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else {
                                    var eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                                    var metadata = new EventTypeMetadata(
                                        eventTypeName,
                                        moduleName,
                                        EventTypeTypeClass.STATEMENTOUT,
                                        EventTypeApplicationType.WRAPPER,
                                        NameAccessModifier.TRANSIENT,
                                        EventTypeBusModifier.NONBUS,
                                        false,
                                        EventTypeIdPair.Unassigned());
                                    resultEventType = WrapperEventTypeUtil.MakeWrapper(
                                        metadata,
                                        eventType,
                                        selPropertyTypes,
                                        null,
                                        beanEventTypeFactoryProtected,
                                        _args.EventTypeCompileTimeResolver);
                                    _args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                                    SelectExprProcessorForge forgeX = new SelectEvalInsertWildcardVariantWrapper(
                                        selectExprForgeContext,
                                        resultEventType,
                                        variantEventType,
                                        resultEventType);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                            }
                        }
                        else {
                            SelectExprProcessorForge forgeX;
                            if (!isVariantEvent) {
                                forgeX = new SelectEvalInsertWildcardJoin(
                                    selectExprForgeContext,
                                    resultEventType,
                                    joinWildcardProcessor);
                            }
                            else {
                                forgeX = new SelectEvalInsertWildcardJoinVariant(
                                    selectExprForgeContext,
                                    resultEventType,
                                    joinWildcardProcessor,
                                    variantEventType);
                            }

                            return new SelectExprProcessorWInsertTarget(
                                forgeX,
                                insertIntoTargetType,
                                additionalForgeables);
                        }
                    }

                    // not using wildcard
                    resultEventType = null;
                    if (columnNames.Length == 1 && _insertIntoDesc.ColumnNames.Count == 0) {
                        if (insertIntoTargetType != null) {
                            // check if the existing type and new type are compatible
                            var columnOneType = expressionReturnTypes[0];
                            if (insertIntoTargetType is WrapperEventType wrapperType && columnOneType is Type type) {
                                var columnOneTypeClass = type;
                                // Map and Object both supported
                                if (wrapperType.UnderlyingEventType.UnderlyingType == columnOneTypeClass ||
                                    (wrapperType.UnderlyingEventType is JsonEventType &&
                                     columnOneTypeClass == typeof(string))) {
                                    singleColumnWrapOrBeanCoercion = true;
                                    resultEventType = wrapperType;
                                }
                            }

                            if (insertIntoTargetType is BeanEventType beanType && columnOneType is Type oneType) {
                                // Map and Object both supported
                                if (TypeHelper.IsSubclassOrImplementsInterface(oneType, beanType.UnderlyingType)) {
                                    singleColumnWrapOrBeanCoercion = true;
                                    resultEventType = beanType;
                                }
                            }
                        }
                    }

                    if (singleColumnWrapOrBeanCoercion) {
                        if (!isVariantEvent) {
                            if (resultEventType is WrapperEventType wrapper) {
                                if (wrapper.UnderlyingEventType is MapEventType) {
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionMapWrap(
                                            selectExprForgeContext,
                                            wrapper);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else if (wrapper.UnderlyingEventType is ObjectArrayEventType) {
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionObjectArrayWrap(
                                            selectExprForgeContext,
                                            wrapper);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else if (wrapper.UnderlyingEventType is JsonEventType) {
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionJsonWrap(
                                            selectExprForgeContext,
                                            wrapper);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else if (wrapper.UnderlyingEventType is AvroSchemaEventType) {
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionAvroWrap(
                                            selectExprForgeContext,
                                            wrapper);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else if (wrapper.UnderlyingEventType is VariantEventType type) {
                                    variantEventType = type;
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionBeanWrapVariant(
                                            selectExprForgeContext,
                                            wrapper,
                                            variantEventType);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                                else {
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionBeanWrap(
                                            selectExprForgeContext,
                                            wrapper);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
                                }
                            }
                            else {
                                if (resultEventType is BeanEventType) {
                                    SelectExprProcessorForge forgeX =
                                        new SelectEvalInsertNoWildcardSingleColCoercionBean(
                                            selectExprForgeContext,
                                            resultEventType);
                                    return new SelectExprProcessorWInsertTarget(
                                        forgeX,
                                        insertIntoTargetType,
                                        additionalForgeables);
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
                            var eventTypeName = eventTypeNameGeneratorStatement.AnonymousTypeName;
                            var metadata = new EventTypeMetadata(
                                eventTypeName,
                                moduleName,
                                EventTypeTypeClass.STREAM,
                                EventTypeApplicationType.MAP,
                                NameAccessModifier.TRANSIENT,
                                EventTypeBusModifier.NONBUS,
                                false,
                                EventTypeIdPair.Unassigned());
                            resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                metadata,
                                selPropertyTypes,
                                null,
                                null,
                                null,
                                null,
                                _args.BeanEventTypeFactoryPrivate,
                                _args.EventTypeCompileTimeResolver);
                            _args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                        }
                        else {
                            var existingType = insertIntoTargetType;
                            if (existingType == null) {
                                // The type may however be an auto-import or fully-qualified class name
                                Type clazz = null;
                                try {
                                    clazz = importService.ResolveType(
                                        _insertIntoDesc.EventTypeName,
                                        false,
                                        ExtensionClassEmpty.INSTANCE);
                                }
                                catch (ImportException) {
                                    Log.Debug(
                                        "Target stream name '" +
                                        _insertIntoDesc.EventTypeName +
                                        "' is not resolved as a class name");
                                }

                                if (clazz != null) {
                                    var nameVisibility = GetVisibility(_insertIntoDesc.EventTypeName);
                                    var typeClass = clazz;
                                    var stem = _args.CompileTimeServices.BeanEventTypeStemService.GetCreateStem(
                                        typeClass,
                                        null);
                                    var metadata = new EventTypeMetadata(
                                        _insertIntoDesc.EventTypeName,
                                        moduleName,
                                        EventTypeTypeClass.STREAM,
                                        EventTypeApplicationType.CLASS,
                                        nameVisibility,
                                        EventTypeBusModifier.NONBUS,
                                        false,
                                        EventTypeIdPair.Unassigned());
                                    existingType = new BeanEventType(
                                        _container,
                                        stem,
                                        metadata,
                                        beanEventTypeFactoryProtected,
                                        null,
                                        null,
                                        null,
                                        null);
                                    _args.EventTypeCompileTimeRegistry.NewType(existingType);
                                }
                            }

                            SelectExprProcessorForge selectExprInsertEventBean = null;
                            if (existingType != null) {
                                selectExprInsertEventBean = SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
                                    existingType,
                                    isUsingWildcard,
                                    typeService,
                                    exprForges,
                                    columnNames,
                                    expressionReturnTypes,
                                    _insertIntoDesc,
                                    columnNamesAsProvided,
                                    false,
                                    _args.StatementName,
                                    _args.ImportService,
                                    _args.EventTypeAvroHandler);
                            }

                            if (selectExprInsertEventBean != null) {
                                return new SelectExprProcessorWInsertTarget(
                                    selectExprInsertEventBean,
                                    insertIntoTargetType,
                                    additionalForgeables);
                            }
                            else {
                                // use the provided override-type if there is one
                                if (_args.OptionalInsertIntoEventType != null) {
                                    resultEventType = insertIntoTargetType;
                                    var propertyTypes =
                                        EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                                    Func<EventTypeApplicationType, EventTypeMetadata> metadata = appType =>
                                        new EventTypeMetadata(
                                            _args.OptionalInsertIntoEventType.Name,
                                            moduleName,
                                            EventTypeTypeClass.STREAM,
                                            appType,
                                            NameAccessModifier.PRIVATE,
                                            EventTypeBusModifier.NONBUS,
                                            false,
                                            EventTypeIdPair.Unassigned());
                                    EventType proposed;
                                    if (resultEventType is MapEventType) {
                                        proposed = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                            metadata.Invoke(EventTypeApplicationType.MAP),
                                            propertyTypes,
                                            null,
                                            null,
                                            null,
                                            null,
                                            _args.BeanEventTypeFactoryPrivate,
                                            _args.EventTypeCompileTimeResolver);
                                    }
                                    else if (resultEventType is ObjectArrayEventType) {
                                        proposed = BaseNestableEventUtil.MakeOATypeCompileTime(
                                            metadata.Invoke(EventTypeApplicationType.OBJECTARR),
                                            propertyTypes,
                                            null,
                                            null,
                                            null,
                                            null,
                                            _args.BeanEventTypeFactoryPrivate,
                                            _args.EventTypeCompileTimeResolver);
                                    }
                                    else if (resultEventType is JsonEventType) {
                                        var pair =
                                            JsonEventTypeUtility.MakeJsonTypeCompileTimeNewType(
                                                metadata.Invoke(EventTypeApplicationType.JSON),
                                                propertyTypes,
                                                null,
                                                null,
                                                _args.StatementRawInfo,
                                                _args.CompileTimeServices);
                                        proposed = pair.EventType;
                                    }
                                    else if (resultEventType is AvroSchemaEventType) {
                                        proposed = _args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                                            metadata.Invoke(EventTypeApplicationType.AVRO),
                                            null,
                                            _args.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory,
                                            propertyTypes,
                                            _args.Annotations,
                                            null,
                                            null,
                                            null,
                                            _args.StatementName);
                                    }
                                    else {
                                        throw new IllegalStateException(
                                            "Unrecognized event type " + resultEventType.Metadata.ApplicationType);
                                    }

                                    EventTypeUtility.CompareExistingType(proposed, resultEventType);
                                }
                                else if (existingType is AvroSchemaEventType) {
                                    _args.EventTypeAvroHandler.AvroCompat(existingType, selPropertyTypes);
                                    resultEventType = existingType;
                                }
                                else {
                                    var visibility = GetVisibility(_insertIntoDesc.EventTypeName);
                                    var @out = EventRepresentationUtil.GetRepresentation(
                                        _args.Annotations,
                                        _args.Configuration,
                                        AssignedType.NONE);
                                    Func<EventTypeApplicationType, EventTypeMetadata> metadata = appType =>
                                        new EventTypeMetadata(
                                            _insertIntoDesc.EventTypeName,
                                            moduleName,
                                            EventTypeTypeClass.STREAM,
                                            appType,
                                            visibility,
                                            EventTypeBusModifier.NONBUS,
                                            false,
                                            EventTypeIdPair.Unassigned());
                                    var propertyTypes =
                                        EventTypeUtility.GetPropertyTypesNonPrimitive(selPropertyTypes);
                                    if (@out == EventUnderlyingType.MAP) {
                                        var proposed = BaseNestableEventUtil.MakeMapTypeCompileTime(
                                            metadata.Invoke(EventTypeApplicationType.MAP),
                                            propertyTypes,
                                            null,
                                            null,
                                            null,
                                            null,
                                            _args.BeanEventTypeFactoryPrivate,
                                            _args.EventTypeCompileTimeResolver);
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            _args.EventTypeCompileTimeRegistry.NewType(proposed);
                                            resultEventType = proposed;
                                        }
                                    }
                                    else if (@out == EventUnderlyingType.OBJECTARRAY) {
                                        var proposed = BaseNestableEventUtil.MakeOATypeCompileTime(
                                            metadata.Invoke(EventTypeApplicationType.OBJECTARR),
                                            propertyTypes,
                                            null,
                                            null,
                                            null,
                                            null,
                                            _args.BeanEventTypeFactoryPrivate,
                                            _args.EventTypeCompileTimeResolver);
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            _args.EventTypeCompileTimeRegistry.NewType(proposed);
                                            resultEventType = proposed;
                                        }
                                    }
                                    else if (@out == EventUnderlyingType.JSON) {
                                        var pair =
                                            JsonEventTypeUtility.MakeJsonTypeCompileTimeNewType(
                                                metadata.Invoke(EventTypeApplicationType.JSON),
                                                propertyTypes,
                                                null,
                                                null,
                                                _args.StatementRawInfo,
                                                _args.CompileTimeServices);
                                        var proposed = pair.EventType;
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            _args.EventTypeCompileTimeRegistry.NewType(proposed);
                                            resultEventType = proposed;
                                            additionalForgeables.AddAll(pair.AdditionalForgeables);
                                        }
                                    }
                                    else if (@out == EventUnderlyingType.AVRO) {
                                        var proposed = _args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                                            metadata.Invoke(EventTypeApplicationType.AVRO),
                                            null,
                                            _args.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory,
                                            propertyTypes,
                                            _args.Annotations,
                                            null,
                                            null,
                                            null,
                                            _args.StatementName);
                                        if (insertIntoTargetType != null) {
                                            EventTypeUtility.CompareExistingType(proposed, insertIntoTargetType);
                                            resultEventType = insertIntoTargetType;
                                        }
                                        else {
                                            _args.EventTypeCompileTimeRegistry.NewType(proposed);
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

                    SelectExprProcessorForge forge;
                    if (!isVariantEvent) {
                        if (resultEventType is MapEventType) {
                            forge = new SelectEvalNoWildcardMap(selectExprForgeContext, resultEventType);
                        }
                        else if (resultEventType is ObjectArrayEventType type) {
                            forge = MakeObjectArrayConsiderReorder(
                                selectExprForgeContext,
                                type,
                                exprForges,
                                _args.StatementRawInfo,
                                _args.CompileTimeServices);
                        }
                        else if (resultEventType is AvroSchemaEventType) {
                            forge = _args.EventTypeAvroHandler.OutputFactory.MakeSelectNoWildcard(
                                selectExprForgeContext,
                                exprForges,
                                resultEventType,
                                _args.TableCompileTimeResolver,
                                _args.StatementName);
                        }
                        else if (resultEventType is JsonEventType jsonEventType) {
                            forge = new SelectEvalNoWildcardJson(selectExprForgeContext, jsonEventType);
                        }
                        else {
                            throw new IllegalStateException("Unrecognized output type " + resultEventType);
                        }
                    }
                    else {
                        forge = new SelectEvalInsertNoWildcardVariant(
                            selectExprForgeContext,
                            resultEventType,
                            variantEventType,
                            resultEventType);
                    }

                    return new SelectExprProcessorWInsertTarget(forge, insertIntoTargetType, additionalForgeables);
                }
                catch (EventAdapterException ex) {
                    Log.Debug("Exception provided by event adapter: " + ex.Message, ex);
                    throw new ExprValidationException(ex.Message, ex);
                }
            }
        }
    }
} // end of namespace