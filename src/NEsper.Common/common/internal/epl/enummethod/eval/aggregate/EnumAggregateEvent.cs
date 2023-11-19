///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.aggregate
{
    public class EnumAggregateEvent : EnumForgeBasePlain
    {
        private readonly ExprForge _initialization;
        private readonly ExprForge _innerExpression;
        private readonly ObjectArrayEventType _eventType;
        private readonly int _numParameters;

        public EnumAggregateEvent(
            int streamCountIncoming,
            ExprForge initialization,
            ExprForge innerExpression,
            ObjectArrayEventType eventType,
            int numParameters)
            : base(streamCountIncoming)
        {
            _initialization = initialization;
            _innerExpression = innerExpression;
            _eventType = eventType;
            _numParameters = numParameters;
        }

        public override EnumEval EnumEvaluator {
            get {
                var init = _initialization.ExprEvaluator;
                var inner = _innerExpression.ExprEvaluator;
                return new ProxyEnumEval(
                    (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        var value = init.Evaluate(eventsLambda, isNewData, context);

                        if (enumcoll.IsEmpty()) {
                            return value;
                        }

                        var beans = (ICollection<EventBean>)enumcoll;
                        var resultEvent = new ObjectArrayEventBean(new object[3], _eventType);
                        eventsLambda[StreamNumLambda] = resultEvent;
                        var props = resultEvent.Properties;
                        props[2] = enumcoll.Count;

                        var count = -1;
                        foreach (var next in beans) {
                            count++;
                            props[0] = value;
                            props[1] = count;
                            eventsLambda[StreamNumLambda + 1] = next;
                            value = inner.Evaluate(eventsLambda, isNewData, context);
                        }

                        return value;
                    });
            }
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var typeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF)));

            var innerType = _innerExpression.EvaluationType;
            var initType = _initialization.EvaluationType;
            if (initType != innerType && initType.GetBoxedType() == innerType) {
                initType = innerType;
            }

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(initType, typeof(EnumAggregateEvent), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block;
            block
                .DeclareVar(
                    initType,
                    "value",
                    _initialization.EvaluateCodegen(initType, methodNode, scope, codegenClassScope))
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(Ref("value"));
            block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance(
                        typeof(ObjectArrayEventBean),
                        NewArrayByLength(typeof(object), Constant(_numParameters - 1)),
                        typeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));
            if (_numParameters > 3) {
                block.AssignArrayElement(
                    "props",
                    Constant(2),
                    ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"));
            }

            if (_numParameters > 2) {
                block.DeclareVar<int>("count", Constant(-1));
            }

            var forEach = block
                .ForEach<EventBean>("next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("value"));

            if (_numParameters > 2) {
                forEach
                    .IncrementRef("count")
                    .AssignArrayElement("props", Constant(1), Ref("count"));
            }

            var innerCodegen = innerType == null
                ? ConstantNull()
                : InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope);

            forEach
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda + 1), Ref("next"))
                .AssignRef("value", innerCodegen)
                .BlockEnd();

            block.MethodReturn(Ref("value"));
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }

        public override int StreamNumSize => StreamNumLambda + 2;
    }
} // end of namespace