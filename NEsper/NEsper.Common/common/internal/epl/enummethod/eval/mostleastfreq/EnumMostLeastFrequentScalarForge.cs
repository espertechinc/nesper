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
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMostLeastFrequentScalarForge : EnumForgeBase,
        EnumEval
    {
        private readonly bool _isMostFrequent;
        private readonly Type _returnType;

        public EnumMostLeastFrequentScalarForge(
            int streamCountIncoming,
            bool isMostFrequent,
            Type returnType)
            : base(streamCountIncoming)
        {
            _isMostFrequent = isMostFrequent;
            _returnType = returnType;
        }

        public override EnumEval EnumEvaluator {
            get => this;
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

            foreach (var next in enumcoll) {
                int? existing = items.Get(next);
                if (existing == null) {
                    existing = 1;
                }
                else {
                    existing++;
                }

                items.Put(next, existing.Value);
            }

            return EnumMostLeastFrequentEventForgeEval.GetEnumMostLeastFrequentResult(items, _isMostFrequent);
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var paramTypes = EnumForgeCodegenNames.PARAMS;
            var block = codegenMethodScope
                .MakeChild(_returnType.GetBoxedType(), typeof(EnumMostLeastFrequentScalarForge), codegenClassScope)
                .AddParam(paramTypes)
                .Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar<IDictionary<object, int>>("items", NewInstance(typeof(HashMap<object, int>)));
            var forEach = block
                .ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar<int?>("existing", ExprDotMethod(Ref("items"), "GetBoxed", Ref("next")))
                .IfCondition(EqualsNull(Ref("existing")))
                .AssignRef("existing", Constant(1))
                .IfElse()
                .IncrementRef("existing")
                .BlockEnd()
                .ExprDotMethod(Ref("items"), "Put", Ref("next"), Unbox(Ref("existing")));
            var method = block.MethodReturn(
                Cast(
                    _returnType,
                    StaticMethod(
                        typeof(EnumMostLeastFrequentEventForgeEval),
                        "GetEnumMostLeastFrequentResult",
                        Ref("items"),
                        Constant(_isMostFrequent))));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace