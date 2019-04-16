///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    ///     Represents a subselect in an expression tree.
    /// </summary>
    public class ExprSubselectInNode : ExprSubselectNode
    {
        private SubselectForgeNR evalStrategy;

        public ExprSubselectInNode(
            StatementSpecRaw statementSpec,
            bool isNotIn)
            : base(statementSpec)
        {
            IsNotIn = isNotIn;
        }

        public override Type EvaluationType => typeof(bool?);

        /// <summary>
        ///     Returns true for not-in, or false for in.
        /// </summary>
        /// <returns>true for not-in</returns>
        public bool IsNotIn { get; }

        public override bool IsAllowMultiColumnSelect => false;

        public override Type ComponentTypeCollection => null;

        public override void ValidateSubquery(ExprValidationContext validationContext)
        {
            evalStrategy = SubselectNRForgeFactory.CreateStrategyAnyAllIn(
                this, IsNotIn, false, false, null, validationContext.ImportService);
        }

        public override IDictionary<string, object> TypableGetRowProperties()
        {
            return null;
        }

        protected override CodegenExpression EvalMatchesPlainCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return evalStrategy.EvaluateMatchesCodegen(parent, symbols, classScope);
        }

        public override EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        protected override CodegenExpression EvalMatchesGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        protected override CodegenExpression EvalMatchesGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public override EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        protected override CodegenExpression EvalMatchesGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        protected override CodegenExpression EvalMatchesTypableSingleCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        protected override CodegenExpression EvalMatchesTypableMultiCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace