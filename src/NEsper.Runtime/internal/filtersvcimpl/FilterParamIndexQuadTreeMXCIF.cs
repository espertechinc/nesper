///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterParamIndexQuadTreeMXCIF : FilterParamIndexLookupableBase
    {
        private readonly IReaderWriterLock _readWriteLock;
        private readonly MXCIFQuadTree _quadTree;
        private readonly FilterSpecLookupableAdvancedIndex _advancedIndex;

        private static readonly QuadTreeCollector<ICollection<FilterHandle>> COLLECTOR =
            new ProxyQuadTreeCollector<ICollection<FilterHandle>>()
            {
                ProcCollectInto = (@event, eventEvaluator, c, ctx) => ((EventEvaluator) eventEvaluator).MatchEvent(@event, c, ctx)
            };

        public FilterParamIndexQuadTreeMXCIF(IReaderWriterLock readWriteLock, ExprFilterSpecLookupable lookupable)
            : base(FilterOperator.ADVANCED_INDEX, lookupable)
        {
            _readWriteLock = readWriteLock;
            _advancedIndex = (FilterSpecLookupableAdvancedIndex) lookupable;
            var quadTreeConfig = _advancedIndex.QuadTreeConfig;
            _quadTree = MXCIFQuadTreeFactory.Make(
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
            var x = _advancedIndex.X.Get(theEvent).AsDouble();
            var y = _advancedIndex.Y.Get(theEvent).AsDouble();
            var width = _advancedIndex.Width.Get(theEvent).AsDouble();
            var height = _advancedIndex.Height.Get(theEvent).AsDouble();
            MXCIFQuadTreeFilterIndexCollect<ICollection<FilterHandle>>
                .CollectRange(_quadTree, x, y, width, height, theEvent, matches, COLLECTOR, ctx);
        }

        public override EventEvaluator Get(object filterConstant)
        {
            var rect = (XYWHRectangle) filterConstant;
            return (EventEvaluator) MXCIFQuadTreeFilterIndexGet.Get(rect.X, rect.Y, rect.W, rect.H, _quadTree);
        }

        public override void Put(object filterConstant, EventEvaluator evaluator)
        {
            var rect = (XYWHRectangle) filterConstant;
            MXCIFQuadTreeFilterIndexSet.Set(rect.X, rect.Y, rect.W, rect.H, evaluator, _quadTree);
        }

        public override void Remove(object filterConstant)
        {
            var rect = (XYWHRectangle) filterConstant;
            MXCIFQuadTreeFilterIndexDelete.Delete(rect.X, rect.Y, rect.W, rect.H, _quadTree);
        }

        public override int CountExpensive => MXCIFQuadTreeFilterIndexCount.Count(_quadTree);

        public override bool IsEmpty => MXCIFQuadTreeFilterIndexEmpty.IsEmpty(_quadTree);

        public override IReaderWriterLock ReadWriteLock => _readWriteLock;

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse, 
            ICollection<int> statementIds, 
            ArrayDeque<FilterItem> evaluatorStack)
        {
            evaluatorStack.AddFirst(new FilterItem(_advancedIndex.Expression, FilterOperator.ADVANCED_INDEX, this));
            MXCIFQuadTreeFilterIndexTraverse.Traverse(_quadTree, obj => {
                if (obj is FilterHandleSetNode filterHandleSetNode) {
                    filterHandleSetNode.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                    return;
                }
                if (obj is FilterHandle filterHandle) {
                    traverse.Invoke(evaluatorStack, filterHandle);
                }
            });
            evaluatorStack.RemoveFirst();
        }

    }
} // end of namespace