///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredForgeRewriteWValue : ExprDeclaredForgeBase
    {
        private readonly ExprEnumerationForge[] _eventEnumerationForges;
        private readonly ObjectArrayEventType _valueEventType;
        private readonly IList<ExprNode> _valueExpressions;
        private ExprEvaluator[] _evaluators;

        public ExprDeclaredForgeRewriteWValue(
            ExprDeclaredNodeImpl parent,
            ExprForge innerForge,
            bool isCache,
            bool audit,
            string statementName,
            ExprEnumerationForge[] eventEnumerationForges,
            ObjectArrayEventType valueEventType,
            IList<ExprNode> valueExpressions)
            : base(parent, innerForge, isCache, audit, statementName)
        {
            _eventEnumerationForges = eventEnumerationForges;
            _valueEventType = valueEventType;
            _valueExpressions = valueExpressions;
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override EventBean[] GetEventsPerStreamRewritten(
            EventBean[] eps,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (_evaluators == null) {
                _evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_valueExpressions);
            }

            var props = new object[_valueEventType.PropertyNames.Length];
            for (var i = 0; i < _evaluators.Length; i++) {
                props[i] = _evaluators[i].Evaluate(eps, isNewData, context);
            }

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
            var method = codegenMethodScope.MakeChild(typeof(EventBean[]), typeof(ExprDeclaredForgeRewriteWValue), codegenClassScope);
            var valueType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(_valueEventType, EPStatementInitServicesConstants.REF)));

            method.Block
                .DeclareVar<object[]>("props", NewArrayByLength(typeof(object), Constant(_valueExpressions.Count)))
                .DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), Constant(_eventEnumerationForges.Length)))
                .AssignArrayElement("events", Constant(0), NewInstance(typeof(ObjectArrayEventBean), Ref("props"), valueType));
            for (var i = 0; i < _valueExpressions.Count; i++) {
                method.Block.AssignArrayElement(
                    "props",
                    Constant(i),
                    _valueExpressions[i].Forge.EvaluateCodegen(typeof(object), method, exprSymbol, codegenClassScope));
            }

            for (var i = 1; i < _eventEnumerationForges.Length; i++) {
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