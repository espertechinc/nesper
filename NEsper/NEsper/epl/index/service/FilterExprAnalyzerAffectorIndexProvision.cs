///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.index.service
{
    public class FilterExprAnalyzerAffectorIndexProvision : FilterExprAnalyzerAffector
    {
        private readonly IList<ExprNode> _indexExpressions;
        private readonly IList<Pair<ExprNode, int[]>> _keyExpressions;
        private readonly string _operationName;
        private readonly int _streamNumIndex;

        public FilterExprAnalyzerAffectorIndexProvision(
            string operationName,
            IList<ExprNode> indexExpressions,
            IList<Pair<ExprNode, int[]>> keyExpressions,
            int streamNumIndex)
        {
            _operationName = operationName;
            _indexExpressions = indexExpressions;
            _keyExpressions = keyExpressions;
            _streamNumIndex = streamNumIndex;
        }

        public void Apply(QueryGraph queryGraph)
        {
            queryGraph.AddCustomIndex(_operationName, _indexExpressions, _keyExpressions, _streamNumIndex);
        }
    }
} // end of namespace