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
    public class EnumOrderByAscDescScalarForge : EnumForgeBase,
        EnumEval
    {
        private readonly bool descending;

        public EnumOrderByAscDescScalarForge(
            int streamCountIncoming,
            bool descending)
            : base(streamCountIncoming)
        {
            this.descending = descending;
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
            if (enumcoll == null || enumcoll.IsEmpty()) {
                return enumcoll;
            }

            IList<object> list = new List<object>(enumcoll);
            if (descending) {
                Collections.SortInPlace(list, Collections.Reverse);
            }
            else {
                Collections.SortInPlace(list);
            }

            return list;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenBlock block = codegenMethodScope.MakeChild(typeof(ICollection<object>), typeof(EnumOrderByAscDescScalarForge), codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS).Block
                .IfCondition(Or(EqualsNull(EnumForgeCodegenNames.REF_ENUMCOLL), ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty")))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar(typeof(IList<object>), "list", NewInstance(typeof(List<object>), EnumForgeCodegenNames.REF_ENUMCOLL));
            if (descending) {
                block.StaticMethod(typeof(Collections), "sort", @Ref("list"), StaticMethod(typeof(Collections), "reverseOrder"));
            }
            else {
                block.StaticMethod(typeof(Collections), "sort", @Ref("list"));
            }

            CodegenMethod method = block.MethodReturn(@Ref("list"));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace