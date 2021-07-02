///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeStrategyRowFilteredUnselectedTable : SubselectForgeStrategyRowPlain
    {
        private readonly TableMetaData _table;

        public SubselectForgeStrategyRowFilteredUnselectedTable(
            ExprSubselectRowNode subselect,
            TableMetaData table)
            :
            base(subselect)
        {
            this._table = table;
        }

        public override CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.EvaluationType.IsNullType()) {
                return ConstantNull();
            }
            
            CodegenExpressionInstanceField eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(_table, classScope, GetType());
            CodegenMethod method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);

            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);

            method.Block.DeclareVar<EventBean>("filtered", ConstantNull());
            CodegenBlock @foreach = method.Block.ForEach(
                typeof(EventBean),
                "@event",
                symbols.GetAddMatchingEvents(method));
            {
                @foreach.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"));
                CodegenMethod filter = CodegenLegoMethodExpression.CodegenExpression(subselect.FilterExpr, method, classScope);
                CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                    @foreach,
                    typeof(bool?),
                    LocalMethod(
                        filter,
                        REF_EVENTS_SHIFTED,
                        symbols.GetAddIsNewData(method),
                        symbols.GetAddExprEvalCtx(method)));
                @foreach.IfCondition(NotEqualsNull(Ref("filtered")))
                    .BlockReturn(ConstantNull())
                    .AssignRef("filtered", Ref("@event"));
            }

            method.Block.IfRefNullReturnNull("filtered")
                .MethodReturn(
                    ExprDotMethod(
                        eventToPublic,
                        "ConvertToUnd",
                        Ref("filtered"),
                        symbols.GetAddEPS(method),
                        symbols.GetAddIsNewData(method),
                        symbols.GetAddExprEvalCtx(method)));
            return LocalMethod(method);
        }
    }
} // end of namespace