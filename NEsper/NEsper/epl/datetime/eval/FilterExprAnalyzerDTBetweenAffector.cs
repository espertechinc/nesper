///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.datetime.eval
{
    public class FilterExprAnalyzerDTBetweenAffector : FilterExprAnalyzerAffector
    {
        private readonly EventType[] _typesPerStream;
        private readonly int _targetStreamNum;
        private readonly string _targetPropertyName;
        private readonly ExprNode _start;
        private readonly ExprNode _end;
        private readonly bool _includeLow;
        private readonly bool _includeHigh;

        public FilterExprAnalyzerDTBetweenAffector(EventType[] typesPerStream, int targetStreamNum,
            string targetPropertyName, ExprNode start, ExprNode end, bool includeLow, bool includeHigh)
        {
            _typesPerStream = typesPerStream;
            _targetStreamNum = targetStreamNum;
            _targetPropertyName = targetPropertyName;
            _start = start;
            _end = end;
            _includeLow = includeLow;
            _includeHigh = includeHigh;
        }

        public void Apply(QueryGraph queryGraph)
        {
            ExprIdentNode targetExpr =
                ExprNodeUtility.GetExprIdentNode(_typesPerStream, _targetStreamNum, _targetPropertyName);
            RangeFilterAnalyzer.Apply(targetExpr, _start, _end, _includeLow, _includeHigh, false, queryGraph);
        }

        public ExprNode[] IndexExpressions => null;

        public IList<Pair<ExprNode, int[]>> KeyExpressions => null;

        public AdvancedIndexConfigContextPartition OptionalIndexSpec => null;

        public string OptionalIndexName => null;
    }
} // end of namespace
