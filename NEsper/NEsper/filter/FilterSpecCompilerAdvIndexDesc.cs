///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.filter
{
    public class FilterSpecCompilerAdvIndexDesc
    {
        private readonly IList<ExprNode> _indexExpressions;
        private readonly IList<ExprNode> _keyExpressions;
        private readonly AdvancedIndexConfigContextPartition _indexSpec;
        private readonly string _indexType;
        private readonly string _indexName;
    
        public FilterSpecCompilerAdvIndexDesc(
            IList<ExprNode> indexExpressions,
            IList<ExprNode> keyExpressions,
            AdvancedIndexConfigContextPartition indexSpec,
            string indexType,
            string indexName)
        {
            _indexExpressions = indexExpressions;
            _keyExpressions = keyExpressions;
            _indexSpec = indexSpec;
            _indexType = indexType;
            _indexName = indexName;
        }

        public IList<ExprNode> IndexExpressions => _indexExpressions;

        public IList<ExprNode> KeyExpressions => _keyExpressions;

        public AdvancedIndexConfigContextPartition IndexSpec => _indexSpec;

        public string IndexName => _indexName;

        public string IndexType => _indexType;
    }
} // end of namespace
