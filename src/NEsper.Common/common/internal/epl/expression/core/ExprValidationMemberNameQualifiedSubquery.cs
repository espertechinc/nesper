///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprValidationMemberNameQualifiedSubquery : ExprValidationMemberName
    {
        private readonly int _subqueryNum;

        public ExprValidationMemberNameQualifiedSubquery(int subqueryNum)
        {
            this._subqueryNum = subqueryNum;
        }

        public CodegenFieldName AggregationResultFutureRef()
        {
            return new CodegenFieldNameSubqueryAgg(_subqueryNum);
        }

        public CodegenFieldName PriorStrategy(int streamNum)
        {
            return new CodegenFieldNameSubqueryPrior(_subqueryNum);
        }

        public CodegenFieldName PreviousStrategy(int streamNum)
        {
            return new CodegenFieldNameSubqueryPrevious(_subqueryNum);
        }

        public CodegenFieldName PreviousMatchrecognizeStrategy()
        {
            throw new IllegalStateException("Match-recognize not supported in subquery");
        }
    }
} // end of namespace