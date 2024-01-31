///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprValidationMemberNameQualifiedView : ExprValidationMemberName
    {
        private readonly int streamNumber;

        public ExprValidationMemberNameQualifiedView(int streamNumber)
        {
            this.streamNumber = streamNumber;
        }

        public CodegenFieldName AggregationResultFutureRef()
        {
            return new CodegenFieldNameViewAgg(streamNumber);
        }

        public CodegenFieldName PriorStrategy(int streamNum)
        {
            throw new UnsupportedOperationException("Not supported for views");
        }

        public CodegenFieldName PreviousStrategy(int streamNumber)
        {
            throw new UnsupportedOperationException("Not supported for views");
        }

        public CodegenFieldName PreviousMatchrecognizeStrategy()
        {
            throw new IllegalStateException("Match-recognize not supported in view parameters");
        }
    }
} // end of namespace