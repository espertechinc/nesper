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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotCollForge : ExprDotEvalRootChildInnerForge
    {
        private readonly ExprForge _rootForge;

        public InnerDotCollForge(ExprForge rootForge)
        {
            _rootForge = rootForge;
        }

        public ExprDotEvalRootChildInnerEval InnerEvaluator => new InnerDotCollEval(_rootForge.ExprEvaluator);

        public CodegenExpression CodegenEvaluate(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return Cast(
                typeof(ICollection<object>),
                _rootForge.EvaluateCodegen(
                    _rootForge.EvaluationType,
                    parentMethod,
                    exprSymbol,
                    codegenClassScope));
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public EventType EventTypeCollection => null;

        public Type ComponentTypeCollection => null;

        public EventType EventTypeSingle => null;

        public EPChainableType TypeInfo =>
            EPChainableTypeHelper.CollectionOfSingleValue(
                typeof(object));
    }
} // end of namespace