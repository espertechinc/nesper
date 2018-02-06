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
    public class CodegenExpressionConditional : ICodegenExpression
    {
        private readonly ICodegenExpression _condition;
        private readonly ICodegenExpression _expressionTrue;
        private readonly ICodegenExpression _expressionFalse;

        public CodegenExpressionConditional(ICodegenExpression condition, ICodegenExpression expressionTrue, ICodegenExpression expressionFalse)
        {
            this._condition = condition;
            this._expressionTrue = expressionTrue;
            this._expressionFalse = expressionFalse;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("(");
            _condition.Render(textWriter);
            textWriter.Write(" ? ");
            _expressionTrue.Render(textWriter);
            textWriter.Write(" : ");
            _expressionFalse.Render(textWriter);
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _condition.MergeClasses(classes);
            _expressionTrue.MergeClasses(classes);
            _expressionFalse.MergeClasses(classes);
        }
    }
} // end of namespace