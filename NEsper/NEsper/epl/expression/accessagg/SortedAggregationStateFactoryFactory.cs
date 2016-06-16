///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class SortedAggregationStateFactoryFactory
    {
        private readonly MethodResolutionService _methodResolutionService;
        private readonly ExprEvaluator[] _evaluators;
        private readonly bool[] _sortDescending;
        private readonly bool _ever;
        private readonly int _streamNum;
        private readonly ExprAggMultiFunctionSortedMinMaxByNode _parent;
    
        public SortedAggregationStateFactoryFactory(MethodResolutionService methodResolutionService, ExprEvaluator[] evaluators, bool[] sortDescending, bool ever, int streamNum, ExprAggMultiFunctionSortedMinMaxByNode parent)
        {
            _methodResolutionService = methodResolutionService;
            _evaluators = evaluators;
            _sortDescending = sortDescending;
            _ever = ever;
            _streamNum = streamNum;
            _parent = parent;
        }
    
        public AggregationStateFactory MakeFactory()
        {
            var sortUsingCollator = _methodResolutionService.IsSortUsingCollator;
            var comparator = CollectionUtil.GetComparator(_evaluators, sortUsingCollator, _sortDescending);
            var criteriaKeyBinding = _methodResolutionService.GetCriteriaKeyBinding(_evaluators);
    
            AggregationStateFactory factory;
            if (_ever) {
                var spec = new AggregationStateMinMaxByEverSpec(_streamNum, _evaluators, _parent.IsMax, comparator, criteriaKeyBinding);
                factory = new ProxyAggregationStateFactory() {
                    ProcCreateAccess = (methodResolutionService, agentInstanceId, groupId, aggregationId, join, groupKey, passThru) =>
                        methodResolutionService.MakeAccessAggMinMaxEver(agentInstanceId, groupId, aggregationId, spec, passThru),
                    ProcAggregationExpression = () => _parent,
                };
            }
            else {
                var spec = new AggregationStateSortedSpec(_streamNum, _evaluators, comparator, criteriaKeyBinding);
                factory = new ProxyAggregationStateFactory() {
                    ProcCreateAccess = (methodResolutionService, agentInstanceId, groupId, aggregationId, join, groupKey, passThru) =>
                    {
                        if (join) {
                            return methodResolutionService.MakeAccessAggSortedJoin(agentInstanceId, groupId, aggregationId, spec, passThru);
                        }
                        return methodResolutionService.MakeAccessAggSortedNonJoin(agentInstanceId, groupId, aggregationId, spec, passThru);
                    },
    
                    ProcAggregationExpression = () => _parent,
                };
            }
            return factory;
        }
    }
}
