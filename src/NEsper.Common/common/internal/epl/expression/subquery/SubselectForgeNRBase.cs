///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.subquery.ExprSubselectEvalMatchSymbol;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeNRSymbol;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public abstract class SubselectForgeNRBase : SubselectForgeNR
    {
        protected readonly ExprSubselectNode subselect;
        protected readonly ExprForge valueEval;
        protected readonly ExprForge selectEval;
        private readonly bool resultWhenNoMatchingEvents;

        protected abstract CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope);

        public SubselectForgeNRBase(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents)
        {
            this.subselect = subselect;
            this.valueEval = valueEval;
            this.selectEval = selectEval;
            this.resultWhenNoMatchingEvents = resultWhenNoMatchingEvents;
        }

        public CodegenExpression EvaluateMatchesCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.EvaluationType == null || valueEval.EvaluationType == null) {
                return ConstantNull();
            }

            var method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);
            method.Block
                .ApplyTri(
                    new ReturnIfNoMatch(Constant(resultWhenNoMatchingEvents), Constant(resultWhenNoMatchingEvents)),
                    method,
                    symbols)
                .DeclareVar(
                    valueEval.EvaluationType,
                    "leftResult",
                    valueEval.EvaluateCodegen(valueEval.EvaluationType, parent, symbols, classScope))
                .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);

            var leftResultType = valueEval.EvaluationType.GetBoxedType();
            var nrSymbols = new SubselectForgeNRSymbol(leftResultType);
            var child = parent.MakeChildWithScope(subselect.EvaluationType, GetType(), nrSymbols, classScope)
                .AddParam(leftResultType, NAME_LEFTRESULT)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam(EPTYPE_COLLECTION_EVENTBEAN, NAME_MATCHINGEVENTS)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            child.Block.MethodReturn(CodegenEvaluateInternal(child, nrSymbols, classScope));
            method.Block.MethodReturn(
                LocalMethod(
                    child,
                    REF_LEFTRESULT,
                    REF_EVENTS_SHIFTED,
                    symbols.GetAddIsNewData(method),
                    symbols.GetAddMatchingEvents(method),
                    symbols.GetAddExprEvalCtx(method)));

            return LocalMethod(method);
        }
    }
} // end of namespace