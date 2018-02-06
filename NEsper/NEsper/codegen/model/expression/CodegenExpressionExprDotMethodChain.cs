///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionExprDotMethodChain
        : ICodegenExpression
        , ICodegenExpressionExprDotMethodChain
    {
        private readonly ICodegenExpression expression;
        private readonly List<CodegenChainElement> chain = new List<CodegenChainElement>();

        public CodegenExpressionExprDotMethodChain(ICodegenExpression expression)
        {
            this.expression = expression;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            expression.Render(builder, imports);
            foreach (CodegenChainElement element in chain)
            {
                builder.Append(".");
                element.Render(builder);
            }
        }

        public ICodegenExpressionExprDotMethodChain AddNoParam(string method)
        {
            chain.Add(new CodegenChainElement(method, null));
            return this;
        }

        public ICodegenExpression AddWConst(string method, params object[] constants)
        {
            chain.Add(new CodegenChainElement(method, constants));
            return this;
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            expression.MergeClasses(classes);
            foreach (CodegenChainElement element in chain)
            {
                element.MergeClasses(classes);
            }
        }
    }
} // end of namespace