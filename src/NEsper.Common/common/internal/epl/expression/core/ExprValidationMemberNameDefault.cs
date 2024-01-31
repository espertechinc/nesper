///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.name;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprValidationMemberNameDefault : ExprValidationMemberName
    {
        public static readonly ExprValidationMemberNameDefault INSTANCE = new ExprValidationMemberNameDefault();

        private ExprValidationMemberNameDefault()
        {
        }

        public CodegenFieldName AggregationResultFutureRef()
        {
            return CodegenFieldNameAgg.INSTANCE;
        }

        public CodegenFieldName PriorStrategy(int streamNum)
        {
            return new CodegenFieldNamePrior(streamNum);
        }

        public CodegenFieldName PreviousStrategy(int streamNumber)
        {
            return new CodegenFieldNamePrevious(streamNumber);
        }

        public CodegenFieldName PreviousMatchrecognizeStrategy()
        {
            return CodegenFieldNameMatchRecognizePrevious.INSTANCE;
        }
    }
} // end of namespace