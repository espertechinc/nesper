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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgeUnpackCollEventBean : ExprDotForge,
        ExprDotEval
    {
        private readonly EPType typeInfo;

        public ExprDotForgeUnpackCollEventBean(EventType type)
        {
            typeInfo = EPTypeHelper.CollectionOfSingleValue(type.UnderlyingType);
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            ICollection<EventBean> it = (ICollection<EventBean>) target;
            return new EventUnderlyingCollection(it);
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(ExprDotForgeUnpackCollEventBean), codegenClassScope)
                .AddParam(typeof(ICollection<object>), "target");

            methodNode.Block
                .IfRefNullReturnNull("target")
                .MethodReturn(NewInstance<EventUnderlyingCollection>(@Ref("target")));
            return LocalMethod(methodNode, inner);
        }

        public EPType TypeInfo {
            get => typeInfo;
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEventColl();
        }

        public ExprDotEval DotEvaluator {
            get => this;
        }

        public ExprDotForge DotForge {
            get => this;
        }
    }
} // end of namespace