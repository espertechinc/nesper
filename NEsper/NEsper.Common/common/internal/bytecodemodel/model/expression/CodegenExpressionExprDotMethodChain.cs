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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionExprDotMethodChain : CodegenExpression
    {
        private readonly IList<CodegenChainElement> _chain = new List<CodegenChainElement>(2);
        private readonly CodegenExpression _expression;

        public CodegenExpressionExprDotMethodChain(CodegenExpression expression)
        {
            this._expression = expression;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            _expression.Render(builder, imports, isInnerClass);
            foreach (var element in _chain) {
                builder.Append(".");
                element.Render(builder, imports, isInnerClass);
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            foreach (var element in _chain) {
                element.MergeClasses(classes);
            }
        }

        public CodegenExpressionExprDotMethodChain Add(
            string method,
            params CodegenExpression[] @params)
        {
            _chain.Add(new CodegenChainElement(method, @params));
            return this;
        }
    }
} // end of namespace