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
    public class ExprValidationMemberNameQualifiedRowRecogAgg : ExprValidationMemberName
    {
        private readonly int streamNum;

        public ExprValidationMemberNameQualifiedRowRecogAgg(int streamNum)
        {
            this.streamNum = streamNum;
        }

        public CodegenFieldName AggregationResultFutureRef()
        {
            return new CodegenFieldNameMatchRecognizeAgg(streamNum);
        }

        public CodegenFieldName PriorStrategy(int streamNum)
        {
            throw new IllegalStateException("Match-recognize measures-clauses not supported in subquery");
        }

        public CodegenFieldName PreviousStrategy(int streamNum)
        {
            throw new IllegalStateException("Match-recognize measures-clauses not supported with previous");
        }

        public CodegenFieldName PreviousMatchrecognizeStrategy()
        {
            throw new IllegalStateException("Match-recognize measures-clauses not supported in subquery");
        }
    }
} // end of namespace