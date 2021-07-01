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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredForgeRewrite : ExprDeclaredForgeBase
    {
        private readonly ExprEnumerationForge[] _eventEnumerationForges;

        public ExprDeclaredForgeRewrite(
            ExprDeclaredNodeImpl parent,
            ExprForge innerForge,
            bool isCache,
            ExprEnumerationForge[] eventEnumerationForges,
            bool audit,
            string statementName)
            : base(parent, innerForge, isCache, audit, statementName)
        {
            _eventEnumerationForges = eventEnumerationForges;
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override EventBean[] GetEventsPerStreamRewritten(
            EventBean[] eps,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            // rewrite streams
            var events = new EventBean[_eventEnumerationForges.Length];
            for (var i = 0; i < _eventEnumerationForges.Length; i++) {
                events[i] = _eventEnumerationForges[i].ExprEvaluatorEnumeration.EvaluateGetEventBean(eps, isNewData, context);
            }

            return events;
        }

        protected override CodegenExpression CodegenEventsPerStreamRewritten(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(typeof(EventBean[]), typeof(ExprDeclaredForgeRewrite), codegenClassScope);
            method.Block.DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), Constant(_eventEnumerationForges.Length)));
            for (var i = 0; i < _eventEnumerationForges.Length; i++) {
                method.Block.AssignArrayElement(
                    "events",
                    Constant(i),
                    _eventEnumerationForges[i].EvaluateGetEventBeanCodegen(method, exprSymbol, codegenClassScope));
            }

            method.Block.MethodReturn(Ref("events"));
            return LocalMethod(method);
        }
    }
} // end of namespace