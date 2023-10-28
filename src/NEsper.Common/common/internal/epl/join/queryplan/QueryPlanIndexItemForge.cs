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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Specifies an index to build as part of an overall query plan.
    /// </summary>
    public class QueryPlanIndexItemForge : CodegenMakeable<SAIFFInitializeSymbol>
    {
        private readonly EventType _eventType;
        private StateMgmtSetting _stateMgmtSettings;

        public QueryPlanIndexItemForge(
            string[] hashProps,
            Type[] hashTypes,
            string[] rangeProps,
            Type[] rangeTypes,
            bool unique,
            EventAdvancedIndexProvisionCompileTime advancedIndexProvisionDesc,
            EventType eventType)
        {
            if (advancedIndexProvisionDesc == null) {
                if (unique && hashProps.Length == 0) {
                    throw new ArgumentException("Invalid unique index planned without hash index props");
                }

                if (unique && rangeProps.Length > 0) {
                    throw new ArgumentException("Invalid unique index planned that includes range props");
                }
            }

            if (hashProps == null || hashTypes == null || rangeProps == null || rangeTypes == null) {
                throw new ArgumentException("Invalid null hash and range props");
            }

            if (hashProps.Length != hashTypes.Length) {
                throw new ArgumentException("Mismatch size hash props and types");
            }

            if (rangeProps.Length != rangeTypes.Length) {
                throw new ArgumentException("Mismatch size hash props and types");
            }

            HashProps = hashProps;
            HashTypes = hashTypes;
            RangeProps = rangeProps;
            RangeTypes = rangeTypes;
            IsUnique = unique;
            AdvancedIndexProvisionDesc = advancedIndexProvisionDesc;
            _eventType = eventType;
        }

        public QueryPlanIndexItemForge(
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            bool unique,
            EventAdvancedIndexProvisionCompileTime advancedIndexProvisionDesc,
            EventType eventType)
            : this(
                GetNames(hashProps),
                GetTypes(hashProps),
                GetNames(btreeProps),
                GetTypes(btreeProps),
                unique,
                advancedIndexProvisionDesc,
                eventType)
        {
            // EventAdvancedIndexProvisionDesc
        }

        public EventAdvancedIndexProvisionCompileTime AdvancedIndexProvisionDesc { get; }

        public string[] HashProps { get; }

        public Type[] HashTypes { get; set; }

        public MultiKeyClassRef HashMultiKeyClasses { get; set; }

        public string[] RangeProps { get; }

        public Type[] RangeTypes { get; }

        public DataInputOutputSerdeForge[] RangeSerdes { get; set; }

        public bool IsUnique { get; }

        public IList<IndexedPropDesc> HashPropsAsList => AsList(HashProps, HashTypes);

        public IList<IndexedPropDesc> BtreePropsAsList => AsList(RangeProps, RangeTypes);

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return Make(parent, classScope);
        }

        public override string ToString()
        {
            return "QueryPlanIndexItemForge{" +
                   "unique=" +
                   IsUnique +
                   ", hashProps=" +
                   HashProps.RenderAny() +
                   ", rangeProps=" +
                   RangeProps.RenderAny() +
                   ", hashTypes=" +
                   HashTypes.RenderAny() +
                   ", rangeTypes=" +
                   RangeTypes.RenderAny() +
                   ", advanced=" +
                   AdvancedIndexProvisionDesc?.IndexDesc.IndexTypeName +
                   "}";
        }

        public bool EqualsCompareSortedProps(QueryPlanIndexItemForge other)
        {
            if (IsUnique != other.IsUnique) {
                return false;
            }

            var otherIndexProps = CollectionUtil.CopySortArray(other.HashProps);
            var thisIndexProps = CollectionUtil.CopySortArray(HashProps);
            var otherRangeProps = CollectionUtil.CopySortArray(other.RangeProps);
            var thisRangeProps = CollectionUtil.CopySortArray(RangeProps);
            var compared = CollectionUtil.Compare(otherIndexProps, thisIndexProps) &&
                           CollectionUtil.Compare(otherRangeProps, thisRangeProps);
            return compared && AdvancedIndexProvisionDesc == null && other.AdvancedIndexProvisionDesc == null;
        }

        private IList<IndexedPropDesc> AsList(
            string[] props,
            Type[] types)
        {
            if (props == null || props.Length == 0) {
                return Collections.GetEmptyList<IndexedPropDesc>();
            }

            IList<IndexedPropDesc> list = new List<IndexedPropDesc>(props.Length);
            for (var i = 0; i < props.Length; i++) {
                list.Add(new IndexedPropDesc(props[i], types[i]));
            }

            return list;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryPlanIndexItem), GetType(), classScope);

            var propertyGetters = EventTypeUtility.GetGetters(_eventType, HashProps);
            var propertyTypes = EventTypeUtility.GetPropertyTypes(_eventType, HashProps);
            var valueGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                _eventType,
                propertyGetters,
                propertyTypes,
                HashTypes,
                HashMultiKeyClasses,
                method,
                classScope);

            CodegenExpression rangeGetters;
            if (RangeProps.Length == 0) {
                rangeGetters = NewArrayByLength(typeof(EventPropertyValueGetter), Constant(0));
            }
            else {
                var makeMethod = parent.MakeChild(typeof(EventPropertyValueGetter[]), GetType(), classScope);
                makeMethod.Block.DeclareVar<EventPropertyValueGetter[]>(
                    "getters",
                    NewArrayByLength(typeof(EventPropertyValueGetter), Constant(RangeProps.Length)));
                for (var i = 0; i < RangeProps.Length; i++) {
                    var getter = ((EventTypeSPI)_eventType).GetGetterSPI(RangeProps[i]);
                    var getterType = _eventType.GetPropertyType(RangeProps[i]);
                    var coercionType = RangeTypes?[i];
                    var eval = EventTypeUtility.CodegenGetterWCoerce(
                        getter,
                        getterType,
                        coercionType,
                        method,
                        GetType(),
                        classScope);
                    makeMethod.Block.AssignArrayElement(Ref("getters"), Constant(i), eval);
                }

                makeMethod.Block.MethodReturn(Ref("getters"));
                rangeGetters = LocalMethod(makeMethod);
            }

            var multiKeyTransform = MultiKeyCodegen.CodegenMultiKeyFromArrayTransform(
                HashMultiKeyClasses,
                method,
                classScope);

            method.Block.MethodReturn(
                NewInstance<QueryPlanIndexItem>(
                    Constant(HashProps),
                    Constant(HashTypes),
                    valueGetter,
                    multiKeyTransform,
                    HashMultiKeyClasses != null
                        ? HashMultiKeyClasses.GetExprMKSerde(method, classScope)
                        : ConstantNull(),
                    Constant(RangeProps),
                    Constant(RangeTypes),
                    rangeGetters,
                    DataInputOutputSerdeForgeExtensions.CodegenArray(RangeSerdes, method, classScope, null),
                    Constant(IsUnique),
                    AdvancedIndexProvisionDesc == null
                        ? ConstantNull()
                        : AdvancedIndexProvisionDesc.CodegenMake(method, classScope),
                    _stateMgmtSettings.ToExpression()));
            return LocalMethod(method);
        }

        private static string[] GetNames(IList<IndexedPropDesc> props)
        {
            var names = new string[props.Count];
            for (var i = 0; i < props.Count; i++) {
                names[i] = props[i].IndexPropName;
            }

            return names;
        }

        private static Type[] GetTypes(IList<IndexedPropDesc> props)
        {
            var types = new Type[props.Count];
            for (var i = 0; i < props.Count; i++) {
                types[i] = props[i].CoercionType;
            }

            return types;
        }

        public QueryPlanIndexItem ToRuntime()
        {
            if (AdvancedIndexProvisionDesc == null) {
                return null;
            }

            return new QueryPlanIndexItem(
                HashProps,
                HashTypes,
                null,
                null,
                null,
                RangeProps,
                RangeTypes,
                null,
                null,
                IsUnique,
                AdvancedIndexProvisionDesc.ToRuntime(),
                _stateMgmtSettings);
        }

        public void PlanStateMgmtSettings(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            string indexName,
            QueryPlanIndexItemForge forge,
            StatementRawInfo raw,
            StatementCompileTimeServices compileTimeServices)
        {
            if (HashProps.Length > 0 && RangeProps.Length == 0) {
                var indexDesc = new StateMgmtIndexDescHash(
                    forge.HashProps,
                    forge.HashMultiKeyClasses,
                    forge.IsUnique);
                _stateMgmtSettings = compileTimeServices.StateMgmtSettingsProvider.Index.IndexHash(
                    fabricCharge,
                    attributionKey,
                    indexName,
                    forge._eventType,
                    indexDesc,
                    raw);
            }
            else if (HashProps.Length == 0 && RangeProps.Length == 1) {
                var indexDesc = new StateMgmtIndexDescSorted(RangeProps[0], RangeSerdes[0]);
                _stateMgmtSettings = compileTimeServices.StateMgmtSettingsProvider
                    .Index
                    .Sorted(fabricCharge, attributionKey, indexName, forge._eventType, indexDesc, raw);
            }
            else if (HashProps.Length + RangeProps.Length > 1) {
                var composite = new StateMgmtIndexDescComposite(
                    HashProps,
                    HashMultiKeyClasses,
                    RangeProps,
                    RangeSerdes);
                _stateMgmtSettings = compileTimeServices.StateMgmtSettingsProvider
                    .Index
                    .Composite(
                        fabricCharge,
                        attributionKey,
                        indexName,
                        forge._eventType,
                        composite,
                        raw);
            }
            else if (AdvancedIndexProvisionDesc == null) {
                _stateMgmtSettings = compileTimeServices.StateMgmtSettingsProvider
                    .Index
                    .Unindexed(
                        fabricCharge,
                        attributionKey,
                        forge._eventType,
                        raw);
            }
            else {
                _stateMgmtSettings = compileTimeServices.StateMgmtSettingsProvider
                    .Index
                    .Advanced(
                        fabricCharge,
                        attributionKey,
                        indexName,
                        forge._eventType,
                        forge.AdvancedIndexProvisionDesc,
                        raw);
            }
        }
    }
} // end of namespace