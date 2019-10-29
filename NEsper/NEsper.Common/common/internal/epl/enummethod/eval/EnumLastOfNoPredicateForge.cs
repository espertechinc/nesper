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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumLastOfNoPredicateForge : EnumForgeBase,
        EnumForge,
        EnumEval
    {
        private readonly EPType resultType;

        public EnumLastOfNoPredicateForge(
            int streamCountIncoming,
            EPType resultType)
            : base(streamCountIncoming)
        {
            this.resultType = resultType;
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
            object result = null;
            foreach (var next in enumcoll) {
                result = next;
            }

            return result;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var type = Boxing.GetBoxedType(EPTypeHelper.GetCodegenReturnType(resultType));
            var paramTypes = (type == typeof(EventBean))
                ? EnumForgeCodegenNames.PARAMS_EVENTBEAN
                : EnumForgeCodegenNames.PARAMS_OBJECT;
            
            var method = codegenMethodScope
                .MakeChild(type, typeof(EnumLastOfNoPredicateForge), codegenClassScope)
                .AddParam(paramTypes)
                .Block
                .DeclareVar<object>("result", ConstantNull())
                .ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignRef("result", @Ref("next"))
                .BlockEnd()
                .MethodReturn(Cast(type, @Ref("result")));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace