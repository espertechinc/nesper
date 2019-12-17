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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMostLeastFrequentScalarLamdaForgeEval : EnumEval
    {
        private readonly EnumMostLeastFrequentScalarLamdaForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumMostLeastFrequentScalarLamdaForgeEval(
            EnumMostLeastFrequentScalarLamdaForge forge,
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
            if (enumcoll.IsEmpty()) {
                return null;
            }

            IDictionary<object, int> items = new LinkedHashMap<object, int>();
            var values = (ICollection<object>) enumcoll;

            var resultEvent = new ObjectArrayEventBean(new object[1], _forge.resultEventType);
            eventsLambda[_forge.StreamNumLambda] = resultEvent;
            var props = resultEvent.Properties;

            foreach (var next in values) {
                props[0] = next;

                var item = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                int? existing = items.Get(item);

                if (existing == null) {
                    existing = 1;
                }
                else {
                    existing++;
                }

                items.Put(item, existing.Value);
            }

            return EnumMostLeastFrequentEventForgeEval.GetEnumMostLeastFrequentResult(items, _forge.isMostFrequent);
        }

        public static CodegenExpression Codegen(
            EnumMostLeastFrequentScalarLamdaForge forge,
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
            var returnType = Boxing.GetBoxedType(forge.InnerExpression.EvaluationType);

            var scope = new ExprForgeCodegenSymbol(false, null);
            var paramTypes = EnumForgeCodegenNames.PARAMS;
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    returnType,
                    typeof(EnumMostLeastFrequentScalarLamdaForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(paramTypes);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar<IDictionary<object, int>>("items", NewInstance(typeof(HashMap<object, int>)))
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));

            block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("next"))
                .DeclareVar<object>(
                    "item",
                    forge.InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar<int?>("existing", ExprDotMethod(Ref("items"), "GetBoxed", Ref("item")))
                .IfCondition(EqualsNull(Ref("existing")))
                .AssignRef("existing", Constant(1))
                .IfElse()
                .Increment("existing")
                .BlockEnd()
                .ExprDotMethod(Ref("items"), "Put", Ref("item"), Unbox(Ref("existing")));
            block.MethodReturn(
                Cast(
                    returnType,
                    StaticMethod(
                        typeof(EnumMostLeastFrequentEventForgeEval),
                        "GetEnumMostLeastFrequentResult",
                        Ref("items"),
                        Constant(forge.isMostFrequent))));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace