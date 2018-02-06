///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Specifies an index to build as part of an overall query plan.
    /// </summary>
    public class QueryPlanIndexItem
    {
        private readonly IList<string> _indexProps;
        private IList<Type> _optIndexCoercionTypes;
        private readonly IList<string> _rangeProps;
        private readonly IList<Type> _optRangeCoercionTypes;
        private readonly bool _unique;
        private readonly EventAdvancedIndexProvisionDesc _advancedIndexProvisionDesc;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="indexProps">
        /// - array of property names with the first dimension suplying the number of
        /// distinct indexes. The second dimension can be empty and indicates a full table scan.
        /// </param>
        /// <param name="optIndexCoercionTypes">- array of coercion types for each index, or null entry for no coercion required</param>
        /// <param name="rangeProps">range props</param>
        /// <param name="optRangeCoercionTypes">coercion for ranges</param>
        /// <param name="unique">whether index is unique on index props (not applicable to range-only)</param>
        /// <param name="advancedIndexProvisionDesc">advanced indexes</param>
        public QueryPlanIndexItem(
            IList<string> indexProps, 
            IList<Type> optIndexCoercionTypes, 
            IList<string> rangeProps, 
            IList<Type> optRangeCoercionTypes, 
            bool unique, 
            EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc) {
            if (advancedIndexProvisionDesc == null) {
                if (unique && indexProps.Count == 0) {
                    throw new ArgumentException("Invalid unique index planned without hash index props");
                }
                if (unique && rangeProps.Count > 0) {
                    throw new ArgumentException("Invalid unique index planned that includes range props");
                }
            }
            _indexProps = indexProps;
            _optIndexCoercionTypes = optIndexCoercionTypes;
            _rangeProps = (rangeProps == null || rangeProps.Count == 0) ? null : rangeProps;
            _optRangeCoercionTypes = optRangeCoercionTypes;
            _unique = unique;
            _advancedIndexProvisionDesc = advancedIndexProvisionDesc;
        }
    
        public QueryPlanIndexItem(IList<IndexedPropDesc> hashProps, IList<IndexedPropDesc> btreeProps, bool unique, EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc)
            : this(GetNames(hashProps), GetTypes(hashProps), GetNames(btreeProps), GetTypes(btreeProps), unique, advancedIndexProvisionDesc)
        {
        }

        public IList<string> IndexProps => _indexProps;

        public IList<Type> OptIndexCoercionTypes
        {
            get => _optIndexCoercionTypes;
            set => _optIndexCoercionTypes = value;
        }

        public IList<string> RangeProps => _rangeProps;

        public IList<Type> OptRangeCoercionTypes => _optRangeCoercionTypes;

        public bool IsUnique => _unique;

        public EventAdvancedIndexProvisionDesc AdvancedIndexProvisionDesc => _advancedIndexProvisionDesc;

        public override string ToString() {
            return "QueryPlanIndexItem{" +
                    "unique=" + _unique +
                    ", indexProps=" + CompatExtensions.Render(_indexProps) +
                    ", rangeProps=" + CompatExtensions.Render(_rangeProps) +
                    ", optIndexCoercionTypes=" + CompatExtensions.Render(_optIndexCoercionTypes) +
                    ", optRangeCoercionTypes=" + CompatExtensions.Render(_optRangeCoercionTypes) +
                    ", advanced=" + (_advancedIndexProvisionDesc == null ? null : _advancedIndexProvisionDesc.IndexDesc.IndexTypeName) +
                    "}";
        }
    
        public bool EqualsCompareSortedProps(QueryPlanIndexItem other) {
            if (_unique != other._unique) {
                return false;
            }
            string[] otherIndexProps = CollectionUtil.CopySortArray(other.IndexProps);
            string[] thisIndexProps = CollectionUtil.CopySortArray(this.IndexProps);
            string[] otherRangeProps = CollectionUtil.CopySortArray(other.RangeProps);
            string[] thisRangeProps = CollectionUtil.CopySortArray(this.RangeProps);
            bool compared = CollectionUtil.Compare(otherIndexProps, thisIndexProps) && CollectionUtil.Compare(otherRangeProps, thisRangeProps);
            return compared && _advancedIndexProvisionDesc == null && other._advancedIndexProvisionDesc == null;
        }

        public IList<IndexedPropDesc> HashPropsAsList => AsList(_indexProps, _optIndexCoercionTypes);

        public IList<IndexedPropDesc> BtreePropsAsList => AsList(_rangeProps, _optRangeCoercionTypes);

        private IList<IndexedPropDesc> AsList(IList<string> props, IList<Type> types)
        {
            if (props == null || props.Count == 0) {
                return Collections.GetEmptyList<IndexedPropDesc>();
            }
            var list = new List<IndexedPropDesc>(props.Count);
            for (int i = 0; i < props.Count; i++) {
                list.Add(new IndexedPropDesc(props[i], types[i]));
            }
            return list;
        }
    
        private static string[] GetNames(IndexedPropDesc[] props) {
            var names = new string[props.Length];
            for (int i = 0; i < props.Length; i++) {
                names[i] = props[i].IndexPropName;
            }
            return names;
        }
    
        private static Type[] GetTypes(IndexedPropDesc[] props) {
            var types = new Type[props.Length];
            for (int i = 0; i < props.Length; i++) {
                types[i] = props[i].CoercionType;
            }
            return types;
        }
    
        private static string[] GetNames(IList<IndexedPropDesc> props) {
            var names = new string[props.Count];
            for (int i = 0; i < props.Count; i++) {
                names[i] = props[i].IndexPropName;
            }
            return names;
        }
    
        private static Type[] GetTypes(IList<IndexedPropDesc> props) {
            var types = new Type[props.Count];
            for (int i = 0; i < props.Count; i++) {
                types[i] = props[i].CoercionType;
            }
            return types;
        }
    
        public static QueryPlanIndexItem FromIndexMultikeyTablePrimaryKey(IndexMultiKey indexMultiKey) {
            return new QueryPlanIndexItem(
                    GetNames(indexMultiKey.HashIndexedProps),
                    GetTypes(indexMultiKey.HashIndexedProps),
                    GetNames(indexMultiKey.RangeIndexedProps),
                    GetTypes(indexMultiKey.RangeIndexedProps),
                    indexMultiKey.IsUnique, null);
        }
    }
} // end of namespace
