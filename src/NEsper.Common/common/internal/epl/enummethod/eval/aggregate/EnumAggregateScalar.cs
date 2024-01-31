///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class EnumAggregateScalar : EnumForgeBasePlain
    {
        private readonly ExprForge _initialization;
        private readonly ExprForge _innerExpression;
        private readonly ObjectArrayEventType _eventType;
        private readonly int _numParameters;

        public EnumAggregateScalar(
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

                        var @event = new ObjectArrayEventBean(new object[4], _eventType);
                        eventsLambda[StreamNumLambda] = @event;
                        var props = @event.Properties;
                        props[3] = enumcoll.Count;

                        var count = -1;
                        foreach (var next in enumcoll) {
                            count++;
                            props[0] = value;
                            props[1] = next;
                            props[2] = count;
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

            var initializationType = _initialization.EvaluationType;
            var innerType = _innerExpression.EvaluationType;
            if (innerType != initializationType && innerType == initializationType.GetBoxedType()) {
                initializationType = innerType;
            }

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(initializationType, typeof(EnumAggregateScalar), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);

            var block = methodNode.Block;

            block
                .DeclareVar(
                    initializationType,
                    "value",
                    _initialization.EvaluateCodegen(initializationType, methodNode, scope, codegenClassScope))
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(Ref("value"));

            block
                .DeclareVar<ObjectArrayEventBean>(
                    "@event",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(_numParameters)),
                        typeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("@event"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("@event"), "Properties"));

            if (_numParameters > 3) {
                block.AssignArrayElement(
                    "props",
                    Constant(3),
                    ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"));
            }

            if (_numParameters > 2) {
                block.DeclareVar<int>("count", Constant(-1));
            }

            var forEach = block.ForEachVar("next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("value"))
                .AssignArrayElement("props", Constant(1), Ref("next"));
            if (_numParameters > 2) {
                forEach
                    .IncrementRef("count")
                    .AssignArrayElement("props", Constant(2), Ref("count"));
            }

            if (innerType == null) {
                forEach.AssignRef("value", ConstantNull());
            }
            else {
                forEach.AssignRef(
                    "value",
                    _innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
            }

            forEach.BlockEnd();

            block.MethodReturn(Ref("value"));

            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }
    }
} // end of namespace