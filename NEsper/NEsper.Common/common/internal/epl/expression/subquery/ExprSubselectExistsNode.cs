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
    ///     Represents an exists-subselect in an expression tree.
    /// </summary>
    public class ExprSubselectExistsNode : ExprSubselectNode
    {
        private SubselectForgeNR subselectEvalStrategyNR;

        public ExprSubselectExistsNode(StatementSpecRaw statementSpec) : base(statementSpec)
        {
        }

        public override Type EvaluationType => typeof(bool?);

        public override Type ComponentTypeCollection => null;

        public override bool IsAllowMultiColumnSelect => false;

        public override void ValidateSubquery(ExprValidationContext validationContext)
        {
            subselectEvalStrategyNR = SubselectNRForgeFactory.CreateStrategyExists(this);
        }

        public override IDictionary<string, object> TypableGetRowProperties()
        {
            return null;
        }

        protected override CodegenExpression EvalMatchesPlainCodegen(
            CodegenMethodScope parent, ExprSubselectEvalMatchSymbol symbols, CodegenClassScope classScope)
        {
            return subselectEvalStrategyNR.EvaluateMatchesCodegen(parent, symbols, classScope);
        }

        public override EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        protected override CodegenExpression EvalMatchesGetCollEventsCodegen(
            CodegenMethodScope parent, ExprSubselectEvalMatchSymbol symbols, CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        protected override CodegenExpression EvalMatchesGetCollScalarCodegen(
            CodegenMethodScope parent, ExprSubselectEvalMatchSymbol symbols, CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        protected override CodegenExpression EvalMatchesTypableMultiCodegen(
            CodegenMethodScope parent, ExprSubselectEvalMatchSymbol symbols, CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public override EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        protected override CodegenExpression EvalMatchesTypableSingleCodegen(
            CodegenMethodScope parent, ExprSubselectEvalMatchSymbol symbols, CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        protected override CodegenExpression EvalMatchesGetEventBeanCodegen(
            CodegenMethodScope parent, ExprSubselectEvalMatchSymbol symbols, CodegenClassScope classScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace