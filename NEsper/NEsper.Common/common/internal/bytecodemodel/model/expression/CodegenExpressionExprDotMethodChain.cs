///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionExprDotMethodChain : CodegenExpression
    {
        private readonly IList<CodegenChainElement> _chain = new List<CodegenChainElement>(2);
        private readonly CodegenExpression _expression;

        public CodegenExpressionExprDotMethodChain(CodegenExpression expression)
        {
            _expression = expression;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            _expression.Render(builder, isInnerClass, level, indent);
            foreach (var element in _chain) {
                builder.Append(".");
                element.Render(builder, isInnerClass);
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            foreach (var element in _chain) {
                element.MergeClasses(classes);
            }
        }

        public CodegenExpressionExprDotMethodChain Get(
            string propertyName)
        {
            _chain.Add(new CodegenChainPropertyElement(propertyName));
            return this;
        }

        public CodegenExpressionExprDotMethodChain Add(
            string method,
            params CodegenExpression[] @params)
        {
            _chain.Add(new CodegenChainMethodElement(method, null, @params));
            return this;
        }

        public CodegenExpressionExprDotMethodChain Add(
            string method,
            Type[] methodTypeParameters,
            params CodegenExpression[] @params)
        {
            _chain.Add(new CodegenChainMethodElement(method, methodTypeParameters, @params));
            return this;
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_expression);
            foreach (var element in _chain) {
                element.TraverseExpressions(consumer);
            }
        }
    }
} // end of namespace