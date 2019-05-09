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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationAtBeanColl : ExprForge
    {
        internal readonly ExprEnumerationForge enumerationForge;
        private readonly EventType eventTypeColl;

        public ExprEvalEnumerationAtBeanColl(
            ExprEnumerationForge enumerationForge,
            EventType eventTypeColl)
        {
            this.enumerationForge = enumerationForge;
            this.eventTypeColl = eventTypeColl;
        }

        public ExprEvaluator ExprEvaluator {
            get { throw new IllegalStateException("Evaluator not available"); }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean[]), this.GetType(), codegenClassScope);
            methodNode.Block
                .DeclareVar(
                    typeof(object), "result", enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfCondition(And(NotEqualsNull(@Ref("result")), InstanceOf(@Ref("result"), typeof(ICollection<object>))))
                .DeclareVar(typeof(ICollection<object>), typeof(EventBean), "events", Cast(typeof(ICollection<object>), @Ref("result")))
                .BlockReturn(
                    Cast(
                        typeof(EventBean[]),
                        ExprDotMethod(@Ref("events"), "toArray", NewArrayByLength(typeof(EventBean), ExprDotMethod(@Ref("events"), "size")))))
                .MethodReturn(Cast(typeof(EventBean[]), @Ref("result")));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public Type EvaluationType {
            get => TypeHelper.GetArrayType(eventTypeColl.UnderlyingType);
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => enumerationForge.EnumForgeRenderable;
        }
    }
} // end of namespace