///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecLookupableAdvancedIndex : ExprFilterSpecLookupable
    {
        public FilterSpecLookupableAdvancedIndex(
            string expression,
            ExprEventEvaluator getter,
            Type returnType)
            : base(expression, getter, null, returnType, true, null)
        {
        }

        public EventPropertyValueGetter X { get; set; }

        public EventPropertyValueGetter Y { get; set; }

        public EventPropertyValueGetter Width { get; set; }

        public EventPropertyValueGetter Height { get; set; }

        public AdvancedIndexConfigContextPartitionQuadTree QuadTreeConfig { get; set; }

        public string IndexType { get; set; }
    }
} // end of namespace