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
    public interface ExprValidationMemberName
    {
        CodegenFieldName AggregationResultFutureRef();

        CodegenFieldName PriorStrategy(int streamNum);

        CodegenFieldName PreviousStrategy(int streamNumber);

        CodegenFieldName PreviousMatchrecognizeStrategy();
    }
} // end of namespace