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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Specifies an index to build as part of an overall query plan.
    /// </summary>
    public class QueryPlanIndexItem
    {
        public QueryPlanIndexItem(
            string[] hashProps,
            Type[] hashPropTypes,
            EventPropertyValueGetter hashGetter,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde hashKeySerde,
            string[] rangeProps,
            Type[] rangePropTypes,
            EventPropertyValueGetter[] rangeGetters,
            DataInputOutputSerde[] rangeKeySerdes,
            bool unique,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc)
        {
            HashProps = hashProps;
            HashPropTypes = hashPropTypes;
            HashGetter = hashGetter;
            HashKeySerde = hashKeySerde;
            RangeProps = rangeProps == null || rangeProps.Length == 0 ? null : rangeProps;
            RangePropTypes = rangePropTypes;
            RangeGetters = rangeGetters;
            RangeKeySerdes = rangeKeySerdes;
            TransformFireAndForget = transformFireAndForget;
            IsUnique = unique;
            AdvancedIndexProvisionDesc = advancedIndexProvisionDesc;
        }

        public string[] HashProps { get; }

        public EventPropertyValueGetter HashGetter { get; }
        
        public MultiKeyFromObjectArray TransformFireAndForget { get; }
        
        public DataInputOutputSerde HashKeySerde { get; }

        public Type[] HashPropTypes { get; }

        public string[] RangeProps { get; }

        public Type[] RangePropTypes { get; }

        public EventPropertyValueGetter[] RangeGetters { get; }
        
        public DataInputOutputSerde[] RangeKeySerdes { get; }

        public bool IsUnique { get; }

        public EventAdvancedIndexProvisionRuntime AdvancedIndexProvisionDesc { get; }

        public IList<IndexedPropDesc> HashPropsAsList => AsList(HashProps, HashPropTypes);

        public IList<IndexedPropDesc> BtreePropsAsList => AsList(RangeProps, RangePropTypes);

        public override string ToString()
        {
            return "QueryPlanIndexItem{" +
                   "unique=" +
                   IsUnique +
                   ", hashProps=" +
                   HashProps.RenderAny() +
                   ", rangeProps=" +
                   RangeProps.RenderAny() +
                   ", hashPropTypes=" +
                   HashPropTypes.RenderAny() +
                   ", rangePropTypes=" +
                   RangePropTypes.RenderAny() +
                   ", advanced=" +
                   AdvancedIndexProvisionDesc +
                   "}";
        }

        public bool EqualsCompareSortedProps(QueryPlanIndexItem other)
        {
            if (IsUnique != other.IsUnique) {
                return false;
            }

            string[] otherIndexProps = CollectionUtil.CopySortArray(other.HashProps);
            string[] thisIndexProps = CollectionUtil.CopySortArray(HashProps);
            string[] otherRangeProps = CollectionUtil.CopySortArray(other.RangeProps);
            string[] thisRangeProps = CollectionUtil.CopySortArray(RangeProps);
            bool compared = CollectionUtil.Compare(otherIndexProps, thisIndexProps) &&
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

        private static string[] GetNames(IndexedPropDesc[] props)
        {
            var names = new string[props.Length];
            for (var i = 0; i < props.Length; i++) {
                names[i] = props[i].IndexPropName;
            }

            return names;
        }

        private static Type[] GetTypes(IndexedPropDesc[] props)
        {
            var types = new Type[props.Length];
            for (var i = 0; i < props.Length; i++) {
                types[i] = props[i].CoercionType;
            }

            return types;
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

        public IndexMultiKey ToIndexMultiKey()
        {
            AdvancedIndexIndexMultiKeyPart part = null;
            if (AdvancedIndexProvisionDesc != null) {
                part = new AdvancedIndexIndexMultiKeyPart(
                    AdvancedIndexProvisionDesc.IndexTypeName,
                    AdvancedIndexProvisionDesc.IndexExpressionTexts,
                    AdvancedIndexProvisionDesc.IndexProperties);
            }

            return new IndexMultiKey(IsUnique, HashPropsAsList, BtreePropsAsList, part);
        }
    }
} // end of namespace