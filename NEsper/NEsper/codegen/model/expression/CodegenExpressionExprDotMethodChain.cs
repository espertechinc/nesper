///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionExprDotMethodChain
        : ICodegenExpression
        , ICodegenExpressionExprDotMethodChain
    {
        private readonly ICodegenExpression _expression;
        private readonly List<CodegenChainElement> _chain = new List<CodegenChainElement>();

        public CodegenExpressionExprDotMethodChain(ICodegenExpression expression)
        {
            this._expression = expression;
        }

        public void Render(TextWriter textWriter)
        {
            _expression.Render(textWriter);
            foreach (CodegenChainElement element in _chain)
            {
                textWriter.Write(".");
                element.Render(textWriter);
            }
        }

        public ICodegenExpressionExprDotMethodChain AddNoParam(string method)
        {
            _chain.Add(new CodegenChainElement(method, null));
            return this;
        }

        public ICodegenExpression AddWConst(string method, params object[] constants)
        {
            _chain.Add(new CodegenChainElement(method, constants));
            return this;
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
            foreach (CodegenChainElement element in _chain)
            {
                element.MergeClasses(classes);
            }
        }
    }
} // end of namespace