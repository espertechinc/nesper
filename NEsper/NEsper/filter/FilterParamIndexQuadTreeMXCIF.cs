///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.index.quadtree;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;
using com.espertech.esper.spatial.quadtree.mxciffilterindex;

namespace com.espertech.esper.filter
{
    public class FilterParamIndexQuadTreeMXCIF : FilterParamIndexLookupableBase
    {
        private readonly IReaderWriterLock _readWriteLock;
        private readonly MXCIFQuadTree<object> _quadTree;
        private readonly FilterSpecLookupableAdvancedIndex _advancedIndex;
    
        private static readonly QuadTreeCollector<EventEvaluator, ICollection<FilterHandle>> COLLECTOR =
            new ProxyQuadTreeCollector<EventEvaluator, ICollection<FilterHandle>>()
        {
            ProcCollectInto = (@event, eventEvaluator, c) => eventEvaluator.MatchEvent(@event, c)
        };
    
        public FilterParamIndexQuadTreeMXCIF(IReaderWriterLock readWriteLock, FilterSpecLookupable lookupable)
            : base(FilterOperator.ADVANCED_INDEX, lookupable)
        {
            _readWriteLock = readWriteLock;
            _advancedIndex = (FilterSpecLookupableAdvancedIndex) lookupable;
            var quadTreeConfig = _advancedIndex.QuadTreeConfig;
            _quadTree = MXCIFQuadTreeFactory<object>.Make(
                quadTreeConfig.X, 
                quadTreeConfig.Y, 
                quadTreeConfig.Width, 
                quadTreeConfig.Height);
        }
    
        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches) {
            var x = _advancedIndex.X.Get(theEvent).AsDouble();
            var y = _advancedIndex.Y.Get(theEvent).AsDouble();
            var width = _advancedIndex.Width.Get(theEvent).AsDouble();
            var height = _advancedIndex.Height.Get(theEvent).AsDouble();
            MXCIFQuadTreeFilterIndexCollect<EventEvaluator, ICollection<FilterHandle>>
                .CollectRange(_quadTree, x, y, width, height, theEvent, matches, COLLECTOR);
        }
    
        public override EventEvaluator Get(object filterConstant) {
            var rect = (XYWHRectangle) filterConstant;
            return MXCIFQuadTreeFilterIndexGet<EventEvaluator>
                .Get(rect.X, rect.Y, rect.W, rect.H, _quadTree);
        }
    
        public override void Put(object filterConstant, EventEvaluator evaluator) {
            var rect = (XYWHRectangle) filterConstant;
            MXCIFQuadTreeFilterIndexSet<EventEvaluator>.Set(rect.X, rect.Y, rect.W, rect.H, evaluator, _quadTree);
        }
    
        public override void Remove(object filterConstant) {
            var rect = (XYWHRectangle) filterConstant;
            MXCIFQuadTreeFilterIndexDelete<EventEvaluator>.Delete(rect.X, rect.Y, rect.W, rect.H, _quadTree);
        }
    
        public override int Count => MXCIFQuadTreeFilterIndexCount.Count(_quadTree);
    
        public override bool IsEmpty => MXCIFQuadTreeFilterIndexEmpty.IsEmpty(_quadTree);
    
        public override IReaderWriterLock ReadWriteLock => _readWriteLock;
    }
} // end of namespace
