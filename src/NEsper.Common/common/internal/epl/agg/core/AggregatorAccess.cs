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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregatorAccess
    {
        void InitAccessForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope);

        void ApplyEnterCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        void ApplyLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        public void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope);

        void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenExpressionRef unitKey,
            CodegenClassScope classScope);

        void CollectFabricType(FabricTypeCollector collector);
    }
} // end of namespace