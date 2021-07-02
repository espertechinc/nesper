///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRExistsDefault : SubselectForgeNR
    {
        private readonly ExprForge _filterEval;
        private readonly ExprForge _havingEval;

        public SubselectForgeNRExistsDefault(
            ExprForge filterEval,
            ExprForge havingEval)
        {
            this._filterEval = filterEval;
            this._havingEval = havingEval;
        }

        public CodegenExpression EvaluateMatchesCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool), GetType(), classScope);
            method.Block.ApplyTri(new ReturnIfNoMatch(ConstantFalse(), ConstantFalse()), method, symbols);

            if (_filterEval == null && _havingEval == null) {
                method.Block.MethodReturn(ConstantTrue());
                return LocalMethod(method);
            }

            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);
            if (_havingEval != null) {
                throw new UnsupportedOperationException();
            }

            var filter = CodegenLegoMethodExpression.CodegenExpression(_filterEval, method, classScope);
            method.Block
                .ForEach(typeof(EventBean), "subselectEvent", symbols.GetAddMatchingEvents(method))
                .AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("subselectEvent"))
                .DeclareVar<bool?>(
                    "pass",
                    LocalMethod(filter, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method)))
                .IfCondition(And(NotEqualsNull(Ref("pass")), Unbox(Ref("pass"))))
                .BlockReturn(ConstantTrue())
                .BlockEnd()
                .MethodReturn(ConstantFalse());
            return LocalMethod(method);
        }
    }
} // end of namespace