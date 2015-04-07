///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Specifies an index to build as part of an overall query plan.
    /// </summary>
    public class QueryPlanIndexItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="indexProps">array of property names with the first dimension suplying the number ofdistinct indexes. The second dimension can be empty and indicates a full table scan.</param>
        /// <param name="optIndexCoercionTypes">array of coercion types for each index, or null entry for no coercion required</param>
        /// <param name="rangeProps">The range props.</param>
        /// <param name="optRangeCoercionTypes">The opt range coercion types.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        public QueryPlanIndexItem(IList<string> indexProps, IList<Type> optIndexCoercionTypes, IList<string> rangeProps, IList<Type> optRangeCoercionTypes, bool unique) 
        {
            IndexProps = indexProps;
            OptIndexCoercionTypes = optIndexCoercionTypes;
            RangeProps = (rangeProps == null || rangeProps.Count == 0) ? null : rangeProps;
            OptRangeCoercionTypes = optRangeCoercionTypes;
            IsUnique = unique;
            if (IsUnique && indexProps.Count == 0)
            {
                throw new IllegalStateException("Invalid unique index planned without hash index props");
            }
            if (IsUnique && rangeProps.Count > 0)
            {
                throw new IllegalStateException("Invalid unique index planned that includes range props");
            }
        }

        public IList<string> IndexProps { get; private set; }

        public IList<Type> OptIndexCoercionTypes { get; set; }

        public IList<string> RangeProps { get; private set; }

        public IList<Type> OptRangeCoercionTypes { get; private set; }

        public Boolean IsUnique { get; private set; }

        public override String ToString() {
            return "QueryPlanIndexItem{" +
                    "unique=" + IsUnique +
                    ", indexProps=" + IndexProps.Render() +
                    ", rangeProps=" + RangeProps.Render() +
                    ", optIndexCoercionTypes=" + OptIndexCoercionTypes.Render() +
                    ", optRangeCoercionTypes=" + OptRangeCoercionTypes.Render() +
                    '}';
        }
    
        public bool EqualsCompareSortedProps(QueryPlanIndexItem other)
        {
            if (IsUnique != other.IsUnique)
            {
                return false;
            }

            String[] otherIndexProps = CollectionUtil.CopySortArray(other.IndexProps);
            String[] thisIndexProps = CollectionUtil.CopySortArray(IndexProps);
            String[] otherRangeProps = CollectionUtil.CopySortArray(other.RangeProps);
            String[] thisRangeProps = CollectionUtil.CopySortArray(RangeProps);
            return 
                CollectionUtil.Compare(otherIndexProps, thisIndexProps) && 
                CollectionUtil.Compare(otherRangeProps, thisRangeProps);
        }
    }
}
