///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.epl.index.quadtree;

namespace com.espertech.esper.filter
{
    [Serializable]
    public class FilterSpecLookupableAdvancedIndex : FilterSpecLookupable
    {
        public FilterSpecLookupableAdvancedIndex(
            string expression, 
            EventPropertyGetter getter, 
            Type returnType,
            AdvancedIndexConfigContextPartitionQuadTree quadTreeConfig, 
            EventPropertyGetter x, 
            EventPropertyGetter y,
            EventPropertyGetter width, 
            EventPropertyGetter height, 
            string indexType)
            : base(expression, getter, returnType, true)
        {
            QuadTreeConfig = quadTreeConfig;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IndexType = indexType;
        }

        public AdvancedIndexConfigContextPartitionQuadTree QuadTreeConfig { get; }

        public EventPropertyGetter X { get; }

        public EventPropertyGetter Y { get; }

        public EventPropertyGetter Width { get; }

        public EventPropertyGetter Height { get; }

        public string IndexType { get; }
    }
} // end of namespace