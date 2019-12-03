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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumDistinctScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumDistinctScalarLambdaForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumDistinctScalarLambdaForgeEval(
            EnumDistinctScalarLambdaForge forge,
            ExprEvaluator innerExpression)
        {
            _forge = forge;
            _innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.Count <= 1) {
                return enumcoll;
            }

            IDictionary<IComparable, object> distinct = new LinkedHashMap<IComparable, object>();
            var resultEvent = new ObjectArrayEventBean(new object[1], _forge.resultEventType);
            eventsLambda[_forge.StreamNumLambda] = resultEvent;
            var props = resultEvent.Properties;

            var values = (ICollection<object>) enumcoll;
            foreach (var next in values) {
                props[0] = next;

                var comparable = (IComparable) _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (!distinct.ContainsKey(comparable)) {
                    distinct.Put(comparable, next);
                }
            }

            return distinct.Values;
        }

        public static CodegenExpression Codegen(
            EnumDistinctScalarLambdaForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));
            var innerType = Boxing.GetBoxedType(forge.InnerExpression.EvaluationType);

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumDistinctScalarLambdaForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_OBJECT);

            var block = methodNode.Block
                .IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar<IDictionary<object, object>>("distinct", NewInstance(typeof(LinkedHashMap<object, object>)))
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), @Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(@Ref("resultEvent"), "Properties"));

            block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("next"))
                .DeclareVar(
                    innerType,
                    "comparable",
                    forge.InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope))
                .IfCondition(Not(ExprDotMethod(@Ref("distinct"), "ContainsKey", @Ref("comparable"))))
                .Expression(ExprDotMethod(@Ref("distinct"), "Put", @Ref("comparable"), @Ref("next")))
                .BlockEnd();
            block.MethodReturn(ExprDotName(@Ref("distinct"), "Values"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace