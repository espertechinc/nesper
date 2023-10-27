///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeStrategyRowFilteredUnselectedTable : SubselectForgeStrategyRowPlain
    {
        private readonly TableMetaData table;

        public SubselectForgeStrategyRowFilteredUnselectedTable(
            ExprSubselectRowNode subselect,
            TableMetaData table) : base(subselect)
        {
            this.table = table;
        }

        public override CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (Subselect.EvaluationType == null) {
                return ConstantNull();
            }

            var eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(table, classScope, GetType());
            var method = parent.MakeChild(Subselect.EvaluationType, GetType(), classScope);

            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);

            method.Block.DeclareVar<EventBean>("filtered", ConstantNull());
            var foreachX = method.Block.ForEach(
                typeof(EventBean),
                "@event",
                symbols.GetAddMatchingEvents(method));
            {
                foreachX.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"));
                var filter = CodegenLegoMethodExpression.CodegenExpression(
                    Subselect.filterExpr,
                    method,
                    classScope);
                CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                    foreachX,
                    typeof(bool?),
                    LocalMethod(
                        filter,
                        REF_EVENTS_SHIFTED,
                        symbols.GetAddIsNewData(method),
                        symbols.GetAddExprEvalCtx(method)));
                foreachX.IfCondition(NotEqualsNull(Ref("filtered")))
                    .BlockReturn(ConstantNull())
                    .AssignRef("filtered", Ref("@event"));
            }

            method.Block.IfRefNullReturnNull("filtered")
                .MethodReturn(
                    ExprDotMethod(
                        eventToPublic,
                        "convertToUnd",
                        Ref("filtered"),
                        symbols.GetAddEPS(method),
                        symbols.GetAddIsNewData(method),
                        symbols.GetAddExprEvalCtx(method)));
            return LocalMethod(method);
        }
    }
} // end of namespace