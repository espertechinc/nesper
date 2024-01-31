///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprStreamDesc
    {
        public SelectExprStreamDesc(SelectClauseStreamCompiledSpec streamSelected)
        {
            StreamSelected = streamSelected;
            ExpressionSelectedAsStream = null;
        }

        public SelectExprStreamDesc(SelectClauseExprCompiledSpec expressionSelectedAsStream)
        {
            ExpressionSelectedAsStream = expressionSelectedAsStream;
            StreamSelected = null;
        }

        public SelectClauseStreamCompiledSpec StreamSelected { get; }

        public SelectClauseExprCompiledSpec ExpressionSelectedAsStream { get; }
    }
} // end of namespace