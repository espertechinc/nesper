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

using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementReturnExpression : CodegenStatementBase, ICodegenStatement
    {
        private readonly ICodegenExpression _expression;

        public CodegenStatementReturnExpression(ICodegenExpression expression)
        {
            this._expression = expression;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            textWriter.Write("return ");
            _expression.Render(textWriter);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace