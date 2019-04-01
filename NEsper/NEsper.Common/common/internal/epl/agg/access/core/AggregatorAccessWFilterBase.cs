///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public abstract class AggregatorAccessWFilterBase : AggregatorAccess
    {
        internal readonly ExprNode optionalFilter;

        internal abstract void ApplyEnterFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        internal abstract void ApplyLeaveFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        public AggregatorAccessWFilterBase(ExprNode optionalFilter)
        {
            this.optionalFilter = optionalFilter;
        }

        public void ApplyEnterCodegen(CodegenMethod method, ExprForgeCodegenSymbol symbols, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            if (optionalFilter != null)
            {
                AggregatorCodegenUtil.PrefixWithFilterCheck(optionalFilter.Forge, method, symbols, classScope);
            }
            ApplyEnterFiltered(method, symbols, classScope, namedMethods);
        }

        public void ApplyLeaveCodegen(CodegenMethod method, ExprForgeCodegenSymbol symbols, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            if (optionalFilter != null)
            {
                AggregatorCodegenUtil.PrefixWithFilterCheck(optionalFilter.Forge, method, symbols, classScope);
            }
            ApplyLeaveFiltered(method, symbols, classScope, namedMethods);
        }

        public abstract void ClearCodegen(
            CodegenMethod method, CodegenClassScope classScope);

        public abstract void WriteCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef @ref, CodegenExpressionRef unitKey,
            CodegenExpressionRef output, CodegenMethod method, CodegenClassScope classScope);

        public abstract void ReadCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenMethod method, CodegenExpressionRef unitKey,
            CodegenClassScope classScope);
    }
} // end of namespace