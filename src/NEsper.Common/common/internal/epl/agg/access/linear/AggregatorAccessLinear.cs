///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public interface AggregatorAccessLinear : AggregatorAccess
    {
        CodegenExpression SizeCodegen();

        CodegenExpression EnumeratorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenNamedMethods namedMethods);

        CodegenExpression CollectionReadOnlyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        CodegenExpression GetLastValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenNamedMethods namedMethods);

        CodegenExpression GetFirstValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod method);

        CodegenExpression GetFirstNthValueCodegen(
            CodegenExpressionRef index,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        CodegenExpression GetLastNthValueCodegen(
            CodegenExpressionRef index,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);
    }
} // end of namespace