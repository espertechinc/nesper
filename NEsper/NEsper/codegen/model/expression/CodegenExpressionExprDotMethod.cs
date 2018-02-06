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

//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.mergeClassesExpressions;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.renderExpressions;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionExprDotMethod : ICodegenExpression
    {
        private readonly ICodegenExpression _expression;
        private readonly string _method;
        private readonly ICodegenExpression[] _parameters;

        public CodegenExpressionExprDotMethod(ICodegenExpression expression, string method, ICodegenExpression[] @params)
        {
            this._expression = expression;
            this._method = method;
            this._parameters = @params;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            if (_expression is CodegenExpressionRef)
            {
                _expression.Render(builder, imports);
            }
            else
            {
                builder.Append("(");
                _expression.Render(builder, imports);
                builder.Append(")");
            }
            builder.Append('.').Append(_method).Append("(");
            CodegenExpressionBuilder.RenderExpressions(builder, _parameters, imports);
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _parameters);
        }
    }
} // end of namespace