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
using com.espertech.esper.collection;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.datetime
{
    public class FilterExprAnalyzerDTIntervalAffector : FilterExprAnalyzerAffector
    {
        private readonly DatetimeMethodEnum _currentMethod;
        private readonly EventType[] _typesPerStream;
        private readonly int _targetStreamNum;
        private readonly String _targetStartProp;
        private readonly String _targetEndProp;
        private readonly int? _parameterStreamNum;
        private readonly String _parameterStartProp;
        private readonly String _parameterEndProp;

    public FilterExprAnalyzerDTIntervalAffector(DatetimeMethodEnum currentMethod, EventType[] typesPerStream, int targetStreamNum, String targetStartProp, String targetEndProp, int? parameterStreamNum, String parameterStartProp, String parameterEndProp)
        {
            _currentMethod = currentMethod;
            _typesPerStream = typesPerStream;
            _targetStreamNum = targetStreamNum;
            _targetStartProp = targetStartProp;
            _targetEndProp = targetEndProp;
            _parameterStreamNum = parameterStreamNum;
            _parameterStartProp = parameterStartProp;
            _parameterEndProp = parameterEndProp;
        }

        public ExprNode[] IndexExpressions => null;

        public IList<Pair<ExprNode, int[]>> KeyExpressions => null;

        public AdvancedIndexConfigContextPartition OptionalIndexSpec => null;

        public String OptionalIndexName => null;

        public void Apply(QueryGraph queryGraph)
        {
            var parameterStreamNum = _parameterStreamNum.Value;
            if (_targetStreamNum == parameterStreamNum)
            {
                return;
            }


            var targetStartExpr = ExprNodeUtility.GetExprIdentNode(_typesPerStream, _targetStreamNum, _targetStartProp);
            var targetEndExpr = ExprNodeUtility.GetExprIdentNode(_typesPerStream, _targetStreamNum, _targetEndProp);
            var parameterStartExpr = ExprNodeUtility.GetExprIdentNode(_typesPerStream, parameterStreamNum, _parameterStartProp);
            var parameterEndExpr = ExprNodeUtility.GetExprIdentNode(_typesPerStream, parameterStreamNum, _parameterEndProp);

            if (targetStartExpr.ExprEvaluator.ReturnType != parameterStartExpr.ExprEvaluator.ReturnType)
            {
                return;
            }

            if (_currentMethod == DatetimeMethodEnum.BEFORE)
            {
                // a.end < b.start
                queryGraph.AddRelationalOpStrict(_targetStreamNum, targetEndExpr, parameterStreamNum, parameterStartExpr, RelationalOpEnum.LT);
            }
            else if (_currentMethod == DatetimeMethodEnum.AFTER)
            {
                // a.start > b.end
                queryGraph.AddRelationalOpStrict(_targetStreamNum, targetStartExpr, parameterStreamNum, parameterEndExpr, RelationalOpEnum.GT);
            }
            else if (_currentMethod == DatetimeMethodEnum.COINCIDES)
            {
                // a.startTimestamp = b.startTimestamp and a.endTimestamp = b.endTimestamp
                queryGraph.AddStrictEquals(_targetStreamNum, _targetStartProp, targetStartExpr,
                        parameterStreamNum, _parameterStartProp, parameterStartExpr);

                var noDuration = _parameterEndProp.Equals(_parameterStartProp) && _targetEndProp.Equals(_targetStartProp);
                if (!noDuration)
                {
                    var leftEndExpr = ExprNodeUtility.GetExprIdentNode(_typesPerStream, _targetStreamNum, _targetEndProp);
                    var rightEndExpr = ExprNodeUtility.GetExprIdentNode(_typesPerStream, parameterStreamNum, _parameterEndProp);
                    queryGraph.AddStrictEquals(_targetStreamNum, _targetEndProp, leftEndExpr,
                            parameterStreamNum, _parameterEndProp, rightEndExpr);
                }
            }
            else if (_currentMethod == DatetimeMethodEnum.DURING || _currentMethod == DatetimeMethodEnum.INCLUDES)
            {
                // DURING:   b.startTimestamp < a.startTimestamp <= a.endTimestamp < b.endTimestamp
                // INCLUDES: a.startTimestamp < b.startTimestamp <= b.endTimestamp < a.endTimestamp
                var relop = _currentMethod == DatetimeMethodEnum.DURING ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                queryGraph.AddRelationalOpStrict(parameterStreamNum, parameterStartExpr,
                        _targetStreamNum, targetStartExpr,
                        relop);

                queryGraph.AddRelationalOpStrict(_targetStreamNum, targetEndExpr,
                        parameterStreamNum, parameterEndExpr,
                        relop);
            }
            else if (_currentMethod == DatetimeMethodEnum.FINISHES || _currentMethod == DatetimeMethodEnum.FINISHEDBY)
            {
                // FINISHES:   b.startTimestamp < a.startTimestamp and a.endTimestamp = b.endTimestamp
                // FINISHEDBY: a.startTimestamp < b.startTimestamp and a.endTimestamp = b.endTimestamp
                var relop = _currentMethod == DatetimeMethodEnum.FINISHES ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                queryGraph.AddRelationalOpStrict(parameterStreamNum, parameterStartExpr,
                        _targetStreamNum, targetStartExpr,
                        relop);

                queryGraph.AddStrictEquals(_targetStreamNum, _targetEndProp, targetEndExpr,
                        parameterStreamNum, _parameterEndProp, parameterEndExpr);
            }
            else if (_currentMethod == DatetimeMethodEnum.MEETS)
            {
                // a.endTimestamp = b.startTimestamp
                queryGraph.AddStrictEquals(_targetStreamNum, _targetEndProp, targetEndExpr,
                        parameterStreamNum, _parameterStartProp, parameterStartExpr);
            }
            else if (_currentMethod == DatetimeMethodEnum.METBY)
            {
                // a.startTimestamp = b.endTimestamp
                queryGraph.AddStrictEquals(_targetStreamNum, _targetStartProp, targetStartExpr,
                        parameterStreamNum, _parameterEndProp, parameterEndExpr);
            }
            else if (_currentMethod == DatetimeMethodEnum.OVERLAPS || _currentMethod == DatetimeMethodEnum.OVERLAPPEDBY)
            {
                // OVERLAPS:     a.startTimestamp < b.startTimestamp < a.endTimestamp < b.endTimestamp
                // OVERLAPPEDBY: b.startTimestamp < a.startTimestamp < b.endTimestamp < a.endTimestamp
                var relop = _currentMethod == DatetimeMethodEnum.OVERLAPS ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                queryGraph.AddRelationalOpStrict(_targetStreamNum, targetStartExpr,
                        parameterStreamNum, parameterStartExpr,
                        relop);

                queryGraph.AddRelationalOpStrict(_targetStreamNum, targetEndExpr,
                        parameterStreamNum, parameterEndExpr,
                        relop);

                if (_currentMethod == DatetimeMethodEnum.OVERLAPS)
                {
                    queryGraph.AddRelationalOpStrict(parameterStreamNum, parameterStartExpr,
                            _targetStreamNum, targetEndExpr,
                            RelationalOpEnum.LT);
                }
                else
                {
                    queryGraph.AddRelationalOpStrict(_targetStreamNum, targetStartExpr,
                            parameterStreamNum, parameterEndExpr,
                            RelationalOpEnum.LT);
                }
            }
            else if (_currentMethod == DatetimeMethodEnum.STARTS || _currentMethod == DatetimeMethodEnum.STARTEDBY)
            {
                // STARTS:       a.startTimestamp = b.startTimestamp and a.endTimestamp < b.endTimestamp
                // STARTEDBY:    a.startTimestamp = b.startTimestamp and b.endTimestamp < a.endTimestamp
                queryGraph.AddStrictEquals(_targetStreamNum, _targetStartProp, targetStartExpr,
                        parameterStreamNum, _parameterStartProp, parameterStartExpr);

                var relop = _currentMethod == DatetimeMethodEnum.STARTS ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                queryGraph.AddRelationalOpStrict(_targetStreamNum, targetEndExpr,
                        parameterStreamNum, parameterEndExpr,
                        relop);
            }
        }

    }
}
