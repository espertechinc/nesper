///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSharedBoolExprRepositoryImpl : FilterSharedBoolExprRepository
    {
        public static readonly FilterSharedBoolExprRepositoryImpl INSTANCE = new FilterSharedBoolExprRepositoryImpl();

        private FilterSharedBoolExprRepositoryImpl()
        {
        }

        public void RegisterBoolExpr(
            int statementId,
            FilterSpecParamExprNode node)
        {
        }

        public FilterSpecParamExprNode GetFilterBoolExpr(
            int statementId,
            int filterBoolExprNum)
        {
            throw new UnsupportedOperationException("Not provided by this implementation");
        }

        public void RemoveStatement(int statementId)
        {
        }
    }
} // end of namespace