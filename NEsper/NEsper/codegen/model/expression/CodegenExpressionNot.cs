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
    public class CodegenExpressionNot : ICodegenExpression
    {
        private readonly ICodegenExpression _expression;

        public CodegenExpressionNot(ICodegenExpression expression)
        {
            this._expression = expression;
        }

        public ICodegenExpression Expression => _expression;

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("!(");
            _expression.Render(textWriter);
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace