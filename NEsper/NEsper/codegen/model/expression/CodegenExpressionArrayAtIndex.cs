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
    public class CodegenExpressionArrayAtIndex : ICodegenExpression
    {
        private readonly ICodegenExpression _expression;
        private readonly ICodegenExpression _index;

        public CodegenExpressionArrayAtIndex(ICodegenExpression expression, ICodegenExpression index)
        {
            this._expression = expression;
            this._index = index;
        }

        public void Render(TextWriter textWriter)
        {
            _expression.Render(textWriter);
            textWriter.Write("[");
            _index.Render(textWriter);
            textWriter.Write("]");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
            _index.MergeClasses(classes);
        }
    }
} // end of namespace