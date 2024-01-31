///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecCompilerAdvIndexDesc
    {
        public FilterSpecCompilerAdvIndexDesc(
            ExprNode[] indexExpressions,
            ExprNode[] keyExpressions,
            AdvancedIndexConfigContextPartition indexSpec,
            string indexType,
            string indexName)
        {
            IndexExpressions = indexExpressions;
            KeyExpressions = keyExpressions;
            IndexSpec = indexSpec;
            IndexType = indexType;
            IndexName = indexName;
        }

        public ExprNode[] IndexExpressions { get; }

        public ExprNode[] KeyExpressions { get; }

        public AdvancedIndexConfigContextPartition IndexSpec { get; }

        public string IndexName { get; }

        public string IndexType { get; }
    }
} // end of namespace