///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgeUnpackCollEventBean : ExprDotForge,
        ExprDotEval
    {
        public ExprDotForgeUnpackCollEventBean(EventType type)
        {
            TypeInfo = EPChainableTypeHelper.CollectionOfSingleValue(
                type.UnderlyingType);
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
            else if (target is FlexCollection flexCollection) {
                return new EventUnderlyingCollection<object>(flexCollection.EventBeanCollection);
            }

            return new EventUnderlyingCollection<object>(target.Unwrap<EventBean>());
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var returnType = TypeInfo.GetCodegenReturnType();
            var methodNode = parent
                .MakeChild(returnType, typeof(ExprDotForgeUnpackCollEventBean), classScope)
                .AddParam(innerType, "target");

            var collectionType = typeof(EventUnderlyingCollection<>).MakeGenericType(returnType.GetComponentType());
            
            methodNode.Block
                .IfRefNullReturnNull("target")
                .MethodReturn(NewInstance(collectionType, Ref("target")));
            
            return LocalMethod(methodNode, inner);
        }

        public EPChainableType TypeInfo { get; }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEventColl();
        }

        public ExprDotEval DotEvaluator => this;

        public ExprDotForge DotForge => this;
    }
} // end of namespace