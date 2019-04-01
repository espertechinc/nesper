///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class SortedAggregationStateFactoryFactory
    {
        private readonly EngineImportService _engineImportService;
        private readonly ExprEvaluator[] _evaluators;
        private readonly bool _ever;
        private readonly ExprEvaluator _optionalFilter;
        private readonly ExprAggMultiFunctionSortedMinMaxByNode _parent;
        private readonly bool[] _sortDescending;
        private readonly StatementExtensionSvcContext _statementExtensionSvcContext;
        private readonly int _streamNum;

        public SortedAggregationStateFactoryFactory(EngineImportService engineImportService,
            StatementExtensionSvcContext statementExtensionSvcContext, ExprEvaluator[] evaluators,
            bool[] sortDescending, bool ever, int streamNum, ExprAggMultiFunctionSortedMinMaxByNode parent,
            ExprEvaluator optionalFilter)
        {
            _engineImportService = engineImportService;
            _statementExtensionSvcContext = statementExtensionSvcContext;
            _evaluators = evaluators;
            _sortDescending = sortDescending;
            _ever = ever;
            _streamNum = streamNum;
            _parent = parent;
            _optionalFilter = optionalFilter;
        }

        public AggregationStateFactory MakeFactory()
        {
            var sortUsingCollator = _engineImportService.IsSortUsingCollator;
            var comparator = CollectionUtil.GetComparator(_evaluators, sortUsingCollator, _sortDescending);

            if (_ever)
            {
                var specX = new AggregationStateMinMaxByEverSpec(
                    _streamNum, _evaluators, _parent.IsMax, comparator, null, _optionalFilter);
                return _engineImportService.AggregationFactoryFactory.MakeMinMaxEver(_statementExtensionSvcContext,
                    _parent, specX);
            }

            var spec = new AggregationStateSortedSpec(_streamNum, _evaluators, comparator, null, _optionalFilter);
            return _engineImportService.AggregationFactoryFactory.MakeSorted(_statementExtensionSvcContext, _parent,
                spec);
        }
    }
} // end of namespace