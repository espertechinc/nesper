///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterParamIndexQuadTreePointRegion : FilterParamIndexLookupableBase
    {
        private readonly IReaderWriterLock _readWriteLock;
        private readonly PointRegionQuadTree<object> _quadTree;
        private readonly FilterSpecLookupableAdvancedIndex _advancedIndex;

        private static readonly QuadTreeCollector<ICollection<FilterHandle>> _collector =
            new ProxyQuadTreeCollector<ICollection<FilterHandle>>()
            {
                ProcCollectInto = (@event, eventEvaluator, c, ctx) => ((EventEvaluator) eventEvaluator).MatchEvent(@event, c, ctx)
            };

        public FilterParamIndexQuadTreePointRegion(IReaderWriterLock readWriteLock, ExprFilterSpecLookupable lookupable)
            : base(FilterOperator.ADVANCED_INDEX, lookupable)
        {
            _readWriteLock = readWriteLock;
            _advancedIndex = (FilterSpecLookupableAdvancedIndex) lookupable;
            var quadTreeConfig = _advancedIndex.QuadTreeConfig;
            _quadTree = PointRegionQuadTreeFactory<object>.Make(
                quadTreeConfig.X,
                quadTreeConfig.Y,
                quadTreeConfig.Width,
                quadTreeConfig.Height);
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            var x = (_advancedIndex.X.Get(theEvent)).AsDouble();
            var y = (_advancedIndex.Y.Get(theEvent)).AsDouble();
            var width = (_advancedIndex.Width.Get(theEvent)).AsDouble();
            var height = (_advancedIndex.Height.Get(theEvent)).AsDouble();
            PointRegionQuadTreeFilterIndexCollect<EventEvaluator, ICollection<FilterHandle>>.CollectRange(
                _quadTree, x, y, width, height, theEvent, matches, _collector, ctx);
        }

        public override EventEvaluator Get(object filterConstant)
        {
            var point = (XYPoint) filterConstant;
            return PointRegionQuadTreeFilterIndexGet<EventEvaluator>.Get(point.X, point.Y, _quadTree);
        }

        public override void Put(object filterConstant, EventEvaluator evaluator)
        {
            var point = (XYPoint) filterConstant;
            PointRegionQuadTreeFilterIndexSet<EventEvaluator>.Set(point.X, point.Y, evaluator, _quadTree);
        }

        public override void Remove(object filterConstant)
        {
            var point = (XYPoint) filterConstant;
            PointRegionQuadTreeFilterIndexDelete<EventEvaluator>.Delete(point.X, point.Y, _quadTree);
        }

        public override int CountExpensive => PointRegionQuadTreeFilterIndexCount.Count(_quadTree);

        public override bool IsEmpty => PointRegionQuadTreeFilterIndexEmpty.IsEmpty(_quadTree);

        public override IReaderWriterLock ReadWriteLock => _readWriteLock;

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            evaluatorStack.AddFirst(new FilterItem(_advancedIndex.Expression, FilterOperator.ADVANCED_INDEX, this));
            PointRegionQuadTreeFilterIndexTraverse.Traverse(_quadTree, @object => {
                if (@object is FilterHandleSetNode filterHandleSetNode) {
                    filterHandleSetNode.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                    return;
                }
                if (@object is FilterHandle filterHandle) {
                    traverse.Invoke(evaluatorStack, filterHandle);
                }
            });
            evaluatorStack.RemoveFirst();
        }

    }
} // end of namespace