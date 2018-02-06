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

using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.mergeClassesExpressions;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.renderExpressions;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementExprDotMethod : CodegenStatementBase
    {
        private readonly ICodegenExpression _expression;
        private readonly string _method;
        private readonly ICodegenExpression[] _parameters;

        public CodegenStatementExprDotMethod(ICodegenExpression expression, string method, ICodegenExpression[] parameters)
        {
            this._expression = expression;
            this._method = method;
            this._parameters = parameters;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            if (_expression is CodegenExpressionRef)
            {
                _expression.Render(textWriter);
            }
            else
            {
                textWriter.Write("(");
                _expression.Render(textWriter);
                textWriter.Write(")");
            }

            textWriter.Write('.');
            textWriter.Write(_method);
            textWriter.Write("(");
            CodegenExpressionBuilder.RenderExpressions(textWriter, _parameters);
            textWriter.Write(")");
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _parameters);
        }
    }
} // end of namespace